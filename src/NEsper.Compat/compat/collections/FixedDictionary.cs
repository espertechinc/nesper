///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class FixedDictionary<TK,TV> : IDictionary<TK,TV>
    {
        private readonly FixedDictionarySchema<TK> _dictionarySchema;
        private readonly Entry[] _dataList;
        private int _dataCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedDictionary{K,V}"/> class.
        /// </summary>
        /// <param name="dictionarySchema">The schema.</param>
        public FixedDictionary(FixedDictionarySchema<TK> dictionarySchema)
        {
            _dictionarySchema = dictionarySchema;
            _dataList = new Entry[_dictionarySchema.Count];
            _dataCount = 0;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<KeyValuePair<TK, TV>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            foreach( var key in _dictionarySchema.Keys ) {
                yield return new KeyValuePair<TK, TV>(key, this[key]);
            }
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<TK, TV>>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(KeyValuePair<TK, TV> item)
        {
            int index;
            if (_dictionarySchema.TryGetIndex(item.Key, out index) && _dataList[index].HasValue) {
                throw new ArgumentException("An element with the same key already exists in the dictionary");
            }

            _dataList[index].Set(item.Value);
            _dataCount++;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            for (var ii = 0; ii < _dataList.Length; ii++ ) {
                _dataList[ii].Clear();
            }

            _dataCount = 0;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            int index;
            if ( _dictionarySchema.TryGetIndex( item.Key, out index ) ) {
                var entry = _dataList[index];
                return entry.HasValue && Equals(entry.Value, item.Value);
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            var indexEnum = _dictionarySchema.GetEnumerator();
            for( var ii = arrayIndex ; ii < array.Length ; ii++ ) {
                if (!indexEnum.MoveNext()) break;

                var index = indexEnum.Current;
                var entry = _dataList[index.Value];
                if ( entry.HasValue ) {
                    array[ii] = new KeyValuePair<TK, TV>(index.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(KeyValuePair<TK, TV> item)
        {
            int index;
            var isRemoved = _dictionarySchema.TryGetIndex(item.Key, out index) && _dataList[index].Clear();
            if (isRemoved) _dataCount--;
            return isRemoved;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count => _dataCount;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IDictionary<TK, TV>

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool ContainsKey(TK key)
        {
            int index;
            return _dictionarySchema.TryGetIndex(key, out index) && _dataList[index].HasValue;
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public void Add(TK key, TV value)
        {
            int index;
            if (!_dictionarySchema.TryGetIndex(key, out index)) {
                throw new ArgumentException("Value '" + key + "' is not supported by schema");
            }

            // Record was found in the schema, check our local table
            if (_dataList[index].HasValue) {
                throw new ArgumentException("An element with the same key already exists");
            }

            _dataList[index].Set(value);
            _dataCount++;
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public bool Remove(TK key)
        {
            int index;
            var isRemoved = _dictionarySchema.TryGetIndex(key, out index) && _dataList[index].Clear();
            if (isRemoved) _dataCount--;
            return isRemoved;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool TryGetValue(TK key, out TV value)
        {
            int index;
            if (_dictionarySchema.TryGetIndex(key, out index) && _dataList[index].HasValue) {
                value = _dataList[index].Value;
                return true;
            }

            value = default(TV);
            return false;
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public TV this[TK key]
        {
            get
            {
                var entry = _dataList[_dictionarySchema[key]];
                if (entry.HasValue) {
                    return entry.Value;
                }

                throw new KeyNotFoundException();
            }

            set
            {
                if (!_dataList[_dictionarySchema[key]].Set(value))
                    _dataCount++;
            }
        }

        /// <summary>
        /// Assigns the index.
        /// </summary>
        /// <param name="keyIndex">MapIndex of the key.</param>
        /// <param name="value">The value.</param>
        public void AssignIndex(int keyIndex, TV value)
        {
            if (!_dataList[keyIndex].Set(value))
                _dataCount++;
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TK> Keys
        {
            get
            {
                ICollection<TK> keyList = new List<TK>();
                foreach( var indexEntry in _dictionarySchema ) {
                    var entry = _dataList[indexEntry.Value];
                    if (entry.HasValue) {
                        keyList.Add(indexEntry.Key);
                    }
                }

                return keyList;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TV> Values
        {
            get
            {
                ICollection<TV> valueList = new List<TV>();
                foreach (var entry in _dataList) {
                    if (entry.HasValue) {
                        valueList.Add(entry.Value);
                    }
                }

                return valueList;
            }
        }

        #endregion

        #region Implementation of IDictionary<TK, TV>

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then the defaultValue is
        /// returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TV Get(TK key, TV defaultValue)
        {
            TV returnValue;
            if (!TryGetValue(key, out returnValue)) {
                returnValue = defaultValue;
            }

            return returnValue;	
        }

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TV Get(TK key)
        {
            return Get(key, default(TV));
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// </summary>
        public void Put(TK key, TV value)
        {
            this[key] = value;
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// If a value was previously mapped it is returned.
        /// </summary>
        public TV Push(TK key, TV value)
        {
            TV temp;
            TryGetValue(key, out temp);
            this[key] = value;
            return temp;
        }

        /// <summary>
        /// Puts all values from the source dictionary into
        /// this dictionary.
        /// </summary>
        /// <param name="source"></param>
        public void PutAll(IDictionary<TK, TV> source)
        {
            foreach( var sourceEntry in source ) {
                this[sourceEntry.Key] = sourceEntry.Value;
            }
        }

        /// <summary>
        /// Returns the first value in the enumeration of values
        /// </summary>
        /// <returns></returns>
        public TV FirstValue
        {
            get
            {
                for( var ii = 0 ; ii < _dataList.Length ; ii++ ) {
                    if ( _dataList[ii].HasValue ) {
                        return _dataList[ii].Value;
                    }
                }

                throw new ArgumentException("Collection had no elements");
            }
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.
        /// </summary>
        /// <param name="key">Search key into the dictionary</param>
        /// <param name="value">The value removed from the dictionary (if found).</param>
        /// <returns></returns>
        public bool Remove(TK key, out TV value)
        {
            int index;
            value = default(TV);
            var isRemoved = _dictionarySchema.TryGetIndex(key, out index) && _dataList[index].Clear(out value);
            if (isRemoved) _dataCount--;
            return isRemoved;
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  The item if found is returned; if not,
        /// default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TV RemoveAndReturn(TK key)
        {
            int index;
            var value = default(TV);
            if (_dictionarySchema.TryGetIndex(key, out index)) {
                if (_dataList[index].Clear(out value)) {
                    _dataCount--;
                }
            }

            return value;
        }

        #endregion

        internal struct Entry
        {
            internal bool HasValue;
            internal TV Value;

            internal bool Set(TV value)
            {
                var hadValue = HasValue;
                HasValue = true;
                Value = value;
                return hadValue;
            }

            internal bool Clear()
            {
                var hadValue = HasValue;
                HasValue = false;
                Value = default(TV);
                return hadValue;
            }

            internal bool Clear(out TV prevValue)
            {
                prevValue = Value;
                var hadValue = HasValue;
                HasValue = false;
                Value = default(TV);
                return hadValue;
            }
        }
    }
}
