using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public class BoundingBox2d
    {
        #region Variables

        protected Point2d _min;
        protected Point2d _max;
        protected bool _isEmpty;

        #endregion

        #region Properties

        public Point2d Min { get => _min; set => _min = value; }

        public Point2d Max { get => _max; set => _max = value; }

        public bool IsEmpty => _isEmpty; 

        public Point2d Size => _max - _min;

        #endregion

        #region Public Constructors

        public BoundingBox2d()
        {
            Reset();
        }

        public BoundingBox2d(Polygon2d poly) 
            : this()
        {
            for(int i = 0; i < poly.Count; i++)
            {
                Update(poly[i]);
            }
        }

        protected BoundingBox2d(SerializationInfo info, StreamingContext context)
        {
            _min = (Point2d)info.GetValue("Min", typeof(Point2d));
            _max = (Point2d)info.GetValue("Max", typeof(Point2d));
            _isEmpty = info.GetBoolean("IsEmpty");
        }

        #endregion

        #region Public Methods Specific

        public void Update(Polygon2d poly)
        {
            for(int i = 0; i < poly.Count; i++)
            {
                Update(poly[i]);
            }
        }

        public void Update(Polygon3d poly)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Update(poly[i]);
            }
        }

        public void Update(Point2d p)
        {
            Update(p.X, p.Y);
        }

        public void Update(Point2d[] p)
        {
            for( int i = 0; i < p.Length; i++)
                Update(p[i].X, p[i].Y);
        }

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

        public void Reset()
        {
            _min = Point2d.Origin;
            _max = Point2d.Origin;
            _isEmpty = true;
        }

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

        public void Move(Vector2d Pan)
        {
            _min += Pan;
            _max += Pan;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return obj is BoundingBox2d d &&
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
    }
}