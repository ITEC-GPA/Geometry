using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// Base for the mesh objects collection: a list of elements with unique ids and a map from the id to the position in the list
    /// </summary>
    /// <typeparam name="T">The type of collection derived from MeshBase</typeparam>
    /// <remarks>Only the methods "Concurrent", <see cref="Clear"/> and the maps take a lock: the other methods are not thread-safe</remarks>
    [Serializable]
    public class MeshBaseCollection<T> : BaseEnumerable<T>, IEnumerable<T> where T : MeshBase, ISerializable
    {
        /// <summary>
        /// The lock of the "Concurrent" methods
        /// </summary>
        private readonly object _locker = new object();

        /// <summary>
        /// The map from the id of an element to its position in the list (the position can change when elements are removed)
        /// </summary>
        protected Dictionary<int, int> _ids = new Dictionary<int, int>();

        /// <summary>
        /// The largest id (decreased by one when the element with the largest id is removed: an upper bound of the ids)
        /// </summary>
        protected int _maxId = -1;

        /// <summary>
        /// The counter of the changes (see <see cref="Version"/>)
        /// </summary>
        [NonSerialized]
        private int _version;

        /// <summary>
        /// The number of elements
        /// </summary>
        public virtual int Count => _collection.Count;

        /// <summary>
        /// Incremented at every change of the collection (add, remove, replace, clear).
        /// Used to know if an index built on the collection is still valid
        /// </summary>
        public int Version => _version;

        /// <summary>
        /// True if the inner list is read only
        /// </summary>
        public virtual bool IsReadOnly => _collection.IsReadOnly;

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public MeshBaseCollection()
        {
            _collection = new List<T>();
        }

        /// <summary>
        /// Deserialization constructor: reads the id map and the list (the largest id is not restored)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected MeshBaseCollection(SerializationInfo info, StreamingContext context)
        {
            _ids = (Dictionary<int, int>)info.GetValue("Id", typeof(Dictionary<int, int>));
            _collection = (List<T>)info.GetValue("Collection", typeof(List<T>));
        }

        #region Public method - Setter

        /// <summary>
        /// Add a <typeparamref name="T"/> at the end of the collection (the duplicates are not checked, see <see cref="AddUnique(T)"/>).
        /// <para>If the id of <paramref name="item"/> is <see cref="MeshBase.Unset"/>, it becomes the largest id + 1; otherwise it is kept (if an
        /// element with the same id exists, the id map points to the new one)</para>
        /// </summary>
        /// <param name="item">The element to add (the instance is kept)</param>
        /// <returns>The Id of the item</returns>
        /// <exception cref="NullReferenceException">If <paramref name="item"/> is null</exception>
        public int Add(T item)
        {
            //if (_ids.ContainsKey(item.Id))
            //{
            //    return item.Id;
            //}

            if (item.Id == MeshBase.Unset)
            {
                item.Id = _maxId + 1;
            }

            _collection.Add(item);
            _ids[item.Id] = _collection.Count - 1;
            _maxId = Math.Max(item.Id, _maxId);
            _version++;
            return item.Id;
        }

        /// <summary>
        /// Sets the id of an element and adds it (see <see cref="Add(T)"/>)
        /// </summary>
        /// <param name="item">The element to add</param>
        /// <param name="id">The id of the element (<see cref="MeshBase.Unset"/>: the largest id + 1)</param>
        /// <returns>The Id of the item</returns>
        public int Add(T item, int id)
        {
            item.Id = id;
            return Add(item);
        }

        /// <summary>
        /// Adds an element with the lock (see <see cref="Add(T)"/>)
        /// </summary>
        /// <param name="item">The element to add</param>
        /// <returns>The Id of the item</returns>
        public int AddConcurrent(T item)
        {
            lock (_locker)
            {
                return Add(item);
            }
        }

        /// <summary>
        /// Adds elements (see <see cref="Add(T)"/>)
        /// </summary>
        /// <param name="items">The elements to add</param>
        /// <returns>The ids of the elements</returns>
        /// <remarks>The items are enumerated once (before, Count() and ElementAt(i) at every step: O(n^2), and a lazy enumerable created new objects every time)</remarks>
        public int[] AddRange(IEnumerable<T> items)
        {
            IList<T> list = items as IList<T> ?? items.ToList();
            int[] ids = new int[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                ids[i] = Add(list[i]);
            }
            return ids;
        }

        /// <summary>
        /// Sets the ids of elements and adds them (see <see cref="Add(T)"/>)
        /// </summary>
        /// <param name="items">The elements to add</param>
        /// <param name="indices">The ids of the elements (at least as many as the elements)</param>
        /// <returns>The ids of the elements</returns>
        public int[] AddRange(IEnumerable<T> items, IEnumerable<int> indices)
        {
            IList<T> list = items as IList<T> ?? items.ToList();
            IList<int> idList = indices as IList<int> ?? indices.ToList();
            int[] ids = new int[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].Id = idList[i];
                ids[i] = Add(list[i]);
            }
            return ids;
        }

        /// <summary>
        /// Add <paramref name="item"/> only if the collection has no element with the same content (point, nodes). This is an O(n) operation
        /// </summary>
        /// <param name="item">The element to add</param>
        /// <returns>The Id of the existing element or of the added one</returns>
        /// <remarks>Before, only the hash codes were compared: two different elements with the same hash code were merged</remarks>
        public int AddUnique(T item)
        {
            int hash = item.GetHashCode();
            for (int i = 0; i < _collection.Count; ++i)
            {
                T element = _collection[i];
                if (element.GetHashCode() == hash && element.HasSameContent(item))
                    return element.Id;
            }
            return Add(item);
        }

        #endregion Public method - Setter

        #region Public method - Getter

        /// <summary>
        /// The element at a position of the list. The setter replaces the element and updates the id map
        /// </summary>
        /// <param name="index">The index of the element in the collection</param>
        /// <returns>The element</returns>
        public virtual T this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                var oldId = _collection[index].Id;
                int newId = value.Id == MeshBase.Unset ? _maxId + 1 : value.Id;
                if (newId != oldId && _ids.ContainsKey(newId))
                    throw new ArgumentException("The id is already present in the collection", nameof(value));
                value.Id = newId;
                if (value.Id != oldId)
                {
                    _ids.Remove(oldId);
                    _ids.Add(value.Id, index);
                }
                _collection[index] = value;
                _maxId = Math.Max(_maxId, newId);
                _version++;
            }
        }

        /// <summary>
        /// Lock and get the element at a position of the list
        /// </summary>
        /// <param name="index">The index of the element in the collection</param>
        /// <returns>The element</returns>
        public virtual T GetElementConcurrent(int index)
        {
            lock (_locker)
            {
                return _collection.ElementAt(index);
            }
        }

        /// <summary>
        /// Get element using its id
        /// </summary>
        /// <param name="id">The id of the element</param>
        /// <returns>The element</returns>
        /// <exception cref="KeyNotFoundException">If no element has the id</exception>
        public virtual T GetElementById(int id)
        {
            return _collection[_ids[id]];
        }

        /// <summary>
        /// Lock and get element using its id
        /// </summary>
        /// <param name="id">The id of the element</param>
        /// <returns>The element</returns>
        /// <exception cref="KeyNotFoundException">If no element has the id</exception>
        public virtual T GetElementByIdConcurrent(int id)
        {
            lock (_locker)
            {
                return _collection.ElementAt(_ids[id]);
            }
        }

        /// <summary>
        /// Groups the positions of the elements by hash code
        /// </summary>
        /// <returns>A map between <typeparamref name="T"/> HashCode and the index of <typeparamref name="T"/> in the <see cref="BaseEnumerable{T}._collection"/> </returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual Dictionary<int, List<int>> GetElementHashMap()
        {
            Dictionary<int, List<int>> hashMap = new Dictionary<int, List<int>>();
            lock (_locker)
            {
                var list = (List<T>)_collection;
                for (int i = 0; i < list.Count; i++)
                {
                    var hash = list[i].GetHashCode();

                    if (hashMap.ContainsKey(hash))
                    {
                        hashMap[hash].Add(i);
                    }
                    else
                    {
                        hashMap.Add(hash, new List<int> { i });
                    }
                }
            }

            return hashMap;
        }

        /// <summary>
        /// Groups the positions of the elements by id
        /// </summary>
        /// <returns>A map between <typeparamref name="T"/>.Id  and the index of <typeparamref name="T"/> in the <see cref="BaseEnumerable{T}._collection"/> </returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual Dictionary<int, List<int>> GetElementIdMap()
        {
            Dictionary<int, List<int>> hashMap = new Dictionary<int, List<int>>();
            lock (_locker)
            {
                var list = (List<T>)_collection;
                for (int i = 0; i < list.Count; i++)
                {
                    if (hashMap.ContainsKey(list[i].Id))
                    {
                        hashMap[list[i].Id].Add(i);
                    }
                    else
                    {
                        hashMap.Add(list[i].Id, new List<int> { i });
                    }
                }
            }

            return hashMap;
        }

        /// <summary>
        /// Get the element by its position on the <see cref="BaseEnumerable{T}._collection"/>
        /// </summary>
        /// <param name="index">The index of <typeparamref name="T"/> in the <see cref="BaseEnumerable{T}._collection"/>
        /// <para>This is different from the ID of <typeparamref name="T"/></para>
        /// </param>
        /// <returns><typeparamref name="T"/></returns>
        /// <remarks>This method should be used along with <see cref="GetElementHashMap()"/>
        /// <para>This is an O(1) operation</para></remarks>
        public virtual T GetElementByIndex(int index)
        {
            return this[index];
        }

        /// <summary>
        /// Lock and get the element by its position (see <see cref="GetElementByIndex(int)"/>)
        /// </summary>
        /// <param name="index">The index of the element in the collection</param>
        /// <returns>The element</returns>
        public virtual T GetElementByIndexConcurrent(int index)
        {
            lock (_locker)
            {
                return this[index];
            }
        }

        /// <summary>
        /// Get the max id of the collection (an upper bound of the ids, see <see cref="_maxId"/>); -1 for an empty collection
        /// </summary>
        /// <returns>The id tag</returns>
        public int GetMaxId()
        {
            return _maxId;
        }


        #endregion Public method - Getter

        #region Public method - Check

        /// <summary>
        /// Check if the collection has an element with the id of <paramref name="item"/>
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>True if the id is present</returns>
        /// <remarks>This is an O(1) operation</remarks>
        public virtual bool Contains(T item)
        {
            return _ids.ContainsKey(item.Id);
        }

        /// <summary>
        /// Lock and check if the collection has an element with the id of <paramref name="item"/>
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>True if the id is present</returns>
        public virtual bool ContainsConcurrent(T item)
        {
            lock (_locker)
            {
                return _ids.ContainsKey(item.Id);
            }
        }

        /// <summary>
        /// Check if already exist an <see cref="MeshBase"/> element with id equal to <paramref name="id"/>
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>True if the id is present</returns>
        /// <remarks>This is an O(1) operation</remarks>
        public virtual bool Contains(int id)
        {
            return _ids.ContainsKey(id);
        }

        /// <summary>
        /// Lock and check if already exist an element with id equal to <paramref name="id"/>
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>True if the id is present</returns>
        public virtual bool ContainsConcurrent(int id)
        {
            lock (_locker)
            {
                return _ids.ContainsKey(id);
            }
        }

        #endregion Public method - Check

        #region Public method - Edit

        /// <summary>
        /// Removes the element with the id of <paramref name="item"/> (see <see cref="Remove(int)"/>)
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>True if the element was removed, false if the id is not present</returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Remove(T item)
        {
            return Remove(item.Id);
        }

        /// <summary>
        /// Lock and remove the element with the id of <paramref name="item"/>
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>True if the element was removed, false if the id is not present</returns>
        public virtual bool RemoveConcurrent(T item)
        {
            lock (_locker)
            {
                return Remove(item);
            }
        }

        /// <summary>
        /// Removes the element with an id; the positions of the following elements decrease by one
        /// </summary>
        /// <param name="id">The id of the element</param>
        /// <returns>True if the element was removed, false if the id is not present</returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Remove(int id)
        {
            if (!_ids.ContainsKey(id))
            {
                return false;
            }
            var pos = _ids[id];
            _collection.RemoveAt(pos);
            _ids.Remove(id);
            UpdateFromIndex(pos);
            if (_maxId == id)
            {
                _maxId--;
            }
            _version++;
            return true;
        }

        /// <summary>
        /// Removes the element at a position; the positions of the following elements decrease by one
        /// </summary>
        /// <param name="index">The position of the element</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is out of range</exception>
        public virtual bool RemoveAt(int index)
        {
            var item = _collection.ElementAt(index);
            _collection.RemoveAt(index);
            _ids.Remove(item.Id);
            UpdateFromIndex(index);
            if (_maxId == item.Id)
            {
                _maxId--;
            }
            _version++;
            return true;
        }

        /// <summary>
        /// Lock and remove the element with an id (see <see cref="Remove(int)"/>)
        /// </summary>
        /// <param name="id">The id of the element</param>
        /// <returns>True if the element was removed, false if the id is not present</returns>
        public virtual bool RemoveConcurrent(int id)
        {
            lock (_locker)
            {
                return Remove(id);
            }
        }

        /// <summary>
        /// Updates the id map from a position to the end of the list (after a removal)
        /// </summary>
        /// <param name="index">The first position to update</param>
        private void UpdateFromIndex(int index)
        {
            for (int i = index; i < _collection.Count; i++)
            {
                _ids[_collection[i].Id] = i;
            }
        }

        /// <summary>
        /// Removes all the elements (with the lock); the largest id becomes -1
        /// </summary>
        public virtual void Clear()
        {
            lock (_locker)
            {
                _ids.Clear();
                _collection.Clear();
                _maxId = -1;
                _version++;
            }
        }

        #endregion Public method - Edit

        #region Equals - HashCode - Serialization

        /// <summary>
        /// Serializes the id map and the list
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", _ids);
            info.AddValue("Collection", _collection);
        }

        #endregion
    }
}
