using GPC.Geometry;
using GPC.Geometry.Meshes;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace Meshes
{
    [TestClass]
    public class MeshIntersection : UnitTestBase
    {

        /// Create a list of commands in toCad, that you can paste into AutoCAD command line to draw edges and faces.
        private static string ExportToAutocadCommandLine(Mesh mesh)
        {
            var sb = new StringBuilder();
            foreach (var edge in mesh.Edges)
            {
                var vertexA = mesh.GetVertex(edge.A);
                var vertexB = mesh.GetVertex(edge.B);
                sb.AppendLine("LINE");
                sb.AppendLine($"{vertexA.Point.X},{vertexA.Point.Y},{vertexA.Point.Z}");
                sb.AppendLine($"{vertexB.Point.X},{vertexB.Point.Y},{vertexB.Point.Z}");
                sb.AppendLine("");
            }
            foreach (var face in mesh.Faces)
            {
                var vertexA = mesh.GetVertex(face.A);
                var vertexB = mesh.GetVertex(face.B);
                var vertexC = mesh.GetVertex(face.C);
                sb.AppendLine("3DFACE");
                sb.AppendLine($"{vertexA.Point.X},{vertexA.Point.Y},{vertexA.Point.Z}");
                sb.AppendLine($"{vertexB.Point.X},{vertexB.Point.Y},{vertexB.Point.Z}");
                sb.AppendLine($"{vertexC.Point.X},{vertexC.Point.Y},{vertexC.Point.Z}");
                sb.AppendLine("");
                sb.AppendLine("");
            }

            string toCad = sb.ToString();
            return toCad;
        }

        [TestMethod]
        public void IntersectWithSemiInfiniteRay1()
        {
            var envelop = new MeshEqualsHashCode();
            // It is a mesh with quadrangular faces, be careful because the intersection method was made for triangular elements.
            var mesh = envelop.CreateSimpleMesh3(40, 20, 10, 10, Point2d.Origin);
            mesh.Move(Vector3d.ZAxis * 10.0);

            var semiRay = new Line3d(new Point3d(4.0, 2.0, 0.0), new Point3d(4.0, 2.0, 20.0));
            var inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            Assert.IsTrue(inters.ContainsKey(new Point3d(4.0, 2.0, 10.0)));

            semiRay = new Line3d(new Point3d(40.0, 20.0, 0.0), new Point3d(40.0, 20.0, 20.0));
            inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            Assert.IsTrue(inters.ContainsKey(new Point3d(40.0, 20.0, 10.0)));

            semiRay = new Line3d(new Point3d(40.0, 20.0, 0.0), new Point3d(40.0, 20.0, -20.0));
            inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(0, inters.Count);
        }

        [TestMethod]
        public void IntersectWithSemiInfiniteRay2()
        {
            var envelop = new MeshEqualsHashCode();
            // It is a mesh with quadrangular faces, be careful because the intersection method was made for triangular elements.
            var origin = Point3d.Origin;
            var radius = 20.0;
            var mesh = envelop.CreateSimpleMesh4(origin, radius, 48, 24);

            var toCad = ExportToAutocadCommandLine(mesh);

            // Catches the intersection in all directions.
            double relative_error = 0.005;
            double distanceMin = double.MaxValue;
            double distance_error = radius * relative_error;
            int faceNumberMeridians = 20;
            int faceNumberParallels = 10;

            for (int i = 0; i < faceNumberMeridians; i++)
            {
                double theta = 2.0 * Math.PI * i / faceNumberMeridians;

                for (int j = 0; j <= faceNumberParallels; j++)
                {
                    double fi = Math.PI * j / faceNumberParallels;
                    double vx = Math.Cos(theta) * Math.Sin(fi);
                    double vy = Math.Sin(theta) * Math.Sin(fi);
                    double vz = Math.Cos(fi);
                    var semiRay = new Line3d(origin, origin + new Vector3d(vx, vy, vz));
                    var inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
                    Assert.IsTrue(inters.Count >= 1);
                    var distance = inters.Min(ii => origin.DistanceTo(ii.Key));
                    distanceMin = Math.Min(distance, distanceMin);
                    Assert.AreEqual(radius, distance, distance_error);
                }
            }
        }

        [TestMethod]
        public void IntersectWithSemiInfiniteRay3()
        {
            var envelop = new MeshEqualsHashCode();
            // It is a mesh with quadrangular faces, be careful because the intersection method was made for triangular elements.
            var mesh = envelop.CreateSimpleMesh1();

            var semiRay = new Line3d(new Point3d(5.0, 1.0, 5.0), new Point3d(5.0, 1.0, 4.0));
            var inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            var solutionPoint = new Point3d(5.0, 1.0, 0.0);
            Assert.IsTrue(inters.ContainsKey(solutionPoint));

            var ray = new Ray3d(semiRay.Start, semiRay.End);
            int faceid;
            if (mesh.PickFace(ray, out MeshFace face, out Point3d intersectionPoint))
                faceid = face.Id;
            else
                faceid = -1;

            Assert.IsTrue(faceid == 0);
            Assert.IsTrue(solutionPoint == intersectionPoint);

            // Change on mesh
            mesh.Vertices[1].Point.Z = 10.0;
            semiRay = new Line3d(new Point3d(-5.0, 1.0, 1.5), new Point3d(-4.0, 1.0, 1.5));
            inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            Assert.IsTrue(inters.ContainsKey(new Point3d(2.0, 1.0, 1.5)));
            ray = new Ray3d(semiRay.Start, semiRay.End);
            if (mesh.PickFace(ray, out face, out intersectionPoint))
                faceid = face.Id;
            else
                faceid = -1;
        }

        [TestMethod]
        public void IntersectWithSemiInfiniteRay4()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new MeshVertex[]
            {
                new MeshVertex(new Point3d(-253840642.842666, 253828767.880941, -259206.914735699)),
                new MeshVertex(new Point3d(-355884114.577654, 153513551.92941, -531623.328038255)),
                new MeshVertex(new Point3d(-348047604.562017, 154858095.379575, -363827.922436537))
            });

            var semiRay = new Line3d(new Point3d(0, 0, -314000), new Point3d(-152419767.9, 129492140.1, -314000));
            var inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            Assert.IsTrue(inters.Keys.First() == new Point3d(-274651523.313684, 233337276.55948, -314000));

            var ray = new Ray3d(semiRay.Start, semiRay.End);
            int faceid;
            if (mesh.PickFace(ray, out MeshFace face, out Point3d intersectionPoint))
                faceid = face.Id;
            else
                faceid = -1;

            Assert.IsTrue(faceid == 0);
        }

        [TestMethod]
        public void IntersectWithSemiInfiniteRay5()
        {
            var mesh = new Mesh();
            mesh.AddFaceMesh(new MeshVertex[]
            {
                new MeshVertex(new Point3d(-54619.0355055447,-354092752.260907,137125.180631757)),
                new MeshVertex(new Point3d(-38359.2311364457,-372789312.800383,-251.918944592238)),
                new MeshVertex(new Point3d(109543043.032806,-273807741.241306,592533.971737554))
            });

            var semiRay = new Line3d(new Point3d(0, 0, 0), new Point3d(0, -50000000, 0));
            var inters = mesh.GetIntersectionWihtSemiInfiniteRay(semiRay);
            Assert.AreEqual(1, inters.Count);
            Assert.IsTrue(inters.Keys.First() == new Point3d(0, -372748618.751107, 0));

            var ray = new Ray3d(semiRay.Start, semiRay.End);
            int faceid;
            if (mesh.PickFace(ray, out MeshFace face, out Point3d intersectionPoint))
                faceid = face.Id;
            else
                faceid = -1;

            Assert.IsTrue(faceid == 0);
        }
    }
}
