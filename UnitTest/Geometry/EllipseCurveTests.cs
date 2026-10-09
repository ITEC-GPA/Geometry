using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class EllipseCurveTests
    {
        private static Point3d P(double x, double y = 0, double z = 0) => new Point3d(x, y, z);
        private static EllipseCurve3d Ellipse(double a = 5, double b = 2, double start = 0, double sweep = 2 * Math.PI,
            CurveInterval? domain = null) => new EllipseCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), a, b, start, sweep, domain);
        private static void Near(Point3d expected, Point3d actual, double tolerance = 1e-9) =>
            Assert.IsTrue(expected.DistanceTo(actual) <= tolerance, $"distance {expected.DistanceTo(actual)} > {tolerance}");

        public static IEnumerable<object[]> Cases()
        {
            for (int i = 0; i < 100; i++) yield return new object[] { i };
        }

        [DataTestMethod, DynamicData(nameof(Cases), DynamicDataSourceType.Method), TestCategory("Ellipse100")]
        public void EllipseParametersLengthProjectionAndTessellation(int index)
        {
            double scale = new[] { 1e-5, 1, 1e5 }[index % 3];
            double a = (2 + index % 7) * scale, b = a * new[] { 1, .8, .2, .01, .000001 }[index % 5];
            double start = -.7 + index * .13, sweep = (index % 2 == 0 ? 1 : -1) * (index % 4 == 0 ? 2 * Math.PI : 1.3 + index % 5);
            var domain = new CurveInterval(-13, 29);
            var curve = Ellipse(a, b, start, sweep, domain);
            double angle = start + sweep * .37, t = domain.ParameterAt(.37);
            Near(P(a * Math.Cos(angle), b * Math.Sin(angle)), curve.PointAt(t), 1e-8 * scale);
            Vector3d tangent = curve.TangentAt(t);
            double norm = Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y + tangent.Z * tangent.Z);
            Assert.AreEqual(1, norm, 1e-12);
            Assert.AreEqual(t, curve.ParameterAtLength(curve.LengthAt(t)), 1e-9);
            // Independent composite Simpson integral of the normalized speed.
            const int intervals = 32768;
            double sum = 0;
            for (int i = 0; i <= intervals; i++)
            {
                double theta = start + sweep * i / intervals, x = a / scale * Math.Sin(theta), y = b / scale * Math.Cos(theta);
                sum += Math.Sqrt(x * x + y * y) * (i == 0 || i == intervals ? 1 : i % 2 == 0 ? 2 : 4);
            }
            double independent = sum * Math.Abs(sweep) / (3 * intervals) * scale;
            Assert.AreEqual(independent, curve.Length, 2e-7 * curve.Length);
            Near(curve.PointAt(t), curve.ClosestPoint(curve.PointAt(t)), 2e-6 * scale);
            Point3d query = P((index % 9 - 4) * a / 2, (index % 11 - 5) * a / 3, a / 2);
            double closest = curve.ClosestPoint(query).DistanceTo(query);
            for (int i = 0; i <= 400; i++)
                Assert.IsTrue(closest <= curve.PointAt(domain.ParameterAt(i / 400.0)).DistanceTo(query) + 1e-8 * scale);
            double tolerance = .001 * scale, maximum = .5 * scale;
            Point3d[] points = curve.ToPolyline(tolerance, maximum);
            for (int i = 0; i < points.Length - 1; i++)
            {
                Assert.IsTrue(points[i].DistanceTo(points[i + 1]) <= maximum * (1 + 1e-10));
                Point3d midpoint = curve.PointAt(domain.ParameterAt((i + .5) / (points.Length - 1)));
                Assert.IsTrue(new LineCurve3d(points[i], points[i + 1]).ClosestPoint(midpoint).DistanceTo(midpoint) <= tolerance * 1.000001);
            }
            if (curve.IsClosed) Near(points[0], points[points.Length - 1], 0);
            var trim = curve.Trim(domain.ParameterAt(.2), domain.ParameterAt(.8));
            Near(curve.PointAt(t), trim.PointAt(t), 1e-8 * scale);
            Near(curve.PointAt(t), curve.Reversed().PointAt(domain.ParameterAt(.63)), 1e-8 * scale);
        }

        [TestMethod]
        public void CircleSpecialCaseAndNonUniformSpeed()
        {
            Assert.AreEqual(10 * Math.PI, Ellipse(5, 5).Length, 1e-12);
            var curve = Ellipse();
            Assert.AreEqual(23.01311259566484, curve.Length, 1e-10);
            Assert.IsTrue(Math.Abs(curve.ParameterAtLength(curve.Length / 8) - .125) > .01);
            Near(P(5), curve.StartPoint); Near(P(0, 2), curve.PointAt(.25));
            Near(P(0, -2), curve.Reversed().PointAt(.25));
        }

        [TestMethod]
        public void ProjectionFindsInteriorMinimaEndpointsAndEarliestTies()
        {
            var full = Ellipse();
            Assert.AreEqual(.25, full.ClosestParameter(P(0)), 1e-12);
            Assert.AreEqual(.25, full.ClosestParameter(P(0, 0, 30)), 1e-12);
            double x = 25.0 / 21;
            Near(P(x, 2 * Math.Sqrt(1 - x * x / 25)), full.ClosestPoint(P(1)), 1e-9);
            Near(P(5), full.ClosestPoint(P(100)), 1e-9);
            var quarter = Ellipse(5, 2, 0, Math.PI / 2, new CurveInterval(3, 8));
            Assert.AreEqual(8, quarter.ClosestParameter(P(-10, 0)), 1e-12);
            Assert.AreEqual(3, quarter.ClosestParameter(P(10, -10)), 1e-12);
            Assert.AreEqual(0, Ellipse(5, 5).ClosestParameter(P(0)));
        }

        [TestMethod]
        public void VeryDistantQueriesRetainProjectionDirection()
        {
            var curve = Ellipse(2e-100, 1e-100);
            // Maximum dot product with (1,1), in normalized ellipse coordinates.
            var closest = curve.ClosestPoint(P(1e300, 1e300));
            Assert.AreEqual(4 / Math.Sqrt(5), closest.X / 1e-100, 1e-10);
            Assert.AreEqual(1 / Math.Sqrt(5), closest.Y / 1e-100, 1e-10);
            Assert.AreEqual(.25, curve.ClosestParameter(P(0, 0, 1e300)), 1e-12);
            Assert.IsTrue(Ellipse(1e150, 1e149).Length > 4e150);
        }

        [TestMethod]
        public void SpatialCopiesSerializationAndNestedCurves()
        {
            var center = P(1, 2, 3);
            var curve = new EllipseCurve3d(center, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1), 5, 2, .3, -4, new CurveInterval(3, 9));
            center.Move(50, 20, 30);
            Assert.AreEqual(2, curve.PointAt(5).Y, 1e-12);
            var copy = curve.DuplicateCurve(); copy.Move(1, 2, 3);
            Near(curve.StartPoint + new Vector3d(1, 2, 3), copy.StartPoint);
            var pieces = curve.Split(6);
            var composite = new PolyCurve3d(pieces);
            Assert.AreEqual(curve.Length, composite.Length, 1e-9);
            Near(curve.EndPoint, composite.EndPoint);
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter(); formatter.Serialize(stream, curve); stream.Position = 0;
                var restored = (EllipseCurve3d)formatter.Deserialize(stream);
                Assert.AreEqual(curve, restored); Assert.AreEqual(curve.Guid, restored.Guid);
                Near(curve.PointAt(5), restored.PointAt(5));
            }
        }

        [TestMethod]
        public void InvalidGeometryParametersAndUnboundedTessellationAreRejected()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Ellipse(0, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Ellipse(1, double.NaN));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Ellipse(1, 2, double.PositiveInfinity));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Ellipse(1, 2, 0, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Ellipse(1, 2, 0, 7));
            Assert.ThrowsException<ArgumentException>(() => new EllipseCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(0, 0, 1), 1, 2));
            var curve = Ellipse();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.PointAt(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.ParameterAtLength(curve.Length + 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.TangentAt(.5, (CurveEvaluationSide)99));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.ToPolyline(0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.ToPolyline(1e-30));
            Assert.ThrowsException<ArgumentException>(() => curve.Trim(.8, .2));
        }
    }
}
