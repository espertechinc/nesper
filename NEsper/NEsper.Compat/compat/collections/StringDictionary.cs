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
    public interface IStringDictionary<V> : IDictionary<String, V>
    {
        /// <summary>
        /// Gets a value indicating whether the keys for the dictionary
        /// are case sensitive.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is case sensitive; otherwise, <c>false</c>.
        /// </value>
        bool IsCaseSensitive { get; }
    }

    public class StringDictionary<V> : IStringDictionary<V>
    {
        private readonly Func<string, string> _normalizeKey;
        private readonly IDictionary<string, V> _subDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDictionary{V}"/> class.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [is case sensitive].</param>
        public StringDictionary(bool isCaseSensitive)
        {
            _subDictionary = new OrderedDictionary<string, V>();
            IsCaseSensitive = isCaseSensitive;
            if (isCaseSensitive) {
                _normalizeKey = s => s;
            }
            else {
                _normalizeKey = s => s.ToUpper();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDictionary&lt;V&gt;"/> class.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [is case sensitive].</param>
        /// <param name="sourceDictionary">The source dictionary.</param>
        public StringDictionary(bool isCaseSensitive, IEnumerable<KeyValuePair<string, V>> sourceDictionary)
            : this(isCaseSensitive)
        {
            foreach( var entry in sourceDictionary )
            {
                this[entry.Key] = entry.Value;
            }
        }

        #region Implementation of IEnumerable

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

        #endregion

        #region Implementation of IEnumerable<KeyValuePair<string,V>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            return _subDictionary.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<string,V>>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(KeyValuePair<string, V> item)
        {
            var temp = new KeyValuePair<string, V>(
                _normalizeKey(item.Key), item.Value);
            _subDictionary.Add(temp);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            _subDictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<string, V> item)
        {
            var temp = new KeyValuePair<string, V>(
                _normalizeKey(item.Key), item.Value);
            return _subDictionary.Contains(temp);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void CopyTo(KeyValuePair<string, V>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(KeyValuePair<string, V> item)
        {
            var temp = new KeyValuePair<string, V>(
                _normalizeKey(item.Key), item.Value);
            return _subDictionary.Remove(temp);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count => _subDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => _subDictionary.IsReadOnly;

        #endregion

        #region Implementation of IDictionary<string,V>

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        public bool ContainsKey(string key)
        {
            return _subDictionary.ContainsKey(_normalizeKey(key));
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
        public void Add(string key, V value)
        {
            _subDictionary.Add(_normalizeKey(key), value);
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
        public bool Remove(string key)
        {
            return _subDictionary.Remove(_normalizeKey(key));
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
        public bool TryGetValue(string key, out V value)
        {
            return _subDictionary.TryGetValue(_normalizeKey(key), out value);
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
        public V this[string key]
        {
            get => _subDictionary[_normalizeKey(key)];
            set { _subDictionary[_normalizeKey(key)] = value; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<string> Keys => _subDictionary.Keys;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<V> Values => _subDictionary.Values;

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether the keys for the dictionary
        /// are case sensitive.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is case sensitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsCaseSensitive { get; private set; }
    }
}
