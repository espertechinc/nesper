///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class EvalObserverFactoryNode : EvalFactoryNodeBase
    {
        public override bool IsFilterChildNonQuitting => false;

        public override bool IsStateful => false;

        public ObserverFactory ObserverFactory { get; set; }

        public bool IsObserverStateNodeNonRestarting => ObserverFactory.IsNonRestarting;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            return new EvalObserverNode(agentInstanceContext, this);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace