///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a guard in the evaluation tree representing an event expressions.
    /// </summary>
    [Serializable]
    public class EvalGuardNode : EvalNodeBase
    {
        private readonly EvalGuardFactoryNode _factoryNode;
        private readonly EvalNode _childNode;
    
        public EvalGuardNode(PatternAgentInstanceContext context, EvalGuardFactoryNode factoryNode, EvalNode childNode)
                    : base(context)
        {
            _factoryNode = factoryNode;
            _childNode = childNode;
        }

        public EvalGuardFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public EvalNode ChildNode
        {
            get { return _childNode; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalGuardStateNode(parentNode, this);
        }
    }
}
