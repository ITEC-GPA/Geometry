using System;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Point3dTest : UnitTestBase
    {
        protected double _tolerance = 0.001;

        [TestMethod]
        public void Distance()
        {
            Point3d p1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d p2 = new Point3d(-31.4545, -75.2447, -17.5889);
            double expected = 204.410324;
            double found = p1.DistanceTo(p2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void SquareDistance()
        {
            Point3d p1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d p2 = new Point3d(-31.4545, -75.2447, -17.5889);
            double expected = 41783.580557784976;
            double found = p1.SquareDistanceTo(p2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void VectorTo1()
        {
            Point3d p1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d p2 = new Point3d(-31.4545, -75.2447, -17.5889);
            Vector3d expected = new Vector3d(-114.0132, -165.3672, -37.9244);
            Vector3d found = p1.VectorTo(p2);
            double diff = expected.Length - found.Length;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");

            p1 = new Point3d(56.57, 56.57, 0);
            p2 = new Point3d(56.57, -56.57, 0);
            expected = new Vector3d(0, -113.14, 0);
            found = p1.VectorTo(p2);
            diff = expected.Length - found.Length;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");


            p1 = new Point3d(0, 0, 0);
            p2 = new Point3d(500, 500, 5);

            expected = new Vector3d(500, 500, 5);
            found = p1.VectorTo(p2);
            diff = expected.Length - found.Length;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Equals()
        {
            Point3d p1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d p2 = new Point3d(82.5587001, 90.1225001, 20.3355001);
            bool equal = p1 == p2;
            double diff = p1.DistanceTo(p2);
            Assert.IsTrue(equal, $"Expected: True, Difference: {diff}");
        }


        [TestMethod]
        public void Pan()
        {
            Point3d p1 = new Point3d(10, 20, 30);

            p1.Move(new Vector3d(100, 200, 300));

            Assert.IsTrue(p1.X == 110 && p1.Y == 220 && p1.Z == 330, p1.ToString());
        }


        [TestMethod]
        public void TryParse()
        {
            Point3d pt = new Point3d(10.4, 11.5, 12.6);
            string s = pt.ToString();

            bool success = Point3d.TryParse(s, out Point3d result);
            Assert.IsTrue(success, "Failed to convert");
            Assert.IsTrue(result.X == pt.X, "Failed to convert X");
            Assert.IsTrue(result.Y == pt.Y, "Failed to convert X");
            Assert.IsTrue(result.Z == pt.Z, "Failed to convert X");
        }
    }
}
