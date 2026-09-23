using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	[Serializable]
	public class BoundingBox3d
	{
		#region Variables

		protected Point3d _min;
		protected Point3d _max;
		protected bool _isEmpty;

		#endregion

		#region Properties

		public Point3d Min { get => _min; set => _min = value; }

		public Point3d Max { get => _max; set => _max = value; }

		public bool IsEmpty => _isEmpty;

		public Point3d Size => _max - _min;

		#endregion

		#region Public Constructors

		public BoundingBox3d()
		{
			Reset();
		}

		protected BoundingBox3d(SerializationInfo info, StreamingContext context)
		{
			_min = (Point3d)info.GetValue("Min", typeof(Point3d));
			_max = (Point3d)info.GetValue("Max", typeof(Point3d));
			_isEmpty = info.GetBoolean("IsEmpty");
		}

		#endregion

		#region Public Methods Specific

		public void Update(Point3d p)
		{
			Update(p.X, p.Y, p.Z);
		}

		public void Update(Point3d[] p)
		{
			for (int i = 0; i < p.Length; i++)
				Update(p[i].X, p[i].Y, p[i].Z);
		}

		public void Update(Polygon3d poly)
		{
			for (int i = 0; i < poly.Count; i++)
			{
				Update(poly[i]);
			}
		}

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

		public void Reset()
		{
			_min = Point3d.Origin;
			_max = Point3d.Origin;
			_isEmpty = true;
		}

		#endregion

		public override bool Equals(object obj)
		{
			return obj is BoundingBox3d d &&
				   _min == d._min &&
				   _max == d._max &&
				   _isEmpty == d._isEmpty;
		}

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

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Min", _min, typeof(Point3d));
			info.AddValue("Max", _max, typeof(Point3d));
			info.AddValue("IsEmpty", _isEmpty, typeof(bool));
		}
	}
}