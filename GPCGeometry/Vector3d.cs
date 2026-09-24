using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	[Serializable]
	public sealed class Vector3d : GeometryBase, ISerializable, IEquatable<Vector3d>, ICloneable
	{
		#region Variables

		private double _x;
		private double _y;
		private double _z;

		#endregion

		#region Properties

		public static Vector3d XAxis => new Vector3d(1, 0, 0);

		public static Vector3d YAxis => new Vector3d(0, 1, 0);

		public static Vector3d ZAxis => new Vector3d(0, 0, 1);

		public static Vector3d Zero => new Vector3d(0, 0, 0);

		public double X { get => _x; set => _x = value; }

		public double Y { get => _y; set => _y = value; }

		public double Z { get => _z; set => _z = value; }

		public double Length => Math.Sqrt(_x * _x + _y * _y + _z * _z);

		#endregion

		#region Public Constructors

		public Vector3d(double x, double y, double z)
			: base()
		{
			_x = x;
			_y = y;
			_z = z;
		}

		public Vector3d(Vector3d v)
			: this(v._x, v._y, v._z)
		{
		}

		public Vector3d(Point3d p)
			: this(p.X, p.Y, p.Z)
		{
		}

		public Vector3d(Point3d p1, Point3d p2)
			: this(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z)
		{
		}

		private Vector3d(SerializationInfo info, StreamingContext context)
		{
			_x = info.GetDouble("X");
			_y = info.GetDouble("Y");
			_z = info.GetDouble("Z");
		}

		public Vector3d(Vector2d v)
			: this(v.X, v.Y, 0.0)
		{
		}

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Calculate the dot product (or scalar product) with the given vector
		/// </summary>
		public double DotProduct(Vector3d vector)
		{
			return _x * vector._x + _y * vector.Y + _z * vector._z;
		}

		/// <summary>
		/// Calculate the dot product (or scalar product) with the given value
		/// </summary>
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
		/// Calculate the angle with the given vector. Angle measured in radians
		/// </summary>
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
		/// Calculate the norm of the vector
		/// /// </summary>
		public double Norm()
		{
			if (_x == 0 && _y == 0 && _z == 0)
				return 0;

			return Math.Sqrt(Math.Pow(_x, 2.0) + Math.Pow(_y, 2.0) + Math.Pow(_z, 2.0));
		}

		/// <summary>
		/// Unitizes the vector in place. A unit vector has length 1 unit.
		/// </summary>
		public void Unitize()
		{
			double lenght = Length;

			_x /= lenght;
			_y /= lenght;
			_z /= lenght;
		}

		/// <summary>
		/// Unitize the given vector
		/// </summary>
		/// <param name="vector">The vector to make of length equal to 1 unit</param>
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
		/// Revert the given vector
		/// </summary>
		/// <param name="vector">The vector to revert</param>
		public static Vector3d Reverse(Vector3d vector)
		{
			Vector3d v = (Vector3d)vector.Clone();
			v.Reverse();
			return v;
		}

		public override object Clone()
		{
			return new Vector3d(this);
		}

		/// <summary>
		/// Add the given increments to the components of the vector (same behaviour of <see cref="Move(Vector3d)"/>)
		/// </summary>
		public override void Move(double v1, double v2, double v3)
		{
			_x += v1;
			_y += v2;
			_z += v3;
		}

		public override void Move(Vector3d vector)
		{
			_x += vector._x;
			_y += vector._y;
			_z += vector._z;
		}

		/// <summary>
		/// Check if two vectors are parallel
		/// </summary>
		public bool IsParallelTo(Vector3d vector, double tolerance = GeometryBase.Tolerance)
		{
			return CrossProduct(vector).Length < tolerance;
		}

		/// <summary>
		/// Check if two vectors are orthogonal
		/// </summary>
		public bool IsOrthogonalTo(Vector3d v, double tolerance = GeometryBase.Tolerance)
		{
			return Math.Abs(DotProduct(v)) < tolerance;
		}

		#endregion

		#region Operators overrides

		public static Vector3d operator +(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3d operator -(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		/// <summary>
		/// Calculate the dot product (or scalar product) beetween two vectors
		/// </summary>
		public static double operator *(Vector3d a, Vector3d b)
		{
			return a.DotProduct(b);
		}

		public static Vector3d operator *(Vector3d a, double b)
		{
			return a.DotProduct(b);
		}

		public static Vector3d operator *(double a, Vector3d b)
		{
			return b.DotProduct(a);
		}

		public static Vector3d operator /(Vector3d a, double b)
		{
			return a * (1 / b);
		}

		/// <summary>
		/// Calculate the cross product (or vector product) beetween two vectors
		/// </summary>
		public static Vector3d operator ^(Vector3d a, Vector3d b)
		{
			return a.CrossProduct(b);
		}

		public static bool operator ==(Vector3d vector1, Vector3d vector2)
		{
			if (ReferenceEquals(vector1, vector2))
				return true;
			if (vector1 is null || vector2 is null)
				return false;
			return vector1.Equals(vector2);
		}

		public static bool operator !=(Vector3d vector1, Vector3d vector2)
		{
			return !(vector1 == vector2);
		}

		public static implicit operator Point3d(Vector3d vector)
		{
			return new Point3d(vector.X, vector.Y, vector.Z);
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

		public bool Equals(Vector3d other)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) + Math.Pow(other._z - _z, 2) <= Math.Pow(Tolerance, 2);
		}

		public bool Equals(Vector3d other, double toll)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) + Math.Pow(other._z - _z, 2) <= Math.Pow(toll, 2);
		}

		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Vector3d vector)
				return Equals(vector);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector3d point)
				return Equals(point);

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + _x.GetHashCode();
				hashCode = hashCode * -17 + _y.GetHashCode();
				hashCode = hashCode * -17 + _z.GetHashCode();
				return hashCode;
			}
		}

		public override string ToString()
		{
			return $"Vector: X: {_x}, Y: {_y}, Z: {_z}";
		}

		#endregion
	}
}
