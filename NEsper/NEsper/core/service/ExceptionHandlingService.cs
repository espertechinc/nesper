///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.service
{
    public class ExceptionHandlingService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _engineURI;

        public event ExceptionHandler UnhandledException;
        public event ConditionHandler UnhandledCondition;

        public ExceptionHandlingService(
            string engineURI,
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

        public void HandleException(
            Exception ex,
            EPStatementAgentInstanceHandle handle,
            ExceptionHandlerExceptionType type)
        {
            HandleException(ex, handle.StatementHandle.StatementName, handle.StatementHandle.EPL, type);
        }

        public void HandleException(Exception ex, String statementName, String epl, ExceptionHandlerExceptionType type)
        {
            if (UnhandledException == null)
            {
                var writer = new StringWriter();
                if (type == ExceptionHandlerExceptionType.PROCESS)
                {
                    writer.Write("Exception encountered processing ");
                }
                else
                {
                    writer.Write("Exception encountered performing instance stop for ");
                }
                writer.Write("statement '");
                writer.Write(statementName);
                writer.Write("' expression '");
                writer.Write(epl);
                writer.Write("' : ");
                writer.Write(ex.Message);
                
                var message = writer.ToString();

                if (type == ExceptionHandlerExceptionType.PROCESS)
                {
                    Log.Error(message, ex);
                }
                else
                {
                    Log.Warn(message, ex);
                }

                return;
            }

            UnhandledException(
                new ExceptionHandlerContext(_engineURI, ex, statementName, epl, type));
        }

        public string EngineURI
        {
            get { return _engineURI; }
        }
    }
}
