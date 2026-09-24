using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public sealed class Point3d : GeometryBase, ISerializable, IEquatable<Point3d>, ICloneable
    {
        #region Variables

        private double _x;
        private double _y;
        private double _z;

        #endregion

        #region Properties

        public static Point3d Origin => new Point3d(0, 0, 0);

        public double X { get => _x; set { _x = value; } }

        public double Y { get => _y; set { _y = value; } }

        public double Z { get => _z; set { _z = value; } }

        public double[] Coordinates => new double[] { _x, _y, _z };

        #endregion

        #region Public Constructors

        public Point3d()
            : base()
        {
            _x = 0.0;
            _y = 0.0;
            _z = 0.0;
        }

        public Point3d(double x, double y, double z)
            : base()
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Point3d(Point3d p)
            : this(p._x, p._y, p._z)
        {
        }

        public Point3d(Point2d p)
            : this(p.X, p.Y, 0)
        {
        }

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
        /// Scale the point respect to the origin 
        /// </summary>
        /// <param name="factor">Scale factor</param>
        public Point3d Scale(double factor)
        {
            return new Point3d(X * factor, Y * factor, Z * factor);
        }

        /// <summary>
        /// Scale the point respect to the origin
        /// </summary>
        /// <param name="factorX">Scale factor</param>
        /// <param name="factorY">Scale factor</param>
        /// <param name="factorZ">Scale factor</param>
        public Point3d Scale(double factorX, double factorY, double factorZ)
        {
            return new Point3d(X * factorX, Y * factorY, Z * factorZ);
        }

        /// <summary>
        /// Scale the point respect to <paramref name="center"/>
        /// </summary>
        /// <param name="center">Center of scale</param>
        /// <param name="factorX">Scale factor</param>
        /// <param name="factorY">Scale factor</param>
        /// <param name="factorZ">Scale factor</param>
        public Point3d Scale(Point3d center, double factorX, double factorY, double factorZ)
        {
            return new Point3d(center.X + (X - center.X) * factorX, center.Y + (Y - center.Y) * factorY, center.Z + (Z - center.Z) * factorZ);
        }

        /// <summary>
        /// Scale the point respect to <paramref name="center"/>
        /// </summary>
        /// <param name="center">Center of scale</param>
        /// <param name="factor">Scale factor</param>
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
        /// <returns></returns>
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
        /// Calculate the square distance with <paramref name="point"/>
        /// </summary>
        /// <param name="point">The input point</param>
        /// <returns>The distance</returns>
        public double SquareDistanceTo(Point3d point)
        {
            double dx = point._x - _x;
            double dy = point._y - _y;
            double dz = point._z - _z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Calculate the vector with <paramref name="point"/>
        /// </summary>
        /// <param name="point">The input point</param>
        /// <returns>The vector</returns>
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
        /// Calculate whether this point lies on an semi-infinite ray.
        /// </summary>
        /// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
        /// <param name="sinAlpha">Angular distance useful for finding the vertex with the smallest angle to the ray.
        /// The goal is to look for an angle as close to 0 as possible, so the sine of the angle is returned, for θ≈0 we have that sinθ≈θ.
        /// Also, for angles close to 0, sine is more accurate than cosine.</param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
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

        public static Point3d operator +(Point3d a, Point3d b)
        {
            return new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3d operator +(Point3d point, Vector3d vector)
        {
            return new Point3d(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        public static Point3d operator -(Point3d a, Point3d b)
        {
            return new Point3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3d operator -(Point3d point, Vector3d vector)
        {
            return new Point3d(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        public static double operator *(Point3d a, Point3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Point3d operator *(Point3d a, double b)
        {
            return new Point3d(a.X * b, a.Y * b, a.Z * b);
        }

        public static Point3d operator *(double a, Point3d b)
        {
            return b * a;
        }

        public static double operator *(Point3d point, Vector3d vector)
        {
            return point.X * vector.X + point.Y * vector.Y + point.Z * vector.Z;
        }

        public static Point3d operator /(Point3d a, double b)
        {
            return a * (1 / b);
        }

        public static Point3d operator ^(Point3d a, Point3d b)
        {
            return new Point3d(a.Y * b.Z - a.Z * b.Y, -(a.X * b.Z - a.Z * b.X), a.X * b.Y - a.Y * b.X);
        }

        public static bool operator ==(Point3d point1, Point3d point2)
        {
            if (ReferenceEquals(point1, point2))
                return true;

            if (point1 is null || point2 is null)
                return false;

            return point1.Equals(point2);
        }

        public static bool operator !=(Point3d point1, Point3d point2)
        {
            return !(point1 == point2);
        }

        public static implicit operator Vector3d(Point3d point)
        {
            return new Vector3d(point.X, point.Y, point.Z);
        }

        public static implicit operator Point2d(Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        #endregion

        #region Public Methods Override

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
            info.AddValue("Z", _z);
        }

		/// <summary>
		/// Check if two points are equals by means of the <paramref name="tolerance"/>
		/// </summary>
		/// <returns>True if the distance between point and other is less than the combined tolerance of the two points, i.e. sqrt(2) * <paramref name="tolerance"/></returns>
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
		/// Check if two points are equals by means of the <see cref="GeometryBase.Tolerance"/>
		/// </summary>
		/// <returns>True if the distance between point and other is less than <see cref="GeometryBase.Tolerance"/></returns>
		public bool Equals(Point3d other)
        {
            return Equals(other, GeometryBase.Tolerance);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Point3d);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _x.GetHashCode();
                hashCode = hashCode * -23 + _y.GetHashCode();
                hashCode = hashCode * -23 + _z.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            return $"{_x}{separator} {_y}{separator} {_z}";
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Point3d point)
                return Equals(point);

            return false;
        }

        #endregion
    }
}
