using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>Side used at a corner. Automatic selects the outgoing segment, except at the curve end.</summary>
    public enum CurveEvaluationSide { Automatic, Below, Above }

    /// <summary>
    /// An oriented, finite, nondegenerate parametric curve. Parameters belong to Domain and are not distances.
    /// Inputs and returned geometry are independent copies. Move and SetDomain mutate; all other geometry operations return copies.
    /// Derivatives, length inversion and tessellation are supplied by each representation, allowing future nonlinear curves.
    /// </summary>
    [Serializable]
    public abstract class Curve3d : GeometryBase
    {
        public CurveInterval Domain { get; private set; }
        public Point3d StartPoint => PointAt(Domain.Start);
        public Point3d EndPoint => PointAt(Domain.End);
        public abstract double Length { get; }
        public virtual bool IsClosed => CurveMath.Distance(StartPoint, EndPoint) <= Tolerance;

        protected Curve3d(CurveInterval? domain = null) { SetDomain(domain ?? new CurveInterval(0, 1)); }
        protected Curve3d(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SetDomain(new CurveInterval(info.GetDouble("DomainStart"), info.GetDouble("DomainEnd")));
        }

        /// <summary>Changes parameter labels without changing geometry, orientation or length.</summary>
        public void SetDomain(CurveInterval domain)
        {
            if (!domain.IsValid) throw new ArgumentException("Invalid curve domain.", nameof(domain));
            Domain = domain;
        }

        public abstract Point3d PointAt(double parameter);
        public abstract Vector3d TangentAt(double parameter, CurveEvaluationSide side = CurveEvaluationSide.Automatic);
        public double GetLength() => Length;
        /// <summary>Arc length measured from StartPoint to the parameter.</summary>
        public abstract double LengthAt(double parameter);
        /// <summary>Parameter at a physical distance from StartPoint, in [0, Length].</summary>
        public abstract double ParameterAtLength(double distance);
        public Point3d PointAtLength(double distance) => PointAt(ParameterAtLength(distance));
        /// <summary>Closest parameter on this finite curve. Ties select the earliest parameter.</summary>
        public abstract double ClosestParameter(Point3d point);
        public Point3d ClosestPoint(Point3d point) => PointAt(ClosestParameter(point));
        /// <summary>Independent subcurve on [start, end], retaining the original parameterization on that interval.</summary>
        public abstract Curve3d Trim(double start, double end);
        public Curve3d[] Split(double parameter)
        {
            Domain.Normalize(parameter);
            if (parameter == Domain.Start || parameter == Domain.End)
                throw new ArgumentOutOfRangeException(nameof(parameter), "Split requires an interior parameter.");
            return new[] { Trim(Domain.Start, parameter), Trim(parameter, Domain.End) };
        }
        /// <summary>Independent curve with reversed orientation and the same Domain.</summary>
        public abstract Curve3d Reversed();
        /// <summary>Deep geometric copy with a new Guid and the same Tag reference. Tags are not serialized.</summary>
        public abstract Curve3d DuplicateCurve();
        public sealed override object Clone() => DuplicateCurve();
        /// <summary>
        /// Ordered polyline vertices, including both endpoints and every corner. Tolerance bounds chord deviation;
        /// maxSegmentLength optionally limits chord length. Throws if more than one million segments are needed.
        /// </summary>
        public abstract Point3d[] ToPolyline(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity);
        public Line3d[] ToLineSegments(double tolerance = Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            Point3d[] points = ToPolyline(tolerance, maxSegmentLength);
            var lines = new Line3d[points.Length - 1];
            for (int i = 0; i < lines.Length; i++) lines[i] = new Line3d(new Point3d(points[i]), new Point3d(points[i + 1]));
            return lines;
        }
        public sealed override void Move(Vector3d vector)
        {
            if (vector is null) throw new ArgumentNullException(nameof(vector));
            Move(vector.X, vector.Y, vector.Z);
        }
        protected void ValidateTrim(double start, double end)
        {
            Domain.Normalize(start); Domain.Normalize(end);
            if (start >= end) throw new ArgumentException("Trim requires start < end.");
        }
        protected T CopyMetadata<T>(T copy) where T : Curve3d { copy.Tag = Tag; return copy; }
        protected abstract bool EqualsCurve(Curve3d other);
        /// <summary>Equality includes concrete representation, orientation and domain; it is not locus equality.</summary>
        public sealed override bool Equals(GeometryBase other) => other is Curve3d curve
            && GetType() == curve.GetType() && Domain.Equals(curve.Domain) && EqualsCurve(curve);
        public sealed override bool Equals(object obj) => obj is GeometryBase geometry && Equals(geometry);
        public sealed override int GetHashCode() => base.GetHashCode();
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DomainStart", Domain.Start); info.AddValue("DomainEnd", Domain.End);
        }
    }
}
