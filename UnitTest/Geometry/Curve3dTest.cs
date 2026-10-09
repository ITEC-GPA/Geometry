using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class Curve3dTest
    {
        private static Point3d P(double x, double y = 0, double z = 0) => new Point3d(x, y, z);
        private static void Near(Point3d expected, Point3d actual, double tolerance = 1e-10)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance);
            Assert.AreEqual(expected.Y, actual.Y, tolerance);
            Assert.AreEqual(expected.Z, actual.Z, tolerance);
        }
        private static T RoundTrip<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter(); formatter.Serialize(stream, value); stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        [TestMethod]
        public void LineCurveSeparatesParametersAndDistanceAndProjectsToFiniteSegment()
        {
            var line = new LineCurve3d(P(1, 2), P(4, 6), new CurveInterval(10, 20));
            Assert.AreEqual(5, line.Length, 1e-12);
            Near(P(2.5, 4), line.PointAt(15)); Near(P(2.5, 4), line.PointAtLength(2.5));
            Assert.AreEqual(2.5, line.LengthAt(15), 1e-12);
            Assert.AreEqual(15, line.ParameterAtLength(2.5), 1e-12);
            Assert.AreEqual(0.6, line.TangentAt(15).X, 1e-12);
            Near(line.StartPoint, line.ClosestPoint(P(-100, -100)));
            Near(line.EndPoint, line.ClosestPoint(P(100, 100)));
            Near(P(2.5, 4), line.ClosestPoint(P(2.5, 4, 20)));
            line.SetDomain(new CurveInterval(-2, 2)); Near(P(2.5, 4), line.PointAt(0));
        }

        [TestMethod]
        public void LineCurveOwnsItsDataAndReversalIsOriented()
        {
            var legacy = new Line3d(P(0), P(10)); var curve = new LineCurve3d(legacy) { Tag = "axis" };
            legacy.End.X = 20; curve.StartPoint.X = -100; curve.ToLine3d().End.X = 40;
            Near(P(10), curve.EndPoint); Near(P(0), curve.StartPoint);
            var copy = curve.DuplicateCurve(); copy.Move(1, 2, 3);
            Near(P(0), curve.StartPoint); Near(P(1, 2, 3), copy.StartPoint);
            Assert.AreEqual("axis", copy.Tag); Assert.AreNotEqual(curve.Guid, copy.Guid);
            var reverse = curve.Reversed(); Near(P(10), reverse.StartPoint);
            Assert.AreEqual(-1, reverse.TangentAt(0).X, 1e-12);
            Assert.AreNotEqual(curve, reverse); Assert.AreEqual(curve, reverse.Reversed());
            Assert.AreEqual(legacy, new Line3d(legacy.End, legacy.Start));
        }

        [TestMethod]
        public void LineCurveTrimSplitAndTessellationPreserveGeometry()
        {
            var line = new LineCurve3d(P(0), P(10), new CurveInterval(2, 12));
            var parts = line.Split(5);
            Assert.AreEqual(3, parts[0].Length, 1e-12); Assert.AreEqual(7, parts[1].Length, 1e-12);
            Near(line.PointAt(4), parts[0].PointAt(4)); Near(line.PointAt(9), parts[1].PointAt(9));
            var points = line.ToPolyline(0.1, 3); Assert.AreEqual(5, points.Length);
            for (int i = 1; i < points.Length; i++) Assert.IsTrue(points[i].DistanceTo(points[i - 1]) <= 3);
            var segments = line.ToLineSegments(); Assert.AreEqual(1, segments.Length);
            Near(P(10), segments[0].End);
        }

        [TestMethod]
        public void CurveContractRejectsInvalidDomainsDegeneracyAndInvalidRequests()
        {
            Assert.ThrowsException<ArgumentException>(() => new CurveInterval(1, 1));
            Assert.ThrowsException<ArgumentException>(() => new CurveInterval(double.NaN, 1));
            Assert.ThrowsException<ArgumentException>(() => new LineCurve3d(P(0), P(1), default(CurveInterval)));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LineCurve3d(P(0), P(0)));
            Assert.ThrowsException<ArgumentNullException>(() => new LineCurve3d((Line3d)null));
            var line = new LineCurve3d(P(0), P(1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.PointAt(2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.ParameterAtLength(double.NaN));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.Split(0));
            Assert.ThrowsException<ArgumentException>(() => line.Trim(0.8, 0.2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.ToPolyline(0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.ToPolyline(0.1, 1e-12));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => line.Move(double.NaN, 0, 0));
            Near(P(0), line.StartPoint);
        }

        [TestMethod]
        public void LineCurveSerializationPreservesDomainGeometryAndIdentity()
        {
            var line = new LineCurve3d(P(2, 3, 4), P(8, 9, 10), new CurveInterval(-1, 3)) { Tag = "not persisted" };
            var restored = RoundTrip(line);
            Assert.AreEqual(line, restored); Assert.AreEqual(line.Guid, restored.Guid); Assert.IsNull(restored.Tag);
            restored.Move(2, 0, 0); Near(P(2, 3, 4), line.StartPoint);
        }

        [TestMethod]
        public void LineCurveHandlesSmallAndLargeFiniteLengths()
        {
            foreach (double size in new[] { 1e-150, 1e150 })
            {
                var line = new LineCurve3d(P(0), P(size));
                Assert.AreEqual(size, line.Length, size * 1e-12);
                Assert.AreEqual(0.5, line.ClosestParameter(P(size / 2, size)), 1e-12);
                Assert.AreEqual(1, line.TangentAt(0).X, 1e-12);
            }
        }
    }
}
