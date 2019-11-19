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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// Description of LinkedHashSet.
    /// </summary>

    [Serializable]
    public sealed class LinkedHashSet<T> 
        : ISet<T>
        , ISerializable
    {
        internal class Entry
        {
            internal T Value;
            internal Entry Next;
            internal Entry Prev;
        }

        /// <summary>
        /// A list of all key-value pairs added to the table.  The list
        /// preserves insertion order and is used to preserve enumeration
        /// ordering.
        /// </summary>

        private readonly Entry _entryListHead;
        private Entry _entryListTail;
        private int _entryCount;

        /// <summary>
        /// Contains a reference to the key and is used for all lookups.  Refers
        /// to the node in the linked list node.  Provides for fast removal of
        /// the node upon removal.
        /// </summary>

        private readonly IDictionary<T, Entry> _indexTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashSet&lt;T&gt;"/> class.
        /// </summary>
        public LinkedHashSet()
        {
            _entryCount = 0;
            _entryListHead = new Entry();
            _entryListTail = _entryListHead;
            _indexTable = new Dictionary<T, Entry>().WithSafeSupport();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public LinkedHashSet(IEnumerable<T> source)
        {
            _entryCount = 0;
            _entryListHead = new Entry();
            _entryListTail = _entryListHead;
            _indexTable = new Dictionary<T, Entry>().WithSafeSupport();

            foreach (var item in source)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">The INFO.</param>
        /// <param name="context">The context.</param>

        public LinkedHashSet(SerializationInfo info, StreamingContext context)
        {
            var count = info.GetInt32("Count");
            var array = (T[])info.GetValue("_list", typeof(T[]));
            Debug.Assert(array.Length == count);

            _entryCount = 0;
            _entryListHead = new Entry();
            _entryListTail = _entryListHead;
            _indexTable = new Dictionary<T, Entry>((array.Length * 3) / 2).WithSafeSupport();

            foreach (T item in array)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var hashArray = ToArray();
            info.AddValue("_list", hashArray);
            info.AddValue("Count", _entryCount);
        }

        /// <summary>
        /// Add all values from the source
        /// </summary>
        /// <param name="source"></param>

        public void AddRange(ICollection<T> source)
        {
            if (source.Count != 0)
            {
                IEnumerator<T> enumObj = source.GetEnumerator();
                while (enumObj.MoveNext())
                {
                    Add(enumObj.Current);
                }
            }
        }

        #region ISet<T> Members

        bool ISet<T>.Add(T item)
        {
            return AddInternal(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>

        public void Add(T item)
        {
            AddInternal(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>

        public void Clear()
        {
            _indexTable.Clear();
            _entryListHead.Next = null;
            _entryListTail = _entryListHead;
            _entryCount = 0;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>

        public bool Contains(T item)
        {
            return _indexTable.ContainsKey(item);
        }

        public T[] ToArray()
        {
            var hashArray = new T[_entryCount];
            var hashIndex = 0;
            for (var entry = _entryListHead.Next; entry != null; entry = entry.Next)
                hashArray[hashIndex++] = entry.Value;
            return hashArray;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var hashIndex = arrayIndex;
            for (var entry = _entryListHead.Next; entry != null; entry = entry.Next)
                array[hashIndex++] = entry.Value;
        }

        /// <summary>
        /// Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => _entryCount == 0;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>

        public int Count => _entryCount;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>

        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>

        public bool Remove(T item)
        {
            Entry entry;
            if (_indexTable.TryGetValue(item, out entry))
            {
                RemoveEntry(entry);
                return true;
            }

            return false;
        }

        public void RemoveWhere(Func<T, Boolean> whereClause)
        {
            var entry = _entryListHead.Next;
            for (; entry != null; )
            {
                if (whereClause.Invoke(entry.Value))
                {
                    var next = entry.Next;
                    RemoveEntry(entry);
                    entry = next;
                }
                else
                {
                    entry = entry.Next;
                }
            }
        }

        private void RemoveEntry(Entry entry)
        {
            _indexTable.Remove(entry.Value);
            _entryCount--;

            entry.Prev.Next = entry.Next;
            if (entry.Next != null)
                entry.Next.Prev = entry.Prev;
            else
                _entryListTail = entry.Prev;
        }

        #endregion

        private bool AddInternal(T item)
        {
            if (!_indexTable.ContainsKey(item))
            {
                var entry = new Entry
                {
                    Value = item,
                    Prev = _entryListTail
                };

                _entryListTail.Next = entry;
                _entryListTail = entry;
                _entryCount++;

                _indexTable[item] = entry;

                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var entry = _entryListHead.Next; entry != null; entry = entry.Next)
                yield return entry.Value;
        }

        public void AddTo(ICollection<T> collection)
        {
            for (var entry = _entryListHead.Next; entry != null; entry = entry.Next)
                collection.Add(entry.Value);
        }

        public void ForEach(Action<T> action)
        {
            for (var entry = _entryListHead.Next; entry != null; entry = entry.Next)
                action.Invoke(entry.Value);
        }
    }
}
