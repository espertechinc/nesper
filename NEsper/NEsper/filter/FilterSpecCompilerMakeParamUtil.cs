///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.events.property;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.pointregion;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public sealed class FilterSpecCompilerMakeParamUtil
    {
        /// <summary>
        /// For a given expression determine if this is optimizable and create the filter parameter
        /// representing the expression, or null if not optimizable.
        /// </summary>
        /// <param name="constituent">is the expression to look at</param>
        /// <param name="arrayEventTypes">event types that provide array values</param>
        /// <param name="statementName">statement name</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <exception cref="ExprValidationException">if the expression is invalid</exception>
        /// <returns>filter parameter representing the expression, or null</returns>
        internal static FilterSpecParam MakeFilterParam(
            ExprNode constituent, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            ExprEvaluatorContext exprEvaluatorContext, 
            string statementName)
        {
            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if ((constituent is ExprEqualsNode) ||
                (constituent is ExprRelationalOpNode)) {
                var param = HandleEqualsAndRelOp(constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null) {
                    return param;
                }
            }
    
            constituent = RewriteOrToInIfApplicable(constituent);
    
            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if (constituent is ExprInNode) {
                var param = HandleInSetNode((ExprInNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null) {
                    return param;
                }
            }
    
            if (constituent is ExprBetweenNode) {
                var param = HandleRangeNode((ExprBetweenNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
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
                var param = HandleAdvancedIndexDescProvider((FilterSpecCompilerAdvIndexDescProvider) constituent, arrayEventTypes, statementName, exprEvaluatorContext);
                if (param != null) {
                    return param;
                }
            }
    
            return null;
        }
    
        private static FilterSpecParam HandleAdvancedIndexDescProvider(
            FilterSpecCompilerAdvIndexDescProvider provider, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            string statementName, 
            ExprEvaluatorContext exprEvaluatorContext)
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
            ExprNodeUtility.ToExpressionString(keyExpressions[0], builder);
            builder.Write(",");
            ExprNodeUtility.ToExpressionString(keyExpressions[1], builder);
            builder.Write(",");
            ExprNodeUtility.ToExpressionString(keyExpressions[2], builder);
            builder.Write(",");
            ExprNodeUtility.ToExpressionString(keyExpressions[3], builder);
            builder.Write("/");
            builder.Write(filterDesc.IndexName.ToLower());
            builder.Write("/");
            builder.Write(filterDesc.IndexType.ToLower());
            builder.Write("/");
            config.ToConfiguration(builder);
            var expression = builder.ToString();
    
            Type returnType;
            switch (filterDesc.IndexType) {
                case EngineImportApplicationDotMethodPointInsideRectangle.INDEX_TYPE_NAME:
                    returnType = typeof(XYPoint);
                    break;
                case EngineImportApplicationDotMethodRectangeIntersectsRectangle.INDEX_TYPE_NAME:
                    returnType = typeof(XYWHRectangle);
                    break;
                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }

            var lookupable = new FilterSpecLookupableAdvancedIndex(expression, null, returnType, config, xGetter, yGetter, widthGetter, heightGetter, filterDesc.IndexType);

            var indexExpressions = filterDesc.IndexExpressions;
            var xEval = ResolveFilterIndexDoubleEval(filterDesc.IndexName, indexExpressions[0], arrayEventTypes, statementName, exprEvaluatorContext);
            var yEval = ResolveFilterIndexDoubleEval(filterDesc.IndexName, indexExpressions[1], arrayEventTypes, statementName, exprEvaluatorContext);
            switch (filterDesc.IndexType) {
                case EngineImportApplicationDotMethodPointInsideRectangle.INDEX_TYPE_NAME:
                    return new FilterSpecParamAdvancedIndexQuadTreePointRegion(lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval);
                case EngineImportApplicationDotMethodRectangeIntersectsRectangle.INDEX_TYPE_NAME:
                    var widthEval = ResolveFilterIndexDoubleEval(filterDesc.IndexName, indexExpressions[2], arrayEventTypes, statementName, exprEvaluatorContext);
                    var heightEval = ResolveFilterIndexDoubleEval(filterDesc.IndexName, indexExpressions[3], arrayEventTypes, statementName, exprEvaluatorContext);
                    return new FilterSpecParamAdvancedIndexQuadTreeMXCIF(lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval, widthEval, heightEval);
                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }
        }
    
        private static FilterSpecParamFilterForEvalDouble ResolveFilterIndexDoubleEval(
            string indexName, 
            ExprNode indexExpression, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            string statementName, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            FilterSpecParamFilterForEvalDouble resolved = null;
            if (indexExpression is ExprIdentNode) {
                resolved = GetIdentNodeDoubleEval((ExprIdentNode) indexExpression, arrayEventTypes, statementName);
            } else if (indexExpression is ExprContextPropertyNode) {
                var node = (ExprContextPropertyNode) indexExpression;
                resolved = new FilterForEvalContextPropDouble(node.Getter, node.PropertyName);
            } else if (ExprNodeUtility.IsConstantValueExpr(indexExpression)) {
                var constantNode = (ExprConstantNode) indexExpression;
                var d = constantNode.GetConstantValue(exprEvaluatorContext).AsDouble();
                resolved = new FilterForEvalConstantDouble(d);
            }
            if (resolved != null) {
                return resolved;
            }
            throw new ExprValidationException("Invalid filter-indexable expression '" + indexExpression.ToExpressionStringMinPrecedenceSafe() + "' in respect to index '" + indexName + "': expected either a constant, context-builtin or property from a previous pattern match");
        }
    
        private static EventPropertyGetter ResolveFilterIndexRequiredGetter(string indexName, ExprNode keyExpression)
        {
            if (!(keyExpression is ExprIdentNode)) {
                throw new ExprValidationException("Invalid filter-index lookup expression '" + keyExpression.ToExpressionStringMinPrecedenceSafe() + "' in respect to index '" + indexName + "': expected an event property name");
            }
            return ((ExprIdentNode) keyExpression).ExprEvaluatorIdent.Getter;
        }
    
        public static ExprNode RewriteOrToInIfApplicable(ExprNode constituent)
        {
            if (!(constituent is ExprOrNode) || constituent.ChildNodes.Count < 2) {
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
            if (ExprNodeUtility.DeepEquals(lhs, rhs, false)) {
                return constituent;
            }
            if (IsExprExistsInAllEqualsChildNodes(childNodes, lhs)) {
                commonExpressionNode = lhs;
            } else if (IsExprExistsInAllEqualsChildNodes(childNodes, rhs)) {
                commonExpressionNode = rhs;
            } else {
                return constituent;
            }
    
            // build node
            var @in = new ExprInNodeImpl(false);
            @in.AddChildNode(commonExpressionNode);
            for (var i = 0; i < constituent.ChildNodes.Count; i++) {
                var child = constituent.ChildNodes[i];
                var nodeindex = ExprNodeUtility.DeepEquals(commonExpressionNode, childNodes[i].ChildNodes[0], false) ? 1 : 0;
                @in.AddChildNode(child.ChildNodes[nodeindex]);
            }
    
            // validate
            try {
                @in.ValidateWithoutContext();
            } catch (ExprValidationException) {
                return constituent;
            }
    
            return @in;
        }
    
        private static FilterSpecParam HandlePlugInSingleRow(ExprPlugInSingleRowNode constituent)
        {
            if (constituent.ExprEvaluator.ReturnType.GetBoxedType() != typeof(bool?)) {
                return null;
            }
            if (!constituent.IsFilterLookupEligible) {
                return null;
            }
            var lookupable = constituent.FilterLookupable;
            return new FilterSpecParamConstant(lookupable, FilterOperator.EQUAL, true);
        }
    
        private static FilterSpecParam HandleRangeNode(
            ExprBetweenNode betweenNode, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            ExprEvaluatorContext exprEvaluatorContext, 
            string statementName)
        {
            var left = betweenNode.ChildNodes[0];
            if (left is ExprFilterOptimizableNode)
            {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                var op = FilterOperatorExtensions.ParseRangeOperator(
                    betweenNode.IsLowEndpointIncluded, 
                    betweenNode.IsHighEndpointIncluded,
                    betweenNode.IsNotBetween);
    
                var low = HandleRangeNodeEndpoint(betweenNode.ChildNodes[1], arrayEventTypes, exprEvaluatorContext, statementName);
                var high = HandleRangeNodeEndpoint(betweenNode.ChildNodes[2], arrayEventTypes, exprEvaluatorContext, statementName);
    
                if ((low != null) && (high != null))
                {
                    return new FilterSpecParamRange(lookupable, op, low, high);
                }
            }
            return null;
        }
    
        private static FilterSpecParamFilterForEval HandleRangeNodeEndpoint(
            ExprNode endpoint, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            ExprEvaluatorContext exprEvaluatorContext, 
            string statementName)
        {
            // constant
            if (ExprNodeUtility.IsConstantValueExpr(endpoint)) {
                var node = (ExprConstantNode) endpoint;
                var value = node.GetConstantValue(exprEvaluatorContext);
                if (value == null) {
                    return null;
                }
                if (value is string) {
                    return new FilterForEvalConstantString((string) value);
                } else {
                    return new FilterForEvalConstantDouble(value.AsDouble());
                }
            }
    
            if (endpoint is ExprContextPropertyNode) {
                var node = (ExprContextPropertyNode) endpoint;
                if (node.ReturnType == typeof(string)) {
                    return new FilterForEvalContextPropString(node.Getter, node.PropertyName);
                } else {
                    return new FilterForEvalContextPropDouble(node.Getter, node.PropertyName);
                }
            }
    
            // or property
            if (endpoint is ExprIdentNode) {
                return GetIdentNodeDoubleEval((ExprIdentNode) endpoint, arrayEventTypes, statementName);
            }
    
            return null;
        }
    
        private static FilterSpecParam HandleInSetNode(
            ExprInNode constituent, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            ExprEvaluatorContext exprEvaluatorContext, 
            string statementName)
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
    
            var expectedNumberOfConstants = constituent.ChildNodes.Count - 1;
            var listofValues = new List<FilterSpecParamInValue>();
            var enumerator = constituent.ChildNodes.GetEnumerator();
            enumerator.MoveNext();  // ignore the first node as it's the identifier
            while (enumerator.MoveNext())
            {
                ExprNode subNode = enumerator.Current;
                if (ExprNodeUtility.IsConstantValueExpr(subNode)) {
                    var constantNode = (ExprConstantNode) subNode;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    if (constant is Array asArray)
                    {
                        for (var i = 0; i < asArray.Length; i++)
                        {
                            var arrayElement = asArray.GetValue(i);
                            var arrayElementCoerced = HandleConstantsCoercion(lookupable, arrayElement);
                            listofValues.Add(new FilterForEvalConstantAnyType(arrayElementCoerced));
                            if (i > 0)
                            {
                                expectedNumberOfConstants++;
                            }
                        }
                    } else if (constant is ICollection<object>) {
                        return null;
                    } else if (constant is IDictionary<string, object>) {
                        return null;
                    } else {
                        constant = HandleConstantsCoercion(lookupable, constant);
                        listofValues.Add(new FilterForEvalConstantAnyType(constant));
                    }
                }
                if (subNode is ExprContextPropertyNode) {
                    var contextPropertyNode = (ExprContextPropertyNode) subNode;
                    var returnType = contextPropertyNode.ReturnType;
                    Coercer coercer;
                    Type coercerType;
                    if (returnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                        coercerType = null;
                    } else {
                        coercer = GetNumberCoercer(
                            left.ExprEvaluator.ReturnType,
                            contextPropertyNode.ReturnType, 
                            lookupable.Expression,
                            out coercerType);
                    }
                    var finalReturnType = coercer != null ? coercerType : returnType;
                    listofValues.Add(new FilterForEvalContextPropMayCoerce(contextPropertyNode.PropertyName, contextPropertyNode.Getter, coercer, finalReturnType));
                }
                if (subNode is ExprIdentNode) {
                    var identNodeInner = (ExprIdentNode) subNode;
                    if (identNodeInner.StreamId == 0) {
                        break; // for same event evals use the bool expression, via count compare failing below
                    }
    
                    var isMustCoerce = false;
                    var coerceToType = lookupable.ReturnType.GetBoxedType();
                    var identReturnType = identNodeInner.ExprEvaluator.ReturnType;
    
                    if (identReturnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(identReturnType, lookupable.ReturnType, lookupable.Expression);
                        coerceToType = identReturnType;
                        // no action
                    } else if (identReturnType != lookupable.ReturnType) {
                        if (lookupable.ReturnType.IsNumeric()) {
                            if (!identReturnType.CanCoerce(lookupable.ReturnType)) {
                                ThrowConversionError(identReturnType, lookupable.ReturnType, lookupable.Expression);
                            }
                            isMustCoerce = true;
                        } else {
                            break;  // assumed not compatible
                        }
                    }
    
                    FilterSpecParamInValue inValue;
                    var streamName = identNodeInner.ResolvedStreamName;
                    if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
                        var indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                        inValue = new FilterForEvalEventPropIndexedMayCoerce(identNodeInner.ResolvedStreamName, indexAndProp.First,
                                indexAndProp.Second, isMustCoerce, coerceToType, statementName);
                    } else {
                        inValue = new FilterForEvalEventPropMayCoerce(identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName, isMustCoerce, coerceToType);
                    }
    
                    listofValues.Add(inValue);
                }
            }
    
            // Fallback if not all values in the in-node can be resolved to properties or constants
            if (listofValues.Count == expectedNumberOfConstants) {
                return new FilterSpecParamIn(lookupable, op, listofValues);
            }
            return null;
        }
    
        private static void CheckArrayCoercion(Type returnTypeValue, Type returnTypeLookupable, string propertyName) {
            if (returnTypeValue == null || !returnTypeValue.IsArray) {
                return;
            }
            if (!returnTypeLookupable.IsArrayTypeCompatible(returnTypeValue.GetElementType())) {
                ThrowConversionError(returnTypeValue.GetElementType(), returnTypeLookupable, propertyName);
            }
        }
    
        private static FilterSpecParam HandleEqualsAndRelOp(
            ExprNode constituent, 
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            ExprEvaluatorContext exprEvaluatorContext, 
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
                } else {
                    op = FilterOperator.IS;
                    if (equalsNode.IsNotEquals) {
                        op = FilterOperator.IS_NOT;
                    }
                }
            } else {
                var relNode = (ExprRelationalOpNode) constituent;
                switch(relNode.RelationalOpEnum)
                {
                    case RelationalOpEnum.GT:
                        op = FilterOperator.GREATER;
                        break;
                    case RelationalOpEnum.LT:
                        op = FilterOperator.LESS;
                        break;
                    case RelationalOpEnum.LE:
                        op = FilterOperator.LESS_OR_EQUAL;
                        break;
                    case RelationalOpEnum.GE:
                        op = FilterOperator.GREATER_OR_EQUAL;
                        break;
                    default:
                        throw new IllegalStateException("Operator '" + relNode.RelationalOpEnum + "' not mapped");
                }
            }
    
            var left = constituent.ChildNodes[0];
            var right = constituent.ChildNodes[1];
    
            // check identifier and constant combination
            if ((ExprNodeUtility.IsConstantValueExpr(right)) && (left is ExprFilterOptimizableNode)) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                if (filterOptimizableNode.IsFilterLookupEligible) {
                    var constantNode = (ExprConstantNode) right;
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    return new FilterSpecParamConstant(lookupable, op, constant);
                }
            }
            if ((ExprNodeUtility.IsConstantValueExpr(left)) && (right is ExprFilterOptimizableNode)) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                if (filterOptimizableNode.IsFilterLookupEligible) {
                    var constantNode = (ExprConstantNode) left;
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    var opReversed = op.IsComparisonOperator() ? op.ReversedRelationalOp() : op;
                    return new FilterSpecParamConstant(lookupable, opReversed, constant);
                }
            }
            // check identifier and expression containing other streams
            if ((left is ExprIdentNode) && (right is ExprIdentNode)) {
                var identNodeLeft = (ExprIdentNode) left;
                var identNodeRight = (ExprIdentNode) right;
    
                if ((identNodeLeft.StreamId == 0) && (identNodeLeft.IsFilterLookupEligible) && (identNodeRight.StreamId != 0)) {
                    return HandleProperty(op, identNodeLeft, identNodeRight, arrayEventTypes, statementName);
                }
                if ((identNodeRight.StreamId == 0) && (identNodeRight.IsFilterLookupEligible) && (identNodeLeft.StreamId != 0)) {
                    op = GetReversedOperator(constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    return HandleProperty(op, identNodeRight, identNodeLeft, arrayEventTypes, statementName);
                }
            }
    
            if ((left is ExprFilterOptimizableNode) && (right is ExprContextPropertyNode)) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var ctxNode = (ExprContextPropertyNode) right;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.IsFilterLookupEligible) {
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, ctxNode.ReturnType, lookupable.Expression);
                    return new FilterSpecParamContextProp(lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            if ((left is ExprContextPropertyNode) && (right is ExprFilterOptimizableNode)) {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                var ctxNode = (ExprContextPropertyNode) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.IsFilterLookupEligible) {
                    op = GetReversedOperator(constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(lookupable.ReturnType, ctxNode.ReturnType, lookupable.Expression);
                    return new FilterSpecParamContextProp(lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            return null;
        }
    
        private static FilterOperator GetReversedOperator(ExprNode constituent, FilterOperator op)
        {
            if (!(constituent is ExprRelationalOpNode)) {
                return op;
            }
    
            var relNode = (ExprRelationalOpNode) constituent;
            var relationalOpEnum = relNode.RelationalOpEnum;
    
            switch (relationalOpEnum)
            {
                case RelationalOpEnum.GT:
                    return FilterOperator.LESS;
                case RelationalOpEnum.LT:
                    return FilterOperator.GREATER;
                case RelationalOpEnum.LE:
                    return FilterOperator.GREATER_OR_EQUAL;
                case RelationalOpEnum.GE:
                    return FilterOperator.LESS_OR_EQUAL;
            }
            return op;
        }
    
        private static FilterSpecParam HandleProperty(
            FilterOperator op, 
            ExprIdentNode identNodeLeft, 
            ExprIdentNode identNodeRight,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            var propertyName = identNodeLeft.ResolvedPropertyName;
    
            var leftType = identNodeLeft.ExprEvaluator.ReturnType;
            var rightType = identNodeRight.ExprEvaluator.ReturnType;
    
            var numberCoercer = GetNumberCoercer(leftType, rightType, propertyName);
            var isMustCoerce = numberCoercer != null;
            var numericCoercionType = leftType.GetBoxedType();
    
            var streamName = identNodeRight.ResolvedStreamName;
            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
                var indexAndProp = GetStreamIndex(identNodeRight.ResolvedPropertyName);
                return new FilterSpecParamEventPropIndexed(identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName, indexAndProp.First,
                        indexAndProp.Second, isMustCoerce, numberCoercer, numericCoercionType, statementName);
            }
            return new FilterSpecParamEventProp(identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName, identNodeRight.ResolvedPropertyName,
                    isMustCoerce, numberCoercer, numericCoercionType, statementName);
        }

        private static Coercer GetNumberCoercer(Type leftType, Type rightType, string expression)
        {
            return GetNumberCoercer(leftType, rightType, expression, out _);
        }

        private static Coercer GetNumberCoercer(Type leftType, Type rightType, string expression, out Type coercerType)
        {
            var numericCoercionType = leftType.GetBoxedType();
            if (rightType != leftType)
            {
                if (rightType.IsNumeric())
                {
                    if (!rightType.CanCoerce(leftType))
                    {
                        ThrowConversionError(rightType, leftType, expression);
                    }

                    coercerType = numericCoercionType;
                    return CoercerFactory.GetCoercer(rightType, numericCoercionType);
                }
            }

            coercerType = null;
            return null;
        }

        private static Pair<int, string> GetStreamIndex(string resolvedPropertyName)
        {
            var property = PropertyParser.ParseAndWalkLaxToSimple(resolvedPropertyName);
            if (!(property is NestedProperty)) {
                throw new IllegalStateException("Expected a nested property providing an index for array match '" + resolvedPropertyName + "'");
            }
            var nested = (NestedProperty) property;
            if (nested.Properties.Count < 2) {
                throw new IllegalStateException("Expected a nested property name for array match '" + resolvedPropertyName + "', none found");
            }
            if (!(nested.Properties[0] is IndexedProperty)) {
                throw new IllegalStateException("Expected an indexed property for array match '" + resolvedPropertyName + "', please provide an index");
            }
            var index = ((IndexedProperty) nested.Properties[0]).Index;
            nested.Properties.RemoveAt(0);
            var writer = new StringWriter();
            nested.ToPropertyEPL(writer);
            return new Pair<int, string>(index, writer.ToString());
        }
    
        private static void ThrowConversionError(Type fromType, Type toType, string propertyName)
        {
            var text = string.Format(
                "Implicit conversion from datatype \'{0}\' to \'{1}\' for property \'{2}\' is not allowed (strict filter type coercion)",
                fromType.Name, toType.Name, propertyName);
            throw new ExprValidationException(text);
        }
    
        // expressions automatically coerce to the most upwards type
        // filters require the same type
        private static Object HandleConstantsCoercion(FilterSpecLookupable lookupable, Object constant)
        {
            var identNodeType = lookupable.ReturnType;
            if (!identNodeType.IsNumeric()) {
                return constant;    // no coercion required, other type checking performed by expression this comes from
            }
    
            if (constant == null) {
                // null constant type
                return null;
            }
    
            if (!constant.GetType().CanCoerce(identNodeType)) {
                ThrowConversionError(constant.GetType(), identNodeType, lookupable.Expression);
            }
    
            var identNodeTypeBoxed = identNodeType.GetBoxedType();
            return CoercerFactory.CoerceBoxed(constant, identNodeTypeBoxed);
        }
    
        private static bool IsExprExistsInAllEqualsChildNodes(IList<ExprNode> childNodes, ExprNode search)
        {
            foreach (var child in childNodes) {
                var lhs = child.ChildNodes[0];
                var rhs = child.ChildNodes[1];
                if (!ExprNodeUtility.DeepEquals(lhs, search, false) && !ExprNodeUtility.DeepEquals(rhs, search, false)) {
                    return false;
                }
                if (ExprNodeUtility.DeepEquals(lhs, rhs, false)) {
                    return false;
                }
            }
            return true;
        }
    
        private static FilterSpecParamFilterForEvalDouble GetIdentNodeDoubleEval(
            ExprIdentNode node,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, 
            string statementName)
        {
            if (node.StreamId == 0) {
                return null;
            }
    
            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(node.ResolvedStreamName)) {
                var indexAndProp = GetStreamIndex(node.ResolvedPropertyName);
                return new FilterForEvalEventPropIndexedDouble(
                    node.ResolvedStreamName, 
                    indexAndProp.First, 
                    indexAndProp.Second, 
                    statementName);
            } else {
                return new FilterForEvalEventPropDouble(node.ResolvedStreamName, node.ResolvedPropertyName);
            }
        }
    }
} // end of namespace
