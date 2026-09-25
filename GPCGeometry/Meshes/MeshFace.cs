using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// A face of a mesh: the ids of its three (triangle) or four (quadrangle) vertices
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshFace : MeshBase, ISerializable, IEquatable<MeshFace>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The id of the first vertex
        /// </summary>
        private int _a;
        /// <summary>
        /// The id of the second vertex
        /// </summary>
        private int _b;
        /// <summary>
        /// The id of the third vertex
        /// </summary>
        private int _c;
        /// <summary>
        /// The id of the fourth vertex; <see cref="MeshVertex.Unassigned"/> for a triangle
        /// </summary>
        private int _d;

        #endregion

        #region Properties

        /// <summary>
        /// The id of the first vertex
        /// </summary>
        public int A => _a;

        /// <summary>
        /// The id of the second vertex
        /// </summary>
        public int B => _b;

        /// <summary>
        /// The id of the third vertex
        /// </summary>
        public int C => _c;

        /// <summary>
        /// The id of the fourth vertex; <see cref="MeshVertex.Unassigned"/> for a triangle
        /// </summary>
        public int D => _d;

        /// <summary>
        /// True if the face has three vertices
        /// </summary>
        public bool IsTriangle => _d == -1;

        /// <summary>
        /// True if the face has four vertices
        /// </summary>
        public bool IsQuad => _d != -1;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a face
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <param name="c">The id of the third vertex</param>
        /// <param name="d">The id of the fourth vertex; <see cref="MeshVertex.Unassigned"/> for a triangle</param>
        /// <param name="tag">The tag of the face</param>
        public MeshFace(int a, int b, int c, int d, object tag = null)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            Tag = tag;
        }

        /// <summary>
        /// Creates a triangular face
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <param name="c">The id of the third vertex</param>
        public MeshFace(int a, int b, int c)
            : this(a, b, c, MeshVertex.Unassigned, null)
        {

        }

        /// <summary>
        /// Creates a triangular or quadrangular face from the ids of its vertices
        /// </summary>
        /// <param name="vertices">The ids of the vertices (3 or 4)</param>
        /// <param name="tag">The tag of the face</param>
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

        /// <summary>
        /// Creates a copy of a face: same id, vertices and tag
        /// </summary>
        /// <param name="face">The face to copy</param>
        public MeshFace(MeshFace face)
        {
            Id = face.Id;
            _a = face._a;
            _b = face._b;
            _c = face._c;
            _d = face._d;
            Tag = face.Tag;
        }

        /// <summary>
        /// Deserialization constructor: reads the id and the vertices
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Serializes the <see cref="BaseObject.Guid"/>, the id and the vertices
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A", _a);
            info.AddValue("B", _b);
            info.AddValue("C", _c);
            info.AddValue("D", _d);
        }

        /// <summary>
        /// Equality of the id and of the vertices, in the same order
        /// </summary>
        /// <param name="other">The face to compare</param>
        /// <returns>True if the faces are equal</returns>
        public bool Equals(MeshFace other)
        {
            return base.Equals(other) && _a == other._a && _b == other._b && _c == other._c && _d == other._d;
        }

        /// <summary>
        /// Equality of the vertices in the same order (see <see cref="MeshBase.HasSameContent(MeshBase)"/>)
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if <paramref name="other"/> is a face with the same vertices</returns>
        internal override bool HasSameContent(MeshBase other)
        {
            return other is MeshFace face && _a == face._a && _b == face._b && _c == face._c && _d == face._d;
        }

        /// <summary>
        /// Creates a copy of the face (see <see cref="MeshFace(MeshFace)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public object Clone()
        {
            return new MeshFace(this);
        }

        /// <summary>
        /// Tells if the nodes of the smaller face (or edge) are consecutive nodes of the larger one, in the same or in the opposite order.
        /// The larger face is closed only once (its first node is repeated at the end): not all the rotations of the nodes are recognized
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(MeshFace)"/>); two null faces are equal
        /// </summary>
        /// <param name="face1">The first face</param>
        /// <param name="face2">The second face</param>
        /// <returns>True if the faces are equal</returns>
        public static bool operator ==(MeshFace face1, MeshFace face2)
        {
            if (ReferenceEquals(face1, face2))
                return true;
            if (face1 is null || face2 is null)
                return false;
            return face1.Equals(face2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(MeshFace)"/>)
        /// </summary>
        /// <param name="face1">The first face</param>
        /// <param name="face2">The second face</param>
        /// <returns>True if the faces are different</returns>
        public static bool operator !=(MeshFace face1, MeshFace face2)
        {
            return !(face1 == face2);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality with another object (see <see cref="Equals(MeshFace)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal face</returns>
        public override bool Equals(object obj)
        {
            if (obj is MeshFace face)
            {
                return Equals(face);
            }
            return false;
        }

        /// <summary>
        /// The hash code of the vertices (dependent on their order)
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>The id and the vertices</returns>
        private string GetDebuggerDisplay()
        {
            return $"Id:{Id}, A:{A}, B:{B}, C:{C}, D:{D}";
        }

        #endregion
    }
}

