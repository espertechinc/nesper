///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// An extended set that uses a tree-based backing store.
    /// As such, the set is always sorted.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    [Serializable]
    public class TreeSet<T> : ICollection<T>
    {
        private readonly SortedDictionary<T, T> _store;
    	
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet&lt;T&gt;"/> class.
        /// </summary>
        public TreeSet()
        {
            _store = new SortedDictionary<T, T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        public TreeSet(IEnumerable<T> sourceData)
        {
            _store = new SortedDictionary<T, T>();
            foreach (var item in sourceData)
            {
                _store[item] = item;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public TreeSet(IComparer<T> comparer)
        {
            _store = new SortedDictionary<T, T>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public TreeSet(Func<T, T, int> comparer)
        {
            _store = new SortedDictionary<T, T>(
                new ProxyComparer<T>(comparer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="comparer">The comparer.</param>
        public TreeSet(IEnumerable<T> sourceData, IComparer<T> comparer)
        {
            _store = new SortedDictionary<T, T>();
            foreach( var item in sourceData ) {
                _store[item] = item;
            }
        }

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		
        public void Add( T item )
        {
            _store[item] = item;
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        /// <param name="items"></param>

        public void RemoveAll(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                _store.Remove(item);
            }
        }

        #endregion
    	
    	
        public int Count {
            get {
                return _store.Count;
            }
        }
    	
        public bool IsReadOnly {
            get {
                return false;
            }
        }
    	
        public T[] ToArray()
        {
            return _store.Keys.ToArray();
        }
    	
        public void Clear()
        {
            _store.Clear();
        }
    	
        public bool Contains(T item)
        {
            return _store.ContainsKey(item);
        }
    	
        public void CopyTo(T[] array, int arrayIndex)
        {
            _store.Keys.CopyTo(array, arrayIndex);
        }
    	
        public bool Remove(T item)
        {
            return _store.Remove(item);
        }
    	
        public IEnumerator<T> GetEnumerator()
        {
            return _store.Keys.GetEnumerator();
        }
    	
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _store.Keys.GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as TreeSet<T>;
            if ( other == null )
            {
                return false;
            }

            return Collections.AreEqual(
                this.GetEnumerator(),
                other.GetEnumerator());
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _store.GetHashCode();
        }
    }
}
