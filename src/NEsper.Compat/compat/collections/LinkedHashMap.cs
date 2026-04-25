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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using com.espertech.esper.compat.attributes;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// Hashtable and linked list implementation designed to mimic Java's LinkedHashMap
    /// functionality.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>

    [Serializable]
    [RenderWithToString]
    public sealed class LinkedHashMap<TK, TV> : IDictionary<TK, TV>
    {
        /// <summary>
        /// Delegate for handling events on dictionary entries.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>

        public delegate bool EntryEventHandler(KeyValuePair<TK, TV> entry);

        /// <summary>
        /// A list of all key-value pairs added to the table.  The list
        /// preserves insertion order and is used to preserve enumeration
        /// ordering.
        /// </summary>

        private readonly LinkedList<Pair<TK, TV>> _hashList;

        /// <summary>
        /// Contains a reference to the key and is used for all lookups.  Refers
        /// to the node in the linked list node.  Provides for fast removal of
        /// the node upon removal.
        /// </summary>

        private readonly IDictionary<TK, LinkedListNode<Pair<TK, TV>>> _hashTable;

        /// <summary>
        /// Shuffles items on access
        /// </summary>

        private bool _shuffleOnAccess;

        [NonSerialized]
        private KeysView _cachedKeysView;
        [NonSerialized]
        private ValuesView _cachedValuesView;

        /// <summary>
        /// Returns a value indicating if items should be shuffled (pushed to the
        /// head of the list) on access requests.
        /// </summary>

        public bool ShuffleOnAccess
        {
            get => _shuffleOnAccess;
            set => _shuffleOnAccess = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashMap{K,V}"/> class.
        /// </summary>

        public LinkedHashMap()
        {
            _shuffleOnAccess = false;
            _hashList = new LinkedList<Pair<TK, TV>>();
            _hashTable = new Dictionary<TK, LinkedListNode<Pair<TK, TV>>>().WithSafeSupport();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashMap{K,V}"/> class.
        /// </summary>
        /// <param name="sourceTable"></param>

        public LinkedHashMap(IEnumerable<KeyValuePair<TK, TV>> sourceTable)
        {
            _hashList = new LinkedList<Pair<TK, TV>>();
            _hashTable = new Dictionary<TK, LinkedListNode<Pair<TK, TV>>>().WithSafeSupport();

            foreach (var entry in sourceTable)
            {
                Put(entry.Key, entry.Value);
            }
        }

        #region IDictionary<TK, TV> Members

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
            if (!TryGetValue(key, out returnValue))
            {
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
        /// already exists, then it is remapped to thenew value.
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
            if (source.Count != 0)
            {
                var enumObj = source.GetEnumerator();
                while (enumObj.MoveNext())
                {
                    this[enumObj.Current.Key] = enumObj.Current.Value;
                }
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
                var kvPairEnum = GetEnumerator();
                kvPairEnum.MoveNext();
                return kvPairEnum.Current.Value;
            }
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  Returns the value that was found at that
        /// location and removed or the defaultValue.
        /// </summary>
        /// <param name="key">Search key into the dictionary</param>
        /// <param name="value">The value removed from the dictionary (if found).</param>
        /// <returns></returns>

        public bool Remove(TK key, out TV value)
        {
            if (!TryGetValue(key, out value))
            {
                return false;
            }

            return Remove(key);
        }

        #endregion

        /// <summary>
        /// Occurs when a potentially destructive operations occurs on the dictionary
        /// and the dictionary is allowed to rebalance.
        /// </summary>

        public event EntryEventHandler RemoveEldest;

        #region IDictionary<TK, TV> Members

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>

        public void Add(TK key, TV value)
        {
            if (_hashTable.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists");
            }

            var keyValuePair = new Pair<TK, TV>(key, value);
            var linkedListNode = _hashList.AddLast(keyValuePair);
            _hashTable.Add(key, linkedListNode);

            CheckEldest();
        }

        /// <summary>
        /// Checks the eldest entry and see if we should remove it.
        /// </summary>

        private void CheckEldest()
        {
            if (RemoveEldest != null)
            {
                var linkedListNode = _hashList.First;
                var eldest = new KeyValuePair<TK, TV>(
                    linkedListNode.Value.First,
                    linkedListNode.Value.Second);
                if (RemoveEldest(eldest))
                {
                    _hashList.Remove(linkedListNode);
                    _hashTable.Remove(linkedListNode.Value.First);
                }
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>

        public bool ContainsKey(TK key)
        {
            return _hashTable.ContainsKey(key);
        }

        /// <summary>
        /// Gets the key enumerator in a faster and more efficient manner.
        /// </summary>
        /// <value>The fast key enumerator.</value>
        public IEnumerator<TK> FastKeyEnumerator
        {
            get { return _hashList.Select(keyValuePair => keyValuePair.First).GetEnumerator(); }
        }

        /// <summary>
        /// Gets the keys in a faster and more efficient manner.
        /// </summary>
        /// <value>The fast key array.</value>
        public TK[] FastKeyArray
        {
            get
            {
                var rawArray = new TK[Count];
                var rawIndex = 0;
                
                foreach (var keyValuePair in _hashList)
                {
                    rawArray[rawIndex++] = keyValuePair.First;
                }

                return rawArray;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>

        public ICollection<TK> Keys => _cachedKeysView ??= new KeysView(_hashList, _hashTable);

        /// <summary>
        /// Gets a faster lighter enumeration of keys.
        /// </summary>

        public IEnumerator<TK> FastKeys
        {
            get {
                return _hashList.Select(keyValuePair => keyValuePair.First).GetEnumerator();
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>

        public bool Remove(TK key)
        {
            LinkedListNode<Pair<TK, TV>> linkedListNode;
            if (_hashTable.TryGetValue(key, out linkedListNode))
            {
                _hashTable.Remove(key);
                _hashList.Remove(linkedListNode);
                return true;
            }

            return false;
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
            TV tempItem;

            return Remove(key, out tempItem)
                    ? tempItem
                    : default(TV);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>

        public bool TryGetValue(TK key, out TV value)
        {
            LinkedListNode<Pair<TK, TV>> linkedListNode;
            if (_hashTable.TryGetValue(key, out linkedListNode))
            {
                value = linkedListNode.Value.Second;
                if (ShuffleOnAccess)
                {
                    _hashList.Remove(linkedListNode);
                    _hashList.AddLast(linkedListNode);
                }
                return true;
            }

            value = default(TV);

            return false;
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>

        public ICollection<TV> Values => _cachedValuesView ??= new ValuesView(_hashList);

        /// <summary>
        /// Gets or sets the value the specified key.
        /// </summary>
        /// <value></value>

        public TV this[TK key]
        {
            get
            {
                var linkedListNode = _hashTable[key];
                if (ShuffleOnAccess)
                {
                    _hashList.Remove(linkedListNode);
                    _hashList.AddLast(linkedListNode);
                }

                return linkedListNode.Value.Second;
            }
            set
            {
                var linkedListNode = _hashTable.Get(key);
                if (linkedListNode != null)
                {
                    linkedListNode.Value.Second = value;
                }
                else
                {
                    var keyValuePair = new Pair<TK, TV>(key, value);
                    linkedListNode = _hashList.AddLast(keyValuePair);
                    _hashTable[key] = linkedListNode;
                }

                CheckEldest();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TK, TV>> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>

        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>

        public void Clear()
        {
            _hashTable.Clear();
            _hashList.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            return _hashTable.ContainsKey(item.Key);
        }

        /// <summary>
        /// The table to the target array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">MapIndex of the array.</param>
        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var ii = arrayIndex;

            foreach (var keyValuePair in _hashList)
            {
                array[ii++] = new KeyValuePair<TK, TV>(keyValuePair.First, keyValuePair.Second);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>

        public int Count => _hashTable.Count;

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

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            return Remove(item.Key);
        }

        public void RemoveWhere(Func<Pair<TK,TV>, Boolean> whereClause)
        {
            var node = _hashList.First;
            for(; node != null; )
            {
                if (whereClause.Invoke(node.Value))
                {
                    var next = node.Next;
                    _hashList.Remove(node);
                    _hashTable.Remove(node.Value.First);
                    node = next;
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TK, TV>> Members

        public Enumerator GetEnumerator() => new Enumerator(_hashList);

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    
        public override string ToString()
        {
            return
                "{" +
                _hashList
                    .Select(subPair => {
                        var renderFirst = subPair.First.RenderAny();
                        var renderSecond = subPair.Second.RenderAny();
                        return $"{renderFirst}={renderSecond}";
                    })
                    .Aggregate((a, b) => $"{a}, {b}") +
                "}";
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TK, TV>>
        {
            private readonly LinkedList<Pair<TK, TV>> _list;
            private LinkedListNode<Pair<TK, TV>> _current;

            internal Enumerator(LinkedList<Pair<TK, TV>> list)
            {
                _list = list;
                _current = null;
            }

            public bool MoveNext()
            {
                _current = _current == null ? _list.First : _current.Next;
                return _current != null;
            }

            public KeyValuePair<TK, TV> Current =>
                new KeyValuePair<TK, TV>(_current.Value.First, _current.Value.Second);

            object IEnumerator.Current => Current;

            public void Reset() => _current = null;

            public void Dispose() { }
        }

        private sealed class KeysView : ICollection<TK>
        {
            private readonly LinkedList<Pair<TK, TV>> _list;
            private readonly IDictionary<TK, LinkedListNode<Pair<TK, TV>>> _table;

            internal KeysView(
                LinkedList<Pair<TK, TV>> list,
                IDictionary<TK, LinkedListNode<Pair<TK, TV>>> table)
            {
                _list = list;
                _table = table;
            }

            public int Count => _table.Count;
            public bool Contains(TK item) => _table.ContainsKey(item);

            public IEnumerator<TK> GetEnumerator()
            {
                var node = _list.First;
                while (node != null)
                {
                    yield return node.Value.First;
                    node = node.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void CopyTo(TK[] array, int arrayIndex)
            {
                var node = _list.First;
                var i = arrayIndex;
                while (node != null)
                {
                    array[i++] = node.Value.First;
                    node = node.Next;
                }
            }

            public bool IsReadOnly => true;

            public void Add(TK item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();

            public bool Remove(TK item) => throw new NotSupportedException();
        }

        private sealed class ValuesView : ICollection<TV>
        {
            private readonly LinkedList<Pair<TK, TV>> _list;

            internal ValuesView(LinkedList<Pair<TK, TV>> list) => _list = list;

            public int Count => _list.Count;
            public bool IsReadOnly => true;

            public bool Contains(TV item)
            {
                var comparer = EqualityComparer<TV>.Default;
                var node = _list.First;
                while (node != null)
                {
                    if (comparer.Equals(node.Value.Second, item)) return true;
                    node = node.Next;
                }
                return false;
            }

            public IEnumerator<TV> GetEnumerator()
            {
                var node = _list.First;
                while (node != null)
                {
                    yield return node.Value.Second;
                    node = node.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void CopyTo(TV[] array, int arrayIndex)
            {
                var node = _list.First;
                var i = arrayIndex;
                while (node != null)
                {
                    array[i++] = node.Value.Second;
                    node = node.Next;
                }
            }

            public void Add(TV item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Remove(TV item) => throw new NotSupportedException();
        }
    }
}
