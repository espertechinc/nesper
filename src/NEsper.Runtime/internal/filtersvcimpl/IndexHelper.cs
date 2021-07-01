///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Utility class for matching filter parameters to indizes. Matches are indicated by the index
    ///     <seealso cref="FilterParamIndexBase" />and the filter parameter <seealso cref="FilterSpecParam" /> featuring the
    ///     same event property name and filter operator.
    /// </summary>
    public class IndexHelper
    {
        /// <summary>
        ///     Find an index that matches one of the filter parameters passed.
        ///     The parameter type and index type match up if the property name and
        ///     filter operator are the same for the index and the filter parameter.
        ///     For instance, for a filter parameter of "count EQUALS 10", the index against property "count" with
        ///     operator type EQUALS will be returned, if present.
        ///     NOTE: The caller is expected to obtain locks, if necessary, on the collections passed in.
        ///     NOTE: Doesn't match non-property based index - thus boolean expressions don't get found and are always entered as a
        ///     new index
        /// </summary>
        /// <param name="parameters">is the list of sorted filter parameters</param>
        /// <param name="indizes">is the collection of indexes</param>
        /// <returns>A matching pair of filter parameter and index, if any matches were found. Null if no matches were found.</returns>
        public static Pair<FilterValueSetParam, FilterParamIndexBase> FindIndex(
            ArrayDeque<FilterValueSetParam> parameters,
            IList<FilterParamIndexBase> indizes)
        {
            foreach (var parameter in parameters) {
                var lookupable = parameter.Lookupable;
                var @operator = parameter.FilterOperator;

                foreach (var index in indizes) {
                    // if property-based index, we prefer this in matching
                    if (index is FilterParamIndexLookupableBase) {
                        var propBasedIndex = (FilterParamIndexLookupableBase) index;
                        if (lookupable.Equals(propBasedIndex.Lookupable) &&
                            @operator.Equals(propBasedIndex.FilterOperator)) {
                            return new Pair<FilterValueSetParam, FilterParamIndexBase>(parameter, index);
                        }
                    }
                    else if (index is FilterParamIndexBooleanExpr && parameters.Count == 1) {
                        // if boolean-expression then match only if this is the last parameter,
                        // all others considered are higher order and sort ahead
                        if (@operator.Equals(FilterOperator.BOOLEAN_EXPRESSION)) {
                            return new Pair<FilterValueSetParam, FilterParamIndexBase>(parameter, index);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Determine among the passed in filter parameters any parameter that matches the given index on property name and
        ///     filter operator type. Returns null if none of the parameters matches the index.
        /// </summary>
        /// <param name="parameters">is the filter parameter list</param>
        /// <param name="index">is a filter parameter constant value index</param>
        /// <returns>filter parameter, or null if no matching parameter found.</returns>
        public static FilterValueSetParam FindParameter(
            ArrayDeque<FilterValueSetParam> parameters,
            FilterParamIndexBase index)
        {
            if (index is FilterParamIndexLookupableBase) {
                var propBasedIndex = (FilterParamIndexLookupableBase) index;
                var indexLookupable = propBasedIndex.Lookupable;
                var indexOperator = propBasedIndex.FilterOperator;

                foreach (var parameter in parameters) {
                    var lookupable = parameter.Lookupable;
                    var paramOperator = parameter.FilterOperator;

                    if (lookupable.Equals(indexLookupable) &&
                        paramOperator.Equals(indexOperator)) {
                        return parameter;
                    }
                }
            }
            else {
                foreach (var parameter in parameters) {
                    var paramOperator = parameter.FilterOperator;

                    if (paramOperator.Equals(index.FilterOperator)) {
                        return parameter;
                    }
                }
            }

            return null;
        }
    }
} // end of namespace