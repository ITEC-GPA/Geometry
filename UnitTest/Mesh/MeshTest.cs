using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meshes
{
    [TestClass]
    public class MeshTest : GenericMeshTest
    {

        [TestMethod]
        public void Create()
        {
            Mesh mesh = CreateSimpleMesh1();
            Assert.IsTrue(mesh.VerticesCount == 4, "Invalid number of vertices");
            Assert.IsTrue(mesh.EdgesCount == 5, "Invalid number of edges");
            Assert.IsTrue(mesh.FacesCount == 2, "Invalid number of faces");
        }

        [TestMethod]
        public void VerticesFromEdgeMesh()
        {
            Mesh mesh = CreateSimpleMesh2();
            MeshVertex[] vertices = mesh.GetEdgeVertices(mesh.GetEdge(1));

            Assert.IsTrue(vertices.Count() == 2, "Wrong numer of vertices");
        }

        [TestMethod]
        public void VerticesFromFaceMesh()
        {
            Mesh mesh = CreateSimpleMesh2();
            MeshVertex[] vertices = mesh.GetFaceVertices(mesh.GetFace(1));

            Assert.IsTrue(vertices.Count() == 4, "Wrong numer of vertices");
        }

        [TestMethod]
        public void EdgesFromFaceMesh()
        {
            Mesh mesh = CreateSimpleMesh2();
            MeshEdge[] edges = mesh.GetFaceEdges(mesh.GetFace(1));

            Assert.IsTrue(edges.Count() == 4, $"Wrong numer of edges: {edges.Count()}");
        }

        [TestMethod]
        public void CloneMesh1()
        {
            Mesh mesh = CreateSimpleMesh1();

            Mesh clonedMesh = (Mesh)mesh.Clone();

            for (int i = 0; i < mesh.FacesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetFace(i).Id == mesh.GetFace(i).Id, clonedMesh.GetFace(i).Id.ToString());
            }
            for (int i = 0; i < mesh.VerticesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetVertex(i).Id == mesh.GetVertex(i).Id, clonedMesh.GetVertex(i).Id.ToString());
            }
            for (int i = 0; i < mesh.EdgesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetEdge(i).Id == mesh.GetEdge(i).Id, clonedMesh.GetEdge(i).Id.ToString());
            }
        }

        [TestMethod]
        public void CloneMesh2()
        {
            Mesh mesh = CreateSimpleMesh1();

            Mesh clonedMesh = (Mesh)mesh.Clone(false);

            for (int i = 0; i < mesh.FacesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetFace(i).Id == mesh.GetFace(i).Id, clonedMesh.GetFace(i).Id.ToString());
            }
            for (int i = 0; i < mesh.VerticesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetVertex(i).Id == mesh.GetVertex(i).Id, clonedMesh.GetVertex(i).Id.ToString());
            }
            for (int i = 0; i < mesh.EdgesCount; i++)
            {
                Assert.IsTrue(clonedMesh.GetEdge(i).Id == mesh.GetEdge(i).Id, clonedMesh.GetEdge(i).Id.ToString());
            }
        }

        [TestMethod]
        public void CloneMesh3()
        {
            Mesh mesh = CreateSimpleMesh1();

            Mesh clonedMesh = (Mesh)mesh.Clone(false);

            Assert.IsTrue(mesh.VerticesCount == clonedMesh.VerticesCount);

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Assert.IsTrue(mesh.Vertices.GetElementByIndex(i) == clonedMesh.Vertices.GetElementByIndex(i));
            }

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Assert.IsTrue(mesh.Faces.GetElementByIndex(i) == clonedMesh.Faces.GetElementByIndex(i));
            }

            for (int i = 0; i < mesh.Volumes.Count; i++)
            {
                Assert.IsTrue(mesh.Volumes.GetElementByIndex(i) == clonedMesh.Volumes.GetElementByIndex(i));
            }
        }

        [TestMethod]
        public void MeshVolume1()
        {
            MeshVolume volume = new MeshVolume(1, 2, 3, 4, 5, 6);
            Assert.IsTrue(volume.IsTriangularPrism);
            Assert.IsFalse(volume.IsQuadrangularPrism);

            MeshVolume volume2 = new MeshVolume(1, 2, 3, 4, 5, 6, -1, -1);
            Assert.IsTrue(volume2.IsTriangularPrism);
            Assert.IsFalse(volume2.IsQuadrangularPrism);

            MeshVolume volume3 = new MeshVolume(1, 2, 3, 4, 5, 6, 1, 1);
            Assert.IsTrue(volume3.IsQuadrangularPrism);
            Assert.IsFalse(volume3.IsTriangularPrism);

            try
            {
                MeshVolume volume4 = new MeshVolume(1, 2, -3, 4, 5, 6, 1, 1);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void MeshVolume2()
        {
            int meshCount1 = 2;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));

            List<GeometryBase> lines = new List<GeometryBase>();
            foreach (var edge in mesh1.Edges)
            {
                var line = new Line3d(mesh1.GetVertex(edge.A).Point, mesh1.GetVertex(edge.B).Point);
                lines.Add(line);
            }

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), new List<Mesh>() { mesh1 });
            GeometryExport.ExportToGeoFormat(base.GetFilePathInOutputFolder(base.GetTestName(), "geo"), lines);

            Assert.IsTrue(mesh1.Vertices.Count == 27, mesh1.Vertices.Count.ToString());
            Assert.IsTrue(mesh1.Volumes.Count() == meshCount1 * meshCount1 * meshCount1);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void ExtrudeMesh1()
        {
            Mesh mesh1 = CreateSimpleMesh3(2, 1, 10, 10, new Point2d(0, 0));

            Mesh volume1 = mesh1.Extrude(10);

            Assert.IsTrue(volume1.VerticesCount == 12);
            Assert.IsTrue(volume1.FacesCount == 0);
            Assert.IsTrue(volume1.EdgesCount == 20);
            Assert.IsTrue(volume1.VolumesCount == 2);

            MeshExport.ExportToMshFormatv2(GetFilePathInOutputFolder("export", "msh"), new List<Mesh> { mesh1 });
        }

        [TestMethod]
        public void JoinMesh1()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleMesh3(meshCount1, meshCount1, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleMesh3(meshCount2, meshCount2, 10, 10, new Point2d(20, 0));

            int mesh1Vertices = mesh1.VerticesCount;
            int mesh2Vertices = mesh2.VerticesCount;


            Assert.IsTrue(mesh1.EdgesCount == 12);
            Assert.IsTrue(mesh2.EdgesCount == 24);

            var edgeMap = mesh1.Edges.GetElementHashMap();
            foreach (var edge in edgeMap)
            {
                if (edge.Value.Count > 1)
                {
                    foreach (var index in edge.Value)
                    {
                        Console.WriteLine($"{mesh1.Edges.GetElementByIndex(index).A} {mesh1.Edges.GetElementByIndex(index).B}");
                    }
                }
            }

            mesh1.JoinMesh(mesh2);

            List<GeometryBase> lines = new List<GeometryBase>();
            foreach (var edge in mesh1.Edges)
            {
                var line = new Line3d(mesh1.GetVertex(edge.A).Point, mesh1.GetVertex(edge.B).Point);
                lines.Add(line);
            }


            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), new List<Mesh>() { mesh1 });
            GeometryExport.ExportToGeoFormat(base.GetFilePathInOutputFolder(base.GetTestName(), "geo"), lines);

            Assert.IsTrue(mesh1.FacesCount == (meshCount1 * meshCount1 + meshCount2 * meshCount2));
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == 22 - 1);
            Assert.IsTrue(mesh1.VerticesCount == ((meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) - 3), mesh1.VerticesCount.ToString());
            Assert.IsTrue(mesh1.FacesCount == 13);
            Assert.IsTrue(mesh1.EdgesCount == 34, mesh1.EdgesCount.ToString());
        }

        [TestMethod]
        public void JoinMesh2()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleMesh3(meshCount1, meshCount1, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleMesh3(meshCount2, meshCount2, 10, 10, new Point2d(100, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            Assert.IsTrue(mesh1.Faces.Count() == (meshCount1 * meshCount1 + meshCount2 * meshCount2));
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == 25 - 1);
            Assert.IsTrue(mesh1.Vertices.Count() == ((meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1)), mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 13);
            Assert.IsTrue(mesh1.Edges.Count() == 36, mesh1.Edges.Count().ToString());       // marco aveva messo 41, dovrebbe essere corretto 36

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh3()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleMesh3(meshCount1, meshCount1, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleMesh3(meshCount2, meshCount2, 10, 10, new Point2d(0, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            Assert.IsTrue(mesh1.Faces.Count() == (meshCount2 * meshCount2));
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == 16 - 1);
            Assert.IsTrue(mesh1.Vertices.Count() == ((meshCount2 + 1) * (meshCount2 + 1)), mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 9);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh4()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(20, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2));
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == 82 - 1);
            Assert.IsTrue(mesh1.Vertices.Count() == ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - 9), mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh5()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(0, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == meshCount2 * meshCount2 * meshCount2);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == 64 - 1);
            Assert.IsTrue(mesh1.Vertices.Count() == ((meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1)), mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void JoinMesh6()
        {
            int meshCount1 = 1;
            int meshCount2 = 1;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(10, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            int commonVertices = (meshCount1 + 1) * (meshCount1 + 1);
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh7()
        {
            int meshCount1 = 3;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(30, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            int commonVertices = (meshCount1 + 1) * (meshCount1 + 1);
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh8()
        {
            int meshCount1 = 5;
            int meshCount2 = 10;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(50, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);


            List<GeometryBase> lines = new List<GeometryBase>();
            foreach (var edge in mesh1.Edges)
            {
                var line = new Line3d(mesh1.GetVertex(edge.A).Point, mesh1.GetVertex(edge.B).Point);
                lines.Add(line);
            }


            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), new List<Mesh>() { mesh1 });
            GeometryExport.ExportToGeoFormat(base.GetFilePathInOutputFolder(base.GetTestName(), "geo"), lines);

            int commonVertices = (meshCount1 + 1) * (meshCount1 + 1);
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

        }

        [TestMethod]
        public void JoinMesh17()
        {
            int meshCount1 = 2;
            int meshCount2 = 3;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(20, 0));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);


            List<GeometryBase> lines = new List<GeometryBase>();
            foreach (var edge in mesh1.Edges)
            {
                var line = new Line3d(mesh1.GetVertex(edge.A).Point, mesh1.GetVertex(edge.B).Point);
                lines.Add(line);
            }


            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), new List<Mesh>() { mesh1 });
            GeometryExport.ExportToGeoFormat(base.GetFilePathInOutputFolder(base.GetTestName(), "geo"), lines);

            int commonVertices = (meshCount1 + 1) * (meshCount1 + 1);
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

        }


        [TestMethod]
        public void JoinMesh9()
        {
            int meshCount1 = 2;
            int meshCount2 = 2;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point2d(10, 10));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            int commonVertices = 12;
            int commonVolumes = 2;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void JoinMesh10()
        {
            int meshCount1 = 2;
            int meshCount2 = 2;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point3d(10, 10, 10));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);

            int commonVertices = 8;
            int commonVolumes = 1;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void JoinMesh11()
        {
            int meshCount1 = 2;
            int meshCount2 = 2;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point3d(10, 10, 20));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);

            int commonVertices = 4;
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void JoinMesh12()
        {
            int meshCount1 = 5;
            int meshCount2 = 5;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point3d(20, 20, 20));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);

            int commonVertices = 64;
            int commonVolumes = 27;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void JoinMesh13()
        {
            int meshCount1 = 4;
            int meshCount2 = 4;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));
            Mesh mesh2 = CreateSimpleVolumeMesh1(meshCount2, meshCount2, meshCount2, 10, 10, 10, new Point3d(40, 20, 5));

            int mesh1Vertices = mesh1.Vertices.Count();
            int mesh2Vertices = mesh2.Vertices.Count();

            mesh1.JoinMesh(mesh2);

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);

            int commonVertices = 0;
            int commonVolumes = 0;
            int totalVolume = (meshCount1 * meshCount1 * meshCount1 + meshCount2 * meshCount2 * meshCount2) - (commonVolumes);
            int totalVertices = ((meshCount1 + 1) * (meshCount1 + 1) * (meshCount1 + 1) + (meshCount2 + 1) * (meshCount2 + 1) * (meshCount2 + 1) - commonVertices);
            Assert.IsTrue(mesh1.Faces.Count() == 0);
            Assert.IsTrue(mesh1.Volumes.Count() == totalVolume);
            Assert.IsTrue(mesh1.Vertices.Select(i => i.Id).Max() == totalVertices - 1, mesh1.Vertices.Select(i => i.Id).Max().ToString());
            Assert.IsTrue(mesh1.Vertices.Count() == totalVertices, mesh1.Vertices.Count().ToString());
            Assert.IsTrue(mesh1.Faces.Count() == 0);
        }

        [TestMethod]
        public void JoinMesh14()
        {
            Mesh mesh1 = CreateSimpleMesh3(4, 4, 10, 10, new Point2d(0, 0));

            Mesh mesh2 = new Mesh();

            double inc = 1.0 / 3.0;
            double total = 0;
            for (int i = 0; i < 50; i++)
            {
                Mesh cloned = (Mesh)mesh1.Clone(true);

                cloned.Move(0, 0, total);
                total += inc;

                Mesh volume1 = cloned.Extrude(inc);
                mesh2.JoinMesh(volume1);
            }

            Assert.IsTrue(mesh2.FacesCount == 0);
            Assert.IsTrue(mesh2.VolumesCount == mesh1.FacesCount * 50);

            MeshExport.ExportToMshFormatv2(GetFilePathInOutputFolder("export", "msh"), new List<Mesh> { mesh2 });
        }

        [TestMethod]
        public void JoinMesh15()
        {
            Shape s1 = GetRectangularShape(100, 100);

            GMesh.GMeshGenerateOptions op = new GMesh.GMeshGenerateOptions
            {
                MeshSize = 10
            };

            GMesh.Generate(new List<Shape> { s1 }, op, out List<Mesh> mesh, out _);

            Mesh mesh2 = mesh.First().Extrude(50);
            mesh.First().Move(0, 0, 50);
            Mesh mesh3 = mesh.First().Extrude(50);

            Mesh meshjoin = new Mesh();
            meshjoin.JoinMesh(mesh2);

            MeshExport.ExportToMshFormatv2(GetFilePathInOutputFolder(GetTestName(), "Msh"), new List<Mesh> { meshjoin });

            Assert.IsTrue(meshjoin.VolumesCount == mesh.First().FacesCount * 1);
        }

        [TestMethod]
        public void JoinMesh16()
        {
            Mesh mesh = new Mesh();
            mesh.AddFaceMesh(new MeshVertex[] {new MeshVertex(new Point3d(0, 0, 0)), new MeshVertex(new Point3d(1, 0, 0)),
                new MeshVertex(new Point3d(1, 1, 0)), new MeshVertex(new Point3d(0, 1, 0)) });

            Mesh mesh2 = new Mesh();
            mesh2.AddFaceMesh(new MeshVertex[] { new MeshVertex(new Point3d(1, 0, 0)), new MeshVertex(new Point3d(2, 0, 0)),
                new MeshVertex(new Point3d(2, 1, 0)), new MeshVertex(new Point3d(1, 1, 0)) });

            mesh.JoinMesh(mesh2, out Dictionary<int, int> vertexIdMap, out _, out _);

            Assert.IsTrue(vertexIdMap[0] == 1);
            Assert.IsTrue(vertexIdMap[1] == 4);
            Assert.IsTrue(vertexIdMap[2] == 5);
            Assert.IsTrue(vertexIdMap[3] == 2);
        }

        [TestMethod]
        public void Area1()
        {
            Mesh mesh1 = CreateSimpleMesh3(2, 3, 10, 20, Point2d.Origin);

            foreach (var face in mesh1.Faces)
            {
                Assert.AreEqual(mesh1.GetFaceArea(face), 10 * 20, 0.00001, $"Area:{mesh1.GetFaceArea(face)}");
            }
        }

        [TestMethod]
        public void Area2()
        {
            Mesh mesh1 = CreateSimpleMesh3(1, 1, 10, 20, Point2d.Origin);

            foreach (var face in mesh1.Faces)
            {
                Assert.AreEqual(mesh1.GetFaceArea(face), 10 * 20, 0.00001, $"Area:{mesh1.GetFaceArea(face)}");
            }
        }

        [TestMethod]
        public void EdgeLenght1()
        {
            Mesh mesh1 = CreateSimpleMesh3(2, 3, 10, 20, Point2d.Origin);

            foreach (var edge in mesh1.Edges)
            {
                Assert.IsTrue(mesh1.GetEdgeLength(edge) == 10 || mesh1.GetEdgeLength(edge) == 20, mesh1.GetEdgeLength(edge).ToString());
            }
        }

        [TestMethod]
        public void FaceExists()
        {
            MeshFace face1 = new MeshFace(1, 2, 3, 4);
            MeshFace face2 = new MeshFace(2, 3, 4, 1);

            MeshFace face3 = new MeshFace(1, 4, 3);
            MeshFace face4 = new MeshFace(4, 3, 4, 5);

            Assert.IsTrue(face1.IsMatch(face2.GetNodes()));
            Assert.IsTrue(face1.IsMatch(face3.GetNodes()));

            Assert.IsFalse(face1.IsMatch(face4.GetNodes()));

            MeshFace face5 = new MeshFace(3, 4, 1);
            MeshFace face6 = new MeshFace(1, 2, 3, 4);
            Assert.IsTrue(face5.IsMatch(face6.GetNodes()));

            MeshFace face7 = new MeshFace(1, 4, 3, 2);
            MeshFace face8 = new MeshFace(1, 2, 3, 4);
            Assert.IsTrue(face7.IsMatch(face8.GetNodes()));
        }

        [TestMethod]
        public void FaceExtrude1()
        {
            Mesh mesh = CreateSimpleMesh3(3, 3, 10, 20, Point2d.Origin);

            Mesh volumeMesh = mesh.Extrude(10);


            Assert.IsTrue(mesh.FacesCount == volumeMesh.VolumesCount);

            Assert.IsTrue(mesh.GetFace(1).A == volumeMesh.GetVolume(1).A);

            Assert.IsTrue(mesh.GetFace(1).IsQuad == volumeMesh.GetVolume(1).IsQuadrangularPrism);
            Assert.IsTrue(mesh.GetFace(8).IsQuad == volumeMesh.GetVolume(8).IsQuadrangularPrism);
        }

        [TestMethod]
        public void FaceExtrude2()
        {
            Mesh mesh = CreateSimpleMesh3(2, 2, 10, 20, Point2d.Origin);

            Mesh volumeMesh = mesh.ExtrudeFaces(new Vector3d(0, 0, 10));

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var v1 = mesh.Vertices.GetElementByIndex(i);

                var v2 = mesh.Vertices.GetElementByIndex(i);
                var v3 = mesh.Vertices.GetElementById(v1.Id);

                Assert.IsTrue(v1.Point == v2.Point);
                Assert.IsTrue(v1.Point == v3.Point);
            }
        }

        [TestMethod]
        public void ExportTest1()
        {
            int meshCount1 = 1;

            Mesh mesh1 = CreateSimpleVolumeMesh1(meshCount1, meshCount1, meshCount1, 10, 10, 10, new Point2d(0, 0));

            List<Mesh> shapes = new List<Mesh>() { mesh1 };
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), shapes);
        }

        [TestMethod]
        public void Refine()
        {
            Mesh mesh = CreateSimpleMesh3(1, 1, 10, 20, Point2d.Origin);
            int faceNumber = mesh.FacesCount;

            mesh.Refine();
            int faceNumber2 = mesh.FacesCount;

            Assert.IsTrue(faceNumber2 == faceNumber * 4);
        }

        [TestMethod]
        public void Refine2()
        {
            Mesh mesh = CreateSimpleMesh3(2, 2, 10, 20, Point2d.Origin);
            int faceNumber = mesh.FacesCount;

            mesh.Refine();
            int faceNumber2 = mesh.FacesCount;

            Assert.IsTrue(faceNumber2 == faceNumber * 4);
        }

        [TestMethod]
        public void CutMesh1()
        {
            Mesh mesh = CreateSimpleMesh3(10, 10, 1, 1, Point2d.Origin);
            Line2d line = new Line2d(new Point2d(-1, 1), new Point2d(11, 5));

            mesh.Cut(line);
        }

        [TestMethod]
        public void CutMesh2()
        {
            Mesh mesh = CreateSimpleMesh3(10, 10, 1, 1, Point2d.Origin);
            Line2d line = new Line2d(new Point2d(-5, 5), new Point2d(15, 5));

            mesh.Cut(line);
        }
    }
}