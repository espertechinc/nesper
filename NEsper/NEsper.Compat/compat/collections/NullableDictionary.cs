///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    [Serializable]
    public class NullableDictionary<K,V> : IDictionary<K,V>
        where K : class
    {
        /// <summary>
        /// Underlying dictionary that handles real requests
        /// </summary>
        private readonly IDictionary<K, V> _baseDictionary;

        /// <summary>
        /// Value of the entry at the null key.
        /// </summary>
        private KeyValuePair<K, V>? _nullEntry;

        /// <summary>
        /// Gets the base dictionary.
        /// </summary>
        /// <value>The base dictionary.</value>
        public IDictionary<K, V> BaseDictionary => _baseDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableDictionary{K, V}"/> class.
        /// </summary>
        public NullableDictionary() : this(new Dictionary<K, V>())
        {
        } 

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableDictionary&lt;K, V&gt;"/> class.
        /// </summary>
        /// <param name="baseDictionary">The base dictionary.</param>
        public NullableDictionary(IDictionary<K, V> baseDictionary)
        {
            _baseDictionary = baseDictionary;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_nullEntry != null)
            {
                yield return _nullEntry.Value;
            }

            IEnumerator tempEnum = _baseDictionary.GetEnumerator();
            while (tempEnum.MoveNext())
            {
                yield return tempEnum.Current;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            if (_nullEntry != null)
            {
                yield return _nullEntry.Value;
            }

            IEnumerator<KeyValuePair<K, V>> tempEnum = _baseDictionary.GetEnumerator();
            while (tempEnum.MoveNext())
            {
                yield return tempEnum.Current;
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
            if (item.Key == null)
            {
                _nullEntry = item;
            }
            else
            {
                _baseDictionary.Add(item);
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
            _baseDictionary.Clear();
            _nullEntry = null;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<K, V> item)
        {
            if (item.Key == null)
                return _nullEntry != null;
            return _baseDictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (_nullEntry == null)
            {
                _baseDictionary.CopyTo(array, arrayIndex);
            }
            else
            {
                array[0] = _nullEntry.Value;
                _baseDictionary.CopyTo(array, arrayIndex + 1);
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
            if ( item.Key == null )
            {
                if ( _nullEntry != null )
                {
                    _nullEntry = null;
                    return true;
                }

                return false;
            }

            return _baseDictionary.Remove(item);
        }

        /// <summary>
        /// Gets the count for the null entry.
        /// </summary>
        /// <value>The null entry count.</value>
        public int NullEntryCount => _nullEntry != null ? 1 : 0;

        /// <summary>
        ///                     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///                     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count => _baseDictionary.Count + NullEntryCount;

        /// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => _baseDictionary.IsReadOnly;

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool ContainsKey(K key)
        {
            if (key == null)
            {
                return _nullEntry != null;
            }

            return _baseDictionary.ContainsKey(key);
        }

        /// <summary>
        ///                     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">
        ///                     The object to use as the key of the element to add.
        ///                 </param>
        /// <param name="value">
        ///                     The object to use as the value of the element to add.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.ArgumentException">
        ///                     An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public void Add(K key, V value)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    throw new ArgumentException("An element with the same key already exists");
                }

                _nullEntry = new KeyValuePair<K, V>(null, value);
            }
            else
            {
                _baseDictionary.Add(key, value);
            }
        }

        /// <summary>
        ///                     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to remove.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public bool Remove(K key)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    _nullEntry = null;
                    return true;
                }

                return false;
            }
            else
            {
                return _baseDictionary.Remove(key);
            }
        }

        /// <summary>
        ///                     Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key whose value to get.
        ///                 </param>
        /// <param name="value">
        ///                     When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool TryGetValue(K key, out V value)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    value = _nullEntry.Value.Value;
                    return true;
                }

                value = default(V);
                return false;
            }
            else
            {
                return _baseDictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        ///                     Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        ///                     The element with the specified key.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to get or set.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        ///                     The property is retrieved and <paramref name="key" /> is not found.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public V this[K key]
        {
            get
            {
                if (key == null)
                {
                    if (_nullEntry != null)
                    {
                        return _nullEntry.Value.Value;
                    }

                    throw new KeyNotFoundException();
                }
                else
                {
                    return _baseDictionary[key];
                }
            }

            set
            {
                if (key == null)
                {
                    _nullEntry = new KeyValuePair<K, V>(null, value);
                }
                else
                {
                    _baseDictionary[key] = value;
                }
            }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<K> Keys
        {
            get
            {
                if (_nullEntry == null)
                {
                    return _baseDictionary.Keys;
                }
                else
                {
                    return new CollectionPlus<K>(_baseDictionary.Keys, _nullEntry.Value.Key);
                }
            }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<V> Values
        {
            get
            {
                if (_nullEntry == null)
                {
                    return _baseDictionary.Values;
                }
                else
                {
                    return new CollectionPlus<V>(_baseDictionary.Values, _nullEntry.Value.Value);
                }
            }
        }
    }



    public class NullableValueTypeDictionary<K, V> : IDictionary<K?, V>
        where K : struct
    {
        /// <summary>
        /// Underlying dictionary that handles real requests
        /// </summary>
        private readonly IDictionary<K?, V> _baseDictionary;

        /// <summary>
        /// Value of the entry at the null key.
        /// </summary>
        private KeyValuePair<K?, V>? _nullEntry;

        /// <summary>
        /// Gets the base dictionary.
        /// </summary>
        /// <value>The base dictionary.</value>
        public IDictionary<K?, V> BaseDictionary => _baseDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableValueTypeDictionary&lt;K, V&gt;"/> class.
        /// </summary>
        /// <param name="baseDictionary">The base dictionary.</param>
        public NullableValueTypeDictionary(IDictionary<K?, V> baseDictionary)
        {
            _baseDictionary = baseDictionary;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_nullEntry != null)
            {
                yield return _nullEntry.Value;
            }

            IEnumerator tempEnum = _baseDictionary.GetEnumerator();
            while (tempEnum.MoveNext())
            {
                yield return tempEnum.Current;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K?, V>> GetEnumerator()
        {
            if (_nullEntry != null)
            {
                yield return _nullEntry.Value;
            }

            var tempEnum = _baseDictionary.GetEnumerator();
            while (tempEnum.MoveNext())
            {
                yield return tempEnum.Current;
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(KeyValuePair<K?, V> item)
        {
            if (item.Key == null)
            {
                _nullEntry = item;
            }
            else
            {
                _baseDictionary.Add(item);
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
            _baseDictionary.Clear();
            _nullEntry = null;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<K?, V> item)
        {
            if (item.Key == null)
                return _nullEntry != null;
            return _baseDictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<K?, V>[] array, int arrayIndex)
        {
            if (_nullEntry == null)
            {
                _baseDictionary.CopyTo(array, arrayIndex);
            }
            else
            {
                array[0] = _nullEntry.Value;
                _baseDictionary.CopyTo(array, arrayIndex + 1);
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
        public bool Remove(KeyValuePair<K?, V> item)
        {
            if (item.Key == null)
            {
                if (_nullEntry != null)
                {
                    _nullEntry = null;
                    return true;
                }

                return false;
            }

            return _baseDictionary.Remove(item);
        }

        /// <summary>
        /// Gets the count for the null entry.
        /// </summary>
        /// <value>The null entry count.</value>
        public int NullEntryCount => _nullEntry != null ? 1 : 0;

        /// <summary>
        ///                     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///                     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count => _baseDictionary.Count + NullEntryCount;

        /// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => _baseDictionary.IsReadOnly;

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool ContainsKey(K? key)
        {
            if (key == null)
            {
                return _nullEntry != null;
            }

            return _baseDictionary.ContainsKey(key);
        }

        /// <summary>
        ///                     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">
        ///                     The object to use as the key of the element to add.
        ///                 </param>
        /// <param name="value">
        ///                     The object to use as the value of the element to add.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.ArgumentException">
        ///                     An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public void Add(K? key, V value)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    throw new ArgumentException("An element with the same key already exists");
                }

                _nullEntry = new KeyValuePair<K?, V>(null, value);
            }
            else
            {
                _baseDictionary.Add(key, value);
            }
        }

        /// <summary>
        ///                     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to remove.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public bool Remove(K? key)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    _nullEntry = null;
                    return true;
                }

                return false;
            }
            else
            {
                return _baseDictionary.Remove(key);
            }
        }

        /// <summary>
        ///                     Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key whose value to get.
        ///                 </param>
        /// <param name="value">
        ///                     When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool TryGetValue(K? key, out V value)
        {
            if (key == null)
            {
                if (_nullEntry != null)
                {
                    value = _nullEntry.Value.Value;
                    return true;
                }

                value = default(V);
                return false;
            }
            else
            {
                return _baseDictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        ///                     Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        ///                     The element with the specified key.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to get or set.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        ///                     The property is retrieved and <paramref name="key" /> is not found.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public V this[K? key]
        {
            get
            {
                if (key == null)
                {
                    if (_nullEntry != null)
                    {
                        return _nullEntry.Value.Value;
                    }

                    throw new KeyNotFoundException();
                }
                else
                {
                    return _baseDictionary[key];
                }
            }

            set
            {
                if (key == null)
                {
                    _nullEntry = new KeyValuePair<K?, V>(null, value);
                }
                else
                {
                    _baseDictionary[key] = value;
                }
            }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<K?> Keys
        {
            get
            {
                if (_nullEntry == null)
                {
                    return _baseDictionary.Keys;
                }
                else
                {
                    return new CollectionPlus<K?>(_baseDictionary.Keys, _nullEntry.Value.Key);
                }
            }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<V> Values
        {
            get
            {
                if (_nullEntry == null)
                {
                    return _baseDictionary.Values;
                }
                else
                {
                    return new CollectionPlus<V>(_baseDictionary.Values, _nullEntry.Value.Value);
                }
            }
        }
    }

}
