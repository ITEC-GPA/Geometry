using GPC.Geometry;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Geometry
{
    [TestClass]
    public class GeometryPerformance
    {

        [TestMethod]
        public void VectorUnitize()
        {
            // 20210517: 0.002203 ms
            //         : 0.000434 ms

            Action ac1 = new Action(() =>
            {
                Vector3d v = new Vector3d(1, 2, 3);
                v.Unitize();
            });

            MeasureTime.FunctionExecutionTime(100, ac1, true);

        }


        [TestMethod]
        public void VectorAngleTo()
        {
            // PC marco
            // 20210517: 0.004794 ms
            // ottimizzata: 0.000509 ms

            Vector3d v1 = new Vector3d(1, 2, 3);
            Vector3d v2 = new Vector3d(3, 2, 1);

            Action ac1 = new Action(() =>
            {
                v1.AngleTo(v2);
            });

            MeasureTime.FunctionExecutionTime(100, ac1, true);

        }


        [TestMethod]
        [TestCategory("Missing assert")]
        public void PointNotEquals1()
        {
            // PC marco
            // 20210525:
            //      Not equals:      0.001952   

            Point3d p1 = new Point3d(1, 2, 3);
            Point3d p2 = new Point3d(3, 2, 1);

            Action notEquals = new Action(() =>
                {
                    p1.Equals(p2);
                }
            );

            MeasureTime.FunctionExecutionTime(100, notEquals, true, "Not equals");

        }


        [TestMethod]
        [TestCategory("Missing assert")]
        public void PointEquals1()
        {
            // PC marco
            // 20210525:
            //      Equals:          0.001861  

            Point3d p41 = new Point3d(1, 2, 3);
            Point3d p4 = new Point3d(1, 2, 3);

            Action equals = new Action(() =>
            {
                p41.Equals(p4);
            }
            );

            MeasureTime.FunctionExecutionTime(100, equals, true, "Equals");

        }


        [TestMethod]
        [TestCategory("Missing assert")]
        public void PointAlmostEquals1()
        {
            // PC marco
            // 20210525:
            //      Almost equals:   0.001964 

            Point3d p31 = new Point3d(1, 2, 3);
            Point3d p3 = new Point3d(1.00000001, 2.00000001, 3.00000001);

            Action almostEquals = new Action(() =>
            {
                p31.Equals(p3);
            }
            );

            MeasureTime.FunctionExecutionTime(100, almostEquals, true, "Almost equals");

        }


        [TestMethod]
        public void Polygon3dGetCenter()
        {

            Polygon2d poly2 = new Polygon2d(100, 100);


            Polygon3d poly3 = new Polygon3d(poly2);
            Console.WriteLine($"Number of points: { poly3.Count}");


            Action ac1 = new Action(() =>
            {
                poly3.GetCenter();
            });


            MeasureTime.FunctionExecutionTime(10000, ac1, true);

            Assert.AreEqual(new Point3d(), poly3.GetCenter());
            Assert.AreEqual(new Point2d(), poly2.GetCenter());
        }


        [TestMethod]
        public void Polygon3dGetCentroid()
        {

            Polygon2d poly2 = new Polygon2d(100, 100);

            Polygon3d poly3 = new Polygon3d(poly2);
            Console.WriteLine($"Number of points: { poly3.Count}");


            Action ac1 = new Action(() =>
            {
                poly3.GetCentroid();
            });


            MeasureTime.FunctionExecutionTime(100, ac1, true);

            Assert.AreEqual(new Point3d(), poly3.GetCentroid());
            Assert.AreEqual(new Point2d(), poly2.GetCentroid());
        }
    }
}
