///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.collections
{
    public class ObsoleteConcurrentDictionary<K, V> : IDictionary<K, V>
    {
        private readonly IDictionary<K, V> _subDictionary;
        private readonly IReaderWriterLock _rwLock;
        private readonly ILockable _rLock;
        private readonly ILockable _wLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsoleteConcurrentDictionary&lt;K, V&gt;"/> class.
        /// </summary>
        public ObsoleteConcurrentDictionary(IReaderWriterLockManager rwLockManager)
        {
            _subDictionary = new Dictionary<K, V>();
            _rwLock = rwLockManager.CreateLock(GetType());
            _rLock = _rwLock.ReadLock;
            _wLock = _rwLock.WriteLock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsoleteConcurrentDictionary&lt;K, V&gt;" /> class.
        /// </summary>
        /// <param name="subDictionary">The sub dictionary.</param>
        /// <param name="rwLockManager">The rw lock manager.</param>
        public ObsoleteConcurrentDictionary(IDictionary<K, V> subDictionary, IReaderWriterLockManager rwLockManager)
        {
            _subDictionary = subDictionary;
            _rwLock = rwLockManager.CreateLock(GetType());
            _rLock = _rwLock.ReadLock;
            _wLock = _rwLock.WriteLock;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            // Iteration over the dictionary causes a read lock to be acquired
            // for the duration of the life of the enumeration.  Use with care
            // and we really need to measure the impact of this to determine if
            // we need to use a copy on write structure.

            using( _rLock.Acquire() ) {
                foreach( var entry in _subDictionary ) {
                    yield return entry;
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(KeyValuePair<K, V> item)
        {
            using (_wLock.Acquire()) 
            {
                ICollection<KeyValuePair<K, V>> asCollection = _subDictionary;
                asCollection.Add(item);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            using (_wLock.Acquire())
            {
                _subDictionary.Clear();
            }
        }

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        ///                     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        public bool Contains(KeyValuePair<K, V> item)
        {
            using (_rLock.Acquire())
            {
                return _subDictionary.Contains(item);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            using (_rLock.Acquire())
            {
                ICollection<KeyValuePair<K, V>> asCollection = _subDictionary;
                asCollection.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        ///                     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">
        ///                     The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///                 </exception>
        public bool Remove(KeyValuePair<K, V> item)
        {
            using (_wLock.Acquire())
            {
                return _subDictionary.Remove(item.Key);
            }
        }

        /// <summary>
        ///                     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///                     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count
        {
            get
            {
                using (_rLock.Acquire()) {
                    return _subDictionary.Count;
                }
            }
        }

        /// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        public bool ContainsKey(K key)
        {
            using (_rLock.Acquire())
            {
                return _subDictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public void Add(K key, V value)
        {
            using (_wLock.Acquire())
            {
                _subDictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public bool Remove(K key)
        {
            using (_wLock.Acquire())
            {
                return _subDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(K key, out V value)
        {
            using (_rLock.Acquire())
            {
                return _subDictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and <paramref name="key"/> is not found.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public V this[K key]
        {
            get
            {
                using (_rLock.Acquire())
                {
                    return _subDictionary[key];
                }
            }
            set
            {
                using (_wLock.Acquire())
                {
                    _subDictionary[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<K> Keys
        {
            get
            {
                using (_rLock.Acquire())
                {
                    return _subDictionary.Keys;
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<V> Values
        {
            get
            {
                using (_rLock.Acquire())
                {
                    return _subDictionary.Values;
                }
            }
        }
    }
}
