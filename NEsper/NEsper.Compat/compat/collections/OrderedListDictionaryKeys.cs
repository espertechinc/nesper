using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections
{
    public class OrderedListDictionaryKeys<TK, TV> : ICollection<TK>
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

        /// <inheritdoc />
        public int Count => _underlying.CountInRange(_range);

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
            return _underlying
                .EnumerateRange(_range)
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
            return _range.IsWithin(key) && _underlying.ContainsKey(key);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Remove(TK item)
        {
            throw new NotSupportedException();
        }
    }
}