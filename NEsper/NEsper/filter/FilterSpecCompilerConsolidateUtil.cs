///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
	/// <summary>
	/// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
	/// </summary>
	public class FilterSpecCompilerConsolidateUtil
	{
	    public static void Consolidate(FilterParamExprMap filterParamExprMap, string statementName)
	    {
	        // consolidate or place in a boolean expression (by removing filter spec param from the map)
	        // any filter parameter that feature the same property name and filter operator,
	        // i.e. we are looking for "a!=5 and a!=6"  to transform to "a not in (5,6)" which can match faster
	        // considering that "a not in (5,6) and a not in (7,8)" is "a not in (5, 6, 7, 8)" therefore
	        // we need to consolidate until there is no more work to do
	        IDictionary<Pair<FilterSpecLookupable, FilterOperator>, IList<FilterSpecParam>> mapOfParams = new Dictionary<Pair<FilterSpecLookupable, FilterOperator>, IList<FilterSpecParam>>();

	        bool haveConsolidated;
	        do
	        {
	            haveConsolidated = false;
	            mapOfParams.Clear();

	            // sort into buckets of propertyName + filterOperator combination
	            foreach (var currentParam in filterParamExprMap.FilterParams)
	            {
	                var lookupable = currentParam.Lookupable;
	                var op = currentParam.FilterOperator;
	                var key = new Pair<FilterSpecLookupable, FilterOperator>(lookupable, op);

	                var existingParam = mapOfParams.Get(key);
	                if (existingParam == null)
	                {
	                    existingParam = new List<FilterSpecParam>();
	                    mapOfParams.Put(key, existingParam);
	                }
	                existingParam.Add(currentParam);
	            }

	            foreach (var entry in mapOfParams.Values)
	            {
	                if (entry.Count > 1)
	                {
	                    haveConsolidated = true;
	                    Consolidate(entry, filterParamExprMap, statementName);
	                }
	            }
	        }
	        while(haveConsolidated);
	    }

	    // remove duplicate propertyName + filterOperator items making a judgement to optimize or simply remove the optimized form
	    private static void Consolidate(IList<FilterSpecParam> items, FilterParamExprMap filterParamExprMap, string statementName)
	    {
	        var op = items[0].FilterOperator;
	        if (op == FilterOperator.NOT_EQUAL)
	        {
	            HandleConsolidateNotEqual(items, filterParamExprMap, statementName);
	        }
	        else
	        {
	            // for all others we simple remove the second optimized form (filter param with same prop name and filter op)
	            // and thus the boolean expression that started this is included
	            for (var i = 1; i < items.Count; i++)
	            {
	                filterParamExprMap.RemoveValue(items[i]);
	            }
	        }
	    }

	    // consolidate "val != 3 and val != 4 and val != 5"
	    // to "val not in (3, 4, 5)"
	    private static void HandleConsolidateNotEqual(IList<FilterSpecParam> parameters, FilterParamExprMap filterParamExprMap, string statementName)
	    {
	        IList<FilterSpecParamInValue> values = new List<FilterSpecParamInValue>();

	        ExprNode lastNotEqualsExprNode = null;
	        foreach (var param in parameters)
	        {
	            if (param is FilterSpecParamConstant)
	            {
	                var constantParam = (FilterSpecParamConstant) param;
	                var constant = constantParam.FilterConstant;
	                values.Add(new FilterForEvalConstantAnyType(constant));
	            }
	            else if (param is FilterSpecParamEventProp)
	            {
	                var eventProp = (FilterSpecParamEventProp) param;
	                values.Add(new FilterForEvalEventPropMayCoerce(eventProp.ResultEventAsName, eventProp.ResultEventProperty,
	                        eventProp.IsMustCoerce, TypeHelper.GetBoxedType(eventProp.CoercionType)));
	            }
	            else if (param is FilterSpecParamEventPropIndexed)
	            {
	                var eventProp = (FilterSpecParamEventPropIndexed) param;
	                values.Add(new FilterForEvalEventPropIndexedMayCoerce(eventProp.ResultEventAsName, eventProp.ResultEventIndex, eventProp.ResultEventProperty,
	                        eventProp.IsMustCoerce, TypeHelper.GetBoxedType(eventProp.CoercionType), statementName));
	            }
	            else
	            {
	                throw new ArgumentException("Unknown filter parameter:" + param.ToString());
	            }

	            lastNotEqualsExprNode = filterParamExprMap.RemoveEntry(param);
	        }

	        var paramIn = new FilterSpecParamIn(parameters[0].Lookupable, FilterOperator.NOT_IN_LIST_OF_VALUES, values);
	        filterParamExprMap.Put(lastNotEqualsExprNode, paramIn);
	    }
	}
} // end of namespace
