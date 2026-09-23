using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    internal class Helper
    {
        public static double eps = 0.0001; // 10^-6

        internal class InitialTriangulationHelper
        {
            private readonly Shape2d _shape2d;
            private readonly List<Point2d> _points;
            private readonly List<Point2d> _leftOverPoints;
            private readonly List<Point2d> _pointsUsed;
            internal List<Triangle> _result;

            internal InitialTriangulationHelper(Shape2d shape2d, List<Point2d> points = null)
            {
                _points = new List<Point2d>();
                _leftOverPoints = new List<Point2d>();
                _pointsUsed = new List<Point2d>();
                _result = new List<Triangle>();

                _shape2d = shape2d;
                _points.AddRange(shape2d.Fill2d.Select(i => i).Distinct().ToList());
                if (_shape2d.HasHoles)
                    for (int i = 0; i < _shape2d.Holes2d.Length; i++)
                        _points.AddRange(shape2d.Holes2d[i].Select(j => j).Distinct().ToList());

                if (points != null)
                    _points.AddRange(points);

                for (int i = 0; i < _points.Count; i++)
                    _leftOverPoints.Add((Point2d)_points[i].Clone());

                if (points != null)
                    for (int i = 0; i < points.Count; i++)
                        _leftOverPoints.Add((Point2d)points[i].Clone());
            }

            internal List<Triangle> Triangulate(double tolerance = GeometryBase.Tolerance)
            {
                _result.Clear();

                List<Point2d> leftOverPointsTemp = new List<Point2d>(_leftOverPoints);
                List<Point2d> leftOverPoints2 = new List<Point2d>(_leftOverPoints);

                foreach (Point2d[] p in GetRandomPoints())
                {
                    Triangle root = new Triangle(p[0], p[1], p[2]);
                    if (SmallestTriangle(root))
                    {
                        if (!PointsInside(root, tolerance))
                        {
                            if (_shape2d.IsPointInside(root.Centroid))
                                _result.Add(root);

                            leftOverPointsTemp.Remove(p[0]);
                            leftOverPointsTemp.Remove(p[1]);
                            leftOverPointsTemp.Remove(p[2]);

                            _pointsUsed.Add(p[0]);
                            _pointsUsed.Add(p[1]);
                            _pointsUsed.Add(p[2]);

                            Pair pair = Triangulate(leftOverPointsTemp, _result);
                            if (pair.IsSuccessful)
                                return pair.Triangles;
                            else
                            {
                                _result.RemoveAt(0);
                                _pointsUsed.RemoveAt(_pointsUsed.Count - 1);
                                _pointsUsed.RemoveAt(_pointsUsed.Count - 1);
                                _pointsUsed.RemoveAt(_pointsUsed.Count - 1);

                                leftOverPointsTemp = leftOverPoints2;
                            }
                        }
                    }
                }
                return null;
            }

            private Pair Triangulate(List<Point2d> leftOverPoints, List<Triangle> res)
            {
                while (leftOverPoints.Count > 0)
                {
                    for (int i = 0; i < leftOverPoints.Count; i++)
                    {
                        List<Triangle> temp = GetNextTriangles(leftOverPoints[i], res);
                        if (temp.Count > 0)
                        {
                            for (int j = 0; j < temp.Count; j++)
                                if (_shape2d.IsPointInside(temp[j].Centroid))
                                    if (!res.Contains(temp[j]))
                                        res.Add(temp[j]);

                            Point2d newPoint = leftOverPoints[i];

                            leftOverPoints.RemoveAt(i);
                            _pointsUsed.Add(newPoint);

                            Pair p = Triangulate(leftOverPoints, res);

                            if (p.IsSuccessful)
                                return p;
                            else
                            {
                                res.RemoveRange(res.Count - temp.Count, temp.Count);
                                leftOverPoints.Insert(i, newPoint);
                                _pointsUsed.Remove(newPoint);
                            }
                        }
                    }
                    return new Pair(res, false);
                }
                return new Pair(res, true);
            }

            private List<Triangle> GetNextTriangles(Point2d newPoint, List<Triangle> trianglesAdded, double tolerance = GeometryBase.Tolerance)
            {
                List<Triangle> result = new List<Triangle>();
                List<Triangle> trianglesAddedBuffer = new List<Triangle>(trianglesAdded);

                foreach (Point2d[] p in GetPairRandomPointsUsed())
                {
                    if (!p[0].Equals(newPoint) && !p[1].Equals(newPoint))
                    {
                        Triangle test = new Triangle(p[0], p[1], newPoint);
                        if (CheckTriangle(test, tolerance))
                        {
                            if (!PointsInside(test, tolerance))
                            {
                                if (!Intersect(test, trianglesAddedBuffer))
                                {
                                    result.Add(test);
                                    trianglesAddedBuffer.Add(test);
                                }
                            }
                        }
                    }
                }

                if (result.Count == 0)
                {
                    foreach (Point2d[] p in GetPairRandomPoints())
                    {
                        if (!p[0].Equals(newPoint) && !p[1].Equals(newPoint))
                        {
                            Triangle test = new Triangle(p[0], p[1], newPoint);
                            if (CheckTriangle(test, tolerance))
                            {
                                if (!PointsInside(test, tolerance))
                                {
                                    if (!Intersect(test, trianglesAddedBuffer))
                                    {
                                        result.Add(test);
                                        trianglesAddedBuffer.Add(test);
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }

            private bool PointsInside(Triangle triangle, double tolerance)
            {
                foreach (Point2d p in _points)
                {
                    if (p.DistanceTo(triangle.VertexA) > tolerance &&
                        p.DistanceTo(triangle.VertexB) > tolerance &&
                        p.DistanceTo(triangle.VertexC) > tolerance)
                        if (triangle.Polygon.IsPointInside(p, tolerance))
                            return true;
                }
                return false;
            }

            private bool Intersect(Triangle triangleToTest, List<Triangle> trianglesAdded, double tolerance = GeometryBase.Tolerance)
            {
                foreach (Triangle triangle in trianglesAdded)
                {
                    if (IntersectLine(triangleToTest.Edge1, triangle.Edge1, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge1, triangle.Edge2, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge1, triangle.Edge3, out _, tolerance))
                        return true;

                    if (IntersectLine(triangleToTest.Edge2, triangle.Edge1, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge2, triangle.Edge2, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge2, triangle.Edge3, out _, tolerance))
                        return true;

                    if (IntersectLine(triangleToTest.Edge3, triangle.Edge1, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge3, triangle.Edge2, out _, tolerance))
                        return true;
                    if (IntersectLine(triangleToTest.Edge3, triangle.Edge3, out _, tolerance))
                        return true;

                    if (PointOnLine(triangleToTest.Edge1, triangle.VertexA))
                        return true;
                    if (PointOnLine(triangleToTest.Edge1, triangle.VertexB))
                        return true;
                    if (PointOnLine(triangleToTest.Edge1, triangle.VertexC))
                        return true;

                    if (PointOnLine(triangleToTest.Edge2, triangle.VertexA))
                        return true;
                    if (PointOnLine(triangleToTest.Edge2, triangle.VertexB))
                        return true;
                    if (PointOnLine(triangleToTest.Edge2, triangle.VertexC))
                        return true;

                    if (PointOnLine(triangleToTest.Edge3, triangle.VertexA))
                        return true;
                    if (PointOnLine(triangleToTest.Edge3, triangle.VertexB))
                        return true;
                    if (PointOnLine(triangleToTest.Edge3, triangle.VertexC))
                        return true;

                    Line2d[] lines = _shape2d.Fill2d.Explode();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (IntersectLine(lines[i], triangleToTest.Edge1, out _, tolerance))
                            return true;
                        if (IntersectLine(lines[i], triangleToTest.Edge2, out _, tolerance))
                            return true;
                        if (IntersectLine(lines[i], triangleToTest.Edge3, out _, tolerance))
                            return true;
                    }

                    if (_shape2d.HasHoles)
                    {
                        for (int i = 0; i < _shape2d.Holes.Length; i++)
                        {
                            Line2d[] ll = _shape2d.Holes2d[i].Explode();
                            for (int j = 0; j < ll.Length; j++)
                            {
                                if (IntersectLine(ll[j], triangleToTest.Edge1, out _, tolerance))
                                    return true;
                                if (IntersectLine(ll[j], triangleToTest.Edge2, out _, tolerance))
                                    return true;
                                if (IntersectLine(ll[j], triangleToTest.Edge3, out _, tolerance))
                                    return true;
                            }
                        }
                    }
                }


                return false;
            }

            private bool IntersectLine(Line2d line1, Line2d line2, out Point2d intersection, double tolerance = GeometryBase.Tolerance)
            {
                if (line1.GetIntersection(line2, out intersection, tolerance))
                {
                    if (intersection.DistanceTo(line2.Start) < tolerance || intersection.DistanceTo(line2.End) < tolerance)
                        return false;
                    else
                        return true;
                }

                return false;
            }

            private bool PointOnLine(Line2d line, Point2d point, double tolerance = GeometryBase.Tolerance)
            {
                if (line.IsPointOnLine(point, tolerance))
                {
                    if (point.DistanceTo(line.Start) < tolerance || point.DistanceTo(line.End) < tolerance)
                        return false;
                    else
                        return true;
                }
                return false;
            }

            private IEnumerable<Point2d[]> GetRandomPoints()
            {
                for (int i = 0; i < _points.Count; i++)
                    for (int j = i + 1; j < _points.Count; j++)
                        for (int k = j + 1; k < _points.Count; k++)
                            yield return new Point2d[3] { _points[i], _points[j], _points[k] };
            }

            private IEnumerable<Point2d[]> GetPairRandomPoints()
            {
                for (int i = 0; i < _points.Count; i++)
                    for (int j = i + 1; j < _points.Count; j++)
                        yield return new Point2d[2] { _points[i], _points[j] };
            }

            private IEnumerable<Point2d[]> GetPairRandomPointsUsed()
            {
                for (int i = 0; i < _pointsUsed.Count; i++)
                    for (int j = i + 1; j < _pointsUsed.Count; j++)
                        yield return new Point2d[2] { _pointsUsed[i], _pointsUsed[j] };
            }

            public Dictionary<Triangle, Circle3d> GetCircles(List<Triangle> Triangles)
            {
                Dictionary<Triangle, Circle3d> res = new Dictionary<Triangle, Circle3d>();
                if (Triangles != null)
                    foreach (Triangle t in Triangles)
                    {
                        try
                        {
                            res[t] = new Circle3d(t.VertexA, t.VertexB, t.VertexC);
                        }
                        catch (Exception)
                        {
                            // i punti sono allineati
                            double x = (t.VertexA.X + t.VertexB.X + t.VertexC.X) / 3.0;
                            double y = (t.VertexA.Y + t.VertexB.Y + t.VertexC.Y) / 3.0;
                            Point2d center = new Point2d(x, y);

                            double radius = Math.Max(Math.Max(center.DistanceTo(t.VertexA), center.DistanceTo(t.VertexB)), center.DistanceTo(t.VertexC));
                            res[t] = new Circle3d(center, radius, new Plane(Point2d.Origin, Vector3d.ZAxis));
                        }
                    }
                return res;
            }

            private bool SmallestTriangle(Triangle t, double tolerance = GeometryBase.Tolerance)
            {
                if (!CheckTriangle(t, tolerance))
                    return false;

                List<Triangle> list = new List<Triangle> { t };
                Dictionary<Triangle, Circle3d> dict = GetCircles(list);
                foreach (Point2d p in _points)
                {
                    if (!p.Equals(t.VertexA) && !p.Equals(t.VertexB) && !p.Equals(t.VertexC))
                    {
                        if (Math.Pow(p.X - dict[t].Center.X, 2) + Math.Pow(p.Y - dict[t].Center.Y, 2) <= Math.Pow(dict[t].Radius, 2))
                            return false;
                    }
                }
                return true;
            }

            private bool CheckTriangle(Triangle t, double tolerance = GeometryBase.Tolerance)
            {
                Polygon2d copy = (Polygon2d)t.Polygon.Clone();
                copy.RemoveAlignedPoints(tolerance);
                if (copy.Count < 3)
                    return false;
                return true;
            }


            class Pair
            {
                public List<Triangle> Triangles;

                public bool IsSuccessful = false;

                public Pair(List<Triangle> res, bool success)
                {
                    Triangles = res;
                    IsSuccessful = success;
                }
            }

        }

        public class Triangle
        {
            private readonly int _id;
            private readonly Point2d[] _vertices = new Point2d[3];

            public Point2d[] Vertices => _vertices;

            public int Id
            {
                get { return this._id; }
            }

            public Point2d VertexA => _vertices[0];

            public Point2d VertexB => _vertices[1];

            public Point2d VertexC => _vertices[2];

            public Point2d Centroid => new Point2d((_vertices[0].X + _vertices[1].X + _vertices[2].X) / 3.0f, (_vertices[0].Y + _vertices[1].Y + _vertices[2].Y) / 3.0f) { Tag = -1 };

            public Line2d Edge1 => new Line2d(_vertices[0], _vertices[1]);

            public Line2d Edge2 => new Line2d(_vertices[1], _vertices[2]);

            public Line2d Edge3 => new Line2d(_vertices[2], _vertices[0]);

            public Polygon2d Polygon => new Polygon2d(new Point2d[] { VertexA, VertexB, VertexC });

            public Circle2d Circle => new Circle2d(_vertices[0], _vertices[1], _vertices[2]);

            public Point2d CircleCenter => Circle.Center;

            public double CircleRadius => Circle.Radius;

            public Triangle(int id, Point2d p1, Point2d p2, Point2d p3)
            {
                _id = id;
                if (!IsCounterClockwise(p1, p2, p3))
                {
                    Vertices[0] = p1;
                    Vertices[1] = p3;
                    Vertices[2] = p2;
                }
                else
                {
                    Vertices[0] = p1;
                    Vertices[1] = p2;
                    Vertices[2] = p3;
                }
            }

            public Triangle(Point2d p1, Point2d p2, Point2d p3)
                : this(-1, p1, p2, p3)
            {

            }

            private bool IsCounterClockwise(Point2d point1, Point2d point2, Point2d point3)
            {
                double result = (point2.X - point1.X) * (point3.Y - point1.Y) - (point3.X - point1.X) * (point2.Y - point1.Y);
                return result > 0;
            }

            public bool EdgeExists(Line2d other)
            {
                if (Edge1.Equals(other) == true || Edge2.Equals(other) == true || Edge3.Equals(other) == true)
                    return true;
                return false;
            }

            public override bool Equals(object obj)
            {
                return obj is Triangle triangle &&
                       ((VertexA.Equals(triangle.VertexA) && VertexB.Equals(triangle.VertexB) && VertexC.Equals(triangle.VertexC)) ||
                       (VertexA.Equals(triangle.VertexB) && VertexB.Equals(triangle.VertexC) && VertexC.Equals(triangle.VertexA)) ||
                       (VertexA.Equals(triangle.VertexC) && VertexB.Equals(triangle.VertexA) && VertexC.Equals(triangle.VertexB)));
            }

            public bool Equals(Triangle the_triangle)
            {
                // find the triangle equals
                if (ContainsEdge(the_triangle.Edge1) == true && ContainsEdge(the_triangle.Edge2) == true && ContainsEdge(the_triangle.Edge3) == true)
                    return true;

                return false;
            }

            public bool ContainsEdge(Line2d edge)
            {
                // find whether the edge belongs to the triangle
                if (edge.Equals(Edge1) == true || edge.Equals(Edge2) == true || edge.Equals(Edge3) == true)
                    return true;

                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = 1474027755;
                    hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(VertexA);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(VertexB);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(VertexC);
                    return hashCode;
                }
            }
        }

        public class MeshStore
        {
            private readonly Shape2d _shape;
            private List<PointStore> _points;
            private List<EdgeStore> _edges;
            private List<TriangleStore> _triangles;

            private readonly List<int> _uniqueEdgeIdList;
            private readonly List<int> _uniqueTriangleIdList;

            private PointStore _superTriangleP1;
            private PointStore _superTriangleP2;
            private PointStore _superTriangleP3;

            public List<Point2d> LocalInputPoints = new List<Point2d>();
            public List<Line2d> LocalOutputEdges = new List<Line2d>();
            public List<Triangle> LocalOutputTriangle = new List<Triangle>();

            public bool IsMeshed;

            public List<PointStore> Points => _points;

            public List<TriangleStore> Triangles => _triangles;

            public List<EdgeStore> Edges => _edges;

            public MeshStore(Shape2d shape)
            {
                _shape = shape;
                _points = new List<PointStore>();
                _edges = new List<EdgeStore>();
                _triangles = new List<TriangleStore>();
                _uniqueEdgeIdList = new List<int>();
                _uniqueTriangleIdList = new List<int>();
                IsMeshed = false;
            }

            public void AddMultiplePoints(List<Point2d> inptPoints)
            {
                // Call this first
                _points = new List<PointStore>();
                // Transfer the parent variable to local point_store list
                for (int i = 0; i < inptPoints.Count; i++)
                {
                    Point2d pt = inptPoints[i];
                    _points.Add(new PointStore((int)pt.Tag, pt.X, pt.Y, pt));
                }

                // set the points to local list
                LocalInputPoints = inptPoints;

                // sort the points first by x and then by y
                _points = _points.OrderBy(obj => obj.X).ThenBy(obj => obj.Y).ToList();


                // intialize the edges and triangles
                _edges = new List<EdgeStore>();
                _triangles = new List<TriangleStore>();

                // Create an imaginary triangle that encloses all the point set
                SetBoundingTriangle(Points);

                for (int i = 0; i < Points.Count; i++)
                {
                    IncrementalPointAddition(Points[i]);
                }
            }

            public void AddSinglePoint(Point2d parentPoint, bool flipBadEdges = true)
            {
                // dont call this before calling Add_multiple_points
                PointStore pt = new PointStore(Points.Count, parentPoint.X, parentPoint.Y, parentPoint);
                // Add the point to local list
                _points.Add(pt);
                // Add to local point list
                LocalInputPoints.Add(Points[Points.Count - 1].ParentPoint);

                // call the incremental add point
                IncrementalPointAddition(Points[Points.Count - 1], flipBadEdges);
            }

            private void IncrementalPointAddition(PointStore pt, bool flipBadEdges = true)
            {
                // Find the index of triangle containing this point
                int triangleIndex = Triangles.FindIndex(obj => obj.IsPointInside(pt) == true);

                if (triangleIndex != -1)
                {
                    // collect the edges of the triangle
                    EdgeStore edgeA = Triangles[triangleIndex].EdgeAB;
                    EdgeStore edgeB = Triangles[triangleIndex].EdgeBC;
                    EdgeStore edgeC = Triangles[triangleIndex].EdgeCA;

                    // remove the single triangle
                    RemoveTriangle(triangleIndex);

                    // add the three triangles
                    int[] triangleId = AddThreeTriangles(pt, edgeA, edgeB, edgeC);

                    if (flipBadEdges)
                    {
                        // Flip the bad triangles recursively
                        FlipBadEdges(triangleId[0], pt);
                        FlipBadEdges(triangleId[1], pt);
                        FlipBadEdges(triangleId[2], pt);
                    }
                }
                else
                {
                    // Point lies on the edge
                    // Find the edge which is closest to the pt
                    int edgeId = Edges.Find(obj => obj.IsPointOnEdge(pt)).Id;

                    if (!pt.Equals(Edges[edgeId].Start) && !pt.Equals(Edges[edgeId].End))
                    {
                        int firstTriangleIndex = Edges[edgeId].LeftTriangle.Id;
                        int secondTriangleIndex = Edges[edgeId].RightTriangle.Id;

                        // collect the other two edges
                        EdgeStore[] firstTriOtherTwoEdge = Triangles[firstTriangleIndex].GetOtherTwoEdges(Edges[edgeId]);
                        EdgeStore[] secondTriOtherTwoEdge = Triangles[secondTriangleIndex].GetOtherTwoEdges(Edges[edgeId]);

                        // Remove the common edge
                        _uniqueEdgeIdList.Add(Edges[edgeId].Id);
                        // this._all_edges.RemoveAt(the_edge_index);
                        _edges[edgeId].CleanEdge();

                        // remove the two triangle
                        RemoveTriangle(firstTriangleIndex);
                        RemoveTriangle(secondTriangleIndex);

                        // add the three triangles
                        int[] trianglesId = AddFourTriangles(pt, firstTriOtherTwoEdge[0], firstTriOtherTwoEdge[1], secondTriOtherTwoEdge[0], secondTriOtherTwoEdge[1]);

                        if (flipBadEdges)
                        {
                            // Flip the bad triangles recursively
                            FlipBadEdges(trianglesId[0], pt);
                            FlipBadEdges(trianglesId[1], pt);
                            FlipBadEdges(trianglesId[2], pt);
                            FlipBadEdges(trianglesId[3], pt);
                        }
                    }
                }
            }

            public void FinalizeMesh()
            {
                // Call this after calling Add_multiple_points & Add_single_point (if required)
                // Finalize mesh is the final step to transfer the mesh from local data to global
                LocalOutputEdges = new List<Line2d>(Edges.Count);
                LocalOutputTriangle = new List<Triangle>(Triangles.Count);
                HashSet<Line2d> outputTriangleEdges = new HashSet<Line2d>();

                int i = 0;
                for (int t = 0; t < Triangles.Count; t++)
                {
                    // Check if the triangles lies inside the surface
                    if (!_shape.IsPointInside(Triangles[t].MidPoint.Point))
                    {
                        // continue because the face is not in surface
                        continue;
                    }

                    Triangle temp = new Triangle(i, Triangles[t].VertexA.ParentPoint, Triangles[t].VertexB.ParentPoint, Triangles[t].VertexC.ParentPoint);
                    LocalOutputTriangle.Add(temp);
                    outputTriangleEdges.Add(temp.Edge1);
                    outputTriangleEdges.Add(temp.Edge2);
                    outputTriangleEdges.Add(temp.Edge3);
                    i++;
                }

                i = 0;
                for (int e = 0; e < Edges.Count; e++)
                {
                    // Check if the edges lies inside the surface
                    Line2d temp = new Line2d(Edges[e].Start.ParentPoint, Edges[e].End.ParentPoint) { Tag = i };

                    if (outputTriangleEdges.Contains(temp)) // Check whether this edge is associated with an output face.
                    {
                        LocalOutputEdges.Add(temp);
                        i++;
                    }
                }

                // set the mesh complete
                IsMeshed = true;
            }

            private void RemoveTriangle(int triangleIndex)
            {
                int edgeIndex1 = Triangles[triangleIndex].EdgeAB.Id;
                int edgeIndex2 = Triangles[triangleIndex].EdgeBC.Id;
                int edgeIndex3 = Triangles[triangleIndex].EdgeCA.Id;

                if (edgeIndex1 != -1)
                    _edges[edgeIndex1].RemoveTriangle(Triangles[triangleIndex]);

                if (edgeIndex2 != -1)
                    _edges[edgeIndex2].RemoveTriangle(Triangles[triangleIndex]);

                if (edgeIndex3 != -1)
                    _edges[edgeIndex3].RemoveTriangle(Triangles[triangleIndex]);

                _uniqueTriangleIdList.Add(Triangles[triangleIndex].Id);
                _triangles[triangleIndex].CleanTriangle();
            }

            public int[] AddThreeTriangles(PointStore pt, EdgeStore edgeA, EdgeStore edgeB, EdgeStore edgeC)
            {
                int[] edgeIndices = new int[3] { AddEdge(pt, edgeA.Start), AddEdge(pt, edgeB.Start), AddEdge(pt, edgeC.Start) };

                int[] outputIndices = new int[3] {
                    AddTriangle(pt, edgeA.Start, edgeA.End, Edges[edgeIndices[0]], edgeA, Edges[edgeIndices[1]].Symmetrical),
                    AddTriangle(pt, edgeB.Start, edgeB.End, Edges[edgeIndices[1]], edgeB, Edges[edgeIndices[2]].Symmetrical),
                    AddTriangle(pt, edgeC.Start, edgeC.End, Edges[edgeIndices[2]], edgeC, Edges[edgeIndices[0]].Symmetrical)};

                _edges[edgeIndices[0]].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeIndices[0]].AddTriangle(Triangles[outputIndices[2]]);
                _edges[edgeIndices[1]].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeIndices[1]].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeIndices[2]].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeIndices[2]].AddTriangle(Triangles[outputIndices[2]]);

                _edges[edgeA.Id].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeB.Id].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeC.Id].AddTriangle(Triangles[outputIndices[2]]);

                return outputIndices;
            }

            public int[] AddFourTriangles(PointStore pt, EdgeStore edgeA, EdgeStore edgeB, EdgeStore edgeC, EdgeStore edgeD)
            {
                int[] edgeIndices = new int[4] { AddEdge(pt, edgeA.Start), AddEdge(pt, edgeB.Start), AddEdge(pt, edgeC.Start), AddEdge(pt, edgeD.Start) };

                int[] outputIndices = new int[4] {
                    AddTriangle(pt, edgeA.Start, edgeA.End, Edges[edgeIndices[0]], edgeA, Edges[edgeIndices[1]].Symmetrical),
                    AddTriangle(pt, edgeB.Start, edgeB.End, Edges[edgeIndices[1]], edgeB, Edges[edgeIndices[2]].Symmetrical),
                    AddTriangle(pt, edgeC.Start, edgeC.End, Edges[edgeIndices[2]], edgeC, Edges[edgeIndices[3]].Symmetrical),
                    AddTriangle(pt, edgeD.Start, edgeD.End, Edges[edgeIndices[3]], edgeD, Edges[edgeIndices[0]].Symmetrical) };

                _edges[edgeIndices[0]].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeIndices[0]].AddTriangle(Triangles[outputIndices[3]]);
                _edges[edgeIndices[1]].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeIndices[1]].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeIndices[2]].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeIndices[2]].AddTriangle(Triangles[outputIndices[2]]);
                _edges[edgeIndices[3]].AddTriangle(Triangles[outputIndices[2]]);
                _edges[edgeIndices[3]].AddTriangle(Triangles[outputIndices[3]]);

                _edges[edgeA.Id].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeB.Id].AddTriangle(Triangles[outputIndices[1]]);
                _edges[edgeC.Id].AddTriangle(Triangles[outputIndices[2]]);
                _edges[edgeD.Id].AddTriangle(Triangles[outputIndices[3]]);

                return outputIndices;
            }

            public int[] AddTwoTriangles(EdgeStore triAEdgeA, EdgeStore triAEdgeB, EdgeStore triBEdgeA, EdgeStore triBEdgeB)
            {
                int edgeIndex = AddEdge(triAEdgeB.Start, triBEdgeB.Start);
                int[] outputIndices = new int[2] {
                    AddTriangle(triAEdgeB.Start, triBEdgeA.Start, Edges[edgeIndex].Symmetrical.Start, triAEdgeB, triBEdgeA, Edges[edgeIndex].Symmetrical),
                    AddTriangle(triBEdgeB.Start, triAEdgeA.Start, Edges[edgeIndex].Start, triBEdgeB, triAEdgeA, Edges[edgeIndex]) };

                _edges[edgeIndex].AddTriangle(Triangles[outputIndices[0]]);
                _edges[edgeIndex].AddTriangle(Triangles[outputIndices[1]]);
                _edges[triAEdgeB.Id].AddTriangle(Triangles[outputIndices[0]]);
                _edges[triBEdgeA.Id].AddTriangle(Triangles[outputIndices[0]]);
                _edges[triBEdgeB.Id].AddTriangle(Triangles[outputIndices[1]]);
                _edges[triAEdgeA.Id].AddTriangle(Triangles[outputIndices[1]]);

                return outputIndices;
            }

            private int AddEdge(PointStore pt1, PointStore pt2)
            {
                int edgeIndex = GetUniqueEdgeId();
                if (edgeIndex > Edges.Count - 1)
                {
                    EdgeStore tempEdge = new EdgeStore();
                    tempEdge.SetEdge(edgeIndex, pt1, pt2);
                    _edges.Add(tempEdge);
                }
                else
                {
                    _edges[edgeIndex].SetEdge(edgeIndex, pt1, pt2);
                }
                return edgeIndex;
            }

            private int AddTriangle(PointStore pt1, PointStore pt2, PointStore pt3, EdgeStore e1, EdgeStore e2, EdgeStore e3)
            {
                // Add Triangle
                int triIndex = GetUniqueTriangleId();
                if (triIndex > Triangles.Count - 1)
                {
                    // new triangle is added
                    TriangleStore tempTri = new TriangleStore();
                    tempTri.SetTriangle(triIndex, pt1, pt2, pt3, e1, e2, e3);
                    _triangles.Add(tempTri);
                }
                else
                {
                    // existing triangle is revised (previously deleted triangle is now filled)
                    _triangles[triIndex].SetTriangle(triIndex, pt1, pt2, pt3, e1, e2, e3);
                }
                return triIndex;
            }

            private void FlipBadEdges(int triangleIndex, PointStore pt, bool flipBadEdges = true)
            {
                //flip recursively for the new pair of triangles
                //
                //           pl                    pl
                //          /||\                  /  \
                //       al/ || \bl            al/    \bl
                //        /  ||  \              /      \
                //       /  a||b  \    flip    /___a____\
                //     p0\   ||   /pt   =>   p0\---b----/pt
                //        \  ||  /              \      /
                //       ar\ || /br            ar\    /br
                //          \||/                  \  /
                //           p2                    p2
                //
                // find the edge of this triangle whihc does not contain pt
                int commonEdgeIndex = Triangles[triangleIndex].GetOtherEdgeId(pt);

                if (commonEdgeIndex != -1)
                {
                    // Find the neighbour tri index
                    int neighbourTriangleIndex = Edges[commonEdgeIndex].OtherTriangleId(Triangles[triangleIndex]);

                    // legalize only if the triangle has a neighbour
                    if (neighbourTriangleIndex != -1)
                    {
                        // check whether the newly added pt is inside the neighbour triangle
                        if (Triangles[neighbourTriangleIndex].IsPointInCircle(pt) == true)
                        {
                            bool exit = false;
                            var lines = _shape.Fill2d.Explode();
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].IsPointOnLine(Edges[commonEdgeIndex].Start.Point) && lines[i].IsPointOnLine(Edges[commonEdgeIndex].End.Point))
                                {
                                    exit = true;
                                    break;
                                }
                            }

                            if (!exit)
                            {
                                EdgeStore[] triangleOtherTwoEdges = Triangles[triangleIndex].GetOtherTwoEdges(Edges[commonEdgeIndex]);
                                EdgeStore[] neighbourTriOtherTwoEdges = Triangles[neighbourTriangleIndex].GetOtherTwoEdges(Edges[commonEdgeIndex]);

                                _uniqueEdgeIdList.Add(Edges[commonEdgeIndex].Id);
                                _edges[commonEdgeIndex].CleanEdge();

                                RemoveTriangle(triangleIndex);
                                RemoveTriangle(neighbourTriangleIndex);

                                int[] triangleId = AddTwoTriangles(triangleOtherTwoEdges[0], triangleOtherTwoEdges[1], neighbourTriOtherTwoEdges[0], neighbourTriOtherTwoEdges[1]);

                                if (flipBadEdges)
                                {
                                    FlipBadEdges(triangleId[0], pt);
                                    FlipBadEdges(triangleId[1], pt);
                                }
                            }
                        }
                    }
                }
            }

            private int GetUniqueEdgeId()
            {
                int edge_id;
                if (_uniqueEdgeIdList.Count != 0)
                {
                    edge_id = _uniqueEdgeIdList[0]; // retrive the edge id from the list which stores the id of deleted edges
                    _uniqueEdgeIdList.RemoveAt(0); // remove that id from the unique edge id list
                }
                else
                {
                    edge_id = Edges.Count;
                }
                return edge_id;

            }

            private int GetUniqueTriangleId()
            {
                int tri_id;
                if (_uniqueTriangleIdList.Count != 0)
                {
                    tri_id = _uniqueTriangleIdList[0]; // retrive the triangle id from the list which stores the id of deleted edges
                    _uniqueTriangleIdList.RemoveAt(0); // remove that id from the unique triangle id list
                }
                else
                {
                    tri_id = Triangles.Count;
                }
                return tri_id;
            }

            private void SetBoundingTriangle(List<PointStore> allInputVertices)
            {
                PointStore[] xSorted = allInputVertices.OrderBy(obj => obj.X).ToArray();
                PointStore[] ySorted = allInputVertices.OrderBy(obj => obj.Y).ToArray();

                double xMax = (xSorted[xSorted.Length - 1].X - xSorted[0].X);
                double yMax = (ySorted[ySorted.Length - 1].Y - ySorted[0].Y);
                double k = 1000 * Math.Max(xMax, yMax);

                double xZero = (xSorted[xSorted.Length - 1].X + xSorted[0].X) * 0.5;
                double yZero = (ySorted[ySorted.Length - 1].Y + ySorted[0].Y) * 0.5;

                int count = allInputVertices.Count;

                _superTriangleP1 = new PointStore(count + 1, 0, Math.Round(k / 2.0f), new Point2d(xZero, -k) { Tag = count + 1 });
                _superTriangleP2 = new PointStore(count + 2, Math.Round(k / 2.0f), 0.0, new Point2d(k, yZero) { Tag = count + 2 });
                _superTriangleP3 = new PointStore(count + 3, -1 * Math.Round(k / 2.0f), -1 * Math.Round(k / 2.0f), new Point2d(-k, k) { Tag = count + 3 });

                int[] edge_indices = new int[3] { AddEdge(_superTriangleP1, _superTriangleP2),
                AddEdge(_superTriangleP2, _superTriangleP3), AddEdge(_superTriangleP3, _superTriangleP1) };

                int superTriangleIndex = AddTriangle(_superTriangleP1, _superTriangleP2, _superTriangleP3,
                    Edges[edge_indices[0]], Edges[edge_indices[1]], Edges[edge_indices[2]]);

                Edges[edge_indices[0]].AddTriangle(Triangles[superTriangleIndex]);
                Edges[edge_indices[1]].AddTriangle(Triangles[superTriangleIndex]);
                Edges[edge_indices[2]].AddTriangle(Triangles[superTriangleIndex]);
            }
        }

        public class SurfaceStore
        {
            // surface variables
            private readonly int _surfaceId;
            private readonly int _encapsulatingSurfaceId;
            private Mesh _myMesh = new Mesh();


            public List<Point2d> SurfaceNodes = new List<Point2d>();
            public List<Point2d> InnerNodes = new List<Point2d>();
            public List<Line2d> SurfaceEdges = new List<Line2d>();
            public List<Line2d> EncapsulatingSeedEdges = new List<Line2d>();
            public List<SurfaceStore> InnerSurfaces = new List<SurfaceStore>();


            public bool IsMeshed;

            public Mesh MyMesh { get => _myMesh; internal set => _myMesh = value; }

            public double Area => Math.Abs(SignedPolygonArea());

            public int Id => _surfaceId;

            public int EncapsulatingSurfaceId => _encapsulatingSurfaceId;

            public SurfaceStore(int id, List<Point2d> surfaceNodes, List<Line2d> surfaceEdges)
            {
                _surfaceId = id; // add surface id
                SurfaceNodes.AddRange(surfaceNodes);
                SurfaceEdges.AddRange(surfaceEdges);
                IsMeshed = false;
                _encapsulatingSurfaceId = -1;
            }

            // Return the polygon's area in "square units."
            // The value will be negative if the polygon is
            // oriented clockwise.
            public double SignedPolygonArea()
            {
                Point2d[] polygon = new Point2d[SurfaceEdges.Count];
                for (int i = 0; i < SurfaceEdges.Count; i++)
                    polygon[i] = SurfaceEdges[i].End;

                Point2d[] pts = new Point2d[polygon.Length + 1];
                polygon.CopyTo(pts, 0);
                pts[polygon.Length] = polygon[0];

                double area = 0;
                for (int i = 0; i < polygon.Length; i++)
                    area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;

                return area;
            }

            // Return True if the point is in the polygon (outside edges of the surface).
            // Note this will return if the point is inside the outside edges of the surface (use point in surface to find the points in surface)
            public bool PointInPolygon(Point2d pt)
            {
                return IsPointInPolygon(SurfaceNodes.ToArray(), pt);
            }

            public bool PointInSurface(PointStore pt)
            {
                return PointInSurface(pt.Point);
            }

            public bool PointInSurface(Point2d pt)
            {
                if (PointInPolygon(pt) == true)
                {
                    foreach (SurfaceStore inner_surf in InnerSurfaces)
                    {
                        if (inner_surf.PointInPolygon(pt) == true) // the point lies in inner surface so not the selected surface
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class PointStore
        {
            private readonly int _id;
            private readonly double _x;
            private readonly double _y;
            private readonly Point2d _parentPoint;

            public int Id => _id;

            public double X => _x;

            public double Y => _y;

            public Point2d Point => new Point2d(_x, _y) { Tag = _id };

            public Point2d ParentPoint => _parentPoint;

            public PointStore(int id, double x, double y, Point2d pt)
            {
                _id = id;
                _x = x;
                _y = y;
                _parentPoint = pt;
            }

            public PointStore(Point2d point, Point2d parent)
                : this((int)point.Tag, point.X, point.Y, parent)
            {
            }

            public bool Equals(PointStore other)
            {
                if (Math.Abs(_x - other.X) <= eps && Math.Abs(_y - other.Y) <= eps)
                    return true;
                return false;
            }

            public bool Equals(PointStore other, double tolerance)
            {
                if (Math.Abs(_x - other.X) <= tolerance && Math.Abs(_y - other.Y) <= tolerance)
                    return true;
                return false;
            }

            // Operators
            public PointStore Subtract(PointStore other)
            {
                double x = X - other.X;
                double y = Y - other.Y;

                return new PointStore(-1, x, y, null);
            }

            public PointStore Add(PointStore other)
            {
                double x = X + other.X;
                double y = Y + other.Y;

                return new PointStore(-1, x, y, null);
            }

            public double Dot(PointStore other)
            {
                return (X * other.X) + (Y * other.Y);
            }

            public double Cross(PointStore other)
            {
                return (Y * other.X) - (X * other.Y);
            }

            public PointStore Multiply(double v)
            {
                return (new PointStore(Id, X * v, Y * v, null));
            }
        }

        public class EdgeStore
        {
            private int _id;
            private PointStore _start;
            private PointStore _end;
            private TriangleStore _leftTriangle;
            private TriangleStore _rightTriangle;
            private EdgeStore _symmetrical;

            public int Id => _id;

            public PointStore Start => _start;

            public PointStore End => _end;

            public EdgeStore Symmetrical => _symmetrical;

            public TriangleStore LeftTriangle => _leftTriangle;

            public TriangleStore RightTriangle => _rightTriangle;

            public double Length => Math.Sqrt(Math.Pow(this._start.X - this._end.X, 2) + Math.Pow(this._start.Y - this._end.Y, 2));

            public Line2d Line => new Line2d(_start.Point, _end.Point);

            public EdgeStore()
            {
                // Empty Constructor
            }

            public void SetEdge(int id, PointStore start, PointStore end)
            {
                _id = id;
                _start = start;
                _end = end;
                _leftTriangle = null;
                _rightTriangle = null;

                _symmetrical = new EdgeStore
                {
                    _id = id,
                    _start = end,
                    _end = start,
                    _leftTriangle = null,
                    _rightTriangle = null,
                    _symmetrical = this
                };
            }

            public void CleanEdge()
            {
                _id = -1;
                _start = null;
                _end = null;
                _leftTriangle = null;
                _rightTriangle = null;
                _symmetrical._id = -1;
                _symmetrical._start = null;
                _symmetrical._end = null;
                _symmetrical._leftTriangle = null;
                _symmetrical._rightTriangle = null;
            }

            public int OtherTriangleId(TriangleStore triangle)
            {
                if (triangle.Id != -1)
                {
                    if (_leftTriangle != null)
                    {
                        if (triangle.Equals(this._leftTriangle) == true)
                        {
                            if (_rightTriangle == null)
                            {
                                return -1;
                            }
                            return _rightTriangle.Id;
                        }
                    }

                    if (_rightTriangle != null)
                    {
                        if (triangle.Equals(_rightTriangle) == true)
                        {
                            if (_leftTriangle == null)
                            {
                                return -1;
                            }
                            return _leftTriangle.Id;
                        }
                    }
                }
                return -1;
            }

            public void AddTriangle(TriangleStore triangle)
            {
                // check whether the input triangle has this edge
                //if (the_triangle.contains_edge(this) == true)
                //{
                if (RightOf(triangle.MidPoint, this) == true)
                {
                    _rightTriangle = triangle;
                    _symmetrical._leftTriangle = triangle;
                }
                else
                {
                    _leftTriangle = triangle;
                    _symmetrical._rightTriangle = triangle;
                }
                //}
            }

            public void RemoveTriangle(TriangleStore triangle)
            {
                // check whether the input triangle has this edge
                //if (the_triangle.contains_edge(this) == true)
                //{
                if (RightOf(triangle.MidPoint, this) == true)
                {
                    // Remove the right triangle
                    _rightTriangle = null;
                    _symmetrical._leftTriangle = null;
                }
                else
                {
                    // Remove the left triangle
                    _leftTriangle = null;
                    _symmetrical._rightTriangle = null;
                }
                //}
            }

            private bool IsOrientedCounterclockwise(PointStore a, PointStore b, PointStore c)
            {
                // Computes | a.x a.y  1 |
                //          | b.x b.y  1 | > 0
                //          | c.x c.y  1 |
                return (((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X))) > 0;
            }

            private bool RightOf(PointStore x, EdgeStore e)
            {
                return IsOrientedCounterclockwise(x, e.End, e.Start);
            }

            private bool EqualsBuffer(EdgeStore other)
            {
                if (other.Start.Equals(_start) == true && other.End.Equals(_end) == true)
                    return true;

                return false;
            }

            public bool Equals(EdgeStore other)
            {
                if (Id != -1)
                    if ((other.EqualsBuffer(this) == true) || (other.EqualsBuffer(Symmetrical) == true))
                        return true;

                return false;
            }

            public bool IsPointOnEdge(PointStore pt, double tolerance = 0.01)
            {
                return Line.IsPointOnLine(pt.Point, tolerance);
            }
        }

        public class TriangleStore
        {
            private int _id;
            private PointStore _pt1;
            private PointStore _pt2;
            private PointStore _pt3;
            private EdgeStore _e1; // from pt1 to pt2
            private EdgeStore _e2; // from pt2 to pt3
            private EdgeStore _e3; // from pt3 to pt1
            private PointStore _circleCenter;
            private double _circleRadius;

            public PointStore[] ShrunkVertices { get; } = new PointStore[3];

            public int Id => _id;

            public PointStore VertexA => _pt1;

            public PointStore VertexB => _pt2;

            public PointStore VertexC => _pt3;

            public EdgeStore EdgeAB => _e1;

            public EdgeStore EdgeBC => _e2;

            public EdgeStore EdgeCA => _e3;

            public PointStore MidPoint => new PointStore(-1, (_pt1.X + _pt2.X + _pt3.X) / 3.0f, (_pt1.Y + _pt2.Y + _pt3.Y) / 3.0f, null);

            public PointStore CircleCenter => _circleCenter;

            public double RadiusShortestEdgeRatio => _circleRadius / ShortestEdge;

            public double CircleRadius => _circleRadius;

            public double ShortestEdge => GetShortestEdge();

            public double LongestEdge => GetLongestEdge();

            public TriangleStore()
            {
            }

            public void SetTriangle(int id, PointStore p1, PointStore p2, PointStore p3, EdgeStore e1, EdgeStore e2, EdgeStore e3)
            {
                _id = id;
                _pt1 = p1;
                _pt2 = p2;
                _pt3 = p3;
                _e1 = e1;
                _e2 = e2;
                _e3 = e3;

                SetCircle();
                SetShrunkVertices();
            }

            public void CleanTriangle()
            {
                _id = -1;
                _pt1 = null;
                _pt2 = null;
                _pt3 = null;
                _e1 = null;
                _e2 = null;
                _e3 = null;
                _circleCenter = null;
                _circleRadius = 0.0;
                ShrunkVertices[0] = null;
                ShrunkVertices[1] = null;
                ShrunkVertices[2] = null;
            }

            private void SetShrunkVertices()
            {
                double shrinkFactor = 0.98;
                PointStore pt1 = new PointStore(-1, MidPoint.X * (1 - shrinkFactor) + (VertexA.X * shrinkFactor), MidPoint.Y * (1 - shrinkFactor) + (VertexA.Y * shrinkFactor), null);
                PointStore pt2 = new PointStore(-1, MidPoint.X * (1 - shrinkFactor) + (VertexB.X * shrinkFactor), MidPoint.Y * (1 - shrinkFactor) + (VertexB.Y * shrinkFactor), null);
                PointStore pt3 = new PointStore(-1, MidPoint.X * (1 - shrinkFactor) + (VertexC.X * shrinkFactor), MidPoint.Y * (1 - shrinkFactor) + (VertexC.Y * shrinkFactor), null);

                ShrunkVertices[0] = pt1;
                ShrunkVertices[1] = pt2;
                ShrunkVertices[2] = pt3;
            }

            private void SetCircle()
            {
                Circle2d circle = new Circle2d(VertexA.Point, VertexB.Point, VertexC.Point);
                _circleCenter = new PointStore(-1, circle.Center.X, circle.Center.Y, new Point2d(circle.Center.X, circle.Center.Y) { Tag = -100 });
                _circleRadius = circle.Radius;
            }

            private double GetShortestEdge()
            {
                double l1 = EdgeAB.Length;
                double l2 = EdgeBC.Length;
                double l3 = EdgeCA.Length;

                if (l1 <= l2 && l1 <= l3)
                    return l1;
                else if (l2 <= l1 && l2 <= l3)
                    return l2;
                else if (l3 <= l2 && l3 <= l1)
                    return l3;
                else
                    return -1;
            }

            private double GetLongestEdge()
            {
                double l1 = EdgeAB.Length;
                double l2 = EdgeBC.Length;
                double l3 = EdgeCA.Length;

                if (l1 >= l2 && l1 >= l3)
                    return l1;
                else if (l2 >= l1 && l2 >= l3)
                    return l2;
                else if (l3 >= l1 && l3 >= l2)
                    return l3;
                return -1;
            }

            public EdgeStore[] GetOtherTwoEdges(EdgeStore edge)
            {
                EdgeStore[] otherTwoEdge = new EdgeStore[2];

                if (EdgeAB.Equals(edge) == true)
                {
                    otherTwoEdge[0] = EdgeBC;
                    otherTwoEdge[1] = EdgeCA;
                    return otherTwoEdge;
                }
                else if (EdgeBC.Equals(edge) == true)
                {
                    otherTwoEdge[0] = EdgeCA;
                    otherTwoEdge[1] = EdgeAB;
                    return otherTwoEdge;
                }
                else if (EdgeCA.Equals(edge) == true)
                {
                    otherTwoEdge[0] = EdgeAB;
                    otherTwoEdge[1] = EdgeBC;
                    return otherTwoEdge;
                }
                return null;
            }

            public int GetOtherEdgeId(PointStore pt, double tolerance)
            {
                if (EdgeAB.Start.Equals(pt, tolerance) == false && EdgeAB.End.Equals(pt, tolerance) == false)
                    return EdgeAB.Id;

                else if (EdgeBC.Start.Equals(pt, tolerance) == false && EdgeBC.End.Equals(pt, tolerance) == false)
                    return EdgeBC.Id;

                else if (EdgeCA.Start.Equals(pt, tolerance) == false && EdgeCA.End.Equals(pt, tolerance) == false)
                    return EdgeCA.Id;

                return -1;
            }

            public int GetOtherEdgeId(PointStore pt)
            {
                if (EdgeAB.Start.Equals(pt, 1) == false && EdgeAB.End.Equals(pt, 1) == false)
                    return EdgeAB.Id;

                else if (EdgeBC.Start.Equals(pt, 1) == false && EdgeBC.End.Equals(pt, 1) == false)
                    return EdgeBC.Id;

                else if (EdgeCA.Start.Equals(pt, 1) == false && EdgeCA.End.Equals(pt, 1) == false)
                    return EdgeCA.Id;

                return -1;
            }

            public bool ContainsEdge(EdgeStore edge)
            {
                if (edge.Equals(_e1) == true || edge.Equals(_e2) == true || edge.Equals(_e3) == true)
                    return true;

                return false;
            }

            public bool IsPointInside(PointStore point)
            {
                double pab = point.Subtract(VertexA).Cross(VertexB.Subtract(VertexA));
                double pbc = point.Subtract(VertexB).Cross(VertexC.Subtract(VertexB));

                if (SameSign(pab, pbc) == false)
                    return false;

                double pca = point.Subtract(VertexC).Cross(VertexA.Subtract(VertexC));

                if (SameSign(pab, pca) == false)
                    return false;

                if (EdgeAB.IsPointOnEdge(point) == true)
                    return false;

                else if (EdgeBC.IsPointOnEdge(point) == true)
                    return false;

                else if (EdgeCA.IsPointOnEdge(point) == true)
                    return false;

                return true;
            }

            private bool SameSign(double a, double b)
            {
                return Math.Sign(a) == Math.Sign(b);
            }

            public bool IsPointInCircle(PointStore point)
            {
                double a11 = VertexA.X - point.X;
                double a21 = VertexB.X - point.X;
                double a31 = VertexC.X - point.X;

                double a12 = VertexA.Y - point.Y;
                double a22 = VertexB.Y - point.Y;
                double a32 = VertexC.Y - point.Y;

                double a13 = (VertexA.X - point.X) * (VertexA.X - point.X) + (VertexA.Y - point.Y) * (VertexA.Y - point.Y);
                double a23 = (VertexB.X - point.X) * (VertexB.X - point.X) + (VertexB.Y - point.Y) * (VertexB.Y - point.Y);
                double a33 = (VertexC.X - point.X) * (VertexC.X - point.X) + (VertexC.Y - point.Y) * (VertexC.Y - point.Y);

                double det = (a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32 - a13 * a22 * a31 - a12 * a21 * a33 - a11 * a23 * a32);

                return ((IsOrientedCounterclockwise() == true) ? det > 0.0 : det < 0.0);
            }

            private bool IsOrientedCounterclockwise()
            {
                double a11 = VertexA.X - VertexC.X;
                double a21 = VertexB.X - VertexC.X;
                double a12 = VertexA.Y - VertexC.Y;
                double a22 = VertexB.Y - VertexC.Y;

                double det = a11 * a22 - a12 * a21;

                return det > 0.0;
            }

            public bool Equals(TriangleStore the_triangle)
            {
                // find the triangle equals
                if (ContainsEdge(the_triangle.EdgeAB) == true && ContainsEdge(the_triangle.EdgeBC) == true && ContainsEdge(the_triangle.EdgeCA) == true)
                    return true;

                return false;
            }
        }

        internal static InitialMesh BuildMesh(List<Triangle> triangles)
        {
            InitialMesh mesh = new InitialMesh();

            Dictionary<Point2d, MeshVertex> pointVertexDict = new Dictionary<Point2d, MeshVertex>();
            Dictionary<Point2d, int> pointIdAssociation = new Dictionary<Point2d, int>();
            int progressVertexId = 0;

            for (int i = 0; i < triangles.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Point2d vertex;
                    if (j == 0)
                        vertex = triangles[i].VertexA;
                    else if (j == 1)
                        vertex = triangles[i].VertexB;
                    else
                        vertex = triangles[i].VertexC;

                    MeshVertex mv = new MeshVertex(vertex);
                    int vertexId;

                    if (pointIdAssociation.ContainsKey(mv.Point))
                    {

                    }
                    else
                    {
                        ++progressVertexId;
                        vertexId = mesh.Vertices.Add(mv, progressVertexId);

                        pointIdAssociation.Add(mv.Point, vertexId);
                        pointVertexDict.Add(vertex, mv);
                    }
                }
            }

            int progressEdgeId = 0;
            int progressFaceId = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                Point2d a = triangles[i].VertexA;
                Point2d b = triangles[i].VertexB;
                Point2d c = triangles[i].VertexC;

                mesh.Edges.Add(new MeshEdge(pointIdAssociation[pointVertexDict[a].Point], pointIdAssociation[pointVertexDict[b].Point],
                    ++progressEdgeId));
                mesh.Edges.Add(new MeshEdge(pointIdAssociation[pointVertexDict[b].Point], pointIdAssociation[pointVertexDict[c].Point],
                    ++progressEdgeId));
                mesh.Edges.Add(new MeshEdge(pointIdAssociation[pointVertexDict[c].Point], pointIdAssociation[pointVertexDict[a].Point],
                    ++progressEdgeId));

                mesh.Faces.Add(new MeshFace(pointIdAssociation[pointVertexDict[a].Point], pointIdAssociation[pointVertexDict[b].Point],
                    pointIdAssociation[pointVertexDict[c].Point]), ++progressFaceId);
            }

            return mesh;
        }

        internal static double GetAngle(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            double dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);
            double cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);
            return Math.Atan2(cross_product, dot_product);
        }

        private static double CrossProductLength(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            return (BAx * BCy - BAy * BCx);
        }

        private static double DotProduct(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            return (BAx * BCx + BAy * BCy);
        }

        internal static bool IsPointInPolygon(Point2d[] polygon, Point2d testPoint)
        {
            double total_angle = GetAngle(polygon[polygon.Length - 1].X, polygon[polygon.Length - 1].Y, testPoint.X, testPoint.Y, polygon[0].X, polygon[0].Y);

            for (int i = 0; i < polygon.Length - 1; i++)
                total_angle += GetAngle(polygon[i].X, polygon[i].Y, testPoint.X, testPoint.Y, polygon[i + 1].X, polygon[i + 1].Y);

            return (Math.Abs(total_angle) > 1);
        }
    }
}
