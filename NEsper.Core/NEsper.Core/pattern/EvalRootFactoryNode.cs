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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class is always the root node in the evaluation tree representing an event expression.
    /// It hold the handle to the EPStatement implementation for notifying when matches are found.
    /// </summary>
    public class EvalRootFactoryNode : EvalNodeFactoryBase
    {
        public readonly int numTreeChildNodes;
    
        public EvalRootFactoryNode(EvalFactoryNode childNode) {
            AddChildNode(childNode);
            this.numTreeChildNodes = AssignFactoryNodeIds();
        }
    
        private static IList<EvalFactoryNode> CollectFactories(EvalRootFactoryNode rootFactory) {
            var factories = new List<EvalFactoryNode>(8);
            foreach (EvalFactoryNode factoryNode in rootFactory.ChildNodes) {
                CollectFactoriesRecursive(factoryNode, factories);
            }
            return factories;
        }
    
        private static void CollectFactoriesRecursive(EvalFactoryNode factoryNode, List<EvalFactoryNode> factories) {
            factories.Add(factoryNode);
            foreach (EvalFactoryNode childNode in factoryNode.ChildNodes) {
                CollectFactoriesRecursive(childNode, factories);
            }
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode) {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(this.ChildNodes, agentInstanceContext, parentNode);
            return new EvalRootNode(agentInstanceContext, this, child);
        }
    
        public override String ToString()
        {
            return "EvalRootNode children=" + this.ChildNodes.Count;
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return this.ChildNodes[0].IsStateful; }
        }

        public int NumTreeChildNodes
        {
            get { return numTreeChildNodes; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if (!ChildNodes.IsEmpty()) {
                ChildNodes[0].ToEPL(writer, Precedence);
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.MINIMUM; }
        }

        // assign factory ids, a short-type number assigned once-per-statement to each pattern node
        // return the count of all ids
        private int AssignFactoryNodeIds() {
            short count = 0;
            FactoryNodeId = count;
            IList<EvalFactoryNode> factories = CollectFactories(this);
            foreach (EvalFactoryNode factoryNode in factories) {
                count++;
                factoryNode.FactoryNodeId = count;
            }
            return count;
        }
    }
} // end of namespace
