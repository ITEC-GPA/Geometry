using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meshes.GMsh
{
    /// <summary>
    /// Corrections of GMesh of September 2026: Gmsh finalized on every exit, calls serialized, embedded geometries across the sides of the
    /// surfaces made by Fragment, faces of the embedded shapes
    /// </summary>
    [TestClass]
    public class GMeshReviewTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        private static Polygon3d Rectangle(double x0, double y0, double x1, double y1) => new Polygon3d { P(x0, y0), P(x1, y0), P(x1, y1), P(x0, y1) };

        private static GMesh.GMeshGenerateOptions Options(double meshSize) => new GMesh.GMeshGenerateOptions
        {
            Algorithm = GMesh.GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
            Recombine = true,
            RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
            MeshSize = meshSize,
            UseGlobalProgressID = true,
            HealShapes = true,
            Transfinite = true
        };

        private static double Area(Mesh mesh) => mesh.Faces.Sum(f => mesh.GetFaceArea(f));

        /// <summary>
        /// A plate 100 x 100 divided in two surfaces (x lower and bigger than 50) by a vertical wall
        /// </summary>
        private static List<Shape> PlateAndWall(out Shape plate)
        {
            plate = new Shape(Rectangle(0, 0, 100, 100));
            var wall = new Shape(new Polygon3d { P(50, -10, -10), P(50, 110, -10), P(50, 110, 10), P(50, -10, 10) });
            return new List<Shape> { plate, wall };
        }

        [TestMethod]
        public void OptionsCloneKeepsTransfiniteSurface()
        {
            var options = new GMesh.GMeshGenerateOptions { TransfiniteSurface = true };
            Assert.IsTrue(((GMesh.GMeshGenerateOptions)options.Clone()).TransfiniteSurface, "it was not copied");
        }

        [TestMethod]
        public void GenerateWorksAfterAnException()
        {
            var shapes = new List<Shape> { new Shape(Rectangle(0, 0, 100, 100)) };

            // an embedded geometry not supported: the exception is thrown after the initialization of Gmsh (before, Gmsh was not finalized)
            var embedded = new Dictionary<Shape, GeometryBase[]> { [shapes[0]] = new GeometryBase[] { new Vector3d(1, 0, 0) } };
            Assert.ThrowsException<ArgumentException>(() => GMesh.Generate(shapes, embedded, null, Options(10), out _, out _));

            Assert.IsTrue(GMesh.Generate(shapes, Options(10), out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Assert.AreEqual(1, meshes.Count);
            Assert.AreEqual(10000, Area(meshes[0]), 1e-6);
        }

        [TestMethod]
        public void ParallelCallsAreSerialized()
        {
            // Gmsh has a global state: the calls from more threads are executed one at a time
            var meshes = new List<Mesh>[6];
            var results = new bool[6];
            Parallel.For(0, meshes.Length, i =>
            {
                results[i] = GMesh.Generate(new List<Shape> { new Shape(Rectangle(0, 0, 100 + 10 * i, 100)) }, Options(10), out meshes[i], out _);
            });

            for (int i = 0; i < meshes.Length; i++)
            {
                Assert.IsTrue(results[i]);
                Assert.AreEqual(1, meshes[i].Count);
                Assert.AreEqual((100 + 10 * i) * 100, Area(meshes[i][0]), 1e-6);
            }
        }

        [TestMethod]
        public void LineAcrossTheSideOfTheFragmentsIsEmbedded()
        {
            List<Shape> shapes = PlateAndWall(out Shape plate);
            var line = new Line3d(P(20, 30), P(80, 70));
            var embedded = new Dictionary<Shape, GeometryBase[]> { [plate] = new GeometryBase[] { line } };

            // before, the line was split only at the first crossing and the part ended on the side without a vertex there
            Assert.IsTrue(GMesh.Generate(shapes, embedded, null, Options(5), out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Mesh mesh = meshes[0];
            Assert.AreEqual(10000, Area(mesh), 1e-6);

            int[] nodes = status.EmbeddedGeometriesVertexMap[mesh].Values.Single();
            Point3d[] points = nodes.Select(id => mesh.Vertices.GetElementById(id).Point).ToArray();
            Assert.IsTrue(points.Length >= 3);
            Assert.IsTrue(points.All(p => line.IsPointOnLine(p, 1e-6)), "the nodes are on the line");
            Assert.IsTrue(points.First().DistanceTo(line.Start) < 1e-6 && points.Last().DistanceTo(line.End) < 1e-6, "from the start to the end");
            Assert.IsTrue(points.Any(p => Math.Abs(p.X - 50) < 1e-6), "a node on the side between the surfaces");

            // the nodes of the line are nodes of the faces
            var faceNodes = new HashSet<int>(mesh.Faces.SelectMany(f => f.GetNodes()));
            Assert.IsTrue(nodes.All(faceNodes.Contains));
        }

        [TestMethod]
        public void PointOnTheSideOfTheFragmentsIsAVertex()
        {
            List<Shape> shapes = PlateAndWall(out Shape plate);
            var point = P(50, 20);
            var embedded = new Dictionary<Shape, GeometryBase[]> { [plate] = new GeometryBase[] { point } };

            Assert.IsTrue(GMesh.Generate(shapes, embedded, null, Options(5), out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Mesh mesh = meshes[0];
            Assert.AreEqual(10000, Area(mesh), 1e-6);

            int node = status.EmbeddedGeometriesVertexMap[mesh].Values.Single().Single();
            Assert.AreEqual(0, mesh.Vertices.GetElementById(node).Point.DistanceTo(point), 1e-6);
            Assert.IsTrue(mesh.Faces.Any(f => f.GetNodes().Contains(node)), "the point is a node of the faces");
        }

        [TestMethod]
        public void EmbeddedShapeFacesAreTheOnesOfTheShape()
        {
            var plate = new Shape(Rectangle(0, 0, 100, 100));
            var square = new Shape(Rectangle(60, 60, 80, 80));
            var embedded = new Dictionary<Shape, GeometryBase[]> { [plate] = new GeometryBase[] { square } };

            Assert.IsTrue(GMesh.Generate(new List<Shape> { plate }, embedded, null, Options(5), out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Mesh mesh = meshes[0];

            // before, every face was mapped to the id of the previous face
            int[] faces = status.EmbeddedGeometriesVertexMap[mesh].Values.Single();
            Assert.IsTrue(faces.Length > 0);
            double area = 0;
            foreach (int id in faces)
            {
                MeshFace face = mesh.Faces.GetElementById(id);
                Point3d centroid = mesh.GetFaceCentroid(face);
                Assert.IsTrue(centroid.X > 60 && centroid.X < 80 && centroid.Y > 60 && centroid.Y < 80, $"face {id} out of the embedded shape");
                area += mesh.GetFaceArea(face);
            }
            Assert.AreEqual(400, area, 1e-6);
        }

        [TestMethod]
        public void LinesOfTwoShapesCrossingTheSideAtTheSamePoint()
        {
            // a plate (z = 0) and a wall (x = 0) crossing along the y axis; a line of each one crosses the axis at the origin
            var plate = new Shape(new Polygon3d { P(100, 100), P(-100, 100), P(-100, -100), P(100, -100) });
            var wall = new Shape(new Polygon3d { P(0, 100, 100), P(0, 100, -100), P(0, -100, -100), P(0, -100, 100) });
            var plateLine = new Line3d(P(50, 50), P(-50, -50));
            var wallLine = new Line3d(P(0, -50, -50), P(0, 50, 50));
            var embedded = new Dictionary<Shape, GeometryBase[]> { [plate] = new GeometryBase[] { plateLine }, [wall] = new GeometryBase[] { wallLine } };

            // before, the vertex made for the first line was 1.5e-7 from the origin and the second line was not split there: it was embedded
            // in one half of the wall across the other one (faces of area ~0)
            Assert.IsTrue(GMesh.Generate(new List<Shape> { plate, wall }, embedded, null, Options(5), out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Assert.AreEqual(2, meshes.Count);
            foreach (Mesh mesh in meshes)
            {
                Assert.AreEqual(40000, Area(mesh), 1e-4);
                double minimum = mesh.Faces.Min(f => mesh.GetFaceArea(f));
                Assert.IsTrue(minimum > 1, $"smallest face {minimum}");
            }

            // both lines have a node at the origin
            foreach (Mesh mesh in meshes)
            {
                int[] nodes = status.EmbeddedGeometriesVertexMap[mesh].Values.Single();
                Assert.IsTrue(nodes.Any(id => mesh.Vertices.GetElementById(id).Point.DistanceTo(P(0, 0)) < 1e-5), "node at the origin");
            }
        }

        [TestMethod]
        public void BlossomRecombinationPreservesCrossingConstraints()
        {
            // Gmsh 4.13 required the Simple fallback here; 4.15 can recombine directly.
            // Test the mesh contract, not the presence of an upstream algorithm failure.
            var plate = new Shape(Rectangle(0, 0, 1000, 1000));
            var embedded = new Dictionary<Shape, GeometryBase[]>
            {
                [plate] = new GeometryBase[] { new Line3d(P(0, 0), P(1000, 1000)), new Line3d(P(1000, 0), P(0, 1000)) }
            };
            GMesh.GMeshGenerateOptions options = Options(50);
            options.RecombinationAlgorithm = GMesh.GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom;

            // before, the generation failed
            Assert.IsTrue(GMesh.Generate(new List<Shape> { plate }, embedded, null, options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus status), status.GetLastCustomErrorMessage());
            Assert.AreEqual(1000000, Area(meshes[0]), 1e-4);
            foreach (int[] nodes in status.EmbeddedGeometriesVertexMap[meshes[0]].Values)
                Assert.IsTrue(nodes.Any(id => meshes[0].Vertices.GetElementById(id).Point.DistanceTo(P(500, 500)) < 1e-6), "both constraints contain the crossing node");
            int quads = meshes[0].Faces.Count(f => f.IsQuad);
            Assert.IsTrue(quads > meshes[0].FacesCount / 2, $"the mesh is recombined: {quads} quads of {meshes[0].FacesCount} faces");
        }
    }
}
