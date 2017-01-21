///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an observer expression in the evaluation tree representing an pattern expression.
    /// </summary>
    public class EvalObserverNode : EvalNodeBase
    {
        private readonly EvalObserverFactoryNode _factoryNode;

        public EvalObserverNode(PatternAgentInstanceContext context, EvalObserverFactoryNode factoryNode)
            : base(context)
        {
            _factoryNode = factoryNode;
        }

        public EvalObserverFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalObserverStateNode(parentNode, this);
        }
    }
}
