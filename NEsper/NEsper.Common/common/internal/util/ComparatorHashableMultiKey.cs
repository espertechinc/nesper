///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     A comparator on multikeys. The multikeys must contain the same number of values.
    /// </summary>
    public class ComparatorHashableMultiKey : IComparer<HashableMultiKey>
    {
        private readonly bool[] isDescendingValues;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isDescendingValues">
        ///     each value is true if the corresponding (same index)entry in the multi-keys is to be sorted in descending order.
        ///     The multikeys
        ///     to be compared must have the same number of values as this array.
        /// </param>
        public ComparatorHashableMultiKey(bool[] isDescendingValues)
        {
            this.isDescendingValues = isDescendingValues;
        }

        public int Compare(
            HashableMultiKey firstValues,
            HashableMultiKey secondValues)
        {
            if (firstValues.Count != isDescendingValues.Length || secondValues.Count != isDescendingValues.Length) {
                throw new ArgumentException("Incompatible size MultiKey sizes for comparison");
            }

            for (var i = 0; i < firstValues.Count; i++) {
                var valueOne = firstValues.Get(i);
                var valueTwo = secondValues.Get(i);
                var isDescending = isDescendingValues[i];

                var comparisonResult = CompareValues(valueOne, valueTwo, isDescending);
                if (comparisonResult != 0) {
                    return comparisonResult;
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