///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper.*;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	/// <summary>
	/// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
	/// </summary>
	public class FilterSpecCompilerIndexPlannerInSetOfValues
	{

		protected static FilterSpecParamForge HandleInSetNode(
			ExprInNode constituent,
			IDictionary<string, Pair<EventType, string>> taggedEventTypes,
			IDictionary<string, Pair<EventType, string>> arrayEventTypes,
			ISet<string> allTagNamesOrdered,
			StatementRawInfo raw,
			StatementCompileTimeServices services)
		{
			ExprNode left = constituent.ChildNodes[0];
			ExprFilterSpecLookupableForge lookupable = null;

			if (left is ExprFilterOptimizableNode) {
				ExprFilterOptimizableNode filterOptimizableNode = (ExprFilterOptimizableNode) left;
				lookupable = filterOptimizableNode.FilterLookupable;
			}
			else if (FilterSpecCompilerIndexPlannerHelper.HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.LKUPCOMPOSITE, raw, services) &&
			         IsLimitedLookupableExpression(left)) {
				lookupable = MakeLimitedLookupableForgeMayNull(left, raw, services);
			}

			if (lookupable == null) {
				return null;
			}

			FilterOperator op = FilterOperator.IN_LIST_OF_VALUES;
			if (constituent.IsNotIn) {
				op = FilterOperator.NOT_IN_LIST_OF_VALUES;
			}

			int expectedNumberOfConstants = constituent.ChildNodes.Length - 1;
			IList<FilterSpecParamInValueForge> listofValues = new List<FilterSpecParamInValueForge>();
			IEnumerator<ExprNode> it = Arrays.AsList(constituent.ChildNodes).Iterator();
			it.Next(); // ignore the first node as it's the identifier
			while (it.HasNext) {
				ExprNode subNode = it.Next();
				if (subNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
					object constant = subNode.Forge.ExprEvaluator.Evaluate(null, true, null);
					if (constant is ICollection) {
						return null;
					}

					if (constant is IDictionary) {
						return null;
					}

					if ((constant != null) && (constant.GetType().IsArray)) {
						for (int i = 0; i < Array.GetLength(constant); i++) {
							object arrayElement = Array.Get(constant, i);
							object arrayElementCoerced = HandleConstantsCoercion(lookupable, arrayElement);
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
					ExprContextPropertyNode contextPropertyNode = (ExprContextPropertyNode) subNode;
					Type returnType = contextPropertyNode.Type;
					Coercer coercer;
					if (TypeHelper.IsCollectionMapOrArray(returnType)) {
						CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
						coercer = null;
					}
					else {
						coercer = GetNumberCoercer(left.Forge.EvaluationType, contextPropertyNode.Type, lookupable.Expression);
					}

					Type finalReturnType = coercer != null ? coercer.ReturnType : returnType;
					listofValues.Add(new FilterForEvalContextPropForge(contextPropertyNode.PropertyName, contextPropertyNode.Getter, coercer, finalReturnType));
				}
				else if (subNode.Forge.ForgeConstantType.IsDeployTimeTimeConstant && subNode is ExprNodeDeployTimeConst) {
					ExprNodeDeployTimeConst deployTimeConst = (ExprNodeDeployTimeConst) subNode;
					Type returnType = subNode.Forge.EvaluationType;
					Coercer coercer;
					if (TypeHelper.IsCollectionMapOrArray(returnType)) {
						CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
						coercer = null;
					}
					else {
						coercer = GetNumberCoercer(left.Forge.EvaluationType, returnType, lookupable.Expression);
					}

					listofValues.Add(new FilterForEvalDeployTimeConstForge(deployTimeConst, coercer, returnType));
				}
				else if (subNode is ExprIdentNode) {
					ExprIdentNode identNodeInner = (ExprIdentNode) subNode;
					if (identNodeInner.StreamId == 0) {
						break; // for same event evals use the boolean expression, via count compare failing below
					}

					bool isMustCoerce = false;
					Type coerceToType = Boxing.GetBoxedType(lookupable.ReturnType);
					Type identReturnType = identNodeInner.Forge.EvaluationType;

					if (TypeHelper.IsCollectionMapOrArray(identReturnType)) {
						CheckArrayCoercion(identReturnType, lookupable.ReturnType, lookupable.Expression);
						coerceToType = identReturnType;
						// no action
					}
					else if (identReturnType != lookupable.ReturnType) {
						if (TypeHelper.IsNumeric(lookupable.ReturnType)) {
							if (!TypeHelper.CanCoerce(identReturnType, lookupable.ReturnType)) {
								ThrowConversionError(identReturnType, lookupable.ReturnType, lookupable.Expression);
							}

							isMustCoerce = true;
						}
						else {
							break; // assumed not compatible
						}
					}

					FilterSpecParamInValueForge inValue;
					string streamName = identNodeInner.ResolvedStreamName;
					if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(streamName)) {
						Pair<int?, string> indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
						EventType innerEventType = GetArrayInnerEventType(arrayEventTypes, streamName);
						inValue = new FilterForEvalEventPropIndexedForge(
							identNodeInner.ResolvedStreamName,
							indexAndProp.First,
							indexAndProp.Second,
							innerEventType,
							isMustCoerce,
							coerceToType);
					}
					else {
						inValue = new FilterForEvalEventPropForge(
							identNodeInner.ResolvedStreamName,
							identNodeInner.ResolvedPropertyName,
							identNodeInner.ExprEvaluatorIdent,
							isMustCoerce,
							coerceToType);
					}

					listofValues.Add(inValue);
				}
				else if (FilterSpecCompilerIndexPlannerHelper.HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.VALUECOMPOSITE, raw, services) &&
				         IsLimitedValueExpression(subNode)) {
					MatchedEventConvertorForge convertor = GetMatchEventConvertor(subNode, taggedEventTypes, arrayEventTypes, allTagNamesOrdered);
					Type valueType = subNode.Forge.EvaluationType;
					Type lookupableType = lookupable.ReturnType;
					Coercer numberCoercer = GetNumberCoercer(lookupableType, valueType, lookupable.Expression);
					FilterForEvalLimitedExprForge forge = new FilterForEvalLimitedExprForge(subNode, convertor, numberCoercer);
					listofValues.Add(forge);
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

			if (!TypeHelper.IsArrayTypeCompatible(returnTypeLookupable, returnTypeValue.GetElementType())) {
				ThrowConversionError(returnTypeValue.GetElementType(), returnTypeLookupable, propertyName);
			}
		}
	}
} // end of namespace
