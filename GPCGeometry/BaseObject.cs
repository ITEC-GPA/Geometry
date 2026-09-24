using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public abstract class BaseObject : ISerializable
    {
        private static readonly object GuidLock = new object();

        // The Guid is generated only when it is requested (Guid, CompareGuid, serialization): Guid.NewGuid() costs
        // about 40 ns, ten times the creation of a point, and the Guid of the geometries is rarely used.
        // _hasGuid is written after _guid (volatile), so a thread that reads _hasGuid == true reads the whole Guid
        private Guid _guid;
        private volatile bool _hasGuid;

        protected object _tag;

        /// <summary>
        /// The unique identifier of the object. It is generated at the first request, if it was not given to the constructor
        /// </summary>
        public Guid Guid
        {
            get
            {
                if (!_hasGuid)
                {
                    lock (GuidLock)
                    {
                        if (!_hasGuid)
                        {
                            _guid = System.Guid.NewGuid();
                            _hasGuid = true;
                        }
                    }
                }

                return _guid;
            }
        }

        public object Tag
		{
            get => _tag;
            set => _tag = value;
		}

        /// <summary>
        /// The <see cref="Guid"/> is generated when it is requested
        /// </summary>
        protected BaseObject()
        {
        }

        protected BaseObject(Guid guid)
        {
            SetGuid(guid);
        }

        protected BaseObject(SerializationInfo info, StreamingContext context)
        {
            SetGuid((Guid)info.GetValue("Guid", typeof(Guid)));
        }

        /// <summary>
        /// Set the <see cref="Guid"/> of the object (e.g. to keep the identity of a copy)
        /// </summary>
        protected void SetGuid(Guid guid)
        {
            _guid = guid;
            _hasGuid = true;
        }

        /// <returns> <see langword="true"/> if <paramref name="guid"/> match the object <see cref="Guid"/> </returns>
        public bool CompareGuid(Guid guid)
        {
            return this.Guid.Equals(guid);
        }

        /// <returns> <see langword="true"/> if <paramref name="baseObject"/> guid match the object <see cref="Guid"/> </returns>
        public bool CompareGuid(BaseObject baseObject)
        {
            return this.Guid.Equals(baseObject.Guid);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", this.Guid);
        }
    }
}