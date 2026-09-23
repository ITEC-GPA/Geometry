using System;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [Serializable]
    public abstract class MeshBase : BaseObject, ISerializable, IEquatable<MeshBase>
    {
        protected const double Tolerance = 1E-4;

        public int Id { get; internal set; }

        public const int Unset = -1;

        protected MeshBase()
            : base(Guid.NewGuid())
        {
            Id = Unset;
        }

        protected MeshBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Id = info.GetInt32("Id");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", Id);
        }

        public abstract override bool Equals(object obj);

        public override int GetHashCode()
        {
            return 757;
        }

        public bool Equals(MeshBase other)
        {
            return (other != null) && Id == other.Id;
        }

        public static bool operator ==(MeshBase obj1, MeshBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(MeshBase obj1, MeshBase obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
