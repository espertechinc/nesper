///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Factory for <seealso cref="FilterParamIndexBase" /> instances based on event property name and filter operator
    ///     type.
    /// </summary>
    public class IndexFactory
    {
        /// <summary>
        ///     Factory for indexes that store filter parameter constants for a given event property and filter
        ///     operator.
        ///     <para />
        ///     Does not perform any check of validity of property name.
        /// </summary>
        /// <param name="filterOperator">is the type of index to use</param>
        /// <param name="lockFactory">lock factory</param>
        /// <param name="lookupable">the lookup item</param>
        /// <returns>the proper index based on the filter operator type</returns>
        public static FilterParamIndexBase CreateIndex(
            ExprFilterSpecLookupable lookupable,
            FilterServiceGranularLockFactory lockFactory,
            FilterOperator filterOperator)
        {
            FilterParamIndexBase index;
            var returnValueType = lookupable.ReturnType;

            // Handle all EQUAL comparisons
            if (filterOperator == FilterOperator.EQUAL) {
                index = new FilterParamIndexEquals(lookupable, lockFactory.ObtainNew());
                return index;
            }

            // Handle all NOT-EQUAL comparisons
            if (filterOperator == FilterOperator.NOT_EQUAL) {
                index = new FilterParamIndexNotEquals(lookupable, lockFactory.ObtainNew());
                return index;
            }

            if (filterOperator == FilterOperator.IS) {
                index = new FilterParamIndexEqualsIs(lookupable, lockFactory.ObtainNew());
                return index;
            }

            if (filterOperator == FilterOperator.IS_NOT) {
                index = new FilterParamIndexNotEqualsIs(lookupable, lockFactory.ObtainNew());
                return index;
            }

            // Handle all GREATER, LESS etc. comparisons
            if (filterOperator == FilterOperator.GREATER ||
                filterOperator == FilterOperator.GREATER_OR_EQUAL ||
                filterOperator == FilterOperator.LESS ||
                filterOperator == FilterOperator.LESS_OR_EQUAL) {
                if (returnValueType != typeof(string)) {
                    index = new FilterParamIndexCompare(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                else {
                    index = new FilterParamIndexCompareString(lookupable, lockFactory.ObtainNew(), filterOperator);
                }

                return index;
            }

            // Handle all normal and inverted RANGE comparisons
            if (filterOperator.IsRangeOperator()) {
                if (returnValueType != typeof(string)) {
                    index = new FilterParamIndexDoubleRange(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                else {
                    index = new FilterParamIndexStringRange(lookupable, lockFactory.ObtainNew(), filterOperator);
                }

                return index;
            }

            if (filterOperator.IsInvertedRangeOperator()) {
                if (returnValueType != typeof(string)) {
                    return new FilterParamIndexDoubleRangeInverted(lookupable, lockFactory.ObtainNew(), filterOperator);
                }

                return new FilterParamIndexStringRangeInverted(lookupable, lockFactory.ObtainNew(), filterOperator);
            }

            // Handle all IN and NOT IN comparisons
            if (filterOperator == FilterOperator.IN_LIST_OF_VALUES) {
                return new FilterParamIndexIn(lookupable, lockFactory.ObtainNew());
            }

            if (filterOperator == FilterOperator.NOT_IN_LIST_OF_VALUES) {
                return new FilterParamIndexNotIn(lookupable, lockFactory.ObtainNew());
            }

            
            // Handle re-usable boolean expression
            if (filterOperator == FilterOperator.REBOOL) {
                if (lookupable.ReturnType == null) {
                    return new FilterParamIndexReboolNoValue(lookupable, lockFactory.ObtainNew());
                }
                return new FilterParamIndexReboolWithValue(lookupable, lockFactory.ObtainNew());
            }
            
            // Handle all boolean expression
            if (filterOperator == FilterOperator.BOOLEAN_EXPRESSION) {
                return new FilterParamIndexBooleanExpr(lockFactory.ObtainNew());
            }

            // Handle advanced-index
            if (filterOperator == FilterOperator.ADVANCED_INDEX) {
                var advLookable = (FilterSpecLookupableAdvancedIndex) lookupable;
                if (advLookable.IndexType.Equals(SettingsApplicationDotMethodPointInsideRectangle.INDEXTYPE_NAME)) {
                    return new FilterParamIndexQuadTreePointRegion(lockFactory.ObtainNew(), lookupable);
                }

                if (advLookable.IndexType.Equals(SettingsApplicationDotMethodRectangeIntersectsRectangle.INDEXTYPE_NAME)) {
                    return new FilterParamIndexQuadTreeMXCIF(lockFactory.ObtainNew(), lookupable);
                }

                throw new IllegalStateException("Unrecognized index type " + advLookable.IndexType);
            }

            throw new ArgumentException("Cannot create filter index instance for filter operator " + filterOperator);
        }
    }
} // end of namespace