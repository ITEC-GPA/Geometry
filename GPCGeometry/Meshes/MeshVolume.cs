using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    [Serializable]
    public class MeshVolume : MeshBase, ISerializable, IEquatable<MeshVolume>, ICloneable
    {

        private int _a;
        private int _b;
        private int _c;
        private int _d;
        private int _e;
        private int _f;
        private int _g;
        private int _h;


        public int A => _a;
        public int B => _b;
        public int C => _c;
        public int D => _d;
        public int E => _e;
        public int F => _f;
        public int G => _g;
        public int H => _h;

        public bool IsTriangularPrism => _h == -1 && _g == -1;

        public bool IsQuadrangularPrism => _h != -1 && _g != -1;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="tag"></param>
        /// <exception cref="ArgumentOutOfRangeException">if Vertices[] lenght is higher than 8 or lower than 6 or equal to 7</exception>
        public MeshVolume(int[] vertices, object tag = null)
        {
            if (vertices.Count() > 8 || vertices.Count() < 6)
                throw new ArgumentOutOfRangeException("Mesh Volume vertices count higher than 8 or lower than 6");
            if (vertices.Count() == 7)
                throw new ArgumentOutOfRangeException("Number of mesh volume vertices cannot be 7");

            _a = vertices[0];
            _b = vertices[1];
            _c = vertices[2];
            _d = vertices[3];
            _e = vertices[4];
            _f = vertices[5];

            if (vertices.Count() > 6)
            {
                _g = vertices[6];
                _h = vertices[7];
            }
            else
            {
                _g = MeshVertex.Unassigned;
                _h = MeshVertex.Unassigned;
            }

            Tag = tag;
        }




        /// <summary>
        /// if the volume have a quadrangular base:
        ///     a,b,c,d represent the vertex id of first face.
        ///     e,f,g,h represent the vertex id of second face.
        /// if the volume have a triangular base:
        ///     a,b,c represent the vertex id of first face. 
        ///     d,e,f represent the vertex id of second face.
        /// The order of the vertex should be the same on both face.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        public MeshVolume(int a, int b, int c, int d, int e, int f)
            : this(a, b, c, d, e, f, -1, -1)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;

            _g = MeshVertex.Unassigned;
            _h = MeshVertex.Unassigned;

            if (a < 0 || b < 0 || c < 0 || d < 0 || e < 0 || f < 0)
            {
                throw new ArgumentException("Vertex from A to F cannot be negative");
            }

        }

        /// <summary>
        /// if the volume have a quadrangular base:
        ///     a,b,c,d represent the vertex id of first face.
        ///     e,f,g,h represent the vertex id of second face.
        /// if the volume have a triangular base:
        ///     a,b,c represent the vertex id of first face. 
        ///     d,e,f represent the vertex id of second face.
        /// The order of the vertex should be the same on both face.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <param name="h"></param>
        /// <param name="tag"></param>
        public MeshVolume(int a, int b, int c, int d, int e, int f, int g, int h, object tag = null)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;

            if (a < 0 || b < 0 || c < 0 || d < 0 || e < 0 || f < 0)
            {
                throw new ArgumentException("Vertex from A to F cannot be negative");
            }
            if ((g >= 0 && h < 0) || (g < 0 && h >= 0))
            {
                throw new ArgumentException();
            }
            base.Tag = tag;
        }


        public MeshVolume(MeshVolume volume)
        {
            _a = volume.A;
            _b = volume.B;
            _c = volume.C;
            _d = volume.D;
            _e = volume.E;
            _f = volume.F;
            _g = volume.G;
            _h = volume.H;

        }

        public MeshVolume(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _a = info.GetInt32("A");
            _b = info.GetInt32("B");
            _c = info.GetInt32("C");
            _d = info.GetInt32("D");
            _e = info.GetInt32("E");
            _f = info.GetInt32("F");
            _g = info.GetInt32("G");
            _h = info.GetInt32("H");
        }


        /// <summary>
        /// Get the nodes ids
        /// </summary>
        /// <param name="reverse">Return the array inverted</param>
        /// <returns>The Ids array</returns>
        public int[] GetNodes(bool reverse = false)
        {
            int[] nodes;
            if (IsQuadrangularPrism)
                nodes = new int[] { A, B, C, D, E, F, G, H };
            else
                nodes = new int[] { A, B, C, D, E, F };
            if (reverse)
                return nodes.Reverse().ToArray();
            return nodes;
        }

        public object Clone()
        {
            return new MeshVolume(this);
        }

        /// <summary>
        /// Tells if 2 volumes have the same nodes also if the volumes don't have the same number of nodes and not in the same order
        /// </summary>
        /// <param name="nodes">The nodes of the volumes to compare</param>
        /// <returns>True if the nodes matches</returns>
        public bool IsMatch(int[] nodes)
        {
            List<int> bigVolume;
            List<int> smallVolume;

            int[] tn = GetNodes();

            if (tn.Length >= nodes.Length)
            {
                bigVolume = tn.ToList();
                smallVolume = new List<int>(nodes);
            }
            else
            {
                bigVolume = new List<int>(nodes);
                smallVolume = tn.ToList();
            }

            bigVolume.Add(bigVolume[0]);
            List<int> reversed = new List<int>(smallVolume);
            reversed.Reverse();

            bool match = false;
            for (int i = 0; i < bigVolume.Count - smallVolume.Count + 1; i++)
            {
                List<int> sub = bigVolume.GetRange(i, smallVolume.Count);
                if (sub.SequenceEqual(smallVolume) || sub.SequenceEqual(reversed))
                {
                    match = true;
                    break;
                }
            }

            return match;

        }

        public bool Equals(MeshVolume other)
        {
            return !(other is null) && _a == other._a
                                    && _b == other._b && _c == other._c && _d == other._d
                                    && _e == other._e && _f == other._f && _g == other._g && _h == other._h;
        }

        public override bool Equals(object obj)
        {
            if (obj is MeshVolume face)
            {
                return Equals(face);
            }
            return Equals(obj);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A", _a);
            info.AddValue("B", _b);
            info.AddValue("C", _c);
            info.AddValue("D", _d);
            info.AddValue("E", _e);
            info.AddValue("F", _f);
            info.AddValue("G", _g);
            info.AddValue("H", _h);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _a.GetHashCode();
                hashCode = hashCode * -17 + _b.GetHashCode();
                hashCode = hashCode * -17 + _c.GetHashCode();
                hashCode = hashCode * -17 + _d.GetHashCode();
                hashCode = hashCode * -17 + _e.GetHashCode();
                hashCode = hashCode * -17 + _f.GetHashCode();
                hashCode = hashCode * -17 + _g.GetHashCode();
                hashCode = hashCode * -17 + _h.GetHashCode();
                return hashCode;
            }
        }


        #region Operators overrides

        public static bool operator ==(MeshVolume vol1, MeshVolume vol2)
        {
            if (ReferenceEquals(vol1, vol2))
                return true;
            return vol1.Equals(vol2);
        }

        public static bool operator !=(MeshVolume vol1, MeshVolume vol2)
        {
            return !(vol1 == vol2);
        }

        #endregion

        private string DebuggerDisplay()
        {
            return $"Id:{Id}, A:{A}, B:{B}, C:{C}, D:{D}, E:{E}, F:{F}, G:{G}, H:{H}";
        }


    }
}
