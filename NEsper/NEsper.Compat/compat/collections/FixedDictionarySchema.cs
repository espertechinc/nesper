///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// FixedDictionarySchema is a class that represents the structure of a map who's
    /// keys are known when the schema is created.  FixedSchemas are immutable
    /// once they have been created, but because they are known they can be
    /// used to create Map objects that have a much smaller memory footprint
    /// than conventional hashtables.
    /// </summary>
    /// <typeparam name="K"></typeparam>

    public class FixedDictionarySchema<K> : IEnumerable<KeyValuePair<K,int>>
    {
        private readonly int _keyCount;

        /// <summary>
        /// This dictionary maps keys to a linear index.
        /// </summary>
        private readonly IDictionary<K, int> _keyToIndex =
            new Dictionary<K, int>();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _keyCount;

        /// <summary>
        /// Gets the keys for the schema.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<K> Keys => _keyToIndex.Keys;

        /// <summary>
        /// Gets the index associated with the specified key.
        /// </summary>
        /// <value></value>
        public int this[K key] => _keyToIndex[key];

        /// <summary>
        /// Tries the get the index for the key.  If the index does not
        /// exist, the method returns false.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool TryGetIndex(K key, out int index)
        {
            return _keyToIndex.TryGetValue(key, out index);
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<T>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<K, int>> GetEnumerator()
        {
            return _keyToIndex.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedDictionarySchema{K}"/> class.
        /// </summary>
        /// <param name="keyList">The key list.</param>
        public FixedDictionarySchema(IEnumerable<K> keyList)
        {
            int index = 0;
            foreach( K key in keyList ) {
                _keyToIndex[key] = index++;
            }

            _keyCount = _keyToIndex.Count;
        }
    }
}
