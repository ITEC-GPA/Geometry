using System;
using System.Collections.Generic;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maffeis.TestUtilities;

namespace Geometry
{
    [TestClass]
    public class Line2dTest : UnitTestBase
    {
        protected double _tolerance = 0.001;

        [TestMethod]
        public void Length1()
        {
            Point2d from = new Point2d(0, 0);
            Point2d to = new Point2d(0, 0);
            Line2d line = new Line2d(from, to);
            double expected = 0;
            double found = line.GetLength();
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Length2()
        {
            Point2d from = new Point2d(82, 90);
            Point2d to = new Point2d(-31, -75);
            Line2d line = new Line2d(from, to);
            double expected = 199.985;
            double found = line.GetLength();
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Intersection1()
        {
            Point2d from1 = new Point2d(82, 90);
            Point2d to1 = new Point2d(-31, -75);
            Line2d line1 = new Line2d(from1, to1);
            Point2d from2 = new Point2d(-50, 59);
            Point2d to2 = new Point2d(39, -65);
            Line2d line2 = new Line2d(from2, to2);
            Point2d expected = new Point2d(6.68373, -19.9751);
            bool result = line1.GetIntersection(line2, out Point2d found);
            Assert.IsTrue(result, "No intersections found");
            double diff = expected.DistanceTo(found);
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Intersection2()
        {
            Point2d from1 = new Point2d(82, 90);
            Point2d to1 = new Point2d(-31, -75);
            Line2d line1 = new Line2d(from1, to1);

            Point2d from2 = new Point2d(-118, 64);
            Point2d to2 = new Point2d(-67, -67);
            Line2d line2 = new Line2d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point2d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");            
        }
        [TestMethod]
        public void Intersection3()
        {
            Point2d from1 = new Point2d(0.0, 0.0);
            Point2d to1 = new Point2d(0.242417120479441, 0.970172118594662);
            Line2d line1 = new Line2d(from1, to1);

            Point2d from2 = new Point2d(-407141.981865088, -1253014.40141545);
            Point2d to2 = new Point2d(-307593.826398102, -1281172.49791966);
            Line2d line2 = new Line2d(from2, to2);

            bool result = line1.GetIntersection(line2, out Point2d found);
            Assert.IsFalse(result, $"No intersections should be found, Result: {found}");
        }

        [TestMethod]
        public void DistanceToPoint1()
        {
            Point2d from = new Point2d(82, 90);
            Point2d to = new Point2d(-31, -75);
            Line2d line = new Line2d(from, to);
            double expected = 42.8982;
            double found = line.DistanceTo(new Point2d(84, 17));
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void DistanceToPoint2()
        {
            Point2d from = new Point2d(-118, 64);
            Point2d to = new Point2d(-67, -67);
            Line2d line = new Line2d(from, to);
            double expected = 30.6453;
            double found = line.DistanceTo(new Point2d(-31, -75));
            double diff = expected - found;
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void Intersection4()
        {
            Point2d from1 = new Point2d(82.5587, 90.1225);
            Point2d to1 = new Point2d(-31.4545, -75.2447);
            Line2d line1 = new Line2d(from1, to1);
            Point2d from2 = new Point2d(-23, 75);
            Point2d to2 = new Point2d(25.0409, -60.7966);
            Line2d line2 = new Line2d(from2, to2);
            Point3d expected = new Point2d(9.26062, -16.1906);
            bool result = line1.GetIntersection(line2, out Point2d found);
            Assert.IsTrue(result, "No intersections found");
            double diff = expected.DistanceTo(found);
            Assert.IsTrue(diff < _tolerance, $"Expected: {expected}, Result: {found}, Difference: {diff}");
        }

        [TestMethod]
        public void IntersectionLineeCoincidenti()
        {
            // Arrange 
            Point2d from1 = new Point2d(82.5587, 90.1225);
            Point2d to1 = new Point2d(-31.4545, -75.2447);
            Line2d line1 = new Line2d(from1, to1);
            Point2d from2 = new Point2d(82.5587, 90.1225);
            Point2d to2 = new Point2d(-31.4545, -75.2447);
            Line2d line2 = new Line2d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point2d _);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectionLineeSovrapposte()
        {
            // Arrange 
            Point2d from1 = new Point2d(1, 1);
            Point2d to1 = new Point2d(3, 3);
            Line2d line1 = new Line2d(from1, to1);
            Point2d from2 = new Point2d(2, 2);
            Point2d to2 = new Point2d(4, 4);
            Line2d line2 = new Line2d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point2d _);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectionLineeSovrapposte2()
        {
            // Arrange 
            Point2d from1 = new Point2d(1, 1);
            Point2d to1 = new Point2d(3, 3);
            Line2d line1 = new Line2d(from1, to1);
            Point2d from2 = new Point2d(2, 2);
            Point2d to2 = new Point2d(-4, -4);
            Line2d line2 = new Line2d(from2, to2);

            //Act
            bool result = line1.GetIntersection(line2, out Point2d _);

            //Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Intersection5()
        {
            //Arrange
            Polygon2d poly1 = new Polygon2d()
            {
                new Point2d(2, 2),
                new Point2d(4, 2),
                new Point2d(4, 4),
                new Point2d(6, 6),
                new Point2d(2, 6),
            };

			Line2d[] edge = poly1.Explode();

            //Act
            Point2d p1 = new Point2d(2, 2);
            Point2d p2 = new Point2d(4, 2);
            Point2d p3 = new Point2d(4, 4);
            Point2d p4 = new Point2d(6, 6);
            Point2d p5 = new Point2d(2, 6);
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p2, p3);
            Line2d line3 = new Line2d(p3, p4);
            Line2d line4 = new Line2d(p4, p5);
            Line2d line5 = new Line2d(p5, p1);

            Point2d pointToTest = new Point2d(3, 4);
            Point2d BBoxMax = new Point2d(8, 8);
            Line2d lineToCheck = new Line2d(pointToTest, BBoxMax);

            //Assert;
            Assert.IsTrue(edge[0] == line1);
            Assert.IsTrue(edge[1] == line2);
            Assert.IsTrue(edge[2] == line3);
            Assert.IsTrue(edge[3] == line4);
            Assert.IsTrue(edge[4] == line5);
            Assert.IsTrue(lineToCheck.GetIntersection(line4, out _));
            Assert.IsFalse(lineToCheck.GetIntersection(line1, out _));
            Assert.IsFalse(lineToCheck.GetIntersection(line2, out _));
            Assert.IsFalse(lineToCheck.GetIntersection(line3, out _));
            Assert.IsFalse(lineToCheck.GetIntersection(line5, out _));
        }

        [TestMethod]
        public void Intersection6()
        {
            //Arrange
            Point2d p1 = new Point2d(2, 2);
            Point2d p2 = new Point2d(4, 4);
            Point2d p3 = new Point2d(6, 6);

            //Act
            Line2d line1 = new Line2d(p1, p2); 
            Line2d line2 = new Line2d(p2, p3);

            //Assert;
            Assert.IsTrue(line1.GetIntersection(line2, out _));
        }

        [TestMethod]
        public void Intersection7()
        {
            //Arrange
            Point2d p1 = new Point2d(0, 1);
            Point2d p2 = new Point2d(1, 1);
            Point2d p3 = new Point2d(2, 1);
            Point2d p4 = new Point2d(1, 2);
            Point2d p5 = new Point2d(0.5, 1);
            Point2d p6 = new Point2d(-1, 1);

            //Act
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p2, p3);
            Line2d line21 = new Line2d(p3, p2);
            Line2d line3 = new Line2d(p2, p4);
            Line2d line4 = new Line2d(p1, p2);
            Line2d line5 = new Line2d(p5, p2);
            Line2d line6 = new Line2d(p6, p2);

            //Assert;
            Assert.IsTrue(line1.GetIntersection(line2, out _));
            Assert.IsTrue(line1.GetIntersection(line21, out _));
            Assert.IsTrue(line1.GetIntersection(line3, out _));
            Assert.IsFalse(line1.GetIntersection(line4, out _));
            Assert.IsFalse(line1.GetIntersection(line5, out _));
            Assert.IsFalse(line1.GetIntersection(line6, out _));
        }

        [TestMethod]
        public void IntersectionWithInfiniteLine1()
        {
            //Arrange
            Point2d p1 = new Point2d(400, 0);
            Point2d p2 = new Point2d(400, 400);
            Point2d p3 = new Point2d(200, 200);
            Point2d p4 = new Point2d(201, 200);

            //Act
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p3, p4);

            //Assert;
            Assert.IsTrue(line1.GetIntersectionWithInfiniteLine(line2, out _));
        }

        [TestMethod]
        public void IntersectionWithInfiniteLine2()
        {
            //Arrange
            Point2d p1 = new Point2d(0, 400);
            Point2d p2 = new Point2d(400, 400);
            Point2d p3 = new Point2d(200, 200);
            Point2d p4 = new Point2d(201, 200);
            Point2d p5 = new Point2d(200, 201);

            //Act
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p3, p4);
            Line2d line3 = new Line2d(p3, p5);

            //Assert;
            Assert.IsFalse(line1.GetIntersectionWithInfiniteLine(line2, out _));
            Assert.IsTrue(line1.GetIntersectionWithInfiniteLine(line3, out _));
        }

        [TestMethod]
        public void IntersectionWithInfiniteLine3()
        {
            //Arrange
            Point2d p1 = new Point2d(0, 400);
            Point2d p2 = new Point2d(400, 400);
            Point2d p3 = new Point2d(200, 200);
            Point2d p4 = new Point2d(600, 200);
            Point2d p5 = new Point2d(200, 600);

            //Act
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p3, p4);
            Line2d line3 = new Line2d(p3, p5);

            //Assert;
            Assert.IsFalse(line1.GetIntersectionWithInfiniteLine(line2, out _));
            Assert.IsTrue(line1.GetIntersectionWithInfiniteLine(line3, out _));
        }

        [TestMethod]
        public void IntersectionWithInfiniteLine4()
        {
            //Arrange
            Point2d p1 = new Point2d(0, 0);
            Point2d p2 = new Point2d(100, 100);
            Point2d p3 = new Point2d(200, 200);
            Point2d p4 = new Point2d(600, 200);
            Point2d p5 = new Point2d(200, 600);

            //Act
            Line2d line1 = new Line2d(p1, p2);
            Line2d line2 = new Line2d(p3, p4);
            Line2d line3 = new Line2d(p3, p5);

            //Assert;
            Assert.IsTrue(line1.GetIntersectionWithInfiniteLine(line2, out _));
            Assert.IsTrue(line1.GetIntersectionWithInfiniteLine(line3, out _));
        }

        [TestMethod]
        public void Rotation()
        {
            // Arrange 
            Point2d from1 = new Point2d(0, 2);
            Point2d to1 = new Point2d(2, 2);
            Line2d line1 = new Line2d(from1, to1);
            Point2d RotationPoint = new Point2d(0, 0);

            //Act
            line1.Rotate(RotationPoint, Math.PI);
            Point2d newStart = new Point2d(0, -2);
            Point2d newEnd = new Point2d(-2, -2);
            Line2d expLine = new Line2d(newStart, newEnd);

            //Assert
            Assert.IsTrue(Math.Abs(line1.Start.X - newStart.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.Start.Y - newStart.Y) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.X - newEnd.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.Y - newEnd.Y) < 0.001);
            Assert.IsTrue(line1 == expLine);

        }

        [TestMethod]
        public void Rotation2()
        {
            // Arrange 
            Point2d from1 = new Point2d(5, 2);
            Point2d to1 = new Point2d(-10, 4);
            Line2d line1 = new Line2d(from1, to1);
            Point2d RotationPoint = new Point2d(1, 1);

            //Act
            line1.Rotate(RotationPoint, Math.PI/4);
            Point2d newStart = new Point2d(3.1213203, 4.5355339);
            Point2d newEnd = new Point2d(-8.8994949, -4.6568542);
            Line2d expLine = new Line2d(newStart, newEnd);

            //Assert
            Assert.IsTrue(Math.Abs(line1.Start.X - newStart.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.Start.Y - newStart.Y) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.X - newEnd.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.Y - newEnd.Y) < 0.001);
            Assert.IsTrue(line1 == expLine);
        }

        [TestMethod]
        public void Rotation5()
        {
            // Arrange 
            Point2d from1 = new Point2d(5, 2);
            Point2d to1 = new Point2d(-10, 4);
            Line2d line1 = new Line2d(from1, to1);
            Point2d RotationPoint = new Point2d(1, 1);

            //Act
            line1.Rotate(RotationPoint, Math.PI / 2);
            Point2d newStart = new Point2d(0.0000000, 5.0000000);
            Point2d newEnd = new Point2d(-2.0000000, -10.0000000);
            Line2d expLine = new Line2d(newStart, newEnd);

            //Assert
            Assert.IsTrue(Math.Abs(line1.Start.X - newStart.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.Start.Y - newStart.Y) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.X - newEnd.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.Y - newEnd.Y) < 0.001);
            Assert.IsTrue(line1 == expLine);

        }
        
        [TestMethod]
        public void Rotation3()
        {
            // Arrange 
            Point2d from1 = new Point2d(5, 2);
            Point2d to1 = new Point2d(-10, 4);
            Line2d line1 = new Line2d(from1, to1);
            Point2d RotationPoint = new Point2d(1, 1);

            //Act
            line1.Rotate(RotationPoint, -Math.PI / 4);
            Point2d newStart = new Point2d(4.5355339, -1.1213203);
            Point2d newEnd = new Point2d(-4.6568542, 10.8994949);
            Line2d expLine = new Line2d(newStart, newEnd);

            //Assert
            Assert.IsTrue(Math.Abs(line1.Start.X - newStart.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.Start.Y - newStart.Y) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.X - newEnd.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.Y - newEnd.Y) < 0.001);
            Assert.IsTrue(line1 == expLine);
        }

        [TestMethod]
        public void Rotation4()
        {
            // Arrange 
            Point2d from1 = new Point2d(-17.7771961, 3.5364489);
            Point2d to1 = new Point2d(8.2611882, 8.8589242);
            Line2d line1 = new Line2d(from1, to1);
            Point2d RotationPoint = new Point2d(-8.0715057, -2.6731057);

            //Act
            line1.Rotate(RotationPoint, Math.PI / 6);
            Point2d newStart = new Point2d(-19.5816574, -2.1483189);
            Point2d newEnd = new Point2d(0.3070072, 15.4802721);
            Line2d expLine = new Line2d(newStart, newEnd);

            //Assert
            Assert.IsTrue(Math.Abs(line1.Start.X - newStart.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.Start.Y - newStart.Y) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.X - newEnd.X) < 0.001);
            Assert.IsTrue(Math.Abs(line1.End.Y - newEnd.Y) < 0.001);
            Assert.IsTrue(line1 == expLine);
        }
    }
}
