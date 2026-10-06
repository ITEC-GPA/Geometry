using GPC.Geometry;
using Maffeis.TestUtilities;
using GPC.Utilities.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Geometry
{
    [TestClass]
    public class CoordinateSystemTest : UnitTestBase
    {
        [TestMethod]
        public void CoordinateSystemTest1()
        {
            // Arrange

            Vector3d v1Expected = new Vector3d(-0.587, 0.809, 0);
            Vector3d v2Expected = new Vector3d(0.809, 0.587, 0);
            Vector3d v3Expected = new Vector3d(0, 0, -1);

            double tollerance = 0.001;

            Point3d p1 = new Point3d(10, 10, 0);
            Point3d p2 = new Point3d(-57.95, 103.69, 0.00);
            Point3d p3 = new Point3d(-3.92, 152.91, 0.00);

            // Act
            CoordinateSystem cs = new CoordinateSystem(p1, p2, p3);

            // Assert

            Assert.IsTrue((cs.V1 - v1Expected).Length < tollerance);
            Assert.IsTrue((cs.V2 - v2Expected).Length < tollerance);
            Assert.IsTrue((cs.V3 - v3Expected).Length < tollerance);

        }

        [TestMethod]
        public void CoordinateSystemTest2()
        {
            // Arrange
            Point3d o = new Point3d(10, 10, 10);
            Vector3d v1 = new Vector3d(6.49, 87.20, 0.00);
            Vector3d v2 = new Vector3d(79.03, 74.19, 0.00);

            bool assert = false;

            // Act
            try
            {
                CoordinateSystem cs = new CoordinateSystem(o, v1, v2);
            }
            catch (ArgumentException)
            {
                assert = true;
                // vettori non ortogonali lanciano una argument exception
            }

            // Assert
            Assert.IsTrue(assert);
        }

        [TestMethod]
        public void CoordinateSystemTest3()
        {
            // Arrange
            double tollerance = 0.001;

            Point3d o = new Point3d(0, 0, 0);
            Vector3d v1 = new Vector3d(13.64, 23.96, 0.00);
            Vector3d v2 = new Vector3d(-48.09, 27.38, 0.00);

            // Act
            CoordinateSystem cs = new CoordinateSystem(o, v1, v2);

            // Assert

            Vector3d v1Expected = v1;
            Vector3d v2Expected = v2;
            Vector3d v3Expected = new Vector3d(0, 0, 1);

            Assert.IsTrue((cs.V1 - v1Expected).Length < tollerance);
            Assert.IsTrue((cs.V2 - v2Expected).Length < tollerance);
            Assert.IsTrue((cs.V3 - v3Expected).Length < tollerance);
        }

        [TestMethod]
        public void CoordinateSystemTest4()
        {
            // sistema di riferimento locale
            Point3d origin = new Point3d(1, 1, 1);
            Vector3d asseX = new Vector3d(1, 0, 0);
            Vector3d asseY = new Vector3d(0, 1, 0);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            Console.WriteLine(coordinateSystem.V1);
            Console.WriteLine(coordinateSystem.V2);
            Console.WriteLine(coordinateSystem.V3);


            Assert.AreEqual(coordinateSystem.V1, CoordinateSystem.Global.V1);
            Assert.AreEqual(coordinateSystem.V2, CoordinateSystem.Global.V2);
            Assert.AreEqual(coordinateSystem.V3, CoordinateSystem.Global.V3);
        }

        [TestMethod]
        public void Constructor_DoesNotModifyOrShareTheArgumentVectors()
        {
            // Non-unit arguments: before, they were unitized in place and kept as the axes of the system.
            Vector3d x = new Vector3d(2, 0, 0), y = new Vector3d(0, 3, 0), z = new Vector3d(0, 0, 4);
            var coordinateSystem = new CoordinateSystem(new Point3d(1, 2, 3), x, y, z, "CS");

            Assert.AreEqual(2, x.X); Assert.AreEqual(3, y.Y); Assert.AreEqual(4, z.Z);
            Assert.IsFalse(ReferenceEquals(x, coordinateSystem.V1) || ReferenceEquals(y, coordinateSystem.V2) || ReferenceEquals(z, coordinateSystem.V3));
            Assert.AreEqual(1, coordinateSystem.V1.X); Assert.AreEqual(1, coordinateSystem.V2.Y); Assert.AreEqual(1, coordinateSystem.V3.Z);

            // A later change of the arguments does not reach the system.
            x.X = -5; y.Y = 7;
            Assert.AreEqual(1, coordinateSystem.V1.X);
            Assert.AreEqual(new Point3d(2, 2, 3), coordinateSystem.ToGlobal(new Point3d(1, 0, 0)));

            // The copy constructor does not share the axes of the source.
            var copy = new CoordinateSystem(coordinateSystem);
            Assert.IsFalse(ReferenceEquals(copy.V1, coordinateSystem.V1) || ReferenceEquals(copy.V2, coordinateSystem.V2));
        }

        [TestMethod]
        public void CoordinateSystemTest5()
        {
            // sistema di riferimento sinistrorso
            Point3d origin = new Point3d(0, 0, 0);
            Vector3d asseX = new Vector3d(-1, 0, 0);
            Vector3d asseY = new Vector3d(0, 1, 0);
            Vector3d asseZ = new Vector3d(0, 0, 1);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, asseZ, "CS");

            Console.WriteLine(coordinateSystem.V1);
            Console.WriteLine(coordinateSystem.V2);
            Console.WriteLine(coordinateSystem.V3);


            Assert.AreEqual(coordinateSystem.V1, new Vector3d(-1, 0, 0));
            Assert.AreEqual(coordinateSystem.V2, new Vector3d(0, +1, 0));
            Assert.AreEqual(coordinateSystem.V3, new Vector3d(0, 0, +1));
        }

        [TestMethod]
        public void CoordinateSystemTest6()
        {
            // sistema di riferimento sinistrorso
            Point3d origin = new Point3d(0, 0, 0);
            Vector3d asseX = new Vector3d(-1, 0, 0);
            Vector3d asseY = new Vector3d(0, 1, 0);
            Vector3d asseZ = new Vector3d(0, 0, -1);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, asseZ, "CS");

            Console.WriteLine(coordinateSystem.V1);
            Console.WriteLine(coordinateSystem.V2);
            Console.WriteLine(coordinateSystem.V3);


            Assert.AreEqual(coordinateSystem.V1, new Vector3d(-1, 0, 0));
            Assert.AreEqual(coordinateSystem.V2, new Vector3d(0, +1, 0));
            Assert.AreEqual(coordinateSystem.V3, new Vector3d(0, 0, -1));
        }

        [TestMethod]
        public void CoordinateSystemTest7()
        {
            Point3d origin = new Point3d(1, 2, 3);
            Vector3d asseX = new Vector3d(4, 5, 6);
            Vector3d asseY = Vector3d.ZAxis ^ asseX;

            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            MathNet.Numerics.LinearAlgebra.Matrix<double> matrixStart = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(3, 4);
            coordinateSystem.TrfMatrix.CopyTo(matrixStart);

            coordinateSystem.Rotate(1, 2, 3);

            Console.WriteLine(coordinateSystem.V1);
            Console.WriteLine(coordinateSystem.V2);
            Console.WriteLine(coordinateSystem.V3);

            Console.Write(matrixStart);
            Console.Write(coordinateSystem.TrfMatrix);
            Assert.AreNotEqual(coordinateSystem.TrfMatrix, matrixStart);
        }

        [TestMethod]
        public void CoordinateSystemTest8()
        {
            Point3d origin = new Point3d(1, 2, 3);
            Vector3d asseX = new Vector3d(4, 5, 6);
            Vector3d asseY = Vector3d.ZAxis ^ asseX;

            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            MathNet.Numerics.LinearAlgebra.Matrix<double> matrixStart = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(3, 4);
            coordinateSystem.TrfMatrix.CopyTo(matrixStart);

            coordinateSystem.Rotate(0, 0, 0);

            Console.WriteLine(coordinateSystem.V1);
            Console.WriteLine(coordinateSystem.V2);
            Console.WriteLine(coordinateSystem.V3);

            Console.Write(matrixStart);
            Console.Write(coordinateSystem.TrfMatrix);

            foreach (var item in coordinateSystem.TrfMatrix.EnumerateIndexed())
            {
                Assert.AreEqual(matrixStart[item.Item1, item.Item2], item.Item3, 0.00001);
            }
        }

        #region PointTo / VectorTo

        [TestMethod]
        public void PointTo()
        {
            // Arrange
            double tollerance = 0.001;

            Point3d o = new Point3d(0, 0, 0);
            Vector3d v2 = new Vector3d(-48.09, 27.38, 0.00);
            Vector3d v1 = new Vector3d(13.64, 23.96, 0.00);

            Point3d poingGlobal = new Point3d(8.57, 23.70, 0.00);
            // Act
            CoordinateSystem cs = new CoordinateSystem(o, v1, v2);

            // Assert

            Point3d pointLocalExpected = new Point3d(24.8362, 4.2786, 0.00);

            Point3d pointLocal = cs.ToLocal(poingGlobal);
            Assert.IsTrue(pointLocal.DistanceTo(pointLocalExpected) < tollerance);
        }

        [TestMethod]
        public void PointTo2()
        {
            // Arrange
            double tollerance = 0.006;

            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(43.44, 15.34, -7.87);
            Point3d p3 = new Point3d(-9.29, 41.74, 30.08);

            // Act
            CoordinateSystem cs = new CoordinateSystem(p1, p2, p3);

            // Assert
            Point3d pointGlobal = new Point3d(34.13, 22.07, 15.47);

            var pointLocalExpected = new Point3d(36.36, 20.46, 12.27);

            Point3d pointLocal = cs.ToLocal(pointGlobal);

            Assert.IsTrue(pointLocal.DistanceTo(pointLocalExpected) < tollerance, pointLocal.DistanceTo(pointLocalExpected).ToString());
        }

        [TestMethod]
        public void LineTo()
        {
            // Arrange
            double tollerance = 0.01;

            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(43.44, 15.34, -7.87);
            Point3d p3 = new Point3d(-9.29, 41.74, 30.08);

            // Act
            CoordinateSystem cs = new CoordinateSystem(p1, p2, p3);

            // Assert
            Line3d line = new Line3d(new Point3d(34.13, 22.07, 15.47), new Point3d(4.44, 32.96, 25.95));
            Line3d lineLocalExpected = new Line3d(new Point3d(36.36, 20.46, 12.27), new Point3d(10.57, 40.46, 5.57));

            Line3d lineLocal = cs.ToLocal(line);

            Assert.IsTrue(lineLocal.Start.DistanceTo(lineLocalExpected.Start) < tollerance, lineLocal.Start.DistanceTo(lineLocalExpected.Start).ToString());
            Assert.IsTrue(lineLocal.End.DistanceTo(lineLocalExpected.End) < tollerance, lineLocal.End.DistanceTo(lineLocalExpected.End).ToString());
        }

        [TestMethod]
        public void VectorTo1()
        {
            // Test fatti con rhino 

            // Arrange
            Vector3d v1 = new Vector3d(1.0, 0.5, 0.70);
            v1.Unitize();
            Vector3d v1Expected = new Vector3d(1.10, -0.21, 0.70);
            v1Expected.Unitize();

            // Act
            CoordinateSystem cs = new CoordinateSystem(new Point3d(0.00, 0.00, 0.00), new Point3d(0.80, 0.60, 0.00), new Point3d(-0.60, 0.80, 0.00));

            // Assert
            Vector3d v1Local = cs.ToLocal(v1);

            Console.WriteLine(cs.V1);
            Console.WriteLine(cs.V2);
            Console.WriteLine(cs.V3);

            Console.WriteLine(v1Local);

            Console.WriteLine(v1Local.Length);
            Console.WriteLine(v1Expected.Length);

            Assert.AreEqual(1, v1Local.DotProduct(v1Expected), 0.01, $"Global: {v1} Local: {v1Local} Dot: {v1Local.DotProduct(v1).ToString()}");
        }

        [TestMethod]
        public void VectorTo2()
        {
            // Test fatti con rhino 

            // Arrange
            Vector3d v1 = new Vector3d(1.0, 0.5, 0.70);
            v1.Unitize();
            Vector3d v1Expected = new Vector3d(1.29, 0.25, -0.14);
            v1Expected.Unitize();

            // Act
            CoordinateSystem cs = new CoordinateSystem(new Point3d(0.00, 0.00, 0.00), new Point3d(0.85, 0.18, 0.50), new Point3d(-0.49, 0.64, 0.59));
            CoordinateSystem csMoved = new CoordinateSystem(new Point3d(0.94, 0.63, 1.05), new Point3d(1.79, 0.81, 1.54), new Point3d(0.45, 1.27, 1.64));

            // Assert
            Vector3d v1Local = cs.ToLocal(v1);
            Vector3d v1LocalMoved = csMoved.ToLocal(v1);

            Console.WriteLine(cs.V1);
            Console.WriteLine(cs.V2);
            Console.WriteLine(cs.V3);

            Console.WriteLine(v1Local);
            Console.WriteLine(v1Local);

            Console.WriteLine(v1Local.Length);
            Console.WriteLine(v1Expected.Length);


            Assert.AreEqual(1, v1Local.DotProduct(v1Expected), 0.01, $"Global: {v1} Local: {v1Local} Dot: {v1Local.DotProduct(v1).ToString()}");
            Assert.AreEqual(1, v1LocalMoved.DotProduct(v1Expected), 0.01, $"Global: {v1} Local: {v1LocalMoved} Dot: {v1LocalMoved.DotProduct(v1).ToString()}");
        }

        [TestMethod]
        public void VectorTo3()
        {
            // Test fatti con rhino 

            // Arrange
            Vector3d v1 = new Vector3d(0, 0, 1);
            v1.Unitize();

            Vector3d v1Expected = new Vector3d(0, 0, 1);


            Plane referencePlane = new Plane(new Point3d(1, 1, 1),
                                             new Vector3d(1, 0, 0),
                                             new Vector3d(0, 1, 0));

            CoordinateSystem cs = referencePlane.GetCoordinateSystem();

            // Act
            Vector3d v1Local = cs.ToLocal(v1);

            // Assert
            Console.WriteLine(cs.Origin);
            Console.WriteLine(cs.V1);
            Console.WriteLine(cs.V2);
            Console.WriteLine(cs.V3);

            Console.WriteLine(v1Local);

            Console.WriteLine(v1Local.Length);
            Console.WriteLine(v1Expected.Length);

            Assert.AreEqual(1, v1Local.DotProduct(v1Expected), 0.01, $"Global: {v1} Local: {v1Local} Dot: {v1Local.DotProduct(v1).ToString()}");
        }

        [TestMethod]
        public void VectorTo4()
        {
            // Test fatti con rhino 

            // Arrange
            Vector3d v1 = new Vector3d(1, 1, 1);
            v1.Unitize();

            Vector3d v1Expected = new Vector3d(1, 1, 1);
            v1Expected.Unitize();


            Plane referencePlane = new Plane(new Point3d(1, 1, 1),
                                             new Vector3d(1, 0, 0),
                                             new Vector3d(0, 1, 0));

            CoordinateSystem cs = referencePlane.GetCoordinateSystem();

            // Act
            Vector3d v1Global = cs.ToGlobal(v1);

            // Assert
            Console.WriteLine(cs.Origin);
            Console.WriteLine(cs.V1);
            Console.WriteLine(cs.V2);
            Console.WriteLine(cs.V3);

            Console.WriteLine(v1Global);

            Console.WriteLine(v1Global.Length);
            Console.WriteLine(v1Expected.Length);

            Assert.AreEqual(1, v1Global.DotProduct(v1Expected), 0.01, $"Global: {v1} Local: {v1Global} Dot: {v1Global.DotProduct(v1).ToString()}");
        }

        [TestMethod]
        public void VectorTo5()
        {

            CoordinateSystem cs = new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(1, 1, 0), new Vector3d(-1, 1, 0));

            Point3d p1 = new Point3d(1, 0, 0);
            Vector3d v1 = new Vector3d(1, 0, 0);

            Console.WriteLine(cs.V1);
            Console.WriteLine(cs.V2);
            Console.WriteLine(cs.V3);

            Console.WriteLine(cs.ToGlobal(v1));
            Console.WriteLine(cs.ToGlobal(p1));

            Assert.AreEqual(new Vector3d(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0), cs.ToGlobal(v1));
        }

        #endregion

        #region Rotation

        [TestMethod]
        public void Rotate()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;
            Point3d pointToTest = new Point3d(10, 10, 10);

            // Act
            double rotationeAngle = 90.ToRadians();
            CS.RotateY(rotationeAngle);

            Point3d expPoint = new Point3d(-10, +10, +10);

            Console.WriteLine(CS.ToLocal(pointToTest));

            // Assert
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).X - expPoint.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Y - expPoint.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Z - expPoint.Z) < tolerance);

        }

        [TestMethod]
        public void Rotate_1()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;
            Point3d pointToTest = new Point3d(10, 10, 10);

            // Act
            double rotationeAngleV1 = Math.PI;
            CS.RotateX(rotationeAngleV1);
            Point3d expPoint = new Point3d(10.000, -10.000, -10.000);

            // Assert
            Point3d point1 = CS.ToLocal(pointToTest);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).X - expPoint.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Y - expPoint.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Z - expPoint.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_2()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;
            Point3d pointToTest = new Point3d(10, 10, 10);

            // Act
            double rotationeAngleV2 = Math.PI;
            CS.RotateY(rotationeAngleV2);
            Point3d expPoint = new Point3d(-10.000, +10.000, -10.000);

            // Assert
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).X - expPoint.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Y - expPoint.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Z - expPoint.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_3()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;
            Point3d pointToTest = new Point3d(10, 10, 10);

            // Act
            double rotationeAngle = Math.PI;
            CS.RotateZ(rotationeAngle);
            Point3d expPoint = new Point3d(-10.000, -10.000, +10.000);

            // Assert
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).X - expPoint.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Y - expPoint.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.ToLocal(pointToTest).Z - expPoint.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_3D_1()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = Math.PI;
            double rotationeAngleV2 = Math.PI;
            double rotationeAngleV3 = Math.PI;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(1, 0, 0);
            Vector3d expV2 = new Vector3d(0, 1, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_4()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = 0;
            double rotationeAngleV2 = Math.PI / 4;
            double rotationeAngleV3 = 0;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(0.707, 0, -0.707);
            Vector3d expV2 = new Vector3d(0, 1, 0);
            Vector3d expV3 = new Vector3d(0.707, 0, 0.707);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_5()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = 0;
            double rotationeAngleV2 = 0;
            double rotationeAngleV3 = Math.PI / 4;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(0.707, +0.707, 0);
            Vector3d expV2 = new Vector3d(-0.707, 0.707, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_7()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = 0;
            double rotationeAngleV2 = 0;
            double rotationeAngleV3 = Math.PI / 6;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(0.866, 0.50, 0);
            Vector3d expV2 = new Vector3d(-0.5, 0.866, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_8()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = 0;
            double rotationeAngleV2 = Math.PI / 6;
            double rotationeAngleV3 = 0;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(0.866, 0, -0.50);
            Vector3d expV2 = new Vector3d(0, 1, 0);
            Vector3d expV3 = new Vector3d(0.5, 0, 0.866);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_9()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = Math.PI / 6;
            double rotationeAngleV2 = 0;
            double rotationeAngleV3 = 0;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(1, 0, 0);
            Vector3d expV2 = new Vector3d(0, 0.866025, 0.50);
            Vector3d expV3 = new Vector3d(0, -0.5, 0.866);


            // Assert
            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate_6()
        {
            // Arrange
            CoordinateSystem CS = CoordinateSystem.Global;
            double tolerance = 0.01;

            // Act
            double rotationeAngleV1 = 0;
            double rotationeAngleV2 = 0;
            double rotationeAngleV3 = -Math.PI / 4;
            CS.Rotate(rotationeAngleV1, rotationeAngleV2, rotationeAngleV3);

            Vector3d expV1 = new Vector3d(+0.707, -0.707, 0);
            Vector3d expV2 = new Vector3d(+0.707, 0.707, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            // Assert
            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void Rotate10()
        {
            // Arrange
            double tolerance = 0.01;

            CoordinateSystem CS = new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));

            // Act
            CS.RotateZ(45.ToRadians());

            Vector3d expV1 = new Vector3d(+0.707106781186548, 0.707106781186548, 0);
            Vector3d expV2 = new Vector3d(-0.707106781186548, 0.707106781186548, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            // Assert

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void RotateV1()
        {
            // Arrange
            double tolerance = 0.01;
            CoordinateSystem CS = CoordinateSystem.Global;

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            // Act
            double rotationeAngle = 90.ToRadians();
            CS.RotateX(rotationeAngle);

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            Vector3d expV1 = new Vector3d(1, 0, 0);
            Vector3d expV2 = new Vector3d(0, 0, 1);
            Vector3d expV3 = new Vector3d(0, -1, 0);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void RotateV2()
        {
            // Arrange
            double tolerance = 0.01;
            CoordinateSystem CS = CoordinateSystem.Global;

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            // Act
            double rotationeAngle = 90.ToRadians();
            CS.RotateY(rotationeAngle);

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            Vector3d expV1 = new Vector3d(0, 0, -1);
            Vector3d expV2 = new Vector3d(0, 1, 0);
            Vector3d expV3 = new Vector3d(1, 0, 0);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void RotateV3()
        {
            // Arrange
            double tolerance = 0.01;
            CoordinateSystem CS = CoordinateSystem.Global;

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);


            // Act
            double rotationeAngle = 90.ToRadians();
            CS.RotateZ(rotationeAngle);

            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            Vector3d expV1 = new Vector3d(0, 1, 0);
            Vector3d expV2 = new Vector3d(-1, 0, 0);
            Vector3d expV3 = new Vector3d(0, 0, 1);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void RotateV3_2()
        {
            // Arrange
            double tolerance = 0.01;
            CoordinateSystem CS = new CoordinateSystem(new Point3d(), new Vector3d(0, 0, -1), new Vector3d(0, 1, 0), new Vector3d(1, 0, 0));

            Console.WriteLine("Before rotation");
            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            // Act
            CS.RotateX(45.ToRadians());

            Console.WriteLine("After rotation");
            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            Vector3d expV1 = new Vector3d(0, 0.707106781186547, -0.707106781186547);
            Vector3d expV2 = new Vector3d(0, 0.707106781186547, 0.707106781186547);
            Vector3d expV3 = new Vector3d(1, 0, 0);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        [TestMethod]
        public void RotateV3_3()
        {
            // Arrange
            double tolerance = 0.01;
            CoordinateSystem CS = new CoordinateSystem(new Point3d(), new Vector3d(0, 0, -1), new Vector3d(0, 1, 0), new Vector3d(1, 0, 0));

            Console.WriteLine("Before rotation");
            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            // Act
            CS.RotateV3(45.ToRadians());

            Console.WriteLine("After rotation");
            Console.WriteLine(CS.V1);
            Console.WriteLine(CS.V2);
            Console.WriteLine(CS.V3);

            Vector3d expV1 = new Vector3d(0, 0.707106781186547, -0.707106781186547);
            Vector3d expV2 = new Vector3d(0, 0.707106781186547, 0.707106781186547);
            Vector3d expV3 = new Vector3d(1, 0, 0);

            Assert.IsTrue(Math.Abs(CS.V1.X - expV1.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Y - expV1.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V1.Z - expV1.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V2.X - expV2.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Y - expV2.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V2.Z - expV2.Z) < tolerance);

            Assert.IsTrue(Math.Abs(CS.V3.X - expV3.X) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Y - expV3.Y) < tolerance);
            Assert.IsTrue(Math.Abs(CS.V3.Z - expV3.Z) < tolerance);
        }

        #endregion
    }
}
