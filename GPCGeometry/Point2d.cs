using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Geometry
{
    /// <summary>
    /// A point in the XY plane. Two points are equal if their distance is not bigger than <see cref="GeometryBase.Tolerance"/>
    /// </summary>
    [Serializable]
    public sealed class Point2d : GeometryBase, ISerializable, IEquatable<Point2d>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The X coordinate
        /// </summary>
        private double _x;
        /// <summary>
        /// The Y coordinate
        /// </summary>
        private double _y;

        #endregion

        #region Properties

        /// <summary>
        /// A new point at the origin (0, 0)
        /// </summary>
        public static Point2d Origin => new Point2d(0, 0);

        /// <summary>
        /// The X coordinate
        /// </summary>
        public double X { get => _x; set { _x = value; } }

        /// <summary>
        /// The Y coordinate
        /// </summary>
        public double Y { get => _y; set { _y = value; } }

        /// <summary>
        /// The coordinates as a new array { X, Y }
        /// </summary>
        public double[] Coordinates => new double[] { _x, _y };

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a point at the origin
        /// </summary>
        public Point2d()
            : base()
        {
            _x = 0.0;
            _y = 0.0;
        }

        /// <summary>
        /// Creates a point
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public Point2d(double x, double y)
            : base()
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Creates a copy of a point (with a new <see cref="BaseObject.Guid"/>)
        /// </summary>
        /// <param name="p">The point to copy</param>
        public Point2d(Point2d p)
            : this(p._x, p._y)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the coordinates (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Point2d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Move point to a new coordinate
        /// </summary>
        /// <param name="newX">New X coordinate</param>
        /// <param name="newY">New Y coordinate</param>
        public void MoveTo(double newX, double newY)
        {
            _x = newX;
            _y = newY;
        }

        /// <summary>
        /// Moves the point by the given increments
        /// </summary>
        /// <param name="dX">The X coordinate increment</param>
        /// <param name="dY">The Y coordinate increment</param>
        /// <param name="dz">Ignored: the point is in the XY plane</param>
        public override void Move(double dX, double dY, double dz = 0)
        {
            _x += dX;
            _y += dY;
        }

        /// <summary>
        /// Moves the point by a vector (its Z component is ignored)
        /// </summary>
        /// <param name="movement">Displacement vector</param>
        public override void Move(Vector3d movement)
        {
            Move(movement.X, movement.Y);
        }

        /// <summary>
        /// A copy of the point scaled respect to the origin (the point is not changed)
        /// </summary>
        /// <param name="factor">Scale factor</param>
        /// <returns>The point (X * factor, Y * factor)</returns>
        public Point2d Scale(double factor)
        {
            return new Point2d(X*factor, Y*factor);
        }

        /// <summary>
        /// Get the mirror point about the straight line defined by the equation ax + by + c = 0
        /// </summary>
        /// <param name="a">The 'a' parameter of the equation</param>
        /// <param name="b">The 'b' parameter of the equation</param>
        /// <param name="c">The 'c' parameter of the equation</param>
        /// <returns>A new point, symmetric of this one respect to the line</returns>
        public Point2d Mirror(double a, double b, double c)
        {
            double k = -2 * (a * _x + b * _y + c) / (a * a + b * b);
            double x = k * a + _x;
            double y = k * b + _y;
            return new Point2d(x, y);
        }

		/// <summary>
		/// Calculate the distance with <paramref name="point"/>
		/// </summary>
		/// <param name="point">The input point</param>
		/// <returns>The distance</returns>
		public double DistanceTo(Point2d point)
        {
            return Math.Sqrt(SquareDistanceTo(point));
        }

		/// <summary>
		/// Calculate the square of the distance with <paramref name="point"/>
		/// </summary>
		/// <param name="point">The input point</param>
		/// <returns>The square of the distance (faster than <see cref="DistanceTo"/> for the comparisons)</returns>
		public double SquareDistanceTo(Point2d point)
        {
            double dx = point._x - _x;
            double dy = point._y - _y;
            return dx * dx + dy * dy;
        }

		/// <summary>
		/// The vector from this point to <paramref name="point"/>
		/// </summary>
		/// <param name="point">The end point</param>
		/// <returns>The vector <paramref name="point"/> - this</returns>
		public Vector2d VectorTo(Point2d point)
        {
            return new Vector2d(point - this);
        }

		/// <summary>
		/// Clone and move the cloned point with the vector <paramref name="movement"/>
		/// </summary>
		/// <param name="movement">The movement vector</param>
		/// <returns>The new point moved</returns>
		public Point2d CloneAndMove(Vector2d movement)
        {
            var p = (Point2d)Clone();
            p.Move(movement);
            return p;
        }

		/// <summary>
		/// Clone the point
		/// </summary>
		/// <returns>The new object cloned</returns>
		public override object Clone()
        {
            return new Point2d(this);
        }

        /// <summary>
        /// Rotates the point in place by a given angle around a given point
        /// </summary>
        /// <param name="point">The center of rotation</param>
        /// <param name="angle">The angle of rotation (radians, counterclockwise)</param>
        public void Rotate(Point2d point, double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            double dx = cos * (_x - point.X) - sin * (_y - point.Y);
            double dy = sin * (_x - point.X) + cos * (_y - point.Y);
            _x = dx + point.X;
            _y = dy + point.Y;
        }

        /// <summary>
        /// Converts the string representation of a point to its Point2d equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a point to convert.</param>
        /// <param name="result">The equivalent Point2d</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Point2d result)
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            string[] parts = s.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                result = null;
                return false;
            }

            if (!double.TryParse(parts[0].Trim(), out double x) || !double.TryParse(parts[1].Trim(), out double y))
            {
                result = null;
                return false;
            }

            result = new Point2d(x, y);
            return true;
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// The sum of the coordinates of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>A new point</returns>
        public static Point2d operator +(Point2d a, Point2d b)
        {
            return new Point2d(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// The point translated by a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The translation</param>
        /// <returns>A new point</returns>
        public static Point2d operator +(Point2d point, Vector2d vector)
        {
            return new Point2d(point.X + vector.X, point.Y + vector.Y);
        }

        /// <summary>
        /// The difference of the coordinates of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The point to subtract</param>
        /// <returns>A new point</returns>
        public static Point2d operator -(Point2d a, Point2d b)
        {
            return new Point2d(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// The point translated by the opposite of a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The vector to subtract</param>
        /// <returns>A new point</returns>
        public static Point2d operator -(Point2d point, Vector2d vector)
        {
            return new Point2d(point.X - vector.X, point.Y - vector.Y);
        }

        /// <summary>
        /// The scalar product of the position vectors of two points
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>a.X * b.X + a.Y * b.Y</returns>
        public static double operator *(Point2d a, Point2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// The scalar product of the position vector of a point and a vector
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="vector">The vector</param>
        /// <returns>point.X * vector.X + point.Y * vector.Y</returns>
        public static double operator *(Point2d point, Vector2d vector)
        {
            return point.X * vector.X + point.Y * vector.Y;
        }

        /// <summary>
        /// The coordinates multiplied by a number
        /// </summary>
        /// <param name="a">The point</param>
        /// <param name="b">The factor</param>
        /// <returns>A new point</returns>
        public static Point2d operator *(Point2d a, double b)
        {
            return new Point2d(a.X * b, a.Y * b);
        }

        /// <summary>
        /// The coordinates multiplied by a number
        /// </summary>
        /// <param name="a">The factor</param>
        /// <param name="b">The point</param>
        /// <returns>A new point</returns>
        public static Point2d operator *(double a, Point2d b)
        {
            return b * a;
        }

        /// <summary>
        /// The coordinates divided by a number
        /// </summary>
        /// <param name="a">The point</param>
        /// <param name="b">The divisor</param>
        /// <returns>A new point</returns>
        public static Point2d operator /(Point2d a, double b)
        {
            return a * (1 / b);
        }

        /// <summary>
        /// Equality within the tolerance: true if the points are the same object, both null or not farther than <see cref="GeometryBase.Tolerance"/>
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>True if the points are equal</returns>
        public static bool operator ==(Point2d point1, Point2d point2)
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
        public static bool operator !=(Point2d point1, Point2d point2)
        {
            return !(point1 == point2);
        }

        /// <summary>
        /// The point in the space, with Z = 0
        /// </summary>
        /// <param name="point">The point</param>
        public static implicit operator Point3d(Point2d point)
        {
            return new Point3d(point.X, point.Y, 0);
        }

        /// <summary>
        /// The position vector of the point
        /// </summary>
        /// <param name="point">The point</param>
        public static implicit operator Vector2d(Point2d point)
        {
            return new Vector2d(point.X, point.Y);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality within the tolerance with another object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a point not farther than <see cref="GeometryBase.Tolerance"/></returns>
        public override bool Equals(object obj)
        {
            if (obj is Point2d point)
            {
                return Equals(point);
            }
            return false;
        }

		/// <summary>
		/// Equality within the tolerance
		/// </summary>
		/// <param name="other">The point to compare</param>
		/// <returns>True if <paramref name="other"/> is not null and not farther than <see cref="GeometryBase.Tolerance"/></returns>
		public bool Equals(Point2d other)
		{
			return !(other is null) && SquareDistanceTo(other) <= Tolerance * Tolerance;
		}

		/// <summary>
		/// A constant hash compatible with tolerance equality. Use ExactComparer for exact coordinate keys or a spatial index for proximity searches.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>Exact coordinate equality and hashing for dictionaries. Do not mutate their keys.</summary>
        public static IEqualityComparer<Point2d> ExactComparer { get; } = new ExactCoordinateComparer<Point2d>(p => (p.X, p.Y, 0.0));

        /// <summary>
        /// The coordinates separated by the list separator of the current culture (the format read by <see cref="TryParse"/>)
        /// </summary>
        /// <returns>"X; Y" (with the separator of the culture)</returns>
        public override string ToString()
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            return $"{_x}{separator} {_y}";
        }

		/// <summary>
		/// Equality within the tolerance with another geometry
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal point</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
            if (geometryBase is Point2d point)            
                return Equals(point);

            return false;
        }

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
		}

		#endregion
	}
}
