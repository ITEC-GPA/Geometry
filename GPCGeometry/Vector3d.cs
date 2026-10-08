using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace GPC.Geometry
{
	/// <summary>
	/// A vector in the space. Two vectors are equal if the distance of their ends is not bigger than <see cref="GeometryBase.Tolerance"/>
	/// </summary>
	[Serializable]
	public sealed class Vector3d : GeometryBase, ISerializable, IEquatable<Vector3d>, ICloneable
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
		/// <summary>
		/// The Z component
		/// </summary>
		private double _z;

		#endregion

		#region Properties

		/// <summary>
		/// A new unit vector along X
		/// </summary>
		public static Vector3d XAxis => new Vector3d(1, 0, 0);

		/// <summary>
		/// A new unit vector along Y
		/// </summary>
		public static Vector3d YAxis => new Vector3d(0, 1, 0);

		/// <summary>
		/// A new unit vector along Z
		/// </summary>
		public static Vector3d ZAxis => new Vector3d(0, 0, 1);

		/// <summary>
		/// A new zero vector
		/// </summary>
		public static Vector3d Zero => new Vector3d(0, 0, 0);

		/// <summary>
		/// The X component
		/// </summary>
		public double X { get => _x; set => _x = value; }

		/// <summary>
		/// The Y component
		/// </summary>
		public double Y { get => _y; set => _y = value; }

		/// <summary>
		/// The Z component
		/// </summary>
		public double Z { get => _z; set => _z = value; }

		/// <summary>
		/// The length of the vector
		/// </summary>
		public double Length => Math.Sqrt(_x * _x + _y * _y + _z * _z);

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a vector
		/// </summary>
		/// <param name="x">The X component</param>
		/// <param name="y">The Y component</param>
		/// <param name="z">The Z component</param>
		public Vector3d(double x, double y, double z)
			: base()
		{
			_x = x;
			_y = y;
			_z = z;
		}

		/// <summary>
		/// Creates a copy of a vector
		/// </summary>
		/// <param name="v">The vector to copy</param>
		public Vector3d(Vector3d v)
			: this(v._x, v._y, v._z)
		{
		}

		/// <summary>
		/// Creates the position vector of a point
		/// </summary>
		/// <param name="p">The point</param>
		public Vector3d(Point3d p)
			: this(p.X, p.Y, p.Z)
		{
		}

		/// <summary>
		/// Creates the vector from <paramref name="p1"/> to <paramref name="p2"/>
		/// </summary>
		/// <param name="p1">The start point</param>
		/// <param name="p2">The end point</param>
		public Vector3d(Point3d p1, Point3d p2)
			: this(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z)
		{
		}

		/// <summary>
		/// Deserialization constructor: reads the components (the saved <see cref="BaseObject.Guid"/> is not read)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		private Vector3d(SerializationInfo info, StreamingContext context)
		{
			_x = info.GetDouble("X");
			_y = info.GetDouble("Y");
			_z = info.GetDouble("Z");
		}

		/// <summary>
		/// Creates the vector of the XY plane (Z = 0)
		/// </summary>
		/// <param name="v">The vector of the plane</param>
		public Vector3d(Vector2d v)
			: this(v.X, v.Y, 0.0)
		{
		}

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Calculate the dot product (or scalar product) with the given vector
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>X * vector.X + Y * vector.Y + Z * vector.Z</returns>
		public double DotProduct(Vector3d vector)
		{
			return _x * vector._x + _y * vector.Y + _z * vector._z;
		}

		/// <summary>
		/// The vector multiplied by a number (a scaling, despite the name)
		/// </summary>
		/// <param name="value">The factor</param>
		/// <returns>A new vector</returns>
		public Vector3d DotProduct(double value)
		{
			return new Vector3d(_x * value, _y * value, _z * value);
		}

		/// <summary>
		/// Calculate the cross product (or vector product) with the given vector
		/// </summary>
		/// <param name="vector">The second vector</param>
		/// <returns>The resulting vector</returns>
		public Vector3d CrossProduct(Vector3d vector)
		{
			return new Vector3d(_y * vector._z - _z * vector._y, -(_x * vector._z - _z * vector._x), _x * vector._y - _y * vector._x);
		}

		/// <summary>
		/// Calculate the angle with the given vector, with a numerically stable formula: atan2(|a × b|, a · b)
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The angle between the two directions, in radians, from 0 to pi (without sign)</returns>
		public double AngleTo(Vector3d vector)
		{
			// https://scicomp.stackexchange.com/questions/27689/numerically-stable-way-of-computing-angles-between-vectors

			//Vector3d v1 = (Vector3d)vector.Clone();
			//v1.Unitize();
			//Vector3d v2 = new Vector3d(this);
			//v2.Unitize();

			//double dot = vector * this;
			//dot = dot > 1.0 ? 1.0 : dot;
			//dot = dot < -1.0 ? -1.0 : dot;

			//double cosine = dot / (vector.Length * this.Length);

			//cosine = cosine > 1.0 ? 1.0 : cosine;
			//cosine = cosine < -1.0 ? -1.0 : cosine;

			//return Math.Acos(cosine);

			return Math.Atan2(CrossProduct(vector).Length, vector * this);
		}

		/// <summary>
		/// Calculate the norm (the length) of the vector
		/// </summary>
		/// <returns>The length, 0 for a zero vector</returns>
		public double Norm()
		{
			if (_x == 0 && _y == 0 && _z == 0)
				return 0;

			return Math.Sqrt(Math.Pow(_x, 2.0) + Math.Pow(_y, 2.0) + Math.Pow(_z, 2.0));
		}

		/// <summary>
		/// Unitizes the vector in place. A unit vector has length 1 unit (a zero vector gets NaN components)
		/// </summary>
		public void Unitize()
		{
			double lenght = Length;

			_x /= lenght;
			_y /= lenght;
			_z /= lenght;
		}

		/// <summary>
		/// A unit vector with the direction of the given one
		/// </summary>
		/// <param name="vector">The vector to make of length equal to 1 unit (it is not changed)</param>
		/// <returns>A new unit vector</returns>
		public static Vector3d Unitize(Vector3d vector)
		{
			Vector3d u = (Vector3d)vector.Clone();
			u.Unitize();
			return u;
		}

		/// <summary>
		/// Reverses this vector in place (reverses the direction).
		/// </summary>
		public void Reverse()
		{
			_x *= -1;
			_y *= -1;
			_z *= -1;
		}

		/// <summary>
		/// The opposite of the given vector
		/// </summary>
		/// <param name="vector">The vector to reverse (it is not changed)</param>
		/// <returns>A new vector</returns>
		public static Vector3d Reverse(Vector3d vector)
		{
			Vector3d v = (Vector3d)vector.Clone();
			v.Reverse();
			return v;
		}

		/// <summary>
		/// Creates a copy of the vector
		/// </summary>
		/// <returns>The copy</returns>
		public override object Clone()
		{
			return new Vector3d(this);
		}

		/// <summary>
		/// Adds the given increments to the components of the vector (same behaviour of <see cref="Move(Vector3d)"/>; note that
		/// <see cref="Vector2d"/> is not changed by Move)
		/// </summary>
		/// <param name="v1">The increment of X</param>
		/// <param name="v2">The increment of Y</param>
		/// <param name="v3">The increment of Z</param>
		public override void Move(double v1, double v2, double v3)
		{
			_x += v1;
			_y += v2;
			_z += v3;
		}

		/// <summary>
		/// Adds a vector to this one, in place
		/// </summary>
		/// <param name="vector">The vector to add</param>
		public override void Move(Vector3d vector)
		{
			_x += vector._x;
			_y += vector._y;
			_z += vector._z;
		}

		/// <summary>
		/// Check if two vectors are parallel (with the same or the opposite direction)
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <param name="tolerance">The tolerance on the length of the cross product</param>
		/// <returns>True if |this × vector| is smaller than <paramref name="tolerance"/> (the result depends on the lengths of the vectors)</returns>
		public bool IsParallelTo(Vector3d vector, double tolerance = GeometryBase.Tolerance)
		{
			return CrossProduct(vector).Length < tolerance;
		}

		/// <summary>
		/// Check if two vectors are orthogonal
		/// </summary>
		/// <param name="v">The other vector</param>
		/// <param name="tolerance">The tolerance on the dot product</param>
		/// <returns>True if |this · v| is smaller than <paramref name="tolerance"/> (the result depends on the lengths of the vectors)</returns>
		public bool IsOrthogonalTo(Vector3d v, double tolerance = GeometryBase.Tolerance)
		{
			return Math.Abs(DotProduct(v)) < tolerance;
		}

		#endregion

		#region Operators overrides

		/// <summary>
		/// The sum of two vectors
		/// </summary>
		/// <param name="a">The first vector</param>
		/// <param name="b">The second vector</param>
		/// <returns>A new vector</returns>
		public static Vector3d operator +(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		/// <summary>
		/// The difference of two vectors
		/// </summary>
		/// <param name="a">The first vector</param>
		/// <param name="b">The vector to subtract</param>
		/// <returns>A new vector</returns>
		public static Vector3d operator -(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		/// <summary>
		/// Calculate the dot product (or scalar product) between two vectors
		/// </summary>
		/// <param name="a">The first vector</param>
		/// <param name="b">The second vector</param>
		/// <returns>The scalar product</returns>
		public static double operator *(Vector3d a, Vector3d b)
		{
			return a.DotProduct(b);
		}

		/// <summary>
		/// The vector multiplied by a number
		/// </summary>
		/// <param name="a">The vector</param>
		/// <param name="b">The factor</param>
		/// <returns>A new vector</returns>
		public static Vector3d operator *(Vector3d a, double b)
		{
			return a.DotProduct(b);
		}

		/// <summary>
		/// The vector multiplied by a number
		/// </summary>
		/// <param name="a">The factor</param>
		/// <param name="b">The vector</param>
		/// <returns>A new vector</returns>
		public static Vector3d operator *(double a, Vector3d b)
		{
			return b.DotProduct(a);
		}

		/// <summary>
		/// The vector divided by a number
		/// </summary>
		/// <param name="a">The vector</param>
		/// <param name="b">The divisor</param>
		/// <returns>A new vector</returns>
		public static Vector3d operator /(Vector3d a, double b)
		{
			return a * (1 / b);
		}

		/// <summary>
		/// Calculate the cross product (or vector product) between two vectors
		/// </summary>
		/// <param name="a">The first vector</param>
		/// <param name="b">The second vector</param>
		/// <returns>A new vector a × b</returns>
		public static Vector3d operator ^(Vector3d a, Vector3d b)
		{
			return a.CrossProduct(b);
		}

		/// <summary>
		/// Equality within the tolerance: true if the vectors are the same object, both null or equal (<see cref="Equals(Vector3d)"/>)
		/// </summary>
		/// <param name="vector1">The first vector</param>
		/// <param name="vector2">The second vector</param>
		/// <returns>True if the vectors are equal</returns>
		public static bool operator ==(Vector3d vector1, Vector3d vector2)
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
		public static bool operator !=(Vector3d vector1, Vector3d vector2)
		{
			return !(vector1 == vector2);
		}

		/// <summary>
		/// The point with the components of the vector as coordinates
		/// </summary>
		/// <param name="vector">The vector</param>
		public static implicit operator Point3d(Vector3d vector)
		{
			return new Point3d(vector.X, vector.Y, vector.Z);
		}

		#endregion

		#region Public Methods Override

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
			info.AddValue("Z", _z);
		}

		/// <summary>
		/// Equality within the tolerance
		/// </summary>
		/// <param name="other">The vector to compare</param>
		/// <returns>True if the length of the difference is not bigger than <see cref="GeometryBase.Tolerance"/></returns>
		public bool Equals(Vector3d other)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) + Math.Pow(other._z - _z, 2) <= Math.Pow(Tolerance, 2);
		}

		/// <summary>
		/// Equality within a given tolerance
		/// </summary>
		/// <param name="other">The vector to compare</param>
		/// <param name="toll">The tolerance on the length of the difference</param>
		/// <returns>True if the length of the difference is not bigger than <paramref name="toll"/></returns>
		public bool Equals(Vector3d other, double toll)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) + Math.Pow(other._z - _z, 2) <= Math.Pow(toll, 2);
		}

		/// <summary>
		/// Equality within the tolerance with another geometry
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal vector</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Vector3d vector)
				return Equals(vector);

			return false;
		}

		/// <summary>
		/// Equality within the tolerance with another object
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal vector</returns>
		public override bool Equals(object obj)
		{
			if (obj is Vector3d point)
				return Equals(point);

			return false;
		}

		/// <summary>
		/// A constant hash compatible with tolerance equality. Use ExactComparer for exact component keys.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

        /// <summary>Exact component equality and hashing for dictionaries. Do not mutate their keys.</summary>
        public static IEqualityComparer<Vector3d> ExactComparer { get; } = new ExactCoordinateComparer<Vector3d>(p => (p.X, p.Y, p.Z));

		/// <summary>
		/// A description of the vector
		/// </summary>
		/// <returns>"Vector: X: x, Y: y, Z: z"</returns>
		public override string ToString()
		{
			return $"Vector: X: {_x}, Y: {_y}, Z: {_z}";
		}

		#endregion
	}
}
