using GPC.Geometry;
using GPC.Geometry.Meshes;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Geometry
{
    [TestClass]
    public class GeometryEqualsHashCode : UnitTestBase
    {


        [TestMethod]
        public void Point1()
        {
            Point3d p1 = new Point3d(1, 2, 3);

            Point3d p2 = new Point3d(1, 2, 3);

            Assert.IsTrue(p1.Equals(p2));
        }

        [TestMethod]
        public void Point2()
        {
            Point3d p1 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);

            Point3d p2 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);

            Assert.IsTrue(p1.Equals(p2));
        }

        [TestMethod]
        public void Point3()
        {
            Point3d p1 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);

            Point3d p2 = null;

            Assert.IsFalse(p1.Equals(p2));
        }

        [TestMethod]
        public void Point4()
        {
            Point3d p1 = new Point3d(10, 0, 40);

            Point3d p2 = new Point3d(70, 90, 10); 

            Assert.IsFalse(p1.Equals(p2));
            Assert.IsFalse(p1.GetHashCode().Equals(p2.GetHashCode()), $"{p1.GetHashCode()}, {p2.GetHashCode()}");
        }

        [TestMethod]
        public void Line1()
        {
            Point3d p1 = new Point3d(1, 2, 3);
            Point3d p2 = new Point3d(4, 5, 6);

            Point3d p3 = new Point3d(1, 2, 3);
            Point3d p4 = new Point3d(4, 5, 6);

            Line3d l1 = new Line3d(p1, p2);
            Line3d l2 = new Line3d(p3, p4);
            Line3d l3 = new Line3d(p4, p3);

            Assert.IsTrue(l1.Equals(l2));
            Assert.IsTrue(l1.Equals(l3));
        }

        [TestMethod]
        public void Line2()
        {
            Point2d p1 = new Point2d(1, 2);
            Point2d p2 = new Point2d(4, 5);

            Point2d p3 = new Point2d(1, 2);
            Point2d p4 = new Point2d(4, 5);

            Line2d l1 = new Line2d(p1, p2);
            Line2d l2 = new Line2d(p3, p4);
            Line2d l3 = new Line2d(p4, p3);

            Assert.IsTrue(l1.Equals(l2));
            Assert.IsTrue(l1.Equals(l3));
        }


        [TestMethod]
        public void Shape1()
        {
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(10, 0, 0);
            Point3d p3 = new Point3d(10, 10, 0);
            Point3d p4 = new Point3d(0, 10, 0);

            Polygon3d poly = new Polygon3d() { p1, p2, p3, p4 };
            Polygon3d poly2 = (Polygon3d)poly.Clone();


            Shape shape1 = new Shape(poly);
            Shape shape2 = new Shape(poly2);
            Shape shape3 = new Shape(poly2, new Polygon3d[1] { poly });
            Shape shape4 = new Shape(poly2, new Polygon3d[1] { poly });

            for (int i = 0; i < poly.Count; i++)
            {
                Assert.IsTrue(poly[i].Equals(poly2[i]), "points");
            }

            Assert.IsTrue(poly.Equals(poly), "polygon");

            Assert.IsTrue(poly.Equals(poly2), "polygon");

            Assert.IsTrue(shape3.Equals(shape4));

            Assert.IsTrue(shape1.Equals(shape2));
            Assert.IsFalse(shape1.Equals(null));
            Assert.IsFalse(shape2.Equals(null));
            Assert.IsFalse(shape3.Equals(shape1));
            Assert.IsFalse(shape1.Equals(shape3));
            Assert.IsTrue(shape1 == shape2);
        }



        [TestMethod]
        public void Shape2()
        {
            Point3d p1 = new Point3d(10, 10, 10);
            Point3d p11 = new Point3d(10, 10, 10);
            Point3d p2 = new Point3d(10, 20, 20);
            Point3d p3 = new Point3d(10, 20, 15);
            Point3d p4 = new Point3d(10, 15, 12);

            Polygon3d poly = new Polygon3d() { p1, p2, p3, p4 };
            Polygon3d poly2 = (Polygon3d)poly.Clone();

            Shape shape1 = new Shape(poly);
            Shape shape2 = new Shape(poly2);

            Assert.IsTrue(p1.Equals(p11));
            Assert.IsFalse(p1.Equals(p2));
            Assert.IsTrue(poly.Equals(poly2));
            Assert.IsFalse(p1.Equals(shape1));
            Assert.IsFalse(shape1.Equals(p1));
        }


        [TestMethod]
        public void GeometryBase()
        {
            Point3d p11 = new Point3d(1, 2, 3);

            Point3d p1 = new Point3d(1, 2, 3);
            p1.Tag = p11;

            Point3d p2 = new Point3d(1, 2, 3);
            p2.Tag = p11;

            Assert.IsTrue(p1.Equals(p2));
        }


        [TestMethod]
        public void PointHashCode1()
        {
            Point3d p1 = new Point3d(1, 2, 3);

            Point3d p2 = new Point3d(1, 2, 3);

            Assert.IsTrue(p1.GetHashCode().Equals(p2.GetHashCode()));
        }


        [TestMethod]
        public void PointHashCode2()
        {
            Point3d p1 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);

            Point3d p2 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);

            Assert.IsTrue(p1.GetHashCode().Equals(p2.GetHashCode()));
        }


        [TestMethod]
        public void PointHashCode3()
        {
            Dictionary<GeometryBase, int> dict = new Dictionary<GeometryBase, int>();

            Point3d p1 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);
            dict.Add(p1, 1);

            Point3d p2 = new Point3d(-0.00102259599691302, 0.00134096938416004, 0);
            Point3d p3 = new Point3d(-0.00102259599691302, 2, 0);


            Assert.IsTrue(dict.ContainsKey(p1));

            Assert.IsTrue(dict.ContainsKey(p2));

            Assert.IsFalse(dict.ContainsKey(p3));
        }



        [TestMethod]
        public void LineHashCode1()
        {
            Point3d p1 = new Point3d(1, 2, 3);
            Point3d p2 = new Point3d(4, 5, 6);

            Point3d p3 = new Point3d(1, 2, 3);
            Point3d p4 = new Point3d(4, 5, 6);

            Line3d l1 = new Line3d(p1, p2);
            Line3d l2 = new Line3d(p3, p4);
            Line3d l3 = new Line3d(p4, p3);

            Assert.IsTrue(l1.GetHashCode().Equals(l2.GetHashCode()), $"normal: {l1.GetHashCode()}  {l2.GetHashCode()}");
            Assert.IsTrue(l1.GetHashCode().Equals(l3.GetHashCode()), $"flipped: {l1.GetHashCode()}  {l3.GetHashCode()}");
        }

        [TestMethod]
        public void LineHashCode2()
        {
            Point2d p1 = new Point2d(1, 2);
            Point2d p2 = new Point2d(4, 5);

            Point2d p3 = new Point2d(1, 2);
            Point2d p4 = new Point2d(4, 5);

            Line2d l1 = new Line2d(p1, p2);
            Line2d l2 = new Line2d(p3, p4);
            Line2d l3 = new Line2d(p4, p3);

            Assert.IsTrue(l1.GetHashCode().Equals(l2.GetHashCode()), $"normal: {l1.GetHashCode()}  {l2.GetHashCode()}");
            Assert.IsTrue(l1.GetHashCode().Equals(l3.GetHashCode()), $"flipped: {l1.GetHashCode()}  {l3.GetHashCode()}");
        }


        [TestMethod]
        public void Polygon3d1()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl2 = new Polygon3d();
            pl2.Add(p1);
            pl2.Add(p2);
            pl2.Add(p3);
            pl2.Add(p4);


            Assert.AreEqual(pl1.GetHashCode(), pl2.GetHashCode(), 0, $"{pl1.GetHashCode()} {pl2.GetHashCode()}");
        }


        [TestMethod]
        public void Polygon3d2()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);
            Point3d p5 = new Point3d(4, 8, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl2 = new Polygon3d();
            pl2.Add(p1);
            pl2.Add(p2);
            pl2.Add(p3);
            pl2.Add(p4);
            pl2.Add(p5);

            Assert.AreNotEqual(pl1.GetHashCode(), pl2.GetHashCode(), 0, $"{pl1.GetHashCode()} {pl2.GetHashCode()}");
            Console.WriteLine($"{pl1.GetHashCode()} {pl2.GetHashCode()}");
        }


        [TestMethod]
        public void Polygon3d3()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl3 = new Polygon3d();
            pl3.Add(p1);
            pl3.Add(p2);
            pl3.Add(p4);
            pl3.Add(p3);

            Assert.AreNotEqual(pl1.GetHashCode(), pl3.GetHashCode(), 0, $"{pl1.GetHashCode()} {pl3.GetHashCode()}");
        }


        [TestMethod]
        public void ShapeHashCode1()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl3 = new Polygon3d();
            pl3.Add(p1);
            pl3.Add(p2);
            pl3.Add(p3);
            pl3.Add(p4);


            Shape s1 = new Shape(pl1, null, null);
            Shape s2 = new Shape(pl3, null, null);

            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), 0, $"{s1.GetHashCode()} {s2.GetHashCode()}");
        }


        [TestMethod]
        public void ShapeHashCode2()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl2 = new Polygon3d();
            pl2.Add(p1);
            pl2.Add(p2);
            pl2.Add(p3);
            pl2.Add(p4);

            Polygon3d pl3 = new Polygon3d();
            pl3.Add(p1);
            pl3.Add(p2);
            pl3.Add(p4);
            pl3.Add(p3);

            Polygon3d pl4 = new Polygon3d();
            pl4.Add(p1);
            pl4.Add(p2);
            pl4.Add(p4);
            pl4.Add(p3);


            Shape s1 = new Shape(pl1, new Polygon3d[] { pl3 }, null);
            Shape s2 = new Shape(pl2, new Polygon3d[] { pl4 }, null);

            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), 0, $"{s1.GetHashCode()} {s2.GetHashCode()}");
        }


        [TestMethod]
        public void Shape3()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl2 = new Polygon3d();
            pl2.Add(p1);
            pl2.Add(p2);
            pl2.Add(p3);
            pl2.Add(p4);

            Polygon3d pl3 = new Polygon3d();
            pl3.Add(p1);
            pl3.Add(p2);
            pl3.Add(p4);
            pl3.Add(p3);

            Polygon3d pl4 = new Polygon3d();
            pl4.Add(p1);
            pl4.Add(p2);
            pl4.Add(p3);
            pl4.Add(p4);


            Shape s1 = new Shape(pl1, new Polygon3d[] { pl3 }, null);
            Shape s2 = new Shape(pl2, new Polygon3d[] { pl4 }, null);

            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), 0, $"{s1.GetHashCode()} {s2.GetHashCode()}");
        }


        [TestMethod]
        public void Shape4()
        {
            Point3d p1 = new Point3d(1, 2, 0);
            Point3d p2 = new Point3d(4, 5, 0);
            Point3d p3 = new Point3d(1, 6, 0);
            Point3d p4 = new Point3d(4, 7, 0);

            Polygon3d pl1 = new Polygon3d();
            pl1.Add(p1);
            pl1.Add(p2);
            pl1.Add(p3);
            pl1.Add(p4);

            Polygon3d pl2 = new Polygon3d();
            pl2.Add(p1);
            pl2.Add(p2);
            pl2.Add(p3);
            pl2.Add(p4);


            Polygon3d pl3 = new Polygon3d();
            pl3.Add(p1);
            pl3.Add(p2);
            pl3.Add(p4);
            pl3.Add(p3);

            Polygon3d pl4 = new Polygon3d();
            pl4.Add(p1);
            pl4.Add(p2);
            pl4.Add(p4);
            pl4.Add(p3);


            Shape s1 = new Shape(pl1, new Polygon3d[] { pl3, pl1 }, null);
            Shape s2 = new Shape(pl2, new Polygon3d[] { pl2, pl3 }, null);

            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), 0, $"{s1.GetHashCode()} {s2.GetHashCode()}");
        }

        [TestMethod]
        public void Shape5()
        {
            Point3d p1 = new Point3d(1266.5865523, -967.1664937, 0.0000000);
            Point3d p2 = new Point3d(2576.8728933, -1055.7319683, 0.0000000);
            Point3d p3 = new Point3d(1460.0421667, -1674.8067673, 0.0000000);
            Point3d p4 = new Point3d(2570.5877856, -1725.0959362, 0.0000000);
            Point3d p5 = new Point3d(1470.6939417, -2513.8769500, 0.0000000);
            Point3d p6 = new Point3d(2611.3767552, -2596.4704218, 0.0000000);
            Point3d p7 = new Point3d(1720.5015636, -1909.3000291, 0.0000000);
            Point3d p8 = new Point3d(2235.8803933, -1972.1511059, 0.0000000);
            Point3d p9 = new Point3d(1843.0611633, -2270.6937207, 0.0000000);
            Point3d p10 = new Point3d(2245.3080548, -2283.2639360, 0.0000000);

            Polygon3d poly1 = new Polygon3d() { p1, p2, p4, p3 };
            Polygon3d poly2 = new Polygon3d() { p3, p4, p6, p5 };
            Polygon3d embS1 = new Polygon3d() { p7, p8, p10, p9 };

            Shape s1 = new Shape(poly1);
            Shape s2 = new Shape(poly2);
            Shape s3 = new Shape(poly2, new Polygon3d[] { embS1 });

            Assert.IsFalse(s1 == s2);
            Assert.IsFalse(s1.GetHashCode() == s2.GetHashCode());
            Assert.IsFalse(s3.Equals(s2));

            Dictionary<Shape, GeometryBase> a = new Dictionary<Shape, GeometryBase>();

            a[s1] = p2;
            a[s2] = p1;

            Shape s22 = (Shape)s2.Clone();
            s22.AddHole(embS1);

            a[s22] = p1;

            Assert.IsTrue(a.ContainsKey(s2));
            Assert.IsTrue(a.ContainsKey(s3)); 
            Assert.IsTrue(a.ContainsKey(s22));

        }
    }
}
