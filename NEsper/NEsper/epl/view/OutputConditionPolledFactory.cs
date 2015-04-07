///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Factory for output condition instances that are polled/queried only.
    /// </summary>
    public class OutputConditionPolledFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates an output condition instance.
        /// </summary>
        /// <param name="outputLimitSpec">specifies what kind of condition to create</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns>instance for handling output condition</returns>
        public static OutputConditionPolled CreateCondition(
            OutputLimitSpec outputLimitSpec,
            AgentInstanceContext agentInstanceContext)
        {
            if (outputLimitSpec == null)
            {
                throw new ArgumentNullException("outputLimitSpec", "Output condition by count requires a non-null callback");
            }

            // Check if a variable is present
            VariableReader reader = null;
            if (outputLimitSpec.VariableName != null)
            {
                reader = agentInstanceContext.StatementContext.VariableService.GetReader(
                    outputLimitSpec.VariableName, agentInstanceContext.AgentInstanceId);
                if (reader == null)
                {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
                }
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB)
            {
                return new OutputConditionPolledCrontab(outputLimitSpec.CrontabAtSchedule, agentInstanceContext);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION)
            {
                return new OutputConditionPolledExpression(
                    outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, agentInstanceContext);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".createCondition creating OutputConditionCount with event rate " + outputLimitSpec);
                }

                if ((reader != null) && (!TypeHelper.IsNumericNonFP(reader.VariableMetaData.VariableType)))
                {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
                }

                int rate = -1;
                if (outputLimitSpec.Rate != null)
                {
                    rate = outputLimitSpec.Rate.AsInt();
                }
                return new OutputConditionPolledCount(rate, reader);
            }
            else
            {
                if ((reader != null) && (!TypeHelper.IsNumeric(reader.VariableMetaData.VariableType)))
                {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
                }

                return new OutputConditionPolledTime(outputLimitSpec.TimePeriodExpr, agentInstanceContext);
            }
        }
    }
}
