using GPC.Geometry;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Geometry
{
	[TestClass]
	public class ShapeTest : UnitTestBase
	{
		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area1()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1, new[] { h1 });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area2()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1, new[] { h1 });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area3()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1.Reverse(), new[] { h1.Reverse() });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area4()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1.Reverse(), new[] { h1.Reverse() });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area5()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1, new[] { h1.Reverse() });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Area test. Check if the shape have the calculated area
		/// </summary>
		[TestMethod]
		public void Area6()
		{
			//Expected
			double expectedArea = 300;
			double tollerance = 0.005;

			//Arrange
			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};
			Polygon3d h1 = new Polygon3d()
			{
				new Point3d(5.0, 3.54, 3.54),
				new Point3d(15.0, 3.54, 3.54),
				new Point3d(15.00, 10.61, 10.61),
				new Point3d(5.00, 10.61, 10.61)
			};

			//Act 
			Shape s1 = new Shape(f1.Reverse(), new[] { h1 });
			double area = s1.GetArea();

			//Assert
			double difference = (area - expectedArea) / area;

			string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
			Assert.IsTrue(difference < tollerance, message);

			Console.Write(message);
		}

		/// <summary>
		/// Coordinate system test. Check if the polygon have the correct direction
		/// </summary>
		[TestMethod]
		public void CoordinateSystem()
		{
			//Arrange
			double tollerance = 0.001;

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(-9.10,-8.43,0.00),
				new Point3d(-1.09,-2.61,0.00),
				new Point3d(-4.15,6.81,0.00),
				new Point3d(-14.05,6.81,0.00),
				new Point3d(-17.11,-2.61,0.00)
			};

			//Act 
			Shape s1 = new Shape(f1);

			var cs = s1.GetCoordinateSystem();

			//Assert

			Vector3d v11Expected = new Vector3d(16.02, 11.64, 0.00);
			v11Expected.Unitize();

			Vector3d v22Expected = new Vector3d(-5.54, 7.62, 0.00);
			v22Expected.Unitize();

			Vector3d v33Expected = new Vector3d(0, 0, 1);

			Assert.IsTrue(v11Expected.DotProduct(cs.V1) - 1 < tollerance);
			Assert.IsTrue(v22Expected.DotProduct(cs.V2) - 1 < tollerance);
			Assert.IsTrue(v33Expected.DotProduct(cs.V3) - 1 < tollerance);
		}

		/// <summary>
		/// Normal vector Test. Check if the shape have the correct normal.
		/// </summary>
		[TestMethod]
		public void NormalTest()
		{
			// Arrange 
			Vector3d expected = new Vector3d(0, -0.7071, +0.7071);

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(20, 0, 0),
				new Point3d(20, 14.14, 14.14),
				new Point3d(0, 14.14, 14.14)
			};

			//Act 
			Shape s1 = new Shape(f1);
			Vector3d v = s1.GetNormalVector();
			v.Unitize();

			//Assert
			Vector3d diff = expected - v;

			Console.WriteLine(v);
			Console.WriteLine(diff);
			Assert.IsTrue(Math.Abs(diff.X) < 0.001 && Math.Abs(diff.Y) < 0.001 && Math.Abs(diff.Z) < 0.001);
		}

		/// <summary>
		/// Normal vector Test. Check if the shape have the correct normal.
		/// </summary>
		[TestMethod]
		public void NormalTest2()
		{
			// Arrange 

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(20, 0, 0)
			};

			//Act 
			try
			{
				Shape s1 = new Shape(f1);
				Vector3d v = s1.GetNormalVector();
			}
			catch (NotSupportedException e)
			{
				// sono punti allineati, dovrebbe andare in eccezione 
				Console.WriteLine(e);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			// Assert.Fail("Exception not raised");
		}

		/// <summary>
		/// Normal vector Test. Check if the shape have the correct normal.
		/// </summary>
		[TestMethod]
		public void NormalTest3()
		{
			// Arrange 
			Vector3d expected = new Vector3d(0, 0, 1);

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0,0,0),
				new Point3d(10,10,0),
				new Point3d(0,10,0),
			};

			//Act 
			Shape s1 = new Shape(f1);
			Vector3d v = s1.GetNormalVector();
			v.Unitize();


			Vector3d diff = expected - v;

			Console.WriteLine(v);
			Console.WriteLine(diff);
			Assert.IsTrue(Math.Abs(diff.X) < 0.001 && Math.Abs(diff.Y) < 0.001 && Math.Abs(diff.Z) < 0.001);
		}

		/// <summary>
		/// Normal vector Test. Check if the shape have the correct normal.
		/// </summary>
		[TestMethod]
		public void NormalTest4()
		{
			// Arrange 
			Vector3d expected = new Vector3d(0, 0, -1);

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0,0,0),
				new Point3d(0,10,0),
				new Point3d(10,10,0),
			};

			//Act 
			Shape s1 = new Shape(f1);
			Vector3d v = s1.GetNormalVector();
			v.Unitize();

			//Assert
			Vector3d diff = expected - v;

			Console.WriteLine(v);
			Console.WriteLine(diff);
			Assert.IsTrue(Math.Abs(diff.X) < 0.001 && Math.Abs(diff.Y) < 0.001 && Math.Abs(diff.Z) < 0.001);
		}

		/// <summary>
		/// Normal vector Test. Check if the shape have the correct normal.
		/// </summary>
		[TestMethod]
		public void NormalTest5()
		{
			// Arrange 
			Vector3d expected = new Vector3d(1, 0, 0);

			Polygon3d f1 = new Polygon3d()
			{
				new Point3d(0,0,10),
				new Point3d(0,10,0),
				new Point3d(0,10,5),
			};

			//Act 
			Shape s1 = new Shape(f1);
			Vector3d v = s1.GetNormalVector();
			v.Unitize();

			//Assert
			Vector3d diff = expected - v;

			Console.WriteLine(v);
			Console.WriteLine(diff);
			Assert.IsTrue(Math.Abs(diff.X) < 0.001 && Math.Abs(diff.Y) < 0.001 && Math.Abs(diff.Z) < 0.001);
		}

		/// <summary>
		/// Normal vector and flatness Test. Check if the shape is flatness and have the correct normal.
		/// </summary>
		[TestMethod]
		public void IsPlanar1()
		{
			Vector3d expected = new Vector3d(0, 0, -1);

			// Arrange 
			Polygon3d p1 = new Polygon3d()
				{
				new Point3d(-37.36,-12.08,0.00 ),
				new Point3d(-21.21,-21.99,0.00),
				new Point3d( -10.85,9.25,0.00),
				new Point3d(-30.66,12.75,0.00),
				new Point3d(-34.92,2.24,0.00),
				new Point3d(-35.95,-3.82,0.00),
				};

			//Act 
			Shape s1 = new Shape(p1);
			Vector3d v = s1.GetNormalVector();
			v.Unitize();

			//Assert
			double dot = v.DotProduct(expected);

			Console.WriteLine(v);
			Console.WriteLine(dot);
			Assert.IsTrue(dot < 0.001, dot.ToString());

		}

		/// <summary>
		/// Flatness test of the shape. it doesn't go into exception if all points are on the same plane
		/// </summary>
		[TestMethod]
		public void IsPlanar2()
		{
			//Arrange
			try
			{
				Polygon3d p1 = new Polygon3d()
				{
				new Point3d(2.5, 15, 1 ),
				new Point3d(1.1, 12.3, 5),
				new Point3d(6, 2, 3.2),
				new Point3d(1.83219, 5,8),
				new Point3d(8.778534, 0, 0)
				};
				Shape s1 = new Shape(p1);
			}
			catch (ArgumentException)
			{
				// se viene aggiunto un punto non planare va in argumentException();
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		/// <summary>
		/// Flatness test of the shape. it doesn't go into exception if all points are on the same plane
		/// </summary>
		[TestMethod]
		public void IsPlanar3()
		{
			//Arrange
			try
			{
				Polygon3d p1 = new Polygon3d()
				{
				new Point3d(2.5, 15, 1 ),
				new Point3d(1.1, 12.3, 5),
				new Point3d(6, 2, 3.2),
				new Point3d(1.83219, 5,8),
				new Point3d(0, 0, 0)
				};
				Shape s1 = new Shape(p1);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("è giusto che fallisca il test");
				// se viene aggiunto un punto non planare va in argumentException();
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		/// <summary>
		/// Flatness test of the shape. it doesn't go into exception if all points are on the same plane
		/// </summary>
		[TestMethod]
		public void IsPlanar4()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
				{
				new Point3d(2, 5, 1),
				new Point3d(3, 2, 3),
				new Point3d(0, 5, 4),
				new Point3d(-0.11111, 0, 10),
				new Point3d(1.83219, -0.784245, 8),
				new Point3d(2, 5, 1.000001),
				};

			//Act
			int pointCount = p1.Count;
			p1.RemoveAlignedPoints();
			p1.RemoveDuplicatedPoints();
			int pointCountAfterRemove = p1.Count;

			for (int i = 0; i < pointCountAfterRemove; i++)
			{
				Console.WriteLine($"{p1[i]}");
			}

			//Assert;
			Assert.IsTrue(pointCount == 6);
			Assert.IsTrue(pointCountAfterRemove == 5);
		}

		/// <summary>
		/// Flatness test of the shape. it doesn't go into exception if all points are on the same plane
		/// </summary>
		[TestMethod]
		public void IsPlanar5()
		{
			//Arrange
			try
			{
				Polygon3d p1 = new Polygon3d()
				{
				new Point3d(0, 0, 0),
				new Point3d(5, 505, -5410),
				new Point3d(1651, 0, -15610),
				new Point3d(1, 1, 0),
				new Point3d(0, 1, 55555),
				};
				Shape s1 = new Shape(p1);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("è giusto che fallisca il test");
				// se viene aggiunto un punto non planare va in argumentException();
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		/// <summary>
		/// Flatness test of the shape. it doesn't go into exception if all points are on the same plane
		/// </summary>
		[TestMethod]
		public void IsPlanar6()
		{
			//Arrange
			try
			{
				Polygon3d p1 = new Polygon3d()
				{
				new Point3d(0, 0, 0),
				new Point3d(0, 0, 0),
				new Point3d(0, 0, 0),
				new Point3d(1, 1, 0),
				new Point3d(0, 1, 0),
				new Point3d(0, 0, 0),
				};
				Shape s1 = new Shape(p1);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("è giusto che fallisca il test");
				// se viene aggiunto un punto non planare va in argumentException();
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape1()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
				{
				new Point3d(2, 5, 1),
				new Point3d(3, 2, 3),
				new Point3d(0, 5, 4),
				};
			Shape s1 = new Shape(p1);

			//Act
			Point3d point1 = new Point3d(-0.11111, 0, 10);          // punto esterno
			Point3d point2 = new Point3d(1.83219, -0.784245, 8);    // punto esterno
			Point3d point3 = new Point3d(50, 20, 10);               // punto esterno
			Point3d point4 = new Point3d(-5, -51, 8);               // punto esterno

			//Assert;
			Assert.IsFalse(s1.IsPointInside(point1));
			Assert.IsFalse(s1.IsPointInside(point2));
			Assert.IsFalse(s1.IsPointInside(point3));
			Assert.IsFalse(s1.IsPointInside(point4));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape2()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(2, 5, 1),
				new Point3d(3, 2, 3),
				new Point3d(0, 5, 4),
			};

			Shape s1 = new Shape(p1);

			//Act
			Point3d point1 = new Point3d(1.6666666, 4.0000000, 2.6666666);     // punto interno
			Point3d point2 = new Point3d(0.6250000, 4.6250000, 3.5000000);     // punto interno
			Point3d point3 = new Point3d(1.2500000, 4.2500000, 3.0000000);     // punto interno
			Point3d point4 = new Point3d(1.8750000, 4.6250000, 1.6250000);     // punto interno
			Point3d point5 = new Point3d(1.7500000, 4.2500000, 2.2500000);     // punto interno
			Point3d point6 = new Point3d(2.5000000, 2.7500000, 2.8750000);     // punto interno
			Point3d point7 = new Point3d(2.0000000, 3.5000000, 2.7500000);     // punto interno
			Point3d point8 = new Point3d(1.5000000, 4.2500000, 2.6250000);     // punto interno
			Point3d point9 = new Point3d(1.6250000, 3.8750000, 2.8750000);     // punto interno
			Point3d point10 = new Point3d(1.8750000, 3.8750000, 2.5000000);    // punto interno
			Point3d point11 = new Point3d(0, 0, 0);            // punto esterno
			Point3d point13 = new Point3d(2, 2, 2);            // punto esterno
			Point3d point14 = new Point3d(5, 5, 5);            // punto esterno
			Point3d point12 = new Point3d(1, 1, 1);            // punto esterno


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1));
			Assert.IsTrue(s1.IsPointInside(point2));
			Assert.IsTrue(s1.IsPointInside(point3));
			Assert.IsTrue(s1.IsPointInside(point4));
			Assert.IsTrue(s1.IsPointInside(point5));
			Assert.IsTrue(s1.IsPointInside(point6));
			Assert.IsTrue(s1.IsPointInside(point7));
			Assert.IsTrue(s1.IsPointInside(point8));
			Assert.IsTrue(s1.IsPointInside(point9));
			Assert.IsTrue(s1.IsPointInside(point10));
			Assert.IsFalse(s1.IsPointInside(point11));
			Assert.IsFalse(s1.IsPointInside(point12));
			Assert.IsFalse(s1.IsPointInside(point13));
			Assert.IsFalse(s1.IsPointInside(point14));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape4()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
				{
				new Point3d(2, 5, 1),
				new Point3d(3, 2, 3),
				new Point3d(0, 5, 4),
				};

			Shape s1 = new Shape(p1, null, null);

			//Act
			Point3d point1 = new Point3d(1.5, 3.5, 3.5);        // punto medio del lato
			Point3d point2 = new Point3d(2.5, 3.5, 2);          // punto medio del lato
			Point3d point3 = new Point3d(1, 5, 2.5);            // punto medio del lato


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1));
			Assert.IsTrue(s1.IsPointInside(point2));
			Assert.IsTrue(s1.IsPointInside(point3));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape5()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(0, 10, 10),
				new Point3d(0, 10, 20),
				new Point3d(0, 25, 10),
				new Point3d(0, 20, 5),
			};

			Shape s1 = new Shape(p1, null, null);

			//Act
			Point3d point1 = new Point3d(0, 3.5, 3.5);          // punto su un lato
			Point3d point2 = new Point3d(0, 20, 3);             // punto sul piano ma esterno
			Point3d point3 = new Point3d(5, 20, 25);            // punto non sul piano
			Point3d point4 = new Point3d(0, 25, 10);            // punto su uno spigolo
			Point3d point5 = new Point3d(0, 25, 11);            // punto sul piano ma esterno   
			Point3d point6 = new Point3d(0, 8.4, 5.6);          // punto interno


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1), ("Error: point1"));
			Assert.IsFalse(s1.IsPointInside(point2), ("Error: point2"));
			Assert.IsFalse(s1.IsPointInside(point3), ("Error: point3"));
			Assert.IsTrue(s1.IsPointInside(point4), ("Error: point4"));
			Assert.IsFalse(s1.IsPointInside(point5), ("Error: point5"));
			Assert.IsTrue(s1.IsPointInside(point6), ("Error: point6"));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape6()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(2, 2, 1),
				new Point3d(4, 2, 1),
				new Point3d(4, 4, 1),
				new Point3d(6, 6, 1),
				new Point3d(2, 6, 1),
			};

			Shape s1 = new Shape(p1, null, null);

			//Act
			Point3d point1 = new Point3d(2, 2, 1);            // punto su un vertice
			Point3d point2 = new Point3d(6, 6, 1);            // punto su un vertice
			Point3d point3 = new Point3d(3, 4, 1);            // punto interno
			Point3d point4 = new Point3d(3, 5, 1);            // punto interno
			Point3d point5 = new Point3d(4, 6, 1);            // punto sul un bordo
			Point3d point6 = new Point3d(3, 3, 1);            // punto DA TESTARE


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1), ("Error: point1"));
			Assert.IsTrue(s1.IsPointInside(point2), ("Error: point2"));
			Assert.IsTrue(s1.IsPointInside(point3), ("Error: point3"));
			Assert.IsTrue(s1.IsPointInside(point4), ("Error: point4"));
			Assert.IsTrue(s1.IsPointInside(point5), ("Error: point5"));
			Assert.IsTrue(s1.IsPointInside(point6), ("Error: point6"));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape7()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(2,2,10),
				new Point3d(8,2,10),
				new Point3d(5,5,10),
				new Point3d(8,8,10),
				new Point3d(2,8,10),
			};

			Shape s1 = new Shape(p1, null, null);

			//Act
			Point3d point1 = new Point3d(2, 2, 10);            // punto su un vertice
			Point3d point2 = new Point3d(5, 5, 10);            // punto su un vertice
			Point3d point3 = new Point3d(3, 3, 10);            // punto DA TESTARE
			Point3d point4 = new Point3d(4, 4, 10);            // punto DA TESTARE
			Point3d point5 = new Point3d(4, 6, 10);            // punto DA TESTARE
			Point3d point6 = new Point3d(3, 7, 10);            // punto DA TESTARE


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1), ("Error: point1"));
			Assert.IsTrue(s1.IsPointInside(point2), ("Error: point2"));
			Assert.IsTrue(s1.IsPointInside(point3), ("Error: point3"));
			Assert.IsTrue(s1.IsPointInside(point4), ("Error: point4"));
			Assert.IsTrue(s1.IsPointInside(point5), ("Error: point5"));
			Assert.IsTrue(s1.IsPointInside(point6), ("Error: point6"));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShape8()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(2,2,10),
				new Point3d(-2,2,10),
				new Point3d(-2,-2,10),
				new Point3d(2,-2,10),
			};

			Shape s1 = new Shape(p1, null, null);

			//Act
			Point3d point1 = new Point3d(2, 2, 10);            // punto su un vertice
			Point3d point2 = new Point3d(1, 1, 10);            // punto su un vertice
			Point3d point3 = new Point3d(-1, -1, 10);          // punto DA TESTARE
			Point3d point4 = new Point3d(0, 0, 10);            // punto DA TESTARE
			Point3d point5 = new Point3d(0, 1, 10);            // punto DA TESTARE
			Point3d point6 = new Point3d(1, 0, 10);            // punto DA TESTARE


			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1), ("Error: point1"));
			Assert.IsTrue(s1.IsPointInside(point2), ("Error: point2"));
			Assert.IsTrue(s1.IsPointInside(point3), ("Error: point3"));
			Assert.IsTrue(s1.IsPointInside(point4), ("Error: point4"));
			Assert.IsTrue(s1.IsPointInside(point5), ("Error: point5"));
			Assert.IsTrue(s1.IsPointInside(point6), ("Error: point6"));
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShapeHole1()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()                                          // creo fill
			{
				new Point3d(   -15.7736144,    30.1516590,     0.0000000),
				new Point3d(   19.9241001,     1.7448179,      -6.8940675),
				new Point3d(   -0.3511055,     -15.9228705,    14.8165318),
				new Point3d(   2.0752429,      15.9482384,     -3.4470338),        // Sono i punti medi dei lati.
				new Point3d(   9.7864973,      -7.0890263,     3.9612322),         // Sono punti di Rhino ottenuti con il massimo di cifre decimali (7)
				new Point3d(   -8.0623600,     7.1143943,      7.4082659),         // deve rimuoverli
			};

			Polygon3d h1 = new Polygon3d                                          // creo hole
			{
				new Point3d(5.9308701,      4.4296061,      0.2570992),
				new Point3d(0.8620687,      0.0126840,      5.6847490),
				new Point3d(-2.9935585,     11.5313164,     1.9806161),
				new Point3d(1.4686558,      7.9804612,      1.1188576),         // punto medio hole
				new Point3d(3.3964694,      2.2211450,      2.9709241),         //
				new Point3d(-1.0657449,     5.7720002,      3.8326826),         // deve rimuovere tutti e 3 i punti
			};

			Shape s1 = new Shape(p1, new Polygon3d[1] { h1 }, null);             // NOTA: SONO TUTTI PUNTI APPARTENTENTI AL PIANO DELLA SHAPE

			//Act
			Point3d point1 = new Point3d(-15.7736144, 30.1516590, 0.0000000);     // vertice shape
			Point3d point2 = new Point3d(19.9241001, 1.7448179, -6.8940675);    // vertice shape
			Point3d point3 = new Point3d(-0.3511055, -15.9228705, 14.8165318);    // vertice shape
			Point3d point4 = new Point3d(5.9308701, 4.4296061, 0.2570992);     // vertice hole
			Point3d point5 = new Point3d(0.8620687, 0.0126840, 5.6847490);     // vertice hole
			Point3d point6 = new Point3d(-2.9935585, 11.5313164, 1.9806161);     // vertice hole
			Point3d point7 = new Point3d(1.4686558, 7.9804612, 1.1188576);     // vertice hole
			Point3d point8 = new Point3d(5, 5, 5);             // punto random
			Point3d point9 = new Point3d(-1.0657449, 5.7720002, 3.8326826);     // vertice hole
			Point3d point10 = new Point3d(1.2846161, 5.3210520, 2.6315429);     // punto dentro hole

			//Assert;
			Assert.IsTrue(s1.IsPointInside(point1));            // vertice shape
			Assert.IsTrue(s1.IsPointInside(point2));            // vertice shape
			Assert.IsTrue(s1.IsPointInside(point3));            // vertice shape
			Assert.IsTrue(s1.IsPointInside(point4));            // vertice hole
			Assert.IsTrue(s1.IsPointInside(point5));            // vertice hole
			Assert.IsTrue(s1.IsPointInside(point6));            // vertice hole
			Assert.IsTrue(s1.IsPointInside(point7));            // vertice hole
			Assert.IsFalse(s1.IsPointInside(point8));           // vertice hole
			Assert.IsTrue(s1.IsPointInside(point9));            // vertice hole
			Assert.IsFalse(s1.IsPointInside(point10));          // punto dentro hole
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShapeHole2()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()                                          // creo fill
			{
				new Point3d(0.0, 0.0, 10.0),
				new Point3d(10.0, 0.0, 10.0),
				new Point3d(10.0, 10.0,10.0),
				new Point3d(5.0, 15.0,10.0),
				new Point3d(0.0, 10.0,10.0),
				new Point3d(0.0, 5.0, 10.0),                          // punto allineato
			};

			Polygon3d h1 = new Polygon3d                                      // creo hole 1
			{
				new Point3d(2, 6, 10),
				new Point3d(4, 6, 10),
				new Point3d(4, 8, 10),
				new Point3d(2, 8, 10),

			};

			Polygon3d h2 = new Polygon3d                                      // creo hole 2
			{
				new Point3d(6, 6, 10),
				new Point3d(8, 6, 10),
				new Point3d(8, 8, 10),
				new Point3d(6, 8, 10),

			};

			Shape s1 = new Shape(p1, new Polygon3d[2] { h1, h2 }, null);

			//Act                   // NOTA: SONO TUTTI PUNTI APPARTENTENTI AL PIANO DELLA SHAPE TRANNE CHE IL PUNTO 9
			Point3d point1 = new Point3d(0, 0, 10);               // vertice
			Point3d point2 = new Point3d(1, 1, 10);               // Punto interno
			Point3d point3 = new Point3d(6, 0, 10);               // Punto sul lato
			Point3d point4 = new Point3d(0, 6, 10);               // Punto sul lato
			Point3d point5 = new Point3d(2, 6, 10);               // Punto bordo hole               
			Point3d point6 = new Point3d(6, 6, 10);               // Punto interno
			Point3d point7 = new Point3d(5, 13, 10);              // Punto interno
			Point3d point8 = new Point3d(-5, -5, 10);             // sul piano ma esterno
			Point3d point9 = new Point3d(5, 5, 0);                // interno ma fuori piano
			Point3d point10 = new Point3d(7, 7, 10);              // dentro hole
			Point3d point11 = new Point3d(3, 7, 10);            // dentro hole

			//Assert;                   
			Assert.IsTrue(s1.IsPointInside(point1));            // vertice
			Assert.IsTrue(s1.IsPointInside(point2));            // Punto interno
			Assert.IsTrue(s1.IsPointInside(point3));            // Punto sul lato
			Assert.IsTrue(s1.IsPointInside(point4));            // Punto sul lato
			Assert.IsTrue(s1.IsPointInside(point5));            // Punto bordo hole        
			Assert.IsTrue(s1.IsPointInside(point6));            // Punto interno
			Assert.IsTrue(s1.IsPointInside(point7));            // Punto interno
			Assert.IsFalse(s1.IsPointInside(point8));           // sul piano ma esterno
			Assert.IsFalse(s1.IsPointInside(point9));           // interno ma fuori piano
			Assert.IsFalse(s1.IsPointInside(point10));          // dentro hole
																//Assert.IsFalse(s1.IsPointInside(point11));          // dentro hole
		}

		/// <summary>
		/// Test if the input points are on shape
		/// </summary>
		[TestMethod]
		public void IsPointOnShapeHole3()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()                          // creo fill
			{
				new Point3d(0, 0, 0),
				new Point3d(10, 0, 0),
				new Point3d(10, 10, 0),
				new Point3d(-10, 10, 0),
				new Point3d(-10, -10, 0),
				new Point3d(-5, -5, 0),                            // punto allineato
			};

			Polygon3d h1 = new Polygon3d                                    // creo hole 1
			{
				new Point3d(8, 2, 0),
				new Point3d(8, 8, 0),
				new Point3d(-8, 8, 0),
				new Point3d(-8, 2, 0),

			};

			Polygon3d p2 = new Polygon3d                                    // creo shape child
			{
				new Point3d(6, 4, 0),
				new Point3d(6, 6, 0),
				new Point3d(-6, 6, 0),
				new Point3d(-6, 4, 0),

			};

			Shape s2 = new Shape(p2);
			Shape s1 = new Shape(p1, new Polygon3d[1] { h1 }, new Shape[1] { s2 });

			//Act                   
			Point3d point1 = new Point3d(0, -2, 0);
			Point3d point2 = new Point3d(0, -12, 0);
			Point3d point3 = new Point3d(4, -2, 0);
			Point3d point4 = new Point3d(4, 12, 0);
			Point3d point5 = new Point3d(6, -2, 0);
			Point3d point6 = new Point3d(6, 12, 0);

			Point3d point7 = new Point3d(-12, 2, 0);
			Point3d point8 = new Point3d(12, 2, 0);
			Point3d point9 = new Point3d(-12, 4, 0);
			Point3d point10 = new Point3d(12, 4, 0);

			Line3d line1 = new Line3d(point1, point2);
			Line3d line2 = new Line3d(point3, point4);
			Line3d line3 = new Line3d(point5, point6);

			Line3d line4 = new Line3d(point7, point8);
			Line3d line5 = new Line3d(point9, point10);

			Point3d point11 = new Point3d(-10, -10, 0);
			Point3d point12 = new Point3d(-10, 0, 0);
			Point3d point13 = new Point3d(-8, 0, 0);
			Point3d point14 = new Point3d(-2, 0, 0);
			Point3d point15 = new Point3d(-6, 6, 0);
			Point3d point16 = new Point3d(6, 6, 0);

			Point3d point17 = new Point3d(-8, 8, 0);
			Point3d point18 = new Point3d(8, 8, 0);
			Point3d point19 = new Point3d(-2, 5, 0);
			Point3d point20 = new Point3d(2, 5, 0);

			Line3d line11 = new Line3d(point11, point12);
			Line3d line12 = new Line3d(point13, point14);
			Line3d line13 = new Line3d(point15, point16);

			Line3d line14 = new Line3d(point17, point18);
			Line3d line15 = new Line3d(point19, point20);

			Point3d point21 = new Point3d(-12, -2, 0);
			Point3d point22 = new Point3d(-8, -2, 0);
			Point3d point23 = new Point3d(-12, -4, 0);
			Point3d point24 = new Point3d(0, -4, 0);
			Point3d point25 = new Point3d(7, 6, 0);
			Point3d point26 = new Point3d(12, 6, 0);

			Point3d point27 = new Point3d(-7, 6, 0);
			Point3d point28 = new Point3d(-9, 6, 0);
			Point3d point29 = new Point3d(-5, 5, 0);
			Point3d point30 = new Point3d(-7, 5, 0);

			Line3d line21 = new Line3d(point21, point22);
			Line3d line22 = new Line3d(point23, point24);
			Line3d line23 = new Line3d(point25, point26);

			Line3d line24 = new Line3d(point27, point28);
			Line3d line25 = new Line3d(point29, point30);

			//Assert;                   
			Assert.IsFalse(s1.IsLineInside(line1));
			Assert.IsFalse(s1.IsLineInside(line2));
			Assert.IsFalse(s1.IsLineInside(line3));
			Assert.IsFalse(s1.IsLineInside(line4));
			Assert.IsFalse(s1.IsLineInside(line5));

			Assert.IsTrue(s1.IsLineInside(line11));
			Assert.IsTrue(s1.IsLineInside(line12));
			Assert.IsTrue(s1.IsLineInside(line13));
			Assert.IsTrue(s1.IsLineInside(line14));
			Assert.IsTrue(s1.IsLineInside(line15));

			Assert.IsFalse(s1.IsLineInside(line21));
			Assert.IsFalse(s1.IsLineInside(line22));
			Assert.IsFalse(s1.IsLineInside(line23));
			Assert.IsFalse(s1.IsLineInside(line24));
			Assert.IsFalse(s1.IsLineInside(line25));

		}

		/// <summary>
		/// Test if the input lines are on shape
		/// </summary>
		[TestMethod]
		public void IsLineInsideShape1()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(2, 5, 1),
				new Point3d(3, 2, 3),
				new Point3d(0, 5, 4),
			};

			Shape s1 = new Shape(p1);

			//Act
			Point3d point1 = new Point3d(1.66666, 4.5, 2.666666);
			Point3d point2 = new Point3d(4, 4.5, 1);
			Point3d point3 = new Point3d(1.66666, 4.1, 2.666666);

			Line3d line1 = new Line3d(point1, point2);
			Line3d line2 = new Line3d(point2, point3);
			Line3d line3 = new Line3d(point3, point1);

			Point3d point4 = new Point3d(1.5, 3.5, 3.5);           // sono i 3 punti medi dei lati
			Point3d point5 = new Point3d(2.5, 3.5, 2);
			Point3d point6 = new Point3d(1, 5, 2.5);

			Line3d line4 = new Line3d(point4, point5);
			Line3d line5 = new Line3d(point5, point6);
			Line3d line6 = new Line3d(point6, point4);

			//Assert;
			Assert.IsFalse(s1.IsLineInside(line1));
			Assert.IsFalse(s1.IsLineInside(line2));
			Assert.IsFalse(s1.IsLineInside(line3));
			Assert.IsTrue(s1.IsLineInside(line4));
			Assert.IsTrue(s1.IsLineInside(line5));
			Assert.IsTrue(s1.IsLineInside(line6));

		}

		/// <summary>
		/// Test if the input lines are on shape
		/// </summary>
		[TestMethod]
		public void IsLineInsideShape2()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()                                          // creo fill
			{
				new Point3d(0.0, 0.0, 10.0),
				new Point3d(10.0, 0.0, 10.0),
				new Point3d(10.0, 10.0, 10.0),
				new Point3d(5.0, 15.0, 10.0),
				new Point3d(0.0, 10.0, 10.0),
				new Point3d(0.0, 5.0, 10.0),                          // punto allineato
			};

			Polygon3d h1 = new Polygon3d                                          // creo hole 1
			{
				new Point3d(2, 6, 10),
				new Point3d(4, 6, 10),
				new Point3d(4, 8, 10),
				new Point3d(2, 8, 10),

			};

			Polygon3d h2 = new Polygon3d                                          // creo hole 2
			{
				new Point3d(6, 6, 10),
				new Point3d(8, 6, 10),
				new Point3d(8, 8, 10),
				new Point3d(6, 8, 10),

			};

			Shape s1 = new Shape(p1, new Polygon3d[2] { h1, h2 }, null);

			//Act                   // NOTA: SONO TUTTI PUNTI APPARTENTENTI AL PIANO DELLA SHAPE TRANNE CHE IL PUNTO 9
			Point3d point1 = new Point3d(0, 0, 10);               // vertice
			Point3d point2 = new Point3d(1, 1, 10);               // Punto interno
			Point3d point3 = new Point3d(6, 0, 10);               // Punto sul lato
			Point3d point4 = new Point3d(0, 6, 10);               // Punto sul lato
			Point3d point5 = new Point3d(2, 6, 10);               // Punto bordo hole               
			Point3d point6 = new Point3d(6, 6, 10);               // Punto interno
			Point3d point7 = new Point3d(5, 13, 10);              // Punto interno
			Point3d point8 = new Point3d(-5, -5, 10);             // sul piano ma esterno
			Point3d point9 = new Point3d(5, 5, 0);                // interno ma fuori piano
			Point3d point10 = new Point3d(7, 7, 10);              // dentro hole
			Point3d point11 = new Point3d(3, 7, 10);              // dentro hole

			Line3d line1 = new Line3d(point1, point2);
			Line3d line2 = new Line3d(point2, point3);
			Line3d line3 = new Line3d(point3, point4);
			Line3d line4 = new Line3d(point4, point5);
			Line3d line5 = new Line3d(point5, point6);
			Line3d line6 = new Line3d(point6, point7);
			Line3d line7 = new Line3d(point7, point8);
			Line3d line8 = new Line3d(point8, point9);
			Line3d line9 = new Line3d(point9, point1);
			Line3d line10 = new Line3d(point10, point11);
			Line3d line11 = new Line3d(point11, point1);


			//Assert;                   
			Assert.IsTrue(s1.IsLineInside(line1));
			Assert.IsTrue(s1.IsLineInside(line2));
			Assert.IsTrue(s1.IsLineInside(line3));
			Assert.IsTrue(s1.IsLineInside(line4));
			Assert.IsTrue(s1.IsLineInside(line5));
			Assert.IsTrue(s1.IsLineInside(line6));
			Assert.IsFalse(s1.IsLineInside(line7));
			Assert.IsFalse(s1.IsLineInside(line8));
			Assert.IsFalse(s1.IsLineInside(line9));
			Assert.IsFalse(s1.IsLineInside(line10));
			Assert.IsFalse(s1.IsLineInside(line11));
		}

		/// <summary>
		/// Test scale
		/// </summary>
		[TestMethod]
		public void Scale()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, -10, -10),
				new Point3d(0, 10, 10),
				new Point3d(0, 10, 20),
				new Point3d(0, -25, 10),
			};

			Shape s1 = new Shape(p1, null, null);

			//Act
			double scaleFactor = 2;
			var scaled = s1.Scale(scaleFactor);
			Point3d point1 = new Point3d(0, -20, -20);
			Point3d point2 = new Point3d(0, 20, 20);
			Point3d point3 = new Point3d(0, 20, 40);
			Point3d point4 = new Point3d(0, -50, 20);

			//Assert;
			Assert.IsTrue(scaled.Fill.PointExists(point1));
			Assert.IsTrue(scaled.Fill.PointExists(point2));
			Assert.IsTrue(scaled.Fill.PointExists(point3));
			Assert.IsTrue(scaled.Fill.PointExists(point4));

			Assert.IsTrue(s1 != scaled);
		}

		[TestMethod]
		public void ToLocal()
		{
			//Arrange
			Polygon3d p1 = new Polygon3d()
			{
				new Point3d(0, -10, -10),
				new Point3d(0, 10, 10),
				new Point3d(0, 10, 20),
				new Point3d(0, -25, 10),
			};

			Shape s1 = new Shape(p1, null, null);

			var s1Local = s1.ToLocal();
		}

		[TestMethod]
		public void ChildTest1()
		{
			Polygon3d poly = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};

			Polygon3d child = new Polygon3d()
			{
				new Point3d(20, 20, 0),
				new Point3d(80, 20, 0),
				new Point3d(80, 80, 0),
				new Point3d(20, 80, 0),
			};

			Shape childShape = new Shape(child);
			Shape s1 = new Shape(poly, null, new Shape[] { childShape });

			Point3d point1 = new Point3d(50, 50, 0);
			Point3d point2 = new Point3d(30, 30, 0);
			Point3d point3 = new Point3d(60, 60, 0);
			Point3d point4 = new Point3d(50, 70, 0);

			Line3d line1 = new Line3d(point1, point2);
			Line3d line2 = new Line3d(point2, point3);
			Line3d line3 = new Line3d(point3, point4);
			Line3d line4 = new Line3d(point4, point1);

			Assert.IsTrue(s1.IsPointInside(point1));
			Assert.IsTrue(s1.IsPointInside(point2));
			Assert.IsTrue(s1.IsPointInside(point3));
			Assert.IsTrue(s1.IsPointInside(point4));
			Assert.IsTrue(s1.IsLineInside(line1));
			Assert.IsTrue(s1.IsLineInside(line2));
			Assert.IsTrue(s1.IsLineInside(line3));
			Assert.IsTrue(s1.IsLineInside(line4));
		}

		[TestMethod]
		public void ChildTest2()
		{
			Polygon3d poly = new Polygon3d()
			{
				new Point3d(0, 0, 0),
				new Point3d(100, 0, 0),
				new Point3d(100, 100, 0),
				new Point3d(0, 100, 0),
			};

			Polygon3d child = new Polygon3d()
			{
				new Point3d(40, 40, 0),
				new Point3d(60, 40, 0),
				new Point3d(60, 60, 0),
				new Point3d(40, 60, 0),
			};

			Shape childShape = new Shape(child);
			Shape s1 = new Shape(poly, null, new Shape[] { childShape });

			Point3d point1 = new Point3d(50, 50, 0);
			Point3d point2 = new Point3d(30, 30, 0);
			Point3d point3 = new Point3d(60, 60, 0);
			Point3d point4 = new Point3d(50, 70, 0);

			Line3d line1 = new Line3d(point1, point2);
			Line3d line2 = new Line3d(point2, point3);
			Line3d line3 = new Line3d(point3, point4);
			Line3d line4 = new Line3d(point4, point1);

			Assert.IsTrue(s1.IsPointInside(point1));
			Assert.IsTrue(s1.IsPointInside(point2));
			Assert.IsTrue(s1.IsPointInside(point3));
			Assert.IsTrue(s1.IsPointInside(point4));
			Assert.IsTrue(s1.IsLineInside(line1));
			Assert.IsTrue(s1.IsLineInside(line2));
			Assert.IsTrue(s1.IsLineInside(line3));
			Assert.IsTrue(s1.IsLineInside(line4));
		}
	}
}
