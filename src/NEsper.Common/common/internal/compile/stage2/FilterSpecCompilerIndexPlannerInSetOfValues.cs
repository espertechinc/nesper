///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public class FilterSpecCompilerIndexPlannerInSetOfValues
    {
        internal static FilterSpecParamForge HandleInSetNode(
            ExprInNode constituent,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            var left = constituent.ChildNodes[0];
            ExprFilterSpecLookupableForge lookupable = null;

            if (left is ExprFilterOptimizableNode filterOptimizableNode) {
                lookupable = filterOptimizableNode.FilterLookupable;
            }
            else if (HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.LKUPCOMPOSITE, raw, services) &&
                     IsLimitedLookupableExpression(left)) {
                lookupable = MakeLimitedLookupableForgeMayNull(left, raw, services);
            }

            if (lookupable == null) {
                return null;
            }

            var op = FilterOperator.IN_LIST_OF_VALUES;
            if (constituent.IsNotIn) {
                op = FilterOperator.NOT_IN_LIST_OF_VALUES;
            }

            var expectedNumberOfConstants = constituent.ChildNodes.Length - 1;
            IList<FilterSpecParamInValueForge> listofValues = new List<FilterSpecParamInValueForge>();
            var enumerator = Arrays.AsList(constituent.ChildNodes).GetEnumerator();
            enumerator.MoveNext(); // ignore the first node as it's the identifier
            while (enumerator.MoveNext()) {
                ExprNode subNode = enumerator.Current;
                if (subNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    var constant = subNode.Forge.ExprEvaluator.Evaluate(null, true, null);
					if (constant is ICollection<object>) {
                        return null;
                    }

					if (constant is IDictionary<object, object>) {
                        return null;
                    }

					if ((constant != null) && (constant is Array arrayConstant)) {
						for (var i = 0; i < arrayConstant.Length; i++) {
							var arrayElement = arrayConstant.GetValue(i);
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
                else if (subNode is ExprContextPropertyNode contextPropertyNode) {
                    if (contextPropertyNode.ValueType == null) {
                        return null;
                    }

                    var returnType = contextPropertyNode.ValueType;
                    Coercer coercer;
                    if (returnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                    }
                    else {
                        coercer = GetNumberCoercer(
                            left.Forge.EvaluationType,
                            contextPropertyNode.ValueType,
                            lookupable.Expression);
                    }

                    var finalReturnType = coercer != null ? coercer.ReturnType : returnType;
                    listofValues.Add(
                        new FilterForEvalContextPropForge(
                            contextPropertyNode.PropertyName,
                            contextPropertyNode.Getter,
                            coercer,
                            finalReturnType));
                }
                else if (subNode.Forge.ForgeConstantType.IsDeployTimeTimeConstant &&
                         subNode is ExprNodeDeployTimeConst deployTimeConst) {
                    var returnType = subNode.Forge.EvaluationType;
                    Coercer coercer;
                    if (returnType.IsCollectionMapOrArray()) {
                        CheckArrayCoercion(returnType, lookupable.ReturnType, lookupable.Expression);
                        coercer = null;
                    }
                    else {
                        coercer = GetNumberCoercer(left.Forge.EvaluationType, returnType, lookupable.Expression);
                    }

                    listofValues.Add(new FilterForEvalDeployTimeConstForge(deployTimeConst, coercer, returnType));
                }
                else if (subNode is ExprIdentNode identNodeInner) {
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
                    else if (identReturnType is Type identTypeClass &&
                             identTypeClass != lookupable.ReturnType) {
                        if (lookupable.ReturnType.IsTypeNumeric()) {
                            if (!identTypeClass.CanCoerce(lookupable.ReturnType)) {
                                ThrowConversionError(
                                    identTypeClass,
                                    lookupable.ReturnType,
                                    lookupable.Expression);
                            }

                            isMustCoerce = true;
                        }
                        else {
                            break; // assumed not compatible
                        }
                    }

                    FilterSpecParamInValueForge inValue;
                    var streamName = identNodeInner.ResolvedStreamName;
                    if (arrayEventTypes != null &&
                        !arrayEventTypes.IsEmpty() &&
                        arrayEventTypes.ContainsKey(streamName)) {
                        var indexAndProp = GetStreamIndex(identNodeInner.ResolvedPropertyName);
                        var innerEventType = GetArrayInnerEventType(arrayEventTypes, streamName);
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
                else if (FilterSpecCompilerIndexPlannerHelper.HasLevelOrHint(
                             FilterSpecCompilerIndexPlannerHint.VALUECOMPOSITE,
                             raw,
                             services) &&
                         IsLimitedValueExpression(subNode)) {
                    var convertor = GetMatchEventConvertor(
                        subNode,
                        taggedEventTypes,
                        arrayEventTypes,
                        allTagNamesOrdered);
                    var valueType = subNode.Forge.EvaluationType;
                    var lookupableType = lookupable.ReturnType;
                    var numberCoercer = GetNumberCoercer(lookupableType, valueType, lookupable.Expression);
                    var forge = new FilterForEvalLimitedExprForge(subNode, convertor, numberCoercer);
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
            if (returnTypeValue == null) {
                return;
            }

            var returnTypeClass = returnTypeValue;
            if (!returnTypeClass.IsArray) {
                return;
            }

            var elementType = returnTypeClass.GetElementType();
            if (!returnTypeLookupable.IsArrayTypeCompatible(elementType)) {
                ThrowConversionError(elementType, returnTypeLookupable, propertyName);
            }
        }
    }
} // end of namespace