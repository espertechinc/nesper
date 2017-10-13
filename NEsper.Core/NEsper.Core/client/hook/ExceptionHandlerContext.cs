///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Context provided to <see cref="ExceptionHandler"/> implementations providing 
    /// exception-contextual information as well as the exception itself.
    /// <para/>
    /// Statement information pertains to the statement currently being processed when 
    /// the unchecked exception occured.
    /// </summary>
    public class ExceptionHandlerContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="exception">exception</param>
        /// <param name="statementName">statement name</param>
        /// <param name="epl">statement EPL expression text</param>
        /// <param name="exceptionType"></param>
        /// <param name="currentEvent"></param>
        public ExceptionHandlerContext(
            string engineURI,
            Exception exception,
            string statementName,
            string epl,
            ExceptionHandlerExceptionType exceptionType,
            EventBean currentEvent)
        {
            EngineURI = engineURI;
            Exception = exception;
            StatementName = statementName;
            Epl = epl;
            ExceptionType = exceptionType;
            CurrentEvent = currentEvent;
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; private set; }

        /// <summary>Returns the exception. </summary>
        /// <value>exception</value>
        public Exception Exception { get; private set; }

        /// <summary>Returns the statement name, if provided, or the statement id assigned to the statement if no name was provided. </summary>
        /// <value>statement name or id</value>
        public string StatementName { get; private set; }

        /// <summary>Returns the expression text of the statement. </summary>
        /// <value>statement.</value>
        public string Epl { get; private set; }

        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        /// <value>
        /// The type of the exception.
        /// </value>
        public ExceptionHandlerExceptionType ExceptionType { get; private set; }

        /// <summary>
        /// Gets the current event, when available.
        /// </summary>
        /// <value>
        /// The current event.
        /// </value>
        public EventBean CurrentEvent { get; private set; }
    }
}
