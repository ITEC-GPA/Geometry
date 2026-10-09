using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>A circular arc with a signed sweep in [-2 PI, 2 PI], excluding zero. A full turn represents a circle.</summary>
    [Serializable]
    public sealed class ArcCurve3d : Curve3d
    {
        private const double FullTurn = 2 * Math.PI;
        private Point3d center;
        private Vector3d xAxis, yAxis;
        public Point3d Center => new Point3d(center);
        public Vector3d Normal => xAxis.CrossProduct(yAxis);
        public Vector3d StartDirection => new Vector3d(xAxis);
        public double Radius { get; private set; }
        public double SweepAngle { get; private set; }
        public override double Length => Radius * Math.Abs(SweepAngle);
        public override bool IsClosed => Math.Abs(SweepAngle) == FullTurn;

        /// <summary>
        /// Creates an arc about center. Directions are normalized and must be perpendicular.
        /// Positive sweep follows the right-hand rule about normal; negative sweep reverses it. Inputs are copied.
        /// </summary>
        public ArcCurve3d(Point3d center, Vector3d normal, Vector3d startDirection, double radius,
            double sweepAngle, CurveInterval? domain = null) : base(domain)
        {
            Initialize(center, normal, startDirection, radius, sweepAngle);
        }
        private ArcCurve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Initialize((Point3d)info.GetValue("Center", typeof(Point3d)), (Vector3d)info.GetValue("Normal", typeof(Vector3d)),
                (Vector3d)info.GetValue("StartDirection", typeof(Vector3d)), info.GetDouble("Radius"), info.GetDouble("Sweep"));
        }
        private void Initialize(Point3d origin, Vector3d normal, Vector3d direction, double radius, double sweep)
        {
            center = CurveMath.Copy(origin, nameof(origin));
            CurveMath.Positive(radius, nameof(radius)); CurveMath.Finite(sweep, nameof(sweep));
            if (sweep == 0 || Math.Abs(sweep) > FullTurn) throw new ArgumentOutOfRangeException(nameof(sweep));
            var n = CurveMath.Unit(normal, nameof(normal)); var x = CurveMath.Unit(direction, nameof(direction));
            if (Math.Abs(CurveMath.Dot(n, x)) > 1e-12)
                throw new ArgumentException("The start direction must lie in the arc plane.", nameof(direction));
            yAxis = CurveMath.Unit(n.CrossProduct(x), nameof(direction));
            xAxis = CurveMath.Unit(yAxis.CrossProduct(n), nameof(direction));
            Radius = radius; SweepAngle = sweep; CurveMath.Positive(Length, nameof(radius));
        }

        /// <summary>Creates a full circle. A missing start direction selects a deterministic direction in its plane.</summary>
        public static ArcCurve3d FromCircle(Circle3d circle, Vector3d startDirection = null, bool clockwise = false,
            CurveInterval? domain = null)
        {
            if (circle is null) throw new ArgumentNullException(nameof(circle));
            if (circle.Plane is null) throw new ArgumentException("A circle plane is required.", nameof(circle));
            var n = CurveMath.Unit(circle.Plane.Normal, nameof(circle));
            if (startDirection is null)
            {
                var axis = Math.Abs(n.X) < 0.9 ? new Vector3d(1, 0, 0) : new Vector3d(0, 1, 0);
                startDirection = axis - n * CurveMath.Dot(axis, n);
            }
            return new ArcCurve3d(circle.Center, n, startDirection, circle.Radius, clockwise ? -FullTurn : FullTurn, domain);
        }

        /// <summary>Creates the oriented arc from start through passage to end, including arcs greater than PI.</summary>
        public static ArcCurve3d FromThreePoints(Point3d start, Point3d passage, Point3d end,
            double tolerance = Tolerance, CurveInterval? domain = null)
        {
            start = CurveMath.Copy(start, nameof(start)); passage = CurveMath.Copy(passage, nameof(passage));
            end = CurveMath.Copy(end, nameof(end)); CurveMath.Positive(tolerance, nameof(tolerance));
            double a = CurveMath.Distance(start, passage), b = CurveMath.Distance(start, end);
            if (a <= tolerance || b <= tolerance || CurveMath.Distance(passage, end) <= tolerance)
                throw new ArgumentException("Three distinct points are required.");
            CurveMath.Positive(a, nameof(passage)); CurveMath.Positive(b, nameof(end));
            var ex = CurveMath.Unit(passage - start, nameof(passage));
            var endDirection = CurveMath.Unit(end - start, nameof(end));
            var cross = ex.CrossProduct(endDirection);
            double sine = CurveMath.Norm(cross.X, cross.Y, cross.Z);
            if (sine * Math.Min(a, b) <= tolerance) throw new ArgumentException("The arc points are collinear within tolerance.");
            var n = CurveMath.Unit(cross, nameof(end)); var ey = n.CrossProduct(ex);
            double scale = Math.Max(a, b), aa = a / scale, bb = b / scale;
            double x = bb * CurveMath.Dot(endDirection, ex), y = bb * CurveMath.Dot(endDirection, ey);
            double cy = ((bb * bb - aa * x) / (2 * y)) * scale;
            Point3d origin = start + ex * (a / 2) + ey * cy;
            double radius = CurveMath.Distance(origin, start);
            var first = CurveMath.Unit(start - origin, nameof(start)); var second = n.CrossProduct(first);
            double lastAngle = PositiveAngle(Math.Atan2(CurveMath.Dot(end - origin, second), CurveMath.Dot(end - origin, first)));
            double midAngle = PositiveAngle(Math.Atan2(CurveMath.Dot(passage - origin, second), CurveMath.Dot(passage - origin, first)));
            double sweep = midAngle <= lastAngle ? lastAngle : lastAngle - FullTurn;
            return new ArcCurve3d(origin, n, first, radius, sweep, domain);
        }

        private static double PositiveAngle(double angle) => angle < 0 ? angle + FullTurn : angle;
        private Vector3d DirectionAt(double fraction)
        {
            if (fraction == 1 && IsClosed) return new Vector3d(xAxis);
            double angle = SweepAngle * fraction;
            return xAxis * Math.Cos(angle) + yAxis * Math.Sin(angle);
        }
        public override Point3d PointAt(double parameter) => center + DirectionAt(Domain.Normalize(parameter)) * Radius;
        public override Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            double u = Domain.Normalize(parameter); CurveMath.Side(side);
            return Normal.CrossProduct(DirectionAt(u)) * Math.Sign(SweepAngle);
        }
        public override double LengthAt(double parameter) => Domain.Normalize(parameter) * Length;
        public override double ParameterAtLength(double distance)
        {
            CurveMath.InRange(distance, 0, Length, nameof(distance));
            return Domain.ParameterAt(distance / Length);
        }
        public override double ClosestParameter(Point3d point)
        {
            point = CurveMath.Copy(point, nameof(point));
            Vector3d delta = point - center;
            double scale = Math.Max(Math.Abs(delta.X), Math.Max(Math.Abs(delta.Y), Math.Abs(delta.Z)));
            CurveMath.Finite(scale, nameof(point));
            if (scale == 0) return Domain.Start;
            delta = new Vector3d(delta.X / scale, delta.Y / scale, delta.Z / scale);
            double x = CurveMath.Dot(delta, xAxis), y = CurveMath.Dot(delta, yAxis);
            if (x == 0 && y == 0) return Domain.Start;
            double angle = PositiveAngle(Math.Atan2(y, x) * Math.Sign(SweepAngle));
            if (angle <= Math.Abs(SweepAngle)) return Domain.ParameterAt(angle / Math.Abs(SweepAngle));
            return CurveMath.Distance(point, StartPoint) <= CurveMath.Distance(point, EndPoint) ? Domain.Start : Domain.End;
        }
        public override Curve3d Trim(double start, double end)
        {
            ValidateTrim(start, end);
            double a = Domain.Normalize(start), b = Domain.Normalize(end);
            return CopyMetadata(new ArcCurve3d(center, Normal, DirectionAt(a), Radius, SweepAngle * (b - a), new CurveInterval(start, end)));
        }
        public override Curve3d Reversed() => CopyMetadata(new ArcCurve3d(center, Normal, DirectionAt(1), Radius, -SweepAngle, Domain));
        public override Curve3d DuplicateCurve() => CopyMetadata(new ArcCurve3d(center, Normal, xAxis, Radius, SweepAngle, Domain));
        public override Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            CurveMath.Tessellation(tolerance, maxSegmentLength);
            // Sagitta = 2 R sin(angle/4)^2. This inverse remains accurate for small tolerance/radius.
            double maxAngle = Math.Min(Math.PI / 2, 4 * Math.Asin(Math.Sqrt(Math.Min(2, tolerance / Radius) / 2)));
            int count = CurveMath.SegmentCount(Math.Max(Math.Abs(SweepAngle) / maxAngle, Length / maxSegmentLength));
            var points = new Point3d[count + 1];
            for (int i = 0; i <= count; i++) points[i] = center + DirectionAt((double)i / count) * Radius;
            return points;
        }
        public override void Move(double x, double y, double z)
        {
            CurveMath.Finite(x, nameof(x)); CurveMath.Finite(y, nameof(y)); CurveMath.Finite(z, nameof(z));
            center = CurveMath.Copy(new Point3d(center.X + x, center.Y + y, center.Z + z), "translation");
        }
        protected override bool EqualsCurve(Curve3d other)
        {
            var arc = (ArcCurve3d)other;
            return center.Equals(arc.center) && xAxis.Equals(arc.xAxis) && yAxis.Equals(arc.yAxis)
                && Math.Abs(Radius - arc.Radius) <= Tolerance && Math.Abs(SweepAngle - arc.SweepAngle) <= AngularTolerance;
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("Center", center); info.AddValue("Normal", Normal);
            info.AddValue("StartDirection", xAxis); info.AddValue("Radius", Radius); info.AddValue("Sweep", SweepAngle);
        }
    }
}
