using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using GPC.Geometry.Meshes.GMesh;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meshes
{
	[TestClass]
	public abstract class GenericMeshTest : UnitTestBase
	{
		#region Private methods

		protected Shape GetRectangularPlanarShape(double width, double height, Point3d basepoint, Polygon3d[] hole = null)
		{
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(basepoint.X, basepoint.Y, 0),
				new Point3d(basepoint.X + width, basepoint.Y, 0),
				new Point3d(basepoint.X + width, basepoint.Y + height, 0),
				new Point3d(basepoint.X, basepoint.Y + height, 0)
			};

			return new Shape(p1, hole);
		}

		protected Shape2d GetRectangularPlanarShape2d(double width, double height, Point3d basepoint, Polygon2d[] hole = null)
		{
			Polygon2d p1 = new Polygon2d()
			{
				new Point2d(basepoint.X, basepoint.Y),
				new Point2d(basepoint.X + width, basepoint.Y),
				new Point2d(basepoint.X + width, basepoint.Y + height),
				new Point2d(basepoint.X, basepoint.Y + height),
			};

			return new Shape2d(p1, hole);
		}

		protected Shape2d GetRectangularPlanarShape2d(double width, double height, Point2d basepoint)
		{
			Polygon2d p1 = new Polygon2d()
			{
				new Point2d(basepoint.X, basepoint.Y),
				new Point2d(basepoint.X + width, basepoint.Y),
				new Point2d(basepoint.X + width, basepoint.Y + height),
				new Point2d(basepoint.X, basepoint.Y + height)
			};

			return new Shape2d(p1);
		}

		protected Shape GetRectangular3dShape(Line3d baseLine, Vector3d vector)
		{
			Point3d movedStartPoint = (Point3d)baseLine.Start.Clone();
			Point3d movedEndPoint = (Point3d)baseLine.End.Clone();
			movedStartPoint.Move(vector.X, vector.Y, vector.Z);
			movedEndPoint.Move(vector.X, vector.Y, vector.Z);

			Polygon3d p1 = new Polygon3d()
			{
				baseLine.Start,
				movedStartPoint,
				movedEndPoint,
				baseLine.End
			};

			return new Shape(p1);
		}

		protected Shape GetRectangularShape(Point3d p, Vector3d vector)
		{
			Polygon3d poly = new Polygon3d()
			{
				new Point3d(p.X, p.Y, p.Z),
				new Point3d(p.X + vector.X, p.Y, p.Z ),
				new Point3d(p.X + vector.X, p.Y + vector.Y, p.Z + vector.Z),
				new Point3d(p.X, p.Y + vector.Y, p.Z + vector.Z)
			};

			return new Shape(poly, null, null);
		}

		protected Shape2d GetRectangularShape2d(Point2d p, Vector2d vector)
		{
			Polygon2d poly = new Polygon2d()
			{
				new Point2d(p.X, p.Y),
				new Point2d(p.X + vector.X, p.Y),
				new Point2d(p.X + vector.X, p.Y + vector.Y),
				new Point2d(p.X, p.Y + vector.Y)
			};

			return new Shape2d(poly, null, null);
		}

		protected GMesh.GMeshGenerateOptions GetGMeshGenerateOptions(double meshSize,
			GMesh.GMeshGenerateOptions.MeshAlgorithm meshAlgorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
			bool recombine = true,
			GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm recombinationMeshAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad)
		{
			GMesh.GMeshGenerateOptions options = new GMesh.GMeshGenerateOptions
			{
				Algorithm = meshAlgorithm,
				Recombine = recombine,
				RecombinationAlgorithm = recombinationMeshAlgorithm,
				MeshSize = meshSize,
				UseGlobalProgressID = true,
				RecombineOptimizeTopology = 5,
				HealShapes = true,
				Optimize = false,
				OptimizeIteration = 1,
				OptimizeAlgorithm = GMesh.GMeshGenerateOptions.MeshOptimize.Netgen,
				OptimizeNetgen = 0,
				Transfinite = true,
				MinQuality = 0.3,
				Refine = false,
				Smoothing = 1,
			};

			return options;
		}

		protected void CommonGMeshAssert(IEnumerable<Mesh> meshes, IEnumerable<Shape> shapes, Dictionary<Shape, GeometryBase[]> embeddedGeometries, double meshSizeMax,
			double meshSizeMin, GMesh.GMeshGenerateMeshStatus generateMeshStatus, double meshFactorQuality = 2.5, double tol = GeometryBase.Tolerance)
		{
			// PARAMETRO MESHFACTORQUALITY
			// 1 rappresenta la mesh perfetta. tutti quadrati o triangoli di lato MeshTransfinite.
			// più aumenta questo valore, peggiore è la qualità.
			// attualmente i test gestiscono il controllo su:
			// ogni area dell'elemento, che sia compresa tra un max e un min in funzione di MeshTransfinite e meshFactorQuality.
			// ogni lato dell'elemento, che sia compreso tra un max e un min in funzione di MeshTransfinite e meshFactorQuality. (più stringente rispetto al singolo elemento)
			// l'area media degli elementi della mesh, che sia compresa tra un max e un min in funzione di MeshTransfinite e meshFactorQuality.
			// il lato medio degli elementi della mesh, che sia compreso tra un max e un min in funzione di MeshTransfinite e meshFactorQuality. (più stringente rispetto al singolo elemento)

			if (generateMeshStatus.Exceptions.Count != 0)
				Assert.IsTrue(generateMeshStatus.Exceptions.Count == 0, $"{generateMeshStatus.GetLastException().Message} \n {generateMeshStatus.GetLastCustomErrorMessage()}");

			foreach (var w in generateMeshStatus.Warnings)
			{
				Console.WriteLine(w);
			}

			Assert.AreEqual(meshes.Count(), shapes.Count(), $"Number of mesh: {meshes.Count()}; number of surfaces: {shapes.Count()}");
			if (embeddedGeometries != null)
				Assert.AreEqual(embeddedGeometries.Keys.Count, generateMeshStatus.EmbeddedGeometriesVertexMap.Keys.Count,
					$"Number of embedded: {embeddedGeometries.Count}; number of generated embedded: {generateMeshStatus.EmbeddedGeometriesVertexMap.Keys.Count}");

			if (embeddedGeometries != null)
			{
				for (int i = 0; i < generateMeshStatus.EmbeddedGeometriesVertexMap.Count; i++)
				{
					Assert.AreEqual(generateMeshStatus.EmbeddedGeometriesVertexMap.ElementAt(i).Value.Count,
						embeddedGeometries.ElementAt(i).Value.Length,
						$"Embedded geometries Map array count: {generateMeshStatus.EmbeddedGeometriesVertexMap.ElementAt(i).Value.Count}; " +
						$"Embedded geometries input array count: {embeddedGeometries.ElementAt(i).Value.Length}");
				}
			}

			if (generateMeshStatus.EmbeddedGeometriesVertexMap.Count() != 0)
			{
				foreach (Mesh mesh in meshes)
				{
					if (generateMeshStatus.EmbeddedGeometriesVertexMap.ContainsKey(mesh))
					{
						for (int i = 0; i < generateMeshStatus.EmbeddedGeometriesVertexMap[mesh].Count; i++)
						{
							//int t = generateMeshStatus.EmbeddedGeometriesVertexMap[mesh].Values.ElementAt(i).Count();           // n° di elementi sulla emb
							Assert.IsTrue(generateMeshStatus.EmbeddedGeometriesVertexMap[mesh].Values.ElementAt(i).Count() != 0);
						}
					}
				}
			}

			double minimumFaceArea = meshSizeMin * meshSizeMin / (2.0 * meshFactorQuality);
			double maximumFaceArea = meshSizeMax * meshSizeMax * meshFactorQuality;
			List<double> faceAreaArray = new List<double>();
			List<double> edgeLenghtArray = new List<double>();

			int faceFailed = 0;
			int smallFaces = 0;
			int bigFaces = 0;
            // CONTROLLO CHE OGNI FACCIA ABBIA UN'AREA COMPRESA IN UN INTERVALLO GESTITO DAL PARAMETRO MASHFACTORQUALITY
            foreach (var face in meshes.First().Faces)
			{
                try
				{
					double faceArea = meshes.First().GetFaceArea(face);
					faceAreaArray.Add(faceArea);
					if (faceArea < minimumFaceArea) { smallFaces++; }
					if (faceArea > maximumFaceArea) { bigFaces++; }
				}
				catch
				{
					faceFailed++;
				}
            }
			Console.WriteLine($"Face degenerated: {faceFailed}.");

            // CONTROLLO CHE L'AREA MEDIA DELLA MESH SIA COMPRESA NELL'INTERVALLO CHE NOI VOGLIAMO SIA LA SIZE DELLA MESH
            double maxAverage = meshSizeMax * meshSizeMax * (meshFactorQuality + 1) / 2.0;
			double minAverage = meshSizeMin * meshSizeMin / 2 / ((meshFactorQuality + 1) / 2.0);
			double idelAverage = meshSizeMax * meshSizeMax;
			double faceAreaAverage = faceAreaArray.Average();

			Assert.IsTrue(faceAreaAverage < (maxAverage), $"Average Area: {(int)faceAreaAverage}, max: {(int)maxAverage}");
			Assert.IsTrue(faceAreaAverage > (minAverage), $"Average Area: {(int)faceAreaAverage}, min: {(int)minAverage}");
			Console.WriteLine($"Average Area: {Math.Round(faceAreaAverage, 1, MidpointRounding.AwayFromZero)}.");
			Console.WriteLine($"Ideal area: {Math.Round(idelAverage, 1, MidpointRounding.AwayFromZero)} max admitted: " +
				$"{Math.Round(maxAverage, 1, MidpointRounding.AwayFromZero)}, min admitted:{Math.Round(minAverage, 1, MidpointRounding.AwayFromZero)}");
			Console.WriteLine($"Face areas smaller than allowed: {smallFaces}.");
			Console.WriteLine($"Face areas bigger than allowed: {bigFaces}.");

            // CONTROLLO CHE OGNI LATO ABBIA UNA LUNGHEZZA COMPRESA IN UN INTERVALLO GESTITO DAL PARAMETRO MASHFACTORQUALITY
            double maximumEdgeLenght = meshSizeMax * meshFactorQuality;
			double minimumEdgeLenght = meshSizeMin * (1 / meshFactorQuality);

			int edgeFailed = 0;
			int smallEdges = 0;
			int bigEdges = 0;
            foreach (var edge in meshes.First().Edges)
			{
				try
				{
					double edgeLength = meshes.First().GetEdgeLength(edge);
					edgeLenghtArray.Add(edgeLength);
					if (edgeLength < minimumEdgeLenght) { smallEdges++;  }
					if (edgeLength > maximumEdgeLenght) { bigEdges++; }
                }
				catch
				{
					edgeFailed++;
				}
            }
			Console.WriteLine($"Edge degenerated: {faceFailed}.");

            // CONTROLLO CHE IL LATO MEDIO DI OGNI ELEMENTRO DELLA MESH SIA COMPRESO IN UN INTERVALLO GESTITO DAL PARAMETRO MASHFACTORQUALITY
            double EdgeLenghtAverage = edgeLenghtArray.Average();
			double diffPercent = (EdgeLenghtAverage - meshSizeMax) / EdgeLenghtAverage * 100;

			Assert.IsTrue(diffPercent < 5 * meshFactorQuality, $"Average Lenght difference to ideal lenght: " +
				$"{(int)diffPercent}%, minimum: {5 * meshFactorQuality}%");
			Console.WriteLine($"Average edge lenght: {Math.Round(EdgeLenghtAverage, 1, MidpointRounding.AwayFromZero)}");
			Console.WriteLine($"Average lenght difference to ideal lenght: {(int)diffPercent}%");
            Console.WriteLine($"Edge lengths smaller than allowed: {smallEdges}.");
            Console.WriteLine($"Edge lengths bigger than allowed: {bigEdges}.");

            Assert.IsTrue(faceFailed == 0, "Mesh has degenerated faces.");
			Assert.IsTrue(edgeFailed == 0, "Mesh has degenerated edges.");
			Assert.IsTrue(smallFaces == 0, "Mesh has faces smaller than allowed.");
			Assert.IsTrue(bigFaces == 0,   "Mesh has faces bigger than allowed."); ;
			Assert.IsTrue(smallEdges == 0, "Mesh has edges smaller than allowed.");;
			Assert.IsTrue(bigEdges == 0,   "Mesh has edges bigger than allowed.");;

            // TODO: CONTROLLO CHE RESTITUISCA I VERTICI DELLE EMBED NELLA MANIERA CORRETTA
        }

		protected void CommonDelaunayAsserts(Mesh mesh, Shape2d shape, DelaunayMesh.DelaunayGenerateMeshStatus status, double tolerance = 0.1)
		{
			Assert.IsNotNull(mesh);
			Assert.IsTrue(mesh.FacesCount > 0);
			Assert.IsTrue(mesh.VerticesCount > 2);

			double meshArea = 0;
			foreach (MeshFace face in mesh.Faces)
				meshArea += mesh.GetFaceArea(face);

			GMesh.Generate(new List<Shape> { shape }, new GMesh.GMeshGenerateOptions(), out List<Mesh> initialMeshes, out _);

			double initialMeshArea = 0;
			foreach (MeshFace face in initialMeshes[0].Faces)
				initialMeshArea += initialMeshes[0].GetFaceArea(face);

			Assert.IsTrue(Math.Abs(initialMeshArea - meshArea) / meshArea * 100 < tolerance);

			Assert.IsNull(status);
		}

        #endregion

        protected Mesh CreateSimpleMesh1()
        {
            Mesh mesh = new Mesh();
            var pointId1 = mesh.Vertices.Add(new MeshVertex(new Point3d(0, 0, 0)));
            var pointId2 = mesh.Vertices.Add(new MeshVertex(new Point3d(10, 0, 0)));
            var pointId3 = mesh.Vertices.Add(new MeshVertex(new Point3d(5, 10, 0)));
            var pointId4 = mesh.Vertices.Add(new MeshVertex(new Point3d(5, -10, 0)));

            mesh.Edges.Add(new MeshEdge(pointId1, pointId2));
            mesh.Edges.Add(new MeshEdge(pointId2, pointId3));
            mesh.Edges.Add(new MeshEdge(pointId3, pointId1));
            mesh.Edges.Add(new MeshEdge(pointId2, pointId4));
            mesh.Edges.Add(new MeshEdge(pointId4, pointId1));

            mesh.Faces.Add(new MeshFace(pointId1, pointId2, pointId3));
            mesh.Faces.Add(new MeshFace(pointId1, pointId2, pointId4));

            return mesh;
        }

        protected Mesh CreateSimpleMesh2()
        {
            return CreateSimpleMesh3(2, 2, 10, 10, Point2d.Origin);
        }

        protected Mesh CreateSimpleMesh3(int faceNumberX, int faceNumberY, double pitchX, double pitchY, Point2d startPoint)
        {
            Mesh mesh = new Mesh();
            for (int j = 0; j <= faceNumberY; j++)
            {
				for (int i = 0; i <= faceNumberX; i++)
                {
                    var pt = new Point3d(startPoint.X + pitchX * i, startPoint.Y + pitchY * j, 0);
                    mesh.Vertices.Add(new MeshVertex(pt));
                }
            }

            var jstep = (faceNumberX + 1);

            for (int j = 0; j <= faceNumberY; j++)
            {
				for (int i = 0; i <= faceNumberX; i++)
                {
                    if (i < faceNumberX)
                    {
                        mesh.Edges.Add(new MeshEdge(j * jstep + i, j * jstep + (i + 1)));
                    }

                    if (j < faceNumberY)
                    {
                        mesh.Edges.Add(new MeshEdge(j * jstep + i, (j + 1) * jstep + i));
                    }
                }
            }

            for (int j = 0; j < faceNumberY; j++)
            {
				for (int i = 0; i < faceNumberX; i++)
                {
                    mesh.Faces.Add(new MeshFace(new int[] {
                        j * jstep + i,
                        j * jstep + i + 1,
                        (j + 1) * jstep + i + 1,
                        (j + 1) * jstep + i,
                    }));
                }
            }
            return mesh;
        }

        protected Shape GetRectangularShape(double width, double height)
        {
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0,          0,      0),
                new Point3d(width,      0,      0),
                new Point3d(width, height,      0),
                new Point3d(0,     height,      0)
            };

            return new Shape(p);
        }

		protected Mesh CreateSimpleVolumeMesh1(int faceNumberX, int faceNumberY, int faceNumberZ, double pitchX, double pitchY, double pitchZ, Point3d startPoint)
		{
			Mesh mesh = new Mesh();
			for (int k = 0; k <= faceNumberZ; k++)
			{
				for (int j = 0; j <= faceNumberY; j++)
				{
					for (int i = 0; i <= faceNumberX; i++)
					{
						var pt = new Point3d(startPoint.X + pitchX * i, startPoint.Y + pitchY * j, startPoint.Z + pitchZ * k);
						mesh.Vertices.Add(new MeshVertex(pt));
					}
				}
			}

            var kstep = (faceNumberX + 1) * (faceNumberY + 1);
            var jstep = (faceNumberX + 1);

            for (int k = 0; k <= faceNumberZ; k++)
			{
				for (int j = 0; j <= faceNumberY; j++)
				{
					for (int i = 0; i <= faceNumberX; i++)
					{
						if (i < faceNumberX)
						{
							mesh.Edges.Add(new MeshEdge(k * kstep + j * jstep + i, k * kstep + j * jstep + (i + 1)));
						}

						if (j < faceNumberY)
						{
							mesh.Edges.Add(new MeshEdge(k * kstep + j * jstep + i, k * kstep + (j + 1) * jstep + i));
						}

						if (k < faceNumberZ)
						{
							mesh.Edges.Add(new MeshEdge(k * kstep + j * jstep + i, (k + 1) * kstep + j * jstep + i));
						}
					}
				}
			}

			for (int k = 0; k < faceNumberZ; k++)
			{
				for (int j = 0; j < faceNumberY; j++)
				{
					for (int i = 0; i < faceNumberX; i++)
					{
						mesh.Volumes.Add(new MeshVolume( new int[8] {
                            k * kstep + j * jstep + i,
                            k * kstep + j * jstep + i + 1,
                            k * kstep + (j + 1) * jstep + i,
                            k * kstep + (j + 1) * jstep + i + 1,
                            (k + 1) * kstep + j * jstep + i,
                            (k + 1) * kstep + j * jstep + i + 1,
                            (k + 1) * kstep + (j + 1) * jstep + i,
                            (k + 1) * kstep + (j + 1) * jstep + i + 1
                        }));
					}
			 
				} 
			}

            return mesh;
        }

    }
}
