using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>An oriented ellipse or elliptical arc in space. The domain maps linearly to the ellipse angle,
    /// not to arc length. SemiAxisX and SemiAxisY are positive and need not be ordered. Lengths are numerical.</summary>
    [Serializable]
    public sealed class EllipseCurve3d : Curve3d
    {
        private const double FullTurn = 2 * Math.PI;
        private Point3d center;
        private Vector3d xAxis, yAxis;
        private double length;
        public Point3d Center => new Point3d(center);
        public Vector3d Normal => xAxis.CrossProduct(yAxis);
        public Vector3d AxisDirection => new Vector3d(xAxis);
        public double SemiAxisX { get; private set; }
        public double SemiAxisY { get; private set; }
        public double StartAngle { get; private set; }
        public double SweepAngle { get; private set; }
        public override double Length => length;
        public override bool IsClosed => Math.Abs(SweepAngle) == FullTurn;

        /// <summary>Angles are radians; a positive sweep follows the right-hand rule about normal.
        /// The default is a full ellipse starting on its X semi-axis. Axis direction must lie in the plane.</summary>
        public EllipseCurve3d(Point3d center, Vector3d normal, Vector3d axisDirection, double semiAxisX, double semiAxisY,
            double startAngle = 0, double sweepAngle = FullTurn, CurveInterval? domain = null) : base(domain)
        {
            Initialize(center, normal, axisDirection, semiAxisX, semiAxisY, startAngle, sweepAngle);
        }

        private EllipseCurve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Initialize((Point3d)info.GetValue("Center", typeof(Point3d)), (Vector3d)info.GetValue("Normal", typeof(Vector3d)),
                (Vector3d)info.GetValue("AxisDirection", typeof(Vector3d)), info.GetDouble("SemiAxisX"), info.GetDouble("SemiAxisY"),
                info.GetDouble("StartAngle"), info.GetDouble("Sweep"));
        }

        private void Initialize(Point3d origin, Vector3d normal, Vector3d direction, double a, double b, double start, double sweep)
        {
            center = CurveMath.Copy(origin, nameof(origin));
            CurveMath.Positive(a, nameof(a)); CurveMath.Positive(b, nameof(b));
            CurveMath.Finite(start, nameof(start)); CurveMath.Finite(sweep, nameof(sweep));
            if (sweep == 0 || Math.Abs(sweep) > FullTurn) throw new ArgumentOutOfRangeException(nameof(sweep));
            Vector3d n = CurveMath.Unit(normal, nameof(normal)), x = CurveMath.Unit(direction, nameof(direction));
            if (Math.Abs(CurveMath.Dot(n, x)) > 1e-12) throw new ArgumentException("The ellipse axis must lie in its plane.", nameof(direction));
            yAxis = CurveMath.Unit(n.CrossProduct(x), nameof(direction));
            xAxis = CurveMath.Unit(yAxis.CrossProduct(n), nameof(direction));
            SemiAxisX = a; SemiAxisY = b; StartAngle = PositiveAngle(start); SweepAngle = sweep;
            length = Integral(1) * Math.Max(a, b);
            CurveMath.Positive(length, nameof(sweep));
        }

        private static double PositiveAngle(double angle)
        {
            double value = angle % FullTurn;
            return value < 0 ? value + FullTurn : value;
        }
        private double AngleAt(double fraction) => IsClosed && fraction == 1 ? StartAngle : StartAngle + SweepAngle * fraction;
        private Point3d PointAtFraction(double fraction)
        {
            double angle = AngleAt(fraction);
            return center + xAxis * (SemiAxisX * Math.Cos(angle)) + yAxis * (SemiAxisY * Math.Sin(angle));
        }
        public override Point3d PointAt(double parameter) => PointAtFraction(Domain.Normalize(parameter));
        public override Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            double angle = AngleAt(Domain.Normalize(parameter)); CurveMath.Side(side);
            // Scale before forming the derivative to avoid over/underflow in its norm.
            double scale = Math.Max(SemiAxisX, SemiAxisY);
            return CurveMath.Unit(xAxis * (-SemiAxisX / scale * Math.Sin(angle)) + yAxis * (SemiAxisY / scale * Math.Cos(angle)),
                nameof(parameter)) * Math.Sign(SweepAngle);
        }

        private double Speed(double angle)
        {
            double scale = Math.Max(SemiAxisX, SemiAxisY);
            return CurveMath.Norm(SemiAxisX / scale * Math.Sin(angle), SemiAxisY / scale * Math.Cos(angle), 0);
        }
        // Adaptive Simpson integration on at most quarter-turn pieces, normalized by the larger semi-axis.
        private double Integral(double fraction)
        {
            double span = Math.Abs(SweepAngle) * fraction;
            if (SemiAxisX == SemiAxisY) return span;
            int pieces = Math.Max(1, (int)Math.Ceiling(span / (Math.PI / 2)));
            double sum = 0;
            for (int i = 0; i < pieces; i++)
            {
                double a = StartAngle + SweepAngle * fraction * i / pieces;
                double b = StartAngle + SweepAngle * fraction * (i + 1) / pieces;
                if (b < a) { double swap = a; a = b; b = swap; }
                double fa = Speed(a), fm = Speed((a + b) / 2), fb = Speed(b);
                double whole = (b - a) * (fa + 4 * fm + fb) / 6;
                sum += Integrate(a, b, fa, fm, fb, whole, 2e-13 * (b - a), 24);
            }
            return sum;
        }
        private double Integrate(double a, double b, double fa, double fm, double fb, double whole, double tolerance, int depth)
        {
            double m = (a + b) / 2, fl = Speed((a + m) / 2), fr = Speed((m + b) / 2);
            double left = (m - a) * (fa + 4 * fl + fm) / 6, right = (b - m) * (fm + 4 * fr + fb) / 6;
            double delta = left + right - whole;
            if (depth == 0 || Math.Abs(delta) <= 15 * tolerance) return left + right + delta / 15;
            return Integrate(a, m, fa, fl, fm, left, tolerance / 2, depth - 1)
                + Integrate(m, b, fm, fr, fb, right, tolerance / 2, depth - 1);
        }
        public override double LengthAt(double parameter)
        {
            double fraction = Domain.Normalize(parameter);
            if (fraction == 0) return 0;
            return fraction == 1 ? Length : Integral(fraction) * Math.Max(SemiAxisX, SemiAxisY);
        }
        public override double ParameterAtLength(double distance)
        {
            CurveMath.InRange(distance, 0, Length, nameof(distance));
            if (distance == 0) return Domain.Start;
            if (distance == Length) return Domain.End;
            double target = distance / Math.Max(SemiAxisX, SemiAxisY), lo = 0, hi = 1, u = distance / Length;
            for (int i = 0; i < 60; i++)
            {
                double difference = Integral(u) - target;
                if (difference > 0) hi = u; else lo = u;
                if (hi - lo <= 2e-14 || difference == 0) break;
                double next = u - difference / (Speed(AngleAt(u)) * Math.Abs(SweepAngle));
                u = next > lo && next < hi ? next : (lo + hi) / 2;
            }
            return Domain.ParameterAt(u);
        }

        public override double ClosestParameter(Point3d point)
        {
            point = CurveMath.Copy(point, nameof(point));
            Vector3d delta = point - center;
            double axisScale = Math.Max(SemiAxisX, SemiAxisY);
            double queryScale = Math.Max(Math.Abs(delta.X), Math.Max(Math.Abs(delta.Y), Math.Abs(delta.Z)));
            CurveMath.Finite(queryScale, nameof(point));
            var local = queryScale == 0 ? new Vector3d(0, 0, 0) : new Vector3d(delta.X / queryScale, delta.Y / queryScale, delta.Z / queryScale);
            double xQuery = CurveMath.Dot(local, xAxis), yQuery = CurveMath.Dot(local, yAxis);
            double projectionScale = Math.Max(Math.Abs(xQuery), Math.Abs(yQuery));
            double quadraticWeight = 1, linearWeight = 0;
            if (projectionScale > 0)
            {
                double inverseRatio = (axisScale / queryScale) / projectionScale;
                if (inverseRatio < 1) { quadraticWeight = inverseRatio; linearWeight = 1; }
                else linearWeight = 1 / inverseRatio;
                xQuery /= projectionScale; yQuery /= projectionScale;
            }
            double a = SemiAxisX / axisScale, b = SemiAxisY / axisScale;
            double ax = a * xQuery * linearWeight, by = b * yQuery * linearWeight, aa = a * a * quadraticWeight, bb = b * b * quadraticWeight;
            double coefficientScale = Math.Max(Math.Max(aa, bb), Math.Max(Math.Abs(ax), Math.Abs(by)));
            if (coefficientScale == 0) return Domain.Start;
            aa /= coefficientScale; bb /= coefficientScale; ax /= coefficientScale; by /= coefficientScale;
            double Score(double u)
            {
                double angle = AngleAt(u), c = Math.Cos(angle), s = Math.Sin(angle);
                // Subtract the common squared query distance before comparison, retaining precision for distant points.
                return aa * c * c + bb * s * s - 2 * ax * c - 2 * by * s;
            }
            double best = 0, score = Score(0);
            void Consider(double u)
            {
                double candidate = Score(u), epsilon = 2e-14 * Math.Max(1, Math.Max(Math.Abs(candidate), Math.Abs(score)));
                if (candidate < score - epsilon || Math.Abs(candidate - score) <= epsilon && u < best)
                { best = u; score = candidate; }
            }
            Consider(1);
            // tan(angle/2) gives a quartic. Two half-turn charts bound every root in [-1,1], avoiding infinite roots.
            for (int chart = 0; chart < 2; chart++)
            {
                double x = chart == 0 ? ax : -ax, y = chart == 0 ? by : -by, d = bb - aa;
                foreach (double root in Roots(new[] { -y, 2 * (d + x), 0, 2 * (x - d), y }))
                {
                    double angle = 2 * Math.Atan(root) + chart * Math.PI;
                    double along = PositiveAngle(Math.Sign(SweepAngle) * (angle - StartAngle));
                    if (along <= Math.Abs(SweepAngle)) Consider(along / Math.Abs(SweepAngle));
                }
            }
            return Domain.ParameterAt(best);
        }
        // Isolate real roots on a bounded interval using derivative roots to partition monotone pieces.
        private static List<double> Roots(double[] coefficients)
        {
            int degree = coefficients.Length - 1;
            while (degree > 0 && coefficients[degree] == 0) degree--;
            var roots = new List<double>();
            if (degree == 0) return roots;
            double maximum = coefficients.Take(degree + 1).Max(value => Math.Abs(value));
            var c = coefficients.Take(degree + 1).Select(value => value / maximum).ToArray();
            double Value(double t) { double v = c[degree]; for (int i = degree - 1; i >= 0; i--) v = v * t + c[i]; return v; }
            var boundaries = new List<double> { -1 };
            if (degree > 1) boundaries.AddRange(Roots(Enumerable.Range(1, degree).Select(i => c[i] * i).ToArray()));
            boundaries.Add(1); boundaries.Sort();
            foreach (double t in boundaries) if (Math.Abs(Value(t)) <= 2e-14) roots.Add(t);
            for (int i = 0; i < boundaries.Count - 1; i++)
            {
                double lo = boundaries[i], hi = boundaries[i + 1], flo = Value(lo), fhi = Value(hi);
                if (flo == 0 || fhi == 0 || Math.Sign(flo) == Math.Sign(fhi)) continue;
                for (int step = 0; step < 60; step++)
                {
                    double mid = (lo + hi) / 2, f = Value(mid);
                    if (f == 0) { lo = hi = mid; break; }
                    if (Math.Sign(f) == Math.Sign(flo)) { lo = mid; flo = f; } else hi = mid;
                }
                roots.Add((lo + hi) / 2);
            }
            return roots;
        }

        public override Curve3d Trim(double start, double end)
        {
            ValidateTrim(start, end);
            double a = Domain.Normalize(start), b = Domain.Normalize(end);
            return CopyMetadata(new EllipseCurve3d(center, Normal, xAxis, SemiAxisX, SemiAxisY,
                StartAngle + SweepAngle * a, SweepAngle * (b - a), new CurveInterval(start, end)));
        }
        public override Curve3d Reversed() => CopyMetadata(new EllipseCurve3d(center, Normal, xAxis, SemiAxisX, SemiAxisY,
            IsClosed ? StartAngle : StartAngle + SweepAngle, -SweepAngle, Domain));
        public override Curve3d DuplicateCurve() => CopyMetadata(new EllipseCurve3d(center, Normal, xAxis, SemiAxisX, SemiAxisY, StartAngle, SweepAngle, Domain));
        public override Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            CurveMath.Tessellation(tolerance, maxSegmentLength);
            double radius = Math.Max(SemiAxisX, SemiAxisY);
            // An ellipse is a scaled unit circle: its maximum scale bounds both sagitta and chord length.
            double maxAngle = Math.Min(Math.PI / 2, 4 * Math.Asin(Math.Sqrt(Math.Min(2, tolerance / radius) / 2)));
            int count = CurveMath.SegmentCount(Math.Max(Math.Abs(SweepAngle) / maxAngle, radius / maxSegmentLength * Math.Abs(SweepAngle)));
            var points = new Point3d[count + 1];
            for (int i = 0; i <= count; i++) points[i] = PointAtFraction((double)i / count);
            return points;
        }
        public override void Move(double x, double y, double z)
        {
            CurveMath.Finite(x, nameof(x)); CurveMath.Finite(y, nameof(y)); CurveMath.Finite(z, nameof(z));
            center = CurveMath.Copy(new Point3d(center.X + x, center.Y + y, center.Z + z), "translation");
        }
        protected override bool EqualsCurve(Curve3d other)
        {
            var ellipse = (EllipseCurve3d)other;
            return center.Equals(ellipse.center) && xAxis.Equals(ellipse.xAxis) && yAxis.Equals(ellipse.yAxis)
                && Math.Abs(SemiAxisX - ellipse.SemiAxisX) <= Tolerance && Math.Abs(SemiAxisY - ellipse.SemiAxisY) <= Tolerance
                && Math.Abs(StartAngle - ellipse.StartAngle) <= AngularTolerance && Math.Abs(SweepAngle - ellipse.SweepAngle) <= AngularTolerance;
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("Center", center); info.AddValue("Normal", Normal);
            info.AddValue("AxisDirection", xAxis); info.AddValue("SemiAxisX", SemiAxisX); info.AddValue("SemiAxisY", SemiAxisY);
            info.AddValue("StartAngle", StartAngle); info.AddValue("Sweep", SweepAngle);
        }
    }
}
