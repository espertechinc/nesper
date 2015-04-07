///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.service
{
    public class ExceptionHandlingService {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _engineURI;

        public event ExceptionHandler UnhandledException;
        public event ConditionHandler UnhandledCondition;

        public ExceptionHandlingService(string engineURI,
                                        IEnumerable<ExceptionHandler> exceptionHandlers,
                                        IEnumerable<ConditionHandler> conditionHandlers)
        {
            _engineURI = engineURI;
            foreach (var handler in exceptionHandlers) UnhandledException += handler;
            foreach (var handler in conditionHandlers) UnhandledCondition += handler;
        }    
        public void HandleCondition(BaseCondition condition, EPStatementHandle handle)
        {
            if (UnhandledCondition == null)
            {
                Log.Info("Condition encountered processing statement '{0}' statement text '{1}' : {2}", handle.StatementName, handle.EPL, condition);
                return;
            }

            UnhandledCondition(
                new ConditionHandlerContext(
                    _engineURI, handle.StatementName, handle.EPL, condition));
        }
    
        public void HandleException(Exception ex, EPStatementAgentInstanceHandle handle)
        {
            if (UnhandledException == null)
            {
                Log.Error(string.Format("Exception encountered processing statement '{0}' statement text '{1}' : {2}", handle.StatementHandle.StatementName, handle.StatementHandle.EPL, ex.Message), ex);
                return;
            }

            UnhandledException(
                new ExceptionHandlerContext(
                    _engineURI, ex, handle.StatementHandle.StatementName, handle.StatementHandle.EPL));
        }

        public string EngineURI
        {
            get { return _engineURI; }
        }
    }
}
