using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections.btree
{
    public class BTreeDictionaryKeys<TK, TV> : IOrderedCollection<TK>
    {
        private readonly BTree<TK, KeyValuePair<TK, TV>> _underlying;
        private readonly BoundRange<TK> _range;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        public BTreeDictionaryKeys(BTree<TK, KeyValuePair<TK, TV>> underlying)
        {
            _underlying = underlying;
            _range = new BoundRange<TK>(null, null, _underlying.KeyComparer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="range"></param>
        public BTreeDictionaryKeys(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            BoundRange<TK> range)
        {
            _underlying = underlying;
            _range = range;
        }

        /// <summary>
        /// Returns the bounded range.
        /// </summary>
        public BoundRange<TK> Range => _range;

        /// <inheritdoc />
        public int Count => BTreeDictionaryExtensions.Count(_underlying, _range);

        /// <inheritdoc />
        public bool IsReadOnly => true;

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<TK> GetEnumerator()
        {
            return BTreeDictionaryExtensions
                .Enumerate(_underlying, _range)
                .Select(_ => _.Key)
                .GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(TK item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains(TK key)
        {
            if (_range.IsWithin(key)) {
                var cursor = _underlying.FindUnique(key, _underlying.RootCursor);
                if (cursor.IsNotEnd) {
                    return Equals(key, cursor.Key);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(
            TK[] array,
            int arrayIndex)
        {
            var enumerable = BTreeDictionaryExtensions
                .Enumerate(_underlying, _range)
                .GetEnumerator();
            while (enumerable.MoveNext() && arrayIndex < array.Length) {
                if (arrayIndex >= 0) {
                    array[arrayIndex] = enumerable.Current.Key;
                }

                arrayIndex++;
            }
        }

        /// <inheritdoc />
        public bool Remove(TK item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the first value in the collection.  If the collection is empty, this method throws
        /// an IllegalOperationException.
        /// </summary>
        public TK FirstEntry {
            get {
                if (_underlying.Count == 0) {
                    throw new InvalidOperationException();
                }

                return BTreeDictionaryExtensions
                    .FirstKeyValuePair(_underlying, _range)
                    .Key;
            }
        }

        /// <summary>
        /// Returns the last value in the collection.  If the collection is empty, this method throws
        /// an IllegalOperationException.
        /// </summary>
        public TK LastEntry {
            get {
                if (_underlying.Count == 0) {
                    throw new InvalidOperationException();
                }

                return BTreeDictionaryExtensions
                    .LastKeyValuePair(_underlying, _range)
                    .Key;
            }
        }
        public IOrderedCollection<TK> Head(
            TK value,
            bool isInclusive = false)
        {
            return new BTreeDictionaryKeys<TK, TV>(
                _underlying,
                _range.Merge(
                    new BoundRange<TK>(
                        null,
                        new Bound<TK>(value, isInclusive),
                        _range.Comparer)));
        }

        public IOrderedCollection<TK> Tail(
            TK value,
            bool isInclusive = true)
        {
            return new BTreeDictionaryKeys<TK, TV>(
                _underlying,
                _range.Merge(
                    new BoundRange<TK>(
                        new Bound<TK>(value, isInclusive),
                        null,
                        _range.Comparer)));
        }

        public IOrderedCollection<TK> Between(
            TK startValue,
            bool isStartInclusive,
            TK endValue,
            bool isEndInclusive)
        {
            return new BTreeDictionaryKeys<TK, TV>(
                _underlying,
                _range.Merge(
                    new BoundRange<TK>(
                        new Bound<TK>(startValue, isStartInclusive),
                        new Bound<TK>(endValue, isEndInclusive),
                        _range.Comparer)));
        }

        public TK GreaterThanOrEqualTo(TK value)
        {
            if (TryGreaterThanOrEqualTo(value, out var result)) {
                return result;
            }
            
            throw new InvalidOperationException();
        }

        public bool TryGreaterThanOrEqualTo(
            TK value,
            out TK result)
        {
            return TryWithCursor(_underlying.GreaterThanOrEqual(value, _underlying.RootCursor), out result);
        }

        public TK LessThanOrEqualTo(TK value)
        {
            if (TryLessThanOrEqualTo(value, out var result)) {
                return result;
            }
            
            throw new InvalidOperationException();
        }

        public bool TryLessThanOrEqualTo(
            TK value,
            out TK result)
        {
            return TryWithCursor(_underlying.LessThanOrEqual(value, _underlying.RootCursor), out result);
        }

        public TK GreaterThan(TK value)
        {
            if (TryGreaterThan(value, out var result)) {
                return result;
            }
            
            throw new InvalidOperationException();
        }

        public bool TryGreaterThan(
            TK value,
            out TK result)
        {
            return TryWithCursor(_underlying.GreaterThan(value, _underlying.RootCursor), out result);
        }

        public TK LessThan(TK value)
        {
            if (TryLessThan(value, out var result)) {
                return result;
            }
            
            throw new InvalidOperationException();
        }

        public bool TryLessThan(
            TK value,
            out TK result)
        {
            return TryWithCursor(_underlying.LessThan(value, _underlying.RootCursor), out result);
        }

        private bool TryWithCursor(
            BTree<TK, KeyValuePair<TK, TV>>.Cursor cursor,
            out TK result)
        {
            if (cursor.IsEnd) {
                result = default;
                return false;
            }

            var cursorKey = cursor.Key;
            if (_range.IsWithin(cursorKey)) {
                result = default;
                return false;
            }
            
            result = cursorKey;
            return true;
        }
    }
}