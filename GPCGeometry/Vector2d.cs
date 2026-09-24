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
    public sealed class Vector2d : GeometryBase, ISerializable, IEquatable<Vector2d>, ICloneable
    {
        #region Variables

        private double _x;
        private double _y;

        #endregion

        #region Properties

        public static Vector2d XAxis => new Vector2d(1, 0);

        public static Vector2d YAxis => new Vector2d(0, 1);

        public static Vector2d Zero => new Vector2d(0, 0);

        public double X { get => _x; set => _x = value; }

        public double Y { get=> _y; set => _y = value; }

        public double Length => Math.Sqrt(Math.Pow(_x, 2) + Math.Pow(_y, 2));

        #endregion

        #region Public Constructors

        public Vector2d(double x, double y)
            : base()
        {
            _x = x;
            _y = y;
        }

        public Vector2d(Vector2d v)
            : this(v._x, v._y)
        {
        }

		public Vector2d(Vector3d v)
            : this(v.X, v.Y)
        {
        }

        public Vector2d(Point2d p)
            : this(p.X, p.Y)
        {
        }

        private Vector2d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
        }

        /// <summary>
        /// Calculate the dot product (or scalar product) with the given vector
        /// </summary>
        public double DotProduct(Vector2d vector)
        {
            return this * vector;
        }

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
        /// Unitizes the vector in place. A unit vector has length 1 unit.
        /// </summary>
        public void Unitize()
        {
            double l = Length;
            Vector2d u = new Vector2d(this / l);
            _x = u.X;
            _y = u.Y;
        }

        /// <summary>
        /// Unitize the given vector
        /// </summary>
        /// <param name="vector">The vector to make of length equal to 1 unit</param>
        public static Vector2d Unitize(Vector2d vector)
        {
            Vector2d u = (Vector2d)vector.Clone();
            u.Unitize();
            return u;
        }

        /// <summary>
        /// Calculate the norm of the vector
        /// /// </summary>
        public double Norm()
        {
            if (_x == 0 && _y == 0)
                return 0;

            return Math.Sqrt(Math.Pow(_x, 2.0) + Math.Pow(_y, 2.0));
        }

        public override object Clone()
        {
            return new Vector2d(this);
        }

        /// <summary>
        /// Calculate the angle with the given vector. Angle measured in radians
        /// </summary>
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

        public override void Move(double v1, double v2, double v3)
        {            
        }

        public override void Move(Vector3d vector)
        {            
        }

        /// <summary>
        /// Rotates a 2D vector in the XY plane by one angle counterclockwise.
        /// </summary>
        /// <param name="angle"></param>
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

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Calculate the dot product (or scalar product) beetween two vectors
        /// </summary>
        public static double operator *(Vector2d a, Vector2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector2d operator *(Vector2d a, double b)
        {
            return new Vector2d(a.X * b, a.Y * b);
        }

        public static Vector2d operator *(double a, Vector2d b)
        {
            return b * a;
        }

        public static Vector2d operator /(Vector2d a, double b)
        {
            return a * (1 / b);
        }

        /// <summary>
        /// Calculate the cross product beetween two vectors
        /// </summary>
        /// <returns></returns>
        public static double operator ^(Vector2d a, Vector2d b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static bool operator ==(Vector2d vector1, Vector2d vector2)
        {
            if (ReferenceEquals(vector1, vector2))
                return true;
            if (vector1 is null || vector2 is null)
                return false;
            return vector1.Equals(vector2);
        }

        public static bool operator !=(Vector2d vector1, Vector2d vector2)
        {
            return !(vector1 == vector2);
        }

        public static implicit operator Vector3d(Vector2d vector)
        {
            return new Vector3d(vector.X, vector.Y, 0);
        }

        public static implicit operator Point2d(Vector2d vector)
        {
            return new Point2d(vector.X, vector.Y);
        }

        #endregion

        #region Public Methods Override

        public override bool Equals(object obj)
        {
            if (obj is Vector2d point)
            {
                return Equals(point);
            }
            return false;
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Vector2d vector)
                return Equals(vector);

            return false;
        }

		public bool Equals(Vector2d other)
		{
			return !(other is null) && Math.Pow(other._x - _x, 2) + Math.Pow(other._y - _y, 2) <= Math.Pow(Tolerance, 2);
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
            return $"Vector: X: {_x}, Y: {_y}";
        }

        #endregion
    }
}
