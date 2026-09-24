using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
	[Serializable]
	public class Shape2d : Shape, ISerializable, ICloneable, IEquatable<Shape2d>
	{
		#region Properties

		public Polygon2d Fill2d => new Polygon2d(_fill);

		/// <remarks>Null if the shape has no holes</remarks>
		public Polygon2d[] Holes2d => _holes?.Select(i => new Polygon2d(i)).ToArray();

		/// <remarks>Null if the shape has no childs</remarks>
		public Shape2d[] Childs2d => _childs?.Cast<Shape2d>().ToArray();

		#endregion

		#region Constructors

		public Shape2d(Shape2d shape2d, double tolerance = GeometryBase.Tolerance)
			: base(shape2d, tolerance)
		{
		}

		public Shape2d(Polygon2d fill, Polygon2d[] holes = null, Shape2d[] childs = null, double tolerance = GeometryBase.Tolerance)
			: base(fill, holes, childs, tolerance)
		{
		}

		protected Shape2d(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		/// <summary>
		/// Get the mirror point about the plane defined by the equation ax + by + cz + d = 0
		/// </summary>
		/// <param name="a">The 'a' parameter of the equation</param>
		/// <param name="b">The 'b' parameter of the equation</param>
		/// <param name="c">The 'c' parameter of the equation</param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
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
		/// Get the mirror point about the plane defined by the equation ax + by + cz + d = 0
		/// </summary>
		/// <param name="a">The 'a' parameter of the equation</param>
		/// <param name="b">The 'b' parameter of the equation</param>
		/// <param name="c">The 'c' parameter of the equation</param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
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

		/// <returns>A unitized vector normal to the shape</returns>
		/// <exception cref="NotSupportedException">Thrown when Number of unique points not sufficient to create a normal vector</exception>
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
		/// Move shape by an given increment
		/// </summary>
		/// <param name="dx"></param>
		/// <param name="dy"></param>
		public void Move(double dx, double dy)
		{
			base.Move(dx, dy, 0);
		}

		/// <summary>
		/// Move shape by an given increment
		/// </summary>
		/// <param name="dx"></param>
		/// <param name="dy"></param>
		/// <param name="dz"></param>
		public override void Move(double dx, double dy, double dz)
		{
			base.Move(dx, dy, 0);
		}

		/// <summary>
		/// Move shape by an given vector
		/// </summary>
		/// <param name="vector"></param>
		public override void Move(Vector3d vector)
		{
			base.Move(vector.X, vector.Y, 0);
		}

		/// <summary>
		/// Move shape by an given vector
		/// </summary>
		/// <param name="vector"></param>
		public void Move(Vector2d vector)
		{
			base.Move(vector.X, vector.Y, 0);
		}

		/// <summary>
		/// Add a hole to the shape
		/// </summary>
		/// <param name="hole"></param>
		public void AddHole(Polygon2d hole)
		{
			base.AddHole(new Polygon3d(hole));
		}

		/// <summary>
		/// Add a hole to the shape
		/// </summary>
		/// <param name="hole"></param>
		public override void AddHole(Polygon3d hole)
		{
			AddHole(new Polygon2d(hole));
		}

		/// <summary>
		/// Check if the point is on shape
		/// </summary>
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
		/// Scale the shape respect to the origin 
		/// </summary>
		/// <param name="factor">Scale factor</param>
		/// <param name="tol">The tolerance</param>
		public override Shape Scale(double factor, double tol = GeometryBase.Tolerance)
		{
			return base.Scale(factor, factor, 0, tol);
		}

		#region Boolean operations

		/// <summary>
		/// Compute the boolean union between the array of shapes <paramref name="a"/> e the array of shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The array of shapes unite</returns>
		public static Shape2d[] Union(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctUnion);
		}

		/// <summary>
		/// Compute the boolean union between the shape <paramref name="a"/> e the shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The array of shapes unite</returns>
		public static Shape2d[] Union(Shape2d a, Shape2d b)
		{
			return Union(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean difference between the array of shapes <paramref name="a"/> e the array of shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] Difference(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctDifference);
		}

		/// <summary>
		/// Compute the boolean difference between the shape <paramref name="a"/> e the shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] Difference(Shape2d a, Shape2d b)
		{
			return Difference(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean intersection between the array of shapes <paramref name="a"/> e the array of shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <param name="shapes">The array of shapes intersection</param>
		/// <param name="tolerance"></param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] Intersection(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctIntersection);
		}

		/// <summary>
		/// Compute the boolean intersection between the shape <paramref name="a"/> e the shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second shape</param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] Intersection(Shape2d a, Shape2d b)
		{
			return Intersection(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Compute the boolean notIntersection between the array of shapes <paramref name="a"/> e the array of shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first array of shapes</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] NotIntersection(Shape2d[] a, Shape2d[] b)
		{
			return Boolean(a, b, ClipType.ctXor);
		}

		/// <summary>
		/// Compute the boolean not intersection between the shape <paramref name="a"/> e the shape <paramref name="b"/>
		/// </summary>
		/// <param name="a">The first shape</param>
		/// <param name="b">The second array of shapes</param>
		/// <returns>The array of shapes</returns>
		public static Shape2d[] NotIntersection(Shape2d a, Shape2d b)
		{
			return NotIntersection(new[] { a }, new[] { b });
		}

		/// <summary>
		/// Execute a boolean operation beetween shapes using the Clipper class
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="code"></param>
		/// <param name="factor"></param>
		/// <returns></returns>
		private static Shape2d[] Boolean(Shape2d[] a, Shape2d[] b, ClipType code, int factor = 1000)
		{
			Clipper clipper = new Clipper();
			for (int i = 0; i < a.Length; i++)
			{
				AddToPath(a[i], factor, clipper, PolyType.ptSubject);
			}
			for (int i = 0; i < b.Length; i++)
			{
				AddToPath(b[i], factor, clipper, PolyType.ptClip);
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
						result.Add(GetShapeFromPolyNode(polyTree.Childs[i], factor));
					}
					return result.ToArray();
				}
			}

			return null;
		}

		/// <summary>
		/// Support function that add the shape polygons to the Clipper object for the boolean operations
		/// </summary>
		/// <param name="shape">The source shape</param>
		/// <param name="factor">The scale factor for double to integer conversion for the coorinates</param>
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
		/// Converts back the Clipper.PolyNode to Shape2d array
		/// </summary>
		/// <param name="polyNode">The PolyNode to convert</param>
		/// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
		/// <returns></returns>
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

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return (obj is Shape2d shape2d) && base.Equals(shape2d);
		}

		public override bool Equals(GeometryBase geometryBase)
		{
			if (geometryBase is Shape2d shape)
				return Equals(shape);

			return false;
		}

		public override object Clone()
		{
			return new Shape2d(this);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public bool Equals(Shape2d other)
		{
			return base.Equals(other);
		}

		public static bool operator ==(Shape2d left, Shape2d right)
		{
			if (ReferenceEquals(left, right))
				return true;
			if (left is null || right is null)
				return false;
			return left.Equals(right);
		}

		public static bool operator !=(Shape2d left, Shape2d right)
		{
			return !(left == right);
		}

		#endregion
	}
}