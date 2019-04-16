///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.not
{
    /// <summary>
    ///     This class represents an 'not' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalNotFactoryNode : EvalFactoryNodeBase
    {
        internal EvalFactoryNode childNode;

        public override bool IsFilterChildNonQuitting => false;

        public override bool IsStateful => true;

        public void SetChildNode(EvalFactoryNode childNode)
        {
            this.childNode = childNode;
        }

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var child = EvalNodeUtil.MakeEvalNodeSingleChild(childNode, agentInstanceContext, parentNode);
            return new EvalNotNode(agentInstanceContext, this, child);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            childNode.Accept(visitor);
        }
    }
} // end of namespace