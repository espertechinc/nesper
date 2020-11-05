using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace com.espertech.esper.compat.collections
{
    public class DebugDictionary<K,V> : IDictionary<K,V>
    {
        private readonly Guid _id;
        private readonly IDictionary<K, V> _subDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDictionary{K, V}"/> class.
        /// </summary>
        /// <param name="subDictionary">The sub dictionary.</param>
        public DebugDictionary(IDictionary<K, V> subDictionary)
        {
            _id = Guid.NewGuid();
            _subDictionary = subDictionary;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _subDictionary.GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(KeyValuePair<K, V> item)
        {
            Debug.WriteLine(" ~~> A1: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, item.Key.GetHashCode());
            _subDictionary.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            Debug.WriteLine(" ~~> C1: {0} | {1}", _id, Thread.CurrentThread.ManagedThreadId);
            _subDictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<K, V> item)
        {
            return _subDictionary.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            _subDictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(KeyValuePair<K, V> item)
        {
            Debug.WriteLine(" ~~> R1: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, item.Key.GetHashCode());
            return _subDictionary.Remove(item);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => _subDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(K key)
        {
            return _subDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(K key, V value)
        {
            Debug.WriteLine(" ~~> A2: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, key.GetHashCode());
            _subDictionary.Add(key, value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public bool Remove(K key)
        {
            Debug.WriteLine(" ~~> R2: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, key.GetHashCode());
            return _subDictionary.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(K key, out V value)
        {
            Debug.WriteLine(" ~~> G1: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, key.GetHashCode());
            return _subDictionary.TryGetValue(key, out value);
        }

        public V this[K key]
        {
            get
            {
                Debug.WriteLine(" ~~> G2: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, key.GetHashCode());
                return _subDictionary[key];
            }
            set
            {
                Debug.WriteLine(" ~~> S1: {0} | {1} | {2}", _id, Thread.CurrentThread.ManagedThreadId, key.GetHashCode());
                _subDictionary[key] = value;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<V> Values => _subDictionary.Values;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<K> Keys => _subDictionary.Keys;
    }
}
