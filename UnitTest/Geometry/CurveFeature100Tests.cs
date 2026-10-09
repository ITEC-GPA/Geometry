using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    /// <summary>Exactly 100 individually discovered cases: 20 domains and 20 for each concrete curve category.</summary>
    [TestClass]
    public class CurveFeature100Tests
    {
        private static Point3d P(double x, double y = 0, double z = 0) => new Point3d(x, y, z);
        private static double Norm(Vector3d v) => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        private static void Near(Point3d expected, Point3d actual, double tolerance)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance);
            Assert.AreEqual(expected.Y, actual.Y, tolerance);
            Assert.AreEqual(expected.Z, actual.Z, tolerance);
        }
        private static T Restore<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter(); formatter.Serialize(stream, value); stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
        private static CurveInterval Domain(int scenario)
        {
            switch (scenario % 5)
            {
                case 0: return new CurveInterval(0, 1);
                case 1: return new CurveInterval(10, 20);
                case 2: return new CurveInterval(-7, -2);
                case 3: return new CurveInterval(-2, 3);
                default: return new CurveInterval(0.001, 0.009);
            }
        }
        private static double Scale(int scenario) => scenario == 6 || scenario == 7 ? 0.001
            : scenario == 8 || scenario == 9 ? 1000000 : 1 + scenario * 0.125;
        private static Point3d Transform(Point3d p, double scale, int scenario)
        {
            double x = p.X, y = p.Y, z = p.Z;
            if (scenario % 3 == 1) { x = p.Z; y = p.X; z = p.Y; }
            if (scenario % 3 == 2) { x = p.Y; y = p.Z; z = p.X; }
            return P(scale * (x + scenario * 0.5), scale * (y - scenario * 0.25), scale * (z + scenario * 0.75));
        }
        private static void CopyAndSerialization(Curve3d curve, double tolerance)
        {
            var copy = curve.DuplicateCurve(); var restored = Restore(curve);
            Assert.AreEqual(curve, copy); Assert.AreEqual(curve, restored);
            Assert.AreNotEqual(curve.Guid, copy.Guid); Assert.AreEqual(curve.Guid, restored.Guid);
            double t = curve.Domain.ParameterAt(0.37); Near(curve.PointAt(t), restored.PointAt(t), tolerance);
            var original = curve.StartPoint; copy.Move(1, 2, 3); Near(original, curve.StartPoint, 0);
        }

        [DataTestMethod, TestCategory("CurveFeature100")]
        [DataRow(0, "unit interval")]
        [DataRow(1, "crosses zero")]
        [DataRow(2, "translated domain")]
        [DataRow(3, "negative domain")]
        [DataRow(4, "small domain")]
        [DataRow(5, "large domain")]
        [DataRow(6, "large offset")]
        [DataRow(7, "start boundary")]
        [DataRow(8, "end boundary")]
        [DataRow(9, "tiny domain")]
        [DataRow(10, "zero width")]
        [DataRow(11, "reversed bounds")]
        [DataRow(12, "NaN bound")]
        [DataRow(13, "infinite bound")]
        [DataRow(14, "overflowing width")]
        [DataRow(15, "uninitialized interval")]
        [DataRow(16, "fraction outside domain")]
        [DataRow(17, "parameter outside domain")]
        [DataRow(18, "NaN parameter")]
        [DataRow(19, "value equality and serialization")]
        public void DomainCases(int scenario, string description)
        {
            if (scenario < 10)
            {
                double[] starts = { 0, -10, 10, -1000000, 1e-9, -1e150, 1e10, -2, -2, -1e-150 };
                double[] ends = { 1, 10, 20, -999990, 2e-9, 1e150, 1e10 + 2, 3, 3, 1e-150 };
                double[] fractions = { 0.5, 0.25, 1.0 / 3, 0.2, 0.75, 0.5, 0.25, 0, 1, 0.25 };
                var interval = new CurveInterval(starts[scenario], ends[scenario]);
                Assert.IsTrue(interval.IsValid);
                Assert.AreEqual(interval.Start, interval.ParameterAt(0)); Assert.AreEqual(interval.End, interval.ParameterAt(1));
                Assert.AreEqual(fractions[scenario], interval.Normalize(interval.ParameterAt(fractions[scenario])), 1e-11);
                Assert.AreEqual(interval, Restore(interval)); return;
            }
            switch (scenario)
            {
                case 10: Assert.ThrowsException<ArgumentException>(() => new CurveInterval(2, 2)); break;
                case 11: Assert.ThrowsException<ArgumentException>(() => new CurveInterval(3, -2)); break;
                case 12: Assert.ThrowsException<ArgumentException>(() => new CurveInterval(double.NaN, 2)); break;
                case 13: Assert.ThrowsException<ArgumentException>(() => new CurveInterval(0, double.PositiveInfinity)); break;
                case 14: Assert.ThrowsException<ArgumentException>(() => new CurveInterval(-double.MaxValue, double.MaxValue)); break;
                case 15:
                    Assert.IsFalse(default(CurveInterval).IsValid);
                    Assert.ThrowsException<InvalidOperationException>(() => default(CurveInterval).ParameterAt(0)); break;
                case 16: Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CurveInterval(0, 1).ParameterAt(-0.1)); break;
                case 17: Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CurveInterval(10, 20).Normalize(0.5)); break;
                case 18: Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CurveInterval(0, 1).Normalize(double.NaN)); break;
                case 19:
                    var a = new CurveInterval(-2, 4); var b = Restore(a);
                    Assert.AreEqual(a, b); Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
                    Assert.AreNotEqual(a, new CurveInterval(-2, 5)); break;
            }
        }

        [DataTestMethod, TestCategory("CurveFeature100")]
        [DataRow(0, "unit xy")]
        [DataRow(1, "reversed xy")]
        [DataRow(2, "translated xy")]
        [DataRow(3, "domain 10 to 20")]
        [DataRow(4, "negative domain")]
        [DataRow(5, "small segment")]
        [DataRow(6, "large segment")]
        [DataRow(7, "vertical segment")]
        [DataRow(8, "spatial diagonal")]
        [DataRow(9, "negative diagonal")]
        [DataRow(10, "short reversed")]
        [DataRow(11, "long reversed")]
        [DataRow(12, "offset domain")]
        [DataRow(13, "negative coordinates")]
        [DataRow(14, "trimmed copy")]
        [DataRow(15, "split pieces")]
        [DataRow(16, "dense tessellation")]
        [DataRow(17, "serialization")]
        [DataRow(18, "subunit domain")]
        [DataRow(19, "domain spanning zero")]
        public void LineCases(int scenario, string description)
        {
            double length = scenario == 5 ? 1e-9 : scenario == 6 ? 1e100 : 2 + scenario;
            Vector3d unit = scenario == 7 ? new Vector3d(0, 0, 1)
                : scenario == 8 ? new Vector3d(2.0 / 7, 3.0 / 7, 6.0 / 7) : new Vector3d(0.6, 0.8, 0);
            if (scenario % 2 == 1) unit = unit * -1;
            Point3d start = P(length * scenario / 5, -length * scenario / 7, length * scenario / 11);
            Point3d end = start + unit * length;
            var curve = new LineCurve3d(start, end, Domain(scenario));
            double tolerance = length * 1e-10;
            Assert.AreEqual(length, curve.Length, tolerance); Assert.IsFalse(curve.IsClosed);
            var expectedMid = start + unit * (length / 2);
            Near(expectedMid, curve.PointAt(curve.Domain.ParameterAt(0.5)), tolerance);
            Near(expectedMid, curve.PointAtLength(curve.Length / 2), tolerance);
            Near(end, curve.ClosestPoint(end + unit * length), tolerance);
            Near(start, curve.ClosestPoint(start - unit * length), tolerance);
            Assert.AreEqual(1, Norm(curve.TangentAt(curve.Domain.Start)), 1e-12);
            double split = curve.Domain.ParameterAt(0.37); var parts = curve.Split(split);
            Assert.AreEqual(curve.Length, parts.Sum(p => p.Length), tolerance);
            Near(curve.PointAt(split), parts[0].EndPoint, tolerance); Near(parts[0].EndPoint, parts[1].StartPoint, tolerance);
            var reversed = curve.Reversed(); Near(end, reversed.StartPoint, tolerance); Near(start, reversed.EndPoint, tolerance);
            var vertices = curve.ToPolyline(length / 100, length / (3 + scenario));
            for (int i = 1; i < vertices.Length; i++) Assert.IsTrue(vertices[i].DistanceTo(vertices[i - 1]) <= length / (3 + scenario) + tolerance);
            CopyAndSerialization(curve, tolerance);
            curve.SetDomain(new CurveInterval(100, 200)); Near(expectedMid, curve.PointAt(150), tolerance);
        }

        [DataTestMethod, TestCategory("CurveFeature100")]
        [DataRow(0, "positive minor xy")]
        [DataRow(1, "negative minor xy")]
        [DataRow(2, "positive quarter yz")]
        [DataRow(3, "negative quarter xz")]
        [DataRow(4, "semicircle xy")]
        [DataRow(5, "negative semicircle yz")]
        [DataRow(6, "major xy")]
        [DataRow(7, "negative major xz")]
        [DataRow(8, "full circle xy")]
        [DataRow(9, "negative full circle yz")]
        [DataRow(10, "small radius")]
        [DataRow(11, "large radius")]
        [DataRow(12, "tilted normal")]
        [DataRow(13, "translated major")]
        [DataRow(14, "translated clockwise")]
        [DataRow(15, "small sweep")]
        [DataRow(16, "large sweep")]
        [DataRow(17, "nonunit domain")]
        [DataRow(18, "negative domain")]
        [DataRow(19, "circle seam")]
        public void ArcCases(int scenario, string description)
        {
            double[] sweeps = { 0.2, -0.2, Math.PI / 2, -Math.PI / 2, Math.PI, -Math.PI, 1.5 * Math.PI, -1.5 * Math.PI,
                2 * Math.PI, -2 * Math.PI, 0.75 * Math.PI, -0.75 * Math.PI, 1.25 * Math.PI, -1.25 * Math.PI,
                -0.4, 0.01, 1.9 * Math.PI, 1.1, -2.7, 2 * Math.PI };
            double radius = scenario == 10 ? 1e-6 : scenario == 11 ? 1e6 : 1 + scenario * 0.25;
            Vector3d normal = scenario % 3 == 0 ? new Vector3d(0, 0, 1)
                : scenario % 3 == 1 ? new Vector3d(1, 0, 0) : new Vector3d(0.6, 0.8, 0);
            Vector3d first = scenario % 3 == 0 ? new Vector3d(1, 0, 0) : new Vector3d(0, 0, 1);
            var center = P(radius * 2, radius * -3, radius * 5);
            var curve = new ArcCurve3d(center, normal, first, radius, sweeps[scenario], Domain(scenario));
            double tolerance = radius * 1e-8;
            Assert.AreEqual(radius * Math.Abs(sweeps[scenario]), curve.Length, tolerance);
            Assert.AreEqual(Math.Abs(sweeps[scenario]) == 2 * Math.PI, curve.IsClosed);
            for (int i = 0; i <= 8; i++)
            {
                double fraction = i / 8.0; var point = curve.PointAt(curve.Domain.ParameterAt(fraction));
                Assert.AreEqual(radius, Norm(point - center), tolerance);
                Assert.AreEqual(0, new Vector3d(point - center).DotProduct(normal), tolerance);
                Near(point, curve.PointAtLength(curve.Length * fraction), tolerance);
                Assert.AreEqual(1, Norm(curve.TangentAt(curve.Domain.ParameterAt(fraction))), 1e-12);
            }
            double middle = curve.Domain.ParameterAt(0.37);
            Near(curve.PointAt(middle), curve.ClosestPoint(curve.PointAt(middle) + normal * radius), tolerance);
            var reversed = curve.Reversed(); Near(curve.StartPoint, reversed.EndPoint, tolerance); Near(curve.EndPoint, reversed.StartPoint, tolerance);
            var parts = curve.Split(curve.Domain.ParameterAt(0.4)); Assert.AreEqual(curve.Length, parts.Sum(p => p.Length), tolerance);
            var vertices = curve.ToPolyline(radius / 1000, radius / 3);
            for (int i = 1; i < vertices.Length; i++)
            {
                var chordMid = new Point3d((vertices[i - 1].X + vertices[i].X) / 2, (vertices[i - 1].Y + vertices[i].Y) / 2,
                    (vertices[i - 1].Z + vertices[i].Z) / 2);
                Assert.IsTrue(radius - Norm(chordMid - center) <= radius / 1000 + tolerance);
                Assert.IsTrue(Norm(vertices[i] - vertices[i - 1]) <= radius / 3 + tolerance);
            }
            if (!curve.IsClosed)
            {
                var through = ArcCurve3d.FromThreePoints(curve.StartPoint, curve.PointAt(middle), curve.EndPoint, radius * 1e-10);
                Assert.AreEqual(curve.Length, through.Length, tolerance);
                Near(curve.PointAt(middle), through.PointAt(0.37), tolerance);
            }
            else Near(curve.StartPoint, curve.EndPoint, 0);
            CopyAndSerialization(curve, tolerance);
        }

        [DataTestMethod, TestCategory("CurveFeature100")]
        [DataRow(0, "open xy")]
        [DataRow(1, "closed xy")]
        [DataRow(2, "open yz")]
        [DataRow(3, "closed yz")]
        [DataRow(4, "open spatial")]
        [DataRow(5, "closed spatial")]
        [DataRow(6, "small open")]
        [DataRow(7, "small closed")]
        [DataRow(8, "large open")]
        [DataRow(9, "large closed")]
        [DataRow(10, "translated open")]
        [DataRow(11, "translated closed")]
        [DataRow(12, "negative domain open")]
        [DataRow(13, "negative domain closed")]
        [DataRow(14, "offset domain open")]
        [DataRow(15, "offset domain closed")]
        [DataRow(16, "trim open")]
        [DataRow(17, "trim closed")]
        [DataRow(18, "serialized open")]
        [DataRow(19, "serialized closed")]
        public void PolylineCases(int scenario, string description)
        {
            double scale = Scale(scenario), tolerance = scale * 1e-9;
            bool closed = scenario % 2 == 1;
            var source = new[] { P(0), P(3), P(3, 4), P(3, 4, 12) };
            if (closed) source = source.Concat(new[] { P(0) }).ToArray();
            var vertices = source.Select(p => Transform(p, scale, scenario)).ToArray();
            var curve = new PolylineCurve3d(vertices, Domain(scenario));
            Assert.AreEqual((closed ? 32 : 19) * scale, curve.Length, tolerance); Assert.AreEqual(closed, curve.IsClosed);
            double[] distances = { 0, 3, 7, 19, 32 };
            for (int i = 0; i < vertices.Length; i++)
            {
                Near(vertices[i], curve.PointAt(curve.ParameterAtVertex(i)), tolerance);
                Near(vertices[i], curve.PointAtLength(distances[i] * scale), tolerance);
            }
            var trimmed = curve.Trim(curve.Domain.ParameterAt(0.13), curve.Domain.ParameterAt(0.87));
            for (int i = 0; i <= 10; i++)
            {
                double t = trimmed.Domain.ParameterAt(i / 10.0); Near(curve.PointAt(t), trimmed.PointAt(t), tolerance);
                double distance = curve.Length * i / 10;
                Assert.AreEqual(distance, curve.LengthAt(curve.ParameterAtLength(distance)), tolerance);
            }
            var reverse = curve.Reversed(); Near(vertices[0], reverse.EndPoint, tolerance); Near(vertices[vertices.Length - 1], reverse.StartPoint, tolerance);
            double corner = curve.ParameterAtVertex(1);
            Assert.IsTrue(Norm(curve.TangentAt(corner, CurveEvaluationSide.Above) - curve.TangentAt(corner, CurveEvaluationSide.Below)) > 1);
            var sampled = curve.ToPolyline(scale / 100, scale * 2);
            foreach (var vertex in vertices) Assert.IsTrue(sampled.Any(p => Norm(p - vertex) <= tolerance));
            for (int i = 1; i < sampled.Length; i++) Assert.IsTrue(Norm(sampled[i] - sampled[i - 1]) <= scale * 2 + tolerance);
            var actualStart = curve.StartPoint; vertices[0].X += scale * 100; curve.Vertices[0].X += scale * 200;
            Near(actualStart, curve.StartPoint, 0); CopyAndSerialization(trimmed, tolerance);
            Near(curve.PointAt(corner), curve.ClosestPoint(curve.PointAt(corner)), tolerance);
        }

        [DataTestMethod, TestCategory("CurveFeature100")]
        [DataRow(0, "line arc line")]
        [DataRow(1, "reversed chain")]
        [DataRow(2, "nested polyline")]
        [DataRow(3, "reversed nested")]
        [DataRow(4, "small chain")]
        [DataRow(5, "small reversed")]
        [DataRow(6, "large chain")]
        [DataRow(7, "large reversed")]
        [DataRow(8, "translated chain")]
        [DataRow(9, "translated reverse")]
        [DataRow(10, "tilted chain")]
        [DataRow(11, "tilted reverse")]
        [DataRow(12, "negative domain")]
        [DataRow(13, "negative reverse")]
        [DataRow(14, "offset domain")]
        [DataRow(15, "offset reverse")]
        [DataRow(16, "nested trimmed")]
        [DataRow(17, "nested reversed trim")]
        [DataRow(18, "serialized nested")]
        [DataRow(19, "serialized reversed")]
        public void PolyCurveCases(int scenario, string description)
        {
            double scale = Scale(scenario), tolerance = scale * 1e-8;
            Point3d a = Transform(P(0), scale, scenario), b = Transform(P(2), scale, scenario);
            var center = Transform(P(2, 1), scale, scenario);
            var normal = (Transform(P(0, 0, 1), scale, scenario) - Transform(P(0), scale, scenario)) / scale;
            var first = (b - center) / scale;
            var arc = new ArcCurve3d(center, normal, first, scale, Math.PI / 2, new CurveInterval(-4, -2));
            Curve3d initial = scenario % 4 < 2 ? (Curve3d)new LineCurve3d(a, b, new CurveInterval(10, 20))
                : new PolylineCurve3d(new[] { a, Transform(P(0.5), scale, scenario), b }, new CurveInterval(10, 20));
            var end = Transform(P(3, 4), scale, scenario);
            Curve3d curve = new PolyCurve3d(new[] { initial, arc, new LineCurve3d(arc.EndPoint, end, new CurveInterval(2, 8)) },
                joinTolerance: scale * 1e-8, domain: Domain(scenario));
            if (scenario % 2 == 1) curve = curve.Reversed();
            Assert.AreEqual((5 + Math.PI / 2) * scale, curve.Length, tolerance);
            Near(scenario % 2 == 0 ? a : end, curve.StartPoint, tolerance);
            Near(scenario % 2 == 0 ? end : a, curve.EndPoint, tolerance);
            for (int i = 0; i <= 20; i++)
            {
                double distance = curve.Length * (i / 20.0);
                Assert.AreEqual(distance, curve.LengthAt(curve.ParameterAtLength(distance)), tolerance);
                var point = curve.PointAt(curve.Domain.ParameterAt(i / 20.0)); Near(point, curve.ClosestPoint(point), tolerance);
            }
            double split = curve.Domain.ParameterAt(0.41); var parts = curve.Split(split);
            Assert.AreEqual(curve.Length, parts.Sum(c => c.Length), tolerance);
            Near(parts[0].EndPoint, parts[1].StartPoint, tolerance);
            var trimmed = curve.Trim(curve.Domain.ParameterAt(0.11), curve.Domain.ParameterAt(0.89));
            for (int i = 0; i <= 10; i++)
            {
                double parameter = trimmed.Domain.ParameterAt(i / 10.0); Near(curve.PointAt(parameter), trimmed.PointAt(parameter), tolerance);
            }
            var samples = curve.ToPolyline(scale * 0.001, scale * 0.3);
            for (int i = 1; i < samples.Length; i++) Assert.IsTrue(Norm(samples[i] - samples[i - 1]) <= scale * 0.3 + tolerance);
            CopyAndSerialization(trimmed, tolerance);
            var nested = new PolyCurve3d(new[] { curve }, joinTolerance: scale * 1e-8);
            Near(curve.PointAtLength(curve.Length * 0.37), nested.PointAtLength(nested.Length * 0.37), tolerance);
            CopyAndSerialization(nested, tolerance);
        }
    }
}

