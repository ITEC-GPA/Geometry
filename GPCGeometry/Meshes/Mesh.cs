using GPC.Geometry.BVH;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [Serializable]
    public class Mesh : MeshBase, ISerializable, ICloneable, IEquatable<Mesh>
    {
        #region Variables

        protected static object syncRoot = new object(); // Usato per sincronizzare le operazion concorrenti

        protected MeshBaseCollection<MeshVertex> _vertices;
        protected MeshBaseCollection<MeshFace> _faces;
        protected MeshBaseCollection<MeshEdge> _edges;
        protected MeshBaseCollection<MeshVolume> _volumes;

        protected GenerateOptions _options;

        [NonSerialized]
        private VertexGrid _vertexGrid; // spatial index of the vertices used by AddFaceMesh

        [NonSerialized]
        private HashSet<long> _edgeKeys; // keys of the edges, used by AddFaceMesh to not add an edge twice
        [NonSerialized]
        private MeshBaseCollection<MeshEdge> _edgeKeysCollection;
        [NonSerialized]
        private int _edgeKeysVersion;

        #endregion

        #region Properties

        public MeshBaseCollection<MeshVertex> Vertices => _vertices;

        public MeshBaseCollection<MeshFace> Faces => _faces;

        public MeshBaseCollection<MeshEdge> Edges => _edges;

        public MeshBaseCollection<MeshVolume> Volumes => _volumes;

        public int VerticesCount => _vertices.Count;

        public int FacesCount => _faces.Count;

        public int EdgesCount => _edges.Count;

        public int VolumesCount => _volumes.Count;

        public GenerateOptions Options => _options;

        public SphereBVH VertexBVH = null;
        public SphereBVH FaceBVH = null;

        #endregion

        #region Public Constructors

        public Mesh()
        {
            _vertices = new MeshBaseCollection<MeshVertex>();
            _faces = new MeshBaseCollection<MeshFace>();
            _edges = new MeshBaseCollection<MeshEdge>();
            _volumes = new MeshBaseCollection<MeshVolume>();
        }

        protected Mesh(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _vertices = (MeshBaseCollection<MeshVertex>)info.GetValue("Vertices", typeof(MeshBaseCollection<MeshVertex>));
            _faces = (MeshBaseCollection<MeshFace>)info.GetValue("Faces", typeof(MeshBaseCollection<MeshFace>));
            _edges = (MeshBaseCollection<MeshEdge>)info.GetValue("Edges", typeof(MeshBaseCollection<MeshEdge>));
            _volumes = (MeshBaseCollection<MeshVolume>)info.GetValue("Volumes", typeof(MeshBaseCollection<MeshVolume>));
        }

        #endregion

        #region Setter

        /// <summary>
        /// Add a face to the mesh
        /// </summary>
        /// <param name="points">Points that will be converted in MeshVertex</param>
        /// <remarks>This is an O(n) operation</remarks>
        public void AddFaceMesh(Point3d[] points)
        {
            MeshVertex[] vertices = new MeshVertex[points.Length];
            for (int i = 0; i < points.Length; i++)
                vertices[i] = new MeshVertex(points[i]);
            AddFaceMesh(vertices);
        }

        /// <summary>
        /// Add a face to the mesh. A vertex closer than <paramref name="tolerance"/> to an existing vertex of the mesh is merged with it
        /// (vertices of the same face are never merged together)
        /// </summary>
        /// <remarks>The existing vertices are searched with a spatial grid updated face by face, so adding n faces is O(n).
        /// An edge shared with a face already in the mesh is not added again (before, every inner edge was added twice)</remarks>
        public int AddFaceMesh(MeshVertex[] vertices, double tolerance = GeometryBase.Tolerance)
        {
            VertexGrid grid = GetVertexGrid(tolerance);
            HashSet<long> edgeKeys = GetEdgeKeys();
            List<MeshVertex> newVertices = null;

            int[] verticesIds = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                MeshVertex existing = grid.FindClosest(vertices[i].Point, tolerance);

                if (existing == null)
                {
                    MeshVertex vertex = new MeshVertex(vertices[i]);
                    verticesIds[i] = _vertices.Add(vertex);
                    (newVertices ?? (newVertices = new List<MeshVertex>())).Add(vertex);
                }
                else
                    verticesIds[i] = existing.Id;

                if (i > 0)
                    AddEdgeOnce(edgeKeys, verticesIds[i - 1], verticesIds[i]);
            }
            AddEdgeOnce(edgeKeys, verticesIds[vertices.Length - 1], verticesIds[0]);
            _edgeKeysVersion = _edges.Version;

            // the new vertices are indexed only now, so they are not merged with the vertices of the same face
            if (newVertices != null)
            {
                for (int i = 0; i < newVertices.Count; i++)
                    grid.Add(newVertices[i]);
                VertexBVH = null; // no more up to date, FindNeighbours will rebuild it
            }
            grid.Version = _vertices.Version;

            MeshFace face = new MeshFace(verticesIds);
            _faces.Add(face);

            return face.Id;
        }

        private void AddEdgeOnce(HashSet<long> edgeKeys, int a, int b)
        {
            if (edgeKeys.Add(EdgeKey(a, b)))
                _edges.Add(new MeshEdge(a, b));
        }

        /// <returns>The keys of the edges, rebuilt only if the edges have been changed by other methods</returns>
        private HashSet<long> GetEdgeKeys()
        {
            if (_edgeKeys == null || !ReferenceEquals(_edgeKeysCollection, _edges) || _edgeKeysVersion != _edges.Version)
            {
                _edgeKeys = new HashSet<long>(LongKeyComparer.Instance);
                foreach (MeshEdge edge in _edges)
                    _edgeKeys.Add(EdgeKey(edge.A, edge.B));
                _edgeKeysCollection = _edges;
                _edgeKeysVersion = _edges.Version;
            }

            return _edgeKeys;
        }

        /// <returns>The spatial grid of the vertices, rebuilt only if the vertices have been changed by other methods or if <paramref name="tolerance"/> needs bigger cells</returns>
        private VertexGrid GetVertexGrid(double tolerance)
        {
            double cellSize = tolerance > 0 ? tolerance : VertexGrid.MinCellSize;

            if (_vertexGrid == null || !ReferenceEquals(_vertexGrid.Collection, _vertices) || _vertexGrid.Version != _vertices.Version ||
                _vertexGrid.CellSize < cellSize || _vertexGrid.CellSize > 16.0 * cellSize)
            {
                // cells bigger than the tolerance: a search usually crosses 1 or 2 cells per axis
                _vertexGrid = new VertexGrid(_vertices, 4.0 * cellSize);
            }

            return _vertexGrid;
        }

        /// <summary>
        /// Uniform grid of the mesh vertices: a point is searched only in the cells around it.
        /// The cell size is not lower than the search tolerance
        /// </summary>
        private sealed class VertexGrid
        {
            public const double MinCellSize = 1E-9;

            private readonly Dictionary<(long, long, long), List<MeshVertex>> _cells = new Dictionary<(long, long, long), List<MeshVertex>>();

            public readonly double CellSize;
            public readonly MeshBaseCollection<MeshVertex> Collection;
            public int Version;

            public VertexGrid(MeshBaseCollection<MeshVertex> vertices, double cellSize)
            {
                CellSize = cellSize;
                Collection = vertices;
                foreach (MeshVertex vertex in vertices)
                    Add(vertex);
                Version = vertices.Version;
            }

            private long Cell(double coordinate)
            {
                return (long)Math.Floor(coordinate / CellSize);
            }

            public void Add(MeshVertex vertex)
            {
                var key = (Cell(vertex.Point.X), Cell(vertex.Point.Y), Cell(vertex.Point.Z));
                if (!_cells.TryGetValue(key, out List<MeshVertex> cell))
                {
                    cell = new List<MeshVertex>(1);
                    _cells.Add(key, cell);
                }
                cell.Add(vertex);
            }

            /// <returns>The vertex closest to <paramref name="point"/> within <paramref name="tolerance"/> (distance lower or equal), null if there is none</returns>
            /// <remarks>Only the cells crossed by the box of side 2 * <paramref name="tolerance"/> around the point are searched (before, always 27 cells)</remarks>
            public MeshVertex FindClosest(Point3d point, double tolerance)
            {
                long i0 = Cell(point.X - tolerance), i1 = Cell(point.X + tolerance);
                long j0 = Cell(point.Y - tolerance), j1 = Cell(point.Y + tolerance);
                long k0 = Cell(point.Z - tolerance), k1 = Cell(point.Z + tolerance);
                double maxSquareDistance = tolerance * tolerance;

                MeshVertex closest = null;
                double closestSquareDistance = double.MaxValue;

                for (long i = i0; i <= i1; i++)
                    for (long j = j0; j <= j1; j++)
                        for (long k = k0; k <= k1; k++)
                        {
                            if (!_cells.TryGetValue((i, j, k), out List<MeshVertex> cell))
                                continue;

                            for (int n = 0; n < cell.Count; n++)
                            {
                                Point3d p = cell[n].Point;
                                double dx = p.X - point.X;
                                double dy = p.Y - point.Y;
                                double dz = p.Z - point.Z;
                                double squareDistance = dx * dx + dy * dy + dz * dz;

                                if (squareDistance <= maxSquareDistance && squareDistance < closestSquareDistance)
                                {
                                    closest = cell[n];
                                    closestSquareDistance = squareDistance;
                                }
                            }
                        }

                return closest;
            }
        }

        public int AddFaceMesh(MeshVertex[] vertices, int[] verticesIds, double tolerance = GeometryBase.Tolerance)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Id = verticesIds[i];
            return AddFaceMesh(vertices, tolerance);
        }

        public int AddVolumeMesh(MeshVertex[] vertices)
        {
            int[] ids = _vertices.AddRange(vertices);
            int sides = vertices.Length / 2;

            for (int i = 0; i < sides; i++)
            {
                _edges.Add(new MeshEdge(ids[i], ids[i + sides]));                          // vertical edge
                _edges.Add(new MeshEdge(ids[i], ids[(i + 1) % sides]));                    // bottom face edge
                _edges.Add(new MeshEdge(ids[i + sides], ids[sides + (i + 1) % sides]));    // top face edge
            }

            MeshVolume volume = new MeshVolume(ids);
            _volumes.Add(volume);

            return volume.Id;
        }

        /// <summary>
        /// Add a vertex, unless there is already a vertex closer than <paramref name="tol"/>
        /// </summary>
        /// <returns>The id of the new vertex, or of the closest existing one</returns>
        /// <remarks>The vertices are searched with the same spatial grid of <see cref="AddFaceMesh(MeshVertex[], double)"/>, always up to date
        /// (before, the BVH was not updated after the addition, so the vertices added later were not found)</remarks>
        public int AddVertex(MeshVertex vertex, double tol = GeometryBase.Tolerance)
        {
            VertexGrid grid = GetVertexGrid(tol);
            MeshVertex existing = grid.FindClosest(vertex.Point, tol);
            if (existing != null)
                return existing.Id;

            _vertices.Add(vertex);
            grid.Add(vertex);
            grid.Version = _vertices.Version;
            VertexBVH = null;
            return vertex.Id;
        }

        #endregion

        #region Getter: element property

        /// <summary>
        /// The length of the edge
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public double GetEdgeLength(MeshEdge edge)
        {
            if (!_vertices.Contains(edge.A))
                throw new ArgumentException($"Edge vertex:{edge.A} not found");

            if (!_vertices.Contains(edge.B))
                throw new ArgumentException($"Edge vertex:{edge.B} not found");

            return _vertices.GetElementById(edge.A).Point.DistanceTo(_vertices.GetElementById(edge.B).Point);
        }

        /// <returns>The area of the face: half the length of its Newell vector (the area of a planar face; for a non planar quadrilateral
        /// the area of its projection on the mean plane)</returns>
        /// <param name="tolerance">Not used (kept for compatibility: the area was computed by a <see cref="Polygon3d"/>)</param>
        public double GetFaceArea(MeshFace face, double tolerance = GeometryBase.Tolerance)
        {
            Point3d[] p = GetCheckedFacePoints(face);
            NewellVector(p, out double nx, out double ny, out double nz);
            return Math.Sqrt(nx * nx + ny * ny + nz * nz) / 2.0;
        }

        /// <returns>The centroid of the face: the mean of the vertices for a triangle, the centroid of the area for a quadrilateral
        /// (the two triangles A B C and A C D weighted by their signed areas)</returns>
        /// <param name="tolerance">Not used (kept for compatibility: the centroid was computed by a <see cref="Polygon3d"/>)</param>
        public Point3d GetFaceCentroid(MeshFace face, double tolerance = GeometryBase.Tolerance)
        {
            Point3d[] p = GetCheckedFacePoints(face);
            if (p.Length == 3)
                return new Point3d((p[0].X + p[1].X + p[2].X) / 3.0, (p[0].Y + p[1].Y + p[2].Y) / 3.0, (p[0].Z + p[1].Z + p[2].Z) / 3.0);

            NewellVector(p, out double nx, out double ny, out double nz);
            double w1 = SignedTriangleArea(p[0], p[1], p[2], nx, ny, nz);
            double w2 = SignedTriangleArea(p[0], p[2], p[3], nx, ny, nz);
            double w = w1 + w2;
            if (!(Math.Abs(w) > 0))
                return new Point3d((p[0].X + p[1].X + p[2].X + p[3].X) / 4.0, (p[0].Y + p[1].Y + p[2].Y + p[3].Y) / 4.0, (p[0].Z + p[1].Z + p[2].Z + p[3].Z) / 4.0);

            return new Point3d((w1 * (p[0].X + p[1].X + p[2].X) + w2 * (p[0].X + p[2].X + p[3].X)) / (3.0 * w),
                               (w1 * (p[0].Y + p[1].Y + p[2].Y) + w2 * (p[0].Y + p[2].Y + p[3].Y)) / (3.0 * w),
                               (w1 * (p[0].Z + p[1].Z + p[2].Z) + w2 * (p[0].Z + p[2].Z + p[3].Z)) / (3.0 * w));
        }

        /// <exception cref="ArgumentException">If a vertex of the face is not in the mesh</exception>
        private Point3d[] GetCheckedFacePoints(MeshFace face)
        {
            int[] nodes = face.GetNodes();
            var points = new Point3d[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!_vertices.Contains(nodes[i]))
                    throw new ArgumentException($"Face vertex:{nodes[i]} not found");
                points[i] = _vertices.GetElementById(nodes[i]).Point;
            }
            return points;
        }

        /// <summary>
        /// Newell vector of the polygon: normal to the polygon, long twice its area
        /// </summary>
        private static void NewellVector(Point3d[] p, out double nx, out double ny, out double nz)
        {
            nx = ny = nz = 0;
            for (int i = 0; i < p.Length; i++)
            {
                Point3d a = p[i], b = p[(i + 1) % p.Length];
                nx += (a.Y - b.Y) * (a.Z + b.Z);
                ny += (a.Z - b.Z) * (a.X + b.X);
                nz += (a.X - b.X) * (a.Y + b.Y);
            }
        }

        /// <returns>Twice the area of the triangle, signed with respect to the direction (nx, ny, nz) (not normalized: only the sign and the ratios matter)</returns>
        private static double SignedTriangleArea(Point3d a, Point3d b, Point3d c, double nx, double ny, double nz)
        {
            double ux = b.X - a.X, uy = b.Y - a.Y, uz = b.Z - a.Z;
            double vx = c.X - a.X, vy = c.Y - a.Y, vz = c.Z - a.Z;
            return (uy * vz - uz * vy) * nx + (uz * vx - ux * vz) * ny + (ux * vy - uy * vx) * nz;
        }

        public Point3d[] GetFacePoints(MeshFace face)
        {
            var points = new Point3d[face.IsQuad ? 4 : 3];
            points[0] = _vertices.GetElementById(face.A).Point;
            points[1] = _vertices.GetElementById(face.B).Point;
            points[2] = _vertices.GetElementById(face.C).Point;
            if (face.IsQuad)
                points[3] = _vertices.GetElementById(face.D).Point;
            return points;
        }

        public Point3d[] GetEdgePoints(MeshEdge edge)
        {
            MeshVertex[] vertices = GetEdgeVertices(edge);
            return vertices.Select(i => i.Point).ToArray();
        }

        public Point3d[] GetVolumePoints(MeshVolume volume)
        {
            MeshVertex[] vertices = GetVolumeVertices(volume);
            return vertices.Select(i => i.Point).ToArray();
        }

        #endregion

        #region Getter: elements

        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshFace GetFace(int id)
        {
            return _faces.GetElementById(id);
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshVertex GetVertex(int id)
        {
            return _vertices.GetElementById(id);
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshEdge GetEdge(int id)
        {
            return _edges.GetElementById(id);
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshVolume GetVolume(int id)
        {
            return _volumes.GetElementById(id);
        }

        /// <summary>
        /// Get the edges of a MeshFace
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public MeshEdge[] GetFaceEdges(MeshFace face)
        {
            int[] vertexIds = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };
            return _edges.Where(e => vertexIds.Contains(e.A) && vertexIds.Contains(e.B)).ToArray();
        }

        /// <summary>
        /// Get the vertices of the given face
        /// </summary>
        /// <returns>The vertices array</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshVertex[] GetFaceVertices(MeshFace face)
        {
            MeshVertex[] vertices = new MeshVertex[face.IsQuad ? 4 : 3];

            vertices[0] = _vertices.GetElementById(face.A);
            vertices[1] = _vertices.GetElementById(face.B);
            vertices[2] = _vertices.GetElementById(face.C);

            if (face.IsQuad)
                vertices[3] = _vertices.GetElementById(face.D);

            return vertices;
        }

        /// <summary>
        /// Get the vertices of the given edge
        /// </summary>
        /// <returns>The vertices array</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.GetElementById(int)"/>
        public MeshVertex[] GetEdgeVertices(MeshEdge edge)
        {
            return new[]
            {
                _vertices.GetElementById(edge.A),
                _vertices.GetElementById(edge.B)
            };
        }

        public MeshVertex[] GetVolumeVertices(MeshVolume volume)
        {
            var nodes = volume.GetNodes();
            var vertices = new MeshVertex[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                vertices[i] = GetVertex(nodes[i]);
            }
            return vertices;
        }

        public IEnumerator<MeshFace> GetFacesEnumerator()
        {
            return _faces.GetEnumerator();
        }

        public IEnumerator<MeshVertex> GetVerticesEnumerator()
        {
            return _vertices.GetEnumerator();
        }

        public IEnumerator<MeshEdge> GetEdgesEnumerator()
        {
            return _edges.GetEnumerator();
        }

        public IEnumerator<MeshVolume> GetVolumesEnumerator()
        {
            return _volumes.GetEnumerator();
        }

        public MeshFace[] GetFaces()
        {
            return _faces.ToArray();
        }

        public MeshVertex[] GetVertices()
        {
            return _vertices.ToArray();
        }

        public MeshEdge[] GetEdges()
        {
            return _edges.ToArray();
        }

        public MeshVolume[] GetVolumes()
        {
            return _volumes.ToArray();
        }

        public int[] GetVerticeIds()
        {
            return _vertices.Select(i => i.Id).ToArray();
        }

        public Dictionary<int, MeshFace> GetFacesDictionary()
        {
            var res = new Dictionary<int, MeshFace>();
            foreach (var face in _faces)
            {
                res.Add(face.Id, face);
            }
            return res;
        }

        public Dictionary<int, MeshVertex> GetVerticesDictionary()
        {
            var res = new Dictionary<int, MeshVertex>();
            foreach (var vertex in _vertices)
            {
                res.Add(vertex.Id, vertex);
            }
            return res;
        }

        public Dictionary<int, MeshEdge> GetEdgesDictionary()
        {
            var res = new Dictionary<int, MeshEdge>();
            foreach (var edge in _edges)
            {
                res.Add(edge.Id, edge);
            }
            return res;
        }

        public MeshEdge[] GetNakedEdges()
        {
            HashSet<(int, int)> edges = new HashSet<(int, int)>();

            foreach (var face in _faces)
            {
                var nodes = face.GetNodes();
                for (int i = 0; i < nodes.Length; i++)
                {
                    edges.Add((nodes[i], nodes[(i + 1) % nodes.Length]));
                }
            }

            List<MeshEdge> naked = new List<MeshEdge>();
            foreach (var edge in _edges)
            {
                var samedir = (edge.A, edge.B);
                var opposite = (edge.B, edge.A);
                if (!edges.Contains(samedir) || !edges.Contains(opposite))
                {
                    naked.Add(edge);
                }
            }

            return naked.ToArray();
        }

        #endregion

        #region Interrogate

        /// <summary>
        /// Tells if the given face already exists
        /// </summary>
        /// <param name="face">The face to test</param>
        /// <returns>True if the face already exists</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool FaceExists(MeshFace face)
        {
            return _faces.Contains(face);
        }

        /// <summary>
        /// Tells if the face specified by the ids of the nodes already exists
        /// </summary>
        /// <param name="ids">The nodes ids array</param>
        /// <returns>True if the face already exists</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        /// <remarks>The faces with the same nodes, in any order, are searched (before, a new face without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool FaceExists(int[] ids)
        {
            return _faces.Any(f => SameNodes(f.GetNodes(), ids));
        }

        /// <returns>True if the two arrays have the same nodes, in any order</returns>
        private static bool SameNodes(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;

            var nodes = new HashSet<int>(a);
            return nodes.SetEquals(b);
        }

        /// <summary>
        /// Return the area of the given face
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The face area</returns>
        public double FaceArea(MeshFace face)
        {
            // the face nodes are vertex Ids, not positions in the collection
            Point3d a = _vertices.GetElementById(face.A).Point;
            Point3d b = _vertices.GetElementById(face.B).Point;
            Point3d c = _vertices.GetElementById(face.C).Point;

            double ab = a.SquareDistanceTo(b);
            double ac = a.SquareDistanceTo(c);
            double bc = b.SquareDistanceTo(c);
            double area = Math.Sqrt(Math.Max(0.0, 4 * bc * ac - Math.Pow(bc + ac - ab, 2))) / 4; // Max: round-off on degenerate triangles
            if (face.IsQuad)
            {
                Point3d d = _vertices.GetElementById(face.D).Point;
                double ad = a.SquareDistanceTo(d);
                double cd = c.SquareDistanceTo(d);
                area += Math.Sqrt(Math.Max(0.0, 4 * cd * ac - Math.Pow(cd + ac - ad, 2))) / 4;
            }
            return area;
        }

        /// <summary>
        /// Return the area of the given face
        /// </summary>
        /// <param name="ids">The nodes ids defining the face</param>
        /// <returns>The face area</returns>
        public double FaceArea(int[] ids)
        {
            MeshFace face = new MeshFace(ids);
            return FaceArea(face);
        }

        /// <summary>
        /// Tells if the given volume already exists
        /// </summary>
        /// <param name="volume">The volume to test</param>
        /// <returns>True if the volume already exists</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool VolumeExists(MeshVolume volume)
        {
            return _volumes.Contains(volume);
        }

        /// <summary>
        /// Tells if the volume specified by the ids of the nodes already exists
        /// </summary>
        /// <param name="ids">The nodes ids array</param>
        /// <returns>True if the volume already exists</returns>
        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        /// <remarks>The volumes with the same nodes, in any order, are searched (before, a new volume without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool VolumeExists(int[] ids)
        {
            return _volumes.Any(v => SameNodes(v.GetNodes(), ids));
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool EdgeExists(MeshEdge edge)
        {
            return _edges.Contains(edge);
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        /// <remarks>The edges between the two nodes, in any direction, are searched (before, a new edge without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool EdgeExists(int a, int b)
        {
            return _edges.Any(e => (e.A == a && e.B == b) || (e.A == b && e.B == a));
        }

        /// <param name="meshVertex"></param>
        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool VertexExist(MeshVertex meshVertex)
        {
            return _vertices.Contains(meshVertex);
        }

        /// <summary>
        /// Find the point of intersection between this mesh and a semi-infinite line.
        /// </summary>
        /// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
        /// <param name="stopAtFirstIntersection">When the first intersection solution is found it stops execution.
        /// Use false if you want to do a search on all mesh elements, it can be useful for check.</param>
        /// <param name="tolerance"></param>
        /// <returns>List of points with element containing the point.</returns>
        public Dictionary<Point3d, MeshBase> GetIntersectionWihtSemiInfiniteRay(in Line3d SemiRay, in bool stopAtFirstIntersection = true, double tolerance = GeometryBase.Tolerance)
        {
            var intersections = new Dictionary<Point3d, MeshBase>();
            var SemiRayLocal = SemiRay;
            // Calculate intersection with all vertex.
            var angularDistances = new Dictionary<MeshVertex, double>();

            foreach (var v in _vertices)
            {
                var p = v.Point;
                if (p.IsOnSemiInfiniteRay(SemiRayLocal, out double sinAlpha, tolerance))
                {
                    intersections.Add(p, v);
                    if (stopAtFirstIntersection)
                        return intersections;
                }
                double absSinAlpha = Math.Abs(sinAlpha);
                if (absSinAlpha != double.MaxValue)
                    angularDistances[v] = absSinAlpha;
            }
            // Order by ascending angles.
            var angularDistancesDesc = from entry in angularDistances orderby entry.Value ascending select entry;

            if (stopAtFirstIntersection)
            {
                var alreadyCheckedEdges = new HashSet<int>(); // Save list with already checked edges.
                var alreadyCheckedFaces = new HashSet<int>(); // Save list with already checked faces.

                foreach (var angDist in angularDistancesDesc)
                {
                    var minAngularDistanceVertex = angDist.Key.Id;
                    foreach (var e in _edges)
                    {
                        if ((e.A == minAngularDistanceVertex || e.B == minAngularDistanceVertex) && !alreadyCheckedEdges.Contains(e.Id))
                        {
                            SearchIntersectionInEdge(e);

                            if (stopAtFirstIntersection && intersections.Count > 0)
                                return intersections;

                            alreadyCheckedEdges.Add(e.Id);
                        }
                    }
                    foreach (var f in _faces)
                    {
                        if ((f.A == minAngularDistanceVertex || f.B == minAngularDistanceVertex || f.C == minAngularDistanceVertex) && !alreadyCheckedFaces.Contains(f.Id))
                        {
                            SearchIntersectionInFace(f);

                            if (stopAtFirstIntersection && intersections.Count > 0)
                                return intersections;

                            alreadyCheckedFaces.Add(f.Id);
                        }
                    }
                }
            }

            // Calculate intersection with all edges.
            foreach (var e in _edges)
            {
                SearchIntersectionInEdge(e);

                if (stopAtFirstIntersection && intersections.Count > 0)
                    return intersections;
            }
            // Calculate intersection with all triangles.
            foreach (var f in _faces)
            {
                SearchIntersectionInFace(f);

                if (stopAtFirstIntersection && intersections.Count > 0)
                    return intersections;
            }
            return intersections;

            void SearchIntersectionInEdge(MeshEdge e)
            {
                var l = new Line3d(_vertices.GetElementById(e.A).Point, _vertices.GetElementById(e.B).Point);

                if (l.GetIntersectionWihtSemiInfiniteRay(SemiRayLocal, out Point3d inters, tolerance))
                    intersections.Add(inters, e);
            }

            void SearchIntersectionInFace(MeshFace f)
            {
                var f1 = _vertices.GetElementById(f.A).Point;
                var f2 = _vertices.GetElementById(f.B).Point;
                var f3 = _vertices.GetElementById(f.C).Point;

                if (Plane.GetIntersectionTriangleWihtRay(f1, f2, f3, SemiRayLocal.Start, SemiRayLocal.End, out double _, out double _, out double s, out Point3d inters))
                    if (s > 0)
                        intersections.Add(inters, f);
            }
        }

        #endregion

        #region Edit 

        /// <summary>
        /// Move mesh by a given vector
        /// </summary>
        /// <param name="displacement"></param>
        public void Move(Vector3d displacement)
        {
            Move(displacement.X, displacement.Y, displacement.Z);
        }

        /// <summary>
        /// Move mesh by an given increment
        /// </summary>
        /// <param name="dX"></param>
        /// <param name="dY"></param>
        /// <param name="dz"></param>
        public void Move(double dX, double dY, double dz)
        {
            foreach (var v in _vertices)
                v.Point.Move(dX, dY, dz);

            // the spatial indices refer to the old positions
            _vertexGrid = null;
            VertexBVH = null;
            FaceBVH = null;
        }

        public void JoinMesh(Mesh meshToJoin)
        {
            JoinMesh(meshToJoin, out _, out _, out _);
        }

        /// <param name="meshToJoin"></param>
        /// <param name="vertexIdMap">Map between <see cref="MeshVertex"/>.Id of <paramref name="meshToJoin"/> and id of the same vertex in this mesh (Map old, new)</param>
        /// <param name="facesIdMap">Map between <see cref="MeshFace"/>.Id of <paramref name="meshToJoin"/> and id of the same faces in this mesh (Map old, new) </param>
        /// <param name="volumesIdMap">Map between <see cref="MeshVolume"/>.Id of <paramref name="meshToJoin"/> and id of the same volume in this mesh  (Map old, new)</param>
        public void JoinMeshOld(Mesh meshToJoin, out Dictionary<int, int> vertexIdMap, out Dictionary<int, int> facesIdMap, out Dictionary<int, int> volumesIdMap)
        {
            vertexIdMap = new Dictionary<int, int>();
            facesIdMap = new Dictionary<int, int>();
            volumesIdMap = new Dictionary<int, int>();

            int faceIndex = _faces.GetMaxId();
            //int edgeIndex = _edges.GetMaxId();
            int verticesIndex = _vertices.GetMaxId();

            Dictionary<int, List<int>> verticesHashMap = _vertices.GetElementHashMap();
            Dictionary<int, List<int>> volumesHashMap = _volumes.GetElementHashMap();
            Dictionary<int, List<int>> edgesHashMap = _edges.GetElementHashMap();
            Dictionary<int, List<int>> facesHashMap = _faces.GetElementHashMap();

            Dictionary<int, List<int>> meshToJoinVerticesIdMap = meshToJoin._vertices.GetElementIdMap();

            foreach (var faceToJoin in meshToJoin._faces)
            {
                int[] faceToJoinVertexIds = faceToJoin.GetNodes(); // Salvo i vertici qua

                for (int i = 0; i < faceToJoinVertexIds.Length; i++)
                {
                    int faceToJoinVertexId = faceToJoinVertexIds[i];
                    int index = meshToJoinVerticesIdMap[faceToJoinVertexId].First();

                    MeshVertex vertex = meshToJoin._vertices.GetElementByIndex(index);

                    if (verticesHashMap.ContainsKey(vertex.GetHashCode()))
                    {
                        // il vertice è già presente nella mesh base

                        // prendo il vertice, e cambio id alla lista dei vertici del volume da joinare 

                        faceToJoinVertexIds[i] = _vertices.GetElementByIndex(verticesHashMap[vertex.GetHashCode()].First()).Id;
                        vertexIdMap[faceToJoinVertexId] = faceToJoinVertexIds[i];
                    }
                    else
                    {
                        // il vertice non è presente nella mesh base
                        // clono oggetto
                        // lo aggiungo 
                        // aggiorno hashMap

                        verticesIndex++;
                        var cloneMeshVertex = (MeshVertex)vertex.Clone();
                        cloneMeshVertex.Id = verticesIndex;

                        vertexIdMap[faceToJoinVertexId] = verticesIndex;

                        var buildIndex = _vertices.Add(cloneMeshVertex, cloneMeshVertex.Id);
                        verticesHashMap.Add(cloneMeshVertex.GetHashCode(), new List<int> { buildIndex });
                        faceToJoinVertexIds[i] = cloneMeshVertex.Id;
                    }
                }

                var meshFace = new MeshFace(faceToJoinVertexIds, faceToJoin.Tag);

                if (facesHashMap.ContainsKey(meshFace.GetHashCode()))
                {
                    // face già esistente
                    facesIdMap[faceToJoin.Id] = Faces.GetElementByIndex(facesHashMap[meshFace.GetHashCode()].First()).Id;
                }
                else
                {
                    // face da aggiungere

                    var buildIndex = _faces.Add(meshFace, ++faceIndex);
                    facesHashMap.Add(meshFace.GetHashCode(), new List<int> { buildIndex });

                    facesIdMap[faceToJoin.Id] = faceIndex;

                    for (int i = 0; i < faceToJoinVertexIds.Count(); i++)
                    {
                        MeshEdge me;

                        if (i == faceToJoinVertexIds.Count() - 1)
                        {
                            me = new MeshEdge(faceToJoinVertexIds[i], faceToJoinVertexIds[0]);
                        }
                        else
                        {
                            me = new MeshEdge(faceToJoinVertexIds[i], faceToJoinVertexIds[i + 1]);
                        }

                        if (me != null && !edgesHashMap.ContainsKey(me.GetHashCode()))
                        {
                            var buildEdgeIndex = _edges.Add(me);
                            edgesHashMap.Add(me.GetHashCode(), new List<int> { buildEdgeIndex });
                        }
                    }
                }
            }

            int volumeIndex = _volumes.GetMaxId();
            foreach (var volumeToJoin in meshToJoin._volumes)
            {
                int[] volumeToJoinVertexIds = volumeToJoin.GetNodes(); // Salvo i vertici qua


                for (int i = 0; i < volumeToJoinVertexIds.Length; i++)
                {
                    int volumeToJoinVertexId = volumeToJoinVertexIds[i];
                    int index = meshToJoinVerticesIdMap[volumeToJoinVertexId].First();

                    MeshVertex vertex = meshToJoin._vertices.GetElementByIndex(index);

                    if (verticesHashMap.ContainsKey(vertex.GetHashCode()))
                    {
                        // il vertice è già presente nella mesh base

                        // prendo il vertice, e cambio id alla lista dei vertici del volume da joinare 

                        volumeToJoinVertexIds[i] = _vertices.GetElementByIndex(verticesHashMap[vertex.GetHashCode()].First()).Id;
                        vertexIdMap[volumeToJoinVertexId] = volumeToJoinVertexIds[i];

                    }
                    else
                    {
                        // il vertice non è presente nella mesh base
                        // clono oggetto
                        // lo aggiungo 
                        // aggiorno hashMap

                        verticesIndex++;
                        var cloneMeshVertex = (MeshVertex)vertex.Clone();
                        cloneMeshVertex.Id = verticesIndex;

                        vertexIdMap[volumeToJoinVertexId] = verticesIndex;

                        var buildIndex = _vertices.Add(cloneMeshVertex, cloneMeshVertex.Id);
                        verticesHashMap.Add(cloneMeshVertex.GetHashCode(), new List<int> { buildIndex });
                        volumeToJoinVertexIds[i] = cloneMeshVertex.Id;

                    }

                }

                var meshVolume = new MeshVolume(volumeToJoinVertexIds, volumeToJoin.Tag);

                if (volumesHashMap.ContainsKey(meshVolume.GetHashCode()))
                {
                    // volume già esistente
                    volumesIdMap[volumeToJoin.Id] = Volumes.GetElementByIndex(volumesHashMap[meshVolume.GetHashCode()].First()).Id;
                }
                else
                {
                    // volume da aggiungere

                    var buildIndex = _volumes.Add(meshVolume, ++volumeIndex);
                    volumesHashMap.Add(meshVolume.GetHashCode(), new List<int> { buildIndex });

                    volumesIdMap[volumeToJoin.Id] = volumeIndex;

                    int splitFactor = -1;
                    if (meshVolume.IsQuadrangularPrism)
                    {
                        splitFactor = 4;

                    }
                    else if (meshVolume.IsTriangularPrism)
                    {
                        splitFactor = 3;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    // edge fra indici 0, 1, 2, 3 e 4, 5, 6, 7 cioè le due facce
                    foreach (var arrayIndex in volumeToJoinVertexIds.Split(splitFactor))
                    {
                        for (int i = 0; i < arrayIndex.Count(); i++)
                        {
                            MeshEdge me = null;

                            if (i == arrayIndex.Count() - 1)
                            {
                                me = new MeshEdge(arrayIndex[i], arrayIndex[0]);
                            }
                            else
                            {
                                me = new MeshEdge(arrayIndex[i], arrayIndex[i + 1]);
                            }

                            if (me != null && !edgesHashMap.ContainsKey(me.GetHashCode()))
                            {
                                var buildEdgeIndex = _edges.Add(me);
                                edgesHashMap.Add(me.GetHashCode(), new List<int> { buildEdgeIndex });
                            }
                        }
                    }

                    // edge fra indici 0, 4 e 1, 5 etc cioè le pareti
                    for (int i = 0; i < splitFactor; i++)
                    {
                        MeshEdge me = new MeshEdge(volumeToJoinVertexIds[i], volumeToJoinVertexIds[i + splitFactor]);

                        if (me != null && !edgesHashMap.ContainsKey(me.GetHashCode()))
                        {
                            var buildEdgeIndex = _edges.Add(me);
                            edgesHashMap.Add(me.GetHashCode(), new List<int> { buildEdgeIndex });
                        }
                    }
                }
            }
        }

        public void JoinMesh(Mesh meshToJoin, out Dictionary<int, int> vertexIdMap, out Dictionary<int, int> facesIdMap, out Dictionary<int, int> volumesIdMap)
        {
            vertexIdMap = new Dictionary<int, int>();
            facesIdMap = new Dictionary<int, int>();
            volumesIdMap = new Dictionary<int, int>();

            for (int i = 0; i < meshToJoin.VerticesCount; i++)
            {
                var oldVertex = meshToJoin.Vertices[i];
                var newVertex = new MeshVertex(oldVertex.Point, oldVertex.Tag);
                vertexIdMap.Add(oldVertex.Id, AddVertex(newVertex));
            }

            for (int i = 0; i < meshToJoin.EdgesCount; i++)
            {
                var oldEdge = meshToJoin.Edges[i];
                var newEdge = new MeshEdge(vertexIdMap[oldEdge.A], vertexIdMap[oldEdge.B], oldEdge.Tag);
                _edges.AddUnique(newEdge);
            }

            for (int i = 0; i < meshToJoin.FacesCount; i++)
            {
                var oldFace = meshToJoin.Faces[i];
                var nodes = oldFace.GetNodes();
                for (int j = 0; j < nodes.Length; ++j)
                {
                    nodes[j] = vertexIdMap[nodes[j]];
                }
                var newFace = new MeshFace(nodes, oldFace.Tag);
                _faces.AddUnique(newFace);
            }

            for (int i = 0; i < meshToJoin.VolumesCount; ++i)
            {
                var oldVolume = meshToJoin.Volumes[i];
                var nodes = oldVolume.GetNodes();
                for (int j = 0; j < nodes.Length; ++j)
                {
                    nodes[j] = vertexIdMap[nodes[j]];
                }
                var newVolume = new MeshVolume(nodes, oldVolume.Tag);
                _volumes.AddUnique(newVolume);
            }
        }

        /// <summary>
        /// Extrude the mesh along vertx normal
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Mesh Extrude(double length)
        {
            // Create a new mesh
            Mesh copy = new Mesh();

            // Copy old mesh vertices into new mesh
            for (int i = 0; i < _vertices.Count; ++i)
            {
                copy.Vertices.Add(_vertices[i]);
            }

            // Save added edges here (by their nodes: two different edges can have the same hash code)
            var addedEdges = new HashSet<long>(LongKeyComparer.Instance);
            // List of the face normals for each of the new edges
            var normals = new Dictionary<int, List<Vector3d>>();
            // map connecting original edges with extruded ones
            var map = new Dictionary<int, int>();

            // Loop on faces
            for (int i = 0; i < _faces.Count; ++i)
            {
                var nodes = _faces[i].GetNodes();
                for (int j = 0; j < nodes.Length; ++j)
                {
                    // Get current node of the faces and the ones before and after
                    var a = _vertices.GetElementById(nodes[(j - 1 + nodes.Length) % nodes.Length]);
                    var b = _vertices.GetElementById(nodes[j]);
                    var c = _vertices.GetElementById(nodes[(j + 1) % nodes.Length]);

                    // If it is the first time we ecounter this node, add it to the map and add a normal list entry
                    if (!map.ContainsKey(nodes[j]))
                    {
                        map[nodes[j]] = copy.Vertices.Add(new MeshVertex(b.Point));
                        normals.Add(map[nodes[j]], new List<Vector3d>());
                    }

                    // Compute the normal for this face
                    var cross = new Vector3d(b.Point, c.Point).CrossProduct(new Vector3d(b.Point, a.Point));
                    cross.Unitize();
                    cross = cross * length;
                    normals[map[nodes[j]]].Add(cross);
                }

                // Add the edges and volumes
                if (nodes.Length == 3)
                {
                    var edges = new MeshEdge[9] {
                        new MeshEdge(nodes[0], nodes[1]),
                        new MeshEdge(nodes[1], nodes[2]),
                        new MeshEdge(nodes[2], nodes[0]),
                        new MeshEdge(map[nodes[2]], map[nodes[1]]),
                        new MeshEdge(map[nodes[1]], map[nodes[0]]),
                        new MeshEdge(map[nodes[0]], map[nodes[2]]),
                        new MeshEdge(nodes[0], map[nodes[0]]),
                        new MeshEdge(nodes[1], map[nodes[1]]),
                        new MeshEdge(nodes[2], map[nodes[2]])
                    };

                    for (int k = 0; k < edges.Length; ++k)
                    {
                        if (addedEdges.Add(EdgeKey(edges[k].A, edges[k].B)))
                            copy._edges.Add(edges[k]);
                    }

                    copy.Volumes.Add(new MeshVolume(
                        nodes[0],
                        nodes[1],
                        nodes[2],
                        map[nodes[0]],
                        map[nodes[1]],
                        map[nodes[2]]
                    ));

                }
                else if (nodes.Length == 4)
                {
                    var edges = new MeshEdge[12] {
                        new MeshEdge(nodes[0], nodes[1]),
                        new MeshEdge(nodes[1], nodes[2]),
                        new MeshEdge(nodes[2], nodes[3]),
                        new MeshEdge(nodes[3], nodes[0]),
                        new MeshEdge(map[nodes[3]], map[nodes[2]]),
                        new MeshEdge(map[nodes[2]], map[nodes[1]]),
                        new MeshEdge(map[nodes[1]], map[nodes[0]]),
                        new MeshEdge(map[nodes[0]], map[nodes[3]]),
                        new MeshEdge(nodes[0], map[nodes[0]]),
                        new MeshEdge(nodes[1], map[nodes[1]]),
                        new MeshEdge(nodes[2], map[nodes[2]]),
                        new MeshEdge(nodes[3], map[nodes[3]])
                    };

                    for (int k = 0; k < edges.Length; ++k)
                    {
                        if (addedEdges.Add(EdgeKey(edges[k].A, edges[k].B)))
                            copy._edges.Add(edges[k]);
                    }

                    copy.Volumes.Add(new MeshVolume(
                        nodes[0],
                        nodes[1],
                        nodes[2],
                        nodes[3],
                        map[nodes[0]],
                        map[nodes[1]],
                        map[nodes[2]],
                        map[nodes[3]]
                    ));
                }
            }

            // Find all new vertices
            for (int i = 0; i < copy.VerticesCount; ++i)
            {
                var vertex = copy.Vertices[i];
                if (normals.ContainsKey(vertex.Id))
                {
                    // Compute the average normal of the vertex and update point
                    var vectors = normals[vertex.Id];
                    var vector = vectors[0];
                    for (int j = 1; j < vectors.Count; ++j)
                    {
                        vector = vector + vectors[j];
                    }
                    vector = vector / vectors.Count;

                    var newVertex = new MeshVertex(vertex.Point + vector);
                    newVertex.Id = vertex.Id;
                    copy.Vertices[i] = newVertex;
                }
            }

            // Return the copy
            return copy;

        }

        /// <param name="extrusion">The extrusion vector</param>
        /// <returns>A new Mesh with all the <see cref="MeshFace"/> extruded to a <see cref="MeshVolume"/></returns>
        public Mesh ExtrudeFaces(Vector3d extrusion)
        {
            Mesh mesh = new Mesh();

            HashSet<int> bottomVerticesIds = new HashSet<int>();
            Dictionary<int, int> topVerticesIds = new Dictionary<int, int>(); // id of the bottom vertex -> id of the extruded (top) vertex
            List<MeshVertex> topVertices = new List<MeshVertex>();
            HashSet<(int, int)> edgesKeys = new HashSet<(int, int)>();
            List<MeshEdge> newEdges = new List<MeshEdge>();
            List<MeshVolume> newVolumes = new List<MeshVolume>();
            int vertexId = _vertices.GetMaxId() + 1;
            int edgeId = 1;
            int volumeId = 1;

            // the edges shared by adjacent volumes are added once, whatever their direction
            void AddEdge(int a, int b)
            {
                if (edgesKeys.Add(a < b ? (a, b) : (b, a)))
                    newEdges.Add(new MeshEdge(a, b) { Id = edgeId++ });
            }

            foreach (var face in _faces)
            {
                int[] nodes = face.GetNodes();
                int sides = nodes.Length;
                int[] ids = new int[2 * sides]; // bottom face ids followed by top face ids, in the same order

                for (int i = 0; i < sides; i++)
                {
                    MeshVertex bottom = _vertices.GetElementById(nodes[i]);

                    if (bottomVerticesIds.Add(bottom.Id))
                        mesh.Vertices.Add(bottom);

                    // the extruded vertex is shared by all the faces that share the bottom vertex
                    if (!topVerticesIds.TryGetValue(bottom.Id, out int topId))
                    {
                        topId = vertexId++;
                        topVertices.Add(new MeshVertex(bottom.Point + extrusion) { Id = topId });
                        topVerticesIds[bottom.Id] = topId;
                    }

                    ids[i] = bottom.Id;
                    ids[i + sides] = topId;
                }

                for (int i = 0; i < sides; i++)
                {
                    AddEdge(ids[i], ids[(i + 1) % sides]);                     // bottom face
                    AddEdge(ids[sides + i], ids[sides + (i + 1) % sides]);     // top face
                    AddEdge(ids[i], ids[i + sides]);                           // side
                }

                newVolumes.Add(new MeshVolume(ids) { Id = volumeId++ });
            }

            foreach (var vertex in topVertices)
                mesh.Vertices.Add(vertex);
            foreach (var edge in newEdges)
                mesh.Edges.Add(edge);
            foreach (var volume in newVolumes)
                mesh.Volumes.Add(volume);

            return mesh;
        }

        #endregion

        #region Clone

        /// <returns>The cloned mesh</returns>
        /// <remarks>The <see cref="MeshVertex"/> Id of the cloned mesh are the same of the original mesh</remarks>
        public object Clone()
        {
            return Clone(false);
        }

        /// <param name="renumber">
        /// <para>If <see langword="false"/> the <see cref="MeshVertex"/>.Ids of the cloned mesh are the same of the original mesh</para>
        /// <para>If <see langword="true"/> the <see cref="MeshVertex"/>.Ids of the cloned mesh will start from the maximum id of the original mesh</para>
        /// </param>
        /// <returns>The cloned mesh</returns>
        public object Clone(bool renumber)
        {
            Mesh clone = new Mesh();

            if (_options != null)
                clone._options = (GenerateOptions)_options.Clone();

            if (_vertices.Count == 0) // Non ci sono vertici ritorna la mesh vuota
                return clone;

            Dictionary<int, int> mapOldNewVertexId = new Dictionary<int, int>();

            if (_vertices.Count > 0)
            {
                int newIndex = 0;
                int maxIndex = _vertices.GetMaxId() + 1;
                foreach (var vertex in _vertices)
                {
                    var clonedVertex = (MeshVertex)vertex.Clone();

                    if (renumber)
                    {
                        clonedVertex.Id = newIndex + maxIndex;
                        mapOldNewVertexId[vertex.Id] = clonedVertex.Id;
                        newIndex++;
                    }

                    lock (syncRoot)
                    {
                        clone._vertices.Add(clonedVertex);
                    }
                }
            }

            if (_faces.Count > 0)
            {
                int newIndex = 0;
                int maxIndex = _faces.GetMaxId() + 1;
                foreach (var face in _faces)
                {
                    if (renumber)
                    {
                        MeshFace clonedFace;
                        if (face.IsQuad)
                        {
                            clonedFace = new MeshFace(mapOldNewVertexId[face.A], mapOldNewVertexId[face.B], mapOldNewVertexId[face.C], mapOldNewVertexId[face.D]);
                        }
                        else
                        {
                            clonedFace = new MeshFace(mapOldNewVertexId[face.A], mapOldNewVertexId[face.B], mapOldNewVertexId[face.C]);
                        }

                        clone._faces.Add(clonedFace, newIndex + maxIndex);
                        newIndex++;
                    }
                    else
                    {
                        lock (syncRoot)
                            clone._faces.Add((MeshFace)face.Clone());
                    }
                }
            }

            if (_edges.Count > 0)
            {
                int newIndex = 0;
                int maxIndex = _edges.GetMaxId() + 1;
                foreach (var edge in _edges)
                {
                    if (renumber)
                    {
                        MeshEdge clonedEdge = new MeshEdge(mapOldNewVertexId[edge.A], mapOldNewVertexId[edge.B]);

                        clone._edges.Add(clonedEdge, newIndex + maxIndex);
                        newIndex++;
                    }
                    else
                    {
                        lock (syncRoot)
                            clone._edges.Add((MeshEdge)edge.Clone());
                    }
                }
            }

            if (_volumes.Count > 0)
            {
                int newIndex = 0;
                int maxIndex = _volumes.GetMaxId() + 1;
                foreach (var volume in _volumes)
                {
                    if (renumber)
                    {
                        MeshVolume clonedVolume;
                        if (volume.IsQuadrangularPrism)
                        {
                            clonedVolume = new MeshVolume(mapOldNewVertexId[volume.A], mapOldNewVertexId[volume.B], mapOldNewVertexId[volume.C], mapOldNewVertexId[volume.D],
                                                            mapOldNewVertexId[volume.E], mapOldNewVertexId[volume.F], mapOldNewVertexId[volume.G], mapOldNewVertexId[volume.H]);
                        }
                        else
                        {
                            clonedVolume = new MeshVolume(mapOldNewVertexId[volume.A], mapOldNewVertexId[volume.B], mapOldNewVertexId[volume.C], mapOldNewVertexId[volume.D],
                                                            mapOldNewVertexId[volume.E], mapOldNewVertexId[volume.F]);
                        }

                        clone._volumes.Add(clonedVolume, newIndex + maxIndex);
                        newIndex++;
                    }
                    else
                    {
                        lock (syncRoot)
                            clone._volumes.Add((MeshVolume)volume.Clone());
                    }
                }
            }

            return clone;
        }

        #endregion

        #region Mesh Transformation

        /// <summary>
        /// Uniform refinement: every triangle is divided in four triangles and every quadrilateral in four quadrilaterals
        /// (midpoints of the edges, shared by the adjacent faces, and center of the quadrilateral). The tags of the faces are kept, the edges are updated
        /// </summary>
        /// <param name="tolerance">Not used: the midpoints are shared through the edges (before, the points were merged by rounding their coordinates,
        /// which could leave two different points on a shared edge)</param>
        public void Refine(double tolerance = GeometryBase.Tolerance)
        {
            MeshFace[] faces = _faces.ToArray();
            var midpoints = new Dictionary<long, int>(LongKeyComparer.Instance);

            int Midpoint(int a, int b)
            {
                long key = EdgeKey(a, b);
                if (midpoints.TryGetValue(key, out int id))
                    return id;

                Point3d p = _vertices.GetElementById(a).Point, q = _vertices.GetElementById(b).Point;
                id = _vertices.Add(new MeshVertex(new Point3d((p.X + q.X) / 2.0, (p.Y + q.Y) / 2.0, (p.Z + q.Z) / 2.0)));
                midpoints[key] = id;
                return id;
            }

            var result = new List<MeshFace>(4 * faces.Length);
            foreach (MeshFace face in faces)
            {
                if (face.IsTriangle)
                {
                    int p1 = Midpoint(face.A, face.B), p2 = Midpoint(face.B, face.C), p3 = Midpoint(face.C, face.A);
                    result.Add(new MeshFace(new[] { face.A, p1, p3 }, face.Tag));
                    result.Add(new MeshFace(new[] { p1, p2, p3 }, face.Tag));
                    result.Add(new MeshFace(new[] { face.B, p2, p1 }, face.Tag));
                    result.Add(new MeshFace(new[] { face.C, p3, p2 }, face.Tag));
                }
                else
                {
                    int p1 = Midpoint(face.A, face.B), p2 = Midpoint(face.B, face.C), p3 = Midpoint(face.C, face.D), p4 = Midpoint(face.D, face.A);
                    Point3d m1 = _vertices.GetElementById(p1).Point, m2 = _vertices.GetElementById(p2).Point;
                    Point3d m3 = _vertices.GetElementById(p3).Point, m4 = _vertices.GetElementById(p4).Point;
                    int p5 = _vertices.Add(new MeshVertex(new Point3d((m1.X + m2.X + m3.X + m4.X) / 4.0, (m1.Y + m2.Y + m3.Y + m4.Y) / 4.0, (m1.Z + m2.Z + m3.Z + m4.Z) / 4.0)));

                    result.Add(new MeshFace(new[] { face.A, p1, p5, p4 }, face.Tag));
                    result.Add(new MeshFace(new[] { p1, face.B, p2, p5 }, face.Tag));
                    result.Add(new MeshFace(new[] { p5, p2, face.C, p3 }, face.Tag));
                    result.Add(new MeshFace(new[] { p4, p5, p3, face.D }, face.Tag));
                }
            }

            UpdateEdges(faces, result, midpoints);

            _faces.Clear();
            foreach (MeshFace face in result)
                _faces.Add(face);

            FaceBVH = null;
            VertexBVH = null;
        }

        /// <summary>
        /// Cut the faces of the mesh with the infinite line through <paramref name="curve"/> (in the XY plane): every face crossed by the line
        /// is divided in the parts on the two sides, so every face lies on one side of the line.
        /// <para>A triangle gives triangles. A quadrilateral cut through two opposite edges gives two quadrilaterals, through a vertex a triangle
        /// and a quadrilateral, through two adjacent edges a triangle and a pentagon, divided in a triangle and a quadrilateral.</para>
        /// <para>The new vertices are on the cut edges (Z interpolated), shared by the faces around the edge. The vertices closer than
        /// <paramref name="tolerance"/> to the line are on it (the edges through them are not cut). The faces not cut, the vertices and the tags are kept;
        /// the edges of the mesh are updated.</para>
        /// </summary>
        public void Cut(Line2d curve, double tolerance = GeometryBase.Tolerance)
        {
            double dx = curve.End.X - curve.Start.X, dy = curve.End.Y - curve.Start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (!(length > 0))
                return;

            dx /= length;
            dy /= length;
            double x0 = curve.Start.X, y0 = curve.Start.Y;

            // signed distance of the vertices from the line, 0 if closer than the tolerance
            var distances = new Dictionary<int, double>(_vertices.Count);
            double Distance(int id)
            {
                if (!distances.TryGetValue(id, out double distance))
                {
                    Point3d p = _vertices.GetElementById(id).Point;
                    distance = dx * (p.Y - y0) - dy * (p.X - x0);
                    if (Math.Abs(distance) <= tolerance)
                        distance = 0;
                    distances[id] = distance;
                }
                return distance;
            }

            // new vertex on the cut edge, computed once for the faces around the edge
            var cutPoints = new Dictionary<long, int>(LongKeyComparer.Instance);
            int CutPoint(int a, int b)
            {
                long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                if (cutPoints.TryGetValue(key, out int id))
                    return id;

                int first = Math.Min(a, b), second = Math.Max(a, b);
                Point3d p = _vertices.GetElementById(first).Point, q = _vertices.GetElementById(second).Point;
                double t = Distance(first) / (Distance(first) - Distance(second));
                id = _vertices.Add(new MeshVertex(new Point3d(p.X + t * (q.X - p.X), p.Y + t * (q.Y - p.Y), p.Z + t * (q.Z - p.Z))));
                distances[id] = 0;
                cutPoints[key] = id;
                return id;
            }

            MeshFace[] faces = _faces.ToArray();
            int nextFaceId = faces.Length > 0 ? faces.Max(f => f.Id) + 1 : 0;
            var result = new List<MeshFace>(faces.Length + 16);
            var positive = new List<int>(5);
            var negative = new List<int>(5);

            foreach (MeshFace face in faces)
            {
                int[] nodes = face.GetNodes();
                bool above = false, below = false;
                foreach (int node in nodes)
                {
                    double distance = Distance(node);
                    above |= distance > 0;
                    below |= distance < 0;
                }

                if (!above || !below)
                {
                    result.Add(face);
                    continue;
                }

                positive.Clear();
                negative.Clear();
                for (int k = 0; k < nodes.Length; k++)
                {
                    int a = nodes[k], b = nodes[(k + 1) % nodes.Length];
                    double da = Distance(a), db = Distance(b);
                    if (da >= 0)
                        positive.Add(a);
                    if (da <= 0)
                        negative.Add(a);
                    if ((da > 0 && db < 0) || (da < 0 && db > 0))
                    {
                        int m = CutPoint(a, b);
                        positive.Add(m);
                        negative.Add(m);
                    }
                }

                AddCutPart(positive, face.Tag, result, ref nextFaceId);
                AddCutPart(negative, face.Tag, result, ref nextFaceId);
            }

            UpdateEdges(faces, result, cutPoints);

            _faces.Clear();
            foreach (MeshFace face in result)
                _faces.Add(face);

            FaceBVH = null;
            VertexBVH = null;
        }

        private static long EdgeKey(int a, int b)
        {
            return a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        }

        /// <summary>
        /// Update the edges after the faces have been divided: the edges divided by a new point (<paramref name="splitPoints"/>: edge key -> point)
        /// are replaced by their two halves; if the edges were the ones of the faces, the new edges of the faces are added too
        /// (the edges not of the faces, e.g. lines, are kept). The duplicated edges are removed
        /// </summary>
        private void UpdateEdges(MeshFace[] oldFaces, List<MeshFace> newFaces, Dictionary<long, int> splitPoints)
        {
            if (_edges.Count == 0)
                return;

            var faceEdges = new HashSet<long>(LongKeyComparer.Instance);
            foreach (MeshFace face in oldFaces)
            {
                int[] nodes = face.GetNodes();
                for (int k = 0; k < nodes.Length; k++)
                    faceEdges.Add(EdgeKey(nodes[k], nodes[(k + 1) % nodes.Length]));
            }

            MeshEdge[] edges = _edges.ToArray();
            var edgeKeys = new HashSet<long>(edges.Select(e => EdgeKey(e.A, e.B)), LongKeyComparer.Instance);
            bool edgesOfTheFaces = faceEdges.IsSubsetOf(edgeKeys);

            _edges.Clear();
            var added = new HashSet<long>(LongKeyComparer.Instance);
            foreach (MeshEdge edge in edges)
            {
                long key = EdgeKey(edge.A, edge.B);
                if (splitPoints.TryGetValue(key, out int m))
                {
                    if (added.Add(EdgeKey(edge.A, m)))
                        _edges.Add(new MeshEdge(edge.A, m));
                    if (added.Add(EdgeKey(m, edge.B)))
                        _edges.Add(new MeshEdge(m, edge.B));
                }
                else if (added.Add(key))
                {
                    _edges.Add(edge);
                }
            }

            if (edgesOfTheFaces)
            {
                foreach (MeshFace face in newFaces)
                {
                    int[] nodes = face.GetNodes();
                    for (int k = 0; k < nodes.Length; k++)
                    {
                        if (added.Add(EdgeKey(nodes[k], nodes[(k + 1) % nodes.Length])))
                            _edges.Add(new MeshEdge(nodes[k], nodes[(k + 1) % nodes.Length]));
                    }
                }
            }
        }

        /// <summary>
        /// Add a part of a cut face: 3 or 4 vertices as they are, 5 vertices (a quadrilateral cut through adjacent edges) as a triangle and
        /// a quadrilateral, choosing the division with the best shapes
        /// </summary>
        private void AddCutPart(List<int> nodes, object tag, List<MeshFace> faces, ref int nextFaceId)
        {
            if (nodes.Count == 3 || nodes.Count == 4)
            {
                faces.Add(new MeshFace(nodes.ToArray(), tag) { Id = nextFaceId++ });
                return;
            }

            // pentagon: triangle (i, i+1, i+2) and quadrilateral (i+2, i+3, i+4, i)
            var points = nodes.Select(n => _vertices.GetElementById(n).Point).ToArray();
            double orientation = 0;
            for (int k = 0; k < 5; k++)
                orientation += points[k].X * points[(k + 1) % 5].Y - points[(k + 1) % 5].X * points[k].Y;

            int best = -1;
            double bestQuality = double.MinValue;
            for (int i = 0; i < 5; i++)
            {
                int[] quad = { (i + 2) % 5, (i + 3) % 5, (i + 4) % 5, i };
                double quality = double.MaxValue;
                for (int k = 0; k < 4; k++)
                    quality = Math.Min(quality, Math.Sign(orientation) * CornerSine(points[quad[(k + 3) % 4]], points[quad[k]], points[quad[(k + 1) % 4]]));
                int[] triangle = { i, (i + 1) % 5, (i + 2) % 5 };
                for (int k = 0; k < 3; k++)
                    quality = Math.Min(quality, Math.Sign(orientation) * CornerSine(points[triangle[(k + 2) % 3]], points[triangle[k]], points[triangle[(k + 1) % 3]]));

                if (quality > bestQuality)
                {
                    bestQuality = quality;
                    best = i;
                }
            }

            faces.Add(new MeshFace(new[] { nodes[best], nodes[(best + 1) % 5], nodes[(best + 2) % 5] }, tag) { Id = nextFaceId++ });
            faces.Add(new MeshFace(new[] { nodes[(best + 2) % 5], nodes[(best + 3) % 5], nodes[(best + 4) % 5], nodes[best] }, tag) { Id = nextFaceId++ });
        }

        /// <returns>Sine of the angle in <paramref name="vertex"/> from the edge to <paramref name="next"/> to the edge to <paramref name="previous"/> (XY plane)</returns>
        private static double CornerSine(Point3d previous, Point3d vertex, Point3d next)
        {
            double e1x = next.X - vertex.X, e1y = next.Y - vertex.Y, e2x = previous.X - vertex.X, e2y = previous.Y - vertex.Y;
            double lengths = Math.Sqrt((e1x * e1x + e1y * e1y) * (e2x * e2x + e2y * e2y));
            return lengths > 0 ? (e1x * e2y - e1y * e2x) / lengths : 0;
        }

        /// <returns>A refined copy of the mesh (see <see cref="Refine"/>)</returns>
        public static Mesh RefineMesh(Mesh mesh, double tolerance = GeometryBase.Tolerance)
        {
            Mesh newMesh = (Mesh)mesh.Clone();
            newMesh.Refine(tolerance);
            return newMesh;
        }

        public void Clean(double edgeTolerance = GeometryBase.Tolerance, double areaTolerance = GeometryBase.Tolerance)
        {
            var nakedEdges = GetNakedEdges();

            HashSet<int> nakedVertices = new HashSet<int>();
            for (int i = 0; i < nakedEdges.Length; i++)
            {
                nakedVertices.Add(nakedEdges[i].A);
                nakedVertices.Add(nakedEdges[i].B);
            }

            Dictionary<int, HashSet<int>> collapseMap = new Dictionary<int, HashSet<int>>();
            UpdateVertexBVH();

            // Find short edges
            foreach (var vertex in _vertices)
            {
                var neighbours = FindNeighbours(vertex.Point, edgeTolerance);
                if (!collapseMap.ContainsKey(vertex.Id))
                {
                    collapseMap.Add(vertex.Id, new HashSet<int>());
                }
                for (int j = 0; j < neighbours.Count; j++)
                {
                    collapseMap[vertex.Id].Add(neighbours[j]);
                }
            }

            // Find small faces
            foreach (var face in _faces)
            {
                var nodes = face.GetNodes();
                var points = GetFacePoints(face);

                var area = new Vector3d(points[1] - points[0]).CrossProduct(new Vector3d(points[1] - points[2])).Length * 0.5;
                if (points.Length > 3)
                {
                    area += new Vector3d(points[3] - points[0]).CrossProduct(new Vector3d(points[3] - points[2])).Length * 0.5;
                }

                if (area < areaTolerance)
                {
                    var order = new List<int>();
                    var lengths = new List<double>();

                    for (int i = 0; i < points.Length; i++)
                    {
                        order.Add(i);
                        lengths.Add(points[i].DistanceTo(points[(i + 1) % points.Length]));
                    }

                    order.Sort((a, b) => { return lengths[a].CompareTo(lengths[b]); });

                    var nodeA = nodes[order[0]];
                    var nodeB = nodes[(order[0] + 1) % nodes.Length];
                    if (!collapseMap.ContainsKey(nodeA)) { collapseMap.Add(nodeA, new HashSet<int>()); }
                    if (!collapseMap.ContainsKey(nodeB)) { collapseMap.Add(nodeB, new HashSet<int>()); }
                    collapseMap[nodeA].Add(nodeB);
                    collapseMap[nodeB].Add(nodeA);

                    if (points.Length > 3)
                    {
                        nodeA = nodes[order[1]];
                        nodeB = nodes[(order[1] + 1) % nodes.Length];
                        if (!collapseMap.ContainsKey(nodeA)) { collapseMap.Add(nodeA, new HashSet<int>()); }
                        if (!collapseMap.ContainsKey(nodeB)) { collapseMap.Add(nodeB, new HashSet<int>()); }
                        collapseMap[nodeA].Add(nodeB);
                        collapseMap[nodeB].Add(nodeA);
                    }
                }
            }


            Dictionary<int, int> map = new Dictionary<int, int>();
            // Closure of the collapsing groups
            foreach (var key in collapseMap.Keys)
            {
                if (map.ContainsKey(key))
                {
                    continue;
                }

                if (collapseMap[key].Count == 1 && collapseMap[key].Contains(key))
                {
                    continue;
                }

                bool repeat = true;
                while (repeat)
                {
                    var oldkeys = collapseMap[key].ToArray();
                    foreach (var subkey in oldkeys)
                    {
                        collapseMap[key].UnionWith(collapseMap[subkey]);
                    }

                    repeat = collapseMap[key].Count > oldkeys.Length;
                }

                var naked = new List<Point3d>();
                var others = new List<Point3d>();

                foreach (var vindex in collapseMap[key])
                {
                    if (nakedVertices.Contains(vindex))
                    {
                        naked.Add(GetVertex(vindex).Point);
                    }
                    else
                    {
                        others.Add(GetVertex(vindex).Point);
                    }
                }

                Point3d newPoint = new Point3d(0, 0, 0);

                if (naked.Count > 0)
                {
                    for (int i = 0; i < naked.Count; ++i)
                    {
                        newPoint = newPoint + naked[i];
                    }
                    newPoint = newPoint / naked.Count;
                }
                else
                {
                    for (int i = 0; i < others.Count; ++i)
                    {
                        newPoint = newPoint + others[i];
                    }
                    newPoint = newPoint / others.Count;
                }

                var newIndex = _vertices.Add(new MeshVertex(newPoint));

                foreach (var vindex in collapseMap[key])
                {
                    _vertices.Remove(vindex);
                    map.Add(vindex, newIndex);
                }
            }

            // Update edges
            var edgesCopy = GetEdges();
            foreach (var edge in edgesCopy)
            {
                bool modified = false;
                var newA = edge.A;
                var newB = edge.B;

                if (map.ContainsKey(edge.A))
                {
                    modified = true;
                    newA = map[edge.A];
                }
                if (map.ContainsKey(edge.B))
                {
                    modified = true;
                    newB = map[edge.B];
                }

                if (!modified)
                {
                    continue;
                }

                MeshEdge newEdge = new MeshEdge(newA, newB);
                newEdge.Id = edge.Id;

                _edges.Remove(edge.Id);
                if (newEdge.A != newEdge.B)
                {
                    _edges.Add(newEdge);
                }
            }

            // Update faces
            var facesCopy = GetFaces();
            foreach (var face in facesCopy)
            {
                var modified = false;
                var nodes = face.GetNodes();
                var check = new HashSet<int>();
                var cleanNodes = new List<int>();
                for (int i = 0; i < nodes.Length; ++i)
                {
                    if (map.ContainsKey(nodes[i]))
                    {
                        nodes[i] = map[nodes[i]];
                        modified = true;
                    }

                    if (!check.Contains(nodes[i]))
                    {
                        cleanNodes.Add(nodes[i]);
                        check.Add(nodes[i]);
                    }
                    else
                    {
                        modified = true;
                    }
                }

                if (!modified)
                {
                    continue;
                }

                _faces.Remove(face.Id);

                if (cleanNodes.Count > 2)
                {
                    var newFace = new MeshFace(cleanNodes.ToArray());
                    newFace.Id = face.Id;
                    _faces.Add(newFace);
                }
            }

        }

        #endregion

        #region Public method override - Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return Equals(obj as Mesh);
        }

        public bool Equals(Mesh other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;

            return other is Mesh mesh && _vertices.ScrambledEquals(mesh._vertices)
                                      && _faces.ScrambledEquals(mesh._faces)
                                      && _edges.ScrambledEquals(mesh._edges)
                                      && _volumes.ScrambledEquals(mesh._volumes);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode += hashCode + _vertices.GetHashCodeScrambled();
                hashCode += hashCode + _faces.GetHashCodeScrambled();
                hashCode += hashCode + _edges.GetHashCodeScrambled();
                hashCode += hashCode + _volumes.GetHashCodeScrambled();

                return hashCode;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Vertices", _vertices, typeof(MeshBaseCollection<MeshVertex>));
            info.AddValue("Faces", _faces, typeof(MeshBaseCollection<MeshFace>));
            info.AddValue("Edges", _edges, typeof(MeshBaseCollection<MeshEdge>));
            info.AddValue("Volumes", _volumes, typeof(MeshBaseCollection<MeshVolume>));
        }

        public static bool operator ==(Mesh obj1, Mesh obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Mesh obj1, Mesh obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region Nested classes

        [Serializable]
        public class GenerateOptions : ICloneable
        {
            /// <summary>
            /// Mesh element size. Default value: '1E+22'
            /// </summary>
            public double MeshSize;

            /// <summary>
            /// Recombine the mesh of the current model.
            /// </summary>
            public bool Recombine;

            /// <summary>
            /// Refine the mesh of the current model by uniformly splitting the elements.
            /// </summary>
            public bool Refine;

            public GenerateOptions()
            {
                MeshSize = 1E+22;
                Recombine = true;
                Refine = false;
            }

            public virtual object Clone()
            {
                GenerateOptions clone = new GenerateOptions
                {
                    MeshSize = MeshSize,
                    Recombine = Recombine,
                    Refine = Refine,
                };

                return clone;
            }
        }

        [Serializable]
        public class GenerateMeshStatus
        {
            protected List<string> _customErrorMessages;
            protected List<string> _warnings;
            protected List<Exception> _exceptions;

            protected int _generatedSurfaces;

            private Dictionary<string, double> _executionTime;

            public List<string> Warnings => _warnings;

            public List<string> CustomErrorMessages => _customErrorMessages;

            public List<Exception> Exceptions => _exceptions;

            public int GeneratedSurfaces { get => _generatedSurfaces; set => _generatedSurfaces = value; }

            public Dictionary<string, double> ExecutionTime { get => _executionTime; set => _executionTime = value; }


            public GenerateMeshStatus()
            {
                _customErrorMessages = new List<string>();
                _warnings = new List<string>();
                _exceptions = new List<Exception>();
                _executionTime = new Dictionary<string, double>();
            }

            public void AddExecutionTimeMessage(string message, double value)
            {
                _executionTime.Add(message, value);
            }

            public void AddException(Exception exception, string customErrorMessage)
            {
                _exceptions.Add(exception);
                _customErrorMessages.Add(customErrorMessage);
            }

            public void AddWarning(string warning)
            {
                _warnings.Add(warning);
            }

            public Exception GetLastException()
            {
                return _exceptions.LastOrDefault();
            }

            public string GetLastCustomErrorMessage()
            {
                return _customErrorMessages.LastOrDefault();
            }

            public Exception GetFirstException()
            {
                return _exceptions.FirstOrDefault();
            }

            public string GetFirstCustomErrorMessage()
            {
                return _customErrorMessages.FirstOrDefault();
            }
        }

        #endregion

        public void UpdateVertexBVH()
        {
            var ids = new List<int>();
            var points = new List<Point3d>();

            for (int i = 0; i < VerticesCount; i++)
            {
                ids.Add(Vertices[i].Id);
                points.Add(Vertices[i].Point);
            }

            VertexBVH = new SphereBVH(points, ids);
        }

        public void UpdateFaceBVH()
        {
            var ids = new List<int>();
            var faces = new List<Point3d[]>();

            for (int i = 0; i < FacesCount; i++)
            {
                ids.Add(Faces[i].Id);
                faces.Add(GetFacePoints(Faces[i]));
            }

            FaceBVH = new SphereBVH(faces, ids);
        }

        public List<int> FindNeighbours(Point3d point, double range)
        {
            if (VertexBVH == null)
            {
                UpdateVertexBVH();
            }

            return VertexBVH.GetIntersections(point, range);
        }

        public bool PickFace(Ray3d ray, out MeshFace face, out Point3d intersectionPoint)
        {
            face = null;
            intersectionPoint = null;
            double doublePrecision = 1e-12;

            if (FaceBVH == null) { UpdateFaceBVH(); }

            var intersections = FaceBVH.GetRayIntersections(ray);

            if (intersections.Count == 0)
            {
                return false;
            }

            var closest = -1;
            var distance = Double.MaxValue;

            for (int i = 0; i < intersections.Count; i++)
            {
                var f = GetFace(intersections[i]);
                var points = GetFacePoints(f);

                Point3d intersection;
                bool pointsAreColinear = false;
                {
                    if (points[0] == points[1] || points[1] == points[2] || points[2] == points[0])
                        pointsAreColinear = true;
                    else
                    {
                        var v1 = new Vector3d(points[0], points[1]);
                        v1.Unitize();
                        var v2 = new Vector3d(points[0], points[2]);
                        v2.Unitize();
                        double sinAlpha = v1.CrossProduct(v2).Norm();
                        pointsAreColinear = sinAlpha < GeometryBase.AngularTolerance ? true : false;
                    }
                }
                if (!pointsAreColinear)
                {
                    Plane.GetIntersectionTriangleWihtRay(points[0], points[1], points[2], ray.Point, ray.Point + ray.Direction, out double u, out double v, out double s, out intersection);

                    // Added precision on doubles to also take the vertices of the triangle and not just the strictly contained points.
                    if (u >= -doublePrecision && v >= -doublePrecision && u + v <= 1.0 + doublePrecision)
                    {
                        if (s > 0)
                        {
                            var d = ray.Point.DistanceTo(intersection);
                            if (d < distance)
                            {
                                closest = f.Id;
                                intersectionPoint = intersection;
                                distance = d;
                            }
                        }
                    }
                }
            }

            if (closest > -1)
            {
                face = GetFace(closest);
                return true;
            }

            return false;
        }
    }
}