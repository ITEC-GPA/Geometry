using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// Base of the collections of geometric objects: enumerates the objects of an inner list
    /// </summary>
    /// <typeparam name="T">The type of the objects</typeparam>
    [Serializable]
    public class BaseEnumerable<T> : IEnumerable<T> where T : BaseObject, ISerializable
    {
        /// <summary>
        /// The objects of the collection
        /// </summary>
        protected IList<T> _collection;

        /// <summary>
        /// Enumerates the objects of the collection
        /// </summary>
        /// <returns>The enumerator of the inner list</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the objects of the collection (see <see cref="GetEnumerator()"/>)
        /// </summary>
        /// <returns>The enumerator of the inner list</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }
    }
}