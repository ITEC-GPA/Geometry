using System;
using System.Collections.Generic;

namespace GPC.Geometry
{
    /// <summary>
    /// The removal of the aligned vertices of a closed polygon, the same for <see cref="Polygon2d"/> and <see cref="Polygon3d"/> (the 2D
    /// polygons use Z = 0)
    /// </summary>
    internal static class AlignedPoints
    {
        /// <summary>
        /// The indices of the vertices to keep. A vertex is removed when it is within <paramref name="tolerance"/> from the line through its
        /// current neighbours (the vertices not removed yet) and all the vertices already removed between those neighbours stay within the
        /// tolerance from that line: every removed vertex is within the tolerance from the line of the final side that replaces it. The
        /// spikes (a vertex on the line of its neighbours but outside their segment) and the repeated consecutive vertices are removed too.
        /// If all the vertices are within the tolerance from one line, the two extremes are kept. Otherwise at least three vertices are kept
        /// </summary>
        /// <param name="x">The X coordinates of the vertices</param>
        /// <param name="y">The Y coordinates of the vertices</param>
        /// <param name="z">The Z coordinates of the vertices</param>
        /// <param name="tolerance">The tolerance on the distance from the line of the neighbours</param>
        /// <returns>The indices of the vertices to keep, in ascending order (all the indices with less than three vertices)</returns>
        internal static int[] GetIndicesToKeep(double[] x, double[] y, double[] z, double tolerance)
        {
            int n = x.Length;
            var all = new int[n];
            for (int i = 0; i < n; i++)
                all[i] = i;

            if (n < 3)
                return all;

            if (tolerance < 0)
                tolerance = 0;

            // all the vertices on one line: the two extremes, in their order
            if (AreAllAligned(x, y, z, tolerance, out int first, out int second))
                return first < second ? new[] { first, second } : new[] { second, first };

            var previous = new int[n];
            var next = new int[n];
            var removedAfter = new List<int>[n]; // the removed vertices between a kept vertex and the next kept one
            var alive = new bool[n];
            for (int i = 0; i < n; i++)
            {
                previous[i] = (i - 1 + n) % n;
                next[i] = (i + 1) % n;
                alive[i] = true;
            }

            int count = n;
            bool changed = true;
            while (changed && count > 3)
            {
                changed = false;
                for (int v = 0; v < n && count > 3; v++)
                {
                    if (!alive[v])
                        continue;

                    int p = previous[v], q = next[v];

                    if (!IsWithinTolerance(x, y, z, p, q, v, tolerance) ||
                        !AreWithinTolerance(x, y, z, p, q, removedAfter[p], tolerance) ||
                        !AreWithinTolerance(x, y, z, p, q, removedAfter[v], tolerance))
                        continue;

                    // v removed: its removed vertices and v itself now lie between p and q
                    var merged = removedAfter[p] ?? new List<int>();
                    merged.Add(v);
                    if (removedAfter[v] != null)
                        merged.AddRange(removedAfter[v]);
                    removedAfter[p] = merged;
                    removedAfter[v] = null;

                    next[p] = q;
                    previous[q] = p;
                    alive[v] = false;
                    count--;
                    changed = true;
                }
            }

            var kept = new List<int>(count);
            for (int i = 0; i < n; i++)
            {
                if (alive[i])
                    kept.Add(i);
            }

            return kept.ToArray();
        }

        /// <summary>
        /// Tell if all the vertices are within the tolerance from the line of the two farthest vertices (the farthest vertex from the first
        /// one and the farthest vertex from it)
        /// </summary>
        /// <param name="x">The X coordinates</param>
        /// <param name="y">The Y coordinates</param>
        /// <param name="z">The Z coordinates</param>
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <param name="first">The index of the first extreme</param>
        /// <param name="second">The index of the second extreme</param>
        /// <returns>True if all the vertices are aligned (or coincident)</returns>
        private static bool AreAllAligned(double[] x, double[] y, double[] z, double tolerance, out int first, out int second)
        {
            first = Farthest(x, y, z, 0);
            second = Farthest(x, y, z, first);

            for (int i = 0; i < x.Length; i++)
            {
                if (!IsWithinTolerance(x, y, z, first, second, i, tolerance))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// The vertex farthest from a given one (the first one for equal distances)
        /// </summary>
        /// <param name="x">The X coordinates</param>
        /// <param name="y">The Y coordinates</param>
        /// <param name="z">The Z coordinates</param>
        /// <param name="from">The index of the given vertex</param>
        /// <returns>The index of the farthest vertex</returns>
        private static int Farthest(double[] x, double[] y, double[] z, int from)
        {
            int farthest = from;
            double maximum = -1;
            for (int i = 0; i < x.Length; i++)
            {
                double dx = x[i] - x[from], dy = y[i] - y[from], dz = z[i] - z[from];
                double distance = dx * dx + dy * dy + dz * dz;
                if (distance > maximum)
                {
                    maximum = distance;
                    farthest = i;
                }
            }

            return farthest;
        }

        /// <summary>
        /// Tell if the given vertices are within the tolerance from the line through two vertices
        /// </summary>
        /// <param name="x">The X coordinates</param>
        /// <param name="y">The Y coordinates</param>
        /// <param name="z">The Z coordinates</param>
        /// <param name="a">The first vertex of the line</param>
        /// <param name="b">The second vertex of the line</param>
        /// <param name="vertices">The vertices to check (null: none)</param>
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <returns>True if all the vertices are within the tolerance</returns>
        private static bool AreWithinTolerance(double[] x, double[] y, double[] z, int a, int b, List<int> vertices, double tolerance)
        {
            if (vertices == null)
                return true;

            for (int k = 0; k < vertices.Count; k++)
            {
                if (!IsWithinTolerance(x, y, z, a, b, vertices[k], tolerance))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tell if a vertex is within the tolerance from the line through two vertices (from the first vertex if the two are closer than the
        /// tolerance)
        /// </summary>
        /// <param name="x">The X coordinates</param>
        /// <param name="y">The Y coordinates</param>
        /// <param name="z">The Z coordinates</param>
        /// <param name="a">The first vertex of the line</param>
        /// <param name="b">The second vertex of the line</param>
        /// <param name="v">The vertex to check</param>
        /// <param name="tolerance">The tolerance on the distance</param>
        /// <returns>True if the distance is not bigger than the tolerance</returns>
        private static bool IsWithinTolerance(double[] x, double[] y, double[] z, int a, int b, int v, double tolerance)
        {
            double ux = x[b] - x[a], uy = y[b] - y[a], uz = z[b] - z[a];
            double wx = x[v] - x[a], wy = y[v] - y[a], wz = z[v] - z[a];
            double length2 = ux * ux + uy * uy + uz * uz;

            if (length2 <= tolerance * tolerance)
                return wx * wx + wy * wy + wz * wz <= tolerance * tolerance;

            // |u x w|² / |u|² is the square of the distance from the line
            double cx = uy * wz - uz * wy, cy = uz * wx - ux * wz, cz = ux * wy - uy * wx;
            return cx * cx + cy * cy + cz * cz <= tolerance * tolerance * length2;
        }
    }
}
