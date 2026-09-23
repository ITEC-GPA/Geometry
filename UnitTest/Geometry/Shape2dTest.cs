using System;
using System.Security.Cryptography;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Shape2dTest : UnitTestBase
    {
        [TestMethod]
        public void Area()
        {
            //Expected
            double expectedArea = 300;
            double tollerance = 0.005;

            //Arrange
            Polygon2d f1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d h1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)
            };

            //Act 
            Shape2d s1 = new Shape2d(f1, new[] { h1 });
            double area = s1.GetArea();

            //Assert
            double difference = (area - expectedArea) / area;

            string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
            Assert.IsTrue(difference < tollerance, message);
            Assert.IsTrue(s1.GetPoints2d().Length == 8);

            Console.Write(message);
        }

        [TestMethod]
        public void AreaWithChild()
        {
            //Expected
            double expectedArea = 336;
            double tollerance = 0.005;

            //Arrange
            Polygon2d f1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d h1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)
            };

            Polygon2d c1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(10, 5),
                new Point2d(10, 13),
                new Point2d(5, 13)
            };
            Polygon2d c1h1 = new Polygon2d()
            {
                new Point2d(7, 7),
                new Point2d(9, 7),
                new Point2d(9, 9),
                new Point2d(7, 9)
            };

            //Act 
            Shape2d s2 = new Shape2d(c1, new[] { c1h1 });
            Shape2d s1 = new Shape2d(f1, new[] { h1 }, new[] { s2 });
            double area = s1.GetArea();

            //Assert
            double difference = (area - expectedArea) / area;

            string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}.";
            Assert.IsTrue(difference < tollerance, message);

            Console.Write(message);
        }

        [TestMethod]
        public void Union()
        {
            Polygon2d f1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d h1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)
            };
            Polygon2d f2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };
            Polygon2d h2 = new Polygon2d()
            {
                new Point2d(15, 15),
                new Point2d(25, 15),
                new Point2d(25, 25),
                new Point2d(15, 25)
            };
            Shape2d s1 = new Shape2d(f1, new[] { h1 });
            Shape2d s2 = new Shape2d(f2, new[] { h2 });

            Shape2d[] resul = Shape2d.Union(s1, s2);
            Assert.IsTrue(resul.Length == 1, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Fill.Count == 8, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void Difference()
        {
            Polygon2d f1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d h1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)
            };
            Polygon2d f2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };
            Polygon2d h2 = new Polygon2d()
            {
                new Point2d(15, 15),
                new Point2d(25, 15),
                new Point2d(25, 25),
                new Point2d(15, 25)
            };
            Shape2d s1 = new Shape2d(f1, new[] { h1 });
            Shape2d s2 = new Shape2d(f2, new[] { h2 });

            Shape2d[] resul = Shape2d.Difference(s1, s2);

            Assert.IsTrue(resul.Length == 2, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Fill.Count == 10, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void Intersection()
        {
            Polygon2d f1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d h1 = new Polygon2d()
            {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)
            };
            Polygon2d f2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };
            Polygon2d h2 = new Polygon2d()
            {
                new Point2d(15, 15),
                new Point2d(25, 15),
                new Point2d(25, 25),
                new Point2d(15, 25)
            };
            Shape2d s1 = new Shape2d(f1, new[] { h1 });
            Shape2d s2 = new Shape2d(f2, new[] { h2 });

            Shape2d[] resul = Shape2d.Intersection(s1, s2);

            Assert.IsTrue(resul.Length == 1, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Fill.Count == 8, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void IsPointOnShapeHole1()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()                          // creo fill
            {
                new Point2d(  0,  0),
                new Point2d( 10,  0),
                new Point2d( 10,  10),
                new Point2d(-10,  10),
                new Point2d(-10, -10),
                new Point2d( -5,  -5),                            // punto allineato
            };

            Polygon2d h1 = new Polygon2d                                    // creo hole 1
            {
                new Point2d(    8,      2),
                new Point2d(    8,      8),
                new Point2d(   -8,      8),
                new Point2d(   -8,      2),

            };

            Polygon2d p2 = new Polygon2d                                    // creo shape child
            {
                new Point2d(    6,      4),
                new Point2d(    6,      6),
                new Point2d(   -6,      6),
                new Point2d(   -6,      4),

            };

            Shape2d s2 = new Shape2d(p2);

            Shape2d s1 = new Shape2d(p1, new Polygon2d[1] { h1 }, new Shape2d[1] { s2 });

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

        [TestMethod]
        public void IsLineInsideShape1()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(10, 10),
                new Point2d(0, 10),
            };

            Shape2d s1 = new Shape2d(p1);

            //Act
            Point2d point1 = new Point2d(-10, 0);
            Point2d point2 = new Point2d(10, 12);
            Point2d point3 = new Point2d(12, 5);

            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point1);

            Point2d point4 = new Point2d(2, 2);           
            Point2d point5 = new Point2d(10, 8);
            Point2d point6 = new Point2d(1, 5);

            Line2d line4 = new Line2d(point4, point5);
            Line2d line5 = new Line2d(point5, point6);
            Line2d line6 = new Line2d(point6, point4);

            //Assert;
            Assert.IsFalse(s1.IsLineInside(line1));
            Assert.IsFalse(s1.IsLineInside(line2));
            Assert.IsFalse(s1.IsLineInside(line3));
            Assert.IsTrue(s1.IsLineInside(line4));
            Assert.IsTrue(s1.IsLineInside(line5));
            Assert.IsTrue(s1.IsLineInside(line6));

        }

        [TestMethod]
        public void ChildTest2()
        {
            Polygon2d poly = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(100, 0),
                new Point2d(100, 100),
                new Point2d(0, 100),
            };

            Polygon2d child = new Polygon2d()
            {
                new Point2d(40, 40),
                new Point2d(60, 40),
                new Point2d(60, 60),
                new Point2d(40, 60),
            };

            Shape2d childShape = new Shape2d(child);
            Shape2d s1 = new Shape2d(poly, null, new Shape2d[] { childShape });

            Point2d point1 = new Point2d(50, 50);
            Point2d point2 = new Point2d(30, 30);
            Point2d point3 = new Point2d(60, 60);
            Point2d point4 = new Point2d(50, 70);

            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point4);
            Line2d line4 = new Line2d(point4, point1);

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
