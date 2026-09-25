using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// A segment in the XY plane from <see cref="Start"/> to <see cref="End"/>. Two segments are equal if their ends are equal within the
    /// tolerance, also with the opposite direction
    /// </summary>
    [Serializable]
    public sealed class Line2d : GeometryBase, ISerializable, IEquatable<Line2d>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The start point
        /// </summary>
        private Point2d _start;
        /// <summary>
        /// The end point
        /// </summary>
        private Point2d _end;

        #endregion

        #region Properties

        /// <summary>
        /// The start point (the instance is kept, not copied)
        /// </summary>
        public Point2d Start { get =>  _start; set => _start = value; }

        /// <summary>
        /// The end point (the instance is kept, not copied)
        /// </summary>
        public Point2d End { get => _end; set => _end = value; }

        /// <summary>
        /// A new point in the middle of the segment
        /// </summary>
        public Point2d Mid => GetMidPoint();

		/// <summary>
		/// The length of the segment
		/// </summary>
		public double Length => GetLength();

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a segment (the point instances are kept, not copied)
        /// </summary>
        /// <param name="start">The start point</param>
        /// <param name="end">The end point</param>
        public Line2d(Point2d start, Point2d end)
        {
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Creates a copy of a segment, with copies of its points
        /// </summary>
        /// <param name="line">The segment to copy</param>
        public Line2d(Line2d line)
        {
            _start = new Point2d(line.Start);
            _end = new Point2d(line.End);
        }

        /// <summary>
        /// Deserialization constructor: reads the ends (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Line2d(SerializationInfo info, StreamingContext context)
        {
            _start = (Point2d)info.GetValue("Start", typeof(Point2d));
            _end = (Point2d)info.GetValue("End", typeof(Point2d));
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Get the line length
        /// </summary>
        /// <returns>The distance between the ends</returns>
        public double GetLength()
        {
            return _start.DistanceTo(_end);
        }

        /// <summary>
        /// Get the line mid point
        /// </summary>
        /// <returns>A new point in the middle of the segment</returns>
        public Point2d GetMidPoint()
        {
			return new Point2d((_start.X + _end.X) * 0.5, (_start.Y + _end.Y) * 0.5);
		}

        /// <summary>
        /// Get the intersection with another segment (see <see cref="Line3d.GetIntersection(Line3d, out Point3d, double)"/>)
        /// </summary>
        /// <param name="line">The other segment</param>
        /// <param name="intersection">The intersection point, null if there is none</param>
        /// <param name="tol">The tolerance</param>
        /// <returns>True if the segments have a single intersection point</returns>
        public bool GetIntersection(Line2d line, out Point2d intersection, double tol = GeometryBase.Tolerance)
        {
            intersection = null;

            Line3d line1 = new Line3d(this);
            Line3d line2 = new Line3d(line);

            bool check = line1.GetIntersection(line2, out Point3d inters, tol);

            if (check == true)
            {
                Point2d inter = new Point2d(inters.X, inters.Y);
                intersection = inter;
                return true;
            }
            
            return false;            
        }

        /// <summary>
        /// Get the intersection of the infinite lines through the two segments (see <see cref="Line3d.GetIntersectionWithInfiniteLine"/>)
        /// </summary>
        /// <param name="line">The other line</param>
        /// <param name="intersection">The intersection point, null if there is none</param>
        /// <param name="tol">The tolerance</param>
        /// <returns>True if the lines have a single intersection point (they are not parallel)</returns>
        public bool GetIntersectionWithInfiniteLine(Line2d line, out Point2d intersection, double tol = GeometryBase.Tolerance)
        {
            intersection = null;

            Line3d line1 = new Line3d(this);
            Line3d line2 = new Line3d(line);

            bool check = line1.GetIntersectionWithInfiniteLine(line2, out Point3d inters, tol);

            if (check == true)
            {
                Point2d inter = new Point2d(inters.X, inters.Y);
                intersection = inter;
                return true;
            }

            return false;            
        }

        /// <summary>
        /// Rotates the segment in place by a given angle around a given point
        /// </summary>
        /// <param name="point">The center of rotation</param>
        /// <param name="angle">The angle of rotation (radians, counterclockwise)</param>
        public void Rotate(Point2d point, double angle)
        {
            _start.Rotate(point, angle);
            _end.Rotate(point, angle);
        }

        /// <summary>
        /// Get the distance from the segment to the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <returns>The distance from the nearest point of the segment (see <see cref="PointDistanceTo"/>)</returns>
        public double DistanceTo(Point2d point)
        {
            return Math.Sqrt(SquareDistanceTo(point));
        }

        /// <summary>
        /// Get the square of the distance from the segment to the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <returns>The square of the distance from the nearest point of the segment</returns>
        public double SquareDistanceTo(Point2d point)
        {
            var distPoint = PointDistanceTo(point);
            var dx = point.X - distPoint.X;
            var dy = point.Y - distPoint.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Get the nearest point of the segment to another point
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>A new point: the projection of <paramref name="point"/> on the segment, or the nearest end if the projection is outside</returns>
        public Point2d PointDistanceTo(Point2d point)
        {
            // https://stackoverflow.com/a/6853926

            double A = point.X - _start.X;
            double B = point.Y - _start.Y;
            double C = _end.X - _start.X;
            double D = _end.Y - _start.Y;

            double dot = A * C + B * D;
            double squareDistance = _start.SquareDistanceTo(_end);
            double param = -1;

            if (squareDistance != 0) // in case of 0 length line
                param = dot / squareDistance;

            Point2d distPoint = new Point2d();

            if (param < 0)
            {
                distPoint.X = _start.X;
                distPoint.Y = _start.Y;
            }
            else if (param > 1)
            {
                distPoint.X = _end.X;
                distPoint.Y = _end.Y;
            }
            else
            {
                distPoint.X = _start.X + param * C;
                distPoint.Y = _start.Y + param * D;
            }

            return distPoint;
        }

        /// <summary>
        /// Distance with sign of a point from the infinite line through the segment: positive on the left, negative on the right
        /// </summary>
        /// <param name="P">The point</param>
        /// <param name="distanceTolerance">The relative tolerance: the distance is 0 if it is smaller than <paramref name="distanceTolerance"/> times the
        /// length of the segment</param>
        /// <returns>The distance with sign</returns>
        public double OrientedDistFromSegment2D(Point2d P, double distanceTolerance = GeometryBase.Tolerance)
        {
            var Pi = _start;
            var Pj = _end;
            double distP1_P2 = Pi.DistanceTo(Pj);
            double distance = (P.X * (Pi.Y - Pj.Y) + Pi.X * (Pj.Y - P.Y) + Pj.X * (P.Y - Pi.Y)) / distP1_P2;

            if (Math.Abs(distance / distP1_P2) < distanceTolerance)
                distance = 0.0;

            return distance;
        }

        /// <summary>
        /// Boundary integration over 2D line: adds the contribution of the segment, as a side of a polygon, to the integrals of the region of the
        /// polygon (Green's theorem). Summed over the sides of a counterclockwise polygon they give the exact integrals of its region
        /// </summary>
        /// <param name="A_l">Area.</param>
        /// <param name="S_X_l">Static moment relative to the X axis.</param>
        /// <param name="S_Y_l">Static moment relative to the Y axis.</param>
        /// <param name="I_X_l">Inertia moment relative to the X axis.</param>
        /// <param name="I_Y_l">Inertia moment relative to the Y axis.</param>
        /// <param name="I_XY_l">Product of inertia moment relative to the X, Y axes.</param>
        public void IntegrateOnBoundary(ref double A_l, ref double S_X_l, ref double S_Y_l, ref double I_X_l, ref double I_Y_l, ref double I_XY_l)
        {
            var Pi = _start;
            var Pj = _end;

            double cXp = Pj.X + Pi.X;
            double cYm = Pj.Y - Pi.Y;
            double cYp = Pj.Y + Pi.Y;
            double cXXp = Pj.X * Pj.X + Pi.X * Pi.X;
            double cXYp = Pj.X * Pj.Y + Pi.X * Pi.Y;
            double cYYp = Pj.Y * Pj.Y + Pi.Y * Pi.Y;

            A_l += 0.5 * cYm * cXp;
            S_X_l += 1.0 / 6.0 * cYm * (cXp * cYp + cXYp);
            S_Y_l += 1.0 / 6.0 * cYm * (cXXp + Pj.X * Pi.X);
            I_X_l += 1.0 / 12.0 * cYm * (cYYp * cXp + 2.0 * cYp * cXYp);
            I_Y_l += 1.0 / 12.0 * cYm * cXp * cXXp;
            I_XY_l += 1.0 / 24.0 * cYm * (cXXp * cYp + 2.0 * cXp * cXYp);
        }

        /// <summary>
        /// A copy of the segment translated by a vector
        /// </summary>
        /// <param name="movement">The translation</param>
        /// <returns>The new segment</returns>
        public Line2d CloneAndMove(Vector2d movement)
        {
            var p = (Line2d)Clone();
            p.Move(movement);
            return p;
        }

        /// <summary>
        /// Tell if a point is on the segment
        /// </summary>
        /// <param name="point">The point (its Z coordinate is ignored)</param>
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <returns>True if the distance of the point from the segment, in the XY plane, is smaller than <paramref name="tolerance"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="point"/> is null</exception>
        public bool IsPointOnLine(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (point == null)
            {
                throw new ArgumentNullException("Point can not be null");
            }

            // The point is on the segment if its distance from the segment (in the XY plane) is lower than the tolerance.
            // The previous check on the cross product used a tolerance proportional to the cross product itself, i.e. almost zero
            return SquareDistanceTo(new Point2d(point.X, point.Y)) < tolerance * tolerance;
        }

        /// <summary>
        /// A copy of the segment scaled respect to the origin
        /// </summary>
        /// <param name="factor">The scale factor</param>
        /// <returns>The new segment</returns>
        public Line2d Scale(double factor)
        {
            Point3d start = this.Start;
            Point3d end = this.End;
            start *= factor;
            end *= factor;
            return new Line2d(start, end);
        }

        /// <summary>
        /// Get the line slope
        /// </summary>
        /// <returns>dY / dX (infinite for a vertical segment)</returns>
        public double GetSlope()
        {
            return ((_end.Y - _start.Y) / (_end.X - _start.X));
        }

        /// <summary>
        /// Split the segment by an array of points (see <see cref="Line3d.Split(Point3d[], double)"/>)
        /// </summary>
        /// <param name="points">The points on the segment</param>
        /// <param name="line">The resultant segments</param>
        /// <param name="tolerance">Tolerance</param>
        public void Split(Point2d[] points, out Line2d[] line, double tolerance = GeometryBase.Tolerance)
        {
            Line3d line3d = new Line3d(Start, End);

            Point3d[] points3d = new Point3d[points.Length];
            for (int i = 0; i < points.Length; i++)
                points3d[i] = new Point3d(points[i].X, points[i].Y, 0);
			Line3d[] outLines = line3d.Split(points3d, tolerance);

            line = new Line2d[outLines.Length];
            for (int i = 0; i < outLines.Length; i++)
                line[i] = new Line2d(outLines[i].Start, outLines[i].End);
        }

        /// <summary>
        /// Split the segment by parameters (see <see cref="Line3d.Split(double[], double)"/>)
        /// </summary>
        /// <param name="parameters">The input parameters (0 - start, 1 - end)</param>
        /// <param name="line">The resultant segments</param>
        /// <param name="tolerance">The tolerance</param>
        public void Split(double[] parameters, out Line2d[] line, double tolerance = GeometryBase.Tolerance)
        {
            Line3d line3d = new Line3d(Start, End);
			Line3d[] outLines = line3d.Split(parameters, tolerance);

            line = new Line2d[outLines.Length];
            for(int i = 0; i < outLines.Length; i++)
                line[i] = new Line2d(outLines[i].Start, outLines[i].End);
        }

        /// <summary>
        /// Split the segment in <paramref name="subdivision"/> equal parts
        /// </summary>
        /// <param name="subdivision">The number of parts (0: no segment, 1: this segment)</param>
        /// <param name="line">The resultant segments</param>
        /// <param name="tolerance">The tolerance</param>
        public void Split(int subdivision, out Line2d[] line, double tolerance = GeometryBase.Tolerance)
        {
            if (subdivision == 0)
                line = new Line2d[0];

            else if (subdivision == 1)
                line = new Line2d[] { this };

            else
            {
                double[] parameters = new double[subdivision - 1];
                double step = 1.0 / subdivision;

                for (int i = 0; i < subdivision - 1; i++)
                    parameters[i] = step * (1 + i);

                Split(parameters, out line, tolerance);
            }
        }

        /// <summary>
        /// Converts the segment to a vector
        /// </summary>
        /// <returns>The vector from Start to End of the segment</returns>
        public Vector2d ToVector()
        {
            return _end - _start;
        }

		/// <summary>
		/// Check if <paramref name="other"/> is one of the ends of the segment
		/// </summary>
		/// <param name="other">Point to check</param>
		/// <returns>True if <paramref name="other"/> is equal (within the tolerance of the points) to one of the ends</returns>
		public bool VertexExists(Point2d other)
        {
            if (Start.Equals(other) == true || End.Equals(other) == true)            
                return true;
            
            return false;
        }

		/// <summary>
		/// Check if <paramref name="other"/> is one of the ends of the segment
		/// </summary>
		/// <param name="other">Point to check</param>
		/// <param name="tolerance">Matching tolerance</param>
		/// <returns>True if <paramref name="other"/> is equal to one of the ends or closer than <paramref name="tolerance"/></returns>
		public bool VertexExists(Point2d other, double tolerance)
		{
			if (Start.Equals(other) == true || End.Equals(other) == true)
				return true;
            if(Start.DistanceTo(other) < tolerance || End.DistanceTo(other) < tolerance) 
                return true;

			return false;
		}

		/// <summary>
		/// Moves the segment by the given increments
		/// </summary>
		/// <param name="dx">The X coordinate increment</param>
		/// <param name="dy">The Y coordinate increment</param>
		/// <param name="dz">Ignored: the segment is in the XY plane</param>
		public override void Move(double dx, double dy, double dz = 0)
        {
            _start.Move(dx, dy);
            _end.Move(dx, dy);
        }

        /// <summary>
        /// Moves the segment by a vector (its Z component is ignored)
        /// </summary>
        /// <param name="movement">Displacement vector</param>
        public override void Move(Vector3d movement)
        {
            _start.Move(movement);
            _end.Move(movement);
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// Equality: true if the segments are the same object, both null or equal (<see cref="Equals(Line2d)"/>)
        /// </summary>
        /// <param name="line1">The first segment</param>
        /// <param name="line2">The second segment</param>
        /// <returns>True if the segments are equal</returns>
        public static bool operator ==(Line2d line1, Line2d line2)
        {
            if (ReferenceEquals(line1, line2))
                return true;

            if (line1 is null || line2 is null)
                return false;

            return line1.Equals(line2);
        }

        /// <summary>
        /// Inequality (see the equality operator)
        /// </summary>
        /// <param name="line1">The first segment</param>
        /// <param name="line2">The second segment</param>
        /// <returns>True if the segments are different</returns>
        public static bool operator !=(Line2d line1, Line2d line2)
        {
            return !(line1 == line2);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Serializes the segment: the <see cref="BaseObject.Guid"/> and the ends
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Start", _start, typeof(Point2d));
            info.AddValue("End", _end, typeof(Point2d));
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Line2d)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal segment</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Line2d);
        }

        /// <summary>
        /// Equality of the ends within the tolerance, in either direction
        /// </summary>
        /// <param name="other">The segment to compare</param>
        /// <returns>True if Start and End of line and otherLine are equals. True also if otherLine is flipped (line.Start == otherLine.End AND line.End == otherLine.Start)</returns>
        public bool Equals(Line2d other)
        {
            return !(other is null) && ((_start == other.Start && _end == other.End) || (_start == other.End && _end == other.Start));
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Line2d)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal segment</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Line2d line)
                return Equals(line);

            return false;
        }

        /// <summary>
        /// The hash code, independent of the direction
        /// </summary>
        /// <returns>The hashCode. If (line.Start == otherLine.End AND line.End == otherLine.Start), line and otherLine returns the same hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode1 = -17;
                hashCode1 = hashCode1 * -23 + EqualityComparer<Point2d>.Default.GetHashCode(_start) + EqualityComparer<Point2d>.Default.GetHashCode(_end);

                int hashCode2 = -17;
                hashCode2 = hashCode2 * -23 + EqualityComparer<Point2d>.Default.GetHashCode(_end) + EqualityComparer<Point2d>.Default.GetHashCode(_start);

                return hashCode1 + hashCode2;
            }
        }

        /// <summary>
        /// Creates a copy of the segment, with copies of its points
        /// </summary>
        /// <returns>The copy</returns>
        public override object Clone()
        {
            return new Line2d(this);
        }

        #endregion
    }
}
