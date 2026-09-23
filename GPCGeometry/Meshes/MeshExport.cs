using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPC.Geometry.Meshes
{
    public static class MeshExport
    {
        /// <summary>
        /// Export mesh in MSH format version 1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="meshes"></param>
        public static void ExportToMshFormatv1(string path, List<Mesh> meshes)
        {
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine("$NOD");
                writetext.WriteLine($"{meshes.Select(m => m.VerticesCount).Sum()}");

                foreach (var mesh in meshes)
                {
                    var v = mesh.GetVerticesEnumerator();
                    while (v.MoveNext())
                    {
                        writetext.WriteLine($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}");
                    }
                }
                writetext.WriteLine("$ENDNOD");

                writetext.WriteLine("$ELM");
                writetext.WriteLine($"{meshes.Select(m => m.FacesCount).Sum()}");
                foreach (var mesh in meshes)
                {
                    var m = mesh.GetFacesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine($"{m.Current.Id} {(m.Current.IsQuad ? 3 : 2)} {meshes.IndexOf(mesh) + 1} {meshes.IndexOf(mesh) + 1} {(m.Current.IsQuad ? 4 : 3)} " +
                            $"{m.Current.A} {m.Current.B} {m.Current.C} {(m.Current.IsQuad ? m.Current.D.ToString() : string.Empty)}");
                    }
                }
                writetext.WriteLine("$ENDELM");
            }
        }

        /// <summary>
        /// Export mesh in MSH format version 2
        /// </summary>
        /// <param name="path"></param>
        /// <param name="meshes"></param>
        public static void ExportToMshFormatv2(string path, List<Mesh> meshes)
        {
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine("$MeshFormat");
                writetext.WriteLine("2.2 0 8");
                writetext.WriteLine("$EndMeshFormat");

                writetext.WriteLine("$Nodes");
                writetext.WriteLine($"{meshes.Select(m => m.VerticesCount).Sum()}");

                foreach (var mesh in meshes)
                {
                    var v = mesh.GetVerticesEnumerator();
                    while (v.MoveNext())
                    {
                        writetext.WriteLine($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}");
                    }
                }
                writetext.WriteLine("$EndNodes");

                writetext.WriteLine("$Elements");
                writetext.WriteLine($"{meshes.Select(m => m.FacesCount).Sum() + meshes.Select(m => m.VolumesCount).Sum()}");
                foreach (var mesh in meshes)
                {
                    var m = mesh.GetFacesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine($"{m.Current.Id} " +
                                            $"{(m.Current.IsQuad ? 3 : 2)} " +
                                            $"2 " +
                                            $"{meshes.IndexOf(mesh) + 1} " +
                                            $"{meshes.IndexOf(mesh) + 1} " +
                                            $"{m.Current.A} " +
                                            $"{m.Current.B} " +
                                            $"{m.Current.C} " +
                                            $"{(m.Current.IsQuad ? m.Current.D.ToString() : string.Empty)}");
                    }
                }
                foreach (var mesh in meshes)
                {
                    var m = mesh.GetVolumesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine($"{m.Current.Id} " +
                                            $"{(m.Current.IsQuadrangularPrism ? 5 : 6)} " +
                                            $"{meshes.IndexOf(mesh) + 1} " +
                                            $"2 " +
                                            $"{m.Current.A} " +
                                            $"{m.Current.B} " +
                                            $"{m.Current.C} " +
                                            $"{m.Current.D} " +
                                            $"{m.Current.E} " +
                                            $"{m.Current.F} " +
                                            $"{(m.Current.IsQuadrangularPrism ? m.Current.G.ToString() : string.Empty)} " +
                                            $"{(m.Current.IsQuadrangularPrism ? m.Current.H.ToString() : string.Empty)}");
                    }
                }
                writetext.WriteLine("$EndElements");
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
        }

        /// <summary>
        /// Export mesh in MSH format version 2
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mesh"></param>
        public static void ExportToMshFormatv2(string path, Mesh mesh)
        {
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine("$MeshFormat");
                writetext.WriteLine("2.2 0 8");
                writetext.WriteLine("$EndMeshFormat");

                writetext.WriteLine("$Nodes");
                writetext.WriteLine($"{mesh.VerticesCount}");

                IEnumerator<MeshVertex> v = mesh.GetVerticesEnumerator();
                while (v.MoveNext())
                {
                    writetext.WriteLine($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}");
                }

                writetext.WriteLine("$EndNodes");

                writetext.WriteLine("$Elements");
                writetext.WriteLine($"{mesh.FacesCount + mesh.VolumesCount}");

                IEnumerator<MeshFace> m = mesh.GetFacesEnumerator();
                while (m.MoveNext())
                {
                    writetext.WriteLine($"{m.Current.Id} " +
                                        $"{(m.Current.IsQuad ? 3 : 2)} " +
                                        $"2 " +
                                        $"{1} " +
                                        $"{1} " +
                                        $"{m.Current.A} " +
                                        $"{m.Current.B} " +
                                        $"{m.Current.C} " +
                                        $"{(m.Current.IsQuad ? m.Current.D.ToString() : string.Empty)}");
                }

                writetext.WriteLine("$EndElements");
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }
}


