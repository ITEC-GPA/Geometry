using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;

namespace Meshes.GMsh
{
    [TestClass]
    public class GMeshTestSplit : GenericMeshTest
    {        
        [TestMethod]
        [TestCategory("Fail: Not implemented Test")]
        public void RectangularMeshesWithIntersection()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(91.498,-20.682,0.000),
                new Point3d(-33.835,-20.682,0.000),
                new Point3d(-33.835,43.571,0.000),
                new Point3d(91.498,43.571,0.000)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(65.627,74.111,26.627),
                new Point3d(65.627,-51.222,26.627),
                new Point3d(29.674,-51.222,-26.627),
                new Point3d(29.674,74.111,-26.627 )
            };
            Shape s2 = new Shape(p2);
            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(28.831,29.421,26.627),
                new Point3d(28.831,-6.532,-26.627),
                new Point3d(-96.502,-6.532,-26.627),
                new Point3d(-96.502,29.421,26.627)
            };
            Shape s3 = new Shape(p3);
            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(-100, 0, 0),
                new Point3d(-100, 100, 0),
                new Point3d(0, 100, 0)
            };
            Shape s4 = new Shape(p4);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

			Line3d l2 = new Line3d(new Point3d(82.739, 36.184, 0.000), new Point3d(-15.815, -15.364, 0.000));
            // Point3d point1 = new Point3d(25.000, 37.500, 75.000);

            GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 1,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 10,
				HealShapes = true,
				Optimize = false,
				OptimizeIteration = 1,
				OptimizeAlgorithm = GMesh.GMeshGenerateOptions.MeshOptimize.Netgen,
				OptimizeNetgen = 1,
				Transfinite = true,
				MinQuality = 0.9,
				Refine = false,
				Smoothing = 10,
				MatchMeshTolerance = 1,
				AngleToleranceFacetOverlap = 1
			};


			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				//embeddedGeometries[s2] = new GeometryBase[1] { p1 };
				[s1] = new GeometryBase[1] { l2 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);            
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithLine()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(90, 90, 0), new Point3d(-90, 50, 0)) }
			    // embeddedGeometries[s2] = new GeometryBase[] { new Line3d(new Point3d(0, -90, -90), new Point3d(0, 0, 0)) };
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 10,
				HealShapes = true,
				Optimize = true,
				OptimizeIteration = 1,
				OptimizeAlgorithm = GMesh.GMeshGenerateOptions.MeshOptimize.Netgen,
				OptimizeNetgen = 1,
				Transfinite = true,
				MinQuality = 0.9,
				Refine = false,
				Smoothing = 10
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithLine2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(200, 200, 0),
                new Point3d(-200, 200, 0),
                new Point3d(-200, -200, 0),
                new Point3d(200, -200, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(90, 90, 0), new Point3d(-90, 50, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, -90, -90), new Point3d(0, 0, 0)) }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };
			Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(-50, -50, 0));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true,
				Optimize = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,3);
        }

        [TestMethod]
        [TestCategory("Fail: Unable to recover the edge")]
        public void TwoPlaneWithIntersectLineTest5()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(-80, 0, 0), new Point3d(80, -80, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true,
				Optimize = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_0()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);
            List<Shape> shapes = new List<Shape> { s1, s2 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, null, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_1()
        {
            Polygon3d p1 = new Polygon3d()
            { 
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(0, 0, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_2_0()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 10, 0), new Point3d(0, 20, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_2_1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 30, 0), new Point3d(0, 30, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_2_2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(0, 50, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_2_3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 20, 0), new Point3d(0, 10, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 6);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest2_2_4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 90, 0), new Point3d(0, 90, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 5);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_3_1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 30, 0), new Point3d(-50, 30, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_3_2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(-50, 50, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_3_3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 80, 0), new Point3d(-50, 80, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithIntersectLineTest2_3_4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, 0, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 90, 0), new Point3d(-50, 90, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 5);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest3()
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
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(0, 0, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void TwoPlaneWithIntersectLineTest1()
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
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);
            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(0, 0, 0));
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { l1 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void TwoPlaneWithHole1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Polygon3d hole1 = new Polygon3d()
            {
                new Point3d(50, 50, 0),
                new Point3d(-50, 50, 0),
                new Point3d(-50, -50, 0),
                new Point3d(50, -50, 0)
            };
            Shape s1 = new Shape(p1, new Polygon3d[] { hole1 }, null);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        public void TwoPlaneWithHole2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Polygon3d hole1 = new Polygon3d()
            {
                new Point3d(50, 50, 0),
                new Point3d(-50, 50, 0),
                new Point3d(-50, -50, 0),
                new Point3d(50, -50, 0)
            };
            Shape s1 = new Shape(p1, new Polygon3d[] { hole1 }, null);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Polygon3d hole2 = new Polygon3d()
            {
                new Point3d(0, 25, 25),
                new Point3d(0, 25, -25),
                new Point3d(0, -25, -25),
                new Point3d(0, -25, 25)
            };
            Shape s2 = new Shape(p2, new Polygon3d[] { hole2 }, null);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithManyIntersectLine2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };


			Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(-50, -50, 0));
			// Line3d l2 = new Line3d(new Point3d(60, 75, 0), new Point3d(-60, 75, 0));
			// Line3d l5 = new Line3d(new Point3d(0, -50, -50), new Point3d(0, 50, 50));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[1] { l1 },
				[s2] = new GeometryBase[0] { }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Simple,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true
			};

			double subd = 5;
			Dictionary<GeometryBase, double> lineSubdivision = new Dictionary<GeometryBase, double>
			{
				{ l1, subd }
			// lineSubdivision.Add(l2, subd);
			// lineSubdivision.Add(l5, subd);
			};


			GMesh.Generate(shapes, embeddedGeometries, lineSubdivision, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,4);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithManyIntersectLine2_0()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(-50, -50, 0));
            Line3d l2 = new Line3d(new Point3d(60, 75, 0), new Point3d(-60, 75, 0));
            Line3d l5 = new Line3d(new Point3d(0, 50, -50), new Point3d(0, 50, 50));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[2] { l1, l2 },
				[s2] = new GeometryBase[1] { l5 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Simple,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true
			};

			double subd = 5;
			Dictionary<GeometryBase, double> lineSubdivision = new Dictionary<GeometryBase, double>
			{
				{ l1, subd },
				{ l2, subd },
				{ l5, subd }
			};


			GMesh.Generate(shapes, embeddedGeometries, lineSubdivision, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,5);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithManyIntersectLine2_1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l3 = new Line3d(new Point3d(50, 50, 0), new Point3d(50, -50, 0));
            Line3d l4 = new Line3d(new Point3d(95, 15, 0), new Point3d(-95, 15, 0));
            Line3d l6 = new Line3d(new Point3d(0, -60, -60), new Point3d(0, -60, 60));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[2] { l3, l4 },
				[s2] = new GeometryBase[1] { l6 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunay,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true
			};

			double subd = 3.5;
			Dictionary<GeometryBase, double> lineSubdivision = new Dictionary<GeometryBase, double>
			{
				{ l3, subd },
				{ l4, subd },
				{ l6, subd }
			};

			GMesh.Generate(shapes, embeddedGeometries, lineSubdivision, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            // the minimum size is the subdivision of the lines, smaller than the mesh size (before, options.MeshSize: two faces of 2.4
            // near the lines were "smaller than allowed", 2.5 = 5^2 / 10)
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, subd, generateMeshStatus,5);
        }

        [TestMethod]
        [TestCategory("Fail: Null Area Test")]
        public void TwoPlaneWithManyIntersectLine2_2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Line3d l1 = new Line3d(new Point3d(50, 50, 0), new Point3d(-50, -50, 0));
            Line3d l2 = new Line3d(new Point3d(60, 75, 0), new Point3d(-60, 75, 0));
            Line3d l3 = new Line3d(new Point3d(50, 50, 0), new Point3d(50, -50, 0));
            Line3d l4 = new Line3d(new Point3d(95, 15, 0), new Point3d(-95, 15, 0));
            Line3d l5 = new Line3d(new Point3d(0, -50, -50), new Point3d(0, 50, 50));
            Line3d l6 = new Line3d(new Point3d(0, -60, -60), new Point3d(0, -60, 60));
            Line3d l7 = new Line3d(new Point3d(0, -50, -50), new Point3d(0, -50, 50));
            Line3d l8 = new Line3d(new Point3d(0, 95, 15), new Point3d(0, -95, 15));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[4] { l1, l2, l3, l4 },
				[s2] = new GeometryBase[4] { l5, l6, l7, l8 }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunay,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Simple,
				MeshSize = 5,
				UseGlobalProgressID = true,
				Transfinite = true
			};

			double subd = 5;
			Dictionary<GeometryBase, double> lineSubdivision = new Dictionary<GeometryBase, double>
			{
				{ l1, subd },
				{ l2, subd },
				{ l3, subd },
				{ l4, subd },
				{ l5, subd },
				{ l6, subd },
				{ l7, subd },
				{ l8, subd }
			};

			GMesh.Generate(shapes, embeddedGeometries, lineSubdivision, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus,5);
        }

        [TestMethod]
        public void FourPlane()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);

            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(50, 100, 100),
                new Point3d(50, 100, -100),
                new Point3d(50, -100, -100),
                new Point3d(50, -100, 100)
            };
            Shape s3 = new Shape(p3);

            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(-50, 100, 100),
                new Point3d(-50, 100, -100),
                new Point3d(-50, -100, -100),
                new Point3d(-50, -100, 100)
            };
            Shape s4 = new Shape(p4);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.BlossomFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, null, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        [TestCategory("Fail: Not implemented Test")]
        public void FourPlaneWithLineEmb()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);

            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(50, 100, 100),
                new Point3d(50, 100, -100),
                new Point3d(50, -100, -100),
                new Point3d(50, -100, 100)
            };
            Shape s3 = new Shape(p3);

            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(-50, 100, 100),
                new Point3d(-50, 100, -100),
                new Point3d(-50, -100, -100),
                new Point3d(-50, -100, 100)
            };
            Shape s4 = new Shape(p4);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(90, 90, 0), new Point3d(-90, 50, 0)) }
			};
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Simple,
				MeshSize = 10,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void SixPlane()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);

            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(50, 100, 100),
                new Point3d(50, 100, -100),
                new Point3d(50, -100, -100),
                new Point3d(50, -100, 100)
            };
            Shape s3 = new Shape(p3);

            Polygon3d p4 = new Polygon3d()
            {
                new Point3d(-50, 100, 100),
                new Point3d(-50, 100, -100),
                new Point3d(-50, -100, -100),
                new Point3d(-50, -100, 100)
            };
            Shape s4 = new Shape(p4);

            Polygon3d p5 = new Polygon3d()
            {
                new Point3d(100, 100, 50),
                new Point3d(-100, 100, 50),
                new Point3d(-100, -100, 50),
                new Point3d(100, -100, 50)
            };
            Shape s5 = new Shape(p5);

            Polygon3d p6 = new Polygon3d()
            {
                new Point3d(100, 100, -50),
                new Point3d(-100, 100, -50),
                new Point3d(-100, -100, -50),
                new Point3d(100, -100, -50)
            };
            Shape s6 = new Shape(p6);

            List<Shape> shapes = new List<Shape> { s1, s2, s3, s4, s5, s6 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.BlossomFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, null, null, options, out List <Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void TwoPlanePointsEmb()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 100, 0),
                new Point3d(-100, 100, 0),
                new Point3d(-100, -100, 0),
                new Point3d(100, -100, 0)
            };
            Shape s1 = new Shape(p1);

            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(0, 100, 100),
                new Point3d(0, 100, -100),
                new Point3d(0, -100, -100),
                new Point3d(0, -100, 100)
            };
            Shape s2 = new Shape(p2);

            List<Shape> shapes = new List<Shape> { s1, s2 };

            Point3d point1 = new Point3d(58, 58, 0);
            Point3d point2 = new Point3d(-58, -58, 0);
            Point3d point3 = new Point3d(0, 58, 58);
            Point3d point4 = new Point3d(0, -58, -58);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { point1, point2 } },
				{ s2, new GeometryBase[] { point3, point4 } }
			};

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.BlossomFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }

        [TestMethod]
        public void ThreePlaneIntersection()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0),
                new Point3d(100, 100, 0),
                new Point3d(0, 100, 0)
            };
            Shape s1 = new Shape(p1);

            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(30, 0, 0),
                new Point3d(30, 100, 0),
                new Point3d(30, 100, 100),
                new Point3d(30, 0, 100)
            };
            Shape s2 = new Shape(p2);

            Polygon3d p3 = new Polygon3d()
            {
                new Point3d(0, 30, 0),
                new Point3d(100, 30, 0),
                new Point3d(100, 30, 100),
                new Point3d(0, 30, 100)
            };
            Shape s3 = new Shape(p3);

            List<Shape> shapes = new List<Shape> { s1, s2, s3 };

            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.BlossomFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				Transfinite = true,
				HealShapes = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
            CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
        }
    }
}
