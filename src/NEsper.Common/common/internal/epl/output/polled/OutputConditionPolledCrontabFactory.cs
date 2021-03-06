///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    ///     Output condition handling crontab-at schedule output.
    /// </summary>
    public class OutputConditionPolledCrontabFactory : OutputConditionPolledFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprEvaluator[] expressions;

        public OutputConditionPolledCrontabFactory(ExprEvaluator[] expressions)
        {
            this.expressions = expressions;
        }

        public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext)
        {
            ScheduleSpec scheduleSpec;
            try {
                var scheduleSpecParameterList = Evaluate(expressions, agentInstanceContext);
                scheduleSpec = ScheduleSpecUtil.ComputeValues(scheduleSpecParameterList);
            }
            catch (ScheduleParameterException e) {
                throw new ArgumentException("Invalid schedule specification : " + e.Message, e);
            }

            var state = new OutputConditionPolledCrontabState(scheduleSpec, null, 0);
            return new OutputConditionPolledCrontab(agentInstanceContext, state);
        }

        public OutputConditionPolled MakeFromState(
            AgentInstanceContext agentInstanceContext,
            OutputConditionPolledState state)
        {
            return new OutputConditionPolledCrontab(agentInstanceContext, (OutputConditionPolledCrontabState) state);
        }

        private static object[] Evaluate(
            ExprEvaluator[] parameters,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var results = new object[parameters.Length];
            var count = 0;
            foreach (var expr in parameters) {
                try {
                    results[count] = expr.Evaluate(null, true, exprEvaluatorContext);
                    count++;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    var message = "Failed expression evaluation in crontab timer-at for parameter " +
                                  count +
                                  ": " +
                                  ex.Message;
                    Log.Error(message, ex);
                    throw new ArgumentException(message);
                }
            }

            return results;
        }
    }
} // end of namespace