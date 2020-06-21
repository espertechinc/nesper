///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerWidthBasic
    {
        internal static FilterSpecPlanForge PlanRemainingNodesBasic(
            FilterSpecParaForgeMap overallExpressions,
            FilterSpecCompilerArgs args,
            int filterServiceMaxFilterWidth)
        {
            var unassigned = overallExpressions.UnassignedExpressions;
            IList<ExprOrNode> orNodes = new List<ExprOrNode>(unassigned.Count);

            foreach (var node in unassigned) {
                if (node is ExprOrNode) {
                    orNodes.Add((ExprOrNode) node);
                }
            }

            var expressionsWithoutOr = new FilterSpecParaForgeMap();
            expressionsWithoutOr.Add(overallExpressions);

            // first dimension: or-node index
            // second dimension: or child node index
            var orNodesMaps = new FilterSpecParaForgeMap[orNodes.Count][];
            var countOr = 0;
            var sizeFactorized = 1;
            var sizePerOr = new int[orNodes.Count];
            foreach (var orNode in orNodes) {
                expressionsWithoutOr.RemoveNode(orNode);
                orNodesMaps[countOr] = new FilterSpecParaForgeMap[orNode.ChildNodes.Length];
                var len = orNode.ChildNodes.Length;

                for (var i = 0; i < len; i++) {
                    var map = new FilterSpecParaForgeMap();
                    orNodesMaps[countOr][i] = map;
                    var nodes = Collections.SingletonList(orNode.ChildNodes[i]);
                    DecomposePopulateConsolidate(map, false, nodes, args);
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
            var result = new FilterSpecPlanPathForge[sizeFactorized];
            var count = 0;
            foreach (var permutation in CombinationEnumeration.FromZeroBasedRanges(sizePerOr)) {
                result[count] = ComputePermutation(expressionsWithoutOr, permutation, orNodesMaps, args);
                count++;
            }

            return new FilterSpecPlanForge(result, null, null, null);
        }

        private static FilterSpecPlanPathForge ComputePermutation(
            FilterSpecParaForgeMap filterParamExprMap,
            object[] permutation,
            FilterSpecParaForgeMap[][] orNodesMaps,
            FilterSpecCompilerArgs args)
        {
            var mapAll = new FilterSpecParaForgeMap();
            mapAll.Add(filterParamExprMap);

            // combine
            for (var orNodeNum = 0; orNodeNum < permutation.Length; orNodeNum++) {
                var orChildNodeNum = permutation[orNodeNum].AsInt32();
                var mapOrSub = orNodesMaps[orNodeNum][orChildNodeNum];
                mapAll.Add(mapOrSub);
            }

            // consolidate across
            FilterSpecCompilerConsolidateUtil.Consolidate(mapAll, args.statementRawInfo.StatementName);

            IList<FilterSpecPlanPathTripletForge> filterParams = new List<FilterSpecPlanPathTripletForge>(mapAll.Triplets);
            var countUnassigned = mapAll.CountUnassignedExpressions();

            if (countUnassigned != 0) {
                FilterSpecPlanPathTripletForge node = MakeRemainingNode(mapAll.UnassignedExpressions, args);
                filterParams.Add(node);
            }

            FilterSpecPlanPathTripletForge[] triplets = filterParams.ToArray();
            return new FilterSpecPlanPathForge(triplets, null);
        }
    }
} // end of namespace