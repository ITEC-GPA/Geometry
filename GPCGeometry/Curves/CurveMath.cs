using System;

namespace GPC.Geometry
{
    internal static class CurveMath
    {
        // Bound allocations explicitly instead of silently violating a requested tessellation tolerance.
        internal const int MaxSegments = 1000000;
        internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        internal static void Finite(double value, string name)
        {
            if (!IsFinite(value)) throw new ArgumentOutOfRangeException(name, "A finite value is required.");
        }
        internal static void Positive(double value, string name)
        {
            if (!IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(name, "A finite positive value is required.");
        }
        internal static void InRange(double value, double min, double max, string name)
        {
            if (!IsFinite(value) || value < min || value > max) throw new ArgumentOutOfRangeException(name);
        }
        internal static Point3d Copy(Point3d point, string name)
        {
            if (point is null) throw new ArgumentNullException(name);
            Finite(point.X, name); Finite(point.Y, name); Finite(point.Z, name);
            return new Point3d(point.X, point.Y, point.Z);
        }
        internal static double Norm(double x, double y, double z)
        {
            double scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
            if (scale == 0) return 0;
            if (!IsFinite(scale)) return double.PositiveInfinity;
            x /= scale; y /= scale; z /= scale;
            return scale * Math.Sqrt(x * x + y * y + z * z);
        }
        internal static double Distance(Point3d a, Point3d b) => Norm(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        internal static Vector3d Unit(Vector3d vector, string name)
        {
            if (vector is null) throw new ArgumentNullException(name);
            Finite(vector.X, name); Finite(vector.Y, name); Finite(vector.Z, name);
            double scale = Math.Max(Math.Abs(vector.X), Math.Max(Math.Abs(vector.Y), Math.Abs(vector.Z)));
            if (scale == 0) throw new ArgumentException("A nonzero direction is required.", name);
            double x = vector.X / scale, y = vector.Y / scale, z = vector.Z / scale;
            double length = Math.Sqrt(x * x + y * y + z * z);
            return new Vector3d(x / length, y / length, z / length);
        }
        internal static double Dot(Vector3d a, Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        internal static double ProjectFraction(Point3d point, Point3d start, Vector3d unit, double length)
        {
            double x = point.X - start.X, y = point.Y - start.Y, z = point.Z - start.Z;
            double scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
            if (scale == 0) return 0;
            if (IsFinite(scale)) { x /= scale; y /= scale; z /= scale; }
            else
            {
                // Subtraction itself can overflow for opposite, finite extreme coordinates.
                scale = Math.Max(Math.Max(Math.Abs(point.X), Math.Max(Math.Abs(point.Y), Math.Abs(point.Z))),
                    Math.Max(Math.Abs(start.X), Math.Max(Math.Abs(start.Y), Math.Abs(start.Z))));
                x = point.X / scale - start.X / scale;
                y = point.Y / scale - start.Y / scale;
                z = point.Z / scale - start.Z / scale;
            }
            double fraction = ((x * unit.X + y * unit.Y + z * unit.Z) * scale) / length;
            // Infinite projections are outside the finite segment and clamp to its appropriate endpoint.
            return Math.Max(0, Math.Min(1, fraction));
        }
        internal static Point3d Lerp(Point3d a, Point3d b, double u) => new Point3d(
            (1 - u) * a.X + u * b.X, (1 - u) * a.Y + u * b.Y, (1 - u) * a.Z + u * b.Z);
        internal static void Tessellation(double tolerance, double maxSegmentLength)
        {
            Positive(tolerance, nameof(tolerance));
            if (double.IsNaN(maxSegmentLength) || maxSegmentLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSegmentLength));
        }
        internal static int SegmentCount(double count)
        {
            if (!IsFinite(count) || count > MaxSegments)
                throw new ArgumentOutOfRangeException(nameof(count), "Tessellation exceeds one million segments.");
            return Math.Max(1, (int)Math.Ceiling(count));
        }
        internal static void Side(CurveEvaluationSide side)
        {
            if (side != CurveEvaluationSide.Automatic && side != CurveEvaluationSide.Below && side != CurveEvaluationSide.Above)
                throw new ArgumentOutOfRangeException(nameof(side));
        }
    }
}
