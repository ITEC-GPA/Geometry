using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class MethodRegressionTests
    {
        private static BoundingBox1d Interval(double a, double b)
        { var box = new BoundingBox1d(); box.Update(a); box.Update(b); return box; }
        private static CoordinateSystem Frame() => new CoordinateSystem(new Point3d(10, 20, 30), Vector3d.YAxis, Vector3d.ZAxis);
        private static void Point(double x, double y, double z, Point3d p)
        { Assert.AreEqual(x, p.X, 1e-10); Assert.AreEqual(y, p.Y, 1e-10); Assert.AreEqual(z, p.Z, 1e-10); }
        private static void Vector(double x, double y, double z, Vector3d v)
        { Assert.AreEqual(x, v.X, 1e-10); Assert.AreEqual(y, v.Y, 1e-10); Assert.AreEqual(z, v.Z, 1e-10); }

        [TestMethod]
        public void BoundingBoxUpdate_DoesNotIncludeZeroInPositiveInterval()
        { var b = Interval(5, 3); Assert.AreEqual(3.0, b.Min); Assert.AreEqual(5.0, b.Max); }
        [TestMethod]
        public void BoundingBoxReset_FirstNewValueReplacesOldExtrema()
        { var b = Interval(-10, 50); b.Reset(); Assert.IsTrue(b.IsEmpty); b.Update(7); Assert.AreEqual(7.0, b.Min); Assert.AreEqual(7.0, b.Max); }
        [TestMethod]
        public void BoundingBoxCenter_UsesBothNegativeAndPositiveEndpoints()
        { Assert.AreEqual(2.0, Interval(-3, 7).Center()); }
        [TestMethod]
        public void BoundingBoxUnion_TouchingIntervalsMerge()
        { var r = BoundingBox1d.GetUnion(Interval(1, 3), Interval(3, 5)); Assert.AreEqual(1, r.Count); Assert.AreEqual(1.0, r[0].Min); Assert.AreEqual(5.0, r[0].Max); }
        [TestMethod]
        public void BoundingBoxDifference_DiscardsOnlyFragmentsShorterThanMinimum()
        { var r = BoundingBox1d.GetDifference(Interval(0, 10), Interval(1, 8), 2); Assert.AreEqual(1, r.Count); Assert.AreEqual(8.0, r[0].Min); Assert.AreEqual(10.0, r[0].Max); }
        [TestMethod]
        public void BoundingBoxIntersection_ExactMinimumLengthIsAccepted()
        { Assert.AreEqual(2.0, BoundingBox1d.GetIntersection(Interval(0, 4), Interval(2, 8), 2).Size); Assert.IsNull(BoundingBox1d.GetIntersection(Interval(0, 4), Interval(2, 8), 2.01)); }
        [TestMethod]
        public void BoundingBoxListIntersection_CollectsAllPairs()
        {
            var r = BoundingBox1d.GetIntersection(new List<BoundingBox1d> { Interval(0, 3), Interval(5, 9) }, new List<BoundingBox1d> { Interval(2, 7) }, 0.5);
            CollectionAssert.AreEqual(new[] { 2.0, 5.0 }, r.Select(x => x.Min).ToArray());
            CollectionAssert.AreEqual(new[] { 3.0, 7.0 }, r.Select(x => x.Max).ToArray());
        }
        [TestMethod]
        public void SetOrigin_UpdatesInverseAndCopiesArgument()
        {
            var cs = Frame(); var origin = new Point3d(3, 4, 5); cs.SetOrigin(origin); origin.X = 99;
            Point(0, 0, 0, cs.ToLocal(new Point3d(3, 4, 5)));
            Point(3, 4, 5, cs.ToGlobal(Point3d.Origin));
        }
        [TestMethod]
        public void ToLocalVector_OnlyRotatesWithoutTranslating()
        { Vector(2, 3, 1, Frame().ToLocal(new Vector3d(1, 2, 3))); }
        [TestMethod]
        public void ToGlobalVector_OnlyRotatesWithoutTranslating()
        { Vector(3, 1, 2, Frame().ToGlobal(new Vector3d(1, 2, 3))); }
        [TestMethod]
        public void ToLocalLine_TransformsBothEndpoints()
        { var r = Frame().ToLocal(new Line3d(new Point3d(11, 22, 33), new Point3d(14, 25, 36))); Point(2, 3, 1, r.Start); Point(5, 6, 4, r.End); }
        [TestMethod]
        public void ToGlobalLine_TransformsBothEndpoints()
        { var r = Frame().ToGlobal(new Line3d(new Point3d(2, 3, 1), new Point3d(5, 6, 4))); Point(11, 22, 33, r.Start); Point(14, 25, 36, r.End); }
        [TestMethod]
        public void RotateV1_UsesLocalAxisInRotatedFrame()
        { var cs = Frame(); cs.RotateV1(Math.PI / 2); Vector(0, 1, 0, cs.V1); Vector(1, 0, 0, cs.V2); Vector(0, 0, -1, cs.V3); Point(10, 20, 30, cs.Origin); }
        [TestMethod]
        public void RotateV2_UsesLocalAxisInRotatedFrame()
        { var cs = Frame(); cs.RotateV2(Math.PI / 2); Vector(-1, 0, 0, cs.V1); Vector(0, 0, 1, cs.V2); Vector(0, 1, 0, cs.V3); }
        [TestMethod]
        public void RotateV3_UsesLocalAxisInRotatedFrame()
        { var cs = Frame(); cs.RotateV3(Math.PI / 2); Vector(0, 0, 1, cs.V1); Vector(0, -1, 0, cs.V2); Vector(1, 0, 0, cs.V3); }
        [TestMethod]
        public void CircleIsPointInside_RejectsPointOutsidePlane()
        { var c = new Circle2d(new Point2d(2, 3), 5); Assert.IsTrue(c.IsPointInside(new Point3d(2, 3, 0))); Assert.IsFalse(c.IsPointInside(new Point3d(2, 3, 1))); }
        [TestMethod]
        public void CircleIsPointOnCircle_DistinguishesBoundaryFromInterior()
        { var c = new Circle2d(new Point2d(2, 3), 5); Assert.IsTrue(c.IsPointOnCircle(new Point3d(7, 3, 0))); Assert.IsFalse(c.IsPointOnCircle(new Point3d(2, 3, 0))); }
        [TestMethod]
        public void CircleIsLineInside_RequiresBothEndpoints()
        { var c = new Circle2d(new Point2d(0, 0), 5); Assert.IsTrue(c.IsLineInside(new Line3d(new Point3d(-5, 0, 0), new Point3d(5, 0, 0)))); Assert.IsFalse(c.IsLineInside(new Line3d(Point3d.Origin, new Point3d(6, 0, 0)))); }
        [TestMethod]
        public void RayIntersectionWithPlane_RejectsIntersectionBehindOrigin()
        { var plane = new Plane(Point3d.Origin, Vector3d.ZAxis); Assert.IsFalse(new Ray3d(new Point3d(0, 0, 2), Vector3d.ZAxis).IntersectionWith(plane, out _)); Assert.IsTrue(new Ray3d(new Point3d(0, 0, 2), new Vector3d(0, 0, -1)).IntersectionWith(plane, out var p)); Point(0, 0, 0, p); }
    }

    [TestClass]
    public class MeshMethodRegressionTests
    {
        private static Shape2d Square() => new Shape2d(new Polygon2d { new Point2d(0, 0), new Point2d(2, 0), new Point2d(2, 2), new Point2d(0, 2) });
        [TestMethod]
        public void DelaunayGenerate_InvalidShapeReturnsDiagnosticAndNoMesh()
        { Assert.IsFalse(DelaunayMesh.Generate((Shape2d)null, null, out var mesh, out var status)); Assert.IsNull(mesh); Assert.IsNotNull(status); }
        [TestMethod]
        public void DelaunayGenerateMany_StopsAtFirstFailureAndPreservesEarlierMesh()
        {
            Assert.IsFalse(DelaunayMesh.Generate(new[] { Square(), null, Square() }, null, out var meshes, out var status));
            Assert.AreEqual(1, meshes.Count); Assert.IsTrue(meshes[0].FacesCount > 0); Assert.IsNotNull(status);
        }
        [TestMethod]
        public void InitialMeshGenerate_SquareHasTwoTrianglesWithoutInteriorNodes()
        {
            Assert.IsTrue(InitialMesh.Generate(Square(), out var mesh, out var status));
            Assert.IsNull(status); Assert.AreEqual(4, mesh.VerticesCount); Assert.AreEqual(2, mesh.FacesCount);
        }
        [TestMethod]
        public void GMeshGenerate_RejectsInvalidEmbeddedSizeBeforeNativeCall()
        {
            var sizes = new Dictionary<GeometryBase, double> { { Point3d.Origin, double.NaN } };
            Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(Array.Empty<Shape>(), null, sizes, new GMesh.GMeshGenerateOptions(), out _, out _));
        }
    }
}
