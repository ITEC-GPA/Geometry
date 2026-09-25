using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Geometry
{
	/// <summary>
	/// A semi-infinite line in the space: it starts at <see cref="Point"/> and goes in the direction <see cref="Direction"/>
	/// </summary>
	[Serializable]
	public class SemiRay3d : GeometryBase, ISerializable, ICloneable, IEquatable<SemiRay3d>
	{
		#region Variables

		/// <summary>
		/// The start point
		/// </summary>
		private Point3d _point;
		/// <summary>
		/// The direction
		/// </summary>
		private Vector3d _direction;

		#endregion

		#region Properties

		/// <summary>
		/// The start point (the instance is kept, not copied)
		/// </summary>
		public Point3d Point { get => _point; set => _point = value; }

		/// <summary>
		/// The direction (not necessarily unitary)
		/// </summary>
		public Vector3d Direction { get => _direction; set => _direction = value; }

		/// <summary>
		/// A new infinite line with the same point and direction
		/// </summary>
		public Ray3d Ray => new Ray3d(_point, _direction);

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a semi-infinite line from a point with a direction (the instances are kept, not copied)
		/// </summary>
		/// <param name="point">The start point</param>
		/// <param name="direction">The direction</param>
		public SemiRay3d(Point3d point, Vector3d direction)
		{
			_point = point;
			_direction = direction;
		}

		/// <summary>
		/// Creates the semi-infinite line from a point through another point
		/// </summary>
		/// <param name="startPoint">The start point (the instance is kept)</param>
		/// <param name="endPoint">A point of the line: the direction is <paramref name="endPoint"/> - <paramref name="startPoint"/></param>
		public SemiRay3d(Point3d startPoint, Point3d endPoint)
		{
			_point = startPoint;
			_direction = endPoint - startPoint;
		}

		/// <summary>
		/// Creates a degenerate line: point at the origin and zero direction
		/// </summary>
		public SemiRay3d()
		{
			_point = Point3d.Origin;
			_direction = Vector3d.Zero;
		}

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Calculates the intersection of the line through this semi-infinite line with another semi-infinite line
		/// </summary>
		/// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
		/// <param name="inters">Point of intersection if any.</param>
		/// <param name="tolerance">The tolerance on the distance between the lines</param>
		/// <returns>True if the lines meet in front of the start of <paramref name="SemiRay"/> (the side of the start of this line is not checked)</returns>
		public bool GetIntersectionWihtSemiRay(in SemiRay3d SemiRay, out Point3d inters, double tolerance = GeometryBase.Tolerance)
		{
			inters = null;
			Ray.CalcShortestLineBetweenTwoRays(SemiRay.Ray, out _, out double mub, out Point3d Pa, out Point3d Pb);

			if (Pa == null || Pb == null)
				return false;

			if (mub > 0 && Pa.DistanceTo(Pb) < tolerance)
			{
				inters = Pb;
				return true;
			}
			else
				return false;
		}

		#endregion

		#region Public Methods Override

		/// <summary>
		/// Serializes the line: the <see cref="BaseObject.Guid"/>, the direction and the point (there is no deserialization constructor)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Direction", _direction);
			info.AddValue("Point", _point);
		}

		/// <summary>
		/// Translates the line (its start point)
		/// </summary>
		/// <param name="v1">The translation along X</param>
		/// <param name="v2">The translation along Y</param>
		/// <param name="v3">The translation along Z</param>
		public override void Move(double v1, double v2, double v3)
		{
			_point.Move(v1, v2, v3);
		}

		/// <summary>
		/// Translates the line (its start point)
		/// </summary>
		/// <param name="vector">The translation</param>
		public override void Move(Vector3d vector)
		{
			_point.Move(vector);
		}

		/// <summary>
		/// Creates a copy of the line, with copies of its point and direction
		/// </summary>
		/// <returns>The copy</returns>
		public override object Clone()
		{
			return new SemiRay3d(new Point3d(_point), new Vector3d(_direction));
		}

		/// <summary>
		/// Equality with another object (see <see cref="Equals(SemiRay3d)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal line</returns>
		public override bool Equals(object obj)
		{ 
			if (obj is SemiRay3d ray)
				return Equals(ray);

			return false;
		}

		/// <summary>
		/// Equality with another geometry (see <see cref="Equals(SemiRay3d)"/>)
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal line</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is SemiRay3d ray)
				return Equals(ray);

			return false;
		}

		/// <summary>
		/// Equality of the start point and of the direction within the tolerance
		/// </summary>
		/// <param name="other">The line to compare</param>
		/// <returns>True if the points and the directions are equal</returns>
		public bool Equals(SemiRay3d other)
		{
			if (ReferenceEquals(this, other))
				return true;
			
			return !(other is null) && other._point.Equals(_point) && other._direction.Equals(_direction);
		}

		/// <summary>
		/// A constant hash code (see <see cref="GeometryBase.GetHashCode"/>)
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
