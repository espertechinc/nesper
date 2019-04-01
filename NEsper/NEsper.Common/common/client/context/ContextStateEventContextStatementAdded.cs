///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Context partition state event indicating a statement added.
    /// </summary>
    public class ContextStateEventContextStatementAdded : ContextStateEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="contextDeploymentId">deployment id of create-context statement</param>
        /// <param name="contextName">context name</param>
        /// <param name="statementDeploymentId">statement deployment id</param>
        /// <param name="statementName">statement name</param>
        public ContextStateEventContextStatementAdded(
            string runtimeURI, string contextDeploymentId, string contextName, string statementDeploymentId,
            string statementName)
            : base(runtimeURI, contextDeploymentId, contextName)
        {
            StatementDeploymentId = statementDeploymentId;
            StatementName = statementName;
        }

        /// <summary>
        ///     Returns the statement name.
        /// </summary>
        /// <value>name</value>
        public string StatementName { get; }

        /// <summary>
        ///     Returns the statement deployment id.
        /// </summary>
        /// <value>deployment id</value>
        public string StatementDeploymentId { get; }
    }
} // end of namespace