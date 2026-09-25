using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Extensions;

namespace GPC.Geometry
{
    /// <summary>
    /// A planar shape in the space: an outer polygon (<see cref="Fill"/>), the polygons of its holes and the shapes inside the holes
    /// (<see cref="Childs"/>). The holes are oriented as the fill when the shape is created from polygons
    /// </summary>
    [Serializable]
    public class Shape : GeometryBase, ISerializable, ICloneable, IEquatable<Shape>
    {
        #region Variables

        /// <summary>
        /// The outer polygon
        /// </summary>
        protected Polygon3d _fill;
        /// <summary>
        /// The holes; null if the shape has no holes
        /// </summary>
        protected Polygon3d[] _holes;
        /// <summary>
        /// The shapes inside the holes; null if the shape has no children
        /// </summary>
        protected Shape[] _childs;

        #endregion

        #region Properties

        /// <summary>
        /// The outer polygon (the instance of the shape)
        /// </summary>
        public Polygon3d Fill { get => _fill; set { _fill = value; } }

        /// <summary>
        /// The holes (the array of the shape); null if the shape has no holes
        /// </summary>
        public Polygon3d[] Holes { get => _holes; set { _holes = value; } }

        /// <summary>
        /// True if the array of the holes is not null
        /// </summary>
        public bool HasHoles => _holes != null;

        /// <summary>
        /// The shapes inside the holes (the array of the shape); null if the shape has no children
        /// </summary>
        public Shape[] Childs { get => _childs; set { _childs = value; } }

        /// <summary>
        /// True if the array of the children is not null
        /// </summary>
        public bool HasChilds => _childs != null;

        #endregion

        #region Public constructor

        /// <summary>
        /// Create a new shape from a 3d planar polygon for fill, 3d planar polygons for holes and shape for childs (the instances are kept, not copied).
        /// The holes not oriented as the fill are reversed in place
        /// </summary>
        /// <param name="fill">The outer polygon</param>
        /// <param name="holes">The holes; null: no holes</param>
        /// <param name="childs">The shapes inside the holes; null: no children</param>
        /// <param name="tolerance">The tolerance of the normals</param>
        /// <exception cref="NotSupportedException">If the fill or a hole has not three distinct and not aligned points</exception>
        public Shape(Polygon3d fill, Polygon3d[] holes = null, Shape[] childs = null, double tolerance = GeometryBase.Tolerance)
        {
            _fill = fill;
            _holes = holes;
            _childs = childs;
            Vector3d normal = _fill.GetNormalVector(tolerance);

            if (_holes != null)
            {
				for (int i = 0; i < _holes.Length; i++)
                {
					Polygon3d hole = _holes[i];
					if ((hole.GetNormalVector(tolerance).DotProduct(normal)) < tolerance)
                    {
                        hole.Reverse();
                    }
                }
            }
        }

        /// <summary>
        /// Create a new shape on the plane z = 0 from 2d polygons for fill and holes (copied) and 2d shapes for childs (the instances are kept).
        /// The holes and the children not oriented as the fill are reversed (the children in place)
        /// </summary>
        /// <param name="fill">The outer polygon</param>
        /// <param name="holes">The holes; null: no holes</param>
        /// <param name="childs">The shapes inside the holes; null: no children</param>
        /// <param name="tolerance">The tolerance of the normals</param>
        /// <exception cref="NotSupportedException">If the fill or a hole has not three distinct and not aligned points</exception>
        public Shape(Polygon2d fill, Polygon2d[] holes = null, Shape2d[] childs = null, double tolerance = GeometryBase.Tolerance)
        {
            _fill = new Polygon3d(fill, tolerance);
            _holes = null;
            _childs = null;

			Vector3d normal = _fill.GetNormalVector(tolerance);

            if (holes != null)
            {
                _holes = new Polygon3d[holes.Length];
                for (int u = 0; u < holes.Length; u++)
                {
                    _holes[u] = new Polygon3d(holes[u], tolerance);
                }
            }

            if (_holes != null)
            {
                for (int i = 0; i < _holes.Length; i++)
                {
                    if (_holes[i].GetNormalVector(tolerance).DotProduct(normal) < tolerance)
                    {
                        _holes[i].Reverse();
                    }
                }
            }

			// The childs are kept as Shape2d instances (Shape2d.Childs2d casts them), as for the holes of the Polygon3d constructor
			// the childs with opposite orientation are reversed in place
			_childs = childs;

			if (_childs != null)
			{
				for (int i = 0; i < _childs.Length; i++)
				{
					if (_childs[i].GetNormalVector(tolerance).DotProduct(normal) < tolerance)
					{
						_childs[i].Reverse();
					}
				}
			}
        }

        /// <summary>
        /// Creates a copy of the shape (fill, holes and children), checking the planarity of the polygons with <paramref name="tolerance"/>.
        /// The children keep their type (<see cref="Shape2d"/> or <see cref="Shape"/>)
        /// </summary>
        /// <param name="shape">The shape to copy</param>
        /// <param name="tolerance">The tolerance of the planarity check</param>
        /// <exception cref="ArgumentException">If a polygon is not planar within <paramref name="tolerance"/></exception>
        public Shape(Shape shape, double tolerance = GeometryBase.Tolerance)
        {
            _fill = new Polygon3d(shape._fill, tolerance);
            if (shape._holes != null)
            {
                _holes = new Polygon3d[shape._holes.Length];
                for (int i = 0; i < shape._holes.Length; i++)
                {
                    _holes[i] = new Polygon3d(shape._holes[i], tolerance);
                }
            }
            if (shape._childs != null)
            {
                _childs = new Shape[shape._childs.Length];
                for (int i = 0; i < shape._childs.Length; i++)
                {
                    _childs[i] = shape._childs[i] is Shape2d child2d ? new Shape2d(child2d, tolerance) : new Shape(shape._childs[i], tolerance); // keep the type of the child
                }
            }
        }

		/// <summary>
		/// Creates a copy of the shape (fill, holes and children, with copies of the vertices). The children keep their type
		/// </summary>
		/// <param name="shape">The shape to copy</param>
		public Shape(Shape shape)
		{
			_fill = new Polygon3d(shape._fill);
			if (shape._holes != null)
			{
				_holes = new Polygon3d[shape._holes.Length];
				for (int i = 0; i < shape._holes.Length; i++)
				{
					_holes[i] = new Polygon3d(shape._holes[i]);
				}
			}
			if (shape._childs != null)
			{
				_childs = new Shape[shape._childs.Length];
				for (int i = 0; i < shape._childs.Length; i++)
				{
					_childs[i] = shape._childs[i] is Shape2d child2d ? new Shape2d(child2d) : new Shape(shape._childs[i]); // keep the type of the child
				}
			}
		}

		/// <summary>
		/// Creates a shape (not a <see cref="Shape2d"/>) that is a copy of a 2d shape; the planarity of the fill is checked with <paramref name="tolerance"/>
		/// </summary>
		/// <param name="shape2d">The shape to copy</param>
		/// <param name="tolerance">The tolerance of the planarity check of the fill</param>
		public Shape(Shape2d shape2d, double tolerance = GeometryBase.Tolerance)
        {
            _fill = new Polygon3d(shape2d.Fill, tolerance);
            if (shape2d.Holes != null)
            {
                _holes = new Polygon3d[shape2d.Holes.Length];
                for (int i = 0; i < shape2d.Holes.Length; i++)
                {
                    _holes[i] = new Polygon3d(shape2d.Holes[i]);
                }
            }
            if (shape2d.Childs != null)
            {
                _childs = new Shape[shape2d.Childs.Length];
                for (int i = 0; i < shape2d.Childs.Length; i++)
                {
                    _childs[i] = shape2d.Childs[i] is Shape2d child2d ? new Shape2d(child2d) : new Shape(shape2d.Childs[i]); // keep the type of the child
                }
            }
        }

		/// <summary>
		/// Creates a shape (not a <see cref="Shape2d"/>) that is a copy of a 2d shape
		/// </summary>
		/// <param name="shape2d">The shape to copy</param>
		public Shape(Shape2d shape2d)
		{
			_fill = new Polygon3d(shape2d.Fill);
			if (shape2d.Holes != null)
			{
				_holes = new Polygon3d[shape2d.Holes.Length];
				for (int i = 0; i < shape2d.Holes.Length; i++)
				{
					_holes[i] = new Polygon3d(shape2d.Holes[i]);
				}
			}
			if (shape2d.Childs != null)
			{
				_childs = new Shape[shape2d.Childs.Length];
				for (int i = 0; i < shape2d.Childs.Length; i++)
				{
					_childs[i] = shape2d.Childs[i] is Shape2d child2d ? new Shape2d(child2d) : new Shape(shape2d.Childs[i]); // keep the type of the child
				}
			}
		}

		/// <summary>
		/// Deserialization constructor: reads fill, holes and children (the saved <see cref="BaseObject.Guid"/> is not read)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected Shape(SerializationInfo info, StreamingContext context)
        {
            _fill = (Polygon3d)info.GetValue("Fill", typeof(Polygon3d));
            int holesCount = info.GetInt32("HolesCount");
            if (holesCount > 0)
            {
                _holes = new Polygon3d[holesCount];
                for (int i = 0; i < holesCount; i++)
                {
                    _holes[i] = (Polygon3d)info.GetValue($"Hole{i}", typeof(Polygon3d));
                }
            }
            int childCount = info.GetInt32("ChildsCount");
            if (childCount > 0)
            {
                _childs = new Shape[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    _childs[i] = (Shape)info.GetValue($"Child{i}", typeof(Shape));
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Move shape by an given increment (in place: fill, holes and children)
        /// </summary>
        /// <param name="dx">The X coordinate increment</param>
        /// <param name="dy">The Y coordinate increment</param>
        /// <param name="dz">The Z coordinate increment</param>
        public override void Move(double dx, double dy, double dz)
        {
            _fill.Move(dx, dy, dz);
            if (_holes != null)
            {
				for (int i = 0; i < _holes.Length; i++)
                {
                    _holes[i].Move(dx, dy, dz);
                }
            }
            if (_childs != null)
            {
				for (int i = 0; i < _childs.Length; i++)
                {
                    _childs[i].Move(dx, dy, dz);
                }
            }
        }

        /// <summary>
        /// Move shape by an given vector (in place)
        /// </summary>
        /// <param name="vector">The displacement</param>
        public override void Move(Vector3d vector)
        {
            Move(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Move the shape (in place) so that the minimum corner of its bounding box goes to the origin of the axes
        /// </summary>
        public void MoveToMin()
        {
            BoundingBox3d bbox = GetBoundingBox();
            Move(-bbox.Min.X, -bbox.Min.Y, -bbox.Min.Z);
        }

		/// <summary>
		/// The mirror shape about a line (see <see cref="Shape2d.Mirror(double, double, double, double)"/>): not implemented for the shapes in the space
		/// </summary>
		/// <param name="a">The 'a' parameter of the equation</param>
		/// <param name="b">The 'b' parameter of the equation</param>
		/// <param name="c">The 'c' parameter of the equation</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>The mirror shape</returns>
		/// <exception cref="NotImplementedException">Always, for a <see cref="Shape"/></exception>
		public virtual Shape Mirror(double a, double b, double c, double tolerance = GeometryBase.Tolerance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a copy of the shape (see <see cref="Shape(Shape)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public override object Clone()
        {
            return new Shape(this);
        }

        /// <summary>
        /// The area of the shape: area of the fill, minus the areas of the holes, plus the areas of the children
        /// </summary>
        /// <param name="tolerance">Not used</param>
        /// <returns>The area of the shape (not negative)</returns>
        /// <exception cref="ArgumentException">If the result is negative (holes larger than the fill)</exception>
        public double GetArea(double tolerance = GeometryBase.Tolerance)
        {
            double area = _fill.GetSignedArea(tolerance);

            if (area < 0)
                area = -area;


            if (_holes != null)
            {
                for (int i = 0; i < _holes.Length; i++)
                {
                    var holeArea = _holes[i].GetSignedArea(tolerance);
                    if (holeArea < 0)
                        holeArea *= -1; 

                    area -= holeArea;
                }
            }

            if (_childs != null)
            {
                for (int i = 0; i < _childs.Length; i++)
                {
                    var childArea = _childs[i].GetArea(tolerance);
                    if (childArea < 0)
                        throw new ArgumentException("Child area lower than zero");

                    area += childArea;
                }
            }

            if (area < 0)
            {
                throw new ArgumentException("Shape area lower than zero");
            }

            return area;
        }

        /// <summary>
        /// The bounding box of the fill and of the holes (the children are inside the holes)
        /// </summary>
        /// <returns>The bounding box of the shape in the global coordinates</returns>
        public BoundingBox3d GetBoundingBox()
        {
            BoundingBox3d bbox = new BoundingBox3d();
            bbox.Update(_fill);
            if (_holes != null)
            {
				for (int i = 0; i < _holes.Length; i++)
                {
					bbox.Update(_holes[i]);
                }
            }
            return bbox;
        }

        /// <summary>
        /// The unit normal of the shape: the normal of the fill (see <see cref="Polygon3d.GetNormalVector(double)"/>)
        /// </summary>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>A unitized vector normal to the shape</returns>
        /// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a normal vector</exception>
        public virtual Vector3d GetNormalVector(double tolerance = GeometryBase.Tolerance)
        {
            return Fill.GetNormalVector(tolerance);
        }

        /// <summary>
        /// Get the coordinate system of the shape. Generated from first 3 points (not aligned, not duplicated) of the fill.
        /// Origin in first point, X axis on the first side, Z axis on the normal, Y to complete the triad.
        /// </summary>
        /// <param name="tolerance">The tolerance of the removal of the duplicated and aligned points</param>
        /// <returns>Coordinate system generated from first three points of the fill polygon. Duplicate and aligned points are not considered</returns>
        /// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a coordinate system</exception>
        public CoordinateSystem GetCoordinateSystem(double tolerance = GeometryBase.Tolerance)
        {
            return Fill.GetCoordinateSystem(tolerance);
        }

        /// <summary>
        /// The vertices of the fill, of the holes and, recursively, of the children
        /// </summary>
        /// <returns>The vertices (the instances of the polygons)</returns>
        public Point3d[] GetPoints()
        {
            List<Point3d> points = new List<Point3d>();

            for (int i = 0; i < Fill.Count; i++)
                points.Add(Fill[i]);

            if (HasHoles)
				for (int i = 0; i < Holes.Length; i++)				
                    for(int j = 0; j < Holes[i].Count; j++)
                        points.Add(Holes[i][j]);				

			if (HasChilds)
            {
				for (int i = 0; i < Childs.Length; i++)
                {
					points.AddRange(Childs[i].GetPoints());
                }
            }

            return points.ToArray();
        }

        /// <summary>
        /// The plane of the fill: through its first vertex, with the Newell normal (right hand rule on the vertices order, also for concave
        /// fills) and the first axis on the first side. For a degenerate fill (area close to zero) the plane of the first three vertices, after
        /// removing the aligned and the duplicated points. Before, always the plane of the first three vertices: with a concave first vertex
        /// the normal was reversed
        /// </summary>
        /// <param name="tolerance">The tolerance of the removal of the points and of the plane</param>
        /// <returns>The plane of the shape</returns>
        /// <exception cref="ArgumentException">If the three points are aligned</exception>
        public Plane GetPlane(double tolerance = GeometryBase.Tolerance)
        {
            if (Fill.TryGetUnitNormal(tolerance, out Vector3d normal))
            {
                for (int i = 1; i < Fill.Count; i++)
                {
                    Vector3d side = Fill[0].VectorTo(Fill[i]);
                    if (side.Length > tolerance)
                        return new Plane(Fill[0], side, normal.CrossProduct(side));
                }
            }

            var fill = (Polygon3d)Fill.Clone();

            fill.RemoveAlignedPoints(tolerance);
            fill.RemoveDuplicatedPoints(tolerance);

            return new Plane(fill[0], fill[1], fill[2], tolerance);
        }

        /// <summary>
        /// Move the shape to its local coordinate system (see <see cref="GetCoordinateSystem"/>)
        /// </summary>
        /// <param name="tolerance">The tolerance of the coordinate system</param>
        /// <returns>A new shape in local coordinate system, on its XY plane</returns>
        public Shape2d ToLocal(double tolerance = GeometryBase.Tolerance)
        {
            CoordinateSystem newCoordSystem = GetCoordinateSystem(tolerance);
            return newCoordSystem.ToLocal(this);
        }

        /// <summary>
        /// Move the shape from a local coordinate system to the global one
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system where the shape is defined</param>
        /// <returns>A new shape in global coordinate system</returns>
        public Shape ToGlobal(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem.ToGlobal(this);
        }

        /// <summary>
        /// Check if the point is on shape: on the plane of the fill, inside the fill or on its border, not strictly inside a hole
        /// (the border of the holes belongs to the shape), or inside a child
        /// </summary>
        /// <param name="pointToTest">The point to test</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <returns>True if the point is on shape</returns>
        public bool IsPointInside(Point3d pointToTest, double tolerance = GeometryBase.Tolerance)
        {
            bool isPointInHole = false;

            if (IsPointOnFillPlane(pointToTest, tolerance))                 // controllo che sia sul piano
            {
                if (!_fill.PointExists(pointToTest, tolerance))                    // controllo che il punto non sia uno spigolo 
                {
                    if (_fill.IsPointOnEdge(pointToTest, tolerance) == -1)         // controllo che non sia sul bordo
                    {
                        if (_fill.IsPointInside(pointToTest, tolerance))           // controllo che sia dentro il fill
                        {
                            if (_holes != null)                         // se non ha holes
                            {
								for (int i = 0; i < _holes.Length; i++)
                                {
									Polygon3d hole = _holes[i];
									if (hole.IsPointOnEdge(pointToTest, tolerance) > -1)
                                        return true;
                                    if (hole.PointExists(pointToTest, tolerance))
                                        return true;
                                    if (hole.IsPointInside(pointToTest, tolerance))
                                        isPointInHole = true;
                                }
                                if (isPointInHole == false)
                                    return true;
                            }
                            if (_childs != null)                        // se non ha figli
                            {
								for (int i = 0; i < _childs.Length; i++)
                                {
									Shape _ = _childs[i];
									bool isPointInChild = _.IsPointInside(pointToTest, tolerance);

                                    if (isPointInChild)
                                        return true;
                                }
                            }

                            if (isPointInHole == false)
                                return true;
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Tell if the point is on the plane of the fill. The plane is given by the Newell normal of the fill (no copies of the polygon);
        /// only for a degenerate fill it is built on the first three points, without duplicated and aligned points
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="tolerance">The tolerance on the distance from the plane</param>
        /// <returns>True if the point is on the plane</returns>
        private bool IsPointOnFillPlane(Point3d point, double tolerance)
        {
            if (_fill.TryGetUnitNormal(tolerance, out Vector3d normal))
            {
                Point3d origin = _fill[0];
                double distance = normal.X * (point.X - origin.X) + normal.Y * (point.Y - origin.Y) + normal.Z * (point.Z - origin.Z);
                return Math.Abs(distance) < tolerance;
            }

            Polygon3d polygon = new Polygon3d();
            for (int i = 0; i < _fill.Count; i++)
                polygon.AddWithoutChecks(_fill[i]);

            polygon.RemoveDuplicatedPoints(tolerance);
            polygon.RemoveAlignedPoints(tolerance);

            Plane plane = new Plane(polygon[0], polygon[1], polygon[2], tolerance);
            return plane.IsPointOnPlane(point, tolerance);
        }

        /// <summary>
        /// Check if the segment is inside the shape: both the ends inside (see <see cref="IsPointInside(Point3d, double)"/>) and no crossing
        /// of the edges of fill and holes other than at the ends. With holes an even number of crossings is accepted; a segment inside a child is inside.
        /// The check is only on the crossings: a segment joining two points of the border that passes outside without crossing edges is considered inside
        /// </summary>
        /// <param name="lineToTest">Line to test</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>True if the line is inside</returns>
        public bool IsLineInside(Line3d lineToTest, double tolerance = GeometryBase.Tolerance)
        {
            int intersectionCount = 0;

            if (IsPointInside(lineToTest.Start, tolerance))                            // controllo che start sia interno (altrimenti false)
            {
                if (IsPointInside(lineToTest.End, tolerance))                          // controllo che end sia interno (altrimenti false)
                {
					Line3d[] lines = _fill.Explode();
					for (int i = 0; i < lines.Length; i++)
                    {
						if (lines[i].Equals(lineToTest))
                            return true;

                        if (lines[i].IsPointOnLine(lineToTest.Start, tolerance))           // se start è sul bordo
                        {                                                   // intersectionCount--
                            intersectionCount--;
                        }

                        if (lines[i].IsPointOnLine(lineToTest.End, tolerance))             // se end è sul bordo
                        {                                                   // intersectionCount--
                            intersectionCount--;
                        }

                        if (lines[i].GetIntersection(lineToTest, out Point3d _, tolerance))      // per ogni intersezione
                        {
                            intersectionCount++;                                            // intersectionCount++
                        }
                    }
                    if (_holes != null)                                             // se non ha holes
                    {
						for (int i = 0; i < _holes.Length; i++)
                        {
							Polygon3d hole = _holes[i];
							Line3d[] holeLines = hole.Explode();

							for (int i1 = 0; i1 < holeLines.Length; i1++)
                            {
								if (holeLines[i1].Equals(lineToTest))
                                    return true;

                                if (holeLines[i1].IsPointOnLine(lineToTest.Start, tolerance))           // se start è sul bordo
                                {                                                   // intersectionCount--
                                    intersectionCount--;
                                }

                                if (holeLines[i1].IsPointOnLine(lineToTest.End, tolerance))             // se end è sul bordo
                                {                                                   // intersectionCount--
                                    intersectionCount--;
                                }

                                if (holeLines[i1].GetIntersection(lineToTest, out Point3d _, tolerance))      // per ogni intersezione
                                {
                                    intersectionCount++;                                            // intersectionCount++
                                }
                            }
                        }
                        if (Math.Abs(intersectionCount) % 2 == 0)
                            return true;
                    }

                    if (_childs != null)                        // se non ha figli
                    {
						for (int i = 0; i < _childs.Length; i++)
                        {
							Shape s = _childs[i];
							bool isLineInChild = s.IsLineInside(lineToTest, tolerance);

                            if (isLineInChild)
                                return true;
                        }
                    }
                }                                             // intersectionCount sono le intersezioni che ha la retta che non siano
                else                                          // quelle degli estremi con i vertici o con i lati
                    return false;                             // se intersectionTotal è pari, vuol dire che è interno oppure interseca
            }                                                 // i lati o vertici del poligono gli gli estremi
            else                                              // se è dispari, vuol dire che la retta esce dal bordo
                return false;                                 // 

            if (Math.Abs(intersectionCount) == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if the polygon is inside the shape: all its edges are inside (see <see cref="IsLineInside(Line3d, double)"/>)
        /// </summary>
        /// <param name="polygonToTest">Polygon to test</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>True if the polygon is inside</returns>
        public bool IsPolygonInside(Polygon3d polygonToTest, double tolerance = GeometryBase.Tolerance)
        {
			Line3d[] lines = polygonToTest.Explode();
			for (int i = 0; i < lines.Length; i++)
            {
				Line3d line = lines[i];
				if (!IsLineInside(line, tolerance))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the fills of the two shapes are equal also with a shifted order of the vertices (see <see cref="Polygon3d.EqualsShifted"/>).
        /// The holes and the children are not compared
        /// </summary>
        /// <param name="other">The shape to test</param>
        /// <returns>True if the fills are equal</returns>
        public bool EqualsShifted(Shape other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;


            bool condition = false;
            if (other._fill != null && _fill != null)
            {
                condition = other._fill.EqualsShifted(_fill);

                if (!condition)
                    return false;
            }
            else if (other._fill is null || _fill is null)
            {
                return false; // se entrambi nulli torna falso (non si possono confrontare due shape con bordi nulli), se uno dei due nullo torna falso
            }

            if (other._holes is null && _holes is null)
            {
                condition = true;
            }
            if (other._childs is null && _childs is null)
            {
                condition = true;
            }

            return condition;
        }

        /// <summary>
        /// Scale the shape respect to the origin of the axes
        /// </summary>
        /// <param name="factor">Scale factor</param>
        /// <param name="tol">The tolerance, multiplied by the factor for the new shape</param>
        /// <returns>A new shape scaled</returns>
        public virtual Shape Scale(double factor, double tol = GeometryBase.Tolerance)
        {
            return Scale(factor, factor, factor, tol);
        }

		/// <summary>
		/// Scale the shape respect to the origin of the axes
		/// </summary>
		/// <param name="factor">Scale factor</param>
		/// <returns>A new shape scaled</returns>
		public virtual Shape Scale(double factor)
		{
			return Scale(factor, factor, factor);
		}

		/// <summary>
		/// Scale the shape respect to the origin of the axes, with a factor for each axis
		/// </summary>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <returns>A new shape scaled (fill, holes and children)</returns>
		public virtual Shape Scale(double factorX, double factorY, double factorZ)
        {
			Polygon3d fillScaled = _fill.Scale(factorX, factorY, factorZ);

            Polygon3d[] scaledHole = null;
            Shape[] scaledChild = null;

            if (_holes != null)
            {
                scaledHole = new Polygon3d[_holes.Count()];

                for (int i = 0; i < _holes.Length; i++)
                {
                    scaledHole[i] = _holes[i].Scale(factorX, factorY, factorZ);
                }
            }

            if (_childs != null)
            {
                scaledChild = new Shape[_childs.Count()];

                for (int i = 0; i < _childs.Length; i++)
                {
					scaledChild[i] = _childs[i].Scale(factorX, factorY, factorZ);
                }
            }

            if (_childs == null && _holes == null)
                return new Shape(fillScaled, null, null);

            else if (_childs == null && _holes != null)
                return new Shape(fillScaled, scaledHole.ToArray(), null);

            else if (_childs != null && _holes == null)
                return new Shape(fillScaled, null, scaledChild.ToArray());

            else
                return new Shape(fillScaled, scaledHole.ToArray(), scaledChild.ToArray());
        }

		/// <summary>
		/// Scale the shape respect to the origin of the axes, with a factor for each axis
		/// </summary>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <param name="tol">The tolerance, multiplied by the largest factor for the new shape</param>
		/// <returns>A new shape scaled (fill, holes and children)</returns>
		public virtual Shape Scale(double factorX, double factorY, double factorZ, double tol = GeometryBase.Tolerance)
		{
			double tolerance = tol * Math.Max(factorX, Math.Max(factorY, factorZ));

			Polygon3d fillScaled = _fill.Scale(factorX, factorY, factorZ);

			Polygon3d[] scaledHole = null;
			Shape[] scaledChild = null;

			if (_holes != null)
			{
				scaledHole = new Polygon3d[_holes.Count()];

				for (int i = 0; i < _holes.Length; i++)
				{
					scaledHole[i] = _holes[i].Scale(factorX, factorY, factorZ);
				}
			}

			if (_childs != null)
			{
				scaledChild = new Shape[_childs.Count()];

				for (int i = 0; i < _childs.Length; i++)
				{
					scaledChild[i] = _childs[i].Scale(factorX, factorY, factorZ);
				}
			}

			if (_childs == null && _holes == null)
				return new Shape(fillScaled, null, null, tolerance);

			else if (_childs == null && _holes != null)
				return new Shape(fillScaled, scaledHole.ToArray(), null, tolerance);

			else if (_childs != null && _holes == null)
				return new Shape(fillScaled, null, scaledChild.ToArray(), tolerance);

			else
				return new Shape(fillScaled, scaledHole.ToArray(), scaledChild.ToArray(), tolerance);
		}

		/// <summary>
		/// Scale the shape respect to a point
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factor">Scale factor</param>
		/// <param name="tol">The tolerance, multiplied by the factor for the new shape</param>
		/// <returns>A new shape scaled</returns>
		public virtual Shape Scale(Point3d center, double factor, double tol = GeometryBase.Tolerance)
		{
            return Scale(center, factor, factor, factor, tol);
		}

		/// <summary>
		/// Scale the shape respect to a point
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factor">Scale factor</param>
		/// <returns>A new shape scaled</returns>
		public virtual Shape Scale(Point3d center, double factor)
		{
			return Scale(center, factor, factor, factor);
		}

		/// <summary>
		/// Scale the shape respect to a point, with a factor for each axis
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <param name="tol">The tolerance, multiplied by the largest factor for the new shape</param>
		/// <returns>A new shape scaled (fill, holes and children)</returns>
		public virtual Shape Scale(Point3d center, double factorX, double factorY, double factorZ, double tol = GeometryBase.Tolerance)
		{
			double tolerance = tol * Math.Max(factorX, Math.Max(factorY, factorZ));

			Polygon3d fillScaled = _fill.Scale(center, factorX, factorY, factorZ);

			Polygon3d[] scaledHole = null;
			Shape[] scaledChild = null;

			if (_holes != null)
			{
				scaledHole = new Polygon3d[_holes.Count()];

				for (int i = 0; i < _holes.Length; i++)
				{
					scaledHole[i] = _holes[i].Scale(center, factorX, factorY, factorZ);
				}
			}

			if (_childs != null)
			{
				scaledChild = new Shape[_childs.Count()];

				for (int i = 0; i < _childs.Length; i++)
				{
					scaledChild[i] = _childs[i].Scale(center, factorX, factorY, factorZ);
				}
			}

			if (_childs == null && _holes == null)
				return new Shape(fillScaled, null, null);

			else if (_childs == null && _holes != null)
				return new Shape(fillScaled, scaledHole.ToArray(), null, tolerance);

			else if (_childs != null && _holes == null)
				return new Shape(fillScaled, null, scaledChild.ToArray(), tolerance);

			else
				return new Shape(fillScaled, scaledHole.ToArray(), scaledChild.ToArray(), tolerance);
		}

		/// <summary>
		/// Scale the shape respect to a point, with a factor for each axis
		/// </summary>
		/// <param name="center">The fixed point of the scaling</param>
		/// <param name="factorX">The scale factor along X</param>
		/// <param name="factorY">The scale factor along Y</param>
		/// <param name="factorZ">The scale factor along Z</param>
		/// <returns>A new shape scaled (fill, holes and children)</returns>
		public virtual Shape Scale(Point3d center, double factorX, double factorY, double factorZ)
		{
			Polygon3d fillScaled = _fill.Scale(center, factorX, factorY, factorZ);

			Polygon3d[] scaledHole = null;
			Shape[] scaledChild = null;

			if (_holes != null)
			{
				scaledHole = new Polygon3d[_holes.Count()];

				for (int i = 0; i < _holes.Length; i++)
				{
					scaledHole[i] = _holes[i].Scale(center, factorX, factorY, factorZ);
				}
			}

			if (_childs != null)
			{
				scaledChild = new Shape[_childs.Count()];

				for (int i = 0; i < _childs.Length; i++)
				{
					scaledChild[i] = _childs[i].Scale(center, factorX, factorY, factorZ);
				}
			}

			if (_childs == null && _holes == null)
				return new Shape(fillScaled, null, null);

			else if (_childs == null && _holes != null)
				return new Shape(fillScaled, scaledHole.ToArray(), null);

			else if (_childs != null && _holes == null)
				return new Shape(fillScaled, null, scaledChild.ToArray());

			else
				return new Shape(fillScaled, scaledHole.ToArray(), scaledChild.ToArray());
		}

		/// <summary>
		/// Add a hole to the shape (the instance is kept), if an equal hole is not already present. The orientation and the position are not checked
		/// </summary>
		/// <param name="hole">The hole to add</param>
		public virtual void AddHole(Polygon3d hole)
        {
            List<Polygon3d> p = new List<Polygon3d>();
            if (HasHoles)
            {
                p.AddRange(_holes);				
			}

            if (!p.Contains(hole))
            {
                p.Add(hole);
                _holes = p.ToArray();
            }
        }

        /// <summary>
        /// Reverse the normal of the shape IN PLACE: the vertices order of fill, holes and childs is reversed
        /// </summary>
        /// <returns>This same shape (not a copy), to allow chaining. To keep the original use <c>new Shape(shape).Reverse()</c></returns>
        public Shape Reverse()
        {
            _fill.Reverse();

            if (_holes != null)
                for (int u = 0; u < _holes.Length; u++)
                    _holes[u].Reverse();

            if (_childs != null)
                for (int u = 0; u < _childs.Length; u++)
                    _childs[u].Reverse();

            return this;
        }

        /// <summary>
        /// The shape in its local coordinate system. Each polygon is moved to its own coordinate system (see <see cref="Polygon3d.GetPolygon2d"/>):
        /// the holes and the children are right only if their systems are the same of the fill
        /// </summary>
        /// <param name="tolerance">The tolerance of the coordinate systems</param>
        /// <returns>The 2d shape</returns>
        internal Shape2d GetShape2d(double tolerance = GeometryBase.Tolerance)
        {
            return new Shape2d(_fill.GetPolygon2d(tolerance),
                _holes?.Select(i => i.GetPolygon2d(tolerance)).ToArray(),
                _childs?.Select(i => i.GetShape2d(tolerance)).ToArray());
        }

        #endregion

        #region Operator ovverride 

        /// <summary>
        /// Serializes the shape: the <see cref="BaseObject.Guid"/>, the fill, the holes and the children
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fill", _fill, typeof(Polygon3d));
            info.AddValue("HolesCount", _holes != null ? _holes.Length : 0);
            if (_holes != null)
            {
                for (int i = 0; i < _holes.Length; i++)
                {
                    info.AddValue($"Hole{i}", _holes[i], typeof(Polygon3d));
                }
            }
            info.AddValue("ChildsCount", _childs != null ? _childs.Length : 0);
            if (_childs != null)
            {
                for (int i = 0; i < _childs.Length; i++)
                {
                    info.AddValue($"Child{i}", _childs[i], typeof(Shape));
                }
            }
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Shape)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal shape</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj is Shape other)            
               return Equals(other);            
            else            
                return false;    
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Shape)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal shape</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Shape shape)
                return Equals(shape);

            return false;
        }

        /// <summary>
        /// Equality of the fills (same vertices in the same order) and of the holes and of the children in any order
        /// </summary>
        /// <param name="other">The shape to compare</param>
        /// <returns>True if the shapes are equal</returns>
        public bool Equals(Shape other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;


            bool condition = false;
            if (other._fill != null && _fill != null)
            {
                condition = other._fill.Equals(_fill);

                if (!condition)
                    return false;
            }
            else if (other._fill is null || _fill is null)
            {
                return false; // se entrambi nulli torna falso (non si possono confrontare due shape con bordi nulli), se uno dei due nullo torna falso
            }


            if (other._holes != null && _holes != null)
            {
                condition = other._holes.ScrambledEquals(_holes);
                if (!condition)
                    return false;
            }
            else if (other._holes is null && _holes is null)
            {
                condition = true;
            }
            else
            {
                return false;   // entrambi diversi da null, entrambi non nulli
            }

            if (other._childs != null && _childs != null)
            {
                condition = other._childs.ScrambledEquals(_childs);
                if (!condition)
                    return false;
            }
            else if (other._childs is null && _childs is null)
            {
                condition = true;
            }
            else
            {
                return false;   // entrambi diversi da null, entrambi non nulli
            }

            return condition;
        }

        /// <summary>
        /// The hash code of the exact coordinates of fill, holes and children (independent of the order of holes and children)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Polygon3d>.Default.GetHashCode(_fill);

                if (_holes != null)
                {
                    foreach (var hole in _holes)
                    {
                        // non moltiplicando l'hashcode precedente, funziona se la lista è scrambled (a + b) == ( b + a)
                        hashCode += 17 * EqualityComparer<Polygon3d>.Default.GetHashCode(hole);
                    }
                }

                if (_childs != null)
                {
                    foreach (var child in _childs)
                    {
                        // non moltiplicando l'hashcode precedente, funziona se la lista è scrambled (a+b) == (b+a)
                        hashCode += 17 * EqualityComparer<Shape>.Default.GetHashCode(child);
                    }
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(Shape)"/>); two null shapes are equal
        /// </summary>
        /// <param name="obj1">The first shape</param>
        /// <param name="obj2">The second shape</param>
        /// <returns>True if the shapes are equal</returns>
        public static bool operator ==(Shape obj1, Shape obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Shape)"/>)
        /// </summary>
        /// <param name="obj1">The first shape</param>
        /// <param name="obj2">The second shape</param>
        /// <returns>True if the shapes are different</returns>
        public static bool operator !=(Shape obj1, Shape obj2)
        {
            return !(obj1 == obj2);
        }

		#endregion

		#region CLIPPER

		/// <summary>
		/// Compute the boolean union between the array of shapes <paramref name="a"/> and the array of shapes <paramref name="b"/>. The shapes must
		/// lie on the plane of the first shape of <paramref name="a"/>: the operation is done in its local coordinate system (Clipper, even-odd rule)
		/// </summary>
		/// <param name="a">The first array of shapes (not empty)</param>
		/// <param name="b">The second array of shapes</param>
		/// <param name="outShapeGlobal">The shapes of the result (outer polygons with their holes; the islands inside the holes are separate
		/// shapes; the children of the inputs are ignored): empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded, false if Clipper throws a <see cref="ClipperException"/></returns>
		public static bool Union(Shape[] a, Shape[] b, out Shape[] outShapeGlobal, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                Shape[] newA = new Shape[a.Count()];
                Shape[] newB = new Shape[b.Count()];

                CoordinateSystem newCoord = a[0].GetCoordinateSystem(tolerance);

                for (int i = 0; i < a.Count(); i++)
                    newA[i] = newCoord.ToLocal(a[i]);

                for (int i = 0; i < b.Count(); i++)
                    newB[i] = newCoord.ToLocal(b[i]);

                Shape[] outShapeLocal = Boolean(newA, newB, ClipType.ctUnion) ?? new Shape[0]; // null when the result is empty
                outShapeGlobal = new Shape[outShapeLocal.Count()];

                for (int i = 0; i < outShapeLocal.Count(); i++)
                    outShapeGlobal[i] = outShapeLocal[i].ToGlobal(newCoord);

                return true;
            }
            catch (ClipperException)
            {
                outShapeGlobal = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean union between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="Union(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <param name="shapes">The shapes of the result: empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded</returns>
		public static bool Union(Shape a, Shape b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                return Union(new[] { a }, new[] { b }, out shapes, tolerance);
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean difference <paramref name="a"/> minus <paramref name="b"/>, in the local coordinate system of the first shape of <paramref name="a"/>
		/// (see <see cref="Union(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The shapes to subtract from (not empty)</param>
		/// <param name="b">The shapes to subtract</param>
		/// <param name="shapes">The shapes of the result; null if the result is empty or Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the result is not empty (unlike the other operations, an empty result gives false)</returns>
		public static bool Difference(Shape[] a, Shape[] b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                Shape[] newA = new Shape[a.Count()];
                Shape[] newB = new Shape[b.Count()];

                CoordinateSystem newCoord = a[0].GetCoordinateSystem(tolerance);

                for (int i = 0; i < a.Count(); i++)
                    newA[i] = newCoord.ToLocal(a[i]);

                for (int i = 0; i < b.Count(); i++)
                    newB[i] = newCoord.ToLocal(b[i]);

                Shape[] outShapeLocal = Boolean(newA, newB, ClipType.ctDifference);
                if (outShapeLocal != null)
                {
                    shapes = new Shape[outShapeLocal.Count()];

                    for (int i = 0; i < outShapeLocal.Count(); i++)
                        shapes[i] = outShapeLocal[i].ToGlobal(newCoord);

                    return true;
                }
                else
                {
                    shapes = null;
                    return false;
                }
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean difference <paramref name="a"/> minus <paramref name="b"/> (see <see cref="Difference(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The shape to subtract from</param>
		/// <param name="b">The shape to subtract</param>
		/// <param name="shapes">The shapes of the result; null if the result is empty or Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the result is not empty</returns>
		public static bool Difference(Shape a, Shape b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                return Difference(new[] { a }, new[] { b }, out shapes, tolerance);
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean intersection between the array of shapes <paramref name="a"/> and the array of shapes <paramref name="b"/>, in the
		/// local coordinate system of the first shape of <paramref name="a"/> (see <see cref="Union(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The first array of shapes (not empty)</param>
		/// <param name="b">The second array of shapes</param>
		/// <param name="shapes">The shapes of the result: empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded</returns>
		public static bool Intersection(Shape[] a, Shape[] b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                Shape[] newA = new Shape[a.Count()];
                Shape[] newB = new Shape[b.Count()];

                CoordinateSystem newCoord = a[0].GetCoordinateSystem(tolerance);

                for (int i = 0; i < a.Count(); i++)
                    newA[i] = newCoord.ToLocal(a[i]);

                for (int i = 0; i < b.Count(); i++)
                    newB[i] = newCoord.ToLocal(b[i]);

                Shape[] outShapeLocal = Boolean(newA, newB, ClipType.ctIntersection);
                if (outShapeLocal != null)
                {
                    shapes = new Shape[outShapeLocal.Count()];
                    for (int i = 0; i < outShapeLocal.Count(); i++)
                        shapes[i] = outShapeLocal[i].ToGlobal(newCoord);
                }
                else
                    shapes = new Shape[0];

                return true;
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean intersection between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="Intersection(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <param name="shapes">The shapes of the result: empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded</returns>
		public static bool Intersection(Shape a, Shape b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                return Intersection(new[] { a }, new[] { b }, out shapes, tolerance);
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean exclusive or (the areas inside only one of the groups) between the array of shapes <paramref name="a"/> and the array of
		/// shapes <paramref name="b"/>, in the local coordinate system of the first shape of <paramref name="a"/> (see <see cref="Union(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The first array of shapes (not empty)</param>
		/// <param name="b">The second array of shapes</param>
		/// <param name="shapes">The shapes of the result: empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded</returns>
		public static bool NotIntersection(Shape[] a, Shape[] b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                Shape[] newA = new Shape[a.Count()];
                Shape[] newB = new Shape[b.Count()];

                CoordinateSystem newCoord = a[0].GetCoordinateSystem(tolerance);

                for (int i = 0; i < a.Count(); i++)
                    newA[i] = newCoord.ToLocal(a[i]);

                for (int i = 0; i < b.Count(); i++)
                    newB[i] = newCoord.ToLocal(b[i]);

                Shape[] outShapeLocal = Boolean(newA, newB, ClipType.ctXor) ?? new Shape[0]; // null when the result is empty
                shapes = new Shape[outShapeLocal.Count()];

                for (int i = 0; i < outShapeLocal.Count(); i++)
                    shapes[i] = outShapeLocal[i].ToGlobal(newCoord);

                return true;
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

		/// <summary>
		/// Compute the boolean exclusive or between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="NotIntersection(Shape[], Shape[], out Shape[], double)"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <param name="shapes">The shapes of the result: empty if the result is empty, null if Clipper fails</param>
		/// <param name="tolerance">The tolerance of the coordinate system</param>
		/// <returns>True if the operation succeeded</returns>
		public static bool NotIntersection(Shape a, Shape b, out Shape[] shapes, double tolerance = GeometryBase.Tolerance)
        {
            try
            {
                return NotIntersection(new[] { a }, new[] { b }, out shapes, tolerance);
            }
            catch (ClipperException)
            {
                shapes = null;
                return false;
            }
        }

        /// <summary>
        /// Execute a boolean operation beetween shapes on the XY plane using the Clipper class (even-odd fill rule; the children are ignored)
        /// </summary>
        /// <param name="a">The subject shapes</param>
        /// <param name="b">The clip shapes</param>
        /// <param name="code">The operation</param>
        /// <param name="factor">Scale of the coordinates; not positive (default): automatic, see <see cref="ClipperScale"/>
        /// (before, always 1000 with the coordinates truncated)</param>
        /// <returns>The shapes of the result; null if the result is empty or Clipper returns false</returns>
        /// <exception cref="NotSupportedException">If the root of the result of Clipper has a contour</exception>
        protected static Shape[] Boolean(Shape[] a, Shape[] b, ClipType code, int factor = 0)
        {
            double scale = factor > 0 ? factor : ClipperScale.Factor(ClipperScale.MaxAbsCoordinate(a, b));

            Clipper clipper = new Clipper();
            foreach (Shape shape in a)
            {
                AddToPath(shape, scale, clipper, PolyType.ptSubject);
            }
            foreach (Shape shape in b)
            {
                AddToPath(shape, scale, clipper, PolyType.ptClip);
            }

            PolyTree polyTree = new PolyTree();
            if (clipper.Execute(code, polyTree, PolyFillType.pftEvenOdd))
            {
                if (polyTree.Contour.Count > 0)
                {
                    throw new NotSupportedException();
                }
                if (polyTree.ChildCount > 0)
                {
                    List<Shape> result = new List<Shape>();
                    foreach (PolyNode node in polyTree.Childs)
                    {
                        AddShapesFromPolyNode(node, scale, result);
                    }
                    return result.ToArray();
                }
            }

            return null;
        }

        /// <summary>
        /// The shape of the outer polygon node and, as separate shapes, the islands inside its holes (before, they were lost)
        /// </summary>
        /// <param name="outer">The node of an outer polygon</param>
        /// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
        /// <param name="result">The list where the shapes are added</param>
        private static void AddShapesFromPolyNode(PolyNode outer, double factor, List<Shape> result)
        {
            result.Add(GetShapeFromPolyNode(outer, factor));
            for (int i = 0; i < outer.ChildCount; i++)
            {
                PolyNode hole = outer.Childs[i];
                for (int j = 0; j < hole.ChildCount; j++)
                    AddShapesFromPolyNode(hole.Childs[j], factor, result);
            }
        }

		/// <summary>
		/// Support function that add the shape polygons (fill and holes, not the children; Z is ignored) to the Clipper object for the boolean operations
		/// </summary>
		/// <param name="shape">The source shape</param>
		/// <param name="factor">The scale factor for double to integer conversion for the coordinates</param>
		/// <param name="clipper">The clipper object for the boolean operations</param>
		/// <param name="type">Tell if the shape polygon is a Subject or Clip polygon</param>
		protected static void AddToPath(Shape shape, double factor, Clipper clipper, PolyType type)
        {
            List<IntPoint> fill = Polygon3d.GetIntPointList(shape.Fill, factor);
            clipper.AddPath(fill, type, true);
            if (shape._holes != null)
            {
                foreach (Polygon3d polygon in shape._holes)
                {
                    List<IntPoint> hole = Polygon3d.GetIntPointList(polygon, factor);
                    clipper.AddPath(hole, type, true);
                }
            }
            // TODO: to implement the childs we need to distingue the holes polygons from the childs polygons
            /*if (shape._childs != null)
            {
                foreach (Shape2d child in shape._childs)
                {
                    AddToPath(child, factor, clipper, type);
                }
            }*/
        }

		/// <summary>
		/// Converts back a Clipper.PolyNode of an outer polygon to a shape on the plane z = 0: the contour is the fill, the contours of the
		/// children nodes are the holes
		/// </summary>
		/// <param name="polyNode">The PolyNode to convert</param>
		/// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
		/// <returns>The shape</returns>
		private static Shape GetShapeFromPolyNode(PolyNode polyNode, double factor)
        {
            Polygon2d contour = new Polygon2d();
			for (int i = 0; i < polyNode.Contour.Count; i++)
            {
				IntPoint p = polyNode.Contour[i];
				contour.Add(p.X / factor, p.Y / factor);
            }

            Polygon2d[] holes = null;
            if (polyNode.ChildCount > 0)
            {
                holes = new Polygon2d[polyNode.ChildCount];
                for (int i = 0; i < polyNode.ChildCount; i++)
                {
                    holes[i] = new Polygon2d();
					for (int j = 0; j < polyNode.Childs[i].Contour.Count; j++)
                    {
						IntPoint p = polyNode.Childs[i].Contour[j];
						holes[i].Add(p.X / factor, p.Y / factor);
                    }
                }
            }

            /*if (polyNode.ChildCount > 0) // how can we distingue from the polyNode.Childs of the hole from the polyNode.Childs of the childs ?
            {
                childs = new Shape2d[polyNode.ChildCount];
                for (int i = 0; i < polyNode.ChildCount; i++)
                {
                    childs[i] = GetShapeFromPolyNode(polyNode.Childs[i], factor);
                }
            }*/


            return new Shape(contour, holes, null);
        }

        #endregion
    }
}
