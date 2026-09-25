using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	/// <summary>
	/// An axis-aligned box that contains a set of points. A new box is empty until the first point is added with Update
	/// </summary>
	[Serializable]
	public class BoundingBox3d
	{
		#region Variables

		/// <summary>
		/// The corner with the minimum coordinates
		/// </summary>
		protected Point3d _min;
		/// <summary>
		/// The corner with the maximum coordinates
		/// </summary>
		protected Point3d _max;
		/// <summary>
		/// True if no point has been added
		/// </summary>
		protected bool _isEmpty;

		#endregion

		#region Properties

		/// <summary>
		/// The corner with the minimum coordinates
		/// </summary>
		public Point3d Min { get => _min; set => _min = value; }

		/// <summary>
		/// The corner with the maximum coordinates
		/// </summary>
		public Point3d Max { get => _max; set => _max = value; }

		/// <summary>
		/// True if no point has been added yet
		/// </summary>
		public bool IsEmpty => _isEmpty;

		/// <summary>
		/// The dimensions of the box along X, Y and Z (Max - Min)
		/// </summary>
		public Point3d Size => _max - _min;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates an empty box
		/// </summary>
		public BoundingBox3d()
		{
			Reset();
		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected BoundingBox3d(SerializationInfo info, StreamingContext context)
		{
			_min = (Point3d)info.GetValue("Min", typeof(Point3d));
			_max = (Point3d)info.GetValue("Max", typeof(Point3d));
			_isEmpty = info.GetBoolean("IsEmpty");
		}

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Extends the box to include a point
		/// </summary>
		/// <param name="p">The point</param>
		public void Update(Point3d p)
		{
			Update(p.X, p.Y, p.Z);
		}

		/// <summary>
		/// Extends the box to include the points
		/// </summary>
		/// <param name="p">The points</param>
		public void Update(Point3d[] p)
		{
			for (int i = 0; i < p.Length; i++)
				Update(p[i].X, p[i].Y, p[i].Z);
		}

		/// <summary>
		/// Extends the box to include the vertices of a polygon
		/// </summary>
		/// <param name="poly">The polygon</param>
		public void Update(Polygon3d poly)
		{
			for (int i = 0; i < poly.Count; i++)
			{
				Update(poly[i]);
			}
		}

		/// <summary>
		/// Extends the box to include the point (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>); the first point sets both
		/// corners
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <param name="z">The Z coordinate</param>
		public void Update(double x, double y, double z)
		{
			if (_isEmpty)
			{
				_min = new Point3d(x, y, z);
				_max = new Point3d(x, y, z);
			}
			else
			{
				if (x < _min.X)
				{
					_min.MoveTo(x, _min.Y, _min.Z);
				}
				else if (x > _max.X)
				{
					_max.MoveTo(x, _max.Y, _max.Z);
				}
				if (y < _min.Y)
				{
					_min.MoveTo(_min.X, y, _min.Z);
				}
				else if (y > _max.Y)
				{
					_max.MoveTo(_max.X, y, _max.Z);
				}
				if (z < _min.Z)
				{
					_min.MoveTo(_min.X, _min.Y, z);
				}
				else if (z > _max.Z)
				{
					_max.MoveTo(_max.X, _max.Y, z);
				}
			}
			_isEmpty = false;
		}

		/// <summary>
		/// Empties the box
		/// </summary>
		public void Reset()
		{
			_min = Point3d.Origin;
			_max = Point3d.Origin;
			_isEmpty = true;
		}

		#endregion

		/// <summary>
		/// Equality of the corners (within the tolerance of the points) and of the empty state
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal box</returns>
		public override bool Equals(object obj)
		{
			return obj is BoundingBox3d d &&
				   _min == d._min &&
				   _max == d._max &&
				   _isEmpty == d._isEmpty;
		}

		/// <summary>
		/// The hash code of the corners and of the empty state
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + _min.GetHashCode();
				hashCode = hashCode * -17 + _max.GetHashCode();
				hashCode = hashCode * -17 + _isEmpty.GetHashCode();

				return hashCode;
			}
		}

		/// <summary>
		/// Writes the box in the serialization data
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Min", _min, typeof(Point3d));
			info.AddValue("Max", _max, typeof(Point3d));
			info.AddValue("IsEmpty", _isEmpty, typeof(bool));
		}
	}
}