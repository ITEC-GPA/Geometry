using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    /// <summary>
    /// Tests of the corrections of the review of September 2026 (boolean operations, mesh methods)
    /// </summary>
    [TestClass]
    public class GeometryReviewTest
    {
        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        private static Polygon2d Square(double x, double y, double size) => new Polygon2d(new[] { Q(x, y), Q(x + size, y), Q(x + size, y + size), Q(x, y + size) });

        private static Polygon2d Circle(double radius, int sides) => new Polygon2d(Enumerable.Range(0, sides).Select(i => Q(radius * Math.Cos(2 * Math.PI * i / sides), radius * Math.Sin(2 * Math.PI * i / sides))).ToArray());

        #region Boolean operations (Clipper)

        [TestMethod]
        public void BooleanKeepsThePrecisionInMetres()
        {
            // circle of radius 0.15 m: the coordinates were rounded to 1 mm
            var circle = new Shape2d(Circle(0.15, 64));
            var square = new Shape2d(Square(-1, -1, 2));

            Shape2d[] result = Shape2d.Intersection(circle, square);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(circle.GetArea(), result[0].GetArea(), 1e-8 * circle.GetArea());

            Polygon2d[] polygons = Polygon2d.Intersection(Circle(0.15, 64), Square(-1, -1, 2));
            Assert.AreEqual(1, polygons.Length);
            Assert.AreEqual(circle.GetArea(), new Shape2d(polygons[0]).GetArea(), 1e-8 * circle.GetArea());
        }

        [TestMethod]
        public void BooleanRoundsTheCoordinates()
        {
            // the truncation moved the coordinates towards zero
            var rectangle = new Shape2d(new Polygon2d(new[] { Q(-3.14159, -2.71828), Q(3.14159, -2.71828), Q(3.14159, 2.71828), Q(-3.14159, 2.71828) }));
            var big = new Shape2d(Square(-10, -10, 20));

            Shape2d[] result = Shape2d.Intersection(rectangle, big);
            Assert.AreEqual(1, result.Length);
            foreach (Point3d point in result[0].Fill)
            {
                Assert.AreEqual(3.14159, Math.Abs(point.X), 1e-9);
                Assert.AreEqual(2.71828, Math.Abs(point.Y), 1e-9);
            }
        }

        [TestMethod]
        public void UnionKeepsTheIslandsInsideTheHoles()
        {
            var ring = new Shape2d(Square(0, 0, 100), new[] { Square(20, 20, 60) });
            var island = new Shape2d(Square(40, 40, 20));

            Shape2d[] result = Shape2d.Union(ring, island);
            Assert.AreEqual(2, result.Length, "the island was lost");
            Assert.AreEqual(100 * 100 - 60 * 60 + 20 * 20, result.Sum(s => s.GetArea()), 1e-6);

            Assert.IsTrue(Shape.Union(ring, island, out Shape[] shapes));
            Assert.AreEqual(2, shapes.Length);
            Assert.AreEqual(100 * 100 - 60 * 60 + 20 * 20, shapes.Sum(s => s.GetArea()), 1e-6);
        }

        #endregion

        #region Mesh

        [TestMethod]
        public void AddVertexFindsTheVerticesAddedLater()
        {
            var mesh = new Mesh();
            int a = mesh.AddVertex(new MeshVertex(P(0, 0)));
            int b = mesh.AddVertex(new MeshVertex(P(10, 0)));

            // before, the spatial index was not updated: the vertex added after the first search was not found
            Assert.AreEqual(b, mesh.AddVertex(new MeshVertex(P(10, 1e-6))));
            Assert.AreEqual(a, mesh.AddVertex(new MeshVertex(P(1e-6, 0))));
            Assert.AreEqual(2, mesh.VerticesCount);

            // the closest vertex is returned
            int c = mesh.AddVertex(new MeshVertex(P(0.5, 0)));
            Assert.AreEqual(c, mesh.AddVertex(new MeshVertex(P(0.4, 0)), 0.45));
            Assert.AreEqual(a, mesh.AddVertex(new MeshVertex(P(0.1, 0)), 0.45));
        }

        [TestMethod]
        public void FaceAreaCentroidAndEdgeLength()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0, 5), P(4, 0, 5), P(3, 2, 5), P(1, 2, 5) }); // trapezoid
            mesh.AddFaceMesh(new[] { P(10, 0), P(13, 0), P(10, 6) });                 // triangle

            MeshFace trapezoid = mesh.Faces[0], triangle = mesh.Faces[1];
            Assert.AreEqual(6, mesh.GetFaceArea(trapezoid), 1e-12);
            Assert.AreEqual(9, mesh.GetFaceArea(triangle), 1e-12);

            Point3d centroid = mesh.GetFaceCentroid(trapezoid);
            Assert.AreEqual(2, centroid.X, 1e-12);
            Assert.AreEqual(2.0 * (4 + 2 * 2) / (3.0 * (4 + 2)), centroid.Y, 1e-12);
            Assert.AreEqual(5, centroid.Z, 1e-12);

            Point3d triangleCentroid = mesh.GetFaceCentroid(triangle);
            Assert.AreEqual(11, triangleCentroid.X, 1e-12);
            Assert.AreEqual(2, triangleCentroid.Y, 1e-12);

            MeshEdge edge = mesh.Edges.First(e => e.A == trapezoid.A && e.B == trapezoid.B);
            Assert.AreEqual(4, mesh.GetEdgeLength(edge), 1e-12);

            // not planar quadrilateral: before, the Polygon3d threw an exception
            var twisted = new Mesh();
            twisted.AddFaceMesh(new[] { P(0, 0, 0), P(1, 0, 0), P(1, 1, 0.3), P(0, 1, 0) });
            Assert.IsTrue(twisted.GetFaceArea(twisted.Faces[0]) > 0.9);
        }

        [TestMethod]
        public void RefineIsConformingAndKeepsTheTags()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(2, 0), P(2, 2), P(0, 2) });
            mesh.AddFaceMesh(new[] { P(2, 0), P(4, 0), P(2, 2) });
            mesh.AddFaceMesh(new[] { P(4, 0), P(4, 2), P(2, 2) });
            mesh.Faces[0].Tag = "quad";
            mesh.Faces[1].Tag = "triangle";

            Mesh copy = Mesh.RefineMesh(mesh);
            Assert.AreEqual(3, mesh.FacesCount, "RefineMesh returns a copy");

            mesh.Refine();
            Assert.AreEqual(12, mesh.FacesCount);
            Assert.AreEqual(4, mesh.Faces.Count(f => (string)f.Tag == "quad"));
            Assert.AreEqual(4, mesh.Faces.Count(f => (string)f.Tag == "triangle"));
            Assert.AreEqual(6 + 8 + 1, mesh.VerticesCount, "the midpoints of the shared edges are shared");
            Assert.AreEqual(12, copy.FacesCount);
            Assert.AreEqual(mesh.VerticesCount, copy.VerticesCount);

            double area = mesh.GetFaces().Sum(f => mesh.GetFaceArea(f));
            Assert.AreEqual(8, area, 1e-12);

            // the edges are the ones of the faces, each once
            var faceEdges = new HashSet<(int, int)>();
            foreach (MeshFace face in mesh.Faces)
            {
                int[] nodes = face.GetNodes();
                for (int k = 0; k < nodes.Length; k++)
                    faceEdges.Add((Math.Min(nodes[k], nodes[(k + 1) % nodes.Length]), Math.Max(nodes[k], nodes[(k + 1) % nodes.Length])));
            }
            var edges = mesh.Edges.Select(e => (Math.Min(e.A, e.B), Math.Max(e.A, e.B))).ToList();
            Assert.AreEqual(edges.Count, edges.Distinct().Count());
            Assert.IsTrue(faceEdges.SetEquals(edges));
        }

        [TestMethod]
        public void ExistsMethodsCompareTheNodes()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(1, 0), P(1, 1), P(0, 1) });
            MeshFace face = mesh.Faces[0];

            Assert.IsTrue(mesh.FaceExists(new[] { face.C, face.D, face.A, face.B }));
            Assert.IsFalse(mesh.FaceExists(new[] { face.A, face.B, face.C }));
            Assert.IsTrue(mesh.EdgeExists(face.B, face.A));
            Assert.IsFalse(mesh.EdgeExists(face.A, face.C));

            Mesh volumes = mesh.Extrude(1);
            int[] nodes = volumes.Volumes[0].GetNodes();
            Assert.IsTrue(volumes.VolumeExists(nodes.Reverse().ToArray()));
            Assert.IsFalse(volumes.VolumeExists(nodes.Take(6).ToArray()));
        }

        [TestMethod]
        public void MeshVolumeEqualsAnotherTypeIsFalse()
        {
            var volume = new MeshVolume(0, 1, 2, 3, 4, 5);
            Assert.IsFalse(volume.Equals("a string")); // before: infinite recursion
            Assert.IsTrue(volume.Equals(new MeshVolume(0, 1, 2, 3, 4, 5)));
        }

        [TestMethod]
        public void ExtrudeKeepsAllTheEdges()
        {
            var mesh = new Mesh();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    mesh.AddFaceMesh(new[] { P(i, j), P(i + 1, j), P(i + 1, j + 1), P(i, j + 1) });

            Mesh volumes = mesh.Extrude(2);
            Assert.AreEqual(9, volumes.VolumesCount);

            // bottom and top: 24 edges each, 16 vertical edges; before, edges with the same hash code were lost
            Assert.AreEqual(2 * 24 + 16, volumes.EdgesCount);
            Assert.AreEqual(volumes.EdgesCount, volumes.Edges.Select(e => (Math.Min(e.A, e.B), Math.Max(e.A, e.B))).Distinct().Count());
        }

        [TestMethod]
        public void AddRangeEnumeratesOnce()
        {
            var collection = new MeshBaseCollection<MeshVertex>();
            int created = 0;
            IEnumerable<MeshVertex> vertices = Enumerable.Range(0, 50).Select(i => { created++; return new MeshVertex(P(i, 0)); });

            int[] ids = collection.AddRange(vertices);

            Assert.AreEqual(50, created, "before, Count() and ElementAt(i) enumerated the items at every step");
            Assert.AreEqual(50, collection.Count);
            CollectionAssert.AreEqual(Enumerable.Range(0, 50).ToArray(), ids);
            for (int i = 0; i < 50; i++)
                Assert.AreEqual(i, collection.GetElementById(ids[i]).Point.X);
        }

        #endregion
    }
}
