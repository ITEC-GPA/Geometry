using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GPC.Geometry
{
	/// <summary>
	/// An arc of circle in the space, from a start point to an end point. It is defined by start, end and center, or by start, a point of
	/// passage and end (then the center is computed); <see cref="Passage"/> is null in the first case
	/// </summary>
	[Serializable]
	public class Circle3dArc : GeometryBase // ISerializable, ICloneable, IEquatable<Circle3d>
	{
		#region VARIABLES

		/// <summary>
		/// The start point
		/// </summary>
		private Point3d _start;
		/// <summary>
		/// A point of the arc between start and end (null for the arcs created from the center)
		/// </summary>
		private Point3d _passage;
		/// <summary>
		/// The end point
		/// </summary>
		private Point3d _end;
		/// <summary>
		/// The center
		/// </summary>
		private Point3d _center;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Start point of the arc of circle
		/// </summary>
		public Point3d Start { get => _start; set => _start = value; }

		/// <summary>
		/// Point of passage of arc
		/// </summary>
		public Point3d Passage { get => _passage; set => _passage = value; }

		/// <summary>
		/// End point of the arc of circle
		/// </summary>
		public Point3d End { get => _end; set => _end = value; }

		/// <summary>
		/// Center point of the arc of circle
		/// </summary>
		public Point3d Center { get => _center; set => _center = value; }

		#endregion

		#region PUBLIC CONSTRUCTOR

		/// <summary>
		/// Initializes the arc with start point, end point and center point (<see cref="Passage"/> is null)
		/// </summary>
		/// <param name="start">The start point</param>
		/// <param name="end">The end point</param>
		/// <param name="center">The center of the circle</param>
		public Circle3dArc(Point3d start, Point3d end, Point3d center)
		{
			_center = center;
			_start = start;
			_end = end;
		}

		/// <summary>
		/// Creates an arc with the start, the end and the center of another arc (the point instances are shared and the point of passage is not
		/// copied)
		/// </summary>
		/// <param name="arc">The arc to copy</param>
		public Circle3dArc(Circle3dArc arc)
			: this(arc.Start, arc.End, arc.Center)
		{

		}

		/// <summary>
		/// Initializes the arc passing through three points: the center is the one of the circle through them
		/// </summary>
		/// <param name="start">The start point</param>
		/// <param name="passage">A point of the arc between start and end</param>
		/// <param name="end">The end point</param>
		/// <param name="tolerance">The tolerance to recognize aligned points</param>
		/// <exception cref="Exception">If the points are aligned</exception>
		public Circle3dArc(Point3d start, Point3d passage, Point3d end, double tolerance = GeometryBase.Tolerance)
		{
			var v1 = new Vector3d(start, passage);
			var v2 = new Vector3d(start, end);
			if (Math.Abs(v1.CrossProduct(v2).Norm()) < tolerance)
			{
				throw new Exception("Collinear points");
			}
			// equazione costitutiva circonferenza :  (x-x0)^2 + (y-y0)^2 + (z-z0)^2 = r^2

			Vector3d v3 = v1.CrossProduct(v2);
			Vector3d v4 = v3.CrossProduct(v1);

			CoordinateSystem coordinateSystem = new CoordinateSystem(start, v1, v4);

			Point3d a1 = coordinateSystem.ToLocal(start);
			Point3d a2 = coordinateSystem.ToLocal(passage);
			Point3d a3 = coordinateSystem.ToLocal(end);

			double d1 = Math.Pow(a1.X, 2) + Math.Pow(a1.Y, 2);
			double d2 = Math.Pow(a2.X, 2) + Math.Pow(a2.Y, 2);
			double d3 = Math.Pow(a3.X, 2) + Math.Pow(a3.Y, 2);
			double f = 2.0 * (a1.X * (a2.Y - a3.Y) - a1.Y * (a2.X - a3.X) + a2.X * a3.Y - a3.X * a2.Y);

			double X = (d1 * (a2.Y - a3.Y) + d2 * (a3.Y - a1.Y) + d3 * (a1.Y - a2.Y)) / f;
			double Y = (d1 * (a3.X - a2.X) + d2 * (a1.X - a3.X) + d3 * (a2.X - a1.X)) / f;

			Point3d centerInLocalCoord = new Point3d(X, Y, 0);
			_center = coordinateSystem.ToGlobal(centerInLocalCoord);                    // centro
			_start = start;
			_end = end;
			_passage = passage;
		}

		#endregion

		#region PUBLIC METHOD

		/// <summary>
		/// Get the angle at center of the arc
		/// </summary>
		/// <returns>The angle between the radii to the start and to the end point, in radians, from 0 to pi: the arcs larger than a half circle
		/// are not recognized</returns>
		public double GetAngle()
		{
			Vector3d v1 = new Vector3d(Start - Center);
			Vector3d v2 = new Vector3d(End - Center);

			return v1.AngleTo(v2);
		}


		/// <summary>
		/// Get the normal vector of the plane of the arc
		/// </summary>
		/// <returns>The normal of the plane through start, center and end</returns>
		public Vector3d GetNormalVector()
		{
			Plane plane = new Plane(Start, Center, End);
			return plane.Normal;
		}

		/// <summary>
		/// Length of the arc: radius * angle at center
		/// </summary>
		/// <returns>The length</returns>
		/// <remarks><see cref="GetAngle"/> is in the range [0, PI], so arcs larger than a half circle are not supported</remarks>
		public double GetLenght()
		{
			double raggio = Start.DistanceTo(Center);

			return raggio * GetAngle();
		}

		/// <summary>
		/// Check if the point is on the arc
		/// </summary>
		/// <param name="point">The point to test</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>True if the point is one of the three points of the arc, or it is on the plane and on the circle with an angle from the start
		/// smaller than the angle of the arc. It needs the point of passage (arcs created with the three points)</returns>
		public bool IsPointOnCircleArc(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			Plane plane = new Plane(Start, End, Passage, tolerance);
			double raggio = Start.DistanceTo(Center);
			double dist1 = point.DistanceTo(Start);
			double dist2 = point.DistanceTo(End);
			double dist3 = point.DistanceTo(Passage);
			double tt = Utilities.Maths.ErrorPropagation.ProductSquareTolerance(dist1, dist2, tolerance, tolerance);

			if (dist1 < tt || dist2 < tt || dist3 < tt)
				return true;

			if (plane.IsPointOnPlane(point, tolerance))
			{
				double dist = Math.Sqrt(Math.Pow((point.X - Center.X), 2) + Math.Pow((point.Y - Center.Y), 2) + Math.Pow((point.Z - Center.Z), 2));
				double t = Math.Sqrt(Utilities.Maths.ErrorPropagation.ProductSquareTolerance(dist, raggio, tolerance, tolerance));
				if (Math.Abs(dist - raggio) < t)
				{
					Vector3d v = new Vector3d(point - Center);
					Vector3d v1 = new Vector3d(Start - Center);
					double angle = v1.AngleTo(v);

					if (angle < GetAngle())
						return true;
					else
						return false;
				}
				else
					return false;
			}
			else
				return false;
		}

		#endregion

		#region PUBLIC OPERATOR OVERRIDE

		/// <summary>
		/// The hash code of start, end and center
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _start.GetHashCode();
				hashCode = hashCode * -17 + _end.GetHashCode();
				hashCode = hashCode * -17 + _center.GetHashCode();

				return hashCode;
			}
		}

		/// <summary>
		/// Translates start, end and center of the arc (the point of passage is not moved)
		/// </summary>
		/// <param name="v1">The translation along X</param>
		/// <param name="v2">The translation along Y</param>
		/// <param name="v3">The translation along Z</param>
		public override void Move(double v1, double v2, double v3)
		{
			_start.Move(v1, v2, v3);
			_end.Move(v1, v2, v3);
			_center.Move(v1, v2, v3);
		}

		/// <summary>
		/// Translates start, end and center of the arc (the point of passage is not moved)
		/// </summary>
		/// <param name="vector">The translation</param>
		public override void Move(Vector3d vector)
		{
			_center.Move(vector);
			_end.Move(vector);
			_start.Move(vector);
		}

		/// <summary>
		/// Creates an arc with the start, the end and the center of this one (see the copy constructor)
		/// </summary>
		/// <returns>The new arc</returns>
		public override object Clone()
		{
			return new Circle3dArc(this);
		}

		/// <summary>
		/// Equality with another object (see <see cref="Equals(Circle3dArc)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal arc</returns>
		public override bool Equals(object obj)
		{
			if (obj is Circle3dArc arc)
				return Equals(arc);

			return false;
		}

		/// <summary>
		/// Equality with another geometry (see <see cref="Equals(Circle3dArc)"/>)
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal arc</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Circle3dArc point)
				return Equals(point);

			return false;
		}

		/// <summary>
		/// Equality of start, end, center and point of passage within the tolerance
		/// </summary>
		/// <param name="circle3DArc">The arc to compare</param>
		/// <returns>True if the arcs are equal</returns>
		public bool Equals(Circle3dArc circle3DArc)
		{
			if (circle3DArc is null)
				return false;

			if (ReferenceEquals(this, circle3DArc))
				return true;

			// _passage is null when the arc is created by start, end and center
			bool passageEquals = _passage is null ? circle3DArc.Passage is null : _passage.Equals(circle3DArc.Passage);

			return passageEquals &&
				_center.Equals(circle3DArc.Center) &&
				_end.Equals(circle3DArc.End) &&
				_start.Equals(circle3DArc.Start);
		}

		#endregion
	}
}
