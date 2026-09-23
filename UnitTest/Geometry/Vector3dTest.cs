using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Geometry;

using Maffeis.TestUtilities;
namespace Geometry
{
    [TestClass]
    public class Vector3dTest : UnitTestBase
    {
        protected double _tolerance = 0.001;

        [TestMethod]
        public void Unitize()
        {
            Vector3d v = new Vector3d(-114.0132, -165.3672, -38.9244);
            v.Unitize();
            double len = v.Length;
            bool isUnitized = (len - 1.0) < 1.0E-5;
            Assert.IsTrue(isUnitized, $"Not unitized, Len: {len}");
        }

        [TestMethod]
        public void Products()
        {
            Vector3d v1 = new Vector3d(-114.0132, -165.3672, -38.9244);
            Vector3d v2 = new Vector3d(-48.0409335, 135.7966119, 49.2101354);

            double dot_expected = -18894.479914431235;
            double dot_found = v1 * v2;
            double diff = dot_expected - dot_found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {dot_expected}, Result: {dot_found}, Difference: {diff}");

            Vector3d cross_expected = new Vector3d(-2851.94066247852, 7480.56952131468, -23427.0009301583);
            Vector3d cross_found = v1 ^ v2;
            diff = cross_expected.Length - cross_found.Length;
            Assert.IsTrue(diff < _tolerance, $"Expected: {cross_expected}, Result: {cross_found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle1()
        {
            Vector3d v1 = new Vector3d(-114.0132, -165.3672, -38.9244);
            Vector3d v2 = new Vector3d(-48.0409335, 135.7966119, 49.2101354);
            double expected = 2.2227;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle2()
        {
            Vector3d v1 = new Vector3d(-48.09, 27.38, 0.00);
            Vector3d v2 = new Vector3d(13.64, 23.96, 0.00);
            double expected = 3.14 / 2.0;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Revert()
        {
            Vector3d v = new Vector3d(-114.0132, -165.3672, -38.9244);
            Vector3d expected = new Vector3d(114.0132, 165.3672, 38.9244);
            Vector3d found = Vector3d.Reverse(v);
            bool success = found == expected;
            Assert.IsTrue(success, "Failed to refert");
        }

        [TestMethod]
        public void Angle3()
        {
            Vector3d v1 = new Vector3d(0, 10, 0);
            Vector3d v2 = new Vector3d(0, 0, 10);
            double expected = -Math.PI/2;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle4()
        {
            Vector3d v1 = new Vector3d(10, 0, 0);
            Vector3d v2 = new Vector3d(10, 10, 0);
            double expected = Math.PI/4;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle3_2()
        {
            Vector3d v1 = new Vector3d(0, -10, 0);
            Vector3d v2 = new Vector3d(0, 0, -10);
            double expected = Math.PI / 2;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle4_2()
        {
            Vector3d v1 = new Vector3d(-10, 0, 0);
            Vector3d v2 = new Vector3d(-10, -10, 0);
            double expected = Math.PI / 4;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle3_3()
        {
            Vector3d v2 = new Vector3d(0, -10, 0);
            Vector3d v1 = new Vector3d(0, 0, -10);
            double expected = Math.PI / 2;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle4_3()
        {
            Vector3d v2 = new Vector3d(-10, 0, 0);
            Vector3d v1 = new Vector3d(-10, -10, 0);
            double expected = - Math.PI / 4;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle5()
        {
            Vector3d v1 = new Vector3d(10, 0, 0);
            Vector3d v2 = new Vector3d(-10, 0, 0);
            double expected = Math.PI;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle5_1()
        {
            Vector3d v1 = new Vector3d(10, 0, 0);
            Vector3d v2 = new Vector3d(10, 0, 0);
            double expected = 0;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle5_2()
        {
            Vector3d v1 = new Vector3d(0.010, 0, 0);
            Vector3d v2 = new Vector3d(-0.010, 0, 0);
            double expected = 0;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle5_3()
        {
            Vector3d v1 = new Vector3d(0.010, 0, 0);
            Vector3d v2 = new Vector3d(0, 0.10, 0);
            double expected = Math.PI / 2;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle5_4()
        {
            Vector3d v1 = new Vector3d(0, -1, 0);
            Vector3d v2 = new Vector3d(0, 0.10, 0);
            double expected = Math.PI;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Angle6_1()
        {
            Vector3d v1 = new Vector3d(0, 0, 500*1000);
            Vector3d v2 = new Vector3d(100000, 100000, 397*1000);
            double expected = 0;
            double found = v1.AngleTo(v2);
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }
    }
}
