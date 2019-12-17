///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     A comparator on multikeys with string values and using the Collator for comparing. The multikeys must contain the
    ///     same number of values.
    /// </summary>
    [Serializable]
    public sealed class ComparatorHashableMultiKeyCollating : IComparer<HashableMultiKey>
    {
        [NonSerialized] private readonly IComparer<object> _collator;
        private readonly bool[] _isDescendingValues;
        private readonly bool[] _stringTypedValue;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isDescendingValues">
        ///     each value is true if the corresponding (same index) entry in the multi-keys is
        ///     to be sorted in descending order. The multikeys to be compared must have the same
        ///     number of values as this array.
        /// </param>
        /// <param name="stringTypeValues">true for each string-typed column</param>
        public ComparatorHashableMultiKeyCollating(
            bool[] isDescendingValues,
            bool[] stringTypeValues)
        {
            _isDescendingValues = isDescendingValues;
            _stringTypedValue = stringTypeValues;
            _collator = Comparers.Collating();
        }

        public int Compare(
            HashableMultiKey firstValues,
            HashableMultiKey secondValues)
        {
            if (firstValues.Count != _isDescendingValues.Length ||
                secondValues.Count != _isDescendingValues.Length) {
                throw new ArgumentException("Incompatible size MultiKey sizes for comparison");
            }

            for (var i = 0; i < firstValues.Count; i++) {
                var valueOne = firstValues.Get(i);
                var valueTwo = secondValues.Get(i);
                var isDescending = _isDescendingValues[i];

                if (!_stringTypedValue[i]) {
                    var comparisonResult = CompareValues(valueOne, valueTwo, isDescending);
                    if (comparisonResult != 0) {
                        return comparisonResult;
                    }
                }
                else {
                    var comparisonResult = CompareValuesCollated(
                        valueOne,
                        valueTwo,
                        isDescending,
                        _collator);
                    if (comparisonResult != 0) {
                        return comparisonResult;
                    }
                }
            }

            // Make the comparator compatible with equals
            if (!firstValues.Equals(secondValues)) {
                return -1;
            }

            return 0;
        }
    }
} // end of namespace