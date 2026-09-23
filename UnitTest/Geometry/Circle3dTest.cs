using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Circle3dTest : UnitTestBase
    {    
        [TestMethod]
        public void GetCircle1()
        {
            Point3d p1 = new Point3d(2, 2, 0);
            Point3d p2 = new Point3d(-2, 2, 0);
            Point3d p3 = new Point3d(-2, -2, 0);
            Circle3d circle = new Circle3d(p1, p2, p3);

            Point3d expCenter = new Point3d(0, 0, 0);
            double expR = p1.DistanceTo(expCenter);

            // Assert
            Assert.IsTrue(Math.Abs(circle.Center.X - expCenter.X) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Y - expCenter.Y) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Z - expCenter.Z) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Radius - expR) < 0.01);
        }

        [TestMethod]
        public void GetCircle2()
        {
            Point3d p1 = new Point3d(5.000, 10.925, 11.614);
            Point3d p2 = new Point3d(13.880, 5.000, 5.000);
            Point3d p3 = new Point3d(5.000, -0.925, -1.614);
            Circle3d circle = new Circle3d(p1, p2, p3);

            Point3d expCenter = new Point3d(5, 5, 5);
            double expR = p1.DistanceTo(expCenter);

            // Assert
            Assert.IsTrue(Math.Abs(circle.Center.X - expCenter.X) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Y - expCenter.Y) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Z - expCenter.Z) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Radius - expR) < 0.01);
        }

        [TestMethod]
        public void GetCircle3()
        {
            Point3d p1 = new Point3d(11.614, -0.925, 5.000);
            Point3d p2 = new Point3d(5.000, 5.000, 13.880);
            Point3d p3 = new Point3d(-1.614, 10.925, 5.000);
            Circle3d circle = new Circle3d(p1, p2, p3);

            Point3d expCenter = new Point3d(5, 5, 5);
            double expR = p1.DistanceTo(expCenter);

            // Assert
            Assert.IsTrue(Math.Abs(circle.Center.X - expCenter.X) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Y - expCenter.Y) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Center.Z - expCenter.Z) < 0.01);
            Assert.IsTrue(Math.Abs(circle.Radius - expR) < 0.01);
        }

        [TestMethod]
        public void IsPointInside()
        {
            Point3d p1 = new Point3d(-0.3770130, 9.8166312, -0.1712856);
            Point3d p2 = new Point3d(5.0000000, 5.0000000, -3.8800000);
            Point3d p3 = new Point3d(-1.3285406, 10.6689925, 7.5819562);
            Circle3d circle = new Circle3d(p1, p2, p3);

            Point3d point1 = new Point3d(9.1112087, 1.3172535, 11.9562735);
            Point3d point2 = new Point3d(5.0000000, 5.0000000, -3.8800000);
            Point3d point3 = new Point3d(7.0556043, 3.1586267, 4.0381367);
            Point3d point4 = new Point3d(2.3114935, 7.4083156, -2.0256428);
            Point3d point5 = new Point3d(-0.3770130, 9.8166312, -0.1712856);
            Point3d point6 = new Point3d(-0.8527768, 10.2428119, 3.7053353);

            // Assert
            Assert.IsTrue(circle.IsPointInside(point1));
            Assert.IsTrue(circle.IsPointInside(point2));
            Assert.IsTrue(circle.IsPointInside(point3));
            Assert.IsTrue(circle.IsPointInside(point4));
            Assert.IsTrue(circle.IsPointInside(point5));
            Assert.IsTrue(circle.IsPointInside(point6));
        }

        [TestMethod]
        public void IsPointInside2()
        {
            Point3d p1 = new Point3d(3, 0, 0);
            Point3d p2 = new Point3d(0, 3, 0);
            Point3d p3 = new Point3d(0, -3, 0);
            Circle3d circle = new Circle3d(p1, p2, p3);

            Point3d point1 = new Point3d(1,1,0) ;
            Point3d point2 = new Point3d(2,2,0);
            Point3d point3 = new Point3d(3,0,0);
            Point3d point4 = new Point3d(-3,0,0);
            Point3d point5 = new Point3d(-10,-10,0);
            Point3d point6 = new Point3d(0,0,1);

            // Assert
            Assert.IsTrue(circle.IsPointInside(point1));
            Assert.IsTrue(circle.IsPointInside(point2));
            Assert.IsTrue(circle.IsPointInside(point3));
            Assert.IsTrue(circle.IsPointInside(point4));
            Assert.IsFalse(circle.IsPointInside(point5));
            Assert.IsFalse(circle.IsPointInside(point6));
        }
        [TestMethod]
        public void GetArcCircle1()
        {
            Point3d p1 = new Point3d(10, 0, 0);
            Point3d p2 = new Point3d(0, 10, 0);
            Point3d p3 = new Point3d(-10, 0, 0);
            Circle3dArc arc = new Circle3dArc(p1, p2, p3, 0.0001);

            Point3d expCenter = new Point3d(0, 0, 0);
            double expR = p1.DistanceTo(expCenter);

            Point3d p4 = new Point3d(7.0710678, 7.0710678, 0);
            Point3d p5 = new Point3d(-7.0710678, 7.0710678, 0);
            Point3d p6 = new Point3d(-8, 7, 0);
            Point3d p7 = new Point3d(8.660254, 5, 0);
            Point3d p8 = new Point3d(-8.660254, 5, 0);

            // Assert
            Assert.IsTrue(Math.Abs(arc.Center.X - expCenter.X) < 0.01);
            Assert.IsTrue(Math.Abs(arc.Center.Y - expCenter.Y) < 0.01);
            Assert.IsTrue(Math.Abs(arc.Center.Z - expCenter.Z) < 0.01);
            Assert.IsTrue(arc.IsPointOnCircleArc(p1));
            Assert.IsTrue(arc.IsPointOnCircleArc(p2));
            Assert.IsTrue(arc.IsPointOnCircleArc(p3));
            Assert.IsTrue(arc.IsPointOnCircleArc(p4));
            Assert.IsTrue(arc.IsPointOnCircleArc(p5));
            Assert.IsFalse(arc.IsPointOnCircleArc(p6));
            Assert.IsTrue(arc.IsPointOnCircleArc(p7));
            Assert.IsTrue(arc.IsPointOnCircleArc(p8));
        }

    }
}
