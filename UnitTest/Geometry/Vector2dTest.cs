using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Geometry;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Vector2dTest : UnitTestBase
    {
        protected double _tolerance = 0.001;

        [TestMethod]
        public void Cast()
        {
            Vector2d v2 = new Vector2d(123.22, 44.32);
            Vector3d v3 = (Vector3d)v2;
            double expected = v2.Length;
            double found = v3.Length;
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }
    }
}
