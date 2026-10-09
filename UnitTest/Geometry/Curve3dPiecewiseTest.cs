using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    public partial class Curve3dTest
    {
        [TestMethod]
        public void PiecewiseCurveRecognizesCornerParametersAfterDomainChanges()
        {
            var vertices = new[] { P(0), P(1), P(1, 2), P(4, 2) };
            var polyline = new PolylineCurve3d(vertices, new CurveInterval(10, 20));
            var polycurve = new PolyCurve3d(new Curve3d[] { new LineCurve3d(vertices[0], vertices[1]),
                new LineCurve3d(vertices[1], vertices[2]), new LineCurve3d(vertices[2], vertices[3]) }, domain: new CurveInterval(10, 20));
            foreach (var curve in new Curve3d[] { polyline, polycurve })
            {
                double corner = curve.Domain.ParameterAt(1.0 / 3);
                Assert.AreEqual(1, curve.TangentAt(corner, CurveEvaluationSide.Below).X, 1e-12);
                Assert.AreEqual(1, curve.TangentAt(corner, CurveEvaluationSide.Above).Y, 1e-12);
                Near(P(1), curve.PointAt(corner), 0);
            }
        }

        private static PolyCurve3d MixedCurve()
        {
            var arc = new ArcCurve3d(P(2, 1), new Vector3d(0, 0, 1), new Vector3d(0, -1, 0), 1, Math.PI / 2,
                new CurveInterval(10, 20));
            return new PolyCurve3d(new Curve3d[] { new LineCurve3d(P(0), P(2)), arc, new LineCurve3d(arc.EndPoint, P(3, 4)) },
                domain: new CurveInterval(0, 3));
        }

        [TestMethod]
        public void PolyCurveEvaluatesMixedSegmentsAndInvertsLengthAcrossTheirDomains()
        {
            var curve = MixedCurve(); Assert.AreEqual(3, curve.SegmentCount);
            Assert.AreEqual(5 + Math.PI / 2, curve.Length, 1e-12);
            Near(P(2), curve.PointAt(1)); Near(P(3, 1), curve.PointAt(2));
            Near(P(3, 2), curve.PointAtLength(3 + Math.PI / 2));
            Assert.AreEqual(2, curve.LengthAt(1), 1e-12);
            for (int i = 0; i <= 100; i++)
            {
                double distance = curve.Length * i / 100;
                Assert.AreEqual(distance, curve.LengthAt(curve.ParameterAtLength(distance)), 1e-11);
            }
            Near(P(3, 2), curve.ClosestPoint(P(4, 2, 10)));
            Assert.AreEqual(1, curve.TangentAt(1, CurveEvaluationSide.Below).X, 1e-12);
            Assert.AreEqual(1, curve.TangentAt(1, CurveEvaluationSide.Above).X, 1e-12);
            var polyline = new PolylineCurve3d(new[] { P(0), P(1), P(1, 9) });
            var nested = new PolyCurve3d(new Curve3d[] { polyline, new LineCurve3d(P(1, 9), P(2, 9)) });
            Near(P(1, 4), nested.PointAtLength(5));
            Assert.AreEqual(5, nested.LengthAt(nested.ParameterAtLength(5)), 1e-12);
            Assert.AreEqual(curve.Domain.End, curve.ParameterAtLength(curve.Length));
        }

        [TestMethod]
        public void PolyCurveTrimSplitAndReverseRetainGlobalParameters()
        {
            var curve = MixedCurve(); var trimmed = curve.Trim(0.3, 2.8);
            for (int i = 0; i <= 20; i++)
            {
                double parameter = 0.3 + 2.5 * i / 20;
                Near(curve.PointAt(parameter), trimmed.PointAt(parameter));
            }
            var parts = curve.Split(1.5); Assert.AreEqual(curve.Length, parts.Sum(p => p.Length), 1e-12);
            var reversed = curve.Reversed();
            for (int i = 0; i <= 30; i++) Near(curve.PointAt(i / 10.0), reversed.PointAt(3 - i / 10.0));
            Assert.AreEqual(curve, reversed.Reversed());
            var singleChild = (PolyCurve3d)curve.Trim(1.2, 1.8); Assert.AreEqual(1, singleChild.SegmentCount);
        }

        [TestMethod]
        public void PolyCurveOwnsChildrenAndRejectsDisconnectedInput()
        {
            var line = new LineCurve3d(P(0), P(1));
            var curve = new PolyCurve3d(new Curve3d[] { line, new LineCurve3d(P(1), P(1, 2)) });
            line.Move(20, 0, 0); curve.GetSegment(0).Move(30, 0, 0); Near(P(0), curve.StartPoint);
            var copy = curve.DuplicateCurve(); copy.Move(2, 3, 4);
            Near(P(0), curve.StartPoint); Near(P(2, 3, 4), copy.StartPoint);
            Assert.AreEqual(1, curve.TangentAt(0.5, CurveEvaluationSide.Below).X, 1e-12);
            Assert.AreEqual(1, curve.TangentAt(0.5).Y, 1e-12);
            Assert.ThrowsException<ArgumentException>(() => new PolyCurve3d(new Curve3d[0]));
            Assert.ThrowsException<ArgumentException>(() => new PolyCurve3d(new Curve3d[] { null }));
            Assert.ThrowsException<ArgumentException>(() => new PolyCurve3d(new Curve3d[] { new LineCurve3d(P(0), P(1)), new LineCurve3d(P(2), P(3)) }));
        }

        [TestMethod]
        public void PolyCurveTessellationPreservesCornersAndChecksJunctionGaps()
        {
            var mixed = MixedCurve(); var points = mixed.ToPolyline(0.001, 0.25);
            Assert.IsTrue(points.Any(p => p.Equals(P(2)))); Assert.IsTrue(points.Any(p => p.Equals(P(3, 1))));
            for (int i = 1; i < points.Length; i++) Assert.IsTrue(points[i].DistanceTo(points[i - 1]) <= 0.25 + 1e-12);
            var gap = new PolyCurve3d(new Curve3d[] { new LineCurve3d(P(0), P(1)), new LineCurve3d(P(1.00001), P(2)) });
            Assert.ThrowsException<ArgumentException>(() => gap.ToPolyline(1e-6));
            var bridged = gap.ToPolyline(1e-4); Assert.AreEqual(4, bridged.Length);
            Near(P(1), bridged[1]); Near(P(1.00001), bridged[2]);
        }

        [TestMethod]
        public void NestedPolyCurveSerializationRestoresGeometryAndTrimmedPartitions()
        {
            var inner = MixedCurve();
            var curve = new PolyCurve3d(new Curve3d[] { inner, new LineCurve3d(inner.EndPoint, P(0)) }).Trim(0.1, 0.9);
            var restored = RoundTrip(curve); Assert.AreEqual(curve, restored); Assert.AreEqual(curve.Guid, restored.Guid);
            Assert.AreEqual(curve.Length, restored.Length, 1e-12);
            for (int i = 1; i <= 9; i++) Near(curve.PointAt(i / 10.0), restored.PointAt(i / 10.0));
            var closed = new PolyCurve3d(new Curve3d[] { inner, new LineCurve3d(inner.EndPoint, P(0)) });
            Assert.IsTrue(closed.IsClosed); Assert.AreEqual(0, closed.ClosestParameter(P(0)), 1e-12);
        }

        [TestMethod]
        public void PolyCurveEnumeratesInputOnceAndKeepsItsDomainWhenReparameterized()
        {
            int visits = 0;
            IEnumerable<Curve3d> Segments()
            {
                for (int i = 0; i < 1000; i++) { visits++; yield return new LineCurve3d(P(i), P(i + 1)); }
            }
            var curve = new PolyCurve3d(Segments()); Assert.AreEqual(1000, visits);
            curve.SetDomain(new CurveInterval(-100, 100)); Near(P(750), curve.PointAt(50));
            Near(P(678), curve.PointAtLength(678));
            Assert.AreEqual(0, curve.SegmentDomain(500).Start, 1e-12);
        }

        [TestMethod]
        public void PolylineCurveUsesSegmentParametersAndInvertsPhysicalDistance()
        {
            var curve = new PolylineCurve3d(new[] { P(0), P(1), P(1, 9) }, new CurveInterval(2, 6));
            Near(P(1), curve.PointAt(4)); Near(P(1, 4), curve.PointAtLength(5));
            Assert.AreEqual(10, curve.Length, 1e-12); Assert.AreEqual(1, curve.LengthAt(4), 1e-12);
            Assert.AreEqual(4 + 8.0 / 9, curve.ParameterAtLength(5), 1e-12);
            Assert.AreEqual(1, curve.TangentAt(4, CurveEvaluationSide.Below).X, 1e-12);
            Assert.AreEqual(1, curve.TangentAt(4).Y, 1e-12);
            Assert.AreEqual(1, curve.TangentAt(6, CurveEvaluationSide.Above).Y, 1e-12);
            for (int i = 0; i <= 100; i++) Assert.AreEqual(i / 10.0, curve.LengthAt(curve.ParameterAtLength(i / 10.0)), 1e-12);
        }

        [TestMethod]
        public void PolylineTrimRetainsParametersAcrossUnequalPartialSegments()
        {
            var curve = new PolylineCurve3d(new[] { P(0), P(1), P(1, 9), P(4, 9) });
            var trimmed = curve.Trim(0.2, 0.9);
            for (int i = 0; i <= 20; i++)
            {
                double parameter = 0.2 + 0.7 * i / 20;
                Near(curve.PointAt(parameter), trimmed.PointAt(parameter));
            }
            var parts = curve.Split(0.5); Assert.AreEqual(curve.Length, parts.Sum(p => p.Length), 1e-12);
            var reverse = trimmed.Reversed();
            for (int i = 0; i <= 20; i++) Near(trimmed.PointAt(0.2 + 0.7 * i / 20), reverse.PointAt(0.9 - 0.7 * i / 20));
            Assert.AreEqual(trimmed, reverse.Reversed());
        }

        [TestMethod]
        public void PolylineClosestPointHandlesCornersAndClosedSeams()
        {
            var curve = new PolylineCurve3d(new[] { P(0), P(2), P(2, 3), P(0) });
            Assert.IsTrue(curve.IsClosed); Assert.AreEqual(0, curve.ClosestParameter(P(0)), 1e-12);
            Near(P(2, 1), curve.ClosestPoint(P(4, 1, 5)));
            Near(P(2), curve.ClosestPoint(P(4, -2)));
        }

        [TestMethod]
        public void PolylineTessellationPreservesEveryCornerAndCopiesVertices()
        {
            var vertices = new[] { P(0), P(1), P(1, 3) }; var curve = new PolylineCurve3d(vertices);
            vertices[1].X = 10; curve.Vertices[1].X = 20;
            var points = curve.ToPolyline(100, 0.7);
            Assert.IsTrue(points.Any(p => p.X == 1 && p.Y == 0));
            for (int i = 1; i < points.Length; i++) Assert.IsTrue(points[i].DistanceTo(points[i - 1]) <= 0.7 + 1e-12);
            var lines = curve.ToLineSegments(); lines[0].End.X = 50;
            Near(P(1), lines[1].Start); Near(P(1), curve.PointAt(0.5));
            curve.Move(2, 3, 4); Near(P(3, 3, 4), curve.PointAt(0.5)); Assert.AreEqual(4, curve.Length, 1e-12);
        }

        [TestMethod]
        public void PolylineSerializationRetainsTrimmedParameterPartition()
        {
            var original = new PolylineCurve3d(new[] { P(0), P(2), P(2, 6) }, new CurveInterval(0, 10)).Trim(1, 8);
            var restored = RoundTrip(original); Assert.AreEqual(original, restored); Assert.AreEqual(original.Guid, restored.Guid);
            for (int i = 1; i <= 8; i++) Near(original.PointAt(i), restored.PointAt(i));
            restored.Move(2, 3, 4); Assert.AreNotEqual(original, restored);
        }

        [TestMethod]
        public void PolylineRejectsDegenerateAndNonFiniteVertices()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PolylineCurve3d(null));
            Assert.ThrowsException<ArgumentException>(() => new PolylineCurve3d(new[] { P(0) }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new PolylineCurve3d(new[] { P(0), P(0) }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new PolylineCurve3d(new[] { P(0), P(double.NaN) }));
            var curve = new PolylineCurve3d(new[] { P(0), P(1), P(1, 1) });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.TangentAt(0.5, (CurveEvaluationSide)42));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => curve.ParameterAtLength(-1));
        }

        [TestMethod]
        public void PolylineEnumeratesSourceOnceAndHandlesManySegments()
        {
            int visits = 0;
            IEnumerable<Point3d> Vertices()
            {
                for (int i = 0; i <= 10000; i++) { visits++; yield return P(i); }
            }
            var curve = new PolylineCurve3d(Vertices()); Assert.AreEqual(10001, visits);
            Near(P(6789), curve.PointAtLength(6789)); Near(P(6789), curve.PointAt(0.6789));
            Assert.AreEqual(10000, curve.Length, 1e-12);
        }
    }
}
