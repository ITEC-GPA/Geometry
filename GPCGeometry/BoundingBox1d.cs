using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// An interval [Min, Max] on a line: the one-dimensional bounding box. A new box is empty until the first value is added with
    /// <see cref="Update(double)"/>
    /// </summary>
    [Serializable]
    public class BoundingBox1d : ISerializable
    {
        #region Variables

        /// <summary>
        /// The lower end
        /// </summary>
        protected double _min;
        /// <summary>
        /// The upper end
        /// </summary>
        protected double _max;
        /// <summary>
        /// True if no value has been added
        /// </summary>
        protected bool _isEmpty;

        #endregion

        #region Properties

        /// <summary>
        /// The lower end of the interval
        /// </summary>
        public double Min { get => _min; set => _min = value; }

        /// <summary>
        /// The upper end of the interval
        /// </summary>
        public double Max { get => _max; set => _max = value; }

        /// <summary>
        /// True if no value has been added yet
        /// </summary>
        public bool IsEmpty => _isEmpty; 

        /// <summary>
        /// The length of the interval: Max - Min
        /// </summary>
        public double Size => _max - _min; 

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an empty interval
        /// </summary>
        public BoundingBox1d()
        {
            Reset();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BoundingBox1d(SerializationInfo info, StreamingContext context)
        {
            _min = info.GetDouble("Min");
            _max = info.GetDouble("Max");
            _isEmpty = info.GetBoolean("IsEmpty");            
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// The middle of the interval
        /// </summary>
        /// <returns>(Min + Max) / 2</returns>
        public double Center()
        {
            return _min + 0.5 * Size;
        }

        /// <summary>
        /// Extends the interval to include <paramref name="x"/> (the first value sets both ends)
        /// </summary>
        /// <param name="x">The value to include</param>
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

        /// <summary>
        /// Empties the interval
        /// </summary>
        public void Reset()
        {
            _min = 0;
            _max = 0;
            _isEmpty = true;
        }

        #endregion

        #region public static Methods

        /// <summary>
        /// The union of two intervals
        /// </summary>
        /// <param name="a">The first interval</param>
        /// <param name="b">The second interval</param>
        /// <returns>One interval if <paramref name="a"/> and <paramref name="b"/> overlap or touch, otherwise the two intervals</returns>
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

        /// <summary>
        /// The parts of <paramref name="a"/> outside <paramref name="b"/>
        /// </summary>
        /// <param name="a">The interval to subtract from</param>
        /// <param name="b">The interval to subtract</param>
        /// <param name="minLength">The minimum length of the parts: shorter parts are discarded</param>
        /// <returns><paramref name="a"/> if the intervals do not overlap; otherwise the parts of <paramref name="a"/> above and below <paramref name="b"/>
        /// not shorter than <paramref name="minLength"/>, null if there are none</returns>
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

        /// <summary>
        /// The intersections of every interval of <paramref name="a"/> with every interval of <paramref name="b"/>
        /// </summary>
        /// <param name="a">The first list of intervals</param>
        /// <param name="b">The second list of intervals</param>
        /// <param name="minLength">The minimum length of the intersections: shorter ones are discarded</param>
        /// <returns>The intersections, null if there are none</returns>
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

        /// <summary>
        /// The intersection of two intervals
        /// </summary>
        /// <param name="a">The first interval</param>
        /// <param name="b">The second interval</param>
        /// <param name="minLength">The minimum length of the intersection</param>
        /// <returns>The common part, null if it is shorter than <paramref name="minLength"/> (or the intervals do not overlap)</returns>
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

		/// <summary>
		/// Equality of the ends and of the empty state (exact comparison)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal interval</returns>
		public override bool Equals(object obj)
		{
            return obj is BoundingBox1d d &&
                   _min == d._min &&
                   _max == d._max &&
                   _isEmpty == d._isEmpty;
		}

        /// <summary>
        /// The hash code of the ends and of the empty state
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
        /// Serializes the interval
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Min", _min);
            info.AddValue("Max", _max);
            info.AddValue("IsEmpty", _isEmpty);
        }

        #endregion
    }
}