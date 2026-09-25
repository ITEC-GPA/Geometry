using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshEdge : MeshBase, ISerializable, IEquatable<MeshEdge>, ICloneable
    {
        #region Variables

        private int _a;
        private int _b;

        #endregion

        #region Properties

        public int A => _a;

        public int B => _b;

        #endregion

        #region Public Constructors

        public MeshEdge(int a, int b, object tag = null)
        {
            _a = a;
            _b = b;
            Tag = tag;
        }

        public MeshEdge(MeshEdge edge)
        {
            Id = edge.Id;
            _a = edge._a;
            _b = edge._b;
        }

        private MeshEdge(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _a = info.GetInt32("A");
            _b = info.GetInt32("B");
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A", _a);
            info.AddValue("B", _b);
        }

        public bool Equals(MeshEdge other)
        {
            if (other is null)
                return false;

            return base.Equals(other) && ((_a == other._a && _b == other._b) || (_a == other._b && _b == other._a));
        }

        public bool EqualsWithoutId(MeshEdge other)
        {
            if (other is null)
                return false;

            return (_a == other._a && _b == other._b) || (_a == other._b && _b == other._a);
        }

        internal override bool HasSameContent(MeshBase other)
        {
            return other is MeshEdge edge && EqualsWithoutId(edge);
        }

        public object Clone()
        {
            return new MeshEdge(this);
        }

        #endregion

        #region Operators overrides

        public static bool operator ==(MeshEdge edge1, MeshEdge edge2)
        {
            if (ReferenceEquals(edge1, edge2))
                return true;
            if (edge1 is null || edge2 is null)
                return false;
            return edge1.Equals(edge2);
        }

        public static bool operator !=(MeshEdge edge1, MeshEdge edge2)
        {
            return !(edge1 == edge2);
        }

        #endregion

        #region Public Methods Override

        public override bool Equals(object obj)
        {
            if (obj is MeshEdge edge)
            {
                return Equals(edge);
            }
            return false;
        }

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

        private string GetDebuggerDisplay()
        {
            return $"Id: {Id}, A: {_a}, B: {_b}";
        }

        #endregion
    }
}
