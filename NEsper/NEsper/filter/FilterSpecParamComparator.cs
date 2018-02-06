///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Sort comparator for filter parameters that sorts filter parameters according to filter operator type.
    /// </summary>
    [Serializable]
    public class FilterSpecParamComparator
        : IComparer<FilterOperator>
        , MetaDefItem
    {
        /// <summary>
        /// Defines the sort order among filter operator types. The idea is to sort EQUAL-type operators first 
        /// then RANGE then other operators, ie. sorting from a more restrictive (usually, not necessarily, really 
        /// depends on the client application) to a less restrictive operand.
        /// </summary>
        private static readonly FilterOperator[] SORT_ORDER =
        {
            FilterOperator.EQUAL,
            FilterOperator.IS,
            FilterOperator.IN_LIST_OF_VALUES,
            FilterOperator.ADVANCED_INDEX,
            FilterOperator.RANGE_OPEN,
            FilterOperator.RANGE_HALF_OPEN,
            FilterOperator.RANGE_HALF_CLOSED,
            FilterOperator.RANGE_CLOSED,
            FilterOperator.LESS,
            FilterOperator.LESS_OR_EQUAL,
            FilterOperator.GREATER_OR_EQUAL,
            FilterOperator.GREATER,
            FilterOperator.NOT_RANGE_CLOSED,
            FilterOperator.NOT_RANGE_HALF_CLOSED,
            FilterOperator.NOT_RANGE_HALF_OPEN,
            FilterOperator.NOT_RANGE_OPEN,
            FilterOperator.NOT_IN_LIST_OF_VALUES,
            FilterOperator.NOT_EQUAL,
            FilterOperator.IS_NOT,
            FilterOperator.BOOLEAN_EXPRESSION
        };

        private static readonly int[] FilterSortOrder;

        static FilterSpecParamComparator()
        {
            var values = Enum.GetValues(typeof(FilterOperator));
            FilterSortOrder = new int[values.Length];
            for (int i = 0; i < FilterSortOrder.Length; i++)
            {
                FilterSortOrder[i] = IndexOf((FilterOperator)values.GetValue(i));
            }
        }

        public int Compare(FilterOperator param1, FilterOperator param2)
        {
            // Within the same filter operator type sort by attribute name
            if (param1 == param2)
            {
                return 0;
            }

            // Within different filter operator types sort by the table above
            var opIndex1 = FilterSortOrder[(int)param1];
            var opIndex2 = FilterSortOrder[(int)param2];
            return opIndex1 < opIndex2 ? -1 : 1;
        }

        private static int IndexOf(FilterOperator filterOperator)
        {
            for (int i = 0; i < SORT_ORDER.Length; i++)
            {
                if (SORT_ORDER[i] == filterOperator)
                {
                    return i;
                }
            }

            return SORT_ORDER.Length;
        }
    }
}
