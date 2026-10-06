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
    /// <para>For the quadrilateral meshes (<see cref="Quadrangulate"/>) the points inside are the nodes of a lattice of rectangles
    /// (see <see cref="Lattice"/>) instead of the circumcenters, then the triangles are recombined by <see cref="QuadRecombination"/>.</para>
    /// </summary>
    internal sealed class ConstrainedDelaunay
    {
        #region Types and constants

        /// <summary>
        /// A triangle of the triangulation, with its neighbours and the boundary flags of its edges
        /// </summary>
        private sealed class Triangle
        {
            /// <summary>
            /// The vertices, counterclockwise
            /// </summary>
            public readonly int[] V = new int[3];
            /// <summary>
            /// N[i]: the neighbour across the edge opposite to V[i] (null on the super triangle)
            /// </summary>
            public readonly Triangle[] N = new Triangle[3];
            /// <summary>
            /// C[i]: the edge opposite to V[i] is a boundary segment
            /// </summary>
            public readonly bool[] C = new bool[3];
            /// <summary>
            /// True if the triangle is inside the shape
            /// </summary>
            public bool Inside;
            /// <summary>
            /// Changed at every modification, to discard the old entries of the refinement queue
            /// </summary>
            public int Stamp;
        }

        /// <summary>
        /// Normalized distance under which two points are the same vertex
        /// </summary>
        private const double MergeTolerance = 1E-10;
        /// <summary>
        /// Normalized distance: a point on an edge (with rounding errors) is inside both triangles
        /// </summary>
        private const double LocateTolerance = 1E-13;
        /// <summary>
        /// The half size of the super triangle: the normalized shape is inside [-0.5, 0.5] x [-0.5, 0.5]
        /// </summary>
        private const double SuperTriangleSize = 20.0;

        /// <summary>
        /// A triangle is too large if its circumradius is greater than this factor by the mesh size: 1.25 / sqrt(3), i.e. an equilateral triangle
        /// with the edges 25% longer than the mesh size
        /// </summary>
        private const double SizeFactor = 0.72;

        /// <summary>
        /// Lines of the lattice used for the quadrilateral meshes, in the normalized coordinates rotated by the angle of the lattice
        /// (u = x cos + y sin, v = -x sin + y cos). The lines pass through the boundary edges parallel to the lattice and their spacing
        /// is at most the mesh size, so the shapes made of edges parallel to the axes (rectangles, T, L, I, box sections) are divided
        /// in rectangles exactly
        /// </summary>
        private sealed class Lattice
        {
            /// <summary>
            /// The cosine of the angle of the lattice
            /// </summary>
            public double Cos = 1.0;
            /// <summary>
            /// The sine of the angle of the lattice
            /// </summary>
            public double Sin;
            /// <summary>
            /// The u coordinates of the lines u = constant, increasing
            /// </summary>
            public double[] U = new double[0];
            /// <summary>
            /// The v coordinates of the lines v = constant, increasing
            /// </summary>
            public double[] V = new double[0];

            /// <summary>
            /// The u coordinate of a point
            /// </summary>
            /// <param name="x">The normalized X</param>
            /// <param name="y">The normalized Y</param>
            /// <returns>x cos + y sin</returns>
            public double ToU(double x, double y) => x * Cos + y * Sin;
            /// <summary>
            /// The v coordinate of a point
            /// </summary>
            /// <param name="x">The normalized X</param>
            /// <param name="y">The normalized Y</param>
            /// <returns>-x sin + y cos</returns>
            public double ToV(double x, double y) => -x * Sin + y * Cos;
            /// <summary>
            /// The normalized X of a point of the lattice
            /// </summary>
            /// <param name="u">The u coordinate</param>
            /// <param name="v">The v coordinate</param>
            /// <returns>u cos - v sin</returns>
            public double ToX(double u, double v) => u * Cos - v * Sin;
            /// <summary>
            /// The normalized Y of a point of the lattice
            /// </summary>
            /// <param name="u">The u coordinate</param>
            /// <param name="v">The v coordinate</param>
            /// <returns>u sin + v cos</returns>
            public double ToY(double u, double v) => u * Sin + v * Cos;
        }

        /// <summary>
        /// Radians: edges with the same direction (angle of the lattice)
        /// </summary>
        private const double AngleTolerance = 1E-7;
        /// <summary>
        /// |sin| of the angle between an edge and the lattice under which the edge is parallel to the lattice
        /// </summary>
        private const double AlignedTolerance = 1E-6;
        /// <summary>
        /// Relative to the mesh size: an extreme of the shape closer than this to a line of an edge is merged (and division points closer than
        /// this to the ends are skipped)
        /// </summary>
        private const double LineMerge = 0.05;
        /// <summary>
        /// Relative to the mesh size: lines of two edges closer than this are merged (thin parts keep their lines)
        /// </summary>
        private const double EdgeLineMerge = 1E-3;
        /// <summary>
        /// Relative to the mesh size: minimum distance of a lattice point from the boundary segments parallel to the lattice
        /// </summary>
        private const double AlignedClearance = 0.1;
        /// <summary>
        /// Relative to the mesh size: minimum distance of a lattice point from the other boundary segments
        /// </summary>
        private const double Clearance = 0.45;
        /// <summary>
        /// Degrees: minimum angle of the triangles not made of regular points (grading near the small edges)
        /// </summary>
        private const double LatticeMinAngle = 20.0;

        #endregion

        #region Variables

        /// <summary>
        /// The normalized X of the vertices
        /// </summary>
        private readonly List<double> _x = new List<double>();
        /// <summary>
        /// The normalized Y of the vertices
        /// </summary>
        private readonly List<double> _y = new List<double>();
        /// <summary>
        /// The original X of the vertices (the input points keep their exact coordinates)
        /// </summary>
        private readonly List<double> _originalX = new List<double>();
        /// <summary>
        /// The original Y of the vertices
        /// </summary>
        private readonly List<double> _originalY = new List<double>();
        /// <summary>
        /// Index of the boundary points, -1 for the points added by the refinement (the tags of the vertices of the mesh)
        /// </summary>
        private readonly List<int> _tags = new List<int>();
        /// <summary>
        /// True for the points inside the shape placed on the lattice
        /// </summary>
        private readonly List<bool> _latticePoints = new List<bool>();
        /// <summary>
        /// A triangle of each vertex (the start of the searches around the vertex)
        /// </summary>
        private readonly List<Triangle> _vertexTriangle = new List<Triangle>();
        /// <summary>
        /// All the triangles, also the ones outside the shape
        /// </summary>
        private readonly List<Triangle> _triangles = new List<Triangle>();
        /// <summary>
        /// The boundary segments (constrained edges), by <see cref="SegmentKey"/>
        /// </summary>
        private readonly HashSet<long> _segments = new HashSet<long>(LongKeyComparer.Instance);
        /// <summary>
        /// The number of loop edges on each boundary segment (by <see cref="SegmentKey"/>), for the classification of the triangles: a segment of
        /// two loops (e.g. a hole with an edge on the fill) does not change the side. The segments split later are not counted
        /// </summary>
        private readonly Dictionary<long, int> _segmentLoops = new Dictionary<long, int>(LongKeyComparer.Instance);

        /// <summary>
        /// The X of the center of the bounding box (normalization)
        /// </summary>
        private double _centerX;
        /// <summary>
        /// The Y of the center of the bounding box (normalization)
        /// </summary>
        private double _centerY;
        /// <summary>
        /// The largest side of the bounding box (normalization)
        /// </summary>
        private double _scale;
        /// <summary>
        /// The last triangle found by the point location (the start of the next search)
        /// </summary>
        private Triangle _lastTriangle;
        /// <summary>
        /// The lattice: not null for the quadrilateral meshes
        /// </summary>
        private Lattice _lattice;
        /// <summary>
        /// Lattice points and boundary points only on edges parallel to the lattice
        /// </summary>
        private HashSet<int> _regularPoints;

        /// <summary>
        /// The triangles to check (with their stamp): not null during the refinement
        /// </summary>
        private Queue<KeyValuePair<Triangle, int>> _refineQueue;
        /// <summary>
        /// The boundary segments (by <see cref="SegmentKey"/>) in the cells of a grid, each one in the cells covered by its diametral circle:
        /// not null during the refinement. The keys of the segments split later remain in the cells and are discarded with <see cref="_segments"/>
        /// </summary>
        private Dictionary<long, List<long>> _encroachmentCells;
        /// <summary>
        /// The size of the cells of <see cref="_encroachmentCells"/> (the normalized mesh size)
        /// </summary>
        private double _encroachmentCellSize;

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
            var triangulation = new ConstrainedDelaunay();
            triangulation.Build(GetLoops(shape), meshSize, refine, minAngle, false);
            return triangulation.ToMesh();
        }

        /// <summary>
        /// Mesh of quadrilaterals as square as possible: the points inside are the nodes of a lattice of rectangles (at most as large as
        /// the mesh size) and the triangles are recombined in pairs (see <see cref="QuadRecombination"/>)
        /// </summary>
        /// <param name="shape">The shape: fill, holes and childs (shapes inside the holes)</param>
        /// <param name="meshSize">Maximum length of the edges of the elements. Infinite or not positive: only the vertices of the shape are used
        /// (unless <paramref name="allQuads"/>)</param>
        /// <param name="refine">If false the interior is not refined: only the points on the boundary are used</param>
        /// <param name="allQuads">If true the mesh is made only of quadrilaterals: the triangles left by the recombination are eliminated by
        /// channels of quadrilaterals divided in two (see <see cref="QuadRecombination.EliminateTriangles"/>); if it is not possible, every
        /// quadrilateral is divided in four and every triangle in three quadrilaterals. If false, the triangles that can not form a good
        /// quadrilateral remain (usually few, along the curved or slanted boundaries)</param>
        /// <returns>The mesh of quadrilaterals and triangles, counterclockwise, in the plane of the shape</returns>
        public static Mesh Quadrangulate(Shape shape, double meshSize, bool refine, bool allQuads)
        {
            List<List<double[]>> loops = GetLoops(shape);

            var triangulation = new ConstrainedDelaunay();
            triangulation.Build(loops, meshSize, refine, 0, true);

            QuadRecombination recombination = triangulation.ToRecombination();
            recombination.Recombine();

            if (allQuads)
            {
                if (recombination.EliminateTriangles())
                {
                    recombination.Smooth();
                    if (recombination.WorstElementQuality() > 0.05)
                        return recombination.ToMesh();
                }

                // fallback: the subdivision always gives convex quadrilaterals
                recombination = triangulation.ToRecombination();
                recombination.Recombine();
                recombination.Subdivide();
            }

            recombination.Smooth();
            return recombination.ToMesh();
        }

        #endregion

        #region Input

        /// <summary>
        /// The loops of a shape: fill, holes and, recursively, fill and holes of the children
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns>The loops, without consecutive duplicated points; the first one is the fill</returns>
        /// <exception cref="ArgumentNullException">If the shape is null</exception>
        /// <exception cref="ArgumentException">If the fill has less than three distinct points</exception>
        private static List<List<double[]>> GetLoops(Shape shape)
        {
            if (shape is null)
                throw new ArgumentNullException(nameof(shape));

            var loops = new List<List<double[]>>();
            CollectLoops(shape, loops);

            if (loops.Count == 0 || loops[0].Count < 3)
                throw new ArgumentException("The shape must have a fill with at least three distinct points");

            return loops;
        }

        /// <summary>
        /// Adds the loops of a shape and of its children
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="loops">The list of the loops</param>
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

        /// <summary>
        /// Tell if two input points are the same (relative tolerance 1E-12)
        /// </summary>
        /// <param name="a">The first point (X, Y)</param>
        /// <param name="b">The second point (X, Y)</param>
        /// <returns>True if the points coincide</returns>
        private static bool SamePoint(double[] a, double[] b)
        {
            double tolerance = 1E-12 * Math.Max(1.0, Math.Max(Math.Abs(a[0]), Math.Abs(a[1])));
            return Math.Abs(a[0] - b[0]) <= tolerance && Math.Abs(a[1] - b[1]) <= tolerance;
        }

        #endregion

        #region Build

        /// <summary>
        /// Builds the triangulation: normalization, super triangle, boundary points and segments, classification of the triangles inside,
        /// lattice and refinement
        /// </summary>
        /// <param name="loops">The loops of the shape</param>
        /// <param name="meshSize">The mesh size (original units)</param>
        /// <param name="refine">If false only the boundary points are used</param>
        /// <param name="minAngle">The minimum angle of the triangles (degrees), 0: only the size</param>
        /// <param name="lattice">True for the quadrilateral meshes (points inside on a lattice)</param>
        /// <exception cref="ArgumentException">If the shape has zero or infinite size</exception>
        private void Build(List<List<double[]>> loops, double meshSize, bool refine, double minAngle, bool lattice)
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

            if (lattice && !double.IsInfinity(size))
                _lattice = BuildLattice(loops, size);

            // Super triangle
            AddVertex(-SuperTriangleSize, -SuperTriangleSize, double.NaN, double.NaN, -1);
            AddVertex(SuperTriangleSize, -SuperTriangleSize, double.NaN, double.NaN, -1);
            AddVertex(0, SuperTriangleSize, double.NaN, double.NaN, -1);
            NewTriangle(0, 1, 2);

            // Boundary points, divided according to the mesh size
            var pointX = new List<double>();
            var pointY = new List<double>();
            var pointLoop = new List<int>();
            for (int l = 0; l < loops.Count; l++)
            {
                List<double[]> loop = loops[l];
                for (int i = 0; i < loop.Count; i++)
                {
                    double[] a = loop[i];
                    double[] b = loop[(i + 1) % loop.Count];

                    pointX.Add(a[0]);
                    pointY.Add(a[1]);
                    pointLoop.Add(l);

                    foreach (double t in Divisions(a, b, size))
                    {
                        pointX.Add(a[0] + t * (b[0] - a[0]));
                        pointY.Add(a[1] + t * (b[1] - a[1]));
                        pointLoop.Add(l);
                    }
                }
            }

            // inserted in a randomized order: in the order of the loops the points of an edge are inside the circumcircles of the triangles
            // of the edge in front of it (e.g. a thin strip), and the number of flips grows with the square of the points
            var pointVertex = new int[pointX.Count];
            foreach (int i in InsertionOrder(pointX.Count))
                pointVertex[i] = InsertPoint((pointX[i] - _centerX) / _scale, (pointY[i] - _centerY) / _scale, pointX[i], pointY[i], -1, null);

            // the vertices of the loops; the tags and the original coordinates are given in the order of the loops, the first occurrence of
            // coincident points (e.g. a hole touching the fill, merged in one vertex) wins
            var loopVertices = new List<List<int>>();
            int tag = 0;
            for (int i = 0; i < pointX.Count; i++)
            {
                if (i == 0 || pointLoop[i] != pointLoop[i - 1])
                    loopVertices.Add(new List<int>());

                List<int> vertices = loopVertices[loopVertices.Count - 1];
                int vertex = pointVertex[i];
                if (_tags[vertex] < 0)
                {
                    _tags[vertex] = tag++;
                    _originalX[vertex] = pointX[i];
                    _originalY[vertex] = pointY[i];
                }

                if (vertices.Count == 0 || vertices[vertices.Count - 1] != vertex)
                    vertices.Add(vertex);
            }

            // Boundary segments
            foreach (var vertices in loopVertices)
            {
                for (int i = 0; i < vertices.Count; i++)
                    RecoverSegment(vertices[i], vertices[(i + 1) % vertices.Count], 0);
            }

            ClassifyTriangles();

            if (refine && !double.IsInfinity(size))
            {
                // with the lattice only the size is checked: the angles of the rectangles are right
                if (_lattice != null)
                {
                    List<int> layer = InsertBoundaryLayer(loopVertices, size);
                    InsertLatticePoints(size, layer);
                    _regularPoints = RegularPoints();
                }
                Refine(size, _lattice is null ? minAngle : LatticeMinAngle);
            }
        }

        /// <summary>
        /// Division of the boundary edge a-b: the edges parallel to the lattice are divided by the lattice lines, the others in equal parts
        /// </summary>
        /// <returns>The parameters (between 0 and 1, increasing) of the division points</returns>
        private List<double> Divisions(double[] a, double[] b, double size)
        {
            var parameters = new List<double>();
            if (double.IsInfinity(size))
                return parameters;

            double ax = (a[0] - _centerX) / _scale, ay = (a[1] - _centerY) / _scale;
            double bx = (b[0] - _centerX) / _scale, by = (b[1] - _centerY) / _scale;
            double length = Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));

            if (_lattice != null && IsParallelToLattice(ax, ay, bx, by, out bool alongU))
            {
                double start = alongU ? _lattice.ToU(ax, ay) : _lattice.ToV(ax, ay);
                double delta = (alongU ? _lattice.ToU(bx, by) : _lattice.ToV(bx, by)) - start;
                double margin = LineMerge * size / length;

                foreach (double line in alongU ? _lattice.U : _lattice.V)
                {
                    double t = (line - start) / delta;
                    if (t > margin && t < 1.0 - margin)
                        parameters.Add(t);
                }

                parameters.Sort();
                return parameters;
            }

            int divisions = Math.Max(1, (int)Math.Ceiling(length / size - 1E-9));
            for (int k = 1; k < divisions; k++)
                parameters.Add((double)k / divisions);

            return parameters;
        }

        /// <summary>
        /// Biased randomized insertion order (Amenta, Choi and Rote): the points are shuffled (with a fixed seed) and divided in rounds that double
        /// in size, each one sorted in the order of the loops. The random order keeps few flips per point, the order of the loops keeps short the
        /// walks of the point location. The order depends only on the number of points: the mesh does not depend on the units and on the position
        /// (a spatial sorting can change with the rounding, and the triangulation of cocircular points depends on the order)
        /// </summary>
        /// <param name="count">The number of points</param>
        /// <returns>The indices of the points in the insertion order</returns>
        private static int[] InsertionOrder(int count)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++)
                order[i] = i;

            // Fisher-Yates shuffle with a xorshift generator
            ulong state = 0x9E3779B97F4A7C15UL;
            for (int i = count - 1; i > 0; i--)
            {
                state ^= state << 13;
                state ^= state >> 7;
                state ^= state << 17;
                int j = (int)(state % (ulong)(i + 1));
                int swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }

            // rounds [n/2, n), [n/4, n/2), ... [0, at most 32), each one sorted in the order of the loops
            for (int end = count; end > 0;)
            {
                int start = end > 32 ? end / 2 : 0;
                Array.Sort(order, start, end - start);
                end = start;
            }

            return order;
        }

        #endregion

        #region Vertices and triangles

        /// <summary>
        /// Adds a vertex (not yet in the triangulation)
        /// </summary>
        /// <param name="x">The normalized X</param>
        /// <param name="y">The normalized Y</param>
        /// <param name="originalX">The original X; NaN: computed from the normalized one</param>
        /// <param name="originalY">The original Y; NaN: computed from the normalized one</param>
        /// <param name="tag">The tag</param>
        /// <returns>The index of the vertex</returns>
        private int AddVertex(double x, double y, double originalX, double originalY, int tag)
        {
            _x.Add(x);
            _y.Add(y);
            _originalX.Add(double.IsNaN(originalX) ? x * _scale + _centerX : originalX);
            _originalY.Add(double.IsNaN(originalY) ? y * _scale + _centerY : originalY);
            _tags.Add(tag);
            _latticePoints.Add(false);
            _vertexTriangle.Add(null);
            return _x.Count - 1;
        }

        /// <summary>
        /// Creates a triangle and adds it to the triangulation
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="c">The third vertex (counterclockwise)</param>
        /// <returns>The triangle</returns>
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

        /// <summary>
        /// Sets the vertices of a triangle and makes it the triangle of its vertices
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="c">The third vertex (counterclockwise)</param>
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

        /// <summary>
        /// The position of a vertex in a triangle
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="vertex">The vertex</param>
        /// <returns>0, 1 or 2; -1 if the vertex is not in the triangle</returns>
        private static int IndexOf(Triangle t, int vertex)
        {
            if (t.V[0] == vertex) return 0;
            if (t.V[1] == vertex) return 1;
            if (t.V[2] == vertex) return 2;
            return -1;
        }

        /// <summary>
        /// The position of a neighbour in a triangle
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="neighbour">The neighbour</param>
        /// <returns>0, 1 or 2; -1 if it is not a neighbour</returns>
        private static int IndexOfNeighbour(Triangle t, Triangle neighbour)
        {
            if (t.N[0] == neighbour) return 0;
            if (t.N[1] == neighbour) return 1;
            if (t.N[2] == neighbour) return 2;
            return -1;
        }

        /// <summary>
        /// Replaces a neighbour of a triangle
        /// </summary>
        /// <param name="t">The triangle (null: nothing is done)</param>
        /// <param name="oldNeighbour">The neighbour to replace</param>
        /// <param name="newNeighbour">The new neighbour</param>
        private static void ReplaceNeighbour(Triangle t, Triangle oldNeighbour, Triangle newNeighbour)
        {
            if (t is null)
                return;

            int k = IndexOfNeighbour(t, oldNeighbour);
            if (k >= 0)
                t.N[k] = newNeighbour;
        }

        /// <summary>
        /// The key of a segment, independent of its direction
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <returns>The smaller index in the high 32 bits, the larger in the low ones</returns>
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

        /// <summary>
        /// Twice the signed area of the triangle a, b, (x, y)
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="x">The normalized X of the third point</param>
        /// <param name="y">The normalized Y of the third point</param>
        /// <returns>Positive if counterclockwise</returns>
        private double Orient(int a, int b, double x, double y)
        {
            return (_x[b] - _x[a]) * (y - _y[a]) - (_y[b] - _y[a]) * (x - _x[a]);
        }

        /// <summary>
        /// The normalized distance of two vertices
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <returns>The distance</returns>
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

        /// <summary>
        /// The circumcenter of a triangle
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="x">The normalized X of the circumcenter (infinite or NaN for a degenerate triangle)</param>
        /// <param name="y">The normalized Y of the circumcenter</param>
        /// <param name="squareRadius">The square of the circumradius</param>
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

                if (_encroachmentCells != null)
                {
                    AddEncroachmentCells(SegmentKey(a, p));
                    AddEncroachmentCells(SegmentKey(p, b));
                }
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
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="t">A triangle with the edge</param>
        /// <param name="edge">Index of the edge in <paramref name="t"/></param>
        /// <returns>True if a-b is an edge of the triangulation</returns>
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

        /// <summary>
        /// Marks an edge as boundary segment, in both its triangles
        /// </summary>
        /// <param name="t">A triangle of the edge</param>
        /// <param name="edge">The index of the edge in <paramref name="t"/></param>
        private void MarkSegment(Triangle t, int edge)
        {
            t.C[edge] = true;
            Triangle u = t.N[edge];
            if (u != null)
                u.C[IndexOfNeighbour(u, t)] = true;

            long key = SegmentKey(t.V[(edge + 1) % 3], t.V[(edge + 2) % 3]);
            _segments.Add(key);
            _segmentLoops.TryGetValue(key, out int loops);
            _segmentLoops[key] = loops + 1;
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

        /// <summary>
        /// The vertex of a triangle different from two given vertices
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <returns>Its position in the triangle; -1 if none</returns>
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
        /// A triangle is inside the shape if it is reached from the super triangle crossing an odd number of loop edges. A segment of two loops
        /// counts twice (before, once: the result depended on the path, e.g. a hole with an edge on the fill could be inside)
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

                    bool crossing = t.C[k] && (!_segmentLoops.TryGetValue(SegmentKey(t.V[(k + 1) % 3], t.V[(k + 2) % 3]), out int loops) || loops % 2 == 1);
                    depth[n] = crossing ? d + 1 : d;
                    queue.Enqueue(n);
                }
            }
        }

        #endregion

        #region Refinement

        /// <summary>
        /// Refinement (Ruppert): the triangles too large or with a too small angle are split by their circumcenter; a circumcenter that
        /// encroaches a boundary segment, or is not visible from its triangle, splits the segment instead. The number of new points is limited
        /// </summary>
        /// <param name="size">The normalized mesh size</param>
        /// <param name="minAngle">The minimum angle (degrees, at most 30), 0: only the size</param>
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

            // the circumcenters are compared only with the segments of their cell (before, with all the segments: quadratic time)
            _encroachmentCellSize = size;
            _encroachmentCells = new Dictionary<long, List<long>>(LongKeyComparer.Instance);
            foreach (long key in _segments)
                AddEncroachmentCells(key);

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
                if (_encroachmentCells.TryGetValue(CellKey(CellIndex(cx, _encroachmentCellSize), CellIndex(cy, _encroachmentCellSize)), out List<long> near))
                {
                    foreach (long key in near)
                    {
                        if (!_segments.Contains(key))
                            continue; // split after it was added to the cell

                        int a = (int)(key >> 32), b = (int)(key & 0xFFFFFFFF);
                        if ((_x[a] - cx) * (_x[b] - cx) + (_y[a] - cy) * (_y[b] - cy) < 0 && Distance(a, b) > minSegmentLength)
                            encroached.Add(key);
                    }
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
            _encroachmentCells = null;
        }

        /// <summary>
        /// Adds a boundary segment to the cells of <see cref="_encroachmentCells"/> covered by the bounding box of its diametral circle,
        /// so every point inside the circle finds the segment in its own cell
        /// </summary>
        /// <param name="key">The key of the segment (see <see cref="SegmentKey"/>)</param>
        private void AddEncroachmentCells(long key)
        {
            int a = (int)(key >> 32), b = (int)(key & 0xFFFFFFFF);
            double centerX = (_x[a] + _x[b]) / 2.0, centerY = (_y[a] + _y[b]) / 2.0, radius = Distance(a, b) / 2.0;

            long i0 = CellIndex(centerX - radius, _encroachmentCellSize), i1 = CellIndex(centerX + radius, _encroachmentCellSize);
            long j0 = CellIndex(centerY - radius, _encroachmentCellSize), j1 = CellIndex(centerY + radius, _encroachmentCellSize);
            for (long i = i0; i <= i1; i++)
            {
                for (long j = j0; j <= j1; j++)
                {
                    long cell = CellKey(i, j);
                    if (!_encroachmentCells.TryGetValue(cell, out List<long> list))
                        _encroachmentCells[cell] = list = new List<long>(4);
                    list.Add(key);
                }
            }
        }

        /// <summary>
        /// Tell if a triangle must be refined: circumradius too large or ratio circumradius / shortest edge too large (small angle), unless the
        /// small angle is between two boundary segments or the triangle is made of regular points of the lattice
        /// </summary>
        /// <param name="t">The triangle</param>
        /// <param name="maxSquareRadius">The largest square circumradius</param>
        /// <param name="maxRatio">The largest ratio circumradius / shortest edge (infinite: the angles are not checked)</param>
        /// <returns>True if the triangle must be refined</returns>
        private bool IsBad(Triangle t, double maxSquareRadius, double maxRatio)
        {
            Circumcenter(t, out _, out _, out double squareRadius);

            if (squareRadius > maxSquareRadius)
                return true;

            if (double.IsInfinity(maxRatio))
                return false;

            // lattice: the rectangles (points of the lattice and of the boundary edges parallel to it) can be long, their angles are right
            if (_regularPoints != null && _regularPoints.Contains(t.V[0]) && _regularPoints.Contains(t.V[1]) && _regularPoints.Contains(t.V[2]))
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

        #region Lattice (quadrilateral meshes)

        /// <summary>
        /// Lattice aligned with the prevailing direction of the boundary edges (the global axes if they are as good), with lines through the
        /// edges parallel to it and spacing not greater than the mesh size
        /// </summary>
        private Lattice BuildLattice(List<List<double[]>> loops, double size)
        {
            double angle = LatticeAngle(loops);
            var lattice = new Lattice { Cos = Math.Cos(angle), Sin = Math.Sin(angle) };
            _lattice = lattice;

            // positions of the lines: value and "it is a boundary edge" (the extremes of the shape have a lower priority when merged)
            var uLines = new List<KeyValuePair<double, bool>>();
            var vLines = new List<KeyValuePair<double, bool>>();
            double uMin = double.MaxValue, uMax = double.MinValue, vMin = double.MaxValue, vMax = double.MinValue;

            foreach (var loop in loops)
            {
                for (int i = 0; i < loop.Count; i++)
                {
                    double ax = (loop[i][0] - _centerX) / _scale, ay = (loop[i][1] - _centerY) / _scale;
                    double[] next = loop[(i + 1) % loop.Count];
                    double bx = (next[0] - _centerX) / _scale, by = (next[1] - _centerY) / _scale;

                    double ua = lattice.ToU(ax, ay), va = lattice.ToV(ax, ay);
                    uMin = Math.Min(uMin, ua);
                    uMax = Math.Max(uMax, ua);
                    vMin = Math.Min(vMin, va);
                    vMax = Math.Max(vMax, va);

                    if (IsParallelToLattice(ax, ay, bx, by, out bool alongU))
                    {
                        if (alongU)
                            vLines.Add(new KeyValuePair<double, bool>((va + lattice.ToV(bx, by)) / 2.0, true));
                        else
                            uLines.Add(new KeyValuePair<double, bool>((ua + lattice.ToU(bx, by)) / 2.0, true));
                    }
                }
            }

            uLines.Add(new KeyValuePair<double, bool>(uMin, false));
            uLines.Add(new KeyValuePair<double, bool>(uMax, false));
            vLines.Add(new KeyValuePair<double, bool>(vMin, false));
            vLines.Add(new KeyValuePair<double, bool>(vMax, false));

            lattice.U = LatticeLines(uLines, size);
            lattice.V = LatticeLines(vLines, size);
            return lattice;
        }

        /// <summary>
        /// The direction (modulo 90 degrees) with the greatest total length of the boundary edges; the global axes if they have at least half of it
        /// or if no direction prevails (less than a quarter of the perimeter, e.g. a circle)
        /// </summary>
        private static double LatticeAngle(List<List<double[]>> loops)
        {
            var directions = new List<KeyValuePair<double, double>>(); // angle in [0, 90) degrees (radians), length
            foreach (var loop in loops)
            {
                for (int i = 0; i < loop.Count; i++)
                {
                    double[] a = loop[i], b = loop[(i + 1) % loop.Count];
                    double dx = b[0] - a[0], dy = b[1] - a[1];
                    double length = Math.Sqrt(dx * dx + dy * dy);
                    if (!(length > 0))
                        continue;

                    double angle = Math.Atan2(dy, dx);
                    angle -= Math.Floor(angle / (Math.PI / 2.0)) * (Math.PI / 2.0);
                    if (angle > Math.PI / 2.0 - AngleTolerance)
                        angle = 0;
                    directions.Add(new KeyValuePair<double, double>(angle, length));
                }
            }

            directions.Sort((p, q) => p.Key.CompareTo(q.Key));

            double bestAngle = 0, bestWeight = 0, axesWeight = 0, perimeter = 0;
            foreach (KeyValuePair<double, double> direction in directions)
                perimeter += direction.Value;

            int start = 0;
            while (start < directions.Count)
            {
                int end = start;
                double weight = directions[start].Value;
                double sum = directions[start].Key * directions[start].Value;
                while (end + 1 < directions.Count && directions[end + 1].Key - directions[end].Key <= AngleTolerance)
                {
                    end++;
                    weight += directions[end].Value;
                    sum += directions[end].Key * directions[end].Value;
                }

                double angle = sum / weight;
                if (angle <= AngleTolerance)
                    axesWeight = weight;
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestAngle = angle;
                }

                start = end + 1;
            }

            return axesWeight >= 0.5 * bestWeight || bestWeight < 0.25 * perimeter ? 0.0 : bestAngle;
        }

        /// <summary>
        /// Lines through the given positions (an extreme of the shape is merged with a line of an edge closer than <see cref="LineMerge"/>,
        /// two lines of edges only if closer than <see cref="EdgeLineMerge"/>), each interval divided in equal parts not larger than the size
        /// </summary>
        private static double[] LatticeLines(List<KeyValuePair<double, bool>> positions, double size)
        {
            positions.Sort((p, q) => p.Key.CompareTo(q.Key));

            var merged = new List<double>();
            int i = 0;
            while (i < positions.Count)
            {
                double value = positions[i].Key;
                bool edge = positions[i].Value;
                int j = i;
                while (j + 1 < positions.Count)
                {
                    double distance = positions[j + 1].Key - value;
                    bool bothEdges = edge && positions[j + 1].Value;
                    if (distance > (bothEdges ? EdgeLineMerge : LineMerge) * size)
                        break;

                    j++;
                    if (!edge && positions[j].Value)
                    {
                        value = positions[j].Key;
                        edge = true;
                    }
                }

                merged.Add(value);
                i = j + 1;
            }

            var lines = new List<double>();
            for (int k = 0; k < merged.Count; k++)
            {
                lines.Add(merged[k]);
                if (k + 1 == merged.Count)
                    break;

                double gap = merged[k + 1] - merged[k];
                int divisions = Math.Max(1, (int)Math.Ceiling(gap / size - 1E-9));
                for (int m = 1; m < divisions; m++)
                    lines.Add(merged[k] + gap * m / divisions);
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Tell if a segment is parallel to an axis of the lattice
        /// </summary>
        /// <param name="ax">The normalized X of the first point</param>
        /// <param name="ay">The normalized Y of the first point</param>
        /// <param name="bx">The normalized X of the second point</param>
        /// <param name="by">The normalized Y of the second point</param>
        /// <param name="alongU">True if the segment is parallel to the u axis (v constant)</param>
        /// <returns>True if the segment a-b (normalized coordinates) is parallel to one of the axes of the lattice</returns>
        private bool IsParallelToLattice(double ax, double ay, double bx, double by, out bool alongU)
        {
            double du = _lattice.ToU(bx, by) - _lattice.ToU(ax, ay);
            double dv = _lattice.ToV(bx, by) - _lattice.ToV(ax, ay);
            double length = Math.Sqrt(du * du + dv * dv);

            alongU = Math.Abs(dv) <= Math.Abs(du);
            return length > 0 && Math.Min(Math.Abs(du), Math.Abs(dv)) <= AlignedTolerance * length;
        }

        /// <summary>
        /// Boundary segments in a grid of cells as large as the mesh size: each segment { a, b, 1 if parallel to the lattice } is in the cells within <paramref name="reach"/>
        /// </summary>
        private Dictionary<long, List<int[]>> SegmentCells(double size, double reach)
        {
            var cells = new Dictionary<long, List<int[]>>(LongKeyComparer.Instance);
            foreach (long key in _segments)
            {
                int a = (int)(key >> 32), b = (int)(key & 0xFFFFFFFF);
                var segment = new[] { a, b, IsParallelToLattice(_x[a], _y[a], _x[b], _y[b], out _) ? 1 : 0 };

                long i0 = CellIndex(Math.Min(_x[a], _x[b]) - reach, size), i1 = CellIndex(Math.Max(_x[a], _x[b]) + reach, size);
                long j0 = CellIndex(Math.Min(_y[a], _y[b]) - reach, size), j1 = CellIndex(Math.Max(_y[a], _y[b]) + reach, size);
                for (long i = i0; i <= i1; i++)
                {
                    for (long j = j0; j <= j1; j++)
                    {
                        long cell = CellKey(i, j);
                        if (!cells.TryGetValue(cell, out List<int[]> list))
                            cells[cell] = list = new List<int[]>();
                        list.Add(segment);
                    }
                }
            }
            return cells;
        }

        /// <summary>
        /// Layer of points parallel to the boundary edges not parallel to the lattice (curves, slanted edges): every boundary point is offset
        /// inside by the local spacing of the boundary points, so the elements along the boundary are quadrilaterals. Where the shape is thin
        /// (less than 2.2 times the spacing) the point is in the middle of the thickness (e.g. a tube is divided in two rows of quadrilaterals)
        /// </summary>
        /// <returns>The points of the layer</returns>
        private List<int> InsertBoundaryLayer(List<List<int>> loopVertices, double size)
        {
            var layer = new List<int>();
            Dictionary<long, List<int[]>> cells = SegmentCells(size, 2.5 * size);
            var layerCells = new Dictionary<long, List<int>>(LongKeyComparer.Instance);

            foreach (List<int> vertices in loopVertices)
            {
                int n = vertices.Count;
                for (int i = 0; i < n; i++)
                {
                    int previous = vertices[(i + n - 1) % n], v = vertices[i], next = vertices[(i + 1) % n];
                    bool previousAligned = IsParallelToLattice(_x[previous], _y[previous], _x[v], _y[v], out _);
                    bool nextAligned = IsParallelToLattice(_x[v], _y[v], _x[next], _y[next], out _);
                    if (previousAligned && nextAligned)
                        continue;

                    double l1 = Distance(previous, v), l2 = Distance(v, next);
                    if (!(l1 > 0) || !(l2 > 0))
                        continue;

                    // smooth boundary only (turn not greater than 45 degrees): the bisector of the normals
                    double e1x = (_x[v] - _x[previous]) / l1, e1y = (_y[v] - _y[previous]) / l1;
                    double e2x = (_x[next] - _x[v]) / l2, e2y = (_y[next] - _y[v]) / l2;
                    if (e1x * e2x + e1y * e2y < Math.Cos(Math.PI / 4.0))
                        continue;

                    double nx = -(e1y + e2y), ny = e1x + e2x;
                    double norm = Math.Sqrt(nx * nx + ny * ny);
                    nx /= norm;
                    ny /= norm;

                    double spacing = Math.Min(size, (l1 + l2) / 2.0);
                    Triangle inside = Locate(_x[v] + 0.1 * spacing * nx, _y[v] + 0.1 * spacing * ny, null);
                    if (inside is null || !inside.Inside)
                    {
                        nx = -nx;
                        ny = -ny;
                        inside = Locate(_x[v] + 0.1 * spacing * nx, _y[v] + 0.1 * spacing * ny, null);
                        if (inside is null || !inside.Inside)
                            continue;
                    }

                    if (!cells.TryGetValue(CellKey(CellIndex(_x[v], size), CellIndex(_y[v], size)), out List<int[]> near))
                        continue;

                    double thickness = RayDistance(v, nx, ny, near, 2.2 * spacing);
                    double depth = thickness >= 2.2 * spacing ? spacing : thickness >= 1.4 * spacing ? thickness / 2.0 : 0;
                    if (depth == 0)
                        continue;

                    double x = _x[v] + depth * nx, y = _y[v] + depth * ny;
                    if (!cells.TryGetValue(CellKey(CellIndex(x, size), CellIndex(y, size)), out List<int[]> around))
                        continue;

                    bool tooClose = false;
                    foreach (int[] segment in around)
                    {
                        if (DistanceToSegment(x, y, segment[0], segment[1]) < 0.45 * depth)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    long cell = CellKey(CellIndex(x, size), CellIndex(y, size));
                    for (long ci = -1; ci <= 1 && !tooClose; ci++)
                    {
                        for (long cj = -1; cj <= 1 && !tooClose; cj++)
                        {
                            if (!layerCells.TryGetValue(CellKey(CellIndex(x, size) + ci, CellIndex(y, size) + cj), out List<int> points))
                                continue;
                            foreach (int p in points)
                            {
                                double dx = _x[p] - x, dy = _y[p] - y;
                                if (dx * dx + dy * dy < 0.25 * spacing * spacing)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (tooClose)
                        continue;

                    Triangle t = Locate(x, y, inside);
                    if (t is null || !t.Inside)
                        continue;

                    int count = _x.Count;
                    int point = InsertPoint(x, y, double.NaN, double.NaN, -1, t);
                    if (_x.Count == count)
                        continue;

                    layer.Add(point);
                    if (!layerCells.TryGetValue(cell, out List<int> list))
                        layerCells[cell] = list = new List<int>();
                    list.Add(point);
                }
            }

            return layer;
        }

        /// <returns>Distance from the vertex along the direction to the first boundary segment (not through the vertex), at most <paramref name="maxDistance"/></returns>
        private double RayDistance(int vertex, double dx, double dy, List<int[]> segments, double maxDistance)
        {
            double best = maxDistance;
            foreach (int[] segment in segments)
            {
                int a = segment[0], b = segment[1];
                if (a == vertex || b == vertex)
                    continue;

                double sx = _x[b] - _x[a], sy = _y[b] - _y[a];
                double denominator = dx * sy - dy * sx;
                if (Math.Abs(denominator) < 1E-14)
                    continue;

                double wx = _x[a] - _x[vertex], wy = _y[a] - _y[vertex];
                double t = (wx * sy - wy * sx) / denominator;   // along the ray
                double s = (wx * dy - wy * dx) / denominator;   // along the segment
                if (t > 0 && s >= 0 && s <= 1 && t < best)
                    best = t;
            }
            return best;
        }

        /// <summary>
        /// Insert the nodes of the lattice inside the shape, far enough from the boundary segments not on the lattice lines and from the boundary layer
        /// </summary>
        private void InsertLatticePoints(double size, List<int> layer)
        {
            Dictionary<long, List<int[]>> cells = SegmentCells(size, Clearance * size);

            var layerCells = new Dictionary<long, List<int>>(LongKeyComparer.Instance);
            foreach (int p in layer)
            {
                long cell = CellKey(CellIndex(_x[p], size), CellIndex(_y[p], size));
                if (!layerCells.TryGetValue(cell, out List<int> list))
                    layerCells[cell] = list = new List<int>();
                list.Add(p);
            }

            foreach (double u in _lattice.U)
            {
                foreach (double v in _lattice.V)
                {
                    double x = _lattice.ToX(u, v), y = _lattice.ToY(u, v);
                    if (Math.Abs(x) > 0.5 + LocateTolerance || Math.Abs(y) > 0.5 + LocateTolerance)
                        continue;

                    if (cells.TryGetValue(CellKey(CellIndex(x, size), CellIndex(y, size)), out List<int[]> near) && IsTooClose(x, y, near, size))
                        continue;

                    if (IsNearLayer(x, y, layerCells, size))
                        continue;

                    Triangle t = Locate(x, y, null);
                    if (t is null || !t.Inside)
                        continue;

                    int count = _x.Count;
                    int p = InsertPoint(x, y, double.NaN, double.NaN, -1, t);
                    if (_x.Count > count)
                        _latticePoints[p] = true;
                }
            }
        }

        /// <returns>The points of the lattice and the boundary points whose boundary segments are all parallel to the lattice</returns>
        private HashSet<int> RegularPoints()
        {
            var regular = new HashSet<int>();
            for (int i = 0; i < _latticePoints.Count; i++)
            {
                if (_latticePoints[i])
                    regular.Add(i);
            }

            var irregular = new HashSet<int>();
            foreach (long key in _segments)
            {
                int a = (int)(key >> 32), b = (int)(key & 0xFFFFFFFF);
                HashSet<int> set = IsParallelToLattice(_x[a], _y[a], _x[b], _y[b], out _) ? regular : irregular;
                set.Add(a);
                set.Add(b);
            }

            regular.ExceptWith(irregular);
            return regular;
        }

        /// <returns>True if the point is closer than 0.6 times the mesh size to a point of the boundary layer</returns>
        private bool IsNearLayer(double x, double y, Dictionary<long, List<int>> layerCells, double size)
        {
            long i0 = CellIndex(x, size), j0 = CellIndex(y, size);
            double limit = 0.6 * size;
            for (long i = i0 - 1; i <= i0 + 1; i++)
            {
                for (long j = j0 - 1; j <= j0 + 1; j++)
                {
                    if (!layerCells.TryGetValue(CellKey(i, j), out List<int> points))
                        continue;
                    foreach (int p in points)
                    {
                        double dx = _x[p] - x, dy = _y[p] - y;
                        if (dx * dx + dy * dy < limit * limit)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tell if a point is too close to the boundary segments to be a lattice point
        /// </summary>
        /// <param name="x">The normalized X</param>
        /// <param name="y">The normalized Y</param>
        /// <param name="segments">The segments near the point: vertices and 1 if parallel to the lattice</param>
        /// <param name="size">The normalized mesh size</param>
        /// <returns>True if the point is closer than the clearance (<see cref="AlignedClearance"/> or <see cref="Clearance"/> Ã— size)</returns>
        private bool IsTooClose(double x, double y, List<int[]> segments, double size)
        {
            foreach (int[] segment in segments)
            {
                double distance = DistanceToSegment(x, y, segment[0], segment[1]);
                if (distance < (segment[2] == 1 ? AlignedClearance : Clearance) * size)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The distance of a point from a segment
        /// </summary>
        /// <param name="x">The normalized X</param>
        /// <param name="y">The normalized Y</param>
        /// <param name="a">The first vertex of the segment</param>
        /// <param name="b">The second vertex of the segment</param>
        /// <returns>The normalized distance</returns>
        private double DistanceToSegment(double x, double y, int a, int b)
        {
            double dx = _x[b] - _x[a], dy = _y[b] - _y[a];
            double squareLength = dx * dx + dy * dy;
            double t = squareLength > 0 ? ((x - _x[a]) * dx + (y - _y[a]) * dy) / squareLength : 0;
            t = Math.Max(0, Math.Min(1, t));
            double px = _x[a] + t * dx - x, py = _y[a] + t * dy - y;
            return Math.Sqrt(px * px + py * py);
        }

        /// <summary>
        /// The index of the cell of a coordinate
        /// </summary>
        /// <param name="value">The coordinate</param>
        /// <param name="size">The size of the cells</param>
        /// <returns>The index</returns>
        private static long CellIndex(double value, double size)
        {
            return (long)Math.Floor(value / size);
        }

        /// <summary>
        /// The key of a cell
        /// </summary>
        /// <param name="i">The index along X</param>
        /// <param name="j">The index along Y</param>
        /// <returns>The key</returns>
        private static long CellKey(long i, long j)
        {
            return (i << 32) ^ (j & 0xFFFFFFFF);
        }

        /// <summary>
        /// The triangles inside the shape, for the recombination in quadrilaterals
        /// </summary>
        private QuadRecombination ToRecombination()
        {
            var triangles = new List<int[]>();
            foreach (Triangle t in _triangles)
            {
                if (t.Inside)
                    triangles.Add(new[] { t.V[0], t.V[1], t.V[2] });
            }

            return new QuadRecombination(_x, _y, _originalX, _originalY, _tags, _latticePoints, triangles, _segments, _scale, _centerX, _centerY);
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

        /// <summary>
        /// The mesh of the triangles inside the shape: the vertices with their original coordinates (Z = 0) and their tags, the faces and the edges
        /// </summary>
        /// <returns>The mesh</returns>
        private Mesh ToMesh()
        {
            var mesh = new Mesh();
            var vertexIds = new int[_x.Count];
            for (int i = 0; i < vertexIds.Length; i++)
                vertexIds[i] = -1;

            int vertexId = 0;
            int faceId = 0;
            int edgeId = 0;
            var edges = new HashSet<long>(LongKeyComparer.Instance);

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
