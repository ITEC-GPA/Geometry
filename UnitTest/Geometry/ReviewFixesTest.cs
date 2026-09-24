using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using GPC.Geometry.Collections;
using GPC.Geometry.Meshes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    /// <summary>
    /// Regression tests for the bugs found in the code review of September 2026
    /// </summary>
    [TestClass]
    public class ReviewFixesTest
    {
        private const double Tol = 1e-4;

        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        private static Polygon3d Square(double x, double y, double size)
        {
            return new Polygon3d(new[] { P(x, y), P(x + size, y), P(x + size, y + size), P(x, y + size) });
        }

        #region Equals(object) without infinite recursion

        [TestMethod]
        public void EqualsObjectWithOtherTypesOrNullReturnsFalse()
        {
            object other = "not a geometry";

            Assert.IsFalse(new Vector3d(1, 2, 3).Equals(other));
            Assert.IsFalse(new Vector3d(1, 2, 3).Equals((object)null));
            Assert.IsFalse(new Point2d(1, 2).Equals(other));
            Assert.IsFalse(new Point2d(1, 2).Equals((object)null));
            Assert.IsFalse(new Vector2d(1, 2).Equals(other));
            Assert.IsFalse(new Plane(P(0, 0), P(1, 0), P(0, 1)).Equals(other));
            Assert.IsFalse(new Plane(P(0, 0), P(1, 0), P(0, 1)).Equals((object)null));
            Assert.IsFalse(new Circle2d(new Point2d(0, 0), 1).Equals(other));
            Assert.IsFalse(new Circle3d(P(1, 0), P(0, 1), P(-1, 0)).Equals(other));
            Assert.IsFalse(new Circle3dArc(P(1, 0), P(0, 1), P(-1, 0), Tol).Equals(other));
            Assert.IsFalse(new MeshVertex(P(0, 0)).Equals(other));
            Assert.IsFalse(new MeshEdge(1, 2).Equals(other));
            Assert.IsFalse(new MeshFace(1, 2, 3).Equals(other));
        }

        [TestMethod]
        public void EqualsObjectWithSameType()
        {
            Assert.IsTrue(new Vector3d(1, 2, 3).Equals((object)new Vector3d(1, 2, 3)));
            Assert.IsTrue(new Point2d(1, 2).Equals((object)new Point2d(1, 2)));
            Assert.IsTrue(new Circle2d(new Point2d(0, 0), 1).Equals((object)new Circle2d(new Point2d(0, 0), 1)));

            var arc1 = new Circle3dArc(P(1, 0), P(0, 1), P(-1, 0), Tol);
            var arc2 = new Circle3dArc(P(1, 0), P(0, 1), P(-1, 0), Tol);
            Assert.IsTrue(arc1.Equals((object)arc2));
        }

        [TestMethod]
        public void EqualityOperatorsWithNull()
        {
            Vector3d v = null;
            Point2d p = null;
            Assert.IsTrue(v == null);
            Assert.IsFalse(new Vector3d(1, 0, 0) == v);
            Assert.IsFalse(v == new Vector3d(1, 0, 0));
            Assert.IsFalse(p == new Point2d(1, 0));
        }

        [TestMethod]
        public void MeshEdgeEqualsNeedsSameId()
        {
            var e1 = new MeshEdge(1, 2);
            var e2 = new MeshEdge(2, 1);
            Assert.IsTrue(e1.Equals(e2), "Same (unset) id and reversed nodes");
            Assert.IsTrue(e1.EqualsWithoutId(e2));

            var mesh = new Mesh();
            mesh.Edges.Add(e1);
            mesh.Edges.Add(new MeshEdge(2, 1));
            Assert.IsFalse(mesh.Edges[0].Equals(mesh.Edges[1]), "Different ids");
            Assert.IsTrue(mesh.Edges[0].EqualsWithoutId(mesh.Edges[1]));
        }

        #endregion

        #region Point3d, Ray3d, Plane

        [TestMethod]
        public void Point3dEqualsTolerance()
        {
            Assert.IsTrue(P(0, 0).Equals(P(1e-4, 0)), "Within the combined tolerance sqrt(2) * 1e-4");
            Assert.IsFalse(P(0, 0).Equals(P(0.011, 0)), "Before the fix points 0.011 apart were equal");
            Assert.IsFalse(P(0, 0).Equals(P(2e-4, 0)));
        }

        [TestMethod]
        public void Point3dScaleRespectToCenter()
        {
            Point3d scaled = P(10, 10, 10).Scale(P(0, 0, 0), 2.0, 2.0, 2.0);
            Assert.AreEqual(20.0, scaled.X, 1e-12);
            Assert.AreEqual(20.0, scaled.Y, 1e-12);
            Assert.AreEqual(20.0, scaled.Z, 1e-12);

            scaled = P(3, 5, 7).Scale(P(1, 1, 1), 2.0);
            Assert.AreEqual(5.0, scaled.X, 1e-12);
            Assert.AreEqual(9.0, scaled.Y, 1e-12);
            Assert.AreEqual(13.0, scaled.Z, 1e-12);
        }

        [TestMethod]
        public void Point3dTryParseInvalidString()
        {
            Assert.IsFalse(Point3d.TryParse("a" + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + "b", out _));
            Assert.IsFalse(Point2d.TryParse("a" + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + "b", out _));
        }

        [TestMethod]
        public void RayIsPointOnRayUsesTheDistance()
        {
            var line = new Line3d(P(0, 0, 0), P(1000, 1000, 1000));
            Assert.IsFalse(line.IsPointOnInfiniteLine(P(500, 520, 480)), "Point 28 mm far from the line");
            Assert.IsTrue(line.IsPointOnInfiniteLine(P(2000, 2000, 2000)));
            Assert.IsTrue(line.IsPointOnInfiniteLine(P(500, 500 + 5e-5, 500)));
            Assert.IsFalse(line.IsPointOnInfiniteLine(P(500, 500 + 1e-3, 500)));

            var ray = new Ray3d(P(0, 0, 0), new Vector3d(1e-3, 0, 0)); // the result must not depend on the length of the direction
            Assert.IsTrue(ray.IsPointOnRay(P(5000, 5e-5, 0)));
            Assert.IsFalse(ray.IsPointOnRay(P(5000, 1e-3, 0)));
        }

        [TestMethod]
        public void RayCloneIsDeep()
        {
            var ray = new Ray3d(P(0, 0, 0), new Vector3d(1, 0, 0));
            var clone = (Ray3d)ray.Clone();
            clone.Move(5, 0, 0);
            Assert.AreEqual(0.0, ray.Point.X);
            Assert.AreEqual(5.0, clone.Point.X);
        }

        [TestMethod]
        public void PlaneFromEquation()
        {
            var plane = new Plane(1, 1, 0, -5);
            Assert.IsFalse(double.IsInfinity(plane.Origin.X) || double.IsInfinity(plane.Origin.Y) || double.IsInfinity(plane.Origin.Z));
            Assert.AreEqual(1.0, plane.Normal.Length, 1e-12);
            Assert.IsTrue(plane.IsPointOnPlane(P(5, 0)));
            Assert.IsTrue(plane.IsPointOnPlane(P(2.5, 2.5, 7)));
            Assert.AreEqual(1.0, plane.DistanceToPlane(P(5 + Math.Sqrt(0.5), Math.Sqrt(0.5))), 1e-9);

            Assert.ThrowsException<ArgumentException>(() => new Plane(0, 0, 0, 1));
        }

        [TestMethod]
        public void PlaneDoesNotModifyTheVectorsOfTheCaller()
        {
            var normal = new Vector3d(0, 0, 10);
            new Plane(P(0, 0), normal);
            Assert.AreEqual(10.0, normal.Z);

            var x = new Vector3d(10, 0, 0);
            var y = new Vector3d(10, 10, 0); // not orthogonal
            var plane = new Plane(P(0, 0), x, y);
            Assert.AreEqual(10.0, x.X);
            Assert.AreEqual(1.0, plane.Normal.Length, 1e-12);
            Assert.IsTrue(plane.IsPointOnPlane(P(3, 7)));
            Assert.IsFalse(plane.IsPointOnPlane(P(3, 7, 0.01)));
        }

        #endregion

        #region CoordinateSystem

        [TestMethod]
        public void RotateV3KeepsTheV3Axis()
        {
            var cs = new CoordinateSystem(P(0, 0, 0), P(1, 0, 0), P(0, 1, 1));
            var v3 = new Vector3d(cs.V3);

            cs.RotateV3(Math.PI / 2);

            Assert.IsTrue(cs.V3.Equals(v3, 1e-9), "V3 must not change rotating around V3");
            Assert.AreEqual(0.0, cs.V1.DotProduct(cs.V2), 1e-12);
            Assert.AreEqual(0.0, cs.V1.DotProduct(v3), 1e-12);
        }

        [TestMethod]
        public void RotateV3OfGlobalSystemIsRotateZ()
        {
            var cs1 = new CoordinateSystem(P(0, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            var cs2 = new CoordinateSystem(P(0, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            cs1.RotateV3(0.3);
            cs2.RotateZ(0.3);
            Assert.IsTrue(cs1.V1.Equals(cs2.V1, 1e-12));
            Assert.IsTrue(cs1.V2.Equals(cs2.V2, 1e-12));
            Assert.IsTrue(cs1.V3.Equals(cs2.V3, 1e-12));
        }

        [TestMethod]
        public void RotateV1AroundTiltedAxis()
        {
            var cs = new CoordinateSystem(P(0, 0, 0), P(1, 1, 0), P(-1, 1, 0));
            var v1 = new Vector3d(cs.V1);
            var v2 = new Vector3d(cs.V2);
            var v3 = new Vector3d(cs.V3);

            cs.RotateV1(Math.PI / 2);

            Assert.IsTrue(cs.V1.Equals(v1, 1e-9));
            Assert.IsTrue(cs.V2.Equals(v3, 1e-9), "V2 goes to V3");
            Assert.IsTrue(cs.V3.Equals(Vector3d.Reverse(v2), 1e-9), "V3 goes to -V2");
        }

        [TestMethod]
        public void ToLocalShapeKeepsHolesAndChilds()
        {
            var child = new Shape(Square(45, 45, 10), null, null, Tol);
            var shape = new Shape(Square(0, 0, 100), new[] { Square(40, 40, 20) }, new[] { child }, Tol);

            Shape2d local = CoordinateSystem.Global.ToLocal(shape);

            Assert.IsTrue(local.HasHoles);
            Assert.AreEqual(1, local.Holes.Length);
            Assert.IsTrue(local.HasChilds);
            Assert.AreEqual(9700.0, local.GetArea(Tol), 1e-6);
        }

        #endregion

        #region Polygon3d

        private static Polygon3d VerticalL()
        {
            // L-shape in the XZ plane: same polygon of HorizontalL rotated
            return new Polygon3d(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 0, 2), P(2, 0, 2), P(2, 0, 10), P(0, 0, 10) });
        }

        private static Polygon3d HorizontalL()
        {
            return new Polygon3d(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 2, 0), P(2, 2, 0), P(2, 10, 0), P(0, 10, 0) });
        }

        [TestMethod]
        public void CentroidOfConcaveVerticalPolygon()
        {
            double expected = 116.0 / 36.0; // (10*2*5 + 2*8*2 ... ) / 36 = 3.2222

            Point3d ch = HorizontalL().GetCentroid();
            Assert.AreEqual(expected, ch.X, 1e-9);
            Assert.AreEqual(expected, ch.Y, 1e-9);

            Point3d cv = VerticalL().GetCentroid();
            Assert.AreEqual(expected, cv.X, 1e-9);
            Assert.AreEqual(0.0, cv.Y, 1e-9);
            Assert.AreEqual(expected, cv.Z, 1e-9);

            // clockwise order gives the same centroid
            Point3d cr = new Polygon3d(VerticalL()).Reverse().GetCentroid();
            Assert.AreEqual(expected, cr.X, 1e-9);
            Assert.AreEqual(expected, cr.Z, 1e-9);
        }

        [TestMethod]
        public void SignedArea()
        {
            Assert.AreEqual(36.0, HorizontalL().GetSignedArea(), 1e-9);
            Assert.AreEqual(-36.0, new Polygon3d(HorizontalL()).Reverse().GetSignedArea(), 1e-9);
            Assert.AreEqual(36.0, Math.Abs(VerticalL().GetSignedArea()), 1e-9);

            // tilted concave polygon whose first three points turn the "wrong" way
            var tilted = new Polygon3d(new[] { P(2, 2, 2), P(2, 10, 10), P(0, 10, 10), P(0, 0, 0), P(10, 0, 0), P(10, 2, 2) });
            Assert.AreEqual(36.0 * Math.Sqrt(2), Math.Abs(tilted.GetSignedArea()), 1e-9);
        }

        [TestMethod]
        public void IsConvexInAnyPlane()
        {
            Assert.IsFalse(HorizontalL().IsConvex());
            Assert.IsFalse(VerticalL().IsConvex(), "Before the fix the check used only X and Y");
            Assert.IsTrue(new Polygon3d(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 0, 10), P(0, 0, 10) }).IsConvex());
            Assert.IsTrue(Square(0, 0, 10).IsConvex());
        }

        [TestMethod]
        public void ReverseIsInPlaceAndReturnsTheSamePolygon()
        {
            Polygon3d square = Square(0, 0, 10);
            Polygon3d reversed = square.Reverse();

            Assert.AreSame(square, reversed);
            Assert.IsTrue(square[0].Equals(P(0, 10)));
            Assert.IsTrue(square[3].Equals(P(0, 0)));

            Polygon3d copy = new Polygon3d(square).Reverse();
            Assert.IsTrue(square[0].Equals(P(0, 10)), "Reversing a copy does not change the original");
            Assert.IsTrue(copy[0].Equals(P(0, 0)));
        }

        [TestMethod]
        public void TransformPointLineAndPolygon()
        {
            Polygon3d basePolygon = Square(0, 0, 10);
            Polygon3d newPolygon = Square(100, 0, 10);

            Point3d point = basePolygon.Transform(P(3, 4), newPolygon, Tol);
            Assert.IsTrue(point.Equals(P(103, 4)));

            Line3d line = basePolygon.Transform(new Line3d(P(3, 4), P(6, 5)), newPolygon, Tol);
            Assert.IsTrue(line.Start.Equals(P(103, 4)));
            Assert.IsTrue(line.End.Equals(P(106, 5)));

            Polygon3d polygon = basePolygon.Transform(Square(2, 2, 3), newPolygon, Tol);
            Assert.AreEqual(4, polygon.Count);
            Assert.IsTrue(polygon[0].Equals(P(102, 2)));
            Assert.IsTrue(polygon[2].Equals(P(105, 5)));
        }

        #endregion

        #region Shape

        [TestMethod]
        public void DifferenceKeepsTheHoles()
        {
            var shape = new Shape(Square(0, 0, 100), new[] { Square(40, 40, 20) }, null, Tol);
            var cut = new Shape(Square(0, 0, 10), null, null, Tol);

            Assert.IsTrue(Shape.Difference(shape, cut, out Shape[] result, Tol));
            Assert.AreEqual(9500.0, result.Sum(s => s.GetArea(Tol)), 1e-6);
            Assert.IsTrue(result.Any(s => s.HasHoles));
        }

        [TestMethod]
        public void UnionAndXorWithEmptyResult()
        {
            var a = new Shape(Square(0, 0, 10), null, null, Tol);
            var b = new Shape(Square(0, 0, 10), null, null, Tol);

            Assert.IsTrue(Shape.NotIntersection(a, b, out Shape[] xor, Tol));
            Assert.AreEqual(0, xor.Length);
        }

        [TestMethod]
        public void CloneKeepsTheChilds()
        {
            var child = new Shape(Square(45, 45, 10), null, null, Tol);
            var shape = new Shape(Square(0, 0, 100), new[] { Square(40, 40, 20) }, new[] { child }, Tol);

            var clone = (Shape)shape.Clone();

            Assert.IsTrue(clone.HasChilds);
            Assert.AreEqual(shape.GetArea(Tol), clone.GetArea(Tol), 1e-9);
            Assert.AreNotSame(shape.Childs[0], clone.Childs[0]);
        }

        [TestMethod]
        public void Shape2dCloneKeepsShape2dChilds()
        {
            var child = new Shape2d(new Polygon2d(new[] { new Point2d(45, 45), new Point2d(55, 45), new Point2d(55, 55), new Point2d(45, 55) }));
            var shape = new Shape2d(new Polygon2d(new[] { new Point2d(0, 0), new Point2d(100, 0), new Point2d(100, 100), new Point2d(0, 100) }), null, new[] { child });

            var clone = (Shape2d)shape.Clone();

            Assert.AreEqual(1, clone.Childs2d.Length);
            Assert.IsNull(clone.Holes2d);
        }

        [TestMethod]
        public void ReverseShapeWithHoles()
        {
            var shape = new Shape(Square(0, 0, 100), new[] { Square(40, 40, 20) }, null, Tol);
            double area = shape.GetArea(Tol);
            bool rightHand = shape.Fill.IsRightHandOrdered();

            Shape reversed = shape.Reverse();

            Assert.AreSame(shape, reversed);
            Assert.AreNotEqual(rightHand, shape.Fill.IsRightHandOrdered());
            Assert.AreEqual(area, shape.GetArea(Tol), 1e-9);
        }

        #endregion

        #region Lines, Polygon2d, arcs

        [TestMethod]
        public void Line2dIsPointOnLine()
        {
            var line = new Line2d(new Point2d(0, 0), new Point2d(1000, 1000));
            Assert.IsTrue(line.IsPointOnLine(P(500, 500 + 5e-5)));
            Assert.IsFalse(line.IsPointOnLine(P(500, 500 + 1e-3)));
            Assert.IsFalse(line.IsPointOnLine(P(1100, 1100)), "Outside the segment");
            Assert.IsTrue(line.IsPointOnLine(P(1000, 1000)));
        }

        [TestMethod]
        public void Line3dSplitWithParameterCloseToOne()
        {
            var line = new Line3d(P(0, 0), P(10, 0));
            Line3d[] lines = line.Split(new[] { 0.5, 1.0 - 1e-6 });
            Assert.AreEqual(2, lines.Length);
            Assert.IsTrue(lines.All(l => l.Length > 1));
        }

        [TestMethod]
        public void Polygon2dIsPointInsideConcave()
        {
            // U-shape
            var u = new Polygon2d(new[] { new Point2d(0, 0), new Point2d(30, 0), new Point2d(30, 30), new Point2d(20, 30),
                new Point2d(20, 10), new Point2d(10, 10), new Point2d(10, 30), new Point2d(0, 30) });

            Assert.IsTrue(u.IsPointInside(new Point2d(5, 20)));
            Assert.IsTrue(u.IsPointInside(new Point2d(25, 20)));
            Assert.IsFalse(u.IsPointInside(new Point2d(15, 20)), "Inside the notch");
            Assert.IsTrue(u.IsPointInside(new Point2d(15, 5)));
            Assert.IsFalse(u.IsPointInside(new Point2d(-5, 10)), "The horizontal ray passes through the vertex (10, 10)");
            Assert.IsTrue(u.IsPointInside(new Point2d(5, 10)), "The horizontal ray passes through vertices (10,10) and (20,10)");
            Assert.IsTrue(u.IsPointInside(new Point2d(20, 20)), "On the border");
            Assert.IsTrue(u.IsPointInside(new Point2d(10, 10)), "On a vertex");
            Assert.IsFalse(u.IsPointInside(new Point2d(35, 30)));

            // deterministic
            for (int i = 0; i < 20; i++)
                Assert.IsFalse(u.IsPointInside(new Point2d(15, 20)));
        }

        [TestMethod]
        public void ArcLength()
        {
            var arc = new Circle3dArc(P(10, 0), P(0, 10), P(0, 0)); // start, end, center: quarter of circle, radius 10
            Assert.AreEqual(Math.PI * 5.0, arc.GetLenght(), 1e-9);
        }

        [TestMethod]
        public void Circle3dSerializationKeepsTheCenter()
        {
            var circle = new Circle3d(P(10, 0, 5), P(0, 10, 5), P(-10, 0, 5));
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, circle);
                stream.Position = 0;
                var copy = (Circle3d)formatter.Deserialize(stream);
                Assert.IsTrue(copy.Center.Equals(P(0, 0, 5)));
                Assert.AreEqual(10.0, copy.Radius, 1e-9);
            }
        }

        [TestMethod]
        public void Circle3dCloneIsDeep()
        {
            var circle = new Circle3d(P(10, 0), P(0, 10), P(-10, 0));
            var clone = (Circle3d)circle.Clone();
            clone.Move(0, 0, 5);
            Assert.AreEqual(0.0, circle.Center.Z, 1e-12);
            Assert.AreEqual(5.0, clone.Center.Z, 1e-12);
        }

        #endregion

        #region Mesh

        [TestMethod]
        public void AddVolumeMeshTopEdges()
        {
            var mesh = new Mesh();
            var vertices = new[] { P(0, 0, 0), P(1, 0, 0), P(1, 1, 0), P(0, 1, 0), P(0, 0, 1), P(1, 0, 1), P(1, 1, 1), P(0, 1, 1) }
                .Select(p => new MeshVertex(p)).ToArray();
            mesh.AddVolumeMesh(vertices);

            Assert.AreEqual(12, mesh.EdgesCount);
            foreach (var edge in mesh.Edges)
            {
                Point3d a = mesh.GetVertex(edge.A).Point;
                Point3d b = mesh.GetVertex(edge.B).Point;
                Assert.AreEqual(1.0, a.DistanceTo(b), 1e-12, "All the edges of a unit cube have unit length");
            }
        }

        [TestMethod]
        public void ExtrudeFacesSharesTheTopVertices()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(1, 0), P(1, 1), P(0, 1) });
            mesh.AddFaceMesh(new[] { P(1, 0), P(2, 0), P(2, 1), P(1, 1) });

            Mesh volumes = mesh.ExtrudeFaces(new Vector3d(0, 0, 1));

            Assert.AreEqual(12, volumes.VerticesCount);
            Assert.AreEqual(2, volumes.VolumesCount);
            Assert.AreEqual(20, volumes.EdgesCount);

            int[] v1 = volumes.Volumes[0].GetNodes();
            int[] v2 = volumes.Volumes[1].GetNodes();
            Assert.AreEqual(4, v1.Intersect(v2).Count(), "Two adjacent volumes share four vertices");
        }

        [TestMethod]
        public void RefineMeshKeepsTheZ()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0, 0), P(10, 0, 0), P(0, 0, 10) });

            Mesh refined = Mesh.RefineMesh(mesh);

            Assert.AreEqual(6, refined.VerticesCount);
            Assert.AreEqual(4, refined.FacesCount);
            Assert.IsTrue(refined.GetVertices().Any(v => v.Point.Equals(P(0, 0, 5))));
            Assert.IsTrue(refined.GetVertices().Any(v => v.Point.Equals(P(5, 0, 5))));
            Assert.AreEqual(50.0, refined.GetFaces().Sum(f => refined.GetFaceArea(f)), 1e-9);
        }

        [TestMethod]
        public void CutTriangleThroughAVertex()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(5, 10) });

            mesh.Cut(new Line2d(new Point2d(5, -5), new Point2d(5, 20)));

            Assert.AreEqual(2, mesh.FacesCount);
            Assert.AreEqual(50.0, mesh.GetFaces().Sum(f => mesh.GetFaceArea(f)), 1e-9);
            foreach (var face in mesh.Faces)
                Assert.AreEqual(25.0, mesh.GetFaceArea(face), 1e-9);

            // the two halves are different triangles: together they use the three original vertices and the new one
            var usedPoints = mesh.GetFaces().SelectMany(f => mesh.GetFacePoints(f)).ToList();
            Assert.IsTrue(usedPoints.Any(p => p.Equals(P(0, 0))));
            Assert.IsTrue(usedPoints.Any(p => p.Equals(P(10, 0))));
            Assert.IsTrue(usedPoints.Any(p => p.Equals(P(5, 10))));
            Assert.IsTrue(usedPoints.Any(p => p.Equals(P(5, 0))));
        }

        [TestMethod]
        public void CutQuadMeshPreservesTheArea()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(10, 10), P(0, 10) });
            mesh.AddFaceMesh(new[] { P(10, 0), P(20, 0), P(20, 10), P(10, 10) });

            mesh.Cut(new Line2d(new Point2d(5, -5), new Point2d(5, 20)));
            Assert.AreEqual(200.0, mesh.GetFaces().Sum(f => mesh.GetFaceArea(f)), 1e-9);
            Assert.IsTrue(mesh.Faces.Any(f => f.IsQuad), "The quad not crossed by the curve is kept");

            // cut along the diagonal of the first quad
            var mesh2 = new Mesh();
            mesh2.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(10, 10), P(0, 10) });
            mesh2.Cut(new Line2d(new Point2d(-1, -1), new Point2d(20, 20)));
            Assert.AreEqual(2, mesh2.FacesCount);
            Assert.AreEqual(100.0, mesh2.GetFaces().Sum(f => mesh2.GetFaceArea(f)), 1e-9);
        }

        [TestMethod]
        public void FaceAreaUsesTheVertexIds()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(10, 10) });
            mesh.AddFaceMesh(new[] { P(100, 0), P(101, 0), P(101, 1) });
            mesh.Vertices.RemoveAt(0); // positions and ids are now different

            MeshFace face = mesh.Faces[1];
            Assert.AreEqual(0.5, mesh.FaceArea(face), 1e-9);
        }

        [TestMethod]
        public void GenerateOptionsClone()
        {
            var options = new Mesh.GenerateOptions { MeshSize = 12.5, Recombine = false, Refine = true };
            var clone = (Mesh.GenerateOptions)options.Clone();
            Assert.AreEqual(12.5, clone.MeshSize);
            Assert.IsFalse(clone.Recombine);
            Assert.IsTrue(clone.Refine);
        }

        [TestMethod]
        public void TriangulateDoesNotModifyTheInput()
        {
            var points = new List<Point3d> { P(0, 0), P(10, 0), P(10, 10), P(0, 10) };
            Mesh mesh = points.Triangulate();

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(2, mesh.FacesCount, "The last triangle is added");
            Assert.AreEqual(100.0, mesh.GetFaces().Sum(f => mesh.FaceArea(f)), 1e-9);

            Point3d center = points.MassCenter();
            Assert.IsTrue(center.Equals(P(5, 5)));
        }

        #endregion
    }
}
