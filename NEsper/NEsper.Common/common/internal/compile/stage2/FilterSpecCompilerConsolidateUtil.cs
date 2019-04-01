///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	/// <summary>
	/// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
	/// </summary>
	public class FilterSpecCompilerConsolidateUtil
	{
	    protected internal static void Consolidate(FilterSpecParaForgeMap filterParamExprMap, string statementName) {
	        // consolidate or place in a boolean expression (by removing filter spec param from the map)
	        // any filter parameter that feature the same property name and filter operator,
	        // i.e. we are looking for "a!=5 and a!=6"  to transform to "a not in (5,6)" which can match faster
	        // considering that "a not in (5,6) and a not in (7,8)" is "a not in (5, 6, 7, 8)" therefore
	        // we need to consolidate until there is no more work to do
	        var mapOfParams = new Dictionary<Pair<ExprFilterSpecLookupableForge, FilterOperator>, IList<FilterSpecParamForge>>();

	        bool haveConsolidated;
	        do {
	            haveConsolidated = false;
	            mapOfParams.Clear();

	            // sort into buckets of propertyName + filterOperator combination
	            foreach (FilterSpecParamForge currentParam in filterParamExprMap.FilterParams) {
	                ExprFilterSpecLookupableForge lookupable = currentParam.Lookupable;
	                FilterOperator op = currentParam.FilterOperator;
	                Pair<ExprFilterSpecLookupableForge, FilterOperator> key = new Pair<ExprFilterSpecLookupableForge, FilterOperator>(lookupable, op);

	                IList<FilterSpecParamForge> existingParam = mapOfParams.Get(key);
	                if (existingParam == null) {
	                    existingParam = new List<>();
	                    mapOfParams.Put(key, existingParam);
	                }
	                existingParam.Add(currentParam);
	            }

	            foreach (IList<FilterSpecParamForge> entry in mapOfParams.Values()) {
	                if (entry.Count > 1) {
	                    haveConsolidated = true;
	                    Consolidate(entry, filterParamExprMap, statementName);
	                }
	            }
	        }
	        while (haveConsolidated);
	    }

	    // remove duplicate propertyName + filterOperator items making a judgement to optimize or simply remove the optimized form
	    private static void Consolidate(IList<FilterSpecParamForge> items, FilterSpecParaForgeMap filterParamExprMap, string statementName) {
	        FilterOperator op = items[0].FilterOperator;
	        if (op == FilterOperator.NOT_EQUAL) {
	            HandleConsolidateNotEqual(items, filterParamExprMap, statementName);
	        } else {
	            // for all others we simple remove the second optimized form (filter param with same prop name and filter op)
	            // and thus the boolean expression that started this is included
	            for (int i = 1; i < items.Count; i++) {
	                filterParamExprMap.RemoveValue(items.Get(i));
	            }
	        }
	    }

	    // consolidate "val != 3 and val != 4 and val != 5"
	    // to "val not in (3, 4, 5)"
	    private static void HandleConsolidateNotEqual(IList<FilterSpecParamForge> parameters, FilterSpecParaForgeMap filterParamExprMap, string statementName) {
	        IList<FilterSpecParamInValueForge> values = new List<FilterSpecParamInValueForge>();

	        ExprNode lastNotEqualsExprNode = null;
	        foreach (FilterSpecParamForge param in parameters) {
	            if (param is FilterSpecParamConstantForge) {
	                FilterSpecParamConstantForge constantParam = (FilterSpecParamConstantForge) param;
	                object constant = constantParam.FilterConstant;
	                values.Add(new FilterForEvalConstantAnyTypeForge(constant));
	            } else if (param is FilterSpecParamEventPropForge) {
	                FilterSpecParamEventPropForge eventProp = (FilterSpecParamEventPropForge) param;
	                values.Add(new FilterForEvalEventPropForge(eventProp.ResultEventAsName, eventProp.ResultEventProperty,
	                        eventProp.ExprIdentNodeEvaluator, eventProp.IsMustCoerce, Boxing.GetBoxedType(eventProp.CoercionType)));
	            } else if (param is FilterSpecParamEventPropIndexedForge) {
	                FilterSpecParamEventPropIndexedForge eventProp = (FilterSpecParamEventPropIndexedForge) param;
	                values.Add(new FilterForEvalEventPropIndexedForge(eventProp.ResultEventAsName, eventProp.ResultEventIndex, eventProp.ResultEventProperty,
	                        eventProp.EventType, eventProp.IsMustCoerce, Boxing.GetBoxedType(eventProp.CoercionType)));
	            } else {
	                throw new ArgumentException("Unknown filter parameter:" + param.ToString());
	            }

	            lastNotEqualsExprNode = filterParamExprMap.RemoveEntry(param);
	        }

	        FilterSpecParamInForge param = new FilterSpecParamInForge(parameters[0].Lookupable, FilterOperator.NOT_IN_LIST_OF_VALUES, values);
	        filterParamExprMap.Put(lastNotEqualsExprNode, param);
	    }

	}
} // end of namespace