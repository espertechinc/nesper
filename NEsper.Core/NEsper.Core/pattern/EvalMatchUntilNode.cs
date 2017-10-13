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
    /// This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalMatchUntilNode : EvalNodeBase
    {
        public EvalMatchUntilNode(PatternAgentInstanceContext context, EvalMatchUntilFactoryNode factoryNode, EvalNode childNodeSub, EvalNode childNodeUntil)
            : base(context)
        {
            FactoryNode = factoryNode;
            ChildNodeSub = childNodeSub;
            ChildNodeUntil = childNodeUntil;
        }

        public EvalMatchUntilFactoryNode FactoryNode { get; private set; }

        public EvalNode ChildNodeSub { get; private set; }

        public EvalNode ChildNodeUntil { get; private set; }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalMatchUntilStateNode(parentNode, this);
        }
    }
}
