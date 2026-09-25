using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// An edge of a mesh: the ids of its two vertices
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshEdge : MeshBase, ISerializable, IEquatable<MeshEdge>, ICloneable
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

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an edge
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <param name="tag">The tag of the edge</param>
        public MeshEdge(int a, int b, object tag = null)
        {
            _a = a;
            _b = b;
            Tag = tag;
        }

        /// <summary>
        /// Creates a copy of an edge: same id and vertices (the tag is not copied)
        /// </summary>
        /// <param name="edge">The edge to copy</param>
        public MeshEdge(MeshEdge edge)
        {
            Id = edge.Id;
            _a = edge._a;
            _b = edge._b;
        }

        /// <summary>
        /// Deserialization constructor: reads the id and the vertices
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private MeshEdge(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _a = info.GetInt32("A");
            _b = info.GetInt32("B");
        }

        #endregion

        #region Public Methods Specific

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
        }

        /// <summary>
        /// Equality of the id and of the vertices, in either direction
        /// </summary>
        /// <param name="other">The edge to compare</param>
        /// <returns>True if the edges are equal</returns>
        public bool Equals(MeshEdge other)
        {
            if (other is null)
                return false;

            return base.Equals(other) && ((_a == other._a && _b == other._b) || (_a == other._b && _b == other._a));
        }

        /// <summary>
        /// Equality of the vertices, in either direction, whatever the id
        /// </summary>
        /// <param name="other">The edge to compare</param>
        /// <returns>True if the edges join the same vertices</returns>
        public bool EqualsWithoutId(MeshEdge other)
        {
            if (other is null)
                return false;

            return (_a == other._a && _b == other._b) || (_a == other._b && _b == other._a);
        }

        /// <summary>
        /// Equality of the vertices (see <see cref="EqualsWithoutId(MeshEdge)"/>)
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if <paramref name="other"/> is an edge with the same vertices</returns>
        internal override bool HasSameContent(MeshBase other)
        {
            return other is MeshEdge edge && EqualsWithoutId(edge);
        }

        /// <summary>
        /// Creates a copy of the edge (see <see cref="MeshEdge(MeshEdge)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public object Clone()
        {
            return new MeshEdge(this);
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// Equality operator (see <see cref="Equals(MeshEdge)"/>); two null edges are equal
        /// </summary>
        /// <param name="edge1">The first edge</param>
        /// <param name="edge2">The second edge</param>
        /// <returns>True if the edges are equal</returns>
        public static bool operator ==(MeshEdge edge1, MeshEdge edge2)
        {
            if (ReferenceEquals(edge1, edge2))
                return true;
            if (edge1 is null || edge2 is null)
                return false;
            return edge1.Equals(edge2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(MeshEdge)"/>)
        /// </summary>
        /// <param name="edge1">The first edge</param>
        /// <param name="edge2">The second edge</param>
        /// <returns>True if the edges are different</returns>
        public static bool operator !=(MeshEdge edge1, MeshEdge edge2)
        {
            return !(edge1 == edge2);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality with another object (see <see cref="Equals(MeshEdge)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal edge</returns>
        public override bool Equals(object obj)
        {
            if (obj is MeshEdge edge)
            {
                return Equals(edge);
            }
            return false;
        }

        /// <summary>
        /// The hash code of the vertices, independent of their order
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 1153724943;
                hashCode *= -1521134295 + _a.GetHashCode();
                hashCode *= -1521134295 + _b.GetHashCode();

                int hashCode2 = 1153724943;
                hashCode2 *= -1521134295 + _b.GetHashCode();
                hashCode2 *= -1521134295 + _a.GetHashCode();


                return hashCode + hashCode2;
            }
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>The id and the vertices</returns>
        private string GetDebuggerDisplay()
        {
            return $"Id: {Id}, A: {_a}, B: {_b}";
        }

        #endregion
    }
}
