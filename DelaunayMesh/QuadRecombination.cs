using System;
using System.Collections.Generic;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    /// <summary>
    /// Recombination of a triangulation in quadrilaterals as square as possible.
    /// <para>1. Every pair of adjacent triangles (not across a boundary segment) is a candidate quadrilateral, rated by <see cref="QuadQuality"/>
    /// (1 for a square, lower for rectangles and distorted shapes, not positive if not convex).</para>
    /// <para>2. The candidates are accepted from the best one (greedy matching); a candidate needs angles between about 17 and 163 degrees
    /// (<see cref="MinSine"/>): a long rectangle (e.g. in a thin web) is better than two triangles.</para>
    /// <para>3. The triangles left alone are paired by augmenting paths (alternating chains of new and existing pairs), accepted only if the
    /// sum of the squared qualities increases, so a square is not traded for two worse quadrilaterals.</para>
    /// <para>4. Optionally every element is divided in quadrilaterals (<see cref="Subdivide"/>): the mesh is made only of quadrilaterals.</para>
    /// <para>5. The points that are neither on the boundary nor on the lattice are smoothed (<see cref="Smooth"/>), only when the elements around improve.</para>
    /// The coordinates are the normalized ones of <see cref="ConstrainedDelaunay"/>; the original coordinates are kept for the output.
    /// </summary>
    internal sealed class QuadRecombination
    {
        #region Constants

        /// <summary>
        /// Minimum sine of the angles of a quadrilateral made of two triangles (angles between about 17 and 163 degrees)
        /// </summary>
        private const double MinSine = 0.3;

        /// <summary>
        /// Maximum number of existing pairs changed by an augmenting path
        /// </summary>
        private const int MaxPathPairs = 6;

        private const int SmoothingIterations = 8;

        #endregion

        #region Variables

        private readonly List<double> _x;           // normalized coordinates
        private readonly List<double> _y;
        private readonly List<double> _originalX;
        private readonly List<double> _originalY;
        private readonly List<int> _tags;
        private readonly List<bool> _fixed;         // points on the lattice (and the points of the regular elements after the subdivision)
        private readonly HashSet<long> _segments;   // boundary segments
        private readonly double _scale;
        private readonly double _centerX;
        private readonly double _centerY;

        private List<int[]> _elements;              // triangles and quadrilaterals, counterclockwise

        #endregion

        #region Constructor

        public QuadRecombination(List<double> x, List<double> y, List<double> originalX, List<double> originalY, List<int> tags, List<bool> latticePoints,
            List<int[]> triangles, HashSet<long> segments, double scale, double centerX, double centerY)
        {
            _x = new List<double>(x);
            _y = new List<double>(y);
            _originalX = new List<double>(originalX);
            _originalY = new List<double>(originalY);
            _tags = new List<int>(tags);
            _fixed = new List<bool>(latticePoints);
            _segments = new HashSet<long>(segments, LongKeyComparer.Instance);
            _scale = scale;
            _centerX = centerX;
            _centerY = centerY;
            _elements = triangles;
        }

        #endregion

        #region Quality

        /// <summary>
        /// Quality of a quadrilateral: the minimum over the corners of 2 |e1 x e2| / (|e1|^2 + |e2|^2), where e1 and e2 are the edges of the corner.
        /// 1 for a square, 2 a b / (a^2 + b^2) for a rectangle a x b, sin(angle) for a rhombus, not positive if the quadrilateral is not convex
        /// </summary>
        private double QuadQuality(int a, int b, int c, int d)
        {
            return Math.Min(Math.Min(CornerQuality(d, a, b), CornerQuality(a, b, c)), Math.Min(CornerQuality(b, c, d), CornerQuality(c, d, a)));
        }

        /// <returns>The minimum sine of the angles of the quadrilateral (not positive if it is not convex)</returns>
        private double MinimumSine(int[] quad)
        {
            double min = double.MaxValue;
            for (int k = 0; k < 4; k++)
            {
                int previous = quad[(k + 3) % 4], vertex = quad[k], next = quad[(k + 1) % 4];
                double e1x = _x[next] - _x[vertex], e1y = _y[next] - _y[vertex];
                double e2x = _x[previous] - _x[vertex], e2y = _y[previous] - _y[vertex];
                double lengths = Math.Sqrt((e1x * e1x + e1y * e1y) * (e2x * e2x + e2y * e2y));
                min = Math.Min(min, lengths > 0 ? (e1x * e2y - e1y * e2x) / lengths : 0);
            }
            return min;
        }

        /// <returns>Quality of the corner in <paramref name="vertex"/> between the edges to <paramref name="previous"/> and <paramref name="next"/> (counterclockwise)</returns>
        private double CornerQuality(int previous, int vertex, int next)
        {
            double e1x = _x[next] - _x[vertex], e1y = _y[next] - _y[vertex];
            double e2x = _x[previous] - _x[vertex], e2y = _y[previous] - _y[vertex];
            double squares = e1x * e1x + e1y * e1y + e2x * e2x + e2y * e2y;
            return squares > 0 ? 2.0 * (e1x * e2y - e1y * e2x) / squares : 0;
        }

        /// <summary>
        /// Quality of a triangle: 4 sqrt(3) area / sum of the squared edges (1 for the equilateral triangle, not positive if clockwise)
        /// </summary>
        private double TriangleQuality(int a, int b, int c)
        {
            double abx = _x[b] - _x[a], aby = _y[b] - _y[a];
            double acx = _x[c] - _x[a], acy = _y[c] - _y[a];
            double bcx = _x[c] - _x[b], bcy = _y[c] - _y[b];
            double squares = abx * abx + aby * aby + acx * acx + acy * acy + bcx * bcx + bcy * bcy;
            return squares > 0 ? 2.0 * Math.Sqrt(3.0) * (abx * acy - aby * acx) / squares : 0;
        }

        private double ElementQuality(int[] element)
        {
            return element.Length == 3 ? TriangleQuality(element[0], element[1], element[2]) : QuadQuality(element[0], element[1], element[2], element[3]);
        }

        #endregion

        #region Recombination

        /// <summary>
        /// Pairs of triangles: triangle t is merged with the triangle across its edge, if the quadrilateral is good enough
        /// </summary>
        public void Recombine()
        {
            List<int[]> triangles = _elements;
            int n = triangles.Count;

            // candidates: adjacent triangles (not across a boundary segment) forming a good quadrilateral
            var neighbours = new List<KeyValuePair<int, double>>[n];
            var candidates = new List<KeyValuePair<double, long>>();
            var edges = new Dictionary<long, int>(3 * n, LongKeyComparer.Instance);

            for (int t = 0; t < n; t++)
            {
                neighbours[t] = new List<KeyValuePair<int, double>>(3);
                for (int k = 0; k < 3; k++)
                {
                    long key = EdgeKey(triangles[t][(k + 1) % 3], triangles[t][(k + 2) % 3]);
                    if (_segments.Contains(key))
                        continue;

                    if (!edges.TryGetValue(key, out int u))
                    {
                        edges[key] = t;
                        continue;
                    }

                    int[] quad = Merge(t, u);
                    if (MinimumSine(quad) < MinSine)
                        continue;

                    double quality = QuadQuality(quad[0], quad[1], quad[2], quad[3]);

                    neighbours[t].Add(new KeyValuePair<int, double>(u, quality));
                    neighbours[u].Add(new KeyValuePair<int, double>(t, quality));
                    candidates.Add(new KeyValuePair<double, long>(quality, ((long)u << 32) | (uint)t));
                }
            }

            // greedy matching, from the best quadrilateral
            candidates.Sort((p, q) => q.Key.CompareTo(p.Key));
            var mate = new int[n];
            for (int t = 0; t < n; t++)
                mate[t] = -1;

            foreach (KeyValuePair<double, long> candidate in candidates)
            {
                int t = (int)(candidate.Value >> 32), u = (int)(candidate.Value & 0xFFFFFFFF);
                if (mate[t] < 0 && mate[u] < 0)
                {
                    mate[t] = u;
                    mate[u] = t;
                }
            }

            // augmenting paths for the triangles left alone
            for (int pass = 0; pass < 3; pass++)
            {
                bool improved = false;
                for (int t = 0; t < n; t++)
                {
                    if (mate[t] < 0 && Augment(t, mate, neighbours))
                        improved = true;
                }
                if (!improved)
                    break;
            }

            // elements
            var elements = new List<int[]>(n);
            for (int t = 0; t < n; t++)
            {
                if (mate[t] < 0)
                    elements.Add(triangles[t]);
                else if (t < mate[t])
                    elements.Add(Merge(t, mate[t]));
            }

            _elements = elements;
        }

        /// <returns>The quadrilateral made of the triangles t and u (adjacent), counterclockwise</returns>
        private int[] Merge(int t, int u)
        {
            int[] a = _elements[t], b = _elements[u];

            // the vertex of t not in u: t = (p, e1, e2), u = (q, e2, e1)
            for (int k = 0; k < 3; k++)
            {
                int p = a[k];
                if (p == b[0] || p == b[1] || p == b[2])
                    continue;

                int e1 = a[(k + 1) % 3], e2 = a[(k + 2) % 3];
                int q = b[0] != e1 && b[0] != e2 ? b[0] : b[1] != e1 && b[1] != e2 ? b[1] : b[2];
                return new[] { p, e1, q, e2 };
            }

            throw new InvalidOperationException("The triangles are not adjacent");
        }

        private double PairQuality(int t, int u)
        {
            int[] quad = Merge(t, u);
            return QuadQuality(quad[0], quad[1], quad[2], quad[3]);
        }

        /// <summary>
        /// Search the best augmenting path from the triangle <paramref name="start"/> (without a pair) to another triangle without a pair:
        /// start - n1 (new pair), n1 - m1 (removed pair), m1 - n2 (new pair), ... , m(k-1) - nk (new pair).
        /// The path is applied if the sum of the squared qualities of the pairs increases
        /// </summary>
        private bool Augment(int start, int[] mate, List<KeyValuePair<int, double>>[] neighbours)
        {
            var path = new List<int> { start };
            var onPath = new HashSet<int> { start };
            List<int> best = null;
            double bestGain = 1E-9;

            void Search(int current, double gain, int pairs)
            {
                foreach (KeyValuePair<int, double> neighbour in neighbours[current])
                {
                    int next = neighbour.Key;
                    if (onPath.Contains(next))
                        continue;

                    double added = neighbour.Value * neighbour.Value;
                    if (mate[next] < 0)
                    {
                        if (gain + added > bestGain)
                        {
                            bestGain = gain + added;
                            best = new List<int>(path) { next };
                        }
                        continue;
                    }

                    int partner = mate[next];
                    if (pairs >= MaxPathPairs || onPath.Contains(partner))
                        continue;

                    double removed = PairQuality(next, partner);
                    path.Add(next);
                    path.Add(partner);
                    onPath.Add(next);
                    onPath.Add(partner);
                    Search(partner, gain + added - removed * removed, pairs + 1);
                    path.RemoveRange(path.Count - 2, 2);
                    onPath.Remove(next);
                    onPath.Remove(partner);
                }
            }

            Search(start, 0, 0);

            if (best is null)
                return false;

            // new pairs: (best[0], best[1]), (best[2], best[3]), ...
            for (int i = 0; i + 1 < best.Count; i += 2)
            {
                mate[best[i]] = best[i + 1];
                mate[best[i + 1]] = best[i];
            }

            return true;
        }

        #endregion

        #region Elimination of the triangles

        /// <summary>
        /// Elimination of the triangles left by the recombination, keeping the size of the elements: a triangle is joined to another triangle
        /// (or to the boundary) by a channel of quadrilaterals, entering and leaving each one through opposite edges. The midpoints of the crossed
        /// edges are inserted: every quadrilateral of the channel is divided in two, the triangles at the ends become quadrilaterals (with the
        /// midpoint on the edge, moved by the smoothing). The channels prefer the irregular quadrilaterals to the ones of the lattice.
        /// </summary>
        /// <returns>True if all the triangles are eliminated</returns>
        public bool EliminateTriangles()
        {
            var edgeElements = new Dictionary<long, List<int>>(LongKeyComparer.Instance);   // edge -> elements (indices in _elements, null if removed)
            for (int e = 0; e < _elements.Count; e++)
                AddElementEdges(e, edgeElements);

            bool eliminated = true;
            for (int e = 0; e < _elements.Count; e++)
            {
                if (_elements[e] != null && _elements[e].Length == 3 && !EliminateTriangle(e, edgeElements))
                    eliminated = false;
            }

            _elements.RemoveAll(element => element is null);
            return eliminated;
        }

        private void AddElementEdges(int e, Dictionary<long, List<int>> edgeElements)
        {
            int[] element = _elements[e];
            for (int k = 0; k < element.Length; k++)
            {
                long key = EdgeKey(element[k], element[(k + 1) % element.Length]);
                if (!edgeElements.TryGetValue(key, out List<int> list))
                    edgeElements[key] = list = new List<int>(2);
                list.Add(e);
            }
        }

        private void RemoveElementEdges(int e, Dictionary<long, List<int>> edgeElements)
        {
            int[] element = _elements[e];
            for (int k = 0; k < element.Length; k++)
            {
                long key = EdgeKey(element[k], element[(k + 1) % element.Length]);
                if (edgeElements.TryGetValue(key, out List<int> list))
                    list.Remove(e);
            }
        }

        /// <returns>The element across the edge a-b, -1 if the edge is on the boundary</returns>
        private int Across(int e, int a, int b, Dictionary<long, List<int>> edgeElements)
        {
            long key = EdgeKey(a, b);
            if (_segments.Contains(key) || !edgeElements.TryGetValue(key, out List<int> list))
                return -1;

            foreach (int other in list)
            {
                if (other != e)
                    return other;
            }
            return -1;
        }

        /// <summary>
        /// Channel from the triangle to another triangle or to the boundary (Dijkstra on the quadrilaterals, the cost of a quadrilateral
        /// grows with its quality). A channel to the boundary costs more: it eliminates one triangle instead of two
        /// </summary>
        private bool EliminateTriangle(int start, Dictionary<long, List<int>> edgeElements)
        {
            const int maxLength = 16;
            const double boundaryPenalty = 1.5;

            // state: element entered through the edge (a, b); the path is rebuilt from the parents
            var states = new List<(int element, int a, int b, int parent, double cost, int length)>();
            var queue = new SortedSet<(double cost, int state)>();
            var visited = new HashSet<int> { start };

            int[] triangle = _elements[start];
            for (int k = 0; k < 3; k++)
            {
                int a = triangle[k], b = triangle[(k + 1) % 3];
                int next = Across(start, a, b, edgeElements);
                if (next < 0 || _elements[next].Length != 4)
                    continue; // a triangle adjacent to the start would give two flat corners on the same midpoint

                states.Add((next, a, b, -1, CrossingCost(next), 1));
                queue.Add((states[states.Count - 1].cost, states.Count - 1));
            }

            int best = -1, bestEnd = -1;
            double bestCost = double.MaxValue;

            while (queue.Count > 0)
            {
                (double cost, int s) = queue.Min;
                queue.Remove(queue.Min);
                if (cost >= bestCost)
                    break;

                var state = states[s];
                if (visited.Contains(state.element))
                    continue;
                visited.Add(state.element);

                // the quadrilateral is left through the edge opposite to the entry
                int[] quad = _elements[state.element];
                int i = IndexOfEdge(quad, state.a, state.b);
                int c = quad[(i + 2) % 4], d = quad[(i + 3) % 4];

                int beyond = Across(state.element, c, d, edgeElements);
                if (beyond < 0)
                {
                    if (_segments.Contains(EdgeKey(c, d)) && cost + boundaryPenalty < bestCost)
                    {
                        bestCost = cost + boundaryPenalty;
                        best = s;
                        bestEnd = -1;
                    }
                    continue;
                }

                if (_elements[beyond].Length == 3)
                {
                    if (beyond != start && cost < bestCost)
                    {
                        bestCost = cost;
                        best = s;
                        bestEnd = beyond;
                    }
                    continue;
                }

                if (state.length >= maxLength || visited.Contains(beyond))
                    continue;

                states.Add((beyond, c, d, s, cost + CrossingCost(beyond), state.length + 1));
                queue.Add((states[states.Count - 1].cost, states.Count - 1));
            }

            if (best < 0)
                return false;

            // the channel: the start triangle, the quadrilaterals (with their entry edges), the end (triangle or boundary)
            var channel = new List<(int element, int a, int b)>();
            for (int s = best; s >= 0; s = states[s].parent)
                channel.Insert(0, (states[s].element, states[s].a, states[s].b));

            var midpoints = new Dictionary<long, int>(LongKeyComparer.Instance);
            int Midpoint(int a, int b)
            {
                long key = EdgeKey(a, b);
                if (midpoints.TryGetValue(key, out int m))
                    return m;
                m = AddVertex((_x[a] + _x[b]) / 2.0, (_y[a] + _y[b]) / 2.0, (_originalX[a] + _originalX[b]) / 2.0, (_originalY[a] + _originalY[b]) / 2.0, false);
                midpoints[key] = m;
                if (_segments.Remove(key))
                {
                    _segments.Add(EdgeKey(a, m));
                    _segments.Add(EdgeKey(m, b));
                }
                return m;
            }

            // start triangle: quadrilateral with the midpoint of the first crossed edge
            ReplaceElement(start, Insert(_elements[start], channel[0].a, channel[0].b, Midpoint(channel[0].a, channel[0].b)), edgeElements);

            int lastA = -1, lastB = -1;
            foreach ((int element, int a, int b) in channel)
            {
                int[] quad = _elements[element];
                int i = IndexOfEdge(quad, a, b);
                int p0 = quad[i], p1 = quad[(i + 1) % 4], p2 = quad[(i + 2) % 4], p3 = quad[(i + 3) % 4];
                int entry = Midpoint(p0, p1), exit = Midpoint(p2, p3);

                ReplaceElement(element, new[] { entry, p1, p2, exit }, edgeElements);
                _elements.Add(new[] { p0, entry, exit, p3 });
                AddElementEdges(_elements.Count - 1, edgeElements);

                lastA = p2;
                lastB = p3;
            }

            // end triangle: quadrilateral with the midpoint of the last crossed edge
            if (bestEnd >= 0)
                ReplaceElement(bestEnd, Insert(_elements[bestEnd], lastA, lastB, Midpoint(lastA, lastB)), edgeElements);

            return true;
        }

        private double CrossingCost(int element)
        {
            int[] quad = _elements[element];
            return 0.3 + QuadQuality(quad[0], quad[1], quad[2], quad[3]);
        }

        /// <returns>Index i such that the element has the edge (a, b) as (element[i], element[i + 1]) or (element[i + 1], element[i])</returns>
        private static int IndexOfEdge(int[] element, int a, int b)
        {
            int n = element.Length;
            for (int i = 0; i < n; i++)
            {
                int p = element[i], q = element[(i + 1) % n];
                if ((p == a && q == b) || (p == b && q == a))
                    return i;
            }
            throw new InvalidOperationException("The edge is not in the element");
        }

        /// <returns>The element with the point <paramref name="m"/> inserted between a and b</returns>
        private static int[] Insert(int[] element, int a, int b, int m)
        {
            int i = IndexOfEdge(element, a, b);
            var result = new List<int>(element);
            result.Insert(i + 1, m);
            return result.ToArray();
        }

        private void ReplaceElement(int e, int[] element, Dictionary<long, List<int>> edgeElements)
        {
            RemoveElementEdges(e, edgeElements);
            _elements[e] = element;
            AddElementEdges(e, edgeElements);
        }

        #endregion

        #region Subdivision

        /// <summary>
        /// Every quadrilateral is divided in four quadrilaterals (midpoints of the edges and center) and every triangle in three
        /// (midpoints of the edges and centroid): the mesh is made only of quadrilaterals, with half the size.
        /// The new points of the regular quadrilaterals (all the vertices on the lattice or on the boundary) are not smoothed
        /// </summary>
        public void Subdivide()
        {
            var boundary = BoundaryVertices();
            var regular = new bool[_elements.Count];
            var regularEdges = new HashSet<long>(LongKeyComparer.Instance);
            for (int e = 0; e < _elements.Count; e++)
            {
                int[] element = _elements[e];
                regular[e] = element.Length == 4;
                foreach (int v in element)
                    regular[e] &= _fixed[v] || boundary.Contains(v);

                if (regular[e])
                {
                    for (int k = 0; k < 4; k++)
                        regularEdges.Add(EdgeKey(element[k], element[(k + 1) % 4]));
                }
            }

            var midpoints = new Dictionary<long, int>(LongKeyComparer.Instance);
            var segments = new HashSet<long>(LongKeyComparer.Instance);

            int Midpoint(int a, int b)
            {
                long key = EdgeKey(a, b);
                if (midpoints.TryGetValue(key, out int m))
                    return m;

                m = AddVertex((_x[a] + _x[b]) / 2.0, (_y[a] + _y[b]) / 2.0, (_originalX[a] + _originalX[b]) / 2.0, (_originalY[a] + _originalY[b]) / 2.0,
                    regularEdges.Contains(key));
                midpoints[key] = m;

                if (_segments.Contains(key))
                {
                    segments.Add(EdgeKey(a, m));
                    segments.Add(EdgeKey(m, b));
                }

                return m;
            }

            var elements = new List<int[]>(4 * _elements.Count);
            for (int e = 0; e < _elements.Count; e++)
            {
                int[] element = _elements[e];
                int count = element.Length;

                double x = 0, y = 0, originalX = 0, originalY = 0;
                foreach (int v in element)
                {
                    x += _x[v];
                    y += _y[v];
                    originalX += _originalX[v];
                    originalY += _originalY[v];
                }
                int center = AddVertex(x / count, y / count, originalX / count, originalY / count, regular[e]);

                for (int k = 0; k < count; k++)
                {
                    int vertex = element[k];
                    int next = Midpoint(vertex, element[(k + 1) % count]);
                    int previous = Midpoint(element[(k + count - 1) % count], vertex);
                    elements.Add(new[] { vertex, next, center, previous });
                }
            }

            _elements = elements;
            _segments.Clear();
            _segments.UnionWith(segments);
        }

        private int AddVertex(double x, double y, double originalX, double originalY, bool isFixed)
        {
            _x.Add(x);
            _y.Add(y);
            _originalX.Add(originalX);
            _originalY.Add(originalY);
            _tags.Add(-1);
            _fixed.Add(isFixed);
            return _x.Count - 1;
        }

        #endregion

        #region Smoothing

        /// <summary>
        /// Laplacian smoothing of the points not on the boundary: a point is moved to the average of its neighbours only if the worst
        /// element around it improves. The points of the lattice are moved only if they are near an irregular element (a triangle or an
        /// element with a point not on the lattice), so the rectangles of the lattice are kept
        /// </summary>
        public void Smooth()
        {
            HashSet<int> boundary = BoundaryVertices();

            // points near an irregular element
            var irregular = new HashSet<int>();
            foreach (int[] element in _elements)
            {
                bool regular = element.Length == 4;
                foreach (int v in element)
                    regular &= _fixed[v] || boundary.Contains(v);

                if (!regular)
                    irregular.UnionWith(element);
            }

            var elementsOf = new Dictionary<int, List<int[]>>();
            var neighboursOf = new Dictionary<int, HashSet<int>>();
            foreach (int[] element in _elements)
            {
                for (int k = 0; k < element.Length; k++)
                {
                    int v = element[k];
                    if (boundary.Contains(v) || !irregular.Contains(v))
                        continue;

                    if (!elementsOf.TryGetValue(v, out List<int[]> list))
                    {
                        elementsOf[v] = list = new List<int[]>();
                        neighboursOf[v] = new HashSet<int>();
                    }
                    list.Add(element);
                    neighboursOf[v].Add(element[(k + 1) % element.Length]);
                    neighboursOf[v].Add(element[(k + element.Length - 1) % element.Length]);
                }
            }

            for (int iteration = 0; iteration < SmoothingIterations; iteration++)
            {
                bool moved = false;
                foreach (KeyValuePair<int, List<int[]>> item in elementsOf)
                {
                    int v = item.Key;
                    double x = 0, y = 0;
                    foreach (int neighbour in neighboursOf[v])
                    {
                        x += _x[neighbour];
                        y += _y[neighbour];
                    }
                    x /= neighboursOf[v].Count;
                    y /= neighboursOf[v].Count;

                    double oldX = _x[v], oldY = _y[v];
                    if (Math.Abs(x - oldX) + Math.Abs(y - oldY) < 1E-12)
                        continue;

                    double before = WorstQuality(item.Value);
                    _x[v] = x;
                    _y[v] = y;

                    if (WorstQuality(item.Value) > before + 1E-9)
                    {
                        _originalX[v] = x * _scale + _centerX;
                        _originalY[v] = y * _scale + _centerY;
                        moved = true;
                    }
                    else
                    {
                        _x[v] = oldX;
                        _y[v] = oldY;
                    }
                }

                if (!moved)
                    break;
            }
        }

        /// <returns>The quality of the worst element of the mesh</returns>
        public double WorstElementQuality()
        {
            return WorstQuality(_elements);
        }

        private double WorstQuality(List<int[]> elements)
        {
            double worst = double.MaxValue;
            foreach (int[] element in elements)
                worst = Math.Min(worst, ElementQuality(element));
            return worst;
        }

        #endregion

        #region Helpers and output

        private HashSet<int> BoundaryVertices()
        {
            var boundary = new HashSet<int>();
            foreach (long key in _segments)
            {
                boundary.Add((int)(key >> 32));
                boundary.Add((int)(key & 0xFFFFFFFF));
            }
            return boundary;
        }

        private static long EdgeKey(int a, int b)
        {
            return a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            var vertexIds = new int[_x.Count];
            for (int i = 0; i < vertexIds.Length; i++)
                vertexIds[i] = -1;

            int vertexId = 0;
            int faceId = 0;
            int edgeId = 0;
            var edges = new HashSet<long>(LongKeyComparer.Instance);

            foreach (int[] element in _elements)
            {
                var ids = new int[element.Length];
                for (int k = 0; k < element.Length; k++)
                {
                    int v = element[k];
                    if (vertexIds[v] < 0)
                    {
                        vertexIds[v] = vertexId;
                        mesh.Vertices.Add(new MeshVertex(new Point3d(_originalX[v], _originalY[v], 0)) { Tag = _tags[v] }, vertexId);
                        vertexId++;
                    }
                    ids[k] = vertexIds[v];
                }

                mesh.Faces.Add(ids.Length == 3 ? new MeshFace(ids[0], ids[1], ids[2]) : new MeshFace(ids[0], ids[1], ids[2], ids[3]), faceId++);

                for (int k = 0; k < ids.Length; k++)
                {
                    int a = ids[k], b = ids[(k + 1) % ids.Length];
                    if (edges.Add(EdgeKey(a, b)))
                        mesh.Edges.Add(new MeshEdge(a, b), edgeId++);
                }
            }

            return mesh;
        }

        #endregion
    }
}
