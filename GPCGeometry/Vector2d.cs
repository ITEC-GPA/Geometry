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
    /// A vector in the XY plane. Two vectors are equal if the distance of their ends is not bigger than <see cref="GeometryBase.Tolerance"/>
    /// </summary>
    [Serializable]
    public sealed class Vector2d : GeometryBase, ISerializable, IEquatable<Vector2d>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The X component
        /// </summary>
        private double _x;
        /// <summary>
        /// The Y component
        /// </summary>
        private double _y;

        #endregion

        #region Properties

        /// <summary>
        /// A new unit vector along X
        /// </summary>
        public static Vector2d XAxis => new Vector2d(1, 0);

        /// <summary>
        /// A new unit vector along Y
        /// </summary>
        public static Vector2d YAxis => new Vector2d(0, 1);

        /// <summary>
        /// A new zero vector
        /// </summary>
        public static Vector2d Zero => new Vector2d(0, 0);

        /// <summary>
        /// The X component
        /// </summary>
        public double X { get => _x; set => _x = value; }

        /// <summary>
        /// The Y component
        /// </summary>
        public double Y { get=> _y; set => _y = value; }

        /// <summary>
        /// The length of the vector
        /// </summary>
        public double Length => Math.Sqrt(Math.Pow(_x, 2) + Math.Pow(_y, 2));

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="x">The X component</param>
        /// <param name="y">The Y component</param>
        public Vector2d(double x, double y)
            : base()
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Creates a copy of a vector
        /// </summary>
        /// <param name="v">The vector to copy</param>
        public Vector2d(Vector2d v)
            : this(v._x, v._y)
        {
        }

		/// <summary>
		/// Creates the projection of a vector on the XY plane (Z is dropped)
		/// </summary>
		/// <param name="v">The vector in the space</param>
		public Vector2d(Vector3d v)
            : this(v.X, v.Y)
        {
        }

        /// <summary>
        /// Creates the position vector of a point
        /// </summary>
        /// <param name="p">The point</param>
        public Vector2d(Point2d p)
            : this(p.X, p.Y)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the components (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Vector2d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Serializes the vector: the <see cref="BaseObject.Guid"/> and the components
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
        }

        /// <summary>
        /// Calculate the dot product (or scalar product) with the given vector
        /// </summary>
        /// <param name="vector">The other vector</param>
        /// <returns>X * vector.X + Y * vector.Y</returns>
        public double DotProduct(Vector2d vector)
        {
            return this * vector;
        }

        /// <summary>
        /// The vector perpendicular to this one, with the same length
        /// </summary>
        /// <param name="onLeftOfdir">True: the vector rotated by +90° (on the left of the direction); false: rotated by -90°</param>
        /// <returns>A new vector</returns>
        public Vector2d Normal(bool onLeftOfdir)
        {
            if (onLeftOfdir)
            {
                return new Vector2d(-_y, _x);
            }
            else
            {
                return new Vector2d(_y, -_x);
            }
        }
        
        /// <summary>
        /// Unitizes the vector in place. A unit vector has length 1 unit (a zero vector gets NaN components)
        /// </summary>
        public void Unitize()
        {
            double l = Length;
            Vector2d u = new Vector2d(this / l);
            _x = u.X;
            _y = u.Y;
        }

        /// <summary>
        /// A unit vector with the direction of the given one
        /// </summary>
        /// <param name="vector">The vector to make of length equal to 1 unit (it is not changed)</param>
        /// <returns>A new unit vector</returns>
        public static Vector2d Unitize(Vector2d vector)
        {
            Vector2d u = (Vector2d)vector.Clone();
            u.Unitize();
            return u;
        }

        /// <summary>
        /// Calculate the norm (the length) of the vector
        /// </summary>
        /// <returns>The length, 0 for a zero vector</returns>
        public double Norm()
        {
            if (_x == 0 && _y == 0)
                return 0;

            return Math.Sqrt(Math.Pow(_x, 2.0) + Math.Pow(_y, 2.0));
        }

        /// <summary>
        /// Creates a copy of the vector
        /// </summary>
        /// <returns>The copy</returns>
        public override object Clone()
        {
            return new Vector2d(this);
        }

        /// <summary>
        /// Calculate the angle with the given vector
        /// </summary>
        /// <param name="vector">The other vector</param>
        /// <returns>The angle between the two directions, in radians, from 0 to pi (without sign)</returns>
        public double AngleTo(Vector2d vector)
        {
            Vector2d v1 = (Vector2d)vector.Clone();
            v1.Unitize();
            Vector2d v2 = (Vector2d)this.Clone();
            v2.Unitize();

            double dot = v2 * v1;
            dot = dot > 1.0 ? 1.0 : dot;
            dot = dot < -1.0 ? -1.0 : dot;

            double len1 = v1.Length;
            double len2 = v2.Length;

            double cosine = dot / (len1 * len2);

            cosine = cosine > 1.0 ? 1.0 : cosine;
            cosine = cosine < -1.0 ? -1.0 : cosine;

            return Math.Acos(cosine);
        }

        /// <summary>
        /// Does nothing: a vector is not changed by a translation
        /// </summary>
        /// <param name="v1">The translation along X</param>
        /// <param name="v2">The translation along Y</param>
        /// <param name="v3">The translation along Z</param>
        public override void Move(double v1, double v2, double v3)
        {            
        }

        /// <summary>
        /// Does nothing: a vector is not changed by a translation
        /// </summary>
        /// <param name="vector">The translation</param>
        public override void Move(Vector3d vector)
        {            
        }

        /// <summary>
        /// Rotates the vector in place, in the XY plane, by an angle counterclockwise
        /// </summary>
        /// <param name="angle">The angle of rotation (radians)</param>
        public void Rotate(in double angle)
        {
            double _xn = _x;
            double _yn = _y;
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            _x = _xn * cosAngle - _yn * sinAngle;
            _y = _xn * sinAngle + _yn * cosAngle;
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// The sum of two vectors
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>A new vector</returns>
        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// The difference of two vectors
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The vector to subtract</param>
        /// <returns>A new vector</returns>
        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Calculate the dot product (or scalar product) between two vectors
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>a.X * b.X + a.Y * b.Y</returns>
        public static double operator *(Vector2d a, Vector2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// The vector multiplied by a number
        /// </summary>
        /// <param name="a">The vector</param>
        /// <param name="b">The factor</param>
        /// <returns>A new vector</returns>
        public static Vector2d operator *(Vector2d a, double b)
        {
            return new Vector2d(a.X * b, a.Y * b);
        }

        /// <summary>
        /// The vector multiplied by a number
        /// </summary>
        /// <param name="a">The factor</param>
        /// <param name="b">The vector</param>
        /// <returns>A new vector</returns>
        public static Vector2d operator *(double a, Vector2d b)
        {
            return b * a;
        }

        /// <summary>
        /// The vector divided by a number
        /// </summary>
        /// <param name="a">The vector</param>
        /// <param name="b">The divisor</param>
        /// <returns>A new vector</returns>
        public static Vector2d operator /(Vector2d a, double b)
        {
            return a * (1 / b);
        }

        /// <summary>
        /// Calculate the cross product between two vectors of the plane
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The Z component of a × b: a.X * b.Y - a.Y * b.X (positive if <paramref name="b"/> is on the left of <paramref name="a"/>)</returns>
        public static double operator ^(Vector2d a, Vector2d b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Equality within the tolerance: true if the vectors are the same object, both null or equal (<see cref="Equals(Vector2d)"/>)
        /// </summary>
        /// <param name="vector1">The first vector</param>
        /// <param name="vector2">The second vector</param>
        /// <returns>True if the vectors are equal</returns>
        public static bool operator ==(Vector2d vector1, Vector2d vector2)
        {
            if (ReferenceEquals(vector1, vector2))
                return true;
            if (vector1 is null || vector2 is null)
                return false;
            return vector1.Equals(vector2);
        }

        /// <summary>
        /// Inequality within the tolerance (see the equality operator)
        /// </summary>
        /// <param name="vector1">The first vector</param>
        /// <param name="vector2">The second vector</param>
        /// <returns>True if the vectors are different</returns>
        public static bool operator !=(Vector2d vector1, Vector2d vector2)
        {
            return !(vector1 == vector2);
        }

        /// <summary>
        /// The vector in the space, with Z = 0
        /// </summary>
        /// <param name="vector">The vector</param>
        public static implicit operator Vector3d(Vector2d vector)
        {
            return new Vector3d(vector.X, vector.Y, 0);
        }

        /// <summary>
        /// The point with the components of the vector as coordinates
        /// </summary>
        /// <param name="vector">The vector</param>
        public static implicit operator Point2d(Vector2d vector)
        {
            return new Point2d(vector.X, vector.Y);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality within the tolerance with another object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal vector</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vector2d point)
            {
                return Equals(point);
            }
            return false;
        }

        /// <summary>
        /// Equality within the tolerance with another geometry
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal vector</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Vector2d vector)
                return Equals(vector);

            return false;
        }

		/// <summary>
		/// Equality within the tolerance
		/// </summary>
		/// <param name="other">The vector to compare</param>
		/// <returns>True if the length of the difference is not bigger than <see cref="GeometryBase.Tolerance"/></returns>
		public bool Equals(Vector2d other)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) <= Math.Pow(Tolerance, 2);
		}

		/// <summary>
		/// The hash code of the exact components (equal vectors within the tolerance can have different hash codes)
		/// </summary>
		/// <returns>The hash code</returns>
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

        /// <summary>
        /// A description of the vector
        /// </summary>
        /// <returns>"Vector: X: x, Y: y"</returns>
        public override string ToString()
        {
            return $"Vector: X: {_x}, Y: {_y}";
        }

        #endregion
    }
}
