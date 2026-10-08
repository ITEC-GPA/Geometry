using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geometry
{
    [TestClass]
    public class OctoberCorrectionsTest
    {
        private static Point3d P(double x, double y, double z = 0) => new Point3d(x, y, z);

        private static bool Pick(Mesh mesh, double x, double y) =>
            mesh.PickFace(new Ray3d(P(x, y, 1), new Vector3d(0, 0, -1)), out _, out _);

        [TestMethod]
        public void MeshIndicesFollowCoordinateEditsIncludingAfterSerialization()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(1, 0), P(0, 1) });
            Assert.IsTrue(Pick(mesh, 0.2, 0.2));
            mesh.FindNeighbours(P(0, 0), 0.1);
            mesh.Vertices[0].Point.X = 10;
            Assert.AreEqual(0, mesh.AddVertex(new MeshVertex(P(10, 0))));
            Assert.AreEqual(3, mesh.VerticesCount);
            CollectionAssert.AreEqual(new[] { 0 }, mesh.FindNeighbours(P(10, 0), 0.1));
            Assert.IsFalse(Pick(mesh, 0.2, 0.2));

            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new System.IO.MemoryStream())
            {
                formatter.Serialize(stream, mesh);
                stream.Position = 0;
                mesh = (Mesh)formatter.Deserialize(stream);
            }
            mesh.FindNeighbours(P(10, 0), 0.1);
            mesh.Vertices[0].Point.MoveTo(20, 0, 0);
            Assert.AreEqual(0, mesh.AddVertex(new MeshVertex(P(20, 0))));
            CollectionAssert.AreEqual(new[] { 0 }, mesh.FindNeighbours(P(20, 0), 0.1));
            mesh.Vertices[0].Point.Move(10, 0, 0);
            Assert.AreEqual(0, mesh.AddVertex(new MeshVertex(P(30, 0))));
        }

        [TestMethod]
        public void CollectionReplacementKeepsIdsUniqueAndFailureIsAtomic()
        {
            var collection = new MeshBaseCollection<MeshVertex>();
            collection.Add(new MeshVertex(P(0, 0)));
            collection.Add(new MeshVertex(P(1, 0)));
            var donor = new MeshBaseCollection<MeshVertex>();
            var replacement = new MeshVertex(P(2, 0));
            donor.Add(replacement, 2);
            collection[0] = replacement;
            Assert.AreEqual(3, collection.Add(new MeshVertex(P(3, 0))));
            Assert.AreEqual(collection.Count, collection.Select(v => v.Id).Distinct().Count());
            Assert.ThrowsException<ArgumentException>(() => collection[0] = collection[1]);
            Assert.AreSame(replacement, collection.GetElementById(2));
            collection[0] = new MeshVertex(P(4, 0));
            Assert.AreEqual(4, collection[0].Id);
            Assert.IsFalse(collection.Contains(2));
        }

        [TestMethod]
        public void MeshSearchesFollowCollectionChanges()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new[] { P(0, 0), P(1, 0), P(0, 1) });
            Assert.IsTrue(Pick(mesh, 0.2, 0.2));
            mesh.AddFaceMesh(new[] { P(10, 0), P(11, 0), P(10, 1) });
            Assert.IsTrue(Pick(mesh, 10.2, 0.2));
            mesh.Faces.RemoveAt(1);
            Assert.IsFalse(Pick(mesh, 10.2, 0.2));

            Assert.AreEqual(1, mesh.FindNeighbours(P(0, 0), 0.1).Count);
            int id = mesh.Vertices.Add(new MeshVertex(P(20, 0)));
            CollectionAssert.AreEqual(new[] { id }, mesh.FindNeighbours(P(20, 0), 0.1));
            mesh.Vertices.Remove(id);
            Assert.AreEqual(0, mesh.FindNeighbours(P(20, 0), 0.1).Count);
            mesh.Faces.Clear();
            Assert.IsFalse(Pick(mesh, 0.2, 0.2));
        }

        [TestMethod]
        public void SegmentIntersectionDoesNotDependOnTheModelScale()
        {
            foreach (double length in new[] { 0.001, 0.01, 1.0, 1000.0 })
            {
                var a = new Line3d(P(0, 0), P(length, 0));
                var b = new Line3d(P(length / 2, -length / 2), P(length / 2, length / 2));
                Assert.IsTrue(a.GetIntersection(b, out Point3d intersection), "Length: " + length);
                Assert.AreEqual(length / 2, intersection.X, length * 1e-9);
                Assert.AreEqual(0, intersection.Y, length * 1e-9);
                Assert.IsTrue(a.GetIntersectionWithInfiniteLine(b, out _));
                Assert.IsFalse(a.GetIntersection(new Line3d(P(0, length), P(length, length)), out _));
                var a2 = new Line2d(new Point2d(0, 0), new Point2d(length, 0));
                var b2 = new Line2d(new Point2d(length / 2, -length / 2), new Point2d(length / 2, length / 2));
                Assert.IsTrue(a2.GetIntersection(b2, out _));
            }
        }
    }
}
