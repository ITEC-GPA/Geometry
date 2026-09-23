using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    public sealed class DelaunayMesh : Mesh
    {
        #region Properties

        public DelaunayGenerateOptions DelaunayMeshOptions => (DelaunayGenerateOptions)_options;

        #endregion

        #region Constructors

        public DelaunayMesh()
            : base()
        {
        }

        private DelaunayMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        public static bool Generate(Shape2d shape, DelaunayGenerateOptions delaunayGenerateOptions, out Mesh mesh, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            if (!CreateConstrainedMesh(shape, delaunayGenerateOptions, out mesh, out generateMeshStatus))
            {
                generateMeshStatus.Exceptions.Add(new Exception("Fail to generate the mesh"));
                return false;
            }

            return true;
        }

        public static bool Generate(IEnumerable<Shape2d> shapes, DelaunayGenerateOptions delaunayGenerateOptions, out List<Mesh> meshes, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            meshes = new List<Mesh>();
            generateMeshStatus = new DelaunayGenerateMeshStatus();

            foreach (Shape2d shape in shapes)
            {
                if (Generate(shape, delaunayGenerateOptions, out Mesh mesh, out generateMeshStatus))
                {
                    meshes.Add(mesh);
                }
                else
                {
                    generateMeshStatus.Exceptions.Add(new Exception("Fail to generate the mesh"));
                    return false;
                }
            }
            return true;
        }

        private static bool CreateConstrainedMesh(Shape2d shape, DelaunayGenerateOptions delaunayGenerateOptions, out Mesh mesh, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            List<Line2d> outerEdges = shape.Fill2d.Explode().ToList();

            List<Line2d>[] innerEdges = null;
            if (shape.HasHoles)
            {
                innerEdges = new List<Line2d>[shape.Holes.Length];
                for (int i = 0; i < shape.Holes.Length; i++)
                {
                    innerEdges[i] = shape.Holes2d[i].Explode().ToList();
                }
            }

            // step : 2 Add the edges vertex to point list
            List<Point2d> points = new List<Point2d>();
            for (int i = 0; i < outerEdges.Count; i++)
                points.Add(new Point2d(outerEdges[i].End.X, outerEdges[i].End.Y) { Tag = points.Count });

            if (innerEdges != null)
                for (int i = 0; i < innerEdges.Length; i++)
                    for (int j = 0; j < innerEdges[i].Count; j++)
                        points.Add(new Point2d(innerEdges[i][j].End.X, innerEdges[i][j].End.Y) { Tag = points.Count });


            List<Line2d> totalEdge = new List<Line2d>();
            totalEdge.AddRange(outerEdges);
            if (innerEdges != null)
                for (int i = 0; i < innerEdges.Length; i++)
                    totalEdge.AddRange(innerEdges[i]);

            int tag = -1;
            if (shape.Tag != null)
                tag = (int)shape.Tag;
            Helper.SurfaceStore sufaceData = new Helper.SurfaceStore(tag, points, totalEdge);

            bool isEncroched = false;
            do
            {
                isEncroched = false;
                List<Point2d> amendNodes = new List<Point2d>();

                EncrochedSegmentRecursiveDivide(ref outerEdges, outerEdges.Count - 1, 0, ref points, ref amendNodes, ref isEncroched);

                if (innerEdges != null)
                    for (int i = 0; i < innerEdges.Length; i++)
                        EncrochedSegmentRecursiveDivide(ref innerEdges[i], innerEdges[i].Count - 1, 0, ref points, ref amendNodes, ref isEncroched);

                points.AddRange(amendNodes);

            } while (isEncroched == true);

            double B_var = Math.Sqrt(2);
            double h_var = delaunayGenerateOptions.MeshSize;

            #region Initial boundary split

            double minEdgeOuter = outerEdges.Select(i => i.Length).Min();
            double[] minEdgeInner = new double[0];
            if (innerEdges != null)
            {
                minEdgeInner = new double[innerEdges.Length];
                for (int i = 0; i < innerEdges.Length; i++)
                {
                    minEdgeInner[i] = innerEdges[i].Select(j => j.Length).Min();
                }
            }

            double minEdgeInnerMin = double.MaxValue;
            if (minEdgeInner.Length > 0)
                minEdgeInnerMin = minEdgeInner.Min();

            double minEdge = Math.Min(minEdgeOuter, minEdgeInnerMin);
            double maxEdgeAdmitted = minEdge * B_var;
            double splitLength = Math.Min(maxEdgeAdmitted, 1.2 * delaunayGenerateOptions.MeshSize);

            SplitLongEdges(outerEdges, points, splitLength);

            if (innerEdges != null)
            {
                for (int j = 0; j < innerEdges.Length; j++)
                    SplitLongEdges(innerEdges[j], points, splitLength);
            }

            #endregion

            // The store belongs to this invocation. A static store made concurrent
            // Generate calls overwrite each other's triangulation state.
            Helper.MeshStore meshStore = new Helper.MeshStore(shape);
            meshStore.AddMultiplePoints(points);
            meshStore.FinalizeMesh();
            int count = 0;

            if (!delaunayGenerateOptions.InitialMeshOnly)
            {
                Helper.TriangleStore[] badTriangles = FindBadTriangles(meshStore.Triangles, sufaceData, outerEdges, innerEdges, B_var, h_var);
                int index = 0;

                while (badTriangles.Length > 0 && count < 10)
                {
                innerPoint:;

                    Point2d innerSurfacePt;

                    if (index < badTriangles.Length)
                    {
                        innerSurfacePt = new Point2d(badTriangles[index].CircleCenter.X, badTriangles[index].CircleCenter.Y) { Tag = -1 };
                        while (!shape.IsPointInside(innerSurfacePt))
                        {
                            index++;
                            goto innerPoint;
                        }
                    }
                    else
                    {
                        innerSurfacePt = new Point2d(badTriangles[0].CircleCenter.X, badTriangles[0].CircleCenter.Y) { Tag = -1 };
                    }

                    index = 0;
                    isEncroched = false;
                    EncrochedSegmentSingleDivide(ref outerEdges, ref innerSurfacePt, h_var, ref isEncroched);

                    if (isEncroched == true)
                    {
                        meshStore.AddSinglePoint(innerSurfacePt);
                        goto loopend;
                    }

                    if (innerEdges != null)
                    {
                        for (int j = 0; j < innerEdges.Length; j++)
                        {
                            isEncroched = false;
                            EncrochedSegmentSingleDivide(ref innerEdges[j], ref innerSurfacePt, h_var, ref isEncroched);

                            if (isEncroched == true)
                            {
                                meshStore.AddSinglePoint(innerSurfacePt);
                                goto loopend;
                            }
                        }
                    }

                    meshStore.AddSinglePoint(innerSurfacePt);

                loopend:;

                    badTriangles = FindBadTriangles(meshStore.Triangles, sufaceData, outerEdges, innerEdges, B_var, splitLength);
                    count++;
                }

                meshStore.FinalizeMesh();
            }

            #region Create Mesh

            sufaceData.MyMesh = new Mesh();
            sufaceData.IsMeshed = true;

            int vertexProgressiveId = 0;
            int edgeProgressiveId = 0;
            int faceProgressiveId = 0;

            Dictionary<Point2d, int> pointVertexAssociation = new Dictionary<Point2d, int>(meshStore.LocalInputPoints.Count);

            for (int i = 0; i < meshStore.LocalInputPoints.Count; i++)
            {
                MeshVertex meshVertex = new MeshVertex(meshStore.LocalInputPoints[i]) { Tag = meshStore.LocalInputPoints[i].Tag };
                sufaceData.MyMesh.Vertices.Add(meshVertex, vertexProgressiveId++);
                pointVertexAssociation.Add(meshStore.LocalInputPoints[i], meshVertex.Id);
            }

            for (int i = 0; i < meshStore.LocalOutputEdges.Count; i++)
            {
                MeshEdge meshEdge = new MeshEdge(pointVertexAssociation[meshStore.LocalOutputEdges[i].Start], pointVertexAssociation[meshStore.LocalOutputEdges[i].End]);
                sufaceData.MyMesh.Edges.Add(meshEdge, edgeProgressiveId++);
            }

            for (int i = 0; i < meshStore.LocalOutputTriangle.Count; i++)
            {
                MeshFace meshFace = new MeshFace(pointVertexAssociation[meshStore.LocalOutputTriangle[i].Vertices[0]],
                    pointVertexAssociation[meshStore.LocalOutputTriangle[i].Vertices[1]],
                    pointVertexAssociation[meshStore.LocalOutputTriangle[i].Vertices[2]]);
                sufaceData.MyMesh.Faces.Add(meshFace, faceProgressiveId++);
            }

            #endregion

            mesh = sufaceData.MyMesh;

            if (mesh != null)
            {
                if (delaunayGenerateOptions.Refine)
                    mesh.Refine();
            }

            generateMeshStatus = null;
            return true;
        }

        private static void EncrochedSegmentRecursiveDivide(ref List<Line2d> surfaceEdges, int endIndex, int startIndex, ref List<Point2d> allNodes,
            ref List<Point2d> amendNodes, ref bool toContinue)
        {
            for (int i = endIndex; i >= startIndex; i--)
            {
                Point2d cicleCenter = new Point2d(surfaceEdges[i].Mid.X, surfaceEdges[i].Mid.Y) { Tag = -1 };
                double circleRadius = (surfaceEdges[i].Length * 0.5) - 0.1;

                for (int j = 0; j < allNodes.Count; j++)
                {
                    Point2d pt = allNodes[j];
                    if (Math.Sqrt(Math.Pow(pt.X - cicleCenter.X, 2) + Math.Pow(pt.Y - cicleCenter.Y, 2)) < circleRadius)
                    {
                        Point2d splitPoint = new Point2d(surfaceEdges[i].Mid.X, surfaceEdges[i].Mid.Y) { Tag = allNodes.Count + amendNodes.Count };
                        List<Line2d> twoSegments = surfaceEdges.FindAll(obj => obj.Start.Equals(pt) || obj.End.Equals(pt));

                        if (twoSegments.Count != 0)
                        {
                            Point2d otherPt1 = twoSegments[0].Start.Equals(pt) == true ? twoSegments[0].End : twoSegments[0].Start;
                            Point2d otherPt2 = twoSegments[twoSegments.Count - 1].End.Equals(pt) == true ? twoSegments[twoSegments.Count - 1].Start : twoSegments[twoSegments.Count - 1].End;

                            if (surfaceEdges[i].VertexExists(otherPt1) == true || surfaceEdges[i].VertexExists(otherPt2) == true)
                            {
                                Point2d apexPoint;
                                Point2d nonApexVertexSeg1;
                                Point2d nonApexVertexSeg2;
                                double splitLength;
                                double tParam;

                                if (surfaceEdges[i].VertexExists(otherPt1) == true)
                                {
                                    apexPoint = otherPt1;
                                    nonApexVertexSeg1 = surfaceEdges[i].Start.Equals(apexPoint) == true ? surfaceEdges[i].End : surfaceEdges[i].Start;
                                    nonApexVertexSeg2 = twoSegments[0].Start.Equals(apexPoint) == true ? twoSegments[0].End : twoSegments[0].Start;
                                    splitLength = twoSegments[0].Length;
                                }
                                else
                                {
                                    apexPoint = otherPt2;
                                    nonApexVertexSeg1 = surfaceEdges[i].Start.Equals(apexPoint) == true ? surfaceEdges[i].End : surfaceEdges[i].Start;
                                    nonApexVertexSeg2 = twoSegments[0].Start.Equals(apexPoint) == true ? twoSegments[twoSegments.Count - 1].End : twoSegments[twoSegments.Count - 1].Start;
                                    splitLength = twoSegments[twoSegments.Count - 1].Length;
                                }

                                if (Helper.GetAngle(nonApexVertexSeg1.X, nonApexVertexSeg1.Y, apexPoint.X, apexPoint.Y, nonApexVertexSeg2.X, nonApexVertexSeg2.Y) < 0.5236)
                                {
                                    tParam = splitLength / surfaceEdges[i].Length;
                                    if (tParam > 0.5f)
                                    {
                                        double splitX = apexPoint.X * (1 - tParam) + (nonApexVertexSeg1.X * tParam);
                                        double splitY = apexPoint.Y * (1 - tParam) + (nonApexVertexSeg1.Y * tParam);

                                        splitPoint = new Point2d(splitX, splitY) { Tag = allNodes.Count + amendNodes.Count };
                                    }
                                }

                            }
                        }

                        Line2d seg1 = new Line2d(surfaceEdges[i].Start, splitPoint) { Tag = surfaceEdges[i].Tag };
                        Line2d seg2 = new Line2d(splitPoint, surfaceEdges[i].End) { Tag = surfaceEdges.Count };
                        amendNodes.Add(new Point2d(splitPoint.X, splitPoint.Y) { Tag = allNodes.Count + amendNodes.Count });

                        surfaceEdges.RemoveAt(i);

                        surfaceEdges.Insert(i, seg1);
                        surfaceEdges.Insert(i + 1, seg2);
                        toContinue = true;

                        EncrochedSegmentRecursiveDivide(ref surfaceEdges, i + 1, i, ref allNodes, ref amendNodes, ref toContinue);

                        break;
                    }
                }
            }
        }

        private static void EncrochedSegmentSingleDivide(ref List<Line2d> surfaceEdges, ref Point2d circumCenterPoint, double meansize, ref bool toContinue)
        {
            for (int i = surfaceEdges.Count - 1; i >= 0; i--)
            {
                Point2d circleCenter = new Point2d(surfaceEdges[i].Mid.X, surfaceEdges[i].Mid.Y) { Tag = -1 };
                double circleRadius = (surfaceEdges[i].Length * 0.5) - 0.1;

                if (Math.Sqrt(Math.Pow((circumCenterPoint.X - circleCenter.X), 2) + Math.Pow((circumCenterPoint.Y - circleCenter.Y), 2)) < circleRadius)
                {
                    Point2d c_pt = circumCenterPoint;
                    if (surfaceEdges.Exists(obj => obj.Start.Equals(c_pt) || obj.End.Equals(c_pt)) == true)
                    {
                        List<Line2d> twoSegments = surfaceEdges.FindAll(obj => obj.Start.Equals(c_pt) || obj.End.Equals(c_pt));
                        Point2d otherPt1 = twoSegments[0].Start.Equals(c_pt) == true ? twoSegments[0].End : twoSegments[0].Start;
                        Point2d otherPt2 = twoSegments[twoSegments.Count - 1].End.Equals(c_pt) == true ? twoSegments[twoSegments.Count - 1].Start : twoSegments[twoSegments.Count - 1].End;

                        if (surfaceEdges[i].VertexExists(otherPt1) == true || surfaceEdges[i].VertexExists(otherPt2) == true)
                        {
                            Point2d apexPoint, nonApexVertexSeg1, nonApexVertexSeg2;
                            double splitLength, tParam;

                            if (surfaceEdges[i].VertexExists(otherPt1) == true)
                            {
                                apexPoint = otherPt1;
                                nonApexVertexSeg1 = surfaceEdges[i].Start.Equals(apexPoint) == true ? surfaceEdges[i].End : surfaceEdges[i].Start;
                                nonApexVertexSeg2 = twoSegments[0].Start.Equals(apexPoint) == true ? twoSegments[0].End : twoSegments[0].Start;
                                splitLength = twoSegments[0].Length;
                            }
                            else
                            {
                                apexPoint = otherPt2;
                                nonApexVertexSeg1 = surfaceEdges[i].Start.Equals(apexPoint) == true ? surfaceEdges[i].End : surfaceEdges[i].Start;
                                nonApexVertexSeg2 = twoSegments[0].Start.Equals(apexPoint) == true ? twoSegments[twoSegments.Count - 1].End : twoSegments[twoSegments.Count - 1].Start;
                                splitLength = twoSegments[twoSegments.Count - 1].Length;
                            }

                            if (Helper.GetAngle(nonApexVertexSeg1.X, nonApexVertexSeg1.Y, apexPoint.X, apexPoint.Y, nonApexVertexSeg2.X, nonApexVertexSeg2.Y) < 0.5236)
                            {
                                tParam = splitLength / surfaceEdges[i].Length;
                                if (tParam > 0.5f)
                                {
                                    double split_x = apexPoint.X * (1 - tParam) + (nonApexVertexSeg1.X * tParam);
                                    double split_y = apexPoint.Y * (1 - tParam) + (nonApexVertexSeg1.Y * tParam);

                                    circumCenterPoint = new Point2d(split_x, split_y) { Tag = -1 };

                                    Line2d tseg_1 = new Line2d(surfaceEdges[i].Start, circumCenterPoint) { Tag = surfaceEdges[i].Tag }; // create a segment 1 with start_pt to mid_pt
                                    Line2d tseg_2 = new Line2d(circumCenterPoint, surfaceEdges[i].End) { Tag = surfaceEdges.Count }; // create a segment 2 with mid_pt to end_pt

                                    surfaceEdges.RemoveAt(i);

                                    surfaceEdges.Insert(i, tseg_1);
                                    surfaceEdges.Insert(i + 1, tseg_2);
                                    toContinue = true;

                                    // break is very important to avoid no longer continuing the search for the pt encroching i_th index segment
                                    break;
                                }
                                else
                                {
                                    // the ratio of length is not more than half which means the longest edge is twice the length of smallest edge
                                    goto half_split;
                                }
                            }
                            else
                            {
                                // Angle is not less than 30 degree so need to impose the split
                                goto half_split;
                            }
                        }
                        else
                        {
                            // No apex point
                            // which means no relation to the circum center point so bisect this edge into half
                            // create two segements from one segment by splitting at the mid point
                            goto half_split;

                        }
                    }

                half_split:;
                    // the vertex point is actualy a circle center
                    // create two segements from one segment by splitting at the mid point
                    Line2d segment1 = new Line2d(surfaceEdges[i].Start, surfaceEdges[i].Mid) { Tag = surfaceEdges[i].Tag }; // create a segment 1 with start_pt to mid_pt
                    Line2d segment2 = new Line2d(surfaceEdges[i].Mid, surfaceEdges[i].End) { Tag = surfaceEdges.Count }; // create a segment 2 with mid_pt to end_pt
                    circumCenterPoint = new Point2d(surfaceEdges[i].Mid.X, surfaceEdges[i].Mid.Y) { Tag = -1 };

                    surfaceEdges.RemoveAt(i); // remove the edge which encroches

                    // Insert the newly created two element edge list to the main list (at the removed index)
                    surfaceEdges.Insert(i, segment1);
                    surfaceEdges.Insert(i + 1, segment2);
                    toContinue = true;

                    // break is very important to avoid no longer continuing the search for the pt encroching i_th index segment
                    break;
                }
            }
        }

        private static Helper.TriangleStore[] FindBadTriangles(List<Helper.TriangleStore> triangles, Helper.SurfaceStore surface,
            List<Line2d> outerEdges, List<Line2d>[] innerEdges, double B, double h)
        {
            HashSet<Line2d> outerEdgeSet = new HashSet<Line2d>(outerEdges);
            HashSet<Line2d>[] innerEdgeSets = innerEdges == null
                ? null
                : innerEdges.Select(edges => new HashSet<Line2d>(edges)).ToArray();
            List<Helper.TriangleStore> result = new List<Helper.TriangleStore>();

            for (int i = 0; i < triangles.Count; i++)
            {
                if (TriangleAngleSizeConstraint(surface, outerEdgeSet, innerEdgeSets, triangles[i], B, h))
                    result.Add(triangles[i]);
            }

            return result.ToArray();
        }

        private static bool TriangleAngleSizeConstraint(Helper.SurfaceStore surface, HashSet<Line2d> outerEdges,
            HashSet<Line2d>[] innerEdges, Helper.TriangleStore gradedTriangle, double B, double h)
        {
            if (surface.PointInSurface(gradedTriangle.ShrunkVertices[0]) == true &&
                surface.PointInSurface(gradedTriangle.ShrunkVertices[1]) == true &&
                surface.PointInSurface(gradedTriangle.ShrunkVertices[2]) == true)
            {
                if (gradedTriangle.RadiusShortestEdgeRatio > B)
                {
                    // condition 1: B parameter => A triangle is well-shaped if all its angles are greater than or equal to 30 degrees
                    // ---------------- Small Angle Input Case ----------------------------------- Very Expensive
                    // Test whether the small angle is due to input from user

                    Line2d line1 = new Line2d(gradedTriangle.VertexA.Point, gradedTriangle.VertexB.Point);
                    Line2d line2 = new Line2d(gradedTriangle.VertexB.Point, gradedTriangle.VertexC.Point);
                    Line2d line3 = new Line2d(gradedTriangle.VertexC.Point, gradedTriangle.VertexA.Point);

                    if (ContainsAtLeastTwo(outerEdges, line1, line2, line3))
                        return false;

                    if (innerEdges != null)
                    {
                        for (int i = 0; i < innerEdges.Length; i++)
                            if (ContainsAtLeastTwo(innerEdges[i], line1, line2, line3))
                                return false;
                    }

                    return true;
                }
                else if (gradedTriangle.LongestEdge > 1.5 * h)
                {
                    return true;
                }
                else if (gradedTriangle.CircleRadius > h && gradedTriangle.ShortestEdge > h)
                {
                    // condition 2: h parameter => A triangle is well-sized if it satisfies a user - supplied grading function
                    return true;
                }
                else
                {
                    // Conditions are not met
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static bool ContainsAtLeastTwo(HashSet<Line2d> edges, Line2d line1, Line2d line2, Line2d line3)
        {
            int matches = edges.Contains(line1) ? 1 : 0;
            if (edges.Contains(line2) && ++matches == 2)
                return true;
            return edges.Contains(line3) && ++matches == 2;
        }

        private static void SplitLongEdges(List<Line2d> edges, List<Point2d> points, double splitLength)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Length <= splitLength)
                    continue;

                SplitAndAdd(ref edges, ref points, i);
                // Recheck the first half at the same index; it may still be too long.
                i--;
            }
        }

        private static void SplitAndAdd(ref List<Line2d> edges, ref List<Point2d> points, int i)
        {
            edges[i].Split(2, out Line2d[] lines);
            points.Add(new Point2d(lines[0].End.X, lines[0].End.Y) { Tag = points.Count });
            edges.RemoveAt(i);

            edges.Insert(i, lines[0]);
            edges.Insert(i + 1, lines[1]);
        }

        [Serializable]
        public sealed class DelaunayGenerateOptions : GenerateOptions, ICloneable
        {
            public bool InitialMeshOnly;

            public DelaunayGenerateOptions()
            {
                MeshSize = 1E+22;
                Recombine = true;
                Refine = false;
                InitialMeshOnly = false;
            }

            public override object Clone()
            {
                DelaunayGenerateOptions clone = new DelaunayGenerateOptions
                {
                    MeshSize = MeshSize,
                    Recombine = Recombine,
                    Refine = Refine,
                    InitialMeshOnly = InitialMeshOnly,
                };

                return clone;
            }
        }

        [Serializable]
        public sealed class DelaunayGenerateMeshStatus : GenerateMeshStatus
        {
            public DelaunayGenerateMeshStatus()
                : base()
            {

            }
        }
    }
}
