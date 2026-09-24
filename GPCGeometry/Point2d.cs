using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Geometry
{
    [Serializable]
    public sealed class Point2d : GeometryBase, ISerializable, IEquatable<Point2d>, ICloneable
    {
        #region Variables

        private double _x;
        private double _y;

        #endregion

        #region Properties

        public static Point2d Origin => new Point2d(0, 0);

        public double X { get => _x; set { _x = value; } }

        public double Y { get => _y; set { _y = value; } }

        public double[] Coordinates => new double[] { _x, _y };

        #endregion

        #region Public Constructors

        public Point2d()
            : base()
        {
            _x = 0.0;
            _y = 0.0;
        }

        public Point2d(double x, double y)
            : base()
        {
            _x = x;
            _y = y;
        }

        public Point2d(Point2d p)
            : this(p._x, p._y)
        {
        }

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
        /// Move point by an given increment dx, dy
        /// </summary>
        /// <param name="dX">The X coordinate increment</param>
        /// <param name="dY">The Y coordinate increment</param>
        /// <param name="dz"></param>
        public override void Move(double dX, double dY, double dz = 0)
        {
            _x += dX;
            _y += dY;
        }

        /// <summary>
        /// Move point by a given vector
        /// </summary>
        /// <param name="movement">Displacement vector</param>
        public override void Move(Vector3d movement)
        {
            Move(movement.X, movement.Y);
        }

        /// <summary>
        /// Scale the point respect to the origin 
        /// </summary>
        /// <param name="factor">Scale factor</param>
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
        /// <returns></returns>
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
		/// Calculate the square distance with <paramref name="point"/>
		/// </summary>
		/// <param name="point">The input point</param>
		/// <returns>The distance</returns>
		public double SquareDistanceTo(Point2d point)
        {
            double dx = point._x - _x;
            double dy = point._y - _y;
            return dx * dx + dy * dy;
        }

		/// <summary>
		/// Calculate the vector with <paramref name="point"/>
		/// </summary>
		/// <param name="point">The input point</param>
		/// <returns>The vector</returns>
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
        /// Rotate the point by a given angle around a given point
        /// </summary>
        /// <param name="point">point around which the line rotates</param>
        /// <param name="angle">angle of rotation (radians)</param>
        /// <returns></returns>
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

        public static Point2d operator +(Point2d a, Point2d b)
        {
            return new Point2d(a.X + b.X, a.Y + b.Y);
        }

        public static Point2d operator +(Point2d point, Vector2d vector)
        {
            return new Point2d(point.X + vector.X, point.Y + vector.Y);
        }

        public static Point2d operator -(Point2d a, Point2d b)
        {
            return new Point2d(a.X - b.X, a.Y - b.Y);
        }

        public static Point2d operator -(Point2d point, Vector2d vector)
        {
            return new Point2d(point.X - vector.X, point.Y - vector.Y);
        }

        public static double operator *(Point2d a, Point2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double operator *(Point2d point, Vector2d vector)
        {
            return point.X * vector.X + point.Y * vector.Y;
        }

        public static Point2d operator *(Point2d a, double b)
        {
            return new Point2d(a.X * b, a.Y * b);
        }

        public static Point2d operator *(double a, Point2d b)
        {
            return b * a;
        }

        public static Point2d operator /(Point2d a, double b)
        {
            return a * (1 / b);
        }

        public static bool operator ==(Point2d point1, Point2d point2)
        {
            if (ReferenceEquals(point1, point2))
                return true;
            if (point1 is null || point2 is null)
                return false;
            return point1.Equals(point2);
        }

        public static bool operator !=(Point2d point1, Point2d point2)
        {
            return !(point1 == point2);
        }

        public static implicit operator Point3d(Point2d point)
        {
            return new Point3d(point.X, point.Y, 0);
        }

        public static implicit operator Vector2d(Point2d point)
        {
            return new Vector2d(point.X, point.Y);
        }

        #endregion

        #region Public Methods Override

        public override bool Equals(object obj)
        {
            if (obj is Point2d point)
            {
                return Equals(point);
            }
            return false;
        }

		public bool Equals(Point2d other)
		{
			return !(other is null) && SquareDistanceTo(other) <= Tolerance * Tolerance;
		}

		public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + _x.GetHashCode();
                hashCode = hashCode * -17 + _y.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            string separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            return $"{_x}{separator} {_y}";
        }

		public override bool Equals(GeometryBase geometryBase)
		{
            if (geometryBase is Point2d point)            
                return Equals(point);

            return false;
        }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("X", _x);
			info.AddValue("Y", _y);
		}

		#endregion
	}
}
