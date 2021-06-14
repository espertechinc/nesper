///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     A comparator on multikeys with string values and using the Collator for comparing. The multikeys must contain the
    ///     same number of values.
    /// </summary>
    public sealed class ComparatorObjectArrayCollating : IComparer<object[]>
    {
        private readonly bool[] isDescendingValues;
        private readonly bool[] stringTypedValue;
        [NonSerialized] private readonly IComparer<object> collator = null;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isDescendingValues">
        ///     each value is true if the corresponding (same index)entry in the multi-keys is to be sorted in descending order.
        ///     The multikeys
        ///     to be compared must have the same number of values as this array.
        /// </param>
        /// <param name="stringTypeValues">true for each string-typed column</param>
        public ComparatorObjectArrayCollating(
            bool[] isDescendingValues,
            bool[] stringTypeValues)
        {
            this.isDescendingValues = isDescendingValues;
            stringTypedValue = stringTypeValues;
            collator = Comparers.Collating();
        }

        public int Compare(
            object[] firstValues,
            object[] secondValues)
        {
            if (firstValues.Length != isDescendingValues.Length || secondValues.Length != isDescendingValues.Length) {
                throw new ArgumentException("Incompatible size MultiKey sizes for comparison");
            }

            for (var i = 0; i < firstValues.Length; i++) {
                var valueOne = firstValues[i];
                var valueTwo = secondValues[i];
                var isDescending = isDescendingValues[i];

                if (!stringTypedValue[i]) {
                    var comparisonResult = CompareValues(valueOne, valueTwo, isDescending);
                    if (comparisonResult != 0) {
                        return comparisonResult;
                    }
                }
                else {
                    var comparisonResult = CollectionUtil.CompareValuesCollated(
                        valueOne,
                        valueTwo,
                        isDescending,
                        collator);
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