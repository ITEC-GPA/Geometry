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
            AddFaceMesh(points.Select(i => new MeshVertex(i)).ToArray());
        }

        public int AddFaceMesh(MeshVertex[] vertices, double tolerance = GeometryBase.Tolerance)
        {
            UpdateVertexBVH();

            int[] verticesIds = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                List<int> ids = FindNeighbours(vertices[i].Point, tolerance);

                if (ids.Count == 0)
                    verticesIds[i] = _vertices.Add(new MeshVertex(vertices[i]));
                else
                    verticesIds[i] = ids.FirstOrDefault();

                if (i > 0)
                    _edges.Add(new MeshEdge(verticesIds[i - 1], verticesIds[i]));
            }
            _edges.Add(new MeshEdge(verticesIds[vertices.Length - 1], verticesIds[0]));

            MeshFace face = new MeshFace(verticesIds);
            _faces.Add(face);

            return face.Id;
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

        public int AddVertex(MeshVertex vertex, double tol = GeometryBase.Tolerance)
        {
            var neighbours = FindNeighbours(vertex.Point, tol);
            if (neighbours.Count > 0)
            {
                // TODO: return closest one
                return neighbours[0];
            }

            _vertices.Add(vertex);
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
            var vertex1 = _vertices.Where(i => i.Id == edge.A).DefaultIfEmpty(null).FirstOrDefault();
            var vertex2 = _vertices.Where(i => i.Id == edge.B).DefaultIfEmpty(null).FirstOrDefault();

            if (vertex1 == null)
                throw new ArgumentException($"Edge vertex:{edge.A} not found");

            if (vertex2 == null)
                throw new ArgumentException($"Edge vertex:{edge.B} not found");

            return vertex1.Point.DistanceTo(vertex2.Point);
        }

        public double GetFaceArea(MeshFace face, double tolerance = GeometryBase.Tolerance)
        {
            var vertex1 = _vertices.GetElementById(face.A);
            var vertex2 = _vertices.GetElementById(face.B);
            var vertex3 = _vertices.GetElementById(face.C);

            if (vertex1 == null)
                throw new ArgumentException($"Face vertex:{face.A} not found");

            if (vertex2 == null)
                throw new ArgumentException($"Face vertex:{face.B} not found");

            if (vertex3 == null)
                throw new ArgumentException($"Face vertex:{face.C} not found");


            Polygon3d p = new Polygon3d()
            {
                vertex1.Point,
                vertex2.Point,
                vertex3.Point
            };

            if (face.IsQuad)
            {
                var vertex4 = _vertices.GetElementById(face.D);
                if (vertex4 == null)
                    throw new ArgumentException($"Face vertex:{face.D} not found");
                p.Add(vertex4.Point);
            }

            return Math.Abs(p.GetSignedArea(tolerance));
        }

        public Point3d GetFaceCentroid(MeshFace face, double tolerance = GeometryBase.Tolerance)
        {
            var vertex1 = _vertices.GetElementById(face.A);
            var vertex2 = _vertices.GetElementById(face.B);
            var vertex3 = _vertices.GetElementById(face.C);

            if (vertex1 == null)
                throw new ArgumentException($"Face vertex:{face.A} not found");

            if (vertex2 == null)
                throw new ArgumentException($"Face vertex:{face.B} not found");

            if (vertex3 == null)
                throw new ArgumentException($"Face vertex:{face.C} not found");


            Polygon3d poly = new Polygon3d()
            {
                vertex1.Point,
                vertex2.Point,
                vertex3.Point
            };

            if (face.IsQuad)
            {
                var vertex4 = _vertices.GetElementById(face.D);
                if (vertex4 == null)
                    throw new ArgumentException($"Face vertex:{face.D} not found");
                poly.Add(vertex4.Point, tolerance);
                return poly.GetCentroid();
            }
            else if (face.IsTriangle)
            {
                return poly.GetCenter(); // se è triangolo centroide == centro
            }

            return poly.GetCentroid();
        }

        public Point3d[] GetFacePoints(MeshFace face)
        {
            MeshVertex[] vertices = GetFaceVertices(face);
            return vertices.Select(i => i.Point).ToArray();
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
        public bool FaceExists(int[] ids)
        {
            MeshFace face = new MeshFace(ids);
            return _faces.Contains(face);
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
        public bool VolumeExists(int[] ids)
        {
            return _volumes.Contains(new MeshVolume(ids));
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool EdgeExists(MeshEdge edge)
        {
            return _edges.Contains(edge);
        }

        /// <inheritdoc cref="MeshBaseCollection{T}.Contains(T)"/>
        public bool EdgeExists(int a, int b)
        {
            return _edges.Contains(new MeshEdge(a, b));
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
            UpdateVertexBVH();

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

            // Save added edges here
            var addedEdges = new HashSet<int>();
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
                        if (!addedEdges.Contains(edges[k].GetHashCode()))
                        {
                            addedEdges.Add(edges[k].GetHashCode());
                            copy._edges.Add(edges[k]);
                        }
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
                        if (!addedEdges.Contains(edges[k].GetHashCode()))
                        {
                            addedEdges.Add(edges[k].GetHashCode());
                            copy._edges.Add(edges[k]);
                        }
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

        public void Refine(double tolerance = GeometryBase.Tolerance)
        {
            Mesh mesh = RefineMesh(this, tolerance);

            _vertices.Clear();
            _edges.Clear();
            _faces.Clear();
            _volumes.Clear();

            _vertices.AddRange(mesh.Vertices.ToArray());
            _edges.AddRange(mesh.Edges.ToArray());
            _faces.AddRange(mesh.Faces.ToArray());
            _volumes.AddRange(mesh.Volumes.ToArray());
        }

        public void Cut(Line2d curve, double tolerance = GeometryBase.Tolerance)
        {
            Mesh mesh = CutMesh(this, curve, tolerance);
            _vertices = mesh.Vertices;
            _edges = mesh.Edges;
            _faces = mesh.Faces;
            _volumes = mesh.Volumes;
        }

        public static Mesh RefineMesh(Mesh mesh, double tolerance = GeometryBase.Tolerance)
        {
            Dictionary<Point3d, int> oldMap = new Dictionary<Point3d, int>();
            Dictionary<Point3d, int> newMap = new Dictionary<Point3d, int>();

            Mesh newMesh = (Mesh)mesh.Clone();
            for (int i = 0; i < newMesh.VerticesCount; ++i)
            {
                MapAdd(newMesh.Vertices[i].Point, i, ref oldMap, tolerance);
            }
            newMesh.Faces.Clear();

            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                var face = mesh.Faces.ElementAt(f);

                if (face.IsTriangle)
                {
                    Line3d ab = new Line3d(mesh.Vertices.GetElementById(face.A).Point, mesh.Vertices.GetElementById(face.B).Point);
                    Line3d bc = new Line3d(mesh.Vertices.GetElementById(face.B).Point, mesh.Vertices.GetElementById(face.C).Point);
                    Line3d ca = new Line3d(mesh.Vertices.GetElementById(face.C).Point, mesh.Vertices.GetElementById(face.A).Point);

                    Point3d p1 = ab.Mid;
                    Point3d p2 = bc.Mid;
                    Point3d p3 = ca.Mid;

                    var p1j = MapGet(p1, ref newMap, tolerance);
                    if (p1j == -1)
                    {
                        p1j = newMesh.Vertices.Add(new MeshVertex(p1));
                        MapAdd(p1, p1j, ref newMap, tolerance);
                    }
                    var p2j = MapGet(p2, ref newMap, tolerance);
                    if (p2j == -1)
                    {
                        p2j = newMesh.Vertices.Add(new MeshVertex(p2));
                        MapAdd(p2, p2j, ref newMap, tolerance);
                    }
                    var p3j = MapGet(p3, ref newMap, tolerance);
                    if (p3j == -1)
                    {
                        p3j = newMesh.Vertices.Add(new MeshVertex(p3));
                        MapAdd(p3, p3j, ref newMap, tolerance);
                    }

                    MeshFace faceBuffer1 = new MeshFace(new int[] { face.A, p1j, p3j });
                    MeshFace faceBuffer2 = new MeshFace(new int[] { p1j, p2j, p3j });
                    MeshFace faceBuffer3 = new MeshFace(new int[] { face.B, p2j, p1j });
                    MeshFace faceBuffer4 = new MeshFace(new int[] { face.C, p3j, p2j });
                    newMesh.Faces.Add(faceBuffer1);
                    newMesh.Faces.Add(faceBuffer2);
                    newMesh.Faces.Add(faceBuffer3);
                    newMesh.Faces.Add(faceBuffer4);
                }
                else
                {
                    Line3d ab = new Line3d(mesh.Vertices.GetElementById(face.A).Point, mesh.Vertices.GetElementById(face.B).Point);
                    Line3d bc = new Line3d(mesh.Vertices.GetElementById(face.B).Point, mesh.Vertices.GetElementById(face.C).Point);
                    Line3d cd = new Line3d(mesh.Vertices.GetElementById(face.C).Point, mesh.Vertices.GetElementById(face.D).Point);
                    Line3d da = new Line3d(mesh.Vertices.GetElementById(face.D).Point, mesh.Vertices.GetElementById(face.A).Point);

                    Point3d p1 = ab.Mid;
                    Point3d p2 = bc.Mid;
                    Point3d p3 = cd.Mid;
                    Point3d p4 = da.Mid;

                    Polygon3d poly = new Polygon3d(new Point3d[] { p1, p2, p3, p4 });

                    Point3d p5 = poly.GetCenter();

                    var p1j = MapGet(p1, ref newMap, tolerance);
                    if (p1j == -1)
                    {
                        p1j = newMesh.Vertices.Add(new MeshVertex(p1));
                        MapAdd(p1, p1j, ref newMap, tolerance);
                    }
                    var p2j = MapGet(p2, ref newMap, tolerance);
                    if (p2j == -1)
                    {
                        p2j = newMesh.Vertices.Add(new MeshVertex(p2));
                        MapAdd(p2, p2j, ref newMap, tolerance);
                    }
                    var p3j = MapGet(p3, ref newMap, tolerance);
                    if (p3j == -1)
                    {
                        p3j = newMesh.Vertices.Add(new MeshVertex(p3));
                        MapAdd(p3, p3j, ref newMap, tolerance);
                    }
                    var p4j = MapGet(p4, ref newMap, tolerance);
                    if (p4j == -1)
                    {
                        p4j = newMesh.Vertices.Add(new MeshVertex(p4));
                        MapAdd(p4, p4j, ref newMap, tolerance);
                    }

                    var p5j = newMesh.Vertices.Add(new MeshVertex(p5));
                    MapAdd(p5, p5j, ref newMap, tolerance);

                    MeshFace faceBuffer1 = new MeshFace(new int[] { face.A, p1j, p5j, p4j });
                    MeshFace faceBuffer2 = new MeshFace(new int[] { p1j, face.B, p2j, p5j });
                    MeshFace faceBuffer3 = new MeshFace(new int[] { p5j, p2j, face.C, p3j });
                    MeshFace faceBuffer4 = new MeshFace(new int[] { p4j, p5j, p3j, face.D });
                    newMesh.Faces.Add(faceBuffer1);
                    newMesh.Faces.Add(faceBuffer2);
                    newMesh.Faces.Add(faceBuffer3);
                    newMesh.Faces.Add(faceBuffer4);
                }
            }

            return newMesh;
        }

        private static void MapAdd(Point3d vert, int index, ref Dictionary<Point3d, int> map, double tolerance)
        {
            int cifreSignificative = GetCifre(tolerance);

            Point3d buffer = new Point3d(Math.Round(vert.X, cifreSignificative), Math.Round(vert.Y, cifreSignificative), Math.Round(vert.Z, cifreSignificative));
            if (!map.ContainsKey(buffer))
                map.Add(buffer, index);
        }

        private static int MapGet(Point3d vert, ref Dictionary<Point3d, int> map, double tolerance)
        {
            int cifreSignificative = GetCifre(tolerance);

            Point3d buffer = new Point3d(Math.Round(vert.X, cifreSignificative), Math.Round(vert.Y, cifreSignificative), Math.Round(vert.Z, cifreSignificative));

            if (map.ContainsKey(buffer))
            {
                return map[buffer];
            }
            return -1;
        }

        private static int GetCifre(double tolerance)
        {
            int cifreSignificative = 4;
            if (tolerance >= 1)
                cifreSignificative = 0;
            else if (tolerance >= 0.1)
                cifreSignificative = 1;
            else if (tolerance >= 0.01)
                cifreSignificative = 2;
            else if (tolerance >= 0.001)
                cifreSignificative = 3;
            else if (tolerance >= 0.0001)
                cifreSignificative = 4;
            else if (tolerance >= 0.00001)
                cifreSignificative = 5;
            else if (tolerance >= 0.000001)
                cifreSignificative = 6;

            return cifreSignificative;
        }

        private Mesh CutMesh(Mesh mesh, Line2d curve, double tolerance = GeometryBase.Tolerance)
        {
            Dictionary<Point3d, int> oldMap = new Dictionary<Point3d, int>();
            Dictionary<Point3d, int> newMap = new Dictionary<Point3d, int>();

            Mesh newMesh = (Mesh)mesh.Clone();
            for (int i = 0; i < newMesh.VerticesCount; ++i)
            {
                MapAdd(newMesh.Vertices[i].Point, i, ref oldMap, tolerance);
            }
            newMesh.Faces.Clear();

            for (int f = 0; f < mesh.Faces.Count; ++f)
            {
                MeshFace face = mesh.Faces[f];

                if (face.IsQuad)
                {
                    // A quad crossed by the curve is split in two triangles (A, B, C) and (A, C, D) and then cut, the other quads are kept
                    bool isCut = TryCutEdge(face.A, face.B, out _) || TryCutEdge(face.B, face.C, out _) ||
                                 TryCutEdge(face.C, face.D, out _) || TryCutEdge(face.D, face.A, out _);
                    if (!isCut)
                    {
                        // the curve along a diagonal divides the quad without cutting its edges
                        if (IsOnCurve(face.A) && IsOnCurve(face.C))
                        {
                            AddFace(face.A, face.B, face.C);
                            AddFace(face.A, face.C, face.D);
                        }
                        else if (IsOnCurve(face.B) && IsOnCurve(face.D))
                        {
                            AddFace(face.A, face.B, face.D);
                            AddFace(face.B, face.C, face.D);
                        }
                        else
                        {
                            newMesh.Faces.Add(face);
                        }
                        continue;
                    }

                    CutTriangle(face.A, face.B, face.C, null);
                    CutTriangle(face.A, face.C, face.D, null);
                }
                else
                {
                    CutTriangle(face.A, face.B, face.C, face);
                }
            }

            return newMesh;

            // Intersection of the edge (i1, i2) with the curve, only if it is inside the edge and it is not an existing vertex
            bool TryCutEdge(int i1, int i2, out Point3d point)
            {
                point = null;
                var edge = new Line2d(mesh.Vertices.GetElementById(i1).Point, mesh.Vertices.GetElementById(i2).Point);

                if (edge.GetIntersectionWithInfiniteLine(curve, out Point2d inter, tolerance) &&
                    edge.IsPointOnLine(inter, tolerance) &&
                    MapGet(inter, ref oldMap, tolerance) == -1)
                {
                    point = inter;
                    return true;
                }

                return false;
            }

            // True if the vertex lies on the infinite line of the curve (XY plane)
            bool IsOnCurve(int id)
            {
                Point3d p = mesh.Vertices.GetElementById(id).Point;
                double dx = curve.End.X - curve.Start.X;
                double dy = curve.End.Y - curve.Start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length == 0)
                    return false;
                double cross = dx * (p.Y - curve.Start.Y) - dy * (p.X - curve.Start.X);
                return Math.Abs(cross) / length < tolerance;
            }

            // Id of the vertex at the given point, added to the new mesh if not already present
            int GetOrAddVertex(Point3d point)
            {
                var j = MapGet(point, ref newMap, tolerance);
                if (j == -1)
                {
                    j = newMesh.Vertices.Add(new MeshVertex(point));
                    MapAdd(point, j, ref newMap, tolerance);
                }
                return j;
            }

            void AddFace(params int[] nodes)
            {
                newMesh.Faces.Add(new MeshFace(nodes));
            }

            // Cut the triangle (a, b, c). originalFace is added unchanged if the triangle is not cut (null: a new face is created)
            void CutTriangle(int a, int b, int c, MeshFace originalFace)
            {
                bool abCut = TryCutEdge(a, b, out Point3d ptAB);
                bool bcCut = TryCutEdge(b, c, out Point3d ptBC);
                bool caCut = TryCutEdge(c, a, out Point3d ptCA);

                int cutCount = (abCut ? 1 : 0) + (bcCut ? 1 : 0) + (caCut ? 1 : 0);

                if (cutCount == 1)
                {
                    // the curve passes through the vertex opposite to the cut edge: two triangles
                    if (abCut)
                    {
                        int j = GetOrAddVertex(ptAB);
                        AddFace(a, j, c);
                        AddFace(j, b, c);
                    }
                    else if (bcCut)
                    {
                        int j = GetOrAddVertex(ptBC);
                        AddFace(b, j, a);
                        AddFace(j, c, a);
                    }
                    else
                    {
                        int j = GetOrAddVertex(ptCA);
                        AddFace(c, j, b);
                        AddFace(j, a, b);
                    }
                }
                else if (cutCount == 2)
                {
                    // two edges cut: one triangle and one quadrilateral split in two triangles
                    if (!abCut)
                    {
                        int j = GetOrAddVertex(ptBC);
                        int k = GetOrAddVertex(ptCA);
                        AddFace(j, c, k);
                        AddFace(k, a, j);
                        AddFace(a, b, j);
                    }
                    else if (!bcCut)
                    {
                        int j = GetOrAddVertex(ptCA);
                        int k = GetOrAddVertex(ptAB);
                        AddFace(j, a, k);
                        AddFace(k, b, j);
                        AddFace(b, c, j);
                    }
                    else
                    {
                        int j = GetOrAddVertex(ptAB);
                        int k = GetOrAddVertex(ptBC);
                        AddFace(j, b, k);
                        AddFace(k, c, j);
                        AddFace(c, a, j);
                    }
                }
                else
                {
                    // not cut (or degenerate case with three cuts): the triangle is kept
                    if (originalFace != null)
                        newMesh.Faces.Add(originalFace);
                    else
                        AddFace(a, b, c);
                }
            }
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