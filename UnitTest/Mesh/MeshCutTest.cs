using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meshes
{
    /// <summary>
    /// Tests of <see cref="Mesh.Cut"/> (September 2026)
    /// </summary>
    [TestClass]
    public class MeshCutTest
    {
        #region Helpers

        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        /// <summary>
        /// Grid of n x m quadrilaterals of the given size, with the edges of the faces
        /// </summary>
        private static Mesh Grid(int n, int m, double size, Func<double, double, double> z = null)
        {
            var mesh = new Mesh();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double x0 = i * size, y0 = j * size, x1 = x0 + size, y1 = y0 + size;
                    mesh.AddFaceMesh(new[] { P(x0, y0, z?.Invoke(x0, y0) ?? 0), P(x1, y0, z?.Invoke(x1, y0) ?? 0), P(x1, y1, z?.Invoke(x1, y1) ?? 0), P(x0, y1, z?.Invoke(x0, y1) ?? 0) });
                }
            }

            var edges = new HashSet<(int, int)>();
            foreach (MeshFace face in mesh.Faces)
            {
                int[] nodes = face.GetNodes();
                for (int k = 0; k < nodes.Length; k++)
                {
                    var key = (Math.Min(nodes[k], nodes[(k + 1) % nodes.Length]), Math.Max(nodes[k], nodes[(k + 1) % nodes.Length]));
                    if (edges.Add(key))
                        mesh.Edges.Add(new MeshEdge(key.Item1, key.Item2));
                }
            }
            return mesh;
        }

        private static double Side(Line2d line, Point3d p)
        {
            double dx = line.End.X - line.Start.X, dy = line.End.Y - line.Start.Y;
            return (dx * (p.Y - line.Start.Y) - dy * (p.X - line.Start.X)) / Math.Sqrt(dx * dx + dy * dy);
        }

        private static double Area(Point3d[] p)
        {
            double area = 0;
            for (int i = 0; i < p.Length; i++)
                area += p[i].X * p[(i + 1) % p.Length].Y - p[(i + 1) % p.Length].X * p[i].Y;
            return area / 2.0;
        }

        /// <summary>
        /// Every face on one side of the lines, counterclockwise and convex, same area, conforming (the free edges have the length of the boundary)
        /// </summary>
        private static void AssertValidCut(Mesh mesh, double area, double perimeter, params Line2d[] lines)
        {
            double total = 0;
            var edges = new Dictionary<(int, int), int>();
            foreach (MeshFace face in mesh.Faces)
            {
                Point3d[] p = mesh.GetFacePoints(face);
                Assert.IsTrue(Area(p) > 0, "faces counterclockwise");
                total += Area(p);

                for (int k = 0; k < p.Length; k++)
                {
                    Point3d v = p[k], next = p[(k + 1) % p.Length], previous = p[(k + p.Length - 1) % p.Length];
                    double cross = (next.X - v.X) * (previous.Y - v.Y) - (next.Y - v.Y) * (previous.X - v.X);
                    Assert.IsTrue(cross > 0, "convex faces");
                }

                foreach (Line2d line in lines)
                {
                    double min = p.Min(q => Side(line, q)), max = p.Max(q => Side(line, q));
                    Assert.IsTrue(min >= -1e-6 || max <= 1e-6, "a face on both sides of the line");
                }

                int[] nodes = face.GetNodes();
                for (int k = 0; k < nodes.Length; k++)
                {
                    var key = (Math.Min(nodes[k], nodes[(k + 1) % nodes.Length]), Math.Max(nodes[k], nodes[(k + 1) % nodes.Length]));
                    edges[key] = edges.TryGetValue(key, out int count) ? count + 1 : 1;
                }
            }

            Assert.AreEqual(area, total, area * 1e-10);
            Assert.IsTrue(edges.Values.All(c => c <= 2));
            double free = edges.Where(e => e.Value == 1).Sum(e => mesh.Vertices.GetElementById(e.Key.Item1).Point.DistanceTo(mesh.Vertices.GetElementById(e.Key.Item2).Point));
            Assert.AreEqual(perimeter, free, perimeter * 1e-9, "hanging vertices");
        }

        #endregion

        [TestMethod]
        public void LineBetweenTheRowsKeepsTheQuadrilaterals()
        {
            Mesh mesh = Grid(10, 10, 1);
            var line = new Line2d(new Point2d(-5, 4.5), new Point2d(15, 4.5));
            mesh.Cut(line);

            AssertValidCut(mesh, 100, 40, line);
            Assert.IsTrue(mesh.Faces.All(f => f.IsQuad), "a quadrilateral cut through opposite edges gives two quadrilaterals");
            Assert.AreEqual(110, mesh.FacesCount);
            Assert.AreEqual(121 + 11, mesh.VerticesCount);
        }

        [TestMethod]
        public void LineThroughTheVerticesDoesNotAddVertices()
        {
            Mesh mesh = Grid(10, 10, 1);
            var line = new Line2d(new Point2d(-1, -1), new Point2d(20, 20));
            mesh.Cut(line);

            AssertValidCut(mesh, 100, 40, line);
            Assert.AreEqual(121, mesh.VerticesCount);
            Assert.AreEqual(100 + 10, mesh.FacesCount, "the diagonal quadrilaterals are divided in two triangles");

            // vertex on the line within the tolerance: no sliver
            Mesh near = Grid(10, 10, 1);
            var almost = new Line2d(new Point2d(-1, -1 + 1e-6), new Point2d(20, 20 + 1e-6));
            near.Cut(almost);
            Assert.AreEqual(121, near.VerticesCount);
        }

        [TestMethod]
        public void SlantedLineGivesConvexParts()
        {
            Mesh mesh = Grid(10, 10, 1);
            var line = new Line2d(new Point2d(-1, 1), new Point2d(11, 5));
            mesh.Cut(line);

            AssertValidCut(mesh, 100, 40, line);
            Assert.IsTrue(mesh.Faces.Count(f => f.IsQuad) > 90, "most of the parts are quadrilaterals");
        }

        [TestMethod]
        public void CornerCutGivesATriangleAndAPentagonDivided()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(10, 10), P(0, 10) });
            var line = new Line2d(new Point2d(8, 0), new Point2d(10, 2));
            mesh.Cut(line);

            AssertValidCut(mesh, 100, 40, line);
            Assert.AreEqual(3, mesh.FacesCount);
            Assert.AreEqual(1, mesh.Faces.Count(f => f.IsQuad));
        }

        [TestMethod]
        public void RepeatedCutsOfATriangleMesh()
        {
            var shape = new Shape2d(new Polygon2d(new[] { new Point2d(0, 0), new Point2d(200, 0), new Point2d(200, 300), new Point2d(0, 300) }));
            Assert.IsTrue(DelaunayMesh.Generate(shape, new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 20, Recombine = false }, out Mesh mesh, out _));

            var random = new Random(7);
            var lines = new List<Line2d>();
            for (int i = 0; i < 15; i++)
            {
                var line = new Line2d(new Point2d(random.NextDouble() * 200, -10), new Point2d(random.NextDouble() * 200, 310));
                mesh.Cut(line);
                lines.Add(line);
            }

            AssertValidCut(mesh, 200 * 300, 1000, lines.ToArray());
        }

        [TestMethod]
        public void RepeatedCutsOfAQuadMesh()
        {
            var shape = new Shape2d(new Polygon2d(Enumerable.Range(0, 48).Select(i => new Point2d(150 * Math.Cos(2 * Math.PI * i / 48), 150 * Math.Sin(2 * Math.PI * i / 48))).ToArray()));
            Assert.IsTrue(DelaunayMesh.Generate(shape, new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 30 }, out Mesh mesh, out _));
            double area = shape.GetArea();
            double perimeter = 48 * 2 * 150 * Math.Sin(Math.PI / 48);

            var lines = new List<Line2d>();
            for (int i = 0; i < 12; i++)
            {
                double angle = i * 0.7;
                var line = new Line2d(new Point2d(-500 * Math.Cos(angle), -500 * Math.Sin(angle) + i * 7), new Point2d(500 * Math.Cos(angle), 500 * Math.Sin(angle) + i * 7));
                mesh.Cut(line);
                lines.Add(line);
            }

            AssertValidCut(mesh, area, perimeter, lines.ToArray());
        }

        [TestMethod]
        public void NewVerticesInterpolateZ()
        {
            Mesh mesh = Grid(4, 4, 1, (x, y) => 5 + 2 * x);
            mesh.Cut(new Line2d(new Point2d(1.5, -1), new Point2d(1.5, 10)));

            foreach (MeshVertex vertex in mesh.Vertices)
                Assert.AreEqual(5 + 2 * vertex.Point.X, vertex.Point.Z, 1e-12);
        }

        [TestMethod]
        public void TagsAndUncutFacesAreKept()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(10, 0), P(10, 10), P(0, 10) });
            mesh.AddFaceMesh(new[] { P(10, 0), P(20, 0), P(20, 10), P(10, 10) });
            mesh.Faces[0].Tag = "first";
            mesh.Faces[1].Tag = "second";
            MeshFace untouched = mesh.Faces[1];

            mesh.Cut(new Line2d(new Point2d(5, -5), new Point2d(5, 20)));

            Assert.AreEqual(3, mesh.FacesCount);
            Assert.AreEqual(2, mesh.Faces.Count(f => (string)f.Tag == "first"));
            Assert.IsTrue(mesh.Faces.Contains(untouched));
            Assert.AreEqual(mesh.FacesCount, mesh.Faces.Select(f => f.Id).Distinct().Count(), "unique ids");
        }

        [TestMethod]
        public void EdgesAreUpdated()
        {
            Mesh mesh = Grid(4, 4, 1);
            var line = new Line2d(new Point2d(-1, 2.5), new Point2d(10, 2.5));
            mesh.Cut(line);

            var faceEdges = new HashSet<(int, int)>();
            foreach (MeshFace face in mesh.Faces)
            {
                int[] nodes = face.GetNodes();
                for (int k = 0; k < nodes.Length; k++)
                    faceEdges.Add((Math.Min(nodes[k], nodes[(k + 1) % nodes.Length]), Math.Max(nodes[k], nodes[(k + 1) % nodes.Length])));
            }

            var edges = new HashSet<(int, int)>(mesh.Edges.Select(e => (Math.Min(e.A, e.B), Math.Max(e.A, e.B))));
            Assert.IsTrue(faceEdges.SetEquals(edges), "the edges are the ones of the faces");
        }

        [TestMethod]
        public void LineOutsideOrRepeatedDoesNothing()
        {
            Mesh mesh = Grid(5, 5, 1);
            mesh.Cut(new Line2d(new Point2d(-10, 7), new Point2d(10, 7)));
            Assert.AreEqual(25, mesh.FacesCount);
            Assert.AreEqual(36, mesh.VerticesCount);

            var line = new Line2d(new Point2d(0.3, -1), new Point2d(2.7, 9));
            mesh.Cut(line);
            int faces = mesh.FacesCount, vertices = mesh.VerticesCount;
            mesh.Cut(line);
            Assert.AreEqual(faces, mesh.FacesCount);
            Assert.AreEqual(vertices, mesh.VerticesCount);
        }

        [TestMethod]
        public void Performance()
        {
            Mesh mesh = Grid(100, 100, 1);
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 20; i++)
                mesh.Cut(new Line2d(new Point2d(-1, 3.3 + 4.7 * i), new Point2d(101, 1.1 + 4.9 * i)));
            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds < 3000, $"{watch.ElapsedMilliseconds} ms");
            Assert.AreEqual(100 * 100, mesh.GetFaces().Sum(f => mesh.GetFaceArea(f)), 1e-6);
        }
    }
}
