using System.Collections.Generic;
using System.Diagnostics;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// The connectivity of a mesh: the vertices adjacent to each vertex, the faces of each vertex and the faces near each face
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class MeshConnectivity
    {
        /// <summary>
        /// Vertex-vertex: for each vertex id, the ids of the vertices joined to it by an edge of a face
        /// </summary>
        public Dictionary<int, HashSet<int>> VVC = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// Vertex-face: for each vertex id, the ids of the faces that contain it
        /// </summary>
        public Dictionary<int, HashSet<int>> VFC = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// Face-face: for each face id, the ids of the faces that share a vertex with it and precede it in the mesh (the relation is not symmetric)
        /// </summary>
        public Dictionary<int, HashSet<int>> FFC = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Adds <paramref name="vb"/> to the vertices adjacent to <paramref name="va"/>
        /// </summary>
        /// <param name="va">The vertex</param>
        /// <param name="vb">The adjacent vertex</param>
        private void AddVVC(int va, int vb)
        {
            if (!VVC.ContainsKey(va))
            {
                VVC[va] = new HashSet<int>();
            }
            VVC[va].Add(vb);
        }

        /// <summary>
        /// Adds <paramref name="f"/> to the faces of the vertex <paramref name="v"/>
        /// </summary>
        /// <param name="v">The vertex</param>
        /// <param name="f">The face</param>
        private void AddVFC(int v, int f)
        {
            if (!VFC.ContainsKey(v))
            {
                VFC[v] = new HashSet<int>();
            }
            VFC[v].Add(f);
        }

        /// <summary>
        /// Adds <paramref name="fb"/> to the faces near <paramref name="fa"/>
        /// </summary>
        /// <param name="fa">The face</param>
        /// <param name="fb">The near face</param>
        private void AddFFC(int fa, int fb)
        {
            if (!FFC.ContainsKey(fa))
            {
                FFC[fa] = new HashSet<int>();
            }
            FFC[fa].Add(fb);
        }

        /// <summary>
        /// Computes the connectivity of the faces of a mesh
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <returns>The connectivity</returns>
        public static MeshConnectivity CreateForMesh(Mesh mesh)
        {
            MeshConnectivity conn = new MeshConnectivity();

            foreach (var face in mesh.Faces)
            {
                var fid = face.Id;

                var nodes = face.GetNodes();
                for (int i = 0; i < nodes.Length; i++)
                {
                    var prev = i > 0 ? i - 1 : nodes.Length - 1;
                    var post = i < nodes.Length - 1 ? i + 1 : 0;

                    conn.AddVVC(nodes[i], nodes[prev]);
                    conn.AddVVC(nodes[i], nodes[post]);
                    if (conn.VFC.ContainsKey(nodes[i]))
                    {
                        foreach (var f in conn.VFC[nodes[i]])
                        {
                            if (f != fid)
                            {
                                conn.AddFFC(fid, f);
                            }
                        }
                    }
                    conn.AddVFC(nodes[i], fid);
                }
            }

            return conn;
        }

    }
}
