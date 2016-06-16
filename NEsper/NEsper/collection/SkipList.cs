///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.collection
{
    public class SkipList<TK,TV> : IDictionary<TK,TV>
        where TK : IComparable<TK>
    {
        /// <summary>
        /// The randomizer for the class
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        /// The head of the list
        /// </summary>
        private readonly Node _head;

        /// <summary>
        /// The number of entries the list.
        /// </summary>
        private int _count;

        /// <summary>
        /// Used to compare keys.
        /// </summary>
        private readonly IComparer<TK> _comparer; 

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipList{TK, TV}"/> class.
        /// </summary>
        public SkipList()
        {
            _head = new Node();
            _head.Next = new Node[16];
            _count = 0;
            _comparer = new DefaultComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipList{TK, TV}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public SkipList(IComparer<TK> comparer)
        {
            _head = new Node();
            _head.Next = new Node[16];
            _count = 0;
            _comparer = comparer;
        }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <key>
        /// The comparer.
        /// </key>
        public IComparer<TK> Comparer
        {
            get { return _comparer; }
        }

        /// <summary>
        /// Returns the height of the skiplist.
        /// </summary>
        /// <key>
        /// The height.
        /// </key>
        public int Height
        {
            get { return _head.Next.Length; }
        }

        /// <summary>
        /// Returns the # of items in the collection.
        /// </summary>
        /// <key>
        /// The count.
        /// </key>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Compares two key values.
        /// </summary>
        /// <param name="k1">The k1.</param>
        /// <param name="k2">The k2.</param>
        /// <returns></returns>
        private int Compare(TK k1, TK k2)
        {
            return _comparer.Compare(k1, k2);
        }

        /// <summary>
        /// Finds the insertion point for a node.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="node">The node.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">key already exists</exception>
        private Node InsertionPoint(Node head, Node node, int level)
        {
            for (var curr = head; ; curr = curr.Next[level])
            {
                if (curr.Next[level] == null)
                {
                    return curr;
                }

                var comp = Compare(node.Key, curr.Next[level].Key);
                if (comp < 0)
                {
                    return curr;
                }
                else if (comp == 0)
                {
                    throw new ArgumentException("key already exists");
                }
            }
        }
        
        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The key.</param>
        public void Add(TK key, TV value)
        {
            var level = _random.Next(0, _head.Next.Length);
            var node = new Node(key, value);
            var curr = _head;
            var pred = new Node[level];

            // find all the insertion points in the skiplist.  may also
            // throw an exception if the item already exists in the list.
            for (int ii = level; ii >= 0; ii--)
            {
                curr = pred[ii] = InsertionPoint(curr, node, ii);
            }

            // insert
            for (int ii = level; ii >= 0; ii--)
            {
                // set the current node
                curr = pred[ii];
                // set the linkages on node
                node.Next[ii] = curr.Next[ii];
                // set the linkages on the current node
                curr.Next[ii] = node;
            }

            _count++;
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the node from the chain.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="key">The key.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private Node Remove(Node head, TK key, int level, Mutable<bool> found)
        {
            for (var curr = head ;; curr = curr.Next[level])
            {
                if (curr.Next[level] == null)
                {
                    return head; // not in this chain
                }

                var comp = Compare(key, curr.Next[level].Key);
                if (comp == 0)
                {
                    // change the reference of curr.Next to curr.Next.Next
                    curr.Next[level] = curr.Next[level].Next[level];
                    found.Value = true;
                    // return the current node
                    return curr;
                }
                else if (comp < 0)
                {
                    // this case happens when we pass over the "key" - in a skip list
                    // this can occur because the level has a gap that does not contain
                    // the key.  since our comparisons are always done on 'curr.Next', we
                    // can safely return curr to indicate that it's removed from this
                    // level and curr is a good starting point for the next level

                    return curr;
                }
            }
        }

        /// <summary>
        /// Removes the specified key (and key).
        /// </summary>
        /// <param name="key">The key.</param>
        public bool Remove(TK key)
        {
            var curr = _head;
            var found = new Mutable<bool>(false);

            for (int ii = Height; ii >= 0; ii--)
            {
                curr = Remove(curr, key, ii, found);
            }

            if (found.Value)
                _count--;

            return found.Value;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(KeyValuePair<TK, TV> item)
        {
            var node = Search(item.Key);
            if (Equals(node.Value, item.Value))
            {
                return Remove(item.Key);
            }

            return false;
        }

        /// <summary>
        /// Searches the specified node for the given key.  The node that is returned
        /// is the node that 
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="key">The key.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private Node Search(Node head, TK key, int level)
        {
            for (var curr = head;; curr = curr.Next[level])
            {
                if (curr.Next[level] == null)
                {
                    return curr;
                }

                var comp = Compare(key, curr.Next[level].Key);
                if (comp <= 0)
                {
                    return curr;
                }
            }
        }

        /// <summary>
        /// Searches the specified node for the given key.  The node that is returned
        /// is the node that
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private Node Search(TK key)
        {
            var curr = _head;

            for (int ii = Height; ii >= 0; ii--)
            {
                var node = Search(curr, key, ii);
                if (node.Key.Equals(key))
                {
                    return node;
                }

                curr = node;
            }

            return null;
        }

        /// <summary>
        /// Tries the find the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The key.</param>
        /// <returns></returns>
        public bool TryGetValue(TK key, out TV value)
        {
            var node = Search(key);
            if (node == null)
            {
                value = default(TV);
                return false;
            }
            else
            {
                value = node.Value;
                return true;
            }
        }

        /// <summary>
        /// Copies the elements of the ICollection<T> to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            for (var curr = _head; curr != null; curr = curr.Next[0])
            {
                array[arrayIndex++] = new KeyValuePair<TK, TV>(curr.Key, curr.Value);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a key indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool ContainsKey(TK key)
        {
            var node = Search(key);
            return (node != null);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific key.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            var node = Search(item.Key);
            return (node != null) && Equals(node.Value, item.Value);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            _count = 0;
            _head.Next = new Node[16];
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            for (var curr = _head; curr != null; curr = curr.Next[0])
                yield return new KeyValuePair<TK, TV>(curr.Key, curr.Value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TK> Keys
        {
            get { return new KeyCollection(this); }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TV> Values
        {
            get { return new ValueCollection(this); }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public TV this[TK key]
        {
            get
            {
                var node = Search(key);
                if (node == null)
                    throw new KeyNotFoundException();
                return node.Value;
            }
            set
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Returns a dictionary that includes everything up to the specified key.
        /// Whether the key is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IDictionary<TK, TV> Head(TK key, bool isInclusive = false)
        {
            return new Submap(
                this,
                new Bound { HasValue = false },
                new Bound { HasValue = true, IsInclusive = isInclusive, Key = key });
        }

        /// <summary>
        /// Returns an enumeration of all key-values up to (and possibly including) the 
        /// given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> GetHead(TK key, bool isInclusive)
        {
            var tailNode = Search(key);
            if (tailNode != null)
            {
                for (var curr = _head.Next[0] ; curr != null && curr != tailNode ; curr = curr.Next[0])
                {
                    yield return new KeyValuePair<TK, TV>(
                        curr.Key,
                        curr.Value
                        );
                }

                if (!isInclusive && Equals(tailNode.Key, key))
                {
                    yield return new KeyValuePair<TK, TV>(
                        tailNode.Key,
                        tailNode.Value
                        );
                }
            }
        }

        /// <summary>
        /// Returns a dictionary that includes everything after the key.
        /// Whether the key is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="key">The end key.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IDictionary<TK, TV> Tail(TK key, bool isInclusive = true)
        {
            return new Submap(
                this,
                new Bound { HasValue = true, IsInclusive = isInclusive, Key = key },
                new Bound { HasValue = false });
        }

        /// <summary>
        /// Returns an enumeration of all key-values from (and possibly including) the 
        /// given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> GetTail(TK key, bool isInclusive)
        {
            var tailNode = Search(key);
            if (tailNode != null)
            {
                if (!isInclusive && Equals(tailNode.Key, key))
                {
                    tailNode = tailNode.Next[0];
                }

                for (var curr = tailNode; curr != null; curr = curr.Next[0])
                {
                    yield return new KeyValuePair<TK, TV>(
                        curr.Key,
                        curr.Value
                        );
                }
            }
        }

        public IDictionary<TK, TV> Between(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_comparer != null)
            {
                if (_comparer.Compare(startValue, endValue) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }
            else
            {
                var aa = (IComparable)startValue;
                var bb = (IComparable)endValue;
                if (aa.CompareTo(bb) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }

            return new Submap(
                this,
                new Bound { HasValue = true, IsInclusive = isStartInclusive, Key = startValue },
                new Bound { HasValue = true, IsInclusive = isEndInclusive, Key = endValue });
        }

        public IEnumerable<KeyValuePair<TK, TV>> EnumerateBetween(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_comparer != null)
            {
                if (_comparer.Compare(startValue, endValue) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }
            else
            {
                var aa = (IComparable)startValue;
                var bb = (IComparable)endValue;
                if (aa.CompareTo(bb) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }

            var head = Search(startValue);
            if (head != null)
            {
                if (!isStartInclusive && Equals(head.Key, startValue))
                {
                    head = head.Next[0];
                }

                var tail = Search(endValue);
                if (tail != null && !isEndInclusive && Equals(tail.Key, endValue))
                {
                    tail = tail.Next[0];
                }

                for (var curr = head; curr != null && curr != tail; curr = curr.Next[0])
                {
                    yield return new KeyValuePair<TK, TV>(
                        curr.Key,
                        curr.Value
                        );
                }
            }
        }

        public int CountBetween(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_comparer != null)
            {
                if (_comparer.Compare(startValue, endValue) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }
            else
            {
                var aa = (IComparable)startValue;
                var bb = (IComparable)endValue;
                if (aa.CompareTo(bb) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }

            int counter = 0;

            var head = Search(startValue);
            if (head != null)
            {
                if (!isStartInclusive && Equals(head.Key, startValue))
                {
                    head = head.Next[0];
                }

                var tail = Search(endValue);
                if (tail != null && !isEndInclusive && Equals(tail.Key, endValue))
                {
                    tail = tail.Next[0];
                }

                for (var curr = head; curr != null && curr != tail; curr = curr.Next[0])
                {
                    counter++;
                }
            }

            return counter;
        }

        public SkipList<TK, TV> Invert()
        {
            var comparer = _comparer;
            var inverted = new StandardComparer<TK>((a, b) => -comparer.Compare(a, b));
            var newList = new SkipList<TK, TV>(inverted);

            for (var curr = _head; curr != null; curr = curr.Next[0])
                newList.Add(curr.Key, curr.Value);

            return newList;
        }

        internal class Submap : IDictionary<TK, TV>
        {
            private readonly SkipList<TK, TV> _subDictionary;
            private Bound _lower;
            private Bound _upper;

            private readonly Func<TK, bool> _lowerTest;
            private readonly Func<TK, bool> _upperTest;

            internal Submap(SkipList<TK, TV> subDictionary, Bound lower, Bound upper)
            {
                _subDictionary = subDictionary;
                _lower = lower;
                _upper = upper;

                if (_subDictionary == null)
                {
                    _lowerTest = k1 => false;
                    _upperTest = k1 => false;
                }
                else
                {
                    var comparer = _subDictionary._comparer ?? new DefaultComparer();
                    _lowerTest = MakeLowerTest(comparer);
                    _upperTest = MakeUpperTest(comparer);
                }
            }

            private Func<TK, bool> MakeUpperTest(IComparer<TK> keyComparer)
            {
                if (_upper.HasValue)
                {
                    if (_upper.IsInclusive)
                    {
                        return k1 => keyComparer.Compare(k1, _upper.Key) <= 0;
                    }
                    else
                    {
                        return k1 => keyComparer.Compare(k1, _upper.Key) < 0;
                    }
                }
                else
                {
                    return k1 => true;
                }
            }

            private Func<TK, bool> MakeLowerTest(IComparer<TK> keyComparer)
            {
                if (_lower.HasValue)
                {
                    if (_lower.IsInclusive)
                    {
                        return k1 => keyComparer.Compare(k1, _lower.Key) >= 0;
                    }
                    else
                    {
                        return k1 => keyComparer.Compare(k1, _lower.Key) > 0;
                    }
                }
                else
                {
                    return k1 => true;
                }
            }

            #region Implementation of IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of IEnumerable<out KeyValuePair<K,V>>

            public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
            {
                IEnumerable<KeyValuePair<TK, TV>> enumerator =
                    _lower.HasValue
                    ? _subDictionary.GetTail(_lower.Key, _lower.IsInclusive)
                    : _subDictionary;

                return enumerator
                    .TakeWhile(pair => _upperTest.Invoke(pair.Key))
                    .GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection<KeyValuePair<K,V>>

            public void Add(KeyValuePair<TK, TV> item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(KeyValuePair<TK, TV> item)
            {
                if (_lowerTest.Invoke(item.Key) && _upperTest.Invoke(item.Key))
                {
                    return _subDictionary.Contains(item);
                }

                return false;
            }

            public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
            {
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    array[arrayIndex++] = enumerator.Current;
                }
            }

            public bool Remove(KeyValuePair<TK, TV> item)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get
                {
                    IEnumerable<KeyValuePair<TK, TV>> iterator;

                    if (_lower.HasValue)
                        iterator = _subDictionary.GetTail(_lower.Key, _lower.IsInclusive);
                    else
                        iterator = _subDictionary;

                    return iterator
                        .TakeWhile(pair => _upperTest.Invoke(pair.Key))
                        .Count();
                }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            #endregion

            #region Implementation of IDictionary<K,V>

            public bool ContainsKey(TK key)
            {
                if (_lowerTest.Invoke(key) && _upperTest.Invoke(key))
                {
                    return _subDictionary.ContainsKey(key);
                }

                return false;
            }

            public void Add(TK key, TV value)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TK key)
            {
                throw new NotSupportedException();
            }

            public bool TryGetValue(TK key, out TV value)
            {
                if (_lowerTest.Invoke(key) && _upperTest.Invoke(key))
                {
                    return _subDictionary.TryGetValue(key, out value);
                }

                value = default(TV);
                return false;
            }

            public TV this[TK key]
            {
                get
                {
                    if (_lowerTest.Invoke(key) && _upperTest.Invoke(key))
                    {
                        return _subDictionary[key];
                    }

                    throw new KeyNotFoundException();
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            public ICollection<TK> Keys
            {
                get
                {
                    IEnumerable<KeyValuePair<TK, TV>> iterator;

                    if (_lower.HasValue)
                        iterator = _subDictionary.GetTail(_lower.Key, _lower.IsInclusive);
                    else
                        iterator = _subDictionary;

                    return iterator
                        .TakeWhile(pair => _upperTest.Invoke(pair.Key))
                        .Select(pair => pair.Key)
                        .ToList();
                }
            }

            public ICollection<TV> Values
            {
                get
                {
                    IEnumerable<KeyValuePair<TK, TV>> iterator;

                    if (_lower.HasValue)
                        iterator = _subDictionary.GetTail(_lower.Key, _lower.IsInclusive);
                    else
                        iterator = _subDictionary;

                    return iterator
                        .TakeWhile(pair => _upperTest.Invoke(pair.Key))
                        .Select(pair => pair.Value)
                        .ToList();
                }
            }

            #endregion
        }

        internal struct Bound
        {
            internal TK Key;
            internal bool IsInclusive;
            internal bool HasValue;
        }

        #region KeyCollection

        internal class KeyCollection : ICollection<TK>
        {
            private readonly SkipList<TK, TV> _parent;

            internal KeyCollection(SkipList<TK, TV> parent)
            {
                _parent = parent;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<TK> GetEnumerator()
            {
                return _parent.Select(keyValuePair => keyValuePair.Key).GetEnumerator();
            }

            public void Add(TK item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TK item)
            {
                return _parent.ContainsKey(item);
            }

            public void CopyTo(TK[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TK item)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return _parent.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }
        }

        #endregion

        #region ValueCollection

        internal class ValueCollection : ICollection<TV>
        {
            private readonly SkipList<TK, TV> _parent;

            internal ValueCollection(SkipList<TK, TV> parent)
            {
                _parent = parent;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<TV> GetEnumerator()
            {
                return _parent.Select(keyValuePair => keyValuePair.Value).GetEnumerator();
            }

            public void Add(TV item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TV item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(TV[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TV item)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return _parent.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }
        }

        #endregion

        #region DefaultComparer

        internal class DefaultComparer : Comparer<TK>
        {
            public override int Compare(TK x, TK y)
            {
                return x.CompareTo(y);
            }
        }

        #endregion

        internal class Node
        {
            internal TK Key;
            internal TV Value;
            internal Node[] Next;

            public Node()
            {
            }

            public Node(TK key, TV value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
