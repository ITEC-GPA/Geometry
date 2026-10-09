using System;
using System.Linq;

namespace GPC.Geometry
{
    /// <summary>Explicit, copying adapters between parametric curves and existing geometric primitives.</summary>
    public static class CurveConversions
    {
        public static LineCurve3d ToCurve3d(this Line3d line) => new LineCurve3d(line) { Tag = line.Tag };
        public static ArcCurve3d ToCurve3d(this Circle3d circle)
        {
            var result = ArcCurve3d.FromCircle(circle); result.Tag = circle.Tag; return result;
        }

        /// <summary>Converts the polygon's closed boundary, repeating the first point as the last vertex.</summary>
        public static PolylineCurve3d ToCurve3d(this Polygon3d polygon)
        {
            if (polygon is null) throw new ArgumentNullException(nameof(polygon));
            if (polygon.Count < 3) throw new ArgumentException("A polygon boundary requires at least three vertices.", nameof(polygon));
            return new PolylineCurve3d(polygon.Concat(new[] { polygon[0] })) { Tag = polygon.Tag };
        }
        public static PolylineCurve3d ToCurve3d(this Polygon2d polygon)
        {
            if (polygon is null) throw new ArgumentNullException(nameof(polygon));
            return new Polygon3d(polygon) { Tag = polygon.Tag }.ToCurve3d();
        }

        /// <summary>
        /// Converts a legacy arc, using its passage point when present. Without a passage point this uses the minor arc;
        /// a semicircle has no recoverable plane and requires the explicit ArcCurve3d constructor instead.
        /// </summary>
        public static ArcCurve3d ToCurve3d(this Circle3dArc arc, double tolerance = GeometryBase.Tolerance)
        {
            if (arc is null) throw new ArgumentNullException(nameof(arc));
            CurveMath.Positive(tolerance, nameof(tolerance));
            ArcCurve3d result;
            if (arc.Passage != null) result = ArcCurve3d.FromThreePoints(arc.Start, arc.Passage, arc.End, tolerance);
            else
            {
                var center = CurveMath.Copy(arc.Center, nameof(arc));
                var start = CurveMath.Copy(arc.Start, nameof(arc)); var end = CurveMath.Copy(arc.End, nameof(arc));
                double radius = CurveMath.Distance(start, center), endRadius = CurveMath.Distance(end, center);
                if (Math.Abs(radius - endRadius) > tolerance) throw new ArgumentException("Arc endpoints have inconsistent radii.", nameof(arc));
                var first = CurveMath.Unit(start - center, nameof(arc)); var last = CurveMath.Unit(end - center, nameof(arc));
                var normal = first.CrossProduct(last); double sine = CurveMath.Norm(normal.X, normal.Y, normal.Z);
                if (sine <= 1e-12) throw new ArgumentException("The legacy arc does not determine a unique plane. Supply an explicit normal.", nameof(arc));
                result = new ArcCurve3d(center, normal, first, radius, Math.Atan2(sine, CurveMath.Dot(first, last)));
            }
            result.Tag = arc.Tag; return result;
        }

        /// <summary>
        /// Approximates a closed planar curve as a legacy polygon. The last repeated endpoint is omitted.
        /// Open, nonplanar and degenerate boundaries are rejected; no projection or endpoint snapping is performed.
        /// </summary>
        public static Polygon3d ToPolygon3d(this Curve3d curve, double tolerance = GeometryBase.Tolerance,
            double maxSegmentLength = double.PositiveInfinity)
        {
            if (curve is null) throw new ArgumentNullException(nameof(curve));
            CurveMath.Tessellation(tolerance, maxSegmentLength);
            if (!curve.IsClosed || CurveMath.Distance(curve.StartPoint, curve.EndPoint) > tolerance)
                throw new ArgumentException("A closed curve is required.", nameof(curve));
            var points = curve.ToPolyline(tolerance, maxSegmentLength);
            int count = points.Length;
            if (Point3d.ExactComparer.Equals(points[0], points[count - 1])) count--;
            if (count < 3) throw new ArgumentException("The boundary has fewer than three vertices.", nameof(curve));
            var polygon = new Polygon3d(points.Take(count).ToArray(), tolerance) { Tag = curve.Tag };
            polygon.GetNormalVector(tolerance);
            return polygon;
        }
    }
}
