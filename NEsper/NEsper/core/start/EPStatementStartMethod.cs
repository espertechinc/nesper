///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public interface EPStatementStartMethod
    {
        /// <summary>
        /// Starts the EPL statement.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="statementContext">statement level services</param>
        /// <param name="isNewStatement">indicator whether the statement is new or a stop-restart statement</param>
        /// <param name="isRecoveringStatement">true to indicate the statement is in the process of being recovered</param>
        /// <param name="isRecoveringResilient">true to indicate the statement is in the process of being recovered and that statement is resilient</param>
        /// <returns>
        /// a viewable to attach to for listening to events, and a stop method to invoke to clean up
        /// </returns>
        /// <throws><seealso cref="ExprValidationException" /> when the expression validation fails</throws>
        /// <throws>com.espertech.esper.view.ViewProcessingException when views cannot be started</throws>
        EPStatementStartResult Start(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient);

        StatementSpecCompiled StatementSpec { get; }
    }

    public class EPStatementStartMethodConst
    {
        public const int DEFAULT_AGENT_INSTANCE_ID = -1;
    }
}
