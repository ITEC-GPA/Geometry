using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>An oriented line segment with uniform parameter speed. Unlike Line3d, equality respects its direction.</summary>
    [Serializable]
    public sealed class LineCurve3d : Curve3d
    {
        private Point3d start, end;
        private double length;
        public override double Length => length;
        public override bool IsClosed => false;

        public LineCurve3d(Point3d start, Point3d end, CurveInterval? domain = null) : base(domain)
        {
            this.start = CurveMath.Copy(start, nameof(start)); this.end = CurveMath.Copy(end, nameof(end));
            length = CurveMath.Distance(start, end);
            CurveMath.Positive(length, nameof(end));
        }
        public LineCurve3d(Line3d line, CurveInterval? domain = null)
            : this((line ?? throw new ArgumentNullException(nameof(line))).Start, line.End, domain) { }
        private LineCurve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            start = CurveMath.Copy((Point3d)info.GetValue("Start", typeof(Point3d)), "Start");
            end = CurveMath.Copy((Point3d)info.GetValue("End", typeof(Point3d)), "End");
            length = CurveMath.Distance(start, end); CurveMath.Positive(length, "Length");
        }
        public Line3d ToLine3d() => new Line3d(new Point3d(start), new Point3d(end));
        public override Point3d PointAt(double parameter) => CurveMath.Lerp(start, end, Domain.Normalize(parameter));
        public override Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            Domain.Normalize(parameter); CurveMath.Side(side);
            return new Vector3d((end.X - start.X) / length, (end.Y - start.Y) / length, (end.Z - start.Z) / length);
        }
        public override double LengthAt(double parameter) => Domain.Normalize(parameter) * length;
        public override double ParameterAtLength(double distance)
        {
            CurveMath.InRange(distance, 0, length, nameof(distance));
            return Domain.ParameterAt(distance / length);
        }
        public override double ClosestParameter(Point3d point)
        {
            CurveMath.Copy(point, nameof(point));
            var tangent = TangentAt(Domain.Start);
            return Domain.ParameterAt(CurveMath.ProjectFraction(point, start, tangent, length));
        }
        public override Curve3d Trim(double from, double to)
        {
            ValidateTrim(from, to);
            return CopyMetadata(new LineCurve3d(PointAt(from), PointAt(to), new CurveInterval(from, to)));
        }
        public override Curve3d Reversed() => CopyMetadata(new LineCurve3d(end, start, Domain));
        public override Curve3d DuplicateCurve() => CopyMetadata(new LineCurve3d(start, end, Domain));
        public override Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            CurveMath.Tessellation(tolerance, maxSegmentLength);
            int count = CurveMath.SegmentCount(length / maxSegmentLength);
            var points = new Point3d[count + 1];
            for (int i = 0; i <= count; i++) points[i] = CurveMath.Lerp(start, end, (double)i / count);
            return points;
        }
        public override void Move(double x, double y, double z)
        {
            CurveMath.Finite(x, nameof(x)); CurveMath.Finite(y, nameof(y)); CurveMath.Finite(z, nameof(z));
            var moved = new LineCurve3d(new Point3d(start.X + x, start.Y + y, start.Z + z),
                new Point3d(end.X + x, end.Y + y, end.Z + z), Domain);
            start = moved.start; end = moved.end; length = moved.length;
        }
        protected override bool EqualsCurve(Curve3d other)
        {
            var line = (LineCurve3d)other;
            return start.Equals(line.start) && end.Equals(line.end);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Start", start); info.AddValue("End", end);
        }
    }
}
