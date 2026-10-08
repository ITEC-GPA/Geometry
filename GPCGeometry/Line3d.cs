using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// A segment in the space from <see cref="Start"/> to <see cref="End"/>. Two segments are equal if their ends are equal within the
    /// tolerance, also with the opposite direction
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class Line3d : GeometryBase, ISerializable, IEquatable<Line3d>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The start point
        /// </summary>
        private Point3d _start;
        /// <summary>
        /// The end point
        /// </summary>
        private Point3d _end;

        #endregion

        #region Properties

        /// <summary>
        /// The start point (the instance is kept, not copied)
        /// </summary>
        public Point3d Start { get => _start; set => _start = value; }

        /// <summary>
        /// The end point (the instance is kept, not copied)
        /// </summary>
        public Point3d End { get => _end; set => _end = value; }

        /// <summary>
        /// A new point in the middle of the segment
        /// </summary>
        public Point3d Mid => GetMidPoint();

        /// <summary>
        /// The length of the segment
        /// </summary>
        public double Length => GetLength();

        /// <summary>
        /// A new infinite line through the segment (origin <see cref="Start"/>, direction <see cref="End"/> - <see cref="Start"/>)
        /// </summary>
        public Ray3d Ray => new Ray3d(Start, new Vector3d(End - Start));

        /// <summary>
        /// A new semi-infinite line from <see cref="Start"/> in the direction of <see cref="End"/>
        /// </summary>
        public SemiRay3d SemiRay => new SemiRay3d(Start, new Vector3d(End - Start));

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a segment (the point instances are kept, not copied)
		/// </summary>
		/// <param name="start">The start point</param>
		/// <param name="end">The end point</param>
		public Line3d(Point3d start, Point3d end)
        {
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Creates a copy of a segment, with copies of its points
        /// </summary>
        /// <param name="line">The segment to copy</param>
        public Line3d(Line3d line)
        {
            _start = new Point3d(line.Start);
            _end = new Point3d(line.End);
        }

        /// <summary>
        /// Creates the segment of the XY plane (Z = 0) with the ends of a planar segment
        /// </summary>
        /// <param name="line">The planar segment</param>
        public Line3d(Line2d line)
        {
            _start = new Point3d(line.Start);
            _end = new Point3d(line.End);
        }

        /// <summary>
        /// Deserialization constructor: reads the ends (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Line3d(SerializationInfo info, StreamingContext context)
        {
            _start = (Point3d)info.GetValue("Start", typeof(Point3d));
            _end = (Point3d)info.GetValue("End", typeof(Point3d));
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
        public Point3d GetMidPoint()
        {
            return new Point3d((_start.X + _end.X) * 0.5, (_start.Y + _end.Y) * 0.5, (_start.Z + _end.Z) * 0.5);
        }

        /// <summary>
        /// Get the intersection with another segment. The intersection must be unique: segments parallel, coincident or overlapping have no
        /// intersection; segments that share an end (and do not overlap) intersect at that end
        /// </summary>
        /// <param name="line">The other segment</param>
        /// <param name="inters">The intersection point, null if there is none</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <returns>True if the segments have a single intersection point</returns>
        public bool GetIntersection(Line3d line, out Point3d inters, double tolerance = GeometryBase.Tolerance)
        {
            // http://paulbourke.net/geometry/pointlineplane                 
            // http://paulbourke.net/geometry/pointlineplane/lineline.c  

            // Il  metodo torna vero SOLO se l'intersezione esiste
            // ed è unica => Point3d inters
            // Se le linee sono parallele ma non sovrapposte torna giustamente falso
            // Se le linee sono parallele e in parte sovrapposte torna falso
            // Se le linee sono coincidenti torna falso.
            // se le linee si intersecano sugli estremi (a meno che non siano sovrapposte) torna vero.

            double tol = Utilities.Maths.ErrorPropagation.SumTolerance(tolerance, tolerance);

            if (GetIntersectionHelper(line, tolerance, out Point3d pa, out Point3d pb, out inters))
			{
                if(inters != null)                
                    return true;
                
                double L12 = GetLength();
                double L34 = line.GetLength();
                double L1p = _start.DistanceTo(pb);
                double L2p = _end.DistanceTo(pb);
                double L3p = line._start.DistanceTo(pb);
                double L4p = line._end.DistanceTo(pb);

                // se l'intersezione esiste ed è compresa nella lunghezza dei segmenti => true
                if (((L12 - L1p) >= tolerance || (L12 == L1p)) &&
                    ((L12 - L2p) >= tolerance || (L12 == L2p)) &&
                    ((L34 - L3p) >= tolerance || (L34 == L3p)) &&
                    ((L34 - L4p) >= tolerance || (L34 == L4p)))
                {
                    if (Math.Abs(pa.X - pb.X) <= tol && Math.Abs(pa.Y - pb.Y) <= tol && Math.Abs(pa.Z - pb.Z) <= tol)
                    {
                        inters = pa;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get the intersection of the infinite lines through the two segments
        /// </summary>
        /// <param name="line">The other line</param>
        /// <param name="inters">The intersection point, null if there is none</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <returns>True if the lines have a single intersection point. Return false if the lines are parallel, coincident or overlap</returns>
        public bool GetIntersectionWithInfiniteLine(Line3d line, out Point3d inters, double tolerance = GeometryBase.Tolerance)
        {
            // http://paulbourke.net/geometry/pointlineplane                 
            // http://paulbourke.net/geometry/pointlineplane/lineline.c  

            // Il  metodo torna vero SOLO se l'intersezione esiste
            // ed è unica => Point3d inters
            // Se le linee sono parallele ma non sovrapposte torna giustamente falso
            // Se le linee sono parallele e in parte sovrapposte torna falso
            // Se le linee sono coincidenti torna falso.
            // se le linee si intersecano sugli estremi (a meno che non siano sovrapposte) torna vero.

            double tol = Utilities.Maths.ErrorPropagation.SumTolerance(tolerance, tolerance);

            if (GetIntersectionHelper(line, tolerance, out Point3d pa, out Point3d pb, out inters))
            {
                if (Math.Abs(pa.X - pb.X) <= tol && Math.Abs(pa.Y - pb.Y) <= tol && Math.Abs(pa.Z - pb.Z) <= tol)
                {
                    inters = pa;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The common part of the intersections: the points of the two infinite lines at the minimum distance and the cases of the segments that
        /// share an end
        /// </summary>
        /// <param name="line">The other segment</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <param name="pa">The point of this line nearest to the other line</param>
        /// <param name="pb">The point of the other line nearest to this line</param>
        /// <param name="inters">The common end when the segments share an end (and do not overlap), otherwise null</param>
        /// <returns>False if the lines are parallel, coincident, overlapping or degenerate (a point); true otherwise</returns>
        private bool GetIntersectionHelper(Line3d line, double tolerance, out Point3d pa, out Point3d pb, out Point3d inters)
        {
            inters = null;

			Point3d p43 = line.End - line.Start;
			Point3d p21 = _end - _start;
			Point3d p13 = _start - line.Start;

			double d1343 = p13 * p43;
			double d4321 = p43 * p21;
			double d1321 = p13 * p21;
			double d4343 = p43 * p43;
			double d2121 = p21 * p21;

			double denom = d2121 * d4343 - d4321 * d4321;
			double numer = d1343 * d4321 - d1321 * d4343;
			double mua = numer / denom;
			double mub = (d1343 + d4321 * (mua)) / d4343;

			pa = new Point3d(_start.X + mua * p21.X, _start.Y + mua * p21.Y, _start.Z + mua * p21.Z);
			pb = new Point3d(line.Start.X + mub * p43.X, line.Start.Y + mub * p43.Y, line.Start.Z + mub * p43.Z);

			if (_start.Equals(line.Start, tolerance))
			{
				if (IsPointOnInfiniteLine(line.End, tolerance))
				{
					if (!IsPointOnLine(line.End, tolerance))
					{
						inters = _start;
						if (!line.IsPointOnLine(_end, tolerance))
						{
							return true;
						}
						else
							return false;
					}
                    return false;
				}
				else
				{
					inters = _start;
					return true;
				}
			}

			if (_start.Equals(line.End, tolerance))
			{
				if (IsPointOnInfiniteLine(line.Start, tolerance))
				{
					if (!IsPointOnLine(line.Start, tolerance))
					{
						inters = _start;
						if (!line.IsPointOnLine(_end, tolerance))
						{
							return true;
						}
						else
							return false;
					}
					return false;
				}
				else
				{
					inters = _start;
					return true;
				}
			}

			if (_end.Equals(line.Start, tolerance))
			{
				if (IsPointOnInfiniteLine(line.End, tolerance))
				{
					if (!IsPointOnLine(line.End, tolerance))
					{
						inters = _end;
						if (!line.IsPointOnLine(_start, tolerance))
						{
							return true;
						}
						else
							return false;
					}
					return false;
				}
				else
				{
					inters = _end;
					return true;
				}
			}

			if (_end.Equals(line.End, tolerance))
			{
				if (IsPointOnInfiniteLine(line.Start, tolerance))
				{
					if (!IsPointOnLine(line.Start, tolerance))
					{
						inters = _end;
						if (!line.IsPointOnLine(_start, tolerance))
						{
							return true;
						}
						else
							return false;
					}
					return false;
				}
				else
				{
					inters = _end;
					return true;
				}
			}

			// se le due linee coincidono => false
			if ((_start.Equals(line.Start, tolerance) || _start.Equals(line.End, tolerance)) && (_end.Equals(line.End, tolerance) || _end.Equals(line.Start, tolerance)))
				return false;

			// se le due linee si intersecano per start e sono sovrapposte => falso
			if ((_start.Equals(line.Start, tolerance) && IsPointOnLine(line.End, tolerance)))
				return false;

			// se le due linee si intersecano per start e sono sovrapposte => falso
			if (_start.Equals(line.End, tolerance) && IsPointOnLine(line.Start, tolerance))
				return false;

			// se le due linee si intersecano per End e sono sovrapposte => falso
			if ((_start.Equals(line.Start, tolerance) && IsPointOnLine(line.End, tolerance)))
				return false;

			// se le due linee si intersecano per End e sono sovrapposte => falso
			if (_start.Equals(line.End, tolerance) && IsPointOnLine(line.Start, tolerance))
				return false;

			// le due rette sono parallele => false
			if (Math.Abs(denom) <= AngularTolerance * AngularTolerance * d2121 * d4343)
				return false;

			// la linea 2 degenera in un punto => false
			if (Math.Abs(p43.X) < tolerance && Math.Abs(p43.Y) < tolerance && Math.Abs(p43.Z) < tolerance)
				return false;

			// la linea 1 degenera in un punto => false
			if (Math.Abs(p21.X) < tolerance && Math.Abs(p21.Y) < tolerance && Math.Abs(p21.Z) < tolerance)
				return false;

            return true;
		}

		/// <summary>
		/// Get the intersection with the segment from the origin to <paramref name="vector"/>
		/// </summary>
		/// <param name="vector">The vector (the segment from the origin to its end)</param>
		/// <param name="inters">The intersection point, null if there is none</param>
		/// <param name="tolerance">The tolerance on the distances</param>
		/// <returns>True if the infinite lines intersect in a point of this segment. Return false if the line and the vector are parallel or overlap</returns>
		public bool GetIntersection(Vector3d vector, out Point3d inters, double tolerance = GeometryBase.Tolerance)
        {
            Line3d vectorLine = new Line3d(Point3d.Origin, new Point3d(vector.X, vector.Y, vector.Z));

            bool isIntersect = GetIntersectionWithInfiniteLine(vectorLine, out inters, tolerance);

            if (isIntersect == false)
            {
                inters = null;
                return false;
            }
            else
            {
                if (IsPointOnLine(inters))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the distance from the infinite line to the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <returns>The distance</returns>
        public double DistanceTo(Point3d point)
        {
            // http://paulbourke.net/geometry/pointlineplane/

            double u = (((point.X - _start.X) * (_end.X - _start.X)) + ((point.Y - _start.Y) * (_end.Y - _start.Y)) +
                ((point.Z - _start.Z) * (_end.Z - _start.Z))) / Math.Pow(GetLength(), 2);

            //if (u < 0.0 || u > 1.0)
            //    return 0;   // Closest point does not fall within the line segment

            double x = _start.X + u * (_end.X - _start.X);
            double y = _start.Y + u * (_end.Y - _start.Y);
            double z = _start.Z + u * (_end.Z - _start.Z);
            Point3d intersection = new Point3d(x, y, z);

            return intersection.DistanceTo(point);
        }

        /// <summary>
        /// Clone and move the cloned line with the vector <paramref name="movement"/>
        /// </summary>
        /// <param name="movement">The movement vector</param>
        /// <returns>The new line moved</returns>
        public Line3d CloneAndMove(Vector3d movement)
        {
            var p = (Line3d)Clone();
            p.Move(movement);
            return p;
        }

        /// <summary>
        /// Clone the line
        /// </summary>
        /// <returns>The new object cloned</returns>
        public override object Clone()
        {
            return new Line3d(this);
        }

        /// <summary>
        /// Move line by an given increment dx, dy, dz
        /// </summary>
        /// <param name="dx">The X coordinate increment</param>
        /// <param name="dy">The Y coordinate increment</param>
        /// <param name="dz">The Z coordinate increment</param>
        public override void Move(double dx, double dy, double dz)
        {
            _start.Move(dx, dy, dz);
            _end.Move(dx, dy, dz);
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

        /// <summary>
        /// Tell if the given point is on the segment
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="tolerance">The tolerance: the cross product of the vectors from the point to the ends is compared with the propagated tolerance</param>
        /// <returns>True if the point is aligned with the ends and between them</returns>
        /// <exception cref="ArgumentNullException">Thrown when the point parameter is null</exception>
        public bool IsPointOnLine(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            if (point == null)
            {
                throw new ArgumentNullException("Point can not be null");
            }

            // http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
            // If the cross product of the vectors n1 and n2 is zero in all directions then the points are collinear, 
            // n1 and n2 are the vectors connecting one point to the other two points

            Vector3d v1 = new Vector3d((_start.X - point.X), (_start.Y - point.Y), (_start.Z - point.Z));
            Vector3d v2 = new Vector3d((_end.X - point.X), (_end.Y - point.Y), (_end.Z - point.Z));

            Vector3d v3 = v1.CrossProduct(v2);

            double tol = Utilities.Maths.ErrorPropagation.ProductTolerance(v1.Length, v2.Length, tolerance, tolerance);

            double L1 = _start.DistanceTo(point);
            double L2 = _end.DistanceTo(point);
            double L = GetLength();

            if (Math.Abs(v3.X) < tol && Math.Abs(v3.Y) < tol && Math.Abs(v3.Z) < tol && Math.Abs(L1) <= Math.Abs(L) && Math.Abs(L2) <= Math.Abs(L))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tell if the given point is on the mathematical (infinite) line through the segment
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <returns>True if the point is on the mathematical line</returns>
        public bool IsPointOnInfiniteLine(Point3d point, double tolerance = GeometryBase.Tolerance)
        {
            return Ray.IsPointOnRay(point, tolerance);
        }

        /// <summary>
        /// Scale the line by the parameter factor respet of the origin
        /// </summary>
        /// <param name="factor">The scale factor</param>
        /// <returns>A new line scaled</returns>
        public Line3d Scale(double factor)
        {
            Point3d start = Start;
            Point3d end = End;
            start *= factor;
            end *= factor;
            return new Line3d(start, end);
        }

        /// <summary>
        /// Split the segment by an array of points: the points on the segment, not at its ends, are the ends of the parts
        /// </summary>
        /// <param name="Points">The points (the ones out of the segment are ignored)</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>The parts, from the start to the end</returns>
        public Line3d[] Split(Point3d[] Points, double tolerance = GeometryBase.Tolerance)
        {
            List<double> param = new List<double>();

            if (Points != null)
            {
                for (int i = 0; i < Points.Count(); i++)
                {
                    if (IsPointOnLine(Points[i], tolerance))
                    {
                        double p = Math.Abs(Math.Abs(Points[i].DistanceTo(_start)) / Math.Abs(_start.DistanceTo(_end)));
                        if ((p > tolerance && Math.Abs(p - 1) > tolerance))
                            param.Add(p);
                    }
                }
            }
            double[] paramToArray = param.ToArray();
            return Split(paramToArray, tolerance);
        }

        /// <summary>
        /// Split the segment by parameters
        /// </summary>
        /// <param name="parameters">The input parameters (0 - start, 1 - end); the ones not strictly inside (within the tolerance) and the repeated
        /// ones are ignored</param>
        /// <param name="tolerance">The tolerance on the parameters</param>
        /// <returns>The parts, from the start to the end (the whole segment if there are no parameters)</returns>
        public Line3d[] Split(double[] parameters, double tolerance = GeometryBase.Tolerance)
        {
            List<double> param = new List<double>();
            if (parameters.Count() != 0)
            {
                for (int i = 0; i < parameters.Count(); i++)
                {
                    // strictly inside the line: parameters close to 0 or 1 would create zero length lines
                    if (parameters[i] > tolerance && (1.0 - parameters[i]) > tolerance)
                    {
                        if (!param.Contains(parameters[i]))
                            param.Add(parameters[i]);
                    }
                }
            }
            double[] paramArray = param.ToArray();
            Array.Sort(paramArray);

            return Split(paramArray);
        }

        /// <summary>
        /// Split the segment at sorted parameters
        /// </summary>
        /// <param name="paramArray">The parameters, sorted, strictly between 0 and 1</param>
        /// <returns>The parts, from the start to the end (a new segment with the same end points if there are no parameters)</returns>
        private Line3d[] Split(double[] paramArray)
        {
            List<Point3d> points = new List<Point3d>();
            List<Line3d> lineList = new List<Line3d>();

            if (paramArray.Count() != 0)
            {
                for (int i = 0; i <= paramArray.Count(); i++)
                {
                    if (i < paramArray.Count())
                    {
                        points.Add(new Point3d(_start + paramArray[i] * (_end - _start)));
                    }
                    if (i == 0)
                    {
                        lineList.Add(new Line3d(_start, points[i]));
                    }
                    else if (i == paramArray.Count())
                    {
                        lineList.Add(new Line3d(points[i - 1], _end));
                    }
                    else
                    {
                        lineList.Add(new Line3d(points[i - 1], points[i]));
                    }
                }
            }
            else
            {
                lineList.Add(new Line3d(_start, _end));
            }

            return lineList.ToArray();
        }

        /// <summary>
        /// Split the segment in <paramref name="subdivision"/> equal parts
        /// </summary>
        /// <param name="subdivision">The number of parts (0: no segment, 1: this segment)</param>
        /// <returns>The parts, from the start to the end</returns>
        public Line3d[] Split(int subdivision)
        {
            if (subdivision == 0)
                return new Line3d[0];

            else if (subdivision == 1)
                return new Line3d[] { this };

            else
            {
                double[] parameters = new double[subdivision - 1];
                double step = 1.0 / subdivision;

                for (int i = 0; i < subdivision - 1; i++)
                    parameters[i] = step * (1 + i);

                return Split(parameters);
            }
        }

        /// <summary>
        /// Converts the line on a Vector3d
        /// </summary>
        /// <returns>The vector from Start to End of the line</returns>
        public Vector3d ToVector()
        {
            return _end - _start;
        }

		/// <summary>
		/// Calculates the intersection of a segment and a semi-infinite line.
		/// </summary>
		/// <param name="semiRay">Semi infinite line (ray), which begins at first point and is infinite in the direction of the end point.</param>
		/// <param name="inters">Point of intersection if any.</param>
		/// <param name="tolerance">The tolerance on the distance between the lines</param>
		/// <returns>True if the ray crosses the segment strictly between its ends, in front of the start of the ray</returns>
		public bool GetIntersectionWihtSemiInfiniteRay(in Line3d semiRay, out Point3d inters, double tolerance = GeometryBase.Tolerance)
        {
            inters = null;

            CalcShortestLineBetweenTwoRays(semiRay, out double mua, out double mub, out Point3d Pa, out Point3d Pb);

            if (Pa == null || Pb == null)
                return false;

            if (mub > 0.0 && mua > 0.0 && mua < 1.0 && Pa.DistanceTo(Pb) < tolerance)
            {
                inters = Pb;
                return true;
            }
            else
                return false;
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
        internal void CalcShortestLineBetweenTwoRays(Line3d ray, out double mua, out double mub, out Point3d Pa, out Point3d Pb)
        {
            Ray.CalcShortestLineBetweenTwoRays(ray.Ray, out mua, out mub, out Pa, out Pb);  
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// Equality: true if the segments are the same object, both null or equal (<see cref="Equals(Line3d)"/>)
        /// </summary>
        /// <param name="line1">The first segment</param>
        /// <param name="line2">The second segment</param>
        /// <returns>True if the segments are equal</returns>
        public static bool operator ==(Line3d line1, Line3d line2)
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
        public static bool operator !=(Line3d line1, Line3d line2)
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
            info.AddValue("Start", _start, typeof(Point3d));
            info.AddValue("End", _end, typeof(Point3d));
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Line3d)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal segment</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Line3d);
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Line3d)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal segment</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Line3d line)
                return Equals(line);

            return false;
        }

        /// <summary>
        /// Equality of the ends within the tolerance, in either direction
        /// </summary>
        /// <param name="other">The segment to compare</param>
        /// <returns>True if Start and End of line and otherLine are equals.
        /// True also if otherLine is flipped (line.Start == otherLine.End AND line.End == otherLine.Start)</returns>
        public bool Equals(Line3d other)
        {
            return !(other is null) && ((_start.Equals(other.Start) && _end.Equals(other.End)) || (_start.Equals(other.End) && _end.Equals(other.Start)));
        }

        /// <summary>
        /// The hash code, independent of the direction
        /// </summary>
        /// <returns>The hashCode. If (line.Start == otherLine.End AND line.End == otherLine.Start), line and otherLine returns the same hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode1 = -23;
                hashCode1 += -17 * base.GetHashCode();
                hashCode1 = hashCode1 + -17 * EqualityComparer<Point3d>.Default.GetHashCode(_start) + EqualityComparer<Point3d>.Default.GetHashCode(_end);

                int hashCode2 = -23;
                hashCode2 += -17 * base.GetHashCode();
                hashCode2 = hashCode2 + -17 * EqualityComparer<Point3d>.Default.GetHashCode(_end) + EqualityComparer<Point3d>.Default.GetHashCode(_start);

                return hashCode1 + hashCode2;
            }
        }

        /// <summary>
        /// A description of the segment
        /// </summary>
        /// <returns>"Start: ... End: ..."</returns>
        public override string ToString()
        {
            return $"Start: {_start} End: {_end}";
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>See <see cref="ToString"/></returns>
        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        #endregion
    }
}
