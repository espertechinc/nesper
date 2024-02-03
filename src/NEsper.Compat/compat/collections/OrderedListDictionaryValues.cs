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
    public class OrderedListDictionaryValues<TK, TV> : ICollection<TV>
    {
        private readonly OrderedListDictionary<TK, TV> _underlying;
        private readonly BoundRange<TK> _range;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        public OrderedListDictionaryValues(OrderedListDictionary<TK, TV> underlying)
        {
            _underlying = underlying;
            _range = new BoundRange<TK>(null, null, _underlying.KeyComparer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="range"></param>
        public OrderedListDictionaryValues(
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

        public IEnumerator<TV> GetEnumerator()
        {
            return _underlying
                .EnumerateRange(_range)
                .Select(_ => _.Value)
                .GetEnumerator();
        }

        public void Add(TV item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(TV value)
        {
            return _underlying
                .EnumerateRange(_range)
                .Any(kvp => Equals(kvp.Value, value));
        }

        public void CopyTo(
            TV[] array,
            int arrayIndex)
        {
            var enumerable = _underlying
                .EnumerateRange(_range)
                .GetEnumerator();
            while (enumerable.MoveNext() && arrayIndex < array.Length) {
                if (arrayIndex >= 0) {
                    array[arrayIndex] = enumerable.Current.Value;
                }

                arrayIndex++;
            }
        }

        public bool Remove(TV item)
        {
            throw new NotSupportedException();
        }
    }
}