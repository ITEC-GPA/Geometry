using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public partial class Curve3dTest
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
        public void ArcCurveSupportsMajorArcsClockwiseArcsAndExactCircleClosure()
        {
            var arc = new ArcCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 2, 1.5 * Math.PI);
            Assert.AreEqual(3 * Math.PI, arc.Length, 1e-12);
            Near(P(2), arc.StartPoint); Near(P(0, -2), arc.EndPoint);
            Near(P(-2), arc.PointAt(2.0 / 3));
            var reverse = arc.Reversed(); Near(arc.EndPoint, reverse.StartPoint);
            for (int i = 0; i <= 10; i++) Near(arc.PointAt(i / 10.0), reverse.PointAt(1 - i / 10.0));
            var circle = ArcCurve3d.FromCircle(new Circle3d(P(1, 2, 3), 2, new Plane(P(1, 2, 3), new Vector3d(0, 0, 1))));
            Assert.IsTrue(circle.IsClosed); Near(circle.StartPoint, circle.EndPoint, 0);
            var points = circle.ToPolyline(10); Assert.IsTrue(points.Length >= 5); Near(points[0], points[points.Length - 1], 0);
        }

        [TestMethod]
        public void ArcThroughThreePointsUsesThePassagePointForMajorAndTiltedArcs()
        {
            var major = ArcCurve3d.FromThreePoints(P(1), P(-1), P(0, -1));
            Assert.AreEqual(1.5 * Math.PI, major.Length, 1e-12);
            Near(P(-1), major.PointAtLength(Math.PI)); Near(P(0, -1), major.EndPoint);
            var tilted = ArcCurve3d.FromThreePoints(P(3, 0, 1), P(3, 1, 0), P(3, 0, -1));
            Assert.AreEqual(Math.PI, tilted.Length, 1e-12); Near(P(3, 1, 0), tilted.PointAt(0.5));
            Assert.ThrowsException<ArgumentException>(() => ArcCurve3d.FromThreePoints(P(0), P(1), P(2)));
            Assert.ThrowsException<ArgumentException>(() => ArcCurve3d.FromThreePoints(P(0), P(0), P(1)));
        }

        [TestMethod]
        public void ArcProjectionUsesThePlaneAndClampsToTheFiniteArc()
        {
            var arc = new ArcCurve3d(P(1, 2, 3), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 2, Math.PI / 2,
                new CurveInterval(-5, 5));
            Assert.AreEqual(0, arc.ClosestParameter(P(4, 5, 100)), 1e-12);
            Near(arc.StartPoint, arc.ClosestPoint(P(4, 0, 3)));
            Near(arc.EndPoint, arc.ClosestPoint(P(-2, 4, 3)));
            Assert.AreEqual(-5, arc.ClosestParameter(arc.Center));
            Near(arc.PointAt(0), arc.PointAtLength(arc.Length / 2));
            var reversed = arc.Reversed(); Near(arc.ClosestPoint(P(4, 5, 100)), reversed.ClosestPoint(P(4, 5, 100)));
        }

        [TestMethod]
        public void ArcTessellationBoundsSagittaAndSegmentLength()
        {
            var arc = new ArcCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 10, -1.75 * Math.PI);
            const double tolerance = 0.003, maxLength = 0.4;
            var points = arc.ToPolyline(tolerance, maxLength);
            for (int i = 1; i < points.Length; i++)
            {
                var chord = new Line3d(points[i - 1], points[i]);
                Assert.IsTrue(chord.Length <= maxLength + 1e-12);
                Assert.IsTrue(10 - chord.Mid.DistanceTo(P(0)) <= tolerance + 1e-12);
            }
            Near(arc.StartPoint, points[0]); Near(arc.EndPoint, points[points.Length - 1]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => arc.ToPolyline(1e-30));
        }

        [TestMethod]
        public void ArcTrimCopiesAndSerializationPreserveTheOrientedGeometry()
        {
            var center = P(2, 3, 4); var axis = new Vector3d(1, 0, 0);
            var arc = new ArcCurve3d(center, new Vector3d(0, 0, 1), axis, 3, -Math.PI, new CurveInterval(2, 8));
            center.X = 100; axis.X = 0; arc.Center.X = 200; Near(P(2, 3, 4), arc.Center);
            var parts = arc.Split(4); Near(arc.PointAt(3), parts[0].PointAt(3)); Near(arc.PointAt(6), parts[1].PointAt(6));
            Assert.AreEqual(arc.Length, parts[0].Length + parts[1].Length, 1e-12);
            var restored = RoundTrip(arc); Assert.AreEqual(arc, restored); Assert.AreEqual(arc.Guid, restored.Guid);
            restored.Move(10, 0, 0); Near(P(2, 3, 4), arc.Center);
            Assert.ThrowsException<ArgumentException>(() => new ArcCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 1), 1, Math.PI));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ArcCurve3d(P(0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 1, 0));
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
