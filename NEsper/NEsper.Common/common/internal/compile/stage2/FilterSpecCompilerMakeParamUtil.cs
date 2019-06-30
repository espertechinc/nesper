///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public class FilterSpecCompilerMakeParamUtil
    {
        /// <summary>
        ///     For a given expression determine if this is optimizable and create the filter parameter
        ///     representing the expression, or null if not optimizable.
        /// </summary>
        /// <param name="constituent">is the expression to look at</param>
        /// <param name="arrayEventTypes">event types that provide array values</param>
        /// <param name="statementName">statement name</param>
        /// <returns>filter parameter representing the expression, or null</returns>
        /// <throws>ExprValidationException if the expression is invalid</throws>
        protected internal static FilterSpecParamForge MakeFilterParam(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if (constituent is ExprEqualsNode || constituent is ExprRelationalOpNode) {
                var param = HandleEqualsAndRelOp(constituent, arrayEventTypes, statementName);
                if (param != null) {
                    return param;
                }
            }

            constituent = RewriteOrToInIfApplicable(constituent);

            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if (constituent is ExprInNode) {
                var param = HandleInSetNode((ExprInNode) constituent, arrayEventTypes);
                if (param != null) {
                    return param;
                }
            }

            if (constituent is ExprBetweenNode) {
                var param = HandleRangeNode((ExprBetweenNode) constituent, arrayEventTypes, statementName);
                if (param != null) {
                    return param;
                }
            }

            if (constituent is ExprPlugInSingleRowNode) {
                var param = HandlePlugInSingleRow((ExprPlugInSingleRowNode) constituent);
                if (param != null) {
                    return param;
                }
            }

            if (constituent is FilterSpecCompilerAdvIndexDescProvider) {
                var param = HandleAdvancedIndexDescProvider(
                    (FilterSpecCompilerAdvIndexDescProvider) constituent, arrayEventTypes, statementName);
                if (param != null) {
                    return param;
                }
            }

            return null;
        }

        private static FilterSpecParamForge HandleAdvancedIndexDescProvider(
            FilterSpecCompilerAdvIndexDescProvider provider,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            var filterDesc = provider.FilterSpecDesc;
            if (filterDesc == null) {
                return null;
            }

            var keyExpressions = filterDesc.KeyExpressions;
            var xGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[0]);
            var yGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[1]);
            var widthGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[2]);
            var heightGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[3]);
            var config = (AdvancedIndexConfigContextPartitionQuadTree) filterDesc.IndexSpec;

            var builder = new StringWriter();
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[0], builder);
            builder.Write(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[1], builder);
            builder.Write(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[2], builder);
            builder.Write(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[3], builder);
            builder.Write("/");
            builder.Write(filterDesc.IndexName.ToLowerInvariant());
            builder.Write("/");
            builder.Write(filterDesc.IndexType.ToLowerInvariant());
            builder.Write("/");
            config.ToConfiguration(builder);
            var expression = builder.ToString();

            Type returnType;
            switch (filterDesc.IndexType) {
                case SettingsApplicationDotMethodPointInsideRectangle.INDEXTYPE_NAME:
                    returnType = typeof(XYPoint);
                    break;

                case SettingsApplicationDotMethodRectangeIntersectsRectangle.INDEXTYPE_NAME:
                    returnType = typeof(XYWHRectangle);
                    break;

                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }

            var lookupable = new FilterSpecLookupableAdvancedIndexForge(
                expression, null, returnType, config, xGetter, yGetter, widthGetter, heightGetter,
                filterDesc.IndexType);

            var indexExpressions = filterDesc.IndexExpressions;
            var xEval = ResolveFilterIndexDoubleEval(
                filterDesc.IndexName, indexExpressions[0], arrayEventTypes, statementName);
            var yEval = ResolveFilterIndexDoubleEval(
                filterDesc.IndexName, indexExpressions[1], arrayEventTypes, statementName);
            switch (filterDesc.IndexType) {
                case SettingsApplicationDotMethodPointInsideRectangle.INDEXTYPE_NAME:
                    return new FilterSpecParamAdvancedIndexQuadTreePointRegionForge(
                        lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval);

                case SettingsApplicationDotMethodRectangeIntersectsRectangle.INDEXTYPE_NAME:
                    var widthEval = ResolveFilterIndexDoubleEval(
                        filterDesc.IndexName, indexExpressions[2], arrayEventTypes, statementName);
                    var heightEval = ResolveFilterIndexDoubleEval(
                        filterDesc.IndexName, indexExpressions[3], arrayEventTypes, statementName);
                    return new FilterSpecParamAdvancedIndexQuadTreeMXCIFForge(
                        lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval, widthEval, heightEval);

                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }
        }

        private static FilterSpecParamFilterForEvalDoubleForge ResolveFilterIndexDoubleEval(
            string indexName,
            ExprNode indexExpression,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            FilterSpecParamFilterForEvalDoubleForge resolved = null;
            if (indexExpression is ExprIdentNode) {
                resolved = GetIdentNodeDoubleEval((ExprIdentNode) indexExpression, arrayEventTypes, statementName);
            }
            else if (indexExpression is ExprContextPropertyNode) {
                var node = (ExprContextPropertyNode) indexExpression;
                resolved = new FilterForEvalContextPropDoubleForge(node.Getter, node.PropertyName);
            }
            else if (indexExpression.Forge.ForgeConstantType.IsCompileTimeConstant) {
                var d = indexExpression.Forge.ExprEvaluator.Evaluate(null, true, null).AsDouble();
                resolved = new FilterForEvalConstantDoubleForge(d);
            }
            else if (indexExpression.Forge.ForgeConstantType.IsConstant) {
                resolved = new FilterForEvalConstRuntimeExprForge(indexExpression);
            }

            if (resolved != null) {
                return resolved;
            }

            throw new ExprValidationException(
                "Invalid filter-indexable expression '" +
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(indexExpression) + "' in respect to index '" +
                indexName + "': expected either a constant, context-builtin or property from a previous pattern match");
        }

        private static EventPropertyGetterSPI ResolveFilterIndexRequiredGetter(
            string indexName,
            ExprNode keyExpression)
        {
            if (!(keyExpression is ExprIdentNode)) {
                throw new ExprValidationException(
                    "Invalid filter-index lookup expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(keyExpression) +
                    "' in respect to index '" + indexName + "': expected an event property name");
            }

            return ((ExprIdentNode) keyExpression).ExprEvaluatorIdent.Getter;
        }

        private static FilterSpecParamForge HandlePlugInSingleRow(ExprPlugInSingleRowNode constituent)
        {
            if (constituent.Forge.EvaluationType.GetBoxedType() != typeof(bool?)) {
                return null;
            }

            if (!constituent.FilterLookupEligible) {
                return null;
            }

            var lookupable = constituent.FilterLookupable;
            return new FilterSpecParamConstantForge(lookupable, FilterOperator.EQUAL, true);
        }

        private static FilterSpecParamForge HandleEqualsAndRelOp(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            FilterOperator op;
            if (constituent is ExprEqualsNode) {
                var equalsNode = (ExprEqualsNode) constituent;
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
                var relNode = (ExprRelationalOpNode) constituent;
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
            if (right.Forge.ForgeConstantType.IsCompileTimeConstant && left is ExprFilterOptimizableNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                if (filterOptimizableNode.FilterLookupEligible) {
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = right.Forge.ExprEvaluator.Evaluate(null, true, null);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    return new FilterSpecParamConstantForge(lookupable, op, constant);
                }
            }

            if (left.Forge.ForgeConstantType.IsCompileTimeConstant && right is ExprFilterOptimizableNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                if (filterOptimizableNode.FilterLookupEligible) {
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = left.Forge.ExprEvaluator.Evaluate(null, true, null);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    var opReversed = op.IsComparisonOperator() ? op.ReversedRelationalOp() : op;
                    return new FilterSpecParamConstantForge(lookupable, opReversed, constant);
                }
            }

            // check identifier and expression containing other streams
            if (left is ExprIdentNode && right is ExprIdentNode) {
                var identNodeLeft = (ExprIdentNode) left;
                var identNodeRight = (ExprIdentNode) right;

                if (identNodeLeft.StreamId == 0 && identNodeLeft.FilterLookupEligible && identNodeRight.StreamId != 0) {
                    return HandleProperty(op, identNodeLeft, identNodeRight, arrayEventTypes, statementName);
                }

                if (identNodeRight.StreamId == 0 && identNodeRight.FilterLookupEligible &&
                    identNodeLeft.StreamId != 0) {
                    op = GetReversedOperator(
                        constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    return HandleProperty(op, identNodeRight, identNodeLeft, arrayEventTypes, statementName);
                }
            }

            if (left is ExprFilterOptimizableNode && right is ExprContextPropertyNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var ctxNode = (ExprContextPropertyNode) right;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType, ctxNode.Type, lookupable.Expression);
                    return new FilterSpecParamContextPropForge(lookupable, op, ctxNode.Getter, numberCoercer);
                }
            }

            if (left is ExprContextPropertyNode && right is ExprFilterOptimizableNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                var ctxNode = (ExprContextPropertyNode) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    op = GetReversedOperator(
                        constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType, ctxNode.Type, lookupable.Expression);
                    return new FilterSpecParamContextPropForge(lookupable, op, ctxNode.Getter, numberCoercer);
                }
            }

            if (left is ExprFilterOptimizableNode && right.Forge.ForgeConstantType.IsDeployTimeTimeConstant &&
                right is ExprNodeDeployTimeConst) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var deployTimeConst = (ExprNodeDeployTimeConst) right;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    var returnType = right.Forge.EvaluationType;
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, returnType, lookupable.Expression);
                    return new FilterSpecParamDeployTimeConstParamForge(
                        lookupable, op, deployTimeConst, returnType, numberCoercer);
                }
            }

            if (left.Forge.ForgeConstantType.IsDeployTimeTimeConstant && left is ExprNodeDeployTimeConst &&
                right is ExprFilterOptimizableNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                var deployTimeConst = (ExprNodeDeployTimeConst) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    var returnType = left.Forge.EvaluationType;
                    op = GetReversedOperator(
                        constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, returnType, lookupable.Expression);
                    return new FilterSpecParamDeployTimeConstParamForge(
                        lookupable, op, deployTimeConst, returnType, numberCoercer);
                }
            }

            return null;
        }

        private static FilterOperator GetReversedOperator(
            ExprNode constituent,
            FilterOperator op)
        {
            if (!(constituent is ExprRelationalOpNode)) {
                return op;
            }

            var relNode = (ExprRelationalOpNode) constituent;
            var relationalOpEnum = relNode.RelationalOpEnum;

            if (relationalOpEnum == RelationalOpEnum.GT) {
                return FilterOperator.LESS;
            }

            if (relationalOpEnum == RelationalOpEnum.LT) {
                return FilterOperator.GREATER;
            }

            if (relationalOpEnum == RelationalOpEnum.LE) {
                return FilterOperator.GREATER_OR_EQUAL;
            }

            if (relationalOpEnum == RelationalOpEnum.GE) {
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
                    identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName, indexAndProp.First,
                    indexAndProp.Second, innerEventType, isMustCoerce, numberCoercer, numericCoercionType,
                    statementName);
            }

            return new FilterSpecParamEventPropForge(
                identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName,
                identNodeRight.ResolvedPropertyName, identNodeRight.ExprEvaluatorIdent,
                isMustCoerce, numberCoercer, numericCoercionType, statementName);
        }

        private static EventType GetArrayInnerEventType(
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string streamName)
        {
            var arrayEventType = arrayEventTypes.Get(streamName);
            var prop = ((MapEventType) arrayEventType.First).Types.Get(streamName);
            return ((EventType[]) prop)[0];
        }

        private static SimpleNumberCoercer GetNumberCoercer(
            Type leftType,
            Type rightType,
            string expression)
        {
            var numericCoercionType = leftType.GetBoxedType();
            if (rightType != leftType) {
                if (rightType.IsNumeric()) {
                    if (!rightType.CanCoerce(leftType)) {
                        ThrowConversionError(rightType, leftType, expression);
                    }

                    return SimpleNumberCoercerFactory.GetCoercer(rightType, numericCoercionType);
                }
            }

            return null;
        }

        private static Pair<int, string> GetStreamIndex(string resolvedPropertyName)
        {
            var property = PropertyParser.ParseAndWalkLaxToSimple(resolvedPropertyName);
            if (!(property is NestedProperty)) {
                throw new IllegalStateException(
                    "Expected a nested property providing an index for array match '" + resolvedPropertyName + "'");
            }

            var nested = (NestedProperty) property;
            if (nested.Properties.Count < 2) {
                throw new IllegalStateException(
                    "Expected a nested property name for array match '" + resolvedPropertyName + "', none found");
            }

            if (!(nested.Properties[0] is IndexedProperty)) {
                throw new IllegalStateException(
                    "Expected an indexed property for array match '" + resolvedPropertyName +
                    "', please provide an index");
            }

            var index = ((IndexedProperty) nested.Properties[0]).Index;
            nested.Properties.RemoveAt(0);
            var writer = new StringWriter();
            nested.ToPropertyEPL(writer);
            return new Pair<int, string>(index, writer.ToString());
        }

        private static void ThrowConversionError(
            Type fromType,
            Type toType,
            string propertyName)
        {
            var text = "Implicit conversion from datatype '" +
                       fromType.Name +
                       "' to '" +
                       toType.Name +
                       "' for property '" +
                       propertyName +
                       "' is not allowed (strict filter type coercion)";
            throw new ExprValidationException(text);
        }

        // expressions automatically coerce to the most upwards type
        // filters require the same type
        private static object HandleConstantsCoercion(
            ExprFilterSpecLookupableForge lookupable,
            object constant)
        {
            var identNodeType = lookupable.ReturnType;
            if (!identNodeType.IsNumeric()) {
                return constant; // no coercion required, other type checking performed by expression this comes from
            }

            if (constant == null) {
                // null constant type
                return null;
            }

            if (!constant.GetType().CanCoerce(identNodeType)) {
                ThrowConversionError(constant.GetType(), identNodeType, lookupable.Expression);
            }

            var identNodeTypeBoxed = identNodeType.GetBoxedType();
            return TypeHelper.CoerceBoxed(constant, identNodeTypeBoxed);
        }

        public static ExprNode RewriteOrToInIfApplicable(ExprNode constituent)
        {
            if (!(constituent is ExprOrNode) || constituent.ChildNodes.Length < 2) {
                return constituent;
            }

            // check eligibility
            var childNodes = constituent.ChildNodes;
            foreach (var child in childNodes) {
                if (!(child is ExprEqualsNode)) {
                    return constituent;
                }

                var equalsNode = (ExprEqualsNode) child;
                if (equalsNode.IsIs || equalsNode.IsNotEquals) {
                    return constituent;
                }
            }

            // find common-expression node
            ExprNode commonExpressionNode;
            var lhs = childNodes[0].ChildNodes[0];
            var rhs = childNodes[0].ChildNodes[1];
            if (ExprNodeUtilityCompare.DeepEquals(lhs, rhs, false)) {
                return constituent;
            }

            if (IsExprExistsInAllEqualsChildNodes(childNodes, lhs)) {
                commonExpressionNode = lhs;
            }
            else if (IsExprExistsInAllEqualsChildNodes(childNodes, rhs)) {
                commonExpressionNode = rhs;
            }
            else {
                return constituent;
            }

            // build node
            var @in = new ExprInNodeImpl(false);
            @in.AddChildNode(commonExpressionNode);
            for (var i = 0; i < constituent.ChildNodes.Length; i++) {
                var child = constituent.ChildNodes[i];
                var nodeindex = ExprNodeUtilityCompare.DeepEquals(
                    commonExpressionNode, childNodes[i].ChildNodes[0], false)
                    ? 1
                    : 0;
                @in.AddChildNode(child.ChildNodes[nodeindex]);
            }

            // validate
            try {
                @in.ValidateWithoutContext();
            }
            catch (ExprValidationException) {
                return constituent;
            }

            return @in;
        }

        private static bool IsExprExistsInAllEqualsChildNodes(
            ExprNode[] childNodes,
            ExprNode search)
        {
            foreach (var child in childNodes) {
                var lhs = child.ChildNodes[0];
                var rhs = child.ChildNodes[1];
                if (!ExprNodeUtilityCompare.DeepEquals(lhs, search, false) &&
                    !ExprNodeUtilityCompare.DeepEquals(rhs, search, false)) {
                    return false;
                }

                if (ExprNodeUtilityCompare.DeepEquals(lhs, rhs, false)) {
                    return false;
                }
            }

            return true;
        }

        private static FilterSpecParamForge HandleRangeNode(
            ExprBetweenNode betweenNode,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            var left = betweenNode.ChildNodes[0];
            if (left is ExprFilterOptimizableNode) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                FilterOperator op = FilterOperatorExtensions.ParseRangeOperator(
                    betweenNode.IsLowEndpointIncluded, betweenNode.IsHighEndpointIncluded,
                    betweenNode.IsNotBetween);

                var low = HandleRangeNodeEndpoint(betweenNode.ChildNodes[1], arrayEventTypes, statementName);
                var high = HandleRangeNodeEndpoint(betweenNode.ChildNodes[2], arrayEventTypes, statementName);

                if (low != null && high != null) {
                    return new FilterSpecParamRangeForge(lookupable, op, low, high);
                }
            }

            return null;
        }

        private static FilterSpecParamFilterForEvalForge HandleRangeNodeEndpoint(
            ExprNode endpoint,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            // constant
            if (endpoint.Forge.ForgeConstantType.IsCompileTimeConstant) {
                var value = endpoint.Forge.ExprEvaluator.Evaluate(null, true, null);
                if (value == null) {
                    return null;
                }

                if (value is string) {
                    return new FilterForEvalConstantStringForge((string) value);
                }

                return new FilterForEvalConstantDoubleForge(value.AsDouble());
            }

            if (endpoint is ExprContextPropertyNode) {
                var node = (ExprContextPropertyNode) endpoint;
                if (node.Type == typeof(string)) {
                    return new FilterForEvalContextPropStringForge(node.Getter, node.PropertyName);
                }

                return new FilterForEvalContextPropDoubleForge(node.Getter, node.PropertyName);
            }

            if (endpoint.Forge.ForgeConstantType.IsDeployTimeTimeConstant && endpoint is ExprNodeDeployTimeConst) {
                var node = (ExprNodeDeployTimeConst) endpoint;
                if (endpoint.Forge.EvaluationType == typeof(string)) {
                    return new FilterForEvalDeployTimeConstStringForge(node);
                }

                return new FilterForEvalDeployTimeConstDoubleForge(node);
            }

            // or property
            if (endpoint is ExprIdentNode) {
                return GetIdentNodeDoubleEval((ExprIdentNode) endpoint, arrayEventTypes, statementName);
            }

            return null;
        }

        private static FilterSpecParamFilterForEvalDoubleForge GetIdentNodeDoubleEval(
            ExprIdentNode node,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            if (node.StreamId == 0) {
                return null;
            }

            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() &&
                arrayEventTypes.ContainsKey(node.ResolvedStreamName)) {
                var indexAndProp = GetStreamIndex(node.ResolvedPropertyName);
                var eventType = GetArrayInnerEventType(arrayEventTypes, node.ResolvedStreamName);
                return new FilterForEvalEventPropIndexedDoubleForge(
                    node.ResolvedStreamName, indexAndProp.First, indexAndProp.Second, eventType);
            }

            return new FilterForEvalEventPropDoubleForge(
                node.ResolvedStreamName, node.ResolvedPropertyName, node.ExprEvaluatorIdent);
        }

        private static FilterSpecParamForge HandleInSetNode(
            ExprInNode constituent,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes)
        {
            var left = constituent.ChildNodes[0];
            if (!(left is ExprFilterOptimizableNode)) {
                return null;
            }

            var filterOptimizableNode = (ExprFilterOptimizableNode) left;
            var lookupable = filterOptimizableNode.FilterLookupable;
            var op = FilterOperator.IN_LIST_OF_VALUES;
            if (constituent.IsNotIn) {
                op = FilterOperator.NOT_IN_LIST_OF_VALUES;
            }

            var expectedNumberOfConstants = constituent.ChildNodes.Length - 1;
            IList<FilterSpecParamInValueForge> listofValues = new List<FilterSpecParamInValueForge>();
            IEnumerator<ExprNode> it = Arrays.AsList(constituent.ChildNodes).GetEnumerator();
            it.MoveNext(); // ignore the first node as it's the identifier
            while (it.MoveNext()) {
                var subNode = it.Current;
                if (subNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    var constant = subNode.Forge.ExprEvaluator.Evaluate(null, true, null);
                    if (constant != null) {
                        if (constant.GetType().IsGenericCollection()) {
                            return null;
                        }

                        if (constant.GetType().IsGenericDictionary()) {
                            return null;
                        }
                    }

                    if (constant != null && constant is Array constantArray) {
                        for (var i = 0; i < constantArray.Length; i++) {
                            object arrayElement = constantArray.GetValue(i);
                            var arrayElementCoerced = HandleConstantsCoercion(lookupable, arrayElement);
                            listofValues.Add(new FilterForEvalConstantAnyTypeForge(arrayElementCoerced));
                            if (i > 0) {
                                expectedNumberOfConstants++;
                            }
                        }
                    }
                    else {
                        constant = HandleConstantsCoercion(lookupable, constant);
                        listofValues.Add(new FilterForEvalConstantAnyTypeForge(constant));
                    }
                }
                else if (subNode is ExprContextPropertyNode) {
                    var contextPropertyNode = (ExprContextPropertyNode) subNode;
                    var returnType = contextPropertyNode.Type;
                    SimpleNumberCoercer coercer;
                    if (returnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                    }
                    else {
                        coercer = GetNumberCoercer(
                            left.Forge.EvaluationType, contextPropertyNode.Type, lookupable.Expression);
                    }

                    var finalReturnType = coercer != null ? coercer.ReturnType : returnType;
                    listofValues.Add(
                        new FilterForEvalContextPropForge(
                            contextPropertyNode.PropertyName, contextPropertyNode.Getter, coercer, finalReturnType));
                }
                else if (subNode.Forge.ForgeConstantType.IsDeployTimeTimeConstant &&
                         subNode is ExprNodeDeployTimeConst) {
                    var deployTimeConst = (ExprNodeDeployTimeConst) subNode;
                    var returnType = subNode.Forge.EvaluationType;
                    SimpleNumberCoercer coercer;
                    if (returnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                    }
                    else {
                        coercer = GetNumberCoercer(left.Forge.EvaluationType, returnType, lookupable.Expression);
                    }

                    listofValues.Add(new FilterForEvalDeployTimeConstForge(deployTimeConst, coercer, returnType));
                }

                if (subNode is ExprIdentNode) {
                    var identNodeInner = (ExprIdentNode) subNode;
                    if (identNodeInner.StreamId == 0) {
                        break; // for same event evals use the boolean expression, via count compare failing below
                    }

                    var isMustCoerce = false;
                    var coerceToType = lookupable.ReturnType.GetBoxedType();
                    var identReturnType = identNodeInner.Forge.EvaluationType;

                    if (identReturnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(identReturnType, lookupable.ReturnType, lookupable.Expression);
                        coerceToType = identReturnType;
                        // no action
                    }
                    else if (identReturnType != lookupable.ReturnType) {
                        if (lookupable.ReturnType.IsNumeric()) {
                            if (!identReturnType.CanCoerce(lookupable.ReturnType)) {
                                ThrowConversionError(identReturnType, lookupable.ReturnType, lookupable.Expression);
                            }

                            isMustCoerce = true;
                        }
                        else {
                            break; // assumed not compatible
                        }
                    }

                    FilterSpecParamInValueForge inValue;
                    var streamName = identNodeInner.ResolvedStreamName;
                    if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() &&
                        arrayEventTypes.ContainsKey(streamName)) {
                        var indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                        var innerEventType = GetArrayInnerEventType(arrayEventTypes, streamName);
                        inValue = new FilterForEvalEventPropIndexedForge(
                            identNodeInner.ResolvedStreamName, indexAndProp.First,
                            indexAndProp.Second, innerEventType, isMustCoerce, coerceToType);
                    }
                    else {
                        inValue = new FilterForEvalEventPropForge(
                            identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName,
                            identNodeInner.ExprEvaluatorIdent, isMustCoerce, coerceToType);
                    }

                    listofValues.Add(inValue);
                }
            }

            // Fallback if not all values in the in-node can be resolved to properties or constants
            if (listofValues.Count == expectedNumberOfConstants) {
                return new FilterSpecParamInForge(lookupable, op, listofValues);
            }

            return null;
        }

        private static void CheckArrayCoercion(
            Type returnTypeValue,
            Type returnTypeLookupable,
            string propertyName)
        {
            if (returnTypeValue == null || !returnTypeValue.IsArray) {
                return;
            }

            if (!returnTypeLookupable.IsArrayTypeCompatible(returnTypeValue.GetElementType())) {
                ThrowConversionError(returnTypeValue.GetElementType(), returnTypeLookupable, propertyName);
            }
        }
    }
} // end of namespace