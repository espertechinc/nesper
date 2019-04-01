///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.epl.index.quadtree;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Factory for <seealso cref="FilterParamIndexBase"/> instances based on event property name and filter operator type.
    /// </summary>
    public class IndexFactory
    {
        /// <summary>
        /// Factory for indexes that store filter parameter constants for a given event property and filter operator.
        /// <para />
        /// Does not perform any check of validity of property name.
        /// </summary>
        /// <param name="lookupable">The lookupable.</param>
        /// <param name="lockFactory">The lock factory.</param>
        /// <param name="filterOperator">is the type of index to use</param>
        /// <returns>
        /// the proper index based on the filter operator type
        /// </returns>
        /// <exception cref="System.ArgumentException">Cannot create filter index instance for filter operator  + filterOperator</exception>
        public static FilterParamIndexBase CreateIndex(
            FilterSpecLookupable lookupable, 
            FilterServiceGranularLockFactory lockFactory, 
            FilterOperator filterOperator)
        {
            FilterParamIndexBase index;
            Type returnValueType = lookupable.ReturnType;
    
            // Handle all EQUAL comparisons
            if (filterOperator == FilterOperator.EQUAL)
            {
                index = new FilterParamIndexEquals(lookupable, lockFactory.ObtainNew());
                return index;
            }
    
            // Handle all NOT-EQUAL comparisons
            if (filterOperator == FilterOperator.NOT_EQUAL)
            {
                index = new FilterParamIndexNotEquals(lookupable, lockFactory.ObtainNew());
                return index;
            }
    
            if (filterOperator == FilterOperator.IS)
            {
                index = new FilterParamIndexEqualsIs(lookupable, lockFactory.ObtainNew());
                return index;
            }
    
            if (filterOperator == FilterOperator.IS_NOT)
            {
                index = new FilterParamIndexNotEqualsIs(lookupable, lockFactory.ObtainNew());
                return index;
            }
    
            // Handle all GREATER, LESS etc. comparisons
            if ((filterOperator == FilterOperator.GREATER) ||
                (filterOperator == FilterOperator.GREATER_OR_EQUAL) ||
                (filterOperator == FilterOperator.LESS) ||
                (filterOperator == FilterOperator.LESS_OR_EQUAL))
            {
                if (returnValueType != typeof(String)) {
                    index = new FilterParamIndexCompare(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                else {
                    index = new FilterParamIndexCompareString(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                return index;
            }
    
            // Handle all normal and inverted RANGE comparisons
            if (filterOperator.IsRangeOperator())
            {
                if (returnValueType != typeof(String)) {
                    index = new FilterParamIndexDoubleRange(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                else {
                    index = new FilterParamIndexStringRange(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                return index;
            }
            if (filterOperator.IsInvertedRangeOperator())
            {
                if (returnValueType != typeof(String)) {
                    return new FilterParamIndexDoubleRangeInverted(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
                else {
                    return new FilterParamIndexStringRangeInverted(lookupable, lockFactory.ObtainNew(), filterOperator);
                }
            }
    
            // Handle all IN and NOT IN comparisons
            if (filterOperator == FilterOperator.IN_LIST_OF_VALUES)
            {
                return new FilterParamIndexIn(lookupable, lockFactory.ObtainNew());
            }
            if (filterOperator == FilterOperator.NOT_IN_LIST_OF_VALUES)
            {
                return new FilterParamIndexNotIn(lookupable, lockFactory.ObtainNew());
            }
    
            // Handle all bool expression
            if (filterOperator == FilterOperator.BOOLEAN_EXPRESSION)
            {
                return new FilterParamIndexBooleanExpr(lockFactory.ObtainNew());
            }

            // Handle advanced-index
            if (filterOperator == FilterOperator.ADVANCED_INDEX)
            {
                FilterSpecLookupableAdvancedIndex advLookable = (FilterSpecLookupableAdvancedIndex)lookupable;
                if (advLookable.IndexType.Equals(EngineImportApplicationDotMethodPointInsideRectangle.INDEX_TYPE_NAME))
                {
                    return new FilterParamIndexQuadTreePointRegion(lockFactory.ObtainNew(), lookupable);
                }
                else if (advLookable.IndexType.Equals(EngineImportApplicationDotMethodRectangeIntersectsRectangle.INDEX_TYPE_NAME))
                {
                    return new FilterParamIndexQuadTreeMXCIF(lockFactory.ObtainNew(), lookupable);
                }
                else
                {
                    throw new IllegalStateException("Unrecognized index type " + advLookable.IndexType);
                }
            }

            throw new ArgumentException("Cannot create filter index instance for filter operator " + filterOperator);
        }
    }
}
