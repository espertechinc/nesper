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
    /// This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalAuditNode : EvalNodeBase
    {
        private readonly EvalAuditFactoryNode _factoryNode;
        private readonly EvalNode _childNode;
    
        public EvalAuditNode(PatternAgentInstanceContext context, EvalAuditFactoryNode factoryNode, EvalNode childNode)
                    : base(context)
        {
            _factoryNode = factoryNode;
            _childNode = childNode;
        }

        public EvalAuditFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public EvalNode ChildNode
        {
            get { return _childNode; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalAuditStateNode(parentNode, this, stateNodeNumber, stateNodeId);
        }
    }
}
