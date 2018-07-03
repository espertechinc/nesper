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
    public class OrderedDictionary<TK,TV> : IDictionary<TK,TV>
    {
        /// <summary>
        /// Value List
        /// </summary>
        private readonly List<TK> _keyList;
        /// <summary>
        /// Value list
        /// </summary>
        private readonly List<TV> _valList;
        /// <summary>
        /// Value comparer
        /// </summary>
        private readonly IComparer<TK> _keyComparer;

        /// <summary>
        /// Gets the key comparer.
        /// </summary>
        /// <value>The key comparer.</value>
        public IComparer<TK> KeyComparer => _keyComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TK,TV}"/> class.
        /// </summary>
        /// <param name="keyList">The key list.</param>
        /// <param name="valList">The val list.</param>
        /// <param name="comparer">The comparer.</param>
        internal OrderedDictionary(List<TK> keyList,
                                   List<TV> valList,
                                   IComparer<TK> comparer)
        {
            _keyList = keyList;
            _valList = valList;
            _keyComparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TK,TV}"/> class.
        /// </summary>
        /// <param name="keyComparer">The key comparer.</param>
        public OrderedDictionary(IComparer<TK> keyComparer)
        {
            _keyList = new List<TK>();
            _valList = new List<TV>();
            _keyComparer = keyComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TK,TV}"/> class.
        /// </summary>
        public OrderedDictionary()
        {
            _keyList = new List<TK>();
            _valList = new List<TV>();
            _keyComparer = null;
        }

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

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            for (int ii = 0; ii < _keyList.Count; ii++)
            {
                yield return new KeyValuePair<TK, TV>(
                    _keyList[ii],
                    _valList[ii]);
            }
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator(TK startKey, bool isInclusive)
        {
            int index = GetHeadIndex(startKey, isInclusive);
            if (index == -1)
                index = 0;

            for (int ii = index; ii < _keyList.Count; ii++)
            {
                yield return new KeyValuePair<TK, TV>(
                    _keyList[ii],
                    _valList[ii]);
            }
        }

        public void ForEach(Action<KeyValuePair<TK, TV>> action)
        {
            for (int ii = 0; ii < _keyList.Count; ii++)
            {
                action.Invoke(
                    new KeyValuePair<TK, TV>(
                        _keyList[ii],
                        _valList[ii]));
            }
        }

        public void ForEach(Action<int,KeyValuePair<TK,TV>> action)
        {
            for (int ii = 0; ii < _keyList.Count; ii++)
            {
                action.Invoke(
                    ii, new KeyValuePair<TK, TV>(
                            _keyList[ii],
                            _valList[ii]));
            }
        }

        /// <summary>
        /// Searches the list for a given key.  The algorithm leverages the binary
        /// search routine built into the class libraries.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal int BinarySearch(TK key)
        {
            return _keyComparer != null
                       ? _keyList.BinarySearch(key, _keyComparer)
                       : _keyList.BinarySearch(key);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<TK, TV> item)
        {
            var index = BinarySearch(item.Key);
            if (index >= 0)
            {
                throw new ArgumentException("An element with the same key already exists");
            }
            else
            {
                _keyList.Insert(~index, item.Key);
                _valList.Insert(~index, item.Value);
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _keyList.Clear();
            _valList.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            var index = BinarySearch(item.Key);
            return (index >= 0);
        }

        /// <summary>
        /// Copies the array to a target.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">MapIndex of the array.</param>
        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            int arrayLength = array.Length;
            for (int ii = arrayIndex, listIndex = 0; ii < arrayLength && listIndex < _keyList.Count ; ii++, listIndex++ )
            {
                array[ii] = new KeyValuePair<TK, TV>(
                    _keyList[listIndex],
                    _valList[listIndex]);
            }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TK, TV> item)
        {
            var index = BinarySearch(item.Key);
            if (index < 0)
                return false;
            _keyList.RemoveAt(index);
            _valList.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _keyList.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TK key)
        {
            var index = BinarySearch(key);
            return (index >= 0);
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TK key, TV value)
        {
            Add(new KeyValuePair<TK, TV>(key, value));
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(TK key)
        {
            return Remove(new KeyValuePair<TK, TV>(key, default(TV)));
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TK key, out TV value)
        {
            var index = BinarySearch(key);
            if (index >= 0)
            {
                value = _valList[index];
                return true;
            }

            value = default(TV);
            return false;
        }

        public TV TryInsert(TK key, Func<TV> valueFactory)
        {
            var index = BinarySearch(key);
            if (index >= 0)
            {
                return _valList[index];
            }

            var value = valueFactory.Invoke();

            _keyList.Insert(~index, key);
            _valList.Insert(~index, value);

            return value;
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value></value>
        public TV this[TK key]
        {
            get
            {
                var index = BinarySearch(key);
                if (index >= 0)
                {
                    return _valList[index];
                }

                throw new KeyNotFoundException();
            }
            set
            {
                var index = BinarySearch(key);
                if (index >= 0)
                {
                    _valList[index] = value;
                }
                else
                {
                    _keyList.Insert(~index, key);
                    _valList.Insert(~index, value);
                }
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TK> Keys => _keyList;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<TV> Values => _valList;

        /// <summary>
        /// Returns a dictionary that includes everything up to the specified value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IDictionary<TK, TV> Head(TK value, bool isInclusive = false)
        {
            return new SubmapDictionary<TK, TV>(
                this,
                new SubmapDictionary<TK, TV>.Bound { HasValue = false },
                new SubmapDictionary<TK, TV>.Bound { HasValue = true, IsInclusive = isInclusive, Key = value });

        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// ending at the head index.
        /// </summary>
        /// <param name="value">The start value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public int GetHeadIndex(TK value, bool isInclusive)
        {
            int headIndex = BinarySearch(value);
            if (headIndex >= 0) // key found
            {
                if (isInclusive == false)
                {
                    headIndex--;
                }
            }
            else
            {
                headIndex = ~headIndex - 1;
            }
            return headIndex;
        }

        public IEnumerable<KeyValuePair<TK, TV>> GetTail(TK value, bool isInclusive)
        {
            int tailIndex = GetTailIndex(value, isInclusive);
            if (tailIndex != -1)
            {
                int count = Count;
                for( ; tailIndex < count ; tailIndex++ )
                {
                    yield return new KeyValuePair<TK, TV>(
                        _keyList[tailIndex],
                        _valList[tailIndex]);
                }
            }
        }

        /// <summary>
        /// Returns a dictionary that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IDictionary<TK, TV> Tail(TK value, bool isInclusive = true)
        {
            return new SubmapDictionary<TK, TV>(
                this,
                new SubmapDictionary<TK, TV>.Bound { HasValue = true, IsInclusive = isInclusive, Key = value },
                new SubmapDictionary<TK, TV>.Bound { HasValue = false });
        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// starting from tail index.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public int GetTailIndex(TK value, bool isInclusive)
        {
            int tailIndex = BinarySearch(value);
            if (tailIndex >= 0) // key found
            {
                if (isInclusive == false)
                {
                    tailIndex++;
                }
            }
            else
            {
                tailIndex = ~tailIndex;
            }
            return tailIndex;
        }

        public IEnumerable<KeyValuePair<TK, TV>> EnumerateBetween(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_keyComparer != null)
            {
                if (_keyComparer.Compare(startValue, endValue) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }
            else
            {
                var aa = (IComparable) startValue;
                var bb = (IComparable) endValue;
                if (aa.CompareTo(bb) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }

            int tailIndex = GetHeadIndex(endValue, isEndInclusive);
            if (tailIndex != -1)
            {
                int headIndex = GetTailIndex(startValue, isStartInclusive);
                if (headIndex != -1)
                {
                    for (int ii = headIndex; ii <= tailIndex; ii++ )
                    {
                        yield return new KeyValuePair<TK, TV>(_keyList[ii], _valList[ii]);
                    }
                }
            }
        }
        
        public IDictionary<TK, TV> Between(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_keyComparer != null)
            {
                if (_keyComparer.Compare(startValue, endValue) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }
            else
            {
                var aa = (IComparable) startValue;
                var bb = (IComparable) endValue;
                if (aa.CompareTo(bb) > 0)
                {
                    throw new ArgumentException("invalid key order");
                }
            }

            return new SubmapDictionary<TK, TV>(
                this,
                new SubmapDictionary<TK, TV>.Bound { HasValue = true, IsInclusive = isStartInclusive, Key = startValue },
                new SubmapDictionary<TK, TV>.Bound { HasValue = true, IsInclusive = isEndInclusive, Key = endValue });
        }

        public int CountBetween(TK startValue, bool isStartInclusive, TK endValue, bool isEndInclusive)
        {
            if (_keyComparer != null)
            {
                if (_keyComparer.Compare(startValue, endValue) > 0)
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

            int tailIndex = GetHeadIndex(endValue, isEndInclusive);
            if (tailIndex != -1)
            {
                int headIndex = GetTailIndex(startValue, isStartInclusive);
                if (headIndex != -1)
                {
                    return tailIndex - headIndex + 1;
                }
            }

            return 0;
        }

        public OrderedDictionary<TK, TV> Invert()
        {
            var comparer = _keyComparer;
            var inverted = new StandardComparer<TK>(
                (a, b) => -comparer.Compare(a, b));

            var invertedKeyList = new List<TK>(_keyList);
            var invertedValList = new List<TV>(_valList);

            invertedKeyList.Reverse();
            invertedValList.Reverse();

            return new OrderedDictionary<TK, TV>(
                invertedKeyList,
                invertedValList,
                inverted);
        }
    }

#if false
    public class DebugCount
    {
        public static int TailCount;
        public static int GetTailIndexCount;
        public static int TryGetValueCount;
        public static int GetTailCount;
        public static int GetEnumeratorCount;
        public static int IndexSetCount;
        public static int IndexGetCount;

        public static string DebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("TailCount: {0}", TailCount);
            stringBuilder.AppendFormat(", GetTailIndexCount: {0}", GetTailIndexCount);
            stringBuilder.AppendFormat(", TryGetValueCount: {0}", TryGetValueCount);
            stringBuilder.AppendFormat(", GetEnumeratorCount: {0}", GetEnumeratorCount);
            stringBuilder.AppendFormat(", GetTailCount: {0}", GetTailCount);
            stringBuilder.AppendFormat(", IndexGetCount: {0}", IndexGetCount);
            stringBuilder.AppendFormat(", IndexSetCount: {0}", IndexSetCount);

            return stringBuilder.ToString();
        }
    }
#endif
}
