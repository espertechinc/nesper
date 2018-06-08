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
using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.service
{
    public class ExceptionHandlingService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ExceptionHandlingService(
            string engineURI,
            IEnumerable<ExceptionHandler> exceptionHandlers,
            IEnumerable<ConditionHandler> conditionHandlers)
        {
            EngineURI = engineURI;

            foreach (var handler in exceptionHandlers) {
                UnhandledException += handler;
            }

            foreach (var handler in conditionHandlers) {
                UnhandledCondition += handler;
            }
        }

        public string EngineURI { get; }

        public event ExceptionHandler UnhandledException;
        public event ConditionHandler UnhandledCondition;
        //public event ExceptionHandlerInboundPool InboundPoolHandler;

        public void HandleCondition(BaseCondition condition, EPStatementHandle handle)
        {
            if (UnhandledCondition == null) {
                Log.Info(
                    "Condition encountered processing statement '{0}' statement text '{1}' : {2}", handle.StatementName,
                    handle.EPL, condition);
                return;
            }

            UnhandledCondition(
                new ConditionHandlerContext(
                    EngineURI, handle.StatementName, handle.EPL, condition));
        }

        public void HandleException(
            Exception ex,
            EPStatementAgentInstanceHandle handle,
            ExceptionHandlerExceptionType type,
            EventBean optionalCurrentEvent)
        {
            HandleException(
                ex, handle.StatementHandle.StatementName, handle.StatementHandle.EPL, type, optionalCurrentEvent);
        }

        public void HandleException(
            Exception ex,
            string statementName,
            string epl,
            ExceptionHandlerExceptionType type,
            EventBean optionalCurrentEvent)
        {
            if (UnhandledException == null) {
                var writer = new StringWriter();
                if (type == ExceptionHandlerExceptionType.PROCESS) {
                    writer.Write("Exception encountered processing ");
                }
                else {
                    writer.Write("Exception encountered performing instance stop for ");
                }

                writer.Write("statement '");
                writer.Write(statementName);
                writer.Write("' expression '");
                writer.Write(epl);
                writer.Write("' : ");
                writer.Write(ex.Message);

                var message = writer.ToString();

                if (type == ExceptionHandlerExceptionType.PROCESS) {
                    Log.Error(message, ex);
                }
                else {
                    Log.Warn(message, ex);
                }

                return;
            }

            if (UnhandledException != null) {
                UnhandledException(
                    this, new ExceptionHandlerEventArgs {
                        Context = new ExceptionHandlerContext(EngineURI, ex, statementName, epl, type, optionalCurrentEvent)
                    });
            }
        }

        public void HandleInboundPoolException(string engineURI, Exception exception, object @event)
        {
            if (UnhandledException != null) {
                UnhandledException(
                    this, new ExceptionHandlerEventArgs {
                        InboundPoolContext = new ExceptionHandlerContextUnassociated(engineURI, exception, @event)
                    });
            }
        }
    }
}