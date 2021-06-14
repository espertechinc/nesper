///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class TransformOrderedDictionary<TK1, TV1, TK2, TV2>
        : TransformDictionary<TK1, TV1, TK2, TV2>
        , IOrderedDictionary<TK1, TV1>
    {
        private readonly IOrderedDictionary<TK2, TV2> _subDictionaryOrdered;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="subDictionary"></param>
        public TransformOrderedDictionary(IOrderedDictionary<TK2, TV2> subDictionary) 
            : base(subDictionary)
        {
            _subDictionaryOrdered = subDictionary;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="subDictionary"></param>
        /// <param name="keyOut"></param>
        /// <param name="keyIn"></param>
        /// <param name="valueOut"></param>
        /// <param name="valueIn"></param>
        public TransformOrderedDictionary(
            IOrderedDictionary<TK2, TV2> subDictionary,
            Func<TK2, TK1> keyOut,
            Func<TK1, TK2> keyIn,
            Func<TV2, TV1> valueOut,
            Func<TV1, TV2> valueIn)
            : base(subDictionary, keyOut, keyIn, valueOut, valueIn)
        {
            _subDictionaryOrdered = subDictionary;
        }

        /// <summary>
        /// Returns a comparer for the key.
        /// </summary>
        public IComparer<TK1> KeyComparer => throw new NotSupportedException();

        /// <summary>
        /// Returns the keys as an ordered collection.
        /// </summary>
        public IOrderedCollection<TK1> OrderedKeys => new TransformOrderedCollection<TK2, TK1>(
            _subDictionaryOrdered.OrderedKeys, KeyIn, KeyOut);

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything before the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK1, TV1> Head(
            TK1 value,
            bool isInclusive = false)
        {
            return new TransformOrderedDictionary<TK1, TV1, TK2, TV2>(
                _subDictionaryOrdered.Head(KeyIn(value), isInclusive),
                KeyOut,
                KeyIn,
                ValueOut,
                ValueIn);
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK1, TV1> Tail(
            TK1 value,
            bool isInclusive = true)
        {
            return new TransformOrderedDictionary<TK1, TV1, TK2, TV2>(
                _subDictionaryOrdered.Tail(KeyIn(value), isInclusive),
                KeyOut,
                KeyIn,
                ValueOut,
                ValueIn);
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything between the
        /// two provided values.  Whether each value is included in the range depends
        /// on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK1, TV1> Between(
            TK1 startValue,
            bool isStartInclusive,
            TK1 endValue,
            bool isEndInclusive)
        {
            return new TransformOrderedDictionary<TK1, TV1, TK2, TV2>(
                _subDictionaryOrdered.Between(
                    KeyIn(startValue), isStartInclusive,
                    KeyIn(endValue), isEndInclusive),
                KeyOut,
                KeyIn,
                ValueOut,
                ValueIn);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK1, TV1>? GreaterThanOrEqualTo(TK1 key)
        {
            var value = _subDictionaryOrdered.GreaterThanOrEqualTo(KeyIn(key));
            return value.HasValue
                ? new KeyValuePair<TK1, TV1>(KeyOut(value.Value.Key), ValueOut(value.Value.Value))
                : default(KeyValuePair<TK1, TV1>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThanOrEqualTo(
            TK1 key,
            out KeyValuePair<TK1, TV1> valuePair)
        {
            if (_subDictionaryOrdered.TryGreaterThanOrEqualTo(KeyIn(key), out var kvp)) {
                valuePair = new KeyValuePair<TK1, TV1>(KeyOut(kvp.Key), ValueOut(kvp.Value));
                return true;
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK1, TV1>? LessThanOrEqualTo(TK1 key)
        {
            var value = _subDictionaryOrdered.LessThanOrEqualTo(KeyIn(key));
            return value.HasValue
                ? new KeyValuePair<TK1, TV1>(
                    KeyOut(value.Value.Key),
                    ValueOut(value.Value.Value))
                : default(KeyValuePair<TK1, TV1>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThanOrEqualTo(
            TK1 key,
            out KeyValuePair<TK1, TV1> valuePair)
        {
            if (_subDictionaryOrdered.TryLessThanOrEqualTo(KeyIn(key), out var kvp)) {
                valuePair = new KeyValuePair<TK1, TV1>(
                    KeyOut(kvp.Key),
                    ValueOut(kvp.Value));
                return true;
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK1, TV1>? GreaterThan(TK1 key)
        {
            var value = _subDictionaryOrdered.GreaterThan(KeyIn(key));
            return value.HasValue
                ? new KeyValuePair<TK1, TV1>(KeyOut(value.Value.Key), ValueOut(value.Value.Value))
                : default(KeyValuePair<TK1, TV1>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThan(
            TK1 key,
            out KeyValuePair<TK1, TV1> valuePair)
        {
            if (_subDictionaryOrdered.TryGreaterThan(KeyIn(key), out var kvp)) {
                valuePair = new KeyValuePair<TK1, TV1>(KeyOut(kvp.Key), ValueOut(kvp.Value));
                return true;
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK1, TV1>? LessThan(TK1 key)
        {
            var value = _subDictionaryOrdered.LessThan(KeyIn(key));
            return value.HasValue
                ? new KeyValuePair<TK1, TV1>(KeyOut(value.Value.Key), ValueOut(value.Value.Value))
                : default(KeyValuePair<TK1, TV1>?);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThan(
            TK1 key,
            out KeyValuePair<TK1, TV1> valuePair)
        {
            if (_subDictionaryOrdered.TryLessThan(KeyIn(key), out var kvp)) {
                valuePair = new KeyValuePair<TK1, TV1>(KeyOut(kvp.Key), ValueOut(kvp.Value));
                return true;
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Returns the first key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an exception.
        /// </summary>
        public KeyValuePair<TK1, TV1> FirstEntry {
            get {
                var value = _subDictionaryOrdered.FirstEntry;
                return new KeyValuePair<TK1, TV1>(
                    KeyOut(value.Key),
                    ValueOut(value.Value));
            }
        }

        /// <summary>
        ///     Returns the last key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an exception.
        /// </summary>
        public KeyValuePair<TK1, TV1> LastEntry {
            get {
                var value = _subDictionaryOrdered.LastEntry;
                return new KeyValuePair<TK1, TV1>(
                    KeyOut(value.Key),
                    ValueOut(value.Value));
            }
        }

        /// <summary>
        /// Returns an ordered dictionary in inverted order.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK1, TV1> Invert()
        {
            return new TransformOrderedDictionary<TK1, TV1, TK2, TV2>(
                _subDictionaryOrdered.Invert(),
                KeyOut,
                KeyIn,
                ValueOut,
                ValueIn);
        }
    }
}