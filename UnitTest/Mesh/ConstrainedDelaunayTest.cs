using System;
using System.Diagnostics;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meshes
{
    /// <summary>
    /// Tests of the constrained Delaunay mesher used by <see cref="DelaunayMesh"/> and <see cref="InitialMesh"/> (September 2026)
    /// </summary>
    [TestClass]
    public class ConstrainedDelaunayTest
    {
        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static Polygon2d Rectangle(double x, double y, double width, double height)
        {
            return new Polygon2d(new[] { Q(x, y), Q(x + width, y), Q(x + width, y + height), Q(x, y + height) });
        }

        private static Mesh Generate(Shape2d shape, double meshSize, bool initialMeshOnly = false, double minAngle = 0)
        {
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = meshSize, InitialMeshOnly = initialMeshOnly, MinAngle = minAngle, Recombine = false };
            Assert.IsTrue(DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus status));
            Assert.IsNull(status);
            return mesh;
        }

        private static double SignedArea(Mesh mesh, MeshFace face)
        {
            Point3d[] p = mesh.GetFacePoints(face);
            return ((p[1].X - p[0].X) * (p[2].Y - p[0].Y) - (p[2].X - p[0].X) * (p[1].Y - p[0].Y)) / 2.0;
        }

        /// <summary>
        /// Area equal to the area of the shape, triangles counterclockwise
        /// </summary>
        private static void AssertValidMesh(Mesh mesh, Shape2d shape)
        {
            Assert.IsTrue(mesh.FacesCount > 0);
            Assert.IsTrue(mesh.Faces.All(f => f.IsTriangle));
            Assert.IsTrue(mesh.Faces.All(f => SignedArea(mesh, f) > 0), "all the triangles must be counterclockwise");

            double area = mesh.Faces.Sum(f => SignedArea(mesh, f));
            Assert.AreEqual(shape.GetArea(), area, shape.GetArea() * 1e-10);
        }

        private static double MaxEdge(Mesh mesh)
        {
            return mesh.Faces.Max(f =>
            {
                Point3d[] p = mesh.GetFacePoints(f);
                return Math.Max(p[0].DistanceTo(p[1]), Math.Max(p[1].DistanceTo(p[2]), p[2].DistanceTo(p[0])));
            });
        }

        private static double MinAngle(Mesh mesh)
        {
            return mesh.Faces.Min(f =>
            {
                Point3d[] p = mesh.GetFacePoints(f);
                double min = double.MaxValue;
                for (int i = 0; i < 3; i++)
                    min = Math.Min(min, new Vector3d(p[i], p[(i + 1) % 3]).AngleTo(new Vector3d(p[i], p[(i + 2) % 3])));
                return min * 180.0 / Math.PI;
            });
        }

        [TestMethod]
        public void MeshSizeIsHonored()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));

            foreach (double meshSize in new[] { 150.0, 50.0, 20.0 })
            {
                Mesh mesh = Generate(shape, meshSize);
                AssertValidMesh(mesh, shape);

                Assert.IsTrue(MaxEdge(mesh) <= 1.5 * meshSize, $"mesh size {meshSize}: max edge {MaxEdge(mesh)}");

                // number of triangles close to the one of equilateral triangles with the edge equal to the mesh size
                double equilateral = 300.0 * 500.0 / (Math.Sqrt(3) / 4.0 * meshSize * meshSize);
                Assert.IsTrue(mesh.FacesCount < 2.0 * equilateral + 10, $"mesh size {meshSize}: {mesh.FacesCount} triangles");
                Assert.IsTrue(mesh.FacesCount > 0.5 * equilateral, $"mesh size {meshSize}: {mesh.FacesCount} triangles");
            }
        }

        [TestMethod]
        public void UnitsAndPositionDoNotMatter()
        {
            Mesh millimetres = Generate(new Shape2d(Rectangle(0, 0, 300, 500)), 50);
            Mesh metres = Generate(new Shape2d(Rectangle(0, 0, 0.3, 0.5)), 0.05);
            Mesh farFromOrigin = Generate(new Shape2d(Rectangle(100000, -250000, 300, 500)), 50);

            Assert.AreEqual(millimetres.FacesCount, metres.FacesCount);
            Assert.AreEqual(millimetres.FacesCount, farFromOrigin.FacesCount);
            AssertValidMesh(metres, new Shape2d(Rectangle(0, 0, 0.3, 0.5)));
            AssertValidMesh(farFromOrigin, new Shape2d(Rectangle(100000, -250000, 300, 500)));

            // small shape far from the origin (the previous mesher crashed)
            var small = new Shape2d(Rectangle(10000, 10000, 10, 10));
            AssertValidMesh(Generate(small, 2), small);
        }

        [TestMethod]
        public void DefaultMeshSizeUsesOnlyTheVerticesOfTheShape()
        {
            var polygon = new Polygon2d(new[] { Q(0, 0), Q(100, 0), Q(100, 20), Q(60, 20), Q(60, 80), Q(100, 80), Q(100, 100), Q(0, 100), Q(0, 80), Q(40, 80), Q(40, 20), Q(0, 20) });
            var shape = new Shape2d(polygon);
            Mesh mesh = Generate(shape, 1E+22);

            AssertValidMesh(mesh, shape);
            Assert.AreEqual(12, mesh.VerticesCount);
            Assert.AreEqual(10, mesh.FacesCount);

            var withHoles = new Shape2d(Rectangle(0, 0, 100, 200), new[] { Rectangle(20, 20, 60, 60), Rectangle(20, 120, 60, 60) });
            Mesh meshWithHoles = Generate(withHoles, 1E+22);
            AssertValidMesh(meshWithHoles, withHoles);
            Assert.AreEqual(12, meshWithHoles.VerticesCount);
            Assert.AreEqual(12 + 2 * 2 - 2, meshWithHoles.FacesCount);
        }

        [TestMethod]
        public void VerticesOfTheShapeKeepTheirCoordinates()
        {
            var polygon = new Polygon2d(new[] { Q(0.1, 0.2), Q(300.3, 0.7), Q(310.9, 500.1), Q(-10.3, 480.7) });
            var shape = new Shape2d(polygon);
            Mesh mesh = Generate(shape, 37.3);

            foreach (Point2d point in polygon)
                Assert.IsTrue(mesh.Vertices.Any(v => v.Point.X == point.X && v.Point.Y == point.Y && v.Point.Z == 0), $"vertex {point} missing");
        }

        [TestMethod]
        public void HolesAndChilds()
        {
            var child = new Shape2d(Rectangle(40, 40, 20, 20));
            var shape = new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(25, 25, 50, 50) }, new[] { child });
            Mesh mesh = Generate(shape, 10);

            AssertValidMesh(mesh, shape);

            // no triangle between the hole and the child
            foreach (MeshFace face in mesh.Faces)
            {
                Point3d[] p = mesh.GetFacePoints(face);
                double x = (p[0].X + p[1].X + p[2].X) / 3.0;
                double y = (p[0].Y + p[1].Y + p[2].Y) / 3.0;
                bool inHole = x > 25 && x < 75 && y > 25 && y < 75;
                bool inChild = x > 40 && x < 60 && y > 40 && y < 60;
                Assert.IsTrue(!inHole || inChild);
            }
        }

        [TestMethod]
        public void LoopsWithSharedEdges()
        {
            // a segment of two loops does not change the side (before: hole inside, child missing or triangles outside the shape)
            var shapes = new[]
            {
                new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(0, 40, 30, 20) }),                                     // hole with an edge on the fill
                new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(20, 20, 30, 30), Rectangle(50, 20, 30, 30) }),         // two holes with a common edge
                new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(50, 50, 40, 40) }, new[] { new Shape2d(Rectangle(50, 50, 40, 40)) }), // child equal to its hole
                new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(0, 40, 30, 20), Rectangle(50, 50, 40, 40) }, new[] { new Shape2d(Rectangle(60, 60, 20, 20)) }),
            };

            foreach (Shape2d shape in shapes)
            {
                foreach (double meshSize in new[] { 1E+22, 5.0 })
                {
                    Mesh mesh = Generate(shape, meshSize);
                    AssertValidMesh(mesh, shape);
                    Assert.IsTrue(mesh.Vertices.All(v => v.Point.X >= 0 && v.Point.X <= 100 && v.Point.Y >= 0 && v.Point.Y <= 100));
                }
            }
        }

        [TestMethod]
        public void BoundaryPointsKeepTheirTags()
        {
            // the boundary points are inserted in a random order, the tags follow the order of the loops (the vertices of the shape first)
            var shape = new Shape2d(Rectangle(0, 0, 300, 500), new[] { Rectangle(100, 100, 100, 100) });
            Mesh mesh = Generate(shape, 50, true);

            var tagged = mesh.Vertices.Where(v => v.Tag is int tag && tag >= 0).OrderBy(v => (int)v.Tag).ToList();
            Assert.AreEqual(2 * (6 + 10) + 4 * 2, tagged.Count);
            CollectionAssert.AreEqual(Enumerable.Range(0, tagged.Count).ToList(), tagged.Select(v => (int)v.Tag).ToList());

            // the fill (tags 0-31), then the hole (32-39), each one from its first vertex along its edges (spacing 50)
            Assert.IsTrue(tagged[0].Point.X == shape.Fill[0].X && tagged[0].Point.Y == shape.Fill[0].Y);
            Assert.IsTrue(tagged[32].Point.X == shape.Holes[0][0].X && tagged[32].Point.Y == shape.Holes[0][0].Y);
            for (int k = 0; k < tagged.Count; k++)
            {
                int next = k == 31 ? 0 : k == 39 ? 32 : k + 1;
                Assert.AreEqual(50, tagged[k].Point.DistanceTo(tagged[next].Point), 1E-9, $"tags {k} and {next}");
            }
        }

        [TestMethod]
        public void InitialMeshOnlyHasOnlyBoundaryPoints()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));
            Mesh mesh = Generate(shape, 50, true);

            AssertValidMesh(mesh, shape);
            Assert.IsTrue(mesh.Vertices.All(v => v.Point.X == 0 || v.Point.X == 300 || v.Point.Y == 0 || v.Point.Y == 500));
            Assert.AreEqual(2 * (6 + 10), mesh.VerticesCount);
        }

        [TestMethod]
        public void MinimumAngle()
        {
            var shape = new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(300, 0), Q(300, 300), Q(250, 300), Q(250, 50), Q(50, 50), Q(50, 300), Q(0, 300) }));

            Mesh coarse = Generate(shape, 100);
            Mesh quality = Generate(shape, 100, false, 28);

            AssertValidMesh(coarse, shape);
            AssertValidMesh(quality, shape);
            Assert.IsTrue(MinAngle(quality) >= 28 - 1e-6, $"min angle {MinAngle(quality)}");
            Assert.IsTrue(quality.FacesCount >= coarse.FacesCount);
        }

        [TestMethod]
        public void ThinWebIsNotRefinedWithoutMinimumAngle()
        {
            // H section with a thin web: the mesh size rules, the web is not divided in small triangles
            var shape = new Shape2d(new Polygon2d(new[] { Q(-200, 0), Q(200, 0), Q(200, 40), Q(6, 40), Q(6, 460), Q(200, 460), Q(200, 500), Q(-200, 500), Q(-200, 460), Q(-6, 460), Q(-6, 40), Q(-200, 40) }));
            Mesh mesh = Generate(shape, 250);

            AssertValidMesh(mesh, shape);
            Assert.IsTrue(mesh.FacesCount < 60, $"{mesh.FacesCount} triangles");
        }

        [TestMethod]
        public void SelfIntersectingShapeFails()
        {
            var shape = new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(100, 100), Q(100, 0), Q(0, 100) }));
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 10 };

            Assert.IsFalse(DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus status));
            Assert.IsNull(mesh);
            Assert.IsNotNull(status);
            Assert.IsTrue(status.Exceptions.Count > 0);
        }

        [TestMethod]
        public void InitialMeshUsesOnlyTheVertices()
        {
            var shape = new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(100, 0), Q(100, 100), Q(50, 150), Q(0, 100), Q(50, 50) }));

            Assert.IsTrue(InitialMesh.Generate(shape, out Mesh mesh, out _));
            AssertValidMesh(mesh, shape);
            Assert.AreEqual(6, mesh.VerticesCount);
            Assert.AreEqual(4, mesh.FacesCount);
        }

        [TestMethod]
        public void Performance()
        {
            var shape = new Shape2d(Rectangle(0, 0, 1000, 1000), new[] { Rectangle(200, 200, 100, 300) });

            var watch = Stopwatch.StartNew();
            Mesh mesh = Generate(shape, 10);
            watch.Stop();

            AssertValidMesh(mesh, shape);
            Assert.IsTrue(mesh.FacesCount > 10000);
            Assert.IsTrue(watch.ElapsedMilliseconds < 5000, $"{watch.ElapsedMilliseconds} ms for {mesh.FacesCount} triangles");
        }
    }
}
