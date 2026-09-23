using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meshes
{
	[TestClass]
	public class InitialMeshTestGeneric : GenericMeshTest
	{
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

			Shape2d s1 = new Shape2d(p1);

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount == 2);
			Assert.IsTrue(mesh.VerticesCount == 4);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount == 6);
			Assert.IsTrue(mesh.VerticesCount == 8);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount == 10);
			Assert.IsTrue(mesh.VerticesCount == 12);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount == 8);
			Assert.IsTrue(mesh.VerticesCount == 8);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount > 8);
			Assert.IsTrue(mesh.VerticesCount == 12);
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

			InitialMesh.Generate(s1, out Mesh mesh, out InitialMesh.InitialGenerateMeshStatus generateMeshStatus);
			MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(base.GetTestName(), "msh"), mesh);
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount == 2);
			Assert.IsTrue(mesh.VerticesCount == 4);
		}
	}
}
