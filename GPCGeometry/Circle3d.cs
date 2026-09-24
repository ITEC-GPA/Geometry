using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Extensions;

namespace GPC.Geometry
{
    /// <summary>
    /// Circle 3d is a planar circle on the (x,y,z) space
    /// </summary>
    [Serializable]
    public class Circle3d : GeometryBase, ISerializable, ICloneable, IEquatable<Circle3d>
    {
        #region VARIABLES

        protected Point3d _center;
        protected double _radius;
        protected Plane _plane;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Center of the circle
        /// </summary>
        public Point3d Center
        { 
            get => _center; 
            set => _center = value;
        }

		/// <summary>
		/// Radius of the circle
		/// </summary>
		public double Radius
        {
            get => _radius;
            set => _radius = value;
        }

        /// <summary>
        /// Plane of the circle
        /// </summary>
        public Plane Plane 
        { 
            get => _plane; 
            set => _plane = value; 
        }

		/// <summary>
		/// Perimeter of the circle
		/// </summary>
		public double Perimeter => 2 * Math.PI * _radius;

		/// <summary>
		/// Area of the circle
		/// </summary>
		public double Area => Math.PI * Math.Pow(_radius, 2);

		/// <summary>
		/// Diameter of the circle
		/// </summary>
		public double Diameter => 2.0 * Radius;

        #endregion

        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// Initializes circle instance using center point, radius and normal vector.
        /// </summary>
        public Circle3d(Point3d Center, double Radius, Plane plane)
        {
            _center = Center;
            _radius = Radius;
            _plane = plane;
        }

        /// <summary>
        /// Initializes circle passing through three points.
        /// </summary>
        public Circle3d(Point3d p1, Point3d p2, Point3d p3, double tolerance = GeometryBase.Tolerance)
        {
            CalculateVaribles(p1, p2, p3, tolerance);
		}

        /// <summary>
        /// Copy constructor (deep copy: moving the new circle does not move the original)
        /// </summary>
        public Circle3d(Circle3d circle)
            :this(new Point3d(circle.Center), circle.Radius, new Plane(circle.Plane))
		{
		}

		public Circle3d(Circle2d circle)
            : this(circle.Center, circle.Radius, circle.Plane)
        {
		}

		protected Circle3d(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _center = (Point3d)info.GetValue("Center", typeof(Point3d));
            _plane = (Plane)info.GetValue("Plane", typeof(Plane));
            _radius = info.GetDouble("R");
        }

        #endregion

        #region PUBLIC METHOD

        /// <summary>
        /// Get the normal vector of the circle
        /// </summary>
        /// <returns>The normal vector</returns>
        public Vector3d GetNormalVector()
        {
            return _plane.Normal;
        }

		/// <summary>
		/// Tell if <paramref name="point"/> is on the edge of the circle
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point is on the edge</returns>
		public bool IsPointOnCircle(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (_plane.IsPointOnPlane(point, tolerance))
            {
                double dist = Math.Sqrt(Math.Pow((point.X - Center.X), 2) + Math.Pow((point.Y - Center.Y), 2) + Math.Pow((point.Z - Center.Z), 2));
                double t = Math.Sqrt(Utilities.Maths.ErrorPropagation.ProductSquareTolerance(dist, Radius, tolerance, tolerance));
                if (Math.Abs(dist - Radius) < t)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

		/// <summary>
		/// Tell if <paramref name="point"/> is inside the circle
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point is inside</returns>
		public bool IsPointInside(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (_plane.IsPointOnPlane(point, tolerance))
            {
                if (IsPointOnCircleNotPlanarCheck(point, tolerance))
                    return true;
                if (point.DistanceTo(Center) < Radius)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

		/// <summary>
		/// Tell if <paramref name="point"/> is on the edge of the circle
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point is on the edge</returns>
		private bool IsPointOnCircleNotPlanarCheck(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            double dist = Math.Sqrt(Math.Pow((point.X - Center.X), 2) + Math.Pow((point.Y - Center.Y), 2) + Math.Pow((point.Z - Center.Z), 2));
            double t = Math.Sqrt(Utilities.Maths.ErrorPropagation.ProductSquareTolerance(dist, Radius, tolerance, tolerance));
            if (Math.Abs(dist - Radius) < t)
                return true;
            else
                return false;
        }

		/// <summary>
		/// Tell if <paramref name="line"/> is inside the circle
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point is inside</returns>
		public bool IsLineInside(Line3d line, double tolerance = GeometryBase.Tolerance)
        {
            if (IsPointInside(line.Start, tolerance) && IsPointInside(line.End, tolerance))
                return true;
            else
                return false;
        }

        protected void CalculateVaribles(Point3d p1, Point3d p2, Point3d p3, double tolerance = GeometryBase.Tolerance)
        {
			var v1 = new Vector3d(p1, p2);
			var v2 = new Vector3d(p1, p3);
			if (Math.Abs(v1.CrossProduct(v2).Norm()) < tolerance)
			{
				throw new Exception("Collinear points");
			}

			Plane plane = new Plane(p1, p2, p3, tolerance);     // piano di giacenza della circonferenza
			_plane = plane;

			// equazione costitutiva  :  (x-x0)^2 + (y-y0)^2 + (z-z0)^2 = r^2

			Vector3d v3 = v1.CrossProduct(v2);
			Vector3d v4 = v3.CrossProduct(v1);

			CoordinateSystem coordinateSystem = new CoordinateSystem(p1, v1, v4);

			Point3d a1 = coordinateSystem.ToLocal(p1);
			Point3d a2 = coordinateSystem.ToLocal(p2);
			Point3d a3 = coordinateSystem.ToLocal(p3);

			double d1 = Math.Pow(a1.X, 2) + Math.Pow(a1.Y, 2);
			double d2 = Math.Pow(a2.X, 2) + Math.Pow(a2.Y, 2);
			double d3 = Math.Pow(a3.X, 2) + Math.Pow(a3.Y, 2);
			double f = 2.0 * (a1.X * (a2.Y - a3.Y) - a1.Y * (a2.X - a3.X) + a2.X * a3.Y - a3.X * a2.Y);

			double X = (d1 * (a2.Y - a3.Y) + d2 * (a3.Y - a1.Y) + d3 * (a1.Y - a2.Y)) / f;
			double Y = (d1 * (a3.X - a2.X) + d2 * (a1.X - a3.X) + d3 * (a2.X - a1.X)) / f;

			Point3d centerInLocalCoord = new Point3d(X, Y, 0);
			_center = coordinateSystem.ToGlobal(centerInLocalCoord);  // centro
			_radius = Math.Sqrt((X - a1.X) * (X - a1.X) + (Y - a1.Y) * (Y - a1.Y));          // raggio
		}

		#endregion

		#region PUBLIC OPERATOR OVERRIDE

		public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _center.GetHashCode();
                hashCode = hashCode * -17 + _radius.GetHashCode();
                hashCode = hashCode * -17 + _plane.GetHashCode();

                return hashCode;
            }
        }

        public override void Move(double v1, double v2, double v3)
        {
            _center.Move(v1, v2, v3);
            _plane.P1.Move(v1, v2, v3);
            _plane.P2.Move(v1, v2, v3);
            _plane.P3.Move(v1, v2, v3);
        }

        public override void Move(Vector3d vector)
        {
            _center.Move(vector);
            _plane.P1.Move(vector);
            _plane.P2.Move(vector);
            _plane.P3.Move(vector);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Center", _center, typeof(Point3d));
            info.AddValue("Plane", _plane, typeof(Plane));
            info.AddValue("R", _radius, typeof(double));
        }

		public override object Clone()
		{
            return new Circle3d(this);
		}

        public override bool Equals(object obj)
        {
            if(obj is Circle3d circle)
            {
                return Equals(circle);
            }
            return false;
        }

		public bool Equals(Circle3d other)
		{
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._center.Equals(_center) && other._radius.Equals(_radius) && other._plane.Equals(_plane);
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Circle3d point)
                return Equals(point);

            return false;
        }

        #endregion
    }
}
