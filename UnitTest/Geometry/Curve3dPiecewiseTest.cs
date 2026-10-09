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
