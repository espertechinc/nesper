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

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections
{
    public partial class OrderedListDictionary<TK, TV> : IOrderedDictionary<TK, TV>
    {
        /// <summary>
        /// Item List
        /// </summary>
        private readonly List<KeyValuePair<TK, TV>> _itemList;

        /// <summary>
        /// Value comparer
        /// </summary>
        private readonly KeyValuePairComparer _itemComparer;

        /// <summary>
        /// Gets the item comparer.
        /// </summary>
        /// <value>The item comparer.</value>
        public IComparer<KeyValuePair<TK, TV>> ItemComparer => _itemComparer;

        /// <summary>
        /// Gets the key comparer.
        /// </summary>
        /// <value>The key comparer.</value>
        public IComparer<TK> KeyComparer => _itemComparer.KeyComparer;

        private readonly OrderedListDictionaryKeys<TK, TV> _keys;
        private readonly OrderedListDictionaryValues<TK, TV> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedListDictionary{TK,TV}" /> class.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="comparer">The comparer.</param>
        internal OrderedListDictionary(
            List<KeyValuePair<TK, TV>> itemList,
            KeyValuePairComparer comparer)
        {
            _itemList = itemList;
            _itemComparer = comparer;

            var range = new BoundRange<TK>(null, null, comparer.KeyComparer);
            _keys = new OrderedListDictionaryKeys<TK, TV>(this, range);
            _values = new OrderedListDictionaryValues<TK, TV>(this, range);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedListDictionary{TK,TV}"/> class.
        /// </summary>
        /// <param name="keyComparer">The key comparer.</param>
        public OrderedListDictionary(IComparer<TK> keyComparer)
        {
            _itemList = new List<KeyValuePair<TK, TV>>();
            _itemComparer = new KeyValuePairComparer(keyComparer, false);

            var range = new BoundRange<TK>(null, null, keyComparer);
            _keys = new OrderedListDictionaryKeys<TK, TV>(this, range);
            _values = new OrderedListDictionaryValues<TK, TV>(this, range);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedListDictionary{TK,TV}"/> class.
        /// </summary>
        public OrderedListDictionary()
        {
            var comparer = Comparers.Default<TK>();
            //var comparer = Comparer<TK>.Default;
            _itemList = new List<KeyValuePair<TK, TV>>();
            _itemComparer = new KeyValuePairComparer(comparer, false);

            var range = new BoundRange<TK>(null, null, comparer);
            _keys = new OrderedListDictionaryKeys<TK, TV>(this, range);
            _values = new OrderedListDictionaryValues<TK, TV>(this, range);
        }

        /// <summary>
        /// Returns the first key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an exception.
        /// </summary>
        public KeyValuePair<TK, TV> FirstEntry {
            get {
                if (_itemList.Count == 0)
                    throw new InvalidOperationException();
                return _itemList[0];
            }
        }

        /// <summary>
        /// Returns the last key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an exception.
        /// </summary>
        public KeyValuePair<TK, TV> LastEntry {
            get {
                if (_itemList.Count == 0)
                    throw new InvalidOperationException();
                return _itemList[_itemList.Count - 1];
            }
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
            return _itemList.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator(
            TK startKey,
            bool isInclusive)
        {
            var index = GetHeadIndex(startKey, isInclusive);
            if (index == -1)
                index = 0;

            for (var ii = index; ii < _itemList.Count; ii++) {
                yield return _itemList[ii];
            }
        }

        public void ForEach(Action<KeyValuePair<TK, TV>> action)
        {
            for (var ii = 0; ii < _itemList.Count; ii++) {
                action.Invoke(_itemList[ii]);
            }
        }

        public void ForEach(Action<int, KeyValuePair<TK, TV>> action)
        {
            for (var ii = 0; ii < _itemList.Count; ii++) {
                action.Invoke(ii, _itemList[ii]);
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
            var keyValuePair = new KeyValuePair<TK, TV>(key, default(TV));
            return _itemList.BinarySearch(keyValuePair, _itemComparer);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<TK, TV> item)
        {
            var index = BinarySearch(item.Key);
            if (index >= 0) {
                throw new ArgumentException("An element with the same key already exists");
            }
            else {
                _itemList.Insert(~index, item);
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _itemList.Clear();
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
        public void CopyTo(
            KeyValuePair<TK, TV>[] array,
            int arrayIndex)
        {
            var arrayLength = array.Length;
            var itemLength = _itemList.Count;
            for (int ii = arrayIndex, listIndex = 0; ii < arrayLength && listIndex < itemLength; ii++, listIndex++) {
                array[ii] = _itemList[listIndex];
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
            _itemList.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _itemList.Count;

        /// <summary>
        /// Returns the count within a range.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public int CountInRange(BoundRange<TK> range)
        {
            return CountInRange(range.Lower, range.Upper);
        }

        /// <summary>
        /// Advances a head index until it reaches a valid point in the range.
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int AdvanceIntoRange(
            int headIndex,
            Bound<TK> start)
        {
            if (headIndex == -1) {
                headIndex = 0;
            }

            while (headIndex < _itemList.Count && !BoundExtensions.IsGreaterThan(start, _itemList[headIndex].Key, _itemComparer.KeyComparer)) {
                headIndex++;
            }

            return headIndex;
        }
        
        /// <summary>
        /// Returns the count within a range.
        /// </summary>
        /// <returns></returns>
        public int CountInRange(
            Bound<TK> start,
            Bound<TK> end)
        {
            var tailIndex = GetTailIndex(end);
            if (tailIndex != -1) {
                if (tailIndex >= _itemList.Count) {
                    tailIndex = _itemList.Count - 1;
                }

                while (tailIndex >= 0 && !BoundExtensions.IsLessThan(end, _itemList[tailIndex].Key, _itemComparer.KeyComparer)) {
                    tailIndex--;
                }

                if (tailIndex == -1) {
                    return 0;
                }

                var headIndex = AdvanceIntoRange(GetHeadIndex(start), start);
                return Math.Max(tailIndex - headIndex + 1, 0);
            }

            return 0;
        }
        
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
        public void Add(
            TK key,
            TV value)
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
        public bool TryGetValue(
            TK key,
            out TV value)
        {
            var index = BinarySearch(key);
            if (index >= 0) {
                value = _itemList[index].Value;
                return true;
            }

            value = default(TV);
            return false;
        }

        public TV TryInsert(
            TK key,
            Func<TV> valueFactory)
        {
            var index = BinarySearch(key);
            if (index >= 0) {
                return _itemList[index].Value;
            }

            var value = valueFactory.Invoke();
            _itemList.Insert(~index, new KeyValuePair<TK, TV>(key, value));
            return value;
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value></value>
        public TV this[TK key] {
            get {
                var index = BinarySearch(key);
                if (index >= 0) {
                    return _itemList[index].Value;
                }

                throw new KeyNotFoundException();
            }
            set {
                var index = BinarySearch(key);
                if (index >= 0) {
                    _itemList[index] = new KeyValuePair<TK, TV>(key, value);
                }
                else {
                    _itemList.Insert(~index, new KeyValuePair<TK, TV>(key, value));
                }
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TK> Keys => _keys;

        /// <summary>
        /// Returns the keys as an ordered collection.
        /// </summary>
        public IOrderedCollection<TK> OrderedKeys => _keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<TV> Values => _values;

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything before the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Head(
            TK value,
            bool isInclusive = false)
        {
            return new OrderedListDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(null, new Bound<TK>(value, isInclusive), KeyComparer));
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Tail(
            TK value,
            bool isInclusive = true)
        {
            return new OrderedListDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(new Bound<TK>(value, isInclusive), null, KeyComparer));
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything between the
        /// two provided values.  Whether each value is included in the range depends
        /// on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Between(
            TK startValue,
            bool isStartInclusive,
            TK endValue,
            bool isEndInclusive)
        {
            return new OrderedListDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(
                    new Bound<TK>(startValue, isStartInclusive),
                    new Bound<TK>(endValue, isEndInclusive),
                    KeyComparer));
        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// ending at the head index.
        /// </summary>
        /// <returns></returns>
        public int GetHeadIndex(Bound<TK> lowerBound)
        {
            if (lowerBound == null)
                return 0;
            return GetHeadIndex(lowerBound.Value, lowerBound.IsInclusive);
        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// ending at the head index.
        /// </summary>
        /// <param name="value">The start value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public int GetHeadIndex(
            TK value,
            bool isInclusive)
        {
            var headIndex = BinarySearch(value);
            if (headIndex >= 0) // key found
            {
                if (isInclusive == false) {
                    headIndex--;
                }
            }
            else {
                headIndex = ~headIndex - 1;
            }

            return headIndex;
        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// starting from tail index.
        /// </summary>
        public int GetTailIndex(Bound<TK> upperBound)
        {
            if (upperBound == null)
                return _itemList.Count - 1;
            return GetTailIndex(upperBound.Value, upperBound.IsInclusive);
        }

        /// <summary>
        /// Gets the index that should be used for an inclusive or exclusive search
        /// starting from tail index.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public int GetTailIndex(
            TK value,
            bool isInclusive)
        {
            var tailIndex = BinarySearch(value);
            if (tailIndex >= 0) // key found
            {
                if (isInclusive == false) {
                    tailIndex++;
                }
            }
            else {
                tailIndex = ~tailIndex;
            }

            return tailIndex;
        }

        public IEnumerable<KeyValuePair<TK, TV>> EnumerateRange(BoundRange<TK> range)
        {
            return EnumerateRange(
                range.Lower,
                range.Upper);
        }

        public IEnumerable<KeyValuePair<TK, TV>> EnumerateRange(
            Bound<TK> start,
            Bound<TK> end)
        {
            var headIndex = AdvanceIntoRange(GetHeadIndex(start), start);
            for (var ii = headIndex; ii < _itemList.Count && BoundExtensions.IsLessThan(end, _itemList[ii].Key, _itemComparer.KeyComparer); ii++) {
                yield return _itemList[ii];
            }
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThanOrEqualTo(TK key)
        {
            return TryGreaterThanOrEqualTo(key, out var valuePair) 
                ? valuePair 
                : default(KeyValuePair<TK, TV>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThanOrEqualTo(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_itemList.Count == 0) {
                valuePair = default;
                return false;
            }

            var index = BinarySearch(key);
            if (index == _itemList.Count) {
                valuePair = default;
                return default; // no values are greater
            }
            if (index >= 0) {
                valuePair = _itemList[index];
                return true;
            }

            // ~index is larger than my item
            // if ~index == 0, all items are greater than key
            // if ~index == count, no items are less than key
            index = ~index;
            if (index == _itemList.Count) {
                valuePair = default;
                return false;
            }

            valuePair = _itemList[index];
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThanOrEqualTo(TK key)
        {
            return TryLessThanOrEqualTo(key, out var valuePair) 
                ? valuePair 
                : default(KeyValuePair<TK, TV>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThanOrEqualTo(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_itemList.Count == 0) {
                valuePair = default;
                return false;
            }

            var index = BinarySearch(key);
            if (index == 0) {
                valuePair = _itemList[index];
                return true;
            }
            
            if (index > 0) {
                valuePair = _itemList[index];
                return false;
            }
            
            // ~index is larger than my item
            // if ~index == 0, no items less than key
            // if ~index == count, all items are less than key
            index = ~index;
            if (index == 0) {
                valuePair = default;
                return false;
            }

            valuePair = _itemList[index - 1];
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThan(TK key)
        {
            return TryGreaterThan(key, out var valuePair) 
                ? valuePair 
                : default(KeyValuePair<TK, TV>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThan(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_itemList.Count == 0) {
                valuePair = default;
            }

            var index = BinarySearch(key);
            if (index == _itemList.Count) {
                valuePair = default; // no values are greater
                return false;
            } 
            if (index == 0) {
                valuePair = _itemList[1]; // note, count is not zero
                return true;
            }
            if (index > 0) {
                valuePair = _itemList[index];
                return true;
            }

            // ~index is larger than my item
            // if ~index == 0, all items are greater than key
            // if ~index == count, no items are less than key
            index = ~index;
            if (index == _itemList.Count) {
                valuePair = default;
                return false;
            }

            valuePair = _itemList[index];
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThan(TK key)
        {
            return TryLessThan(key, out var valuePair) 
                ? valuePair 
                : default(KeyValuePair<TK, TV>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThan(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_itemList.Count == 0) {
                valuePair = default;
                return false;
            }

            var index = BinarySearch(key);
            if (index == 0) {
                valuePair = default;
                return false;
            }
            
            if (index > 0) {
                valuePair = _itemList[index - 1];
                return true;
            }
            
            // ~index is larger than my item
            // if ~index == 0, no items less than key
            // if ~index == count, all items are less than key
            index = ~index;
            if (index == 0) {
                valuePair = default;
                return false;
            }

            valuePair = _itemList[index - 1];
            return true;
        }

        /// <summary>
        /// Returns the first key-value pair in a range.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public KeyValuePair<TK,TV> FirstInRange(BoundRange<TK> range)
        {
            if (range.IsUnbounded) {
                return FirstEntry;
            }

            if (range.Lower == null) {
                return FirstEntry;
            }

            if (range.Lower.IsInclusive) {
                if (!TryGreaterThanOrEqualTo(range.Lower.Value, out var result)) {
                    return result;
                }

                throw new InvalidOperationException();
            }
            else {
                if (!TryGreaterThan(range.Lower.Value, out var result)) {
                    return result;
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns the last key-value pair in a range.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public KeyValuePair<TK, TV> LastInRange(BoundRange<TK> range)
        {
            if (range.IsUnbounded) {
                return LastEntry;
            }

            if (range.Upper == null) {
                return LastEntry;
            }

            if (range.Upper.IsInclusive) {
                if (!TryLessThanOrEqualTo(range.Upper.Value, out var result)) {
                    return result;
                }

                throw new InvalidOperationException();
            }
            else {
                if (!TryLessThan(range.Upper.Value, out var result)) {
                    return result;
                }

                throw new InvalidOperationException();
            }
        }
         
        /// <summary>
        /// Returns an inverted version of the dictionary.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Invert()
        {
            var inverted = _itemComparer.Invert();
            var invertedList = new List<KeyValuePair<TK, TV>>(_itemList);
            invertedList.Reverse();

            return new OrderedListDictionary<TK, TV>(invertedList, inverted);
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
