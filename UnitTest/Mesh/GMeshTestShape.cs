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
	public class GMeshTestShape : GenericMeshTest
	{
		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape")]
		public void RectangularWithShapeEmb1()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0),
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0)
			};

			Shape s = new Shape(shape, new Polygon3d[] { hole }, null);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape; 4 points; 4 lines")]
		public void RectangularWithShapeEmb7()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(60, 100, 0),
				new Point3d(40, 100, 0),
				new Point3d(40, 80, 0),
				new Point3d(60, 80, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(80, 40, 0),
				new Point3d(80, 80, 0),
				new Point3d(40, 80, 0)
			};
			Polygon3d hole2 = new Polygon3d()
			{
				new Point3d(20, 40, 0),
				new Point3d(40, 40, 0),
				new Point3d(40, 80, 0),
				new Point3d(20, 80, 0)
			};

			Point3d p1 = new Point3d(52,40,0);
			Point3d p2 = new Point3d(52, 80, 0);
			Point3d p3 = new Point3d(52, 52, 0);
			Point3d p4 = new Point3d(80, 52, 0);
			Line3d line1 = new Line3d(new Point3d(50, 80, 0), new Point3d(100, 40, 0));
			Line3d line2 = new Line3d(new Point3d(60, 0, 0), new Point3d(80, 100, 0));

			Shape s = new Shape(shape, new Polygon3d[] { hole, hole2 }, null);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 2.5,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, p1, p2, p3, p4, line1, line2 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape")]
		public void RectangularWithShapeEmb3()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(80, 0, 0),
				new Point3d(80, 80, 0),
				new Point3d(0, 80, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(60, 20, 0),
				new Point3d(60, 60, 0),
				new Point3d(20, 60, 0)
			};

			Shape s = new Shape(shape);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 3 shapes")]
		public void RectangularWithShapeEmb2()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0),
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(20, 60, 0),
				new Point3d(40, 60, 0),
				new Point3d(40, 80, 0),
				new Point3d(20, 80, 0)
			};
			Polygon3d embS3 = new Polygon3d()
			{
				new Point3d(60, 20, 0),
				new Point3d(80, 20, 0),
				new Point3d(80, 40, 0),
				new Point3d(60, 40, 0)
			};

			Shape s = new Shape(shape, new Polygon3d[] { hole }, null);
			Shape embShape1 = new Shape(embS1);
			Shape embShape2 = new Shape(embS2);
			Shape embShape3 = new Shape(embS3);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape1, embShape2, embShape3 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shapes; 6 lines")]
		public void RectangularWithShapeEmb4()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0),
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(20, 60, 0),
				new Point3d(40, 60, 0),
				new Point3d(40, 80, 0),
				new Point3d(20, 80, 0)
			};
			Polygon3d embS3 = new Polygon3d()
			{
				new Point3d(60, 20, 0),
				new Point3d(80, 20, 0),
				new Point3d(80, 40, 0),
				new Point3d(60, 40, 0)
			};

			Shape s = new Shape(shape);
			Shape embShape1 = new Shape(embS1);
			Shape embShape2 = new Shape(embS2);
			Shape embShape3 = new Shape(embS3);
			Shape embShape4 = new Shape(hole);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 2.5,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Line3d line1 = new Line3d(new Point3d(0, 70, 0), new Point3d(100, 70, 0));
			Line3d line2 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));
			Line3d line3 = new Line3d(new Point3d(70, 0, 0), new Point3d(70, 100, 0));
			Line3d line4 = new Line3d(new Point3d(50, 0, 0), new Point3d(50, 100, 0));
			Line3d line5 = new Line3d(new Point3d(0, 35, 0), new Point3d(100, 35, 0));
			Line3d line6 = new Line3d(new Point3d(0, 25, 0), new Point3d(100, 25, 0));

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape1, embShape2, embShape3, embShape4, line1, line2, line3, line4, line5, line6 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape; 1 line ")]
		public void RectangularWithShapeAndLineEmb1()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(50, 50, 0),
				new Point3d(70, 50, 0),
				new Point3d(70, 70, 0),
				new Point3d(50, 70, 0)
			};
			Line3d line = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 70, 0));

			Shape s = new Shape(shape, new Polygon3d[] { hole }, null);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, line } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape; 2 lines ")]
		public void RectangularWithShapeAndLineEmb2()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(80, 40, 0),
				new Point3d(80, 80, 0),
				new Point3d(40, 80, 0)
			};
			Line3d line1 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 70, 0));
			Line3d line2 = new Line3d(new Point3d(0, 70, 0), new Point3d(100, 50, 0));

			Shape s = new Shape(shape, new Polygon3d[] { hole }, null);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 10,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> dictionary = new Dictionary<Shape, GeometryBase[]>();
			Dictionary<Shape, GeometryBase[]> embeddedGeometries = dictionary;
			embeddedGeometries.Add(s, new GeometryBase[] { embShape, line1, line2 });

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape; 4 lines ")]
		public void RectangularWithShapeAndLineEmb3()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d hole = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(80, 40, 0),
				new Point3d(80, 80, 0),
				new Point3d(40, 80, 0)
			};
			Line3d line1 = new Line3d(new Point3d(0, 70, 0), new Point3d(100, 70, 0));
			Line3d line2 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));
			Line3d line3 = new Line3d(new Point3d(70, 0, 0), new Point3d(70, 100, 0));
			Line3d line4 = new Line3d(new Point3d(50, 0, 0), new Point3d(50, 100, 0));

			Shape s = new Shape(shape, new Polygon3d[] { hole }, null);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 5,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, line1, line2, line3, line4 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 shapes; 6 lines ")]
		public void RectangularWithShapeAndLineEmb4()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(40, 20, 0),
				new Point3d(40, 40, 0),
				new Point3d(20, 40, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(80, 40, 0),
				new Point3d(80, 80, 0),
				new Point3d(40, 80, 0)
			};
			Line3d line1 = new Line3d(new Point3d(0, 70, 0), new Point3d(100, 70, 0));
			Line3d line2 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));
			Line3d line3 = new Line3d(new Point3d(70, 0, 0), new Point3d(70, 100, 0));
			Line3d line4 = new Line3d(new Point3d(50, 0, 0), new Point3d(50, 100, 0));
			Line3d line5 = new Line3d(new Point3d(0, 35, 0), new Point3d(100, 35, 0));
			Line3d line6 = new Line3d(new Point3d(0, 25, 0), new Point3d(100, 25, 0));

			Shape s = new Shape(shape);
			Shape embShape = new Shape(embS1);
			Shape embShape2 = new Shape(embS2);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 5,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, embShape2, line1, line2, line3, line4, line5, line6 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 4);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape; 1 line ")]
		public void RectangularWithShapeAndLineEmb5()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(50, 50, 0),
				new Point3d(100, 50, 0),
				new Point3d(100, 100, 0),
				new Point3d(50, 100, 0)
			};
			Line3d line = new Line3d(new Point3d(20, 20, 0), new Point3d(80, 70, 0));

			Shape s = new Shape(shape);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				MeshSize = 5,
				UseGlobalProgressID = true,
				HealShapes = true,
				Transfinite = true
			};

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, line } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("4 intersect shape. emb: 3 shape")]
		public void FourPlaneWithEmbShapeSplit()
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

			Polygon3d ep2 = new Polygon3d()
			{
				new Point3d(0, 20, 20),
				new Point3d(0, 20, 50),
				new Point3d(0, 50, 50),
				new Point3d(0, 50, 20)
			};
			Shape es2 = new Shape(ep2);
			Polygon3d ep3 = new Polygon3d()
			{
				new Point3d(50, 50, 50),
				new Point3d(50, 50, 20),
				new Point3d(50, 20, 20),
				new Point3d(50, 20, 50)
			};
			Shape es3 = new Shape(ep3);
			Polygon3d ep4 = new Polygon3d()
			{
				new Point3d(-50, 50, 50),
				new Point3d(-50, 50, 20),
				new Point3d(-50, 20, 20),
				new Point3d(-50, 20, 50)
			};
			Shape es4 = new Shape(ep4);

			var shapes = new List<Shape> { s1, s2, s3, s4 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s2, new GeometryBase[] { es2 } },
				{ s3, new GeometryBase[] { es3 } },
				{ s4, new GeometryBase[] { es4 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 intersect shape. emb: 1 shape")]
		public void TwoPlaneWithEmbShapeSplit()
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

			Polygon3d ep1 = new Polygon3d()
			{
				new Point3d(50, 50, 0),
				new Point3d(-50, 50, 0),
				new Point3d(-50, -50, 0),
				new Point3d(50, -50, 0)
			};
			Shape es1 = new Shape(ep1);

			List<Shape> shapes = new List<Shape> { s1, s2 };

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { es1 } }
			};

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 4 shapes")]
		public void RectangularWithShapeEmb5()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(60, 20, 0),
				new Point3d(60, 60, 0),
				new Point3d(20, 60, 0)
			};
			Polygon3d embS12 = new Polygon3d()
			{
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0),
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0)
			};
			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape11 = new Shape(embS12);

			Polygon3d shape2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(200, 0, 0),
				new Point3d(200, 100, 0),
				new Point3d(100, 100, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(120, 20, 0),
				new Point3d(160, 20, 0),
				new Point3d(160, 60, 0),
				new Point3d(120, 60, 0)
			};
			Polygon3d embS22 = new Polygon3d()
			{
				new Point3d(180, 80, 0),
				new Point3d(160, 80, 0),
				new Point3d(160, 60, 0),
				new Point3d(180, 60, 0)
			};
			Shape s2 = new Shape(shape2);
			Shape embShape2 = new Shape(embS2);
			Shape embShape22 = new Shape(embS22);

			List<Shape> shapes = new List<Shape>() { s1, s2 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
			   true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape11 } },
				{ s2, new GeometryBase[] { embShape2, embShape22 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 4 shapes; 4 lines")]
		public void RectangularWithShapeEmb6()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(60, 20, 0),
				new Point3d(60, 60, 0),
				new Point3d(20, 60, 0)
			};
			Polygon3d embS12 = new Polygon3d()
			{
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0),
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0)
			};
			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape11 = new Shape(embS12);

			Polygon3d shape2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(200, 0, 0),
				new Point3d(200, 100, 0),
				new Point3d(100, 100, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(120, 20, 0),
				new Point3d(160, 20, 0),
				new Point3d(160, 60, 0),
				new Point3d(120, 60, 0)
			};
			Polygon3d embS22 = new Polygon3d()
			{
				new Point3d(180, 80, 0),
				new Point3d(160, 80, 0),
				new Point3d(160, 60, 0),
				new Point3d(180, 60, 0)
			};
			Shape s2 = new Shape(shape2);
			Shape embShape2 = new Shape(embS2);
			Shape embShape22 = new Shape(embS22);
			List<Shape> shapes = new List<Shape>() { s1, s2 };

			Line3d line1 = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
			Line3d line2 = new Line3d(new Point3d(100, 100, 0), new Point3d(200, 0, 0));
			Line3d line3 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));
			Line3d line4 = new Line3d(new Point3d(100, 50, 0), new Point3d(200, 50, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape11, line1, line3 } },
				{ s2, new GeometryBase[] { embShape2, embShape22, line2, line4 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shapes; 4 lines")]
		public void RectangularWithShapeEmb8()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(40, 20, 0),
				new Point3d(80, 20, 0),
				new Point3d(80, 40, 0),
				new Point3d(40, 40, 0)
			};
			Polygon3d embS12 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(20, 80, 0),
				new Point3d(40, 80, 0),
				new Point3d(40, 20, 0)
			};
			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape11 = new Shape(embS12);

			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(60, 40, 0),
				new Point3d(60, 60, 0),
				new Point3d(40, 60, 0)
			};
			Polygon3d embS22 = new Polygon3d()
			{
				new Point3d(80, 80, 0),
				new Point3d(60, 80, 0),
				new Point3d(60, 60, 0),
				new Point3d(80, 60, 0)
			};
			Shape embShape2 = new Shape(embS2);
			Shape embShape22 = new Shape(embS22);
			List<Shape> shapes = new List<Shape>() { s1 };

			Line3d line1 = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
			Line3d line3 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape11, embShape2, embShape22, line1, line3 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shapes; 4 lines")]
		public void RectangularWithShapeEmb9()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(20, 80, 0),
				new Point3d(40, 80, 0),
				new Point3d(40, 20, 0)
			};
			Polygon3d embS12 = new Polygon3d()
			{
				new Point3d(40, 30, 0),
				new Point3d(40, 70, 0),
				new Point3d(60, 70, 0),
				new Point3d(60, 30, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(60, 40, 0),
				new Point3d(80, 40, 0),
				new Point3d(80, 60, 0),
				new Point3d(60, 60, 0)
			};
			Polygon3d embS22 = new Polygon3d()
			{
				new Point3d(80, 45, 0),
				new Point3d(80, 55, 0),
				new Point3d(90, 55, 0),
				new Point3d(90, 45, 0)
			};

			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape11 = new Shape(embS12);
			Shape embShape2 = new Shape(embS2);
			Shape embShape22 = new Shape(embS22);
			List<Shape> shapes = new List<Shape>() { s1 };

			Line3d line1 = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
			Line3d line3 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape11, embShape2, embShape22, line1, line3 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 2 shapes")]
		public void RectangularWithShapeEmb10()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(20, 80, 0),
				new Point3d(40, 80, 0),
				new Point3d(40, 20, 0)
			};
			Polygon3d embS12 = new Polygon3d()
			{
				new Point3d(40, 30, 0),
				new Point3d(40, 70, 0),
				new Point3d(60, 70, 0),
				new Point3d(60, 30, 0)
			};

			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape11 = new Shape(embS12);
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
			   true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape11 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shapes intersect")]
		public void RectangularWithShapeEmb11()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(20, 50, 0),
				new Point3d(55, 50, 0),
				new Point3d(55, 20, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(40, 90, 0),
				new Point3d(70, 90, 0),
				new Point3d(70, 40, 0)
			};
			Polygon3d embS3 = new Polygon3d()
			{
				new Point3d(10, 30, 0),
				new Point3d(30, 30, 0),
				new Point3d(30, 80, 0),
				new Point3d(10, 80, 0)
			};
			Polygon3d embS4 = new Polygon3d()
			{
				new Point3d(60, 80, 0),
				new Point3d(80, 80, 0),
				new Point3d(80, 95, 0),
				new Point3d(60, 95, 0)
			};

			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape2 = new Shape(embS2);
			Shape embShape3 = new Shape(embS3);
			Shape embShape4 = new Shape(embS4);
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
			   true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2, embShape3, embShape4 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 2 shapes")]
		public void RectangularWithShapeEmb12()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(23, 23, 0),
				new Point3d(23, 83, 0),
				new Point3d(43, 83, 0),
				new Point3d(43, 23, 0)
			};
			Polygon3d shape2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(200, 0, 0),
				new Point3d(200, 100, 0),
				new Point3d(100, 100, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(123, 23, 0),
				new Point3d(123, 83, 0),
				new Point3d(143, 83, 0),
				new Point3d(143, 23, 0)
			};

			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape s2 = new Shape(shape2);
			Shape embShape2 = new Shape(embS2);
			List<Shape> shapes = new List<Shape>() { s1, s2 };

			Line3d line1 = new Line3d(new Point3d(0, 50, 0), new Point3d(100, 50, 0));
			Line3d line2 = new Line3d(new Point3d(100, 50, 0), new Point3d(200, 50, 0));

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, line1 } },
				{ s2, new GeometryBase[] { embShape2, line2 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);

			// September 2026: the faces of the embedded shape were mapped to the id of the previous face (26: centroid (71, 75), out of the
			// shape; 29 was missing): the ids are the ones of the faces inside the shape
			int[] embFaces1 = new int[3] { 27, 28, 29 };
			int[] embFaces2 = new int[3] { 56, 57, 58 };

			int[] outFace1 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[0]][embShape1];
			int[] outFace2 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[1]][embShape2];

			for (int i = 0; i < outFace1.Count(); i++)
				Assert.IsTrue(outFace1[i] == embFaces1[i]);
			for (int i = 0; i < outFace2.Count(); i++)
				Assert.IsTrue(outFace2[i] == embFaces2[i]);

		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 shapes intersect")]
		public void RectangularWithShapeEmb13()
		{
			Polygon3d shape1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embS1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(20, 50, 0),
				new Point3d(55, 50, 0),
				new Point3d(55, 20, 0)
			};
			Polygon3d embS2 = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(40, 90, 0),
				new Point3d(70, 90, 0),
				new Point3d(70, 40, 0)
			};

			Shape s1 = new Shape(shape1);
			Shape embShape1 = new Shape(embS1);
			Shape embShape2 = new Shape(embS2);
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(5);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shapes")]
		public void RectangularWithShapeEmb14()
		{
			double majorSide = 1000;
			double minorSide = 800;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));
			Shape embShape1 = GetRectangularShape(new Point3d(minorSide / 2.0 - 10.0 / 2.0, majorSide / 2.0 - 10.0 / 2.0, 0), new Vector3d(10, 10, 0));

			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25, GMesh.GMeshGenerateOptions.MeshAlgorithm.PackingOfParallelograms,
			   true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);            
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);            
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 6.5);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shapes")]
		public void RectangularWithShapeEmb15()
		{
			double majorSide = 1200;
			double minorSide = 800;
			double loadHeight = 500;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));
			Shape embShape1 = GetRectangularShape(new Point3d(0, loadHeight - 10.0 / 2.0, 0), new Vector3d(minorSide, 10, 0));

			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shapes")]
		public void RectangularWithShapeEmb16()
		{
			double majorSide = 1200;
			double minorSide = 800;
			double loadPosition = 500;
			double loadHeight = 50;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));

			Shape embShape1 = GetRectangularShape(new Point3d(0, loadPosition - loadHeight / 2.0, 0), new Vector3d(minorSide, loadHeight, 0));

			List<Shape> shapes = new List<Shape>() { s1 };
			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(25);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shapes")]
		public void RectangularWithShapeEmbSplit()
		{
			double majorSide = 1200;
			double minorSide = 800;
			double loadHeight = 500;
			double thickness = 100;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));
			Shape embShape1 = GetRectangularShape(new Point3d(0, loadHeight - thickness / 2.0, 0), new Vector3d(minorSide, thickness, 0));
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]> { { s1, new GeometryBase[] { embShape1 } } };

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		public void RectangularWithLineAndShapeOnBorder()
		{
			Polygon3d shape = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(800, 0, 0),
				new Point3d(800, 1600, 0),
				new Point3d(0, 1600, 0)
			};
			Polygon3d embS = new Polygon3d()
			{
				new Point3d(0, 500, 0),
				new Point3d(800, 500, 0),
				new Point3d(800, 520, 0),
				new Point3d(0, 520, 0)
			};
			embS.Reverse();

			Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(0, 1600, 0));

			Shape s = new Shape(shape);
			Shape embShape = new Shape(embS);
			List<Shape> shapes = new List<Shape>() { s };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s, new GeometryBase[] { embShape, line } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 intersect shapes")]
		public void RectangularWithIntersectEmbShapeOnBorder()
		{
			double majorSide = 1200;
			double minorSide = 800;
			double loadHeight = 500;
			double loadWidth = 400;
			double thickness = 100;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));
			Shape embShape1 = GetRectangularShape(new Point3d(0, loadHeight - thickness / 2.0, 0), new Vector3d(minorSide, thickness, 0));
			Shape embShape2 = GetRectangularShape(new Point3d(loadWidth - thickness / 2.0, 0, 0), new Vector3d(thickness, majorSide, 0));
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(50, GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
			   true, GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]> { { s1, new GeometryBase[] { embShape1, embShape2 } } };

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 intersect shapes, 2 lines")]
		public void RectangularWithIntersectEmbShapeOnBorderAnd2Lines()
		{
			double majorSide = 1200;
			double minorSide = 800;
			double loadHeight = 400;
			double loadWidth = 400;
			double thickness = 100;

			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(Math.Min(majorSide, minorSide), Math.Max(majorSide, minorSide), 0));
			Shape embShape1 = GetRectangularShape(new Point3d(0, loadHeight - thickness / 2.0, 0), new Vector3d(minorSide, thickness, 0));
			Shape embShape2 = GetRectangularShape(new Point3d(loadWidth - thickness / 2.0, 0, 0), new Vector3d(thickness, majorSide, 0));

			//Line3d line1 = new Line3d(new Point3d(0, 0, 0), new Point3d(0, majorSide, 0));
			//Line3d line2 = new Line3d(new Point3d(0, 0, 0), new Point3d(minorSide, 0, 0));
			Line3d line3 = new Line3d(new Point3d(minorSide / 2.0, 0, 0), new Point3d(minorSide / 2.0, majorSide, 0));
			//Line3d line4 = new Line3d(new Point3d(0, majorSide / 2.0, 0), new Point3d(minorSide, majorSide / 2.0, 0));

			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(20);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]> 
			{ { s1, new GeometryBase[] { embShape1, embShape2, line3 } } };

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("Fail: Not Implemented")]
		public void RectangularWithShapeEmbRefinement()
		{
			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(100, 100, 0));
			Shape embShape1 = GetRectangularShape(new Point3d(20, 20, 0), new Vector3d(40, 40, 0));
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{ { s1, new GeometryBase[] { embShape1 } } };

			Dictionary<GeometryBase, double> embeddedMeshSize = new Dictionary<GeometryBase, double>() { };
			embeddedMeshSize.Add(embShape1, 10);

			GMesh.Generate(shapes, embeddedGeometries, embeddedMeshSize, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("8 shape. emb: 8 shapes")]
		public void RectangularWithShapeEmb17()
		{
			Shape s1 = GetRectangularShape(new Point3d(0, 0, 0), new Vector3d(100, 100, 0));
			Shape embShape1 = GetRectangularShape(new Point3d(30, 30, 0), new Vector3d(40, 40, 0));

			Shape s2 = GetRectangularShape(new Point3d(100, 0, 0), new Vector3d(100, 100, 0));
			Shape embShape2 = GetRectangularShape(new Point3d(130, 30, 0), new Vector3d(40, 40, 0));

			Shape s3 = GetRectangularShape(new Point3d(200, 0, 0), new Vector3d(100, 100, 0));
			Shape embShape3 = GetRectangularShape(new Point3d(230, 30, 0), new Vector3d(40, 40, 0));

			Shape s4 = GetRectangularShape(new Point3d(300, 0, 0), new Vector3d(100, 100, 0));
			Shape embShape4 = GetRectangularShape(new Point3d(330, 30, 0), new Vector3d(40, 40, 0));

			Shape s5 = GetRectangularShape(new Point3d(0, 100, 0), new Vector3d(100, 100, 0));
			Shape embShape5 = GetRectangularShape(new Point3d(30, 130, 0), new Vector3d(40, 40, 0));

			Shape s6 = GetRectangularShape(new Point3d(100, 100, 0), new Vector3d(100, 100, 0));
			Shape embShape6 = GetRectangularShape(new Point3d(130, 130, 0), new Vector3d(40, 40, 0));

			Shape s7 = GetRectangularShape(new Point3d(200, 100, 0), new Vector3d(100, 100, 0));
			Shape embShape7 = GetRectangularShape(new Point3d(230, 130, 0), new Vector3d(40, 40, 0));

			Shape s8 = GetRectangularShape(new Point3d(300, 100, 0), new Vector3d(100, 100, 0));
			Shape embShape8 = GetRectangularShape(new Point3d(330, 130, 0), new Vector3d(40, 40, 0));


			List<Shape> shapes = new List<Shape>() { s1, s2, s3, s4, s5, s6, s7, s8 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(12);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } },
				{ s2, new GeometryBase[] { embShape2 } },
				{ s3, new GeometryBase[] { embShape3 } },
				{ s4, new GeometryBase[] { embShape4 } },
				{ s5, new GeometryBase[] { embShape5 } },
				{ s6, new GeometryBase[] { embShape6 } },
				{ s7, new GeometryBase[] { embShape7 } },
				{ s8, new GeometryBase[] { embShape8 } },
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus, 3);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 1 shape HashTest")]
		public void RectangularWithShapeEmb18()
		{
			Point3d p1 = new Point3d(1266.5865523, -967.1664937, 0.0000000); 
			Point3d p2 = new Point3d(2576.8728933, -1055.7319683, 0.0000000); 
			Point3d p3 = new Point3d(1460.0421667, -1674.8067673, 0.0000000); 
			Point3d p4 = new Point3d(2570.5877856, -1725.0959362, 0.0000000); 
			Point3d p5 = new Point3d(1470.6939417, -2513.8769500, 0.0000000); 
			Point3d p6 = new Point3d(2611.3767552, -2596.4704218, 0.0000000); 
			Point3d p7 = new Point3d(1720.5015636, -1909.3000291, 0.0000000); 
			Point3d p8 = new Point3d(2235.8803933, -1972.1511059, 0.0000000); 
			Point3d p9 = new Point3d(1843.0611633, -2270.6937207, 0.0000000);
			Point3d p10 = new Point3d(2245.3080548, -2283.2639360, 0.0000000);

			Polygon3d poly1 = new Polygon3d() { p1, p2, p4, p3 };
			Polygon3d poly2 = new Polygon3d() { p3, p4, p6, p5 };
			Polygon3d embS = new Polygon3d() { p7, p8, p10, p9 };

			Shape s1 = new Shape(poly1);
			Shape s2 = new Shape(poly2);
			Shape embShape = new Shape(embS);

			List<Shape> shapes = new List<Shape>() { s1, s2 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(250);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s2, new GeometryBase[] { embShape } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("2 shape. emb: 1 shape HashTest")]
		public void RectangularWithShapeEmb19()
		{
			Polygon3d poly1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embP1 = new Polygon3d()
			{
				new Point3d(30, 30, 0),
				new Point3d(30, 50, 0),
				new Point3d(58, 60, 0),
				new Point3d(50, 10, 0)
			};

			Polygon3d poly2 = new Polygon3d()
			{
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(200, 100, 0),
				new Point3d(200, 0, 0)
			};
			Polygon3d embP2 = new Polygon3d()
			{
				new Point3d(120, 20, 0),
				new Point3d(120, 50, 0),
				new Point3d(155, 50, 0),
				new Point3d(155, 20, 0)
			};

			Shape s1 = new Shape(poly1);
			Shape s2 = new Shape(poly2);
			Shape embShape1 = new Shape(embP1);
			Shape embShape2 = new Shape(embP2);

			List<Shape> shapes = new List<Shape>() { s1, s2 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } },
				{ s2, new GeometryBase[] { embShape2 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 1 shape")]
		public void RectangularWithShapeEmb20()
		{
			Polygon3d poly1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0)
			};
			Polygon3d embP1 = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(45, 15, 0),
				new Point3d(50, 45, 0),
				new Point3d(15, 40, 0)
			};
						
			Shape s1 = new Shape(poly1);
			Shape embShape1 = new Shape(embP1);
			List<Shape> shapes = new List<Shape>() { s1 };

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(10);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1 } }
			};

			GMesh.Generate(shapes, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, shapes, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}


		[TestMethod]
		[TestCategory("1 shape. emb: 2 shape")]
		public void RectangularWithShapeEmb21()
		{

			double majorSide = 2000;
			double minorSide = 1000;
			double loadHeight1 = 800;
			double loadHeight2 = 1200;
			double loadHeight3 = 1500;

			// Arrange

			Shape s1 = base.GetRectangularPlanarShape(minorSide, majorSide, new Point3d(0, 0, 0));

			Shape embShape1 = new Shape(new Polygon3d() 
			{ 
				new Point3d(0, loadHeight1, 0),    
				new Point3d(minorSide, loadHeight1, 0),
				new Point3d(minorSide, loadHeight2, 0),
				new Point3d(0, loadHeight2, 0) 
			});
			Shape embShape2 = new Shape(new Polygon3d() 
			{ 
				new Point3d(0, loadHeight1, 0),
				new Point3d(minorSide, loadHeight1, 0),
				new Point3d(minorSide, loadHeight3, 0),
				new Point3d(0, loadHeight3, 0) 
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 shape")]
		public void RectangularWithShapeEmb22()
		{
			double majorSide = 2000;
			double minorSide = 1000;

			// Arrange            
			Shape s1 = base.GetRectangularPlanarShape(minorSide, majorSide, new Point3d(0, 0, 0));

			Shape embShape1 = new Shape(new Polygon3d() 
			{ 
				new Point3d(0, 1000, 0),
				new Point3d(minorSide, 1000, 0),
				new Point3d(minorSide, 1200, 0),
				new Point3d(0, 1200, 0) 
			});
			Shape embShape2 = new Shape(new Polygon3d() 
			{ 
				new Point3d(0, 800, 0),
				new Point3d(minorSide, 800, 0),
				new Point3d(minorSide, 1400, 0),
				new Point3d(0, 1400, 0) 
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(200);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);

			// September 2026: the faces of the embedded shapes were mapped to the id of the previous face: the ids are one more
			int[] embShape1Faces = new int[] { 36, 37, 38, 39, 40 };
			int[] embShape2Faces = new int[] { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50 };

			int[] outFace1 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[0]].ElementAt(0).Value;
			int[] outFace2 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[0]].ElementAt(1).Value;

			for(int i = 0; i < embShape1Faces.Count(); i++)
				Assert.IsTrue(outFace1.Contains(embShape1Faces[i]));
			for (int i = 0; i < embShape2Faces.Count(); i++)
				Assert.IsTrue(outFace2.Contains(embShape2Faces[i]));
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 3 shape")]
		public void RectangularWithShapeEmb23()
		{
			double majorSide = 2000;
			double minorSide = 1000;

			// Arrange            
			Shape s1 = base.GetRectangularPlanarShape(minorSide, majorSide, new Point3d(0, 0, 0));

			Shape embShape1 = new Shape(new Polygon3d()
			{ 
				new Point3d(0, 1000, 0),
				new Point3d(minorSide, 1000, 0),
				new Point3d(minorSide, 1200, 0),
				new Point3d(0, 1200, 0) 
			});
			Shape embShape2 = new Shape(new Polygon3d() 
			{ 
				new Point3d(0, 800, 0),
				new Point3d(minorSide, 800, 0),
				new Point3d(minorSide, 1400, 0),
				new Point3d(0, 1400, 0) 
			});
			Shape embShape3 = new Shape(new Polygon3d() 
			{ 
				new Point3d(200, 900, 0),
				new Point3d(200, 1300, 0),
				new Point3d(800, 1300, 0),
				new Point3d(800, 900, 0)
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2, embShape3 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shape")]
		public void RectangularWithShapeEmb24()
		{
			// Arrange            
			Shape s1 = base.GetRectangularPlanarShape(1000, 1000, new Point3d(0, 0, 0));

			Shape embShape1 = new Shape(new Polygon3d() 
			{ 
				new Point3d(200, 200, 0),
				new Point3d(600, 200, 0),
				new Point3d(600, 600, 0),
				new Point3d(200, 600, 0) 
			});

			Shape embShape2 = new Shape(new Polygon3d()
			{ 
				new Point3d(200, 400, 0),
				new Point3d(200, 800, 0),
				new Point3d(600, 800, 0),
				new Point3d(600, 400, 0) 
			});

			Shape embShape3 = new Shape(new Polygon3d() 
			{ 
				new Point3d(400, 300, 0),
				new Point3d(400, 700, 0),
				new Point3d(900, 700, 0),
				new Point3d(900, 300, 0)
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2, embShape3 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 4 shape")]
		public void Shape2DWithShapeEmb1()
		{
			// Arrange            
			Shape s1 = base.GetRectangularPlanarShape2d(1000, 1000, new Point2d(0, 0));

			Shape2d embShape1 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(200, 200),
				new Point2d(600, 200),
				new Point2d(600, 600),
				new Point2d(200, 600) 
			});

			Shape2d embShape2 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(200, 400),
				new Point2d(200, 800),
				new Point2d(600, 800),
				new Point2d(600, 400) 
			});

			Shape2d embShape3 = new Shape2d(new Polygon2d()
			{ 
				new Point2d(400, 300),
				new Point2d(400, 700),
				new Point2d(900, 700),
				new Point2d(900, 300)
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2, embShape3 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 shape")]
		public void Shape2DWithShapeEmb2()
		{
			double majorSide = 2000;
			double minorSide = 1000;

			Shape2d s1 = base.GetRectangularPlanarShape2d(minorSide, majorSide, new Point2d(0, 0));
			Shape2d embShape1 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(0, 1000),
				new Point2d(minorSide, 1000),
				new Point2d(minorSide, 1200),
				new Point2d(0, 1200) 
			});
			Shape2d embShape2 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(0, 800),
				new Point2d(minorSide, 800),
				new Point2d(minorSide, 1400),
				new Point2d(0, 1400) 
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(200);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);

			// September 2026: the faces of the embedded shapes were mapped to the id of the previous face: the ids are one more
			int[] embShape1Faces = new int[] { 36, 37, 38, 39, 40 };
			int[] embShape2Faces = new int[] { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50 };

			int[] outFace1 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[0]].ElementAt(0).Value;
			int[] outFace2 = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes[0]].ElementAt(1).Value;

			for (int i = 0; i < outFace1.Count(); i++)
				Assert.IsTrue(outFace1[i] == embShape1Faces[i]);
			for (int i = 0; i < outFace2.Count(); i++)
				Assert.IsTrue(outFace2[i] == embShape2Faces[i]);
		}

		[TestMethod]
		[TestCategory("1 shape. emb: 2 shape")]
		public void Shape2DWithShapeEmb3()
		{
			double majorSide = 2000;
			double minorSide = 1000;
			double loadHeight1 = 800;
			double loadHeight2 = 1200;
			double loadHeight3 = 1500;

			Shape2d s1 = base.GetRectangularPlanarShape2d(minorSide, majorSide, new Point2d(0, 0));
			Shape2d embShape1 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(0, loadHeight1),
				new Point2d(minorSide, loadHeight1),
				new Point2d(minorSide, loadHeight2),
				new Point2d(0, loadHeight2) 
			});
			Shape2d embShape2 = new Shape2d(new Polygon2d() 
			{ 
				new Point2d(0, loadHeight1),
				new Point2d(minorSide, loadHeight1),
				new Point2d(minorSide, loadHeight3),
				new Point2d(0, loadHeight3) 
			});

			GMesh.GMeshGenerateOptions options = GetGMeshGenerateOptions(100);

			Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>
			{
				{ s1, new GeometryBase[] { embShape1, embShape2 } }
			};

			GMesh.Generate(new[] { s1 }, embeddedGeometries, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), meshes);
			CommonGMeshAssert(meshes, new[] { s1 }, embeddedGeometries, options.MeshSize, options.MeshSize, generateMeshStatus);
		}
	}
}
