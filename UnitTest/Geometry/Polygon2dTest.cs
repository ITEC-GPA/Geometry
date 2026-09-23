using System;
using System.Collections.Generic;
using GPC.Geometry;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class Polygon2dTest : UnitTestBase
    {
        [TestMethod]
        public void Union()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d p2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };

            Polygon2d[] resul = Polygon2d.Union(p1, p2);

            Assert.IsTrue(resul.Length == 1, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Count == 8, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void Difference()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d p2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };

            Polygon2d[] resul = Polygon2d.Difference(p1, p2);

            Assert.IsTrue(resul.Length == 1, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Count == 6, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void Intersection()
        {
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)
            };
            Polygon2d p2 = new Polygon2d()
            {
                new Point2d(10, 10),
                new Point2d(30, 10),
                new Point2d(30, 30),
                new Point2d(10, 30)
            };

            Polygon2d[] resul = Polygon2d.Intersection(p1, p2);

            Assert.IsTrue(resul.Length == 1, "Wrong number of resulting polygons");
            Assert.IsTrue(resul[0].Count == 4, "Wrong number of resulting polygon vertices");
        }

        [TestMethod]
        public void SignedArea()
        {
            //Arrange
            double expectedArea = 33927;
            double tollerance = 0.005;

            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(649.140, -346.557),
                new Point2d(334.512, -266.397),
                new Point2d(494.832, -148.161),
                new Point2d(639.112, -218.301)
            };

            //Act
            if (!p1.IsRightHandOrdered())
            {
                p1.Reverse();
            }

            double area = p1.GetSignedArea();

            //Assert
            double difference = (area - expectedArea) / area;

            string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}";
            Assert.IsTrue(difference < tollerance, message);
            Console.Write(message);
        }

        // test rimozione punti duplicati

        [TestMethod]
        public void RemoveDuplicatedPoints1()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(-37.36,-12.08),
                new Point2d(-21.21,-21.99),
                new Point2d( -10.85,9.25),
                new Point2d(-30.66,12.75),
                new Point2d(-34.92,2.24),
                new Point2d(-35.95,-3.82),
                new Point2d(-37.36,-12.08)
            };

            //Act
            p1.RemoveDuplicatedPoints();

            //Assert;
            Assert.IsTrue(p1.Count == 6);
        }

        // test planarità del poligono. non va in exception se tutti i punti sono sullo stesso piano

        [TestMethod]
        public void IsPlanar1()
        {
            //Arrange
            try
            {
                Polygon2d p1 = new Polygon2d()
                {
                new Point2d(-37.36,-12.08 ),
                new Point2d(-21.21,-21.99),
                new Point2d( -10.85,9.25),
                new Point2d(-30.66,12.75),
                new Point2d(-34.92,2.24),
                new Point2d(-35.95,-3.82),
                new Point2d(-37.36,-12.08)
                };
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

        // test planarità del poligono. non va in exception se tutti i punti sono sullo stesso piano

        [TestMethod]
        public void IsPlanar5()
        {
            //Arrange
            try
            {
                Polygon2d p1 = new Polygon2d()
                {
                new Point2d(0, 0),
                new Point2d(0.5, 0),
                new Point2d(1, 0),
                new Point2d(1, 1),
                new Point2d(0, 1),
                };

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

        // test planarità del poligono. non va in exception se tutti i punti sono sullo stesso piano

        [TestMethod]
        public void IsPlanar6()
        {
            //Arrange
            try
            {
                Polygon2d p1 = new Polygon2d()
                {
                new Point2d(0, 0),
                new Point2d(0, 0),
                new Point2d(0, 0),
                new Point2d(1, 1),
                new Point2d(0, 1),
                new Point2d(0, 0),
                };

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

        // test rimozione punti. corretto che rimuova l'ultimo perchè è duplicato. punti corretti dopo la rimozione = 6

        [TestMethod]
        public void RemoveDuplicatedPoints2()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
                {
                new Point2d(-37.36,-12.08 ),
                new Point2d(-21.21,-21.99),
                new Point2d( -10.85,9.25),
                new Point2d(-30.66,12.75),
                new Point2d(-34.92,2.24),
                new Point2d(-35.95,-3.82),
                new Point2d(-37.36,-12.08)
                };

            //Act
            p1.RemoveDuplicatedPoints();

            //Assert;
            Assert.IsTrue(p1.Count == 6);
        }

        // test rimozione punti. corretto tenga tutti i punti poichè sono sullo stesso piano. 3 punti sono allineati

        [TestMethod]
        public void RemoveDuplicatedPoints3()
        {
            //Arrange

            Polygon2d p1 = new Polygon2d()
            {
            new Point2d(0, 0),
            new Point2d(0.5, 0),
            new Point2d(1, 0),
            new Point2d(1, 1),
            new Point2d(0, 1),
            };

            //Act
            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCountAfterRemove == 5);
        }

        // test rimozione punti. corretto rimuova 3 punti (0, 0, 0). punti dopo la rimozione =3

        [TestMethod]
        public void RemoveDuplicatedPoints4()
        {
            //Arrange

            Polygon2d p1 = new Polygon2d()
            {
            new Point2d(0, 0),
            new Point2d(0, 0),
            new Point2d(0, 0),
            new Point2d(1, 1),
            new Point2d(0, 1),
            new Point2d(0, 0),
            };

            //Act
            p1.RemoveDuplicatedPoints();

            //Assert;
            Assert.IsTrue(p1.Count == 3);
        }

        // test di rimozione di punti allineati. corretto rimuova 3 punti. punti inseriti = 5, punti dopo la rimozione = 2. tiene gli estremi

        [TestMethod]
        public void AlignedPointsXdir()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(    -5,     0.00 ),
                new Point2d(    -2,     0.00 ),
                new Point2d(    0,      0.00 ),
                new Point2d(    10,     0.00 ),
                new Point2d(    50,     0.00 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            Point2d expPoint1 = new Point2d(-5, 0);
            Point2d expPoint2 = new Point2d(50, 0);

            //Assert;
            Assert.IsTrue(pointCount == 5);
            Assert.IsTrue(pointCountAfterRemove == 2);
            Assert.IsTrue(Math.Abs(p1[0].X - expPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[0].Y - expPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].X - expPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].Y - expPoint2.Y) < 0.001);
        }

        // test di rimozione di punti allineati. corretto rimuova 3 punti. punti inseriti = 5, punti dopo la rimozione = 2. tiene gli estremi

        [TestMethod]
        public void AlignedPointsYdir()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(    0.00,   -50),
                new Point2d(    0.00,   -10),
                new Point2d(    0.00,   50),
                new Point2d(    0.00,   20),
                new Point2d(    0.00,   0),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();
            int pointCountAfterRemove = p1.Count;

            Point2d expPoint1 = new Point2d(0, -50);
            Point2d expPoint2 = new Point2d(0, 50);

            //Assert;
            Assert.IsTrue(pointCount == 5);
            Assert.IsTrue(pointCountAfterRemove == 2);
            Assert.IsTrue(Math.Abs(p1[0].X - expPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[0].Y - expPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].X - expPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].Y - expPoint2.Y) < 0.001);
        }

        // test di rimozione di punti allineati. corretto rimuova 4 punti. punti inseriti = 6, punti dopo la rimozione = 2. tiene gli estremi

        [TestMethod]
        public void AlignedPointsLimit()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   0.00,    0 ),
                new Point2d(   0.00,    4 ),
                new Point2d(   0.00,    2 ),
                new Point2d(   0.00,    5 ),
                new Point2d(   0.00,    3 ),
                new Point2d(   0.00,    6 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            Point2d expPoint1 = new Point2d(0, 0);
            Point2d expPoint2 = new Point2d(0, 6);

            //Assert;
            Assert.IsTrue(pointCount == 6);
            Assert.IsTrue(pointCountAfterRemove == 2);
            Assert.IsTrue(Math.Abs(p1[0].X - expPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[0].Y - expPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].X - expPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].Y - expPoint2.Y) < 0.001);
        }

        [TestMethod]
        public void AlignedPointsLimit2()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   0.00,    3 ),
                new Point2d(   0.00,    -5 ),
                new Point2d(   0.00,    2 ),
                new Point2d(   0.00,    5 ),
                new Point2d(   0.00,    3 ),
                new Point2d(   0.00,    6 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            Point2d expPoint1 = new Point2d(0, -5);
            Point2d expPoint2 = new Point2d(0, 6);

            //Assert;
            Assert.IsTrue(pointCount == 6);
            Assert.IsTrue(pointCountAfterRemove == 2);
            Assert.IsTrue(Math.Abs(p1[0].X - expPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[0].Y - expPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].X - expPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].Y - expPoint2.Y) < 0.001);
        }

        [TestMethod]
        public void AlignedPointsLimit3()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d( 0  ,  0 ),         // 0
                new Point2d( 0  ,  2 ),         // 1
                new Point2d( 1  ,  2 ),         // 2
                new Point2d( 2  ,  2 ),         // 3
                new Point2d( 2  ,  1 ),         // 4
                new Point2d( 3  ,  1 ),         // 5
                new Point2d( 3  ,  2 ),         // 6
                new Point2d( 4  ,  2 ),         // 7
                new Point2d( 5  ,  2 ),         // 8
                new Point2d( 5  ,  1 ),         // 9
                new Point2d( 6  ,  1 ),         // 10
                new Point2d( 6  ,  2 ),         // 11
                new Point2d( 7  ,  2 ),         // 12
                new Point2d( 8  ,  2 ),         // 13
                new Point2d( 8  ,  1 ),         // 14
                new Point2d( 8  ,  0 ),         // 15
                new Point2d( 7  ,  0 ),         // 16
                new Point2d( 3  ,  0 ),         // 17
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            Point2d expPoint0 = new Point2d(0, 0);
            Point2d expPoint1 = new Point2d(0, 2);
            Point2d expPoint2 = new Point2d(2, 2);
            Point2d expPoint3 = new Point2d(2, 1);
            Point2d expPoint4 = new Point2d(3, 1);
            Point2d expPoint5 = new Point2d(3, 2);
            Point2d expPoint6 = new Point2d(5, 2);
            Point2d expPoint7 = new Point2d(5, 1);
            Point2d expPoint8 = new Point2d(6, 1);
            Point2d expPoint9 = new Point2d(6, 2);
            Point2d expPoint10 = new Point2d(8, 2);
            Point2d expPoint11 = new Point2d(8, 0);

            //Assert;
            Assert.IsTrue(pointCount == 18);
            Assert.IsTrue(pointCountAfterRemove == 12);
            Assert.IsTrue(Math.Abs(p1[0].X - expPoint0.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[0].Y - expPoint0.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].X - expPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[1].Y - expPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[2].X - expPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[2].Y - expPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[3].X - expPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[3].Y - expPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[4].X - expPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[4].Y - expPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[5].X - expPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[5].Y - expPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[6].X - expPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[6].Y - expPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[7].X - expPoint7.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[7].Y - expPoint7.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[8].X - expPoint8.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[8].Y - expPoint8.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[9].X - expPoint9.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[9].Y - expPoint9.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[10].X - expPoint10.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[10].Y - expPoint10.Y) < 0.001);
            Assert.IsTrue(Math.Abs(p1[11].X - expPoint11.X) < 0.001);
            Assert.IsTrue(Math.Abs(p1[11].Y - expPoint11.Y) < 0.001);
        }

        [TestMethod]
        public void PolygonExplode()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   0,    0 ),
                new Point2d(   0,    10 ),
                new Point2d(   10,   10 ),
                new Point2d(   10,   0 ),
            };

            //Act
            Line2d[] List = p1.Explode();
            Point2d point1 = new Point2d(0, 0);
            Point2d point2 = new Point2d(0, 10);
            Point2d point3 = new Point2d(10, 10);
            Point2d point4 = new Point2d(10, 0);
            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point4);
            Line2d line4 = new Line2d(point4, point1);

            //Assert;
            Assert.IsTrue(List[0] == line1);
            Assert.IsTrue(List[1] == line2);
            Assert.IsTrue(List[2] == line3);
            Assert.IsTrue(List[3] == line4);
            Assert.IsFalse(List[3] == line1);
            Assert.IsFalse(List[2] == line2);
            Assert.IsFalse(List[1] == line3);
            Assert.IsFalse(List[0] == line4);
        }

        [TestMethod]
        public void PolygonExplode2()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   32,      2 ),
                new Point2d(   5,       150 ),
                new Point2d(   -100,   -10 ),
                new Point2d(   10,      0 ),
            };

            //Act
            Line2d[] List = p1.Explode();
            Point2d point1 = new Point2d(   32,     2);
            Point2d point2 = new Point2d(   5,      150);
            Point2d point3 = new Point2d(   -100,   -10);
            Point2d point4 = new Point2d(   10,     0);
            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point4);
            Line2d line4 = new Line2d(point4, point1);

            //Assert;
            Assert.IsTrue(List[0] == line1);
            Assert.IsTrue(List[1] == line2);
            Assert.IsTrue(List[2] == line3);
            Assert.IsTrue(List[3] == line4);
            Assert.IsFalse(List[3] == line1);
            Assert.IsFalse(List[2] == line2);
            Assert.IsFalse(List[1] == line3);
            Assert.IsFalse(List[0] == line4);
        }

        [TestMethod]
        public void IsPointInside()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   0,      0  ),
                new Point2d(   10,     0  ),
                new Point2d(   10,     10 ),
                new Point2d(   0,      10 ),
            };

            //Act
            Point2d point1 = new Point2d(1, 2);
            Point2d point2 = new Point2d(5, 10);
            Point2d point3 = new Point2d(0, 0);
            Point2d point4 = new Point2d(10, 10);

            Point2d point5 = new Point2d(-1, 2);
            Point2d point6 = new Point2d(5, 12);
            Point2d point7 = new Point2d(-10, -10);
            Point2d point8 = new Point2d(15, 15);

            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point4);
            Line2d line4 = new Line2d(point4, point1);

            Line2d line5 = new Line2d(point5, point6);
            Line2d line6 = new Line2d(point6, point7);
            Line2d line7 = new Line2d(point7, point8);
            Line2d line8 = new Line2d(point8, point5);

            //Assert;
            Assert.IsTrue(p1.IsLineInside(line1));
            Assert.IsTrue(p1.IsLineInside(line2));
            Assert.IsTrue(p1.IsLineInside(line3));
            Assert.IsTrue(p1.IsLineInside(line4));
            Assert.IsFalse(p1.IsLineInside(line5));
            Assert.IsFalse(p1.IsLineInside(line6));
            Assert.IsFalse(p1.IsLineInside(line7));
            Assert.IsFalse(p1.IsLineInside(line8));
        }

        [TestMethod]
        public void IsPointInside2()
        {
            //Arrange
            Polygon2d p1 = new Polygon2d()
            {
                new Point2d(   -1,     -1  ),
                new Point2d(    1,     -1  ),
                new Point2d(    1,      1 ),
                new Point2d(   -1,      1 ),
            };

            //Act
            Point2d point1 = new Point2d(0, 0);
            Point2d point2 = new Point2d(1, 1);
            Point2d point3 = new Point2d(-1,-1);
            Point2d point4 = new Point2d(-1, 1);

            Point2d point5 = new Point2d(-1, -1);
            Point2d point6 = new Point2d(5, 12);
            Point2d point7 = new Point2d(-1, -1);
            Point2d point8 = new Point2d(1, -1);

            Line2d line1 = new Line2d(point1, point2);
            Line2d line2 = new Line2d(point2, point3);
            Line2d line3 = new Line2d(point3, point4);
            Line2d line4 = new Line2d(point4, point1);

            Line2d line5 = new Line2d(point5, point6);
            Line2d line6 = new Line2d(point6, point7);
            Line2d line7 = new Line2d(point7, point8);
            Line2d line8 = new Line2d(point8, point5);

            //Assert;
            Assert.IsTrue(p1.IsLineInside(line1));
            Assert.IsTrue(p1.IsLineInside(line2));
            Assert.IsTrue(p1.IsLineInside(line3));
            Assert.IsTrue(p1.IsLineInside(line4));
            Assert.IsFalse(p1.IsLineInside(line5));
            Assert.IsFalse(p1.IsLineInside(line6));
            Assert.IsTrue(p1.IsLineInside(line7));
            Assert.IsTrue(p1.IsLineInside(line8));
        }

        [TestMethod]
        public void CircleBarycenter()
        {
            double diameter = 100;
            //Arrange
            Polygon2d p1 = new Polygon2d(diameter, 32, new Point2d(50, 50));

            //Assert
            Assert.AreEqual(new Point3d(50, 50, 0).DistanceTo(p1.GetCenter()), 0, 0.01);
        }
    }
}
