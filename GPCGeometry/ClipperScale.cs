using System;

namespace GPC.Geometry
{
    /// <summary>
    /// Scale of the coordinates for the boolean operations of Clipper, which works with integers
    /// </summary>
    /// <remarks>Before September 2026 the factor was always 1000 and the coordinates were truncated: 0.001 units of precision
    /// (1 mm for shapes in metres), with an error towards zero</remarks>
    internal static class ClipperScale
    {
        /// <summary>
        /// Largest scaled coordinate: up to about 1E9 Clipper uses the 64 bit arithmetic (beyond it the slower 128 bit one)
        /// </summary>
        private const double Range = 1E9;

        /// <returns>The power of ten that brings the largest coordinate close to <see cref="Range"/> (relative precision 1E-9)</returns>
        public static double Factor(double maxAbsCoordinate)
        {
            if (!(maxAbsCoordinate > 0) || double.IsInfinity(maxAbsCoordinate) || double.IsNaN(maxAbsCoordinate))
                return 1E6;

            return Math.Pow(10.0, Math.Floor(Math.Log10(Range / maxAbsCoordinate)));
        }

        /// <returns>The scaled and rounded coordinate</returns>
        public static long Scale(double coordinate, double factor)
        {
            return (long)Math.Round(coordinate * factor);
        }

        /// <returns>The largest absolute coordinate (X and Y) of the polygons</returns>
        public static double MaxAbsCoordinate(params Polygon3d[] polygons)
        {
            double max = 0;
            foreach (Polygon3d polygon in polygons)
            {
                if (polygon is null)
                    continue;
                for (int i = 0; i < polygon.Count; i++)
                    max = Math.Max(max, Math.Max(Math.Abs(polygon[i].X), Math.Abs(polygon[i].Y)));
            }
            return max;
        }

        /// <returns>The largest absolute coordinate (X and Y) of the polygons</returns>
        public static double MaxAbsCoordinate(params Polygon2d[] polygons)
        {
            double max = 0;
            foreach (Polygon2d polygon in polygons)
            {
                if (polygon is null)
                    continue;
                for (int i = 0; i < polygon.Count; i++)
                    max = Math.Max(max, Math.Max(Math.Abs(polygon[i].X), Math.Abs(polygon[i].Y)));
            }
            return max;
        }

        /// <returns>The largest absolute coordinate (X and Y) of the fills and of the holes of the shapes</returns>
        public static double MaxAbsCoordinate(Shape[] a, Shape[] b)
        {
            double max = 0;
            foreach (Shape[] shapes in new[] { a, b })
            {
                foreach (Shape shape in shapes)
                {
                    max = Math.Max(max, MaxAbsCoordinate(shape.Fill));
                    if (shape.Holes != null)
                        max = Math.Max(max, MaxAbsCoordinate(shape.Holes));
                }
            }
            return max;
        }
    }
}
