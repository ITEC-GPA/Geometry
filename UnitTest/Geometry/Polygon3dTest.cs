using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Polygon3dTest : UnitTestBase
    {
        [TestMethod]
        public void RemoveDuplicatePoints()
        {
            //Arrange

            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(34.00, -4, 6.03),
                new Point3d(34.00, -4, 6.03),
                new Point3d(34.00, -4, 6.03),
                new Point3d(34.00, -4, 6.03),
                new Point3d(34.00, -4, 6.03)
            };

            //Act
            p1.RemoveDuplicatedPoints();

            //Assert
            Assert.AreEqual(4, p1.Count, p1.Count.ToString());
        }

        /// <summary>
        /// Area test. Check if the polygon have the calculated area
        /// </summary>
        [TestMethod]
        public void SignedArea()
        {
            //Arrange
            double expectedArea = 1463.53775;
            double tollerance = 0.005;

            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(34.00, -4, 6.03)
            };

            //Act
            double area = p1.GetSignedArea();

            p1.Reverse();
            double reversedArea = p1.GetSignedArea();

            //Assert
            double difference = (area - expectedArea) / area;

            string message = $"Result: {area}, Expected: {expectedArea}, Difference: {difference}. Reversed area: {reversedArea}";
            Assert.IsTrue(difference < tollerance, message);
            Assert.IsTrue(area + reversedArea < 0.001, message);

            Console.Write(message);
        }

        /// <summary>
        /// Right oriented test. Check if the polygon is right hand ordered
        /// </summary>
        [TestMethod]
        public void RightOriented()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(62.45, 27.99, -22.40 ),
                new Point3d(40.04, 51.30 ,0.00),
                new Point3d(25.81, 35.30, 14.22),
                new Point3d(34.00, -4, 6.03)
            };

            //Act

            //Assert;
            Assert.IsTrue(p1.IsRightHandOrdered());

            p1.Reverse();
            Assert.IsFalse(p1.IsRightHandOrdered());
        }

        /// <summary>
        /// Removal points test. Remove duplicated points.
        /// points removed = 5
        /// points keeps after remove = 5
        /// </summary>
        [TestMethod]
        public void DuplicatedPoints()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(-37.36,-12.08,0.00 ),
                new Point3d(-21.21,-21.99,0.00),
                new Point3d( -10.85,9.25,0.00),
                new Point3d(-30.66,12.75,0.00),
                new Point3d(-34.92,2.24,0.00),
                new Point3d(-35.95,-3.82,0.00),
                new Point3d(-37.36,-12.08,0.00)
            };

            //Act
            p1.RemoveDuplicatedPoints();

            //Assert;
            Assert.IsTrue(p1.Count == 6);
        }

        /// <summary>
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
        /// </summary>
        [TestMethod]
        public void IsPlanar1()
        {
            //Arrange
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                new Point3d(-37.36,-12.08,0.00 ),
                new Point3d(-21.21,-21.99,0.00),
                new Point3d( -10.85,9.25,0.00),
                new Point3d(-30.66,12.75,0.00),
                new Point3d(-34.92,2.24,0.00),
                new Point3d(-35.95,-3.82,0.00),
                new Point3d(-37.36,-12.08,0.00)
                };

                p1.RemoveAlignedPoints(0.001);
                int pointCountAfterRemove = p1.Count;
                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
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

                p1.RemoveAlignedPoints(0.001);
                int pointCountAfterRemove = p1.Count;
                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
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
                new Point3d(8.778534, 0, 0)
                };

                p1.RemoveAlignedPoints(0.001);
                int pointCountAfterRemove = p1.Count;
                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
        /// </summary>
        [TestMethod]
        public void IsPlanar4()
        {
            //Arrange
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
                new Point3d(-0.11111, 0, 10),
                new Point3d(1.83219, -0.784245, 8),
                new Point3d(2, 5, 1.000001),
                };

                p1.RemoveAlignedPoints(0.001);
                int pointCountAfterRemove = p1.Count;
                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
        /// </summary>
        [TestMethod]
        public void IsPlanar5()
        {
            //Arrange
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                new Point3d(1, 1, 1),
                new Point3d(2, 1, 1),
                new Point3d(3, 1, 1),
                new Point3d(1, 4658461, 0),
                new Point3d(0, 1, -548455),
                };

                p1.RemoveAlignedPoints(0.001);
                int pointCountAfterRemove = p1.Count;
                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("non planare");
                // se viene aggiunto un punto non planare va in argumentException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.Fail(e.Message);
            }
        }

        /// <summary>
        /// Flatness test of the polygon. it doesn't go into exception if all points are on the same plane
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
                new Point3d(0.5, 0.5, 0),
                new Point3d(0.5, 0.5, 0),
                new Point3d(1, 1, 0),
                new Point3d(0, 1, 0),
                new Point3d(0, 0, 0),
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
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// </summary>
        [TestMethod]
        public void IsPlanar1RemoveDuplicatedPoints()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
                {
                new Point3d(-37.36,-12.08,0.00 ),
                new Point3d(-21.21,-21.99,0.00),
                new Point3d( -10.85,9.25,0.00),
                new Point3d(-30.66,12.75,0.00),
                new Point3d(-34.92,2.24,0.00),
                new Point3d(-35.95,-3.82,0.00),
                new Point3d(-37.36,-12.08,0.00)
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
            Assert.IsTrue(pointCount == 7);
            Assert.IsTrue(pointCountAfterRemove == 6);
        }

        /// <summary>
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// points removed = 0
        /// points keeps after remove = 5
        /// </summary>
        [TestMethod]
        public void IsPlanar2RemoveDuplicatedPoints()
        {
            //Arrange

            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2.5, 15, 1 ),
                new Point3d(1.1, 12.3, 5),
                new Point3d(6, 2, 3.2),
            };
            Point3d point1 = new Point3d(1.83219, 5, 8);            // è distante 0.02 => test fallisce
            Point3d point2 = new Point3d(8.778534, 0, 0);           // è distante 0.008 => test fallisce
            Point3d point3 = new Point3d(-8.662555, 0, 0);          // è distante 0.005 => test fallisce
            Point3d point4 = new Point3d(3.900, 7.8250, 3.100);     // è interno
            Point3d point5 = new Point3d(2.6750, 10.400, 3.550);    // è interno
            Point3d point6 = new Point3d(3.0250, 11.075, 2.550);    // è interno

            //Act
            p1.IsPointInside(point1);
            p1.IsPointInside(point2);
            p1.IsPointInside(point3);
            p1.IsPointInside(point4);
            p1.IsPointInside(point5);
            p1.IsPointInside(point6);

            //Assert;
            Assert.IsFalse(p1.IsPointInside(point1));
            Assert.IsFalse(p1.IsPointInside(point2));
            Assert.IsFalse(p1.IsPointInside(point3));
            Assert.IsTrue(p1.IsPointInside(point4));
            Assert.IsTrue(p1.IsPointInside(point5));
            Assert.IsTrue(p1.IsPointInside(point6));
        }

        /// <summary>
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// points removed = 1
        /// points keeps after remove = 4
        /// </summary>
        [TestMethod]
        public void IsPlanar3RemoveDuplicatedPoints()
        {
            // Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2.5, 15, 1 ),                   // tutti punti sul piano
                new Point3d(1.1, 12.3, 5),
                new Point3d(6, 2, 3.2),
                new Point3d(3.900, 7.8250, 3.100),
                new Point3d(2.6750, 10.400, 3.550),
                new Point3d(3.0250, 11.075, 2.550),
            };

            // Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();
            int pointCountAfterRemove = p1.Count;

            for (int i = 0; i < pointCountAfterRemove; i++)
            {
                Console.WriteLine($"{p1[i]}");
            }

            // Assert;
            Assert.IsTrue(pointCount == 6);
            Assert.IsTrue(pointCountAfterRemove == 6, pointCountAfterRemove.ToString());
        }

        /// <summary>
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// points removed = 1
        /// points keeps after remove = 5
        /// </summary>
        [TestMethod]
        public void IsPlanar4RemoveDuplicatedPoints()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
            new Point3d(2, 5, 1),
            new Point3d(3, 2, 3),
            new Point3d(0, 5, 4),
            new Point3d(-0.11111, 0, 10),
            new Point3d(1.83219, -0.784245, 8),
            new Point3d(2, 5, 1.0000001),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveDuplicatedPoints();
            p1.RemoveAlignedPoints();
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
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// points removed = 1
        /// points keeps after remove = 4
        /// </summary>
        [TestMethod]
        public void IsPlanar5RemoveDuplicatedPoints()
        {
            //Arrange

            Polygon3d p1 = new Polygon3d()
            {
            new Point3d(0, 0, 0),
            new Point3d(0.5, 0, 0),
            new Point3d(1, 0, 0),
            new Point3d(1, 1, 0),
            new Point3d(0, 1, 0),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();
            int pointCountAfterRemove = p1.Count;

            for (int i = 0; i < pointCountAfterRemove; i++)
            {
                Console.WriteLine($"{p1[i]}");
            }

            //Assert;
            Assert.IsTrue(pointCount == 5);
            Assert.IsTrue(pointCountAfterRemove == 4);
        }

        /// <summary>
        /// removal points test. Remove points if they aren't on the same plane and if they are duplicates.
        /// points removed = 3
        /// points keeps after remove = 3
        /// </summary>
        [TestMethod]
        public void IsPlanar6RemoveDuplicatedPoints()
        {
            //Arrange

            Polygon3d p1 = new Polygon3d()
            {
            new Point3d(0, 0, 0),
            new Point3d(0, 0, 0),
            new Point3d(0, 0, 0),
            new Point3d(1, 1, 0),
            new Point3d(0, 1, 0),
            new Point3d(0, 0, 0),
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
            Assert.IsTrue(pointCountAfterRemove == 3);
        }

        /// <summary>
        /// Removal points test. Remove aligned points.
        /// points removed = 3
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPointsXdir()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    -5,     0.00,   0.00 ),
                new Point3d(    -2,     0.00,   0.00 ),
                new Point3d(    0,      0.00,   0.00 ),
                new Point3d(    10,     0.00,   0.00 ),
                new Point3d(    50,     0.00,   0.00 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            for (int i = 0; i < pointCountAfterRemove; i++)
            {
                Console.WriteLine($"{p1[i]}");
            }

            //Assert;
            Assert.IsTrue(pointCount == 5);
            Assert.IsTrue(pointCountAfterRemove == 2);
        }

        /// <summary>
        /// Removal points test. Remove aligned points.
        /// points removed = 3
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPointsYdir()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    0.00,   -50,      0.00 ),
                new Point3d(    0.00,   -10,    0.00 ),
                new Point3d(    0.00,   50,     0.00 ),
                new Point3d(    0.00,   20,    0.00 ),
                new Point3d(    0.00,   0,      0.00 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            for (int i = 0; i < pointCountAfterRemove; i++)
            {
                Console.WriteLine($"{p1[i]}");
            }

            //Assert;
            Assert.IsTrue(pointCount == 5);
            Assert.IsTrue(pointCountAfterRemove == 2);
        }

        /// <summary>
        /// Removal points test. Remove aligned points.
        /// points removed = 3
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPointsZdir()
        {
            try
            {
                Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    0.00,   0.00,    50 ),
                new Point3d(    0.00,   0.00,    -20 ),
                new Point3d(    0.00,   0.00,    20 ),
                new Point3d(    0.00,   0.00,    10 ),
                new Point3d(    0.00,   0.00,    0 ),
            };

                //Act
                int pointCount = p1.Count;
                p1.RemoveAlignedPoints();

                int pointCountAfterRemove = p1.Count;

                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
            /// Removal points test. Remove aligned points.
            /// points removed = 4
            /// points keeps after remove = 2
            /// </summary>
        [TestMethod]
        public void AlignedPointsLimit()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    0.00,   0.00,    0 ),
                new Point3d(    0.00,   0.00,    4 ),
                new Point3d(    0.00,   0.00,    2 ),
                new Point3d(    0.00,   0.00,    5 ),
                new Point3d(    0.00,   0.00,    3 ),
                new Point3d(    0.00,   0.00,    6 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 6);
            Assert.IsTrue(pointCountAfterRemove == 2);
        }

        /// <summary>
        /// Removal points test. Remove aligned points.
        /// points removed = 8
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPoints1()
        {
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                    new Point3d(1, 1, 1 ),
                    new Point3d(1, 1, 2 ),
                    new Point3d(1, 1, 3 ),
                    new Point3d(1, 1, 2 ),
                    new Point3d(1, 1, 1 ),
                    new Point3d(1, 1, 2 ),
                    new Point3d(1, 1, 3 ),
                    new Point3d(1, 1, 2 ),
                    new Point3d(1, 1, 1 ),
                    new Point3d(1, 1, 0 ),
                };

                //Act
                int pointCount = p1.Count;
                p1.RemoveAlignedPoints();

                int pointCountAfterRemove = p1.Count;

                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
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
        /// Removal points test. Remove aligned points.
        /// points removed = 8
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPoints2()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    1,   5,    1 ),
                new Point3d(    5,   3,    2 ),
                new Point3d(    5,   1,    3 ),
                new Point3d(    1,   3,    2 ),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            for (int i = 0; i < pointCountAfterRemove; i++)
            {
                Console.WriteLine($"{p1[i]}");
            }

            //Assert;
            Assert.IsTrue(pointCount == 4);
            Assert.IsTrue(pointCountAfterRemove == 4);
        }

        /// <summary>
        /// Removal points test. Remove aligned points.
        /// points removed = 8
        /// points keeps after remove = 2
        /// </summary>
        [TestMethod]
        public void AlignedPoints3()
        {
            //Arrange
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                    new Point3d(    0,   5,    1 ),
                    new Point3d(    1,   3,    2 ),
                    new Point3d(    2,   1,    3 ),
                    new Point3d(    1,   3,    2 ),
                    new Point3d(    0,   5,    1 ),
                    new Point3d(    1,   3,    2 ),
                    new Point3d(    2,   1,    3 ),
                    new Point3d(    1,   3,    2 ),
                    new Point3d(    0,   5,    1 ),
                    new Point3d(    -1,   7,    0 ),
                };
                //Act
                int pointCount = p1.Count;
                p1.RemoveAlignedPoints();

                int pointCountAfterRemove = p1.Count;

                for (int i = 0; i < pointCountAfterRemove; i++)
                {
                    Console.WriteLine($"{p1[i]}");
                }
            }

            catch (ArgumentException)
            {
                // i punti sono tutti su una stessa retta. ne terrebbe 2. va in argumentException();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void AlignedPoints4()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    1,   1,    1 ),
                new Point3d(    1,   1,    2 ),
                new Point3d(    1,   1,    3 ),
                new Point3d(    1,   1,    4 )
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 4, pointCount.ToString());
            Assert.IsTrue(pointCountAfterRemove == 2, pointCountAfterRemove.ToString());
        }

        [TestMethod]
        public void AlignedPoints5()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    1,   1,    1 ),
                new Point3d(    1,   1,    2 ),
                new Point3d(    1,   1,    4 ),
                new Point3d(    1,   1,    3 ),
                new Point3d(    1,   1,    5 )
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 5, pointCount.ToString());
            Assert.IsTrue(pointCountAfterRemove == 2, pointCountAfterRemove.ToString());
        }

        [TestMethod]
        public void AlignedPoints6()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(    0,   0,    0 ),
                new Point3d(   0.5,  0,  0 ),
                new Point3d(    1,   0,    0 ),
                new Point3d(    1,   1,    0 ),
                new Point3d(    0,   1,    0 ),
                new Point3d(    0,   0.5,  0 )
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 6, pointCount.ToString());
            Assert.IsTrue(pointCountAfterRemove == 4, pointCountAfterRemove.ToString());
        }

        [TestMethod]
        public void AlignedPoints7()
        {
            try
            {
                Polygon3d p1 = new Polygon3d()
                {
                new Point3d( 0.04,  54,    1 ),
                new Point3d(   11,   1,    5.4 ),
                new Point3d(    1,  71,    3 ),
                new Point3d(    1,   1,    4 ),
                new Point3d(    1,   1,    2 )
                };

                //Act
                int pointCount = p1.Count;
                p1.RemoveAlignedPoints();

                int pointCountAfterRemove = p1.Count;
            }
        

            catch (ArgumentException)
            {
                // se viene aggiunto un punto non planare va in argumentException();
            }
        }

        [TestMethod]
        public void AlignedPoints8()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d( -0.25,  0.25,   0.0),
                new Point3d(     0,     0,     0 ),
                new Point3d(   0.5,     0,    0 ),
                new Point3d(     1,     0,    0 ),
                new Point3d(     1,     1,    0 ),
                new Point3d(    -0.5,    1,    0 ),
                new Point3d(    -0.5,  0.5,  0.0),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 7, pointCount.ToString());
            Assert.IsTrue(pointCountAfterRemove == 5, pointCountAfterRemove.ToString());
        }

        [TestMethod]
        public void AlignedPoints9()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d( -0.25,  0.25,   0.0),
                new Point3d(     0,     0,     0 ),
                new Point3d(   0.24,     0,    0 ),
                new Point3d(   0.5,     0,    0 ),
                new Point3d(     1,     0,    0 ),
                new Point3d(     1,     1,    0 ),
                new Point3d(    -0.5,    1,    0 ),
                new Point3d(    -0.5,  0.5,  0.0),
            };

            //Act
            int pointCount = p1.Count;
            p1.RemoveAlignedPoints();

            int pointCountAfterRemove = p1.Count;

            //Assert;
            Assert.IsTrue(pointCount == 8, pointCount.ToString());
            Assert.IsTrue(pointCountAfterRemove == 5, pointCountAfterRemove.ToString());
        }

        /// <summary>
        /// Test if the input points are on shape
        /// </summary>
        [TestMethod]
        public void IsPointOnPolygon1()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
            };

            //Act
            Point3d point1 = new Point3d(-0.11111, 0, 10);
            Point3d point2 = new Point3d(1.83219, -0.784245, 8);

            //Assert;
            Assert.IsFalse(p1.IsPointInside(point1));
            Assert.IsFalse(p1.IsPointInside(point2));
        }

        /// <summary>
        /// Test if the input points are on polygon
        /// </summary>
        [TestMethod]
        public void IsPointOnPolygon2()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
            };

            //Act

            Point3d point1 = new Point3d(1.66666, 4.000, 2.666666);

            //Assert;
            Assert.IsTrue(p1.IsPointInside(point1));
        }

        [TestMethod]
        public void IsPointOnPolygon3()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
            };

            //Act
            Point3d point1 = new Point3d(1.66666, 4.5, 2.666666);
            Point3d point2 = new Point3d(4, 4.5, 1);
            Point3d point3 = new Point3d(1.66666, 4.1, 2.666666);

            //Assert;
            Assert.IsFalse(p1.IsPointInside(point1));
            Assert.IsFalse(p1.IsPointInside(point2));
            Assert.IsFalse(p1.IsPointInside(point3));
        }

        [TestMethod]
        public void IsPointOnPolygon4()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
            };

            //Act
            Point3d point1 = new Point3d(1.5, 3.5, 3.5);           // sono i 3 punti medi dei lati
            Point3d point2 = new Point3d(2.5, 3.5, 2);
            Point3d point3 = new Point3d(1, 5, 2.5);


            //Assert;
            Assert.IsTrue(p1.IsPointInside(point1));
            Assert.IsTrue(p1.IsPointInside(point2));
            Assert.IsTrue(p1.IsPointInside(point3));
        }

        [TestMethod]
        public void IsPointOnPolygon5()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 10, 20),
                new Point3d(0, 25, 10),
                new Point3d(0, 20, 5),
                new Point3d(0, 10, 10),
            };

            //Act
            Point3d point1 = new Point3d(0, 5, 5);
            Point3d point2 = new Point3d(0, 20, 2);
            Point3d point3 = new Point3d(5, 20, 25);
            Point3d point4 = new Point3d(0, 25, 10);
            Point3d point5 = new Point3d(0, 25, 11);
            Point3d point6 = new Point3d(0, 25, 10.1);
            Point3d point7 = new Point3d(0, 20, 0);
            Point3d point8 = new Point3d(0, 20, 6);
            Point3d point9 = new Point3d(0, 10, 5);


            //Assert;
            Assert.IsTrue(p1.IsPointInside(point1), point1.ToString());
            Assert.IsFalse(p1.IsPointInside(point2), point2.ToString());
            Assert.IsFalse(p1.IsPointInside(point3), point3.ToString());
            Assert.IsTrue(p1.IsPointInside(point4), point4.ToString());
            Assert.IsFalse(p1.IsPointInside(point5), point5.ToString());
            Assert.IsFalse(p1.IsPointInside(point6), point6.ToString());
            Assert.IsFalse(p1.IsPointInside(point7), point6.ToString());
            Assert.IsTrue(p1.IsPointInside(point8), point6.ToString());
            Assert.IsFalse(p1.IsPointInside(point9), point6.ToString());
        }

        [TestMethod]
        public void IsLineInsidePolygon1()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 10, 20),
                new Point3d(0, 25, 10),
                new Point3d(0, 20, 5),
                new Point3d(0, 10, 10),
            };

            //Act
            Point3d point1 = new Point3d(0, 5, 5);
            Point3d point2 = new Point3d(0, 20, 2);
            Point3d point3 = new Point3d(5, 20, 25);
            Point3d point4 = new Point3d(0, 25, 10);
            Point3d point5 = new Point3d(0, 25, 11);
            Point3d point6 = new Point3d(0, 25, 10.1);
            Point3d point7 = new Point3d(0, 20, 0);
            Point3d point8 = new Point3d(0, 20, 6);
            Point3d point9 = new Point3d(0, 10, 5);

            Point3d point10 = new Point3d(0, 10, 11);
            Point3d point11 = new Point3d(0, 10, 19);
            Point3d point12 = new Point3d(0, 20, 10);
            Point3d point13 = new Point3d(0, 15, 15);

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
            Line3d line11 = new Line3d(point11, point12);
            Line3d line12 = new Line3d(point12, point13);
            Line3d line13 = new Line3d(point13, point10);

            //Assert;
            Assert.IsFalse(p1.IsLineInside(line1));
            Assert.IsFalse(p1.IsLineInside(line2));
            Assert.IsFalse(p1.IsLineInside(line3));
            Assert.IsFalse(p1.IsLineInside(line4));
            Assert.IsFalse(p1.IsLineInside(line5));
            Assert.IsFalse(p1.IsLineInside(line6));
            Assert.IsFalse(p1.IsLineInside(line7));
            Assert.IsFalse(p1.IsLineInside(line8));
            Assert.IsFalse(p1.IsLineInside(line9));

            Assert.IsTrue(p1.IsLineInside(line10));
            Assert.IsTrue(p1.IsLineInside(line11));
            Assert.IsTrue(p1.IsLineInside(line12));
            Assert.IsTrue(p1.IsLineInside(line13));
        }

        [TestMethod]
        public void IsLineInsidePolygon2()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(2, 5, 1),
                new Point3d(3, 2, 3),
                new Point3d(0, 5, 4),
            };

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
            Assert.IsFalse(p1.IsLineInside(line1));
            Assert.IsFalse(p1.IsLineInside(line2));
            Assert.IsFalse(p1.IsLineInside(line3));
            Assert.IsTrue(p1.IsLineInside(line4));
            Assert.IsTrue(p1.IsLineInside(line5));
            Assert.IsTrue(p1.IsLineInside(line6));

        }

        [TestMethod]
        public void Triangularization3vertex()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d c = new Point3d(2, 2, 0);
            Polygon3d[] list = p.Triangularization(c);
            Point3d centre = p.GetCentroid();

            Polygon3d p1 = new Polygon3d { new Point3d(0,0,0), new Point3d(10, 0, 0), new Point3d(2, 2, 0)};
            Polygon3d p2 = new Polygon3d { new Point3d(10, 0, 0), new Point3d(0, 10, 0), new Point3d(2, 2, 0) };
            Polygon3d p3 = new Polygon3d { new Point3d(0, 10, 0), new Point3d(0, 0, 0), new Point3d(2, 2, 0) };
            Point3d expCPoint = new Point3d(3.33333, 3.33333, 0);
            Point3d expBarycenter = new Point3d(3.33333, 3.33333, 0);

            //Assert;
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p2);
            Assert.IsTrue(list[2] == p3);
            Assert.IsTrue(Math.Abs(expCPoint.X-centre.X)<0.1);
            Assert.IsTrue(Math.Abs(expCPoint.Y - centre.Y) < 0.1);
            Assert.IsTrue(Math.Abs(expCPoint.Z - centre.Z) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.X - expBarycenter.X) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.Y - expBarycenter.Y) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.Z - expBarycenter.Z) < 0.1);
        }

        [TestMethod]
        public void Triangularization3vertex2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(1, 0, 1),
                new Point3d(2, 0, 1),
                new Point3d(0, 3, 1),
            };

            //Act
            Point3d c = new Point3d(1.2, 0.8, 1);
            Polygon3d[] list = p.Triangularization(c);
            Point3d centre = p.GetCentroid();

            Polygon3d p1 = new Polygon3d { new Point3d(1, 0, 1), new Point3d(2, 0, 1), new Point3d(1.2, 0.8, 1) };
            Polygon3d p2 = new Polygon3d { new Point3d(2, 0, 1), new Point3d(0, 3, 1), new Point3d(1.2, 0.8, 1) };
            Polygon3d p3 = new Polygon3d { new Point3d(0, 3, 1), new Point3d(1, 0, 1), new Point3d(1.2, 0.8, 1) };
            Point3d expCPoint = new Point3d(1, 1, 1);
            Point3d expBarycenter = new Point3d(1, 1, 1);

            //Assert;
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p2);
            Assert.IsTrue(list[2] == p3);
            Assert.IsTrue(Math.Abs(expCPoint.X - centre.X) < 0.1);
            Assert.IsTrue(Math.Abs(expCPoint.Y - centre.Y) < 0.1);
            Assert.IsTrue(Math.Abs(expCPoint.Z - centre.Z) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.X - expBarycenter.X) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.Y - expBarycenter.Y) < 0.1);
            Assert.IsTrue(Math.Abs(expBarycenter.Z - expBarycenter.Z) < 0.1);
        }

        [TestMethod]
        public void Triangularization4Vertex()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d c = new Point3d(5, 5, 0);
            Polygon3d[] list = p.Triangularization(c);
            Point3d centre = p.GetCenter();
            Point3d barycenter = p.GetCentroid();

            Polygon3d p1 = new Polygon3d { new Point3d(0, 0, 0), new Point3d(10, 0, 0), new Point3d(5, 5, 0) };
            Polygon3d p2 = new Polygon3d { new Point3d(10, 0, 0), new Point3d(10, 10, 0), new Point3d(5, 5, 0) };
            Polygon3d p3 = new Polygon3d { new Point3d(10, 10, 0), new Point3d(0, 10, 0), new Point3d(5, 5, 0) };
            Polygon3d p4 = new Polygon3d { new Point3d(0, 10, 0), new Point3d(0, 0, 0), new Point3d(5, 5, 0) };
            Point3d expPoint = new Point3d(5, 5, 0);

            //Assert;
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p2);
            Assert.IsTrue(list[2] == p3);
            Assert.IsTrue(list[3] == p4);
            Assert.IsTrue(Math.Abs(expPoint.X - centre.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Y - centre.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Z - centre.Z) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.X - centre.X) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Y - centre.Y) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Z - centre.Z) < 0.001);
        }

        [TestMethod]
        public void Triangularization5Vertex()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(5, 12, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d c = new Point3d(5, 5, 0);
            Polygon3d[] list = p.Triangularization(c);
            Point3d centre = p.GetCenter();
            Point3d barycenter = p.GetCentroid();

            Polygon3d p1 = new Polygon3d { new Point3d(0, 0, 0), new Point3d(10, 0, 0), new Point3d(5, 5, 0) };
            Polygon3d p2 = new Polygon3d { new Point3d(10, 0, 0), new Point3d(10, 10, 0), new Point3d(5, 5, 0) };
            Polygon3d p3 = new Polygon3d { new Point3d(10, 10, 0), new Point3d(5, 12, 0), new Point3d(5, 5, 0) };
            Polygon3d p4 = new Polygon3d { new Point3d(5, 12, 0), new Point3d(0, 10, 0), new Point3d(5, 5, 0) };
            Polygon3d p5 = new Polygon3d { new Point3d(0, 10, 0), new Point3d(0, 0, 0), new Point3d(5, 5, 0) };
            Point3d expPoint = new Point3d(5, 6.4, 0);
            Point3d expBary = new Point3d(5, 5.515151, 0);

            //Assert;
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p2);
            Assert.IsTrue(list[2] == p3);
            Assert.IsTrue(list[3] == p4);
            Assert.IsTrue(list[4] == p5);
            Assert.IsTrue(Math.Abs(expPoint.X - centre.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Y - centre.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Z - centre.Z) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.X - expBary.X) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Y - expBary.Y) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Z - expBary.Z) < 0.001);
        }

        [TestMethod]
        public void Triangularization5Vertex2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(1, 0, 0),
                new Point3d(2, 1, 0),
                new Point3d(0, 3, 0),
                new Point3d(-1, 2, 0),
                new Point3d(-2, -1, 0),
            };

            //Act
            Point3d c = new Point3d(0, 1, 0);
            Polygon3d[] list = p.Triangularization(c);
            Point3d centre = p.GetCenter();
            Point3d barycenter = p.GetCentroid();

            Polygon3d p1 = new Polygon3d { new Point3d(1, 0, 0), new Point3d(2, 1, 0), new Point3d(0, 1, 0) };
            Polygon3d p2 = new Polygon3d { new Point3d(2, 1, 0), new Point3d(0, 3, 0), new Point3d(0, 1, 0) };
            Polygon3d p3 = new Polygon3d { new Point3d(0, 3, 0), new Point3d(-1, 2, 0), new Point3d(0, 1, 0) };
            Polygon3d p4 = new Polygon3d { new Point3d(-1, 2, 0), new Point3d(-2, -1, 0), new Point3d(0, 1, 0) };
            Polygon3d p5 = new Polygon3d { new Point3d(-2, -1, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0) };
            Point3d expPoint = new Point3d(0, 1, 0);
            Point3d expBary = new Point3d(-0.0833333, 0.91666666, 0);

            //Assert;
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p2);
            Assert.IsTrue(list[2] == p3);
            Assert.IsTrue(list[3] == p4);
            Assert.IsTrue(list[4] == p5);
            Assert.IsTrue(Math.Abs(expPoint.X - centre.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Y - centre.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint.Z - centre.Z) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.X - expBary.X) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Y - expBary.Y) < 0.001);
            Assert.IsTrue(Math.Abs(barycenter.Z - expBary.Z) < 0.001);
        }

        [TestMethod]
        public void TriangleBarycenter()
        {
            //Arrange
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(1, 0, 0),
                new Point3d(2, 0, 0),
                new Point3d(0, 3, 0),
            };

            //Arrange
            Polygon3d p2 = new Polygon3d()
            {
                new Point3d(-60.742, 45.233, -26.681),
                new Point3d(-50.006, 54.715, 41.133 ),
                new Point3d(-23.620, 36.893, 2.583),
            };

            Assert.AreEqual(new Point3d(1,1,0), p1.GetBarycenterOfTriangle());
            // exact values: the rounded ones (-44.789, 45.614, 5.678) are outside the tolerance of Point3d.Equals
            Assert.AreEqual(new Point3d(-134.368 / 3.0, 136.841 / 3.0, 17.035 / 3.0), p2.GetBarycenterOfTriangle());
            Console.WriteLine(p2.GetBarycenterOfTriangle());
        }

        [TestMethod]
        public void Parametrization1()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d pointToParametrize_1 = new Point3d(0,0,0);
            p.GetPointParametrization(pointToParametrize_1, out Polygon3d _, out double N1_1, out double N2_1, out double N3_1);

            Point3d pointToParametrize_2 = new Point3d(10, 0, 0);
            p.GetPointParametrization(pointToParametrize_2, out Polygon3d _, out double N1_2, out double N2_2, out double N3_2);

            Point3d pointToParametrize_3 = new Point3d(0, 10, 0);
            p.GetPointParametrization(pointToParametrize_3, out Polygon3d _, out double N1_3, out double N2_3, out double N3_3);

            Point3d pointToParametrize_4 = new Point3d(3.33333, 3.333333, 0);
            p.GetPointParametrization(pointToParametrize_4, out Polygon3d _, out double N1_4, out double N2_4, out double N3_4);

            double expN1_1 = 1;
            double expN2_1 = 0;
            double expN3_1 = 0;

            double expN1_2 = 0;
            double expN2_2 = 1;
            double expN3_2 = 0;

            double expN1_3 = 0;
            double expN2_3 = 1;
            double expN3_3 = 0;

            double expN1_4 = 0;
            double expN2_4 = 0;
            double expN3_4 = 1;

            //Assert;
            Assert.IsTrue(N1_1 == expN1_1);
            Assert.IsTrue(N2_1 == expN2_1);
            Assert.IsTrue(N3_1 == expN3_1);

            Assert.IsTrue(N1_2 == expN1_2 );
            Assert.IsTrue(N2_2 == expN2_2 );
            Assert.IsTrue(N3_2 == expN3_2);

            Assert.IsTrue(N1_3 == expN1_3);
            Assert.IsTrue(N2_3 == expN2_3);
            Assert.IsTrue(N3_3 == expN3_3);

            Assert.IsTrue(Math.Abs(N1_4 - expN1_4)<0.001);
            Assert.IsTrue(Math.Abs(N2_4 - expN2_4)<0.001);
            Assert.IsTrue(Math.Abs(N3_4 - expN3_4)<0.001);
        }

        [TestMethod]
        public void Parametrization2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d pointToParametrize_1 = new Point3d(5, 0, 0);
            p.GetPointParametrization(pointToParametrize_1, out Polygon3d _, out double N1_1, out double N2_1, out double N3_1);

            Point3d pointToParametrize_2 = new Point3d(5, 5, 0);
            p.GetPointParametrization(pointToParametrize_2, out Polygon3d _, out double N1_2, out double N2_2, out double N3_2);

            Point3d pointToParametrize_3 = new Point3d(0, 2, 0);
            p.GetPointParametrization(pointToParametrize_3, out Polygon3d _, out double N1_3, out double N2_3, out double N3_3);

            double expN1_1 = 0.5;
            double expN2_1 = 0.5;
            double expN3_1 = 0;

            double expN1_2 = 0.5;
            double expN2_2 = 0.5;
            double expN3_2 = 0;

            double expN1_3 = 0.2;
            double expN2_3 = 0.8;
            double expN3_3 = 0;

            //Assert;
            Assert.IsTrue(N1_1 == expN1_1);
            Assert.IsTrue(N2_1 == expN2_1);
            Assert.IsTrue(N3_1 == expN3_1);

            Assert.IsTrue(N1_2 == expN1_2);
            Assert.IsTrue(N2_2 == expN2_2);
            Assert.IsTrue(N3_2 == expN3_2);

            Assert.IsTrue(N1_3 == expN1_3);
            Assert.IsTrue(N2_3 == expN2_3);
            Assert.IsTrue(N3_3 == expN3_3);
        }

        [TestMethod]
        public void ParametrizationTestQuad()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d pointToParametrize_1 = new Point3d(0, 0, 0);
            p.GetPointParametrization(pointToParametrize_1, out Polygon3d poly_1, out double N1_1, out double N2_1, out double N3_1);

            Point3d pointToParametrize_2 = new Point3d(10, 0, 0);
            p.GetPointParametrization(pointToParametrize_2, out Polygon3d poly_2, out double N1_2, out double N2_2, out double N3_2);

            Point3d pointToParametrize_3 = new Point3d(10, 10, 0);
            p.GetPointParametrization(pointToParametrize_3, out Polygon3d poly_3, out double N1_3, out double N2_3, out double N3_3);

            Point3d pointToParametrize_4 = new Point3d(0, 10, 0);
            p.GetPointParametrization(pointToParametrize_4, out Polygon3d _, out double N1_4, out double N2_4, out double N3_4);

            double expN1_1 = 1;
            double expN2_1 = 0;
            double expN3_1 = 0;

            double expN1_2 = 0;
            double expN2_2 = 1;
            double expN3_2 = 0;

            double expN1_3 = 0;
            double expN2_3 = 1;
            double expN3_3 = 0;

            double expN1_4 = 0;
            double expN2_4 = 1;
            double expN3_4 = 0;

            //Assert;
            Assert.IsTrue(N1_1 == expN1_1);
            Assert.IsTrue(N2_1 == expN2_1);
            Assert.IsTrue(N3_1 == expN3_1);
            Assert.IsTrue(poly_1[0] == new Point3d(0, 0, 0));
            Assert.IsTrue(poly_1[1] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_1[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(N1_2 == expN1_2);
            Assert.IsTrue(N2_2 == expN2_2);
            Assert.IsTrue(N3_2 == expN3_2);
            Assert.IsTrue(poly_2[0] == new Point3d(0, 0, 0));
            Assert.IsTrue(poly_2[1] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_2[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(N1_3 == expN1_3);
            Assert.IsTrue(N2_3 == expN2_3);
            Assert.IsTrue(N3_3 == expN3_3);
            Assert.IsTrue(poly_3[0] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_3[1] == new Point3d(10, 10, 0));
            Assert.IsTrue(poly_3[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(Math.Abs(N1_4 - expN1_4) < 0.001);
            Assert.IsTrue(Math.Abs(N2_4 - expN2_4) < 0.001);
            Assert.IsTrue(Math.Abs(N3_4 - expN3_4) < 0.001);
        }

        [TestMethod]
        public void ParametrizationTestQuad2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            Point3d pointToParametrize_1 = new Point3d(5, 0, 0);
            p.GetPointParametrization(pointToParametrize_1, out Polygon3d poly_1, out double N1_1, out double N2_1, out double N3_1);

            Point3d pointToParametrize_2 = new Point3d(10, 5, 0);
            p.GetPointParametrization(pointToParametrize_2, out Polygon3d poly_2, out double N1_2, out double N2_2, out double N3_2);

            Point3d pointToParametrize_3 = new Point3d(5, 10, 0);
            p.GetPointParametrization(pointToParametrize_3, out Polygon3d poly_3, out double N1_3, out double N2_3, out double N3_3);

            Point3d pointToParametrize_4 = new Point3d(0, 5, 0);
            p.GetPointParametrization(pointToParametrize_4, out Polygon3d poly_4, out double N1_4, out double N2_4, out double N3_4);

            Point3d pointToParametrize_5 = new Point3d(5, 5, 0);
            p.GetPointParametrization(pointToParametrize_5, out Polygon3d poly_5, out double N1_5, out double N2_5, out double N3_5);

            double expN1_1 = 0.5;
            double expN2_1 = 0.5;
            double expN3_1 = 0;

            double expN1_2 = 0.5;
            double expN2_2 = 0.5;
            double expN3_2 = 0;

            double expN1_3 = 0.5;
            double expN2_3 = 0.5;
            double expN3_3 = 0;

            double expN1_4 = 0.5;
            double expN2_4 = 0.5;
            double expN3_4 = 0;

            double expN1_5 = 0;
            double expN2_5 = 0;
            double expN3_5 = 1;

            //Assert;
            Assert.IsTrue(Math.Abs(N1_1 - expN1_1) < 0.001);
            Assert.IsTrue(Math.Abs(N2_1 - expN2_1) < 0.001);
            Assert.IsTrue(Math.Abs(N3_1 - expN3_1) < 0.001);
            Assert.IsTrue(poly_1[0] == new Point3d(0, 0, 0));
            Assert.IsTrue(poly_1[1] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_1[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(Math.Abs(N1_2 - expN1_2) < 0.001);
            Assert.IsTrue(Math.Abs(N2_2 - expN2_2) < 0.001);
            Assert.IsTrue(Math.Abs(N3_2 - expN3_2) < 0.001);
            Assert.IsTrue(poly_2[0] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_2[1] == new Point3d(10, 10, 0));
            Assert.IsTrue(poly_2[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(Math.Abs(N1_3 - expN1_3) < 0.001);
            Assert.IsTrue(Math.Abs(N2_3 - expN2_3) < 0.001);
            Assert.IsTrue(Math.Abs(N3_3 - expN3_3) < 0.001);
            Assert.IsTrue(poly_3[0] == new Point3d(10, 10, 0));
            Assert.IsTrue(poly_3[1] == new Point3d(0, 10, 0));
            Assert.IsTrue(poly_3[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(Math.Abs(N1_4 - expN1_4) < 0.001);
            Assert.IsTrue(Math.Abs(N2_4 - expN2_4) < 0.001);
            Assert.IsTrue(Math.Abs(N3_4 - expN3_4) < 0.001);
            Assert.IsTrue(poly_4[0] == new Point3d(0, 10, 0));
            Assert.IsTrue(poly_4[1] == new Point3d(0, 0, 0));
            Assert.IsTrue(poly_4[2] == new Point3d(5, 5, 0));

            Assert.IsTrue(Math.Abs(N1_5 - expN1_5) < 0.001);
            Assert.IsTrue(Math.Abs(N2_5 - expN2_5) < 0.001);
            Assert.IsTrue(Math.Abs(N3_5 - expN3_5) < 0.001);
            Assert.IsTrue(poly_5[0] == new Point3d(0, 0, 0));
            Assert.IsTrue(poly_5[1] == new Point3d(10, 0, 0));
            Assert.IsTrue(poly_5[2] == new Point3d(5, 5, 0));
        }

        [TestMethod]
        public void Transform4Verticles()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(2, 0, 0),
                new Point3d(2, 2, 0),
                new Point3d(0, 2, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(10, 10, 0);
            Point3d point5 = new Point3d(8, 5, 0);
            Point3d point6 = new Point3d(5, 2, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(1, 1, 0);
            Point3d expPoint2 = new Point3d(1, 0, 0);
            Point3d expPoint3 = new Point3d(0, 1, 0);
            Point3d expPoint4 = new Point3d(2, 2, 0);
            Point3d expPoint5 = new Point3d(1.6, 1, 0);
            Point3d expPoint6 = new Point3d(1, 0.4, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void Transform4VerticlesRotation()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(2, 2, 0),
                new Point3d(0, 2, 0),
                new Point3d(0, 0, 0),
                new Point3d(2, 0, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(10, 10, 0);
            Point3d point5 = new Point3d(8, 5, 0);
            Point3d point6 = new Point3d(5, 2, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(1, 1, 0);
            Point3d expPoint2 = new Point3d(1, 2, 0);
            Point3d expPoint3 = new Point3d(2, 1, 0);
            Point3d expPoint4 = new Point3d(0, 0, 0);
            Point3d expPoint5 = new Point3d(0.4, 1, 0);
            Point3d expPoint6 = new Point3d(1, 1.6, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void Transform4VerticlesScale()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(20, 0, 0),
                new Point3d(20, 2, 0),
                new Point3d(0, 2, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(10, 10, 0);
            Point3d point5 = new Point3d(8, 5, 0);
            Point3d point6 = new Point3d(5, 2, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(10, 1, 0);
            Point3d expPoint2 = new Point3d(10, 0, 0);
            Point3d expPoint3 = new Point3d(0, 1, 0);
            Point3d expPoint4 = new Point3d(20, 2, 0);
            Point3d expPoint5 = new Point3d(16, 1, 0);
            Point3d expPoint6 = new Point3d(10, 0.4, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void Transform4VerticlesScaleAndRotate()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(20, 2, 0),
                new Point3d(0, 2, 0),
                new Point3d(0, 0, 0),
                new Point3d(20, 0, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(10, 10, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);

            Point3d expPoint1 = new Point3d(10, 1, 0);
            Point3d expPoint2 = new Point3d(10, 2, 0);
            Point3d expPoint3 = new Point3d(20, 1, 0);
            Point3d expPoint4 = new Point3d(0, 0, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);
        }

        [TestMethod]
        public void Transform4VerticlesToTrapezoid()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(15, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(5, 10, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(0, 5, 0);
            Point3d point3 = new Point3d(10, 5, 0);
            Point3d point4 = new Point3d(10, 10, 0);
            Point3d point5 = new Point3d(0, 0, 0);
            Point3d point6 = new Point3d(10, 0, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(7.5, 5, 0);
            Point3d expPoint2 = new Point3d(2.5, 5, 0);
            Point3d expPoint3 = new Point3d(12.5, 5, 0);
            Point3d expPoint4 = new Point3d(10, 10, 0);
            Point3d expPoint5 = new Point3d(0, 0, 0);
            Point3d expPoint6 = new Point3d(15, 0, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void Transform3Verticles()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(2, 0, 0),
                new Point3d(0, 2, 0),
            };

            //Act
            Point3d point1 = new Point3d(5, 5, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(2, 2, 0);
            Point3d point5 = new Point3d(8, 1, 0);
            Point3d point6 = new Point3d(1, 5, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(1, 1, 0);
            Point3d expPoint2 = new Point3d(1, 0, 0);
            Point3d expPoint3 = new Point3d(0, 1, 0);
            Point3d expPoint4 = new Point3d(0.4, 0.4, 0);
            Point3d expPoint5 = new Point3d(1.4, 0.0, 0);
            Point3d expPoint6 = new Point3d(0.2, 1, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void Transform5Verticles()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(5, 12, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(5, 0, 0),
                new Point3d(10, 5, 0),
                new Point3d(5, 10, 0),
                new Point3d(0, 5, 0),
            };

            //Act
            Point3d point1 = new Point3d(7.5, 11, 0);
            Point3d point2 = new Point3d(5, 0, 0);
            Point3d point3 = new Point3d(0, 5, 0);
            Point3d point4 = new Point3d(5, 12, 0);
            Point3d point5 = new Point3d(0, 0, 0);
            Point3d point6 = new Point3d(10, 5, 0);

            Point3d newPoint1 = p.Transform(point1, q);
            Point3d newPoint2 = p.Transform(point2, q);
            Point3d newPoint3 = p.Transform(point3, q);
            Point3d newPoint4 = p.Transform(point4, q);
            Point3d newPoint5 = p.Transform(point5, q);
            Point3d newPoint6 = p.Transform(point6, q);

            Point3d expPoint1 = new Point3d(7.5, 7.5, 0);
            Point3d expPoint2 = new Point3d(2.5, 0, 0);
            Point3d expPoint3 = new Point3d(0, 2.5, 0);
            Point3d expPoint4 = new Point3d(5, 10, 0);
            Point3d expPoint5 = new Point3d(0, 0, 0);
            Point3d expPoint6 = new Point3d(7.5, 2.5, 0);

            //Assert;
            Assert.IsTrue(Math.Abs(expPoint1.X - newPoint1.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Y - newPoint1.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint1.Z - newPoint1.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint2.X - newPoint2.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Y - newPoint2.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint2.Z - newPoint2.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint3.X - newPoint3.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Y - newPoint3.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint3.Z - newPoint3.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint4.X - newPoint4.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Y - newPoint4.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint4.Z - newPoint4.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint5.X - newPoint5.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Y - newPoint5.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint5.Z - newPoint5.Z) < 0.001);

            Assert.IsTrue(Math.Abs(expPoint6.X - newPoint6.X) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Y - newPoint6.Y) < 0.001);
            Assert.IsTrue(Math.Abs(expPoint6.Z - newPoint6.Z) < 0.001);
        }

        [TestMethod]
        public void ShiftedEquals1()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
                new Point3d(0, 0, 0),
            };

            Assert.IsTrue(p.EqualsShifted(q));
        }

        [TestMethod]
        public void ShiftedEquals2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
                new Point3d(2, 0, 0),
            };

            Assert.IsFalse(p.EqualsShifted(q));
        }

        [TestMethod]
        public void ShiftedEquals3()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(5, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 12, 0),
                new Point3d(0, 10, 0),
            };

            Polygon3d q = new Polygon3d()
            {
                new Point3d(10, 12, 0),
                new Point3d(0, 10, 0),
                new Point3d(5, 0, 0),
                new Point3d(10, 0, 0),
            };

            Assert.IsTrue(p.EqualsShifted(q));
        }

        [TestMethod]
        public void Reverse1()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
                new Point3d(0, 10, 0),
            };

            //Act
            p.Reverse();

            Polygon3d expP = new Polygon3d { new Point3d(0, 10, 0), new Point3d(10, 10, 0), new Point3d(10, 0, 0), new Point3d(0, 0, 0) };

            //Assert;
            Assert.IsTrue(p.Equals(expP));
            Assert.IsTrue(p[0] == new Point3d(0, 10, 0));
            Assert.IsTrue(p[1] == new Point3d(10, 10, 0));
            Assert.IsTrue(p[2] == new Point3d(10, 0, 0));
            Assert.IsTrue(p[3] == new Point3d(0, 0, 0));
        }

        [TestMethod]
        public void Reverse2()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(10, 0, 0),
                new Point3d(10, 10, 0),
            };

            //Act
            p.Reverse();

            Polygon3d expP = new Polygon3d { new Point3d(10, 10, 0), new Point3d(10, 0, 0), new Point3d(0, 0, 0) };

            //Assert;
            Assert.IsTrue(p.Equals(expP));
            Assert.IsTrue(p[0] == new Point3d(10, 10, 0));
            Assert.IsTrue(p[1] == new Point3d(10, 0, 0));
            Assert.IsTrue(p[2] == new Point3d(0, 0, 0));
        }

        [TestMethod]
        public void Scale1()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(1, 0, 0),
                new Point3d(2, 1, 0),
                new Point3d(0, 3, 0),
                new Point3d(-1, 2, 0),
                new Point3d(-2, -1, 0),
            };

            var scaled = p.Scale(2);

            Assert.IsTrue(p != scaled);
            Assert.AreEqual(p.Explode()[0].GetLength() * 2.0, scaled.Explode()[0].GetLength(), 0.01, $"{p.Explode()[0].GetLength()}  {scaled.Explode()[0].GetLength()}" );

        }

        [TestMethod]
        public void IsPointInside()
        {
            //Arrange
            Polygon3d p = new Polygon3d()
            {
                new Point3d(150.011449006273, 15.0155822100663, 0),
                new Point3d(135.01274370134, 15.0155302112964, 0),
                new Point3d(135, 0, 0),
                new Point3d(150, 0, 0),
            };

            Point3d pointToTest = new Point3d(142.506048176903, 7.50777810534067, 0);

			bool isPointInside = p.IsPointInside(pointToTest);

            Assert.IsTrue(isPointInside == true);
        }
    }    
}

