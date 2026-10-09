using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meshes.GMsh
{
    [TestClass, DoNotParallelize]
    public class CurveMeshingTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);
        private static Shape Plate() => new Shape(new Polygon3d(new[] { P(0, 0), P(10, 0), P(10, 10), P(0, 10) }));

        [TestMethod]
        public void GMeshEmbedsEllipticalArcsWithOriginalCurveNodeMaps()
        {
            var ellipse = new EllipseCurve3d(P(5, 5), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 3, 1, .2, 4);
            var shape = Plate();
            var input = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { ellipse } };
            var options = new GMesh.GMeshGenerateOptions { MeshSize = 1, CurveChordTolerance = .01, CurveMaxSegmentLength = .4 };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, input, null, options, out var meshes, out var status), status.GetLastCustomErrorMessage());
            var mesh = meshes.Single();
            int[] ids = status.EmbeddedGeometriesVertexMap[mesh][ellipse];
            Assert.IsTrue(ids.Length > 15);
            double previous = -1;
            foreach (int id in ids)
            {
                Point3d point = mesh.GetVertex(id).Point;
                Assert.IsTrue(ellipse.ClosestPoint(point).DistanceTo(point) <= .010001);
                double parameter = ellipse.ClosestParameter(point);
                Assert.IsTrue(parameter >= previous - 1e-8); previous = parameter;
            }
            Assert.AreEqual(100, mesh.Faces.Sum(f => mesh.GetFaceArea(f)), 1e-6);
        }

        [TestMethod]
        public void GMeshEmbedsAnArcAndReturnsNodesUnderTheOriginalCurve()
        {
            var arc = new ArcCurve3d(P(5, 5), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 2, Math.PI);
            var shape = Plate(); var input = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { arc } };
            var sizes = new Dictionary<GeometryBase, double> { [arc] = 0.3 };
            var options = new GMesh.GMeshGenerateOptions { MeshSize = 1, CurveChordTolerance = 0.02, CurveMaxSegmentLength = 0.8 };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, input, sizes, options, out var meshes, out var status), status.GetLastCustomErrorMessage());
            Assert.IsNull(status.GetLastException()); Assert.AreEqual(1, meshes.Count);
            var mesh = meshes[0]; var ids = status.EmbeddedGeometriesVertexMap[mesh][arc];
            Assert.IsTrue(ids.Length > 10); Assert.AreEqual(ids.Length, ids.Distinct().Count());
            Assert.AreEqual(100, mesh.Faces.Sum(f => mesh.GetFaceArea(f)), 1e-6);
            Assert.IsTrue(mesh.GetVertex(ids[0]).Point.DistanceTo(arc.StartPoint) < 1e-6);
            Assert.IsTrue(mesh.GetVertex(ids[ids.Length - 1]).Point.DistanceTo(arc.EndPoint) < 1e-6);
            double previous = -1;
            foreach (int id in ids)
            {
                var point = mesh.GetVertex(id).Point;
                Assert.IsTrue(point.DistanceTo(arc.ClosestPoint(point)) <= 0.020001);
                double parameter = arc.ClosestParameter(point); Assert.IsTrue(parameter >= previous - 1e-8); previous = parameter;
            }
            Assert.AreSame(arc, input[shape][0]); Assert.AreEqual(1, sizes.Count);
            Assert.AreEqual(0.3, sizes[arc]);
        }

        [TestMethod]
        public void GMeshMapsOppositeCurveDirectionsWithGeometryScaling()
        {
            var line = new LineCurve3d(P(2, 4), P(8, 4)); var reverse = line.Reversed(); var shape = Plate();
            var input = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { line, reverse } };
            var options = new GMesh.GMeshGenerateOptions { MeshSize = 1, GeometryBaseScaleFactor = 2, MeshScalingFactor = 0.5 };
            var sizes = new Dictionary<GeometryBase, double> { [line] = 0.6, [reverse] = 0.3 };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, input, sizes, options, out var meshes, out var status), status.GetLastCustomErrorMessage());
            var mesh = meshes.Single(); var map = status.EmbeddedGeometriesVertexMap[mesh];
            CollectionAssert.AreEqual(map[line].Reverse().ToArray(), map[reverse]);
            Assert.IsTrue(map[line].Length >= 20);
            Assert.IsTrue(mesh.GetVertex(map[line][0]).Point.DistanceTo(line.StartPoint) < 1e-6);
            Assert.AreEqual(100, mesh.Faces.Sum(f => mesh.GetFaceArea(f)), 1e-6);
        }

        [TestMethod]
        public void GMeshMapsMixedCurvesAcrossSegmentJunctions()
        {
            var arc = new ArcCurve3d(P(5, 5), new Vector3d(0, 0, 1), new Vector3d(0, -1, 0), 2, Math.PI / 2);
            var curve = new PolyCurve3d(new Curve3d[] { new LineCurve3d(P(2, 3), arc.StartPoint), arc, new LineCurve3d(arc.EndPoint, P(7, 8)) });
            var shape = Plate(); var input = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { curve } };
            var options = new GMesh.GMeshGenerateOptions { MeshSize = 1, CurveChordTolerance = 0.02 };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, input, null, options, out var meshes, out var status), status.GetLastCustomErrorMessage());
            var mesh = meshes.Single(); var ids = status.EmbeddedGeometriesVertexMap[mesh][curve];
            Assert.IsTrue(mesh.GetVertex(ids[0]).Point.DistanceTo(curve.StartPoint) < 1e-6);
            Assert.IsTrue(mesh.GetVertex(ids[ids.Length - 1]).Point.DistanceTo(curve.EndPoint) < 1e-6);
            Assert.IsTrue(ids.Select(id => mesh.GetVertex(id).Point).Any(p => p.DistanceTo(arc.StartPoint) < 1e-6));
            Assert.IsTrue(ids.Select(id => mesh.GetVertex(id).Point).Any(p => p.DistanceTo(arc.EndPoint) < 1e-6));
            double previous = -1;
            foreach (int id in ids)
            {
                double parameter = curve.ClosestParameter(mesh.GetVertex(id).Point);
                Assert.IsTrue(parameter >= previous - 1e-8); previous = parameter;
            }
        }

        [TestMethod]
        public void GMeshAcceptsClosedCurveBoundariesThroughExplicitPolygonConversion()
        {
            var circle = new ArcCurve3d(P(0, 0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 3, 2 * Math.PI);
            var polygon = circle.ToPolygon3d(0.01); var shape = new Shape(polygon);
            Assert.IsTrue(GMesh.Generate(new[] { shape }, new GMesh.GMeshGenerateOptions { MeshSize = 0.8 }, out var meshes, out var status), status.GetLastCustomErrorMessage());
            double area = meshes.Single().Faces.Sum(f => meshes[0].GetFaceArea(f));
            Assert.AreEqual(Math.Abs(polygon.GetSignedArea()), area, 1e-6);
            Assert.AreEqual(9 * Math.PI, area, 0.2);
        }

        [TestMethod]
        public void GMeshCopiesCurveOptionsAndRejectsInvalidCurveToleranceBeforeGeneration()
        {
            var options = new GMesh.GMeshGenerateOptions { CurveChordTolerance = 0.02, CurveMaxSegmentLength = 0.3 };
            var clone = (GMesh.GMeshGenerateOptions)options.Clone(); Assert.AreEqual(0.02, clone.CurveChordTolerance); Assert.AreEqual(0.3, clone.CurveMaxSegmentLength);
            var shape = Plate(); var input = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { new LineCurve3d(P(1, 1), P(2, 1)) } };
            options.CurveChordTolerance = double.NaN;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => GMesh.Generate(new[] { shape }, input, null, options, out var meshes, out var status));
        }
    }
}
