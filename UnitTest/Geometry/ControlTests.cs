using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    /// <summary>
    /// Simple control tests with results computed by hand: points and vectors, segments, planes, circles, polygons, shapes,
    /// coordinate systems, boolean operations and meshes (September 2026)
    /// </summary>
    [TestClass]
    public class ControlTests
    {
        private const double T = 1e-9;

        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        private static Vector3d V(double x, double y, double z) => new Vector3d(x, y, z);

        private static Polygon2d Rectangle(double x, double y, double width, double height) =>
            new Polygon2d(new[] { Q(x, y), Q(x + width, y), Q(x + width, y + height), Q(x, y + height) });

        /// <summary>L shape: 4 x 1 bottom leg and 1 x 2 vertical leg; area 6, centroid (1.5, 1)</summary>
        private static Polygon2d LShape() => new Polygon2d(new[] { Q(0, 0), Q(4, 0), Q(4, 1), Q(1, 1), Q(1, 3), Q(0, 3) });

        private static void AssertPoint(double x, double y, double z, Point3d p, double tolerance = T)
        {
            Assert.AreEqual(x, p.X, tolerance, "X");
            Assert.AreEqual(y, p.Y, tolerance, "Y");
            Assert.AreEqual(z, p.Z, tolerance, "Z");
        }

        private static void AssertPoint(double x, double y, Point2d p, double tolerance = T)
        {
            Assert.AreEqual(x, p.X, tolerance, "X");
            Assert.AreEqual(y, p.Y, tolerance, "Y");
        }

        #region Points and vectors

        [TestMethod]
        public void PointDistances()
        {
            Assert.AreEqual(5.0, Q(1, 1).DistanceTo(Q(4, 5)), T);
            Assert.AreEqual(25.0, Q(1, 1).SquareDistanceTo(Q(4, 5)), T);
            Assert.AreEqual(7.0, P(1, 2, 3).DistanceTo(P(3, 5, 9)), T);
            Assert.AreEqual(0.0, P(1, 2, 3).DistanceTo(P(1, 2, 3)), 0.0);
        }

        [TestMethod]
        public void Point2dRotationIsCounterclockwise()
        {
            var p = Q(2, 1);
            p.Rotate(Q(1, 1), Math.PI / 2);
            AssertPoint(1, 2, p);
            p.Rotate(Q(1, 1), Math.PI);
            AssertPoint(1, 0, p);
            var r = Q(3, 0);
            r.Rotate(Q(0, 0), -Math.PI / 6);
            AssertPoint(3 * Math.Cos(Math.PI / 6), -1.5, r);
        }

        [TestMethod]
        public void Point2dMirrorOnLines()
        {
            // line x - y = 0
            AssertPoint(1, 3, Q(3, 1).Mirror(1, -1, 0));
            // line x = 2
            AssertPoint(-1, 7, Q(5, 7).Mirror(1, 0, -2));
            // a point on the line does not move
            AssertPoint(2, 9, Q(2, 9).Mirror(1, 0, -2));
            // the coefficients do not need to be normalized
            AssertPoint(1, 3, Q(3, 1).Mirror(5, -5, 0));
        }

        [TestMethod]
        public void Point3dMirrorOnPlanes()
        {
            AssertPoint(1, 2, -3, P(1, 2, 5).Mirror(0, 0, 1, -1));
            // plane x + y + z = 0: the origin-symmetric component along (1,1,1) changes sign
            AssertPoint(-1, -1, -1, P(1, 1, 1).Mirror(1, 1, 1, 0));
            // plane 2x + 2y + 2z - 6 = 0 (x + y + z = 3): the origin goes to (2, 2, 2)
            AssertPoint(2, 2, 2, P(0, 0, 0).Mirror(2, 2, 2, -6));
        }

        [TestMethod]
        public void Point3dScaleAroundACenter()
        {
            AssertPoint(5, 5, 5, P(3, 3, 3).Scale(P(1, 1, 1), 2));
            AssertPoint(2, -3, 8, P(1, 1, 1).Scale(2, -3, 8));
            AssertPoint(1, 1, 1, P(3, 3, 3).Scale(P(1, 1, 1), 0));
            AssertPoint(-2, 4, 6, P(-1, 2, 3).Scale(2));
        }

        [TestMethod]
        public void Point3dEqualityUsesTheCombinedTolerance()
        {
            // two points are equal if their distance is not larger than sqrt(2) times the tolerance
            Assert.IsTrue(P(0, 0, 0).Equals(P(1.4e-4, 0, 0)));
            Assert.IsFalse(P(0, 0, 0).Equals(P(1.5e-4, 0, 0)));
            Assert.IsTrue(P(0, 0, 0).Equals(P(0.1, 0, 0), 0.08));
            Assert.IsFalse(P(0, 0, 0).Equals(null));
        }

        [TestMethod]
        public void PointArithmetic()
        {
            AssertPoint(4, 6, 8, P(1, 2, 3) + P(3, 4, 5));
            AssertPoint(-2, -2, -2, P(1, 2, 3) - P(3, 4, 5));
            Assert.AreEqual(1 * 3 + 2 * 4 + 3 * 5, P(1, 2, 3) * P(3, 4, 5), T);
            AssertPoint(0.5, 1, 1.5, P(1, 2, 3) / 2);
            AssertPoint(0, 0, 1, P(1, 0, 0) ^ P(0, 1, 0));
            Point3d fromPoint2d = Q(7, 8);
            AssertPoint(7, 8, 0, fromPoint2d);
            AssertPoint(2, 5, 0, P(1, 2) + V(1, 3, 0));
            AssertPoint(2, 5, Q(1, 2) + new Vector2d(1, 3));
        }

        [TestMethod]
        public void CrossProductOfTheAxes()
        {
            Assert.IsTrue(Vector3d.XAxis.CrossProduct(Vector3d.YAxis).Equals(Vector3d.ZAxis));
            Assert.IsTrue(Vector3d.YAxis.CrossProduct(Vector3d.ZAxis).Equals(Vector3d.XAxis));
            Assert.IsTrue(Vector3d.ZAxis.CrossProduct(Vector3d.XAxis).Equals(Vector3d.YAxis));
            var a = V(1, 2, 3);
            var b = V(-4, 0.5, 2);
            Vector3d ab = a ^ b;
            Vector3d ba = b ^ a;
            Assert.AreEqual(-ab.X, ba.X, T);
            Assert.AreEqual(-ab.Y, ba.Y, T);
            Assert.AreEqual(-ab.Z, ba.Z, T);
            // the cross product is orthogonal to both vectors and its length is |a||b|sin(angle)
            Assert.AreEqual(0, ab * a, T);
            Assert.AreEqual(0, ab * b, T);
            Assert.AreEqual(a.Length * b.Length * Math.Sin(a.AngleTo(b)), ab.Length, 1e-9);
        }

        [TestMethod]
        public void VectorAngles()
        {
            Assert.AreEqual(Math.PI / 4, V(1, 0, 0).AngleTo(V(1, 1, 0)), T);
            Assert.AreEqual(Math.PI, V(1, 2, 3).AngleTo(V(-2, -4, -6)), T);
            Assert.AreEqual(Math.PI / 2, V(0, 0, 5).AngleTo(V(3, -2, 0)), T);
            // Atan2: accurate also for very small angles (acos would give 0)
            Assert.AreEqual(1e-9, V(1, 0, 0).AngleTo(V(1, 1e-9, 0)), 1e-15);
            Assert.AreEqual(Math.PI / 3, new Vector2d(1, 0).AngleTo(new Vector2d(0.5, Math.Sqrt(3) / 2)), T);
            Assert.AreEqual(Math.PI / 3, new Vector2d(0.5, Math.Sqrt(3) / 2).AngleTo(new Vector2d(1, 0)), T);
        }

        [TestMethod]
        public void ParallelAndOrthogonalVectors()
        {
            Assert.IsTrue(V(1, 2, 3).IsParallelTo(V(2, 4, 6)));
            Assert.IsTrue(V(1, 2, 3).IsParallelTo(V(-1, -2, -3)), "antiparallel vectors are parallel");
            Assert.IsFalse(V(1, 2, 3).IsParallelTo(V(1, 2, 3.1)));
            Assert.IsTrue(V(1, 1, 0).IsOrthogonalTo(V(-1, 1, 7)));
            Assert.IsFalse(V(1, 1, 0).IsOrthogonalTo(V(1, 0, 0)));
        }

        [TestMethod]
        public void UnitizeAndReverse()
        {
            var v = V(3, 0, 4);
            Assert.AreEqual(5.0, v.Length, T);
            Assert.AreEqual(5.0, v.Norm(), T);
            v.Unitize();
            Assert.AreEqual(1.0, v.Length, T);
            Assert.AreEqual(0.6, v.X, T);
            Vector3d r = Vector3d.Reverse(v);
            Assert.AreEqual(-0.8, r.Z, T);
            Assert.AreEqual(0.8, v.Z, T, "the static Reverse does not change the input");
            Assert.AreEqual(5.0, new Vector2d(3, 4).Length, T);
        }

        [TestMethod]
        public void Vector2dNormalsAndRotation()
        {
            var d = new Vector2d(2, 1);
            Vector2d left = d.Normal(true);
            Vector2d right = d.Normal(false);
            Assert.AreEqual(0, d * left, T);
            Assert.IsTrue((d ^ left) > 0, "the left normal is counterclockwise");
            Assert.IsTrue((d ^ right) < 0);
            var x = new Vector2d(1, 0);
            x.Rotate(Math.PI / 2);
            Assert.AreEqual(0, x.X, T);
            Assert.AreEqual(1, x.Y, T);
            Assert.AreEqual(1.0, Vector2d.XAxis ^ Vector2d.YAxis, T);
        }

        #endregion

        #region Segments

        [TestMethod]
        public void SegmentLengthAndMidPoint()
        {
            var l = new Line2d(Q(1, 1), Q(7, 9));
            Assert.AreEqual(10.0, l.Length, T);
            AssertPoint(4, 5, l.Mid);
            var l3 = new Line3d(P(1, 2, 3), P(3, 5, 9));
            Assert.AreEqual(7.0, l3.Length, T);
            AssertPoint(2, 3.5, 6, l3.Mid);
        }

        [TestMethod]
        public void SegmentIntersections()
        {
            Assert.IsTrue(new Line2d(Q(0, 0), Q(2, 2)).GetIntersection(new Line2d(Q(0, 2), Q(2, 0)), out Point2d i));
            AssertPoint(1, 1, i);
            // parallel segments
            Assert.IsFalse(new Line2d(Q(0, 0), Q(2, 0)).GetIntersection(new Line2d(Q(0, 1), Q(2, 1)), out _));
            // the lines cross at (1.5, 1.5), outside both segments
            var a = new Line2d(Q(0, 0), Q(1, 1));
            var b = new Line2d(Q(3, 0), Q(2, 1));
            Assert.IsFalse(a.GetIntersection(b, out _));
            Assert.IsTrue(a.GetIntersectionWithInfiniteLine(b, out Point2d j));
            AssertPoint(1.5, 1.5, j);
        }

        [TestMethod]
        public void Segment3dIntersections()
        {
            Assert.IsTrue(new Line3d(P(0, 0, 0), P(2, 2, 2)).GetIntersection(new Line3d(P(2, 0, 0), P(0, 2, 2)), out Point3d i));
            AssertPoint(1, 1, 1, i);
            // skew lines: x axis and a line parallel to y at z = 1
            Assert.IsFalse(new Line3d(P(-5, 0, 0), P(5, 0, 0)).GetIntersection(new Line3d(P(0, -5, 1), P(0, 5, 1)), out _));
        }

        [TestMethod]
        public void DistanceFromSegmentAndProjection()
        {
            var l = new Line2d(Q(0, 0), Q(10, 0));
            Assert.AreEqual(3.0, l.DistanceTo(Q(4, 3)), T);
            Assert.AreEqual(5.0, l.DistanceTo(Q(13, 4)), T, "beyond the end: distance from the end");
            AssertPoint(4, 0, l.PointDistanceTo(Q(4, 3)));
            AssertPoint(0, 0, l.PointDistanceTo(Q(-4, 3)));
            Assert.AreEqual(3.0, new Line3d(P(0, 0, 0), P(0, 0, 10)).DistanceTo(P(3, 0, 5)), T);
        }

        [TestMethod]
        public void OrientedDistanceIsPositiveOnTheLeft()
        {
            var l = new Line2d(Q(0, 0), Q(4, 0));
            Assert.AreEqual(2.0, l.OrientedDistFromSegment2D(Q(1, 2)), T);
            Assert.AreEqual(-3.0, l.OrientedDistFromSegment2D(Q(1, -3)), T);
            Assert.AreEqual(0.0, l.OrientedDistFromSegment2D(Q(9, 0)), T);
            // reversing the segment changes the sign
            Assert.AreEqual(-2.0, new Line2d(Q(4, 0), Q(0, 0)).OrientedDistFromSegment2D(Q(1, 2)), T);
        }

        [DataTestMethod]
        [DataRow(0.0, 0.0)]
        [DataRow(5.0, -3.0)]
        [DataRow(-2.5, 7.0)]
        public void BoundaryIntegralsOfARectangle(double x0, double y0)
        {
            // rectangle b x h with the lower left corner in (x0, y0), counterclockwise
            double b = 4, h = 6;
            double A = 0, Sx = 0, Sy = 0, Ix = 0, Iy = 0, Ixy = 0;
            foreach (Line2d side in Rectangle(x0, y0, b, h).Explode())
                side.IntegrateOnBoundary(ref A, ref Sx, ref Sy, ref Ix, ref Iy, ref Ixy);
            double xc = x0 + b / 2, yc = y0 + h / 2;
            Assert.AreEqual(b * h, A, 1e-9);
            Assert.AreEqual(b * h * yc, Sx, 1e-9);
            Assert.AreEqual(b * h * xc, Sy, 1e-9);
            Assert.AreEqual(b * h * h * h / 12 + b * h * yc * yc, Ix, 1e-9);
            Assert.AreEqual(h * b * b * b / 12 + b * h * xc * xc, Iy, 1e-9);
            Assert.AreEqual(b * h * xc * yc, Ixy, 1e-9);
        }

        [TestMethod]
        public void BoundaryIntegralsOfATriangle()
        {
            // right triangle with legs b (x) and h (y): Ix0 = b h³/12, Iy0 = h b³/12, Ixy0 = b² h² / 24 about the corner
            double b = 3, h = 5;
            double A = 0, Sx = 0, Sy = 0, Ix = 0, Iy = 0, Ixy = 0;
            foreach (Line2d side in new Polygon2d(new[] { Q(0, 0), Q(b, 0), Q(0, h) }).Explode())
                side.IntegrateOnBoundary(ref A, ref Sx, ref Sy, ref Ix, ref Iy, ref Ixy);
            Assert.AreEqual(b * h / 2, A, 1e-12);
            Assert.AreEqual(b * h / 2 * h / 3, Sx, 1e-12);
            Assert.AreEqual(b * h / 2 * b / 3, Sy, 1e-12);
            Assert.AreEqual(b * h * h * h / 12, Ix, 1e-12);
            Assert.AreEqual(h * b * b * b / 12, Iy, 1e-12);
            Assert.AreEqual(b * b * h * h / 24, Ixy, 1e-12);
        }

        [TestMethod]
        public void SplitSegments()
        {
            new Line2d(Q(0, 0), Q(8, 0)).Split(4, out Line2d[] parts);
            Assert.AreEqual(4, parts.Length);
            Assert.IsTrue(parts.All(p => Math.Abs(p.Length - 2) < T));
            Line3d[] parts3 = new Line3d(P(0, 0, 0), P(0, 0, 10)).Split(new[] { 0.5, 0.25, 0.25, 1.0 });
            CollectionAssert.AreEqual(new[] { 2.5, 2.5, 5.0 }, parts3.Select(p => Math.Round(p.Length, 9)).ToArray());
            AssertPoint(0, 0, 2.5, parts3[0].End);
        }

        [TestMethod]
        public void PointOnSegmentAndOnInfiniteLine()
        {
            var l = new Line3d(P(0, 0, 0), P(2, 2, 2));
            Assert.IsTrue(l.IsPointOnLine(P(1, 1, 1)));
            Assert.IsTrue(l.IsPointOnLine(P(2, 2, 2)));
            Assert.IsFalse(l.IsPointOnLine(P(3, 3, 3)));
            Assert.IsTrue(l.IsPointOnInfiniteLine(P(3, 3, 3)));
            Assert.IsTrue(l.IsPointOnInfiniteLine(P(-1, -1, -1)));
            Assert.IsFalse(l.IsPointOnInfiniteLine(P(1, 1, 1.01)));
        }

        [TestMethod]
        public void SlopeOfSegments()
        {
            Assert.AreEqual(1.0, new Line2d(Q(0, 0), Q(3, 3)).GetSlope(), T);
            Assert.AreEqual(-0.5, new Line2d(Q(0, 0), Q(4, -2)).GetSlope(), T);
            Assert.IsTrue(double.IsInfinity(new Line2d(Q(1, 0), Q(1, 5)).GetSlope()));
        }

        [TestMethod]
        public void ShortestSegmentBetweenSkewLines()
        {
            // x axis and a line parallel to y at z = 3: the common perpendicular is the z axis from 0 to 3
            Line3d s = new Ray3d(P(0, 0, 0), V(1, 0, 0)).CalcShortestLineBetweenTwoRays(new Ray3d(P(0, 7, 3), V(0, 1, 0)));
            AssertPoint(0, 0, 0, s.Start);
            AssertPoint(0, 0, 3, s.End);
            Assert.AreEqual(3.0, s.Length, T);
        }

        #endregion

        #region Planes and circles

        [TestMethod]
        public void PlaneThroughThreePoints()
        {
            var plane = new Plane(P(0, 0, 2), P(1, 0, 2), P(0, 1, 2));
            Assert.AreEqual(0, plane.A, T);
            Assert.AreEqual(0, plane.B, T);
            Assert.AreEqual(1, plane.C, T);
            Assert.AreEqual(-2, plane.D, T);
            Assert.IsTrue(plane.IsPointOnPlane(P(17, -3, 2)));
            Assert.IsFalse(plane.IsPointOnPlane(P(17, -3, 2.01)));
        }

        [TestMethod]
        public void PlaneFromEquation()
        {
            // x + y + z - 3 = 0
            var plane = new Plane(1, 1, 1, -3);
            Assert.IsTrue(plane.IsPointOnPlane(P(1, 1, 1)));
            Assert.IsTrue(plane.IsPointOnPlane(P(3, 0, 0)));
            Assert.AreEqual(Math.Sqrt(3), plane.DistanceToPlane(P(0, 0, 0)), T);
            Assert.AreEqual(3.0, plane.SquareDistanceToPlane(P(0, 0, 0)), T);
            AssertPoint(1, 1, 1, plane.Project(P(0, 0, 0)));
            Assert.AreEqual(1.0, plane.Normal.Length, T);
            Assert.ThrowsException<ArgumentException>(() => new Plane(0, 0, 0, 1));
        }

        [TestMethod]
        public void PlaneLineIntersectionAndAngles()
        {
            var plane = new Plane(P(0, 0, 2), V(0, 0, 1));
            Assert.IsTrue(plane.Intersect(new Line3d(P(1, 1, 0), P(1, 1, 5)), out Point3d i));
            AssertPoint(1, 1, 2, i);
            Assert.AreEqual(0.0, plane.AngleTo(V(1, 1, 0)), T);
            Assert.AreEqual(Math.PI / 2, plane.AngleTo(V(0, 0, -3)), T);
            Assert.AreEqual(Math.PI / 4, plane.AngleTo(new Line3d(P(0, 0, 0), P(1, 0, 1))), T);
            // plane rotated by 30° about x
            var tilted = new Plane(P(0, 0, 0), V(0, -Math.Sin(Math.PI / 6), Math.Cos(Math.PI / 6)));
            Assert.AreEqual(Math.PI / 6, plane.AngleTo(tilted), T);
            // opposite normals: the angle between planes is not larger than 90°
            Assert.AreEqual(0.0, plane.AngleTo(new Plane(P(0, 0, 0), V(0, 0, -1))), T);
        }

        [TestMethod]
        public void PlaneCoordinateSystemHasTheNormalAsZ()
        {
            var plane = new Plane(P(1, 2, 3), V(1, 1, 0));
            CoordinateSystem cs = plane.GetCoordinateSystem();
            Assert.IsTrue(cs.V3.IsParallelTo(plane.Normal));
            Assert.AreEqual(0, cs.V1 * cs.V2, T);
            Assert.AreEqual(0, cs.ToLocal(P(1, 2, 3)).Z, T);
            Assert.AreEqual(0, cs.ToLocal(P(1 + 5, 2 - 5, 3 + 7)).Z, T, "a point of the plane has local z = 0");
        }

        [TestMethod]
        public void CircleThroughThreePoints()
        {
            var c = new Circle3d(P(1, 0, 5), P(0, 1, 5), P(-1, 0, 5));
            AssertPoint(0, 0, 5, c.Center);
            Assert.AreEqual(1.0, c.Radius, T);
            Assert.AreEqual(2 * Math.PI, c.Perimeter, T);
            Assert.AreEqual(Math.PI, c.Area, T);
            Assert.IsTrue(c.GetNormalVector().IsParallelTo(Vector3d.ZAxis));
            Assert.IsTrue(c.IsPointOnCircle(P(Math.Sqrt(0.5), -Math.Sqrt(0.5), 5)));
            Assert.IsTrue(c.IsPointInside(P(0.3, 0.3, 5)));
            Assert.IsFalse(c.IsPointInside(P(0.9, 0.9, 5)));
        }

        [TestMethod]
        public void Circle2dThroughThreePoints()
        {
            // right triangle: the center is the mid point of the hypotenuse
            var c = new Circle2d(Q(0, 0), Q(6, 0), Q(0, 8));
            AssertPoint(3, 4, c.Center);
            Assert.AreEqual(5.0, c.Radius, T);
            Assert.AreEqual(10.0, c.Diameter, T);
            Assert.AreEqual(25 * Math.PI, c.Area, T);
        }

        [TestMethod]
        public void QuarterArc()
        {
            var arc = new Circle3dArc(P(2, 0, 0), P(0, 2, 0), P(0, 0, 0));
            Assert.AreEqual(Math.PI / 2, arc.GetAngle(), T);
            Assert.AreEqual(Math.PI, arc.GetLenght(), T);
            // the same arc through three points (the tolerance selects the constructor with the point of passage)
            var through = new Circle3dArc(P(2, 0, 0), P(Math.Sqrt(2), Math.Sqrt(2), 0), P(0, 2, 0), GeometryBase.Tolerance);
            AssertPoint(0, 0, 0, through.Center);
            Assert.AreEqual(Math.PI, through.GetLenght(), T);
            Assert.IsTrue(through.IsPointOnCircleArc(P(2 * Math.Cos(0.3), 2 * Math.Sin(0.3), 0)));
            Assert.IsFalse(through.IsPointOnCircleArc(P(-Math.Sqrt(2), -Math.Sqrt(2), 0)));
            Assert.IsFalse(through.IsPointOnCircleArc(P(1, 1, 0)));
        }

        [TestMethod]
        public void CircleToPolygonArea()
        {
            // inscribed regular polygon: A = n/2 r² sin(2π/n)
            foreach (int n in new[] { 4, 6, 32, 100 })
            {
                // (before the fix the polygon had half the radius)
                Polygon2d p = new Circle2d(Q(1, 2), 3).ConvertToPolygon(n);
                Assert.AreEqual(3.0, p[0].DistanceTo(Q(1, 2)), 1e-12);
                Assert.AreEqual(n, p.Count);
                Assert.AreEqual(n / 2.0 * 9 * Math.Sin(2 * Math.PI / n), p.GetSignedArea(), 1e-9, $"{n} edges");
                AssertPoint(1, 2, p.GetCentroid(), 1e-9);
            }
        }

        #endregion

        #region Polygons

        [TestMethod]
        public void SignedAreaAndOrientation()
        {
            Polygon2d ccw = Rectangle(1, 1, 3, 2);
            Assert.AreEqual(6.0, ccw.GetSignedArea(), T);
            Assert.IsTrue(ccw.IsRightHandOrdered());
            ccw.Reverse();
            Assert.AreEqual(-6.0, ccw.GetSignedArea(), T);
            Assert.IsFalse(ccw.IsRightHandOrdered());
            Assert.AreEqual(0.0, new Polygon2d(new[] { Q(0, 0), Q(1, 1) }).GetSignedArea(), 0.0);
        }

        [TestMethod]
        public void RegularPolygonFromTheDiameter()
        {
            var p = new Polygon2d(10, 6);
            Assert.AreEqual(6, p.Count);
            // hexagon of radius 5: 6 equilateral triangles
            Assert.AreEqual(6 * Math.Sqrt(3) / 4 * 25, p.GetSignedArea(), 1e-9);
            Assert.AreEqual(5.0, p.Explode()[0].Length, 1e-9);
            AssertPoint(5, 0, p[0]);
            var moved = new Polygon2d(10, 6, Q(2, 3));
            AssertPoint(7, 3, moved[0]);
            Assert.ThrowsException<ArgumentException>(() => new Polygon2d(10, 1));
        }

        [TestMethod]
        public void CentroidOfAConcavePolygonIsNotTheMeanOfTheVertices()
        {
            Polygon2d l = LShape();
            Assert.AreEqual(6.0, l.GetSignedArea(), T);
            AssertPoint(1.5, 1.0, l.GetCentroid());
            AssertPoint(10.0 / 6, 8.0 / 6, l.GetCenter());
            // same centroid with the opposite order
            l.Reverse();
            AssertPoint(1.5, 1.0, l.GetCentroid());
        }

        [TestMethod]
        public void CentroidOfATriangleIsTheMeanOfTheVertices()
        {
            var t = new Polygon2d(new[] { Q(1, 1), Q(7, 2), Q(3, 9) });
            AssertPoint(11.0 / 3, 4.0, t.GetCentroid());
            AssertPoint(11.0 / 3, 4.0, t.GetCenter());
        }

        [TestMethod]
        public void PointInsideAConcavePolygon()
        {
            Polygon2d l = LShape();
            Assert.IsTrue(l.IsPointInside(Q(0.5, 2.5)));
            Assert.IsTrue(l.IsPointInside(Q(3.5, 0.5)));
            Assert.IsFalse(l.IsPointInside(Q(2.5, 2)), "the notch of the L is outside");
            Assert.IsFalse(l.IsPointInside(Q(-1, 0.5)));
            Assert.IsTrue(l.IsPointInside(Q(2, 1)), "a point on an edge is inside");
            Assert.IsTrue(l.IsPointInside(Q(1, 1)), "a vertex is inside");
            // the horizontal ray through a vertex
            Assert.IsFalse(l.IsPointInside(Q(-5, 1)));
            Assert.IsTrue(l.IsPointInside(Q(0.5, 1)));
        }

        [TestMethod]
        public void DistanceFromTheBorder()
        {
            Polygon2d r = Rectangle(0, 0, 10, 4);
            Assert.AreEqual(1.0, r.DistanceTo(Q(5, 1)), T);
            Assert.AreEqual(2.0, r.DistanceTo(Q(5, 2)), T);
            Assert.AreEqual(5.0, r.DistanceTo(Q(13, 8)), T);
        }

        [TestMethod]
        public void BoundingBoxOfAPolygon()
        {
            BoundingBox2d box = LShape().GetBoundingBox();
            AssertPoint(0, 0, box.Min);
            AssertPoint(4, 3, box.Max);
            AssertPoint(4, 3, box.Size);
            var b3 = new Polygon3d(new[] { P(1, -2, 3), P(4, 5, -6), P(0, 0, 0) }).GetBoundingBox();
            AssertPoint(0, -2, -6, b3.Min);
            AssertPoint(4, 5, 3, b3.Max);
        }

        [TestMethod]
        public void RemoveAlignedAndDuplicatedPoints()
        {
            var p = new Polygon2d(new[] { Q(0, 0), Q(1, 0), Q(2, 0), Q(2, 1), Q(2, 2), Q(1, 2), Q(0, 2), Q(0, 1) });
            p.RemoveAlignedPoints();
            Assert.AreEqual(4, p.Count);
            Assert.AreEqual(4.0, Math.Abs(p.GetSignedArea()), T);
            var d = new Polygon2d(new[] { Q(0, 0), Q(0, 0), Q(3, 0), Q(3, 3), Q(3, 3.00001), Q(0, 3) });
            d.RemoveDuplicatedPoints();
            Assert.AreEqual(4, d.Count);
        }

        [TestMethod]
        public void MirrorAndScaleOfAPolygon()
        {
            Polygon2d l = LShape();
            Polygon2d m = l.Mirror(1, 0, 0); // mirror about the y axis
            Assert.AreEqual(-6.0, m.GetSignedArea(), T, "mirroring changes the orientation");
            AssertPoint(-1.5, 1.0, m.GetCentroid());
            Polygon2d s = l.Scale(3);
            Assert.AreEqual(54.0, s.GetSignedArea(), 1e-9);
        }

        [TestMethod]
        public void TriangulationFromACenterKeepsTheArea()
        {
            Polygon2d hexagon = new Polygon2d(8, 6, Q(1, 1));
            Polygon2d[] triangles = hexagon.Triangularization(Q(1, 1));
            Assert.AreEqual(6, triangles.Length);
            Assert.AreEqual(hexagon.GetSignedArea(), triangles.Sum(t => t.GetSignedArea()), 1e-9);
        }

        [TestMethod]
        public void BooleanOperationsOfTwoSquares()
        {
            // two 4 x 4 squares overlapping in a 2 x 3 rectangle
            Polygon2d a = Rectangle(0, 0, 4, 4);
            Polygon2d b = Rectangle(2, 1, 4, 4);
            double Area(Polygon2d[] r) => r.Sum(p => Math.Abs(p.GetSignedArea()));
            Assert.AreEqual(6.0, Area(Polygon2d.Intersection(a, b)), 1e-6);
            Assert.AreEqual(26.0, Area(Polygon2d.Union(a, b)), 1e-6);
            Assert.AreEqual(10.0, Area(Polygon2d.Difference(a, b)), 1e-6);
            Assert.AreEqual(20.0, Area(Polygon2d.NotIntersection(a, b)), 1e-6);
            Assert.AreEqual(1, Polygon2d.Union(a, b).Length);
            // disjoint squares: no intersection (null), two polygons in the union
            Assert.IsNull(Polygon2d.Intersection(a, Rectangle(10, 10, 1, 1)));
            Assert.AreEqual(2, Polygon2d.Union(a, Rectangle(10, 10, 1, 1)).Length);
        }

        [TestMethod]
        public void VerticalAndTiltedPolygonAreas()
        {
            // 3 x 2 rectangle in the xz plane
            var vertical = new Polygon3d(new[] { P(0, 0, 0), P(3, 0, 0), P(3, 0, 2), P(0, 0, 2) });
            Assert.AreEqual(6.0, Math.Abs(vertical.GetSignedArea()), T);
            Assert.IsTrue(vertical.GetNormalVector().IsParallelTo(Vector3d.YAxis));
            // rectangle 3 x 2 with the y side tilted by 60°: projected area 3 x 1
            double c = Math.Cos(Math.PI / 3), s = Math.Sin(Math.PI / 3);
            var tilted = new Polygon3d(new[] { P(0, 0, 0), P(3, 0, 0), P(3, 2 * c, 2 * s), P(0, 2 * c, 2 * s) });
            Assert.AreEqual(6.0, tilted.GetSignedArea(), T);
            Assert.AreEqual(3.0, Math.Abs(new Polygon2d(new[] { Q(0, 0), Q(3, 0), Q(3, 2 * c), Q(0, 2 * c) }).GetSignedArea()), T);
            Assert.AreEqual(Math.PI / 3, tilted.GetNormalVector().AngleTo(Vector3d.ZAxis), T);
        }

        [TestMethod]
        public void NewellNormalOfAConcavePolygon()
        {
            // L shape in the plane z = 5, counterclockwise from +z; the first three vertices are not a convex corner
            var l = new Polygon3d(new[] { P(1, 1, 5), P(1, 3, 5), P(0, 3, 5), P(0, 0, 5), P(4, 0, 5), P(4, 1, 5) });
            Assert.AreEqual(6.0, l.GetSignedArea(), T);
            Vector3d n = l.GetNormalVector();
            Assert.AreEqual(1.0, n.Z, T);
            Assert.IsTrue(l.IsRightHandOrdered());
            AssertPoint(1.5, 1, 5, l.GetCentroid());
        }

        [TestMethod]
        public void ConvexityAndEdges()
        {
            var square = new Polygon3d(Rectangle(0, 0, 2, 2));
            Assert.IsTrue(square.IsConvex());
            Assert.IsFalse(new Polygon3d(LShape()).IsConvex());
            // aligned points do not make a polygon concave
            Assert.IsTrue(new Polygon3d(new[] { P(0, 0), P(1, 0), P(2, 0), P(2, 2), P(0, 2) }).IsConvex());
            Assert.AreEqual(1, square.IsPointOnEdge(P(2, 1)));
            Assert.AreEqual(-1, square.IsPointOnEdge(P(1, 1)));
            Assert.IsTrue(square.PointExists(P(2, 2)));
            Assert.IsFalse(square.PointExists(P(2, 2.001)));
        }

        [TestMethod]
        public void Polygon3dToLocal2dKeepsTheArea()
        {
            double c = Math.Cos(0.4), s = Math.Sin(0.4);
            // L shape rotated about the x axis and moved
            var l = new Polygon3d(LShape().Select(p => P(p.X + 10, p.Y * c - 3, p.Y * s + 7)).ToArray());
            Polygon2d local = l.GetPolygon2d();
            Assert.AreEqual(6.0, Math.Abs(local.GetSignedArea()), 1e-9);
            Assert.AreEqual(6.0, Math.Abs(l.GetSignedArea()), 1e-9);
            // perimeter preserved
            Assert.AreEqual(LShape().Explode().Sum(e => e.Length), local.Explode().Sum(e => e.Length), 1e-9);
        }

        #endregion

        #region Shapes

        [TestMethod]
        public void ShapeWithHoles()
        {
            var shape = new Shape2d(Rectangle(0, 0, 10, 6), new[] { Rectangle(1, 1, 2, 2), Rectangle(6, 2, 3, 1) });
            Assert.AreEqual(60 - 4 - 3, shape.GetArea(), T);
            Assert.IsTrue(shape.IsPointInside(Q(5, 5)));
            Assert.IsFalse(shape.IsPointInside(Q(2, 2)), "inside a hole");
            Assert.IsTrue(shape.IsPointInside(Q(1, 2)), "on the border of a hole");
            Assert.IsFalse(shape.IsPointInside(Q(11, 2)));
        }

        [TestMethod]
        public void ShapeScaleAndMirrorKeepTheArea()
        {
            var shape = new Shape2d(Rectangle(0, 0, 10, 6), new[] { Rectangle(1, 1, 2, 2) });
            Assert.AreEqual(56 * 4, shape.Scale(2).GetArea(), 1e-9);
            Assert.AreEqual(56 * 6, shape.Scale(2, 3, 1).GetArea(), 1e-9);
            Assert.AreEqual(56, shape.Mirror(1, 1, -3).GetArea(), 1e-9);
            var moved = new Shape2d(shape);
            moved.Move(5, -7);
            Assert.AreEqual(56, moved.GetArea(), 1e-9);
            Assert.IsTrue(moved.IsPointInside(Q(10, -6)));
        }

        [TestMethod]
        public void ShapeBooleanOperations()
        {
            var a = new Shape2d(Rectangle(0, 0, 4, 4));
            var b = new Shape2d(Rectangle(2, 1, 4, 4));
            Assert.AreEqual(26.0, Shape2d.Union(a, b).Sum(s => s.GetArea()), 1e-6);
            Assert.AreEqual(6.0, Shape2d.Intersection(a, b).Sum(s => s.GetArea()), 1e-6);
            Assert.AreEqual(10.0, Shape2d.Difference(a, b).Sum(s => s.GetArea()), 1e-6);
            Assert.AreEqual(20.0, Shape2d.NotIntersection(a, b).Sum(s => s.GetArea()), 1e-6);
            // a square minus a smaller one inside: one shape with a hole
            Shape2d[] frame = Shape2d.Difference(new Shape2d(Rectangle(0, 0, 10, 10)), new Shape2d(Rectangle(3, 3, 4, 4)));
            Assert.AreEqual(1, frame.Length);
            Assert.IsTrue(frame[0].HasHoles);
            Assert.AreEqual(84.0, frame[0].GetArea(), 1e-6);
            // two rectangles sharing a side: one shape
            Shape2d[] joined = Shape2d.Union(new Shape2d(Rectangle(0, 0, 2, 1)), new Shape2d(Rectangle(2, 0, 3, 1)));
            Assert.AreEqual(1, joined.Length);
            Assert.AreEqual(5.0, joined[0].GetArea(), 1e-6);
        }

        [TestMethod]
        public void ShapeToLocalAndBack()
        {
            double c = Math.Cos(0.7), s = Math.Sin(0.7);
            var fill = new Polygon3d(LShape().Select(p => P(p.X * c, p.X * s, p.Y + 4)).ToArray());
            var shape = new Shape(fill);
            Assert.AreEqual(6.0, shape.GetArea(), 1e-9);
            Shape2d local = shape.ToLocal();
            Assert.AreEqual(6.0, local.GetArea(), 1e-9);
            Shape global = local.ToGlobal(shape.GetCoordinateSystem());
            for (int i = 0; i < fill.Count; i++)
                AssertPoint(fill[i].X, fill[i].Y, fill[i].Z, global.Fill[i], 1e-9);
        }

        #endregion

        #region Coordinate systems

        [TestMethod]
        public void LocalCoordinatesOfARotatedSystem()
        {
            // origin (1, 2, 3), x along global y, y along global -x: z along global z
            var cs = new CoordinateSystem(P(1, 2, 3), V(0, 1, 0), V(-1, 0, 0));
            AssertPoint(0, 0, 1, new Point3d(cs.V3.X, cs.V3.Y, cs.V3.Z));
            AssertPoint(3, 0, 0, cs.ToLocal(P(1, 5, 3)));
            AssertPoint(0, 1, 0, cs.ToLocal(P(0, 2, 3)));
            AssertPoint(0, 0, -2, cs.ToLocal(P(1, 2, 1)));
            AssertPoint(1, 5, 3, cs.ToGlobal(P(3, 0, 0)));
            // vectors do not feel the origin
            Vector3d v = cs.ToLocal(V(0, 1, 0));
            AssertPoint(1, 0, 0, new Point3d(v.X, v.Y, v.Z));
        }

        [TestMethod]
        public void RoundTripAndDistancesInAGenericSystem()
        {
            var cs = new CoordinateSystem(P(3, -1, 2), P(4, 1, 5), P(0, 2, 2));
            Assert.AreEqual(0, cs.V1 * cs.V2, T);
            Assert.AreEqual(0, cs.V1 * cs.V3, T);
            Assert.AreEqual(1, cs.V1.CrossProduct(cs.V2) * cs.V3, T, "right handed");
            var a = P(7, 8, -9);
            var b = P(-2, 0.5, 4);
            Point3d la = cs.ToLocal(a), lb = cs.ToLocal(b);
            Assert.AreEqual(a.DistanceTo(b), la.DistanceTo(lb), 1e-9);
            AssertPoint(a.X, a.Y, a.Z, cs.ToGlobal(la), 1e-9);
            // p2 is on the local x axis, p3 on the local xy plane
            Point3d p2 = cs.ToLocal(P(4, 1, 5));
            Assert.AreEqual(0, p2.Y, T);
            Assert.AreEqual(0, p2.Z, T);
            Assert.IsTrue(p2.X > 0);
            Assert.AreEqual(0, cs.ToLocal(P(0, 2, 2)).Z, T);
        }

        [TestMethod]
        public void NotOrthogonalAxesThrow()
        {
            Assert.ThrowsException<ArgumentException>(() => new CoordinateSystem(P(0, 0, 0), V(1, 0, 0), V(1, 1, 0)));
        }

        [TestMethod]
        public void RotationAboutTheGlobalZ()
        {
            CoordinateSystem cs = CoordinateSystem.Global;
            cs.RotateZ(Math.PI / 2);
            AssertPoint(0, 1, 0, new Point3d(cs.V1.X, cs.V1.Y, cs.V1.Z));
            AssertPoint(-1, 0, 0, new Point3d(cs.V2.X, cs.V2.Y, cs.V2.Z));
            // a global point on +y is on the local +x
            AssertPoint(2, 0, 0, cs.ToLocal(P(0, 2, 0)));
            CoordinateSystem other = CoordinateSystem.Global;
            other.RotateV3(Math.PI / 2);
            AssertPoint(0, 1, 0, new Point3d(other.V1.X, other.V1.Y, other.V1.Z));
        }

        #endregion

        #region Intervals and meshes

        [TestMethod]
        public void IntervalOperations()
        {
            BoundingBox1d I(double a, double b) { var r = new BoundingBox1d(); r.Update(a); r.Update(b); return r; }
            Assert.AreEqual(1, BoundingBox1d.GetUnion(I(0, 5), I(3, 8)).Count);
            Assert.AreEqual(8.0, BoundingBox1d.GetUnion(I(0, 5), I(3, 8))[0].Size, T);
            Assert.AreEqual(2, BoundingBox1d.GetUnion(I(0, 1), I(3, 8)).Count);
            BoundingBox1d common = BoundingBox1d.GetIntersection(I(0, 5), I(3, 8), 0);
            Assert.AreEqual(3.0, common.Min, T);
            Assert.AreEqual(5.0, common.Max, T);
            Assert.AreEqual(4.0, common.Center(), T);
            Assert.IsNull(BoundingBox1d.GetIntersection(I(0, 5), I(4.5, 8), 1));
            List<BoundingBox1d> diff = BoundingBox1d.GetDifference(I(0, 10), I(3, 5), 0);
            Assert.AreEqual(2, diff.Count);
            Assert.AreEqual(8.0, diff.Sum(d => d.Size), T);
            Assert.IsTrue(new BoundingBox1d().IsEmpty);
        }

        private static Mesh Generate(Shape2d shape, double meshSize)
        {
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = meshSize, Recombine = false };
            Assert.IsTrue(DelaunayMesh.Generate(shape, options, out Mesh mesh, out _));
            return mesh;
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MeshAreaAndEulerCharacteristic(bool hole)
        {
            // planar triangulation: V - E + F = 1 for a region without holes, 1 - (number of holes) in general
            var shape = hole ? new Shape2d(Rectangle(0, 0, 100, 60), new[] { Rectangle(30, 20, 40, 20) }) : new Shape2d(Rectangle(0, 0, 100, 60));
            Mesh mesh = Generate(shape, 10);
            Assert.AreEqual(shape.GetArea(), mesh.Faces.Sum(f => mesh.GetFaceArea(f)), 1e-6);
            Assert.AreEqual(hole ? 0 : 1, mesh.VerticesCount - mesh.EdgesCount + mesh.FacesCount);
            // the free edges are the boundary: their length is the perimeter
            double perimeter = hole ? 2 * (100 + 60) + 2 * (40 + 20) : 2 * (100 + 60);
            Assert.AreEqual(perimeter, mesh.GetNakedEdges().Sum(e => mesh.GetEdgeLength(e)), 1e-6);
        }

        [TestMethod]
        public void MeshOfACircleKeepsThePolygonArea()
        {
            var shape = new Shape2d(new Polygon2d(200, 48));
            Mesh mesh = Generate(shape, 15);
            Assert.AreEqual(shape.GetArea(), mesh.Faces.Sum(f => mesh.GetFaceArea(f)), 1e-6);
            // all the vertices are inside the circle
            Assert.IsTrue(mesh.GetVertices().All(v => Math.Sqrt(v.Point.X * v.Point.X + v.Point.Y * v.Point.Y) <= 100 + 1e-9));
        }

        [TestMethod]
        public void MeshMoveAndJoin()
        {
            Mesh a = Generate(new Shape2d(Rectangle(0, 0, 10, 10)), 5);
            Mesh b = Generate(new Shape2d(Rectangle(0, 0, 10, 10)), 5);
            b.Move(10, 0, 0);
            int faces = a.FacesCount + b.FacesCount;
            a.JoinMesh(b);
            Assert.AreEqual(faces, a.FacesCount);
            Assert.AreEqual(200.0, a.Faces.Sum(f => a.GetFaceArea(f)), 1e-6);
            // the common side is not free any more
            Assert.AreEqual(2 * (20 + 10), a.GetNakedEdges().Sum(e => a.GetEdgeLength(e)), 1e-6);
        }

        #endregion
    }
}
