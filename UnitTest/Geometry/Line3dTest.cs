using GPC.Geometry;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Geometry
{
    [TestClass]
    public class Line3dTest : UnitTestBase
    {
        protected double _tolerance = 0.001;

        [TestMethod]
        public void Length1()
        {
            Point3d from = new Point3d(0, 0, 0);
            Point3d to = new Point3d(0, 0, 0);
            Line3d line = new Line3d(from, to);
            double expected = 0;
            double found = line.GetLength();
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Length2()
        {
            Point3d from = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d to = new Point3d(-31.4545, -75.2447, -17.5889);
            Line3d line = new Line3d(from, to);
            double expected = 204.41;
            double found = line.GetLength();
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void DistanceToPoint()
        {
            Point3d from = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d to = new Point3d(-31.4545, -75.2447, -17.5889);
            Line3d line = new Line3d(from, to);
            double expected = 27.8583;
            double found = line.DistanceTo(new Point3d(56, 3, 0));
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void DistanceToPoint2()
        {
            Point3d from = new Point3d(0, 0, 100);
            Point3d to = new Point3d(200, 400, 100);

            Line3d line = new Line3d(from, to);

            double expected = 0;
            double found = line.DistanceTo(new Point3d(100, 200, 100));
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void DistanceToPoint3()
        {
            Point3d from = new Point3d(0, 0, 0);
            Point3d to = new Point3d(100, 0, 0);

            Line3d line = new Line3d(from, to);

            Point3d p = new Point3d(200, 200, 0);

            double expected = p.X - p.Y;
            double found = line.DistanceTo(p);
            double diff = expected - found;

            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }


        [TestMethod]
        public void IntersectionLineeCoincidenti()
        {
            // Arrange 
            Point3d from1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d to1 = new Point3d(-31.4545, -75.2447, -17.5889);
            Line3d line1 = new Line3d(from1, to1);
            Point3d from2 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d to2 = new Point3d(-31.4545, -75.2447, -17.5889);
            Line3d line2 = new Line3d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point3d _);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectionLineeSovrapposte()
        {
            // Arrange 
            Point3d from1 = new Point3d(1, 1, 1);
            Point3d to1 = new Point3d(3, 3, 3);
            Line3d line1 = new Line3d(from1, to1);
            Point3d from2 = new Point3d(2, 2, 2);
            Point3d to2 = new Point3d(4, 4, 4);
            Line3d line2 = new Line3d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point3d _);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectionLineeSovrapposte2()
        {
            // Arrange 
            Point3d from1 = new Point3d(1, 1, 1);
            Point3d to1 = new Point3d(3, 3, 3);
            Line3d line1 = new Line3d(from1, to1);
            Point3d from2 = new Point3d(2, 2, 2);
            Point3d to2 = new Point3d(-4, -4, -4);
            Line3d line2 = new Line3d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point3d _);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection1()
        {
            Point3d from1 = new Point3d(82.5587, 90.1225, 20.3355);
            Point3d to1 = new Point3d(-31.4545, -75.2447, -17.5889);
            Line3d line1 = new Line3d(from1, to1);
            Point3d from2 = new Point3d(-23, 75, 29);
            Point3d to2 = new Point3d(25.0409, -60.7966, -20.2101);
            Line3d line2 = new Line3d(from2, to2);
            Point3d expected = new Point3d(9.26062, -16.1906, -4.04576);
            bool result = line1.GetIntersection(line2, out Point3d found);
            Assert.IsTrue(result, "No intersections found");
            double diff = expected.DistanceTo(found);
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Intersection2()
        {
            Point3d from1 = new Point3d(82, 90, 10);
            Point3d to1 = new Point3d(-31, -75, 10);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-118, 64, 10);
            Point3d to2 = new Point3d(-67, -67, 10);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point3d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");
        }

        [TestMethod]
        public void Intersection3()
        {
            Point3d from1 = new Point3d(12, 20, 5);
            Point3d to1 = new Point3d(1, 40, 15);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-10, -10, 5);
            Point3d to2 = new Point3d(-25, -40, 5);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point3d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");
        }
        [TestMethod]
        public void Intersection4()
        {
            Point3d from1 = new Point3d(12, 20, 0);
            Point3d to1 = new Point3d(1, 40, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-10, -10, 0);
            Point3d to2 = new Point3d(-25, -40, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point3d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");
        }

        [TestMethod]
        public void Intersection5()
        {
            Point3d from1 = new Point3d(0, -10, 0);
            Point3d to1 = new Point3d(0, 10, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-10, 0, 0);
            Point3d to2 = new Point3d(10, 0, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point3d _);
            Assert.IsTrue(result, $"Intersections should be found");
        }

        [TestMethod]
        public void Intersection6()
        {
            Point3d from1 = new Point3d(-10, -10, -10);
            Point3d to1 = new Point3d(10, 10, 10);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-5, -5, -5);
            Point3d to2 = new Point3d(15, 15, 15);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point3d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");
        }

        [TestMethod]
        public void Intersection7()
        {
            Point3d from1 = new Point3d(10, 0, 0);
            Point3d to1 = new Point3d(-10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 10, 0);
            Point3d to2 = new Point3d(0, -10, 0);
            Line3d line2 = new Line3d(from2, to2);

            double scaleFactor = 0.001;
            double tol = 1E-4 * scaleFactor;

            line1.Scale(scaleFactor);
            line2.Scale(scaleFactor);

            bool result = line1.GetIntersection(line2, out Point3d found, tol);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Intersection8()
        {
            Point3d from1 = new Point3d(2.56372951653475, -2.03760053695104, 0);
            Point3d to1 = new Point3d(-0.435746784816809, 1.92959521077706, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(3.68452331481815, 0.43490705677099, -1.50000000457757E-07);
            Point3d to2 = new Point3d(-1.31587756168708, 0.082815686326692, -1.5000000042367E-07);
            Line3d line2 = new Line3d(from2, to2);

            double scaleFactor = 0.01;
            double tol = 1E-4 * scaleFactor;
            double tol2 = tol * tol;

            line1.Scale(scaleFactor);
            line2.Scale(scaleFactor);

            bool result = line1.GetIntersection(line2, out Point3d found, tol);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Intersection9()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(5, 0, 0);
            Point3d to2 = new Point3d(5, 10, 10);
            Line3d line2 = new Line3d(from2, to2);

            double scaleFactor = 0.0001;
            double tol = 1E-4 * scaleFactor;
            double tol2 = tol * tol;

            line1.Scale(scaleFactor);
            line2.Scale(scaleFactor);

            bool result = line1.GetIntersection(line2, out Point3d found, tol);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(5, 0, 0));
        }

        [TestMethod]
        public void Intersection10()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(15, 5, 0);
            Point3d to2 = new Point3d(15, -5, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(15, 0, 0));
        }

        [TestMethod]
        public void Intersection11()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 10, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 100, 0);
            Point3d to2 = new Point3d(200, 100, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(100, 100, 0));
        }

        [TestMethod]
        public void Intersection12()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 100, 0);
            Point3d to2 = new Point3d(200, 100, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsFalse(found != null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection13()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 100, 1);
            Point3d to2 = new Point3d(200, 100, 0);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsFalse(found != null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection14()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 100, 1);
            Point3d to2 = new Point3d(200, 100, 1);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsFalse(found != null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection15()
        {
            Point3d from1 = new Point3d(0, 0, 0);
            Point3d to1 = new Point3d(10, 0, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 100, 1);
            Point3d to2 = new Point3d(50, 100, 1);
            Line3d line2 = new Line3d(from2, to2);

            bool result = line1.GetIntersectionWithInfiniteLine(line2, out Point3d found);
            Assert.IsFalse(found != null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection16()
        {
            Point3d from1 = new Point3d(10, -50, 0);
            Point3d to1 = new Point3d(10, 50, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 0, 0);
            Point3d to2 = new Point3d(50, 0, 0);
            Vector3d vector = new Vector3d(from2, to2);

            bool result = line1.GetIntersection(vector, out Point3d found);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(10, 0, 0));
        }

        [TestMethod]
        public void Intersection17()
        {
            Point3d from1 = new Point3d(10, -50, 0);
            Point3d to1 = new Point3d(10, 50, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(0, 0, 0);
            Point3d to2 = new Point3d(5, 0, 0);
            Vector3d vector = new Vector3d(from2, to2);

            bool result = line1.GetIntersection(vector, out Point3d found);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(10, 0, 0));
        }

        [TestMethod]
        public void Intersection18()
        {
            Point3d from1 = new Point3d(10, -50, 0);
            Point3d to1 = new Point3d(10, 50, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from = new Point3d(0, 0, 1);
            Vector3d vector = new Vector3d(from);

            bool result = line1.GetIntersection(vector, out Point3d found);
            Assert.IsFalse(found != null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Intersection19()
        {
            Point3d from1 = new Point3d(10, -50, 0);
            Point3d to1 = new Point3d(10, 50, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from = new Point3d(-1, 0, 0);
            Vector3d vector = new Vector3d(from);

            bool result = line1.GetIntersection(vector, out Point3d found);
            Assert.IsTrue(found != null);
            Assert.IsTrue(result);
            Assert.IsTrue(found == new Point3d(10, 0, 0));
        }

        [TestMethod]
        public void Intersection1Plane()
        {
            Point3d from1 = new Point3d(10, 10, 0);
            Point3d to1 = new Point3d(-10, -10, 0);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-10, 10, 0);
            Point3d to2 = new Point3d(10, -10, 0);
            Line3d line2 = new Line3d(from2, to2);

            Point3d exp = new Point3d(0, 0, 0);
            bool result = line1.GetIntersection(line2, out Point3d found);
            Console.WriteLine($"Intersection point = {found}");
            Assert.IsTrue(result, $"Intersections should be found");
            Point3d diff = found - exp;
            Assert.IsTrue(diff.X < 0.001 && diff.Y < 0.001 && diff.Z < 0.001);
        }

        [TestMethod]
        public void Intersection2Plane()
        {
            Point3d from1 = new Point3d(0, 10, 10);
            Point3d to1 = new Point3d(0, -10, -10);
            Line3d line1 = new Line3d(from1, to1);

            Point3d from2 = new Point3d(-10, 10, 0);
            Point3d to2 = new Point3d(10, -10, 0);
            Line3d line2 = new Line3d(from2, to2);

            Point3d exp = new Point3d(0, 0, 0);
            bool result = line1.GetIntersection(line2, out Point3d found);
            Console.WriteLine($"Intersection point = {found}");
            Assert.IsTrue(result, $"Intersections should be found");
            Point3d diff = found - exp;
            Assert.IsTrue(diff.X < 0.001 && diff.Y < 0.001 && diff.Z < 0.001);
        }

        [TestMethod]
        public void PointOnLine1()
        {
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(5, 5, 5);

            Line3d line = new Line3d(startPoint, endPoint);

            Point3d testPoint = new Point3d(10, 10, 10);

            bool result = line.IsPointOnLine(testPoint);
            Assert.IsFalse(result, "ERROR: the Point is NOT on Line");

        }

        [TestMethod]
        public void PointOnLine2()
        {
            Point3d startPoint = new Point3d(-3, -5, -7);
            Point3d endPoint = new Point3d(9, 15, 21);

            Line3d line = new Line3d(startPoint, endPoint);

            Point3d testPoint = new Point3d(3, 5, 7);

            bool result = line.IsPointOnLine(testPoint);
            Assert.IsTrue(result, "ERROR: the Point is on Line");

        }

        [TestMethod]
        public void PointOnLint3()
        {
            Point3d startPoint = new Point3d(1, 1, 1);
            Point3d endPoint = new Point3d(9, 15, 21);

            Line3d line = new Line3d(startPoint, endPoint);

            Point3d testPoint = new Point3d(1, 1, 1);

            bool result = line.IsPointOnLine(testPoint);
            Assert.IsTrue(result, "ERROR: the Point is on Line");

        }

        [TestMethod]
        public void PointOnLint4()
        {
            Point3d startPoint = new Point3d(8, -7, -5);
            Point3d endPoint = new Point3d(99, 99, 99);

            Line3d line = new Line3d(startPoint, endPoint);

            Point3d testPoint = new Point3d(0, 0, 0);

            bool result = line.IsPointOnLine(testPoint);
            Assert.IsFalse(result, "ERROR: the Point is NOT on Line");
        }

        [TestMethod]
        public void PointOnLint5()
        {
            Point3d startPoint = new Point3d(26.299999999538, 52.1999999997675, -9.15);
            Point3d endPoint = new Point3d(26.299999999538, 52.1999999997675, -6.36000000000001);

            Line3d line = new Line3d(startPoint, endPoint);
            double tol = 1E-6;
            Point3d testPoint = new Point3d(26.299999999538, 52.1999999998562, -6.71000000000002);

            bool result = line.IsPointOnLine(testPoint, tol);
            Assert.IsTrue(result);
        }

        public void PointOnLint6()
        {
            Point3d startPoint = new Point3d(1, 1, 1);
            Point3d endPoint = new Point3d(1, 1, 1);

            Line3d line = new Line3d(startPoint, endPoint);

            Point3d testPoint = new Point3d(1, 1, 1);

            bool result = line.IsPointOnLine(testPoint);
            Assert.IsTrue(result, "ERROR: the Point is on Line");

        }

        [TestMethod]
        public void SplitWithParameters()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            double[] param = new double[] { 0.2, 0.5, 0.8 };
            Line3d[] splitLine = line.Split(param);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(20, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(20, 0, 0), new Point3d(50, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(50, 0, 0), new Point3d(80, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(80, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
        }

        [TestMethod]
        public void SplitWithParameters2()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            double[] param = new double[] { 0.2, 0.8, 0.9, 0.5, 0.0 };
            Line3d[] splitLine = line.Split(param);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(20, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(20, 0, 0), new Point3d(50, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(50, 0, 0), new Point3d(80, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(80, 0, 0), new Point3d(90, 0, 0));
            Line3d expLine5 = new Line3d(new Point3d(90, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
            Assert.IsTrue(splitLine[4] == expLine5);
        }

        [TestMethod]
        public void SplitWithPoint()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            Point3d p1 = new Point3d(20, 0, 0);
            Point3d p2 = new Point3d(50, 0, 0);
            Point3d p3 = new Point3d(80, 0, 0);

            Point3d[] points = new Point3d[] { p1, p2, p3 };

            Line3d[] splitLine = line.Split(points, 0.0001);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(20, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(20, 0, 0), new Point3d(50, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(50, 0, 0), new Point3d(80, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(80, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
        }

        [TestMethod]
        public void SplitWithPoints2()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            Point3d p1 = new Point3d(20, 0, 0);
            Point3d p2 = new Point3d(50, 0, 0);
            Point3d p3 = new Point3d(80, 0, 0);
            Point3d p4 = new Point3d(50, 0, 0);
            Point3d p5 = new Point3d(90, 0, 0);
            Point3d p6 = new Point3d(0, 0, 0);

            Point3d[] points = new Point3d[] { p1, p2, p3, p4, p5, p6 };

            Line3d[] splitLine = line.Split(points, 0.0001);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(20, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(20, 0, 0), new Point3d(50, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(50, 0, 0), new Point3d(80, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(80, 0, 0), new Point3d(90, 0, 0));
            Line3d expLine5 = new Line3d(new Point3d(90, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
            Assert.IsTrue(splitLine[4] == expLine5);
        }

        [TestMethod]
        public void Split1()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            Line3d[] splitLine = line.Split(4);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(25, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(25, 0, 0), new Point3d(50, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(50, 0, 0), new Point3d(75, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(75, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
        }


        [TestMethod]
        public void Split2()
        {
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0));

            Line3d[] splitLine = line.Split(5);

            Line3d expLine1 = new Line3d(new Point3d(0, 0, 0), new Point3d(20, 0, 0));
            Line3d expLine2 = new Line3d(new Point3d(20, 0, 0), new Point3d(40, 0, 0));
            Line3d expLine3 = new Line3d(new Point3d(40, 0, 0), new Point3d(60, 0, 0));
            Line3d expLine4 = new Line3d(new Point3d(60, 0, 0), new Point3d(80, 0, 0));
            Line3d expLine5 = new Line3d(new Point3d(80, 0, 0), new Point3d(100, 0, 0));

            Assert.IsTrue(splitLine[0] == expLine1);
            Assert.IsTrue(splitLine[1] == expLine2);
            Assert.IsTrue(splitLine[2] == expLine3);
            Assert.IsTrue(splitLine[3] == expLine4);
            Assert.IsTrue(splitLine[4] == expLine5);
        }

        [TestMethod]
        public void IsOnSemiInfiniteRay()
        {
            var semiRay = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 10.0, 10.0));

            Assert.IsTrue(new Point3d(0.0, 0.0, 0.0).IsOnSemiInfiniteRay(semiRay, out _) == true);
            Assert.IsTrue(new Point3d(5.0, 5.0, 5.0).IsOnSemiInfiniteRay(semiRay, out _) == true);
            Assert.IsTrue(new Point3d(10.0, 10.0, 10.0).IsOnSemiInfiniteRay(semiRay, out _) == true);
            Assert.IsTrue(new Point3d(20.0, 20.0, 20.0).IsOnSemiInfiniteRay(semiRay, out _) == true);
            Assert.IsTrue(new Point3d(1.0e10, 1.0e10, 1.0e10).IsOnSemiInfiniteRay(semiRay, out _) == true);

            Assert.IsTrue(new Point3d(10.0, 10.0, 9.0).IsOnSemiInfiniteRay(semiRay, out _) == false);
            Assert.IsTrue(new Point3d(1.0, 1.0, 7.0).IsOnSemiInfiniteRay(semiRay, out _) == false);
            Assert.IsTrue(new Point3d(-5.0, -5.0, -5.0).IsOnSemiInfiniteRay(semiRay, out _) == false);
            Assert.IsTrue(new Point3d(-10.0, -10.0, -10.0).IsOnSemiInfiniteRay(semiRay, out _) == false);
        }

        [TestMethod]
        public void ShortestLineBetweenTwoRays1()
        {
            // Special case with parallel rays.
            var ray1 = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(5.0, 5.0, 0.0));
            var ray2 = new Line3d(new Point3d(0.0, 1.0, 0.0), new Point3d(10.0, 11.0, 0.0));

            ray1.CalcShortestLineBetweenTwoRays(ray2, out double mua, out double mub, out Point3d Pa, out Point3d Pb);
            Assert.AreEqual(0.0, mua, 1.0e-12);
            Assert.AreEqual(-0.05, mub, 1.0e-12);
            Assert.AreEqual(new Point3d(0.0, 0.0, 0.0), Pa);
            Assert.AreEqual(new Point3d(-0.5, 0.5, 0.0), Pb);

            ray2.CalcShortestLineBetweenTwoRays(ray1, out mua, out mub, out Pa, out Pb);
            Assert.AreEqual(0.0, mua, 1.0e-12);
            Assert.AreEqual(0.1, mub, 1.0e-12);
            Assert.AreEqual(new Point3d(0.0, 1.0, 0.0), Pa);
            Assert.AreEqual(new Point3d(0.5, 0.5, 0.0), Pb);

            // Special case with parallel rays.
            ray1 = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 10.0, 0.0));
            ray2 = new Line3d(new Point3d(0.0, 1.0, 0.0), new Point3d(5.0, 6.0, 0.0));

            ray1.CalcShortestLineBetweenTwoRays(ray2, out mua, out mub, out Pa, out Pb);
            Assert.AreEqual(0.0, mua, 1.0e-12);
            Assert.AreEqual(-0.1, mub, 1.0e-12);
            Assert.AreEqual(new Point3d(0.0, 0.0, 0.0), Pa);
            Assert.AreEqual(new Point3d(-0.5, 0.5, 0.0), Pb);

            ray2.CalcShortestLineBetweenTwoRays(ray1, out mua, out mub, out Pa, out Pb);
            Assert.AreEqual(0.0, mua, 1.0e-12);
            Assert.AreEqual(0.05, mub, 1.0e-12);
            Assert.AreEqual(new Point3d(0.0, 1.0, 0.0), Pa);
            Assert.AreEqual(new Point3d(0.5, 0.5, 0.0), Pb);

            // Not parallel rays.
            ray1 = new Line3d(new Point3d(2.85155814, 1.0, 5.21338934), new Point3d(19.97496484, 10.80492908, 2.71136512));
            ray2 = new Line3d(new Point3d(8.73672891, 3.26281315, 3.79638958), new Point3d(18.73672891, 13.26281315, 3.84826293));

            ray1.CalcShortestLineBetweenTwoRays(ray2, out mua, out mub, out Pa, out Pb);
            Assert.AreEqual(0.50768013089325725, mua, 1.0e-12);
            Assert.AreEqual(0.27618420755136275, mub, 1.0e-12);
            Assert.AreEqual(new Point3d(11.5447714947945, 5.97776767873351, 3.9431613564923), Pa);
            Assert.AreEqual(new Point3d(11.4985709855136, 6.02465522551363, 3.81071618006278), Pb);
        }

        [TestMethod]
        public void LineIntersectSemiInfiniteLine()
        {
            var semiRay = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 10.0, 0.0));

            var line1 = new Line3d(new Point3d(4.0, 2.0, 0.0), new Point3d(4.0, 8.0, 0.0));
            Assert.IsTrue(line1.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line1int));
            Assert.AreEqual(new Point3d(4.0, 4.0, 0.0), line1int);

            var line2 = new Line3d(new Point3d(4.0, 2.0, 0.05), new Point3d(4.0, 8.0, 0.05));
            Assert.IsTrue(line2.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line2int, 0.1));
            Assert.AreEqual(new Point3d(4.0, 4.0, 0.0), line2int);

            var line3 = new Line3d(new Point3d(-4.0, -2.0, 0.05), new Point3d(-4.0, -8.0, 0.05));
            Assert.IsFalse(line3.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line3int, 0.1));
            Assert.AreEqual(null, line3int);

            var line4 = new Line3d(new Point3d(15.0, 12.0, 0.05), new Point3d(15.0, 20.0, 0.05));
            Assert.IsTrue(line4.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line4int, 0.1));
            Assert.AreEqual(new Point3d(15.0, 15.0, 0.0), line4int);

            var line5 = new Line3d(new Point3d(5.0, 5.0, -9.0), new Point3d(5.0, 5.0, 9.0));
            Assert.IsTrue(line5.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line5int, 0.1));
            Assert.AreEqual(new Point3d(5.0, 5.0, 0.0), line5int);

            var line6 = new Line3d(new Point3d(0.0, 1.0, 0.0), new Point3d(10.0, 11.0, 0.0));
            Assert.IsFalse(line6.GetIntersectionWihtSemiInfiniteRay(semiRay, out Point3d line6int, 0.1));
            Assert.AreEqual(null, line6int);
        }
    }
}
