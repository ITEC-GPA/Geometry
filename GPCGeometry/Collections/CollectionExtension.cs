using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Geometry.Collections
{
    public static class CollectionExtension
    {
        /// <summary>
        /// Sort the Point2d collection in clockwide order
        /// </summary>
        /// <param name="points">The points to sort</param>
        /// <returns>The sorted collection</returns>
        public static ICollection<Point2d> SortClockwise(this ICollection<Point2d> points)
        {
            double cx = points.Average(p => p.X);
            double cy = points.Average(p => p.Y);
            return points.OrderBy(p => Math.Atan2(p.X - cx, p.Y - cy)).ToList();
        }

        /// <summary>
        /// Calculate the center of the given Point2d collection
        /// </summary>
        /// <param name="points">The points collection</param>
        /// <returns>The center point</returns>
        public static Point2d Center(this ICollection<Point2d> points)
        {
            double cx = points.Average(p => p.X);
            double cy = points.Average(p => p.Y);
            return new Point2d(cx, cy);
        }

        /// <summary>
        /// Triangulate the given Point3d collection
        /// </summary>
        /// <param name="points">The points to triangulate</param>
        /// <returns>A Mesh with the triangles</returns>
        /// <remarks>The input collection is not modified. A last point equal to the first one (closed polyline) is ignored</remarks>
        public static Mesh Triangulate(this ICollection<Point3d> points)
        {
            // work on a copy: the points are removed while the triangles are created
            List<Point3d> buffer = points.ToList();
            if (buffer.Count > 3 && buffer[0].Equals(buffer[buffer.Count - 1]))
                buffer.RemoveAt(buffer.Count - 1);

            points = buffer;

            if (points.Count < 3)
                return null;
            Mesh mesh = new Mesh();
            while (points.Count > 3)
            {
                int min_i = -1;
                double min_a = double.MaxValue;
                for (int i = 1; i < points.Count - 1; i++)
                {
                    Point3d p1 = points.ElementAt(i - 1);
                    Point3d p0 = points.ElementAt(i);
                    Point3d p2 = points.ElementAt(i + 1);
                    Vector3d v1 = p0.VectorTo(p1);
                    Vector3d v2 = p0.VectorTo(p2);
                    double a = v1.AngleTo(v2);
                    if (a < min_a)
                    {
                        min_a = a;
                        min_i = i;
                    }
                }
                if (min_i > 0)
                {
                    mesh.AddFaceMesh(new[]
                        {
                            points.ElementAt(min_i - 1),
                            points.ElementAt(min_i),
                            points.ElementAt(min_i + 1)
                        });
                    buffer.RemoveAt(min_i);
                }
                else
                {
                    break;
                }
            }

            // the last three points are the last triangle
            if (points.Count == 3)
                mesh.AddFaceMesh(new[] { buffer[0], buffer[1], buffer[2] });

            return mesh;
        }

        /// <summary>
        /// Calculate the mass center of the genven Point3d collection
        /// </summary>
        /// <param name="points">The points</param>
        /// <returns>The mass center</returns>
        public static Point3d MassCenter(this ICollection<Point3d> points)
        {
            Mesh mesh = points.Triangulate();
            double totalArea = 0;
            double x = 0, y = 0, z = 0;
            foreach (MeshFace tri in mesh.Faces)
            {
                double area = mesh.FaceArea(tri);
                totalArea += area;
                x += (mesh.Vertices[tri.A].Point.X + mesh.Vertices[tri.B].Point.X + mesh.Vertices[tri.C].Point.X) / 3 * area;
                y += (mesh.Vertices[tri.A].Point.Y + mesh.Vertices[tri.B].Point.Y + mesh.Vertices[tri.C].Point.Y) / 3 * area;
                z += (mesh.Vertices[tri.A].Point.Z + mesh.Vertices[tri.B].Point.Z + mesh.Vertices[tri.C].Point.Z) / 3 * area;
            }
            x /= totalArea;
            y /= totalArea;
            z /= totalArea;

            return new Point3d(x, y, z);
        }

        /// <summary>
        /// Calculate the area of the polygon defined by the given points
        /// </summary>
        /// <param name="points">The Point2d collection</param>
        /// <returns>The area</returns>
        public static double Area(this ICollection<Point2d> points)
        {
            double area = 0;
            int n = points.Count;
            for (int i = 0; i < n; i++)
            {
                int i1 = i;
                int i2 = i + 1;
                if (i2 >= n)
                    i2 = 0;
                area += (points.ElementAt(i1).X * points.ElementAt(i2).Y) - (points.ElementAt(i1).Y * points.ElementAt(i2).X);
            }

            return area / 2;
        }
    }
}
