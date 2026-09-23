using GPC.Geometry;
using GPC.Geometry.Meshes;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Meshes
{
    [TestClass]
    public class MeshEqualsHashCode : UnitTestBase
    {

        internal Mesh CreateSimpleMesh1()
        {
            Mesh mesh = new Mesh();
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(0, 0, 0)),
                new MeshVertex(new Point3d(10, 0, 0)),
                new MeshVertex(new Point3d(5, 10, 0))
            });
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(0, 0, 0)),
                new MeshVertex(new Point3d(10, 0, 0)),
                new MeshVertex(new Point3d(5, -10, 0))
            });
            return mesh;
        }

        private Mesh CreateSimpleMesh2()
        {
            Mesh mesh = new Mesh();
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(0, 0, 0)),
                new MeshVertex(new Point3d(10, 0, 0)),
                new MeshVertex(new Point3d(10, 10, 0)),
                new MeshVertex(new Point3d(0, 10, 0))
            });
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(10, 0, 0)),
                new MeshVertex(new Point3d(20, 0, 0)),
                new MeshVertex(new Point3d(20, 10, 0)),
                new MeshVertex(new Point3d(10, 10, 0))
            });
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(0, 10, 0)),
                new MeshVertex(new Point3d(10, 10, 0)),
                new MeshVertex(new Point3d(10, 20, 0)),
                new MeshVertex(new Point3d(0, 20, 0))
            });
            mesh.AddFaceMesh(new[] {
                new MeshVertex(new Point3d(10, 10, 0)),
                new MeshVertex(new Point3d(20, 10, 0)),
                new MeshVertex(new Point3d(20, 20, 0)),
                new MeshVertex(new Point3d(10, 20, 0))
            });
            return mesh;
        }

        internal Mesh CreateSimpleMesh3(int faceNumberX, int faceNumberY, double pitchX, double pitchY, Point2d startPoint)
        {
            Mesh mesh = new Mesh();
            for (int i = 0; i < faceNumberX; i++)
            {
                for (int j = 0; j < faceNumberY; j++)
                {
                    mesh.AddFaceMesh(new[] {    new MeshVertex(new Point3d(startPoint.X + pitchX * i ,           startPoint.Y + pitchY * j ,                0)),
                                                new MeshVertex(new Point3d(startPoint.X + pitchX * i + pitchX ,  startPoint.Y + pitchY * j ,                0)),
                                                new MeshVertex(new Point3d(startPoint.X + pitchX * i + pitchX ,  startPoint.Y + pitchY * j + pitchY ,       0)),
                                                new MeshVertex(new Point3d(startPoint.X + pitchX * i,            startPoint.Y + pitchY * j + pitchY ,       0))
                                        });
                }
            }
            return mesh;
        }

        /// <summary>
        /// Builds a sphere with triangular faces.
        /// </summary>
        /// <returns></returns>
        internal Mesh CreateSimpleMesh4(in Point3d origin, in double radius, in int faceNumberMeridians, in int faceNumberParallels)
        {
            var mesh = new Mesh();
            Point3d[][] domainPoint = new Point3d[faceNumberMeridians][];

            for (int i = 0; i < faceNumberMeridians; i++)
            {
                domainPoint[i] = new Point3d[faceNumberParallels + 1];
                double theta = 2.0 * Math.PI * i / faceNumberMeridians;

                for (int j = 0; j <= faceNumberParallels; j++)
                {
                    double fi = Math.PI * j / faceNumberParallels;
                    double x = origin.X + radius * Math.Cos(theta) * Math.Sin(fi);
                    double y = origin.Y + radius * Math.Sin(theta) * Math.Sin(fi);
                    double z = origin.Z + radius * Math.Cos(fi);
                    domainPoint[i][j] = new Point3d(x, y, z);
                }
            }

            int progressVertexId = 0;
            int progressEdgeId = 0;
            int progressPlateId = 0;
            Dictionary<Point3d, MeshVertex> pointVertexAssociation = new Dictionary<Point3d, MeshVertex>();
            Dictionary<MeshVertex, int> pointIdAssociation = new Dictionary<MeshVertex, int>();

            for (int i = 0; i < domainPoint.Length; i++)
            {
                for (int j = 0; j < domainPoint[i].Length; j++)
                {
                    bool commonPoint = false;
                    int vertexId = -1;

                    MeshVertex mv = new MeshVertex(domainPoint[i][j]);

                    if (pointVertexAssociation.ContainsKey(domainPoint[i][j]))
                    {
                        vertexId = pointIdAssociation[pointVertexAssociation[domainPoint[i][j]]];
                        commonPoint = true;
                    }

                    if (!commonPoint)
                    {
                        if (!pointIdAssociation.ContainsKey(mv))
                        {
                            vertexId = mesh.Vertices.Add(mv, progressVertexId++);
                        }
                        else
                        {
                            vertexId = pointIdAssociation[mv];
                        }

                        pointIdAssociation.Add(mv, vertexId);
                        pointVertexAssociation.Add(domainPoint[i][j], mv);
                    }

                    if (vertexId == -1)
                    {
                        throw new NotSupportedException("Vertex id not assigned");
                    }

                }
            }

            for (int i = 0; i < domainPoint.Length; i++) // meridians
            {
                int i1 = i;
                int i2 = i + 1;
                if (i2 == domainPoint.Length)
                    i2 = 0;

                for (int j = 0; j < domainPoint[i].Length - 1; j++) // parallels
                {
                    if (j == 0) // pole
                    {
                        var vA = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j]]];
                        var vB = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j + 1]]];
                        var vC = pointIdAssociation[pointVertexAssociation[domainPoint[i2][j + 1]]];

                        mesh.Faces.Add(new MeshFace(vA, vB, vC), progressPlateId++);

                        // only left side
                        mesh.Edges.Add(new MeshEdge(vA, vB), progressEdgeId++);
                    }
                    else if (j == domainPoint[i].Length - 2) // pole
                    {
                        var vA = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j]]];
                        var vB = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j + 1]]];
                        var vC = pointIdAssociation[pointVertexAssociation[domainPoint[i2][j]]];

                        mesh.Faces.Add(new MeshFace(vA, vB, vC), progressPlateId++);

                        // upper and left side
                        mesh.Edges.Add(new MeshEdge(vC, vA), progressEdgeId++);
                        mesh.Edges.Add(new MeshEdge(vA, vB), progressEdgeId++);
                    }
                    else
                    {
                        var vA = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j]]];
                        var vB = pointIdAssociation[pointVertexAssociation[domainPoint[i1][j + 1]]];
                        var vC = pointIdAssociation[pointVertexAssociation[domainPoint[i2][j]]];
                        var vD = pointIdAssociation[pointVertexAssociation[domainPoint[i2][j + 1]]];

                        mesh.Faces.Add(new MeshFace(vA, vB, vD), progressPlateId++);
                        mesh.Faces.Add(new MeshFace(vA, vD, vC), progressPlateId++);

                        // upper, middle and left side
                        mesh.Edges.Add(new MeshEdge(vC, vA), progressEdgeId++);
                        mesh.Edges.Add(new MeshEdge(vA, vD), progressEdgeId++);
                        mesh.Edges.Add(new MeshEdge(vA, vB), progressEdgeId++);
                    }
                }
            }

            return mesh;
        }

        [TestMethod]
        public void Equals1()
        {
            Mesh mesh1 = CreateSimpleMesh3(40, 20, 10, 10, Point2d.Origin);
            Mesh mesh2 = CreateSimpleMesh3(40, 20, 10, 10, Point2d.Origin);

            Assert.AreEqual(mesh1, mesh2);
        }


        [TestMethod]
        public void HashCode1()
        {
            Mesh mesh1 = CreateSimpleMesh3(4, 6, 10, 10, Point2d.Origin);
            Mesh mesh2 = CreateSimpleMesh3(4, 6, 10, 10, Point2d.Origin);

            var h1 = mesh1.GetHashCode();
            var h2 = mesh2.GetHashCode();

            Console.WriteLine(h1);
            Console.WriteLine(h2);
            Assert.AreEqual(h1, h2, $"{h1} {h2}");
        }

        [TestMethod]
        public void HashCode2()
        {
            Mesh mesh1 = CreateSimpleMesh3(40, 20, 10, 10, Point2d.Origin);
            Mesh mesh2 = CreateSimpleMesh3(40, 20, 10, 10, Point2d.Origin);

            var h1 = mesh1.GetHashCode();
            var h2 = mesh2.GetHashCode();

            Console.WriteLine(h1);
            Console.WriteLine(h2);
            Assert.AreEqual(h1, h2, $"{h1} {h2}");
        }

        [TestMethod]
        public void HashCode3()
        {
            MeshVertex v1 = new MeshVertex(new Point3d(0, 1, 0));
            MeshVertex v2 = new MeshVertex(new Point3d(0, 2, 0));
            MeshVertex v3 = new MeshVertex(new Point3d(0, 3, 0));
            MeshVertex v4 = new MeshVertex(new Point3d(0, 4, 0));

            MeshVertex v11 = new MeshVertex(new Point3d(0, 11, 0));
            MeshVertex v22 = new MeshVertex(new Point3d(0, 22, 0));
            MeshVertex v33 = new MeshVertex(new Point3d(0, 33, 0));
            MeshVertex v44 = new MeshVertex(new Point3d(0, 44, 0));

            Mesh mesh1 = new Mesh();
            mesh1.AddFaceMesh(new MeshVertex[] { v1, v2, v3, v4 });
            mesh1.AddFaceMesh(new MeshVertex[] { v11, v22, v33, v44 });

            Mesh mesh2 = new Mesh();
            mesh2.AddFaceMesh(new MeshVertex[] { v11, v22, v33, v44 });
            mesh2.AddFaceMesh(new MeshVertex[] { v1, v2, v3, v4 });

            var h1 = mesh1.GetHashCode();
            var h2 = mesh2.GetHashCode();

            Console.WriteLine(h1);
            Console.WriteLine(h2);
            Assert.AreEqual(h1, h2, $"{h1} {h2}");
        }


        [TestMethod]
        public void MeshEdge()
        {

            MeshEdge me1 = new MeshEdge(1, 2);
            MeshEdge me2 = new MeshEdge(2, 1);
            MeshEdge me3 = null;
            MeshEdge me4 = new MeshEdge(1, 2);

            Assert.AreEqual(me1, me2, $"{me1} {me2}");
            Assert.IsTrue(me1.GetHashCode() == me2.GetHashCode());
            Assert.IsTrue(me1.GetHashCode() == me4.GetHashCode());

            Assert.IsFalse(me1.Equals(me3));
            Assert.IsFalse(me1.Equals(null));

        }


    }
}
