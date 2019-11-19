///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     This class represents a guard in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalGuardFactoryNode : EvalFactoryNodeBase
    {
        internal EvalFactoryNode childNode;
        internal GuardFactory guardFactory;

        public EvalFactoryNode ChildNode {
            set => childNode = value;
        }

        public override bool IsFilterChildNonQuitting => false;

        public override bool IsStateful => true;

        public GuardFactory GuardFactory {
            get => guardFactory;
            set => guardFactory = value;
        }

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var child = EvalNodeUtil.MakeEvalNodeSingleChild(childNode, agentInstanceContext, parentNode);
            return new EvalGuardNode(agentInstanceContext, this, child);
        }

        public override string ToString()
        {
            return "EvalGuardNode guardFactory=" + guardFactory;
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            childNode.Accept(visitor);
        }
    }
} // end of namespace