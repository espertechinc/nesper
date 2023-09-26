///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public class FilterSpecCompilerConsolidateUtil
    {
        internal static void Consolidate(
            FilterSpecParaForgeMap filterParamExprMap,
            string statementName)
        {
            // consolidate or place in a boolean expression (by removing filter spec param from the map)
            // any filter parameter that feature the same property name and filter operator,
            // i.e. we are looking for "a!=5 and a!=6"  to transform to "a not in (5,6)" which can match faster
            // considering that "a not in (5,6) and a not in (7,8)" is "a not in (5, 6, 7, 8)" therefore
            // we need to consolidate until there is no more work to do
            var mapOfParams =
                new Dictionary<Pair<ExprFilterSpecLookupableForge, FilterOperator>,
                    IList<FilterSpecPlanPathTripletForge>>();

            bool haveConsolidated;
            do {
                haveConsolidated = false;
                mapOfParams.Clear();

                // sort into buckets of propertyName + filterOperator combination
                foreach (var currenttriplet in filterParamExprMap.Triplets) {
                    var lookupable = currenttriplet.Param.Lookupable;
                    var op = currenttriplet.Param.FilterOperator;
                    var key = new Pair<ExprFilterSpecLookupableForge, FilterOperator>(lookupable, op);

                    var existingParam = mapOfParams.Get(key);
                    if (existingParam == null) {
                        existingParam = new List<FilterSpecPlanPathTripletForge>();
                        mapOfParams.Put(key, existingParam);
                    }

                    existingParam.Add(currenttriplet);
                }

                foreach (IList<FilterSpecPlanPathTripletForge> entry in mapOfParams.Values) {
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
            var eligible = false;
            var op = items[0].Param.FilterOperator;
            if (op == FilterOperator.NOT_EQUAL) {
                eligible = HandleConsolidateNotEqual(items, filterParamExprMap, statementName);
            }

            // for all others we simple remove the second optimized form (filter param with same prop name and filter op)
            // and thus the boolean expression that started this is included
            if (!eligible) {
                for (var i = 1; i < items.Count; i++) {
                    filterParamExprMap.RemoveValue(items[i]);
                }
            }
        }

        // consolidate "val != 3 and val != 4 and val != 5"
        // to "val not in (3, 4, 5)"
        private static bool HandleConsolidateNotEqual(
            IList<FilterSpecPlanPathTripletForge> parameters,
            FilterSpecParaForgeMap filterParamExprMap,
            string statementName)
        {
            // determine eligible
            foreach (var tripletA in parameters) {
                var paramA = tripletA.Param;
                if (paramA is FilterSpecParamConstantForge ||
                    paramA is FilterSpecParamEventPropForge ||
                    paramA is FilterSpecParamEventPropIndexedForge) {
                    continue;
                }

                return false;
            }

            IList<FilterSpecParamInValueForge> values = new List<FilterSpecParamInValueForge>();
            ExprNode lastNotEqualsExprNode = null;

            foreach (var tripletB in parameters) {
                var paramB = tripletB.Param;
                if (paramB is FilterSpecParamConstantForge constantParam) {
                    var constant = constantParam.FilterConstant;
                    values.Add(new FilterForEvalConstantAnyTypeForge(constant));
                }
                else if (paramB is FilterSpecParamEventPropForge prop) {
                    values.Add(
                        new FilterForEvalEventPropForge(
                            prop.ResultEventAsName,
                            prop.ResultEventProperty,
                            prop.ExprIdentNodeEvaluator,
                            prop.IsMustCoerce,
                            prop.CoercionType.GetBoxedType()));
                }
                else if (paramB is FilterSpecParamEventPropIndexedForge eventProp) {
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
                    throw new IllegalStateException("Unknown filter parameter:" + paramB.ToString());
                }

                lastNotEqualsExprNode = filterParamExprMap.RemoveEntry(tripletB);
            }

            var param = new FilterSpecParamInForge(
                parameters[0].Param.Lookupable,
                FilterOperator.NOT_IN_LIST_OF_VALUES,
                values);
            var triplet = new FilterSpecPlanPathTripletForge(param, null);
            filterParamExprMap.Put(lastNotEqualsExprNode, triplet);

            return true;
        }
    }
} // end of namespace