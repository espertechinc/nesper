///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>This class represents an 'or' operator in the evaluation tree representing any event expressions. </summary>
    public class EvalOrNode : EvalNodeBase
    {
        public EvalOrNode(PatternAgentInstanceContext context, EvalOrFactoryNode factoryNode, EvalNode[] childNodes)
                    : base(context)
        {
            FactoryNode = factoryNode;
            ChildNodes = childNodes;
        }

        public EvalOrFactoryNode FactoryNode { get; private set; }

        public EvalNode[] ChildNodes { get; private set; }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalOrStateNode(parentNode, this);
        }
    }
}
