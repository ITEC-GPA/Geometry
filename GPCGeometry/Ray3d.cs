using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public class Ray3d : GeometryBase, ISerializable, ICloneable, IEquatable<Ray3d>
    {
        #region Variables

        private Point3d _point;
        private Vector3d _direction;

        #endregion

        #region Properties

        public Point3d Point { get => _point; set => _point = value; }

        public Vector3d Direction { get => _direction; set => _direction = value; }

        #endregion

        #region Constructors

        public Ray3d(Point3d point, Vector3d direction)
        {
            _point = point;
            _direction = direction;
        }

        public Ray3d(Point3d startPoint, Point3d endPoint)
        {
            _point = startPoint;
            _direction = endPoint - startPoint;
        }

        public Ray3d()
        {
            _point = Point3d.Origin;
            _direction = Vector3d.Zero;
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Tell if the given point is on the mathematical line
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="tolerance"></param>
        /// <returns>True if the point is on the mathematical line</returns>
        /// <exception cref="ArgumentNullException">Thrown when the point parameter is null</exception>
        public bool IsPointOnRay(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (point == null)
            {
                throw new ArgumentNullException("Point can not be null");
            }

            // http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
            // If the cross product of the vectors n1 and n2 is zero in all directions then the points are collinear, 
            // n1 and n2 are the vectors connecting one point to the other two points       

            var _start = Point;
            var _end = Point + Direction;

            double crossProductX = ((_end.Y - _start.Y) * (point.Z - _start.Z) - (point.Y - _start.Y) * (_end.Z - _start.Z));
            double crossProductY = ((point.X - _start.X) * (_end.Z - _start.Z) - (_end.X - _start.X) * (point.Z - _start.Z));
            double crossProductZ = ((_end.X - _start.X) * (point.Y - _start.Y) - (point.X - _start.X) * (_end.Y - _start.Y));

            double tol = Utilities.Maths.ErrorPropagation.ProductTolerance(crossProductX, crossProductY, crossProductZ, tolerance, tolerance, tolerance);

            if (Math.Abs(crossProductX) < tol && Math.Abs(crossProductY) < tol && Math.Abs(crossProductZ) < tol)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get intersection of ray with plane.
        /// </summary>
        /// <param name="s">The plane</param>
        /// <param name="intersection">Returns null is there is no intersection or the intersection point
        /// <param name="tolerance">The calculation tolerance</param>
        /// <returns>True if the instersection exist</returns>
        public bool IntersectionWith(Plane s, out Point3d intersection, double tolerance = GeometryBase.Tolerance)
        {
            intersection = null;

            Vector3d r1 = Point;
            Vector3d s1 = Direction;
            Vector3d n2 = s.Normal;

            if (Math.Abs(s1 * n2) < tolerance)
            {
                // Ray and plane are parallel
                if (s.IsPointOnPlane(Point, tolerance))
                {
                    // Ray lies in the plane
                    intersection = Point;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Intersection point
                Point3d pp = new Point3d(Point.X, Point.Y, Point.Z);
                //s.Origin = pp;

                r1 -= ((r1 * n2) + s.D) / (s1 * n2) * s1;

                Point3d rr = new Point3d(r1);

                if (rr.IsOnSemiInfiniteRay(new Line3d(_point, _point + _direction), out _, tolerance))
                {
                    intersection = rr;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// From http://paulbourke.net/geometry/pointlineplane/
        /// The shortest line between two rays in 3D.
        /// This object --> first ray.
        /// Parameter ray --> second ray.
        /// If they are parallel there is no solution and returns null for the points.
        /// </summary>
        /// <param name="ray">Second ray.</param>
        /// <param name="mua">Parameter for point Pa in first ray, this object.</param>
        /// <param name="mub">Parameter for point Pb in second ray.</param>
        /// <param name="Pa">First point of shortest line segment in first ray. If two rays are parallel return null.</param>
        /// <param name="Pb">Second point of shortest line segment in second ray. If two rays are parallel return null.</param>
        public void CalcShortestLineBetweenTwoRays(Ray3d ray, out double mua, out double mub, out Point3d Pa, out Point3d Pb)
        {
            // Pa = P1 + mua (P2 - P1)
            // Pb = P3 + mub (P4 - P3)

            var P1 = _point;
            var P2 = _point + _direction;
            var P3 = ray._point;
            var P4 = ray._point + ray._direction;

            var v31 = new Vector3d(P3, P1);
            var v34 = new Vector3d(P3, P4);
            var v12 = new Vector3d(P1, P2);

            double d1343 = v31 * v34;
            double d4321 = v34 * v12;
            double d1321 = v31 * v12;
            double d4343 = v34 * v34;
            double d2121 = v12 * v12;

            double denom = d2121 * d4343 - d4321 * d4321;

            if (denom == 0.0)
            {
                // the lines are almost parallel
                // use the starting point of the first ray
                mua = 0.0;
                mub = d4321 > d4343 ? d1321 / d4321 : d1343 / d4343; // use the largest denominator
            }
            else
            {
                mua = (d1343 * d4321 - d1321 * d4343) / denom;
                mub = (d1343 + mua * d4321) / d4343;
            }

            Pa = P1 + mua * v12;
            Pb = P3 + mub * v34;
        }

        /// <summary>
        /// From http://paulbourke.net/geometry/pointlineplane/
        /// The shortest line between two rays in 3D.
        /// This object --> first ray.
        /// Parameter ray --> second ray.
        /// If they are parallel there is no solution and returns null for the points.
        /// </summary>
        /// <param name="ray">Second ray.</param>
        /// <param name="mua">Parameter for point Pa in first ray, this object.</param>
        /// <param name="mub">Parameter for point Pb in second ray.</param>
        /// <param name="Pa">First point of shortest line segment in first ray. If two rays are parallel return null.</param>
        /// <param name="Pb">Second point of shortest line segment in second ray. If two rays are parallel return null.</param>
        public Line3d CalcShortestLineBetweenTwoRays(Ray3d ray)
        {
            // Pa = P1 + mua (P2 - P1)
            // Pb = P3 + mub (P4 - P3)

            var P1 = _point;
            var P2 = _point + _direction;
            var P3 = ray._point;
            var P4 = ray._point + ray._direction;

            var v31 = new Vector3d(P3, P1);
            var v34 = new Vector3d(P3, P4);
            var v12 = new Vector3d(P1, P2);

            double d1343 = v31 * v34;
            double d4321 = v34 * v12;
            double d1321 = v31 * v12;
            double d4343 = v34 * v34;
            double d2121 = v12 * v12;

            double denom = d2121 * d4343 - d4321 * d4321;

            double mua;
            double mub;

            if (denom == 0.0)
            {
                // the lines are almost parallel
                // use the starting point of the first ray
                mua = 0.0;
                mub = d4321 > d4343 ? d1321 / d4321 : d1343 / d4343; // use the largest denominator
            }
            else
            {
                mua = (d1343 * d4321 - d1321 * d4343) / denom;
                mub = (d1343 + mua * d4321) / d4343;
            }

            var Pa = P1 + mua * v12;
            var Pb = P3 + mub * v34;

            return new Line3d(Pa, Pb);
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
            return new Ray3d(_point, _direction);
        }

        public override bool Equals(object obj)
        {
            if (obj is Ray3d ray)
                return Equals(ray);

            return false;
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Ray3d ray)
                return Equals(ray);

            return false;
        }

        public bool Equals(Ray3d other)
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
