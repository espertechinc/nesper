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

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionCountFactory : OutputConditionFactory
    {
        internal readonly long eventRate;
        internal readonly Variable variable;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="eventRate">
        ///     is the number of old or new events thatmust arrive in order for the condition to be satisfied
        /// </param>
        /// <param name="variable">varianle</param>
        public OutputConditionCountFactory(int eventRate, Variable variable)
        {
            if (eventRate < 1 && variable == null) {
                throw new ArgumentException(
                    "Limiting output by event count requires an event count of at least 1 or a variable name");
            }

            this.eventRate = eventRate;
            this.variable = variable;
        }

        public long EventRate => eventRate;

        public object Variable => variable;

        public OutputCondition InstantiateOutputCondition(
            AgentInstanceContext agentInstanceContext, OutputCallback outputCallback)
        {
            VariableReader variableReader = null;
            if (variable != null) {
                variableReader = agentInstanceContext.StatementContext.VariableManagementService.GetReader(
                    variable.DeploymentId, variable.MetaData.VariableName, agentInstanceContext.AgentInstanceId);
            }

            return new OutputConditionCount(outputCallback, eventRate, variableReader);
        }
    }
} // end of namespace