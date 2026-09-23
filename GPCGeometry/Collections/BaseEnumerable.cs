using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public class BaseEnumerable<T> : IEnumerable<T> where T : BaseObject, ISerializable
    {
        protected IList<T> _collection;

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }
    }
}