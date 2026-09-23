using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Geometry
{
    public static class GeometryExport
    {


        /// <summary>
        /// Export geometry to .geo format. This file can be opened by Gmsh
        /// </summary>
        /// <param name="path"></param>
        /// <param name="geometries"></param>
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

        private class GeometryMapCollection<T> where T : GeometryBase
        {
            private HashSet<GeometryMap<T>> _map;

            public static int MaxId { get; private set; }

            public GeometryMapCollection()
            {
                _map = new HashSet<GeometryMap<T>>();
            }

            public void Add(GeometryMap<T> map)
            {
                _map.Add(map);
            }

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


            public int GetNextId()
            {
                MaxId++;
                return MaxId;
            }

        }


        private class GeometryMap<T> where T : GeometryBase
        {
            public T GeometryBase;
            public int ExportId;

            public GeometryMap(T geometryBase, int exportId)
            {
                GeometryBase = geometryBase;
                ExportId = exportId;
            }

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

            public override bool Equals(object obj)
            {
                return obj is GeometryMap<T> map &&
                       EqualityComparer<T>.Default.Equals(GeometryBase, map.GeometryBase) &&
                       ExportId == map.ExportId;
            }

            public override int GetHashCode()
            {
                return 1439058172 + EqualityComparer<T>.Default.GetHashCode(GeometryBase);
            }


        }


    }
}
