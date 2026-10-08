using GPC.Geometry.BVH;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// A mesh: vertices, edges, faces (triangles and quadrangles) and volumes (prisms), each in a collection with unique ids.
    /// The edges, the faces and the volumes refer to the vertices by id
    /// </summary>
    [Serializable]
    public class Mesh : MeshBase, ISerializable, ICloneable, IEquatable<Mesh>
    {
        #region Variables

        /// <summary>
        /// The lock shared by all the meshes for the concurrent operations
        /// </summary>
        protected static object syncRoot = new object(); // Usato per sincronizzare le operazion concorrenti

        /// <summary>
        /// The vertices
        /// </summary>
        protected MeshBaseCollection<MeshVertex> _vertices;
        /// <summary>
        /// The faces
        /// </summary>
        protected MeshBaseCollection<MeshFace> _faces;
        /// <summary>
        /// The edges
        /// </summary>
        protected MeshBaseCollection<MeshEdge> _edges;
        /// <summary>
        /// The volumes
        /// </summary>
        protected MeshBaseCollection<MeshVolume> _volumes;

        /// <summary>
        /// The options used to generate the mesh (null if the mesh was not generated with options)
        /// </summary>
        protected GenerateOptions _options;

        /// <summary>
        /// The spatial index of the vertices used by AddFaceMesh and AddVertex (rebuilt when the vertices are changed by other methods)
        /// </summary>
        [NonSerialized]
        private VertexGrid _vertexGrid; // spatial index of the vertices used by AddFaceMesh

        /// <summary>
        /// The keys of the edges, used by AddFaceMesh to not add an edge twice
        /// </summary>
        [NonSerialized]
        private HashSet<long> _edgeKeys; // keys of the edges, used by AddFaceMesh to not add an edge twice
        /// <summary>
        /// The edge collection of <see cref="_edgeKeys"/>: the keys are rebuilt if the collection is replaced
        /// </summary>
        [NonSerialized]
        private MeshBaseCollection<MeshEdge> _edgeKeysCollection;
        /// <summary>
        /// The version of the edge collection of <see cref="_edgeKeys"/>: the keys are rebuilt if the edges are changed by other methods
        /// </summary>
        [NonSerialized]
        private int _edgeKeysVersion;

        [NonSerialized] private MeshBaseCollection<MeshVertex> _vertexBvhVertices;
        [NonSerialized] private int _vertexBvhVersion;
        [NonSerialized] private MeshBaseCollection<MeshVertex> _faceBvhVertices;
        [NonSerialized] private MeshBaseCollection<MeshFace> _faceBvhFaces;
        [NonSerialized] private int _faceBvhVertexVersion;
        [NonSerialized] private int _faceBvhFaceVersion;

        #endregion

        #region Properties

        /// <summary>
        /// The vertices of the mesh
        /// </summary>
        public MeshBaseCollection<MeshVertex> Vertices => _vertices;

        /// <summary>
        /// The faces of the mesh
        /// </summary>
        public MeshBaseCollection<MeshFace> Faces => _faces;

        /// <summary>
        /// The edges of the mesh
        /// </summary>
        public MeshBaseCollection<MeshEdge> Edges => _edges;

        /// <summary>
        /// The volumes of the mesh
        /// </summary>
        public MeshBaseCollection<MeshVolume> Volumes => _volumes;

        /// <summary>
        /// The number of vertices
        /// </summary>
        public int VerticesCount => _vertices.Count;

        /// <summary>
        /// The number of faces
        /// </summary>
        public int FacesCount => _faces.Count;

        /// <summary>
        /// The number of edges
        /// </summary>
        public int EdgesCount => _edges.Count;

        /// <summary>
        /// The number of volumes
        /// </summary>
        public int VolumesCount => _volumes.Count;

        /// <summary>
        /// The options used to generate the mesh
        /// </summary>
        public GenerateOptions Options => _options;

        /// <summary>
        /// The hierarchy of spheres of the vertices, built on demand by the searches (null when it is not up to date)
        /// </summary>
        public SphereBVH VertexBVH = null;
        /// <summary>
        /// The hierarchy of spheres of the faces, built on demand by the searches (null when it is not up to date)
        /// </summary>
        public SphereBVH FaceBVH = null;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an empty mesh
        /// </summary>
        public Mesh()
        {
            _vertices = new MeshBaseCollection<MeshVertex>();
            _faces = new MeshBaseCollection<MeshFace>();
            _edges = new MeshBaseCollection<MeshEdge>();
            _volumes = new MeshBaseCollection<MeshVolume>();
        }

        /// <summary>
        /// Deserialization constructor: reads the id and the collections of vertices, faces, edges and volumes
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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
        /// Add a face to the mesh (see <see cref="AddFaceMesh(MeshVertex[], double)"/>, with the default tolerance)
        /// </summary>
        /// <param name="points">Points that will be converted in MeshVertex (3 or 4)</param>
        public void AddFaceMesh(Point3d[] points)
        {
            MeshVertex[] vertices = new MeshVertex[points.Length];
            for (int i = 0; i < points.Length; i++)
                vertices[i] = new MeshVertex(points[i]);
            AddFaceMesh(vertices);
        }

        /// <summary>
        /// Add a face to the mesh. A vertex closer than <paramref name="tolerance"/> to an existing vertex of the mesh is merged with it
        /// (vertices of the same face are never merged together); the new vertices are copies of the given ones
        /// </summary>
        /// <param name="vertices">The vertices of the face (3 or 4), in order</param>
        /// <param name="tolerance">The distance within which a vertex is merged with an existing one</param>
        /// <returns>The id of the new face</returns>
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
            FaceBVH = null;

            return face.Id;
        }

        /// <summary>
        /// Adds the edge between two vertices, if it is not already in the mesh (in either direction)
        /// </summary>
        /// <param name="edgeKeys">The keys of the existing edges</param>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        private void AddEdgeOnce(HashSet<long> edgeKeys, int a, int b)
        {
            if (edgeKeys.Add(EdgeKey(a, b)))
                _edges.Add(new MeshEdge(a, b));
        }

        /// <summary>
        /// The keys of the edges (see <see cref="EdgeKey"/>)
        /// </summary>
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

        /// <summary>
        /// The spatial grid of the vertices
        /// </summary>
        /// <param name="tolerance">The tolerance of the searches</param>
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
            /// <summary>
            /// The smallest size of a cell (used for a zero tolerance)
            /// </summary>
            public const double MinCellSize = 1E-9;

            /// <summary>
            /// The vertices of each cell, by the indices of the cell along X, Y, Z
            /// </summary>
            private readonly Dictionary<(long, long, long), List<MeshVertex>> _cells = new Dictionary<(long, long, long), List<MeshVertex>>();

            /// <summary>
            /// The side of the cubic cells
            /// </summary>
            public readonly double CellSize;
            /// <summary>
            /// The vertex collection of the grid
            /// </summary>
            public readonly MeshBaseCollection<MeshVertex> Collection;
            /// <summary>
            /// The version of <see cref="Collection"/> indexed by the grid
            /// </summary>
            public int Version;

            /// <summary>
            /// Creates the grid of the vertices of a collection
            /// </summary>
            /// <param name="vertices">The vertices</param>
            /// <param name="cellSize">The side of the cells</param>
            public VertexGrid(MeshBaseCollection<MeshVertex> vertices, double cellSize)
            {
                CellSize = cellSize;
                Collection = vertices;
                foreach (MeshVertex vertex in vertices)
                    Add(vertex);
                Version = vertices.Version;
            }

            /// <summary>
            /// The index of the cell of a coordinate
            /// </summary>
            /// <param name="coordinate">The coordinate</param>
            /// <returns>The index along the axis</returns>
            private long Cell(double coordinate)
            {
                return (long)Math.Floor(coordinate / CellSize);
            }

            /// <summary>
            /// Adds a vertex to its cell
            /// </summary>
            /// <param name="vertex">The vertex</param>
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

            /// <summary>
            /// The closest vertex to a point
            /// </summary>
            /// <param name="point">The point</param>
            /// <param name="tolerance">The largest distance</param>
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

        /// <summary>
        /// Sets the ids of the vertices (of the caller's instances) and adds the face (see <see cref="AddFaceMesh(MeshVertex[], double)"/>):
        /// the new vertices keep the given ids, the merged ones take the id of the existing vertex
        /// </summary>
        /// <param name="vertices">The vertices of the face (3 or 4), in order</param>
        /// <param name="verticesIds">The ids of the vertices</param>
        /// <param name="tolerance">The distance within which a vertex is merged with an existing one</param>
        /// <returns>The id of the new face</returns>
        public int AddFaceMesh(MeshVertex[] vertices, int[] verticesIds, double tolerance = GeometryBase.Tolerance)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Id = verticesIds[i];
            return AddFaceMesh(vertices, tolerance);
        }

        /// <summary>
        /// Adds a volume: all its vertices are added (the instances, never merged with the existing ones) with the edges of the two bases and the
        /// lateral ones (not checked for duplicates)
        /// </summary>
        /// <param name="vertices">The vertices: the first base, then the second one in the same order (6 or 8)</param>
        /// <returns>The id of the new volume</returns>
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
        /// Add a vertex (the instance), unless there is already a vertex closer than <paramref name="tol"/>
        /// </summary>
        /// <param name="vertex">The vertex to add</param>
        /// <param name="tol">The distance within which the vertex is merged with an existing one</param>
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
        /// <param name="edge">The edge</param>
        /// <returns>The distance of its vertices</returns>
        /// <exception cref="ArgumentException">If a vertex of the edge is not in the mesh</exception>
        public double GetEdgeLength(MeshEdge edge)
        {
            if (!_vertices.Contains(edge.A))
                throw new ArgumentException($"Edge vertex:{edge.A} not found");

            if (!_vertices.Contains(edge.B))
                throw new ArgumentException($"Edge vertex:{edge.B} not found");

            return _vertices.GetElementById(edge.A).Point.DistanceTo(_vertices.GetElementById(edge.B).Point);
        }

        /// <summary>
        /// The area of a face
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The area of the face: half the length of its Newell vector (the area of a planar face; for a non planar quadrilateral
        /// the area of its projection on the mean plane)</returns>
        /// <param name="tolerance">Not used (kept for compatibility: the area was computed by a <see cref="Polygon3d"/>)</param>
        /// <exception cref="ArgumentException">If a vertex of the face is not in the mesh</exception>
        public double GetFaceArea(MeshFace face, double tolerance = GeometryBase.Tolerance)
        {
            Point3d[] p = GetCheckedFacePoints(face);
            NewellVector(p, out double nx, out double ny, out double nz);
            return Math.Sqrt(nx * nx + ny * ny + nz * nz) / 2.0;
        }

        /// <summary>
        /// The centroid of a face
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The centroid of the face: the mean of the vertices for a triangle, the centroid of the area for a quadrilateral
        /// (the two triangles A B C and A C D weighted by their signed areas)</returns>
        /// <param name="tolerance">Not used (kept for compatibility: the centroid was computed by a <see cref="Polygon3d"/>)</param>
        /// <exception cref="ArgumentException">If a vertex of the face is not in the mesh</exception>
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

        /// <summary>
        /// The positions of the vertices of a face, checking that they are in the mesh
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The points of the vertices (the instances of the mesh)</returns>
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
        /// <param name="p">The vertices of the polygon</param>
        /// <param name="nx">The X component</param>
        /// <param name="ny">The Y component</param>
        /// <param name="nz">The Z component</param>
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

        /// <summary>
        /// The signed area of a triangle respect to a direction
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="c">The third vertex</param>
        /// <param name="nx">The X component of the direction</param>
        /// <param name="ny">The Y component of the direction</param>
        /// <param name="nz">The Z component of the direction</param>
        /// <returns>Twice the area of the triangle, signed with respect to the direction (nx, ny, nz) (not normalized: only the sign and the ratios matter)</returns>
        private static double SignedTriangleArea(Point3d a, Point3d b, Point3d c, double nx, double ny, double nz)
        {
            double ux = b.X - a.X, uy = b.Y - a.Y, uz = b.Z - a.Z;
            double vx = c.X - a.X, vy = c.Y - a.Y, vz = c.Z - a.Z;
            return (uy * vz - uz * vy) * nx + (uz * vx - ux * vz) * ny + (ux * vy - uy * vx) * nz;
        }

        /// <summary>
        /// The positions of the vertices of a face
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The points of the vertices (the instances of the mesh)</returns>
        /// <exception cref="KeyNotFoundException">If a vertex of the face is not in the mesh</exception>
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

        /// <summary>
        /// The positions of the vertices of an edge
        /// </summary>
        /// <param name="edge">The edge</param>
        /// <returns>The points of the two vertices (the instances of the mesh)</returns>
        public Point3d[] GetEdgePoints(MeshEdge edge)
        {
            MeshVertex[] vertices = GetEdgeVertices(edge);
            return vertices.Select(i => i.Point).ToArray();
        }

        /// <summary>
        /// The positions of the vertices of a volume
        /// </summary>
        /// <param name="volume">The volume</param>
        /// <returns>The points of the vertices (the instances of the mesh), in the order of the volume</returns>
        public Point3d[] GetVolumePoints(MeshVolume volume)
        {
            MeshVertex[] vertices = GetVolumeVertices(volume);
            return vertices.Select(i => i.Point).ToArray();
        }

        #endregion

        #region Getter: elements

        /// <summary>
        /// The face with an id
        /// </summary>
        /// <param name="id">The id of the face</param>
        /// <returns>The face</returns>
        /// <exception cref="KeyNotFoundException">If no face has the id</exception>
        public MeshFace GetFace(int id)
        {
            return _faces.GetElementById(id);
        }

        /// <summary>
        /// The vertex with an id
        /// </summary>
        /// <param name="id">The id of the vertex</param>
        /// <returns>The vertex</returns>
        /// <exception cref="KeyNotFoundException">If no vertex has the id</exception>
        public MeshVertex GetVertex(int id)
        {
            return _vertices.GetElementById(id);
        }

        /// <summary>
        /// The edge with an id
        /// </summary>
        /// <param name="id">The id of the edge</param>
        /// <returns>The edge</returns>
        /// <exception cref="KeyNotFoundException">If no edge has the id</exception>
        public MeshEdge GetEdge(int id)
        {
            return _edges.GetElementById(id);
        }

        /// <summary>
        /// The volume with an id
        /// </summary>
        /// <param name="id">The id of the volume</param>
        /// <returns>The volume</returns>
        /// <exception cref="KeyNotFoundException">If no volume has the id</exception>
        public MeshVolume GetVolume(int id)
        {
            return _volumes.GetElementById(id);
        }

        /// <summary>
        /// Get the edges of a MeshFace: the edges of the mesh with both the vertices in the face (also a diagonal of a quadrangle, if it is an edge).
        /// This is an O(n) operation
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The edges of the face</returns>
        public MeshEdge[] GetFaceEdges(MeshFace face)
        {
            int[] vertexIds = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };
            return _edges.Where(e => vertexIds.Contains(e.A) && vertexIds.Contains(e.B)).ToArray();
        }

        /// <summary>
        /// Get the vertices of the given face
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The vertices array</returns>
        /// <exception cref="KeyNotFoundException">If a vertex of the face is not in the mesh</exception>
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
        /// <param name="edge">The edge</param>
        /// <returns>The vertices array</returns>
        /// <exception cref="KeyNotFoundException">If a vertex of the edge is not in the mesh</exception>
        public MeshVertex[] GetEdgeVertices(MeshEdge edge)
        {
            return new[]
            {
                _vertices.GetElementById(edge.A),
                _vertices.GetElementById(edge.B)
            };
        }

        /// <summary>
        /// Get the vertices of the given volume
        /// </summary>
        /// <param name="volume">The volume</param>
        /// <returns>The vertices array, in the order of the volume</returns>
        /// <exception cref="KeyNotFoundException">If a vertex of the volume is not in the mesh</exception>
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

        /// <summary>
        /// Enumerates the faces
        /// </summary>
        /// <returns>The enumerator of the faces</returns>
        public IEnumerator<MeshFace> GetFacesEnumerator()
        {
            return _faces.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the vertices
        /// </summary>
        /// <returns>The enumerator of the vertices</returns>
        public IEnumerator<MeshVertex> GetVerticesEnumerator()
        {
            return _vertices.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the edges
        /// </summary>
        /// <returns>The enumerator of the edges</returns>
        public IEnumerator<MeshEdge> GetEdgesEnumerator()
        {
            return _edges.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the volumes
        /// </summary>
        /// <returns>The enumerator of the volumes</returns>
        public IEnumerator<MeshVolume> GetVolumesEnumerator()
        {
            return _volumes.GetEnumerator();
        }

        /// <summary>
        /// The faces (the instances of the mesh, in a new array)
        /// </summary>
        /// <returns>The faces</returns>
        public MeshFace[] GetFaces()
        {
            return _faces.ToArray();
        }

        /// <summary>
        /// The vertices (the instances of the mesh, in a new array)
        /// </summary>
        /// <returns>The vertices</returns>
        public MeshVertex[] GetVertices()
        {
            return _vertices.ToArray();
        }

        /// <summary>
        /// The edges (the instances of the mesh, in a new array)
        /// </summary>
        /// <returns>The edges</returns>
        public MeshEdge[] GetEdges()
        {
            return _edges.ToArray();
        }

        /// <summary>
        /// The volumes (the instances of the mesh, in a new array)
        /// </summary>
        /// <returns>The volumes</returns>
        public MeshVolume[] GetVolumes()
        {
            return _volumes.ToArray();
        }

        /// <summary>
        /// The ids of the vertices
        /// </summary>
        /// <returns>The ids, in the order of the collection</returns>
        public int[] GetVerticeIds()
        {
            return _vertices.Select(i => i.Id).ToArray();
        }

        /// <summary>
        /// The faces by id
        /// </summary>
        /// <returns>A new dictionary from the id to the face</returns>
        public Dictionary<int, MeshFace> GetFacesDictionary()
        {
            var res = new Dictionary<int, MeshFace>();
            foreach (var face in _faces)
            {
                res.Add(face.Id, face);
            }
            return res;
        }

        /// <summary>
        /// The vertices by id
        /// </summary>
        /// <returns>A new dictionary from the id to the vertex</returns>
        public Dictionary<int, MeshVertex> GetVerticesDictionary()
        {
            var res = new Dictionary<int, MeshVertex>();
            foreach (var vertex in _vertices)
            {
                res.Add(vertex.Id, vertex);
            }
            return res;
        }

        /// <summary>
        /// The edges by id
        /// </summary>
        /// <returns>A new dictionary from the id to the edge</returns>
        public Dictionary<int, MeshEdge> GetEdgesDictionary()
        {
            var res = new Dictionary<int, MeshEdge>();
            foreach (var edge in _edges)
            {
                res.Add(edge.Id, edge);
            }
            return res;
        }

        /// <summary>
        /// The edges of the mesh not shared by two faces with opposite directions: the boundary edges of a mesh with consistently oriented faces
        /// (and the edges of no face)
        /// </summary>
        /// <returns>The naked edges</returns>
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
        /// Tells if the mesh has a face with the id of the given face
        /// </summary>
        /// <param name="face">The face to test</param>
        /// <returns>True if the face already exists</returns>
        public bool FaceExists(MeshFace face)
        {
            return _faces.Contains(face);
        }

        /// <summary>
        /// Tells if the face specified by the ids of the nodes already exists
        /// </summary>
        /// <param name="ids">The nodes ids array</param>
        /// <returns>True if the face already exists</returns>
        /// <remarks>The faces with the same nodes, in any order, are searched (before, a new face without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool FaceExists(int[] ids)
        {
            return _faces.Any(f => SameNodes(f.GetNodes(), ids));
        }

        /// <summary>
        /// Tell if two arrays have the same nodes
        /// </summary>
        /// <param name="a">The first array</param>
        /// <param name="b">The second array</param>
        /// <returns>True if the two arrays have the same nodes, in any order</returns>
        private static bool SameNodes(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;

            var nodes = new HashSet<int>(a);
            return nodes.SetEquals(b);
        }

        /// <summary>
        /// Return the area of the given face: a triangle, or a quadrangle as the sum of the triangles A B C and A C D (not signed)
        /// </summary>
        /// <param name="face">The face</param>
        /// <returns>The face area</returns>
        /// <exception cref="KeyNotFoundException">If a vertex of the face is not in the mesh</exception>
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
        /// Return the area of the face with the given vertices (see <see cref="FaceArea(MeshFace)"/>)
        /// </summary>
        /// <param name="ids">The nodes ids defining the face (3 or 4)</param>
        /// <returns>The face area</returns>
        public double FaceArea(int[] ids)
        {
            MeshFace face = new MeshFace(ids);
            return FaceArea(face);
        }

        /// <summary>
        /// Tells if the mesh has a volume with the id of the given volume
        /// </summary>
        /// <param name="volume">The volume to test</param>
        /// <returns>True if the volume already exists</returns>
        public bool VolumeExists(MeshVolume volume)
        {
            return _volumes.Contains(volume);
        }

        /// <summary>
        /// Tells if the volume specified by the ids of the nodes already exists
        /// </summary>
        /// <param name="ids">The nodes ids array</param>
        /// <returns>True if the volume already exists</returns>
        /// <remarks>The volumes with the same nodes, in any order, are searched (before, a new volume without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool VolumeExists(int[] ids)
        {
            return _volumes.Any(v => SameNodes(v.GetNodes(), ids));
        }

        /// <summary>
        /// Tells if the mesh has an edge with the id of the given edge
        /// </summary>
        /// <param name="edge">The edge to test</param>
        /// <returns>True if the edge already exists</returns>
        public bool EdgeExists(MeshEdge edge)
        {
            return _edges.Contains(edge);
        }

        /// <summary>
        /// Tells if the mesh has an edge between two vertices
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <returns>True if the edge already exists</returns>
        /// <remarks>The edges between the two nodes, in any direction, are searched (before, a new edge without id was searched by id: always false).
        /// This is an O(n) operation</remarks>
        public bool EdgeExists(int a, int b)
        {
            return _edges.Any(e => (e.A == a && e.B == b) || (e.A == b && e.B == a));
        }

        /// <summary>
        /// Tells if the mesh has a vertex with the id of the given vertex
        /// </summary>
        /// <param name="meshVertex">The vertex to test</param>
        /// <returns>True if the vertex already exists</returns>
        public bool VertexExist(MeshVertex meshVertex)
        {
            return _vertices.Contains(meshVertex);
        }

        /// <summary>
        /// Find the point of intersection between this mesh and a semi-infinite line: the vertices on the line, the crossed edges and the crossed
        /// faces (for a quadrangle only its triangle A B C is checked)
        /// </summary>
        /// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
        /// <param name="stopAtFirstIntersection">When the first intersection solution is found it stops execution (the vertices nearer to the line
        /// are checked first). Use false if you want to do a search on all mesh elements, it can be useful for check.</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <returns>The intersection points with the element containing the point.</returns>
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
        /// Move mesh by a given vector (the vertices in place)
        /// </summary>
        /// <param name="displacement">The translation</param>
        public void Move(Vector3d displacement)
        {
            Move(displacement.X, displacement.Y, displacement.Z);
        }

        /// <summary>
        /// Move mesh by an given increment (the vertices in place); the spatial indices are reset
        /// </summary>
        /// <param name="dX">The translation along X</param>
        /// <param name="dY">The translation along Y</param>
        /// <param name="dz">The translation along Z</param>
        public void Move(double dX, double dY, double dz)
        {
            foreach (var v in _vertices)
                v.Point.Move(dX, dY, dz);

            // the spatial indices refer to the old positions
            _vertexGrid = null;
            VertexBVH = null;
            FaceBVH = null;
        }

        /// <summary>
        /// Adds the elements of another mesh (see <see cref="JoinMesh(Mesh, out Dictionary{int, int}, out Dictionary{int, int}, out Dictionary{int, int})"/>)
        /// </summary>
        /// <param name="meshToJoin">The mesh to add</param>
        public void JoinMesh(Mesh meshToJoin)
        {
            JoinMesh(meshToJoin, out _, out _, out _);
        }

        /// <summary>
        /// The previous version of <see cref="JoinMesh(Mesh, out Dictionary{int, int}, out Dictionary{int, int}, out Dictionary{int, int})"/>: the
        /// vertices are merged by exact coordinates; edges, faces and volumes by their nodes, after resolving hash collisions
        /// </summary>
        /// <param name="meshToJoin">The mesh to add</param>
        /// <param name="vertexIdMap">Map between <see cref="MeshVertex"/>.Id of <paramref name="meshToJoin"/> and id of the same vertex in this mesh (Map old, new)</param>
        /// <param name="facesIdMap">Map between <see cref="MeshFace"/>.Id of <paramref name="meshToJoin"/> and id of the same faces in this mesh (Map old, new) </param>
        /// <param name="volumesIdMap">Map between <see cref="MeshVolume"/>.Id of <paramref name="meshToJoin"/> and id of the same volume in this mesh  (Map old, new)</param>
        public void JoinMeshOld(Mesh meshToJoin, out Dictionary<int, int> vertexIdMap, out Dictionary<int, int> facesIdMap, out Dictionary<int, int> volumesIdMap)
        {
            vertexIdMap = new Dictionary<int, int>();
            facesIdMap = new Dictionary<int, int>();
            volumesIdMap = new Dictionary<int, int>();
            var exactVertices = new Dictionary<Point3d, int>(Point3d.ExactComparer);
            foreach (var vertex in _vertices)
                if (!exactVertices.ContainsKey(vertex.Point)) exactVertices.Add(vertex.Point, vertex.Id);
            foreach (var vertex in meshToJoin._vertices)
            {
                if (!exactVertices.TryGetValue(vertex.Point, out int id))
                {
                    var copy = new MeshVertex(vertex.Point, vertex.Tag);
                    id = _vertices.Add(copy);
                    exactVertices.Add(copy.Point, id);
                }
                vertexIdMap.Add(vertex.Id, id);
            }
            foreach (var edge in meshToJoin._edges)
                _edges.AddUnique(new MeshEdge(vertexIdMap[edge.A], vertexIdMap[edge.B], edge.Tag));
            foreach (var face in meshToJoin._faces)
            {
                var nodes = face.GetNodes();
                for (int i = 0; i < nodes.Length; i++) nodes[i] = vertexIdMap[nodes[i]];
                facesIdMap.Add(face.Id, _faces.AddUnique(new MeshFace(nodes, face.Tag)));
            }
            foreach (var volume in meshToJoin._volumes)
            {
                var nodes = volume.GetNodes();
                for (int i = 0; i < nodes.Length; i++) nodes[i] = vertexIdMap[nodes[i]];
                volumesIdMap.Add(volume.Id, _volumes.AddUnique(new MeshVolume(nodes, volume.Tag)));
            }
        }
        /// <summary>
        /// Adds the elements of another mesh: a vertex closer than the default tolerance to an existing one is merged with it (see
        /// <see cref="AddVertex(MeshVertex, double)"/>), the edges, faces and volumes with the same nodes of existing ones are not added again
        /// </summary>
        /// <param name="meshToJoin">The mesh to add (not changed)</param>
        /// <param name="vertexIdMap">Map between the ids of the vertices of <paramref name="meshToJoin"/> and the ids of the same vertices in this mesh</param>
        /// <param name="facesIdMap">Not filled (empty)</param>
        /// <param name="volumesIdMap">Not filled (empty)</param>
        public void JoinMesh(Mesh meshToJoin, out Dictionary<int, int> vertexIdMap, out Dictionary<int, int> facesIdMap, out Dictionary<int, int> volumesIdMap)
        {
            vertexIdMap = new Dictionary<int, int>();
            facesIdMap = new Dictionary<int, int>();
            volumesIdMap = new Dictionary<int, int>();

            // Index the topology once; scanning AddUnique for every element was quadratic.
            HashSet<long> edgeKeys = GetEdgeKeys();
            var faceKeys = new HashSet<(int, int, int, int)>();
            foreach (var face in _faces) faceKeys.Add((face.A, face.B, face.C, face.D));
            var volumeKeys = new HashSet<(int, int, int, int, int, int, int, int)>();
            foreach (var volume in _volumes) volumeKeys.Add((volume.A, volume.B, volume.C, volume.D, volume.E, volume.F, volume.G, volume.H));

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
                if (edgeKeys.Add(EdgeKey(newEdge.A, newEdge.B))) _edges.Add(newEdge);
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
                if (faceKeys.Add((newFace.A, newFace.B, newFace.C, newFace.D))) _faces.Add(newFace);
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
                if (volumeKeys.Add((newVolume.A, newVolume.B, newVolume.C, newVolume.D, newVolume.E, newVolume.F, newVolume.G, newVolume.H)))
                    _volumes.Add(newVolume);
            }
            _edgeKeysVersion = _edges.Version;
        }

        /// <summary>
        /// Extrude the faces of the mesh along the normals of the vertices (the mean of the normals of the faces at the vertex, times
        /// <paramref name="length"/>): every face becomes a prism
        /// </summary>
        /// <param name="length">The length of the extrusion (the length of the mean of the unit normals)</param>
        /// <returns>A new mesh with the prisms; its bottom vertices are the instances of this mesh</returns>
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

        /// <summary>
        /// Extrude every face of the mesh along a vector: every face becomes a prism; the extruded vertex of a vertex is shared by its faces
        /// </summary>
        /// <param name="extrusion">The extrusion vector</param>
        /// <returns>A new Mesh with all the <see cref="MeshFace"/> extruded to a <see cref="MeshVolume"/>; its bottom vertices are the instances of this mesh</returns>
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

        /// <summary>
        /// Clone the mesh, with copies of its elements (see <see cref="Clone(bool)"/>)
        /// </summary>
        /// <returns>The cloned mesh</returns>
        /// <remarks>The <see cref="MeshVertex"/> Id of the cloned mesh are the same of the original mesh</remarks>
        public object Clone()
        {
            return Clone(false);
        }

        /// <summary>
        /// Clone the mesh, with copies of its vertices, faces, edges, volumes and of the generation options
        /// </summary>
        /// <param name="renumber">
        /// <para>If <see langword="false"/> the <see cref="MeshVertex"/>.Ids of the cloned mesh are the same of the original mesh</para>
        /// <para>If <see langword="true"/> the ids of the cloned elements start from the maximum id of the original mesh + 1 (the tags of the
        /// faces, edges and volumes are not copied)</para>
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
        /// <param name="curve">The segment that defines the line (nothing is done if its length is zero)</param>
        /// <param name="tolerance">The distance within which a vertex is on the line</param>
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

        /// <summary>
        /// The key of an edge, independent of its direction: the smaller id in the high 32 bits, the larger in the low ones
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <returns>The key</returns>
        private static long EdgeKey(int a, int b)
        {
            return a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        }

        /// <summary>
        /// Update the edges after the faces have been divided: the edges divided by a new point (<paramref name="splitPoints"/>: edge key -> point)
        /// are replaced by their two halves; if the edges were the ones of the faces, the new edges of the faces are added too
        /// (the edges not of the faces, e.g. lines, are kept). The duplicated edges are removed
        /// </summary>
        /// <param name="oldFaces">The faces before the division</param>
        /// <param name="newFaces">The faces after the division</param>
        /// <param name="splitPoints">The new points on the divided edges, by edge key</param>
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
        /// <param name="nodes">The vertices of the part, in order</param>
        /// <param name="tag">The tag of the cut face</param>
        /// <param name="faces">The list where the new faces are added</param>
        /// <param name="nextFaceId">The id of the next new face (incremented)</param>
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

        /// <summary>
        /// The sine of the angle of a corner, in the XY plane
        /// </summary>
        /// <param name="previous">The previous vertex</param>
        /// <param name="vertex">The vertex of the corner</param>
        /// <param name="next">The next vertex</param>
        /// <returns>Sine of the angle in <paramref name="vertex"/> from the edge to <paramref name="next"/> to the edge to <paramref name="previous"/> (XY plane)</returns>
        private static double CornerSine(Point3d previous, Point3d vertex, Point3d next)
        {
            double e1x = next.X - vertex.X, e1y = next.Y - vertex.Y, e2x = previous.X - vertex.X, e2y = previous.Y - vertex.Y;
            double lengths = Math.Sqrt((e1x * e1x + e1y * e1y) * (e2x * e2x + e2y * e2y));
            return lengths > 0 ? (e1x * e2y - e1y * e2x) / lengths : 0;
        }

        /// <summary>
        /// A refined copy of a mesh
        /// </summary>
        /// <param name="mesh">The mesh to refine (not changed)</param>
        /// <param name="tolerance">Not used (see <see cref="Refine"/>)</param>
        /// <returns>A refined copy of the mesh (see <see cref="Refine"/>)</returns>
        public static Mesh RefineMesh(Mesh mesh, double tolerance = GeometryBase.Tolerance)
        {
            Mesh newMesh = (Mesh)mesh.Clone();
            newMesh.Refine(tolerance);
            return newMesh;
        }

        /// <summary>
        /// Cleans the mesh: the vertices closer than <paramref name="edgeTolerance"/> and the ends of the shortest edge (two for a quadrangle) of the
        /// faces with area smaller than <paramref name="areaTolerance"/> are collapsed. Every group of collapsed vertices is replaced by a new vertex,
        /// at the mean of the naked vertices of the group (at the mean of all the vertices if none is naked); the edges and the faces are updated,
        /// the degenerate ones removed
        /// </summary>
        /// <param name="edgeTolerance">The distance within which the vertices are collapsed</param>
        /// <param name="areaTolerance">The area under which a face is collapsed</param>
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

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Mesh)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal mesh</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return Equals(obj as Mesh);
        }

        /// <summary>
        /// Equality of the vertices, faces, edges and volumes, in any order (each element equal to the one with the same id and content)
        /// </summary>
        /// <param name="other">The mesh to compare</param>
        /// <returns>True if the meshes are equal</returns>
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

        /// <summary>
        /// The hash code of the elements, independent of their order
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Serializes the <see cref="BaseObject.Guid"/>, the id and the collections of the elements
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Vertices", _vertices, typeof(MeshBaseCollection<MeshVertex>));
            info.AddValue("Faces", _faces, typeof(MeshBaseCollection<MeshFace>));
            info.AddValue("Edges", _edges, typeof(MeshBaseCollection<MeshEdge>));
            info.AddValue("Volumes", _volumes, typeof(MeshBaseCollection<MeshVolume>));
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(Mesh)"/>); two null meshes are equal
        /// </summary>
        /// <param name="obj1">The first mesh</param>
        /// <param name="obj2">The second mesh</param>
        /// <returns>True if the meshes are equal</returns>
        public static bool operator ==(Mesh obj1, Mesh obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Mesh)"/>)
        /// </summary>
        /// <param name="obj1">The first mesh</param>
        /// <param name="obj2">The second mesh</param>
        /// <returns>True if the meshes are different</returns>
        public static bool operator !=(Mesh obj1, Mesh obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// The options of the generation of a mesh
        /// </summary>
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

            /// <summary>
            /// The default options: size 1E+22 (no limit), recombination, no refinement
            /// </summary>
            public GenerateOptions()
            {
                MeshSize = 1E+22;
                Recombine = true;
                Refine = false;
            }

            /// <summary>
            /// Creates a copy of the options
            /// </summary>
            /// <returns>The copy</returns>
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

        /// <summary>
        /// The result of the generation of a mesh: warnings, exceptions with their messages, generated surfaces and execution times
        /// </summary>
        [Serializable]
        public class GenerateMeshStatus
        {
            /// <summary>
            /// The messages of the exceptions (one for each exception)
            /// </summary>
            protected List<string> _customErrorMessages;
            /// <summary>
            /// The warnings
            /// </summary>
            protected List<string> _warnings;
            /// <summary>
            /// The exceptions
            /// </summary>
            protected List<Exception> _exceptions;

            /// <summary>
            /// The number of generated surfaces
            /// </summary>
            protected int _generatedSurfaces;

            /// <summary>
            /// The execution times of the phases, by description
            /// </summary>
            private Dictionary<string, double> _executionTime;

            /// <summary>
            /// The warnings (the list of the status)
            /// </summary>
            public List<string> Warnings => _warnings;

            /// <summary>
            /// The messages of the exceptions (the list of the status)
            /// </summary>
            public List<string> CustomErrorMessages => _customErrorMessages;

            /// <summary>
            /// The exceptions (the list of the status)
            /// </summary>
            public List<Exception> Exceptions => _exceptions;

            /// <summary>
            /// The number of generated surfaces
            /// </summary>
            public int GeneratedSurfaces { get => _generatedSurfaces; set => _generatedSurfaces = value; }

            /// <summary>
            /// The execution times of the phases, by description
            /// </summary>
            public Dictionary<string, double> ExecutionTime { get => _executionTime; set => _executionTime = value; }


            /// <summary>
            /// Creates an empty status
            /// </summary>
            public GenerateMeshStatus()
            {
                _customErrorMessages = new List<string>();
                _warnings = new List<string>();
                _exceptions = new List<Exception>();
                _executionTime = new Dictionary<string, double>();
            }

            /// <summary>
            /// Adds an execution time
            /// </summary>
            /// <param name="message">The description of the phase (unique)</param>
            /// <param name="value">The time</param>
            /// <exception cref="ArgumentException">If the description is already present</exception>
            public void AddExecutionTimeMessage(string message, double value)
            {
                _executionTime.Add(message, value);
            }

            /// <summary>
            /// Adds an exception with its message
            /// </summary>
            /// <param name="exception">The exception</param>
            /// <param name="customErrorMessage">The message</param>
            public void AddException(Exception exception, string customErrorMessage)
            {
                _exceptions.Add(exception);
                _customErrorMessages.Add(customErrorMessage);
            }

            /// <summary>
            /// Adds a warning
            /// </summary>
            /// <param name="warning">The warning</param>
            public void AddWarning(string warning)
            {
                _warnings.Add(warning);
            }

            /// <summary>
            /// The last exception
            /// </summary>
            /// <returns>The last exception; null if there are none</returns>
            public Exception GetLastException()
            {
                return _exceptions.LastOrDefault();
            }

            /// <summary>
            /// The message of the last exception
            /// </summary>
            /// <returns>The message; null if there are none</returns>
            public string GetLastCustomErrorMessage()
            {
                return _customErrorMessages.LastOrDefault();
            }

            /// <summary>
            /// The first exception
            /// </summary>
            /// <returns>The first exception; null if there are none</returns>
            public Exception GetFirstException()
            {
                return _exceptions.FirstOrDefault();
            }

            /// <summary>
            /// The message of the first exception
            /// </summary>
            /// <returns>The message; null if there are none</returns>
            public string GetFirstCustomErrorMessage()
            {
                return _customErrorMessages.FirstOrDefault();
            }
        }

        #endregion

        /// <summary>
        /// Builds the hierarchy of spheres of the vertices (<see cref="VertexBVH"/>)
        /// </summary>
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
            _vertexBvhVertices = _vertices;
            _vertexBvhVersion = _vertices.Version;
        }

        /// <summary>
        /// Builds the hierarchy of spheres of the faces (<see cref="FaceBVH"/>)
        /// </summary>
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
            _faceBvhVertices = _vertices;
            _faceBvhFaces = _faces;
            _faceBvhVertexVersion = _vertices.Version;
            _faceBvhFaceVersion = _faces.Version;
        }

        /// <summary>
        /// The vertices near a point (the hierarchy of the vertices is built if it is not up to date)
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="range">The largest distance</param>
        /// <returns>The ids of the vertices not farther than <paramref name="range"/></returns>
        public List<int> FindNeighbours(Point3d point, double range)
        {
            if (VertexBVH == null || !ReferenceEquals(_vertexBvhVertices, _vertices) || _vertexBvhVersion != _vertices.Version)
            {
                UpdateVertexBVH();
            }

            return VertexBVH.GetIntersections(point, range);
        }

        /// <summary>
        /// The face crossed by a ray nearest to its start, in the direction of the ray (both triangles of a quadrangle are checked;
        /// the faces with aligned vertices are skipped)
        /// </summary>
        /// <param name="ray">The ray: its point and its direction</param>
        /// <param name="face">The face; null if none is crossed</param>
        /// <param name="intersectionPoint">The intersection point; null if no face is crossed</param>
        /// <returns>True if a face is crossed</returns>
        public bool PickFace(Ray3d ray, out MeshFace face, out Point3d intersectionPoint)
        {
            face = null;
            intersectionPoint = null;
            const double precision = 1e-12;
            if (FaceBVH == null || !ReferenceEquals(_faceBvhVertices, _vertices) || !ReferenceEquals(_faceBvhFaces, _faces) ||
                _faceBvhVertexVersion != _vertices.Version || _faceBvhFaceVersion != _faces.Version)
                UpdateFaceBVH();

            double distance = double.MaxValue;
            Point3d rayEnd = ray.Point + ray.Direction;
            foreach (int id in FaceBVH.GetRayIntersections(ray))
            {
                MeshFace candidate = GetFace(id);
                Point3d[] points = GetFacePoints(candidate);
                int first = 0;
                if (points.Length == 4)
                {
                    NewellVector(points, out double nx, out double ny, out double nz);
                    // Use the interior diagonal, including for concave quadrangles.
                    if (SignedTriangleArea(points[0], points[1], points[2], nx, ny, nz) < 0 ||
                        SignedTriangleArea(points[0], points[2], points[3], nx, ny, nz) < 0)
                        first = 1;
                }
                for (int t = 1; t < points.Length - 1; t++)
                {
                    Point3d a = points[first], b = points[(first + t) % points.Length], c = points[(first + t + 1) % points.Length];
                    if (a == b || b == c || c == a) continue;
                    var ab = new Vector3d(a, b);
                    var ac = new Vector3d(a, c);
                    ab.Unitize();
                    ac.Unitize();
                    if (ab.CrossProduct(ac).Norm() < GeometryBase.AngularTolerance) continue;
                    Plane.GetIntersectionTriangleWihtRay(a, b, c, ray.Point, rayEnd,
                        out double u, out double v, out double s, out Point3d intersection);
                    if (intersection != null && s > 0 && u >= -precision && v >= -precision && u + v <= 1 + precision)
                    {
                        double d = ray.Point.SquareDistanceTo(intersection);
                        if (d < distance)
                        {
                            distance = d;
                            face = candidate;
                            intersectionPoint = intersection;
                        }
                    }
                }
            }
            return face != null;
        }
    }
}
