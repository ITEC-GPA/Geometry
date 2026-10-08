using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class OctoberCorrectionsTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        [TestMethod]
        public void SegmentIntersectionDoesNotDependOnTheModelScale()
        {
            foreach (double length in new[] { 0.001, 0.01, 1.0, 1000.0 })
            {
                var a = new Line3d(P(0, 0), P(length, 0));
                var b = new Line3d(P(length / 2, -length / 2), P(length / 2, length / 2));
                Assert.IsTrue(a.GetIntersection(b, out Point3d intersection), "Length: " + length);
                Assert.AreEqual(length / 2, intersection.X, length * 1e-9);
                Assert.AreEqual(0, intersection.Y, length * 1e-9);
                Assert.IsTrue(a.GetIntersectionWithInfiniteLine(b, out _));
                Assert.IsFalse(a.GetIntersection(new Line3d(P(0, length), P(length, length)), out _));
                var a2 = new Line2d(new Point2d(0, 0), new Point2d(length, 0));
                var b2 = new Line2d(new Point2d(length / 2, -length / 2), new Point2d(length / 2, length / 2));
                Assert.IsTrue(a2.GetIntersection(b2, out _));
            }
        }
    }
}
