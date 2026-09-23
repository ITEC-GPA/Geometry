using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public abstract class BaseObject : ISerializable
    {
        protected Guid _guid;

        protected object _tag;

        public Guid Guid => _guid;

        public object Tag
		{
            get => _tag;
            set => _tag = value;
		}

        protected BaseObject(Guid guid)
        {
            _guid = guid;
        }

        protected BaseObject(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
        }

        /// <returns> <see langword="true"/> if <paramref name="guid"/> match the object <see cref="Guid"/> </returns>
        public bool CompareGuid(Guid guid)
        {
            return _guid.Equals(guid);
        }

        /// <returns> <see langword="true"/> if <paramref name="baseObject"/> guid match the object <see cref="Guid"/> </returns>
        public bool CompareGuid(BaseObject baseObject)
        {
            return _guid.Equals(baseObject._guid);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
        }
    }
}