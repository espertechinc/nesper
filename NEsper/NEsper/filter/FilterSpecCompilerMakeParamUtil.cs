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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.events.property;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public sealed class FilterSpecCompilerMakeParamUtil {
        /// <summary>
        /// For a given expression determine if this is optimizable and create the filter parameter
        /// representing the expression, or null if not optimizable.
        /// </summary>
        /// <param name="constituent">is the expression to look at</param>
        /// <param name="arrayEventTypes">event types that provide array values</param>
        /// <param name="statementName">statement name</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">if the expression is invalid</exception>
        /// <returns>filter parameter representing the expression, or null</returns>
        internal static FilterSpecParam MakeFilterParam(ExprNode constituent, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, ExprEvaluatorContext exprEvaluatorContext, string statementName)
                {
            // Is this expresson node a simple compare, i.e. a=5 or b<4; these can be indexed
            if ((constituent is ExprEqualsNode) ||
                    (constituent is ExprRelationalOpNode)) {
                FilterSpecParam param = HandleEqualsAndRelOp(constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null) {
                    return param;
                }
            }
    
            constituent = RewriteOrToInIfApplicable(constituent);
    
            // Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
            if (constituent is ExprInNode) {
                FilterSpecParam param = HandleInSetNode((ExprInNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null) {
                    return param;
                }
            }
    
            if (constituent is ExprBetweenNode) {
                FilterSpecParam param = HandleRangeNode((ExprBetweenNode) constituent, arrayEventTypes, exprEvaluatorContext, statementName);
                if (param != null) {
                    return param;
                }
            }
    
            if (constituent is ExprPlugInSingleRowNode) {
                FilterSpecParam param = HandlePlugInSingleRow((ExprPlugInSingleRowNode) constituent);
                if (param != null) {
                    return param;
                }
            }
    
            return null;
        }
    
        public static ExprNode RewriteOrToInIfApplicable(ExprNode constituent) {
            if (!(constituent is ExprOrNode) || constituent.ChildNodes.Length < 2) {
                return constituent;
            }
    
            // check eligibility
            ExprNode[] childNodes = constituent.ChildNodes;
            foreach (ExprNode child in childNodes) {
                if (!(child is ExprEqualsNode)) {
                    return constituent;
                }
                ExprEqualsNode equalsNode = (ExprEqualsNode) child;
                if (equalsNode.IsIs || equalsNode.IsNotEquals) {
                    return constituent;
                }
            }
    
            // find common-expression node
            ExprNode commonExpressionNode;
            ExprNode lhs = childNodes[0].ChildNodes[0];
            ExprNode rhs = childNodes[0].ChildNodes[1];
            if (ExprNodeUtility.DeepEquals(lhs, rhs)) {
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
            var in = new ExprInNodeImpl(false);
            in.AddChildNode(commonExpressionNode);
            for (int i = 0; i < constituent.ChildNodes.Length; i++) {
                ExprNode child = constituent.ChildNodes[i];
                int nodeindex = ExprNodeUtility.DeepEquals(commonExpressionNode, childNodes[i].ChildNodes[0]) ? 1 : 0;
                in.AddChildNode(child.ChildNodes[nodeindex]);
            }
    
            // validate
            try {
                in.ValidateWithoutContext();
            } catch (ExprValidationException ex) {
                return constituent;
            }
    
            return in;
        }
    
        private static FilterSpecParam HandlePlugInSingleRow(ExprPlugInSingleRowNode constituent) {
            if (TypeHelper.GetBoxedType(constituent.ExprEvaluator.Type) != typeof(bool?)) {
                return null;
            }
            if (!constituent.FilterLookupEligible) {
                return null;
            }
            FilterSpecLookupable lookupable = constituent.FilterLookupable;
            return new FilterSpecParamConstant(lookupable, FilterOperator.EQUAL, true);
        }
    
        private static FilterSpecParam HandleRangeNode(ExprBetweenNode betweenNode, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, ExprEvaluatorContext exprEvaluatorContext, string statementName) {
            ExprNode left = betweenNode.ChildNodes[0];
            if (left is ExprFilterOptimizableNode) {
                ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) left;
                FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
                FilterOperator op = FilterOperator.ParseRangeOperator(betweenNode.IsLowEndpointIncluded, betweenNode.IsHighEndpointIncluded,
                        betweenNode.IsNotBetween);
    
                FilterSpecParamRangeValue low = HandleRangeNodeEndpoint(betweenNode.ChildNodes[1], arrayEventTypes, exprEvaluatorContext, statementName);
                FilterSpecParamRangeValue high = HandleRangeNodeEndpoint(betweenNode.ChildNodes[2], arrayEventTypes, exprEvaluatorContext, statementName);
    
                if ((low != null) && (high != null)) {
                    return new FilterSpecParamRange(lookupable, op, low, high);
                }
            }
            return null;
        }
    
        private static FilterSpecParamRangeValue HandleRangeNodeEndpoint(ExprNode endpoint, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, ExprEvaluatorContext exprEvaluatorContext, string statementName) {
            // constant
            if (ExprNodeUtility.IsConstantValueExpr(endpoint)) {
                ExprConstantNode node = (ExprConstantNode) endpoint;
                Object value = node.GetConstantValue(exprEvaluatorContext);
                if (value == null) {
                    return null;
                }
                if (value is string) {
                    return new RangeValueString((string) value);
                } else {
                    return new RangeValueDouble(((Number) value).DoubleValue());
                }
            }
    
            if (endpoint is ExprContextPropertyNode) {
                ExprContextPropertyNode node = (ExprContextPropertyNode) endpoint;
                return new RangeValueContextProp(node.Getter);
            }
    
            // or property
            if (endpoint is ExprIdentNode) {
                ExprIdentNode identNodeInner = (ExprIdentNode) endpoint;
                if (identNodeInner.StreamId == 0) {
                    return null;
                }
    
                if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(identNodeInner.ResolvedStreamName)) {
                    Pair<int?, string> indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                    return new RangeValueEventPropIndexed(identNodeInner.ResolvedStreamName, indexAndProp.First, indexAndProp.Second, statementName);
                } else {
                    return new RangeValueEventProp(identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName);
                }
            }
    
            return null;
        }
    
        private static FilterSpecParam HandleInSetNode(ExprInNode constituent, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, ExprEvaluatorContext exprEvaluatorContext, string statementName)
                {
            ExprNode left = constituent.ChildNodes[0];
            if (!(left is ExprFilterOptimizableNode)) {
                return null;
            }
    
            ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) left;
            FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
            FilterOperator op = FilterOperator.IN_LIST_OF_VALUES;
            if (constituent.IsNotIn) {
                op = FilterOperator.NOT_IN_LIST_OF_VALUES;
            }
    
            int expectedNumberOfConstants = constituent.ChildNodes.Length - 1;
            var listofValues = new List<FilterSpecParamInValue>();
            IEnumerator<ExprNode> it = Arrays.AsList(constituent.ChildNodes).GetEnumerator();
            it.Next();  // ignore the first node as it's the identifier
            while (it.HasNext()) {
                ExprNode subNode = it.Next();
                if (ExprNodeUtility.IsConstantValueExpr(subNode)) {
                    ExprConstantNode constantNode = (ExprConstantNode) subNode;
                    Object constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    if (constant is Collection) {
                        return null;
                    }
                    if (constant is Map) {
                        return null;
                    }
                    if ((constant != null) && (constant.Class.IsArray)) {
                        for (int i = 0; i < Array.GetLength(constant); i++) {
                            Object arrayElement = Array.Get(constant, i);
                            Object arrayElementCoerced = HandleConstantsCoercion(lookupable, arrayElement);
                            listofValues.Add(new InSetOfValuesConstant(arrayElementCoerced));
                            if (i > 0) {
                                expectedNumberOfConstants++;
                            }
                        }
                    } else {
                        constant = HandleConstantsCoercion(lookupable, constant);
                        listofValues.Add(new InSetOfValuesConstant(constant));
                    }
                }
                if (subNode is ExprContextPropertyNode) {
                    ExprContextPropertyNode contextPropertyNode = (ExprContextPropertyNode) subNode;
                    Type returnType = contextPropertyNode.Type;
                    Coercer coercer;
                    if (TypeHelper.IsCollectionMapOrArray(returnType)) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                    } else {
                        coercer = GetNumberCoercer(left.ExprEvaluator.Type, contextPropertyNode.Type, lookupable.Expression);
                    }
                    Type finalReturnType = coercer != null ? Coercer.ReturnType : returnType;
                    listofValues.Add(new InSetOfValuesContextProp(contextPropertyNode.PropertyName, contextPropertyNode.Getter, coercer, finalReturnType));
                }
                if (subNode is ExprIdentNode) {
                    ExprIdentNode identNodeInner = (ExprIdentNode) subNode;
                    if (identNodeInner.StreamId == 0) {
                        break; // for same event evals use the bool expression, via count compare failing below
                    }
    
                    bool isMustCoerce = false;
                    Type coerceToType = TypeHelper.GetBoxedType(lookupable.ReturnType);
                    Type identReturnType = identNodeInner.ExprEvaluator.Type;
    
                    if (TypeHelper.IsCollectionMapOrArray(identReturnType)) {
                        CheckArrayCoercion(identReturnType, lookupable.ReturnType, lookupable.Expression);
                        coerceToType = identReturnType;
                        // no action
                    } else if (identReturnType != lookupable.ReturnType) {
                        if (TypeHelper.IsNumeric(lookupable.ReturnType)) {
                            if (!TypeHelper.CanCoerce(identReturnType, lookupable.ReturnType)) {
                                ThrowConversionError(identReturnType, lookupable.ReturnType, lookupable.Expression);
                            }
                            isMustCoerce = true;
                        } else {
                            break;  // assumed not compatible
                        }
                    }
    
                    FilterSpecParamInValue inValue;
                    string streamName = identNodeInner.ResolvedStreamName;
                    if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
                        Pair<int?, string> indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                        inValue = new InSetOfValuesEventPropIndexed(identNodeInner.ResolvedStreamName, indexAndProp.First,
                                indexAndProp.Second, isMustCoerce, coerceToType, statementName);
                    } else {
                        inValue = new InSetOfValuesEventProp(identNodeInner.ResolvedStreamName, identNodeInner.ResolvedPropertyName, isMustCoerce, coerceToType);
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
            if (!TypeHelper.IsArrayTypeCompatible(returnTypeLookupable, returnTypeValue.ComponentType)) {
                ThrowConversionError(returnTypeValue.ComponentType, returnTypeLookupable, propertyName);
            }
        }
    
        private static FilterSpecParam HandleEqualsAndRelOp(ExprNode constituent, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, ExprEvaluatorContext exprEvaluatorContext, string statementName)
                {
            FilterOperator op;
            if (constituent is ExprEqualsNode) {
                ExprEqualsNode equalsNode = (ExprEqualsNode) constituent;
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
                ExprRelationalOpNode relNode = (ExprRelationalOpNode) constituent;
                if (relNode.RelationalOpEnum == RelationalOpEnum.GT) {
                    op = FilterOperator.GREATER;
                } else if (relNode.RelationalOpEnum == RelationalOpEnum.LT) {
                    op = FilterOperator.LESS;
                } else if (relNode.RelationalOpEnum == RelationalOpEnum.LE) {
                    op = FilterOperator.LESS_OR_EQUAL;
                } else if (relNode.RelationalOpEnum == RelationalOpEnum.GE) {
                    op = FilterOperator.GREATER_OR_EQUAL;
                } else {
                    throw new IllegalStateException("Opertor '" + relNode.RelationalOpEnum + "' not mapped");
                }
            }
    
            ExprNode left = constituent.ChildNodes[0];
            ExprNode right = constituent.ChildNodes[1];
    
            // check identifier and constant combination
            if ((ExprNodeUtility.IsConstantValueExpr(right)) && (left is ExprFilterOptimizableNode)) {
                ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) left;
                if (filterOptimizableNode.FilterLookupEligible) {
                    ExprConstantNode constantNode = (ExprConstantNode) right;
                    FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
                    Object constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    return new FilterSpecParamConstant(lookupable, op, constant);
                }
            }
            if ((ExprNodeUtility.IsConstantValueExpr(left)) && (right is ExprFilterOptimizableNode)) {
                ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) right;
                if (filterOptimizableNode.FilterLookupEligible) {
                    ExprConstantNode constantNode = (ExprConstantNode) left;
                    FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
                    Object constant = constantNode.GetConstantValue(exprEvaluatorContext);
                    constant = HandleConstantsCoercion(lookupable, constant);
                    FilterOperator opReversed = op.IsComparisonOperator ? Op.ReversedRelationalOp() : op;
                    return new FilterSpecParamConstant(lookupable, opReversed, constant);
                }
            }
            // check identifier and expression containing other streams
            if ((left is ExprIdentNode) && (right is ExprIdentNode)) {
                ExprIdentNode identNodeLeft = (ExprIdentNode) left;
                ExprIdentNode identNodeRight = (ExprIdentNode) right;
    
                if ((identNodeLeft.StreamId == 0) && (identNodeLeft.FilterLookupEligible) && (identNodeRight.StreamId != 0)) {
                    return HandleProperty(op, identNodeLeft, identNodeRight, arrayEventTypes, statementName);
                }
                if ((identNodeRight.StreamId == 0) && (identNodeRight.FilterLookupEligible) && (identNodeLeft.StreamId != 0)) {
                    op = GetReversedOperator(constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    return HandleProperty(op, identNodeRight, identNodeLeft, arrayEventTypes, statementName);
                }
            }
    
            if ((left is ExprFilterOptimizableNode) && (right is ExprContextPropertyNode)) {
                ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) left;
                ExprContextPropertyNode ctxNode = (ExprContextPropertyNode) right;
                FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    Coercer numberCoercer = GetNumberCoercer(lookupable.ReturnType, ctxNode.Type, lookupable.Expression);
                    return new FilterSpecParamContextProp(lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            if ((left is ExprContextPropertyNode) && (right is ExprFilterOptimizableNode)) {
                ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) right;
                ExprContextPropertyNode ctxNode = (ExprContextPropertyNode) left;
                FilterSpecLookupable lookupable = filterOptimizableNode.FilterLookupable;
                if (filterOptimizableNode.FilterLookupEligible) {
                    op = GetReversedOperator(constituent, op); // reverse operators, as the expression is "stream1.prop xyz stream0.prop"
                    Coercer numberCoercer = GetNumberCoercer(lookupable.ReturnType, ctxNode.Type, lookupable.Expression);
                    return new FilterSpecParamContextProp(lookupable, op, ctxNode.PropertyName, ctxNode.Getter, numberCoercer);
                }
            }
            return null;
        }
    
        private static FilterOperator GetReversedOperator(ExprNode constituent, FilterOperator op) {
            if (!(constituent is ExprRelationalOpNode)) {
                return op;
            }
    
            ExprRelationalOpNode relNode = (ExprRelationalOpNode) constituent;
            RelationalOpEnum relationalOpEnum = relNode.RelationalOpEnum;
    
            if (relationalOpEnum == RelationalOpEnum.GT) {
                return FilterOperator.LESS;
            } else if (relationalOpEnum == RelationalOpEnum.LT) {
                return FilterOperator.GREATER;
            } else if (relationalOpEnum == RelationalOpEnum.LE) {
                return FilterOperator.GREATER_OR_EQUAL;
            } else if (relationalOpEnum == RelationalOpEnum.GE) {
                return FilterOperator.LESS_OR_EQUAL;
            }
            return op;
        }
    
        private static FilterSpecParam HandleProperty(FilterOperator op, ExprIdentNode identNodeLeft, ExprIdentNode identNodeRight, LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes, string statementName)
                {
            string propertyName = identNodeLeft.ResolvedPropertyName;
    
            Type leftType = identNodeLeft.ExprEvaluator.Type;
            Type rightType = identNodeRight.ExprEvaluator.Type;
    
            Coercer numberCoercer = GetNumberCoercer(leftType, rightType, propertyName);
            bool isMustCoerce = numberCoercer != null;
            Type numericCoercionType = TypeHelper.GetBoxedType(leftType);
    
            string streamName = identNodeRight.ResolvedStreamName;
            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
                Pair<int?, string> indexAndProp = GetStreamIndex(identNodeRight.ResolvedPropertyName);
                return new FilterSpecParamEventPropIndexed(identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName, indexAndProp.First,
                        indexAndProp.Second, isMustCoerce, numberCoercer, numericCoercionType, statementName);
            }
            return new FilterSpecParamEventProp(identNodeLeft.FilterLookupable, op, identNodeRight.ResolvedStreamName, identNodeRight.ResolvedPropertyName,
                    isMustCoerce, numberCoercer, numericCoercionType, statementName);
        }
    
        private static Coercer GetNumberCoercer(Type leftType, Type rightType, string expression) {
            Type numericCoercionType = TypeHelper.GetBoxedType(leftType);
            if (rightType != leftType) {
                if (TypeHelper.IsNumeric(rightType)) {
                    if (!TypeHelper.CanCoerce(rightType, leftType)) {
                        ThrowConversionError(rightType, leftType, expression);
                    }
                    return SimpleNumberCoercerFactory.GetCoercer(rightType, numericCoercionType);
                }
            }
            return null;
        }
    
        private static Pair<int?, string> GetStreamIndex(string resolvedPropertyName) {
            Property property = PropertyParser.ParseAndWalkLaxToSimple(resolvedPropertyName);
            if (!(property is NestedProperty)) {
                throw new IllegalStateException("Expected a nested property providing an index for array match '" + resolvedPropertyName + "'");
            }
            NestedProperty nested = (NestedProperty) property;
            if (nested.Properties.Count < 2) {
                throw new IllegalStateException("Expected a nested property name for array match '" + resolvedPropertyName + "', none found");
            }
            if (!(nested.Properties[0] is IndexedProperty)) {
                throw new IllegalStateException("Expected an indexed property for array match '" + resolvedPropertyName + "', please provide an index");
            }
            int index = ((IndexedProperty) nested.Properties[0]).Index;
            nested.Properties.Remove(0);
            var writer = new StringWriter();
            nested.ToPropertyEPL(writer);
            return new Pair<int?, string>(index, writer.ToString());
        }
    
        private static void ThrowConversionError(Type fromType, Type toType, string propertyName)
                {
            string text = "Implicit conversion from datatype '" +
                    fromType.SimpleName +
                    "' to '" +
                    toType.SimpleName +
                    "' for property '" +
                    propertyName +
                    "' is not allowed (strict filter type coercion)";
            throw new ExprValidationException(text);
        }
    
        // expressions automatically coerce to the most upwards type
        // filters require the same type
        private static Object HandleConstantsCoercion(FilterSpecLookupable lookupable, Object constant)
                {
            Type identNodeType = lookupable.ReturnType;
            if (!TypeHelper.IsNumeric(identNodeType)) {
                return constant;    // no coercion required, other type checking performed by expression this comes from
            }
    
            if (constant == null) {
                // null constant type
                return null;
            }
    
            if (!TypeHelper.CanCoerce(constant.Class, identNodeType)) {
                ThrowConversionError(constant.Class, identNodeType, lookupable.Expression);
            }
    
            Type identNodeTypeBoxed = TypeHelper.GetBoxedType(identNodeType);
            return TypeHelper.CoerceBoxed((Number) constant, identNodeTypeBoxed);
        }
    
        private static bool IsExprExistsInAllEqualsChildNodes(ExprNode[] childNodes, ExprNode search) {
            foreach (ExprNode child in childNodes) {
                ExprNode lhs = child.ChildNodes[0];
                ExprNode rhs = child.ChildNodes[1];
                if (!ExprNodeUtility.DeepEquals(lhs, search) && !ExprNodeUtility.DeepEquals(rhs, search)) {
                    return false;
                }
                if (ExprNodeUtility.DeepEquals(lhs, rhs)) {
                    return false;
                }
            }
            return true;
        }
    }
} // end of namespace
