using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// A spatial polyline, open or closed by a repeated first vertex. Consecutive identical vertices are rejected.
    /// Initially every segment occupies an equal parameter interval, irrespective of its length.
    /// Trimming preserves the original parameters. Vertices and cached lengths cannot be mutated externally.
    /// </summary>
    [Serializable]
    public sealed class PolylineCurve3d : Curve3d
    {
        private Point3d[] points;
        private double[] knots, lengths;
        public int VertexCount => points.Length;
        public Point3d[] Vertices => points.Select(p => new Point3d(p)).ToArray();
        public override double Length => lengths[lengths.Length - 1];

        public PolylineCurve3d(IEnumerable<Point3d> vertices, CurveInterval? domain = null) : base(domain)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            Initialize(vertices.ToArray(), null);
        }
        private PolylineCurve3d(Point3d[] vertices, double[] partition, CurveInterval domain) : base(domain)
        {
            Initialize(vertices, partition);
        }
        private PolylineCurve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var xyz = (double[])info.GetValue("Coordinates", typeof(double[]));
            if (xyz == null || xyz.Length % 3 != 0) throw new SerializationException("Invalid polyline coordinates.");
            var vertices = new Point3d[xyz.Length / 3];
            for (int i = 0; i < vertices.Length; i++) vertices[i] = new Point3d(xyz[3 * i], xyz[3 * i + 1], xyz[3 * i + 2]);
            Initialize(vertices, (double[])info.GetValue("Knots", typeof(double[])));
        }
        private void Initialize(Point3d[] vertices, double[] partition)
        {
            if (vertices.Length < 2) throw new ArgumentException("A polyline requires at least two vertices.", nameof(vertices));
            points = vertices.Select(p => CurveMath.Copy(p, nameof(vertices))).ToArray();
            knots = CurvePartition.CopyKnots(partition ?? CurvePartition.Uniform(points.Length - 1), points.Length - 1);
            lengths = new double[points.Length];
            for (int i = 1; i < points.Length; i++)
            {
                double length = CurveMath.Distance(points[i - 1], points[i]);
                CurveMath.Positive(length, nameof(vertices)); lengths[i] = lengths[i - 1] + length;
                if (!CurveMath.IsFinite(lengths[i]) || lengths[i] <= lengths[i - 1])
                    throw new ArgumentException("Polyline lengths cannot be represented at this numeric scale.", nameof(vertices));
            }
        }
        public double ParameterAtVertex(int index) => Domain.ParameterAt(knots[index]);
        public override Point3d PointAt(double parameter)
        {
            double u = CurvePartition.Normalize(Domain, knots, parameter); int i = CurvePartition.Find(knots, u);
            return CurveMath.Lerp(points[i], points[i + 1], CurvePartition.Fraction(knots, i, u));
        }
        public override Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            double u = CurvePartition.Normalize(Domain, knots, parameter); int i = CurvePartition.Find(knots, u, side);
            return CurveMath.Unit(points[i + 1] - points[i], nameof(parameter));
        }
        public override double LengthAt(double parameter)
        {
            double u = CurvePartition.Normalize(Domain, knots, parameter); int i = CurvePartition.Find(knots, u);
            return lengths[i] + CurvePartition.Fraction(knots, i, u) * (lengths[i + 1] - lengths[i]);
        }
        public override double ParameterAtLength(double distance)
        {
            CurveMath.InRange(distance, 0, Length, nameof(distance)); int i = CurvePartition.Find(lengths, distance);
            double fraction = CurvePartition.Fraction(lengths, i, distance);
            return Domain.ParameterAt(knots[i] + fraction * (knots[i + 1] - knots[i]));
        }
        public override double ClosestParameter(Point3d point)
        {
            point = CurveMath.Copy(point, nameof(point)); double bestDistance = double.PositiveInfinity, best = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                double length = CurveMath.Distance(points[i], points[i + 1]);
                var unit = CurveMath.Unit(points[i + 1] - points[i], nameof(point));
                double fraction = CurveMath.ProjectFraction(point, points[i], unit, length);
                double distance = CurveMath.Distance(point, CurveMath.Lerp(points[i], points[i + 1], fraction));
                if (distance < bestDistance)
                {
                    bestDistance = distance; best = knots[i] + fraction * (knots[i + 1] - knots[i]);
                }
            }
            return Domain.ParameterAt(best);
        }
        public override Curve3d Trim(double start, double end)
        {
            ValidateTrim(start, end);
            double a = CurvePartition.Normalize(Domain, knots, start), b = CurvePartition.Normalize(Domain, knots, end);
            var vertices = new List<Point3d> { PointAt(start) }; var partition = new List<double> { 0 };
            for (int i = 1; i < points.Length - 1; i++)
                if (knots[i] > a && knots[i] < b) { vertices.Add(points[i]); partition.Add((knots[i] - a) / (b - a)); }
            vertices.Add(PointAt(end)); partition.Add(1);
            return CopyMetadata(new PolylineCurve3d(vertices.ToArray(), partition.ToArray(), new CurveInterval(start, end)));
        }
        public override Curve3d Reversed()
            => CopyMetadata(new PolylineCurve3d(points.Reverse().ToArray(), CurvePartition.Reverse(knots), Domain));
        public override Curve3d DuplicateCurve() => CopyMetadata(new PolylineCurve3d(points, knots, Domain));
        public override Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            CurveMath.Tessellation(tolerance, maxSegmentLength);
            var counts = new int[points.Length - 1]; int total = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = CurveMath.SegmentCount(CurveMath.Distance(points[i], points[i + 1]) / maxSegmentLength);
                total = CurveMath.SegmentCount((double)total + counts[i]);
            }
            var result = new Point3d[total + 1]; result[0] = new Point3d(points[0]); int index = 1;
            for (int i = 0; i < counts.Length; i++)
                for (int j = 1; j <= counts[i]; j++) result[index++] = CurveMath.Lerp(points[i], points[i + 1], (double)j / counts[i]);
            return result;
        }
        public override void Move(double x, double y, double z)
        {
            CurveMath.Finite(x, nameof(x)); CurveMath.Finite(y, nameof(y)); CurveMath.Finite(z, nameof(z));
            var moved = new PolylineCurve3d(points.Select(p => new Point3d(p.X + x, p.Y + y, p.Z + z)).ToArray(), knots, Domain);
            points = moved.points; lengths = moved.lengths;
        }
        protected override bool EqualsCurve(Curve3d other)
        {
            var polyline = (PolylineCurve3d)other;
            if (points.Length != polyline.points.Length) return false;
            for (int i = 0; i < points.Length; i++)
                if (!points[i].Equals(polyline.points[i]) || Math.Abs(knots[i] - polyline.knots[i]) > 1e-12) return false;
            return true;
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            var xyz = new double[points.Length * 3];
            for (int i = 0; i < points.Length; i++) { xyz[3 * i] = points[i].X; xyz[3 * i + 1] = points[i].Y; xyz[3 * i + 2] = points[i].Z; }
            info.AddValue("Coordinates", xyz); info.AddValue("Knots", knots);
        }
    }
}
