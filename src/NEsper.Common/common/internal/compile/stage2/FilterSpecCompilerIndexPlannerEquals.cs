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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerEquals
    {
        internal static FilterSpecParamForge HandleEqualsAndRelOp(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            string statementName,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            FilterOperator op;
            if (constituent is ExprEqualsNode equalsNode) {
                if (!equalsNode.IsIs) {
                    op = FilterOperator.EQUAL;
                    if (equalsNode.IsNotEquals) {
                        op = FilterOperator.NOT_EQUAL;
                    }
                }
                else {
                    op = FilterOperator.IS;
                    if (equalsNode.IsNotEquals) {
                        op = FilterOperator.IS_NOT;
                    }
                }
            }
            else {
                var relNode = (ExprRelationalOpNode)constituent;
                if (relNode.RelationalOpEnum == RelationalOpEnum.GT) {
                    op = FilterOperator.GREATER;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.LT) {
                    op = FilterOperator.LESS;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.LE) {
                    op = FilterOperator.LESS_OR_EQUAL;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.GE) {
                    op = FilterOperator.GREATER_OR_EQUAL;
                }
                else {
                    throw new IllegalStateException("Opertor '" + relNode.RelationalOpEnum + "' not mapped");
                }
            }

            var left = constituent.ChildNodes[0];
            var right = constituent.ChildNodes[1];

            // check identifier and constant combination
            if (right.Forge.ForgeConstantType.IsCompileTimeConstant && left is ExprFilterOptimizableNode node) {
                if (node.IsFilterLookupEligible) {
                    var lookupable = node.FilterLookupable;
                    var constant = right.Forge.ExprEvaluator.Evaluate(null, true, null);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    return new FilterSpecParamConstantForge(lookupable, op, constant);
                }
            }

            if (left.Forge.ForgeConstantType.IsCompileTimeConstant &&
                right is ExprFilterOptimizableNode optimizableNode) {
                if (optimizableNode.IsFilterLookupEligible) {
                    var lookupable = optimizableNode.FilterLookupable;
                    var constant = left.Forge.ExprEvaluator.Evaluate(null, true, null);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    var opReversed = op.IsComparisonOperator() ? op.ReversedRelationalOp() : op;
                    return new FilterSpecParamConstantForge(lookupable, opReversed, constant);
                }
            }

            // check identifier and expression containing other streams
            if (left is ExprIdentNode identNodeLeft && right is ExprIdentNode identNodeRight) {
                if (identNodeLeft.StreamId == 0 &&
                    identNodeLeft.IsFilterLookupEligible &&
                    identNodeRight.StreamId != 0) {
                    return HandleProperty(op, identNodeLeft, identNodeRight, arrayEventTypes, statementName);
                }

                if (identNodeRight.StreamId == 0 &&
                    identNodeRight.IsFilterLookupEligible &&
                    identNodeLeft.StreamId != 0) {
                    op = GetReversedOperator(
                        constituent,
                        op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    return HandleProperty(op, identNodeRight, identNodeLeft, arrayEventTypes, statementName);
                }
            }

            if (left is ExprFilterOptimizableNode exprFilterOptimizableNode &&
                right is ExprContextPropertyNode propertyNode) {
                var lookupable = exprFilterOptimizableNode.FilterLookupable;
                if (exprFilterOptimizableNode.IsFilterLookupEligible) {
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType,
                        propertyNode.ValueType,
                        lookupable.Expression);
                    return new FilterSpecParamContextPropForge(
                        lookupable,
                        op,
                        propertyNode.PropertyName,
                        propertyNode.Getter,
                        numberCoercer);
                }
            }

            if (left is ExprContextPropertyNode ctxNode && right is ExprFilterOptimizableNode filterOptimizableNode1) {
                var lookupable = filterOptimizableNode1.FilterLookupable;
                if (filterOptimizableNode1.IsFilterLookupEligible) {
                    op = GetReversedOperator(
                        constituent,
                        op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType,
                        ctxNode.ValueType,
                        lookupable.Expression);
                    return new FilterSpecParamContextPropForge(
                        lookupable,
                        op,
                        ctxNode.PropertyName,
                        ctxNode.Getter,
                        numberCoercer);
                }
            }

            if (left is ExprFilterOptimizableNode node1 &&
                right.Forge.ForgeConstantType.IsDeployTimeTimeConstant &&
                right is ExprNodeDeployTimeConst @const) {
                var lookupable = node1.FilterLookupable;
                if (node1.IsFilterLookupEligible) {
                    var returnType = right.Forge.EvaluationType;
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, returnType, lookupable.Expression);
                    return new FilterSpecParamDeployTimeConstParamForge(
                        lookupable,
                        op,
                        @const,
                        returnType,
                        numberCoercer);
                }
            }

            if (left.Forge.ForgeConstantType.IsDeployTimeTimeConstant &&
                left is ExprNodeDeployTimeConst deployTimeConst &&
                right is ExprFilterOptimizableNode filterOptimizableNode) {
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.IsFilterLookupEligible) {
                    var returnType = left.Forge.EvaluationType;
                    op = GetReversedOperator(
                        constituent,
                        op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, returnType, lookupable.Expression);
                    return new FilterSpecParamDeployTimeConstParamForge(
                        lookupable,
                        op,
                        deployTimeConst,
                        returnType,
                        numberCoercer);
                }
            }

            // check lookable-limited and value-limited expression
            ExprNode lookupableOuter = null;
            ExprNode value = null;
            var opWReverse = op;
            if (IsLimitedLookupableExpression(left) && IsLimitedValueExpression(right)) {
                lookupableOuter = left;
                value = right;
            }
            else if (IsLimitedLookupableExpression(right) && IsLimitedValueExpression(left)) {
                lookupableOuter = right;
                value = left;
                opWReverse = GetReversedOperator(constituent, op);
            }

            if (lookupableOuter != null) {
                return HandleLimitedExpr(
                    opWReverse,
                    lookupableOuter,
                    value,
                    taggedEventTypes,
                    arrayEventTypes,
                    allTagNamesOrdered,
                    raw,
                    services);
            }

            return null;
        }

        private static FilterSpecParamForge HandleLimitedExpr(
            FilterOperator op,
            ExprNode lookupable,
            ExprNode value,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            ExprFilterSpecLookupableForge lookupableForge;
            var lookupableType = lookupable.Forge.EvaluationType;
            var valueType = value.Forge.EvaluationType;
            if (lookupable is ExprIdentNode identNode) {
                if (!HasLevelOrHint(
                        FilterSpecCompilerIndexPlannerHint.VALUECOMPOSITE,
                        raw,
                        services)) {
                    return null;
                }

                if (!identNode.IsFilterLookupEligible) {
                    return null;
                }

                lookupableForge = identNode.FilterLookupable;
            }
            else {
                if (!HasLevelOrHint(
                        FilterSpecCompilerIndexPlannerHint.LKUPCOMPOSITE,
                        raw,
                        services)) {
                    return null;
                }

                lookupableForge = MakeLimitedLookupableForgeMayNull(lookupable, raw, services);
                if (lookupableForge == null) {
                    return null;
                }
            }

            var convertor = GetMatchEventConvertor(value, taggedEventTypes, arrayEventTypes, allTagNamesOrdered);
            var numberCoercer = GetNumberCoercer(lookupableType, valueType, lookupableForge.Expression);
            return new FilterSpecParamValueLimitedExprForge(lookupableForge, op, value, convertor, numberCoercer);
        }

        private static FilterOperator GetReversedOperator(
            ExprNode constituent,
            FilterOperator op)
        {
            if (!(constituent is ExprRelationalOpNode relNode)) {
                return op;
            }

            var relationalOpEnum = relNode.RelationalOpEnum;

            if (relationalOpEnum == RelationalOpEnum.GT) {
                return FilterOperator.LESS;
            }
            else if (relationalOpEnum == RelationalOpEnum.LT) {
                return FilterOperator.GREATER;
            }
            else if (relationalOpEnum == RelationalOpEnum.LE) {
                return FilterOperator.GREATER_OR_EQUAL;
            }
            else if (relationalOpEnum == RelationalOpEnum.GE) {
                return FilterOperator.LESS_OR_EQUAL;
            }

            return op;
        }

        private static FilterSpecParamForge HandleProperty(
            FilterOperator op,
            ExprIdentNode identNodeLeft,
            ExprIdentNode identNodeRight,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            var propertyName = identNodeLeft.ResolvedPropertyName;

            var leftType = identNodeLeft.Forge.EvaluationType;
            var rightType = identNodeRight.Forge.EvaluationType;

            var numberCoercer = GetNumberCoercer(leftType, rightType, propertyName);
            var isMustCoerce = numberCoercer != null;
            var numericCoercionType = leftType.GetBoxedType();

            var streamName = identNodeRight.ResolvedStreamName;
            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
                var innerEventType = GetArrayInnerEventType(arrayEventTypes, streamName);
                var indexAndProp = GetStreamIndex(identNodeRight.ResolvedPropertyName);
                return new FilterSpecParamEventPropIndexedForge(
                    identNodeLeft.FilterLookupable,
                    op,
                    identNodeRight.ResolvedStreamName,
                    indexAndProp.First,
                    indexAndProp.Second,
                    innerEventType,
                    isMustCoerce,
                    numberCoercer,
                    numericCoercionType);
            }

            return new FilterSpecParamEventPropForge(
                identNodeLeft.FilterLookupable,
                op,
                identNodeRight.ResolvedStreamName,
                identNodeRight.ResolvedPropertyName,
                identNodeRight.ExprEvaluatorIdent,
                isMustCoerce,
                numberCoercer,
                numericCoercionType);
        }
    }
} // end of namespace