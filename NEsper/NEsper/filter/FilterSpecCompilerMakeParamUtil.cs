///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.events.property;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
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
        /// <param name="arrayEventTypes">@return filter parameter representing the expression, or null</param>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if the expression is invalid</throws>
        /// <returns>FilterSpecParam filter param</returns>
        public static FilterSpecParam MakeFilterParam(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            string statementName)
        {
            // Is this expresson node a simple compare, i.e. a=5 or b<4; these can be indexed
            if ((constituent is ExprEqualsNode) ||
                (constituent is ExprRelationalOpNode))
            {
                var param = HandleEqualsAndRelOp(
                    constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null)
                {
                    return param;
                }
            }

            constituent = RewriteOrToInIfApplicable(constituent);

            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if (constituent is ExprInNode)
            {
                var param = HandleInSetNode(
                    (ExprInNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null)
                {
                    return param;
                }
            }

            if (constituent is ExprBetweenNode)
            {
                var param = HandleRangeNode(
                    (ExprBetweenNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null)
                {
                    return param;
                }
            }

            if (constituent is ExprPlugInSingleRowNode)
            {
                var param = HandlePlugInSingleRow((ExprPlugInSingleRowNode) constituent);
                if (param != null)
                {
                    return param;
                }
            }

            return null;
        }

        public static ExprNode RewriteOrToInIfApplicable(ExprNode constituent)
        {
            if (!(constituent is ExprOrNode) || constituent.ChildNodes.Length < 2)
            {
                return constituent;
            }

            // check eligibility
            var childNodes = constituent.ChildNodes;
            foreach (var child in childNodes)
            {
                if (!(child is ExprEqualsNode))
                {
                    return constituent;
                }
                var equalsNode = (ExprEqualsNode) child;
                if (equalsNode.IsIs || equalsNode.IsNotEquals)
                {
                    return constituent;
                }
            }

            // find common-expression node
            ExprNode commonExpressionNode;
            var lhs = childNodes[0].ChildNodes[0];
            var rhs = childNodes[0].ChildNodes[1];
            if (ExprNodeUtility.DeepEquals(lhs, rhs))
            {
                return constituent;
            }
            if (IsExprExistsInAllEqualsChildNodes(childNodes, lhs))
            {
                commonExpressionNode = lhs;
            }
            else if (IsExprExistsInAllEqualsChildNodes(childNodes, rhs))
            {
                commonExpressionNode = rhs;
            }
            else
            {
                return constituent;
            }

            // build node
            var @in = new ExprInNodeImpl(false);
            @in.AddChildNode(commonExpressionNode);
            for (var i = 0; i < constituent.ChildNodes.Length; i++)
            {
                var child = constituent.ChildNodes[i];
                var nodeindex = ExprNodeUtility.DeepEquals(commonExpressionNode, childNodes[i].ChildNodes[0]) ? 1 : 0;
                @in.AddChildNode(child.ChildNodes[nodeindex]);
            }

            // validate
            try
            {
                @in.ValidateWithoutContext();
            }
            catch (ExprValidationException)
            {
                return constituent;
            }

            return @in;
        }

        private static FilterSpecParam HandlePlugInSingleRow(ExprPlugInSingleRowNode constituent)
        {
            if (constituent.ExprEvaluator.ReturnType.GetBoxedType() != typeof (bool?))
            {
                return null;
            }
            if (!constituent.IsFilterLookupEligible)
            {
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
                    betweenNode.IsLowEndpointIncluded, betweenNode.IsHighEndpointIncluded,
                    betweenNode.IsNotBetween);

                var low = HandleRangeNodeEndpoint(
                    betweenNode.ChildNodes[1], arrayEventTypes, exprEvaluatorContext, statementName);
                var high = HandleRangeNodeEndpoint(
                    betweenNode.ChildNodes[2], arrayEventTypes, exprEvaluatorContext, statementName);

                if ((low != null) && (high != null))
                {
                    return new FilterSpecParamRange(lookupable, op, low, high);
                }
            }
            return null;
        }

        private static FilterSpecParamRangeValue HandleRangeNodeEndpoint(
            ExprNode endpoint,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            string statementName)
        {
            // constant
            if (ExprNodeUtility.IsConstantValueExpr(endpoint))
            {
                var node = (ExprConstantNode) endpoint;
                var value = node.GetConstantValue(exprEvaluatorContext);
                if (value == null)
                {
                    return null;
                }
                else if (value is string)
                {
                    return new RangeValueString((string) value);
                }
                else
                {
                    return new RangeValueDouble(value.AsDouble());
                }
            }

            if (endpoint is ExprContextPropertyNode)
            {
                var node = (ExprContextPropertyNode) endpoint;
                return new RangeValueContextProp(node.Getter);
            }

            // or property
            if (endpoint is ExprIdentNode)
            {
                var identNodeInner = (ExprIdentNode) endpoint;
                if (identNodeInner.StreamId == 0)
                {
                    return null;
                }

                if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() &&
                    arrayEventTypes.ContainsKey(identNodeInner.ResolvedStreamName))
                {
                    var indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                    return new RangeValueEventPropIndexed(
                        identNodeInner.ResolvedStreamName, indexAndProp.First.Value, indexAndProp.Second, statementName);
                }
                else
                {
                    return new RangeValueEventProp(
                        identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName);
                }
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
            if (!(left is ExprFilterOptimizableNode))
            {
                return null;
            }

            var filterOptimizableNode = (ExprFilterOptimizableNode) left;
            var lookupable = filterOptimizableNode.FilterLookupable;
            var op = FilterOperator.IN_LIST_OF_VALUES;
            if (constituent.IsNotIn)
            {
                op = FilterOperator.NOT_IN_LIST_OF_VALUES;
            }

            var expectedNumberOfConstants = constituent.ChildNodes.Length - 1;
            IList<FilterSpecParamInValue> listofValues = new List<FilterSpecParamInValue>();
            foreach (var subNode in constituent.ChildNodes.Skip(1)) // ignore the first node as it's the identifier
            {
                if (ExprNodeUtility.IsConstantValueExpr(subNode))
                {
                    var constantNode = (ExprConstantNode) subNode;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    if (constant != null && !constant.GetType().IsArray)
                    {
                        var constantType = constant.GetType();
                        if (constantType.IsGenericCollection())
                            return null;
                        if (constantType.IsGenericDictionary())
                            return null;
                    }

                    var asArray = constant as Array;
                    if (asArray != null)
                    {
                        for (var i = 0; i < asArray.Length; i++)
                        {
                            var arrayElement = asArray.GetValue(i);
                            var arrayElementCoerced = HandleConstantsCoercion(lookupable, arrayElement);
                            listofValues.Add(new InSetOfValuesConstant(arrayElementCoerced));
                            if (i > 0)
                            {
                                expectedNumberOfConstants++;
                            }
                        }
                    }
                    else
                    {
                        constant = HandleConstantsCoercion(lookupable, constant);
                        listofValues.Add(new InSetOfValuesConstant(constant));
                    }
                }
                if (subNode is ExprContextPropertyNode)
                {
                    var contextPropertyNode = (ExprContextPropertyNode) subNode;
                    var returnType = contextPropertyNode.ReturnType;
                    if (contextPropertyNode.ReturnType.IsImplementsInterface(typeof (ICollection<>)) ||
                        contextPropertyNode.ReturnType.IsImplementsInterface(typeof (IDictionary<,>)))
                    {
                        return null;
                    }
                    if ((returnType != null) && (returnType.GetType().IsArray))
                    {
                        return null;
                    }
                    var coercer = GetNumberCoercer(
                        left.ExprEvaluator.ReturnType, contextPropertyNode.ReturnType, lookupable.Expression);
                    listofValues.Add(
                        new InSetOfValuesContextProp(
                            contextPropertyNode.PropertyName, contextPropertyNode.Getter, coercer));
                }
                if (subNode is ExprIdentNode)
                {
                    var identNodeInner = (ExprIdentNode) subNode;
                    if (identNodeInner.StreamId == 0)
                    {
                        break; // for same event evals use the boolean expression, via count compare failing below
                    }

                    var isMustCoerce = false;
                    var numericCoercionType = lookupable.ReturnType.GetBoxedType();
                    if (identNodeInner.ExprEvaluator.ReturnType != lookupable.ReturnType)
                    {
                        if (lookupable.ReturnType.IsNumeric())
                        {
                            if (!identNodeInner.ExprEvaluator.ReturnType.CanCoerce(lookupable.ReturnType))
                            {
                                ThrowConversionError(
                                    identNodeInner.ExprEvaluator.ReturnType, lookupable.ReturnType,
                                    lookupable.Expression);
                            }
                            isMustCoerce = true;
                        }
                        else
                        {
                            break; // assumed not compatible
                        }
                    }

                    FilterSpecParamInValue inValue;
                    var streamName = identNodeInner.ResolvedStreamName;
                    if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName))
                    {
                        var indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                        inValue = new InSetOfValuesEventPropIndexed(
                            identNodeInner.ResolvedStreamName, indexAndProp.First.Value,
                            indexAndProp.Second, isMustCoerce, numericCoercionType, statementName);
                    }
                    else
                    {
                        inValue = new InSetOfValuesEventProp(
                            identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName, isMustCoerce,
                            numericCoercionType);
                    }

                    listofValues.Add(inValue);
                }
            }

            // Fallback if not all values in the in-node can be resolved to properties or constants
            if (listofValues.Count == expectedNumberOfConstants)
            {
                return new FilterSpecParamIn(lookupable, op, listofValues);
            }
            return null;
        }

        private static FilterSpecParam HandleEqualsAndRelOp(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            string statementName)
        {
            FilterOperator op;
            if (constituent is ExprEqualsNode)
            {
                var equalsNode = (ExprEqualsNode) constituent;
                if (!equalsNode.IsIs)
                {
                    op = FilterOperator.EQUAL;
                    if (equalsNode.IsNotEquals)
                    {
                        op = FilterOperator.NOT_EQUAL;
                    }
                }
                else
                {
                    op = FilterOperator.IS;
                    if (equalsNode.IsNotEquals)
                    {
                        op = FilterOperator.IS_NOT;
                    }
                }
            }
            else
            {
                var relNode = (ExprRelationalOpNode) constituent;
                if (relNode.RelationalOpEnum == RelationalOpEnum.GT)
                {
                    op = FilterOperator.GREATER;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.LT)
                {
                    op = FilterOperator.LESS;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.LE)
                {
                    op = FilterOperator.LESS_OR_EQUAL;
                }
                else if (relNode.RelationalOpEnum == RelationalOpEnum.GE)
                {
                    op = FilterOperator.GREATER_OR_EQUAL;
                }
                else
                {
                    throw new IllegalStateException(string.Format("Operator '{0}' not mapped", relNode.RelationalOpEnum));
                }
            }

            var left = constituent.ChildNodes[0];
            var right = constituent.ChildNodes[1];

            // check identifier and constant combination
            if ((ExprNodeUtility.IsConstantValueExpr(right)) && (left is ExprFilterOptimizableNode))
            {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                if (filterOptimizableNode.IsFilterLookupEligible)
                {
                    var constantNode = (ExprConstantNode) right;
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    return new FilterSpecParamConstant(lookupable, op, constant);
                }
            }
            if ((ExprNodeUtility.IsConstantValueExpr(left)) && (right is ExprFilterOptimizableNode))
            {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                if (filterOptimizableNode.IsFilterLookupEligible)
                {
                    var constantNode = (ExprConstantNode) left;
                    var lookupable = filterOptimizableNode.FilterLookupable;
                    var constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    var opReversed = op.IsComparisonOperator() ? op.ReversedRelationalOp() : op;
                    return new FilterSpecParamConstant(lookupable, opReversed, constant);
                }
            }
            // check identifier and expression containing other streams
            if ((left is ExprIdentNode) && (right is ExprIdentNode))
            {
                var identNodeLeft = (ExprIdentNode) left;
                var identNodeRight = (ExprIdentNode) right;

                if ((identNodeLeft.StreamId == 0) && (identNodeLeft.IsFilterLookupEligible) &&
                    (identNodeRight.StreamId != 0))
                {
                    return HandleProperty(op, identNodeLeft, identNodeRight, arrayEventTypes, statementName);
                }
                if ((identNodeRight.StreamId == 0) && (identNodeRight.IsFilterLookupEligible) &&
                    (identNodeLeft.StreamId != 0))
                {
                    op = GetReversedOperator(constituent, op);
                        // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    return HandleProperty(op, identNodeRight, identNodeLeft, arrayEventTypes, statementName);
                }
            }

            if ((left is ExprFilterOptimizableNode) && (right is ExprContextPropertyNode))
            {
                var filterOptimizableNode = (ExprFilterOptimizableNode) left;
                var ctxNode = (ExprContextPropertyNode) right;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.IsFilterLookupEligible)
                {
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType, ctxNode.ReturnType, lookupable.Expression);
                    return new FilterSpecParamContextProp(
                        lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            if ((left is ExprContextPropertyNode) && (right is ExprFilterOptimizableNode))
            {
                var filterOptimizableNode = (ExprFilterOptimizableNode) right;
                var ctxNode = (ExprContextPropertyNode) left;
                var lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.IsFilterLookupEligible)
                {
                    op = GetReversedOperator(constituent, op);
                        // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    var numberCoercer = GetNumberCoercer(
                        lookupable.ReturnType, ctxNode.ReturnType, lookupable.Expression);
                    return new FilterSpecParamContextProp(
                        lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            return null;
        }

        private static FilterOperator GetReversedOperator(ExprNode constituent, FilterOperator op)
        {
            if (!(constituent is ExprRelationalOpNode))
            {
                return op;
            }

            var relNode = (ExprRelationalOpNode) constituent;
            var relationalOpEnum = relNode.RelationalOpEnum;

            if (relationalOpEnum == RelationalOpEnum.GT)
            {
                return FilterOperator.LESS;
            }
            else if (relationalOpEnum == RelationalOpEnum.LT)
            {
                return FilterOperator.GREATER;
            }
            else if (relationalOpEnum == RelationalOpEnum.LE)
            {
                return FilterOperator.GREATER_OR_EQUAL;
            }
            else if (relationalOpEnum == RelationalOpEnum.GE)
            {
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
            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName))
            {
                var indexAndProp = GetStreamIndex(identNodeRight.ResolvedPropertyName);
                return new FilterSpecParamEventPropIndexed(
                    identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName,
                    indexAndProp.First.Value,
                    indexAndProp.Second, isMustCoerce, numberCoercer, numericCoercionType, statementName);
            }
            return new FilterSpecParamEventProp(
                identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName,
                identNodeRight.ResolvedPropertyName,
                isMustCoerce, numberCoercer, numericCoercionType, statementName);
        }

        private static Coercer GetNumberCoercer(Type leftType, Type rightType, string expression)
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
                    return CoercerFactory.GetCoercer(rightType, numericCoercionType);
                }
            }
            return null;
        }

        private static Pair<int?, string> GetStreamIndex(string resolvedPropertyName)
        {
            var property = PropertyParser.ParseAndWalk(resolvedPropertyName);
            if (!(property is NestedProperty))
            {
                throw new IllegalStateException(
                    string.Format("Expected a nested property providing an index for array match '{0}'", resolvedPropertyName));
            }
            var nested = ((NestedProperty) property);
            if (nested.Properties.Count < 2)
            {
                throw new IllegalStateException(
                    string.Format("Expected a nested property name for array match '{0}', none found", resolvedPropertyName));
            }
            if (!(nested.Properties[0] is IndexedProperty))
            {
                throw new IllegalStateException(
                    string.Format("Expected an indexed property for array match '{0}', please provide an index", resolvedPropertyName));
            }
            var index = ((IndexedProperty) nested.Properties[0]).Index;
            nested.Properties.RemoveAt(0);
            var writer = new StringWriter();
            nested.ToPropertyEPL(writer);
            return new Pair<int?, string>(index, writer.ToString());
        }

        private static void ThrowConversionError(Type fromType, Type toType, string propertyName)

        {
            var text = string.Format("Implicit conversion from datatype '{0}' to '{1}' for property '{2}' is not allowed (strict filter type coercion)", fromType.Name, toType.Name, propertyName);
            throw new ExprValidationException(text);
        }

        // expressions automatically coerce to the most upwards type
        // filters require the same type
        private static object HandleConstantsCoercion(FilterSpecLookupable lookupable, object constant)

        {
            var identNodeType = lookupable.ReturnType;
            if (!identNodeType.IsNumeric())
            {
                return constant; // no coercion required, other type checking performed by expression this comes from
            }

            if (constant == null) // null constant type
            {
                return null;
            }

            if (!constant.GetType().CanCoerce(identNodeType))
            {
                ThrowConversionError(constant.GetType(), identNodeType, lookupable.Expression);
            }

            var identNodeTypeBoxed = identNodeType.GetBoxedType();
            return CoercerFactory.CoerceBoxed(constant, identNodeTypeBoxed);
        }

        private static bool IsExprExistsInAllEqualsChildNodes(ExprNode[] childNodes, ExprNode search)
        {
            foreach (var child in childNodes)
            {
                var lhs = child.ChildNodes[0];
                var rhs = child.ChildNodes[1];
                if (!ExprNodeUtility.DeepEquals(lhs, search) && !ExprNodeUtility.DeepEquals(rhs, search))
                {
                    return false;
                }
                if (ExprNodeUtility.DeepEquals(lhs, rhs))
                {
                    return false;
                }
            }
            return true;
        }
    }
} // end of namespace