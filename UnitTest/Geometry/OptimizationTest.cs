using System;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    /// <summary>
    /// Tests of the optimized methods (September 2026): same results of the previous implementation
    /// </summary>
    [TestClass]
    public class OptimizationTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        // point (u, v) of a tilted plane
        private static Point3d OnPlane(double u, double v) => new Point3d(10 + 0.8 * u + 0.1 * v, -5 + 0.6 * u - 0.2 * v, 3 + 0.97 * v);

        #region Mesh.AddFaceMesh

        [TestMethod]
        public void AddFaceMeshMergesTheVerticesOfAdjacentFaces()
        {
            var mesh = new Mesh();
            int n = 10;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    mesh.AddFaceMesh(new[] { P(i, j), P(i + 1, j), P(i + 1, j + 1), P(i, j + 1) });

            Assert.AreEqual(n * n, mesh.FacesCount);
            Assert.AreEqual((n + 1) * (n + 1), mesh.VerticesCount);
        }

        [TestMethod]
        public void AddFaceMeshUsesTheTolerance()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { new MeshVertex(P(0, 0)), new MeshVertex(P(1, 0)), new MeshVertex(P(0, 1)) }, 1e-3);
            mesh.AddFaceMesh(new[] { new MeshVertex(P(1.0005, 0)), new MeshVertex(P(1, 1)), new MeshVertex(P(0, 1.002)) }, 1e-3);

            // (1.0005, 0) is merged with (1, 0), (0, 1.002) is farther than the tolerance from (0, 1)
            Assert.AreEqual(5, mesh.VerticesCount);
            MeshFace second = mesh.Faces[1];
            Assert.AreEqual(mesh.Faces[0].B, second.A);
        }

        [TestMethod]
        public void AddFaceMeshDoesNotMergeTheVerticesOfTheSameFace()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(0, 0), P(1, 0), P(0, 1) });

            Assert.AreEqual(4, mesh.VerticesCount);
        }

        [TestMethod]
        public void AddFaceMeshAfterMoveAndAfterOtherChanges()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(1, 0), P(0, 1) });
            mesh.Move(10, 0, 0);
            mesh.AddFaceMesh(new[] { P(11, 0), P(11, 1), P(10, 1) });
            Assert.AreEqual(4, mesh.VerticesCount, "the vertices moved must be found in their new position");

            int id = mesh.Vertices.Add(new MeshVertex(P(20, 20)));
            mesh.AddFaceMesh(new[] { P(20, 20), P(21, 20), P(20, 21) });
            Assert.AreEqual(id, mesh.Faces[2].A, "a vertex added directly to the collection must be found");
        }

        #endregion

        #region Polygon3d

        [TestMethod]
        public void Polygon3dAddOnATiltedPlane()
        {
            var polygon = new Polygon3d();
            for (int i = 0; i < 200; i++)
                polygon.Add(OnPlane(100 * Math.Cos(2 * Math.PI * i / 200), 100 * Math.Sin(2 * Math.PI * i / 200)));

            Assert.AreEqual(200, polygon.Count);
            Assert.ThrowsException<ArgumentException>(() => polygon.Add(new Point3d(OnPlane(0, 0).X, OnPlane(0, 0).Y, OnPlane(0, 0).Z + 1)));
        }

        [TestMethod]
        public void Polygon3dAddWithAlignedAndCoincidentPoints()
        {
            var polygon = new Polygon3d();
            polygon.Add(P(0, 0));
            polygon.Add(P(0, 0));
            polygon.Add(P(1, 0));
            polygon.Add(P(2, 0));
            polygon.Add(P(2, 0, 1)); // defines the plane y = 0
            polygon.Add(P(0, 0, 1));

            Assert.AreEqual(6, polygon.Count);
            Assert.ThrowsException<ArgumentException>(() => polygon.Add(P(1, 1, 1)));
        }

        [TestMethod]
        public void Polygon3dAddAfterReverse()
        {
            var polygon = new Polygon3d();
            polygon.Add(P(0, 0));
            polygon.Add(P(1, 0));
            polygon.Add(P(1, 1));
            polygon.Reverse();
            polygon.Add(P(0, 1));

            Assert.AreEqual(4, polygon.Count);
            Assert.ThrowsException<ArgumentException>(() => polygon.Add(P(0, 2, 1)));
        }

        [TestMethod]
        public void GetNormalVectorOfAConcavePolygonStartingFromTheConcaveVertex()
        {
            // L counterclockwise: the first three points turn clockwise
            var polygon = new Polygon3d(new[] { P(100, 10), P(10, 10), P(10, 100), P(0, 100), P(0, 0), P(100, 0) });
            Vector3d normal = polygon.GetNormalVector();

            Assert.AreEqual(0, normal.X, 1e-12);
            Assert.AreEqual(0, normal.Y, 1e-12);
            Assert.AreEqual(1, normal.Z, 1e-12);
            Assert.AreEqual(1, new Shape(polygon).GetNormalVector().Z, 1e-12);
        }

        [TestMethod]
        public void GetNormalVectorOfATiltedPolygon()
        {
            var polygon = new Polygon3d(Enumerable.Range(0, 64).Select(i => OnPlane(100 * Math.Cos(2 * Math.PI * i / 64), 100 * Math.Sin(2 * Math.PI * i / 64))).ToArray());
            Vector3d normal = polygon.GetNormalVector();
            Vector3d expected = new Vector3d(0.8, 0.6, 0).CrossProduct(new Vector3d(0.1, -0.2, 0.97));
            expected.Unitize();

            Assert.AreEqual(expected.X, normal.X, 1e-12);
            Assert.AreEqual(expected.Y, normal.Y, 1e-12);
            Assert.AreEqual(expected.Z, normal.Z, 1e-12);
        }

        [TestMethod]
        public void GetNormalVectorOfAlignedPointsThrows()
        {
            var polygon = new Polygon3d(new[] { P(0, 0), P(1, 0), P(2, 0) });
            Assert.ThrowsException<NotSupportedException>(() => polygon.GetNormalVector());
        }

        [TestMethod]
        public void Polygon3dIsPointInsideOnATiltedPlane()
        {
            var polygon = new Polygon3d(new[] { OnPlane(0, 0), OnPlane(100, 0), OnPlane(100, 100), OnPlane(50, 30), OnPlane(0, 100) });

            Assert.IsTrue(polygon.IsPointInside(OnPlane(10, 10)));
            Assert.IsTrue(polygon.IsPointInside(OnPlane(90, 60)));
            Assert.IsFalse(polygon.IsPointInside(OnPlane(50, 60)), "in the concavity");
            Assert.IsFalse(polygon.IsPointInside(OnPlane(-1, 10)));
            Assert.IsTrue(polygon.IsPointInside(OnPlane(50, 0)), "on the border");
            Assert.IsTrue(polygon.IsPointInside(OnPlane(100, 100)), "on a vertex");

            Point3d offPlane = OnPlane(10, 10);
            offPlane.Z += 1;
            Assert.IsFalse(polygon.IsPointInside(offPlane));
        }

        [TestMethod]
        public void ShapeIsPointInsideWithHole()
        {
            var fill = new Polygon3d(new[] { OnPlane(0, 0), OnPlane(100, 0), OnPlane(100, 100), OnPlane(0, 100) });
            var hole = new Polygon3d(new[] { OnPlane(30, 30), OnPlane(70, 30), OnPlane(70, 70), OnPlane(30, 70) });
            var shape = new Shape(fill, new[] { hole });

            Assert.IsTrue(shape.IsPointInside(OnPlane(10, 10)));
            Assert.IsFalse(shape.IsPointInside(OnPlane(50, 50)), "in the hole");
            Assert.IsTrue(shape.IsPointInside(OnPlane(30, 50)), "on the border of the hole");
            Assert.IsTrue(shape.IsPointInside(OnPlane(0, 50)), "on the border of the fill");
            Assert.IsFalse(shape.IsPointInside(OnPlane(110, 50)));

            Point3d offPlane = OnPlane(10, 10);
            offPlane.X += 1;
            Assert.IsFalse(shape.IsPointInside(offPlane));
        }

        #endregion
    }
}
