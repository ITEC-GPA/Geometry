using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meshes
{
    [TestClass]
    public class MeshBVH : GenericMeshTest
    {

        [TestMethod]
        public void Create()
        {
            Mesh mesh = CreateSimpleMesh3(10, 10, 10, 10, Point2d.Origin);

            mesh.UpdateVertexBVH();

            Assert.IsTrue(mesh.VertexBVH != null, "Unable to create BVH");
        }

        [TestMethod]
        public void AddVertices()
        {
            Mesh mesh = CreateSimpleMesh3(10, 10, 10, 10, Point2d.Origin);

            mesh.AddVertex(new MeshVertex(new Point3d(0, 0, 0)));
            mesh.AddVertex(new MeshVertex(new Point3d(0.00001, 0, 0)));
            mesh.AddVertex(new MeshVertex(new Point3d(0, 1, 0)), 10);
            mesh.AddVertex(new MeshVertex(new Point3d(0, 0, 0.1)), 1);

            Assert.IsTrue(mesh.VerticesCount == 121, "Invalid number of vertices");


            mesh.AddVertex(new MeshVertex(new Point3d(0.00001, 0, 0)), 0);
            Assert.IsTrue(mesh.VerticesCount == 122, "Invalid number of vertices");
        }


        [TestMethod]
        public void StressFind()
        {
            Mesh mesh = CreateSimpleMesh3(400, 400, 1, 1, Point2d.Origin);

            var neighbours = mesh.FindNeighbours(new Point3d(0, 0, 0), 10);
            Assert.IsTrue(neighbours.Count() == 90, "Wrong number of vertices found");

            neighbours = mesh.FindNeighbours(new Point3d(400, 400, 0), 10);
            Assert.IsTrue(neighbours.Count() == 90, "Wrong number of vertices found");

            neighbours = mesh.FindNeighbours(new Point3d(200, 200, 0), 10);
            Assert.IsTrue(neighbours.Count() == 317, "Wrong number of vertices found");
        }


    }
}