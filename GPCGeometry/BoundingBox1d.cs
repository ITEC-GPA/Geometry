using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public class BoundingBox1d : ISerializable
    {
        #region Variables

        protected double _min;
        protected double _max;
        protected bool _isEmpty;

        #endregion

        #region Properties

        public double Min { get => _min; set => _min = value; }

        public double Max { get => _max; set => _max = value; }

        public bool IsEmpty => _isEmpty; 

        public double Size => _max - _min; 

        #endregion

        #region Public Constructors

        public BoundingBox1d()
        {
            Reset();
        }

        protected BoundingBox1d(SerializationInfo info, StreamingContext context)
        {
            _min = info.GetDouble("Min");
            _max = info.GetDouble("Max");
            _isEmpty = info.GetBoolean("IsEmpty");            
        }

        #endregion

        #region Public Methods Specific

        public double Center()
        {
            return _min + 0.5 * Size;
        }

        public void Update(double x)
        {
            if (_isEmpty)
            {
                _min = x;
                _max = x;
            }
            else
            {
                if (x < _min)
                {
                    _min = x;
                }
                else if (x > _max)
                {
                    _max = x;
                }
            }
            _isEmpty = false;
        }

        public void Reset()
        {
            _min = 0;
            _max = 0;
            _isEmpty = true;
        }

        #endregion

        #region public static Methods

        public static List<BoundingBox1d> GetUnion(BoundingBox1d a, BoundingBox1d b)
        {
			BoundingBox1d buffer = GetIntersection(a, b, 0);
			if (buffer != null)
            {
                buffer = new BoundingBox1d();
                buffer.Update(Math.Min(a.Min, b.Min));
                buffer.Update(Math.Max(a.Max, b.Max));
                return new List<BoundingBox1d>() { buffer };
            }
            else
            {
                return new List<BoundingBox1d>() { a, b };
            }
        }

        public static List<BoundingBox1d> GetDifference(BoundingBox1d a, BoundingBox1d b, double minLength)
        {
			BoundingBox1d buffer = GetIntersection(a, b, 0);
			if (buffer != null)
            {
				List<BoundingBox1d> result = null;
				if (b.Max <= a.Max - minLength)
                {
                    if (result == null)
                    {
                        result = new List<BoundingBox1d>();
                    }
                    buffer = new BoundingBox1d();
                    buffer.Update(b.Max);
                    buffer.Update(a.Max);
                    result.Add(buffer);
                }
                if (b.Min >= a.Min + minLength)
                {
                    if (result == null)
                    {
                        result = new List<BoundingBox1d>();
                    }
                    buffer = new BoundingBox1d();
                    buffer.Update(b.Min);
                    buffer.Update(a.Min);
                    result.Add(buffer);
                }
                return result;
            }
            else
            {
                return new List<BoundingBox1d>() { a };
            }
        }

        public static List<BoundingBox1d> GetIntersection(List<BoundingBox1d> a, List<BoundingBox1d> b, double minLength)
        {
			List<BoundingBox1d> result = null;

			for (int i = 0; i < a.Count; i++)
            {
				BoundingBox1d aBBox = a[i];
				for (int j = 0; j < b.Count; j++)
                {
					BoundingBox1d bBbox = b[j];
					BoundingBox1d inters = GetIntersection(aBBox, bBbox, minLength);
					if (inters != null)
                    {
                        if (result == null)
                        {
                            result = new List<BoundingBox1d>();
                        }
                        result.Add(inters);
                    }
                }
            }
            return result;
        }

        public static BoundingBox1d GetIntersection(BoundingBox1d a, BoundingBox1d b, double minLength)
        {
            double maxOfMin = Math.Max(a.Min, b.Min);
            double minOfMax = Math.Min(a.Max, b.Max);
            if (minOfMax - minLength >= maxOfMin)
            {
                BoundingBox1d result;
                result = new BoundingBox1d();
                result.Update(maxOfMin);
                result.Update(minOfMax);
                return result;
            }
            else
            {
                return null;
            }
        }

		public override bool Equals(object obj)
		{
            return obj is BoundingBox1d d &&
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
            info.AddValue("Min", _min);
            info.AddValue("Max", _max);
            info.AddValue("IsEmpty", _isEmpty);
        }

        #endregion
    }
}