///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionPolledCrontab : OutputConditionPolled
    {
        private long? _currentReferencePoint;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly ScheduleSpec _scheduleSpec;
        private long _nextScheduledTime;

        /// <summary>Constructor. </summary>
        /// <param name="agentInstanceContext">is the view context for time scheduling</param>
        /// <param name="scheduleSpecExpressionList">list of schedule parameters</param>
        /// <throws><seealso cref="ExprValidationException" /> if the crontab expression failed to validate</throws>
        public OutputConditionPolledCrontab(
            IList<ExprNode> scheduleSpecExpressionList,
            AgentInstanceContext agentInstanceContext)
        {
            if (agentInstanceContext == null)
            {
                const string message = "OutputConditionTime requires a non-null view context";
                throw new ArgumentNullException("agentInstanceContext", message);
            }

            _agentInstanceContext = agentInstanceContext;

            // Validate the expression
            var expressions = new ExprEvaluator[scheduleSpecExpressionList.Count];
            var count = 0;
            var validationContext =
                new ExprValidationContext(
                    new StreamTypeServiceImpl(agentInstanceContext.StatementContext.EngineURI, false),
                    agentInstanceContext.StatementContext.MethodResolutionService, null,
                    agentInstanceContext.StatementContext.SchedulingService,
                    agentInstanceContext.StatementContext.VariableService,
                    agentInstanceContext.StatementContext.TableService, agentInstanceContext,
                    agentInstanceContext.StatementContext.EventAdapterService,
                    agentInstanceContext.StatementContext.StatementName,
                    agentInstanceContext.StatementContext.StatementId,
                    agentInstanceContext.StatementContext.Annotations,
                    agentInstanceContext.StatementContext.ContextDescriptor,
                    agentInstanceContext.StatementContext.ScriptingService,
                    false, false, false, false, null, false);

            foreach (var parameters in scheduleSpecExpressionList)
            {
                var node = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, parameters, validationContext);
                expressions[count++] = node.ExprEvaluator;
            }

            try
            {
                var scheduleSpecParameterList = Evaluate(expressions, agentInstanceContext);
                _scheduleSpec = ScheduleSpecUtil.ComputeValues(scheduleSpecParameterList);
            }
            catch (ScheduleParameterException e)
            {
                throw new ArgumentException("Invalid schedule specification : " + e.Message, e);
            }
        }

        public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".updateOutputCondition, " +
                    "  newEventsCount==" + newEventsCount +
                    "  oldEventsCount==" + oldEventsCount);
            }

            var output = false;
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            if (_currentReferencePoint == null)
            {
                _currentReferencePoint = currentTime;
                _nextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(_scheduleSpec, currentTime, _agentInstanceContext.StatementContext.MethodResolutionService.EngineImportService.TimeZone);
                output = true;
            }

            if (_nextScheduledTime <= currentTime)
            {
                _nextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(_scheduleSpec, currentTime, _agentInstanceContext.StatementContext.MethodResolutionService.EngineImportService.TimeZone);
                output = true;
            }

            return output;
        }

        private static Object[] Evaluate(ExprEvaluator[] parameters, ExprEvaluatorContext exprEvaluatorContext)
        {
            var results = new Object[parameters.Length];
            var count = 0;
            foreach (var expr in parameters)
            {
                try
                {
                    results[count] = expr.Evaluate(new EvaluateParams(null, true, exprEvaluatorContext));
                    count++;
                }
                catch (Exception ex)
                {
                    var message = string.Format("Failed expression evaluation in crontab timer-at for parameter {0}: {1}", count, ex.Message);
                    Log.Error(message, ex);
                    throw new ArgumentException(message);
                }
            }
            return results;
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
