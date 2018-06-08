///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.visitor;

namespace com.espertech.esper.filter
{
    public class FilterSpecCompilerPlanner
    {
        internal static IList<FilterSpecParam>[] PlanFilterParameters(
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            if (validatedNodes.IsEmpty())
            {
                return AllocateListArray(0);
            }

            var filterParamExprMap = new FilterParamExprMap();

            // Make filter parameter for each expression node, if it can be optimized
            DecomposePopulateConsolidate(filterParamExprMap, validatedNodes, args);

            // Use all filter parameter and unassigned expressions
            IList<FilterSpecParam> filterParams = new List<FilterSpecParam>();
            filterParams.AddAll(filterParamExprMap.FilterParams);
            int countUnassigned = filterParamExprMap.CountUnassignedExpressions();

            // we are done if there are no remaining nodes
            if (countUnassigned == 0)
            {
                return AllocateListArraySizeOne(filterParams);
            }

            // determine max-width
            int filterServiceMaxFilterWidth =
                args.ConfigurationInformation.EngineDefaults.Execution.FilterServiceMaxFilterWidth;
            HintAttribute hint = HintEnum.MAX_FILTER_WIDTH.GetHint(args.Annotations);
            if (hint != null)
            {
                string hintValue = HintEnum.MAX_FILTER_WIDTH.GetHintAssignedValue(hint);
                filterServiceMaxFilterWidth = int.Parse(hintValue);
            }

            IList<FilterSpecParam>[] plan = null;
            if (filterServiceMaxFilterWidth > 0)
            {
                plan = PlanRemainingNodesIfFeasible(filterParamExprMap, args, filterServiceMaxFilterWidth);
            }

            if (plan != null)
            {
                return plan;
            }

            // handle no-plan
            FilterSpecParamExprNode node = MakeRemainingNode(filterParamExprMap.UnassignedExpressions, args);
            filterParams.Add(node);
            return AllocateListArraySizeOne(filterParams);
        }

        private static IList<FilterSpecParam>[] PlanRemainingNodesIfFeasible(
            FilterParamExprMap overallExpressions,
            FilterSpecCompilerArgs args,
            int filterServiceMaxFilterWidth)

        {
            IList<ExprNode> unassigned = overallExpressions.UnassignedExpressions;
            IList<ExprOrNode> orNodes = new List<ExprOrNode>(unassigned.Count);

            foreach (ExprNode node in unassigned)
            {
                if (node is ExprOrNode)
                {
                    orNodes.Add((ExprOrNode) node);
                }
            }

            var expressionsWithoutOr = new FilterParamExprMap();
            expressionsWithoutOr.Add(overallExpressions);

            // first dimension: or-node index
            // second dimension: or child node index
            var orNodesMaps = new FilterParamExprMap[orNodes.Count][];
            int countOr = 0;
            int sizeFactorized = 1;
            var sizePerOr = new int[orNodes.Count];
            foreach (ExprOrNode orNode in orNodes)
            {
                expressionsWithoutOr.RemoveNode(orNode);
                orNodesMaps[countOr] = new FilterParamExprMap[orNode.ChildNodes.Count];
                int len = orNode.ChildNodes.Count;

                for (int i = 0; i < len; i++)
                {
                    var map = new FilterParamExprMap();
                    orNodesMaps[countOr][i] = map;
                    IList<ExprNode> nodes = Collections.SingletonList(orNode.ChildNodes[i]);
                    DecomposePopulateConsolidate(map, nodes, args);
                }

                sizePerOr[countOr] = len;
                sizeFactorized = sizeFactorized*len;
                countOr++;
            }

            // we become too large
            if (sizeFactorized > filterServiceMaxFilterWidth)
            {
                return null;
            }

            // combine
            var result = new IList<FilterSpecParam>[sizeFactorized];
            IEnumerable<object[]> permutations = CombinationEnumeration.FromZeroBasedRanges(sizePerOr);
            int count = 0;
            foreach (var permutation in permutations)
            {
                result[count] = ComputePermutation(expressionsWithoutOr, permutation, orNodesMaps, args);
                count++;
            }
            return result;
        }

        private static IList<FilterSpecParam> ComputePermutation(
            FilterParamExprMap filterParamExprMap,
            object[] permutation,
            FilterParamExprMap[][] orNodesMaps,
            FilterSpecCompilerArgs args)
        {
            var mapAll = new FilterParamExprMap();
            mapAll.Add(filterParamExprMap);

            // combine
            for (int orNodeNum = 0; orNodeNum < permutation.Length; orNodeNum++)
            {
                var orChildNodeNum = permutation[orNodeNum].AsInt();
                FilterParamExprMap mapOrSub = orNodesMaps[orNodeNum][orChildNodeNum];
                mapAll.Add(mapOrSub);
            }

            // consolidate across
            FilterSpecCompilerConsolidateUtil.Consolidate(mapAll, args.StatementName);

            IList<FilterSpecParam> filterParams = new List<FilterSpecParam>();
            filterParams.AddAll(mapAll.FilterParams);
            int countUnassigned = mapAll.CountUnassignedExpressions();

            if (countUnassigned == 0)
            {
                return filterParams;
            }

            FilterSpecParamExprNode node = MakeRemainingNode(mapAll.UnassignedExpressions, args);
            filterParams.Add(node);
            return filterParams;
        }

        private static void DecomposePopulateConsolidate(
            FilterParamExprMap filterParamExprMap,
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)

        {
            IList<ExprNode> constituents = DecomposeCheckAggregation(validatedNodes);

            // Make filter parameter for each expression node, if it can be optimized
            foreach (ExprNode constituent in constituents)
            {
                FilterSpecParam param = FilterSpecCompilerMakeParamUtil.MakeFilterParam(
                    constituent, args.ArrayEventTypes, args.ExprEvaluatorContext, args.StatementName);
                filterParamExprMap.Put(constituent, param);
                    // accepts null values as the expression may not be optimized
            }

            // Consolidate entries as possible, i.e. (a != 5 and a != 6) is (a not in (5,6))
            // Removes duplicates for same property and same filter operator for filter service index optimizations
            FilterSpecCompilerConsolidateUtil.Consolidate(filterParamExprMap, args.StatementName);
        }

        private static FilterSpecParamExprNode MakeRemainingNode(
            IList<ExprNode> unassignedExpressions,
            FilterSpecCompilerArgs args)

        {
            if (unassignedExpressions.IsEmpty())
            {
                throw new ArgumentException();
            }

            // any unoptimized expression nodes are put under one AND
            ExprNode exprNode;
            if (unassignedExpressions.Count == 1)
            {
                exprNode = unassignedExpressions[0];
            }
            else
            {
                exprNode = MakeValidateAndNode(unassignedExpressions, args);
            }
            return MakeBooleanExprParam(exprNode, args);
        }

        private static IList<FilterSpecParam>[] AllocateListArraySizeOne(IList<FilterSpecParam> @params)
        {
            IList<FilterSpecParam>[] arr = AllocateListArray(1);
            arr[0] = @params;
            return arr;
        }

        private static IList<FilterSpecParam>[] AllocateListArray(int i)
        {
            return new IList<FilterSpecParam>[i];
        }

        private static FilterSpecParamExprNode MakeBooleanExprParam(ExprNode exprNode, FilterSpecCompilerArgs args)
        {
            bool hasSubselectFilterStream = DetermineSubselectFilterStream(exprNode);
            bool hasTableAccess = DetermineTableAccessFilterStream(exprNode);
            var lookupable = new FilterSpecLookupable(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, null, exprNode.ExprEvaluator.ReturnType, false);
            return new FilterSpecParamExprNode(
                lookupable, FilterOperator.BOOLEAN_EXPRESSION, exprNode,
                args.TaggedEventTypes, 
                args.ArrayEventTypes,
                args.VariableService,
                args.TableService, 
                args.EventAdapterService,
                args.FilterBooleanExpressionFactory, 
                args.ConfigurationInformation,
                hasSubselectFilterStream, hasTableAccess);
        }

        private static ExprAndNode MakeValidateAndNode(IList<ExprNode> remainingExprNodes, FilterSpecCompilerArgs args)
        {
            ExprAndNode andNode = ExprNodeUtility.ConnectExpressionsByLogicalAnd(remainingExprNodes);
            var validationContext = new ExprValidationContext(
                args.Container,
                args.StreamTypeService,
                args.EngineImportService,
                args.StatementExtensionSvcContext, null,
                args.TimeProvider,
                args.VariableService,
                args.TableService,
                args.ExprEvaluatorContext,
                args.EventAdapterService,
                args.StatementName,
                args.StatementId,
                args.Annotations,
                args.ContextDescriptor,
                args.ScriptingService,
                false, false, true, false, null, false);
            andNode.Validate(validationContext);
            return andNode;
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
            if (visitor.Subselects.IsEmpty())
            {
                return false;
            }
            foreach (ExprSubselectNode subselectNode in visitor.Subselects)
            {
                if (subselectNode.IsFilterStreamSubselect)
                {
                    return true;
                }
            }
            return false;
        }

        private static IList<ExprNode> DecomposeCheckAggregation(IList<ExprNode> validatedNodes)
        {
            // Break a top-level AND into constituent expression nodes
            IList<ExprNode> constituents = new List<ExprNode>();
            foreach (ExprNode validated in validatedNodes)
            {
                if (validated is ExprAndNode)
                {
                    RecursiveAndConstituents(constituents, validated);
                }
                else
                {
                    constituents.Add(validated);
                }

                // Ensure there is no aggregation nodes
                var aggregateExprNodes = new List<ExprAggregateNode>();
                ExprAggregateNodeUtil.GetAggregatesBottomUp(validated, aggregateExprNodes);
                if (!aggregateExprNodes.IsEmpty())
                {
                    throw new ExprValidationException("Aggregation functions not allowed within filters");
                }
            }

            return constituents;
        }

        private static void RecursiveAndConstituents(IList<ExprNode> constituents, ExprNode exprNode)
        {
            foreach (ExprNode inner in exprNode.ChildNodes)
            {
                if (inner is ExprAndNode)
                {
                    RecursiveAndConstituents(constituents, inner);
                }
                else
                {
                    constituents.Add(inner);
                }
            }
        }
    }
} // end of namespace