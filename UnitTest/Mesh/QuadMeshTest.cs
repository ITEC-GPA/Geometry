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
    /// Tests of the quadrilateral meshes of <see cref="DelaunayMesh"/> (Recombine, September 2026)
    /// </summary>
    [TestClass]
    public class QuadMeshTest
    {
        #region Helpers

        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static Polygon2d Rectangle(double x, double y, double width, double height)
        {
            return new Polygon2d(new[] { Q(x, y), Q(x + width, y), Q(x + width, y + height), Q(x, y + height) });
        }

        private static Polygon2d Circle(double x, double y, double radius, int sides)
        {
            return new Polygon2d(Enumerable.Range(0, sides).Select(i => Q(x + radius * Math.Cos(2 * Math.PI * i / sides), y + radius * Math.Sin(2 * Math.PI * i / sides))).ToArray());
        }

        private static Polygon2d Rotate(Polygon2d polygon, double degrees)
        {
            double c = Math.Cos(degrees * Math.PI / 180.0), s = Math.Sin(degrees * Math.PI / 180.0);
            return new Polygon2d(polygon.Select(p => Q(p.X * c - p.Y * s, p.X * s + p.Y * c)).ToArray());
        }

        private static Shape2d LShape => new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(300, 0), Q(300, 60), Q(60, 60), Q(60, 400), Q(0, 400) }));

        private static Shape2d TShape => new Shape2d(new Polygon2d(new[] { Q(-300, 400), Q(-300, 300), Q(-60, 300), Q(-60, 0), Q(60, 0), Q(60, 300), Q(300, 300), Q(300, 400) }));

        private static Shape2d ISection => new Shape2d(new Polygon2d(new[] { Q(-200, 0), Q(200, 0), Q(200, 40), Q(6, 40), Q(6, 460), Q(200, 460), Q(200, 500), Q(-200, 500), Q(-200, 460), Q(-6, 460), Q(-6, 40), Q(-200, 40) }));

        private static Shape2d BoxSection => new Shape2d(Rectangle(0, 0, 400, 600), new[] { Rectangle(40, 40, 320, 520) });

        private static Shape2d PlateWithChild => new Shape2d(Rectangle(0, 0, 100, 100), new[] { Rectangle(25, 25, 50, 50) }, new[] { new Shape2d(Rectangle(40, 40, 20, 20)) });

        private static Mesh Generate(Shape2d shape, double meshSize, bool recombineAll = false, bool initialMeshOnly = false, bool refine = false)
        {
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = meshSize, RecombineAll = recombineAll, InitialMeshOnly = initialMeshOnly, Refine = refine };
            Assert.IsTrue(options.Recombine, "the quadrilaterals are the default");
            Assert.IsTrue(DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus status));
            Assert.IsNull(status);
            return mesh;
        }

        private static double SignedArea(Point3d[] p)
        {
            double area = 0;
            for (int i = 0; i < p.Length; i++)
                area += p[i].X * p[(i + 1) % p.Length].Y - p[(i + 1) % p.Length].X * p[i].Y;
            return area / 2.0;
        }

        /// <summary>
        /// Minimum over the corners of 2 |e1 x e2| / (|e1|^2 + |e2|^2): 1 for a square, not positive if not convex
        /// </summary>
        private static double Quality(Point3d[] p)
        {
            double min = double.MaxValue;
            for (int i = 0; i < p.Length; i++)
            {
                Point3d v = p[i], next = p[(i + 1) % p.Length], previous = p[(i + p.Length - 1) % p.Length];
                double e1x = next.X - v.X, e1y = next.Y - v.Y, e2x = previous.X - v.X, e2y = previous.Y - v.Y;
                min = Math.Min(min, 2.0 * (e1x * e2y - e1y * e2x) / (e1x * e1x + e1y * e1y + e2x * e2x + e2y * e2y));
            }
            return min;
        }

        /// <summary>
        /// Minimum sine of the angles of the polygon
        /// </summary>
        private static double MinSine(Point3d[] p)
        {
            double min = double.MaxValue;
            for (int i = 0; i < p.Length; i++)
            {
                Point3d v = p[i], next = p[(i + 1) % p.Length], previous = p[(i + p.Length - 1) % p.Length];
                double e1x = next.X - v.X, e1y = next.Y - v.Y, e2x = previous.X - v.X, e2y = previous.Y - v.Y;
                min = Math.Min(min, (e1x * e2y - e1y * e2x) / (v.DistanceTo(next) * v.DistanceTo(previous)));
            }
            return min;
        }

        private static double Perimeter(Polygon3d polygon)
        {
            double length = 0;
            for (int i = 0; i < polygon.Count; i++)
                length += polygon[i].DistanceTo(polygon[(i + 1) % polygon.Count]);
            return length;
        }

        private static double Perimeter(Shape shape)
        {
            return Perimeter(shape.Fill) + (shape.Holes?.Sum(h => Perimeter(h)) ?? 0) + (shape.Childs?.Sum(c => Perimeter(c)) ?? 0);
        }

        /// <summary>
        /// Elements counterclockwise and convex, area of the shape, conforming mesh (every edge in one or two elements,
        /// the edges in one element are the boundary of the shape: no hanging points)
        /// </summary>
        private static void AssertValidMesh(Mesh mesh, Shape2d shape, bool quadsOnly = false)
        {
            Assert.IsTrue(mesh.FacesCount > 0);
            if (quadsOnly)
                Assert.IsTrue(mesh.Faces.All(f => f.IsQuad), $"{mesh.Faces.Count(f => f.IsTriangle)} triangles");

            var edges = new Dictionary<(int, int), int>();
            double area = 0;
            foreach (MeshFace face in mesh.Faces)
            {
                Point3d[] p = mesh.GetFacePoints(face);
                Assert.IsTrue(SignedArea(p) > 0, "the elements must be counterclockwise");
                if (face.IsQuad)
                    Assert.IsTrue(Quality(p) > 0.02, $"not convex quadrilateral (quality {Quality(p)})");
                area += SignedArea(p);

                int[] v = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };
                for (int k = 0; k < v.Length; k++)
                {
                    var key = (Math.Min(v[k], v[(k + 1) % v.Length]), Math.Max(v[k], v[(k + 1) % v.Length]));
                    edges[key] = edges.TryGetValue(key, out int count) ? count + 1 : 1;
                }
            }

            Assert.AreEqual(shape.GetArea(), area, shape.GetArea() * 1e-10);
            Assert.IsTrue(edges.Values.All(count => count <= 2), "an edge in more than two elements");

            double boundary = edges.Where(e => e.Value == 1).Sum(e => mesh.Vertices.GetElementById(e.Key.Item1).Point.DistanceTo(mesh.Vertices.GetElementById(e.Key.Item2).Point));
            Assert.AreEqual(Perimeter(shape), boundary, Perimeter(shape) * 1e-9, "hanging points: the free edges are not the boundary of the shape");
        }

        private static double MaxEdge(Mesh mesh)
        {
            return mesh.Faces.Max(f =>
            {
                Point3d[] p = mesh.GetFacePoints(f);
                double max = 0;
                for (int i = 0; i < p.Length; i++)
                    max = Math.Max(max, p[i].DistanceTo(p[(i + 1) % p.Length]));
                return max;
            });
        }

        /// <summary>
        /// All the elements are rectangles (right angles)
        /// </summary>
        private static void AssertRectangles(Mesh mesh)
        {
            foreach (MeshFace face in mesh.Faces)
            {
                Assert.IsTrue(face.IsQuad);
                Point3d[] p = mesh.GetFacePoints(face);
                for (int i = 0; i < 4; i++)
                {
                    Point3d v = p[i], next = p[(i + 1) % 4], previous = p[(i + 3) % 4];
                    double dot = (next.X - v.X) * (previous.X - v.X) + (next.Y - v.Y) * (previous.Y - v.Y);
                    Assert.AreEqual(0, dot / (next.DistanceTo(v) * previous.DistanceTo(v)), 1e-9, "not a right angle");
                }
            }
        }

        #endregion

        [TestMethod]
        public void RectangleIsDividedInSquares()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));
            Mesh mesh = Generate(shape, 50);

            AssertValidMesh(mesh, shape, true);
            AssertRectangles(mesh);
            Assert.AreEqual(60, mesh.FacesCount);
            Assert.AreEqual(7 * 11, mesh.VerticesCount);
            Assert.IsTrue(mesh.Faces.All(f => Math.Abs(Quality(mesh.GetFacePoints(f)) - 1) < 1e-9), "squares");
        }

        [TestMethod]
        public void RectangleNotDivisibleByTheMeshSize()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));

            foreach (double size in new[] { 37.0, 150.0, 333.0 })
            {
                Mesh mesh = Generate(shape, size);
                AssertValidMesh(mesh, shape, true);
                AssertRectangles(mesh);
                Assert.IsTrue(MaxEdge(mesh) <= size * (1 + 1e-9), $"mesh size {size}: max edge {MaxEdge(mesh)}");

                // the smallest number of rectangles not larger than the mesh size
                int expected = (int)Math.Ceiling(300 / size) * (int)Math.Ceiling(500 / size);
                Assert.AreEqual(expected, mesh.FacesCount, $"mesh size {size}");
            }
        }

        [TestMethod]
        public void RectilinearShapesAreDividedInRectangles()
        {
            foreach (var (shape, size) in new[] { (LShape, 40.0), (TShape, 50.0), (ISection, 50.0), (BoxSection, 50.0), (PlateWithChild, 10.0) })
            {
                Mesh mesh = Generate(shape, size);
                AssertValidMesh(mesh, shape, true);
                AssertRectangles(mesh);
                Assert.IsTrue(MaxEdge(mesh) <= size * (1 + 1e-9), $"max edge {MaxEdge(mesh)} with mesh size {size}");
            }
        }

        [TestMethod]
        public void ThinPartsAreNotRefined()
        {
            // the web (12 wide) is one rectangle wide, the mesh size rules along it
            Mesh mesh = Generate(ISection, 50);
            Assert.IsTrue(mesh.FacesCount <= 30, $"{mesh.FacesCount} elements");

            Mesh coarse = Generate(ISection, 250);
            AssertValidMesh(coarse, ISection, true);
            Assert.IsTrue(coarse.FacesCount <= 12, $"{coarse.FacesCount} elements");
        }

        [TestMethod]
        public void RotatedRectangleFollowsItsEdges()
        {
            var shape = new Shape2d(Rotate(Rectangle(0, 0, 300, 500), 30));
            Mesh mesh = Generate(shape, 50);

            AssertValidMesh(mesh, shape, true);
            AssertRectangles(mesh);
            Assert.AreEqual(60, mesh.FacesCount);
        }

        [TestMethod]
        public void TubeIsDividedInQuadrilaterals()
        {
            var shape = new Shape2d(Circle(0, 0, 150, 48), new[] { Circle(0, 0, 120, 48) });
            Mesh mesh = Generate(shape, 20);

            AssertValidMesh(mesh, shape, true);
            Assert.IsTrue(mesh.Faces.All(f => Quality(mesh.GetFacePoints(f)) > 0.9), $"worst quality {mesh.Faces.Min(f => Quality(mesh.GetFacePoints(f)))}");
        }

        [TestMethod]
        public void CircleIsMostlyQuadrilaterals()
        {
            var shape = new Shape2d(Circle(0, 0, 300, 64));
            Mesh mesh = Generate(shape, 50);

            AssertValidMesh(mesh, shape);
            int quads = mesh.Faces.Count(f => f.IsQuad);
            Assert.IsTrue(quads >= 0.75 * mesh.FacesCount, $"{quads} quadrilaterals of {mesh.FacesCount}");
            Assert.IsTrue(mesh.Faces.Where(f => f.IsQuad).All(f => MinSine(mesh.GetFacePoints(f)) >= 0.29), "angles between 17 and 163 degrees");
            Assert.IsTrue(mesh.Faces.Count(f => f.IsQuad && Quality(mesh.GetFacePoints(f)) > 0.95) >= 0.4 * quads, "squares inside");
            Assert.IsTrue(MaxEdge(mesh) <= 1.5 * 50, $"max edge {MaxEdge(mesh)}");
        }

        [TestMethod]
        public void RecombineAllGivesOnlyQuadrilaterals()
        {
            var shapes = new[]
            {
                new Shape2d(Circle(0, 0, 300, 64)),
                new Shape2d(Circle(0, 0, 150, 32)),
                new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(400, 0), Q(300, 300), Q(100, 300) })),
                new Shape2d(Rectangle(0, 0, 500, 500), new[] { Circle(250, 250, 100, 32) }),
            };

            foreach (Shape2d shape in shapes)
            {
                Mesh mesh = Generate(shape, 50, recombineAll: true);
                AssertValidMesh(mesh, shape, true);
                Assert.IsTrue(MaxEdge(mesh) <= 1.5 * 50, $"max edge {MaxEdge(mesh)}");
            }

            // the rectilinear shapes do not change
            Mesh box = Generate(BoxSection, 50, recombineAll: true);
            AssertRectangles(box);
            Assert.AreEqual(Generate(BoxSection, 50).FacesCount, box.FacesCount);
        }

        [TestMethod]
        public void WithoutRecombineTheMeshIsMadeOfTriangles()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 50, Recombine = false };
            Assert.IsTrue(DelaunayMesh.Generate(shape, options, out Mesh mesh, out _));

            Assert.IsTrue(mesh.Faces.All(f => f.IsTriangle));
            AssertValidMesh(mesh, shape);
        }

        [TestMethod]
        public void InitialMeshOnlyHasOnlyBoundaryPoints()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));
            Mesh mesh = Generate(shape, 50, initialMeshOnly: true);

            AssertValidMesh(mesh, shape);
            Assert.AreEqual(2 * (6 + 10), mesh.VerticesCount);
            Assert.IsTrue(mesh.Vertices.All(v => v.Point.X == 0 || v.Point.X == 300 || v.Point.Y == 0 || v.Point.Y == 500));
            Assert.IsTrue(mesh.Faces.Any(f => f.IsQuad));
        }

        [TestMethod]
        public void DefaultMeshSizeUsesOnlyTheVerticesOfTheShape()
        {
            var rectangle = new Shape2d(Rectangle(0, 0, 300, 500));
            Mesh one = Generate(rectangle, 1E+22);
            AssertValidMesh(one, rectangle, true);
            Assert.AreEqual(1, one.FacesCount);
            Assert.AreEqual(4, one.VerticesCount);

            Mesh l = Generate(LShape, 1E+22);
            AssertValidMesh(l, LShape);
            Assert.AreEqual(6, l.VerticesCount);
        }

        [TestMethod]
        public void UnitsAndPositionDoNotMatter()
        {
            foreach (bool all in new[] { false, true })
            {
                Mesh millimetres = Generate(new Shape2d(Circle(0, 0, 150, 32)), 50, all);
                Mesh metres = Generate(new Shape2d(Circle(0, 0, 0.15, 32)), 0.05, all);
                Mesh farFromOrigin = Generate(new Shape2d(Circle(100000, -250000, 150, 32)), 50, all);

                Assert.AreEqual(millimetres.FacesCount, metres.FacesCount);
                Assert.AreEqual(millimetres.FacesCount, farFromOrigin.FacesCount);
                Assert.AreEqual(millimetres.Faces.Count(f => f.IsQuad), metres.Faces.Count(f => f.IsQuad));
                AssertValidMesh(farFromOrigin, new Shape2d(Circle(100000, -250000, 150, 32)), all);
            }
        }

        [TestMethod]
        public void VerticesOfTheShapeKeepTheirCoordinatesAndTags()
        {
            var polygon = new Polygon2d(new[] { Q(0.1, 0.2), Q(300.3, 0.7), Q(310.9, 500.1), Q(-10.3, 480.7) });
            var shape = new Shape2d(polygon);

            foreach (bool all in new[] { false, true })
            {
                Mesh mesh = Generate(shape, 37.3, all);
                AssertValidMesh(mesh, shape, all);
                foreach (Point2d point in polygon)
                {
                    MeshVertex vertex = mesh.Vertices.FirstOrDefault(v => v.Point.X == point.X && v.Point.Y == point.Y && v.Point.Z == 0);
                    Assert.IsNotNull(vertex, $"vertex {point} missing");
                    Assert.IsTrue(vertex.Tag is int tag && tag >= 0, "the points of the boundary have the index as tag");
                }
            }
        }

        [TestMethod]
        public void RefineDividesTheQuadrilaterals()
        {
            var shape = new Shape2d(Rectangle(0, 0, 300, 500));
            Mesh mesh = Generate(shape, 50, refine: true);

            AssertValidMesh(mesh, shape, true);
            AssertRectangles(mesh);
            Assert.AreEqual(4 * 60, mesh.FacesCount);
        }

        [TestMethod]
        public void OptionsCloneKeepsRecombineAll()
        {
            var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 12, RecombineAll = true, Recombine = true, MinAngle = 15 };
            var clone = (DelaunayMesh.DelaunayGenerateOptions)options.Clone();

            Assert.IsTrue(clone.RecombineAll);
            Assert.IsTrue(clone.Recombine);
            Assert.AreEqual(12, clone.MeshSize);
            Assert.AreEqual(15, clone.MinAngle);
            Assert.IsFalse(new DelaunayMesh.DelaunayGenerateOptions().RecombineAll);
        }

        [TestMethod]
        public void RandomShapesGiveValidMeshes()
        {
            var random = new Random(20260924);
            for (int i = 0; i < 150; i++)
            {
                Shape2d shape = RandomShape(random);
                double size = Math.Sqrt(shape.GetArea()) / (2 + random.Next(15));

                foreach (bool all in new[] { false, true })
                {
                    Mesh mesh = Generate(shape, size, all);
                    AssertValidMesh(mesh, shape, all);
                }
            }
        }

        [TestMethod]
        public void Performance()
        {
            var shape = new Shape2d(Rectangle(0, 0, 1000, 1000), new[] { Rectangle(200, 200, 100, 300), Circle(700, 700, 100, 64) });

            var watch = Stopwatch.StartNew();
            Mesh mesh = Generate(shape, 10);
            watch.Stop();

            AssertValidMesh(mesh, shape);
            Assert.IsTrue(mesh.FacesCount > 8000);
            Assert.IsTrue(mesh.Faces.Count(f => f.IsQuad) > 0.95 * mesh.FacesCount);
            Assert.IsTrue(watch.ElapsedMilliseconds < 5000, $"{watch.ElapsedMilliseconds} ms for {mesh.FacesCount} elements");
        }

        private static Shape2d RandomShape(Random random)
        {
            switch (random.Next(4))
            {
                case 0:
                    {
                        // star-shaped polygon
                        int n = 5 + random.Next(20);
                        return new Shape2d(new Polygon2d(Enumerable.Range(0, n).Select(i =>
                        {
                            double angle = 2 * Math.PI * i / n, radius = 50 + random.NextDouble() * 100;
                            return Q(radius * Math.Cos(angle), radius * Math.Sin(angle));
                        }).ToArray()));
                    }
                case 1:
                    {
                        // rectilinear staircase
                        var points = new List<Point2d> { Q(0, 0) };
                        double x = 0, y = 0;
                        for (int i = 0, steps = 2 + random.Next(5); i < steps; i++)
                        {
                            x += 20 + random.Next(200);
                            points.Add(Q(x, y));
                            y += 20 + random.Next(200);
                            points.Add(Q(x, y));
                        }
                        points.Add(Q(0, y));
                        return new Shape2d(new Polygon2d(points.ToArray()));
                    }
                case 2:
                    {
                        // rectangle, rotated or with holes
                        double width = 200 + random.Next(600), height = 200 + random.Next(600);
                        var holes = new List<Polygon2d>();
                        if (random.Next(2) == 0)
                            holes.Add(Rectangle(width * 0.2, height * 0.2, width * 0.25, height * 0.25));
                        if (random.Next(2) == 0)
                            holes.Add(Circle(width * 0.7, height * 0.7, Math.Min(width, height) * 0.12, 8 + random.Next(40)));
                        if (holes.Count == 0)
                            return new Shape2d(Rotate(Rectangle(0, 0, width, height), random.Next(90)));
                        return new Shape2d(Rectangle(0, 0, width, height), holes.ToArray());
                    }
                default:
                    {
                        // circle or tube
                        double radius = 50 + random.Next(300);
                        int sides = 12 + random.Next(60);
                        return random.Next(2) == 0
                            ? new Shape2d(Circle(0, 0, radius, sides))
                            : new Shape2d(Circle(0, 0, radius, sides), new[] { Circle(0, 0, radius * (0.5 + 0.4 * random.NextDouble()), sides) });
                    }
            }
        }
    }
}
