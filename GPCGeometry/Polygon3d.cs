using GPC.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GPC.Geometry
{
	/// <summary>
	/// Polygon 3d is a planar polygon on the (x,y,z) space
	/// </summary>
	[Serializable]
	public sealed class Polygon3d : GeometryBase, IEnumerable<Point3d>, ISerializable, ICloneable, IEquatable<Polygon3d>
	{
		#region Variables

		private Point3d[] _points;

		#endregion

		#region Properties

		public int Count => _points.Length;

		public Point3d this[int index] => _points[index];

		public Point3d[] Points { get => _points; set => _points = value; }

		#endregion

		#region Public Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <exception cref="ArgumentException">Thrown when the polygon is not planar</exception>
		public Polygon3d()
		{
			_points = new Point3d[0];
		}

		/// <summary>
		/// Create a new polygon from a point array
		/// </summary>
		/// <param name="points">The polygon vertices</param>
		/// <param name="tolerance"></param>
		/// <exception cref="ArgumentException">Thrown when the polygon is not planar</exception>
		public Polygon3d(Point3d[] points, double tolerance = GeometryBase.Tolerance)
			: this()
		{
			if (points != null)
			{
				AddRange(points);
				if (!IsPlanar(tolerance))
					throw new ArgumentException($"Polygon is not planar");
			}
		}

		/// <summary>
		/// Create a new polygon from a point array 
		/// </summary>
		/// <param name="points">The polygon vertices</param>
		public Polygon3d(Point3d[] points)
			: this()
		{
			if (points != null)
				AddRange(points);
		}

		/// <summary>
		/// Create a new polygon from a point array
		/// </summary>
		/// <param name="points">The polygon vertices</param>
		/// <param name="tolerance"></param>
		/// <exception cref="ArgumentException">Thrown when the polygon is not planar</exception>
		public Polygon3d(IEnumerable<Point3d> points, double tolerance = GeometryBase.Tolerance)
			: this()
		{
			if (points != null)
			{
				AddRange(points);
				if (!IsPlanar(tolerance))
					throw new ArgumentException($"Polygon is not planar");
			}
		}

		public Polygon3d(Polygon3d polygon, double tolerance = GeometryBase.Tolerance)
			: this()
		{
			for (int i = 0; i < polygon.Count; i++)
			{
				Add(new Point3d(polygon[i]), tolerance);
			}
		}

		public Polygon3d(Polygon3d polygon)
		{
			// one allocation (AddWithoutChecks copied the array at every point)
			_points = new Point3d[polygon.Count];
			for (int i = 0; i < _points.Length; i++)
				_points[i] = new Point3d(polygon[i]);
		}

		public Polygon3d(Polygon2d polygon)
		{
			_points = new Point3d[polygon.Count];
			for (int i = 0; i < _points.Length; i++)
				_points[i] = new Point3d(polygon[i]);
		}

		/// <remarks>The points of a <see cref="Polygon2d"/> are on the plane z = 0: they are copied in one allocation without the planarity check
		/// (before, Add copied the array at every point: O(n^2))</remarks>
		public Polygon3d(Polygon2d polygon, double tolerance = GeometryBase.Tolerance)
			: this(polygon)
		{
		}

		/// <remarks>The count is read once as Int32 (before, GetInt16 at every iteration: overflow over 32767 points).
		/// The points are copied without the planarity check: the polygon was valid when it was serialized, also if it was created
		/// with a tolerance larger than the default one (before, Add checked it again with the default tolerance)</remarks>
		private Polygon3d(SerializationInfo info, StreamingContext context)
		{
			int count = info.GetInt32("Count");
			var points = new List<Point3d>(count);

			for (int i = 0; i < count; i++)
			{
				Point3d point = (Point3d)info.GetValue($"Point{i}", typeof(Point3d));
				if (point != null)
					points.Add(point);
			}

			_points = points.ToArray();
		}

		#endregion

		#region Add/ Addrange / RemoveAt / Insert

		/// <summary>
		/// Add a new point to the polygon
		/// </summary>
		/// <param name="point">The point to add</param>
		/// <param name="tolerance"></param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar</exception>
		/// <remarks>Only the new point is checked, against a plane of three points of the polygon kept up to date by the previous calls (O(1)).
		/// The whole polygon is checked only if the new point is not on that plane</remarks>
		public void Add(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			if (point == null)
				return;

			if (!IsPlaneBasisValid(tolerance))
				ComputePlaneBasis(tolerance);

			AddWithoutChecks(point);

			bool isOnPlane = UpdatePlaneBasis(point, _points.Length - 1, tolerance);

			if (!isOnPlane)
			{
				_planeBasisIndices = null; // it will be computed again with the new point

				if (!IsPlanar(tolerance))
					throw new ArgumentException($"Polygon with additional point {point} is not planar");
			}
		}

		#region Plane basis used by Add

		// Up to three points of the polygon (index and reference) that define its plane (3 points), its line (2 points: the points are aligned)
		// or its position (1 point: the points are coincident). The references tell if the points have been changed by other methods
		[NonSerialized]
		private int[] _planeBasisIndices;
		[NonSerialized]
		private Point3d[] _planeBasisPoints;
		[NonSerialized]
		private double _planeBasisTolerance;

		private bool IsPlaneBasisValid(double tolerance)
		{
			if (_planeBasisIndices == null || _planeBasisTolerance != tolerance)
				return false;

			for (int k = 0; k < _planeBasisIndices.Length; k++)
			{
				int i = _planeBasisIndices[k];
				if (i >= _points.Length || !ReferenceEquals(_points[i], _planeBasisPoints[k]))
					return false;
			}

			return true;
		}

		private void SetPlaneBasis(double tolerance, params int[] indices)
		{
			_planeBasisIndices = indices;
			_planeBasisPoints = new Point3d[indices.Length];
			for (int k = 0; k < indices.Length; k++)
				_planeBasisPoints[k] = _points[indices[k]];
			_planeBasisTolerance = tolerance;
		}

		/// <summary>
		/// Choose the basis among all the points: the first point, the farthest point from it and the farthest point from the line of the two
		/// </summary>
		private void ComputePlaneBasis(double tolerance)
		{
			if (_points.Length == 0)
			{
				SetPlaneBasis(tolerance);
				return;
			}

			int b = -1;
			double maxDistance = tolerance;
			for (int i = 1; i < _points.Length; i++)
			{
				double distance = _points[0].DistanceTo(_points[i]);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					b = i;
				}
			}

			if (b < 0)
			{
				SetPlaneBasis(tolerance, 0);
				return;
			}

			int c = -1;
			maxDistance = tolerance;
			for (int i = 1; i < _points.Length; i++)
			{
				double distance = DistanceFromLine(_points[0], _points[b], _points[i]);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					c = i;
				}
			}

			if (c < 0)
				SetPlaneBasis(tolerance, 0, b);
			else
				SetPlaneBasis(tolerance, 0, b, c);
		}

		/// <summary>
		/// Update the basis with the point just added at <paramref name="index"/>
		/// </summary>
		/// <returns>False if the basis is a plane and the point is not on it</returns>
		private bool UpdatePlaneBasis(Point3d point, int index, double tolerance)
		{
			int[] basis = _planeBasisIndices;

			switch (basis.Length)
			{
				case 0:
					SetPlaneBasis(tolerance, index);
					return true;

				case 1:
					if (_points[basis[0]].DistanceTo(point) > tolerance)
						SetPlaneBasis(tolerance, basis[0], index);
					return true;

				case 2:
				{
					Point3d a = _points[basis[0]];
					Point3d b = _points[basis[1]];
					if (DistanceFromLine(a, b, point) > tolerance)
						SetPlaneBasis(tolerance, basis[0], basis[1], index);
					else if (a.DistanceTo(point) > a.DistanceTo(b))
						SetPlaneBasis(tolerance, basis[0], index); // longer line, better defined
					return true;
				}

				default:
				{
					Point3d a = _points[basis[0]];
					Point3d b = _points[basis[1]];
					Point3d c = _points[basis[2]];

					double abX = b.X - a.X, abY = b.Y - a.Y, abZ = b.Z - a.Z;
					double acX = c.X - a.X, acY = c.Y - a.Y, acZ = c.Z - a.Z;
					double nX = abY * acZ - abZ * acY;
					double nY = abZ * acX - abX * acZ;
					double nZ = abX * acY - abY * acX;
					double nLength = Math.Sqrt(nX * nX + nY * nY + nZ * nZ);

					double distanceFromPlane = Math.Abs(nX * (point.X - a.X) + nY * (point.Y - a.Y) + nZ * (point.Z - a.Z)) / nLength;
					if (!(distanceFromPlane < tolerance))
						return false;

					if (DistanceFromLine(a, b, point) > DistanceFromLine(a, b, c))
						SetPlaneBasis(tolerance, basis[0], basis[1], index); // wider triangle, better defined plane
					return true;
				}
			}
		}

		/// <returns>The distance of <paramref name="point"/> from the infinite line through <paramref name="a"/> and <paramref name="b"/></returns>
		private static double DistanceFromLine(Point3d a, Point3d b, Point3d point)
		{
			double abX = b.X - a.X, abY = b.Y - a.Y, abZ = b.Z - a.Z;
			double apX = point.X - a.X, apY = point.Y - a.Y, apZ = point.Z - a.Z;
			double cX = abY * apZ - abZ * apY;
			double cY = abZ * apX - abX * apZ;
			double cZ = abX * apY - abY * apX;
			return Math.Sqrt((cX * cX + cY * cY + cZ * cZ) / (abX * abX + abY * abY + abZ * abZ));
		}

		#endregion

		/// <summary>
		/// Add a new point to the polygon. This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="point"></param>
		internal void AddWithoutChecks(Point3d point)
		{
			if (point != null)
			{
				Point3d[] pointBuffer = _points;
				_points = new Point3d[_points.Length + 1];
				for (int i = 0; i < pointBuffer.Length; i++)
					_points[i] = pointBuffer[i];
				_points[_points.Length - 1] = point;
			}
		}

		/// <summary>
		/// Add a range of points to the polygon. This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="point"></param>
		internal void AddWithoutChecks(Point3d[] point)
		{
			if (point != null)
			{
				AddRange(point);
			}
		}

		/// <summary>
		/// Add a new point to the polygon
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar</exception>
		public void Add(double x, double y, double z)
		{
			Add(new Point3d(x, y, z));
		}

		/// <summary>
		/// Add a range of points to the polygon
		/// </summary>
		/// <param name="points"></param>
		internal void AddRange(Point3d[] points)
		{
			if (points != null)
			{
				Point3d[] pointBuffer = _points;
				_points = new Point3d[_points.Length + points.Length];

				for (int i = 0; i < pointBuffer.Length; i++)
					_points[i] = pointBuffer[i];

				for (int i = 0; i < points.Length; i++)
					_points[pointBuffer.Length + i] = points[i];
			}
		}

		/// <summary>
		/// Add a range of points to the polygon
		/// </summary>
		/// <param name="points"></param>
		internal void AddRange(IEnumerable<Point3d> points)
		{
			AddRange(points.ToArray());
		}

		/// <summary>
		/// Insert a new point at the position given by index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="point"></param>
		/// <param name="tolerance"></param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar</exception>            
		public void Insert(int index, Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			Insert(index, point);
			if (!IsPlanar(tolerance))
			{
				throw new ArgumentException($"Polygon with additional point {point} is not planar");
			}
		}

		/// <summary>
		/// Insert a new point at the position given by index. This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="index"></param>
		/// <param name="point"></param>          
		public void Insert(int index, Point3d point)
		{
			Point3d[] pointsBuffer = _points;
			_points = new Point3d[_points.Length + 1];
			for (int i = 0; i < _points.Length; i++)
			{
				if (i < index)
					_points[i] = pointsBuffer[i];
				else if (i == index)
					_points[i] = point;
				else
					_points[i] = pointsBuffer[i - 1];
			}
		}

		/// <summary>
		/// Insert a new point at the position given by index. This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="index"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void Insert(int index, double x, double y, double z)
		{
			Insert(index, new Point3d(x, y, z));
		}

		/// <summary>
		/// Return the index of point ad index <paramref name="i"/> - 1
		/// </summary>
		/// <param name="i"></param>
		/// <returns>Return the previous index of point of the polygon</returns>
		/// <exception cref="IndexOutOfRangeException">If i > <see cref="Count" - 1/></exception>
		public int GetPreviousIndex(int i)
		{
			if (i > 0 && i < _points.Length)
			{
				return i - 1;
			}
			else if (i == 0)
			{
				return _points.Length - 1;
			}
			else
			{
				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Return the point ad index <paramref name="i"/> - 1
		/// </summary>
		/// <param name="i"></param>
		/// <returns>Return the previous point of the polygon</returns>
		public Point3d GetPreviousPoint(int i)
		{
			return this[GetPreviousIndex(i)];
		}

		/// <summary>
		/// Return the index of point ad index <paramref name="i"/> + 1
		/// </summary>
		/// <param name="i"></param>
		/// <returns>Return the next index of point of the polygon</returns>
		/// <exception cref="IndexOutOfRangeException">If i > <see cref="Count" - 1/></exception>
		public int GetNextIndex(int i)
		{
			if (i < _points.Length - 1)
			{
				return i + 1;
			}
			else if (i == _points.Length - 1)
			{
				return 0;
			}
			else
			{
				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Return the point ad index <paramref name="i"/> + 1
		/// </summary>
		/// <param name="i"></param>
		/// <returns>Return the next point of the polygon</returns>
		public Point3d GetNextPoint(int i)
		{
			return this[GetNextIndex(i)];
		}

		/// <summary>
		/// Remove point ad index <paramref name="index"/>
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			Point3d[] pointsBuffer = _points;
			_points = new Point3d[_points.Length - 1];
			for (int i = 0; i < pointsBuffer.Length; i++)
			{
				if (i < index)
					_points[i] = pointsBuffer[i];
				else if (i == index)
				{ }
				else
					_points[i - 1] = pointsBuffer[i];
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Tell if a point already exists in the polygon
		/// </summary>
		/// <param name="point">The point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point already exists</returns>
		public bool PointExists(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			for (int i = 0; i < _points.Length; i++)
			{
				if (_points[i].DistanceTo(point) < tolerance)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Tell if a point already exists in the polygon
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="tolerance"></param>
		/// <returns>True if the point already exists</returns>
		public bool PointExists(double x, double y, double z, double tolerance = GeometryBase.Tolerance)
		{
			return PointExists(new Point3d(x, y, z), tolerance);
		}

		/// <summary>
		/// Tell if a point is on an edge
		/// </summary>
		/// <param name="point">The point to test</param>
		/// <param name="tolerance"></param>
		/// <returns>The index of the edge where the point is on, otherwise -1</returns>
		public int IsPointOnEdge(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			if (point == null)
				throw new ArgumentNullException("Point can not be null");

			for (int i = 0; i < _points.Length; i++)
			{
				if (IsPointOnSegment(_points[i], GetNextPoint(i), point, tolerance))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Same test of <see cref="Line3d.IsPointOnLine(Point3d, double)"/>, without creating lines and vectors
		/// </summary>
		private static bool IsPointOnSegment(Point3d start, Point3d end, Point3d point, double tolerance)
		{
			double v1X = start.X - point.X, v1Y = start.Y - point.Y, v1Z = start.Z - point.Z;
			double v2X = end.X - point.X, v2Y = end.Y - point.Y, v2Z = end.Z - point.Z;

			double crossX = v1Y * v2Z - v1Z * v2Y;
			double crossY = -(v1X * v2Z - v1Z * v2X);
			double crossZ = v1X * v2Y - v1Y * v2X;

			double tol = Utilities.Maths.ErrorPropagation.ProductTolerance(Math.Sqrt(v1X * v1X + v1Y * v1Y + v1Z * v1Z), Math.Sqrt(v2X * v2X + v2Y * v2Y + v2Z * v2Z), tolerance, tolerance);

			if (!(Math.Abs(crossX) < tol && Math.Abs(crossY) < tol && Math.Abs(crossZ) < tol))
				return false;

			double length = start.DistanceTo(end);
			return start.DistanceTo(point) <= length && end.DistanceTo(point) <= length;
		}

		/// <summary>
		/// Tell if a point is on an edge
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="tolerance"></param>
		/// <returns>The index of the edge where the point is on, otherwise -1</returns>
		public int IsPointOnEdge(double x, double y, double z, double tolerance = GeometryBase.Tolerance)
		{
			return IsPointOnEdge(new Point3d(x, y, z), tolerance);
		}

		/// <summary>
		/// Calculate each edge of the polygon3d
		/// </summary>
		/// <returns>Array of Line3d</returns>
		public Line3d[] Explode()
		{
			Line3d[] lines = new Line3d[_points.Length];

			for (int i = 0; i < _points.Length; i++)
				lines[i] = new Line3d(this[i], GetNextPoint(i));

			return lines;
		}

		/// <summary>
		/// Remove duplicated points if they have the same coordinates.
		/// </summary>
		/// <param name="tolerance">Tolerance</param>
		public void RemoveDuplicatedPoints(double tolerance = GeometryBase.Tolerance)
		{
			if (_points.Length == 1) { return; }

			for (int i = Count - 1; i > -1; i--)
			{
				int nextI;
				if (i == Count - 1)
				{
					nextI = 0;
				}
				else
				{
					nextI = i + 1;
				}
				if (this[nextI].DistanceTo(this[i]) < tolerance)
				{
					if (i == Count - 1)
					{
						RemoveAt(Count - 1);
					}
					else
					{
						RemoveAt(i + 1);
					}
				}
			}
		}

		/// <summary>
		/// Move polygon by an given increment dx, dy, dz
		/// </summary>
		/// <param name="dx">The X coordinate increment</param>
		/// <param name="dy">The Y coordinate increment</param>
		/// <param name="dz">The Z coordinate increment</param>
		public override void Move(double dx, double dy, double dz)
		{
			for (int i = 0; i < _points.Length; i++)
				_points[i].Move(dx, dy, dz);
		}

		/// <summary>
		/// Move polygon by a given vector
		/// </summary>
		/// <param name="vector">Displacement vector</param>
		public override void Move(Vector3d vector)
		{
			Move(vector.X, vector.Y, vector.Z);
		}

		/// <summary>
		/// Calculate the area, positive if polygon is rightOriented
		/// </summary>
		/// <returns>The area of the polygon</returns>
		/// <remarks>The area is positive if the polygon normal (right hand rule on the vertices order) has a positive component along the global Z axis,
		/// negative otherwise (vertical polygons included)</remarks>
		public double GetSignedArea(double tolerance = GeometryBase.Tolerance)
		{
			//TODO: testare con poligoni auto intersecanti
			// https://math.stackexchange.com/a/2152697

			// Area è la lunghezza di un vettore calcolato come somma dei cross product
			// di tutti i vertici con un punto che sta nel piano
			// se il vettore è orientato verso le Z globali positive allora l'area è positiva

			if (Count < 3)
				return 0;

			Vector3d area = GetAreaVector();
			double norm = area.Length;

			return area.Z > 0 ? norm : -norm;
		}

		/// <summary>
		/// Newell area vector: its direction is the normal of the polygon (right hand rule on the vertices order) and its length is the area.
		/// Correct also for concave polygons and for polygons in any plane
		/// </summary>
		private Vector3d GetAreaVector()
		{
			double x = 0, y = 0, z = 0;
			Point3d basePoint = _points[0];

			for (int i = 1; i < _points.Length - 1; i++)
			{
				double ax = _points[i].X - basePoint.X;
				double ay = _points[i].Y - basePoint.Y;
				double az = _points[i].Z - basePoint.Z;
				double bx = _points[i + 1].X - basePoint.X;
				double by = _points[i + 1].Y - basePoint.Y;
				double bz = _points[i + 1].Z - basePoint.Z;

				x += ay * bz - az * by;
				y += az * bx - ax * bz;
				z += ax * by - ay * bx;
			}

			return new Vector3d(0.5 * x, 0.5 * y, 0.5 * z);
		}

		/// <summary>
		/// Get the vector normal of the polygon
		/// </summary>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>A unitized vector normal to the polygon</returns>
		/// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a normal vector</exception>
		/// <remarks>The normal is the unitized Newell area vector (right hand rule on the vertices order), so it is correct also for concave polygons.
		/// Only for degenerate polygons (area close to zero) the normal is computed from the first three not aligned points</remarks>
		public Vector3d GetNormalVector(double tolerance = GeometryBase.Tolerance)
		{
			if (TryGetUnitNormal(tolerance, out Vector3d normal))
				return normal;

			return GetNormalVectorFromFirstPoints(tolerance);
		}

		/// <summary>
		/// Unit normal from the Newell area vector
		/// </summary>
		/// <returns>False if the polygon has less than three points or if its area is too small, compared with its perimeter, to define a normal</returns>
		internal bool TryGetUnitNormal(double tolerance, out Vector3d normal)
		{
			normal = null;
			if (_points.Length < 3)
				return false;

			Vector3d area = GetAreaVector();
			double length = area.Length;

			if (!(length > tolerance * GetPerimeter()))
				return false;

			normal = new Vector3d(area.X / length, area.Y / length, area.Z / length);
			return true;
		}

		private double GetPerimeter()
		{
			double perimeter = 0;
			for (int i = 0; i < _points.Length; i++)
				perimeter += _points[i].DistanceTo(_points[(i + 1) % _points.Length]);
			return perimeter;
		}

		private Vector3d GetNormalVectorFromFirstPoints(double tolerance)
		{
			Polygon3d p = (Polygon3d)Clone();
			p.RemoveDuplicatedPoints(tolerance);
			p.RemoveAlignedPoints(tolerance);

			if (p.Count < 3)
				throw new NotSupportedException("Number of unique points not sufficient to create a normal vector");

			var v1 = p[0].VectorTo(p[1]);
			var v2 = p[0].VectorTo(p[2]);

			var normal = v1.CrossProduct(v2);
			normal.Unitize();

			return normal;
		}

		/// <summary>
		/// Get the vector normal of the polygon
		/// </summary>
		/// <returns>A unitized vector normal to the polygon</returns>
		/// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a normal vector</exception>
		internal Vector3d GetNormalVectorWithoutChekcs()
		{
			if (Count < 3)
				throw new NotSupportedException("Number of unique points not sufficient to create a normal vector");

			Vector3d normal = this[0].VectorTo(this[1]).CrossProduct(this[0].VectorTo(this[2]));
			normal.Unitize();

			return normal;
		}

		/// <summary>
		/// Polygon is right oriented if normal is directed towards the Z global positive axis.
		/// </summary>
		/// <returns>True if the polygon is right hand oriented</returns>
		public bool IsRightHandOrdered(double tolerance = GeometryBase.AngularTolerance)
		{
			return GetSignedArea(tolerance) > 0;
		}

		/// <summary>
		/// Flip the normal of the polygon, reversing the order of the vertices IN PLACE
		/// </summary>
		/// <returns>This same polygon (not a copy), to allow chaining. To keep the original use <c>new Polygon3d(polygon).Reverse()</c></returns>
		public Polygon3d Reverse()
		{
			Array.Reverse(_points);
			return this;
		}

		/// <summary>
		/// Remove aligned points keeping only the last 2 point of the segment.
		/// If all the points are aligned, it keeps the last two points, not necessary the extremes of the line
		/// </summary>
		/// <param name="tolerance">Tolleranza</param>
		public void RemoveAlignedPoints(double tolerance = GeometryBase.AngularTolerance)
		{
			List<int> pointToRemove = new List<int>();

			for (int i = 0; i < _points.Length; i++)
			{
				if (_points.Length > 2)
				{
					Vector3d v1 = new Vector3d(GetPreviousPoint(i) - _points[i]);

					Vector3d v2 = new Vector3d(GetNextPoint(i) - _points[i]);

					double angle = v1.AngleTo(v2);
					double tol = Utilities.Maths.ErrorPropagation.ProductTolerance(angle, angle, tolerance, tolerance);

					// Il metodo costruisce l'angolo tra il punto da testare, il punto precedente e il punto successivo. 
					// Se questo angolo è 180° => pi greco, allora i punti sono allineati.

					if (Math.Abs(Math.Abs(angle) - Math.PI) < tol)
					{
						if (pointToRemove.Count < _points.Length - 2)
							pointToRemove.Add(i);
					}

					// Se l'angolo è 0, cerca il punto successivo. Se trovo un nuovo punto allineato in cui i è nel mezzo, allora tolgo i, 
					// se non trovo altri punti allora vuol dire che è un estremo o che non ci sono punti allineati

					if (Math.Abs(Math.Abs(angle)) < tol)
					{
						for (int k = 1; k < _points.Length - 1 - i; k++)
						{
							Vector3d v3 = new Vector3d(GetPreviousPoint(i) - _points[i]);

							Vector3d v4 = new Vector3d(GetNextPoint(k + i) - _points[i]);

							double angle2 = v3.AngleTo(v4);

							if (Math.Abs(Math.Abs(angle2) - Math.PI) < tol)
							{
								if (pointToRemove.Count < _points.Length - 2)
								{
									pointToRemove.Add(i);
									i--;
								}
								break;
							}

							if (Math.Abs(Math.Abs(angle)) < tol)
							{

							}

							else
							{
								break;
							}
						}
					}
				}
			}

			// Rimozione punti leggendo la lista al contrario
			for (int i = pointToRemove.Count - 1; i >= 0; i--)
			{
				RemoveAt(pointToRemove[i]);
			}
		}

		/// <summary>
		///  Check if the polygon is Planar with the default tollerance
		/// </summary>
		/// <returns>True if the polygon is planar</returns>
		private bool IsPlanar(double tolerance = GeometryBase.Tolerance)
		{
			/*
			 * http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
			 * possibile alternativa di calcolo. funziona con il calcolo del determinante.
			 */

			// https://math.stackexchange.com/a/221856

			if (Count <= 3)
				return true;

			Polygon3d polygon = new Polygon3d();  // Non si può usare clona, altrimenti viene chiamato IsPlanar();
			polygon.AddRange(_points);

			polygon.RemoveDuplicatedPoints(tolerance);
			polygon.RemoveAlignedPoints(tolerance);

			if (polygon.Count <= 3)
				return true;

			Plane plane = new Plane(polygon[0], polygon[1], polygon[2], tolerance);
			// Se va in argumentException i punti che creano il piano sono allineati, 
			// vuol dire che RemoveAlignedPoints() non ha funzionato
			// Dato che da test RemoveAlignedPoints() funziona sempre, 
			// non catturo questa eccezione dato che si tratta di casi molto particolari che non dovrebbero esistere

			for (int i = 0; i < _points.Length; i++)
			{
				if (!_points[i].Equals(polygon[0], tolerance) && !_points[i].Equals(polygon[1], tolerance) && !_points[i].Equals(polygon[2], tolerance))
				{
					if (!plane.IsPointOnPlane(_points[i], tolerance))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tell if a point is on the polygon plane
		/// </summary>
		/// <param name="pointToTest">Point to test</param>
		/// <param name="tol"></param>
		/// <returns>True if the point is on polygon plane. 
		///          True if the polygon contains less than three unique and not aligned points.
		/// </returns>
		private bool IsPointOnPlane(Point3d pointToTest, double tol = GeometryBase.Tolerance)
		{
			/*
			* http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
			* possibile alternativa di calcolo. funziona con il calcolo del determinante.
			*/

			// https://math.stackexchange.com/a/221856

			Polygon3d polygon = new Polygon3d();  // Non si può usare clona, altrimenti viene chiamato IsPlanar();
			polygon.AddRange(_points);

			polygon.RemoveDuplicatedPoints(tol);
			polygon.RemoveAlignedPoints(tol);

			if (polygon.Count < 3)
				return true;

			Plane plane = new Plane(polygon[0], polygon[1], polygon[2], tol);
			// Se va in argumentException i punti che creano il piano sono allineati, 
			// vuol dire che RemoveAlignedPoints() non ha funzionato
			// Dato che da test RemoveAlignedPoints() funziona sempre, 
			// non catturo questa eccezione dato che si tratta di casi molto particolari che non dovrebbero esistere

			return plane.IsPointOnPlane(pointToTest, tol);
		}

		/// <summary>
		/// Tell if a point is inside a polygon
		/// </summary>
		/// <param name="pointToTest">Point to test</param>
		/// <param name="tol"></param>
		/// <returns>True if the point is inside the polygon</returns>
		/// <remarks>The plane and the projection are given by the Newell area vector, without copies of the polygon.
		/// Points on the border (within <paramref name="tol"/> in the projection) are inside</remarks>
		public bool IsPointInside(Point3d pointToTest, double tol = GeometryBase.Tolerance)
		{
			if (_points.Length < 3)
				return IsPointInsideByProjections(pointToTest, tol);

			Vector3d area = GetAreaVector();
			double length = area.Length;

			if (!(length > tol * GetPerimeter()))
				return IsPointInsideByProjections(pointToTest, tol); // degenerate polygon

			Point3d origin = _points[0];
			double distanceFromPlane = (area.X * (pointToTest.X - origin.X) + area.Y * (pointToTest.Y - origin.Y) + area.Z * (pointToTest.Z - origin.Z)) / length;
			if (!(Math.Abs(distanceFromPlane) < tol))
			{
				// Point off the plane: the previous method decides, so the result does not change.
				// With a large tolerance it does not check the plane, because RemoveAlignedPoints uses the tolerance as an angle
				// and removes vertices of the polygon (e.g. Checker MixedSectionTest.FailureDomain02 relies on it)
				return IsPointInsideByProjections(pointToTest, tol);
			}

			// Projection on the coordinate plane where the polygon has the largest area: the components of the area vector are the projected areas
			double areaYZ = Math.Abs(area.X);
			double areaXZ = Math.Abs(area.Y);
			double areaXY = Math.Abs(area.Z);

			if (areaXY >= areaYZ && areaXY >= areaXZ)
				return IsPointInsideProjection(pointToTest, 0, 1, tol);
			else if (areaYZ >= areaXY && areaYZ >= areaXZ)
				return IsPointInsideProjection(pointToTest, 1, 2, tol);
			else
				return IsPointInsideProjection(pointToTest, 0, 2, tol);
		}

		private static double Coordinate(Point3d point, int axis)
		{
			return axis == 0 ? point.X : (axis == 1 ? point.Y : point.Z);
		}

		/// <summary>
		/// Same test of <see cref="Polygon2d.IsPointInside(Point2d, double)"/> on the projection of the polygon on the coordinates <paramref name="u"/>, <paramref name="v"/>
		/// </summary>
		private bool IsPointInsideProjection(Point3d pointToTest, int u, int v, double tol)
		{
			int count = _points.Length;
			double px = Coordinate(pointToTest, u);
			double py = Coordinate(pointToTest, v);

			// Points on the border (vertices and edges) are inside
			double squareTolerance = tol * tol;
			for (int i = 0; i < count; i++)
			{
				Point3d start = _points[i];
				Point3d end = _points[(i + 1) % count];
				double sx = Coordinate(start, u), sy = Coordinate(start, v);
				double ex = Coordinate(end, u), ey = Coordinate(end, v);
				double c = ex - sx;
				double d = ey - sy;

				double squareLength = c * c + d * d;
				double param = squareLength != 0 ? ((px - sx) * c + (py - sy) * d) / squareLength : -1;

				double qx, qy;
				if (param < 0) { qx = sx; qy = sy; }
				else if (param > 1) { qx = ex; qy = ey; }
				else { qx = sx + param * c; qy = sy + param * d; }

				double dx = px - qx, dy = py - qy;
				if (dx * dx + dy * dy < squareTolerance)
					return true;
			}

			// Crossing number with the "half-open" rule on the edges
			bool inside = false;
			for (int i = 0, j = count - 1; i < count; j = i++)
			{
				double ax = Coordinate(_points[i], u), ay = Coordinate(_points[i], v);
				double bx = Coordinate(_points[j], u), by = Coordinate(_points[j], v);

				if ((ay > py) != (by > py))
				{
					double xCross = ax + (py - ay) * (bx - ax) / (by - ay);
					if (px < xCross)
						inside = !inside;
				}
			}

			return inside;
		}

		private bool IsPointInsideByProjections(Point3d pointToTest, double tol = GeometryBase.Tolerance)
		{
			if (IsPointOnPlane(pointToTest, tol))                              // se il punto non è sul piano torna falso
			{
				Polygon2d ProjectionXY = new Polygon2d();
				Polygon2d ProjectionYZ = new Polygon2d();
				Polygon2d ProjectionXZ = new Polygon2d();

				for (int i = 0; i < _points.Length; i++)                             // Creo le 3 proiezioni sui piani XY, XZ ,YZ
				{
					Point2d point2dXY = new Point2d(_points[i].X, _points[i].Y);
					Point2d point2dYZ = new Point2d(_points[i].Y, _points[i].Z);
					Point2d point2dXZ = new Point2d(_points[i].X, _points[i].Z);

					ProjectionXY.Add(point2dXY);
					ProjectionYZ.Add(point2dYZ);
					ProjectionXZ.Add(point2dXZ);
				}

				double AreaXY = Math.Abs(ProjectionXY.GetSignedArea());             // Calcolo le aree delle 3 proiezioni
				double AreaYZ = Math.Abs(ProjectionYZ.GetSignedArea());             // Voglio usare l'area maggiore
				double AreaXZ = Math.Abs(ProjectionXZ.GetSignedArea());

				Polygon2d projectedPolygon;
				Point2d projectedPointTotest;

				if (AreaXY >= AreaYZ && AreaXY >= AreaXZ)
				{
					projectedPolygon = ProjectionXY;
					projectedPointTotest = new Point2d(pointToTest.X, pointToTest.Y);
				}
				else if (AreaYZ >= AreaXY && AreaYZ >= AreaXZ)
				{
					projectedPolygon = ProjectionYZ;
					projectedPointTotest = new Point2d(pointToTest.Y, pointToTest.Z);
				}
				else // if (AreaXZ >= AreaXY && AreaXZ >= AreaYZ)
				{
					projectedPolygon = ProjectionXZ;
					projectedPointTotest = new Point2d(pointToTest.X, pointToTest.Z);
				}

				if (projectedPolygon.IsPointInside(projectedPointTotest, tol))           // controlla che il punto proiettato sia dentro il 
					return true;                                                    // poligono proiettato
				else
					return false;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Tell if the line to test is inside the polygon
		/// </summary>
		/// <param name="lineToTest">The line to test</param>
		/// <returns>True if the line is inside</returns>
		public bool IsLineInside(Line3d lineToTest)
		{
			int intersectionCount = 0;

			if (IsPointInside(lineToTest.Start))                                  // controllo che start sia interno (altrimenti false)
			{
				if (IsPointInside(lineToTest.End))                                // controllo che end sia interno (altrimenti false)
				{
					Line3d[] lines = Explode();
					for (int i = 0; i < lines.Length; i++)
					{
						if (lines[i].Equals(lineToTest))
							return true;

						if (lines[i].IsPointOnLine(lineToTest.Start))                 // se start è sul bordo
						{                                                   // intersectionCount--
							intersectionCount--;
						}

						if (lines[i].IsPointOnLine(lineToTest.End))                   // se end è sul bordo
						{                                                   // intersectionCount--
							intersectionCount--;
						}

						if (lines[i].GetIntersection(lineToTest, out _))      // per ogni intersezione
						{
							intersectionCount++;                                            // intersectionCount++
						}
					}                                                         //
				}                                                             // intersectionCount sono le intersezioni che ha la retta che non siano
				else                                                          // quelle degli estremi con i vertici o con i lati
					return false;                                             // se intersectionTotal è pari, vuol dire che è interno oppure interseca
			}                                                                 // i lati o vertici del poligono gli gli estremi
			else                                                              // se è dispari, vuol dire che la retta esce dal bordo
				return false;                                                 // 

			if (Math.Abs(intersectionCount) == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Scale the polygon by <paramref name="factor"/> respect to the origin 
		/// </summary>
		/// <param name="factor">Scale factor</param>
		/// <returns>A new polygon scaled</returns>
		public Polygon3d Scale(double factor)
		{
			return Scale(factor, factor, factor);
		}

		/// <summary>
		/// Scale the polygon by a factor respect to the origin 
		/// </summary>
		/// <returns>A new polygon scaled</returns>
		public Polygon3d Scale(double factorX, double factorY, double factorZ)
		{
			Point3d[] points = new Point3d[_points.Length];
			for (int i = 0; i < _points.Length; i++)
				points[i] = _points[i].Scale(factorX, factorY, factorZ);

			return new Polygon3d(points);
		}

		/// <summary>
		/// Scale the polygon by a factor respect to the origin 
		/// </summary>
		/// <returns>A new polygon scaled</returns>
		public Polygon3d Scale(Point3d center, double factorX, double factorY, double factorZ)
		{
			Point3d[] points = new Point3d[_points.Length];
			for (int i = 0; i < _points.Length; i++)
				points[i] = _points[i].Scale(center, factorX, factorY, factorZ);

			return new Polygon3d(points);
		}

		/// <summary>
		/// Scale the polygon by a factor respect to the origin 
		/// </summary>
		/// <returns>A new polygon scaled</returns>
		public Polygon3d Scale(Point3d center, double factor)
		{
			return Scale(center, factor, factor, factor);
		}

		/// <summary>
		/// Check if the polygon is convex
		/// </summary>
		/// <returns>Return true if the polygon is convex</returns>
		public bool IsConvex()
		{
			// For each set of three adjacent points A, B, C, find the cross product AB x BC projected on the polygon normal. If the sign of
			// all the cross products is the same, the angles are all positive or negative (depending on the
			// order in which we visit them) so the polygon is convex.
			// The projection on the normal makes the check valid for polygons in any plane (not only the XY plane)

			bool got_negative = false;
			bool got_positive = false;
			int num_points = _points.Count();

			if (num_points < 3)
				return true;

			Vector3d normal = GetAreaVector();
			double zero = 1e-12 * normal.DotProduct(normal); // round-off threshold, same dimension of cross_product

			int B, C;
			for (int A = 0; A < num_points; A++)
			{
				B = (A + 1) % num_points;
				C = (B + 1) % num_points;

				double cross_product = new Vector3d(_points[A], _points[B]).CrossProduct(new Vector3d(_points[B], _points[C])).DotProduct(normal);

				// aligned points (null cross product apart from round-off) do not change the convexity
				if (cross_product < -zero)
				{
					got_negative = true;
				}
				else if (cross_product > zero)
				{
					got_positive = true;
				}
				if (got_negative && got_positive) return false;
			}

			// If we got this far, the polygon is convex.
			return true;
		}

		/// <summary>
		/// Get the coordinate of the shape. Generated from first 3 points (not aligned, not duplicated) of the polygon. 
		/// Origin in first point, X axis on the first side, Z axis on the normal, Y to complete the triad.
		/// </summary>
		/// <returns>Coordinate system generated from first three points of the fill. Duplicate and aligned points are not considered</returns>
		/// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a coordinate system</exception>
		public CoordinateSystem GetCoordinateSystem(double tolerance = GeometryBase.Tolerance)
		{
			Polygon3d p = (Polygon3d)Clone();
			p.RemoveDuplicatedPoints(tolerance);
			p.RemoveAlignedPoints(tolerance);

			if (p.Count < 3)
				throw new NotSupportedException("Number of unique points not sufficient to create a coordinate system");

			return new CoordinateSystem(p[0], p[1], p[2]);
		}

		/// <summary>
		/// Return the Bounding Box of the polygon
		/// </summary>
		///<returns>The bounding box of the shape in the global coordinates</returns>
		public BoundingBox3d GetBoundingBox()
		{
			BoundingBox3d bbox = new BoundingBox3d();
			bbox.Update(_points);
			return bbox;
		}

		/// <summary>
		/// Create the triangles between each pair of consecutive vertices and the point center in input
		/// </summary>
		/// <param name="center">The point common at all triangles</param>
		/// <param name="tolerance">Tolerance of IsPointInside check</param>
		/// <returns>A list of polygon3d</returns>
		public Polygon3d[] Triangularization(Point3d center, double tolerance = GeometryBase.Tolerance)
		{
			if (!IsPointInside(center, tolerance))
				throw new Exception("The point must be internal");
			if (IsPointOnEdge(center, tolerance) != -1)
				throw new Exception("The point can't be on edge");

			List<Polygon3d> polygons = new List<Polygon3d>();

			for (int i = 0; i < _points.Count(); i++)
				polygons.Add(new Polygon3d(new Point3d[3] { _points[i], _points[GetNextIndex(i)], center }));

			return polygons.ToArray();
		}

		/// <summary>
		/// Create the triangles between each pair of consecutive vertices and the point center in input
		/// </summary>
		/// <param name="center">The point common at all triangles</param>
		/// <returns>A list of polygon3d</returns>
		public Polygon3d[] TriangularizationWithoutChecks(Point3d center)
		{
			Polygon3d[] polygons = new Polygon3d[_points.Count()];

			for (int i = 0; i < _points.Count(); i++)
				polygons[i] = new Polygon3d(new Point3d[3] { _points[i], _points[GetNextIndex(i)], center });

			return polygons.ToArray();
		}

		/// <summary>
		/// Get the the arithmetic mean position of all the points. This is not the centroid of the polygon
		/// </summary>
		/// <returns>The center point of polygon</returns>
		public Point3d GetCenter()
		{
			// è la media delle coordinate X, Y, Z. non è il baricentro (centroid). è un metodo più rapido per trovare un punto interno molto prossimo al baricentro
			// NON E' IL BARICENTRO DELLE AREE

			int nLati = this.Count;
			double Xsum = 0;
			double Ysum = 0;
			double Zsum = 0;

			for (int i = 0; i < _points.Length; i++)
			{
				Xsum += _points[i].X;
				Ysum += _points[i].Y;
				Zsum += _points[i].Z;
			}

			return new Point3d(Xsum / nLati, Ysum / nLati, Zsum / nLati);
		}

		/// <summary>
		/// Get the barycenter. The barycenter is the intersection point of the three lines going through one of the vertices and the middle of the opposite edge of the triangle
		/// </summary>
		/// <returns>The barycenter point of triangle</returns>
		internal Point3d GetBarycenterOfTriangle()
		{
			if (_points.Length != 3)
				throw new Exception("Polygon must be a triangle");

			return new Point3d((_points[0].X + _points[1].X + _points[2].X) / 3.0,
				(_points[0].Y + _points[1].Y + _points[2].Y) / 3.0,
				(_points[0].Z + _points[1].Z + _points[2].Z) / 3.0);
		}

		/// <summary>
		/// Get the centroid of the area of polygon
		/// </summary>
		/// <returns>The centroid point of polygon</returns>
		public Point3d GetCentroid()
		{
			// https://bell0bytes.eu/centroid-convex/
			// Unfortunately computing the centroid of a polygon isn't just as easy as computing the barycenter of a triangle; in the case of a polygon simply averaging
			// over the coordinates of the vertices no longer results in the correct coordinates of the centroid - the only exception being regular polygons.
			// While the centroid of a polygon is indeed its center of mass, the mass of a polygon is uniformly distributed over its entire surface, not only at the vertices.
			// Note that for simple shapes, such as triangles, rectangles or the above mentioned regular polygons, the mass being evenly distributed over the surface
			// is equivalent to the mass being at the vertices only.
			// In the case of a convex polygon, it is easy enough to see, however, how triangulating the polygon will lead to a formula for its centroid.

			// Fan triangulation from the first vertex. The area of each triangle is signed respect to the polygon normal,
			// so the result is exact also for concave polygons and for polygons in any plane (e.g. vertical)

			if (_points.Length < 3)
				return GetCenter();

			Vector3d normal = GetAreaVector();
			double normalLength = normal.Length;
			if (normalLength == 0)
				return GetCenter(); // degenerate polygon (aligned points)

			normal /= normalLength;

			Point3d p0 = _points[0];
			double areaTot = 0;
			double numeratorX = 0;
			double numeratorY = 0;
			double numeratorZ = 0;

			for (int i = 1; i < _points.Length - 1; i++)
			{
				Point3d p1 = _points[i];
				Point3d p2 = _points[i + 1];

				double area = 0.5 * new Vector3d(p0, p1).CrossProduct(new Vector3d(p0, p2)).DotProduct(normal);

				areaTot += area;
				numeratorX += area * (p0.X + p1.X + p2.X) / 3.0;
				numeratorY += area * (p0.Y + p1.Y + p2.Y) / 3.0;
				numeratorZ += area * (p0.Z + p1.Z + p2.Z) / 3.0;
			}

			return new Point3d(numeratorX / areaTot, numeratorY / areaTot, numeratorZ / areaTot);

			//double areaTot = 0;
			//double numeratorX = 0;
			//double numeratorY = 0;
			//double numeratorZ = 0;

			//for (int i = 0; i < listOfTriangles.Length; i++)
			//{
			//    Point3d triangleBarycenter = listOfTriangles[i].GetBarycenterOfTriangle();

			//    double area = listOfTriangles[i].GetSignedArea();

			//    numeratorX += (triangleBarycenter.X * area);
			//    numeratorY += (triangleBarycenter.Y * area);
			//    numeratorZ += (triangleBarycenter.Z * area);

			//    areaTot += area;
			//}

			//return new Point3d(numeratorX / areaTot, numeratorY / areaTot, numeratorZ / areaTot);
		}

		/// <summary>
		/// Return the parametrization of the point in the polygon
		/// </summary>
		/// <param name="pointToParametrize">The point to find the parametrization</param>
		/// <param name="triangle">The triangle of the triangularization where the point is inside</param>
		/// <param name="N1">The shape function N1</param>
		/// <param name="N2">The shape function N2</param>
		/// <param name="N3">The shape function N3</param>
		/// <param name="tolerance">The tolerance</param>
		internal void GetPointParametrization(Point3d pointToParametrize, out Polygon3d triangle, out double N1, out double N2, out double N3, double tolerance = GeometryBase.Tolerance)
		{
			N1 = 0;
			N2 = 0;
			N3 = 0;
			triangle = null;

			if (!IsPointInside(pointToParametrize, tolerance))
				throw new Exception("The point must be internal");

			Point3d centre = GetCenter();
			Polygon3d[] listOfTriangle = TriangularizationWithoutChecks(centre);

			for (int i = 0; i < listOfTriangle.Count(); i++)
			{
				if (listOfTriangle[i].IsPointInside(pointToParametrize, tolerance))
				{
					triangle = listOfTriangle[i];
					Polygon2d ProjectionXY = new Polygon2d();
					Polygon2d ProjectionYZ = new Polygon2d();
					Polygon2d ProjectionXZ = new Polygon2d();

					for (int j = 0; j < listOfTriangle[i]._points.Length; j++)                             // Creo le 3 proiezioni sui piani XY, XZ ,YZ
					{
						Point3d point = listOfTriangle[i]._points[j];
						Point2d point2dXY = new Point2d(point.X, point.Y);
						Point2d point2dYZ = new Point2d(point.Y, point.Z);
						Point2d point2dXZ = new Point2d(point.X, point.Z);

						ProjectionXY.Add(point2dXY);
						ProjectionYZ.Add(point2dYZ);
						ProjectionXZ.Add(point2dXZ);
					}

					double AreaXY = Math.Abs(ProjectionXY.GetSignedArea());             // Calcolo le aree delle 3 proiezioni
					double AreaYZ = Math.Abs(ProjectionYZ.GetSignedArea());             // Voglio usare l'area maggiore
					double AreaXZ = Math.Abs(ProjectionXZ.GetSignedArea());

					Polygon2d projectedPolygon;
					Point2d projectedPointTotest;

					if (AreaXY >= AreaYZ && AreaXY >= AreaXZ)
					{
						projectedPolygon = ProjectionXY;
						projectedPointTotest = new Point2d(pointToParametrize.X, pointToParametrize.Y);

					}
					else if (AreaYZ >= AreaXY && AreaYZ >= AreaXZ)
					{
						projectedPolygon = ProjectionYZ;
						projectedPointTotest = new Point2d(pointToParametrize.Y, pointToParametrize.Z);
					}
					else // if (AreaXZ >= AreaXY && AreaXZ >= AreaYZ)
					{
						projectedPolygon = ProjectionXZ;
						projectedPointTotest = new Point2d(pointToParametrize.X, pointToParametrize.Z);

					}

					N1 = Utilities.Fem.LinearShapeFunctionsTri3.LocalShapeFunction(1, projectedPointTotest.X, projectedPointTotest.Y, projectedPolygon[0].X, projectedPolygon[0].Y,
							projectedPolygon[1].X, projectedPolygon[1].Y, projectedPolygon[2].X, projectedPolygon[2].Y);
					N2 = Utilities.Fem.LinearShapeFunctionsTri3.LocalShapeFunction(2, projectedPointTotest.X, projectedPointTotest.Y, projectedPolygon[0].X, projectedPolygon[0].Y,
							projectedPolygon[1].X, projectedPolygon[1].Y, projectedPolygon[2].X, projectedPolygon[2].Y);
					N3 = Utilities.Fem.LinearShapeFunctionsTri3.LocalShapeFunction(3, projectedPointTotest.X, projectedPointTotest.Y, projectedPolygon[0].X, projectedPolygon[0].Y,
							projectedPolygon[1].X, projectedPolygon[1].Y, projectedPolygon[2].X, projectedPolygon[2].Y);
					break;
				}
			}
		}

		/// <summary>
		/// Return the point3d (with shape functions N1, N2, N3) in global coordinates 
		/// </summary>
		/// <param name="N1">The shape function N1</param>
		/// <param name="N2">The shape function N2</param>
		/// <param name="N3">The shape function N3</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The point3d in global coordinates</returns>
		private Point3d GetPointFromParametrization(double N1, double N2, double N3, double tolerance = GeometryBase.Tolerance)
		{
			if (Count > 3)
				throw new Exception("Polygon must have 3 verticles");

			if ((Math.Abs(N1 - 1) < tolerance) && N2 < tolerance && N3 < tolerance)
				return new Point3d(this[0]);
			if ((Math.Abs(N2 - 1) < tolerance) && N1 < tolerance && N3 < tolerance)
				return new Point3d(this[1]);
			if ((Math.Abs(N3 - 1) < tolerance) && N1 < tolerance && N2 < tolerance)
				return new Point3d(this[2]);

			Line3d[] listOfEdge = this.Explode();

			double[] N1Array = new double[1] { N1 };
			double[] revN1Array = new double[1] { 1 - N1 };
			double[] N2Array = new double[1] { N2 };
			double[] revN2Array = new double[1] { 1 - N2 };
			double[] N3Array = new double[1] { N3 };
			double[] revN3Array = new double[1] { 1 - N3 };

			Line3d l1 = null;
			Line3d l2 = null;
			Line3d l3 = null;
			Point3d int1 = null;
			Point3d int2 = null;
			Point3d int3 = null;

			if (N1 != 0)
			{
				Line3d[] splitL0_N1 = listOfEdge[0].Split(revN1Array, tolerance);
				Line3d[] splitL2_N1 = listOfEdge[2].Split(N1Array, tolerance);
				l1 = new Line3d(splitL0_N1[0].End, splitL2_N1[0].End);
			}

			if (N2 != 0)
			{
				Line3d[] splitL0_N2 = listOfEdge[0].Split(N2Array, tolerance);
				Line3d[] splitL1_N2 = listOfEdge[1].Split(revN2Array, tolerance);
				l2 = new Line3d(splitL0_N2[0].End, splitL1_N2[0].End);
			}

			if (N3 != 0)
			{
				Line3d[] splitL1_N3 = listOfEdge[1].Split(N3Array, tolerance);
				Line3d[] splitL2_N3 = listOfEdge[2].Split(revN3Array, tolerance);
				l3 = new Line3d(splitL1_N3[0].End, splitL2_N3[0].End);
			}

			if (N1 != 0 && N2 != 0)
				l1.GetIntersection(l2, out int1, tolerance);

			if (N2 != 0 && N3 != 0)
				l2.GetIntersection(l3, out int2, tolerance);

			if (N3 != 0 && N1 != 0)
				l3.GetIntersection(l1, out int3, tolerance);

			double tol = Math.Sqrt(Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance));

			if (int1 != null && int2 != null)
				if ((Math.Abs(int1.X - int2.X) < tol) && (Math.Abs(int1.Y - int2.Y) < tol) && (Math.Abs(int1.Z - int2.Z) < tol))
					return int1;
			if (int1 != null && int3 != null)
				if ((Math.Abs(int1.X - int3.X) < tol) && (Math.Abs(int1.Y - int3.Y) < tol) && (Math.Abs(int1.Z - int3.Z) < tol))
					return int1;
			if (int3 != null && int2 != null)
				if ((Math.Abs(int2.X - int3.X) < tol) && (Math.Abs(int2.Y - int3.Y) < tol) && (Math.Abs(int2.Z - int3.Z) < tol))
					return int3;
			if (int1 != null)
				return int1;
			if (int2 != null)
				return int2;
			if (int3 != null)
				return int3;
			else
				throw new Exception("Error to recover the point");
		}

		/// <summary>
		/// Transform the insert point from the base polygon to the new polygon
		/// </summary>
		/// <param name="pointToParam"></param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The point3d in global coordinates in the new polygon</returns>
		public Point3d Transform(Point3d pointToParam, Polygon3d newPolygon, double tolerance = GeometryBase.Tolerance)
		{
			if (newPolygon.IsPlanar(tolerance))
			{
				if (newPolygon.IsConvex())
				{
					if (newPolygon._points.Length == this._points.Length)
					{
						GetPointParametrization(pointToParam, out Polygon3d triangle, out double N1, out double N2, out double N3, tolerance);

						int index = -1;

						double tol = Math.Sqrt(Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance));

						for (int i = 0; i < Count; i++)
						{
							if (Math.Abs(triangle[0].X - this[i].X) < tol && Math.Abs(triangle[0].Y - this[i].Y) < tol && Math.Abs(triangle[0].Z - this[i].Z) < tol)
							{
								// il triangolo in cui è pointToParam è quello i, i+1, i+2 
								index = i;
								break;
							}
						}

						if (index == -1)
							throw new InvalidOperationException("Fail to find the triangle of the parametrization in the base polygon");

						// ricerco la terna i, i+1, i+2 nel nuovo polygono
						Point3d newCenter = newPolygon.GetCenter();
						Point3d[] pointsTriangle = new Point3d[] { newPolygon._points[index], newPolygon._points[GetNextIndex(index)], newCenter };
						Polygon3d newTriangle = new Polygon3d(pointsTriangle);

						return newTriangle.GetPointFromParametrization(N1, N2, N3, tolerance);
					}
					else
						throw new Exception($"The polygon must have {this._points.Count()} edge");
				}
				else
					throw new Exception("The polygon must be convex");
			}
			else
				throw new Exception("The polygon must be planar");
		}

		/// <summary>
		/// Transform the insert line from the base polygon to the new polygon
		/// </summary>
		/// <param name="lineToParam">Line to transform</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The Line3d in global coordinates in the new polygon</returns>
		public Line3d Transform(Line3d lineToParam, Polygon3d newPolygon, double tolerance = GeometryBase.Tolerance)
		{
			Point3d newStart = Transform(lineToParam.Start, newPolygon, tolerance);
			Point3d newEnd = Transform(lineToParam.End, newPolygon, tolerance);

			return new Line3d(newStart, newEnd);
		}

		/// <summary>
		/// Transform the insert polygon from the base polygon to the new polygon
		/// </summary>
		/// <param name="polygonToParam">Polygon to transform</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The Line3d in global coordinates in the new polygon</returns>
		public Polygon3d Transform(Polygon3d polygonToParam, Polygon3d newPolygon, double tolerance = GeometryBase.Tolerance)
		{
			Point3d[] points = new Point3d[polygonToParam.Count];

			for (int i = 0; i < polygonToParam.Count; i++)
				points[i] = Transform(polygonToParam[i], newPolygon, tolerance);

			return new Polygon3d(points, tolerance);
		}

		/// <summary>
		/// Transform the insert shape from the base polygon to the new polygon
		/// </summary>
		/// <param name="shapeToParam">Shape to transform</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The Line3d in global coordinates in the new polygon</returns>
		public Shape Transform(Shape shapeToParam, Polygon3d newPolygon, double tolerance = GeometryBase.Tolerance)
		{
			Polygon3d newFill = Transform(shapeToParam.Fill, newPolygon, tolerance);

			Polygon3d[] holes = null;
			if (shapeToParam.HasHoles)
			{
				holes = new Polygon3d[shapeToParam.Holes.Length];
				for (int i = 0; i < shapeToParam.Holes.Length; i++)
					holes[i] = Transform(shapeToParam.Holes[i], newPolygon, tolerance);
			}

			if (shapeToParam.HasChilds && shapeToParam.Childs.Length > 0)
			{
				throw new NotImplementedException("Childs not implemented");
			}

			return new Shape(newFill, holes, null);
		}

		/// <summary>
		/// Return the projected polygon on the local coordinate system
		/// </summary>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public Polygon2d GetPolygon2d(double tolerance = GeometryBase.Tolerance)
		{
			return GetCoordinateSystem(tolerance).ToLocal(this);
		}

		#endregion

		#region Interface implementation

		/// <summary>
		/// Clone the polygon
		/// </summary>
		/// <param name="tolerance"></param>
		/// <returns>The new object cloned</returns>
		public object Clone(double tolerance = GeometryBase.Tolerance)
		{
			return new Polygon3d(this, tolerance);
		}

		/// <summary>
		/// Clone the polygon
		/// </summary>
		/// <returns>The new object cloned</returns>
		public override object Clone()
		{
			return new Polygon3d(this);
		}

		public IEnumerator<Point3d> GetEnumerator()
		{
			return ((IEnumerable<Point3d>)_points).GetEnumerator(); // the array is replaced (not changed) when points are added or removed
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Public method override

		public override bool Equals(object obj)
		{
			return Equals(obj as Polygon3d);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _points.GetHashCodeSequence();

				return hashCode;
			}
		}

		public bool EqualsShifted(object obj, double tolerance = GeometryBase.Tolerance)
		{
			if (obj is Polygon3d poly && obj != null)
			{
				if (poly.Count == Count)
				{
					double tol = Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance);
					for (int i = 0; i < poly.Count; i++)
					{
						if (poly[0].Equals(this[i]))
						{
							int index = i;
							for (int j = 0; j < poly.Count; j++)
							{
								if (poly[j].SquareDistanceTo(this[index]) < tol)
								{
									index = GetNextIndex(index);
								}
								else
									return false;

							}
							return true;
						}
					}
				}
				else
					return false;
			}
			return false;
		}

		#endregion

		#region CLIPPER

		/// <summary>
		/// Execute a boolean operation beetween polygons using the Clipper class
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="code"></param>
		/// <param name="factor"></param>
		/// <returns></returns>
		/// <param name="factor">Scale of the coordinates; not positive (default): automatic, see <see cref="ClipperScale"/></param>
		private static Polygon3d[] Boolean(Polygon3d[] a, Polygon3d[] b, ClipType code, int factor = 0)
		{
			double scale = factor > 0 ? factor : ClipperScale.Factor(Math.Max(ClipperScale.MaxAbsCoordinate(a), ClipperScale.MaxAbsCoordinate(b)));

			Clipper clipper = new Clipper();

			for (int i = 0; i < a.Length; i++)
			{
				Polygon3d polygon = a[i];
				AddToPath(polygon, scale, clipper, PolyType.ptSubject);
			}

			for (int i = 0; i < b.Length; i++)
			{
				Polygon3d polygon = b[i];
				AddToPath(polygon, scale, clipper, PolyType.ptClip);
			}

			PolyTree polyTree = new PolyTree();
			if (clipper.Execute(code, polyTree, PolyFillType.pftEvenOdd))
			{
				if (polyTree.Contour.Count > 0)
				{
					throw new InvalidOperationException();
				}
				if (polyTree.ChildCount > 0)
				{
					List<Polygon3d> bufferResult = new List<Polygon3d>();
					for (int i = 0; i < polyTree.Childs.Count; i++)
					{
						PolyNode node = polyTree.Childs[i];
						List<Polygon3d> result = new List<Polygon3d>();
						GetPolygonsFromPolyNode(node, scale, ref result);
						bufferResult.AddRange(result);
					}
					return bufferResult.ToArray();
				}
			}

			return null;
		}

		/// <summary>
		/// Support function that add the polygon to the Clipper object for the boolean operations
		/// </summary>
		/// <param name="polygon">The polygon</param>
		/// <param name="factor">The scale factor for double to integer conversion for the coorinates</param>
		/// <param name="clipper">The clipper object for the boolean operations</param>
		/// <param name="type">Tell if the shape polygon is a Subject or Clip polygon</param>
		private static void AddToPath(Polygon3d polygon, double factor, Clipper clipper, PolyType type)
		{
			List<IntPoint> buffer = GetIntPointList(polygon, factor);
			clipper.AddPath(buffer, type, true);
		}

		/// <summary>
		/// Create an IntPoint array from the given Polygon2d. IntPoint is a subclass of Clipper
		/// </summary>
		/// <param name="polygon">The source polygon</param>
		/// <param name="factor">The scale factor in conversion from double to int of the coordinates</param>
		/// <returns>The list of IntPoint</returns>
		internal static List<IntPoint> GetIntPointList(Polygon3d polygon, double factor)
		{
			List<IntPoint> result = new List<IntPoint>();

			for (int v = 0; v < polygon.Count; v++)
			{
				result.Add(new IntPoint(ClipperScale.Scale(polygon[v].X, factor), ClipperScale.Scale(polygon[v].Y, factor)));
			}
			return result;
		}

		/// <summary>
		/// Converts back the resulting clipper.PolyNode in Polygon2d array
		/// </summary>
		/// <param name="polyNode">The PolyNode to convert</param>
		/// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
		/// <param name="result"></param>
		/// <returns></returns>
		private static void GetPolygonsFromPolyNode(PolyNode polyNode, double factor, ref List<Polygon3d> result)
		{
			// The contour polygon
			Polygon3d contourPoly = new Polygon3d();
			for (int i = 0; i < polyNode.Contour.Count; i++)
			{
				IntPoint p = polyNode.Contour[i];
				contourPoly.Add(p.X / factor, p.Y / factor, 0);
			}
			result.Add(contourPoly);

			// Add the child polygons to the result
			if (polyNode.ChildCount > 0)
			{
				for (int i = 0; i < polyNode.ChildCount; i++)
				{
					GetPolygonsFromPolyNode(polyNode.Childs[i], factor, ref result);
				}
			}
		}

		#endregion

		#region Operator ovverride 

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Count", _points.Length);
			for (int i = 0; i < _points.Length; i++)
			{
				info.AddValue($"Point{i}", _points[i], typeof(Point3d));
			}
		}

		public bool Equals(Polygon3d other)
		{
			if (ReferenceEquals(this, other))
				return true;

			// Sequence Equals:
			// true if the two source sequences are of equal length and their corresponding elements are equal according to the default equality comparer for their type; 
			// otherwise, false.
			return !(other is null) && other._points.SequenceEqual(_points);
		}

		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Polygon3d point)
				return Equals(point);

			return false;
		}

		public static bool operator ==(Polygon3d obj1, Polygon3d obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(Polygon3d obj1, Polygon3d obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}


