using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshFace : MeshBase, ISerializable, IEquatable<MeshFace>, ICloneable
    {
        #region Variables

        private int _a;
        private int _b;
        private int _c;
        private int _d;

        #endregion

        #region Properties

        public int A => _a;

        public int B => _b;

        public int C => _c;

        public int D => _d;

        public bool IsTriangle => _d == -1;

        public bool IsQuad => _d != -1;

        #endregion

        #region Public Constructors

        public MeshFace(int a, int b, int c, int d, object tag = null)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            Tag = tag;
        }

        public MeshFace(int a, int b, int c)
            : this(a, b, c, MeshVertex.Unassigned, null)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="tag"></param>
        /// <exception cref="ArgumentOutOfRangeException">if Vertices[] lenght is higher than 4 or lower than 3</exception>
        public MeshFace(int[] vertices, object tag = null)
        {
            if (vertices.Count() > 4 || vertices.Count() < 3)
                throw new ArgumentOutOfRangeException("MeshFace vertices count higher than 4 or lower than 3");

            _a = vertices[0];
            _b = vertices[1];
            _c = vertices[2];
            if (vertices.Count() > 3)
                _d = vertices[3];
            else
                _d = MeshVertex.Unassigned;

            base.Tag = tag;
        }

        public MeshFace(MeshFace face)
        {
            Id = face.Id;
            _a = face._a;
            _b = face._b;
            _c = face._c;
            _d = face._d;
            Tag = face.Tag;
        }

        private MeshFace(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _a = info.GetInt32("A");
            _b = info.GetInt32("B");
            _c = info.GetInt32("C");
            _d = info.GetInt32("D");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Get the nodes ids
        /// </summary>
        /// <param name="reverse">Return the array inverted</param>
        /// <returns>The Ids array</returns>
        public int[] GetNodes(bool reverse = false)
        {
            int[] nodes;
            if (IsQuad)
                nodes = new int[] { A, B, C, D };
            else
                nodes = new int[] { A, B, C };
            if (reverse)
                return nodes.Reverse().ToArray();
            return nodes;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A", _a);
            info.AddValue("B", _b);
            info.AddValue("C", _c);
            info.AddValue("D", _d);
        }

        public bool Equals(MeshFace other)
        {
            return base.Equals(other) && _a == other._a && _b == other._b && _c == other._c && _d == other._d;
        }

        public object Clone()
        {
            return new MeshFace(this);
        }

        /// <summary>
        /// Tells if 2 faces have the same nodes also if the faces don't have the same number of nodes and not in the same order
        /// </summary>
        /// <param name="nodes">The nodes of the face to compare</param>
        /// <returns>True if the nodes matches</returns>
        public bool IsMatch(int[] nodes)
        {
            List<int> bigFace;
            List<int> smallFace;

            int[] tn = GetNodes();

            if (tn.Length >= nodes.Length)
            {
                bigFace = tn.ToList();
                smallFace = new List<int>(nodes);
            }
            else
            {
                bigFace = new List<int>(nodes);
                smallFace = tn.ToList();
            }

            bigFace.Add(bigFace[0]);
            List<int> reversed = new List<int>(smallFace);
            reversed.Reverse();

            bool match = false;
            for (int i = 0; i < bigFace.Count - smallFace.Count + 1; i++)
            {
                List<int> sub = bigFace.GetRange(i, smallFace.Count);
                if (sub.SequenceEqual(smallFace) || sub.SequenceEqual(reversed))
                {
                    match = true;
                    break;
                }
            }

            return match;

        }

        #endregion

        #region Operators overrides

        public static bool operator ==(MeshFace face1, MeshFace face2)
        {
            if (ReferenceEquals(face1, face2))
                return true;
            if (face1 is null || face2 is null)
                return false;
            return face1.Equals(face2);
        }

        public static bool operator !=(MeshFace face1, MeshFace face2)
        {
            return !(face1 == face2);
        }

        #endregion

        #region Public Methods Override

        public override bool Equals(object obj)
        {
            if (obj is MeshFace face)
            {
                return Equals(face);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + _a.GetHashCode();
                hashCode = hashCode * -17 + _b.GetHashCode();
                hashCode = hashCode * -17 + _c.GetHashCode();
                hashCode = hashCode * -17 + _d.GetHashCode();
                return hashCode;
            }
        }

        private string GetDebuggerDisplay()
        {
            return $"Id:{Id}, A:{A}, B:{B}, C:{C}, D:{D}";
        }

        #endregion
    }
}

