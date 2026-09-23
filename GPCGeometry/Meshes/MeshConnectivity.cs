using System.Collections.Generic;
using System.Diagnostics;

namespace GPC.Geometry.Meshes
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class MeshConnectivity
    {
        public Dictionary<int, HashSet<int>> VVC = new Dictionary<int, HashSet<int>>();
        public Dictionary<int, HashSet<int>> VFC = new Dictionary<int, HashSet<int>>();
        public Dictionary<int, HashSet<int>> FFC = new Dictionary<int, HashSet<int>>();

        private void AddVVC(int va, int vb)
        {
            if (!VVC.ContainsKey(va))
            {
                VVC[va] = new HashSet<int>();
            }
            VVC[va].Add(vb);
        }

        private void AddVFC(int v, int f)
        {
            if (!VFC.ContainsKey(v))
            {
                VFC[v] = new HashSet<int>();
            }
            VFC[v].Add(f);
        }

        private void AddFFC(int fa, int fb)
        {
            if (!FFC.ContainsKey(fa))
            {
                FFC[fa] = new HashSet<int>();
            }
            FFC[fa].Add(fb);
        }

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
