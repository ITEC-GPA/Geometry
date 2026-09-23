using GPC.Geometry;
using GPC.Geometry.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using GPC.Geometry.Meshes;

namespace UnitTest.Collections
{
    [TestClass]
    public class CollectionExtension
    {
        [TestMethod]
        public void SortClockwise()
        {
            List<Point2d> points = new List<Point2d>
            {
                new Point2d(20, 10),
                new Point2d(40, 50),
                new Point2d(50, 30),
                new Point2d(10, 50),
                new Point2d(30, 60)

            };
            Point2d[] sorted = points.SortClockwise().ToArray();

            Point2d[] ptsArray = points.ToArray();
            sorted = ptsArray.SortClockwise().ToArray();
        }

        [TestMethod]
        public void Area()
        {
            List<Point2d> points = new List<Point2d>
            {
                new Point2d(-2, 10),
                new Point2d(-11, 2),
                new Point2d(-3, -11),
                new Point2d(5, -4),
                new Point2d(9, -10),
                new Point2d(13, 5)
            };
            double area = points.Area();
            Assert.AreEqual(area, 300.5);
        }


        [TestMethod]
        public void MassCenter()
        {
            List<Point3d> points = new List<Point3d>
            {
                new Point3d(-2, 9.510565, 3.09017),
                new Point3d(-11, 1.902113, 0.618034),
                new Point3d(-3, -10.461622, -3.399187),
                new Point3d(5, -3.804226, -1.236068),
                new Point3d(9, -9.510565, -3.09017),
                new Point3d(13, 4.755283, 1.545085),
                new Point3d(-2, 9.510565, 3.09017)
            };
            Point3d pt = points.MassCenter();
            Assert.AreEqual(pt, new Point3d(1.204659, 0.28748, 0.093408));
        }

        [TestMethod]
        public void Triangulate()
        {
            List<Point3d> points = new List<Point3d>
            {
                new Point3d(-2, 9.510565, 3.09017),
                new Point3d(-11, 1.902113, 0.618034),
                new Point3d(-3, -10.461622, -3.399187),
                new Point3d(5, -3.804226, -1.236068),
                new Point3d(9, -9.510565, -3.09017),
                new Point3d(13, 4.755283, 1.545085),
                new Point3d(-2, 9.510565, 3.09017)
            };
            Mesh mesh = points.Triangulate();
            foreach (var face in mesh.Faces)
            {
                System.Diagnostics.Debug.WriteLine(mesh.Vertices[face.A].Point.ToString(), $"{face.Id}-A");
                System.Diagnostics.Debug.WriteLine(mesh.Vertices[face.B].Point.ToString(), $"{face.Id}-B");
                System.Diagnostics.Debug.WriteLine(mesh.Vertices[face.C].Point.ToString(), $"{face.Id}-C");
            }

            Assert.IsNotNull(mesh);
        }
    }
}
