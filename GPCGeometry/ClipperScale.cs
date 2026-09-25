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

        /// <summary>
        /// The scale factor for the coordinates of the polygons of an operation
        /// </summary>
        /// <param name="maxAbsCoordinate">The largest absolute coordinate of the polygons</param>
        /// <returns>The power of ten that brings the largest coordinate close to <see cref="Range"/> (relative precision 1E-9); 1E6 if the
        /// coordinate is zero or not finite</returns>
        public static double Factor(double maxAbsCoordinate)
        {
            if (!(maxAbsCoordinate > 0) || double.IsInfinity(maxAbsCoordinate) || double.IsNaN(maxAbsCoordinate))
                return 1E6;

            return Math.Pow(10.0, Math.Floor(Math.Log10(Range / maxAbsCoordinate)));
        }

        /// <summary>
        /// Converts a coordinate to the integer used by Clipper
        /// </summary>
        /// <param name="coordinate">The coordinate</param>
        /// <param name="factor">The scale factor (see <see cref="Factor"/>)</param>
        /// <returns>The scaled and rounded coordinate</returns>
        public static long Scale(double coordinate, double factor)
        {
            return (long)Math.Round(coordinate * factor);
        }

        /// <summary>
        /// The largest absolute coordinate of the vertices of polygons in the space (Z is ignored)
        /// </summary>
        /// <param name="polygons">The polygons (the null ones are skipped)</param>
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

        /// <summary>
        /// The largest absolute coordinate of the vertices of planar polygons
        /// </summary>
        /// <param name="polygons">The polygons (the null ones are skipped)</param>
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

        /// <summary>
        /// The largest absolute coordinate of the vertices of two groups of shapes
        /// </summary>
        /// <param name="a">The first group of shapes</param>
        /// <param name="b">The second group of shapes</param>
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
