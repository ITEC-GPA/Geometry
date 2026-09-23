using System;
using GPC.Geometry;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class BoundingBox2dTest : UnitTestBase
    {
        [TestMethod]
        public void BoundingBox1()
        {
            // Arrange
            Polygon2d P1 = new Polygon2d()
            {
                new Point2d(0,0),
                new Point2d(10,0),
                new Point2d(10,10),
                new Point2d(0,10),
            };

            // Act
            BoundingBox2d boundingBox = new BoundingBox2d(P1);

            // Assert
            Assert.IsTrue(boundingBox.Min == new Point2d(0, 0));
            Assert.IsTrue(boundingBox.Max == new Point2d(10, 10));
        }

        [TestMethod]
        public void BoundingBox2()
        {
            // Arrange
            Polygon2d P1 = new Polygon2d()
            {
                new Point2d(0,0),
                new Point2d(10,0),
                new Point2d(5,10),
            };

            // Act
            BoundingBox2d boundingBox = new BoundingBox2d(P1);

            // Assert
            Assert.IsTrue(boundingBox.Min == new Point2d(0, 0));
            Assert.IsTrue(boundingBox.Max == new Point2d(10, 10));
        }

        [TestMethod]
        public void BoundingBox3()
        {
            // Arrange
            Polygon2d P1 = new Polygon2d()
            {
                new Point2d(0,0),
                new Point2d(10,0),
                new Point2d(5,10),
            };

            // Act
            BoundingBox2d boundingBox = new BoundingBox2d(P1);
            boundingBox.Scale(2);

            // Assert
            Assert.IsTrue(boundingBox.Min == new Point2d(-5, -5));
            Assert.IsTrue(boundingBox.Max == new Point2d(15, 15));
        }

        [TestMethod]
        public void BoundingBox4()
        {
            // Arrange
            Polygon2d P1 = new Polygon2d()
            {
                new Point2d(0,0),
                new Point2d(10,0),
                new Point2d(10,10),
                new Point2d(0,10),
            };

            // Act
            BoundingBox2d boundingBox = new BoundingBox2d(P1);
            boundingBox.Scale(10);

            // Assert
            Assert.IsTrue(boundingBox.Min == new Point2d(-45, -45));
            Assert.IsTrue(boundingBox.Max == new Point2d(55, 55));
        }
    }
}
