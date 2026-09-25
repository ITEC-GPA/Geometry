using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Meshes.GMsh
{
	[TestClass]
	public class GMeshTestGeneric : GenericMeshTest
	{
		[TestMethod]
		public void RectangularMeshesTri()
		{
			Shape s1 = GetRectangularPlanarShape(100, 200, new Point2d(0, 0));
			Shape s2 = GetRectangularPlanarShape(100, 200, new Point2d(500, 500));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { s1, s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(new List<Shape>() { s1, s2 }, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularMeshQuad1()
		{
			Shape s1 = GetRectangularPlanarShape(100, 200, new Point2d(0, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularMeshesQuad1()
		{
			Shape s1 = GetRectangularPlanarShape(100, 200, new Point2d(0, 0));
			Shape s2 = GetRectangularPlanarShape(100, 200, new Point2d(500, 500));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { s1, s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularMeshesQuad2()
		{
			Shape s1 = GetRectangular3dShape(new Line3d(new Point3d(0, 0, 0), new Point3d(0, 50, 100)), new Vector3d(100, 0, 0));
			Shape s2 = GetRectangular3dShape(new Line3d(new Point3d(0, 50, 100), new Point3d(0, 200, 300)), new Vector3d(100, 0, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { new Shape(s1), new Shape(s2) };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularMeshesQuad3()
		{
			Shape s1 = GetRectangularPlanarShape(100, 200, new Point2d(0, 0));

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 25,
				HealShapes = true,
				Transfinite = true
			};

			List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularMeshesQuad7()
		{
			Shape s1 = GetRectangularPlanarShape(100, 200, new Point2d(0, 0));

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 25,
				HealShapes = true,
				Refine = true,
				Transfinite = true
			};

			List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize / 2.0, options.MeshSize / 2.0, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularWithHole()
		{
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(575.03, -135.79, 0.00),
				new Point3d(621.63, -135.79, 0.00),
				new Point3d(621.63, -60.88, 0.00),
				new Point3d(575.03, -60.88, 0.00)
			};

			Shape baseShape = GetRectangularPlanarShape(450, 400, new Point3d(279.02, -203.32, 0.00), new Polygon3d[1] { hole });

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			List<Shape> shapes = new List<Shape>() { baseShape };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularWithHole2()
		{
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(50, 50, 0.00),
				new Point3d(50, 400, 0.00),
				new Point3d(200, 400, 0.00),
				new Point3d(200, 50, 0.00)
			};

			Shape baseShape = GetRectangularPlanarShape(450, 400, new Point3d(0, 0, 0.00), new Polygon3d[1] { hole });

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { baseShape };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularWithSmallHole()
		{
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(400, 400, 0),
				new Point3d(600, 400, 0),
				new Point3d(600, 600, 0),
				new Point3d(400, 600, 0)
			};

			Shape baseShape = GetRectangularPlanarShape(1000, 1000, new Point3d(0, 0, 0), new Polygon3d[1] { hole });
			Shape[] shapes = new Shape[] { new Shape(baseShape.Fill, baseShape.Holes), new Shape(hole) };
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 250,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true,
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 8);
		}

		[TestMethod]
		public void RectangularWithManyHoles()
		{
			Polygon3d s = new Polygon3d()
			{
				new Point3d(0, 0, 0.00),
				new Point3d(100, 0, 0.00),
				new Point3d(100, 100, 0.00),
				new Point3d(0, 100, 0.00)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(10, 10, 0.00),
				new Point3d(20, 10, 0.00),
				new Point3d(20, 20, 0.00),
				new Point3d(10, 20, 0.00)
			};
			Polygon3d h2 = new Polygon3d()
			{
				new Point3d(50, 10, 0.00),
				new Point3d(60, 10, 0.00),
				new Point3d(60, 20, 0.00),
				new Point3d(50, 20, 0.00)
			};
			Polygon3d h3 = new Polygon3d()
			{
				new Point3d(10, 60, 0.00),
				new Point3d(20, 60, 0.00),
				new Point3d(20, 70, 0.00),
				new Point3d(10, 70, 0.00)
			};
			Polygon3d h4 = new Polygon3d()
			{
				new Point3d(50, 60, 0.00),
				new Point3d(60, 60, 0.00),
				new Point3d(60, 70, 0.00),
				new Point3d(50, 70, 0.00)
			};

			Shape shape = new Shape(s, new Polygon3d[] { h1, h2, h3, h4 }, null);

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			List<Shape> shapes = new List<Shape>() { shape };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularWithShapesOnHole()
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

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			List<Shape> shapes = new List<Shape>() { s1, s2, s3, s4 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void RectangularWithLineOnHole2()
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

			Point3d p1 = new Point3d(45, 40, 0);
			Point3d p2 = new Point3d(55, 40, 0);
			Line3d l1 = new Line3d(p1, p2);

			Point3d p3 = new Point3d(60, 40, 0);
			Point3d p4 = new Point3d(80, 60, 0);
			Line3d l2 = new Line3d(p3, p4);

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 5,
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
				Smoothing = 10
			};

			Shape s2 = new Shape(shape, new Polygon3d[1] { hole });

			List<Shape> shapes = new List<Shape>() { s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s2] = new GeometryBase[2] { l1, l2 }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void ComplexShapes1()
		{
			Polygon3d hole1 = new Polygon3d()
			{
				new Point3d(575.03, -135.79, 0.00),
				new Point3d(621.63, -135.79, 0.00),
				new Point3d(621.63, -60.88, 0.00),
				new Point3d(575.03, -60.88, 0.00)
			};

			Polygon3d hole2 = new Polygon3d()
			{
				new Point3d(410, -156, 0.00),
				new Point3d(480, -156, 0.00),
				new Point3d(480, -75, 0.00),
				new Point3d(410, -75, 0.00 )
			};

			Polygon3d hole3 = new Polygon3d()
			{
				new Point3d(428, -128, 0.00),
				new Point3d(460, -128, 0.00),
				new Point3d(460, -105, 0.00),
				new Point3d(428, -105, 0.00 )
			};

			Shape baseShape = GetRectangularPlanarShape(450, 400, new Point3d(280, -200, 0.00), new Polygon3d[2] { hole1, hole2 });

			Shape s2 = new Shape(hole2, new Polygon3d[1] { hole3 });

			Line3d line = new Line3d(new Point3d(330, -120, 0.00), new Point3d(335, 145, 0.00));

			Point3d p1 = new Point3d(375, 0, 0.00);
			Point3d p2 = new Point3d(405, 0, 0.00);
			Point3d p3 = new Point3d(375, 10, 0.00);
			Point3d p4 = new Point3d(405, 10, 0.00);
			Point3d p5 = new Point3d(430, 100, 0.00);
			Point3d p6 = new Point3d(610, -60.88, 0);
			Point3d p7 = new Point3d(600, -60.88, 0);
			Point3d p8 = new Point3d(580, -60.88, 0);

			Shape w1 = GetRectangular3dShape(new Line3d(new Point3d(543.17, 82.92, 0.00), new Point3d(543.17, 82.92, 100.38)), new Vector3d(71, 0, 0));
			Shape w2 = GetRectangular3dShape(new Line3d(new Point3d(543.17, 82.92, -100), new Point3d(543.17, 82.92, 100.38)), new Vector3d(0, -100, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.QuasiStructuredQuad);

			List<Shape> shapes = new List<Shape>() { baseShape, s2, w1, w2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[baseShape] = new GeometryBase[] { line, p1, p2, p3, p4, p5, p6, p7, p8 }
			};

			Dictionary<GeometryBase, double> embGeomMeshSize = new Dictionary<GeometryBase, double>
			{
				{ p1, 5 },
				{ p2, 5 },
				{ p3, 5 },
				{ p4, 5 }
			};

			GMesh.Generate(shapes, embeddedGeometries, embGeomMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			// The embedded points request size 5, so validate against the actual local minimum.
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, embGeomMeshSize.Values.Min(), generateMeshStatus, 8);
		}

		[TestMethod]
		public void ComplexShapes2()
		{
			Polygon3d poly1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Shape s1 = new Shape(poly1);
			Polygon3d poly2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(200, 0, 0),
				new Point3d(200, 100, 0),
				new Point3d(100, 100, 0)
			};
			Shape s2 = new Shape(poly2);
			Polygon3d poly3 = new Polygon3d()
			{
				new Point3d(200, 0, 0),
				new Point3d(300, 0, 0),
				new Point3d(300, 100, 0),
				new Point3d(200, 100, 0)
			};
			Shape s3 = new Shape(poly3);
			Polygon3d poly4 = new Polygon3d()
			{
				new Point3d(0, 100, 0),
				new Point3d(100, 100, 0),
				new Point3d(100, 200, 0),
				new Point3d(0, 200, 0)
			};
			Shape s4 = new Shape(poly4);
			Polygon3d poly5 = new Polygon3d()
			{
				new Point3d(100, 100, 0),
				new Point3d(200, 100, 0),
				new Point3d(200, 200, 0),
				new Point3d(100, 200, 0)
			};
			Shape s5 = new Shape(poly5);
			Polygon3d poly6 = new Polygon3d()
			{
				new Point3d(200, 100, 0),
				new Point3d(300, 100, 0),
				new Point3d(300, 200, 0),
				new Point3d(200, 200, 0)
			};
			Shape s6 = new Shape(poly6);

			List<Shape> shapes = new List<Shape>() { s1, s2, s3, s4, s5, s6 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, null, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void ComplexShapes3()
		{
			Polygon3d poly1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Shape s1 = new Shape(poly1);
			Polygon3d poly2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(200, 0, 0),
				new Point3d(200, 100, 0),
				new Point3d(100, 100, 0)
			};
			Shape s2 = new Shape(poly2);

			List<Shape> shapes = new List<Shape>() { s1, s2 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, null, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void ComplexShapes4()
		{
			Polygon3d poly1 = new Polygon3d()
			{
				new Point3d(-131.833399321, -24.163781259, 33.614690782),
				new Point3d(-130.654441719, -23.940079563, 33.615980154),
				new Point3d(-130.647523607, -23.97831992,  33.574090343),
				new Point3d(-130.227116296, -24.281964706, 33.16840969),
				new Point3d(-130.768889269, -24.638660329, 32.898871778),
				new Point3d(-131.043005852, -24.422785847, 33.182336682),
				new Point3d(-131.142901945, -24.434164037, 33.190253129),
				new Point3d(-131.202089946, -24.62923634,  32.995450298),
				new Point3d(-131.872239719, -24.572744503, 33.189252073),
				new Point3d(-131.793581885, -24.382659994, 33.374879499),
			};

			//Assert.IsTrue(poly1.IsPlanar(0.000001));
			Shape s1 = new Shape(poly1);

			List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(0.1);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, null, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void SquareWithSquareEmbedded()
		{
			double side = 500;
			Polygon3d outer = new Polygon3d()
			{
				new Point3d(-side / 2, -side / 2, 0),
				new Point3d(side / 2, -side / 2, 0),
				new Point3d(side / 2, side / 2, 0),
				new Point3d(-side / 2, side / 2, 0)
			};

			Polygon3d inner = new Polygon3d()
			{
				new Point3d(-side / 4, -side / 4, 0),
				new Point3d(side / 4, -side / 4, 0),
				new Point3d(side / 4, side / 4, 0),
				new Point3d(-side / 4, side / 4, 0)
			};

			Shape s1 = new Shape(outer, new Polygon3d[1] { inner }, null);
			Shape s2 = new Shape(inner);
			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = side / 10,
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
				Smoothing = 10
			};

			GMesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void DuplicatedFacesClean()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(-20, -10, 0),
				new Point3d(0, -10, 0),
				new Point3d(0, 10, 0),
				new Point3d(-20, 10, 0)
			};
			Shape s1 = new Shape(p1);
			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(0, -10, 0),
				new Point3d(20, -10, 0),
				new Point3d(20, 10, 0),
				new Point3d(0, 10, 0)
			};
			Shape s2 = new Shape(p2);
			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void DuplicatedFacesClean2()
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
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, -100, 0),
				new Point3d(0, -100, 0)
			};
			Shape s2 = new Shape(p2);
			Polygon3d p3 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(-100, 0, 0),
				new Point3d(-100, -100, 0),
				new Point3d(0, -100, 0)
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
			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2,
				s3,
				s4
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(10, 10, 0), new Point3d(90, 90, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(10, -10, 0), new Point3d(90, -90, 0)) },
				[s3] = new GeometryBase[] { new Line3d(new Point3d(-10, -10, 0), new Point3d(-90, -90, 0)) },
				[s4] = new GeometryBase[] { new Line3d(new Point3d(-10, 10, 0), new Point3d(-90, 90, 0)) }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void ManyShape()
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

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(-15, -15, 0), new Point3d(0, 15, 0)) },
				[s3] = new GeometryBase[] { new Line3d(new Point3d(-27, 15, 0), new Point3d(-35, -15, 0)) },
				[s6] = new GeometryBase[] { new Line3d(new Point3d(-35, 21, 0), new Point3d(35, 35, 0)) }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(2, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);

			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void OneVerticleCoincident()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(-20, -10, 0),
				new Point3d(0, -10, 0),
				new Point3d(-10, 10, 0),
				new Point3d(-20, 10, 0)
			};
			Shape s1 = new Shape(p1);
			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(0, -10, 0),
				new Point3d(20, -10, 0),
				new Point3d(20, 10, 0),
				new Point3d(10, 10, 0)
			};
			Shape s2 = new Shape(p2);
			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(1, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void DuplicatedFacesCleanWithLine1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(-200, -100, 0),
				new Point3d(0, -100, 0),
				new Point3d(0, 100, 0),
				new Point3d(-200, 100, 0)
			};

			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(0, -100, 0),
				new Point3d(200, -100, 0),
				new Point3d(200, 100, 0),
				new Point3d(0, 100, 0)
			};
			Shape s1 = new Shape(p1);
			Shape s2 = new Shape(p2);

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { new Line3d(new Point3d(-150, 100, 0), new Point3d(0, -50, 0)) }
			};
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void DuplicatedFacesCleanWithLine2()
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
			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, -50, 0), new Point3d(150, 100, 0)) }
			};
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);

			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void DuplicatedFacesCleanWithTwoLine()
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
				[s1] = new GeometryBase[] { new Line3d(new Point3d(-150, 100, 0), new Point3d(0, -50, 0)) },
				[s2] = new GeometryBase[] { new Line3d(new Point3d(0, -50, 0), new Point3d(150, 100, 0)) }
			};
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void TransfiniteSurface1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(-20, -10, 0),
				new Point3d(0, -10, 0),
				new Point3d(-10, 10, 0),
				new Point3d(-20, 10, 0)
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 1,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true,
				TransfiniteSurface = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 5);
		}

		[TestMethod]
		public void TransfiniteSurface2()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(-30, -10, 0),
				new Point3d(0, -10, 0),
				new Point3d(-10, 10, 0),
				new Point3d(-20, 10, 0)
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = 1,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true,
				TransfiniteSurface = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 7);
		}

		[TestMethod]
		public void PointOnCommonBorder()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(10, 10, 0),
				new Point3d(0, 10, 0)
			};
			Shape s1 = new Shape(p1);

			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(10, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 10, 0),
				new Point3d(10, 10, 0)
			};
			Shape s2 = new Shape(p2);

			List<Shape> shapes = new List<Shape>
			{
				s1,
				s2
			};

			Point3d point1 = new Point3d(10, 7, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { point1 }
			};
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(2, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 7);
		}

		[TestMethod]
		public void RectangularWithHoleScaled()
		{
			Polygon3d fill = new Polygon3d()
			{
				new Point3d(-31.54, 10.50, 0.00),
				new Point3d(13.72, 10.50, 0.00),
				new Point3d(13.72, -23.97, 0.00),
				new Point3d(-31.54, -23.97, 0.00 )
			};

			Polygon3d hole = new Polygon3d()
			{
				new Point3d(-2.32,-10.98,0.00),
				new Point3d(5.74,-10.98,0.00 ),
				new Point3d(5.74,-17.49,0.00),
				new Point3d(-2.32,-17.49,0.00)
			};

			Shape s1 = new Shape(fill, new Polygon3d[] { hole }, null);


			double meshSize = 2;
			double scaleFactor = 0.01;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = scaleFactor,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom,
				MeshSize = meshSize * scaleFactor,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true,
				MeshScalingFactor = 1 / scaleFactor
			};

			List<Shape> shapes = new List<Shape>() { s1 };
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, meshSize, meshSize, generateMeshStatus);
		}

		[TestMethod]
		public void PreProcessingTimeTest1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(10, 10, 0),
				new Point3d(0, 10, 0)
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Point3d point1 = new Point3d(9, 1, 0);
			Point3d point2 = new Point3d(9, 2, 0);
			Point3d point3 = new Point3d(9, 3, 0);
			Point3d point4 = new Point3d(9, 4, 0);
			Point3d point5 = new Point3d(9, 5, 0);
			Point3d point6 = new Point3d(9, 6, 0);
			Point3d point7 = new Point3d(9, 7, 0);
			Point3d point8 = new Point3d(9, 8, 0);
			Point3d point9 = new Point3d(9, 9, 0);
			Point3d point10 = new Point3d(1, 1, 0);
			Point3d point11 = new Point3d(1, 2, 0);
			Point3d point12 = new Point3d(1, 3, 0);
			Point3d point13 = new Point3d(1, 4, 0);
			Point3d point14 = new Point3d(1, 5, 0);
			Point3d point15 = new Point3d(1, 6, 0);
			Point3d point16 = new Point3d(1, 7, 0);
			Point3d point17 = new Point3d(1, 8, 0);
			Point3d point18 = new Point3d(1, 9, 0);
			Point3d point19 = new Point3d(1, 0.5, 0);
			Point3d point20 = new Point3d(1, 9.5, 0);
			Point3d point21 = new Point3d(2, 1, 0);
			Point3d point22 = new Point3d(2, 2, 0);
			Point3d point23 = new Point3d(2, 3, 0);
			Point3d point24 = new Point3d(2, 4, 0);
			Point3d point25 = new Point3d(2, 5, 0);
			Point3d point26 = new Point3d(2, 6, 0);
			Point3d point27 = new Point3d(2, 7, 0);
			Point3d point28 = new Point3d(2, 8, 0);
			Point3d point29 = new Point3d(2, 9, 0);
			Point3d point30 = new Point3d(4, 1, 0);
			Point3d point31 = new Point3d(4, 2, 0);
			Point3d point32 = new Point3d(4, 3, 0);
			Point3d point33 = new Point3d(4, 4, 0);
			Point3d point34 = new Point3d(4, 5, 0);
			Point3d point35 = new Point3d(4, 6, 0);
			Point3d point36 = new Point3d(4, 7, 0);
			Point3d point37 = new Point3d(4, 8, 0);
			Point3d point38 = new Point3d(4, 9, 0);
			Point3d point39 = new Point3d(5, 9, 0);
			Point3d point40 = new Point3d(6, 1, 0);
			Point3d point41 = new Point3d(6, 2, 0);
			Point3d point42 = new Point3d(6, 3, 0);
			Point3d point43 = new Point3d(6, 4, 0);
			Point3d point44 = new Point3d(6, 5, 0);
			Point3d point45 = new Point3d(6, 6, 0);
			Point3d point46 = new Point3d(6, 7, 0);
			Point3d point47 = new Point3d(6, 8, 0);
			Point3d point48 = new Point3d(6, 9, 0);
			Point3d point49 = new Point3d(7, 9, 0);
			Point3d point50 = new Point3d(8, 1, 0);
			Point3d point51 = new Point3d(8, 2, 0);
			Point3d point52 = new Point3d(8, 3, 0);
			Point3d point53 = new Point3d(8, 4, 0);
			Point3d point54 = new Point3d(8, 5, 0);
			Point3d point55 = new Point3d(8, 6, 0);
			Point3d point56 = new Point3d(8, 7, 0);
			Point3d point57 = new Point3d(8, 8, 0);
			Point3d point58 = new Point3d(8, 9, 0);
			Point3d point59 = new Point3d(9, 9.5, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { point1, point2, point3, point4, point5, point6, point7, point8, point9,
					point10, point11, point12, point13, point14, point15, point16, point17, point18, point19, point20,
					point21, point22, point23, point24, point25, point26, point27, point28, point29, point30, point31,
					point32, point33, point34, point35, point36, point37, point38, point39, point40, point41, point42,
					point43, point44, point45, point46, point47, point48, point49, point50, point51, point52, point53,
					point54, point55, point56, point57, point58, point59}
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(1, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 7);
		}

		[TestMethod]
		public void PostProcessingTimeTest1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);  // per vedere i tempi di calcolo portare a 1 oppure 0.5

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void PreProcessingTimeTest2()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(1, 0, 0),
				new Point3d(2, 0, 0),
				new Point3d(3, 0, 0),
				new Point3d(4, 0, 0),
				new Point3d(5, 0, 0),
				new Point3d(6, 0, 0),
				new Point3d(7, 0, 0),
				new Point3d(8, 0, 0),
				new Point3d(9, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(10, 1, 0),
				new Point3d(10, 2, 0),
				new Point3d(10, 3, 0),
				new Point3d(10, 4, 0),
				new Point3d(10, 5, 0),
				new Point3d(10, 6, 0),
				new Point3d(10, 7, 0),
				new Point3d(10, 8, 0),
				new Point3d(10, 9, 0),
				new Point3d(10, 10, 0),
				new Point3d(9, 10, 0),
				new Point3d(8, 10, 0),
				new Point3d(7, 10, 0),
				new Point3d(6, 10, 0),
				new Point3d(5, 10, 0),
				new Point3d(4, 10, 0),
				new Point3d(3, 10, 0),
				new Point3d(2, 10, 0),
				new Point3d(1, 10, 0),
				new Point3d(0, 10, 0),
				new Point3d(0, 9, 0),
				new Point3d(0, 8, 0),
				new Point3d(0, 7, 0),
				new Point3d(0, 6, 0),
				new Point3d(0, 5, 0),
				new Point3d(0, 4, 0),
				new Point3d(0, 3, 0),
				new Point3d(0, 2, 0),
				new Point3d(0, 1, 0),
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };
			Point3d point1 = new Point3d(5, 5, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]> { [s1] = new GeometryBase[] { point1 } };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(1);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 7);
		}

		[TestMethod]
		public void PreProcessingTimeTest3()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(1, 0, 0),
				new Point3d(2, 0, 0),
				new Point3d(3, 0, 0),
				new Point3d(4, 0, 0),
				new Point3d(5, 0, 0),
				new Point3d(6, 0, 0),
				new Point3d(7, 0, 0),
				new Point3d(8, 0, 0),
				new Point3d(9, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(10, 1, 0),
				new Point3d(10, 2, 0),
				new Point3d(10, 3, 0),
				new Point3d(10, 4, 0),
				new Point3d(10, 5, 0),
				new Point3d(10, 6, 0),
				new Point3d(10, 7, 0),
				new Point3d(10, 8, 0),
				new Point3d(10, 9, 0),
				new Point3d(10, 10, 0),
				new Point3d(9, 10, 0),
				new Point3d(8, 10, 0),
				new Point3d(7, 10, 0),
				new Point3d(6, 10, 0),
				new Point3d(5, 10, 0),
				new Point3d(4, 10, 0),
				new Point3d(3, 10, 0),
				new Point3d(2, 10, 0),
				new Point3d(1, 10, 0),
				new Point3d(0, 10, 0),
				new Point3d(0, 9, 0),
				new Point3d(0, 8, 0),
				new Point3d(0, 7, 0),
				new Point3d(0, 6, 0),
				new Point3d(0, 5, 0),
				new Point3d(0, 4, 0),
				new Point3d(0, 3, 0),
				new Point3d(0, 2, 0),
				new Point3d(0, 1, 0),
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Point3d point1 = new Point3d(9, 1, 0);
			Point3d point2 = new Point3d(9, 2, 0);
			Point3d point3 = new Point3d(9, 3, 0);
			Point3d point4 = new Point3d(9, 4, 0);
			Point3d point5 = new Point3d(9, 5, 0);
			Point3d point6 = new Point3d(9, 6, 0);
			Point3d point7 = new Point3d(9, 7, 0);
			Point3d point8 = new Point3d(9, 8, 0);
			Point3d point9 = new Point3d(9, 9, 0);
			Point3d point10 = new Point3d(1, 1, 0);
			Point3d point11 = new Point3d(1, 2, 0);
			Point3d point12 = new Point3d(1, 3, 0);
			Point3d point13 = new Point3d(1, 4, 0);
			Point3d point14 = new Point3d(1, 5, 0);
			Point3d point15 = new Point3d(1, 6, 0);
			Point3d point16 = new Point3d(1, 7, 0);
			Point3d point17 = new Point3d(1, 8, 0);
			Point3d point18 = new Point3d(1, 9, 0);
			Point3d point19 = new Point3d(1, 0.5, 0);
			Point3d point20 = new Point3d(1, 9.5, 0);
			Point3d point21 = new Point3d(2, 1, 0);
			Point3d point22 = new Point3d(2, 2, 0);
			Point3d point23 = new Point3d(2, 3, 0);
			Point3d point24 = new Point3d(2, 4, 0);
			Point3d point25 = new Point3d(2, 5, 0);
			Point3d point26 = new Point3d(2, 6, 0);
			Point3d point27 = new Point3d(2, 7, 0);
			Point3d point28 = new Point3d(2, 8, 0);
			Point3d point29 = new Point3d(2, 9, 0);
			Point3d point30 = new Point3d(4, 1, 0);
			Point3d point31 = new Point3d(4, 2, 0);
			Point3d point32 = new Point3d(4, 3, 0);
			Point3d point33 = new Point3d(4, 4, 0);
			Point3d point34 = new Point3d(4, 5, 0);
			Point3d point35 = new Point3d(4, 6, 0);
			Point3d point36 = new Point3d(4, 7, 0);
			Point3d point37 = new Point3d(4, 8, 0);
			Point3d point38 = new Point3d(4, 9, 0);
			Point3d point39 = new Point3d(5, 9, 0);
			Point3d point40 = new Point3d(6, 1, 0);
			Point3d point41 = new Point3d(6, 2, 0);
			Point3d point42 = new Point3d(6, 3, 0);
			Point3d point43 = new Point3d(6, 4, 0);
			Point3d point44 = new Point3d(6, 5, 0);
			Point3d point45 = new Point3d(6, 6, 0);
			Point3d point46 = new Point3d(6, 7, 0);
			Point3d point47 = new Point3d(6, 8, 0);
			Point3d point48 = new Point3d(6, 9, 0);
			Point3d point49 = new Point3d(7, 9, 0);
			Point3d point50 = new Point3d(8, 1, 0);
			Point3d point51 = new Point3d(8, 2, 0);
			Point3d point52 = new Point3d(8, 3, 0);
			Point3d point53 = new Point3d(8, 4, 0);
			Point3d point54 = new Point3d(8, 5, 0);
			Point3d point55 = new Point3d(8, 6, 0);
			Point3d point56 = new Point3d(8, 7, 0);
			Point3d point57 = new Point3d(8, 8, 0);
			Point3d point58 = new Point3d(8, 9, 0);
			Point3d point59 = new Point3d(9, 9.5, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { point1, point2, point3, point4, point5, point6, point7, point8, point9,
					point10, point11, point12, point13, point14, point15, point16, point17, point18, point19, point20,
					point21, point22, point23, point24, point25, point26, point27, point28, point29, point30, point31,
					point32, point33, point34, point35, point36, point37, point38, point39, point40, point41, point42,
					point43, point44, point45, point46, point47, point48, point49, point50, point51, point52, point53,
					point54, point55, point56, point57, point58, point59}
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(1, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 7);
		}

		[TestMethod]
		public void EmbedInWrongShape()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Shape s1 = new Shape(p1);

			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(50, 0, 0),
				new Point3d(50, 0, 30),
				new Point3d(50, 100, 30),
				new Point3d(50, 100, 0),
			};
			Shape s2 = new Shape(p2);

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Point3d point1 = new Point3d(50, 50, 0);
			Point3d point2 = new Point3d(50, 20, 0);
			Point3d point3 = new Point3d(50, 80, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				[s1] = new GeometryBase[] { point1, point2, point3 }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void GetNormal1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 80, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void GetNormal2()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(0, 100, 0),
				new Point3d(100, 100, 0),
				new Point3d(100, 0, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void GetNormal3()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void GetNormal4()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(100, 0, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void PointsCloserThanTolerance1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Point3d point1 = new Point3d(51, 0, 0);
			Point3d point2 = new Point3d(51.00001, 0, 0);
			Point3d point3 = new Point3d(51, 100, 0);
			Point3d point4 = new Point3d(51.00001, 100, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { point1, point2, point3, point4 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void PointsCloserThanTolerance2()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Point3d point1 = new Point3d(48, 0, 0);
			Point3d point2 = new Point3d(49, 0, 0);
			Point3d point3 = new Point3d(50, 0, 0);
			Point3d point4 = new Point3d(51, 0, 0);
			Point3d point5 = new Point3d(52, 0, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { point1, point2, point3, point4, point5 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
		}

		[TestMethod]
		public void PointsCloserThanToleranceScaled1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Shape s1 = new Shape(p1);
			List<Shape> shapes = new List<Shape> { s1 };

			Point3d point1 = new Point3d(49, 0, 0);
			Point3d point2 = new Point3d(50, 0, 0);
			Point3d point3 = new Point3d(51, 100, 0);
			Point3d point4 = new Point3d(52, 100, 0);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { point1, point2, point3, point4 } }
			};

			double meshSize = 25;
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(meshSize, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			try
			{
				GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			}
			catch (GmshNet.GmshException e)
			{
				Debug.WriteLine(e, "Unexpected error");
			}
		}

		[TestMethod]
		public void LinesThroughHoles0()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 0, 100),
				new Point3d(0, 0, 100),
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(30, 0, 30),
				new Point3d(70, 0, 30),
				new Point3d(70, 0, 70),
				new Point3d(30, 0, 70),
			};
			Shape s1 = new Shape(p1, new Polygon3d[] { h1 }, null);

			List<Shape> shapes = new List<Shape> { s1 };

			Line3d l1_1 = new Line3d(new Point3d(0, 0, 30), new Point3d(100, 0, 70));
			Line3d l1_2 = new Line3d(new Point3d(0, 0, 60), new Point3d(100, 0, 20));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { l1_1, l1_2 } }
			};

			double tol = 0.00001;
			double scaleFactor = 1e-7 / tol;
			double meshSize = 5;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = scaleFactor,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = meshSize * scaleFactor,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 5,
				HealShapes = false,
				Transfinite = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void LinesThroughHoles1()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 0, 100),
				new Point3d(0, 0, 100),
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(30, 0, 30),
				new Point3d(70, 0, 30),
				new Point3d(70, 0, 70),
				new Point3d(30, 0, 70),
			};
			Shape s1 = new Shape(p1, new Polygon3d[] { h1 }, null);

			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(100, 100, 100),
				new Point3d(100, 0, 100),
			};
			Polygon3d h2 = new Polygon3d()
			{
				new Point3d(100, 30, 30),
				new Point3d(100, 70, 30),
				new Point3d(100, 70, 70),
				new Point3d(100, 30, 70),
			};
			Shape s2 = new Shape(p2, new Polygon3d[] { h2 }, null);

			Polygon3d p3 = new Polygon3d()
			{
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
				new Point3d(0, 100, 100),
				new Point3d(100, 100, 100),
			};
			Polygon3d h3 = new Polygon3d()
			{
				new Point3d(70, 100, 30),
				new Point3d(30, 100, 30),
				new Point3d(30, 100, 70),
				new Point3d(70, 100, 70),
			};
			Shape s3 = new Shape(p3, new Polygon3d[] { h3 }, null);

			Polygon3d p4 = new Polygon3d()
			{
				new Point3d(0, 100, 0),
				new Point3d(0, 0, 0),
				new Point3d(0, 0, 100),
				new Point3d(0, 100, 100),
			};
			Polygon3d h4 = new Polygon3d()
			{
				new Point3d(0, 30, 30),
				new Point3d(0, 70, 30),
				new Point3d(0, 70, 70),
				new Point3d(0, 30, 70),
			};
			Shape s4 = new Shape(p4, new Polygon3d[] { h4 }, null);

			List<Shape> shapes = new List<Shape> { s1, s2, s3, s4 };

			Line3d l1_1 = new Line3d(new Point3d(0, 0, 30), new Point3d(100, 0, 70));
			Line3d l1_2 = new Line3d(new Point3d(0, 0, 60), new Point3d(100, 0, 20));

			Line3d l2_1 = new Line3d(new Point3d(100, 0, 70), new Point3d(100, 100, 30));
			Line3d l2_2 = new Line3d(new Point3d(100, 0, 20), new Point3d(100, 100, 60));

			Line3d l3_1 = new Line3d(new Point3d(100, 100, 30), new Point3d(0, 100, 70));
			Line3d l3_2 = new Line3d(new Point3d(100, 100, 60), new Point3d(0, 100, 20));

			Line3d l4_1 = new Line3d(new Point3d(0, 100, 70), new Point3d(0, 0, 30));
			Line3d l4_2 = new Line3d(new Point3d(0, 100, 20), new Point3d(0, 0, 60));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { l1_1, l1_2 } },
				{ s2, new GeometryBase[] { l2_1, l2_2 } },
				{ s3, new GeometryBase[] { l3_1, l3_2 } },
				{ s4, new GeometryBase[] { l4_1, l4_2 } }
			};

			double tol = 0.00001;
			double scaleFactor = 1e-7 / tol;
			double meshSize = 5;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				GeometryBaseScaleFactor = scaleFactor,
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = meshSize * scaleFactor,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 5,
				HealShapes = false,
				Transfinite = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void SplitHoles()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(30, 100, 0),
				new Point3d(40, 100, 0),
				new Point3d(40, 50, 0),
				new Point3d(70, 50, 0),
				new Point3d(70, 100, 0),
				new Point3d(80, 100, 0),
				new Point3d(80, 40, 0),
				new Point3d(30, 40, 0),
			};
			Shape s1 = new Shape(p1, new Polygon3d[] { h1 }, null);

			List<Shape> shapes = new List<Shape> { s1 };

			Line3d l1_1 = new Line3d(new Point3d(0, 30, 0), new Point3d(100, 70, 0));
			Line3d l1_2 = new Line3d(new Point3d(0, 60, 0), new Point3d(100, 20, 0));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { l1_1, l1_2 } }
			};

			double meshSize = 5;
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = meshSize,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 5,
				HealShapes = false,
				Transfinite = true
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void RebuildObjectTagsTest()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(30, 30, 0),
				new Point3d(70, 30, 0),
				new Point3d(70, 70, 0),
				new Point3d(30, 70, 0),
			};
			Shape s1 = new Shape(p1, new Polygon3d[] { h1 }, null);
			Shape s2 = (Shape)s1.Clone();
			Shape s3 = (Shape)s1.Clone();
			Shape s4 = (Shape)s1.Clone();
			Shape s5 = (Shape)s1.Clone();
			s2.Move(100, 0, 0);
			s3.Move(200, 0, 0);
			s4.Move(300, 0, 0);
			s5.Move(400, 0, 0);

			List<Shape> shapes = new List<Shape> { s1, s2, s3, s4, s5 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void ShapeSplitLine()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};
			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(50, 0, 0),
				new Point3d(50, 72, 0),
				new Point3d(50, 40, 50),
				new Point3d(50, 0, 50),
			};

			Shape s1 = new Shape(p1);
			Shape s2 = new Shape(p2);
			Line3d l1 = new Line3d(new Point3d(0, 30, 0), new Point3d(100, 60, 0));
			Line3d l2 = new Line3d(new Point3d(50, 10, 0), new Point3d(50, 40, 50));

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { l1 } },
				{ s2, new GeometryBase[] { l2 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void ShapeSplitLine2()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(1, 0, 0),
				new Point3d(1, 1, 0),
				new Point3d(0, 1, 0),
			};
			Polygon3d p2 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(1.5, 0, 0),
				new Point3d(1.5, 0, 1),
				new Point3d(0, 0, 1),
			};

			Shape s1 = new Shape(p1);
			Shape s2 = new Shape(p2);
			Line3d l1 = new Line3d(new Point3d(0, 0.3, 0), new Point3d(0.85, 0, 0));
			Line3d l2 = new Line3d(new Point3d(0, 0, .6), new Point3d(0.54, 0, 0));

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { l1 } },
				{ s2, new GeometryBase[] { l2 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(0.05);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void RectangularIsInsideTestPoints()
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(0, 1000, 0),
				new Point3d(1000, 1000, 0),
				new Point3d(1000, 0, 0)
			};
			Shape s1 = new Shape(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			List<GeometryBase> gbList = new List<GeometryBase>();
			for (int i = 1; i < 50; i++)
				for (int j = 1; j < 50; j++)
					gbList.Add(new Point3d(20 * i, 20 * j, 0));
			embeddedGeometries[s1] = gbList.ToArray();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			foreach (KeyValuePair<string, double> message in generateMeshStatus.ExecutionTime)
				Console.WriteLine(message.Key + ": " + message.Value.ToString());
		}

		[TestMethod]
		public void Shape2DTest1()
		{
			Polygon2d p1 = new Polygon2d()
			{
				new Point2d(10, 0),
				new Point2d(-100, 1000),
				new Point2d(1200, 900),
				new Point2d(1000, -110)
			};
			Shape2d s1 = new Shape2d(p1);

			List<Shape> shapes = new List<Shape> { s1 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		public void Shape2DTest2()
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

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}
	}
}
