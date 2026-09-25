using System;
using System.Linq;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.Geometry
{
    /// <summary>
    /// RemoveAlignedPoints with the tolerance on the distance and the Newell normal of the polygons (September 2026)
    /// </summary>
    [TestClass]
    public class AlignedPointsTest
    {
        /// <summary>
        /// The distance of a point from the segment between two points
        /// </summary>
        private static double DistanceFromSegment(Point2d p, Point2d a, Point2d b)
        {
            double ux = b.X - a.X, uy = b.Y - a.Y;
            double length2 = ux * ux + uy * uy;
            double t = length2 > 0 ? ((p.X - a.X) * ux + (p.Y - a.Y) * uy) / length2 : 0;
            t = Math.Max(0, Math.Min(1, t));
            double dx = p.X - (a.X + t * ux), dy = p.Y - (a.Y + t * uy);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// The distance of a point from the border of a polygon
        /// </summary>
        private static double DistanceFromBorder(Point2d p, Polygon2d polygon)
        {
            double minimum = double.MaxValue;
            for (int i = 0; i < polygon.Count; i++)
                minimum = Math.Min(minimum, DistanceFromSegment(p, polygon[i], polygon[(i + 1) % polygon.Count]));
            return minimum;
        }

        [TestMethod]
        public void RemovedPointsStayWithinTheToleranceFromTheFinalSides()
        {
            // an arc y = a x² with small steps: every vertex is within the tolerance from the line of its original neighbours, but the chord
            // of the whole arc is 1.25e-3 far from it. Before, all the vertices of the arc were removed
            const double tolerance = 1e-4, a = 5e-5;
            var points = Enumerable.Range(0, 11).Select(i => new Point2d(i, a * i * i)).ToList();
            points.Add(new Point2d(10, -5));
            points.Add(new Point2d(0, -5));
            var original = new Polygon2d(points.ToArray());
            var polygon = new Polygon2d(points.ToArray());

            polygon.RemoveAlignedPoints(tolerance);

            Assert.IsTrue(polygon.Count < original.Count, polygon.Count.ToString());
            Assert.IsTrue(polygon.Count > 4, polygon.Count.ToString());
            for (int i = 0; i < original.Count; i++)
                Assert.IsTrue(DistanceFromBorder(original[i], polygon) <= tolerance * (1 + 1e-9), $"{original[i]}: {DistanceFromBorder(original[i], polygon)}");
        }

        [TestMethod]
        public void TheToleranceIsADistance()
        {
            // the vertex (10000, 0.4) deviates 8e-5 rad from the line of its neighbours (the old angular tolerance removed it), but it is 0.4 far
            var polygon = new Polygon2d(new[] { new Point2d(0, 0), new Point2d(10000, 0.4), new Point2d(20000, 0), new Point2d(10000, 5000) });
            polygon.RemoveAlignedPoints();
            Assert.AreEqual(4, polygon.Count);

            // a vertex 5e-5 far from the side is removed
            polygon = new Polygon2d(new[] { new Point2d(0, 0), new Point2d(0.5, 0.00005), new Point2d(1, 0), new Point2d(1, 1), new Point2d(0, 1) });
            polygon.RemoveAlignedPoints();
            Assert.AreEqual(4, polygon.Count);
        }

        [TestMethod]
        public void SpikesAndRepeatedPointsAreRemoved()
        {
            // (4, 0) is on the line of its neighbours (0, 0) and (2, 0), outside their segment
            var polygon = new Polygon3d(new[] { new Point3d(0, 0, 0), new Point3d(4, 0, 0), new Point3d(2, 0, 0), new Point3d(2, 2, 0) });
            polygon.RemoveAlignedPoints();
            Assert.AreEqual(3, polygon.Count);
            Assert.IsTrue(polygon[0].Equals(new Point3d(0, 0, 0)));
            Assert.IsTrue(polygon[1].Equals(new Point3d(2, 0, 0)));
            Assert.IsTrue(polygon[2].Equals(new Point3d(2, 2, 0)));

            // repeated consecutive vertices
            var square = new Polygon2d(new[] { new Point2d(0, 0), new Point2d(1, 0), new Point2d(1, 0), new Point2d(1, 1), new Point2d(0, 1), new Point2d(0, 0) });
            square.RemoveAlignedPoints();
            Assert.AreEqual(4, square.Count);
        }

        [TestMethod]
        public void AllAlignedKeepsTheExtremesInTheirOrder()
        {
            var polygon = new Polygon3d(new[] { new Point3d(0, 0, 3), new Point3d(0, 0, -5), new Point3d(0, 0, 2), new Point3d(0, 0, 6), new Point3d(0, 0, 1) });
            polygon.RemoveAlignedPoints();
            Assert.AreEqual(2, polygon.Count);
            Assert.AreEqual(-5, polygon[0].Z, 1e-12);
            Assert.AreEqual(6, polygon[1].Z, 1e-12);
        }

        [TestMethod]
        public void CoordinateSystemAndPlaneOfAConcavePolygonFollowTheNormal()
        {
            // L shape counterclockwise in XY starting before the concave vertex (1, 1): the first three vertices turn clockwise. Before, the Z
            // axis and the normal of the plane were -Z
            var points = new[] { new Point3d(2, 1, 0), new Point3d(1, 1, 0), new Point3d(1, 2, 0), new Point3d(0, 2, 0), new Point3d(0, 0, 0), new Point3d(2, 0, 0) };
            var polygon = new Polygon3d(points);

            CoordinateSystem cs = polygon.GetCoordinateSystem();
            Assert.AreEqual(1, cs.V3.Z, 1e-12);
            Assert.AreEqual(-1, cs.V1.X, 1e-12); // along the first side, from (2, 1) to (1, 1)

            Plane plane = new Shape(polygon).GetPlane();
            Assert.AreEqual(1, plane.Normal.Z, 1e-12);

            Assert.IsTrue(polygon.GetNormalVector().Z > 0.999999);
        }
    }
}
