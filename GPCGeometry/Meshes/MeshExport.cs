using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.FormattableString;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// Export of meshes to the MSH format of Gmsh (versions 1 and 2.2, ASCII)
    /// </summary>
    /// <remarks>The numbers are written with the invariant culture (before, the version 1 used the culture of the thread: decimal comma with the
    /// Italian culture; the version 2 changed the culture of the thread and did not restore it after an exception).
    /// The physical tag of the elements is the position of the mesh in the list, computed once for every mesh (before, meshes.IndexOf(mesh)
    /// for every element: Mesh.Equals compares the whole content, 70 s to export 4 meshes of 8000 faces)</remarks>
    public static class MeshExport
    {
        /// <summary>
        /// Export meshes in MSH format version 1: the vertices and the faces of all the meshes, with the ids of the meshes (the ids must be
        /// unique among all the meshes); the physical and elementary tag of a face is the position of its mesh in the list + 1
        /// </summary>
        /// <param name="path">The path of the file to write (overwritten)</param>
        /// <param name="meshes">The meshes</param>
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
                        writetext.WriteLine(Invariant($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}"));
                    }
                }
                writetext.WriteLine("$ENDNOD");

                writetext.WriteLine("$ELM");
                writetext.WriteLine($"{meshes.Select(m => m.FacesCount).Sum()}");
                for (int k = 0; k < meshes.Count; k++)
                {
                    int tag = k + 1;
                    var m = meshes[k].GetFacesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine(Invariant($"{m.Current.Id} {(m.Current.IsQuad ? 3 : 2)} {tag} {tag} {(m.Current.IsQuad ? 4 : 3)} ") +
                            Invariant($"{m.Current.A} {m.Current.B} {m.Current.C} {(m.Current.IsQuad ? m.Current.D.ToString() : string.Empty)}"));
                    }
                }
                writetext.WriteLine("$ENDELM");
            }
        }

        /// <summary>
        /// Export meshes in MSH format version 2.2: the vertices, the faces and the volumes of all the meshes, with the ids of the meshes (the ids
        /// must be unique among all the meshes); the tags of an element are the position of its mesh in the list + 1
        /// </summary>
        /// <param name="path">The path of the file to write (overwritten)</param>
        /// <param name="meshes">The meshes</param>
        public static void ExportToMshFormatv2(string path, List<Mesh> meshes)
        {
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
                        writetext.WriteLine(Invariant($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}"));
                    }
                }
                writetext.WriteLine("$EndNodes");

                writetext.WriteLine("$Elements");
                writetext.WriteLine($"{meshes.Select(m => m.FacesCount).Sum() + meshes.Select(m => m.VolumesCount).Sum()}");
                for (int k = 0; k < meshes.Count; k++)
                {
                    int tag = k + 1;
                    var m = meshes[k].GetFacesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine($"{m.Current.Id} " +
                                            $"{(m.Current.IsQuad ? 3 : 2)} " +
                                            $"2 " +
                                            $"{tag} " +
                                            $"{tag} " +
                                            $"{m.Current.A} " +
                                            $"{m.Current.B} " +
                                            $"{m.Current.C} " +
                                            $"{(m.Current.IsQuad ? m.Current.D.ToString() : string.Empty)}");
                    }
                }
                for (int k = 0; k < meshes.Count; k++)
                {
                    int tag = k + 1;
                    var m = meshes[k].GetVolumesEnumerator();
                    while (m.MoveNext())
                    {
                        writetext.WriteLine($"{m.Current.Id} " +
                                            $"{(m.Current.IsQuadrangularPrism ? 5 : 6)} " +
                                            $"{tag} " +
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
        }

        /// <summary>
        /// Export a mesh in MSH format version 2.2: the vertices and the faces (the volumes are not written), with tags 1
        /// </summary>
        /// <param name="path">The path of the file to write (overwritten)</param>
        /// <param name="mesh">The mesh</param>
        public static void ExportToMshFormatv2(string path, Mesh mesh)
        {
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
                    writetext.WriteLine(Invariant($"{v.Current.Id} {v.Current.Point.X} {v.Current.Point.Y} {v.Current.Point.Z}"));
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
        }
    }
}
