///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output limit condition that is satisfied when either
    /// the total number of new events arrived or the total number
    /// of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionPolledCountFactory : OutputConditionPolledFactory
    {
        private int eventRate;
        private Variable variable;

        public void SetEventRate(int eventRate)
        {
            this.eventRate = eventRate;
        }

        public void SetVariable(Variable variable)
        {
            this.variable = variable;
        }

        public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext)
        {
            OutputConditionPolledCountState state = new OutputConditionPolledCountState(
                eventRate,
                eventRate,
                eventRate,
                true);
            return new OutputConditionPolledCount(state, GetVariableReader(agentInstanceContext));
        }

        public OutputConditionPolled MakeFromState(
            AgentInstanceContext agentInstanceContext,
            OutputConditionPolledState state)
        {
            return new OutputConditionPolledCount(
                (OutputConditionPolledCountState) state,
                GetVariableReader(agentInstanceContext));
        }

        private VariableReader GetVariableReader(AgentInstanceContext agentInstanceContext)
        {
            if (variable == null) {
                return null;
            }

            return agentInstanceContext.VariableManagementService.GetReader(
                variable.DeploymentId,
                variable.MetaData.VariableName,
                agentInstanceContext.AgentInstanceId);
        }
    }
} // end of namespace