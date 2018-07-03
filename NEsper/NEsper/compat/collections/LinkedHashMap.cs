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
using System.Linq;
using System.Runtime.Serialization;

using com.espertech.esper.collection;
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
    public sealed class LinkedHashMap<TK, TV> : IDictionary<TK, TV>, ISerializable
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

        /// <summary>
        /// Returns a value indicating if items should be shuffled (pushed to the
        /// head of the list) on access requests.
        /// </summary>

        public bool ShuffleOnAccess
        {
            get => _shuffleOnAccess;
            set { _shuffleOnAccess = value; }
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

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">The INFO.</param>
        /// <param name="context">The context.</param>

        public LinkedHashMap(SerializationInfo info, StreamingContext context)
        {
            var count = info.GetInt32("Count");
            var pairList = (Pair<TK, TV>[]) info.GetValue("_hashList", typeof (Pair<TK, TV>[]));
            Debug.Assert(pairList.Length == count);

            _shuffleOnAccess = info.GetBoolean("_shuffle");

            _hashList = new LinkedList<Pair<TK, TV>>(pairList);
            if ( _hashList == null ) {
                throw new SerializationException("unable to deserialize hashList");
            }

            _hashTable = new Dictionary<TK, LinkedListNode<Pair<TK, TV>>>((_hashList.Count * 3) / 2)
                .WithSafeSupport();

            for (LinkedListNode<Pair<TK, TV>> node = _hashList.First; node != null; node = node.Next) {
                _hashTable.Add(node.Value.First, node);
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
            // Note: we identified a bug in the serializer that occurs with serialization of a linked list
            // Note: henceforth, use an array to serialize and deserialize.
            info.AddValue("_hashList", _hashList.ToArray());
            info.AddValue("_shuffle", _shuffleOnAccess);
            info.AddValue("Count", _hashList.Count);
        }

        #region IDictionary<K,V> Members

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
                IEnumerator<KeyValuePair<TK, TV>> enumObj = source.GetEnumerator();
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
                IEnumerator<KeyValuePair<TK, TV>> kvPairEnum = GetEnumerator();
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

        #region IDictionary<K,V> Members

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
                
                foreach (Pair<TK, TV> keyValuePair in _hashList)
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

        public ICollection<TK> Keys
        {
            get
            {
                return _hashList.Select(keyValuePair => keyValuePair.First).ToList();
            }
        }

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

        public ICollection<TV> Values
        {
            get
            {
                return _hashList.Select(keyValuePair => keyValuePair.Second).ToList();
            }
        }

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
                LinkedListNode<Pair<TK, TV>> linkedListNode = _hashTable.Get(key);
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

        #region ICollection<KeyValuePair<K,V>> Members

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

            int ii = arrayIndex;

            foreach (Pair<TK, TV> keyValuePair in _hashList)
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

        #region IEnumerable<KeyValuePair<K,V>> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return _hashList.Select(subPair => new KeyValuePair<TK, TV>(subPair.First, subPair.Second)).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    
        public override string ToString()
        {
            return
                "{" +
                _hashList
                    .Select(subPair => string.Format("{0}={1}", subPair.First.RenderAny(), subPair.Second.RenderAny()))
                    .Aggregate((a, b) => a + ", " + b) +
                "}";
        }
    }
}
