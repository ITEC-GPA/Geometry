using System;
using System.Collections.Generic;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    /// <summary>
    /// Constrained Delaunay triangulation of a shape (fill minus holes, plus childs) refined according to the mesh size.
    /// <para>1. The boundary is divided in segments not longer than the mesh size.</para>
    /// <para>2. The points are inserted one by one in a Delaunay triangulation (Lawson: split of the triangle and edge flips),
    /// then the boundary segments missing in the triangulation are recovered with edge flips (Sloan).</para>
    /// <para>3. The triangles outside the shape are discarded: a triangle is inside if it is reached crossing an odd number of boundary segments.</para>
    /// <para>4. Refinement (Ruppert): the triangles too large (circumradius compared with the mesh size) or, if requested, with a too small angle
    /// are split inserting their circumcenter. A circumcenter inside the diametral circle of a boundary segment, or not visible
    /// from its triangle, splits the segment in its midpoint instead.</para>
    /// The coordinates are normalized (bounding box centered in the origin and with size 1), so the tolerances do not depend
    /// on the units and on the position of the shape. The input vertices keep their original coordinates in the mesh.
    /// </summary>
    internal sealed class ConstrainedDelaunay
    {
        #region Types and constants

        private sealed class Triangle
        {
            public readonly int[] V = new int[3];           // vertices, counterclockwise
            public readonly Triangle[] N = new Triangle[3];  // N[i]: neighbour across the edge opposite to V[i]
            public readonly bool[] C = new bool[3];         // C[i]: the edge opposite to V[i] is a boundary segment
            public bool Inside;
            public int Stamp;                                // changed at every modification, to discard the old entries of the refinement queue
        }

        private const double MergeTolerance = 1E-10;       // normalized distance under which two points are the same vertex
        private const double LocateTolerance = 1E-13;      // normalized distance: a point on an edge (with rounding errors) is inside both triangles
        private const double SuperTriangleSize = 20.0;     // the normalized shape is inside [-0.5, 0.5] x [-0.5, 0.5]

        /// <summary>
        /// A triangle is too large if its circumradius is greater than this factor by the mesh size: 1.25 / sqrt(3), i.e. an equilateral triangle
        /// with the edges 25% longer than the mesh size
        /// </summary>
        private const double SizeFactor = 0.72;

        #endregion

        #region Variables

        private readonly List<double> _x = new List<double>();          // normalized coordinates
        private readonly List<double> _y = new List<double>();
        private readonly List<double> _originalX = new List<double>();  // original coordinates
        private readonly List<double> _originalY = new List<double>();
        private readonly List<int> _tags = new List<int>();             // index of the boundary points, -1 for the points added by the refinement
        private readonly List<Triangle> _vertexTriangle = new List<Triangle>();
        private readonly List<Triangle> _triangles = new List<Triangle>();
        private readonly HashSet<long> _segments = new HashSet<long>(); // boundary segments (constrained edges)

        private double _centerX;
        private double _centerY;
        private double _scale;
        private Triangle _lastTriangle;

        private Queue<KeyValuePair<Triangle, int>> _refineQueue;       // not null during the refinement

        #endregion

        #region Public method

        /// <summary>
        /// Triangulate a shape
        /// </summary>
        /// <param name="shape">The shape: fill, holes and childs (shapes inside the holes)</param>
        /// <param name="meshSize">Maximum length of the boundary segments and size of the triangles. Infinite or not positive: only the vertices of the shape are used</param>
        /// <param name="refine">If false the interior is not refined: only the points on the boundary are used</param>
        /// <param name="minAngle">Minimum angle (degrees) of the triangles: the triangles with a smaller angle are refined, if the angle is not
        /// between two boundary segments. Zero: only the size of the triangles is checked. Values greater than 30 degrees can not be reached</param>
        /// <returns>The mesh of triangles, counterclockwise, in the plane of the shape</returns>
        /// <exception cref="ArgumentException">If the shape has not a valid fill</exception>
        /// <exception cref="InvalidOperationException">If the triangulation fails (e.g. boundary that intersects itself)</exception>
        public static Mesh Triangulate(Shape shape, double meshSize, bool refine, double minAngle = 0)
        {
            if (shape is null)
                throw new ArgumentNullException(nameof(shape));

            var loops = new List<List<double[]>>();
            CollectLoops(shape, loops);

            if (loops.Count == 0 || loops[0].Count < 3)
                throw new ArgumentException("The shape must have a fill with at least three distinct points");

            var triangulation = new ConstrainedDelaunay();
            triangulation.Build(loops, meshSize, refine, minAngle);
            return triangulation.ToMesh();
        }

        #endregion

        #region Input

        private static void CollectLoops(Shape shape, List<List<double[]>> loops)
        {
            AddLoop(shape.Fill, loops);

            if (shape.Holes != null)
            {
                for (int i = 0; i < shape.Holes.Length; i++)
                    AddLoop(shape.Holes[i], loops);
            }

            if (shape.Childs != null)
            {
                for (int i = 0; i < shape.Childs.Length; i++)
                    CollectLoops(shape.Childs[i], loops);
            }
        }

        /// <summary>
        /// Add the points of the polygon without consecutive duplicated points. Polygons with less than three points are ignored
        /// </summary>
        private static void AddLoop(Polygon3d polygon, List<List<double[]>> loops)
        {
            if (polygon is null)
                return;

            var loop = new List<double[]>(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                double[] point = new[] { polygon[i].X, polygon[i].Y };
                if (loop.Count == 0 || !SamePoint(loop[loop.Count - 1], point))
                    loop.Add(point);
            }

            while (loop.Count > 1 && SamePoint(loop[0], loop[loop.Count - 1]))
                loop.RemoveAt(loop.Count - 1);

            if (loop.Count >= 3)
                loops.Add(loop);
        }

        private static bool SamePoint(double[] a, double[] b)
        {
            double tolerance = 1E-12 * Math.Max(1.0, Math.Max(Math.Abs(a[0]), Math.Abs(a[1])));
            return Math.Abs(a[0] - b[0]) <= tolerance && Math.Abs(a[1] - b[1]) <= tolerance;
        }

        #endregion

        #region Build

        private void Build(List<List<double[]>> loops, double meshSize, bool refine, double minAngle)
        {
            // Normalization
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (var loop in loops)
            {
                foreach (var point in loop)
                {
                    minX = Math.Min(minX, point[0]);
                    minY = Math.Min(minY, point[1]);
                    maxX = Math.Max(maxX, point[0]);
                    maxY = Math.Max(maxY, point[1]);
                }
            }

            _centerX = (minX + maxX) / 2.0;
            _centerY = (minY + maxY) / 2.0;
            _scale = Math.Max(maxX - minX, maxY - minY);

            if (!(_scale > 0) || double.IsInfinity(_scale))
                throw new ArgumentException("The shape has not a valid size");

            double size = meshSize > 0 && !double.IsInfinity(meshSize) && !double.IsNaN(meshSize) ? meshSize / _scale : double.PositiveInfinity;

            // Super triangle
            AddVertex(-SuperTriangleSize, -SuperTriangleSize, double.NaN, double.NaN, -1);
            AddVertex(SuperTriangleSize, -SuperTriangleSize, double.NaN, double.NaN, -1);
            AddVertex(0, SuperTriangleSize, double.NaN, double.NaN, -1);
            NewTriangle(0, 1, 2);

            // Boundary points, divided according to the mesh size
            var loopVertices = new List<List<int>>();
            int tag = 0;
            foreach (var loop in loops)
            {
                var vertices = new List<int>();
                for (int i = 0; i < loop.Count; i++)
                {
                    double[] a = loop[i];
                    double[] b = loop[(i + 1) % loop.Count];

                    AddBoundaryVertex(vertices, a[0], a[1], ref tag);

                    double length = Math.Sqrt((b[0] - a[0]) * (b[0] - a[0]) + (b[1] - a[1]) * (b[1] - a[1])) / _scale;
                    int divisions = double.IsInfinity(size) ? 1 : Math.Max(1, (int)Math.Ceiling(length / size - 1E-9));
                    for (int k = 1; k < divisions; k++)
                    {
                        double t = (double)k / divisions;
                        AddBoundaryVertex(vertices, a[0] + t * (b[0] - a[0]), a[1] + t * (b[1] - a[1]), ref tag);
                    }
                }
                loopVertices.Add(vertices);
            }

            // Boundary segments
            foreach (var vertices in loopVertices)
            {
                for (int i = 0; i < vertices.Count; i++)
                    RecoverSegment(vertices[i], vertices[(i + 1) % vertices.Count], 0);
            }

            ClassifyTriangles();

            if (refine && !double.IsInfinity(size))
                Refine(size, minAngle);
        }

        private void AddBoundaryVertex(List<int> vertices, double x, double y, ref int tag)
        {
            int vertex = InsertPoint((x - _centerX) / _scale, (y - _centerY) / _scale, x, y, tag, null);
            if (vertex == _x.Count - 1 && _tags[vertex] == tag)
                tag++;

            // coincident points (e.g. a hole touching the fill) are merged
            if (vertices.Count == 0 || vertices[vertices.Count - 1] != vertex)
                vertices.Add(vertex);
        }

        #endregion

        #region Vertices and triangles

        private int AddVertex(double x, double y, double originalX, double originalY, int tag)
        {
            _x.Add(x);
            _y.Add(y);
            _originalX.Add(double.IsNaN(originalX) ? x * _scale + _centerX : originalX);
            _originalY.Add(double.IsNaN(originalY) ? y * _scale + _centerY : originalY);
            _tags.Add(tag);
            _vertexTriangle.Add(null);
            return _x.Count - 1;
        }

        private Triangle NewTriangle(int a, int b, int c)
        {
            var triangle = new Triangle();
            triangle.V[0] = a;
            triangle.V[1] = b;
            triangle.V[2] = c;
            _triangles.Add(triangle);
            SetVertices(triangle, a, b, c);
            return triangle;
        }

        private void SetVertices(Triangle t, int a, int b, int c)
        {
            t.V[0] = a;
            t.V[1] = b;
            t.V[2] = c;
            _vertexTriangle[a] = t;
            _vertexTriangle[b] = t;
            _vertexTriangle[c] = t;
        }

        /// <summary>
        /// Mark the triangle as modified: during the refinement it is checked again
        /// </summary>
        private void Touch(Triangle t)
        {
            t.Stamp++;
            if (_refineQueue != null && t.Inside)
                _refineQueue.Enqueue(new KeyValuePair<Triangle, int>(t, t.Stamp));
        }

        private static int IndexOf(Triangle t, int vertex)
        {
            if (t.V[0] == vertex) return 0;
            if (t.V[1] == vertex) return 1;
            if (t.V[2] == vertex) return 2;
            return -1;
        }

        private static int IndexOfNeighbour(Triangle t, Triangle neighbour)
        {
            if (t.N[0] == neighbour) return 0;
            if (t.N[1] == neighbour) return 1;
            if (t.N[2] == neighbour) return 2;
            return -1;
        }

        private static void ReplaceNeighbour(Triangle t, Triangle oldNeighbour, Triangle newNeighbour)
        {
            if (t is null)
                return;

            int k = IndexOfNeighbour(t, oldNeighbour);
            if (k >= 0)
                t.N[k] = newNeighbour;
        }

        private static long SegmentKey(int a, int b)
        {
            return a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        }

        #endregion

        #region Geometric predicates

        /// <returns>Twice the signed area of the triangle a, b, c: positive if counterclockwise</returns>
        private double Orient(int a, int b, int c)
        {
            return Orient(a, b, _x[c], _y[c]);
        }

        private double Orient(int a, int b, double x, double y)
        {
            return (_x[b] - _x[a]) * (y - _y[a]) - (_y[b] - _y[a]) * (x - _x[a]);
        }

        private double Distance(int a, int b)
        {
            double dx = _x[b] - _x[a];
            double dy = _y[b] - _y[a];
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <returns>True if the point is on the right of the edge a-b, i.e. outside of a counterclockwise triangle with that edge, farther than <see cref="LocateTolerance"/></returns>
        private bool IsOutside(int a, int b, double x, double y)
        {
            return Orient(a, b, x, y) < -LocateTolerance * Distance(a, b);
        }

        /// <returns>True if <paramref name="d"/> is inside the circumcircle of the counterclockwise triangle a, b, c (with a relative tolerance, so cocircular points are not swapped forever)</returns>
        private bool InCircle(int a, int b, int c, int d)
        {
            double adx = _x[a] - _x[d], ady = _y[a] - _y[d];
            double bdx = _x[b] - _x[d], bdy = _y[b] - _y[d];
            double cdx = _x[c] - _x[d], cdy = _y[c] - _y[d];

            double alift = adx * adx + ady * ady;
            double blift = bdx * bdx + bdy * bdy;
            double clift = cdx * cdx + cdy * cdy;

            double det = alift * (bdx * cdy - cdx * bdy) + blift * (cdx * ady - adx * cdy) + clift * (adx * bdy - bdx * ady);
            double permanent = alift * (Math.Abs(bdx * cdy) + Math.Abs(cdx * bdy)) + blift * (Math.Abs(cdx * ady) + Math.Abs(adx * cdy)) + clift * (Math.Abs(adx * bdy) + Math.Abs(bdx * ady));

            return det > 1E-12 * permanent;
        }

        private void Circumcenter(Triangle t, out double x, out double y, out double squareRadius)
        {
            int a = t.V[0], b = t.V[1], c = t.V[2];
            double bx = _x[b] - _x[a], by = _y[b] - _y[a];
            double cx = _x[c] - _x[a], cy = _y[c] - _y[a];
            double d = 2.0 * (bx * cy - by * cx);
            double b2 = bx * bx + by * by;
            double c2 = cx * cx + cy * cy;
            double ux = (cy * b2 - by * c2) / d;
            double uy = (bx * c2 - cx * b2) / d;
            x = _x[a] + ux;
            y = _y[a] + uy;
            squareRadius = ux * ux + uy * uy;
        }

        #endregion

        #region Point insertion

        /// <summary>
        /// Find the triangle containing the point (visibility walk)
        /// </summary>
        private Triangle Locate(double x, double y, Triangle start)
        {
            Triangle t = start ?? _lastTriangle ?? _triangles[0];
            int maxSteps = 4 * _triangles.Count + 16;

            for (int step = 0; step < maxSteps; step++)
            {
                int exit = -1;
                for (int k = 0; k < 3; k++)
                {
                    int e = (k + step) % 3; // the rotation of the first edge avoids cycles
                    if (IsOutside(t.V[(e + 1) % 3], t.V[(e + 2) % 3], x, y))
                    {
                        exit = e;
                        break;
                    }
                }

                if (exit < 0)
                {
                    _lastTriangle = t;
                    return t;
                }

                if (t.N[exit] is null)
                    return null; // outside the super triangle

                t = t.N[exit];
            }

            // it should never happen: exhaustive search
            foreach (Triangle triangle in _triangles)
            {
                if (!IsOutside(triangle.V[0], triangle.V[1], x, y) && !IsOutside(triangle.V[1], triangle.V[2], x, y) && !IsOutside(triangle.V[2], triangle.V[0], x, y))
                    return triangle;
            }

            return null;
        }

        /// <returns>The index of the vertex: an existing vertex if the point coincides with it</returns>
        private int InsertPoint(double x, double y, double originalX, double originalY, int tag, Triangle start)
        {
            Triangle t = Locate(x, y, start);
            if (t is null)
                throw new InvalidOperationException($"The point ({x * _scale + _centerX}; {y * _scale + _centerY}) is outside the triangulation");

            for (int k = 0; k < 3; k++)
            {
                int v = t.V[k];
                double dx = _x[v] - x, dy = _y[v] - y;
                if (dx * dx + dy * dy <= MergeTolerance * MergeTolerance)
                    return v;
            }

            int edge = -1;
            for (int k = 0; k < 3; k++)
            {
                int a = t.V[(k + 1) % 3], b = t.V[(k + 2) % 3];
                if (Math.Abs(Orient(a, b, x, y)) <= MergeTolerance * Distance(a, b))
                {
                    edge = k;
                    break;
                }
            }

            int p = AddVertex(x, y, originalX, originalY, tag);

            if (edge >= 0)
                SplitEdge(t, edge, p);
            else
                SplitTriangle(t, p);

            Validate("InsertPoint");
            return p;
        }

        /// <summary>
        /// Split the triangle in three triangles with the new vertex <paramref name="p"/> inside it
        /// </summary>
        private void SplitTriangle(Triangle t, int p)
        {
            int a = t.V[0], b = t.V[1], c = t.V[2];
            Triangle na = t.N[0], nb = t.N[1], nc = t.N[2];
            bool ca = t.C[0], cb = t.C[1], cc = t.C[2];

            Triangle t1 = t;
            Triangle t2 = NewTriangle(p, c, a);
            Triangle t3 = NewTriangle(p, a, b);
            SetVertices(t1, p, b, c);

            t1.N[0] = na; t1.N[1] = t2; t1.N[2] = t3;
            t1.C[0] = ca; t1.C[1] = false; t1.C[2] = false;

            t2.N[0] = nb; t2.N[1] = t3; t2.N[2] = t1;
            t2.C[0] = cb; t2.C[1] = false; t2.C[2] = false;

            t3.N[0] = nc; t3.N[1] = t1; t3.N[2] = t2;
            t3.C[0] = cc; t3.C[1] = false; t3.C[2] = false;

            ReplaceNeighbour(nb, t, t2);
            ReplaceNeighbour(nc, t, t3);

            t2.Inside = t.Inside;
            t3.Inside = t.Inside;

            Touch(t1);
            Touch(t2);
            Touch(t3);

            Legalize(new Stack<Triangle>(new[] { t1, t2, t3 }));
        }

        /// <summary>
        /// Split the edge <paramref name="edge"/> of <paramref name="t"/> (and the neighbour triangle) with the new vertex <paramref name="p"/> on the edge.
        /// If the edge is a boundary segment, the two halves are boundary segments
        /// </summary>
        private void SplitEdge(Triangle t, int edge, int p)
        {
            int c = t.V[edge];
            int a = t.V[(edge + 1) % 3];
            int b = t.V[(edge + 2) % 3];
            bool constrained = t.C[edge];

            Triangle u = t.N[edge];
            Triangle tNextA = t.N[(edge + 2) % 3]; // across (c, a)
            bool tNextACon = t.C[(edge + 2) % 3];
            Triangle tNextB = t.N[(edge + 1) % 3]; // across (b, c)
            bool tNextBCon = t.C[(edge + 1) % 3];

            // t: (c, a, b) -> t1 (p, c, a) and t2 (p, b, c)
            Triangle t1 = t;
            Triangle t2 = NewTriangle(p, b, c);
            SetVertices(t1, p, c, a);

            t1.N[0] = tNextA; t1.C[0] = tNextACon;
            t2.N[0] = tNextB; t2.C[0] = tNextBCon;
            t1.N[1] = null; t1.C[1] = constrained;  // (a, p): set below
            t1.N[2] = t2; t1.C[2] = false;          // (p, c)
            t2.N[1] = t1; t2.C[1] = false;          // (c, p)
            t2.N[2] = null; t2.C[2] = constrained;  // (p, b): set below
            ReplaceNeighbour(tNextB, t, t2);
            t2.Inside = t.Inside;

            var stack = new Stack<Triangle>();
            stack.Push(t1);
            stack.Push(t2);
            Touch(t1);
            Touch(t2);

            if (u != null)
            {
                int j = IndexOfNeighbour(u, t);
                int d = u.V[j]; // u: (d, b, a)
                Triangle uNextB = u.N[(j + 2) % 3]; // across (d, b)
                bool uNextBCon = u.C[(j + 2) % 3];
                Triangle uNextA = u.N[(j + 1) % 3]; // across (a, d)
                bool uNextACon = u.C[(j + 1) % 3];

                // u -> t3 (p, d, b) and t4 (p, a, d)
                Triangle t3 = u;
                Triangle t4 = NewTriangle(p, a, d);
                SetVertices(t3, p, d, b);

                t3.N[0] = uNextB; t3.C[0] = uNextBCon;
                t3.N[1] = t2; t3.C[1] = constrained;    // (b, p)
                t3.N[2] = t4; t3.C[2] = false;          // (p, d)
                t4.N[0] = uNextA; t4.C[0] = uNextACon;
                t4.N[1] = t3; t4.C[1] = false;          // (d, p)
                t4.N[2] = t1; t4.C[2] = constrained;    // (p, a)
                ReplaceNeighbour(uNextA, u, t4);
                t4.Inside = u.Inside;

                t1.N[1] = t4;
                t2.N[2] = t3;

                stack.Push(t3);
                stack.Push(t4);
                Touch(t3);
                Touch(t4);
            }

            if (constrained)
            {
                _segments.Remove(SegmentKey(a, b));
                _segments.Add(SegmentKey(a, p));
                _segments.Add(SegmentKey(p, b));
            }

            Legalize(stack);
        }

        /// <summary>
        /// Lawson legalization: the triangles in the stack have the new vertex in V[0]; the opposite edge is flipped if not Delaunay
        /// </summary>
        private void Legalize(Stack<Triangle> stack)
        {
            int guard = 0;
            while (stack.Count > 0 && guard++ < 1000000)
            {
                Triangle t = stack.Pop();
                if (t.C[0] || t.N[0] is null)
                    continue;

                Triangle u = t.N[0];
                int j = IndexOfNeighbour(u, t);
                int q = u.V[j];

                if (InCircle(t.V[0], t.V[1], t.V[2], q) && IsFlippable(t, 0))
                {
                    Flip(t, 0);
                    stack.Push(t);
                    stack.Push(u);
                }
            }
        }

        /// <returns>True if the quadrilateral formed by <paramref name="t"/> and its neighbour across <paramref name="edge"/> is strictly convex</returns>
        private bool IsFlippable(Triangle t, int edge)
        {
            Triangle u = t.N[edge];
            if (u is null)
                return false;

            int p = t.V[edge];
            int a = t.V[(edge + 1) % 3];
            int b = t.V[(edge + 2) % 3];
            int q = u.V[IndexOfNeighbour(u, t)];

            double scale = Distance(p, q);
            return Orient(p, q, a) < -1E-14 * scale * Distance(p, a) && Orient(p, q, b) > 1E-14 * scale * Distance(p, b);
        }

        /// <summary>
        /// Flip the edge <paramref name="edge"/> of <paramref name="t"/>: t (p, a, b) and u (q, b, a) become t (p, a, q) and u (p, q, b)
        /// </summary>
        private void Flip(Triangle t, int edge)
        {
            Triangle u = t.N[edge];
            int j = IndexOfNeighbour(u, t);

            int p = t.V[edge];
            int a = t.V[(edge + 1) % 3];
            int b = t.V[(edge + 2) % 3];
            int q = u.V[j];

            Triangle nBP = t.N[(edge + 1) % 3]; bool cBP = t.C[(edge + 1) % 3];
            Triangle nPA = t.N[(edge + 2) % 3]; bool cPA = t.C[(edge + 2) % 3];
            Triangle nAQ = u.N[(j + 1) % 3]; bool cAQ = u.C[(j + 1) % 3];
            Triangle nQB = u.N[(j + 2) % 3]; bool cQB = u.C[(j + 2) % 3];

            SetVertices(t, p, a, q);
            t.N[0] = nAQ; t.C[0] = cAQ;
            t.N[1] = u; t.C[1] = false;
            t.N[2] = nPA; t.C[2] = cPA;

            SetVertices(u, p, q, b);
            u.N[0] = nQB; u.C[0] = cQB;
            u.N[1] = nBP; u.C[1] = cBP;
            u.N[2] = t; u.C[2] = false;

            ReplaceNeighbour(nAQ, u, t);
            ReplaceNeighbour(nBP, t, u);

            Touch(t);
            Touch(u);
            Validate("Flip");
        }

        #endregion

        #region Boundary segments

        /// <summary>
        /// Find the edge a-b
        /// </summary>
        /// <param name="t">A triangle with the edge</param>
        /// <param name="edge">Index of the edge in <paramref name="t"/></param>
        private bool FindEdge(int a, int b, out Triangle t, out int edge)
        {
            foreach (Triangle triangle in Fan(a))
            {
                int k = IndexOf(triangle, a);
                if (triangle.V[(k + 1) % 3] == b)
                {
                    t = triangle;
                    edge = (k + 2) % 3;
                    return true;
                }
                if (triangle.V[(k + 2) % 3] == b)
                {
                    t = triangle;
                    edge = (k + 1) % 3;
                    return true;
                }
            }

            t = null;
            edge = -1;
            return false;
        }

        /// <returns>The triangles around the vertex, counterclockwise</returns>
        private IEnumerable<Triangle> Fan(int vertex)
        {
            Triangle start = _vertexTriangle[vertex];
            if (start is null || IndexOf(start, vertex) < 0)
            {
                start = null;
                foreach (Triangle triangle in _triangles)
                {
                    if (IndexOf(triangle, vertex) >= 0)
                    {
                        start = triangle;
                        break;
                    }
                }
                if (start is null)
                    yield break;
                _vertexTriangle[vertex] = start;
            }

            Triangle t = start;
            int guard = 0;
            do
            {
                yield return t;
                Triangle next = t.N[(IndexOf(t, vertex) + 1) % 3];
                if (next is null)
                    break;
                t = next;
            }
            while (t != start && guard++ < 100000);

            if (t != start)
            {
                // the fan is open (vertex of the super triangle): the other side
                t = start;
                while (true)
                {
                    Triangle previous = t.N[(IndexOf(t, vertex) + 2) % 3];
                    if (previous is null || previous == start || guard++ > 100000)
                        break;
                    t = previous;
                    yield return t;
                }
            }
        }

        private void MarkSegment(Triangle t, int edge)
        {
            t.C[edge] = true;
            Triangle u = t.N[edge];
            if (u != null)
                u.C[IndexOfNeighbour(u, t)] = true;

            _segments.Add(SegmentKey(t.V[(edge + 1) % 3], t.V[(edge + 2) % 3]));
        }

        /// <summary>
        /// Make the segment a-b an edge of the triangulation (Sloan's algorithm: flip of the edges crossed by the segment) and mark it as boundary
        /// </summary>
        private void RecoverSegment(int a, int b, int depth)
        {
            if (a == b)
                return;

            if (depth > 1000)
                throw new InvalidOperationException("Can not recover the boundary segment");

            if (FindEdge(a, b, out Triangle t, out int edge))
            {
                MarkSegment(t, edge);
                return;
            }

            var crossing = new Queue<KeyValuePair<int, int>>();
            int collinear = FindCrossedEdges(a, b, crossing);
            if (collinear >= 0)
            {
                // a vertex on the segment: two segments
                RecoverSegment(a, collinear, depth + 1);
                RecoverSegment(collinear, b, depth + 1);
                return;
            }

            var newEdges = new List<KeyValuePair<int, int>>();
            int guard = 0;
            int maxGuard = 1000 + 50 * crossing.Count * crossing.Count;

            while (crossing.Count > 0)
            {
                if (guard++ > maxGuard)
                    throw new InvalidOperationException("Can not recover the boundary segment: the boundary of the shape may intersect itself");

                KeyValuePair<int, int> crossed = crossing.Dequeue();
                if (!FindEdge(crossed.Key, crossed.Value, out t, out edge))
                    continue;

                if (t.C[edge])
                    throw new InvalidOperationException("The boundary of the shape intersects itself");

                if (!IsFlippable(t, edge))
                {
                    crossing.Enqueue(crossed);
                    continue;
                }

                int p = t.V[edge];
                Flip(t, edge);
                int q = t.V[2]; // after the flip t is (p, a, q)

                if (p != a && p != b && q != a && q != b && SegmentsCross(a, b, p, q))
                    crossing.Enqueue(new KeyValuePair<int, int>(p, q));
                else
                    newEdges.Add(new KeyValuePair<int, int>(p, q));
            }

            if (!FindEdge(a, b, out t, out edge))
                throw new InvalidOperationException("Can not recover the boundary segment");

            MarkSegment(t, edge);
            Validate("RecoverSegment");

            // Delaunay property of the new edges
            bool swapped = true;
            for (int iteration = 0; swapped && iteration < 100; iteration++)
            {
                swapped = false;
                for (int i = 0; i < newEdges.Count; i++)
                {
                    int p = newEdges[i].Key, q = newEdges[i].Value;
                    if (!FindEdge(p, q, out t, out edge) || t.C[edge] || t.N[edge] is null)
                        continue;

                    Triangle u = t.N[edge];
                    int opposite = u.V[IndexOfNeighbour(u, t)];
                    if (InCircle(t.V[0], t.V[1], t.V[2], opposite) && IsFlippable(t, edge))
                    {
                        int apex = t.V[edge];
                        Flip(t, edge);
                        newEdges[i] = new KeyValuePair<int, int>(apex, opposite);
                        swapped = true;
                    }
                }
            }
        }

        /// <summary>
        /// Find the edges crossed by the segment a-b, walking from a
        /// </summary>
        /// <returns>A vertex on the segment (between a and b), -1 if there is none</returns>
        private int FindCrossedEdges(int a, int b, Queue<KeyValuePair<int, int>> crossing)
        {
            Triangle t = null;
            int left = -1, right = -1;

            foreach (Triangle triangle in Fan(a))
            {
                int k = IndexOf(triangle, a);
                int x = triangle.V[(k + 1) % 3];
                int y = triangle.V[(k + 2) % 3];

                if (IsOnSegment(a, b, x, MergeTolerance))
                    return x;
                if (IsOnSegment(a, b, y, MergeTolerance))
                    return y;

                // the segment leaves a between x (on its right) and y (on its left)
                if (Orient(a, b, x) < 0 && Orient(a, b, y) > 0)
                {
                    t = triangle;
                    right = x;
                    left = y;
                    break;
                }
            }

            if (t is null)
                throw new InvalidOperationException("Can not find the boundary segment in the triangulation");

            int guard = 0;
            while (guard++ < 10 * _triangles.Count + 10)
            {
                crossing.Enqueue(new KeyValuePair<int, int>(right, left));

                // the triangle on the other side of the crossed edge
                Triangle u = t.N[OppositeIndex(t, right, left)];
                if (u is null)
                    throw new InvalidOperationException("Can not find the boundary segment in the triangulation");

                int w = u.V[OppositeIndex(u, right, left)];
                if (w == b)
                    return -1;

                if (IsOnSegment(a, b, w, MergeTolerance))
                    return w;

                if (Orient(a, b, w) > 0)
                    left = w;
                else
                    right = w;

                t = u;
            }

            throw new InvalidOperationException("Can not find the boundary segment in the triangulation");
        }

        private static int OppositeIndex(Triangle t, int v1, int v2)
        {
            for (int k = 0; k < 3; k++)
            {
                if (t.V[k] != v1 && t.V[k] != v2)
                    return k;
            }
            return -1;
        }

        /// <returns>True if the vertex p is between a and b, closer than <paramref name="tolerance"/> to the segment</returns>
        private bool IsOnSegment(int a, int b, int p, double tolerance)
        {
            if (p == a || p == b)
                return false;

            if (Math.Abs(Orient(a, b, p)) > tolerance * Distance(a, b))
                return false;

            double dot = (_x[p] - _x[a]) * (_x[b] - _x[a]) + (_y[p] - _y[a]) * (_y[b] - _y[a]);
            double squareLength = (_x[b] - _x[a]) * (_x[b] - _x[a]) + (_y[b] - _y[a]) * (_y[b] - _y[a]);
            return dot > 0 && dot < squareLength;
        }

        /// <returns>True if the segments a-b and p-q cross in a point inside both</returns>
        private bool SegmentsCross(int a, int b, int p, int q)
        {
            double o1 = Orient(a, b, p);
            double o2 = Orient(a, b, q);
            double o3 = Orient(p, q, a);
            double o4 = Orient(p, q, b);
            return ((o1 > 0 && o2 < 0) || (o1 < 0 && o2 > 0)) && ((o3 > 0 && o4 < 0) || (o3 < 0 && o4 > 0));
        }

        #endregion

        #region Inside / outside

        /// <summary>
        /// A triangle is inside the shape if it is reached from the super triangle crossing an odd number of boundary segments
        /// </summary>
        private void ClassifyTriangles()
        {
            var depth = new Dictionary<Triangle, int>(_triangles.Count);
            var queue = new Queue<Triangle>();

            Triangle start = _vertexTriangle[0];
            depth[start] = 0;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Triangle t = queue.Dequeue();
                int d = depth[t];
                t.Inside = d % 2 == 1;

                for (int k = 0; k < 3; k++)
                {
                    Triangle n = t.N[k];
                    if (n is null || depth.ContainsKey(n))
                        continue;

                    depth[n] = t.C[k] ? d + 1 : d;
                    queue.Enqueue(n);
                }
            }
        }

        #endregion

        #region Refinement

        private void Refine(double size, double minAngle)
        {
            double maxSquareRadius = SizeFactor * size * SizeFactor * size;
            double minSegmentLength = Math.Max(1E-8, 1E-4 * size);

            // circumradius / shortest edge = 1 / (2 sin(minimum angle))
            double maxRatio = minAngle > 0 ? 1.0 / (2.0 * Math.Sin(Math.Min(minAngle, 30.0) * Math.PI / 180.0)) : double.PositiveInfinity;

            // upper limit of the points, in case the refinement does not converge (e.g. very small angles of the shape)
            double area = 0;
            foreach (Triangle t in _triangles)
            {
                if (t.Inside)
                    area += Orient(t.V[0], t.V[1], t.V[2]) / 2.0;
            }
            int boundaryVertices = _x.Count;
            long expected = (long)(area / (0.433 * size * size));
            int maxInserted = (int)Math.Min(2000000, 20 * expected + 20 * boundaryVertices + 100);

            _refineQueue = new Queue<KeyValuePair<Triangle, int>>();
            foreach (Triangle t in _triangles)
            {
                if (t.Inside)
                    _refineQueue.Enqueue(new KeyValuePair<Triangle, int>(t, t.Stamp));
            }

            int inserted = 0;
            var encroached = new List<long>();

            while (_refineQueue.Count > 0 && inserted < maxInserted)
            {
                KeyValuePair<Triangle, int> item = _refineQueue.Dequeue();
                Triangle t = item.Key;
                if (t.Stamp != item.Value || !t.Inside)
                    continue;

                if (!IsBad(t, maxSquareRadius, maxRatio))
                    continue;

                Circumcenter(t, out double cx, out double cy, out _);
                if (double.IsNaN(cx) || double.IsInfinity(cx) || double.IsNaN(cy) || double.IsInfinity(cy))
                    continue;

                // the circumcenter inside the diametral circle of a boundary segment: the segment is split instead
                encroached.Clear();
                foreach (long key in _segments)
                {
                    int a = (int)(key >> 32), b = (int)(key & 0xFFFFFFFF);
                    if ((_x[a] - cx) * (_x[b] - cx) + (_y[a] - cy) * (_y[b] - cy) < 0 && Distance(a, b) > minSegmentLength)
                        encroached.Add(key);
                }

                if (encroached.Count > 0)
                {
                    foreach (long key in encroached)
                    {
                        if (SplitSegment((int)(key >> 32), (int)(key & 0xFFFFFFFF)))
                            inserted++;
                    }
                    _refineQueue.Enqueue(new KeyValuePair<Triangle, int>(t, t.Stamp));
                    continue;
                }

                // the circumcenter must be visible from the triangle: otherwise the segment in between is split
                Triangle target = Walk(t, cx, cy, out Triangle blocked, out int blockedEdge);
                if (target is null)
                {
                    if (blocked != null && Distance(blocked.V[(blockedEdge + 1) % 3], blocked.V[(blockedEdge + 2) % 3]) > minSegmentLength &&
                        SplitSegment(blocked.V[(blockedEdge + 1) % 3], blocked.V[(blockedEdge + 2) % 3]))
                    {
                        inserted++;
                        _refineQueue.Enqueue(new KeyValuePair<Triangle, int>(t, t.Stamp));
                    }
                    continue;
                }

                if (!target.Inside)
                    continue;

                int count = _x.Count;
                InsertPoint(cx, cy, double.NaN, double.NaN, -1, target);
                if (_x.Count > count)
                    inserted++;
            }

            _refineQueue = null;
        }

        private bool IsBad(Triangle t, double maxSquareRadius, double maxRatio)
        {
            Circumcenter(t, out _, out _, out double squareRadius);

            if (squareRadius > maxSquareRadius)
                return true;

            if (double.IsInfinity(maxRatio))
                return false;

            // shortest edge and the vertex opposite to it (the smallest angle)
            int shortest = 0;
            double shortestLength = double.MaxValue;
            for (int k = 0; k < 3; k++)
            {
                double length = Distance(t.V[(k + 1) % 3], t.V[(k + 2) % 3]);
                if (length < shortestLength)
                {
                    shortestLength = length;
                    shortest = k;
                }
            }

            if (!(Math.Sqrt(squareRadius) > maxRatio * shortestLength))
                return false;

            // small angle between two boundary segments: it is an angle of the shape, it can not be improved
            return !(t.C[(shortest + 1) % 3] && t.C[(shortest + 2) % 3]);
        }

        /// <summary>
        /// Visibility walk from <paramref name="start"/> to the point, without crossing the boundary segments
        /// </summary>
        /// <returns>The triangle containing the point, null if a boundary segment is in between (<paramref name="blocked"/>, <paramref name="blockedEdge"/>)</returns>
        private Triangle Walk(Triangle start, double x, double y, out Triangle blocked, out int blockedEdge)
        {
            blocked = null;
            blockedEdge = -1;
            Triangle t = start;
            int maxSteps = 4 * _triangles.Count + 16;

            for (int step = 0; step < maxSteps; step++)
            {
                int exit = -1;
                for (int k = 0; k < 3; k++)
                {
                    int e = (k + step) % 3;
                    if (IsOutside(t.V[(e + 1) % 3], t.V[(e + 2) % 3], x, y))
                    {
                        exit = e;
                        break;
                    }
                }

                if (exit < 0)
                    return t;

                if (t.C[exit])
                {
                    blocked = t;
                    blockedEdge = exit;
                    return null;
                }

                t = t.N[exit];
                if (t is null)
                    return null;
            }

            return null;
        }

        /// <summary>
        /// Split the boundary segment a-b in its midpoint
        /// </summary>
        private bool SplitSegment(int a, int b)
        {
            if (!_segments.Contains(SegmentKey(a, b)) || !FindEdge(a, b, out Triangle t, out int edge))
                return false;

            // the midpoint is computed also in the original coordinates, so it is exactly on the boundary of the shape
            int p = AddVertex((_x[a] + _x[b]) / 2.0, (_y[a] + _y[b]) / 2.0, (_originalX[a] + _originalX[b]) / 2.0, (_originalY[a] + _originalY[b]) / 2.0, -1);
            SplitEdge(t, edge, p);
            Validate("SplitSegment");
            return true;
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Consistency check of the triangulation, compiled only with the symbol DEBUG_TRIANGULATION
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG_TRIANGULATION")]
        private void Validate(string step)
        {
            double area = 0;
            foreach (Triangle t in _triangles)
                area += Orient(t.V[0], t.V[1], t.V[2]);
            double superArea = Orient(0, 1, 2);
            if (Math.Abs(area - superArea) > 1E-9 * superArea)
                throw new InvalidOperationException($"{step}: the triangles do not cover the super triangle ({area:R} instead of {superArea:R}), vertices {_x.Count}");

            foreach (Triangle t in _triangles)
            {
                if (!(Orient(t.V[0], t.V[1], t.V[2]) > 0))
                    throw new InvalidOperationException($"{step}: triangle {t.V[0]} {t.V[1]} {t.V[2]} not counterclockwise ({Orient(t.V[0], t.V[1], t.V[2]):E3})");

                for (int k = 0; k < 3; k++)
                {
                    Triangle n = t.N[k];
                    if (n is null)
                        continue;

                    int j = IndexOfNeighbour(n, t);
                    if (j < 0)
                        throw new InvalidOperationException($"{step}: neighbour not symmetric");

                    int a = t.V[(k + 1) % 3], b = t.V[(k + 2) % 3];
                    if (!(n.V[(j + 1) % 3] == b && n.V[(j + 2) % 3] == a))
                        throw new InvalidOperationException($"{step}: neighbour with a different edge");

                    if (n.C[j] != t.C[k])
                        throw new InvalidOperationException($"{step}: boundary flag not symmetric");
                }
            }
        }

        #endregion

        #region Output

        private Mesh ToMesh()
        {
            var mesh = new Mesh();
            var vertexIds = new int[_x.Count];
            for (int i = 0; i < vertexIds.Length; i++)
                vertexIds[i] = -1;

            int vertexId = 0;
            int faceId = 0;
            int edgeId = 0;
            var edges = new HashSet<long>();

            foreach (Triangle t in _triangles)
            {
                if (!t.Inside)
                    continue;

                for (int k = 0; k < 3; k++)
                {
                    int v = t.V[k];
                    if (vertexIds[v] < 0)
                    {
                        vertexIds[v] = vertexId;
                        mesh.Vertices.Add(new MeshVertex(new Point3d(_originalX[v], _originalY[v], 0)) { Tag = _tags[v] }, vertexId);
                        vertexId++;
                    }
                }

                mesh.Faces.Add(new MeshFace(vertexIds[t.V[0]], vertexIds[t.V[1]], vertexIds[t.V[2]]), faceId++);

                for (int k = 0; k < 3; k++)
                {
                    int a = vertexIds[t.V[(k + 1) % 3]];
                    int b = vertexIds[t.V[(k + 2) % 3]];
                    if (edges.Add(SegmentKey(a, b)))
                        mesh.Edges.Add(new MeshEdge(a, b), edgeId++);
                }
            }

            return mesh;
        }

        #endregion
    }
}
