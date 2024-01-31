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
using System.Linq;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections
{
    public class OrderedListDictionaryKeys<TK, TV> : IOrderedCollection<TK>
    {
        private readonly OrderedListDictionary<TK, TV> _underlying;
        private readonly BoundRange<TK> _range;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        public OrderedListDictionaryKeys(OrderedListDictionary<TK, TV> underlying)
        {
            _underlying = underlying;
            _range = new BoundRange<TK>(null, null, _underlying.KeyComparer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="range"></param>
        public OrderedListDictionaryKeys(
            OrderedListDictionary<TK, TV> underlying,
            BoundRange<TK> range)
        {
            _underlying = underlying;
            _range = range;
        }

        /// <summary>
        /// Returns the bounded range.
        /// </summary>
        public BoundRange<TK> Range => _range;

        public int Count => _underlying.CountInRange(_range);

        public bool IsReadOnly => true;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TK> GetEnumerator()
        {
            return _underlying
                .EnumerateRange(_range)
                .Select(_ => _.Key)
                .GetEnumerator();
        }

        public void Add(TK item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(TK key)
        {
            return _range.IsWithin(key) && _underlying.ContainsKey(key);
        }

        public void CopyTo(
            TK[] array,
            int arrayIndex)
        {
            var enumerable = _underlying
                .EnumerateRange( _range)
                .GetEnumerator();
            while (enumerable.MoveNext() && arrayIndex < array.Length) {
                if (arrayIndex >= 0) {
                    array[arrayIndex] = enumerable.Current.Key;
                }

                arrayIndex++;
            }
        }

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

                return _underlying
                    .FirstInRange(_range)
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

                return _underlying
                    .LastInRange(_range)
                    .Key;
            }
        }
        public IOrderedCollection<TK> Head(
            TK value,
            bool isInclusive = false)
        {
            return new OrderedListDictionaryKeys<TK, TV>(
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
            return new OrderedListDictionaryKeys<TK, TV>(
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
            return new OrderedListDictionaryKeys<TK, TV>(
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
            if (_underlying.TryGreaterThanOrEqualTo(value, out var pair)) {
                if (_range.IsWithin(pair.Key)) {
                    result = pair.Key;
                    return true;
                }
            }

            result = default;
            return false;
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
            if (_underlying.TryLessThanOrEqualTo(value, out var pair)) {
                if (_range.IsWithin(pair.Key)) {
                    result = pair.Key;
                    return true;
                }
            }

            result = default;
            return false;
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
            if (_underlying.TryGreaterThan(value, out var pair)) {
                if (_range.IsWithin(pair.Key)) {
                    result = pair.Key;
                    return true;
                }
            }

            result = default;
            return false;
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
            if (_underlying.TryLessThan(value, out var pair)) {
                if (_range.IsWithin(pair.Key)) {
                    result = pair.Key;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}