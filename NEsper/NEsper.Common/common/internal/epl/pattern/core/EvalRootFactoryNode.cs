///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     This class is always the root node in the evaluation tree representing an event expression.
    ///     It hold the handle to the EPStatement implementation for notifying when matches are found.
    /// </summary>
    public class EvalRootFactoryNode : EvalFactoryNodeBase
    {
        private EvalFactoryNode _childNode;

        public EvalFactoryNode ChildNode {
            get => _childNode;
            set => _childNode = value;
        }

        public override bool IsFilterChildNonQuitting => false;

        public override bool IsStateful => _childNode.IsStateful;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var child = _childNode.MakeEvalNode(agentInstanceContext, parentNode);
            return new EvalRootNode(agentInstanceContext, this, child);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            _childNode.Accept(visitor);
        }
    }
} // end of namespace