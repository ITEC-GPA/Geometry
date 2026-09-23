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
    public class GMeshTestPoint : GenericMeshTest
    {      
        [TestMethod]
        public void RectangularWithPointInsideEmbMeshSize()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 100, 50);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[1] { p1 }
			};

			Dictionary<GeometryBase, double> embGeomMeshSize = new Dictionary<GeometryBase, double>
			{
				{ p1, 10 }
			};

			GMesh.Generate(shapes, embeddedGeometries, embGeomMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,7);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsInsideEmbMeshSize()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 100, 50);
            Point3d p2 = new Point3d(0, 150, 50);
            Point3d p3 = new Point3d(0, 50, 50);
            Point3d p4 = new Point3d(0, 100, 25);
            Point3d p5 = new Point3d(0, 100, 75);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			Dictionary<GeometryBase, double> embGeomMeshSize = new Dictionary<GeometryBase, double>
			{
				{ p1, 10 }
			};

			GMesh.Generate(shapes, embeddedGeometries, embGeomMeshSize, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,8);
        }

        [TestMethod]
        public void RectangularWithManyPointsInsideEmbMeshSize()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 1000)), new Vector3d(0, 2000, 0));

            Point3d p1 = new Point3d(0, 1000, 500);
            Point3d p2 = new Point3d(0, 1500, 500);
            Point3d p3 = new Point3d(0, 500, 500);
            Point3d p4 = new Point3d(0, 1000, 250);
            Point3d p5 = new Point3d(0, 1000, 750);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			Dictionary<GeometryBase, double> embGeomMeshSize = new Dictionary<GeometryBase, double>
			{
				{ p1, 15 },
				{ p2, 15 },
				{ p3, 15 },
				{ p4, 15 },
				{ p5, 15 }
			};

			GMesh.Generate(shapes, embeddedGeometries, embGeomMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 15);
        }

        [TestMethod]           
        public void RectangularWithPointsInside2()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 100, 50);
            Point3d p2 = new Point3d(0, 150, 50);
            Point3d p3 = new Point3d(0, 50, 50);
            Point3d p4 = new Point3d(0, 100, 25);
            Point3d p5 = new Point3d(0, 100, 75);

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = 0.01,
				MeshScalingFactor = 100,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
                UseGlobalProgressID = true,
                RecombineOptimizeTopology = 10,
                HealShapes = true,
                Transfinite = true,
                Refine = false,
			};
			options.MeshSize = 10 * options.GeometryBaseScaleFactor;

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, 10, 10, generateMeshStatus);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsOnBorder1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 0, 10);
            Point3d p2 = new Point3d(0, 0, 20);
            Point3d p3 = new Point3d(0, 0, 40);
            Point3d p4 = new Point3d(0, 0, 50);
            Point3d p5 = new Point3d(0, 0, 80);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options,out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsOnBorder2()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 200, 20);
            Point3d p2 = new Point3d(0, 200, 40);
            Point3d p3 = new Point3d(0, 200, 60);
            Point3d p4 = new Point3d(0, 200, 80);
            Point3d p5 = new Point3d(0, 200, 50);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsOnBorder3()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 50, 100);
            Point3d p2 = new Point3d(0, 20, 100);
            Point3d p3 = new Point3d(0, 80, 100);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[3] { p1, p2, p3 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsOnBorder4()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 50, 0);
            Point3d p2 = new Point3d(0, 10, 0);
            Point3d p3 = new Point3d(0, 20, 0);
            Point3d p4 = new Point3d(0, 60, 0);
            Point3d p5 = new Point3d(0, 80, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[5] { p1, p2, p3, p4, p5 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
                               
        [TestMethod]           
        public void RectangularWithPointsOnBorder2_0()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 195, 100);
            Point3d p2 = new Point3d(0, 195, 0);
            Point3d p3 = new Point3d(0, 200, 95);
            Point3d p4 = new Point3d(0, 200, 5);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[4] { p1, p2, p3, p4 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularPointsOnBorderRandom2()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 185, 100);
            Point3d p2 = new Point3d(0, 185, 0);
            Point3d p3 = new Point3d(0, 200, 85);
            Point3d p4 = new Point3d(0, 200, 15);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[4] { p1, p2, p3, p4 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularPointsOnBorderRandom1()
        {
            Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 0, 100)), new Vector3d(0, 200, 0));

            Point3d p1 = new Point3d(0, 150, 100);
            Point3d p2 = new Point3d(0, 150, 0);
            Point3d p3 = new Point3d(0, 200, 50);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[3] { p1, p2, p3 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularIsInsideTestPoints()
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
				[s1] = new GeometryBase[] { (new Point3d(-150, 50, 0)) },
				[s2] = new GeometryBase[] { (new Point3d(50, -50, 0)) }
			};
            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithShapesOnHoleEmbPoints()
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
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(20, 80, 0.0)
            };            
            Shape s1 = new Shape(shape, new Polygon3d[1] { hole });

            Polygon3d InnerShape1 = new Polygon3d()
            {
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 20, 20),
                new Point3d(20, 20, 20)
            };
            Polygon3d InnerShape2 = new Polygon3d()
            {
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(80, 80, 20),
                new Point3d(80, 20, 20)
            };
            Polygon3d InnerShape3 = new Polygon3d()
            {
                new Point3d(25, 20, 20),
                new Point3d(75, 20, 20),
                new Point3d(55, 20, 60),
                new Point3d(35, 20, 60)
            };
            Polygon3d InnerShape3hole = new Polygon3d()
            {
                new Point3d(35, 20, 30),
                new Point3d(65, 20, 30),
                new Point3d(50, 20, 50),
                new Point3d(40, 20, 50)
            };

            Shape s2 = new Shape(InnerShape1);
            Shape s3 = new Shape(InnerShape2);
            Shape s4 = new Shape(InnerShape3, new Polygon3d[1] { InnerShape3hole }, null);

            Point3d p1 = new Point3d(20, 55, 0);
            Point3d p2 = new Point3d(80, 55, 0);
            Point3d p3 = new Point3d(55, 20, 0);
            Point3d p4 = new Point3d(55, 5, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1, s2, s3, s4 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[2] { p1, p2 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithPointsOnHole()
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
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(20, 80, 0.0)
            };

            Point3d p1 = new Point3d(20, 55, 0);
            Point3d p2 = new Point3d(80, 55, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Shape s2 = new Shape(shape, new Polygon3d[1] { hole });

            List<Shape> shapes = new List<Shape>() { s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s2] = new GeometryBase[2] { p1, p2 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithPointsOnHole2()
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
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(20, 80, 0.0)
            };

            Point3d p1 = new Point3d(20, 55, 0);
            Point3d p2 = new Point3d(80, 55, 0);
            Point3d p4 = new Point3d(80, 20, 0);
            Point3d p5 = new Point3d(80, 30, 0);
            Point3d p6 = new Point3d(80, 60, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Shape s2 = new Shape(shape, new Polygon3d[1] { hole });

            List<Shape> shapes = new List<Shape>() { s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s2] = new GeometryBase[5] { p1, p2, p4, p5, p6 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void RectangularWithPointsOnHole3()
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
                new Point3d(20, 20, 0.0),
                new Point3d(40, 20, 0.0),
                new Point3d(40, 40, 0.0),
                new Point3d(20, 40, 0.0)
            };
            Polygon3d hole2 = new Polygon3d()
            {
                new Point3d(60, 60, 0.0),
                new Point3d(60, 80, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(80, 60, 0.0)
            };
            Polygon3d hole3 = new Polygon3d()
            {
                new Point3d(20, 60, 0.0),
                new Point3d(40, 60, 0.0),
                new Point3d(40, 80, 0.0),
                new Point3d(20, 80, 0.0)
            };

            Point3d p1 = new Point3d(32, 20, 0);
            Point3d p2 = new Point3d(27, 40, 0);
            Point3d p3 = new Point3d(62, 80, 0);
            Point3d p4 = new Point3d(77, 60, 0);
            Point3d p5 = new Point3d(32, 80, 0);
            Point3d p6 = new Point3d(27, 60, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            Shape s2 = new Shape(shape, new Polygon3d[3] { hole, hole2, hole3 });

            List<Shape> shapes = new List<Shape>() { s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s2] = new GeometryBase[6] { p1, p2, p3, p4, p5, p6 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void RectangularWithShapesOnHoleEmbPoints2()
        {
            Polygon3d shape = new Polygon3d()
            {
                new Point3d(-100, -100, 0.0),
                new Point3d(200, -100, 0.0),
                new Point3d(200, 200, 0.0),
                new Point3d(-100, 200, 0.0)
            };
            Polygon3d hole = new Polygon3d()
            {
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(20, 80, 0.0)
            };
            Shape s1 = new Shape(shape, new Polygon3d[1] { hole });

            Polygon3d InnerShape1 = new Polygon3d()
            {
                new Point3d(20, 20, 0.0),
                new Point3d(80, 20, 0.0),
                new Point3d(80, 20, 50),
                new Point3d(20, 20, 50)
            };
            Polygon3d InnerShape2 = new Polygon3d()
            {
                new Point3d(80, 20, 0.0),
                new Point3d(80, 80, 0.0),
                new Point3d(80, 80, 50),
                new Point3d(80, 20, 50)
            };
            Polygon3d InnerShape3 = new Polygon3d()
            {
                new Point3d(80, 80, 0),
                new Point3d(20, 80, 0),
                new Point3d(20, 80, 50),
                new Point3d(80, 80, 50)
            };
            Polygon3d InnerShape4 = new Polygon3d()
            {
                new Point3d(20, 20, 0.0),
                new Point3d(20, 80, 0.0),
                new Point3d(20, 80, 50),
                new Point3d(20, 20, 50)
            };

            Shape s2 = new Shape(InnerShape1);
            Shape s3 = new Shape(InnerShape2);
            Shape s4 = new Shape(InnerShape3);
            Shape s5 = new Shape(InnerShape4);

            Point3d p1 = new Point3d(20, 55, 0);
            Point3d p2 = new Point3d(80, 55, 0);
            Point3d p3 = new Point3d(55, 20, 0);
            Point3d p4 = new Point3d(55, 5, 0);
            Point3d p5 = new Point3d(0, 0, 0);
            Point3d p6 = new Point3d(150, 150, 0);
            Point3d p7 = new Point3d(0, 150, 0);
            Point3d p8 = new Point3d(150, 0, 0);
            Point3d p9 = new Point3d(20, 30, 0);
            Point3d p10 = new Point3d(70, 80, 0);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1, s2, s3, s4, s5 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void TOB002Test()
        {
            Polygon3d shape = new Polygon3d()
            {
                new Point3d(-4518.6000005,19849.7359315,-0.0000000),
                new Point3d(25481.4051467,19849.7359315,-0.0000000 ),
                new Point3d(25481.4051467,-2450.2640686,-0.0000000),
                new Point3d(-4518.6000005,-2450.2640686,-0.0000000)
            };

            Shape s1 = new Shape(shape);

            Polygon3d shape2 = new Polygon3d()
            {
                new Point3d(-4518.6000005,-2450.2640686,-10000.0000000),
                new Point3d(65481.4051467,-2450.2640686,-10000.0000000),                
                new Point3d(65481.4051467,-2450.2640686,-0.0000000),
                new Point3d(-4518.6000005,-2450.2640686,-0.0000000)
            };

            Shape s2 = new Shape(shape2);

            Point3d p1 = new Point3d(1481.4051467, 8849.7359315, -0.0000000);
            Point3d p2 = new Point3d(19481.4051467, 8849.7359315, -0.0000000);
            Point3d p3 = new Point3d(25481.4051467, 8849.7359315, -0.0000000);
            Point3d p4 = new Point3d(7481.4051467, -2450.2640686, -0.0000000);
            Point3d p5 = new Point3d(1481.4051467, -2450.2640686, -0.0000000);
            Point3d p6 = new Point3d(19481.4051467, -2450.2640686, -0.0000000);
            Point3d p7 = new Point3d(1481.40514669762, -2450.26406855804, -7.58006990508875E-12);

            double meshSize = 2000;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = 0.01,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 10,
				HealShapes = true,
				Transfinite = true,
				Refine = false
			};
			options.MeshSize = meshSize * options.GeometryBaseScaleFactor;
            options.MeshScalingFactor = 1 / options.GeometryBaseScaleFactor;

            List<Shape> shapes = new List<Shape>() { s1, s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { p1, p2, p3, p4, p5, p6, p7 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, meshSize, meshSize, generateMeshStatus);
        }

        [TestMethod]
        public void Shape2DWithShapesOnHoleEmbPoints1()
        {
            Polygon2d shape = new Polygon2d()
            {
                new Point2d(-100, -100),
                new Point2d(200, -100),
                new Point2d(200, 200),
                new Point2d(-100, 200)
            };

            Polygon2d hole = new Polygon2d()
            {
                new Point2d(20, 20),
                new Point2d(80, 20),
                new Point2d(80, 80),
                new Point2d(20, 80)
            };

            Shape2d s1 = new Shape2d(shape, new Polygon2d[1] { hole });

            Point2d p1 = new Point2d(20, 55);
            Point2d p2 = new Point2d(80, 55);
            Point2d p3 = new Point2d(55, 20);
            Point2d p4 = new Point2d(55, 5);
            Point2d p5 = new Point2d(0, 0);
            Point2d p6 = new Point2d(150, 150);
            Point2d p7 = new Point2d(0, 150);
            Point2d p8 = new Point2d(150, 0);
            Point2d p9 = new Point2d(20, 30);
            Point2d p10 = new Point2d(70, 80);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void Shape2DWithShapesOnHole1()
        {
            Polygon2d shape = new Polygon2d()
            {
                new Point2d(-100, -100),
                new Point2d(200, -100),
                new Point2d(200, 200),
                new Point2d(-100, 200)
            };
            Polygon2d hole1 = new Polygon2d()
            {
                new Point2d(-40, -40),
                new Point2d(-40, 40),
                new Point2d(40, 40),
                new Point2d(40, -40)
            };
            Polygon2d hole2 = new Polygon2d()
            {
                new Point2d(-40, 60),
                new Point2d(-40, 140),
                new Point2d(40, 140),
                new Point2d(40, 60)
            };

            Shape2d s1 = new Shape2d(shape, new Polygon2d[] { hole1, hole2 });

            Point2d p1 = new Point2d(200, -15);
            Point2d p2 = new Point2d(200, -40);
            Point2d p3 = new Point2d(200, -80);
            Point2d p4 = new Point2d(200, -90);
            Point2d p5 = new Point2d(200, 15);
            Point2d p6 = new Point2d(200, 40);
            Point2d p7 = new Point2d(200, 80);
            Point2d p8 = new Point2d(200, 100);
            Point2d p9 = new Point2d(200, 0);
            Point2d p10 = new Point2d(200, 10);

            GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
               true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 5);
        }

        [TestMethod]
        public void Shape2dTest1()
        {
            Polygon2d shape = new Polygon2d()
            {
                new Point2d(-4518.6000005,19849.7359315),
                new Point2d(25481.4051467,19849.7359315 ),
                new Point2d(25481.4051467,-2450.2640686),
                new Point2d(-4518.6000005,-2450.2640686)
            };
            Shape2d s1 = new Shape2d(shape);

            Point2d p1 = new Point2d(1481.4051467, 8849.7359315);
            Point2d p2 = new Point2d(19481.4051467, 8849.7359315);
            Point2d p3 = new Point2d(25481.4051467, 8849.7359315);
            Point2d p4 = new Point2d(7481.4051467, -2450.2640686);
            Point2d p5 = new Point2d(1481.4051467, -2450.2640686);
            Point2d p6 = new Point2d(19481.4051467, -2450.2640686);
            Point2d p7 = new Point2d(1481.40514669762, -2450.26406855804);

            double meshSize = 2000;

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = 0.01,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				UseGlobalProgressID = true
			};
			options.MeshSize = meshSize * options.GeometryBaseScaleFactor;
            options.MeshScalingFactor = 1 / options.GeometryBaseScaleFactor;

            List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { p1, p2, p3, p4, p5, p6, p7 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, meshSize, meshSize, generateMeshStatus);
        }
    }
}
