using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	/// <summary>
	/// A shape on the XY plane (z = 0): fill, holes and children are polygons on the XY plane; the children are <see cref="Shape2d"/>
	/// </summary>
	[Serializable]
	public class Shape2d : Shape, ISerializable, ICloneable, IEquatable<Shape2d>
	{
		#region Properties

		/// <summary>
		/// A new 2d copy of the fill (changing it does not change the shape)
		/// </summary>
		public Polygon2d Fill2d => new Polygon2d(_fill);

		/// <summary>
		/// New 2d copies of the holes (changing them does not change the shape)
		/// </summary>
		/// <remarks>Null if the shape has no holes</remarks>
		public Polygon2d[] Holes2d => _holes?.Select(i => new Polygon2d(i)).ToArray();

		/// <summary>
		/// The children, as <see cref="Shape2d"/> (the instances of the shape)
		/// </summary>
		/// <remarks>Null if the shape has no childs</remarks>
		/// <exception cref="InvalidCastException">If a child is not a <see cref="Shape2d"/></exception>
		public Shape2d[] Childs2d => _childs?.Cast<Shape2d>().ToArray();

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a copy of the shape, checking the planarity of the fill with <paramref name="tolerance"/>
		/// </summary>
		/// <param name="shape2d">The shape to copy</param>
		/// <param name="tolerance">The tolerance of the planarity check of the fill</param>
		public Shape2d(Shape2d shape2d, double tolerance = GeometryBase.Tolerance)
			: base(shape2d, tolerance)
		{
		}

		/// <summary>
		/// Create a new shape from 2d polygons for fill and holes (copied) and 2d shapes for childs (the instances are kept).
		/// The holes and the children not oriented as the fill are reversed (the children in place)
		/// </summary>
		/// <param name="fill">The outer polygon</param>
		/// <param name="holes">The holes; null: no holes</param>
		/// <param name="childs">The shapes inside the holes; null: no children</param>
		/// <param name="tolerance">The tolerance of the normals</param>
		public Shape2d(Polygon2d fill, Polygon2d[] holes = null, Shape2d[] childs = null, double tolerance = GeometryBase.Tolerance)
			: base(fill, holes, childs, tolerance)
		{
		}

		/// <summary>
		/// Deserialization constructor (see <see cref="Shape"/>)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected Shape2d(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		/// <summary>
		/// Get the mirror shape about the straight line defined by the equation ax + by + c = 0
		/// </summary>
		/// <param name="a">The 'a' parameter of the equation</param>
		/// <param name="b">The 'b' parameter of the equation</param>
		/// <param name="c">The 'c' parameter of the equation</param>
		/// <param name="tolerance">The tolerance of the coordinate systems</param>
		/// <returns>A new 2d shape with the mirrored fill, holes and children</returns>
		public Shape2d Mirror2d(double a, double b, double c, double tolerance = GeometryBase.Tolerance)
		{
			Polygon2d fill = Fill2d.Mirror(a, b, c);
			Polygon2d[] holes = null;
			if (_holes != null)
			{
				holes = new Polygon2d[_holes.Length];
				for (int i = 0; i < _holes.Length; i++)
					holes[i] = _holes[i].GetPolygon2d(tolerance).Mirror(a, b, c);
			}

			Shape2d[] childs = null;
			if (_childs != null)
			{
				childs = new Shape2d[_childs.Length];
				for (int i = 0; i < _childs.Length; i++)
					childs[i] = _childs[i].GetShape2d(tolerance).Mirror(a, b, c, tolerance).GetShape2d(tolerance);
			}

			return new Shape2d(fill, holes, childs);
		}

		/// <summary>
		/// The vertices of the fill, of the holes and, recursively, of the children, as 2d points
		/// </summary>
		/// <returns>New 2d points</returns>
		public Point2d[] GetPoints2d()
		{
			List<Point2d> points = new List<Point2d>();

			for (int i = 0; i < Fill.Count; i++)
				points.Add(Fill[i]);

			if (HasHoles)
				for (int i = 0; i < Holes2d.Length; i++)
					for (int j = 0; j < Holes2d[i].Count; j++)
						points.Add(Holes2d[i][j]);

			if (HasChilds)
				for (int i = 0; i < Childs2d.Length; i++)
					points.AddRange(Childs2d[i].GetPoints2d());

			return points.ToArray();
		}

		/// <summary>
		/// Get the mirror shape about the straight line defined by the equation ax + by + c = 0
		/// </summary>
		/// <param name="a">The 'a' parameter of the equation</param>
		/// <param name="b">The 'b' parameter of the equation</param>
		/// <param name="c">The 'c' parameter of the equation</param>
		/// <param name="tolerance">The tolerance of the coordinate systems</param>
		/// <returns>A new shape (a <see cref="Shape"/>, see <see cref="Mirror2d"/> for a <see cref="Shape2d"/>) with the mirrored fill, holes and children</returns>
		public override Shape Mirror(double a, double b, double c, double tolerance = GeometryBase.Tolerance)
		{
			Polygon2d fill = Fill2d.Mirror(a, b, c);
			Polygon2d[] holes = null;
			if (_holes != null)
			{
				holes = new Polygon2d[_holes.Length];
				for (int i = 0; i < _holes.Length; i++)
					holes[i] = _holes[i].GetPolygon2d(tolerance).Mirror(a, b, c);
			}

			Shape2d[] childs = null;
			if (_childs != null)
			{
				childs = new Shape2d[_childs.Length];
				for (int i = 0; i < _childs.Length; i++)
					childs[i] = _childs[i].GetShape2d(tolerance).Mirror(a, b, c, tolerance).GetShape2d(tolerance);
			}

			return new Shape(fill, holes, childs);
		}

		/// <summary>
		/// The 2d bounding box of the fill and of the holes
		/// </summary>
		/// <returns>The bounding box of the shape in the global coordinates</returns>
		public BoundingBox2d Get2dBoundingBox()
		{
			BoundingBox2d bbox = new BoundingBox2d();
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
		/// The unit normal of the shape: the Z axis if the fill is counterclockwise, the opposite otherwise
		/// </summary>
		/// <param name="tolerance">Not used</param>
		/// <returns>A unitized vector normal to the shape</returns>
		public override Vector3d GetNormalVector(double tolerance = GeometryBase.Tolerance)
		{
			if (_fill.IsRightHandOrdered(tolerance))
			{
				return new Vector3d(0, 0, 1);
			}
			else
			{
				return new Vector3d(0, 0, -1);
			}
		}

		/// <summary>
		/// Move shape by an given increment (in place)
		/// </summary>
		/// <param name="dx">The X coordinate increment</param>
		/// <param name="dy">The Y coordinate increment</param>
		public void Move(double dx, double dy)
		{
			base.Move(dx, dy, 0);
		}

		/// <summary>
		/// Move shape by an given increment (in place)
		/// </summary>
		/// <param name="dx">The X coordinate increment</param>
		/// <param name="dy">The Y coordinate increment</param>
		/// <param name="dz">Ignored: the shape stays on the XY plane</param>
		public override void Move(double dx, double dy, double dz)
		{
			base.Move(dx, dy, 0);
		}

		/// <summary>
		/// Move shape by an given vector (in place)
		/// </summary>
		/// <param name="vector">The displacement (its Z is ignored)</param>
		public override void Move(Vector3d vector)
		{
			base.Move(vector.X, vector.Y, 0);
		}

		/// <summary>
		/// Move shape by an given vector (in place)
		/// </summary>
		/// <param name="vector">The displacement</param>
		public void Move(Vector2d vector)
		{
			base.Move(vector.X, vector.Y, 0);
		}

		/// <summary>
		/// Add a hole to the shape (a copy), if an equal hole is not already present. The orientation and the position are not checked
		/// </summary>
		/// <param name="hole">The hole to add</param>
		public void AddHole(Polygon2d hole)
		{
			base.AddHole(new Polygon3d(hole));
		}

		/// <summary>
		/// Add the projection of a polygon on the XY plane as a hole of the shape (see <see cref="AddHole(Polygon2d)"/>)
		/// </summary>
		/// <param name="hole">The hole to add</param>
		public override void AddHole(Polygon3d hole)
		{
			AddHole(new Polygon2d(hole));
		}

		/// <summary>
		/// Check if the point is on shape (see <see cref="Shape.IsPointInside(Point3d, double)"/>)
		/// </summary>
		/// <param name="pointToTest">The point to test</param>
		/// <param name="tolerance">The tolerance on the distances</param>
		/// <returns>True if the point is on shape</returns>
		public bool IsPointInside(Point2d pointToTest, double tolerance = GeometryBase.Tolerance)
		{
			return base.IsPointInside((Point3d)pointToTest, tolerance);
		}

		/// <summary>
		/// Check if the line is inside the shape
		/// </summary>
		/// <param name="lineToTest">Line to test</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>True if the line is inside</returns>
		public bool IsLineInside(Line2d lineToTest, double tolerance = GeometryBase.Tolerance)
		{
			return IsLineInside(new Line3d(lineToTest), tolerance);
		}

		/// <summary>
		/// Check if the polygon is inside the shape
		/// </summary>
		/// <param name="polygonToTest">Polygon to test</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns>True if the polygon is inside</returns>
		public bool IsPolygonInside(Polygon2d polygonToTest, double tolerance = GeometryBase.Tolerance)
		{
			return base.IsPolygonInside(new Polygon3d(polygonToTest), tolerance);
		}

		/// <summary>
		/// Scale the shape respect to the origin of the axes (Z is set to 0)
		/// </summary>
		/// <param name="factor">Scale factor</param>
		/// <param name="tol">The tolerance, multiplied by the factor for the new shape</param>
		/// <returns>A new shape scaled (a <see cref="Shape"/>)</returns>
		public override Shape Scale(double factor, double tol = GeometryBase.Tolerance)
		{
			return base.Scale(factor, factor, 0, tol);
		}

		#region Boolean operations

		/// <summary>
		/// Compute the boolean union between the array of shapes <paramref name="a"/> and the array of shapes <paramref name="b"/> (Clipper, even-odd rule;
		/// the children are ignored)
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The shapes of the result (outer polygons with their holes; the islands inside the holes are separate shapes); null if the result is empty</returns>
		public static Shape2d[] Union(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctUnion);
		}

		/// <summary>
		/// Compute the boolean union between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] Union(Shape2d a, Shape2d b)
		{
			return Union(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean difference <paramref name="a"/> minus <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The shapes to subtract from</param>
		/// <param name="b">The shapes to subtract</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] Difference(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctDifference);
		}

		/// <summary>
		/// Compute the boolean difference <paramref name="a"/> minus <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The shape to subtract from</param>
		/// <param name="b">The shape to subtract</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] Difference(Shape2d a, Shape2d b)
		{
			return Difference(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean intersection between the array of shapes <paramref name="a"/> and the array of shapes <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] Intersection(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctIntersection);
		}

		/// <summary>
		/// Compute the boolean intersection between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] Intersection(Shape2d a, Shape2d b)
		{
			return Intersection(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean exclusive or (the areas inside only one of the groups) between the array of shapes <paramref name="a"/> and the array
		/// of shapes <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] NotIntersection(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctXor);
		}

		/// <summary>
		/// Compute the boolean exclusive or between the shape <paramref name="a"/> and the shape <paramref name="b"/> (see <see cref="Union(Shape2d[], Shape2d[])"/>)
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The shapes of the result; null if the result is empty</returns>
		public static Shape2d[] NotIntersection(Shape2d a, Shape2d b)
		{
			return NotIntersection(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Execute a boolean operation beetween shapes using the Clipper class (even-odd fill rule; the children are ignored)
		/// </summary>
		/// <param name="a">The subject shapes</param>
		/// <param name="b">The clip shapes</param>
		/// <param name="code">The operation</param>
		/// <param name="factor">Scale of the coordinates; not positive (default): automatic, see <see cref="ClipperScale"/></param>
		/// <returns>The shapes of the result; null if the result is empty or Clipper returns false</returns>
		/// <exception cref="NotSupportedException">If the root of the result of Clipper has a contour</exception>
		private static Shape2d[] Boolean(Shape2d[] a, Shape2d[] b, ClipType code, int factor = 0)
		{
			double scale = factor > 0 ? factor : ClipperScale.Factor(ClipperScale.MaxAbsCoordinate(a, b));

			Clipper clipper = new Clipper();
			for (int i = 0; i < a.Length; i++)
			{
				AddToPath(a[i], scale, clipper, PolyType.ptSubject);
			}
			for (int i = 0; i < b.Length; i++)
			{
				AddToPath(b[i], scale, clipper, PolyType.ptClip);
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
					List<Shape2d> result = new List<Shape2d>();
					for (int i = 0; i < polyTree.Childs.Count; i++)
					{
						AddShapesFromPolyNode(polyTree.Childs[i], scale, result);
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
		private static void AddShapesFromPolyNode(PolyNode outer, double factor, List<Shape2d> result)
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
		/// Support function that add the shape polygons (fill and holes, not the children) to the Clipper object for the boolean operations
		/// </summary>
		/// <param name="shape">The source shape</param>
		/// <param name="factor">The scale factor for double to integer conversion for the coordinates</param>
		/// <param name="clipper">The clipper object for the boolean operations</param>
		/// <param name="type">Tell if the shape polygon is a Subject or Clip polygon</param>
		private static void AddToPath(Shape2d shape, double factor, Clipper clipper, PolyType type)
		{
			List<IntPoint> fill = Polygon3d.GetIntPointList(shape.Fill, factor);
			clipper.AddPath(fill, type, true);
			if (shape._holes != null)
			{
				for (int i = 0; i < shape._holes.Length; i++)
				{
					Polygon3d polygon = shape._holes[i];
					List<IntPoint> hole = Polygon3d.GetIntPointList(polygon, factor);
					clipper.AddPath(hole, type, true);
				}
			}

		}

		/// <summary>
		/// Converts back a Clipper.PolyNode of an outer polygon to a 2d shape: the contour is the fill, the contours of the children nodes are the holes
		/// </summary>
		/// <param name="polyNode">The PolyNode to convert</param>
		/// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
		/// <returns>The shape</returns>
		private static Shape2d GetShapeFromPolyNode(PolyNode polyNode, double factor)
		{
			Polygon2d contour = new Polygon2d();
			for (int i = 0; i < polyNode.Contour.Count; i++)
			{
				contour.Add(polyNode.Contour[i].X / factor, polyNode.Contour[i].Y / factor);
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

			Shape2d[] childs = null;

			return new Shape2d(contour, holes, childs);
		}

		#endregion

		#region Equals - hashcode - operators

		/// <summary>
		/// Equality with another object: it must be a <see cref="Shape2d"/> (see <see cref="Shape.Equals(Shape)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal 2d shape</returns>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return (obj is Shape2d shape2d) && base.Equals(shape2d);
		}

		/// <summary>
		/// Equality with another geometry: it must be a <see cref="Shape2d"/> (see <see cref="Shape.Equals(Shape)"/>)
		/// </summary>
		/// <param name="geometryBase">The geometry to compare</param>
		/// <returns>True if <paramref name="geometryBase"/> is an equal 2d shape</returns>
		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Shape2d shape)
				return Equals(shape);

			return false;
		}

		/// <summary>
		/// Creates a copy of the shape (a <see cref="Shape2d"/>)
		/// </summary>
		/// <returns>The copy</returns>
		public override object Clone()
		{
			return new Shape2d(this);
		}

		/// <summary>
		/// The hash code (see <see cref="Shape.GetHashCode"/>)
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Equality of fill, holes and children (see <see cref="Shape.Equals(Shape)"/>)
		/// </summary>
		/// <param name="other">The shape to compare</param>
		/// <returns>True if the shapes are equal</returns>
		public bool Equals(Shape2d other)
		{
			return base.Equals(other);
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(Shape2d)"/>); two null shapes are equal
		/// </summary>
		/// <param name="left">The first shape</param>
		/// <param name="right">The second shape</param>
		/// <returns>True if the shapes are equal</returns>
		public static bool operator ==(Shape2d left, Shape2d right)
		{
			if (ReferenceEquals(left, right))
				return true;
			if (left is null || right is null)
				return false;
			return left.Equals(right);
		}

		/// <summary>
		/// Inequality operator (see <see cref="Equals(Shape2d)"/>)
		/// </summary>
		/// <param name="left">The first shape</param>
		/// <param name="right">The second shape</param>
		/// <returns>True if the shapes are different</returns>
		public static bool operator !=(Shape2d left, Shape2d right)
		{
			return !(left == right);
		}

		#endregion
	}
}