///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.pool
{
    public interface PatternSubexpressionPoolRuntimeSvc
    {
        void AddPatternContext(
            int statementId,
            string statementName,
            PatternSubexpressionPoolStmtHandler stmtCounts);

        void RemoveStatement(int statementId);

        void DecreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext);

        bool TryIncreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext);

        void ForceIncreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace