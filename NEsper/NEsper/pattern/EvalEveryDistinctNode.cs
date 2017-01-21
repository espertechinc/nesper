///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    [Serializable]
    public class EvalEveryDistinctNode : EvalNodeBase
    {
        private readonly EvalEveryDistinctFactoryNode _factoryNode;
        private readonly EvalNode _childNode;
    
        public EvalEveryDistinctNode(EvalEveryDistinctFactoryNode factoryNode, EvalNode childNode, PatternAgentInstanceContext agentInstanceContext)
            : base(agentInstanceContext)
        {
            _factoryNode = factoryNode;
            _childNode = childNode;
        }

        public EvalEveryDistinctFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public EvalNode ChildNode
        {
            get { return _childNode; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            if (_factoryNode.TimeDeltaComputation == null) {
                return new EvalEveryDistinctStateNode(parentNode, this);
            }
            else {
                return new EvalEveryDistinctStateExpireKeyNode(parentNode, this);
            }
        }
    }
}
