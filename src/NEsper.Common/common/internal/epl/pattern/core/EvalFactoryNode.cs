///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public interface EvalFactoryNode
    {
        short FactoryNodeId { get; }

        string TextForAudit { get; }

        bool IsFilterChildNonQuitting { get; }

        bool IsStateful { get; }

        EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode);

        void Accept(EvalFactoryNodeVisitor visitor);
    }
} // end of namespace