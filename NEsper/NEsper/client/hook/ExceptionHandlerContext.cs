///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ExceptionHandlerContext {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="exception">exception</param>
        /// <param name="statementName">statement name</param>
        /// <param name="epl">statement EPL expression text</param>
        public ExceptionHandlerContext(String engineURI, Exception exception, String statementName, String epl)
        {
            EngineURI = engineURI;
            Exception = exception;
            StatementName = statementName;
            Epl = epl;
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
    }
}
