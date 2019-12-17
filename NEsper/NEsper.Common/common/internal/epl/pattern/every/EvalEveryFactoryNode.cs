///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.every
{
    /// <summary>
    ///     This class represents an 'every' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryFactoryNode : EvalFactoryNodeBase
    {
        public EvalFactoryNode ChildNode { get; set; }

        public override bool IsFilterChildNonQuitting => true;

        public override bool IsStateful => true;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNode, agentInstanceContext, parentNode);
            return new EvalEveryNode(agentInstanceContext, this, child);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            ChildNode.Accept(visitor);
        }
    }
} // end of namespace