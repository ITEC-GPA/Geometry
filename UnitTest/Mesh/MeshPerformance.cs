using GPC.Geometry;
using GPC.Geometry.Meshes;
using Maffeis.TestUtilities;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Meshes
{
	[TestClass]
	public class MeshPerformance : UnitTestBase
	{
		private static Mesh mesh1;
		private static Mesh mesh2;

		private Mesh CreateSimpleMesh3(int faceNumberX, int faceNumberY, double pitchX, double pitchY, Point2d startPoint)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < faceNumberX; i++)
			{
				for (int j = 0; j < faceNumberY; j++)
				{
					mesh.AddFaceMesh(new[] {
						new MeshVertex(new Point3d(startPoint.X + pitchX * i , startPoint.Y + pitchY * j , 0)),
						new MeshVertex(new Point3d(startPoint.X + pitchX * i + pitchX ,startPoint.Y + pitchY * j , 0)),
						new MeshVertex(new Point3d(startPoint.X + pitchX * i + pitchX ,startPoint.Y + pitchY * j + pitchY , 0)),
						new MeshVertex(new Point3d(startPoint.X + pitchX * i, startPoint.Y + pitchY * j + pitchY , 0))
					});
				}
			}
			return mesh;
		}


		private void FunctionToTest2()
		{
			mesh1.GetHashCode().Equals(mesh2.GetHashCode());
		}


		[TestMethod]
		public void MeshEquals1()
		{
			mesh1 = CreateSimpleMesh3(20, 40, 10, 10, Point2d.Origin);
			mesh2 = CreateSimpleMesh3(20, 40, 10, 10, Point2d.Origin);

			Action ac1 = new Action(() =>
			{
				mesh1.Equals(mesh2);
			});

			var bb0 = MeasureTime.FunctionExecutionTime(3, ac1, true);
		}


		[TestMethod]
		public void MeshHashCode1()
		{
			mesh1 = CreateSimpleMesh3(20, 40, 10, 10, Point2d.Origin);
			mesh2 = CreateSimpleMesh3(20, 40, 10, 10, Point2d.Origin);

			Debug.WriteLine("Start");
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			mesh1.GetHashCode().Equals(mesh2.GetHashCode());
			stopWatch.Stop();
			Debug.WriteLine(stopWatch.Elapsed, "Elapsed time");
			Debug.WriteLine("Finished");
			Assert.IsTrue(stopWatch.ElapsedMilliseconds <= 2, "Too slow");
		}


		[TestMethod]
		public void Test1()
		{
			MeshBaseCollection<MeshVertex> col = new MeshBaseCollection<MeshVertex>();

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			for (int i = 0; i < 10000; i++)
			{
				col.Add(new MeshVertex(new Point3d(0, 0, i)));
			}
			stopWatch.Stop();
			Debug.WriteLine(stopWatch.Elapsed, "Build of 10000 MeshVertex. Elapsed time");

			col.Clear();
			stopWatch.Restart();
			for (int i = 0; i < 10000; i++)
			{
				col.Add(new MeshVertex(new Point3d(0, 0, i)));
			}
			stopWatch.Stop();
			Debug.WriteLine(stopWatch.Elapsed, "Add of 10000 MeshVertex. Elapsed time");

			stopWatch.Restart();
			col.Remove(new MeshVertex(new Point3d(0, 0, 9999)));
			stopWatch.Stop();
			Debug.WriteLine(stopWatch.Elapsed, "Removed a MeshVertex by ref. Elapsed time");

			stopWatch.Restart();
			col.Remove(9991);
			stopWatch.Stop();
			Debug.WriteLine(stopWatch.Elapsed, "Removed a MeshVertex by index. Elapsed time");
		}

		[TestMethod]
		public void Clone()
		{
			Mesh mesh = CreateSimpleMesh3(40, 40, 25, 60, new Point2d(0, 0));
			Debug.WriteLine($"Starting clone test of a mesh of {mesh.VerticesCount} vertices");

			// Exclude first-use JIT cost and a single scheduling/GC pause, while
			// retaining the existing 5 ms budget and checking every returned clone.
			Assert.IsTrue(mesh.Equals((Mesh)mesh.Clone()));
			var samples = new double[7];
			for (int i = 0; i < samples.Length; i++)
			{
				Stopwatch stopWatch = Stopwatch.StartNew();
				Mesh copy = (Mesh)mesh.Clone();
				stopWatch.Stop();
				samples[i] = stopWatch.Elapsed.TotalMilliseconds;
				Assert.IsTrue(mesh.Equals(copy));
				Assert.AreNotSame(mesh, copy);
			}
			Array.Sort(samples);
			double median = samples[samples.Length / 2];
			Assert.IsTrue(median <= 5, $"Too slow, median {median:F3} ms");
		}

		[TestMethod]
		public void ExtrudeFaces()
		{
			// Partito ad ottimizzare da: 00:00:13.8081043
			// 19/05/2021: 165 ms 

			Mesh mesh = CreateSimpleMesh3(40, 40, 25, 60, new Point2d(0, 0));
			Vector3d v = new Vector3d(0, 100, 0);

			Debug.WriteLine($"Starting extrude test of a mesh of {mesh.VerticesCount} vertices");

			Action ac1 = new Action(() =>
			{
				mesh.ExtrudeFaces(v);
			});

			var time = MeasureTime.FunctionExecutionTime(10, ac1, true);

			Assert.IsTrue(time <= 200, "Too slow");
		}
	}
}
