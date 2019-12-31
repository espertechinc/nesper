///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// <summary>
    /// A wrapper that provide a dictionary that is readonly.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class ReadOnlyDictionary<TK, TV> : IDictionary<TK, TV>
    {
        private readonly IDictionary<TK, TV> _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Collections.ObjectModel.ReadOnlyDictionary{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public ReadOnlyDictionary(IDictionary<TK, TV> parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <value></value>
        public TV this[TK index] {
            get => _parent[index];
            set => throw new NotSupportedException();
        }

        public ICollection<TK> Keys => _parent.Keys;
        public ICollection<TV> Values => _parent.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count => _parent.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly => true;

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TK key)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        public void Add(
            TK key,
            TV value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TK, TV> keyValuePair)
        {
            return _parent.Contains(keyValuePair);
        }

        public bool ContainsKey(TK key)
        {
            return _parent.ContainsKey(key);
        }

        public bool TryGetValue(
            TK key,
            out TV value)
        {
            return _parent.TryGetValue(key, out value);
        }

        public void CopyTo(
            KeyValuePair<TK, TV>[] array,
            int arrayIndex)
        {
            _parent.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return _parent.GetEnumerator();
        }
    }
}
