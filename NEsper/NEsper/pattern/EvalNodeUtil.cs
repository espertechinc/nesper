///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    public class EvalNodeUtil
    {
        /// <summary>Searched recursivly for pattern evaluation filter nodes. </summary>
        /// <param name="currentNode">is the root node</param>
        /// <returns>list of filter nodes</returns>
        public static EvalNodeAnalysisResult RecursiveAnalyzeChildNodes(EvalFactoryNode currentNode)
        {
            var evalNodeAnalysisResult = new EvalNodeAnalysisResult();
            RecursiveAnalyzeChildNodes(evalNodeAnalysisResult, currentNode);
            return evalNodeAnalysisResult;
        }

        private static void RecursiveAnalyzeChildNodes(EvalNodeAnalysisResult evalNodeAnalysisResult, EvalFactoryNode currentNode)
        {
            if ((currentNode is EvalFilterFactoryNode) ||
                (currentNode is EvalGuardFactoryNode) ||
                (currentNode is EvalObserverFactoryNode) ||
                (currentNode is EvalMatchUntilFactoryNode) ||
                (currentNode is EvalEveryDistinctFactoryNode))
            {
                evalNodeAnalysisResult.AddNode(currentNode);
            }

            foreach (EvalFactoryNode node in currentNode.ChildNodes)
            {
                RecursiveAnalyzeChildNodes(evalNodeAnalysisResult, node);
            }
        }

        /// <summary>
        /// Returns all child nodes as a set.
        /// </summary>
        /// <param name="currentNode">parent node</param>
        /// <param name="filter">The filter.</param>
        /// <returns>all child nodes</returns>
        public static ICollection<EvalFactoryNode> RecursiveGetChildNodes(EvalFactoryNode currentNode, EvalNodeUtilFactoryFilter filter)
        {
            ICollection<EvalFactoryNode> result = new LinkedHashSet<EvalFactoryNode>();
            if (filter.Consider(currentNode))
            {
                result.Add(currentNode);
            }
            RecursiveGetChildNodes(result, currentNode, filter);
            return result;
        }

        private static void RecursiveGetChildNodes(ICollection<EvalFactoryNode> set, EvalFactoryNode currentNode, EvalNodeUtilFactoryFilter filter)
        {
            foreach (var node in currentNode.ChildNodes)
            {
                if (filter.Consider(node))
                {
                    set.Add(node);
                }
                RecursiveGetChildNodes(set, node, filter);
            }
        }

        public static EvalRootNode MakeRootNodeFromFactory(EvalRootFactoryNode rootFactoryNode, PatternAgentInstanceContext patternAgentInstanceContext)
        {
            return (EvalRootNode)rootFactoryNode.MakeEvalNode(patternAgentInstanceContext);
        }

        public static EvalNode MakeEvalNodeSingleChild(IList<EvalFactoryNode> childNodes, PatternAgentInstanceContext agentInstanceContext)
        {
            return childNodes[0].MakeEvalNode(agentInstanceContext);
        }

        public static EvalNode[] MakeEvalNodeChildren(IList<EvalFactoryNode> childNodes, PatternAgentInstanceContext agentInstanceContext)
        {
            var children = new EvalNode[childNodes.Count];
            for (int i = 0; i < childNodes.Count; i++)
            {
                children[i] = childNodes[i].MakeEvalNode(agentInstanceContext);
            }
            return children;
        }
    }
}
