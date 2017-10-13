///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.view
{
    public sealed class OutputConditionCountFactory : OutputConditionFactory
    {
        internal long EventRate;
        internal readonly VariableMetaData VariableMetaData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventRate">is the number of old or new events thatmust arrive in order for the condition to be satisfied</param>
        /// <param name="variableMetaData">is the variable metadata, if a variable was supplied, else null</param>
        public OutputConditionCountFactory(int eventRate, VariableMetaData variableMetaData)
        {
            if ((eventRate < 1) && (variableMetaData == null))
            {
                throw new ArgumentException("Limiting output by event count requires an event count of at least 1 or a variable name");
            }
            EventRate = eventRate;
            VariableMetaData = variableMetaData;
        }

        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback)
        {
            VariableReader variableReader = null;
            if (VariableMetaData != null)
            {
                variableReader = agentInstanceContext.StatementContext.VariableService.GetReader(
                    VariableMetaData.VariableName, agentInstanceContext.AgentInstanceId);
            }

            return new OutputConditionCount(outputCallback, EventRate, variableReader);
        }
    }
}
