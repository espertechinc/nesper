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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlanner; //PROPERTY_NAME_BOOLEAN_EXPRESSION;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerHelper
    {
        internal static ExprNode DecomposePopulateConsolidate(
            FilterSpecParaForgeMap filterParamExprMap,
            bool performConditionPlanning,
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            var constituents = DecomposeCheckAggregation(validatedNodes);

            // Remove constituents that are value-expressions
            ExprNode topLevelControl = null;
            if (performConditionPlanning) {
                IList<ExprNode> valueOnlyConstituents = null;
                foreach (var node in constituents) {
                    var visitor = new FilterSpecExprNodeVisitorValueLimitedExpr();
                    node.Accept(visitor);
                    if (visitor.IsLimited) {
                        if (valueOnlyConstituents == null) {
                            valueOnlyConstituents = new List<ExprNode>();
                        }

                        valueOnlyConstituents.Add(node);
                    }
                }

                if (valueOnlyConstituents != null) {
                    constituents.RemoveAll(valueOnlyConstituents);
                    topLevelControl = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(valueOnlyConstituents);
                }
            }

            // Make filter parameter for each expression node, if it can be optimized
            foreach (var constituent in constituents) {
                var triplet = FilterSpecCompilerIndexPlannerConstituent.MakeFilterParam(
                    constituent,
                    performConditionPlanning,
                    args.taggedEventTypes,
                    args.arrayEventTypes,
                    args.allTagNamesOrdered,
                    args.statementRawInfo.StatementName,
                    args.streamTypeService,
                    args.statementRawInfo,
                    args.compileTimeServices);
                filterParamExprMap.Put(constituent, triplet); // accepts null values as the expression may not be optimized
            }

            // Consolidate entries as possible, i.e. (a != 5 and a != 6) is (a not in (5,6))
            // Removes duplicates for same property and same filter operator for filter service index optimizations
            FilterSpecCompilerConsolidateUtil.Consolidate(filterParamExprMap, args.statementRawInfo.StatementName);
            return topLevelControl;
        }

        internal static Coercer GetNumberCoercer(
            Type leftType,
            Type rightType,
            string expression)
        {
            var numericCoercionType = leftType.GetBoxedType();
            if (numericCoercionType.IsNullTypeSafe() || rightType.IsNullTypeSafe()) {
                return null;
            }
            
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

        internal static void ThrowConversionError(
            Type fromType,
            Type toType,
            string propertyName)
        {
            var text = "Implicit conversion from datatype '" +
                       fromType.TypeSafeName() +
                       "' to '" +
                       toType.TypeSafeName() +
                       "' for property '" +
                       propertyName +
                       "' is not allowed (strict filter type coercion)";
            throw new ExprValidationException(text);
        }

        internal static MatchedEventConvertorForge GetMatchEventConvertor(
            ExprNode value,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered)
        {
            var streamUseCollectVisitor = new ExprNodeStreamUseCollectVisitor();
            value.Accept(streamUseCollectVisitor);

            ISet<int> streams = new HashSet<int>();
            foreach (var streamRefNode in streamUseCollectVisitor.Referenced) {
                if (streamRefNode.StreamReferencedIfAny == null) {
                    continue;
                }

                streams.Add(streamRefNode.StreamReferencedIfAny.Value);
            }

            return new MatchedEventConvertorForge(taggedEventTypes, arrayEventTypes, allTagNamesOrdered, streams, true);
        }

        internal static Pair<int, string> GetStreamIndex(string resolvedPropertyName)
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

        internal static IList<ExprNode> DecomposeCheckAggregation(IList<ExprNode> validatedNodes)
        {
            // Break a top-level AND into constituent expression nodes
            IList<ExprNode> constituents = new List<ExprNode>();
            foreach (var validated in validatedNodes) {
                if (validated is ExprAndNode) {
                    RecursiveAndConstituents(constituents, validated);
                }
                else {
                    constituents.Add(validated);
                }

                // Ensure there is no aggregation nodes
                var aggregateExprNodes = new List<ExprAggregateNode>();
                ExprAggregateNodeUtil.GetAggregatesBottomUp(validated, aggregateExprNodes);
                if (!aggregateExprNodes.IsEmpty()) {
                    throw new ExprValidationException("Aggregation functions not allowed within filters");
                }
            }

            return constituents;
        }

        private static void RecursiveAndConstituents(
            IList<ExprNode> constituents,
            ExprNode exprNode)
        {
            foreach (var inner in exprNode.ChildNodes) {
                if (inner is ExprAndNode) {
                    RecursiveAndConstituents(constituents, inner);
                }
                else {
                    constituents.Add(inner);
                }
            }
        }

        internal static bool IsLimitedValueExpression(ExprNode node)
        {
            var visitor = new FilterSpecExprNodeVisitorValueLimitedExpr();
            node.Accept(visitor);
            return visitor.IsLimited;
        }

        internal static EventType GetArrayInnerEventType(
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string streamName)
        {
            var arrayEventType = arrayEventTypes.Get(streamName);
            var prop = ((MapEventType) arrayEventType.First).Types.Get(streamName);
            return ((EventType[]) prop)[0];
        }

        // expressions automatically coerce to the most upwards type
        // filters require the same type
        internal static object HandleConstantsCoercion(
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

        internal static FilterSpecParamFilterForEvalDoubleForge GetIdentNodeDoubleEval(
            ExprIdentNode node,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            if (node.StreamId == 0) {
                return null;
            }

            if (arrayEventTypes != null && !arrayEventTypes.IsEmpty() && arrayEventTypes.ContainsKey(node.ResolvedStreamName)) {
                var indexAndProp = GetStreamIndex(node.ResolvedPropertyName);
                var eventType = GetArrayInnerEventType(arrayEventTypes, node.ResolvedStreamName);
                return new FilterForEvalEventPropIndexedDoubleForge(node.ResolvedStreamName, indexAndProp.First, indexAndProp.Second, eventType);
            }

            return new FilterForEvalEventPropDoubleForge(node.ResolvedStreamName, node.ResolvedPropertyName, node.ExprEvaluatorIdent);
        }

        internal static bool IsLimitedLookupableExpression(ExprNode node)
        {
            var visitor = new FilterSpecExprNodeVisitorLookupableLimitedExpr();
            node.Accept(visitor);
            return visitor.IsLimited && visitor.HasStreamZeroReference;
        }

        internal static ExprFilterSpecLookupableForge MakeLimitedLookupableForgeMayNull(
            ExprNode lookupable,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            if (!HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.LKUPCOMPOSITE, raw, services)) {
                return null;
            }

            var lookupableType = lookupable.Forge.EvaluationType;
            var expression = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(lookupable);
            var getterForge = new FilterSpecCompilerIndexLimitedLookupableGetterForge(lookupable);
            var serde = services.SerdeResolver.SerdeForFilter(lookupableType, raw);
            return new ExprFilterSpecLookupableForge(expression, getterForge, null, lookupableType, true, serde);
        }

        internal static FilterSpecPlanPathTripletForge MakeRemainingNode(
            IList<ExprNode> unassignedExpressions,
            FilterSpecCompilerArgs args)
        {
            if (unassignedExpressions.IsEmpty()) {
                throw new ArgumentException();
            }

            // any unoptimized expression nodes are put under one AND
            ExprNode exprNode;
            if (unassignedExpressions.Count == 1) {
                exprNode = unassignedExpressions[0];
            }
            else {
                exprNode = MakeValidateAndNode(unassignedExpressions, args);
            }

            var param = MakeBooleanExprParam(exprNode, args);
            return new FilterSpecPlanPathTripletForge(param, null);
        }

        private static ExprAndNode MakeValidateAndNode(
            IList<ExprNode> remainingExprNodes,
            FilterSpecCompilerArgs args)
        {
            var andNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAnd(remainingExprNodes);
            var validationContext = new ExprValidationContextBuilder(args.streamTypeService, args.statementRawInfo, args.compileTimeServices)
                .WithAllowBindingConsumption(true)
                .WithContextDescriptor(args.contextDescriptor)
                .Build();
            andNode.Validate(validationContext);
            return andNode;
        }

        internal static bool HasLevelOrHint(
            FilterSpecCompilerIndexPlannerHint requiredHint,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            var config = services.Configuration.Compiler.Execution.FilterIndexPlanning;
            if (config == ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED) {
                return true;
            }

            var hints = HintEnum.FILTERINDEX.GetHintAssignedValues(raw.Annotations);
            if (hints == null) {
                return false;
            }

            foreach (var hint in hints) {
                var hintAtoms = HintEnumExtensions.SplitCommaUnlessInParen(hint);
                for (var i = 0; i < hintAtoms.Length; i++) {
                    var hintAtom = hintAtoms[i];
                    var hintLowercase = hintAtom.ToLowerInvariant().Trim();
                    FilterSpecCompilerIndexPlannerHint? found = null;
                    foreach (var available in EnumHelper.GetValues<FilterSpecCompilerIndexPlannerHint>()) {
                        if (hintLowercase.Equals(available.GetNameInvariant())) {
                            found = available;
                            if (requiredHint == available) {
                                return true;
                            }
                        }
                    }

                    if (found == null) {
                        throw new ExprValidationException("Unrecognized filterindex hint value '" + hintAtom + "'");
                    }
                }
            }

            return false;
        }

        private static FilterSpecParamForge MakeBooleanExprParam(
            ExprNode exprNode,
            FilterSpecCompilerArgs args)
        {
            var hasSubselectFilterStream = DetermineSubselectFilterStream(exprNode);
            var hasTableAccess = DetermineTableAccessFilterStream(exprNode);

            var visitor = new ExprNodeVariableVisitor(args.compileTimeServices.VariableCompileTimeResolver);
            exprNode.Accept(visitor);
            var hasVariable = visitor.IsVariables;

            var evalType = exprNode.Forge.EvaluationType;
            var serdeForge = args.compileTimeServices.SerdeResolver.SerdeForFilter(evalType, args.statementRawInfo);
            var lookupable = new ExprFilterSpecLookupableForge(PROPERTY_NAME_BOOLEAN_EXPRESSION, null, null, evalType, false, serdeForge);

            return new FilterSpecParamExprNodeForge(
                lookupable,
                FilterOperator.BOOLEAN_EXPRESSION,
                exprNode,
                args.taggedEventTypes,
                args.arrayEventTypes,
                args.streamTypeService,
                hasSubselectFilterStream,
                hasTableAccess,
                hasVariable,
                args.compileTimeServices);
        }

        private static bool DetermineTableAccessFilterStream(ExprNode exprNode)
        {
            var visitor = new ExprNodeTableAccessFinderVisitor();
            exprNode.Accept(visitor);
            return visitor.HasTableAccess;
        }

        private static bool DetermineSubselectFilterStream(ExprNode exprNode)
        {
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            exprNode.Accept(visitor);
            if (visitor.Subselects.IsEmpty()) {
                return false;
            }

            foreach (var subselectNode in visitor.Subselects) {
                if (subselectNode.IsFilterStreamSubselect) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace