using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Meshes
{
    [TestClass]
    public class DelaunayMeshTestGeneric : GenericMeshTest
    {
        private void ExportToGmsh(Mesh mesh)
        {
            GmshNet.Gmsh.Initialize();

            int[] fillTag = new int[mesh.FacesCount * 3];
            int count = 0;

            for (int i = 0; i < mesh.FacesCount; i++)
            {
                var meshFace = mesh.Faces.ElementAt(i);
                var point1 = mesh.GetVertex(meshFace.A).Point;
                var point2 = mesh.GetVertex(meshFace.B).Point;
                var point3 = mesh.GetVertex(meshFace.C).Point;

                fillTag[count] = GmshNet.Gmsh.Model.Occ.AddPoint(point1.X, point1.Y, point1.Z);
                fillTag[count + 1] = GmshNet.Gmsh.Model.Occ.AddPoint(point2.X, point2.Y, point2.Z);
                fillTag[count + 2] = GmshNet.Gmsh.Model.Occ.AddPoint(point3.X, point3.Y, point3.Z);

                GmshNet.Gmsh.Model.Occ.AddLine(fillTag[count], fillTag[count + 1]);
                GmshNet.Gmsh.Model.Occ.AddLine(fillTag[count + 1], fillTag[count + 2]);
                GmshNet.Gmsh.Model.Occ.AddLine(fillTag[count + 2], fillTag[count]);

                count++;
                count++;
                count++;
            }

            GmshNet.Gmsh.Model.Occ.Synchronize();
            GmshNet.Gmsh.Fltk.Run();
            GmshNet.Gmsh.Finalize();
        }

        private void DelaunayCommonCheck(Shape2d shape, double[] meshSize, bool initialMesh = false, double tolerance = 0.1, bool exportToGMsh = false)
        {
            for (int i = 0; i < meshSize.Length; i++)
            {
                DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions()
                {
                    MeshSize = meshSize[i],
                    InitialMeshOnly = initialMesh
                };
                DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);
                CommonDelaunayAsserts(mesh, shape, generateMeshStatus, tolerance);

                if (exportToGMsh)
                    ExportToGmsh(mesh);
            }
        }

        [TestMethod]
        public void RectangularMeshTri1()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(120, 120),
                new Point2d(0, 90),
            };

            Shape2d s1 = new Shape2d(p1) { Tag = 1 };

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { Recombine = false };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
            Assert.IsTrue(mesh.FacesCount == 2);
            Assert.IsTrue(mesh.VerticesCount == 4);
        }

        [TestMethod]
        public void RectangularMeshTri2()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(0, 50),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri3()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(0, 100),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { Recombine = false };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
            Assert.IsTrue(mesh.FacesCount == 2);
            Assert.IsTrue(mesh.VerticesCount == 4);
        }

        [TestMethod]
        public void RectangularMeshTri4()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(50, 150),
                new Point2d(0, 100),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { Recombine = false };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
            Assert.IsTrue(mesh.FacesCount == 3);
            Assert.IsTrue(mesh.VerticesCount == 5);
        }

        [TestMethod]
        public void RectangularMeshTri5()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(50, 150),
                new Point2d(0, 100),
                new Point2d(50, 50),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { Recombine = false };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
            Assert.IsTrue(mesh.FacesCount == 4);
            Assert.IsTrue(mesh.VerticesCount == 6);
        }

        [TestMethod]
        public void RectangularMeshTri6()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 20),
                new Point2d(60, 20),
                new Point2d(60, 80),
                new Point2d(40, 80),
                new Point2d(40, 20),
                new Point2d(0, 20),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri7()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 20),
                new Point2d(60, 20),
                new Point2d(60, 80),
                new Point2d(100, 80),
                new Point2d(100, 100),
                new Point2d(0, 100),
                new Point2d(0, 80),
                new Point2d(40, 80),
                new Point2d(40, 20),
                new Point2d(0, 20),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri8()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(0, 100),
            };
            Polygon2d hole = new Polygon2d()
            {
                new Point2d(20, 20),
                new Point2d(80, 20),
                new Point2d(80, 80),
                new Point2d(20, 80),
            };

            Shape2d s1 = new Shape2d(p1, new Polygon2d[] { hole });

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 10 };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri9()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 200),
                new Point2d(0, 200),
            };
            Polygon2d hole1 = new Polygon2d()
            {
                new Point2d(20, 20),
                new Point2d(80, 20),
                new Point2d(80, 80),
                new Point2d(20, 80),
            };
            Polygon2d hole2 = new Polygon2d()
            {
                new Point2d(20, 120),
                new Point2d(80, 120),
                new Point2d(80, 180),
                new Point2d(20, 180),
            };

            Shape2d s1 = new Shape2d(p1, new Polygon2d[] { hole1, hole2 });

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri9_2()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 200),
                new Point2d(0, 200),
            };
            Polygon2d hole1 = new Polygon2d()
            {
                new Point2d(20, 20),
                new Point2d(80, 20),
                new Point2d(80, 80),
                new Point2d(20, 80),
            };
            Polygon2d hole2 = new Polygon2d()
            {
                new Point2d(20, 120),
                new Point2d(80, 120),
                new Point2d(80, 180),
                new Point2d(20, 180),
            };

            Shape2d s1 = new Shape2d(p1, new Polygon2d[] { hole1, hole2 });

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 10 };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri10()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(80, 80),
                new Point2d(0, 90),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { Recombine = false };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
            Assert.IsTrue(mesh.FacesCount == 2);
            Assert.IsTrue(mesh.VerticesCount == 4);
        }

        [TestMethod]
        public void RectangularMeshTri11()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(-100, -100),
                new Point2d(100, -100),
                new Point2d(100, 100),
                new Point2d(-100, 100),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 25 };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularMeshTri12()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(-100, -100),
                new Point2d(100, -100),
                new Point2d(100, 100),
                new Point2d(-100, 100),
            };

            Shape2d s1 = new Shape2d(p1);

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 25, Refine = true };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void CircularMeshTri1()
        {
            double diameter = 200;
            Shape2d s1 = new Shape2d(new Polygon2d(diameter));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void CircularMeshTri2()
        {
            double diameter = 400;
            Shape2d s1 = new Shape2d(new Polygon2d(diameter));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions();
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void CircularMeshTri3()
        {
            double diameter = 400;
            Shape2d s1 = new Shape2d(new Polygon2d(diameter, 64));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 50 };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void CircularMeshTri4()
        {
            double diameter = 400;
            Shape2d s1 = new Shape2d(new Polygon2d(diameter, 64));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { };
            DelaunayMesh.Generate(s1, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, s1, generateMeshStatus);
        }

        [TestMethod]
        public void TSectionMeshTri1()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-1200, 0),
                new Point2d(1200, 0),
                new Point2d(1200, 550),
                new Point2d(325, 550),
                new Point2d(325, 3100),
                new Point2d(-325, 3100),
                new Point2d(-325, 550),
                new Point2d(-1200, 550),
            }));

            //Point2d bBox = shape.Get2dBoundingBox().Size;
            //double size = Math.Min(Math.Max(bBox.X, bBox.Y) / 5.0, Math.Min(bBox.X, bBox.Y));
            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 2500, Refine = false, InitialMeshOnly = false };
            DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, shape, generateMeshStatus);
        }

        [TestMethod]
        public void TSectionMeshTri2()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-1200, 0),
                new Point2d(1200, 0),
                new Point2d(1200, 550),
                new Point2d(325, 550),
                new Point2d(325, 3100),
                new Point2d(-325, 3100),
                new Point2d(-325, 550),
                new Point2d(-1200, 550),
            }));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 500, InitialMeshOnly = true };
            DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, shape, generateMeshStatus);
        }

        [TestMethod]
        public void TSectionMeshTri3()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-1200, 0),
                new Point2d(1200, 0),
                new Point2d(1200, 550),
                new Point2d(325, 550),
                new Point2d(325, 3100),
                new Point2d(-325, 3100),
                new Point2d(-325, 550),
                new Point2d(-1200, 550),
            }));

            DelaunayCommonCheck(shape, new double[] { 5000, 2500, 1500, 1000, 500, 250 });
        }

        [TestMethod]
        public void TSectionMeshTri4()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-800, 0),
                new Point2d(800, 0),
                new Point2d(800, 550),
                new Point2d(200, 550),
                new Point2d(200, 4500),
                new Point2d(-205, 4500),
                new Point2d(-205, 550),
                new Point2d(-800, 550),
            }));

            DelaunayCommonCheck(shape, new double[] { 2500, 1500, 1000, 500 }, false, 0.1);
        }

        [TestMethod]
        public void TSectionMeshTri5()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-800, 0),
                new Point2d(800, 0),
                new Point2d(800, 550),
                new Point2d(200, 550),
                new Point2d(200, 4500),
                new Point2d(-200, 4500),
                new Point2d(-200, 550),
                new Point2d(-800, 550),
            }));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { InitialMeshOnly = true };
            DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, shape, generateMeshStatus);
        }

        [TestMethod]
        public void TSectionMeshTri6()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-800, 0),
                new Point2d(800, 0),
                new Point2d(800, 550),
                new Point2d(200, 550),
                new Point2d(200, 4500),
                new Point2d(-200, 4500),
                new Point2d(-200, 550),
                new Point2d(-800, 550),
            }));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { };
            DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, shape, generateMeshStatus);
        }

        [TestMethod]
        public void TSectionMeshTri7()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-800, 0),
                new Point2d(800, 0),
                new Point2d(800, 550),
                new Point2d(200, 550),
                new Point2d(200, 4500),
                new Point2d(-200, 4500),
                new Point2d(-200, 550),
                new Point2d(-800, 550),
            }));

            BoundingBox3d bBox = shape.GetBoundingBox();
            double size = Math.Min(bBox.Size.X, bBox.Size.Y) / 2.0;

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = size };
            DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
            CommonDelaunayAsserts(mesh, shape, generateMeshStatus);
            //ExportToGmsh(mesh);
        }

        [TestMethod]
        public void HSectionMeshTri1()
        {
            double height = 500;
            double bottomWidth = 400;
            double topWidth = 400;
            double webThickness = 12;
            double bottomThickness = 40;
            double topThickness = 40;

            Point2d[] points = new[]
            {
                new Point2d(-bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, + bottomThickness),
                new Point2d(webThickness / 2, + bottomThickness),
                new Point2d(webThickness / 2, height - topThickness),
                new Point2d(topWidth / 2, height - topThickness),
                new Point2d(topWidth / 2, height),
                new Point2d(-topWidth / 2, height),
                new Point2d(-topWidth / 2, height - topThickness),
                new Point2d(-webThickness / 2, height - topThickness),
                new Point2d(-webThickness / 2, 0.0 + bottomThickness),
                new Point2d(-bottomWidth / 2, 0.0 + bottomThickness),
            };

            Shape2d shape = new Shape2d(new Polygon2d(points));
            DelaunayCommonCheck(shape, new double[] { 500, 250, 150, 100, 50, 25 }, true, 0.1);
        }

        [TestMethod]
        public void HSectionMeshTri2()
        {
            double height = 500;
            double bottomWidth = 400;
            double topWidth = 400;
            double webThickness = 12;
            double bottomThickness = 40;
            double topThickness = 40;

            Point2d[] points = new[]
            {
                new Point2d(-bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, + bottomThickness),
                new Point2d(webThickness / 2, + bottomThickness),
                new Point2d(webThickness / 2, height - topThickness),
                new Point2d(topWidth / 2, height - topThickness),
                new Point2d(topWidth / 2, height),
                new Point2d(-topWidth / 2, height),
                new Point2d(-topWidth / 2, height - topThickness),
                new Point2d(-webThickness / 2, height - topThickness),
                new Point2d(-webThickness / 2, 0.0 + bottomThickness),
                new Point2d(-bottomWidth / 2, 0.0 + bottomThickness),
            };

            Shape2d shape = new Shape2d(new Polygon2d(points));

            DelaunayCommonCheck(shape, new double[] { 500, 250, 150, 100, 50, 25 }, false, 0.1);
        }

        [TestMethod]
        public void HSectionMeshTri3()
        {
            double height = 1000;
            double bottomWidth = 800;
            double topWidth = 600;
            double webThickness = 15;
            double bottomThickness = 40;
            double topThickness = 25;

            Point2d[] points = new[]
            {
                new Point2d(-bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, 0.0),
                new Point2d(bottomWidth / 2, + bottomThickness),
                new Point2d(webThickness / 2, + bottomThickness),
                new Point2d(webThickness / 2, height - topThickness),
                new Point2d(topWidth / 2, height - topThickness),
                new Point2d(topWidth / 2, height),
                new Point2d(-topWidth / 2, height),
                new Point2d(-topWidth / 2, height - topThickness),
                new Point2d(-webThickness / 2, height - topThickness),
                new Point2d(-webThickness / 2, 0.0 + bottomThickness),
                new Point2d(-bottomWidth / 2, 0.0 + bottomThickness),
            };

            Shape2d shape = new Shape2d(new Polygon2d(points));

            DelaunayCommonCheck(shape, new double[] { 1000, 500, 250, 150, 100 }, false, 0.1);
        }

        [TestMethod]
        public void GenericMeshTri1()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-1200, 0),
                new Point2d(1200, 0),
                new Point2d(1500, 550),
                new Point2d(600, 750),
                new Point2d(1000, 3100),
                new Point2d(-1000, 3100),
                new Point2d(-325, 2000),
                new Point2d(-1200, 1000),
            }));

            DelaunayCommonCheck(shape, new double[] { 5000, 2500, 1500, 1000, 500, 250 });
        }

        [TestMethod]
        public void GenericMeshTri2()
        {
            Shape2d shape = new Shape2d(
                new Polygon2d(new Point2d[]
                {
                    new Point2d(0, 0),
                    new Point2d(1200, 0),
                    new Point2d(1500, 2000),
                    new Point2d(-200, 1500),
                }),
                new Polygon2d[]
                {
                    new Polygon2d(new Point2d[]
                    {
                        new Point2d(300, 300),
                        new Point2d(800, 300),
                        new Point2d(800, 1000),
                        new Point2d(300, 1000),
                    })
                });

            DelaunayCommonCheck(shape, new double[] { 5000, 2500, 1500, 1000, 500, 250 });
        }

        [TestMethod]
        public void GenerateIsThreadSafe()
        {
            const int meshCount = 32;
            Mesh[] meshes = new Mesh[meshCount];

            Parallel.For(0, meshCount, i =>
            {
                double width = 100 + i;
                double height = 80 + i % 5;
                Shape2d shape = new Shape2d(new Polygon2d
                {
                    new Point2d(0, 0),
                    new Point2d(width, 0),
                    new Point2d(width, height),
                    new Point2d(0, height),
                });
                var options = new DelaunayMesh.DelaunayGenerateOptions
                {
                    MeshSize = 20,
                    InitialMeshOnly = true,
                    Refine = false,
                };

                Assert.IsTrue(DelaunayMesh.Generate(shape, options, out meshes[i], out var status));
                Assert.IsNull(status);

                double area = meshes[i].Faces.Sum(face => meshes[i].GetFaceArea(face));
                Assert.AreEqual(width * height, area, 1e-6);
            });
        }

        [TestMethod]
        public void CutMesh1()
        {
            Shape2d shape2D = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(200, 0),
                new Point2d(200, 10),
                new Point2d(105, 10),
                new Point2d(105, 190),
                new Point2d(200, 190),
                new Point2d(200, 200),
                new Point2d(0, 200),
                new Point2d(0, 190),
                new Point2d(95, 190),
                new Point2d(95, 10),
                new Point2d(0, 10),
            }));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 10, Refine = false };
            DelaunayMesh.Generate(shape2D, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus _);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Line2d line = new Line2d(new Point2d(-200 + i * 40, -200 + j * 40), new Point2d(200 + j * 40, 200 + i * 40));
                    mesh.Cut(line);
                }
            }
        }

        [TestMethod]
        public void CutMesh2()
        {
            Shape2d shape2D = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(200, 0),
                new Point2d(200, 40),
                new Point2d(105, 40),
                new Point2d(105, 160),
                new Point2d(200, 160),
                new Point2d(200, 200),
                new Point2d(0, 200),
                new Point2d(0, 160),
                new Point2d(95, 160),
                new Point2d(95, 40),
                new Point2d(0, 40),
            }));

            DelaunayMesh.DelaunayGenerateOptions options = new DelaunayMesh.DelaunayGenerateOptions() { MeshSize = 50, Refine = false };
            DelaunayMesh.Generate(shape2D, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus _);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Line2d line = new Line2d(new Point2d(-200 + i * 40, -200 + j * 40), new Point2d(200 + j * 40, 200 + i * 40));
                    mesh.Cut(line);
                }
            }
        }
    }
}
