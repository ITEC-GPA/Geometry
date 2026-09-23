using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Maffeis.TestUtilities;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;

namespace Geometry
{
    [TestClass]
    public class PlaneTest : UnitTestBase
    {
        [TestMethod]
        public void IsPointOnPlane1()
        {
            //Arrange
            Point3d p1 = new Point3d(2, 5, 1);
            Point3d p2 = new Point3d(3, 2, 3);
            Point3d p3 = new Point3d(0, 5, 4);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(1.66666, 4.5, 2.666666);
            Point3d point2 = new Point3d(4, 4.5, 1);
            Point3d point3 = new Point3d(1.66666, 4.1, 2.666666);
            Point3d point4 = new Point3d(1.5, 3.5, 3.5);           // sono i 3 punti medi dei lati
            Point3d point5 = new Point3d(2.5, 3.5, 2);
            Point3d point6 = new Point3d(1, 5, 2.5);
            Point3d point7 = new Point3d(1.66666, 4.0, 2.666666); // baricentro

            //Assert;
            Assert.IsFalse(plane.IsPointOnPlane(point1));
            Assert.IsFalse(plane.IsPointOnPlane(point2));
            Assert.IsFalse(plane.IsPointOnPlane(point3));
            Assert.IsTrue(plane.IsPointOnPlane(point4));
            Assert.IsTrue(plane.IsPointOnPlane(point5));
            Assert.IsTrue(plane.IsPointOnPlane(point6));
            Assert.IsTrue(plane.IsPointOnPlane(point7));
        }

        [TestMethod]
        public void IsPointOnPlane2()
        {
            //Arrange
            Point3d p1 = new Point3d(-37.36, -12.08, 0.00);
            Point3d p2 = new Point3d(-21.21, -21.99, 0.00);
            Point3d p3 = new Point3d(-10.85, 9.25, 0.00);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(-30.66, 12.75, 0.00);
            Point3d point2 = new Point3d(-34.92, 2.24, 0.00);
            Point3d point3 = new Point3d(-35.95, -3.82, 0.00);
            Point3d point4 = new Point3d(-37.36, -12.08, 0.00);

            //Assert;
            Assert.IsTrue(plane.IsPointOnPlane(point1));
            Assert.IsTrue(plane.IsPointOnPlane(point2));
            Assert.IsTrue(plane.IsPointOnPlane(point3));
            Assert.IsTrue(plane.IsPointOnPlane(point4));
        }

        [TestMethod]
        public void IsPointOnPlane3()
        {
            //Arrange
            Point3d p1 = new Point3d(2.5, 15, 1);
            Point3d p2 = new Point3d(1.1, 12.3, 5);
            Point3d p3 = new Point3d(6, 2, 3.2);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(1.521, 9.767, 3.067);
            Point3d point2 = new Point3d(1.800, 13.650, 3.000);
            Point3d point3 = new Point3d(3.550, 7.150, 4.100);
            Point3d point4 = new Point3d(4.250, 8.500, 2.100);
            Point3d point5 = new Point3d(2.675, 10.400, 3.550);
            Point3d point6 = new Point3d(3.025, 11.075, 2.550);
            Point3d point7 = new Point3d(3.900, 7.825, 3.100);

            //Assert;
            Assert.IsFalse(plane.IsPointOnPlane(point1));
            Assert.IsTrue(plane.IsPointOnPlane(point2));
            Assert.IsTrue(plane.IsPointOnPlane(point3));
            Assert.IsTrue(plane.IsPointOnPlane(point4));
            Assert.IsTrue(plane.IsPointOnPlane(point5));
            Assert.IsTrue(plane.IsPointOnPlane(point6));
            Assert.IsTrue(plane.IsPointOnPlane(point7));
        }

        [TestMethod]
        public void IsPointOnPlane4()
        {
            //Arrange
            Point3d p1 = new Point3d(2.50000, 15.00000, 1.00000);
            Point3d p2 = new Point3d(3.4673914, 2.0000000, 3.2000000);
            Point3d p3 = new Point3d(-22.6748383, 43.5208770, 0.0000000);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(2.983696, 8.5, 2.1);
            Point3d point2 = new Point3d(-9.60372, 22.76044, 1.599999);
            Point3d point3 = new Point3d(-10.08742, 29.26044, 0.5);
            Point3d point4 = new Point3d(-3.31001, 15.63022, 1.849999);
            Point3d point5 = new Point3d(-3.551854, 18.880007, 1.300037);
            Point3d point6 = new Point3d(-9.845558, 26.010014, 1.050075);
            Point3d point7 = new Point3d(3.738855, 7.648723, 2.129224);
            Point3d point8 = new Point3d(5.489472, 1.535435, 2.948701);

            //Assert;
            Assert.IsTrue(plane.IsPointOnPlane(point1));
            Assert.IsTrue(plane.IsPointOnPlane(point2));
            Assert.IsTrue(plane.IsPointOnPlane(point3));
            Assert.IsTrue(plane.IsPointOnPlane(point4));
            Assert.IsTrue(plane.IsPointOnPlane(point5));
            Assert.IsTrue(plane.IsPointOnPlane(point6));
            Assert.IsTrue(plane.IsPointOnPlane(point7));
            Assert.IsTrue(plane.IsPointOnPlane(point8));
        }

        [TestMethod]
        public void IsPointOnPlane5()
        {
            //Arrange
            Point3d p1 = new Point3d(-15.7736144, 30.1516590, 0.0000000);
            Point3d p2 = new Point3d(19.9241001, 1.7448179, -6.8940675);
            Point3d p3 = new Point3d(-0.3511055, -15.9228705, 14.8165318);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(2.07524291694133, 15.9482384145719, -3.44703377232041);  // Sono tutti punti interni al piano.
            Point3d point2 = new Point3d(9.78649728147272, -7.08902631593605, 3.96123216972917);  // Sono punti di Rhino ottenuti con il massimo di cifre decimali
            Point3d point3 = new Point3d(-8.06235999841402, 7.11439430136416, 7.40826590259124);
            Point3d point4 = new Point3d(5.93087008945335, 4.4296060909284, 0.257099182768329);
            Point3d point5 = new Point3d(0.862068698716992, 0.0126839988964249, 5.68474899790374);
            Point3d point6 = new Point3d(-2.99355853035697, 11.5313163738888, 1.98061605040124);
            Point3d point7 = new Point3d(1.46865580782916, 7.98046120673416, 1.11885761279167);
            Point3d point8 = new Point3d(3.39646940383885, 2.22114500330193, 2.97092410627209);
            Point3d point9 = new Point3d(-1.06574493434728, 5.77200017045655, 3.83268254388166);
            Point3d point10 = new Point3d(1.28461609336863, 5.32105199429609, 2.63154288916532);

            //Assert;
            Assert.IsTrue(plane.IsPointOnPlane(point1));
            Assert.IsTrue(plane.IsPointOnPlane(point2));
            Assert.IsTrue(plane.IsPointOnPlane(point3));
            Assert.IsTrue(plane.IsPointOnPlane(point4));
            Assert.IsTrue(plane.IsPointOnPlane(point5));
            Assert.IsTrue(plane.IsPointOnPlane(point6));
            Assert.IsTrue(plane.IsPointOnPlane(point7));
            Assert.IsTrue(plane.IsPointOnPlane(point8));
            Assert.IsTrue(plane.IsPointOnPlane(point9));
            Assert.IsTrue(plane.IsPointOnPlane(point10));
        }

        [TestMethod]
        public void IsPointOnPlane6()
        {
            //Arrange
            Point3d p1 = new Point3d(-15.7736144, 30.1516590, 0.0000000);
            Point3d p2 = new Point3d(19.9241001, 1.7448179, -6.8940675);
            Point3d p3 = new Point3d(-0.3511055, -15.9228705, 14.8165318);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(2.07, 15.94, -3.44);         //
            Point3d point2 = new Point3d(9.78, -7.08, 3.96);          // Sono gli stessi punti di prima ma con meno cifre decimali.      
            Point3d point3 = new Point3d(-8.06, 7.11, 7.40);          // 
            Point3d point4 = new Point3d(5.93, 4.42, 0.25);          // Dovrebbero tornare tutti falsi perchè approssima la distanza
            Point3d point5 = new Point3d(0.86, 0.01, 5.68);          // Ci sta che qualcuno torni vero, ma è un caso
            Point3d point6 = new Point3d(-2.99, 11.53, 1.98);          // 
            Point3d point7 = new Point3d(1.46, 7.98, 1.11);
            Point3d point8 = new Point3d(3.39, 2.22, 2.97);
            Point3d point9 = new Point3d(-1.09, 5.77, 3.83);
            Point3d point10 = new Point3d(1.28, 5.32, 2.63);

            //Assert;
            Assert.IsFalse(plane.IsPointOnPlane(point1));
            Assert.IsFalse(plane.IsPointOnPlane(point2));
            Assert.IsFalse(plane.IsPointOnPlane(point3));
            Assert.IsFalse(plane.IsPointOnPlane(point4));
            Assert.IsFalse(plane.IsPointOnPlane(point5));
            Assert.IsFalse(plane.IsPointOnPlane(point6));
            Assert.IsFalse(plane.IsPointOnPlane(point7));
            Assert.IsFalse(plane.IsPointOnPlane(point8));
            Assert.IsFalse(plane.IsPointOnPlane(point9));
            Assert.IsFalse(plane.IsPointOnPlane(point10));
        }

        [TestMethod]
        public void Normal1()
        {
            Vector3d expectedNormal = new Vector3d(-0.476228, -0.409622, -0.778085);

            Point3d p1 = new Point3d(-15.7736144, 30.1516590, 0.0000000);
            Point3d p2 = new Point3d(19.9241001, 1.7448179, -6.8940675);
            Point3d p3 = new Point3d(-0.3511055, -15.9228705, 14.8165318);

            Plane p = new Plane(p1, p2, p3);

            Vector3d difference = p.Normal - expectedNormal;

            Assert.IsTrue(difference.Length < 0.001);
        }


        [TestMethod]
        public void Normal2()
        {
            Vector3d expectedNormal = new Vector3d(0, 0, -1);

            Point3d p1 = new Point3d(-9.59, -25.55, 0);
            Point3d p2 = new Point3d(-7.45, -12.27, 0);
            Point3d p3 = new Point3d(3.18, -7.18, 0);

            Plane p = new Plane(p1, p2, p3);

            Vector3d difference = p.Normal - expectedNormal;

            Assert.IsTrue(difference.Length < 0.001);
        }

        [TestMethod]
        public void IsPointOnPlaneScaled1()
        {
            //Arrange
            Point3d p1 = new Point3d(-2.32E-6, -10.98E-6, 0.00);
            Point3d p2 = new Point3d(5.74E-6, -10.98E-6, 0.00);
            Point3d p3 = new Point3d(5.74E-6, -17.49E-6, 0.00);
            Point3d p4 = new Point3d(-2.32E-6, -17.49E-6, 0.00);

            double tolerance = 1E-11;
            Plane plane = new Plane(p1, p2, p3, tolerance);
            Assert.IsTrue(plane.IsPointOnPlane(p4, tolerance));
        }

        [TestMethod]
        public void OriginNormalConstructor()
        {
            Point3d o = new Point3d(7.795016, -6.934559, 6.299883);
            Vector3d n = new Vector3d(-0.639583, 0.568982, -0.516907);

            Plane plane = new Plane(o, n);

            Point3d p1 = new Point3d(-9.59, -25.55, 7.32);
            Point3d p2 = new Point3d(-7.45, -12.27, 19.29);
            Point3d p3 = new Point3d(3.18, -7.18, 11.74);

            Assert.IsTrue(plane.IsPointOnPlane(p1));
            Assert.IsTrue(plane.IsPointOnPlane(p2));
            Assert.IsTrue(plane.IsPointOnPlane(p3));
        }

        [TestMethod]
        public void OriginNormalConstructor2()
        {
            Point3d o = new Point3d(1306.25, 75, 4700);
            Vector3d n = new Vector3d(3.68714068286322E-17, 1, 0);

            Plane plane = new Plane(o, n);
            
            Point3d p1 = new Point3d(1306.25, 75, 4700);
            //Point3d p2 = new Point3d(1305.25, 75, 4700);
            //Point3d p3 = new Point3d(1306.25, 75, 4701);
            Point3d p2 = new Point3d(1306.25, 75, 4699);
            Point3d p3 = new Point3d(1305.25, 75, 4700);

            Assert.IsTrue(plane.P1 == p1);
            Assert.IsTrue(plane.P2 == p2);
            Assert.IsTrue(plane.P3 == p3);

            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(2462.5, 74.9999999999999, 3400)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(150, 75, 3400)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(150, 75, 6000)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(2462.5, 74.9999999999999, 6000)));
        }

        [TestMethod]
        public void OriginNormalConstructor3()
        {
            Point3d o = new Point3d(600, 600, 0);
            Vector3d n = new Vector3d(0, 0, 1);

            Plane plane = new Plane(o, n);

            Point3d p1 = new Point3d(600, 600, 0);
            Point3d p2 = new Point3d(600, 601, 0);
            Point3d p3 = new Point3d(599, 600, 0);

            Assert.IsTrue(plane.P1 == p1);
            Assert.IsTrue(plane.P2 == p2);
            Assert.IsTrue(plane.P3 == p3);

            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(1100, 100, 0)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(1100, 1100, 0)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(100, 1100, 0)));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(100, 100, 0)));
        }

        [TestMethod]
        public void OriginNormalConstructor4()
        {
            Point3d o = new Point3d(-475.340871, 5464.705935, 1056.588512);
            Vector3d n = new Vector3d(-0.138971, -0.413436, 0.899866);

            Plane plane = new Plane(o, n);

            Point3d p1 = new Point3d(-475.340871, 5464.705935, 1056.588512);
            Point3d p2 = new Point3d(-475.340871, 5465.614618, 1057.005999);
            Point3d p3 = new Point3d(-476.331167, 5464.763954, 1056.462232);
            Assert.IsTrue(plane.P1.Equals(p1, 0.01));
            Assert.IsTrue(plane.P2.Equals(p2, 0.01));
            Assert.IsTrue(plane.P3.Equals(p3, 0.01));

            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(-2388.148432, 3807.946392, 0), 0.001));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(2422.281456, 3537.42282, 618.608677), 0.001));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(2412.567399, 7018.318404, 2216.37659), 0.001));
            Assert.IsTrue(plane.IsPointOnPlane(new Point3d(-3388.900816, 7344.574881, 1470.322981), 0.001));
        }

        [TestMethod]
        public void EquationConstructor1()
        {
            Point3d o = new Point3d(0.0, 0.0, 0.0);
            Vector3d n = new Vector3d(0.0, 0.0, 1.0);

            Plane plane = new Plane(o, n);

            double A = plane.A;
            double B = plane.B;
            double C = plane.C;
            double D = plane.D;

            Plane plane1 = new Plane(A, B, C, D);

            Assert.IsTrue(Math.Abs(plane.Normal.X - plane1.Normal.X) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Y - plane1.Normal.Y) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Z - plane1.Normal.Z) < GeometryBase.Tolerance);
        }

        [TestMethod]
        public void EquationConstructor2()
        {
            Point3d o = new Point3d(0.0, 0.0, 0.0);
            Vector3d n = new Vector3d(1.0, 0.0, 0.0);

            Plane plane = new Plane(o, n);

            double A = plane.A;
            double B = plane.B;
            double C = plane.C;
            double D = plane.D;

            Plane plane1 = new Plane(A, B, C, D);

            Assert.IsTrue(Math.Abs(plane.Normal.X - plane1.Normal.X) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Y - plane1.Normal.Y) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Z - plane1.Normal.Z) < GeometryBase.Tolerance);
        }

        [TestMethod]
        public void EquationConstructor3()
        {
            Point3d p1 = new Point3d(10.0, 10.0, 10.0);
            Point3d p2 = new Point3d(5.0, 2.0, 3.0);
            Point3d p3 = new Point3d(15.0, 7.0, 0.0);

            Plane plane = new Plane(p1, p2, p3);

            double A = plane.A;
            double B = plane.B;
            double C = plane.C;
            double D = plane.D;

            Plane plane1 = new Plane(A, B, C, D);

            Assert.IsTrue(Math.Abs(plane.Normal.X - plane1.Normal.X) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Y - plane1.Normal.Y) < GeometryBase.Tolerance);
            Assert.IsTrue(Math.Abs(plane.Normal.Z - plane1.Normal.Z) < GeometryBase.Tolerance);
        }

        [TestMethod]
        public void ProjectPoint()
        {
            Point3d o = new Point3d(7.795016, -6.934559, 6.299883);
            Vector3d n = new Vector3d(-0.639583, 0.568982, -0.516907);

            Plane plane = new Plane(o, n);

            Point3d expectedPoint = new Point3d(7.795016, -6.934559, 6.299883);
            Point3d test = plane.Project(new Point3d(0, 0, 0));

            Assert.IsTrue(test == expectedPoint);
        }

        [TestMethod]
        public void ProjectPoint1()
        {
            //Arrange
            Point3d p1 = new Point3d(-15.7736144, 30.1516590, 0.0000000);
            Point3d p2 = new Point3d(19.9241001, 1.7448179, -6.8940675);
            Point3d p3 = new Point3d(-0.3511055, -15.9228705, 14.8165318);

            Plane plane = new Plane(p1, p2, p3);

            //Act
            Point3d point1 = new Point3d(2.07, 15.94, -3.44);     
            Point3d point2 = new Point3d(9.78, -7.08, 3.96);          
            Point3d point3 = new Point3d(-8.06, 7.11, 7.40);      
            Point3d point4 = new Point3d(5.93, 4.42, 0.25);       
            Point3d point5 = new Point3d(0.86, 0.01, 5.68);       
            Point3d point6 = new Point3d(-2.99, 11.53, 1.98);     
            Point3d point7 = new Point3d(1.46, 7.98, 1.11);
            Point3d point8 = new Point3d(3.39, 2.22, 2.97);
            Point3d point9 = new Point3d(-1.09, 5.77, 3.83);
            Point3d point10 = new Point3d(1.28, 5.32, 2.63);

            //Assert;
            Assert.IsTrue(plane.Project(point1) == new Point3d(2.07018981818509, 15.940163270063, -3.43968986543239));
            Assert.IsTrue(plane.Project(point2) == new Point3d(9.78016931787849, -7.07985436305449, 3.96027664012807));
            Assert.IsTrue(plane.Project(point3) == new Point3d(-8.05661512310161, 7.11291146532698, 7.40553038336579));
            Assert.IsTrue(plane.Project(point4) == new Point3d(5.93470179719068, 4.42404420010123, 0.257682034458907));
            Assert.IsTrue(plane.Project(point5) == new Point3d(0.86275246508144, 0.0123675031290265, 5.68449711689914));
            Assert.IsTrue(plane.Project(point6) == new Point3d(-2.9903219850798, 11.5297230480092, 1.97947392446377));
            Assert.IsTrue(plane.Project(point7) == new Point3d(1.46533519757024, 7.98458901260064, 1.11871691608923));
            Assert.IsTrue(plane.Project(point8) == new Point3d(3.39203299902089, 2.22174866216313, 2.97332161679885));
            Assert.IsTrue(plane.Project(point9) == new Point3d(-1.08311493535646, 5.7759221140341, 3.84124916743504));
            Assert.IsTrue(plane.Project(point10)== new Point3d(1.28182382505561, 5.32156874343477, 2.63297985777693));
        }

        [TestMethod]
        public void AngleOnPlane()
        {
            Point3d p1 = new Point3d(-9.59, -25.55, 7.32);
            Point3d p2 = new Point3d(-7.45, -12.27, 19.29);
            Point3d p3 = new Point3d(3.18, -7.18, 11.74);

            Plane p = new Plane(p1, p2, p3);

            Point3d point = new Point3d(7.795016, -6.934559, 6.299883);

            double angle = p.AngleOnPlane(point);

            Assert.IsTrue(Math.Abs(angle - 0.935967) < 0.001);
        }

        [TestMethod]
        public void IntersectionPlaneLine1()
        {
            Point3d p1 = new Point3d(0.0, 0.0, 5.0);
            Vector3d normal = new Vector3d(0.0, 0.0, 1.0);

            Plane p = new Plane(p1, normal);

            Line3d line = new Line3d(new Point3d(0.0, 0.0, 10.0), new Point3d(-10.0, -10.0, 0.0));
            Point3d expPoint = new Point3d(-5.0, -5.0, 5.0);

            bool inters = p.IntersectWithRay(line, out Point3d intersectionPoint);

            Assert.IsTrue(inters);
            Assert.IsTrue(Math.Abs(intersectionPoint.DistanceTo(expPoint)) < 0.001);

            // Intersection with parametric result.
            bool _ = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double _, out double _, out double s, out Point3d intersParametric);
            Assert.IsTrue(intersParametric != null);
            Assert.IsTrue(intersParametric.DistanceTo(expPoint) < 0.001);
            Assert.AreEqual(0.5, s, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine2()
        {
            Point3d p1 = new Point3d(5.0, 0.0, 0.0);
            Vector3d normal = new Vector3d(1.0, 0.0, 0.0);

            Plane p = new Plane(p1, normal);

            Line3d line = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 0.0, 10.0));
            Point3d expPoint = new Point3d(5.0, 0.0, 5.0);

            bool inters = p.IntersectWithRay(line, out Point3d intersectionPoint);

            Assert.IsTrue(inters);
            Assert.IsTrue(Math.Abs(intersectionPoint.DistanceTo(expPoint)) < 0.001);

            // Intersection with parametric result.
            bool _ = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double _, out double _, out double s, out Point3d intersParametric);
            Assert.IsTrue(intersParametric != null);
            Assert.IsTrue(intersParametric.DistanceTo(expPoint) < 0.001);
            Assert.AreEqual(0.5, s, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine3()
        {
            Point3d p1 = new Point3d(10.0, 5.0, 3.0);
            Point3d p2 = new Point3d(4.0, 8.0, 6.0);
            Point3d p3 = new Point3d(8.0, 12.0, 7.0);

            Plane p = new Plane(p1, p2, p3);

            Line3d line1 = new Line3d(new Point3d(5.0, 5.0, 0.0), new Point3d(10.0, 15.0, 12.0));
            Line3d line2 = new Line3d(new Point3d(10.0, 10.0, 0.0), new Point3d(5.0, 5.0, 10.0));
            Point3d expPoint1 = new Point3d(7.576, 10.152, 6.182);
            Point3d expPoint2 = new Point3d(7.556, 7.556, 4.889);

            bool inters1 = p.IntersectWithRay(line1, out Point3d intersectionPoint1);
            bool inters2 = p.IntersectWithRay(line2, out Point3d intersectionPoint2);

            Assert.IsTrue(inters1);
            Assert.IsTrue(Math.Abs(intersectionPoint1.DistanceTo(expPoint1)) < 0.001);

            Assert.IsTrue(inters2);
            Assert.IsTrue(Math.Abs(intersectionPoint2.DistanceTo(expPoint2)) < 0.001);

            // Intersection with parametric result.
            bool intersFounded1 = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line1.Start, line1.End, out double u1, out double v1, out double s1, out Point3d intersParametric1);
            Assert.IsTrue(intersParametric1 != null);
            Assert.IsTrue(intersParametric1.DistanceTo(expPoint1) < 0.001);
            Assert.IsTrue(intersFounded1);
            Assert.AreEqual(0.18518518518518517, u1, 1.0e-12);
            Assert.AreEqual(0.65656565656565657, v1, 1.0e-12);
            Assert.AreEqual(0.51515151515151514, s1, 1.0e-12);

            bool intersFounded2 = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line2.Start, line2.End, out double u2, out double v2, out double s2, out Point3d intersParametric2);
            Assert.IsTrue(intersParametric2 != null);
            Assert.IsTrue(intersParametric2.DistanceTo(expPoint2) < 0.001);
            Assert.IsTrue(intersFounded2);
            Assert.AreEqual(0.33333333333333331, u2, 1.0e-12);
            Assert.AreEqual(0.22222222222222221, v2, 1.0e-12);
            Assert.AreEqual(0.48888888888888887, s2, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine4()
        {
            Point3d p1 = new Point3d(1258, 185, 28);
            Point3d p2 = new Point3d(512, 58, 375);
            Point3d p3 = new Point3d(-4982, 58, 20);

            Plane p = new Plane(p1, p2, p3);

            Line3d line1 = new Line3d(new Point3d(-307.499, -3167.325, 0.000), new Point3d(-3745.578, 5011.793, 0.000));
            Line3d line2 = new Line3d(new Point3d(1550.170, 1206.285, 0.000), new Point3d(-4664.507, -1416.741, 0.000));
            Point3d expPoint1 = new Point3d(-1694.654, 132.687, 0.000);
            Point3d expPoint2 = new Point3d(-957.200, 148.000, 0.000);

            bool inters1 = p.IntersectWithRay(line1, out Point3d intersectionPoint1);
            bool inters2 = p.IntersectWithRay(line2, out Point3d intersectionPoint2);

            Assert.IsTrue(inters1);
            Assert.IsTrue(Math.Abs(intersectionPoint1.DistanceTo(expPoint1)) < 0.001);

            Assert.IsTrue(inters2);
            Assert.IsTrue(Math.Abs(intersectionPoint2.DistanceTo(expPoint2)) < 0.001);

            // Intersection with parametric result.
            bool intersFounded1 = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line1.Start, line1.End, out double u1, out double v1, out double s1, out Point3d intersParametric1);
            Assert.IsTrue(intersParametric1 != null);
            Assert.IsTrue(intersParametric1.DistanceTo(expPoint1) < 0.001);
            Assert.IsFalse(intersFounded1);
            Assert.AreEqual(-0.069590746537480389, u1, 1.0e-12);
            Assert.AreEqual(0.4815013689367878, v1, 1.0e-12);
            Assert.AreEqual(0.40346799629926938, s1, 1.0e-12);

            bool intersFounded2 = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line2.Start, line2.End, out double u2, out double v2, out double s2, out Point3d intersParametric2);
            Assert.IsTrue(intersParametric2 != null);
            Assert.IsTrue(intersParametric2.DistanceTo(expPoint2) < 0.001);
            Assert.IsFalse(intersFounded2);
            Assert.AreEqual(-0.07230790596869445, u2, 1.0e-12);
            Assert.AreEqual(0.36364457860787852, v2, 1.0e-12);
            Assert.AreEqual(0.40345949961044103, s2, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine5()
        {
            Point3d p1 = new Point3d(0.0, 0.0, 7.0);
            Point3d p2 = new Point3d(10.0, 0.0, 3.0);
            Point3d p3 = new Point3d(0.0, 8.0, 6.0);

            Plane p = new Plane(p1, p2, p3);

            Line3d line = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 10.0, 10.0));

            bool intersFounded = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double u, out double v, out double s, out Point3d _);
            Assert.IsFalse(intersFounded);
            Assert.AreEqual(0.45901639344262296, u, 1.0e-12);
            Assert.AreEqual(0.57377049180327866, v, 1.0e-12);
            Assert.AreEqual(0.45901639344262296, s, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine6()
        {
            Point3d p1 = new Point3d(0.0, 0.0, 7.0);
            Point3d p2 = new Point3d(10.0, 0.0, 3.0);
            Point3d p3 = new Point3d(0.0, 8.0, 6.0);

            Plane p = new Plane(p1, p2, p3);

            Line3d line = new Line3d(new Point3d(2.0, 2.0, 0.0), new Point3d(10.0, 10.0, 10.0));

            bool intersFounded = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double u, out double v, out double s, out Point3d _);
            Assert.IsFalse(intersFounded);
            Assert.AreEqual(0.53521126760563376, u, 1.0e-12);
            Assert.AreEqual(0.66901408450704225, v, 1.0e-12);
            Assert.AreEqual(0.41901408450704225, s, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine7()
        {
            Point3d p1 = new Point3d(0.0, 0.0, 7.0);
            Point3d p2 = new Point3d(10.0, 0.0, 1.5);
            Point3d p3 = new Point3d(0.0, 8.0, 6.0);

            Plane p = new Plane(p1, p2, p3);

            Line3d line = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 10.0, 10.0));

            bool intersFounded = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double u, out double v, out double s, out Point3d _);
            Assert.IsTrue(intersFounded);
            Assert.AreEqual(0.41791044776119401, u, 1.0e-12);
            Assert.AreEqual(0.52238805970149249, v, 1.0e-12);
            Assert.AreEqual(0.41791044776119401, s, 1.0e-12);
        }

        [TestMethod]
        public void IntersectionPlaneLine8()
        {
            Point3d p1 = new Point3d(10.0, 11.0, 4.35);
            Point3d p2 = new Point3d(25.0, 11.0, -3.0);
            Point3d p3 = new Point3d(10.0, 21.0, 6.0);

            Plane p = new Plane(p1, p2, p3);

            Line3d line = new Line3d(new Point3d(26.98103581, 9.67930946, 12.96501999), new Point3d(13.48103581, 18.67930946, -0.92395251));

            bool intersFounded = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double u, out double v, out double s, out Point3d _);
            Assert.IsFalse(intersFounded);
            Assert.AreEqual(0.42997725192057146, u, 1.0e-12);
            Assert.AreEqual(0.57002274807942865, v, 1.0e-12);
            Assert.AreEqual(0.78010200231047655, s, 1.0e-12);
        }

        /// <summary>
        /// Is intersection inside the triangle?
        /// </summary>
        [TestMethod]
        public void IntersectionPlaneLine9()
        {
            Point3d p1 = new Point3d(0.12119181, -0.02118724, 0.32087946);
            Point3d p2 = new Point3d(0.18514975, 0.0, 0.2941618);
            Point3d p3 = new Point3d(0.12119181, 0.02118724, 0.32087946);

            Plane p = new Plane(p1, p2, p3);

            Line3d line = new Line3d(new Point3d(0.0, 0.0, 0.0), new Point3d(0.17940651107496444, 0.0, 0.29639136994437787));

            bool intersFounded = Plane.GetIntersectionTriangleWihtRay(p.P1, p.P2, p.P3, line.Start, line.End, out double u, out double v, out double s, out Point3d _);
            // Intersection is inside the triangle.
            Assert.IsTrue(u >= 0.0);
            Assert.IsTrue(v >= 0.0);
            Assert.IsTrue(u + v <= 1.0);
            Assert.IsTrue(intersFounded);
        }
    }
}
