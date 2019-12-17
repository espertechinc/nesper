///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyScript : MethodTargetStrategy,
        MethodTargetStrategyFactory,
        StatementReadyCallback
    {
        public ScriptEvaluator ScriptEvaluator { get; set; }

        public object Invoke(
            object lookupValues,
            AgentInstanceContext agentInstanceContext)
        {
            return ScriptEvaluator.Evaluate(lookupValues, agentInstanceContext);
        }

        public string Plan => GetType().GetSimpleName();

        public MethodTargetStrategy Make(AgentInstanceContext agentInstanceContext)
        {
            return this;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            // no action
        }
    }
} // end of namespace