///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.or
{
    /// <summary>
    ///     This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalOrFactoryNode : EvalFactoryNodeBase
    {
        protected EvalFactoryNode[] children;

        public EvalFactoryNode[] Children {
            get => children;
            set => children = value;
        }

        public override bool IsFilterChildNonQuitting => false;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var nodes = EvalNodeUtil.MakeEvalNodeChildren(children, agentInstanceContext, parentNode);
            return new EvalOrNode(agentInstanceContext, this, nodes);
        }

        public override bool IsStateful {
            get {
                foreach (var child in children) {
                    if (child.IsStateful) {
                        return true;
                    }
                }

                return false;
            }
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in children) {
                child.Accept(visitor);
            }
        }
    }
} // end of namespace