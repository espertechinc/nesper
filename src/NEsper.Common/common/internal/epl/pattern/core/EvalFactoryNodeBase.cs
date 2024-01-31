///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public abstract class EvalFactoryNodeBase : EvalFactoryNode
    {
        public string TextForAudit { get; set; }
        public short FactoryNodeId { get; set; }

        public abstract bool IsFilterChildNonQuitting { get; }
        public abstract bool IsStateful { get; }

        public abstract EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode);

        public abstract void Accept(EvalFactoryNodeVisitor visitor);
    }
} // end of namespace