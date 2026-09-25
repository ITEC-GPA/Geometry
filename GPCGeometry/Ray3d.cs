using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// An infinite line in the space through <see cref="Point"/> with direction <see cref="Direction"/>
    /// </summary>
    [Serializable]
    public class Ray3d : GeometryBase, ISerializable, ICloneable, IEquatable<Ray3d>
    {
        #region Variables

        /// <summary>
        /// A point of the line
        /// </summary>
        private Point3d _point;
        /// <summary>
        /// The direction of the line
        /// </summary>
        private Vector3d _direction;

        #endregion

        #region Properties

        /// <summary>
        /// A point of the line (the instance is kept, not copied)
        /// </summary>
        public Point3d Point { get => _point; set => _point = value; }

        /// <summary>
        /// The direction of the line (not necessarily unitary)
        /// </summary>
        public Vector3d Direction { get => _direction; set => _direction = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a line through a point with a direction (the instances are kept, not copied)
        /// </summary>
        /// <param name="point">A point of the line</param>
        /// <param name="direction">The direction</param>
        public Ray3d(Point3d point, Vector3d direction)
        {
            _point = point;
            _direction = direction;
        }

        /// <summary>
        /// Creates the line through two points
        /// </summary>
        /// <param name="startPoint">The first point (the instance is kept)</param>
        /// <param name="endPoint">The second point: the direction is <paramref name="endPoint"/> - <paramref name="startPoint"/></param>
        public Ray3d(Point3d startPoint, Point3d endPoint)
        {
            _point = startPoint;
            _direction = endPoint - startPoint;
        }

        /// <summary>
        /// Creates a degenerate line: point at the origin and zero direction
        /// </summary>
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
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <returns>True if the distance of the point from the line is lower than <paramref name="tolerance"/> (from <see cref="Point"/> if the
        /// direction is zero)</returns>
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

            // The point is on the line if its distance from the line is lower than the tolerance.
            // Distance = |d x (P - P0)| / |d|, independent from the length of the direction vector

            double dx = Direction.X;
            double dy = Direction.Y;
            double dz = Direction.Z;
            double squareLength = dx * dx + dy * dy + dz * dz;

            if (squareLength == 0.0)
                return point.DistanceTo(Point) < tolerance;

            double wx = point.X - Point.X;
            double wy = point.Y - Point.Y;
            double wz = point.Z - Point.Z;

            double crossProductX = dy * wz - dz * wy;
            double crossProductY = dz * wx - dx * wz;
            double crossProductZ = dx * wy - dy * wx;

            double squareCrossLength = crossProductX * crossProductX + crossProductY * crossProductY + crossProductZ * crossProductZ;

            // distance^2 < tolerance^2
            return squareCrossLength < tolerance * tolerance * squareLength;
        }

        /// <summary>
        /// Get intersection of the line with a plane, only in the half of the line in front of <see cref="Point"/> (in the direction)
        /// </summary>
        /// <param name="s">The plane</param>
        /// <param name="intersection">The intersection point, null if there is none (<see cref="Point"/> if the line lies on the plane)</param>
        /// <param name="tolerance">The calculation tolerance</param>
        /// <returns>True if the intersection exists</returns>
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
        /// The shortest line between two lines in 3D: Pa = Point + mua * Direction on this line, Pb = ray.Point + mub * ray.Direction on the other one.
        /// If the lines are parallel (exactly) Pa is <see cref="Point"/> (mua = 0) and Pb its projection on the other line
        /// </summary>
        /// <param name="ray">Second line.</param>
        /// <param name="mua">Parameter for point Pa in first line, this object.</param>
        /// <param name="mub">Parameter for point Pb in second line.</param>
        /// <param name="Pa">First point of shortest line segment, on this line.</param>
        /// <param name="Pb">Second point of shortest line segment, on the second line.</param>
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
        /// The shortest line between two lines in 3D (see <see cref="CalcShortestLineBetweenTwoRays(Ray3d, out double, out double, out Point3d, out Point3d)"/>)
        /// </summary>
        /// <param name="ray">Second line.</param>
        /// <returns>The segment from the point of this line to the point of <paramref name="ray"/> at the minimum distance</returns>
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
        /// Translates the line (its point)
        /// </summary>
        /// <param name="v1">The translation along X</param>
        /// <param name="v2">The translation along Y</param>
        /// <param name="v3">The translation along Z</param>
        public override void Move(double v1, double v2, double v3)
        {
            _point.Move(v1, v2, v3);
        }

        /// <summary>
        /// Translates the line (its point)
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
            return new Ray3d(new Point3d(_point), new Vector3d(_direction));
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Ray3d)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal line</returns>
        public override bool Equals(object obj)
        {
            if (obj is Ray3d ray)
                return Equals(ray);

            return false;
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Ray3d)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal line</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Ray3d ray)
                return Equals(ray);

            return false;
        }

        /// <summary>
        /// Equality of the point and of the direction within the tolerance (the same line described by another point or another length of the
        /// direction is not equal)
        /// </summary>
        /// <param name="other">The line to compare</param>
        /// <returns>True if the points and the directions are equal</returns>
        public bool Equals(Ray3d other)
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
