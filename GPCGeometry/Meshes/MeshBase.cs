using System;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// Base of the elements of a mesh (vertices, edges, faces, volumes): an integer <see cref="Id"/>, unique in the collection of the mesh
    /// </summary>
    [Serializable]
    public abstract class MeshBase : BaseObject, ISerializable, IEquatable<MeshBase>
    {
        /// <summary>
        /// A tolerance for the mesh elements (1E-4)
        /// </summary>
        protected const double Tolerance = 1E-4;

        /// <summary>
        /// The id of the element in its collection (<see cref="Unset"/> until it is added to a collection)
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The value of <see cref="Id"/> of an element not yet added to a collection
        /// </summary>
        public const int Unset = -1;

        /// <summary>
        /// Creates an element with <see cref="Id"/> = <see cref="Unset"/>
        /// </summary>
        /// <remarks>The Guid is generated only when it is requested (see <see cref="BaseObject.Guid"/>)</remarks>
        protected MeshBase()
            : base()
        {
            Id = Unset;
        }

        /// <summary>
        /// Deserialization constructor: reads the <see cref="BaseObject.Guid"/> and the <see cref="Id"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected MeshBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Id = info.GetInt32("Id");
        }

        /// <summary>
        /// Serializes the <see cref="BaseObject.Guid"/> and the <see cref="Id"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", Id);
        }

        /// <summary>
        /// Equality with another object, defined by the derived classes
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal element</returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// A constant hash code (the derived classes use their content)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return 757;
        }

        /// <summary>
        /// Equality of the ids
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if the elements have the same <see cref="Id"/></returns>
        public bool Equals(MeshBase other)
        {
            return (other != null) && Id == other.Id;
        }

        /// <summary>
        /// True if <paramref name="other"/> has the same content (point, nodes), whatever its Id.
        /// Used by <see cref="MeshBaseCollection{T}.AddUnique(T)"/> after the comparison of the hash codes
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if the contents are equal (always true for the base class)</returns>
        internal virtual bool HasSameContent(MeshBase other)
        {
            return true;
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null elements are equal
        /// </summary>
        /// <param name="obj1">The first element</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are equal</returns>
        public static bool operator ==(MeshBase obj1, MeshBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="obj1">The first element</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are different</returns>
        public static bool operator !=(MeshBase obj1, MeshBase obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
