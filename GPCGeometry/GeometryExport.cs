using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Geometry
{
    /// <summary>
    /// Export of geometries to the .geo format of Gmsh (a draft: only the lines are exported)
    /// </summary>
    public static class GeometryExport
    {


        /// <summary>
        /// Export geometry to .geo format. This file can be opened by Gmsh. Only the <see cref="Line3d"/> are exported, with their end points;
        /// the other geometries are ignored
        /// </summary>
        /// <param name="path">The path of the file to write</param>
        /// <param name="geometries">The geometries</param>
        public static void ExportToGeoFormat(string path, List<GeometryBase> geometries)
        {
            // questa classe non è finita, è solo un abbozzo, vanno aggiunte le altre geometrie


            using (StreamWriter writetext = new StreamWriter(path))
            {
                StringBuilder sb = new StringBuilder();

                GeometryMapCollection<Point3d> point3dMap = new GeometryMapCollection<Point3d>();
                GeometryMapCollection<Line3d> line3dMap = new GeometryMapCollection<Line3d>();


                foreach (var geometry in geometries)
                {
                    if (geometry is Line3d line)
                    {
                        int startIndex = 0;
                        int endIndex = 0;

                        if (!point3dMap.Constains(line.Start, out startIndex))
                        {
                            startIndex = point3dMap.GetNextId();
                            point3dMap.Add(new GeometryMap<Point3d>(line.Start, startIndex));
                        }

                        if (!point3dMap.Constains(line.End, out endIndex))
                        {
                            endIndex = point3dMap.GetNextId();
                            point3dMap.Add(new GeometryMap<Point3d>(line.End, endIndex));
                        }

                        int lineIndex = line3dMap.GetNextId();
                        line3dMap.Add(new GeometryMap<Line3d>(line, lineIndex));

                        sb.AppendLine( $"Point({startIndex}) = {{{line.Start.X},{line.Start.Y},{line.Start.Z}}};");
                        sb.AppendLine( $"Point({endIndex}) = {{{line.End.X},{line.End.Y},{line.End.Z}}};");

                        sb.AppendLine($"Line({lineIndex}) = {{{startIndex},{endIndex}}};");
                    }
                }

                writetext.Write(sb.ToString());

            }
        }

        /// <summary>
        /// The geometries already exported, with their ids in the .geo file
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        private class GeometryMapCollection<T> where T : GeometryBase
        {
            /// <summary>
            /// The exported geometries with their ids
            /// </summary>
            private HashSet<GeometryMap<T>> _map;

            /// <summary>
            /// The last id given (static: shared by all the exports of the same type of geometry)
            /// </summary>
            public static int MaxId { get; private set; }

            /// <summary>
            /// Creates an empty collection
            /// </summary>
            public GeometryMapCollection()
            {
                _map = new HashSet<GeometryMap<T>>();
            }

            /// <summary>
            /// Adds an exported geometry
            /// </summary>
            /// <param name="map">The geometry and its id</param>
            public void Add(GeometryMap<T> map)
            {
                _map.Add(map);
            }

            /// <summary>
            /// Tell if a geometry has already been exported
            /// </summary>
            /// <param name="geometry">The geometry</param>
            /// <param name="index">The id of the geometry, -1 if it has not been exported</param>
            /// <returns>True if the geometry has been exported</returns>
            public bool Constains(T geometry, out int index)
            {
                GeometryMap<T> fakeGeom = new GeometryMap<T>(geometry, -1);

                if (_map.Contains(fakeGeom))
                {
                    index = fakeGeom.ExportId;

                    return true;
                }
                else
                {
                    index = -1;
                    return false;
                }


            }


            /// <summary>
            /// A new id: the last one plus one
            /// </summary>
            /// <returns>The new id</returns>
            public int GetNextId()
            {
                MaxId++;
                return MaxId;
            }

        }


        /// <summary>
        /// A geometry and its id in the .geo file
        /// </summary>
        /// <typeparam name="T">The type of the geometry</typeparam>
        private class GeometryMap<T> where T : GeometryBase
        {
            /// <summary>
            /// The geometry
            /// </summary>
            public T GeometryBase;
            /// <summary>
            /// The id in the .geo file
            /// </summary>
            public int ExportId;

            /// <summary>
            /// Creates the association of a geometry with its id
            /// </summary>
            /// <param name="geometryBase">The geometry</param>
            /// <param name="exportId">The id</param>
            public GeometryMap(T geometryBase, int exportId)
            {
                GeometryBase = geometryBase;
                ExportId = exportId;
            }

            /// <summary>
            /// Tell if <paramref name="geom"/> is the geometry of this association
            /// </summary>
            /// <param name="geom">The geometry to compare</param>
            /// <param name="id">The id of the association if the geometries are equal, otherwise -1</param>
            /// <returns>True if the geometries are equal</returns>
            public bool Match(T geom, out int id)
            {
                if (GeometryBase.Equals(geom))
                {
                    id = ExportId;
                    return true;
                }
                else
                {
                    id = -1;
                    return false;
                }
            }

            /// <summary>
            /// Equality of the geometry and of the id
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> is an equal association</returns>
            public override bool Equals(object obj)
            {
                return obj is GeometryMap<T> map &&
                       EqualityComparer<T>.Default.Equals(GeometryBase, map.GeometryBase) &&
                       ExportId == map.ExportId;
            }

            /// <summary>
            /// The hash code of the geometry
            /// </summary>
            /// <returns>The hash code</returns>
            public override int GetHashCode()
            {
                return 1439058172 + EqualityComparer<T>.Default.GetHashCode(GeometryBase);
            }


        }


    }
}
