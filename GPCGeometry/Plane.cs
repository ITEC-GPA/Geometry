using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// A plane in the space, described by three points <see cref="P1"/> (the origin), <see cref="P2"/>, <see cref="P3"/> and by the unit
    /// normal (P2 - P1) × (P3 - P1). Two planes are equal if they are the same geometric plane (the origin of one on the other and parallel normals)
    /// </summary>
    [Serializable]
    public sealed class Plane : GeometryBase, IEquatable<Plane>, ISerializable
    {
        #region Variables

        /// <summary>
        /// The first point (the origin)
        /// </summary>
        private Point3d _p1;
        /// <summary>
        /// The second point
        /// </summary>
        private Point3d _p2;
		/// <summary>
		/// The third point
		/// </summary>
		private Point3d _p3;
        /// <summary>
        /// The unit normal
        /// </summary>
        private Vector3d _normal;

        #endregion 

        #region Properties

        /// <summary>
        /// The first point, the origin of the plane. Changing the points does not update <see cref="Normal"/>
        /// </summary>
        public Point3d P1 { get => _p1; set => _p1 = value; }

        /// <summary>
        /// The second point (the direction P2 - P1 is the X axis of <see cref="GetCoordinateSystem"/>)
        /// </summary>
        public Point3d P2 { get => _p2; set => _p2 = value; }

        /// <summary>
        /// The third point
        /// </summary>
        public Point3d P3 { get => _p3; set => _p3 = value; }

        /// <summary>
        /// The unit normal of the plane
        /// </summary>
        public Vector3d Normal => _normal;

        /// <summary>
        /// The origin of the plane: the same as <see cref="P1"/>
        /// </summary>
        public Point3d Origin { get => _p1; set => _p1 = value; }

        /// <summary>
        /// The coefficient A of the equation A x + B y + C z + D = 0 (the X component of the unit normal)
        /// </summary>
        public double A => _normal.X;

        /// <summary>
        /// The coefficient B of the equation A x + B y + C z + D = 0 (the Y component of the unit normal)
        /// </summary>
        public double B => _normal.Y;

        /// <summary>
        /// The coefficient C of the equation A x + B y + C z + D = 0 (the Z component of the unit normal)
        /// </summary>
        public double C => _normal.Z;

        /// <summary>
        /// The coefficient D of the equation A x + B y + C z + D = 0: minus the distance with sign of the origin of the axes along the normal
        /// </summary>
        public double D => -_normal.X * _p1.X - _normal.Y * _p1.Y - _normal.Z * _p1.Z;

        #endregion 

        #region Public Constructors

        /// <summary>
        /// Plane by 3 points
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third</param>
        /// <param name="tolerance">Tolerance used to check that the points are not aligned or coincidend</param>
        /// <exception cref="ArgumentException">Throw when points are aligned or coincident</exception>
        public Plane(Point3d p1, Point3d p2, Point3d p3, double tolerance = GeometryBase.Tolerance)
        {
            Line3d line1 = new Line3d(p1, p2);
            Line3d line2 = new Line3d(p1, p3);
            Line3d line3 = new Line3d(p3, p2);

            double dist1 = line1.Length;
            double dist2 = line2.Length;
            double dist3 = line3.Length;

            if (line1.IsPointOnLine(p3, tolerance) || line2.IsPointOnLine(p2, tolerance) || line3.IsPointOnLine(p1, tolerance) || dist1 < tolerance || dist2 < tolerance || dist3 < tolerance)
                throw new ArgumentException("Fail to create the plane: Point cannot be aligned or coincident");

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            Vector3d v1 = p2 - p1;
            Vector3d v2 = p3 - p1;
            _normal = v1.CrossProduct(v2);
            _normal.Unitize();
        }

        /// <summary>
        /// Construct a plane by the origin point and the normal vector
        /// </summary>
        /// <param name="point">The origin point</param>
        /// <param name="normal">The normal vector</param>
        /// <exception cref="ArgumentException">If <paramref name="normal"/> has zero length</exception>
        public Plane(Point3d point, Vector3d normal)
        {
            if (normal is null)
                throw new ArgumentNullException(nameof(normal));

            _normal = new Vector3d(normal); // the vector of the caller must not be modified
            UnitizeNormal();
            _p1 = point ?? throw new ArgumentNullException(nameof(point));
            ComputePointsFromOriginAndNormal();
        }

        /// <summary>
        /// Plane by origin and two directions lying on the plane. The two directions must not be parallel, but they can be not orthogonal
        /// </summary>
        /// <param name="origin">The origin (the instance is kept)</param>
        /// <param name="xAxis">The first direction (not changed)</param>
        /// <param name="yAxis">The second direction (not changed)</param>
        /// <exception cref="ArgumentException">If the two directions are parallel</exception>
        /// <exception cref="ArgumentNullException">If a parameter is null</exception>
        public Plane(Point3d origin, Vector3d xAxis, Vector3d yAxis)
        {
            if (xAxis is null)
                throw new ArgumentNullException(nameof(xAxis));

            if (yAxis is null)
                throw new ArgumentNullException(nameof(yAxis));

            // copies: the vectors of the caller must not be modified
            Vector3d x = Vector3d.Unitize(xAxis);
            Vector3d y = Vector3d.Unitize(yAxis);

            _normal = x.CrossProduct(y);
            UnitizeNormal(); // x and y are unit vectors but not necessarily orthogonal
            _p1 = origin ?? throw new ArgumentNullException(nameof(origin));
            _p2 = _p1 + x;
            _p3 = _p1 + y;
        }

        /// <summary>
        /// Create plane using general equation in 3D space: A*x+B*y+C*z+D=0.
        /// </summary>
        /// <param name="a">Parameter "A" in general plane equation.</param>
        /// <param name="b">Parameter "B" in general plane equation.</param>
        /// <param name="c">Parameter "C" in general plane equation.</param>
        /// <param name="d">Parameter "D" in general plane equation.</param>
        /// <exception cref="ArgumentException">If a, b and c are all zero</exception>
        public Plane(double a, double b, double c, double d)
        {
            // the origin is taken on the axis with the largest coefficient, so the division is never by zero
            if (Math.Abs(a) >= Math.Abs(b) && Math.Abs(a) >= Math.Abs(c) && a != 0)
                _p1 = new Point3d(-d / a, 0, 0);
            else if (Math.Abs(b) >= Math.Abs(c) && b != 0)
                _p1 = new Point3d(0, -d / b, 0);
            else if (c != 0)
                _p1 = new Point3d(0, 0, -d / c);
            else
                throw new ArgumentException("The coefficients a, b and c cannot be all zero");

            _normal = new Vector3d(a, b, c);
            UnitizeNormal(); // the normal must be unitized, IsPointOnPlane and D rely on it
            ComputePointsFromOriginAndNormal();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="plane">The source plane</param>
        public Plane(Plane plane)
        {
            _p1 = new Point3d(plane._p1);
            _p2 = new Point3d(plane._p2);
            _p3 = new Point3d(plane._p3);
            _normal = new Vector3d(plane._normal);
        }

        /// <summary>
        /// Deserialization constructor: reads the points and the normal (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Plane(SerializationInfo info, StreamingContext context)
            : base()
        {
            _p1 = (Point3d)info.GetValue("P1", typeof(Point3d));
            _p2 = (Point3d)info.GetValue("P2", typeof(Point3d));
            _p3 = (Point3d)info.GetValue("P3", typeof(Point3d));
            _normal = (Vector3d)info.GetValue("Normal", typeof(Vector3d));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Makes the normal a unit vector
        /// </summary>
        /// <exception cref="ArgumentException">If the normal has zero length</exception>
        private void UnitizeNormal()
        {
            if (_normal.Length == 0)
                throw new ArgumentException("Fail to create the plane: the normal vector has zero length");

            _normal.Unitize();
        }

        /// <summary>
        /// Calculate 3 points on plane starting from the plane origin and normal
        /// </summary>
        private void ComputePointsFromOriginAndNormal()
        {
            Vector3d a = _normal.CrossProduct(Vector3d.XAxis);
            Vector3d b = _normal.CrossProduct(Vector3d.YAxis);
            Vector3d max_ab = a.DotProduct(a) < b.DotProduct(b) ? b : a;
            Vector3d c = _normal.CrossProduct(Vector3d.ZAxis);
            Vector3d u = max_ab.DotProduct(max_ab) < c.DotProduct(c) ? c : max_ab;
            u.Unitize();
            _p2 = _p1 + u;
            Vector3d v = _normal.CrossProduct(u);
            v.Unitize();
            _p3 = _p1 + v;
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Tell if a point is on Plane
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="tolerance">The tolerance on the distance from the plane</param>
        /// <returns>True if the distance of the point from the plane is smaller than <paramref name="tolerance"/></returns>
        public bool IsPointOnPlane(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            return Math.Abs(_normal.X * point.X + _normal.Y * point.Y + _normal.Z * point.Z + D) < tolerance;
        }

        /// <summary>
        /// Projects a point on the plane
        /// </summary>
        /// <param name="point">The point to project</param>
        /// <returns>The projected point</returns>
        public Point3d Project(Point3d point)
        {
            return point - (point * _normal + D) / (_normal * _normal) * _normal;
        }

        /// <summary>
        /// The angle, on the plane, between the direction P1 → P2 and the direction P1 → <paramref name="point"/>
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>The angle in radians, from 0 to pi (without sign); NaN if the point is not on the plane</returns>
        public double AngleOnPlane(Point3d point)
        {
            if (!IsPointOnPlane(point))
                return double.NaN;

            Vector3d v1 = _p2 - _p1;
            Vector3d v2 = point - _p1;

            double dot = v1.DotProduct(v2);
            return Math.Acos(dot / (v1.Length * v2.Length));
        }

        /// <summary>
        /// Angle between vector and plane in radians (0 &lt;= angle &lt;= Pi/2)
        /// </summary>
        /// <param name="v">The vector</param>
        /// <returns>The angle</returns>
        public double AngleTo(Vector3d v)
        {
            return Math.Abs(Math.PI / 2 - _normal.AngleTo(v));
        }

        /// <summary>
        /// Angle between line and plane in radians (0 &lt;= angle &lt;= Pi/2)
        /// </summary>
        /// <param name="l">The line</param>
        /// <returns>The angle</returns>
        public double AngleTo(Line3d l)
        {
            return Math.Abs(Math.PI / 2 - _normal.AngleTo(l.ToVector()));
        }

        /// <summary>
        /// Angle between two planes in radians (0 &lt;= angle &lt;= Pi/2)
        /// </summary>
        /// <param name="s">The other plane</param>
        /// <returns>The angle</returns>
        public double AngleTo(Plane s)
        {
            double ang = _normal.AngleTo(s.Normal);
            if (ang <= Math.PI / 2)
                return ang;
            else
                return Math.PI - ang;
        }

        /// <summary>
        /// Calculate the square distance of a point from the plane
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <returns>The square of the distance (computed from the three points)</returns>
        public double SquareDistanceToPlane(Point3d point)
        {
            double a1 = _p2.X - _p1.X;
            double b1 = _p2.Y - _p1.Y;
            double c1 = _p2.Z - _p1.Z;
            double a2 = _p3.X - _p1.X;
            double b2 = _p3.Y - _p1.Y;
            double c2 = _p3.Z - _p1.Z;

            double a = b1 * c2 - b2 * c1;
            double b = a2 * c1 - a1 * c2;
            double c = a1 * b2 - b1 * a2;

            double d = (-a * _p1.X - b * _p1.Y - c * _p1.Z);

            // double test = a * point.X + b * point.Y + c * point.Z + d;

            double distancePointToPlanePow2 = Math.Pow(Math.Abs(a * point.X + b * point.Y + c * point.Z + d), 2) /
                ((Math.Pow(a, 2)) + (Math.Pow(b, 2)) + (Math.Pow(c, 2)));

            return distancePointToPlanePow2;
        }

        /// <summary>
        /// Calculate the distance of a point from the plane
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <returns>The distance</returns>
        public double DistanceToPlane(Point3d point)
        {
            double DistancePointToPlanePow2 = SquareDistanceToPlane(point);
            return Math.Sqrt(DistancePointToPlanePow2);
        }

        /// <summary>
        /// The intersection of the plane with the infinite line through a segment
        /// </summary>
        /// <param name="line">The segment that defines the line</param>
        /// <param name="intersectionPoint">The intersection point; a point at the origin if there is none</param>
        /// <param name="tolerance">The tolerance on the product of the normal and the direction (parallel line)</param>
        /// <returns>False if the line is parallel to the plane</returns>
        public bool IntersectWithRay(Line3d line, out Point3d intersectionPoint, double tolerance = GeometryBase.Tolerance)
        {
            Vector3d direction = new Vector3d(line.End.X - line.Start.X, line.End.Y - line.Start.Y, line.End.Z - line.Start.Z);

            // replace the x,y,z components of the line equation into the plane equation and solve for t
            double numerator = -D - A * line.Start.X - B * line.Start.Y - C * line.Start.Z;
            double denominator = A * direction.X + B * direction.Y + C * direction.Z;

            if (Math.Abs(denominator) < tolerance)
            {
                intersectionPoint = new Point3d();
                return false;
            }

            double t = numerator / denominator;

            // replace t in the line equation to get the x,y,z
            double x = t * direction.X + line.Start.X;
            double y = t * direction.Y + line.Start.Y;
            double z = t * direction.Z + line.Start.Z;

            intersectionPoint = new Point3d(x, y, z);
            return true;
        }

        /// <summary>
        /// The intersection of the plane with the infinite line through a segment (see <see cref="IntersectWithRay"/>)
        /// </summary>
        /// <param name="line">The segment that defines the line</param>
        /// <param name="intersectionPoint">The intersection point; a point at the origin if there is none</param>
        /// <param name="tolerance">The tolerance on the product of the normal and the direction (parallel line)</param>
        /// <returns>False if the line is parallel to the plane</returns>
        public bool Intersect(Line3d line, out Point3d intersectionPoint, double tolerance = GeometryBase.Tolerance)
        {
            if (IntersectWithRay(line, out Point3d p, tolerance))
            {
                intersectionPoint = p;
                return true;
            }
            else
            {
                intersectionPoint = new Point3d();
                return false;
            }
        }

        /// <summary>
        /// The coordinate system of the plane: origin P1, X axis along P2 - P1, XY plane through P3
        /// </summary>
        /// <returns>The coordinate system</returns>
        public CoordinateSystem GetCoordinateSystem()
        {
            return new CoordinateSystem(_p1, _p2, _p3);
        }

        /// <summary>
        /// Move plane by an given increment
        /// </summary>
        /// <param name="v1">Dx displacement</param>
        /// <param name="v2">Dy displacement</param>
        /// <param name="v3">Dz displacement</param>
        public override void Move(double v1, double v2, double v3)
        {
            Vector3d vector = new Vector3d(v1, v2, v3);
            Move(vector);
        }

        /// <summary>
        /// Move plane by an given vector
        /// </summary>
        /// <param name="vector">The displacement vector</param>
        public override void Move(Vector3d vector)
        {
            _p1.Move(vector);
            _p2.Move(vector);
            _p3.Move(vector);
        }

        /// <summary>
        /// Creates a copy of the plane, with copies of its points and of the normal
        /// </summary>
        /// <returns>The copy</returns>
        public override object Clone()
        {
            return new Plane(this);
        }

        #endregion

        #region Public Static Method

        /// <summary>
        /// Given a plane surface given by three points (triangle) and a ray, find the point of intersection in parametric coordinates.
        /// Plane surface: r(u, v) = p1 + (p2 - p1) * u + (p3 - p1) * v
        /// Ray passing through origin: t(s) = q2 * s
        /// Ray: t(s) = q1 + (q2 - q1) * s
        ///
        /// The three points defining the plane can also define a triangle and the parameters u and v can tell whether the
        /// point of intersection is inside or outside the triangle with the comparisons: u &gt; 0; v &gt; 0; u+v &lt; 1.
        /// Points p1, p2 and p3 must be not overlapped or aligned, no check are made inside this method.
        ///
        /// Obtained with Octave:
        /// syms u v s
        /// syms Q1_1 Q1_2 Q1_3 Q2_1 Q2_2 Q2_3 P1_1 P1_2 P1_3 P2_1 P2_2 P2_3 P3_1 P3_2 P3_3 real
        /// P1_=[[P1_1];[P1_2];[P1_3]]
        /// P2_=[[P2_1];[P2_2];[P2_3]]
        /// P3_=[[P3_1];[P3_2];[P3_3]]
        /// Q1_=[[Q1_1];[Q1_2];[Q1_3]]
        /// Q2_=[[Q2_1];[Q2_2];[Q2_3]]
        /// t=Q1_+(Q2_-Q1_)*s
        /// r=P1_+(P2_-P1_)*u+(P3_-P1_)*v
        /// inters=r-t
        /// [s_sol,u_sol,v_sol]=solve([inters(1)==0, inters(2)==0, inters(3)==0], [s, u, v])
        /// </summary>
        /// <param name="p1">Plane/triangle, point 1.</param>
        /// <param name="p2">Plane/triangle, point 2.</param>
        /// <param name="p3">Plane/triangle, point 3.</param>
        /// <param name="q1">Ray, point 1.</param>
        /// <param name="q2">Ray, point 2.</param>
        /// <param name="u">Surface parameter of the solution. Return double.NaN if there is no solution.</param>
        /// <param name="v">Surface parameter of the solution. Return double.NaN if there is no solution.</param>
        /// <param name="s">Ray parameter of the solution. Return double.NaN if there is no solution.</param>
        /// <param name="inters">Intersection point. null if there is no solution.</param>
        /// <returns>true if intersection is inside the triangle.</returns>
        public static bool GetIntersectionTriangleWihtRay(in Point3d p1, in Point3d p2, in Point3d p3,
            in Point3d q1, in Point3d q2, out double u, out double v, out double s, out Point3d inters)
        {
            Point3d p1p2, p1p3, p2p3, q1q2;
            double k1, k2, k3, k4, k5, k6, k7, k8;
            var vZero = new Point3d(0.0, 0.0, 0.0);
            u = double.NaN;
            v = double.NaN;
            s = double.NaN;
            inters = null;

            p1p2 = p1 ^ p2;
            p1p3 = p1 ^ p3;
            p2p3 = p2 ^ p3;

            k3 = p1p3 * q2;

            if (q1 == vZero)
            {
                // Special case, first point of ray is the origin.
                k6 = -(p1p2 * p3);
                k7 = -(p1p2 * q2);
                k8 = (k7 + k3 - p2p3 * q2);
                if (Math.Abs(k8) == 0.0)
                    return false;

                u = k3 / k8;
                v = k7 / k8;
                s = k6 / k8;
                inters = s * q2;
            }
            else
            {
                q1q2 = q1 ^ q2;

                k1 = -(p1p3 * q1);
                k2 = p2p3 * q1;
                k4 = q1q2 * (p3 - p1);
                k5 = q1q2 * (p1 - p2);
                k6 = p1p2 * (q1 - p3);
                k7 = p1p2 * (q1 - q2);
                k8 = (k7 + k1 + k3 + k2 - p2p3 * q2);
                if (Math.Abs(k8) == 0.0)
                    return false;

                u = (k1 + k3 + k4) / k8;
                v = (k7 + k5) / k8;
                s = (k6 + k1 + k2) / k8;
                inters = q1 + s * (q2 - q1);
            }
            return u > 0.0 && v > 0.0 && u + v < 1.0;
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality of the geometric planes: the origin of <paramref name="other"/> is on this plane and the normals are parallel (also opposite)
        /// </summary>
        /// <param name="other">The plane to compare</param>
        /// <returns>True if the planes are the same geometric plane</returns>
        public bool Equals(Plane other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;

            //return other._p1.Equals(_p1) && other._p2.Equals(_p2) && other._p3.Equals(_p3);
            return IsPointOnPlane(other.Origin) && other.Normal.IsParallelTo(_normal);
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Plane)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is the same plane</returns>
        public override bool Equals(object obj)
        {
            if (obj is Plane plane)
                return Equals(plane);
            return false;
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Plane)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is the same plane</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Plane plane)
                return Equals(plane);

            return false;
        }

        /// <summary>
        /// The hash code of the exact origin and normal: planes equal for <see cref="Equals(Plane)"/> (another origin on the plane, opposite
        /// normal) can have different hash codes
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                //hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_p1);
                //hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_p2);
                //hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_p3);
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_p1);
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_normal);
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the plane: the <see cref="BaseObject.Guid"/>, the points and the normal
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("P1", _p1);
            info.AddValue("P2", _p2);
            info.AddValue("P3", _p3);
            info.AddValue("Normal", _normal);
        }

        #endregion
    }
}
