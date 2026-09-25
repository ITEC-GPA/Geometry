using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// Base for the mesh objects collection
    /// </summary>
    /// <typeparam name="T">The type of collection derived from MeshBase</typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public class MeshBaseCollection<T> : BaseEnumerable<T>, IEnumerable<T> where T : MeshBase, ISerializable
    {
        private readonly object _locker = new object();

        /// <summary>
        /// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
        /// </summary>
        protected Dictionary<int, int> _ids = new Dictionary<int, int>();

        protected int _maxId = -1;

        [NonSerialized]
        private int _version;

        public virtual int Count => _collection.Count;

        /// <summary>
        /// Incremented at every change of the collection (add, remove, replace, clear).
        /// Used to know if an index built on the collection is still valid
        /// </summary>
        public int Version => _version;

        public virtual bool IsReadOnly => _collection.IsReadOnly;

        public MeshBaseCollection()
        {
            _collection = new List<T>();
        }

        protected MeshBaseCollection(SerializationInfo info, StreamingContext context)
        {
            _ids = (Dictionary<int, int>)info.GetValue("Id", typeof(Dictionary<int, int>));
            _collection = (List<T>)info.GetValue("Collection", typeof(List<T>));
        }

        #region Public method - Setter

        /// <summary>
        /// Add a <typeparamref name="T"/> to the collection.
        /// <para>Object will be added only if not already present</para>
        /// <para>In any case, if the <paramref name="item"/> id already exist in the collection, its ID will be replaced with the collection maximum index + 1</para>
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null </exception>
        /// <remarks>This is an O(n) operation. For faster method refer to <see cref="Build(T)"/></remarks>
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

        public int Add(T item, int id)
        {
            item.Id = id;
            return Add(item);
        }

        public int AddConcurrent(T item)
        {
            lock (_locker)
            {
                return Add(item);
            }
        }

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

        public int AddUnique(T item)
        {
            for (int i = 0; i < _collection.Count; ++i)
            {
                if (_collection.ElementAt(i).GetHashCode() == item.GetHashCode())
                {
                    return _collection.ElementAt(i).Id;
                }
            }
            return Add(item);
        }

        #endregion Public method - Setter

        #region Public method - Getter

        /// <param name="index">The index of the element in the collection</param>
        public virtual T this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                var oldId = _collection[index].Id;
                if (value.Id != oldId)
                {
                    _ids.Remove(oldId);
                    _ids.Add(value.Id, index);
                }
                _collection.RemoveAt(index);
                _collection.Insert(index, value);
                _version++;
            }
        }

        /// Lock and get the element
        /// <param name="index">The index of the element in the collection</param>
        public virtual T GetElementConcurrent(int index)
        {
            lock (_locker)
            {
                return _collection.ElementAt(index);
            }
        }

        /// Get element using its id
        public virtual T GetElementById(int id)
        {
            return _collection[_ids[id]];
        }

        /// Lock and get element using its id
        public virtual T GetElementByIdConcurrent(int id)
        {
            lock (_locker)
            {
                return _collection.ElementAt(_ids[id]);
            }
        }

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

        public virtual T GetElementByIndexConcurrent(int index)
        {
            lock (_locker)
            {
                return this[index];
            }
        }

        /// <summary>
        /// Get the max id of the collection
        /// </summary>
        /// <returns>The id tag</returns>
        public int GetMaxId()
        {
            return _maxId;
        }


        #endregion Public method - Getter

        #region Public method - Check

        /// <summary>
        /// Check if <paramref name="item"/> is contained in the collection
        /// </summary>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Contains(T item)
        {
            return _ids.ContainsKey(item.Id);
        }

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
        /// <remarks>This is an O(1) operation</remarks>
        public virtual bool Contains(int id)
        {
            return _ids.ContainsKey(id);
        }

        public virtual bool ContainsConcurrent(int id)
        {
            lock (_locker)
            {
                return _ids.ContainsKey(id);
            }
        }

        #endregion Public method - Check

        #region Public method - Edit

        /// <inheritdoc cref="List{T}.Remove(T)"/>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Remove(T item)
        {
            return Remove(item.Id);
        }

        public virtual bool RemoveConcurrent(T item)
        {
            lock (_locker)
            {
                return Remove(item);
            }
        }

        /// <inheritdoc cref="List{T}.Remove(T)"/>
        /// <inheritdoc cref="this[int]"/>
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

        public virtual bool RemoveConcurrent(int id)
        {
            lock (_locker)
            {
                return Remove(id);
            }
        }

        private void UpdateFromIndex(int index)
        {
            for (int i = index; i < _collection.Count; i++)
            {
                _ids[_collection[i].Id] = i;
            }
        }

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

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", _ids);
            info.AddValue("Collection", _collection);
        }

        #endregion
    }
}
