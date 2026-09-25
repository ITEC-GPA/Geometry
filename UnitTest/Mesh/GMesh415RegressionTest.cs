using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Options = GPC.Geometry.Meshes.GMesh.GMesh.GMeshGenerateOptions;

namespace Meshes.GMsh
{
    [TestClass]
    [DoNotParallelize]
    public class GMesh415RegressionTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);
        private static Polygon3d Rect(double x = 0, double y = 0, double w = 20, double h = 10) =>
            new Polygon3d { P(x, y), P(x + w, y), P(x + w, y + h), P(x, y + h) };
        private static Shape Plate() => new Shape(Rect());
        private static Options Opt() => new Options { MeshSize = 2 };
        private static double Area(Mesh mesh) => mesh.Faces.Sum(f => mesh.GetFaceArea(f));

        // Check every face and every output mesh, including connectivity and finite coordinates.
        private static void Valid(Mesh mesh, double expectedArea)
        {
            Assert.IsTrue(mesh.FacesCount > 0);
            Assert.IsTrue(mesh.VerticesCount >= 3);
            var ids = new HashSet<int>(mesh.Vertices.Select(v => v.Id));
            Assert.AreEqual(mesh.VerticesCount, ids.Count);
            var used = new HashSet<int>();
            foreach (var v in mesh.Vertices)
                Assert.IsTrue(new[] { v.Point.X, v.Point.Y, v.Point.Z }.All(x => !double.IsNaN(x) && !double.IsInfinity(x)));
            foreach (var face in mesh.Faces)
            {
                int[] nodes = face.GetNodes();
                Assert.AreEqual(nodes.Length, nodes.Distinct().Count(), "repeated face node");
                Assert.IsTrue(nodes.All(ids.Contains), "dangling node reference");
                used.UnionWith(nodes);
                Assert.IsTrue(mesh.GetFaceArea(face) > expectedArea * 1e-12, "degenerate face");
            }
            Assert.AreEqual(ids.Count, used.Count, "orphan vertices");
            Assert.AreEqual(expectedArea, Area(mesh), Math.Max(1e-8, expectedArea * 1e-7));
        }

        private static List<Mesh> Generate(Shape[] shapes, double[] areas, Options options = null,
            Dictionary<Shape, GeometryBase[]> embedded = null, Dictionary<GeometryBase, double> sizes = null)
        {
            Assert.IsTrue(GMesh.Generate(shapes, embedded, sizes, options ?? Opt(), out var meshes, out var status), status.GetLastCustomErrorMessage());
            Assert.IsNull(status.GetLastException());
            Assert.AreEqual(areas.Length, meshes.Count);
            for (int i = 0; i < meshes.Count; i++) Valid(meshes[i], areas[i]);
            return meshes;
        }

        private static Mesh Single(Shape shape = null, double area = 200, Options options = null) =>
            Generate(new[] { shape ?? Plate() }, new[] { area }, options)[0];

        private static void Algorithm(Options.MeshAlgorithm algorithm)
        {
            var options = Opt();
            options.Algorithm = algorithm;
            options.Recombine = false;
            var mesh = Single(options: options);
            Assert.IsTrue(mesh.Faces.Any(f => f.IsTriangle));
        }

        private static void Recombination(Options.RecombinationMeshAlgorithm algorithm)
        {
            var options = Opt();
            options.RecombinationAlgorithm = algorithm;
            var mesh = Single(options: options);
            Assert.IsTrue(mesh.Faces.Count(f => f.IsQuad) > mesh.FacesCount / 2);
        }

        private static void EmbeddedNode(Point3d point)
        {
            var shape = Plate();
            var embedded = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { point } };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, embedded, null, Opt(), out var meshes, out var status), status.GetLastCustomErrorMessage());
            Valid(meshes[0], 200);
            int node = status.EmbeddedGeometriesVertexMap[meshes[0]].Values.Single().Single();
            Assert.AreEqual(0, meshes[0].GetVertex(node).Point.DistanceTo(point), 1e-7);
            Assert.IsTrue(meshes[0].Faces.Any(f => f.GetNodes().Contains(node)));
        }

        private static void EmbeddedLines(params Line3d[] lines)
        {
            var shape = Plate();
            var embedded = new Dictionary<Shape, GeometryBase[]> { [shape] = lines.Cast<GeometryBase>().ToArray() };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, embedded, null, Opt(), out var meshes, out var status), status.GetLastCustomErrorMessage());
            Mesh mesh = meshes[0];
            Valid(mesh, 200);
            var map = status.EmbeddedGeometriesVertexMap[mesh];
            Assert.AreEqual(lines.Length, map.Count);
            foreach (var entry in map)
            {
                var line = (Line3d)entry.Key;
                var points = entry.Value.Select(id => mesh.GetVertex(id).Point).ToArray();
                Assert.IsTrue(points.Length >= 2);
                Assert.IsTrue(points.All(p => line.IsPointOnLine(p, 1e-7)));
                Assert.AreEqual(0, points.First().DistanceTo(line.Start), 1e-7);
                Assert.AreEqual(0, points.Last().DistanceTo(line.End), 1e-7);
                // Each consecutive pair must be a face edge, not merely lie on the line.
                for (int i = 1; i < entry.Value.Length; i++)
                {
                    int a = entry.Value[i - 1], b = entry.Value[i];
                    Assert.IsTrue(mesh.Faces.Any(f =>
                    {
                        var nodes = f.GetNodes();
                        return Enumerable.Range(0, nodes.Length).Any(j =>
                            nodes[j] == a && nodes[(j + 1) % nodes.Length] == b ||
                            nodes[j] == b && nodes[(j + 1) % nodes.Length] == a);
                    }), "constraint must follow mesh edges");
                }
            }
        }

        [TestMethod] public void RectangleAreaAndConnectivity() => Single();
        [TestMethod] public void TriangleAreaAndConnectivity() => Single(new Shape(new Polygon3d { P(0, 0), P(20, 0), P(0, 10) }), 100);
        [TestMethod] public void ConcavePolygonAreaAndConnectivity() => Single(new Shape(new Polygon3d { P(0, 0), P(20, 0), P(20, 5), P(10, 5), P(10, 10), P(0, 10) }), 150);
        [TestMethod] public void ReversedWindingPreservesArea() { var p = Rect(); p.Reverse(); Single(new Shape(p)); }
        [TestMethod] public void HorizontalPlaneAtNonzeroHeight() { var s = Plate(); s.Move(0, 0, 37); var m = Single(s); Assert.IsTrue(m.Vertices.All(v => Math.Abs(v.Point.Z - 37) < 1e-7)); }
        [TestMethod] public void VerticalXZPlane() { var m = Single(new Shape(new Polygon3d { P(0, 3, 0), P(20, 3, 0), P(20, 3, 10), P(0, 3, 10) })); Assert.IsTrue(m.Vertices.All(v => Math.Abs(v.Point.Y - 3) < 1e-7)); }
        [TestMethod] public void VerticalYZPlane() { var m = Single(new Shape(new Polygon3d { P(7, 0, 0), P(7, 20, 0), P(7, 20, 10), P(7, 0, 10) })); Assert.IsTrue(m.Vertices.All(v => Math.Abs(v.Point.X - 7) < 1e-7)); }
        [TestMethod] public void InclinedPlane() { var m = Single(new Shape(new Polygon3d { P(0, 0), P(20, 0), P(20, 6, 8), P(0, 6, 8) })); Assert.IsTrue(m.Vertices.All(v => Math.Abs(4 * v.Point.Y - 3 * v.Point.Z) < 1e-6)); }
        [TestMethod] public void NegativeCoordinates() { var m = Single(new Shape(Rect(-40, -30))); Assert.IsTrue(m.Vertices.All(v => v.Point.X <= -20 + 1e-7 && v.Point.Y <= -20 + 1e-7)); }
        [TestMethod] public void SmallGeometry() { var o = Opt(); o.MeshSize = 0.02; Single(new Shape(Rect(0, 0, 0.2, 0.1)), 0.02, o); }
        [TestMethod] public void LargeCoordinatesPreserveArea() => Single(new Shape(Rect(1000000, -1000000)));

        [TestMethod] public void RectangularHoleRemainsEmpty()
        {
            var mesh = Single(new Shape(Rect(), new[] { Rect(8, 3, 4, 4) }), 184);
            Assert.IsFalse(mesh.Faces.Any(f => { var p = mesh.GetFaceCentroid(f); return p.X > 8 && p.X < 12 && p.Y > 3 && p.Y < 7; }));
        }
        [TestMethod] public void TwoHolesPreserveNetArea() => Single(new Shape(Rect(), new[] { Rect(3, 3, 2, 2), Rect(14, 3, 2, 2) }), 192);
        [TestMethod] public void DisconnectedSurfacesRemainSeparate() => Generate(new[] { Plate(), new Shape(Rect(30, 0)) }, new[] { 200d, 200d });
        [TestMethod] public void AdjacentSurfacesShareBoundaryCoordinates()
        {
            var meshes = Generate(new[] { Plate(), new Shape(Rect(20, 0)) }, new[] { 200d, 200d });
            var left = meshes[0].Vertices.Where(v => Math.Abs(v.Point.X - 20) < 1e-7).Select(v => v.Point.Y).OrderBy(y => y).ToArray();
            var right = meshes[1].Vertices.Where(v => Math.Abs(v.Point.X - 20) < 1e-7).Select(v => v.Point.Y).OrderBy(y => y).ToArray();
            Assert.AreEqual(left.Length, right.Length);
            for (int i = 0; i < left.Length; i++) Assert.AreEqual(left[i], right[i], 1e-7);
        }
        [TestMethod] public void IntersectingPlateAndWallShareNodes()
        {
            var wall = new Shape(new Polygon3d { P(10, 0, -5), P(10, 10, -5), P(10, 10, 5), P(10, 0, 5) });
            var meshes = Generate(new[] { Plate(), wall }, new[] { 200d, 100d });
            var seam = meshes[0].Vertices.Where(v => Math.Abs(v.Point.X - 10) < 1e-7).ToArray();
            Assert.IsTrue(seam.Length > 2);
            Assert.IsTrue(seam.All(v => meshes[1].Vertices.Any(w => w.Point.DistanceTo(v.Point) < 1e-7)));
        }
        [TestMethod] public void MeshAdaptProducesTriangles() => Algorithm(Options.MeshAlgorithm.MeshAdapt);
        [TestMethod] public void DelaunayProducesTriangles() => Algorithm(Options.MeshAlgorithm.Delaunay);
        [TestMethod] public void FrontalDelaunayProducesTriangles() => Algorithm(Options.MeshAlgorithm.FrontalDelaunay);
        [TestMethod] public void QuadFrontalWithoutRecombinationProducesTriangles() => Algorithm(Options.MeshAlgorithm.FrontalDelaunayForQuads);
        [TestMethod] public void AutomaticProducesTriangles() => Algorithm(Options.MeshAlgorithm.Automatic);
        [TestMethod] public void QuasiStructuredProducesValidQuads() { var o = Opt(); o.Algorithm = Options.MeshAlgorithm.QuasiStructuredQuad; Assert.IsTrue(Single(options: o).Faces.Any(f => f.IsQuad)); }
        [TestMethod] public void PackingWorkaroundReportsEffectiveAlgorithm()
        {
            var shape = Plate(); var o = Opt(); o.Algorithm = Options.MeshAlgorithm.PackingOfParallelograms;
            var embedded = new Dictionary<Shape, GeometryBase[]> { [shape] = new GeometryBase[] { new Shape(Rect(9, 4, 2, 2)) } };
            Assert.IsTrue(GMesh.Generate(new[] { shape }, embedded, null, o, out var meshes, out var status), status.GetLastCustomErrorMessage());
            Valid(meshes[0], 200);
            Assert.IsTrue(status.Warnings.Any(w => w.Contains("UntangleTris") && w.Contains("FrontalDelaunayForQuads")));
            Assert.AreEqual(Options.MeshAlgorithm.PackingOfParallelograms, o.Algorithm);
        }
        [TestMethod] public void SimpleRecombination() => Recombination(Options.RecombinationMeshAlgorithm.Simple);
        [TestMethod] public void BlossomRecombination() => Recombination(Options.RecombinationMeshAlgorithm.Blossom);
        [TestMethod] public void SimpleFullQuadRecombination() => Recombination(Options.RecombinationMeshAlgorithm.SimpleFullQuad);
        [TestMethod] public void BlossomFullQuadRecombination() => Recombination(Options.RecombinationMeshAlgorithm.BlossomFullQuad);
        [TestMethod] public void RefinementIncreasesResolutionWithoutChangingArea()
        {
            var o = Opt(); o.Recombine = false; int coarse = Single(options: o).FacesCount;
            o.Refine = true; Assert.IsTrue(Single(options: o).FacesCount > coarse);
        }
        [TestMethod] public void RenumberDisabledKeepsConnectivity() { var o = Opt(); o.Renumber = false; Single(options: o); }
        [TestMethod] public void LocalIdentifiersRemainValidPerMesh() { var o = Opt(); o.UseGlobalProgressID = false; Generate(new[] { Plate(), new Shape(Rect(30, 0)) }, new[] { 200d, 200d }, o); }
        [TestMethod] public void TransfiniteSurfaceProducesStructuredQuads() { var o = Opt(); o.TransfiniteSurface = true; Assert.IsTrue(Single(options: o).Faces.All(f => f.IsQuad)); }
        [TestMethod] public void RelocateOptimizationPreservesBoundaryAndArea() { var o = Opt(); o.Optimize = true; o.OptimizeAlgorithm = Options.MeshOptimize.Relocate2D; o.Recombine = false; var m = Single(options: o); Assert.IsTrue(m.Vertices.All(v => v.Point.X >= -1e-7 && v.Point.X <= 20 + 1e-7 && v.Point.Y >= -1e-7 && v.Point.Y <= 10 + 1e-7)); }
        [TestMethod] public void InteriorPointIsAFaceVertex() => EmbeddedNode(P(7, 4));
        [TestMethod] public void BoundaryPointIsAFaceVertex() => EmbeddedNode(P(7, 0));
        [TestMethod] public void CornerPointIsNotDuplicated() => EmbeddedNode(P(0, 0));
        [TestMethod] public void InteriorLineFollowsFaceEdges() => EmbeddedLines(new Line3d(P(3, 3), P(17, 7)));
        [TestMethod] public void BoundaryLineFollowsFaceEdges() => EmbeddedLines(new Line3d(P(0, 0), P(20, 0)));
        [TestMethod] public void CrossingLinesFollowFaceEdges() => EmbeddedLines(new Line3d(P(0, 0), P(20, 10)), new Line3d(P(0, 10), P(20, 0)));
        [TestMethod] public void EmbeddedShapeMapsExactlyItsFaces()
        {
            var s = Plate(); var patch = new Shape(Rect(6, 2, 8, 6));
            var embedded = new Dictionary<Shape, GeometryBase[]> { [s] = new GeometryBase[] { patch } };
            Assert.IsTrue(GMesh.Generate(new[] { s }, embedded, null, Opt(), out var meshes, out var status), status.GetLastCustomErrorMessage());
            Valid(meshes[0], 200);
            var faces = status.EmbeddedGeometriesVertexMap[meshes[0]].Values.Single().Select(id => meshes[0].GetFace(id)).ToArray();
            Assert.AreEqual(48, faces.Sum(f => meshes[0].GetFaceArea(f)), 1e-7);
            Assert.IsTrue(faces.All(f => { var p = meshes[0].GetFaceCentroid(f); return p.X >= 6 && p.X <= 14 && p.Y >= 2 && p.Y <= 8; }));
        }
        [TestMethod] public void LocalSizingIncreasesResolution()
        {
            var s = Plate(); var p = P(10, 5);
            var embedded = new Dictionary<Shape, GeometryBase[]> { [s] = new GeometryBase[] { p } };
            var coarse = Generate(new[] { s }, new[] { 200d }, embedded: embedded)[0];
            var fine = Generate(new[] { s }, new[] { 200d }, embedded: embedded, sizes: new Dictionary<GeometryBase, double> { [p] = 0.4 })[0];
            Assert.IsTrue(fine.VerticesCount > coarse.VerticesCount);
        }
        [TestMethod] public void RepeatedCallsDoNotLeakModels() { Single(); Single(new Shape(Rect(0, 0, 12, 8)), 96); Single(); }
        [TestMethod] public void UnsupportedEmbeddedGeometryDoesNotPoisonNextCall()
        {
            var s = Plate(); var embedded = new Dictionary<Shape, GeometryBase[]> { [s] = new GeometryBase[] { new Vector3d(1, 2, 3) } };
            Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(new[] { s }, embedded, null, Opt(), out _, out _));
            Single();
        }
        [TestMethod] public void ConcurrentCallsKeepTheirOwnResults()
        {
            Parallel.For(0, 4, i => Single(new Shape(Rect(0, 0, 20 + i, 10)), (20 + i) * 10));
        }
        [TestMethod] public void OptionsClonePreservesAllPublicFields()
        {
            var o = Opt(); o.TransfiniteSurface = true; o.MeshScalingFactor = 2; o.Renumber = false;
            var copy = (Options)o.Clone(); Assert.AreNotSame(o, copy);
            foreach (var field in typeof(Options).GetFields()) Assert.AreEqual(field.GetValue(o), field.GetValue(copy), field.Name);
            copy.MeshSize = 7; Assert.AreEqual(2, o.MeshSize);
        }
        [TestMethod] public void GenerationDoesNotMutateInputGeometry()
        {
            var s = Plate(); var before = s.Fill.Select(p => P(p.X, p.Y, p.Z)).ToArray(); var o = Opt();
            Single(s, options: o);
            for (int i = 0; i < before.Length; i++) Assert.AreEqual(0, before[i].DistanceTo(s.Fill[i]), 1e-12);
            Assert.AreEqual(2, o.MeshSize);
        }
        [TestMethod] public void NullShapesAreRejected() => Assert.ThrowsException<ArgumentNullException>(() => GMesh.Generate(null, Opt(), out _, out _));
        [TestMethod] public void NullOptionsAreRejected() => Assert.ThrowsException<ArgumentNullException>(() => GMesh.Generate(new[] { Plate() }, null, out _, out _));
        [TestMethod] public void InvalidMeshSizesAreRejectedBeforeNativeCalls()
        {
            foreach (double size in new[] { 0d, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                var o = Opt(); o.MeshSize = size;
                Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(new[] { Plate() }, o, out _, out _));
                o = Opt(); o.MeshSizeMax = size;
                Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(new[] { Plate() }, o, out _, out _));
                var s = Plate(); var p = P(5, 5);
                Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(new[] { s }, new Dictionary<Shape, GeometryBase[]> { [s] = new GeometryBase[] { p } }, new Dictionary<GeometryBase, double> { [p] = size }, Opt(), out _, out _));
            }
            var reversed = Opt(); reversed.MeshSizeMin = 5; reversed.MeshSizeMax = 2;
            Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(new[] { Plate() }, reversed, out _, out _));
            Single();
        }
        [TestMethod] public void GeneratedMeshCloneIsIndependent()
        {
            var original = Single(); var clone = (Mesh)original.Clone(); clone.Move(0, 0, 7);
            Valid(clone, 200); Valid(original, 200);
            Assert.IsTrue(original.Vertices.All(v => Math.Abs(v.Point.Z) < 1e-7));
            Assert.IsTrue(clone.Vertices.All(v => Math.Abs(v.Point.Z - 7) < 1e-7));
        }
        [TestMethod] public void ManagedMeshRefinementPreservesArea()
        {
            var mesh = Single(); int before = mesh.FacesCount; mesh.Refine();
            Valid(mesh, 200); Assert.IsTrue(mesh.FacesCount > before);
        }
    }
}
