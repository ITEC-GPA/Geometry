using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	/// <summary>
	/// Circle 2d is a planar circle on the (x,y) space
	/// </summary>
	[Serializable]
	public class Circle2d : GeometryBase, ISerializable, ICloneable, IEquatable<Circle2d>
	{
		#region Variables

		/// <summary>
		/// The center
		/// </summary>
		protected Point2d _center;
		/// <summary>
		/// The radius
		/// </summary>
		protected double _r;

		#endregion

		#region Properties

		/// <summary>
		/// Center of the circle
		/// </summary>
		public Point2d Center { get => _center; set => _center = value; }

		/// <summary>
		/// Radius of the circle
		/// </summary>
		public double Radius { get { return _r; } set { _r = value; } }

		/// <summary>
		/// Plane of the circle
		/// </summary>
		public Plane Plane => new Plane(Point2d.Origin, Vector2d.XAxis, Vector2d.YAxis);

		/// <summary>
		/// Diameter of the circle
		/// </summary>
		public double Diameter => 2.0 * Radius;

		/// <summary>
		/// Perimeter of the circle
		/// </summary>
		public double Perimeter
		{
			get { return 2 * Math.PI * _r; }
		}

		/// <summary>
		/// Area of the circle
		/// </summary>
		public double Area
		{
			get { return Math.PI * Math.Pow(_r, 2); }
		}

		#endregion

		#region Public Constructor

		/// <summary>
		/// Initializes the circle with center and radius
		/// </summary>
		/// <param name="Center">The center (the instance is kept, not copied)</param>
		/// <param name="Radius">The radius</param>
		public Circle2d(Point2d Center, double Radius)
		{
			_center = Center;
			_r = Radius;
		}

		/// <summary>
		/// Initializes the circle passing through three points (the circumscribed circle of the triangle)
		/// </summary>
		/// <param name="a1">The first point</param>
		/// <param name="a2">The second point</param>
		/// <param name="a3">The third point (the three points must not be aligned)</param>
		public Circle2d(Point2d a1, Point2d a2, Point2d a3)
		{
			double d1 = Math.Pow(a1.X, 2) + Math.Pow(a1.Y, 2);
			double d2 = Math.Pow(a2.X, 2) + Math.Pow(a2.Y, 2);
			double d3 = Math.Pow(a3.X, 2) + Math.Pow(a3.Y, 2);
			double f = 2.0 * (a1.X * (a2.Y - a3.Y) - a1.Y * (a2.X - a3.X) + a2.X * a3.Y - a3.X * a2.Y);

			double X = (d1 * (a2.Y - a3.Y) + d2 * (a3.Y - a1.Y) + d3 * (a1.Y - a2.Y)) / f;
			double Y = (d1 * (a3.X - a2.X) + d2 * (a1.X - a3.X) + d3 * (a2.X - a1.X)) / f;

			_center = new Point2d(X, Y);
			_r = Math.Sqrt((X - a1.X) * (X - a1.X) + (Y - a1.Y) * (Y - a1.Y));          // raggio
		}

		/// <summary>
		/// Creates a copy of a circle
		/// </summary>
		/// <param name="circle">The circle to copy</param>
		public Circle2d(Circle2d circle)
			: this(circle.Center, circle.Radius)
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		private Circle2d(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_center = (Point2d)info.GetValue("Center", typeof(Point2d));
			_r = info.GetDouble("R");
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Get the normal vector of the circle
		/// </summary>
		/// <returns>The normal vector</returns>
		public Vector3d GetNormalVector()
		{
			return Vector3d.ZAxis;
		}

		/// <summary>
		/// Tell if <paramref name="point"/> is on the edge of the circle
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance">The tolerance on the distance from the plane and from the circumference</param>
		/// <returns>True if the point is on the XY plane and on the circumference</returns>
		public bool IsPointOnCircle(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			if (Plane.IsPointOnPlane(point, tolerance))
			{
				double dist = Math.Sqrt(Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2));
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
		/// <param name="tolerance">The tolerance on the distance from the plane and from the circumference</param>
		/// <returns>True if the point is on the XY plane and inside the circle or on its edge</returns>
		public bool IsPointInside(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			if (Plane.IsPointOnPlane(point, tolerance))
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
		/// Tell if <paramref name="point"/> is on the edge of the circle, without checking that it is on the XY plane
		/// </summary>
		/// <param name="point">Point to test</param>
		/// <param name="tolerance">The tolerance on the distance from the circumference</param>
		/// <returns>True if the point is on the edge</returns>
		private bool IsPointOnCircleNotPlanarCheck(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			double dist = Math.Sqrt(Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2));
			double t = Math.Sqrt(Utilities.Maths.ErrorPropagation.ProductSquareTolerance(dist, Radius, tolerance, tolerance));
			if (Math.Abs(dist - Radius) < t)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Tell if <paramref name="line"/> is inside the circle
		/// </summary>
		/// <param name="line">Line to test</param>
		/// <param name="tolerance">The tolerance on the distances</param>
		/// <returns>True if both the ends are inside the circle or on its edge (the circle is convex: then the whole segment is inside)</returns>
		public bool IsLineInside(Line3d line, double tolerance = GeometryBase.Tolerance)
		{
			if (IsPointInside(line.Start, tolerance) && IsPointInside(line.End, tolerance))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Convert the circle in a regular polygon with <paramref name="numberOfEdges"/> edges, inscribed in the circle
		/// </summary>
		/// <param name="numberOfEdges">The number of edges</param>
		/// <returns>The polygon</returns>
		public Polygon2d ConvertToPolygon(int numberOfEdges = 32)
		{			
			return new Polygon2d(Radius, numberOfEdges, Center);
		}

		#endregion

		#region Public Operator

		/// <summary>
		/// Equality with another object (see <see cref="Equals(Circle2d)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal circle</returns>
		public override bool Equals(object obj)
		{
			if (obj is Circle2d circle)
				return Equals(circle);

			return false;
		}

		/// <summary>
		/// The hash code of the center and of the radius
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _center.GetHashCode();
				hashCode = hashCode * -17 + _r.GetHashCode();

				return hashCode;
			}
		}

		/// <summary>
		/// Translates the circle
		/// </summary>
		/// <param name="v1">The translation along X</param>
		/// <param name="v2">The translation along Y</param>
		/// <param name="v3">Ignored: the circle is in the XY plane</param>
		public override void Move(double v1, double v2, double v3)
		{
			_center.Move(v1, v2, v3);
		}

		/// <summary>
		/// Translates the circle (the Z component is ignored)
		/// </summary>
		/// <param name="vector">The translation</param>
		public override void Move(Vector3d vector)
		{
			_center.Move(vector);
		}

		/// <summary>
		/// Serializes the circle: the <see cref="BaseObject.Guid"/>, the center and the radius
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Center", _center, typeof(Point2d));
			info.AddValue("R", _r, typeof(double));
		}

		/// <summary>
		/// Creates a copy of the circle
		/// </summary>
		/// <returns>The copy</returns>
		public override object Clone()
		{
			return new Circle2d(this);
		}

		/// <summary>
		/// Equality: the centers within the tolerance and the same radius (exact comparison)
		/// </summary>
		/// <param name="other">The circle to compare</param>
		/// <returns>True if the circles are equal</returns>
		public bool Equals(Circle2d other)
		{
			if (ReferenceEquals(this, other))
				return true;

			return !(other is null) && other._center.Equals(_center) && other._r.Equals(_r);
		}

		/// <summary>
		/// Equality with another geometry (see <see cref="Equals(Circle2d)"/>)
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal circle</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Circle2d point)
				return Equals(point);

			return false;
		}

		#endregion
	}
}
