using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections.btree
{
    public class BTreeDictionaryValues<TK, TV> : ICollection<TV>
    {
        private readonly BTree<TK, KeyValuePair<TK, TV>> _underlying;
        private readonly BoundRange<TK> _range;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        public BTreeDictionaryValues(BTree<TK, KeyValuePair<TK, TV>> underlying)
        {
            _underlying = underlying;
            _range = new BoundRange<TK>(null, null, _underlying.KeyComparer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="range"></param>
        public BTreeDictionaryValues(
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
        public IEnumerator<TV> GetEnumerator()
        {
            return BTreeDictionaryExtensions
                .Enumerate(_underlying, _range)
                .Select(_ => _.Value)
                .GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(TV item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains(TV value)
        {
            return BTreeDictionaryExtensions
                .Enumerate(_underlying, _range)
                .Any(kvp => Equals(kvp.Value, value));
        }

        /// <inheritdoc />
        public void CopyTo(
            TV[] array,
            int arrayIndex)
        {
            var enumerable = BTreeDictionaryExtensions
                .Enumerate(_underlying, _range)
                .GetEnumerator();
            while (enumerable.MoveNext() && arrayIndex < array.Length) {
                if (arrayIndex >= 0) {
                    array[arrayIndex] = enumerable.Current.Value;
                }

                arrayIndex++;
            }
        }

        /// <inheritdoc />
        public bool Remove(TV item)
        {
            throw new NotSupportedException();
        }
    }
}