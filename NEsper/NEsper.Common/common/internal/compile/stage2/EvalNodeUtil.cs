///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.everydistinct;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class EvalNodeUtil
    {
        /// <summary>Searched recursivly for pattern evaluation filter nodes. </summary>
        /// <param name="currentNode">is the root node</param>
        /// <returns>list of filter nodes</returns>
        public static EvalNodeAnalysisResult RecursiveAnalyzeChildNodes(EvalForgeNode currentNode)
        {
            var evalNodeAnalysisResult = new EvalNodeAnalysisResult();
            RecursiveAnalyzeChildNodes(evalNodeAnalysisResult, currentNode);
            return evalNodeAnalysisResult;
        }

        private static void RecursiveAnalyzeChildNodes(
            EvalNodeAnalysisResult evalNodeAnalysisResult,
            EvalForgeNode currentNode)
        {
            if (currentNode is EvalFilterForgeNode ||
                currentNode is EvalGuardForgeNode ||
                currentNode is EvalObserverForgeNode ||
                currentNode is EvalMatchUntilForgeNode ||
                currentNode is EvalEveryDistinctForgeNode) {
                evalNodeAnalysisResult.AddNode(currentNode);
            }

            if (currentNode is EvalObserverForgeNode) {
                evalNodeAnalysisResult.AddNode(currentNode);
            }

            foreach (EvalForgeNode node in currentNode.ChildNodes) {
                RecursiveAnalyzeChildNodes(evalNodeAnalysisResult, node);
            }
        }

        /// <summary>
        ///     Returns all child nodes as a set.
        /// </summary>
        /// <param name="currentNode">parent node</param>
        /// <param name="filter">The filter.</param>
        /// <returns>all child nodes</returns>
        public static ICollection<EvalForgeNode> RecursiveGetChildNodes(
            EvalForgeNode currentNode,
            EvalNodeUtilFactoryFilter filter)
        {
            ICollection<EvalForgeNode> result = new LinkedHashSet<EvalForgeNode>();
            if (filter.Consider(currentNode)) {
                result.Add(currentNode);
            }

            RecursiveGetChildNodes(result, currentNode, filter);
            return result;
        }

        private static void RecursiveGetChildNodes(
            ICollection<EvalForgeNode> set,
            EvalForgeNode currentNode,
            EvalNodeUtilFactoryFilter filter)
        {
            foreach (var node in currentNode.ChildNodes) {
                if (filter.Consider(node)) {
                    set.Add(node);
                }

                RecursiveGetChildNodes(set, node, filter);
            }
        }

        public static EvalNode MakeEvalNodeSingleChild(
            EvalFactoryNode child,
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            return child.MakeEvalNode(agentInstanceContext, parentNode);
        }

        public static EvalRootNode MakeRootNodeFromFactory(
            EvalRootFactoryNode rootFactoryNode,
            PatternAgentInstanceContext patternAgentInstanceContext)
        {
            return (EvalRootNode) rootFactoryNode.MakeEvalNode(patternAgentInstanceContext, null);
        }

        public static EvalNode[] MakeEvalNodeChildren(
            IEnumerable<EvalFactoryNode> factories,
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            return factories
                .Select(f => f.MakeEvalNode(agentInstanceContext, parentNode))
                .ToArray();
        }
    }
}