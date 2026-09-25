using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// Base class of the geometric objects: a unique identifier (<see cref="Guid"/>), generated when it is first requested, and a free
    /// <see cref="Tag"/> for the user data
    /// </summary>
    [Serializable]
    public abstract class BaseObject : ISerializable
    {
        /// <summary>
        /// The lock of the generation of the Guid
        /// </summary>
        private static readonly object GuidLock = new object();

        /// <summary>
        /// The Guid, valid when <see cref="_hasGuid"/> is true. It is generated only when it is requested (Guid, CompareGuid, serialization):
        /// Guid.NewGuid() costs about 40 ns, ten times the creation of a point, and the Guid of the geometries is rarely used
        /// </summary>
        private Guid _guid;
        /// <summary>
        /// True when <see cref="_guid"/> has been generated or read. It is written after _guid (volatile), so a thread that reads true reads the whole Guid
        /// </summary>
        private volatile bool _hasGuid;

        /// <summary>
        /// The user data of <see cref="Tag"/>
        /// </summary>
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

        /// <summary>
        /// Any user data attached to the object. It is not serialized; the mesh operations (copy, refinement, cut, join) keep the tag of the
        /// mesh elements
        /// </summary>
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

        /// <summary>
        /// Creates the object with the given identifier (e.g. to keep the identity of a copy)
        /// </summary>
        /// <param name="guid">The identifier of the object</param>
        protected BaseObject(Guid guid)
        {
            SetGuid(guid);
        }

        /// <summary>
        /// Deserialization constructor: reads the saved <see cref="Guid"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BaseObject(SerializationInfo info, StreamingContext context)
        {
            SetGuid((Guid)info.GetValue("Guid", typeof(Guid)));
        }

        /// <summary>
        /// Sets the <see cref="Guid"/> of the object (e.g. to keep the identity of a copy)
        /// </summary>
        /// <param name="guid">The identifier to assign</param>
        protected void SetGuid(Guid guid)
        {
            _guid = guid;
            _hasGuid = true;
        }

        /// <summary>
        /// Compares the identifier of the object with <paramref name="guid"/>
        /// </summary>
        /// <param name="guid">The identifier to compare</param>
        /// <returns><see langword="true"/> if <paramref name="guid"/> is the <see cref="Guid"/> of the object</returns>
        public bool CompareGuid(Guid guid)
        {
            return this.Guid.Equals(guid);
        }

        /// <summary>
        /// Compares the identifier of the object with the one of <paramref name="baseObject"/>
        /// </summary>
        /// <param name="baseObject">The object to compare</param>
        /// <returns><see langword="true"/> if the two objects have the same <see cref="Guid"/></returns>
        public bool CompareGuid(BaseObject baseObject)
        {
            return this.Guid.Equals(baseObject.Guid);
        }

        /// <summary>
        /// Serializes the object: writes the <see cref="Guid"/> (generating it if it was never requested)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", this.Guid);
        }
    }
}