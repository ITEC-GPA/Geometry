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
    /// Polygon 2d is a planar polygon on the (x,y) plane
    /// </summary>
    [Serializable]
    public sealed class Polygon2d : GeometryBase, IEnumerable<Point2d>, ISerializable, ICloneable, IEquatable<Polygon2d>
    {
        #region Variables

        private Point2d[] _points;

        #endregion

        #region Properties

        public int Count => _points.Length;

        public Point2d this[int index] => _points[index];

		public Point2d[] Points { get => _points; set => _points = value; }

		#endregion

		#region Public Constructors

		public Polygon2d()
        {
            _points = new Point2d[0];
        }

        public Polygon2d(IEnumerable<Point2d> list)
        {
            _points = list.ToArray() ?? throw new ArgumentNullException("Point list can not be null");
        }

        internal Polygon2d(IEnumerable<Point3d> list)
            : this()
        {
            foreach (Point2d p in list)
            {
                Add(p);
            }
        }

        public Polygon2d(Polygon2d polygon) 
            : this()
        {
            for(int i = 0; i < polygon.Count; i++)
            {
                Add(polygon[i]);
            }
        }

        public Polygon2d(Polygon3d polygon)
            : this()
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                Add(polygon[i]);
            }
        }

        /// <summary>
        /// Create a polygon shaped as circle 
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="numberOfEdges"></param>
        /// <param name="origin"></param>
        /// <exception cref="ArgumentException"></exception>
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
		/// Add a range of points to the polygon
		/// </summary>
		/// <param name="points"></param>
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
		/// Add a range of points to the polygon
		/// </summary>
		/// <param name="points"></param>
		internal void AddRange(IEnumerable<Point3d> points)
        {
            AddRange(points.ToArray());
        }

        /// <summary>
        /// Add a new point to the polygon
        /// </summary>
        /// <param name="point">The point to add</param>
        /// <exception cref="ArgumentException">Thrown when the point parameter make the polygon not planar</exception>
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
		/// Add a new point to the polygon
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Add(double x, double y)
        {
            Add(new Point2d(x, y));
        }

		/// <summary>
		/// Remove point ad index <paramref name="index"/>
		/// </summary>
		/// <param name="index"></param>
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
		public Point2d GetNextPoint(int i)
        {
            return this[GetNextIndex(i)];
        }

        /// <summary>
        /// Calculate each edge of the polygon2d
        /// </summary>
        /// <returns>List of Line2d </returns>
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
        /// Remove duplicated points if they have the same coordinates.
        /// </summary>
        /// <param name="toll">Tolleranza</param>
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
        /// Calculate the area, positive if polygon is rightOriented
        /// /// </summary>
        /// <returns>The area of the polygon</returns>
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
        /// Polygon is right oriented if normal is directed towards the Z global positive axis.
        /// </summary>
        /// <returns>True if the polygon is right hand oriented</returns>
        public bool IsRightHandOrdered()
        {
            return GetSignedArea() > 0;
        }

        /// <summary>
        /// Reverse the direction of the array of points
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
        /// <returns></returns>
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
        /// Move polygon by an given increment dx, dy, dz
        /// </summary>
        /// <param name="dx">The X coordinate increment</param>
        /// <param name="dy">The Y coordinate increment</param>
        /// <param name="dz"></param>
        public override void Move(double dx, double dy, double dz = 0)
        {
            for(int i = 0; i < _points.Length; i++)
                _points[i].Move(dx, dy);
        }

        /// <summary>
        /// Move polygon by a given vector
        /// </summary>
        /// <param name="vector">Displacement vector</param>
        public override void Move(Vector3d vector)
        {
            Move(vector.X, vector.Y, 0.0);
        }

        /// <summary>
        /// Get minimum distance from the given point to the border of the polygon
        /// </summary>
        /// <param name="p">The given point</param>
        /// <returns>The minimum distance</returns>
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

        /// <returns>The bounding box of the shape in the global coordinates</returns>
        public BoundingBox2d GetBoundingBox()
        {
            BoundingBox2d bbox = new BoundingBox2d();
            bbox.Update(_points);
            return bbox;
        }

        public override object Clone()
        {
            return new Polygon2d(this);
        }
        
        /// <summary>
        /// Tell if the point to test is inside the polygon
        /// </summary>
        /// <param name="pointToTest">Point to test</param>
        /// <param name="tolerance"></param>
        /// <returns>True if the point is inside</returns>
        public bool IsPointInside(Point2d pointToTest, double tolerance = GeometryBase.Tolerance)
        {
            BoundingBox2d bbox = new BoundingBox2d(this);                       // Creo la Bounding Box del poligono
            bbox.Scale(1.05);                                                   // Scalo per evitare di avere un vertice della BBox coincidente con uno del poligono
            Line2d ray = new Line2d(pointToTest, bbox.Max);                     // Creo la linea tra il punto da testare e il max della BoundingBox (ray)
            double tol = Math.Sqrt(Utilities.Maths.ErrorPropagation.SumSquareTolerance(tolerance, tolerance));

			for (int i = 0; i < _points.Length; i++)                                  // controllo che il punto non sia un vertice
            {
				if (Math.Abs(pointToTest.X - _points[i].X) < tol && Math.Abs(pointToTest.Y - _points[i].Y) < tol)
                     return true;
            }

            Random random = new Random();
            for (int i = 0; i < _points.Length; i++)                             // controllo che il raggio da pointToTest e BBox.max non intersechi un vertice.
            {                                                                   // Se vero: ruoto il raggio (voglio evitare il caso limite del raggio che interseca uno spigolo)
                //if (ray.IsPointOnLine(_points[i], tolerance))                              // continua a controllare finchè non trova un raggio che non interseca vertici
                if (ray.DistanceTo(_points[i]) < tolerance)
                {
                    ray.Rotate(pointToTest, 1.0 / 30.0 + random.NextDouble());
                    i = -1;
                }
            }

            int intersectionCount = 0;

			Line2d[] array = this.Explode();
			for (int i = 0; i < array.Length; i++)                             // creo i lati del poligono
            {
				//if (array[i].IsPointOnLine(pointToTest, tolerance))                            // controllo che il punto da testare non sia su un lato del poligono, altrimenti torna vero
                if (array[i].DistanceTo(pointToTest) < tolerance)
                    return true;
                if (array[i].GetIntersection(ray, out _, tolerance))   // Cerco le intersezioni tra i lati e il raggio                
                    intersectionCount++;                                        // e le aggiungo al contatore                
            }

            if (intersectionCount % 2 == 0)
                return false;                                                   // se sono dispari è interno
            else
                return true;                                                    // se sono dispari è interno

            //
            // Documentation: http://geomalgorithms.com/a03-_inclusion.html
            // Esistono 2 metodi: Crossing Number e Winding Number. Abbiamo utilizzato il crossing number. E' più leggero computazionalmente.
            // Su poligoni a molti lati può essere pesante ma comunque molto meno dello winding number
            // Possibili codici alternativi: https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/
            //
            // Concetto base del metodo: creo una line2d ( ray ) dal punto da testare ad un punto " all'infinito " (che noi abbiamo preso coincidente con 
            // il max della Bounding Box scalata => vedi commenti accanto al codice).
            // Ricerco le intersezioni tra ray e tutti i lati del poligono. Se il conteggio è pari, il punto è esterno. Se è dispari, è interno.
            // 
            // "which counts the number of times a ray starting from the point P crosses the polygon boundary edges. 
            // The point is outside when this "crossing number" is even; otherwise, when it is odd, the point is inside.
        }

        /// <summary>
        /// Tell if the line to test is inside the polygon
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if the line is inside</returns>
        /// <param name="tolerance"></param>
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
        /// Scale the polygon respect to the origin 
        /// </summary>
        /// <param name="factor">Scale factor</param>
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
        /// <param name="center">The point common at all triangles</param>
        /// <param name="tolerance">Tolerance of IsPointInside check</param>
        /// <returns>A list of polygon2d</returns>
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
        /// Create the triangles between each pair of consecutive vertices and the point center in input
        /// </summary>
        /// <param name="center">The point common at all triangles</param>
        /// <returns>A list of polygon2d</returns>
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
        /// <returns>The centroid point of polygon</returns>
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
        /// Get the barycenter. The barycenter is the intersection point of the three lines going through one of the vertices and the middle of the opposite edge of the triangle
        /// </summary>
        /// <returns>The barycenter point of triangle</returns>
        internal Point2d GetBarycenterOfTriangle()
        {
            if (_points.Length != 3)
                throw new Exception("Polygon must be a triangle");

            return new Point2d((_points[0].X + _points[1].X + _points[2].X) / 3.0,
                (_points[0].Y + _points[1].Y + _points[2].Y) / 3.0);
        }

        /// <summary>
        /// Get the barycenter of the area.
        /// </summary>
        /// <returns>The barycenter point of polygon</returns>
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

        public IEnumerator<Point2d> GetEnumerator()
        {
            return _points.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<Point2d>        

        #region Boolean operations

        /// <summary>
        /// Execute the boolean union between <paramref name="a"/> and <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The array of new polygon</returns>
        public static Polygon2d[] Union(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctUnion);
        }

		/// <summary>
		/// Execute the boolean union between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] Union(Polygon2d a, Polygon2d b)
        {
            return Union(new[] { a }, new[] { b });
        }

		/// <summary>
		/// Execute the boolean difference between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] Difference(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctDifference);
        }

		/// <summary>
		/// Execute the boolean difference between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] Difference(Polygon2d a, Polygon2d b)
        {
            return Difference(new[] { a }, new[] { b });
        }

		/// <summary>
		/// Execute the boolean intersection between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] Intersection(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctIntersection);
        }

		/// <summary>
		/// Execute the boolean union intersection <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] Intersection(Polygon2d a, Polygon2d b)
        {
            return Intersection(new[] { a }, new[] { b });
        }
		/// <summary>
		/// Execute the boolean not intersection between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] NotIntersection(Polygon2d[] a, Polygon2d[] b)
        {
            return Boolean(a, b, ClipType.ctXor);
        }

		/// <summary>
		/// Execute the boolean not intersection between <paramref name="a"/> and <paramref name="b"/>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>The array of new polygon</returns>
		public static Polygon2d[] NotIntersection(Polygon2d a, Polygon2d b)
        {
            return NotIntersection(new[] { a }, new[] { b });
        }

        /// <summary>
        /// Execute a boolean operation beetween polygons using the Clipper class
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="code"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        private static Polygon2d[] Boolean(Polygon2d[] a, Polygon2d[] b, ClipType code, int factor = 1000)
        {
            Clipper clipper = new Clipper();
			for (int i = 0; i < a.Length; i++)
            {
				Polygon2d polygon = a[i];
				AddToPath(polygon, factor, clipper, PolyType.ptSubject);
            }
			for (int i = 0; i < b.Length; i++)
            {
				Polygon2d polygon = b[i];
				AddToPath(polygon, factor, clipper, PolyType.ptClip);
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
                        GetPolygonsFromPolyNode(node, factor, ref result);
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
                result.Add(new IntPoint((long)(polygon[v].X * factor), (long)(polygon[v].Y * factor)));
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

        public bool Equals(Polygon2d other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._points.SequenceEqual(_points);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Polygon2d);
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is Polygon2d poly)
                return Equals(poly);

            return false;
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Count", _points.Length);
            for (int i = 0; i < _points.Length; i++)
            {
                info.AddValue($"Point{i}", _points[i], typeof(Point2d));
            }
        }

        public static bool operator ==(Polygon2d obj1, Polygon2d obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Polygon2d obj1, Polygon2d obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
