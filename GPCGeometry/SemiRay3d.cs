using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Geometry
{
	[Serializable]
	public class SemiRay3d : GeometryBase, ISerializable, ICloneable, IEquatable<SemiRay3d>
	{
		#region Variables

		private Point3d _point;
		private Vector3d _direction;

		#endregion

		#region Properties

		public Point3d Point { get => _point; set => _point = value; }

		public Vector3d Direction { get => _direction; set => _direction = value; }

		public Ray3d Ray => new Ray3d(_point, _direction);

		#endregion

		#region Constructors

		public SemiRay3d(Point3d point, Vector3d direction)
		{
			_point = point;
			_direction = direction;
		}

		public SemiRay3d(Point3d startPoint, Point3d endPoint)
		{
			_point = startPoint;
			_direction = endPoint - startPoint;
		}

		public SemiRay3d()
		{
			_point = Point3d.Origin;
			_direction = Vector3d.Zero;
		}

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Calculates the intersection of a segment and a semi-infinite line.
		/// </summary>
		/// <param name="SemiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
		/// <param name="inters">Point of intersection if any.</param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
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

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Direction", _direction);
			info.AddValue("Point", _point);
		}

		public override void Move(double v1, double v2, double v3)
		{
			_point.Move(v1, v2, v3);
		}

		public override void Move(Vector3d vector)
		{
			_point.Move(vector);
		}

		public override object Clone()
		{
			return new SemiRay3d(new Point3d(_point), new Vector3d(_direction));
		}

		public override bool Equals(object obj)
		{ 
			if (obj is SemiRay3d ray)
				return Equals(ray);

			return false;
		}

		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is SemiRay3d ray)
				return Equals(ray);

			return false;
		}

		public bool Equals(SemiRay3d other)
		{
			if (ReferenceEquals(this, other))
				return true;
			
			return !(other is null) && other._point.Equals(_point) && other._direction.Equals(_direction);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
