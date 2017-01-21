///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.variable;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.db;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Viewable providing historical data from a database.
    /// </summary>
    public abstract class MethodPollingExecStrategyBase : PollExecStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        protected readonly EventAdapterService EventAdapterService;
        protected readonly FastMethod Method;
        protected readonly EventType EventType;
        protected readonly Object InvocationTarget;
        protected readonly MethodPollingExecStrategyEnum Strategy;
        protected readonly VariableReader VariableReader;
        protected readonly String VariableName;
        protected readonly VariableService VariableService;

        protected MethodPollingExecStrategyBase(
            EventAdapterService eventAdapterService,
            FastMethod method,
            EventType eventType,
            Object invocationTarget,
            MethodPollingExecStrategyEnum strategy,
            VariableReader variableReader,
            String variableName,
            VariableService variableService)
        {
            EventAdapterService = eventAdapterService;
            Method = method;
            EventType = eventType;
            InvocationTarget = invocationTarget;
            Strategy = strategy;
            VariableReader = variableReader;
            VariableName = variableName;
            VariableService = variableService;
        }
    
        protected abstract IList<EventBean> HandleResult(Object invocationResult);
    
        protected bool CheckNonNullArrayValue(Object value)
        {
            if (value == null) {
                Log.Warn("Expected non-null return result from method '" + Method.Name + "', but received null array element value");
                return false;
            }
            return true;
        }
    
        public void Start()
        {
        }
    
        public void Done()
        {
        }
    
        public void Dispose()
        {
        }
    
        public IList<EventBean> Poll(Object[] lookupValues, ExprEvaluatorContext exprEvaluatorContext)
        {
            switch (Strategy)
            {
                case MethodPollingExecStrategyEnum.TARGET_CONST:
                    return InvokeInternal(lookupValues, InvocationTarget);
                case MethodPollingExecStrategyEnum.TARGET_VAR:
                    return InvokeInternalVariable(lookupValues, VariableReader);
                case MethodPollingExecStrategyEnum.TARGET_VAR_CONTEXT:
                    var reader = VariableService.GetReader(VariableName, exprEvaluatorContext.AgentInstanceId);
                    if (reader == null)
                    {
                        return null;
                    }
                    return InvokeInternalVariable(lookupValues, reader);
                default:
                    throw new NotSupportedException("unrecognized strategy " + Strategy);
            }
        }

        private IList<EventBean> InvokeInternalVariable(Object[] lookupValues, VariableReader variableReader)
        {
            var target = variableReader.Value;
            if (target == null)
            {
                return null;
            }
            if (target is EventBean)
            {
                target = ((EventBean) target).Underlying;
            }
            return InvokeInternal(lookupValues, target);
        }

        private IList<EventBean> InvokeInternal(Object[] lookupValues, Object invocationTarget)
        {
            try
            {
                var invocationResult = Method.Invoke(invocationTarget, lookupValues);
                if (invocationResult != null)
                {
                    return HandleResult(invocationResult);
                }
                return null;
            }
            catch (TargetException ex)
            {
                throw new EPException("Method '" + Method.Name + "' of class '" + Method.DeclaringType.TargetType.FullName +
                        "' reported an exception: " + ex.InnerException, ex.InnerException);
            }
        }
    }
}
