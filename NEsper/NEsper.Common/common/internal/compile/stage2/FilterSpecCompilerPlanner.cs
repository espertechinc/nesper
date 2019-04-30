///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerPlanner
    {
        /// <summary>
        /// Assigned for filter parameters that are based on boolean expression and not on
        /// any particular property name.
        /// <para />Keeping this artificial property name is a simplification as optimized filter parameters
        /// generally keep a property name.
        /// </summary>
        public const string PROPERTY_NAME_BOOLEAN_EXPRESSION = ".boolean_expression";

        public static IList<FilterSpecParamForge>[] PlanFilterParameters(
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            if (validatedNodes.IsEmpty()) {
                return AllocateListArray(0);
            }

            FilterSpecParaForgeMap filterParamExprMap = new FilterSpecParaForgeMap();

            // Make filter parameter for each expression node, if it can be optimized
            DecomposePopulateConsolidate(filterParamExprMap, validatedNodes, args);

            // Use all filter parameter and unassigned expressions
            IList<FilterSpecParamForge> filterParams = new List<FilterSpecParamForge>();
            filterParams.AddAll(filterParamExprMap.FilterParams);
            int countUnassigned = filterParamExprMap.CountUnassignedExpressions();

            // we are done if there are no remaining nodes
            if (countUnassigned == 0) {
                return AllocateListArraySizeOne(filterParams);
            }

            // determine max-width
            int filterServiceMaxFilterWidth = args.compileTimeServices.Configuration.Compiler.Execution.FilterServiceMaxFilterWidth;
            var hint = HintEnum.MAX_FILTER_WIDTH.GetHint(args.statementRawInfo.Annotations);
            if (hint != null) {
                string hintValue = HintEnum.MAX_FILTER_WIDTH.GetHintAssignedValue(hint);
                filterServiceMaxFilterWidth = Int32.Parse(hintValue);
            }

            IList<FilterSpecParamForge>[] plan = null;
            if (filterServiceMaxFilterWidth > 0) {
                plan = PlanRemainingNodesIfFeasible(filterParamExprMap, args, filterServiceMaxFilterWidth);
            }

            if (plan != null) {
                return plan;
            }

            // handle no-plan
            FilterSpecParamForge node = MakeRemainingNode(filterParamExprMap.UnassignedExpressions, args);
            filterParams.Add(node);
            return AllocateListArraySizeOne(filterParams);
        }

        private static IList<FilterSpecParamForge>[] PlanRemainingNodesIfFeasible(
            FilterSpecParaForgeMap overallExpressions,
            FilterSpecCompilerArgs args,
            int filterServiceMaxFilterWidth)
        {
            IList<ExprNode> unassigned = overallExpressions.UnassignedExpressions;
            IList<ExprOrNode> orNodes = new List<ExprOrNode>(unassigned.Count);

            foreach (ExprNode node in unassigned) {
                if (node is ExprOrNode) {
                    orNodes.Add((ExprOrNode) node);
                }
            }

            FilterSpecParaForgeMap expressionsWithoutOr = new FilterSpecParaForgeMap();
            expressionsWithoutOr.Add(overallExpressions);

            // first dimension: or-node index
            // second dimension: or child node index
            FilterSpecParaForgeMap[][] orNodesMaps = new FilterSpecParaForgeMap[orNodes.Count][];
            int countOr = 0;
            int sizeFactorized = 1;
            int[] sizePerOr = new int[orNodes.Count];
            foreach (ExprOrNode orNode in orNodes) {
                expressionsWithoutOr.RemoveNode(orNode);
                orNodesMaps[countOr] = new FilterSpecParaForgeMap[orNode.ChildNodes.Length];
                int len = orNode.ChildNodes.Length;

                for (int i = 0; i < len; i++) {
                    FilterSpecParaForgeMap map = new FilterSpecParaForgeMap();
                    orNodesMaps[countOr][i] = map;
                    IList<ExprNode> nodes = Collections.SingletonList(orNode.ChildNodes[i]);
                    DecomposePopulateConsolidate(map, nodes, args);
                }

                sizePerOr[countOr] = len;
                sizeFactorized = sizeFactorized * len;
                countOr++;
            }

            // we become too large
            if (sizeFactorized > filterServiceMaxFilterWidth) {
                return null;
            }

            // combine
            IList<FilterSpecParamForge>[] result = new IList<FilterSpecParamForge>[sizeFactorized];
            IEnumerable<object[]> permutations = CombinationEnumeration.FromZeroBasedRanges(sizePerOr);
            int count = 0;
            foreach (var permutation in permutations)
            { 
                result[count] = ComputePermutation(expressionsWithoutOr, permutation, orNodesMaps, args);
                count++;
            }

            return result;
        }

        private static IList<FilterSpecParamForge> ComputePermutation(
            FilterSpecParaForgeMap filterParamExprMap,
            object[] permutation,
            FilterSpecParaForgeMap[][] orNodesMaps,
            FilterSpecCompilerArgs args)
        {
            FilterSpecParaForgeMap mapAll = new FilterSpecParaForgeMap();
            mapAll.Add(filterParamExprMap);

            // combine
            for (int orNodeNum = 0; orNodeNum < permutation.Length; orNodeNum++) {
                int orChildNodeNum = (int) permutation[orNodeNum];
                FilterSpecParaForgeMap mapOrSub = orNodesMaps[orNodeNum][orChildNodeNum];
                mapAll.Add(mapOrSub);
            }

            // consolidate across
            FilterSpecCompilerConsolidateUtil.Consolidate(mapAll, args.statementRawInfo.StatementName);

            IList<FilterSpecParamForge> filterParams = new List<FilterSpecParamForge>();
            filterParams.AddAll(mapAll.FilterParams);
            int countUnassigned = mapAll.CountUnassignedExpressions();

            if (countUnassigned == 0) {
                return filterParams;
            }

            FilterSpecParamForge node = MakeRemainingNode(mapAll.UnassignedExpressions, args);
            filterParams.Add(node);
            return filterParams;
        }

        private static void DecomposePopulateConsolidate(
            FilterSpecParaForgeMap filterParamExprMap,
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            IList<ExprNode> constituents = DecomposeCheckAggregation(validatedNodes);

            // Make filter parameter for each expression node, if it can be optimized
            foreach (ExprNode constituent in constituents) {
                FilterSpecParamForge param = FilterSpecCompilerMakeParamUtil.MakeFilterParam(
                    constituent, args.arrayEventTypes, args.statementRawInfo.StatementName);
                filterParamExprMap.Put(constituent, param); // accepts null values as the expression may not be optimized
            }

            // Consolidate entries as possible, i.e. (a != 5 and a != 6) is (a not in (5,6))
            // Removes duplicates for same property and same filter operator for filter service index optimizations
            FilterSpecCompilerConsolidateUtil.Consolidate(filterParamExprMap, args.statementRawInfo.StatementName);
        }

        private static FilterSpecParamForge MakeRemainingNode(
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

            return MakeBooleanExprParam(exprNode, args);
        }

        private static IList<FilterSpecParamForge>[] AllocateListArraySizeOne(IList<FilterSpecParamForge> @params)
        {
            IList<FilterSpecParamForge>[] arr = AllocateListArray(1);
            arr[0] = @params;
            return arr;
        }

        private static IList<FilterSpecParamForge>[] AllocateListArray(int size)
        {
            return new IList<FilterSpecParamForge>[size];
        }

        private static FilterSpecParamForge MakeBooleanExprParam(
            ExprNode exprNode,
            FilterSpecCompilerArgs args)
        {
            bool hasSubselectFilterStream = DetermineSubselectFilterStream(exprNode);
            bool hasTableAccess = DetermineTableAccessFilterStream(exprNode);

            ExprNodeVariableVisitor visitor = new ExprNodeVariableVisitor(args.compileTimeServices.VariableCompileTimeResolver);
            exprNode.Accept(visitor);
            bool hasVariable = visitor.IsVariables;

            ExprFilterSpecLookupableForge lookupable = new ExprFilterSpecLookupableForge(
                PROPERTY_NAME_BOOLEAN_EXPRESSION, null, exprNode.Forge.EvaluationType, false);

            return new FilterSpecParamExprNodeForge(
                lookupable, FilterOperator.BOOLEAN_EXPRESSION, exprNode,
                args.taggedEventTypes,
                args.arrayEventTypes,
                args.streamTypeService,
                hasSubselectFilterStream,
                hasTableAccess,
                hasVariable,
                args.compileTimeServices);
        }

        private static ExprAndNode MakeValidateAndNode(
            IList<ExprNode> remainingExprNodes,
            FilterSpecCompilerArgs args)
        {
            ExprAndNode andNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAnd(remainingExprNodes);
            ExprValidationContext validationContext =
                new ExprValidationContextBuilder(args.streamTypeService, args.statementRawInfo, args.compileTimeServices)
                    .WithAllowBindingConsumption(true).WithContextDescriptor(args.contextDescriptor).Build();
            andNode.Validate(validationContext);
            return andNode;
        }

        private static bool DetermineTableAccessFilterStream(ExprNode exprNode)
        {
            ExprNodeTableAccessFinderVisitor visitor = new ExprNodeTableAccessFinderVisitor();
            exprNode.Accept(visitor);
            return visitor.HasTableAccess;
        }

        private static bool DetermineSubselectFilterStream(ExprNode exprNode)
        {
            ExprNodeSubselectDeclaredDotVisitor visitor = new ExprNodeSubselectDeclaredDotVisitor();
            exprNode.Accept(visitor);
            if (visitor.Subselects.IsEmpty()) {
                return false;
            }

            foreach (ExprSubselectNode subselectNode in visitor.Subselects) {
                if (subselectNode.IsFilterStreamSubselect) {
                    return true;
                }
            }

            return false;
        }

        private static IList<ExprNode> DecomposeCheckAggregation(IList<ExprNode> validatedNodes)
        {
            // Break a top-level AND into constituent expression nodes
            IList<ExprNode> constituents = new List<ExprNode>();
            foreach (ExprNode validated in validatedNodes) {
                if (validated is ExprAndNode) {
                    RecursiveAndConstituents(constituents, validated);
                }
                else {
                    constituents.Add(validated);
                }

                // Ensure there is no aggregation nodes
                IList<ExprAggregateNode> aggregateExprNodes = new List<ExprAggregateNode>();
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
            foreach (ExprNode inner in exprNode.ChildNodes) {
                if (inner is ExprAndNode) {
                    RecursiveAndConstituents(constituents, inner);
                }
                else {
                    constituents.Add(inner);
                }
            }
        }
    }
} // end of namespace