///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'and' operator in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalAndNode : EvalNodeBase
    {
        public EvalAndNode(PatternAgentInstanceContext context, EvalAndFactoryNode factoryNode, EvalNode[] childNodes) 
            : base(context)
        {
            FactoryNode = factoryNode;
            ChildNodes = childNodes;
        }

        public EvalAndFactoryNode FactoryNode { get; private set; }

        public EvalNode[] ChildNodes { get; private set; }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalAndStateNode(parentNode, this);
        }
    }
}
