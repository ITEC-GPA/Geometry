using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GPC.Utilities.Extensions;

namespace GPC.Geometry
{
    /// <summary>
    /// A polygon on the XY plane: the ordered list of its vertices, closed (the last vertex is joined to the first one).
    /// Two polygons are equal if they have equal vertices in the same order, starting from the same vertex
    /// </summary>
    [Serializable]
    public sealed class Polygon2d : GeometryBase, IEnumerable<Point2d>, ISerializable, ICloneable, IEquatable<Polygon2d>
    {
        #region Variables

        /// <summary>
        /// The vertices (the array is replaced, not changed, when points are added or removed)
        /// </summary>
        private Point2d[] _points;

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
        public Point2d this[int index] => _points[index];

		/// <summary>
		/// The vertices (the array of the polygon, not a copy)
		/// </summary>
		public Point2d[] Points { get => _points; set => _points = value; }

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates an empty polygon
		/// </summary>
		public Polygon2d()
        {
            _points = new Point2d[0];
        }

        /// <summary>
        /// Creates a polygon from its vertices (the instances are kept, not copied)
        /// </summary>
        /// <param name="list">The vertices, in order</param>
        /// <exception cref="ArgumentNullException">If <paramref name="list"/> is null</exception>
        public Polygon2d(IEnumerable<Point2d> list)
        {
            _points = list.ToArray() ?? throw new ArgumentNullException("Point list can not be null");
        }

        /// <summary>
        /// Creates a polygon from the projections on the XY plane of points in the space (the Z coordinates are dropped)
        /// </summary>
        /// <param name="list">The vertices, in order</param>
        internal Polygon2d(IEnumerable<Point3d> list)
            : this()
        {
            foreach (Point2d p in list)
            {
                Add(p);
            }
        }

        /// <summary>
        /// Creates a copy of the polygon, with copies of the vertices (moving the copy does not move the original)
        /// </summary>
        /// <param name="polygon">The polygon to copy</param>
        public Polygon2d(Polygon2d polygon)
        {
            _points = new Point2d[polygon.Count];
            for (int i = 0; i < _points.Length; i++)
                _points[i] = new Point2d(polygon[i]);
        }

        /// <summary>
        /// Creates the projection of a polygon on the XY plane (the Z coordinates are dropped)
        /// </summary>
        /// <param name="polygon">The polygon in the space</param>
        public Polygon2d(Polygon3d polygon)
        {
            _points = new Point2d[polygon.Count];
            for (int i = 0; i < _points.Length; i++)
                _points[i] = polygon[i];
        }

        /// <summary>
        /// Creates a regular polygon inscribed in a circle: the first vertex is on the X axis through the center, the others follow counterclockwise
        /// </summary>
        /// <param name="diameter">The diameter of the circle (not the radius)</param>
        /// <param name="numberOfEdges">The number of edges (and of vertices)</param>
        /// <param name="origin">The center of the circle; null (default): the origin of the axes</param>
        /// <exception cref="ArgumentException">If <paramref name="numberOfEdges"/> is smaller than 2</exception>
        public Polygon2d(double diameter, int numberOfEdges = 32, Point2d origin = default(Point2d))
            : this()
        {
            if (numberOfEdges < 2)
                throw new ArgumentException($"{numberOfEdges} must be at least 3");

            double teta = 2.0 * Math.PI / numberOfEdges;
            double radius = diameter / 2.0;

            if (origin == default)
                origin = new Point2d();

            Point2d[] pts = new Point2d[numberOfEdges];
            
            for (int i = 0; i < numberOfEdges; i++)            
				pts[i] = new Point2d(radius * Math.Cos(teta * i) + origin.X, radius * Math.Sin(teta * i) + origin.Y);            

            _points = pts;
        }
                
        /// <summary>
        /// Deserialization constructor: reads the vertices (the <see cref="BaseObject.Guid"/> is not serialized)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Polygon2d(SerializationInfo info, StreamingContext context)
        {
            _points = new Point2d[0];

            for (int i = 0; i < info.GetInt16("Count"); i++)
            {
                Point2d point = (Point2d)info.GetValue($"Point{i}", typeof(Point2d));
                Add(point);
            }
        }

		#endregion

		#region Public Methods Specific

		/// <summary>
		/// Adds points at the end of the polygon (their projections on the XY plane)
		/// </summary>
		/// <param name="points">The points to add; null: nothing is added</param>
		internal void AddRange(Point3d[] points)
        {
            if (points != null)
            {
                Point2d[] pointBuffer = _points;
                _points = new Point2d[_points.Length + points.Length];

                for (int i = 0; i < pointBuffer.Length; i++)
                    _points[i] = pointBuffer[i];

                for (int i = 0; i < points.Length; i++)
                    _points[pointBuffer.Length + i] = points[i];
            }
        }

		/// <summary>
		/// Adds points at the end of the polygon (their projections on the XY plane)
		/// </summary>
		/// <param name="points">The points to add (not null)</param>
		internal void AddRange(IEnumerable<Point3d> points)
        {
            AddRange(points.ToArray());
        }

        /// <summary>
        /// Adds a vertex at the end of the polygon (the instance is kept, not copied)
        /// </summary>
        /// <param name="point">The point to add; null: nothing is added</param>
        public void Add(Point2d point)
        {
            if (point != null)
            {
                Point2d[] pointBuffer = _points;
                _points = new Point2d[_points.Length + 1];
                for (int i = 0; i < pointBuffer.Length; i++)
                    _points[i] = pointBuffer[i];
                _points[_points.Length - 1] = point;
            }
        }

		/// <summary>
		/// Adds a new vertex at the end of the polygon
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		public void Add(double x, double y)
        {
            Add(new Point2d(x, y));
        }

		/// <summary>
		/// Removes the vertex at index <paramref name="index"/>
		/// </summary>
		/// <param name="index">The index of the vertex, from 0 to <see cref="Count"/> - 1</param>
		/// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is out of range (the polygon loses its last vertex anyway)</exception>
		public void RemoveAt(int index)
        {
            Point2d[] pointsBuffer = _points;
            _points = new Point2d[_points.Length - 1];
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
		public Point2d GetNextPoint(int i)
        {
            return this[GetNextIndex(i)];
        }

        /// <summary>
        /// The edges of the polygon: the edge i goes from the vertex i to the next one (the last edge closes the polygon)
        /// </summary>
        /// <returns>The edges, as many as the vertices</returns>
        public Line2d[] Explode()
        {
			Line2d[] lines = new Line2d[_points.Length];
            for (int i = 0; i < _points.Length; i++)
            {
                lines[i] = (new Line2d(this[i], GetNextPoint(i)));
            }
            return lines;
        }

        /// <summary>
        /// Removes the consecutive vertices (also the last and the first one) closer than about 1.19 × <paramref name="toll"/>: of each group of
        /// close vertices only the first one is kept
        /// </summary>
        /// <param name="toll">The tolerance on the distance</param>
        public void RemoveDuplicatedPoints(double toll = GeometryBase.Tolerance)
        {
            if (_points.Length == 1)             
                return; 

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
                if (this[nextI].DistanceTo(this[i]) < Utilities.Maths.ErrorPropagation.DefaultProductTolerance(toll))    
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
        /// The signed area of the polygon (shoelace formula): positive if the vertices are counterclockwise
        /// </summary>
        /// <returns>The signed area; 0 if the polygon has less than 3 vertices</returns>
        public double GetSignedArea()
        {
            //TODO: testare con poligoni auto intersecanti
            if (Count < 3) 
                return 0;

            double result = 0;
            for (int i = 0; i < Count; i++)
            {
                Point2d item = this[i];
                Point2d nextItem = GetNextPoint(i);
                result += 0.5 * (item.X * nextItem.Y - nextItem.X * item.Y);
            }
            return result;
        }

        /// <summary>
        /// Tell if the vertices are counterclockwise: the normal given by the right hand rule is directed as the global Z axis
        /// </summary>
        /// <returns>True if the signed area is positive</returns>
        public bool IsRightHandOrdered()
        {
            return GetSignedArea() > 0;
        }

        /// <summary>
        /// Reverses the order of the vertices (in place)
        /// </summary>
        public void Reverse()
        {
            Array.Reverse(_points);
        }

        /// <summary>
        /// Remove aligned points keeping only the last 2 point of the segment.
        /// If all the points are aligned, it keeps the last two points, not necessary the extremes of the line
        /// </summary>
        /// <param name="tolerance">Tolleranza</param>
        public void RemoveAlignedPoints(double tolerance = GeometryBase.AngularTolerance)
        {
            List<int> pointToRemove = new List<int>();
            double tollerance = Math.Sqrt(Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance));

            for (int i = 0; i < _points.Length; i++)
            {
                if (_points.Length > 2)
                {
                    Vector2d v1 = new Vector2d(GetPreviousPoint(i) - _points[i]);
                    Vector2d v2 = new Vector2d(GetNextPoint(i) - _points[i]);

                    double angle = v1.AngleTo(v2);                    

                    // Il metodo costruisce l'angolo tra il punto da testare, il punto precedente e il punto successivo. 
                    // Se questo angolo è 180° => pi greco, allora i punti sono allineati.

                    if (Math.Abs(Math.Abs(angle) - Math.PI) < tollerance)                       
                    {
                        if (pointToRemove.Count < _points.Length - 2)
                            pointToRemove.Add(i);
                    }

                    // Se l'angolo è 0, cerca il punto successivo. Se trovo un nuovo punto allineato in cui i è nel mezzo, allora tolgo i, 
                    // se non trovo altri punti allora vuol dire che è un estremo o che non ci sono punti allineati

                    if (Math.Abs(Math.Abs(angle)) < tollerance)
                    {
                        for (int k = 1; k < _points.Length - 1 - i; k++)
                        {
                            Vector2d v3 = new Vector2d(GetPreviousPoint(i) - _points[i]);

                            Vector2d v4 = new Vector2d(GetNextPoint(k + i) - _points[i]);

                            double angle2 = v3.AngleTo(v4);

                            if (Math.Abs(Math.Abs(angle2) - Math.PI) < tollerance)
                            {
                                if (pointToRemove.Count < _points.Length - 2)
                                {
                                    pointToRemove.Add(i);
                                    i--;
                                }
                                break;
                            }

                            if (Math.Abs(Math.Abs(angle)) < tollerance)
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
            for (int i = pointToRemove.Count -1; i >= 0; i--)
            {
                RemoveAt(pointToRemove[i]);
            }
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
		public Point2d GetPreviousPoint(int i)
        {
            return this[GetPreviousIndex(i)];
        }
               
        /// <summary>
        /// Get the mirror polygon about the straight line defined by the equation ax + by + c = 0
        /// </summary>
        /// <param name="a">The 'a' parameter of the equation</param>
        /// <param name="b">The 'b' parameter of the equation</param>
        /// <param name="c">The 'c' parameter of the equation</param>
        /// <returns>A new polygon with the mirrored vertices in the same order (then with the opposite orientation)</returns>
        public Polygon2d Mirror(double a, double b, double c)
        {
            Polygon2d mirror = new Polygon2d();
            for (int i = 0; i < _points.Length; i++)
            {
                Point2d pt = _points[i].Mirror(a, b, c);
                mirror.Add(pt);
            }
            return mirror;
        }

        /// <summary>
        /// Move the polygon by the given increments (in place)
        /// </summary>
        /// <param name="dx">The X coordinate increment</param>
        /// <param name="dy">The Y coordinate increment</param>
        /// <param name="dz">Ignored: the polygon is on the XY plane</param>
        public override void Move(double dx, double dy, double dz = 0)
        {
            for(int i = 0; i < _points.Length; i++)
                _points[i].Move(dx, dy);
        }

        /// <summary>
        /// Move the polygon by a given vector (in place)
        /// </summary>
        /// <param name="vector">Displacement vector (its Z is ignored)</param>
        public override void Move(Vector3d vector)
        {
            Move(vector.X, vector.Y, 0.0);
        }

        /// <summary>
        /// Get the minimum distance from the given point to the border of the polygon (also for the points inside)
        /// </summary>
        /// <param name="p">The given point</param>
        /// <returns>The minimum distance from the edges; <see cref="double.MaxValue"/> if the polygon is empty</returns>
        public double DistanceTo(Point2d p)
        {
            double minDistance = double.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                Point2d start = this[i];
                Point2d end = GetNextPoint(i);
                Line2d line = new Line2d(start, end);

                double distance = line.DistanceTo(p);

                if (minDistance > distance)
                    minDistance = distance;
            }
            return minDistance;
        }

        /// <summary>
        /// The bounding box of the vertices
        /// </summary>
        /// <returns>The bounding box of the polygon</returns>
        public BoundingBox2d GetBoundingBox()
        {
            BoundingBox2d bbox = new BoundingBox2d();
            bbox.Update(_points);
            return bbox;
        }

        /// <summary>
        /// Creates a copy of the polygon, with copies of the vertices
        /// </summary>
        /// <returns>The copy</returns>
        public override object Clone()
        {
            return new Polygon2d(this);
        }
        
        /// <summary>
        /// Tell if the point to test is inside the polygon (crossing number with the half-open rule on the edges)
        /// </summary>
        /// <param name="pointToTest">Point to test</param>
        /// <param name="tolerance">The tolerance on the distance from the border</param>
        /// <returns>True if the point is inside the polygon or on its border (vertices included) within <paramref name="tolerance"/></returns>
        public bool IsPointInside(Point2d pointToTest, double tolerance = GeometryBase.Tolerance)
        {
            // Documentation: http://geomalgorithms.com/a03-_inclusion.html
            // Crossing Number: a horizontal ray starting from the point to test is intersected with the edges of the polygon.
            // If the number of crossings is odd the point is inside, if it is even the point is outside.
            // The "half-open" rule on the edges (upward edges include the start point and exclude the end point, downward edges the opposite)
            // handles the rays passing through the vertices without random rotations of the ray.

            int count = _points.Length;
            if (count == 0)
                return false;

            // Points on the border (vertices and edges) are inside
            double squareTolerance = tolerance * tolerance;
            for (int i = 0; i < count; i++)
            {
                if (new Line2d(_points[i], _points[(i + 1) % count]).SquareDistanceTo(pointToTest) < squareTolerance)
                    return true;
            }

            if (count < 3)
                return false;

            bool inside = false;
            double px = pointToTest.X;
            double py = pointToTest.Y;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Point2d a = _points[i];
                Point2d b = _points[j];

                if ((a.Y > py) != (b.Y > py))
                {
                    double xCross = a.X + (py - a.Y) * (b.X - a.X) / (b.Y - a.Y);
                    if (px < xCross)
                        inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Tell if a segment is inside the polygon: both the ends are inside (or on the border) and the segment does not cross the edges.
        /// The check is only on the crossings: a segment joining two points of the border that passes outside the polygon without crossing
        /// edges is considered inside
        /// </summary>
        /// <param name="line">The segment to test</param>
        /// <param name="tolerance">The tolerance on the distances</param>
        /// <returns>True if the segment is inside the polygon or on its border</returns>
        public bool IsLineInside(Line2d line, double tolerance = GeometryBase.Tolerance)
        {
            int intersectionCount = 0;

            if (IsPointInside(line.Start, tolerance))                                  // controllo che start sia interno (altrimenti false)
            {
                if (IsPointInside(line.End, tolerance))                                // controllo che end sia interno (altrimenti false)
                {
					Line2d[] lines = Explode();
					for (int i = 0; i < lines.Length; i++)                 
                    {
						if (lines[i].Equals(line))
                            return true;

                        if (lines[i].IsPointOnLine(line.Start, tolerance))                 // se start è sul bordo  
                            intersectionCount--;                                                // intersectionCount--

                        if (lines[i].IsPointOnLine(line.End, tolerance))                   // se end è sul bordo                                                                          
                            intersectionCount--;                        // intersectionCount--

                        if (lines[i].GetIntersection(line, out _, tolerance))      // per ogni intersezione                        
                            intersectionCount++;                                            // intersectionCount++                        
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
        /// Scale the polygon respect to the origin of the axes
        /// </summary>
        /// <param name="factor">Scale factor</param>
        /// <returns>A new polygon with the scaled vertices</returns>
        public Polygon2d Scale(double factor)
        {
            Point2d[] points = new Point2d[_points.Length]; 
            for (int i = 0; i < _points.Count(); i++)            
                points[i] = _points[i].Scale(factor);
            
            return new Polygon2d(points);
        }

        /// <summary>
        /// Create the triangles between each pair of consecutive vertices and the point center in input
        /// </summary>
        /// <param name="center">The point common at all triangles: it must be inside the polygon and not on an edge</param>
        /// <param name="tolerance">The tolerance of the checks on the position of <paramref name="center"/></param>
        /// <returns>The triangles, one for each edge</returns>
        /// <exception cref="Exception">If <paramref name="center"/> is outside the polygon or on an edge</exception>
        public Polygon2d[] Triangularization(Point2d center, double tolerance = GeometryBase.Tolerance)
        {
            Polygon3d poly = new Polygon3d(this);
            Polygon3d[] polygons = poly.Triangularization(center, tolerance);

            Polygon2d[] outPolys = new Polygon2d[polygons.Count()];
            for (int i = 0; i < polygons.Length; i++)
			{
                Polygon2d polygon2d = new Polygon2d();
                for(int j = 0; j < polygons[i].Count; j++)
                    polygon2d.Add(polygons[i][j]);

                outPolys[i] = polygon2d;
            }                

            return outPolys;
        }

        /// <summary>
        /// Create the triangles between each pair of consecutive vertices and the point center in input, without checking the position of
        /// <paramref name="center"/>
        /// </summary>
        /// <param name="center">The point common at all triangles</param>
        /// <returns>The triangles, one for each edge</returns>
        public Polygon2d[] TriangularizationWithoutChecks(Point2d center)
        {
            Polygon2d[] polygons = new Polygon2d[_points.Count()];

            for (int i = 0; i < _points.Count(); i++)            
                polygons[i] = new Polygon2d(new Point2d[3] { _points[i], _points[GetNextIndex(i)], center });            

            return polygons;
        }

        /// <summary>
        /// Get the the arithmetic mean position of all the points. This is not the centroid of the polygon
        /// </summary>
        /// <returns>The mean of the vertices (NaN coordinates if the polygon is empty)</returns>
        public Point2d GetCenter()
        {
            double Xsum = 0;
            double Ysum = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                Xsum += _points[i].X;
                Ysum += _points[i].Y;
            }

            return new Point2d(Xsum / Count, Ysum / Count);
        }

        /// <summary>
        /// The barycenter of a triangle: the mean of its three vertices (the intersection of the medians)
        /// </summary>
        /// <returns>The barycenter of the triangle</returns>
        /// <exception cref="Exception">If the polygon is not a triangle</exception>
        internal Point2d GetBarycenterOfTriangle()
        {
            if (_points.Length != 3)
                throw new Exception("Polygon must be a triangle");

            return new Point2d((_points[0].X + _points[1].X + _points[2].X) / 3.0,
                (_points[0].Y + _points[1].Y + _points[2].Y) / 3.0);
        }

        /// <summary>
        /// The centroid of the area of the polygon, from the triangles joining the edges with the mean of the vertices. The areas of the
        /// triangles have sign, so the centroid is right also for the concave polygons
        /// </summary>
        /// <returns>The centroid of the area; NaN coordinates if the area is zero</returns>
        public Point2d GetCentroid()
        {
            // https://bell0bytes.eu/centroid-convex/
            // Unfortunately computing the centroid of a polygon isn't just as easy as computing the barycenter of a triangle; in the case of a polygon simply averaging
            // over the coordinates of the vertices no longer results in the correct coordinates of the centroid - the only exception being regular polygons.
            // While the centroid of a polygon is indeed its center of mass, the mass of a polygon is uniformly distributed over its entire surface, not only at the vertices.
            // Note that for simple shapes, such as triangles, rectangles or the above mentioned regular polygons, the mass being evenly distributed over the surface
            // is equivalent to the mass being at the vertices only.
            // In the case of a convex polygon, it is easy enough to see, however, how triangulating the polygon will lead to a formula for its centroid.

            Point2d centre = GetCenter();
            Polygon2d[] listOfTriangles = TriangularizationWithoutChecks(centre);

            double areaTot = 0;
            double numeratorX = 0;
            double numeratorY = 0;

            for(int i = 0; i < listOfTriangles.Length; i++)
            {
                Point2d triangleBarycenter = listOfTriangles[i].GetBarycenterOfTriangle();

                double areaTotBuffer = listOfTriangles[i].GetSignedArea();

                numeratorX += triangleBarycenter.X * areaTotBuffer;
                numeratorY += triangleBarycenter.Y * areaTotBuffer;
                areaTot += areaTotBuffer;
            }

            return new Point2d(numeratorX / areaTot, numeratorY / areaTot);
        }

        #endregion

        #region IEnumerable<Point2d>

        /// <summary>
        /// Enumerates the vertices (of the array at the moment of the call: adding or removing points does not change the enumeration)
        /// </summary>
        /// <returns>The enumerator of the vertices</returns>
        public IEnumerator<Point2d> GetEnumerator()
        {
            return ((IEnumerable<Point2d>)_points).GetEnumerator(); // the array is replaced (not changed) when points are added or removed
        }

        /// <summary>
        /// Enumerates the vertices (see <see cref="GetEnumerator()"/>)
        /// </summary>
        /// <returns>The enumerator of the vertices</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<Point2d>        

        #region Boolean operations

        /// <summary>
        /// Execute the boolean union between <paramref name="a"/> and <paramref name="b"/> (with Clipper, even-odd fill rule)
        /// </summary>
        /// <param name="a">The first group of polygons (the polygons inside other ones are holes)</param>
        /// <param name="b">The second group of polygons (the polygons inside other ones are holes)</param>
        /// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
        public static Polygon2d[] Union(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctUnion);
        }

		/// <summary>
		/// Execute the boolean union between <paramref name="a"/> and <paramref name="b"/> (with Clipper)
		/// </summary>
		/// <param name="a">The first polygon</param>
		/// <param name="b">The second polygon</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] Union(Polygon2d a, Polygon2d b)
        {
            return Union(new[] { a }, new[] { b });
        }

		/// <summary>
		/// Execute the boolean difference <paramref name="a"/> minus <paramref name="b"/> (with Clipper, even-odd fill rule)
		/// </summary>
		/// <param name="a">The polygons to subtract from (the polygons inside other ones are holes)</param>
		/// <param name="b">The polygons to subtract (the polygons inside other ones are holes)</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] Difference(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctDifference);
        }

		/// <summary>
		/// Execute the boolean difference <paramref name="a"/> minus <paramref name="b"/> (with Clipper)
		/// </summary>
		/// <param name="a">The polygon to subtract from</param>
		/// <param name="b">The polygon to subtract</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] Difference(Polygon2d a, Polygon2d b)
        {
            return Difference(new[] { a }, new[] { b });
        }

		/// <summary>
		/// Execute the boolean intersection between <paramref name="a"/> and <paramref name="b"/> (with Clipper, even-odd fill rule)
		/// </summary>
		/// <param name="a">The first group of polygons (the polygons inside other ones are holes)</param>
		/// <param name="b">The second group of polygons (the polygons inside other ones are holes)</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] Intersection(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctIntersection);
        }

		/// <summary>
		/// Execute the boolean intersection between <paramref name="a"/> and <paramref name="b"/> (with Clipper)
		/// </summary>
		/// <param name="a">The first polygon</param>
		/// <param name="b">The second polygon</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] Intersection(Polygon2d a, Polygon2d b)
        {
            return Intersection(new[] { a }, new[] { b });
        }
		/// <summary>
		/// Execute the boolean exclusive or (the areas inside only one of the groups) between <paramref name="a"/> and <paramref name="b"/>
		/// (with Clipper, even-odd fill rule)
		/// </summary>
		/// <param name="a">The first group of polygons (the polygons inside other ones are holes)</param>
		/// <param name="b">The second group of polygons (the polygons inside other ones are holes)</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] NotIntersection(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctXor);
        }

		/// <summary>
		/// Execute the boolean exclusive or (the areas inside only one of the polygons) between <paramref name="a"/> and <paramref name="b"/>
		/// (with Clipper)
		/// </summary>
		/// <param name="a">The first polygon</param>
		/// <param name="b">The second polygon</param>
		/// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty</returns>
		public static Polygon2d[] NotIntersection(Polygon2d a, Polygon2d b)
        {
            return NotIntersection(new[] { a }, new[] { b });
        }

        /// <summary>
        /// Execute a boolean operation beetween polygons using the Clipper class (even-odd fill rule)
        /// </summary>
        /// <param name="a">The subject polygons</param>
        /// <param name="b">The clip polygons</param>
        /// <param name="code">The operation</param>
        /// <param name="factor">Scale of the coordinates; not positive (default): automatic, see <see cref="ClipperScale"/></param>
        /// <returns>The contours of the result, outer ones and holes in the same array; null if the result is empty or Clipper fails</returns>
        /// <exception cref="InvalidOperationException">If the root of the result of Clipper has a contour</exception>
        private static Polygon2d[] Boolean(Polygon2d[] a, Polygon2d[] b, ClipType code, int factor = 0)
        {
            double scale = factor > 0 ? factor : ClipperScale.Factor(Math.Max(ClipperScale.MaxAbsCoordinate(a), ClipperScale.MaxAbsCoordinate(b)));

            Clipper clipper = new Clipper();
			for (int i = 0; i < a.Length; i++)
            {
				Polygon2d polygon = a[i];
				AddToPath(polygon, scale, clipper, PolyType.ptSubject);
            }
			for (int i = 0; i < b.Length; i++)
            {
				Polygon2d polygon = b[i];
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
                    List<Polygon2d> bufferResult = new List<Polygon2d>();
					for (int i = 0; i < polyTree.Childs.Count; i++)
                    {
						PolyNode node = polyTree.Childs[i];
						List<Polygon2d> result = new List<Polygon2d>();
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
        /// <param name="factor">The scale factor for double to integer conversion for the coordinates</param>
        /// <param name="clipper">The clipper object for the boolean operations</param>
        /// <param name="type">Tell if the shape polygon is a Subject or Clip polygon</param>
        private static void AddToPath(Polygon2d polygon, double factor, Clipper clipper, PolyType type)
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
        internal static List<IntPoint> GetIntPointList(Polygon2d polygon, double factor)
        {
			List<IntPoint> result = new List<IntPoint>();

            for(int v = 0; v < polygon.Count; v++)
            {
                result.Add(new IntPoint(ClipperScale.Scale(polygon[v].X, factor), ClipperScale.Scale(polygon[v].Y, factor)));
            }
            return result;
        }

        /// <summary>
        /// Converts back the resulting clipper.PolyNode in Polygon2d array: the contour of the node and, recursively, the ones of its children
        /// </summary>
        /// <param name="polyNode">The PolyNode to convert</param>
        /// <param name="factor">The scale factor for converting the integer coordinates back to doubles</param>
        /// <param name="result">The list where the polygons are added</param>
        private static void GetPolygonsFromPolyNode(PolyNode polyNode, double factor, ref List<Polygon2d> result)
        {
            // The contour polygon
            Polygon2d contourPoly = new Polygon2d();
			for (int i = 0; i < polyNode.Contour.Count; i++)
            {
				IntPoint p = polyNode.Contour[i];
				contourPoly.Add(p.X / factor, p.Y / factor);
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
        /// Equality of the vertices, in the same order and starting from the same vertex (see <see cref="Point2d.Equals(Point2d)"/>)
        /// </summary>
        /// <param name="other">The polygon to compare</param>
        /// <returns>True if the polygons have equal vertices</returns>
        public bool Equals(Polygon2d other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._points.SequenceEqual(_points);
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Polygon2d)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal polygon</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Polygon2d);
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(Polygon2d)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal polygon</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Polygon2d poly)
                return Equals(poly);

            return false;
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
        /// Serializes the vertices (the <see cref="BaseObject.Guid"/> is not saved)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Count", _points.Length);
            for (int i = 0; i < _points.Length; i++)
            {
                info.AddValue($"Point{i}", _points[i], typeof(Point2d));
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(Polygon2d)"/>); two null polygons are equal
        /// </summary>
        /// <param name="obj1">The first polygon</param>
        /// <param name="obj2">The second polygon</param>
        /// <returns>True if the polygons are equal</returns>
        public static bool operator ==(Polygon2d obj1, Polygon2d obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Polygon2d)"/>)
        /// </summary>
        /// <param name="obj1">The first polygon</param>
        /// <param name="obj2">The second polygon</param>
        /// <returns>True if the polygons are different</returns>
        public static bool operator !=(Polygon2d obj1, Polygon2d obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
