using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public sealed class Line2d : GeometryBase, ISerializable, IEquatable<Line2d>, ICloneable
    {
        #region Variables

        private Point2d _start;
        private Point2d _end;

        #endregion

        #region Properties

        public Point2d Start { get =>  _start; set => _start = value; }

        public Point2d End { get => _end; set => _end = value; }

        public Point2d Mid => GetMidPoint();

		public double Length => GetLength();

        #endregion

        #region Public Constructors

        public Line2d(Point2d start, Point2d end)
        {
            _start = start;
            _end = end;
        }

        public Line2d(Line2d line)
        {
            _start = new Point2d(line.Start);
            _end = new Point2d(line.End);
        }

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
        public double GetLength()
        {
            return _start.DistanceTo(_end);
        }

        /// <summary>
        /// Get the line mid point
        /// </summary>
        /// <returns></returns>
        public Point2d GetMidPoint()
        {
			return new Point2d((_start.X + _end.X) * 0.5, (_start.Y + _end.Y) * 0.5);
		}

        /// <summary>
        /// Get the intersection with another line
        /// </summary>
        /// <param name="line">The other line</param>
        /// <param name="intersection">The intersection point</param>
        /// <param name="tol"></param>
        /// <returns>True if the lines have an intersection</returns>
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
        /// Get the intersection with another line
        /// </summary>
        /// <param name="line">The other line</param>
        /// <param name="intersection">The intersection point</param>
        /// <param name="tol"></param>
        /// <returns>True if the lines have an intersection</returns>
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
        /// Rotate the line by a given angle around a given point
        /// </summary>
        /// <param name="point">point around which the line rotates</param>
        /// <param name="angle">angle of rotation (radians)</param>
        /// <returns></returns>
        public void Rotate(Point2d point, double angle)
        {
            _start.Rotate(point, angle);
            _end.Rotate(point, angle);
        }

        /// <summary>
        /// Get the distance from the line to the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <returns>The distance</returns>
        public double DistanceTo(Point2d point)
        {
            return Math.Sqrt(SquareDistanceTo(point));
        }

        /// <summary>
        /// Get the sqare distance from the line to the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <returns>The distance</returns>
        public double SquareDistanceTo(Point2d point)
        {
            var distPoint = PointDistanceTo(point);
            var dx = point.X - distPoint.X;
            var dy = point.Y - distPoint.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Get nearest point in line from another point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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
        /// Distance with sign, on the left the sign is positive, on the right the sign is negative.
        /// </summary>
        /// <param name="P"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
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
        /// Boundary integration over 2D line.
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

        public Line2d CloneAndMove(Vector2d movement)
        {
            var p = (Line2d)Clone();
            p.Move(movement);
            return p;
        }

        public bool IsPointOnLine(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (point == null)
            {
                throw new ArgumentNullException("Point can not be null");
            }

            // http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
            // If the cross product of the vectors n1 and n2 is zero in all directions then the points are collinear, 
            // n1 and n2 are the vectors connecting one point to the other two points

            double crossProductZ = ((_end.X - _start.X) * (point.Y - _start.Y) - (point.X - _start.X) * (_end.Y - _start.Y));
            double L1 = _start.DistanceTo(point);
            double L2 = _end.DistanceTo(point);
            double L = GetLength();

            double tol = Utilities.Maths.ErrorPropagation.ProductTolerance(crossProductZ, crossProductZ, tolerance, tolerance);

            if (Math.Abs(crossProductZ) < tol && L1 <= L && L2 <= L)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Scale the line respect <see cref="Point2d.Origin"/> point
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        public double GetSlope()
        {
            return ((_end.Y - _start.Y) / (_end.X - _start.X));
        }

        /// <summary>
        /// Split line by a array of points.
        /// </summary>
        /// <param name="points">The list of 3d points</param>
        /// <param name="line">The list of resultant lines</param>
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
        /// Split the line by parameters
        /// </summary>
        /// <param name="parameters">The imput parameters (0 - start, 1 - end)</param>
        /// <param name="tolerance">The tolerance</param>
        /// <param name="line">The list of resultant lines</param>
        public void Split(double[] parameters, out Line2d[] line, double tolerance = GeometryBase.Tolerance)
        {
            Line3d line3d = new Line3d(Start, End);
			Line3d[] outLines = line3d.Split(parameters, tolerance);

            line = new Line2d[outLines.Length];
            for(int i = 0; i < outLines.Length; i++)
                line[i] = new Line2d(outLines[i].Start, outLines[i].End);
        }

        /// <summary>
        /// Split the line in <paramref name="subdivision"/> parts
        /// </summary>
        /// <param name="subdivision"></param>
        /// <param name="line"></param>
        /// <param name="tolerance"></param>
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
        /// Converts the line on a Vector3d
        /// </summary>
        /// <returns>The vector from Start to End of the line</returns>
        public Vector2d ToVector()
        {
            return _end - _start;
        }

		/// <summary>
		/// Check if <paramref name="other"/> is one of the ends of the line
		/// </summary>
		/// <param name="other">Point to check</param>
		/// <returns></returns>
		public bool VertexExists(Point2d other)
        {
            if (Start.Equals(other) == true || End.Equals(other) == true)            
                return true;
            
            return false;
        }

		/// <summary>
		/// Check if <paramref name="other"/> is one of the ends of the line
		/// </summary>
		/// <param name="other">Point to check</param>
        /// <param name="tolerance">Matching tolerance</param>
		/// <returns></returns>
		public bool VertexExists(Point2d other, double tolerance)
		{
			if (Start.Equals(other) == true || End.Equals(other) == true)
				return true;
            if(Start.DistanceTo(other) < tolerance || End.DistanceTo(other) < tolerance) 
                return true;

			return false;
		}

		/// <summary>
		/// Move line by an given increment dx, dy
		/// </summary>
		/// <param name="dx">The X coordinate increment</param>
		/// <param name="dy">The Y coordinate increment</param>
		/// <param name="dz"></param>
		public override void Move(double dx, double dy, double dz = 0)
        {
            _start.Move(dx, dy);
            _end.Move(dx, dy);
        }

        /// <summary>
        /// Move line by a given vector
        /// </summary>
        /// <param name="movement">Displacement vector</param>
        public override void Move(Vector3d movement)
        {
            _start.Move(movement);
            _end.Move(movement);
        }

        #endregion

        #region Operators overrides

        public static bool operator ==(Line2d line1, Line2d line2)
        {
            if (ReferenceEquals(line1, line2))
                return true;

            if (line1 is null || line2 is null)
                return false;

            return line1.Equals(line2);
        }

        public static bool operator !=(Line2d line1, Line2d line2)
        {
            return !(line1 == line2);
        }

        #endregion

        #region Public Methods Override

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Start", _start, typeof(Point2d));
            info.AddValue("End", _end, typeof(Point2d));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Line2d);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if Start and End of line and otherLine are equals. True also if otherLine is flipped (line.Start == otherLine.End AND line.End == otherLine.Start)</returns>
        public bool Equals(Line2d other)
        {
            return !(other is null) && ((_start == other.Start && _end == other.End) || (_start == other.End && _end == other.Start));
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Line2d line)
                return Equals(line);

            return false;
        }

        /// <summary>
        /// 
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

        public override object Clone()
        {
            return new Line2d(this);
        }

        #endregion
    }
}
