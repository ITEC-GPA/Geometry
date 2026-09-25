using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// An axis-aligned rectangle in the XY plane that contains a set of points. A new box is empty until the first point is added with Update
    /// </summary>
    [Serializable]
    public class BoundingBox2d
    {
        #region Variables

        /// <summary>
        /// The corner with the minimum coordinates
        /// </summary>
        protected Point2d _min;
        /// <summary>
        /// The corner with the maximum coordinates
        /// </summary>
        protected Point2d _max;
        /// <summary>
        /// True if no point has been added
        /// </summary>
        protected bool _isEmpty;

        #endregion

        #region Properties

        /// <summary>
        /// The corner with the minimum coordinates
        /// </summary>
        public Point2d Min { get => _min; set => _min = value; }

        /// <summary>
        /// The corner with the maximum coordinates
        /// </summary>
        public Point2d Max { get => _max; set => _max = value; }

        /// <summary>
        /// True if no point has been added yet
        /// </summary>
        public bool IsEmpty => _isEmpty; 

        /// <summary>
        /// The dimensions of the box along X and Y (Max - Min)
        /// </summary>
        public Point2d Size => _max - _min;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an empty box
        /// </summary>
        public BoundingBox2d()
        {
            Reset();
        }

        /// <summary>
        /// Creates the box of the vertices of a polygon
        /// </summary>
        /// <param name="poly">The polygon</param>
        public BoundingBox2d(Polygon2d poly) 
            : this()
        {
            for(int i = 0; i < poly.Count; i++)
            {
                Update(poly[i]);
            }
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BoundingBox2d(SerializationInfo info, StreamingContext context)
        {
            _min = (Point2d)info.GetValue("Min", typeof(Point2d));
            _max = (Point2d)info.GetValue("Max", typeof(Point2d));
            _isEmpty = info.GetBoolean("IsEmpty");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Extends the box to include the vertices of a polygon
        /// </summary>
        /// <param name="poly">The polygon</param>
        public void Update(Polygon2d poly)
        {
            for(int i = 0; i < poly.Count; i++)
            {
                Update(poly[i]);
            }
        }

        /// <summary>
        /// Extends the box to include the vertices of a polygon, projected on the XY plane (Z is ignored)
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
        /// Extends the box to include a point
        /// </summary>
        /// <param name="p">The point</param>
        public void Update(Point2d p)
        {
            Update(p.X, p.Y);
        }

        /// <summary>
        /// Extends the box to include the points
        /// </summary>
        /// <param name="p">The points</param>
        public void Update(Point2d[] p)
        {
            for( int i = 0; i < p.Length; i++)
                Update(p[i].X, p[i].Y);
        }

        /// <summary>
        /// Extends the box to include the point (<paramref name="x"/>, <paramref name="y"/>); the first point sets both corners
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void Update(double x, double y)
        {
            if (_isEmpty)
            {
                _min = new Point2d(x, y);
                _max = new Point2d(x, y);
            }
            else
            {
                if (x < _min.X)
                {
                    _min.MoveTo(x, _min.Y);
                }
                else if (x > _max.X)
                {
                    _max.MoveTo(x, _max.Y);
                }
                if (y < _min.Y)
                {
                    _min.MoveTo(_min.X, y);
                }
                else if (y > _max.Y)
                {
                    _max.MoveTo(_max.X, y);
                }
            }
            _isEmpty = false;
        }

        /// <summary>
        /// Empties the box
        /// </summary>
        public void Reset()
        {
            _min = Point2d.Origin;
            _max = Point2d.Origin;
            _isEmpty = true;
        }

        /// <summary>
        /// Scales the box respect to its center (not respect to the origin)
        /// </summary>
        /// <param name="Scale">The scale factor of the dimensions</param>
        public void Scale(double Scale)
        {
            double b = Math.Abs(_max.X - _min.X);                 // Scala la buonding box rispetto al suo centro
            double h = Math.Abs(_max.Y - _min.Y);                 // e non rispetto allo'origine (0,0)                        
            double dx = b * Scale - b;
            double dy = h * Scale - h;

            Point2d Delta = new Point2d(dx/2, dy/2);    // Ri-centro la bounding box

            _max += Delta;
            _min -= Delta;
        }

        /// <summary>
        /// Translates the box
        /// </summary>
        /// <param name="Pan">The translation vector</param>
        public void Move(Vector2d Pan)
        {
            _min += Pan;
            _max += Pan;
        }

        #endregion

        /// <summary>
        /// Equality of the corners (within the tolerance of the points) and of the empty state
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal box</returns>
        public override bool Equals(object obj)
        {
            return obj is BoundingBox2d d &&
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
            info.AddValue("Min", _min);
            info.AddValue("Max", _max);
            info.AddValue("IsEmpty", _isEmpty);
        }
    }
}