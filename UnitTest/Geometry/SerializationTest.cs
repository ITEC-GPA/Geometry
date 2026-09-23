using GPC.Geometry;
using Maffeis.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    [TestClass]
    public class SerializationTest :  UnitTestBase
    {
        private bool SerializationClassesCommonAsserts(object objToTest)
        {
            bool check = true;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, objToTest);
                ms.Position = 0;

                var oggettoDeserializzato = formatter.Deserialize(ms);

                if (objToTest == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {objToTest.ToString().Replace("GPC.Geometry", "")} is serializable");
                }
                else
                {
                    Console.WriteLine($"Warning: Class {objToTest.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }
            return check;
        }

        [TestMethod]
        public void SerializableGeneric()
        {
            string assemblyName = "GPCGeometry";
            string nameSpace = "GPC.Geometry";

            var assembly = Assembly.Load(assemblyName);
            var classes = assembly.GetTypes().Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(nameSpace)).ToList();

            foreach (var c in classes)
            {
                Assert.IsTrue(SerializationClassesCommonAsserts(c));
            }
        }

        [TestMethod]
        public void SerializableClassPoint2dTest()
        {
            bool check = true;
            Point2d p = new Point2d();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, p);
                ms.Position = 0;

                Point2d oggettoDeserializzato = (Point2d)formatter.Deserialize(ms);

                if (p == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {p.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (p.X != oggettoDeserializzato.X || p.Y != oggettoDeserializzato.Y)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {p.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassPoint3dTest()
        {
            bool check = true;
            Point3d p = new Point3d();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, p);
                ms.Position = 0;

				Point3d oggettoDeserializzato = (Point3d)formatter.Deserialize(ms);

                if (p == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {p.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if(p.X != oggettoDeserializzato.X || p.Y != oggettoDeserializzato.Y || p.Z != oggettoDeserializzato.Z)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {p.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }      

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassLine2dTest()
        {
            bool check = true;
            Line2d l = new Line2d(new Point2d(), new Point2d());

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                Line2d oggettoDeserializzato = (Line2d)formatter.Deserialize(ms);

                if (l == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {l.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (l.Start.X != oggettoDeserializzato.Start.X || l.Start.Y != oggettoDeserializzato.Start.Y ||
                        l.End.X != oggettoDeserializzato.End.X || l.End.Y != oggettoDeserializzato.End.Y)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {l.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassLine3dTest()
        {
            bool check = true;
            Line3d l = new Line3d(new Point3d(), new Point3d());

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                Line3d oggettoDeserializzato = (Line3d)formatter.Deserialize(ms);

                if (l == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {l.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (l.Start.X != oggettoDeserializzato.Start.X || l.Start.Y != oggettoDeserializzato.Start.Y || l.Start.Z != oggettoDeserializzato.Start.Z
                        || l.End.X != oggettoDeserializzato.End.X || l.End.Y != oggettoDeserializzato.End.Y || l.End.Z != oggettoDeserializzato.End.Z)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {l.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassVector2dTest()
        {
            bool check = true;
            Vector2d l = Vector2d.XAxis;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                Vector2d oggettoDeserializzato = (Vector2d)formatter.Deserialize(ms);

                if (l == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {l.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (l.X != oggettoDeserializzato.X || l.Y != oggettoDeserializzato.Y )
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {l.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassVector3dTest()
        {
            bool check = true;
            Vector3d l = Vector3d.XAxis;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                Vector3d oggettoDeserializzato = (Vector3d)formatter.Deserialize(ms);

                if (l == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {l.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (l.X != oggettoDeserializzato.X || l.Y != oggettoDeserializzato.Y || l.Y != oggettoDeserializzato.Y)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {l.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassBoundingBox1dTest()
        {
            bool check = true;
            BoundingBox1d b = new BoundingBox1d();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, b);
                ms.Position = 0;

                BoundingBox1d oggettoDeserializzato = (BoundingBox1d)formatter.Deserialize(ms);

                if (b.Equals(oggettoDeserializzato))
                {
                    if (b.Max != oggettoDeserializzato.Max || b.Min != oggettoDeserializzato.Min || b.IsEmpty != oggettoDeserializzato.IsEmpty)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {b.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {b.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassBoundingBox2dTest()
        {
            bool check = true;
            BoundingBox2d b = new BoundingBox2d();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, b);
                ms.Position = 0;

                BoundingBox2d oggettoDeserializzato = (BoundingBox2d)formatter.Deserialize(ms);

                if (b.Equals(oggettoDeserializzato))
                {
                    Console.WriteLine($"Class {b.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (b.Max != oggettoDeserializzato.Max || b.Min != oggettoDeserializzato.Min || b.Size != oggettoDeserializzato.Size)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {b.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassBoundingBox3dTest()
        {
            bool check = true;
            BoundingBox3d b = new BoundingBox3d();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, b);
                ms.Position = 0;

                BoundingBox3d oggettoDeserializzato = (BoundingBox3d)formatter.Deserialize(ms);

                if (b.Equals(oggettoDeserializzato))
                {
                    Console.WriteLine($"Class {b.ToString().Replace("GPC.Geometry", "")} is serializable");

                    if (b.Max != oggettoDeserializzato.Max || b.Min != oggettoDeserializzato.Min || b.Size != oggettoDeserializzato.Size)
                        check = false;
                }
                else
                {
                    Console.WriteLine($"Warning: Class {b.ToString().Replace("GPC.Geometry", "")} is not serializable");
                    check = false;
                }
            }

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassCoordinateSystemTest()
        {
            bool check = true;
            CoordinateSystem cs = new CoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, "cs");

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, cs);
                ms.Position = 0;

                CoordinateSystem oggettoDeserializzato = (CoordinateSystem)formatter.Deserialize(ms);

                if (cs == oggettoDeserializzato)
                {
                    if (cs.V1 != oggettoDeserializzato.V1 || cs.V2 != oggettoDeserializzato.V2 || cs.V3 != oggettoDeserializzato.V3 ||
                         cs.Name != oggettoDeserializzato.Name || cs.Origin != oggettoDeserializzato.Origin)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {cs.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {cs.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassPlaneTest()
        {
            bool check = true;
            Plane plane = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, plane);
                ms.Position = 0;

                Plane oggettoDeserializzato = (Plane)formatter.Deserialize(ms);

                if (plane == oggettoDeserializzato)
                {
                    if (plane.P1 != oggettoDeserializzato.P1 || plane.P2 != oggettoDeserializzato.P2 || plane.P3 != oggettoDeserializzato.P3 ||
                         plane.Normal != oggettoDeserializzato.Normal || plane.Origin != oggettoDeserializzato.Origin || 
                         plane.A != oggettoDeserializzato.A || plane.B != oggettoDeserializzato.B ||
                         plane.C != oggettoDeserializzato.C || plane.D != oggettoDeserializzato.D)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if(check)
                Console.WriteLine($"Class {plane.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {plane.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassPolygon2dTest()
        {
            bool check = true;
            Polygon2d poly = new Polygon2d(new Point2d[] {
                new Point2d(10, 10),
                new Point2d(20, 10),
                new Point2d(20, 20),
                new Point2d(10, 20)});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, poly);
                ms.Position = 0;

                Polygon2d oggettoDeserializzato = (Polygon2d)formatter.Deserialize(ms);

                if (poly == oggettoDeserializzato)
                {
                    for (int i = 0; i < poly.Count; i++)
                        if (poly[i] != oggettoDeserializzato[i])
                            check = false;
                    if (poly.Count != oggettoDeserializzato.Count)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {poly.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {poly.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassPolygon3dTest()
        {
            bool check = true;
            Polygon3d poly = new Polygon3d(new Point3d[] {
                new Point3d(10, 10, 2),
                new Point3d(20, 10, 2),
                new Point3d(20, 20, 2),
                new Point3d(10, 20, 2)});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, poly);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                Polygon3d oggettoDeserializzato = (Polygon3d)casted;

                if (poly == oggettoDeserializzato)
                {
                    for (int i = 0; i < poly.Count; i++)
                        if (poly[i] != oggettoDeserializzato[i])
                            check = false;
                    if (poly.Count != oggettoDeserializzato.Count)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {poly.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {poly.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassShapeTest()
        {
            bool check = true;

            Polygon3d poly = new Polygon3d(new Point3d[] {
                new Point3d(0, 0, 0),
                new Point3d(20, 0, 0),
                new Point3d(20, 20, 0),
                new Point3d(0, 20, 0)});

            Polygon3d hole = new Polygon3d(new Point3d[] {
                new Point3d(5, 5, 0),
                new Point3d(15, 5, 0),
                new Point3d(15, 15, 0),
                new Point3d(5, 15, 0)});

            Shape s = new Shape(poly, new Polygon3d[] { hole });
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                Shape oggettoDeserializzato = (Shape)casted;

                if (s == oggettoDeserializzato)
                {
                    for (int i = 0; i < s.Fill.Count; i++)
                        if (s.Fill[i] != oggettoDeserializzato.Fill[i])
                            check = false;
                    if (s.HasHoles != oggettoDeserializzato.HasHoles)
                        check = false;
                    if (s.HasChilds != oggettoDeserializzato.HasChilds)
                        check = false;
                    if (s.Fill != oggettoDeserializzato.Fill)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassShape2dTest()
        {
            bool check = true;

            Polygon2d poly = new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(20, 0),
                new Point2d(20, 20),
                new Point2d(0, 20)});

            Polygon2d hole = new Polygon2d(new Point2d[] {
                new Point2d(5, 5),
                new Point2d(15, 5),
                new Point2d(15, 15),
                new Point2d(5, 15)});

            Shape2d s = new Shape2d(poly, new Polygon2d[] { hole });
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                Shape2d oggettoDeserializzato = (Shape2d)casted;

                if (s == oggettoDeserializzato)
                {
                    for (int i = 0; i < s.Fill.Count; i++)
                        if (s.Fill[i] != oggettoDeserializzato.Fill[i])
                            check = false;
                    if (s.HasHoles != oggettoDeserializzato.HasHoles)
                        check = false;
                    if (s.HasChilds != oggettoDeserializzato.HasChilds)
                        check = false;
                    if (s.Fill != oggettoDeserializzato.Fill)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }
    }
}
