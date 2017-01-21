///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Output condition handling crontab-at schedule output.
	/// </summary>
	public sealed class OutputConditionPolledCrontabFactory : OutputConditionPolledFactory
	{
	    private readonly ExprEvaluator[] _expressions;

	    public OutputConditionPolledCrontabFactory(IList<ExprNode> scheduleSpecExpressionList, StatementContext statementContext)
	    {
	        var validationContext = new ExprValidationContext(
	            new StreamTypeServiceImpl(statementContext.EngineURI, false),
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null, 
                statementContext.SchedulingService, 
                statementContext.VariableService, 
                statementContext.TableService,
	            new ExprEvaluatorContextStatement(statementContext, false),
                statementContext.EventAdapterService,
	            statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations,
	            statementContext.ContextDescriptor, 
                statementContext.ScriptingService,
                false, false, false, false, null, false);
	        _expressions = new ExprEvaluator[scheduleSpecExpressionList.Count];
	        var count = 0;
	        foreach (var parameters in scheduleSpecExpressionList) {
	            var node = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, parameters, validationContext);
	            _expressions[count++] = node.ExprEvaluator;
	        }
	    }

	    public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext) {
	        ScheduleSpec scheduleSpec;
	        try {
	            var scheduleSpecParameterList = Evaluate(_expressions, agentInstanceContext);
	            scheduleSpec = ScheduleSpecUtil.ComputeValues(scheduleSpecParameterList);
	        }
	        catch (ScheduleParameterException e) {
	            throw new ArgumentException("Invalid schedule specification : " + e.Message, e);
	        }
	        var state = new OutputConditionPolledCrontabState(scheduleSpec, null, 0);
	        return new OutputConditionPolledCrontab(agentInstanceContext, state);
	    }

	    public OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state) {
	        return new OutputConditionPolledCrontab(agentInstanceContext, (OutputConditionPolledCrontabState) state);
	    }

	    private static object[] Evaluate(ExprEvaluator[] parameters, ExprEvaluatorContext exprEvaluatorContext)
	    {
            var evaluateParams = new EvaluateParams(null, true, exprEvaluatorContext);
            var results = new object[parameters.Length];
	        var count = 0;
	        foreach (var expr in parameters)
            {
	            try
	            {
	                results[count] = expr.Evaluate(evaluateParams);
	                count++;
	            }
	            catch (Exception ex) {
	                var message = "Failed expression evaluation in crontab timer-at for parameter " + count + ": " + ex.Message;
	                Log.Error(message, ex);
	                throw new ArgumentException(message);
	            }
	        }
	        return results;
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
