using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace GPC.Geometry
{
    /// <summary>
    /// A point in the space. Two points are equal if their distance is not bigger than the combined tolerance sqrt(2) * <see cref="GeometryBase.Tolerance"/>
    /// (see <see cref="Equals(Point3d, double)"/>; <see cref="Point2d"/> uses <see cref="GeometryBase.Tolerance"/>)
    /// </summary>
    [Serializable]
    public sealed class Point3d : GeometryBase, ISerializable, IEquatable<Point3d>, ICloneable
    {
        #region Variables

        /// <summary>Exact coordinate equality and hashing for dictionaries. Do not mutate their keys.</summary>
        public static IEqualityComparer<Point3d> ExactComparer { get; } = new ExactCoordinateComparer<Point3d>(p => (p.X, p.Y, p.Z));

        /// <summary>
        /// The X coordinate
        /// </summary>
        private double _x;
        /// <summary>
        /// The Y coordinate
        /// </summary>
        private double _y;
        /// <summary>
        /// The Z coordinate
        /// </summary>
        private double _z;

        // Weak observers do not keep a mesh or polygon alive when a caller retains a point.
        internal sealed class ChangeTracker { internal int Version; }
        [NonSerialized] private List<WeakReference<ChangeTracker>> _changeTrackers;

        internal void TrackChanges(ChangeTracker tracker)
        {
            if (_changeTrackers == null)
                _changeTrackers = new List<WeakReference<ChangeTracker>>();
            _changeTrackers.RemoveAll(reference => !reference.TryGetTarget(out _));
            _changeTrackers.Add(new WeakReference<ChangeTracker>(tracker));
        }

        internal void UntrackChanges(ChangeTracker tracker)
        {
            if (_changeTrackers == null) return;
            for (int i = _changeTrackers.Count - 1; i >= 0; i--)
                if (!_changeTrackers[i].TryGetTarget(out ChangeTracker target) || ReferenceEquals(target, tracker))
                {
                    _changeTrackers.RemoveAt(i);
                    if (ReferenceEquals(target, tracker)) return;
                }
        }

        private void CoordinatesChanged()
        {
            if (_changeTrackers == null) return;
            for (int i = _changeTrackers.Count - 1; i >= 0; i--)
                if (_changeTrackers[i].TryGetTarget(out ChangeTracker tracker))
                    unchecked { tracker.Version++; }
                else
                    _changeTrackers.RemoveAt(i);
        }

        #endregion

        #region Properties

        /// <summary>
        /// A new point at the origin (0, 0, 0)
        /// </summary>
        public static Point3d Origin => new Point3d(0, 0, 0);

        /// <summary>
        /// The X coordinate
        /// </summary>
        public double X { get => _x; set { _x = value; CoordinatesChanged(); } }

        /// <summary>
        /// The Y coordinate
        /// </summary>
        public double Y { get => _y; set { _y = value; CoordinatesChanged(); } }

        /// <summary>
        /// The Z coordinate
        /// </summary>
        public double Z { get => _z; set { _z = value; CoordinatesChanged(); } }

        /// <summary>
        /// The coordinates as a new array { X, Y, Z }
        /// </summary>
        public double[] Coordinates => new double[] { _x, _y, _z };

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a point at the origin
        /// </summary>
        public Point3d()
            : base()
        {
            _x = 0.0;
            _y = 0.0;
            _z = 0.0;
        }

        /// <summary>
        /// Creates a point
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="z">The Z coordinate</param>
        public Point3d(double x, double y, double z)
            : base()
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Creates a copy of a point (with a new <see cref="BaseObject.Guid"/>)
        /// </summary>
        /// <param name="p">The point to copy</param>
        public Point3d(Point3d p)
            : this(p._x, p._y, p._z)
        {
        }

        /// <summary>
        /// Creates the point of the XY plane (Z = 0)
        /// </summary>
        /// <param name="p">The point of the plane</param>
        public Point3d(Point2d p)
            : this(p.X, p.Y, 0)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the coordinates (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Point3d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
            _z = info.GetDouble("Z");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Move point to a new coordinates
        /// </summary>
        /// <param name="newX">New X coordinate</param>
        /// <param name="newY">New Y coordinate</param>
        /// <param name="newZ">New Z coordinate</param>
        public void MoveTo(double newX, double newY, double newZ)
        {
            _x = newX;
            _y = newY;
            _z = newZ;
            CoordinatesChanged();
        }

        /// <summary>
        /// Move point by an given increment dx, dy, dz
        /// </summary>
        /// <param name="dX">The X coordinate increment</param>
        /// <param name="dY">The Y coordinate increment</param>
        /// <param name="dz">The Z coordinate increment</param>
        public override void Move(double dX, double dY, double dz)
        {
            _x += dX;
            _y += dY;
            _z += dz;
            CoordinatesChanged();
        }

        /// <summary>
        /// Move point by a given vector
        /// </summary>
        /// <param name="movement">Displacement vector</param>
        public override void Move(Vector3d movement)
        {
            Move(movement.X, movement.Y, movement.Z);
        }

        /// <summary>
        /// A copy of the point scaled respect to the origin (the point is not changed)
        /// </summary>
        /// <param name="factor">Scale factor</param>
        /// <returns>The point (X * factor, Y * factor, Z * factor)</returns>
        public Point3d Scale(double factor)
        {
            return new Point3d(X * factor, Y * factor, Z * factor);
        }

        /// <summary>
        /// A copy of the point scaled respect to the origin with a factor for every axis (the point is not changed)
        /// </summary>
        /// <param name="factorX">Scale factor along X</param>
        /// <param name="factorY">Scale factor along Y</param>
        /// <param name="factorZ">Scale factor along Z</param>
        /// <returns>The scaled point</returns>
        public Point3d Scale(double factorX, double factorY, double factorZ)
        {
            return new Point3d(X * factorX, Y * factorY, Z * factorZ);
        }

        /// <summary>
        /// A copy of the point scaled respect to <paramref name="center"/> with a factor for every axis (the point is not changed)
        /// </summary>
        /// <param name="center">Center of scale</param>
        /// <param name="factorX">Scale factor along X</param>
        /// <param name="factorY">Scale factor along Y</param>
        /// <param name="factorZ">Scale factor along Z</param>
        /// <returns>The scaled point</returns>
        public Point3d Scale(Point3d center, double factorX, double factorY, double factorZ)
        {
            return new Point3d(center.X + (X - center.X) * factorX, center.Y + (Y - center.Y) * factorY, center.Z + (Z - center.Z) * factorZ);
        }

        /// <summary>
        /// A copy of the point scaled respect to <paramref name="center"/> (the point is not changed)
        /// </summary>
        /// <param name="center">Center of scale</param>
        /// <param name="factor">Scale factor</param>
        /// <returns>The scaled point</returns>
        public Point3d Scale(Point3d center, double factor)
        {
            return Scale(center, factor, factor, factor);
        }

        /// <summary>
        /// Get the mirror point about the plane defined by the equation ax + by + cz + d = 0
        /// </summary>
        /// <param name="a">The 'a' parameter of the equation</param>
        /// <param name="b">The 'b' parameter of the equation</param>
        /// <param name="c">The 'c' parameter of the equation</param>
        /// <param name="d">The 'd' parameter of the equation</param>
        /// <returns>A new point, symmetric of this one respect to the plane</returns>
        public Point3d Mirror(double a, double b, double c, double d)
        {
            double k = (-a * _x - b * _y - c * _z - d) / (a * a + b * b + c * c);
            double x2 = a * k + _x;
            double y2 = b * k + _y;
            double z2 = c * k + _z;
            double x3 = 2 * x2 - _x;
            double y3 = 2 * y2 - _y;
            double z3 = 2 * z2 - _z;
            return new Point3d(x3, y3, z3);
        }

        /// <summary>
        /// Calculate the distance with <paramref name="point"/>
        /// </summary>
        /// <param name="point">The input point</param>
        /// <returns>The distance</returns>
        public double DistanceTo(Point3d point)
        {
            return Math.Sqrt(SquareDistanceTo(point));
        }

        /// <summary>
        /// Calculate the square of the distance with <paramref name="point"/>
        /// </summary>
        /// <param name="point">The input point</param>
        /// <returns>The square of the distance (faster than <see cref="DistanceTo"/> for the comparisons)</returns>
        public double SquareDistanceTo(Point3d point)
        {
            double dx = point._x - _x;
            double dy = point._y - _y;
            double dz = point._z - _z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// The vector from this point to <paramref name="point"/>
        /// </summary>
        /// <param name="point">The end point</param>
        /// <returns>The vector <paramref name="point"/> - this</returns>
        public Vector3d VectorTo(Point3d point)
        {
            return new Vector3d(point - this);
        }

        /// <summary>
        /// Clone and move the cloned point with the vector <paramref name="movement"/>
        /// </summary>
        /// <param name="movement">The movement vector</param>
        /// <returns>The new point moved</returns>
        public Point3d CloneAndMove(Vector3d movement)
        {
            var p = (Point3d)Clone();
            p.Move(movement);
            return p;
        }

        /// <summary>
        /// Clone the point
        /// </summary>
        /// <returns>The new object cloned</returns>
        public override object Clone()
        {
            return new Point3d(this);
        }

        /// <summary>
        /// Converts the string representation of a point to its Point3d equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a point to convert.</param>
        /// <param name="result">The equivalent Point3d</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Point3d result)
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            string[] parts = s.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                result = null;
                return false;
            }

            double z = 0;
            if (!double.TryParse(parts[0].Trim(), out double x) || !double.TryParse(parts[1].Trim(), out double y) ||
                (parts.Length >= 3 && !double.TryParse(parts[2].Trim(), out z)))
            {
                result = null;
                return false;
            }

            result = new Point3d(x, y, z);
            return true;
        }

        /// <summary>
        /// Calculate whether this point lies on a semi-infinite ray
        /// </summary>
        /// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
        /// <param name="sinAlpha">Angular distance useful for finding the vertex with the smallest angle to the ray: the sine of the angle between the
        /// ray and the direction from its start to the point (for θ≈0 sinθ≈θ, and for angles close to 0 the sine is more accurate than the cosine);
        /// <see cref="double.MaxValue"/> if the point is behind the start, 0 if it is at the start or at the end point</param>
        /// <param name="tolerance">The tolerance on the distance from the ray</param>
        /// <returns>True if the point is at an end of <paramref name="SemiRay"/>, or on its side of the start (angle below 30°) and not farther than
        /// <paramref name="tolerance"/> from the line</returns>
        public bool IsOnSemiInfiniteRay(in Line3d SemiRay, out double sinAlpha, double tolerance = GeometryBase.Tolerance)
        {
            if (DistanceTo(SemiRay.Start) < tolerance || DistanceTo(SemiRay.End) < tolerance)
            {
                sinAlpha = 0.0;
                return true;
            }
            // Maximum angle considering distance tolerance, estimate to assess whether we are on the same direction
            // considering that the point could be very close to the initial point of the semi-infinite line.
            // Asin(tol / (2*tol)) = Asin(0.5)
            // The comparison is made on cosine of the angle, so:
            // Cos(Asin(0.5))=0.866
            double cosine_tolerance = 0.866;
            var v1 = new Vector3d(SemiRay.Start, this);
            v1.Unitize();
            var v2 = new Vector3d(SemiRay.Start, SemiRay.End);
            v2.Unitize();
            double cosAlpha = v1 * v2;

            sinAlpha = double.MaxValue;
            if (cosAlpha > 0.0)
                sinAlpha = (v1 ^ v2).Length;

            if (cosAlpha < cosine_tolerance)
                return false;
            // This point and the semi infinite line are on the same side, now check the distance.
            if (SemiRay.DistanceTo(this) < tolerance)
                return true;
            else
                return false;
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// The sum of the coordinates of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>A new point</returns>
        public static Point3d operator +(Point3d a, Point3d b)
        {
            return new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// The point translated by a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The translation</param>
        /// <returns>A new point</returns>
        public static Point3d operator +(Point3d point, Vector3d vector)
        {
            return new Point3d(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// The difference of the coordinates of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The point to subtract</param>
        /// <returns>A new point</returns>
        public static Point3d operator -(Point3d a, Point3d b)
        {
            return new Point3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// The point translated by the opposite of a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The vector to subtract</param>
        /// <returns>A new point</returns>
        public static Point3d operator -(Point3d point, Vector3d vector)
        {
            return new Point3d(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        /// <summary>
        /// The scalar product of the position vectors of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>a.X * b.X + a.Y * b.Y + a.Z * b.Z</returns>
        public static double operator *(Point3d a, Point3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// The coordinates multiplied by a number
        /// </summary>
        /// <param name="a">The point</param>
        /// <param name="b">The factor</param>
        /// <returns>A new point</returns>
        public static Point3d operator *(Point3d a, double b)
        {
            return new Point3d(a.X * b, a.Y * b, a.Z * b);
        }

        /// <summary>
        /// The coordinates multiplied by a number
        /// </summary>
        /// <param name="a">The factor</param>
        /// <param name="b">The point</param>
        /// <returns>A new point</returns>
        public static Point3d operator *(double a, Point3d b)
        {
            return b * a;
        }

        /// <summary>
        /// The scalar product of the position vector of a point and a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The vector</param>
        /// <returns>point.X * vector.X + point.Y * vector.Y + point.Z * vector.Z</returns>
        public static double operator *(Point3d point, Vector3d vector)
        {
            return point.X * vector.X + point.Y * vector.Y + point.Z * vector.Z;
        }

        /// <summary>
        /// The coordinates divided by a number
        /// </summary>
        /// <param name="a">The point</param>
        /// <param name="b">The divisor</param>
        /// <returns>A new point</returns>
        public static Point3d operator /(Point3d a, double b)
        {
            return a * (1 / b);
        }

        /// <summary>
        /// The cross product of the position vectors of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>A new point with the coordinates of a × b</returns>
        public static Point3d operator ^(Point3d a, Point3d b)
        {
            return new Point3d(a.Y * b.Z - a.Z * b.Y, -(a.X * b.Z - a.Z * b.X), a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// Equality within the tolerance: true if the points are the same object, both null or equal (<see cref="Equals(Point3d)"/>)
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>True if the points are equal</returns>
        public static bool operator ==(Point3d point1, Point3d point2)
        {
            if (ReferenceEquals(point1, point2))
                return true;

            if (point1 is null || point2 is null)
                return false;

            return point1.Equals(point2);
        }

        /// <summary>
        /// Inequality within the tolerance (see the equality operator)
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>True if the points are different</returns>
        public static bool operator !=(Point3d point1, Point3d point2)
        {
            return !(point1 == point2);
        }

        /// <summary>
        /// The position vector of the point
        /// </summary>
        /// <param name="point">The point</param>
        public static implicit operator Vector3d(Point3d point)
        {
            return new Vector3d(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// The projection of the point on the XY plane (Z is dropped)
        /// </summary>
        /// <param name="point">The point</param>
        public static implicit operator Point2d(Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Serializes the point: the <see cref="BaseObject.Guid"/> and the coordinates
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
            info.AddValue("Z", _z);
        }

		/// <summary>
		/// Check if two points are equal by means of the <paramref name="tolerance"/>
		/// </summary>
		/// <param name="other">The point to compare</param>
		/// <param name="tolerance">The tolerance of each point</param>
		/// <returns>True if the distance between the points is not bigger than the combined tolerance of the two points, sqrt(2) * <paramref name="tolerance"/></returns>
		public bool Equals(Point3d other, double tolerance = GeometryBase.Tolerance)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Square of the combined tolerance of two points: tol^2 + tol^2
            double squareTolerance = Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance);
            return SquareDistanceTo(other) <= squareTolerance;
        }

		/// <summary>
		/// Check if two points are equal by means of the <see cref="GeometryBase.Tolerance"/>
		/// </summary>
		/// <param name="other">The point to compare</param>
		/// <returns>True if the distance between the points is not bigger than sqrt(2) * <see cref="GeometryBase.Tolerance"/> (see
		/// <see cref="Equals(Point3d, double)"/>)</returns>
		public bool Equals(Point3d other)
        {
            return Equals(other, GeometryBase.Tolerance);
        }

        /// <summary>
        /// Equality within the tolerance with another object (see <see cref="Equals(Point3d)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal point</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Point3d);
        }

        /// <summary>
        /// A constant hash compatible with tolerance equality. Use ExactComparer for exact coordinate keys or a spatial index for proximity searches.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// The coordinates separated by the list separator of the current culture (the format read by <see cref="TryParse"/>)
        /// </summary>
        /// <returns>"X; Y; Z" (with the separator of the culture)</returns>
        public override string ToString()
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            return $"{_x}{separator} {_y}{separator} {_z}";
        }

        /// <summary>
        /// Equality within the tolerance with another geometry
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal point</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Point3d point)
                return Equals(point);

            return false;
        }

        #endregion
    }
}
