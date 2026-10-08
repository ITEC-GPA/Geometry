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
	/// A planar polygon in the space: the ordered list of its vertices, closed (the last vertex is joined to the first one).
	/// The methods that add points check the planarity within a tolerance, the ones "WithoutChecks" do not.
	/// Two polygons are equal if they have equal vertices in the same order, starting from the same vertex (see also <see cref="EqualsShifted"/>)
	/// </summary>
	[Serializable]
	public sealed class Polygon3d : GeometryBase, IEnumerable<Point3d>, ISerializable, ICloneable, IEquatable<Polygon3d>
	{
		#region Variables

		/// <summary>
		/// The vertices (the array is replaced, not changed, when points are added or removed)
		/// </summary>
		private Point3d[] _points;

		#endregion

		#region Properties

		/// <summary>
		/// The number of vertices
		/// </summary>
		public int Count => _points.Length;

		/// <summary>
		/// The vertex at an index
		/// </summary>
		/// <param name="index">The index, from 0 to <see cref="Count"/> - 1</param>
		/// <returns>The vertex (the instance of the polygon)</returns>
		public Point3d this[int index] => _points[index];

		/// <summary>
		/// The vertices (the array of the polygon, not a copy). The setter does not check the planarity
		/// </summary>
		public Point3d[] Points
		{
			get { _pointsExposed = true; return _points; }
			set { _points = value; _pointsExposed = true; _planeBasisIndices = null; }
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates an empty polygon
		/// </summary>
		public Polygon3d()
		{
			_points = new Point3d[0];
		}

		/// <summary>
		/// Create a new polygon from a point array (the instances are kept, not copied)
		/// </summary>
		/// <param name="points">The polygon vertices; null: empty polygon</param>
		/// <param name="tolerance">The tolerance of the planarity check</param>
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
		/// Create a new polygon from a point array (the instances are kept, not copied), without the planarity check
		/// </summary>
		/// <param name="points">The polygon vertices; null: empty polygon</param>
		public Polygon3d(Point3d[] points)
			: this()
		{
			if (points != null)
				AddRange(points);
		}

		/// <summary>
		/// Create a new polygon from a sequence of points (the instances are kept, not copied)
		/// </summary>
		/// <param name="points">The polygon vertices; null: empty polygon</param>
		/// <param name="tolerance">The tolerance of the planarity check</param>
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

		/// <summary>
		/// Creates a copy of the polygon, with copies of the vertices, checking the planarity with <paramref name="tolerance"/>
		/// </summary>
		/// <param name="polygon">The polygon to copy</param>
		/// <param name="tolerance">The tolerance of the planarity check</param>
		/// <exception cref="ArgumentException">If the polygon is not planar within <paramref name="tolerance"/></exception>
		public Polygon3d(Polygon3d polygon, double tolerance = GeometryBase.Tolerance)
			: this(polygon)
		{
			// Copy once and validate the complete polygon, without reallocating at every vertex.
			if (!IsPlanar(tolerance))
				throw new ArgumentException("Polygon is not planar", nameof(polygon));
		}

		/// <summary>
		/// Creates a copy of the polygon, with copies of the vertices (moving the copy does not move the original)
		/// </summary>
		/// <param name="polygon">The polygon to copy</param>
		public Polygon3d(Polygon3d polygon)
		{
			// one allocation (AddWithoutChecks copied the array at every point)
			_points = new Point3d[polygon.Count];
			for (int i = 0; i < _points.Length; i++)
				_points[i] = new Point3d(polygon[i]);
		}

		/// <summary>
		/// Creates a polygon on the plane z = 0 from a polygon on the XY plane (copies of the vertices)
		/// </summary>
		/// <param name="polygon">The polygon on the XY plane</param>
		public Polygon3d(Polygon2d polygon)
		{
			_points = new Point3d[polygon.Count];
			for (int i = 0; i < _points.Length; i++)
				_points[i] = new Point3d(polygon[i]);
		}

		/// <summary>
		/// Creates a polygon on the plane z = 0 from a polygon on the XY plane (copies of the vertices)
		/// </summary>
		/// <param name="polygon">The polygon on the XY plane</param>
		/// <param name="tolerance">Not used: the polygon is planar</param>
		/// <remarks>The points of a <see cref="Polygon2d"/> are on the plane z = 0: they are copied in one allocation without the planarity check
		/// (before, Add copied the array at every point: O(n^2))</remarks>
		public Polygon3d(Polygon2d polygon, double tolerance = GeometryBase.Tolerance)
			: this(polygon)
		{
		}

		/// <summary>
		/// Deserialization constructor: reads the vertices (the saved <see cref="BaseObject.Guid"/> is not read)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
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
		/// Add a new point at the end of the polygon (the instance is kept, not copied); a null point is ignored
		/// </summary>
		/// <param name="point">The point to add</param>
		/// <param name="tolerance">The tolerance of the planarity check</param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar (the point stays in the polygon)</exception>
		/// <remarks>Only the new point is checked, against a plane of three points of the polygon kept up to date by the previous calls (O(1)).
		/// The whole polygon is checked when its coordinates or vertices changed, or the new point is not on that plane</remarks>
		public void Add(Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			if (point == null)
				return;

			bool changedPoints = EnsurePointTracking();
			if (changedPoints || _planePointVersion != _pointChanges.Version || !IsPlaneBasisValid(tolerance))
			{
				if (!IsPlanar(tolerance))
					throw new ArgumentException("The existing polygon is not planar");
				ComputePlaneBasis(tolerance);
				_planePointVersion = _pointChanges.Version;
			}

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

		[NonSerialized] private Point3d.ChangeTracker _pointChanges;
		[NonSerialized] private List<Point3d> _observedPoints;
		[NonSerialized] private Point3d[] _trackingArray;
		[NonSerialized] private int _planePointVersion;
		[NonSerialized] private bool _pointsExposed;

		private bool EnsurePointTracking()
		{
			bool changed = !ReferenceEquals(_trackingArray, _points);
			if (!changed && _pointsExposed)
				for (int i = 0; i < _points.Length; i++)
					if (!ReferenceEquals(_observedPoints[i], _points[i])) { changed = true; break; }
			if (!changed) return false;
			if (_pointChanges == null) _pointChanges = new Point3d.ChangeTracker();
			if (_observedPoints != null)
				foreach (Point3d point in _observedPoints) point?.UntrackChanges(_pointChanges);
			_observedPoints = new List<Point3d>(_points);
			foreach (Point3d point in _observedPoints) point?.TrackChanges(_pointChanges);
			_trackingArray = _points;
			return true;
		}

		/// <summary>
		/// The indices of up to three points of the polygon that define its plane (3 points), its line (2 points: the points are aligned)
		/// or its position (1 point: the points are coincident); used by <see cref="Add(Point3d, double)"/>
		/// </summary>
		[NonSerialized]
		private int[] _planeBasisIndices;
		/// <summary>
		/// The instances of the points of <see cref="_planeBasisIndices"/>: they tell if the points have been changed by other methods
		/// </summary>
		[NonSerialized]
		private Point3d[] _planeBasisPoints;
		/// <summary>
		/// The tolerance used to choose the points of <see cref="_planeBasisIndices"/>
		/// </summary>
		[NonSerialized]
		private double _planeBasisTolerance;

		/// <summary>
		/// Tell if the plane basis can be used: computed with the same tolerance and with the points still in the polygon at the same indices
		/// </summary>
		/// <param name="tolerance">The tolerance of the current check</param>
		/// <returns>True if the basis is valid</returns>
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

		/// <summary>
		/// Sets the plane basis
		/// </summary>
		/// <param name="tolerance">The tolerance used to choose the points</param>
		/// <param name="indices">The indices of the points (0 to 3)</param>
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
		/// <param name="tolerance">The distances not larger than the tolerance are not considered (coincident or aligned points)</param>
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
		/// <param name="point">The point just added</param>
		/// <param name="index">The index of the point</param>
		/// <param name="tolerance">The tolerance on the distances</param>
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

		/// <summary>
		/// The distance of a point from a line
		/// </summary>
		/// <param name="a">A point of the line</param>
		/// <param name="b">Another point of the line (different from <paramref name="a"/>)</param>
		/// <param name="point">The point</param>
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
		/// Add a new point at the end of the polygon (the instance is kept). This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="point">The point to add; null: nothing is added</param>
		internal void AddWithoutChecks(Point3d point)
		{
			if (point != null)
			{
				Point3d[] pointBuffer = _points;
				_points = new Point3d[_points.Length + 1];
				for (int i = 0; i < pointBuffer.Length; i++)
					_points[i] = pointBuffer[i];
				_points[_points.Length - 1] = point;
				if (ReferenceEquals(_trackingArray, pointBuffer))
				{
					point.TrackChanges(_pointChanges);
					_observedPoints.Add(point);
					_trackingArray = _points;
				}
				_pointsExposed = false; // Previously exported arrays no longer own the polygon's slots.
			}
		}

		/// <summary>
		/// Add a range of points at the end of the polygon (the instances are kept). This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="point">The points to add; null: nothing is added</param>
		internal void AddWithoutChecks(Point3d[] point)
		{
			if (point != null)
			{
				AddRange(point);
			}
		}

		/// <summary>
		/// Add a new point at the end of the polygon, checking the planarity with the default tolerance
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <param name="z">The Z coordinate</param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar (the point stays in the polygon)</exception>
		public void Add(double x, double y, double z)
		{
			Add(new Point3d(x, y, z));
		}

		/// <summary>
		/// Add a range of points at the end of the polygon (the instances are kept), without the planarity check
		/// </summary>
		/// <param name="points">The points to add; null: nothing is added</param>
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
		/// Add a range of points at the end of the polygon (the instances are kept), without the planarity check
		/// </summary>
		/// <param name="points">The points to add (not null)</param>
		internal void AddRange(IEnumerable<Point3d> points)
		{
			AddRange(points.ToArray());
		}

		/// <summary>
		/// Insert a new point at the position given by index (the instance is kept), then checks the planarity of the whole polygon
		/// </summary>
		/// <param name="index">The index of the new point, from 0 to <see cref="Count"/></param>
		/// <param name="point">The point to insert</param>
		/// <param name="tolerance">The tolerance of the planarity check</param>
		/// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar (the point stays in the polygon)</exception>
		public void Insert(int index, Point3d point, double tolerance = GeometryBase.Tolerance)
		{
			Insert(index, point);
			if (!IsPlanar(tolerance))
			{
				throw new ArgumentException($"Polygon with additional point {point} is not planar");
			}
		}

		/// <summary>
		/// Insert a new point at the position given by index (the instance is kept). This method don't check if the polygon is planar or not
		/// </summary>
		/// <param name="index">The index of the new point, from 0 to <see cref="Count"/></param>
		/// <param name="point">The point to insert (a null point is inserted too)</param>
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
		/// <param name="index">The index of the new point, from 0 to <see cref="Count"/></param>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <param name="z">The Z coordinate</param>
		public void Insert(int index, double x, double y, double z)
		{
			Insert(index, new Point3d(x, y, z));
		}

		/// <summary>
		/// The index of the vertex before the vertex <paramref name="i"/>: i - 1, the last vertex before the first one
		/// </summary>
		/// <param name="i">The index of the vertex</param>
		/// <returns>The index of the previous vertex</returns>
		/// <exception cref="IndexOutOfRangeException">If <paramref name="i"/> is negative or greater than <see cref="Count"/> - 1</exception>
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
		/// The vertex before the vertex <paramref name="i"/> (the last one before the first one)
		/// </summary>
		/// <param name="i">The index of the vertex</param>
		/// <returns>The previous vertex</returns>
		public Point3d GetPreviousPoint(int i)
		{
			return this[GetPreviousIndex(i)];
		}

		/// <summary>
		/// The index of the vertex after the vertex <paramref name="i"/>: i + 1, 0 after the last vertex
		/// </summary>
		/// <param name="i">The index of the vertex</param>
		/// <returns>The index of the next vertex</returns>
		/// <exception cref="IndexOutOfRangeException">If <paramref name="i"/> is greater than <see cref="Count"/> - 1 (the negative values are not checked)</exception>
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
		/// The vertex after the vertex <paramref name="i"/> (the first one after the last one)
		/// </summary>
		/// <param name="i">The index of the vertex</param>
		/// <returns>The next vertex</returns>
		public Point3d GetNextPoint(int i)
		{
			return this[GetNextIndex(i)];
		}

		/// <summary>
		/// Removes the vertex at index <paramref name="index"/>
		/// </summary>
		/// <param name="index">The index of the vertex, from 0 to <see cref="Count"/> - 1</param>
		/// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is out of range (the polygon loses its last vertex anyway)</exception>
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
		/// <param name="tolerance">The tolerance on the distance</param>
		/// <returns>True if a vertex is closer than <paramref name="tolerance"/> to the point</returns>
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
		/// <param name="x">The X coordinate of the point</param>
		/// <param name="y">The Y coordinate of the point</param>
		/// <param name="z">The Z coordinate of the point</param>
		/// <param name="tolerance">The tolerance on the distance</param>
		/// <returns>True if a vertex is closer than <paramref name="tolerance"/> to the point</returns>
		public bool PointExists(double x, double y, double z, double tolerance = GeometryBase.Tolerance)
		{
			return PointExists(new Point3d(x, y, z), tolerance);
		}

		/// <summary>
		/// Tell if a point is on an edge (vertices included)
		/// </summary>
		/// <param name="point">The point to test</param>
		/// <param name="tolerance">The tolerance (see <see cref="Line3d.IsPointOnLine(Point3d, double)"/>)</param>
		/// <returns>The index of the first edge where the point is on (the edge i goes from the vertex i to the next one), otherwise -1</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="point"/> is null</exception>
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
		/// <param name="start">The start of the segment</param>
		/// <param name="end">The end of the segment</param>
		/// <param name="point">The point to test</param>
		/// <param name="tolerance">The tolerance on the coordinates</param>
		/// <returns>True if the point is aligned with the ends (within the tolerance) and between them</returns>
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
		/// Tell if a point is on an edge (vertices included)
		/// </summary>
		/// <param name="x">The X coordinate of the point</param>
		/// <param name="y">The Y coordinate of the point</param>
		/// <param name="z">The Z coordinate of the point</param>
		/// <param name="tolerance">The tolerance (see <see cref="Line3d.IsPointOnLine(Point3d, double)"/>)</param>
		/// <returns>The index of the first edge where the point is on, otherwise -1</returns>
		public int IsPointOnEdge(double x, double y, double z, double tolerance = GeometryBase.Tolerance)
		{
			return IsPointOnEdge(new Point3d(x, y, z), tolerance);
		}

		/// <summary>
		/// The edges of the polygon: the edge i goes from the vertex i to the next one (the last edge closes the polygon)
		/// </summary>
		/// <returns>The edges, as many as the vertices</returns>
		public Line3d[] Explode()
		{
			Line3d[] lines = new Line3d[_points.Length];

			for (int i = 0; i < _points.Length; i++)
				lines[i] = new Line3d(this[i], GetNextPoint(i));

			return lines;
		}

		/// <summary>
		/// Removes the consecutive vertices (also the last and the first one) closer than <paramref name="tolerance"/>: of each group of
		/// close vertices only the first one is kept
		/// </summary>
		/// <param name="tolerance">The tolerance on the distance</param>
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
		/// The area of the polygon with a sign: positive if the normal (right hand rule on the vertices order) has a positive component along the
		/// global Z axis, negative otherwise (vertical polygons included)
		/// </summary>
		/// <param name="tolerance">Not used</param>
		/// <returns>The signed area; 0 if the polygon has less than 3 vertices</returns>
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
		/// <returns>The area vector (zero vector if the polygon has less than 3 vertices)</returns>
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
		/// <param name="tolerance">The polygon is degenerate if its area is not larger than tolerance × perimeter</param>
		/// <param name="normal">The unit normal; null if the result is false</param>
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

		/// <summary>
		/// The perimeter of the polygon (the last edge included)
		/// </summary>
		/// <returns>The sum of the lengths of the edges</returns>
		private double GetPerimeter()
		{
			double perimeter = 0;
			for (int i = 0; i < _points.Length; i++)
				perimeter += _points[i].DistanceTo(_points[(i + 1) % _points.Length]);
			return perimeter;
		}

		/// <summary>
		/// The normal from the first three vertices after removing the duplicated and the aligned ones (for the degenerate polygons)
		/// </summary>
		/// <param name="tolerance">The tolerance of the removal of the points</param>
		/// <returns>The unit normal (right hand rule on the three vertices)</returns>
		/// <exception cref="NotSupportedException">If less than three vertices remain</exception>
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
		/// Get the vector normal of the polygon from its first three vertices, without removing duplicated or aligned points
		/// </summary>
		/// <returns>A unitized vector normal to the polygon (NaN components if the first three vertices are aligned)</returns>
		/// <exception cref="NotSupportedException">Thrown when the polygon has less than three vertices</exception>
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
		/// <param name="tolerance">Not used</param>
		/// <returns>True if the signed area is positive (see <see cref="GetSignedArea"/>)</returns>
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
		/// Remove the aligned points: a vertex is removed when it is within <paramref name="tolerance"/> from the line through its current
		/// neighbours (the vertices not removed) and the points already removed between them stay within the tolerance from that line, so every
		/// removed point is within the tolerance from the final side. The spikes (a vertex on the line of its neighbours but outside their
		/// segment) and the repeated consecutive vertices are removed too. If all the points are aligned, the two extremes are kept, in their
		/// order; otherwise at least three points are kept.
		/// Before, the tolerance was an angle, the neighbours were the original ones and, with all the points aligned, the last two points were kept
		/// </summary>
		/// <param name="tolerance">The tolerance on the distance from the line of the neighbours</param>
		public void RemoveAlignedPoints(double tolerance = GeometryBase.Tolerance)
		{
			if (_points.Length < 3)
				return;

			var x = new double[_points.Length];
			var y = new double[_points.Length];
			var z = new double[_points.Length];
			for (int i = 0; i < _points.Length; i++)
			{
				x[i] = _points[i].X;
				y[i] = _points[i].Y;
				z[i] = _points[i].Z;
			}

			int[] kept = AlignedPoints.GetIndicesToKeep(x, y, z, tolerance);
			if (kept.Length == _points.Length)
				return;

			var points = new Point3d[kept.Length];
			for (int i = 0; i < kept.Length; i++)
				points[i] = _points[kept[i]];
			_points = points;
		}

		/// <summary>
		/// Check if the polygon is planar: all the vertices are within the tolerance from the plane through their mean point with the Newell normal
		/// (before, the plane of the first three not aligned vertices); for a degenerate polygon (area close to zero) the plane of the first three
		/// not aligned vertices
		/// </summary>
		/// <param name="tolerance">The tolerance on the distances from the plane (and of the removal of the duplicated and aligned points)</param>
		/// <returns>True if the polygon is planar (always true with 3 vertices or less)</returns>
		private bool IsPlanar(double tolerance = GeometryBase.Tolerance)
		{
			/*
			 * http://www.ambrsoft.com/TrigoCalc/Line3D/LineColinear.htm#:~:text=Collinear%203%20dimentional%20lines&text=Collinear%20points%20are%20all%20located%20on%20the%20same%20line.&text=Another%20way%20of%20checking%20whether,then%20the%20points%20are%20collinear.
			 * possibile alternativa di calcolo. funziona con il calcolo del determinante.
			 */

			// https://math.stackexchange.com/a/221856

			if (Count <= 3)
				return true;

			if (TryGetUnitNormal(tolerance, out Vector3d normal))
			{
				double mx = 0, my = 0, mz = 0;
				for (int i = 0; i < _points.Length; i++)
				{
					mx += _points[i].X;
					my += _points[i].Y;
					mz += _points[i].Z;
				}
				mx /= _points.Length;
				my /= _points.Length;
				mz /= _points.Length;

				for (int i = 0; i < _points.Length; i++)
				{
					double distance = normal.X * (_points[i].X - mx) + normal.Y * (_points[i].Y - my) + normal.Z * (_points[i].Z - mz);
					if (!(Math.Abs(distance) <= tolerance))
						return false;
				}

				return true;
			}

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
		/// Tell if a point is on the polygon plane (the plane of the first three not duplicated and not aligned vertices)
		/// </summary>
		/// <param name="pointToTest">Point to test</param>
		/// <param name="tol">The tolerance on the distance from the plane</param>
		/// <returns>True if the point is on polygon plane.
		/// True if the polygon contains less than three unique and not aligned points.
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
		/// <param name="tol">The tolerance on the distance from the plane and from the border</param>
		/// <returns>True if the point is on the plane of the polygon and inside it or on its border</returns>
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
				// With a tolerance large compared with the polygon it does not check the plane, because RemoveAlignedPoints removes
				// vertices of the polygon (before, the tolerance was used as an angle: e.g. Checker MixedSectionTest.FailureDomain02 relied on it)
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

		/// <summary>
		/// A coordinate of a point
		/// </summary>
		/// <param name="point">The point</param>
		/// <param name="axis">0: X, 1: Y, 2: Z</param>
		/// <returns>The coordinate</returns>
		private static double Coordinate(Point3d point, int axis)
		{
			return axis == 0 ? point.X : (axis == 1 ? point.Y : point.Z);
		}

		/// <summary>
		/// Same test of <see cref="Polygon2d.IsPointInside(Point2d, double)"/> on the projection of the polygon on the coordinates <paramref name="u"/>, <paramref name="v"/>
		/// </summary>
		/// <param name="pointToTest">The point to test</param>
		/// <param name="u">The first coordinate of the projection (0: X, 1: Y, 2: Z)</param>
		/// <param name="v">The second coordinate of the projection</param>
		/// <param name="tol">The tolerance on the distance from the border</param>
		/// <returns>True if the projection of the point is inside the projection of the polygon or on its border</returns>
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

		/// <summary>
		/// The previous test: the point must be on the plane of the first three not aligned vertices (see <see cref="IsPointOnPlane"/>) and inside
		/// the projection of the polygon with the largest area. Used for the degenerate polygons and for the points off the plane
		/// </summary>
		/// <param name="pointToTest">The point to test</param>
		/// <param name="tol">The tolerance</param>
		/// <returns>True if the point is inside</returns>
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
		/// Tell if a segment is inside the polygon (default tolerance): both the ends are inside (or on the border) and the segment does not
		/// cross the edges. The check is only on the crossings: a segment joining two points of the border that passes outside the polygon
		/// without crossing edges is considered inside
		/// </summary>
		/// <param name="lineToTest">The segment to test</param>
		/// <returns>True if the segment is inside the polygon or on its border</returns>
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
		/// Scale the polygon respect to the origin of the axes, with a factor for each axis
		/// </summary>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <returns>A new polygon scaled (the planarity is not checked)</returns>
		public Polygon3d Scale(double factorX, double factorY, double factorZ)
		{
			Point3d[] points = new Point3d[_points.Length];
			for (int i = 0; i < _points.Length; i++)
				points[i] = _points[i].Scale(factorX, factorY, factorZ);

			return new Polygon3d(points);
		}

		/// <summary>
		/// Scale the polygon respect to a point, with a factor for each axis
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <returns>A new polygon scaled (the planarity is not checked)</returns>
		public Polygon3d Scale(Point3d center, double factorX, double factorY, double factorZ)
		{
			Point3d[] points = new Point3d[_points.Length];
			for (int i = 0; i < _points.Length; i++)
				points[i] = _points[i].Scale(center, factorX, factorY, factorZ);

			return new Polygon3d(points);
		}

		/// <summary>
		/// Scale the polygon respect to a point
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factor">The scale factor</param>
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
		/// Get the coordinate system of the polygon: origin in the first point, X axis on the first side (to the first vertex farther than the
		/// tolerance), Z axis on the Newell normal (right hand rule on the vertices order, also for concave polygons), Y to complete the triad.
		/// For a degenerate polygon (area close to zero) it is generated from the first 3 points (not aligned, not duplicated). Before, always
		/// from the first 3 points: with a concave first vertex the Z axis was opposite to the normal
		/// </summary>
		/// <param name="tolerance">The tolerance of the removal of the duplicated and aligned points</param>
		/// <returns>Coordinate system generated from first three points of the polygon. Duplicate and aligned points are not considered</returns>
		/// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a coordinate system</exception>
		public CoordinateSystem GetCoordinateSystem(double tolerance = GeometryBase.Tolerance)
		{
			if (TryGetUnitNormal(tolerance, out Vector3d normal))
			{
				for (int i = 1; i < _points.Length; i++)
				{
					Vector3d side = _points[0].VectorTo(_points[i]);
					if (side.Length > tolerance)
					{
						side.Unitize();
						Vector3d yAxis = normal.CrossProduct(side);
						yAxis.Unitize();
						return new CoordinateSystem(_points[0], side, yAxis);
					}
				}
			}

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
		/// <returns>The bounding box of the vertices in the global coordinates</returns>
		public BoundingBox3d GetBoundingBox()
		{
			BoundingBox3d bbox = new BoundingBox3d();
			bbox.Update(_points);
			return bbox;
		}

		/// <summary>
		/// Create the triangles between each pair of consecutive vertices and the point center in input
		/// </summary>
		/// <param name="center">The point common at all triangles: it must be inside the polygon and not on an edge</param>
		/// <param name="tolerance">The tolerance of the checks on the position of <paramref name="center"/></param>
		/// <returns>The triangles, one for each edge</returns>
		/// <exception cref="Exception">If <paramref name="center"/> is outside the polygon or on an edge</exception>
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
		/// The barycenter of a triangle: the mean of its three vertices (the intersection of the medians)
		/// </summary>
		/// <returns>The barycenter of the triangle</returns>
		/// <exception cref="Exception">If the polygon is not a triangle</exception>
		internal Point3d GetBarycenterOfTriangle()
		{
			if (_points.Length != 3)
				throw new Exception("Polygon must be a triangle");

			return new Point3d((_points[0].X + _points[1].X + _points[2].X) / 3.0,
				(_points[0].Y + _points[1].Y + _points[2].Y) / 3.0,
				(_points[0].Z + _points[1].Z + _points[2].Z) / 3.0);
		}

		/// <summary>
		/// Get the centroid of the area of polygon. The areas of the triangles of the fan from the first vertex have sign respect to the
		/// normal, so the centroid is right also for the concave polygons and in any plane
		/// </summary>
		/// <returns>The centroid of the area; the mean of the vertices for polygons with less than three vertices or with zero area</returns>
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
		/// Find the triangle, among the ones joining the edges with the mean of the vertices, that contains a point and the linear shape
		/// functions of the point in that triangle (computed on the projection where the triangle has the largest area)
		/// </summary>
		/// <param name="pointToParametrize">The point to find the parametrization</param>
		/// <param name="triangle">The triangle of the triangularization where the point is inside; null if none is found</param>
		/// <param name="N1">The shape function of the first vertex of the triangle</param>
		/// <param name="N2">The shape function of the second vertex of the triangle</param>
		/// <param name="N3">The shape function of the third vertex (the mean of the vertices of the polygon)</param>
		/// <param name="tolerance">The tolerance</param>
		/// <exception cref="Exception">If the point is not inside the polygon</exception>
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
		/// Return the point3d (with shape functions N1, N2, N3) in global coordinates, as intersection of the lines of constant shape functions
		/// </summary>
		/// <param name="N1">The shape function N1</param>
		/// <param name="N2">The shape function N2</param>
		/// <param name="N3">The shape function N3</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The point3d in global coordinates</returns>
		/// <exception cref="Exception">If the polygon has more than three vertices or the point cannot be found</exception>
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
		/// Transform the insert point from the base polygon to the new polygon: the point keeps the shape functions of its triangle
		/// (see <see cref="GetPointParametrization"/>) in the corresponding triangle of the new polygon
		/// </summary>
		/// <param name="pointToParam">The point to transform, inside this polygon</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The point3d in global coordinates in the new polygon</returns>
		/// <exception cref="Exception">If the new polygon is not planar, not convex or it has a different number of vertices; if the point is not inside this polygon</exception>
		/// <exception cref="InvalidOperationException">If the triangle of the point is not found</exception>
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
		/// Transform the insert polygon from the base polygon to the new polygon (each vertex, see <see cref="Transform(Point3d, Polygon3d, double)"/>)
		/// </summary>
		/// <param name="polygonToParam">Polygon to transform</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The polygon in global coordinates in the new polygon</returns>
		public Polygon3d Transform(Polygon3d polygonToParam, Polygon3d newPolygon, double tolerance = GeometryBase.Tolerance)
		{
			Point3d[] points = new Point3d[polygonToParam.Count];

			for (int i = 0; i < polygonToParam.Count; i++)
				points[i] = Transform(polygonToParam[i], newPolygon, tolerance);

			return new Polygon3d(points, tolerance);
		}

		/// <summary>
		/// Transform the insert shape from the base polygon to the new polygon (fill and holes, see <see cref="Transform(Point3d, Polygon3d, double)"/>)
		/// </summary>
		/// <param name="shapeToParam">Shape to transform</param>
		/// <param name="newPolygon">The new polygon. It must have the same number of edges of the base polygon and it must be planar and convex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The shape in global coordinates in the new polygon</returns>
		/// <exception cref="NotImplementedException">If the shape has children</exception>
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
		/// Return the polygon in the local coordinate system of the polygon (see <see cref="GetCoordinateSystem"/>), on its XY plane
		/// </summary>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>The polygon in local coordinates</returns>
		public Polygon2d GetPolygon2d(double tolerance = GeometryBase.Tolerance)
		{
			return GetCoordinateSystem(tolerance).ToLocal(this);
		}

		#endregion

		#region Interface implementation

		/// <summary>
		/// Clone the polygon, with copies of the vertices, checking the planarity with <paramref name="tolerance"/>
		/// </summary>
		/// <param name="tolerance">The tolerance of the planarity check</param>
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

		/// <summary>
		/// Enumerates the vertices (of the array at the moment of the call: adding or removing points does not change the enumeration)
		/// </summary>
		/// <returns>The enumerator of the vertices</returns>
		public IEnumerator<Point3d> GetEnumerator()
		{
			return ((IEnumerable<Point3d>)_points).GetEnumerator(); // the array is replaced (not changed) when points are added or removed
		}

		/// <summary>
		/// Enumerates the vertices (see <see cref="GetEnumerator()"/>)
		/// </summary>
		/// <returns>The enumerator of the vertices</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Public method override

		/// <summary>
		/// Equality with another object (see <see cref="Equals(Polygon3d)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal polygon</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as Polygon3d);
		}

		/// <summary>
		/// The hash code of the exact coordinates of the vertices
		/// </summary>
		/// <returns>The hash code</returns>
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

		/// <summary>
		/// Tell if <paramref name="obj"/> is a polygon with the same vertices in the same order, starting from any vertex
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <param name="tolerance">The tolerance on the distances of the vertices (the first vertex is found with the default tolerance)</param>
		/// <returns>True if the polygons have the same vertices, also shifted</returns>
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
		/// Execute a boolean operation beetween polygons on the XY plane using the Clipper class (even-odd fill rule). Not used
		/// </summary>
		/// <param name="a">The subject polygons</param>
		/// <param name="b">The clip polygons</param>
		/// <param name="code">The operation</param>
		/// <param name="factor">Scale of the coordinates; not positive (default): automatic, see <see cref="ClipperScale"/></param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty or Clipper fails</returns>
		/// <exception cref="InvalidOperationException">If the root of the result of Clipper has a contour</exception>
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
		/// <param name="polygon">The polygon (Z is ignored)</param>
		/// <param name="factor">The scale factor for double to integer conversion for the coordinates</param>
		/// <param name="clipper">The clipper object for the boolean operations</param>
		/// <param name="type">Tell if the shape polygon is a Subject or Clip polygon</param>
		private static void AddToPath(Polygon3d polygon, double factor, Clipper clipper, PolyType type)
		{
			List<IntPoint> buffer = GetIntPointList(polygon, factor);
			clipper.AddPath(buffer, type, true);
		}

		/// <summary>
		/// Create an IntPoint list from the X and Y of the vertices of the given polygon (Z is ignored). IntPoint is a subclass of Clipper
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
		/// Converts back the resulting clipper.PolyNode in polygons on the plane z = 0: the contour of the node and, recursively, the ones of its children
		/// </summary>
		/// <param name="polyNode">The PolyNode to convert</param>
		/// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
		/// <param name="result">The list where the polygons are added</param>
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

		/// <summary>
		/// Serializes the polygon: the <see cref="BaseObject.Guid"/> and the vertices
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Count", _points.Length);
			for (int i = 0; i < _points.Length; i++)
			{
				info.AddValue($"Point{i}", _points[i], typeof(Point3d));
			}
		}

		/// <summary>
		/// Equality of the vertices, in the same order and starting from the same vertex (see <see cref="Point3d.Equals(Point3d)"/>)
		/// </summary>
		/// <param name="other">The polygon to compare</param>
		/// <returns>True if the polygons have equal vertices</returns>
		public bool Equals(Polygon3d other)
		{
			if (ReferenceEquals(this, other))
				return true;

			// Sequence Equals:
			// true if the two source sequences are of equal length and their corresponding elements are equal according to the default equality comparer for their type; 
			// otherwise, false.
			return !(other is null) && other._points.SequenceEqual(_points);
		}

		/// <summary>
		/// Equality with another geometry (see <see cref="Equals(Polygon3d)"/>)
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal polygon</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Polygon3d point)
				return Equals(point);

			return false;
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(Polygon3d)"/>); two null polygons are equal
		/// </summary>
		/// <param name="obj1">The first polygon</param>
		/// <param name="obj2">The second polygon</param>
		/// <returns>True if the polygons are equal</returns>
		public static bool operator ==(Polygon3d obj1, Polygon3d obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		/// <summary>
		/// Inequality operator (see <see cref="Equals(Polygon3d)"/>)
		/// </summary>
		/// <param name="obj1">The first polygon</param>
		/// <param name="obj2">The second polygon</param>
		/// <returns>True if the polygons are different</returns>
		public static bool operator !=(Polygon3d obj1, Polygon3d obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}


