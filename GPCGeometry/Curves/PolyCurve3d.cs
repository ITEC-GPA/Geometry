using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// An ordered chain of owned curve copies. Each child initially occupies an equal parameter interval.
    /// Consecutive endpoints must meet within JoinTolerance; they are never moved or reordered automatically.
    /// Length excludes tolerated junction gaps. Tessellation bridges those gaps explicitly, or rejects a tighter tolerance.
    /// </summary>
    [Serializable]
    public sealed class PolyCurve3d : Curve3d
    {
        private Curve3d[] segments;
        private double[] knots;
        [NonSerialized] private double[] lengths;
        public double JoinTolerance { get; private set; }
        public int SegmentCount { get { EnsureInitialized(); return segments.Length; } }
        public override double Length { get { EnsureInitialized(); return lengths[lengths.Length - 1]; } }
        public override bool IsClosed => CurveMath.Distance(StartPoint, EndPoint) <= JoinTolerance;

        public PolyCurve3d(IEnumerable<Curve3d> segments, double joinTolerance = Tolerance, CurveInterval? domain = null) : base(domain)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            JoinTolerance = joinTolerance; Initialize(segments.ToArray(), null);
        }
        private PolyCurve3d(Curve3d[] curves, double[] partition, double joinTolerance, CurveInterval domain) : base(domain)
        {
            JoinTolerance = joinTolerance; Initialize(curves, partition);
        }
        private PolyCurve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Object-array fixups, including nested polycurves, finish after serialization constructors.
            // Rebuild and validate the owned copies lazily, on the first public geometry operation.
            segments = (Curve3d[])info.GetValue("Segments", typeof(Curve3d[]));
            knots = (double[])info.GetValue("Knots", typeof(double[]));
            JoinTolerance = info.GetDouble("JoinTolerance");
        }
        private void EnsureInitialized() { if (lengths == null) Initialize(segments, knots); }
        private void Initialize(Curve3d[] curves, double[] partition)
        {
            CurveMath.Finite(JoinTolerance, nameof(JoinTolerance));
            if (JoinTolerance < 0) throw new ArgumentOutOfRangeException(nameof(JoinTolerance));
            if (curves == null || curves.Length == 0) throw new ArgumentException("A polycurve requires at least one segment.", nameof(curves));
            var owned = new Curve3d[curves.Length]; var accumulated = new double[curves.Length + 1];
            for (int i = 0; i < curves.Length; i++)
            {
                if (curves[i] is null) throw new ArgumentException("A polycurve cannot contain null segments.", nameof(curves));
                owned[i] = curves[i].DuplicateCurve(); CurveMath.Positive(owned[i].Length, nameof(curves));
                if (i > 0 && CurveMath.Distance(owned[i - 1].EndPoint, owned[i].StartPoint) > JoinTolerance)
                    throw new ArgumentException("Consecutive curve endpoints do not meet within JoinTolerance.", nameof(curves));
                accumulated[i + 1] = accumulated[i] + owned[i].Length;
                if (!CurveMath.IsFinite(accumulated[i + 1]) || accumulated[i + 1] <= accumulated[i])
                    throw new ArgumentException("Curve lengths cannot be represented at this numeric scale.", nameof(curves));
            }
            knots = CurvePartition.CopyKnots(partition ?? CurvePartition.Uniform(curves.Length), curves.Length);
            segments = owned; lengths = accumulated;
        }
        /// <summary>Returns an independent child curve in its own parameter domain.</summary>
        public Curve3d GetSegment(int index) { EnsureInitialized(); return segments[index].DuplicateCurve(); }
        /// <summary>The interval occupied by a child in this curve's domain.</summary>
        public CurveInterval SegmentDomain(int index)
        {
            EnsureInitialized(); return new CurveInterval(Domain.ParameterAt(knots[index]), Domain.ParameterAt(knots[index + 1]));
        }
        private double ChildParameter(int index, double normalized)
            => segments[index].Domain.ParameterAt(CurvePartition.Fraction(knots, index, normalized));
        public override Point3d PointAt(double parameter)
        {
            EnsureInitialized(); double u = Domain.Normalize(parameter); int i = CurvePartition.Find(knots, u);
            return segments[i].PointAt(ChildParameter(i, u));
        }
        public override Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            EnsureInitialized(); double u = Domain.Normalize(parameter); int i = CurvePartition.Find(knots, u, side);
            return segments[i].TangentAt(ChildParameter(i, u), side);
        }
        public override double LengthAt(double parameter)
        {
            EnsureInitialized(); double u = Domain.Normalize(parameter); int i = CurvePartition.Find(knots, u);
            return lengths[i] + segments[i].LengthAt(ChildParameter(i, u));
        }
        public override double ParameterAtLength(double distance)
        {
            EnsureInitialized(); CurveMath.InRange(distance, 0, Length, nameof(distance));
            if (distance == 0) return Domain.Start;
            if (distance == Length) return Domain.End;
            int i = CurvePartition.Find(lengths, distance);
            double localDistance = Math.Max(0, Math.Min(segments[i].Length, distance - lengths[i]));
            double u = segments[i].Domain.Normalize(segments[i].ParameterAtLength(localDistance));
            return Domain.ParameterAt(knots[i] + u * (knots[i + 1] - knots[i]));
        }
        public override double ClosestParameter(Point3d point)
        {
            EnsureInitialized(); point = CurveMath.Copy(point, nameof(point));
            double best = Domain.Start, bestDistance = double.PositiveInfinity;
            for (int i = 0; i < segments.Length; i++)
            {
                double local = segments[i].ClosestParameter(point);
                double distance = CurveMath.Distance(point, segments[i].PointAt(local));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = Domain.ParameterAt(knots[i] + segments[i].Domain.Normalize(local) * (knots[i + 1] - knots[i]));
                }
            }
            return best;
        }
        public override Curve3d Trim(double start, double end)
        {
            EnsureInitialized(); ValidateTrim(start, end);
            double a = Domain.Normalize(start), b = Domain.Normalize(end);
            var curves = new List<Curve3d>(); var partition = new List<double> { 0 };
            for (int i = 0; i < segments.Length; i++)
            {
                double left = Math.Max(a, knots[i]), right = Math.Min(b, knots[i + 1]);
                if (left >= right) continue;
                curves.Add(segments[i].Trim(ChildParameter(i, left), ChildParameter(i, right)));
                partition.Add((right - a) / (b - a));
            }
            partition[partition.Count - 1] = 1;
            return CopyMetadata(new PolyCurve3d(curves.ToArray(), partition.ToArray(), JoinTolerance, new CurveInterval(start, end)));
        }
        public override Curve3d Reversed()
        {
            EnsureInitialized();
            return CopyMetadata(new PolyCurve3d(segments.Reverse().Select(c => c.Reversed()).ToArray(), CurvePartition.Reverse(knots), JoinTolerance, Domain));
        }
        public override Curve3d DuplicateCurve()
        {
            EnsureInitialized(); return CopyMetadata(new PolyCurve3d(segments, knots, JoinTolerance, Domain));
        }
        public override Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            EnsureInitialized(); CurveMath.Tessellation(tolerance, maxSegmentLength);
            var result = new List<Point3d>();
            foreach (var segment in segments)
            {
                Point3d[] part = segment.ToPolyline(tolerance, maxSegmentLength);
                if (result.Count == 0) result.Add(part[0]);
                else
                {
                    double gap = CurveMath.Distance(result[result.Count - 1], part[0]);
                    if (gap > tolerance) throw new ArgumentException("A junction gap exceeds the requested tessellation tolerance.", nameof(tolerance));
                    if (gap > 0)
                    {
                        var bridge = new LineCurve3d(result[result.Count - 1], part[0]).ToPolyline(tolerance, maxSegmentLength);
                        Append(result, bridge);
                    }
                }
                Append(result, part);
            }
            return result.ToArray();
        }
        private static void Append(List<Point3d> target, Point3d[] source)
        {
            CurveMath.SegmentCount((double)target.Count + source.Length - 2);
            for (int i = 1; i < source.Length; i++) target.Add(source[i]);
        }
        public override void Move(double x, double y, double z)
        {
            EnsureInitialized(); CurveMath.Finite(x, nameof(x)); CurveMath.Finite(y, nameof(y)); CurveMath.Finite(z, nameof(z));
            var moved = segments.Select(c => c.DuplicateCurve()).ToArray();
            foreach (var curve in moved) curve.Move(x, y, z);
            Initialize(moved, knots);
        }
        protected override bool EqualsCurve(Curve3d other)
        {
            EnsureInitialized(); var curve = (PolyCurve3d)other; curve.EnsureInitialized();
            if (segments.Length != curve.segments.Length || JoinTolerance != curve.JoinTolerance) return false;
            for (int i = 0; i < segments.Length; i++)
                if (!segments[i].Equals(curve.segments[i]) || Math.Abs(knots[i] - curve.knots[i]) > 1e-12) return false;
            return true;
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            EnsureInitialized(); base.GetObjectData(info, context);
            info.AddValue("Segments", segments); info.AddValue("Knots", knots); info.AddValue("JoinTolerance", JoinTolerance);
        }
    }
}
