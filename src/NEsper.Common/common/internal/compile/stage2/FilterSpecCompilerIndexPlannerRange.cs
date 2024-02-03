///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerRange
    {
        public static FilterSpecParamForge HandleRangeNode(
            ExprBetweenNode betweenNode,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            string statementName,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            var left = betweenNode.ChildNodes[0];
            ExprFilterSpecLookupableForge lookupable = null;

            if (left is ExprFilterOptimizableNode filterOptimizableNode) {
                lookupable = filterOptimizableNode.FilterLookupable;
            }
            else if (HasLevelOrHint(
                         FilterSpecCompilerIndexPlannerHint.LKUPCOMPOSITE,
                         raw,
                         services) &&
                     IsLimitedLookupableExpression(left)) {
                lookupable = MakeLimitedLookupableForgeMayNull(left, raw, services);
            }

            if (lookupable == null) {
                return null;
            }

            var op = FilterOperatorExtensions.ParseRangeOperator(
                betweenNode.IsLowEndpointIncluded,
                betweenNode.IsHighEndpointIncluded,
                betweenNode.IsNotBetween);

            var low = HandleRangeNodeEndpoint(
                betweenNode.ChildNodes[1],
                taggedEventTypes,
                arrayEventTypes,
                allTagNamesOrdered,
                statementName,
                raw,
                services);
            var high = HandleRangeNodeEndpoint(
                betweenNode.ChildNodes[2],
                taggedEventTypes,
                arrayEventTypes,
                allTagNamesOrdered,
                statementName,
                raw,
                services);
            return low == null || high == null ? null : new FilterSpecParamRangeForge(lookupable, op, low, high);
        }

        private static FilterSpecParamFilterForEvalForge HandleRangeNodeEndpoint(
            ExprNode endpoint,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            string statementName,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            // constant
            if (endpoint.Forge.ForgeConstantType.IsCompileTimeConstant) {
                var value = endpoint.Forge.ExprEvaluator.Evaluate(null, true, null);
                if (value == null) {
                    return null;
                }

                if (value is string s) {
                    return new FilterForEvalConstantStringForge(s);
                }
                else {
                    return new FilterForEvalConstantDoubleForge(value.AsDouble());
                }
            }

            if (endpoint is ExprContextPropertyNode propertyNode) {
                if (propertyNode.ValueType == null) {
                    return null;
                }

                var type = propertyNode.ValueType;
                if (type == typeof(string)) {
                    return new FilterForEvalContextPropStringForge(propertyNode.Getter, propertyNode.PropertyName);
                }
                else {
                    return new FilterForEvalContextPropDoubleForge(propertyNode.Getter, propertyNode.PropertyName);
                }
            }

            if (endpoint.Forge.ForgeConstantType.IsDeployTimeTimeConstant && endpoint is ExprNodeDeployTimeConst node) {
                var type = endpoint.Forge.EvaluationType;
                if (type == typeof(string)) {
                    return new FilterForEvalDeployTimeConstStringForge(node);
                }
                else {
                    return new FilterForEvalDeployTimeConstDoubleForge(node);
                }
            }

            // or property
            if (endpoint is ExprIdentNode identNode) {
                return GetIdentNodeDoubleEval(identNode, arrayEventTypes, statementName);
            }

            // or limited expression
            if (HasLevelOrHint(
                    FilterSpecCompilerIndexPlannerHint.VALUECOMPOSITE,
                    raw,
                    services) &&
                IsLimitedValueExpression(endpoint)) {
                var returnType = endpoint.Forge.EvaluationType;
                if (returnType == null) {
                    return null;
                }

                var returnClass = returnType;
                var convertor = GetMatchEventConvertor(endpoint, taggedEventTypes, arrayEventTypes, allTagNamesOrdered);
                if (returnClass == typeof(string)) {
                    return new FilterForEvalLimitedExprForge(endpoint, convertor, null);
                }

                var coercer = SimpleNumberCoercerFactory.GetCoercer(returnClass, typeof(double?));
                return new FilterForEvalLimitedExprForge(endpoint, convertor, coercer);
            }

            return null;
        }
    }
} // end of namespace