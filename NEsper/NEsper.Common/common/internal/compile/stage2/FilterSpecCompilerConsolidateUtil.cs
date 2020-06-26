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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public class FilterSpecCompilerConsolidateUtil
    {
        protected internal static void Consolidate(
            FilterSpecParaForgeMap filterParamExprMap,
            string statementName)
        {
            // consolidate or place in a boolean expression (by removing filter spec param from the map)
            // any filter parameter that feature the same property name and filter operator,
            // i.e. we are looking for "a!=5 and a!=6"  to transform to "a not in (5,6)" which can match faster
            // considering that "a not in (5,6) and a not in (7,8)" is "a not in (5, 6, 7, 8)" therefore
            // we need to consolidate until there is no more work to do
            var mapOfParams = new Dictionary<Pair<ExprFilterSpecLookupableForge, FilterOperator>, IList<FilterSpecPlanPathTripletForge>>();

            bool haveConsolidated;
            do {
                haveConsolidated = false;
                mapOfParams.Clear();

                // sort into buckets of propertyName + filterOperator combination
                foreach (var currentTriplet in filterParamExprMap.Triplets) {
                    var lookupable = currentTriplet.Param.Lookupable;
                    var op = currentTriplet.Param.FilterOperator;
                    var key = new Pair<ExprFilterSpecLookupableForge, FilterOperator>(lookupable, op);

                    var existingParam = mapOfParams.Get(key);
                    if (existingParam == null) {
                        existingParam = new List<FilterSpecPlanPathTripletForge>();
                        mapOfParams.Put(key, existingParam);
                    }

                    existingParam.Add(currentTriplet);
                }

                foreach (var entry in mapOfParams.Values) {
                    if (entry.Count > 1) {
                        haveConsolidated = true;
                        Consolidate(entry, filterParamExprMap, statementName);
                    }
                }
            } while (haveConsolidated);
        }

        // remove duplicate propertyName + filterOperator items making a judgement to optimize or simply remove the optimized form
        private static void Consolidate(
            IList<FilterSpecPlanPathTripletForge> items,
            FilterSpecParaForgeMap filterParamExprMap,
            string statementName)
        {
            var op = items[0].Param.FilterOperator;
            if (op == FilterOperator.NOT_EQUAL) {
                HandleConsolidateNotEqual(items, filterParamExprMap, statementName);
            }
            else {
                // for all others we simple remove the second optimized form (filter param with same prop name and filter op)
                // and thus the boolean expression that started this is included
                for (var i = 1; i < items.Count; i++) {
                    filterParamExprMap.RemoveValue(items[i]);
                }
            }
        }

        // consolidate "val != 3 and val != 4 and val != 5"
        // to "val not in (3, 4, 5)"
        private static void HandleConsolidateNotEqual(
            IList<FilterSpecPlanPathTripletForge> parameters,
            FilterSpecParaForgeMap filterParamExprMap,
            string statementName)
        {
            IList<FilterSpecParamInValueForge> values = new List<FilterSpecParamInValueForge>();

            ExprNode lastNotEqualsExprNode = null;
            foreach (FilterSpecPlanPathTripletForge triplet in parameters) {
                FilterSpecParamForge paramForge = triplet.Param;
                if (paramForge is FilterSpecParamConstantForge) {
                    var constantParam = (FilterSpecParamConstantForge) paramForge;
                    var constant = constantParam.FilterConstant;
                    values.Add(new FilterForEvalConstantAnyTypeForge(constant));
                }
                else if (paramForge is FilterSpecParamEventPropForge) {
                    var eventProp = (FilterSpecParamEventPropForge) paramForge;
                    values.Add(
                        new FilterForEvalEventPropForge(
                            eventProp.ResultEventAsName,
                            eventProp.ResultEventProperty,
                            eventProp.ExprIdentNodeEvaluator,
                            eventProp.IsMustCoerce,
                            eventProp.CoercionType.GetBoxedType()));
                }
                else if (paramForge is FilterSpecParamEventPropIndexedForge) {
                    var eventProp = (FilterSpecParamEventPropIndexedForge) paramForge;
                    values.Add(
                        new FilterForEvalEventPropIndexedForge(
                            eventProp.ResultEventAsName,
                            eventProp.ResultEventIndex,
                            eventProp.ResultEventProperty,
                            eventProp.EventType,
                            eventProp.IsMustCoerce,
                            eventProp.CoercionType.GetBoxedType()));
                }
                else {
                    throw new ArgumentException("Unknown filter parameter:" + paramForge);
                }

                lastNotEqualsExprNode = filterParamExprMap.RemoveEntry(triplet);
            }

            FilterSpecParamInForge param = new FilterSpecParamInForge(
                parameters[0].Param.Lookupable, FilterOperator.NOT_IN_LIST_OF_VALUES, values);
            FilterSpecPlanPathTripletForge tripletForge = new FilterSpecPlanPathTripletForge(param, null);
            filterParamExprMap.Put(lastNotEqualsExprNode, tripletForge);
        }
    }
} // end of namespace