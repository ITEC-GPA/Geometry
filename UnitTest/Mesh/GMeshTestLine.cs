using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;

namespace Meshes.GMsh
{
    [TestClass]
    public class GMeshTestLine : GenericMeshTest
    {      
        [TestMethod]
        public void TwoRectangularMeshesWithLine1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 50, 100)), new Vector3d(100, 0, 0));
            Shape s2 = GetRectangular3dShape(new Line3d(new Point3d(25, 50, 100), new Point3d(25, 200, 300)), new Vector3d(100, 0, 0));

            Line3d l1 = new Line3d(new Point3d(26.05, 134.77, 213.02), new Point3d(100, 162.50, 250));
            Line3d l2 = new Line3d(new Point3d(79.10, 10.45, 20.90), new Point3d(20.88, 39.56, 79.12));
            Point3d p1 = new Point3d(100.54, 82.47, 143.29);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1, s2 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l2 },
                [s2] = new GeometryBase[2] { l1, p1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.Combine(Environment.CurrentDirectory, "RectangularMeshesWithLine.msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void TwoRectangularMeshesWithLine3()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 50, 100)), new Vector3d(100, 0, 0));
            Shape s2 = GetRectangular3dShape(new Line3d(new Point3d(25, 50, 100), new Point3d(25, 200, 300)), new Vector3d(100, 0, 0));

            Line3d l1 = new Line3d(new Point3d(26.05, 134.77, 213.02), new Point3d(100, 162.50, 250));
            Line3d l2 = new Line3d(new Point3d(79.10, 10.45, 20.90), new Point3d(20.88, 39.56, 79.12));
            Point3d p1 = new Point3d(100.54, 82.47, 143.29);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(7.5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            double mshSize = 3;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, mshSize },
                { l2, mshSize },
                { p1, mshSize }
            };

            List<Shape> shapes = new List<Shape>() { s1, s2 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l2 },
                [s2] = new GeometryBase[2] { l1, p1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.Combine(Environment.CurrentDirectory, "RectangularMeshesWithLine.msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 6);
        }

        [TestMethod]
        public void RectangularMeshesWithLine1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 20, 40), new Point3d(0, 160, 80));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void ExternalLine()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));
            Line3d l1 = new Line3d(new Point3d(0, 0, 500), new Point3d(0, 500, 500));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus _);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            //CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);        //la linea è esterna. da errore perchè ha un input ma non un output
        }

        [TestMethod]
        public void ExternalLine2()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));
            Line3d l1 = new Line3d(new Point3d(0, -100, 500), new Point3d(0, 800, 500));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus _);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            //CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);       // la linea è esterna. da errore perchè ha un input ma non un output
        }

        [TestMethod]
        public void RectangularWithTwoLines1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));


            Line3d l1 = new Line3d(new Point3d(0, 20, 40), new Point3d(0, 160, 80));
            Line3d l2 = new Line3d(new Point3d(0, 20, 80), new Point3d(0, 160, 20));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithTwoLines2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 0, 100),
                new Point3d(0, 200, 100),
                new Point3d(0, 200, 0)
            };
            Shape s1 = new Shape(p1);

            Line3d l1 = new Line3d(new Point3d(0, 180, 100), new Point3d(0, 0, 10));
            Line3d l2 = new Line3d(new Point3d(0, 10, 0), new Point3d(0, 90, 100));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,4);
        }

        [TestMethod]
        public void RectangularWithManyLines()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 180, 100), new Point3d(0, 0, 10));
            Line3d l2 = new Line3d(new Point3d(0, 10, 0), new Point3d(0, 90, 100));
            Line3d l3 = new Line3d(new Point3d(0, 100, 100), new Point3d(0, 200, 10));
            Line3d l4 = new Line3d(new Point3d(0, 200, 50), new Point3d(0, 20, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[4] { l1, l2, l3, l4, }
            };

            double subd = 3.5;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd },
                { l2, subd },
                { l3, subd },
                { l4, subd }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,8);
        }

        [TestMethod]
        public void RectangularWithTwoLineswithOnePointCoincident()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 180, 80), new Point3d(0, 0, 10));
            Line3d l2 = new Line3d(new Point3d(0, 180, 80), new Point3d(0, 90, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,4);
        }

        [TestMethod]
        public void RectangularWithLineOnEdge1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));
            List<Shape> shapes = new List<Shape>() { s1 };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Line3d l1 = new Line3d(new Point3d(0, 200, 50), new Point3d(0, 100, 20));
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                              
        [TestMethod]          
        public void RectangularWithLineOnEdge2()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 100, 100), new Point3d(0, 10, 20));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            double subd = 3.5;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,5);
        }
                              
        [TestMethod]          
        public void RectangularWithLineOnEdge2Scale()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 100, 100), new Point3d(0, 10, 20));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            double subd = 3.5;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,5);
        }
                              
        [TestMethod]          
        public void RectangularWithLineOnEdge3WithLineSubdivision()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 0, 50), new Point3d(0, 190, 90));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            double subd = 3;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,5);
        }
                              
        [TestMethod]          
        public void RectangularWithLineOnEdge3()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 0, 50), new Point3d(0, 190, 90));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                              
        [TestMethod]          
        public void RectangularWithLineOnEdge4()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 0, 50), new Point3d(0, 200, 50));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineOnEdge5()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Line3d l1 = new Line3d(new Point3d(0, 0, 50), new Point3d(0, 100, 100));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void ExagonalWithLineOnEdge1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 50, 0),
                new Point3d(200, 50, 0),
                new Point3d(250, 100, 0),
                new Point3d(200, 150, 0),
                new Point3d(100, 150, 0),
                new Point3d(50, 100, 0)
            };
            Shape s1 = new Shape(p1);

            Line3d l1 = new Line3d(new Point3d(150, 150, 0), new Point3d(150, 50, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[1] { l1 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularIsInsideTestLine()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(-200, -100, 0),
                new Point3d(0, -100, 0),
                new Point3d(0, 100, 0),
                new Point3d(-200, 100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, -100, 0),
                new Point3d(200, -100, 0),
                new Point3d(200, 100, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);
            List<Shape> shapes = new List<Shape> { s1, s2 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { new Line3d(new Point3d(-150, 50, 0), new Point3d(-50, -50, 0)) },
                [s2] = new GeometryBase[] { new Line3d(new Point3d(50, -50, 0), new Point3d(150, 100, 0)) }
            };
            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);

            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void MultipleEmbeddTest1()
        {
            Shape s1 = GetRectangularPlanarShape(100, 200, new Point3d(0, 0, 0));
            List<Shape> shapes = new List<Shape> { s1 };

            Point3d p1 = new Point3d(35, 35, 0);
            Line3d l1 = new Line3d(new Point3d(35, 150, 0), new Point3d(75, 100, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { p1, l1 } };

            GMesh.Generate(new List<Shape>() { s1 }, embedded, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus); 
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void EmbedPolygon()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(-200, -100, 0),
                new Point3d(0, -100, 0),
                new Point3d(0, 100, 0),
                new Point3d(-200, 100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, -50, 0),
                new Point3d(-25, -50, 0),
                new Point3d(-25, 50, 0),
                new Point3d(-100, 50, 0)
            };

			List<Shape> shapes = new List<Shape> { s1 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { p2 }
            };
            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);

            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void EmbedPolygon1Explode()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(-200, -100, 0),
                new Point3d(0, -100, 0),
                new Point3d(0, 100, 0),
                new Point3d(-200, 100, 0)
            };
            Shape s1 = new Shape(p1);

            Point3d point1 = new Point3d(-100, -50, 0);
            Point3d point2 = new Point3d(-25, -50, 0);
            Point3d point3 = new Point3d(-25, 50, 0);
            Point3d point4 = new Point3d(-100, 50, 0);

            Line3d line1 = new Line3d(point1, point2);
            Line3d line2 = new Line3d(point2, point3);
            Line3d line3 = new Line3d(point3, point4);
            Line3d line4 = new Line3d(point4, point1);

            List<Shape> shapes = new List<Shape> { s1 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { line1, line2, line3, line4 }
            };
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLine()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(800, 0, 0)), new Vector3d(0, 0, 1600));
            List<Shape> shapes = new List<Shape> { s1 };
            Line3d l1 = new Line3d(new Point3d(800, 0, 500), new Point3d(0, 0, 500));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1 } };

            GMesh.Generate(shapes, embedded, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineAndPolygon()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(800, 0, 0)), new Vector3d(0, 0, 1600));

            Line3d l1 = new Line3d(new Point3d(800, 0, 500), new Point3d(0, 0, 500));
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(350, 0, 450),
                new Point3d(450, 0, 450),
                new Point3d(450, 0, 550),
                new Point3d(350, 0, 550)
            };
            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

            List<Shape> shapes = new List<Shape> { s1 };
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, p1 } };

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineAndPolygonMeshRefinementOnEmbedded()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(800, 0, 0)), new Vector3d(0, 0, 1600));

            Line3d l1 = new Line3d(new Point3d(800, 0, 500), new Point3d(0, 0, 500));
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(350, 0, 450),
                new Point3d(450, 0, 450),
                new Point3d(450, 0, 550),
                new Point3d(350, 0, 550)
            };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

            List<Shape> shapes = new List<Shape> { s1 };
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, p1 } };

            double subd = 25;
            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd },
                { p1, subd }
            };

            GMesh.Generate(shapes, embedded, embMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);

            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus,5.5);
        }

        [TestMethod]
        public void RectangularWithLineScaled()
        {
            Shape s1 = GetRectangularPlanarShape(800, 1600, Point3d.Origin);

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(800, 500, 0));

            double scaleFactor = 1e-7 / 0.00001;
            double meshSize = 50;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = scaleFactor * meshSize,
				UseGlobalProgressID = true,
				OptimizeIteration = 1,
				OptimizeAlgorithm = GMesh.GMeshGenerateOptions.MeshOptimize.Netgen,
				OptimizeNetgen = 1,
				Transfinite = true,
				Refine = false,
				GeometryBaseScaleFactor = scaleFactor,
				MeshScalingFactor = 1.0 / scaleFactor
			};

			List<Shape> shapes = new List<Shape> { s1 };
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1 } };

            GMesh.Generate(shapes, embedded, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, meshSize, meshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithBorderEmbedded()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(800, 0, 0)), new Vector3d(0, 0, 1600));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape> { s1 };

            List<GeometryBase> geometryEmbedded = new List<GeometryBase>
            {
                new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 1600)),
                new Line3d(new Point3d(800, 0, 0), new Point3d(800, 0, 1600)),
                new Line3d(new Point3d(0, 0, 500), new Point3d(800, 0, 500))
            };

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = geometryEmbedded.ToArray() };
                        
            GMesh.Generate(shapes, embedded, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineOnBorder()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(800, 0, 0)), new Vector3d(0, 0, 1600));
            List<Shape> shapes = new List<Shape> { s1 };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(200, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<GeometryBase> geometryEmbedded = new List<GeometryBase>
            {
                new Line3d(new Point3d(0, 0, 150), new Point3d(0, 0, 1450)),
                new Line3d(new Point3d(800, 0, 150), new Point3d(800, 0, 1450))
            };

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = geometryEmbedded.ToArray() };

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_3_1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(50, 20, 0), new Point3d(0, 20, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, 20, 0), new Point3d(-50, 20, 0)) }
			};

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_3_2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { new Line3d(new Point3d(50, 50, 0), new Point3d(0, 50, 0)) },
                [s2] = new GeometryBase[] { new Line3d(new Point3d(0, 50, 0), new Point3d(-50, 50, 0)) }
            };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_3_3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(50, 80, 0), new Point3d(0, 80, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, 80, 0), new Point3d(-50, 80, 0)) }
			};
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_3_4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(50, 82, 0), new Point3d(0, 82, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, 82, 0), new Point3d(-50, 82, 0)) }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void FourPlaneWithCommonEdgeAndIntersectLine()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);
            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 0, 100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s3 = new Shape(p3);
            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(0, 100, -100),
                new Point3d(0, 0, -100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s4 = new Shape(p4);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(50, 20, 0), new Point3d(0, 20, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, 20, 0), new Point3d(-50, 20, 0)) }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void FourPlaneWithCommonEdgeAndIntersectLine2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);
            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 0, 100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s3 = new Shape(p3);
            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(0, 100, -100),
                new Point3d(0, 0, -100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s4 = new Shape(p4);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };


            Line3d l1 = new Line3d(new Point3d(50, 20, 0), new Point3d(0, 20, 0));
            Line3d l2 = new Line3d(new Point3d(0, 20, 0), new Point3d(-50, 20, 0));
            Line3d l3 = new Line3d(new Point3d(50, 60, 0), new Point3d(0, 60, 0));
            Line3d l4 = new Line3d(new Point3d(0, 60, 0), new Point3d(-50, 60, 0));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1, l3 },
				[s2] = new GeometryBase[] { l2, l4 }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void FourPlaneWithCommonEdgeAndIntersectLine3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0),
                new Point3d(0, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(0, -100, 0),
                new Point3d(0, 100, 0)
            };
            Shape s2 = new Shape(p2);
            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 0, 100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s3 = new Shape(p3);
            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(0, 100, -100),
                new Point3d(0, 0, -100),
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0)
            };
            Shape s4 = new Shape(p4);
            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

            Line3d l1 = new Line3d(new Point3d(50, 10, 0), new Point3d(0, 20, 0));
            Line3d l2 = new Line3d(new Point3d(0, 20, 0), new Point3d(-50, 30, 0));
            Line3d l3 = new Line3d(new Point3d(50, 40, 0), new Point3d(0, 60, 0));
            Line3d l4 = new Line3d(new Point3d(0, 60, 0), new Point3d(-50, 80, 0));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1, l3 },
				[s2] = new GeometryBase[] { l2, l4 }
			};

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void RectangularWithLineOnHole()
        {
            Polygon3d shape = new Polygon3d()
            {
                new Point3d(0.0, 0.0, 0.0),
                new Point3d(100, 0.0, 0.0),
                new Point3d(100, 100, 0.0),
                new Point3d(0, 100, 0.0)
            };

            Polygon3d hole = new Polygon3d()
            {
                new Point3d(40, 40, 0.0),
                new Point3d(60, 40, 0.0),
                new Point3d(60, 60, 0.0),
                new Point3d(40, 60, 0.0)
            };

            Shape s2 = new Shape(shape, new Polygon3d[1] { hole });
            List<Shape> shapes = new List<Shape>() { s2 };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s2] = new GeometryBase[1] { new Line3d(new Point3d(50, 10, 0), new Point3d(50, 40, 0)) }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void ManyShape2LinesEmbedded()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(-20, -20, 0),
                new Point3d(0, -20, 0),
                new Point3d(0, 20, 0),
                new Point3d(-20, 20, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, -20, 0),
                new Point3d(20, -20, 0),
                new Point3d(20, 20, 0),
                new Point3d(0, 20, 0)
            };
            Shape s2 = new Shape(p2);
            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(-20, -20, 0),
                new Point3d(-20, 20, 0),
                new Point3d(-40, 20, 0),
                new Point3d(-40, -20, 0)
            };
            Shape s3 = new Shape(p3);
            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(20, 20, 0),
                new Point3d(20, -20, 0),
                new Point3d(40, -20, 0),
                new Point3d(40, 20, 0)
            };
            Shape s4 = new Shape(p4);
            Polygon3d p5 = new Polygon3d()
            {
                new Point3d(-40, -20, 0),
                new Point3d(-40, -40, 0),
                new Point3d(40, -40, 0),
                new Point3d(40, -20, 0)
            };
            Shape s5 = new Shape(p5);
            Polygon3d p6 = new Polygon3d()
            {
                new Point3d(40, 20, 0),
                new Point3d(40, 40, 0),
                new Point3d(-40, 40, 0),
                new Point3d(-40, 20, 0)
            };
            Shape s6 = new Shape(p6);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4, s5, s6 };

            Line3d l1 = new Line3d(new Point3d(-5, -15, 0), new Point3d(-18, 15, 0));
            Line3d l2 = new Line3d(new Point3d(-15, -15, 0), new Point3d(0, 15, 0));
            Line3d l3 = new Line3d(new Point3d(-35, -15, 0), new Point3d(-21, 15, 0));
            Line3d l4 = new Line3d(new Point3d(-37, 15, 0), new Point3d(-35, -15, 0));
            Line3d l5 = new Line3d(new Point3d(-35, 39, 0), new Point3d(35, 30, 0));
            Line3d l6 = new Line3d(new Point3d(-35, 21, 0), new Point3d(35, 35, 0));

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 },
                [s3] = new GeometryBase[2] { l3, l4 },
                [s6] = new GeometryBase[2] { l5, l6 }
            };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(2);

            double subd = 1;

            Dictionary<GeometryBase, double> embMeshSize = new Dictionary<GeometryBase, double>
            {
                { l1, subd },
                { l2, subd },
                { l3, subd },
                { l4, subd },
                { l5, subd },
                { l6, subd }
            };

            GMesh.Generate(shapes, embeddedGeometries, embMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 10);
        }

        [TestMethod]
        public void RectangularWithHoleAndLineEmb()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(800, 0, 0),
                new Point3d(800, 0, 1600),
                new Point3d(0, 0, 1600)
            };
            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 0, 400),
                new Point3d(500, 0, 400),
                new Point3d(500, 0, 600),
                new Point3d(300, 0, 600)
            };
            Shape s1 = new Shape(fill, new Polygon3d[1] { hole }, null);
            List<Shape> shapes = new List<Shape> { s1 };

            Line3d l1 = new Line3d(new Point3d(0, 0, 500), new Point3d(300, 0, 500));
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1 } };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithHoleAndLinesEmb()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(800, 0, 0),
                new Point3d(800, 0, 1600),
                new Point3d(0, 0, 1600)
            };
            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 0, 400),
                new Point3d(500, 0, 400),
                new Point3d(500, 0, 600),
                new Point3d(300, 0, 600)
            };
            Shape s1 = new Shape(fill, new Polygon3d[1] { hole }, null);
            List<Shape> shapes = new List<Shape> { s1 };

            Line3d l1 = new Line3d(new Point3d(0, 0, 500), new Point3d(300, 0, 500));
            Line3d l2 = new Line3d(new Point3d(500, 0, 500), new Point3d(800, 0, 500));
            Line3d l3 = new Line3d(new Point3d(400, 0, 0), new Point3d(400, 0, 400));
            Line3d l4 = new Line3d(new Point3d(400, 0, 600), new Point3d(400, 0, 1600));
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, l2, l3, l4 } };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineOnHoleWithRefinement()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 0, 1000),
                new Point3d(0, 0, 1000)
            };
            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 0, 300),
                new Point3d(700, 0, 300),
                new Point3d(700, 0, 700),
                new Point3d(300, 0, 700)
            };
            Shape s1 = new Shape(fill, new Polygon3d[1] { hole }, null);

            Line3d l1 = new Line3d(new Point3d(300, 0, 300), new Point3d(300, 0, 700));
            Line3d l2 = new Line3d(new Point3d(300, 0, 700), new Point3d(700, 0, 700));
            Line3d l3 = new Line3d(new Point3d(700, 0, 700), new Point3d(700, 0, 300));
            Line3d l4 = new Line3d(new Point3d(700, 0, 300), new Point3d(300, 0, 300));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape> { s1 };
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, l2, l3, l4 } };
            Dictionary<GeometryBase, double> embeddedMeshSize = new Dictionary<GeometryBase, double>()
            {
                {l1, 25 },
                {l2, 25 },
                {l3, 25 },
                {l4, 25 },
            };

            GMesh.Generate(shapes, embedded, embeddedMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus, 20);
        }

        [TestMethod]
        public void RectangularWithLineOnHoleWithRefinement2()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 0, 1000),
                new Point3d(0, 0, 1000)
            };
            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 0, 300),
                new Point3d(700, 0, 300),
                new Point3d(700, 0, 700),
                new Point3d(300, 0, 700)
            };
            Shape s1 = new Shape(fill, new Polygon3d[1] { hole }, null);

            Line3d l1 = new Line3d(new Point3d(300, 0, 400), new Point3d(300, 0, 600));
            Line3d l2 = new Line3d(new Point3d(400, 0, 700), new Point3d(600, 0, 700));
            Line3d l3 = new Line3d(new Point3d(700, 0, 600), new Point3d(700, 0, 400));
            Line3d l4 = new Line3d(new Point3d(600, 0, 300), new Point3d(400, 0, 300));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape> { s1 };
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, l2, l3, l4 } };
            Dictionary<GeometryBase, double> embeddedMeshSize = new Dictionary<GeometryBase, double>()
            {
                {l1, 25},
                {l2, 25},
                {l3, 25},
                {l4, 25} 
            };

            GMesh.Generate(shapes, embedded, embeddedMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus, 20);
        }

        [TestMethod]
        public void RectangularWithRadialLine1()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 125, 0),
                new Point3d(875, 250, 0),
                new Point3d(750, 375, 0),
                new Point3d(500, 450, 0),
                new Point3d(250, 375, 0),
                new Point3d(125, 250, 0),
                new Point3d(0, 125, 0),
            };

            Shape s1 = new Shape(fill);
            List<Shape> shapes = new List<Shape>() { s1 };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Line3d l1 = new Line3d(new Point3d(125, 0, 0), new Point3d(0, 125, 0));
            Line3d l2 = new Line3d(new Point3d(250, 0, 0), new Point3d(125, 250, 0));
            Line3d l3 = new Line3d(new Point3d(375, 0, 0), new Point3d(250, 375, 0));
            Line3d l4 = new Line3d(new Point3d(500, 0, 0), new Point3d(500, 450, 0));
            Line3d l5 = new Line3d(new Point3d(625, 0, 0), new Point3d(750, 375, 0));
            Line3d l6 = new Line3d(new Point3d(750, 0, 0), new Point3d(875, 250, 0));
            Line3d l7 = new Line3d(new Point3d(875, 0, 0), new Point3d(1000, 125, 0));

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { l1, l2, l3, l4, l5, l6, l7 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineIntersectHole()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(800, 0, 0),
                new Point3d(800, 1600, 0),
                new Point3d(0, 1600, 0),
            };

            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 300, 0),
                new Point3d(500, 300, 0),
                new Point3d(500, 700, 0),
                new Point3d(300, 700, 0),
            };

            Shape s1 = new Shape(fill, new Polygon3d[] { hole }, null);
            List<Shape> shapes = new List<Shape>() { s1 };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(800, 500, 0));
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1 } };
            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineIntersectHole2()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(800, 0, 0),
                new Point3d(800, 1600, 0),
                new Point3d(0, 1600, 0),
            };

            Polygon3d hole = new Polygon3d()
            {
                new Point3d(300, 300, 0),
                new Point3d(500, 300, 0),
                new Point3d(500, 700, 0),
                new Point3d(300, 700, 0),
            };

            Shape s1 = new Shape(fill, new Polygon3d[] { hole }, null);
            List<Shape> shapes = new List<Shape>() { s1 };

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(800, 500, 0));
            Line3d l2 = new Line3d(new Point3d(0, 700, 0), new Point3d(800, 300, 0));
            Line3d l3 = new Line3d(new Point3d(0, 300, 0), new Point3d(800, 700, 0));
            Line3d l4 = new Line3d(new Point3d(400, 0, 0), new Point3d(400, 1600, 0));
            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>() { [s1] = new GeometryBase[] { l1, l2, l3, l4 } };

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineIntersectHole2_2()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(500, 0, 0),
                new Point3d(500, 1000, 0),
                new Point3d(0, 1000, 0),
            };

            Polygon3d hole1 = new Polygon3d()
            {
                new Point3d(200, 200, 0),
                new Point3d(400, 200, 0),
                new Point3d(400, 600, 0),
                new Point3d(200, 600, 0),
            };
            Polygon3d hole12 = new Polygon3d()
            {
                new Point3d(100, 700, 0),
                new Point3d(400, 700, 0),
                new Point3d(400, 900, 0),
                new Point3d(100, 900, 0),
            };

            Polygon3d fill2 = new Polygon3d()
            {
                new Point3d(500, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 1000, 0),
                new Point3d(500, 1000, 0),
            };

            Polygon3d hole2 = new Polygon3d()
            {
                new Point3d(700, 200, 0),
                new Point3d(900, 200, 0),
                new Point3d(900, 600, 0),
                new Point3d(700, 600, 0),
            };
            Polygon3d hole22 = new Polygon3d()
            {
                new Point3d(600, 700, 0),
                new Point3d(900, 700, 0),
                new Point3d(900, 900, 0),
                new Point3d(600, 900, 0),
            };

            Shape s1 = new Shape(fill, new Polygon3d[] { hole1, hole12 }, null);
            Shape s2 = new Shape(fill2, new Polygon3d[] { hole2, hole22 }, null);
            List<Shape> shapes = new List<Shape>() { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(500, 500, 0));
            Line3d l2 = new Line3d(new Point3d(0, 700, 0), new Point3d(500, 300, 0));
            Line3d l3 = new Line3d(new Point3d(0, 300, 0), new Point3d(500, 700, 0));

            Line3d l21 = new Line3d(new Point3d(500, 500, 0), new Point3d(1000, 500, 0));
            Line3d l22 = new Line3d(new Point3d(500, 700, 0), new Point3d(1000, 300, 0));
            Line3d l23 = new Line3d(new Point3d(500, 300, 0), new Point3d(1000, 700, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>
            {
                { s1, new GeometryBase[] { l1, l2, l3 } },
                { s2, new GeometryBase[] { l21, l22, l23 } }
            };

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineIntersectHole3()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(500, 0, 0),
                new Point3d(500, 1000, 0),
                new Point3d(0, 1000, 0),
            };

            Polygon3d hole1 = new Polygon3d()
            {
                new Point3d(200, 200, 0),
                new Point3d(400, 200, 0),
                new Point3d(400, 600, 0),
                new Point3d(200, 600, 0),
            };
            Polygon3d hole12 = new Polygon3d()
            {
                new Point3d(100, 700, 0),
                new Point3d(400, 700, 0),
                new Point3d(400, 900, 0),
                new Point3d(100, 900, 0),
            };

            Polygon3d fill2 = new Polygon3d()
            {
                new Point3d(500, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 1000, 0),
                new Point3d(500, 1000, 0),
            };

            Polygon3d hole2 = new Polygon3d()
            {
                new Point3d(700, 200, 0),
                new Point3d(900, 200, 0),
                new Point3d(900, 600, 0),
                new Point3d(700, 600, 0),
            };
            Polygon3d hole22 = new Polygon3d()
            {
                new Point3d(600, 700, 0),
                new Point3d(900, 700, 0),
                new Point3d(900, 900, 0),
                new Point3d(600, 900, 0),
            };

            Shape s1 = new Shape(fill, new Polygon3d[] { hole1, hole12 }, null);
            Shape s2 = new Shape(fill2, new Polygon3d[] { hole2, hole22 }, null);
            List<Shape> shapes = new List<Shape>() { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(500, 500, 0));
            Line3d l2 = new Line3d(new Point3d(0, 700, 0), new Point3d(500, 300, 0));
            Line3d l3 = new Line3d(new Point3d(0, 300, 0), new Point3d(500, 700, 0));
            Line3d l4 = new Line3d(new Point3d(0, 800, 0), new Point3d(500, 800, 0));
            Line3d l5 = new Line3d(new Point3d(0, 0, 0), new Point3d(500, 1000, 0));

            Line3d l21 = new Line3d(new Point3d(500, 500, 0), new Point3d(1000, 500, 0));
            Line3d l22 = new Line3d(new Point3d(500, 700, 0), new Point3d(1000, 300, 0));
            Line3d l23 = new Line3d(new Point3d(500, 300, 0), new Point3d(1000, 700, 0));
            Line3d l24 = new Line3d(new Point3d(500, 800, 0), new Point3d(1000, 800, 0));
            Line3d l25 = new Line3d(new Point3d(500, 1000, 0), new Point3d(1000, 0, 0));


            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25);

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>
            {
                { s1, new GeometryBase[] { l1, l2, l3, l4, l5 } },
                { s2, new GeometryBase[] { l21, l22, l23, l24, l25 } }
            };

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithLineIntersectHole3Scaled()
        {
            Polygon3d fill = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(500, 0, 0),
                new Point3d(500, 1000, 0),
                new Point3d(0, 1000, 0),
            };

            Polygon3d hole1 = new Polygon3d()
            {
                new Point3d(200, 200, 0),
                new Point3d(400, 200, 0),
                new Point3d(400, 600, 0),
                new Point3d(200, 600, 0),
            };
            Polygon3d hole12 = new Polygon3d()
            {
                new Point3d(100, 700, 0),
                new Point3d(400, 700, 0),
                new Point3d(400, 900, 0),
                new Point3d(100, 900, 0),
            };

            Polygon3d fill2 = new Polygon3d()
            {
                new Point3d(500, 0, 0),
                new Point3d(1000, 0, 0),
                new Point3d(1000, 1000, 0),
                new Point3d(500, 1000, 0),
            };

            Polygon3d hole2 = new Polygon3d()
            {
                new Point3d(700, 200, 0),
                new Point3d(900, 200, 0),
                new Point3d(900, 600, 0),
                new Point3d(700, 600, 0),
            };
            Polygon3d hole22 = new Polygon3d()
            {
                new Point3d(600, 700, 0),
                new Point3d(900, 700, 0),
                new Point3d(900, 900, 0),
                new Point3d(600, 900, 0),
            };

            Shape s1 = new Shape(fill, new Polygon3d[] { hole1, hole12 }, null);
            Shape s2 = new Shape(fill2, new Polygon3d[] { hole2, hole22 }, null);
            List<Shape> shapes = new List<Shape>() { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(0, 500, 0), new Point3d(500, 500, 0));
            Line3d l2 = new Line3d(new Point3d(0, 700, 0), new Point3d(500, 300, 0));
            Line3d l3 = new Line3d(new Point3d(0, 300, 0), new Point3d(500, 700, 0));
            Line3d l4 = new Line3d(new Point3d(0, 800, 0), new Point3d(500, 800, 0));
            Line3d l5 = new Line3d(new Point3d(0, 0, 0), new Point3d(500, 1000, 0));

            Line3d l21 = new Line3d(new Point3d(500, 500, 0), new Point3d(1000, 500, 0));
            Line3d l22 = new Line3d(new Point3d(500, 700, 0), new Point3d(1000, 300, 0));
            Line3d l23 = new Line3d(new Point3d(500, 300, 0), new Point3d(1000, 700, 0));
            Line3d l24 = new Line3d(new Point3d(500, 800, 0), new Point3d(1000, 800, 0));
            Line3d l25 = new Line3d(new Point3d(500, 1000, 0), new Point3d(1000, 0, 0));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25);

            Dictionary<Shape, GeometryBase[]> embedded = new Dictionary<Shape, GeometryBase[]>
            {
                { s1, new GeometryBase[] { l1, l2, l3, l4, l5 } },
                { s2, new GeometryBase[] { l21, l22, l23, l24, l25 } }
            };

            GMesh.Generate(shapes, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embedded, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void Shape2DTwoLinesEmb1()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(0, 1000),
                new Point2d(1000, 1000),
                new Point2d(1000, 0)
            };
            Shape2d s1 = new Shape2d(p1);

            List<Shape> shapes = new List<Shape> { s1 };

            Line2d l1 = new Line2d(new Point2d(200, 200), new Point2d(160, 800));
            Line2d l2 = new Line2d(new Point2d(800, 200), new Point2d(0, 400));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);
                        
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void Shape2DTwoLinesEmb2()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(0, 1000),
                new Point2d(1000, 1000),
                new Point2d(1000, 0)
            };
            Shape2d s1 = new Shape2d(p1);

            Line2d l1 = new Line2d(new Point2d(0, 0), new Point2d(1000, 1000));
            Line2d l2 = new Line2d(new Point2d(1000, 0), new Point2d(0, 1000));

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[2] { l1, l2 }
            };

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void Shape2DEmbedPolygon()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(-200, -100),
                new Point2d(0, -100),
                new Point2d(0, 100),
                new Point2d(-200, 100)
            };
            Shape2d s1 = new Shape2d(p1);
            Polygon2d p2 = new Polygon2d()
            {
                new Point2d(-100, -50),
                new Point2d(-25, -50),
                new Point2d(-25, 50),
                new Point2d(-100, 50)
            };

			List<Shape> shapes = new List<Shape> { s1 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
            {
                [s1] = new GeometryBase[] { p2 }
            };
            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
    }
}
