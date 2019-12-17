///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Context object for filter for use with the EPStatementSource operator.
    /// </summary>
    public class EPDataFlowEPStatementFilterContext
    {
        private readonly string deploymentId;
        private readonly string statementName;
        private readonly object epStatement;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statementName">statement name</param>
        /// <param name="epStatement">statement</param>
        public EPDataFlowEPStatementFilterContext(
            string deploymentId,
            string statementName,
            object epStatement)
        {
            this.deploymentId = deploymentId;
            this.statementName = statementName;
            this.epStatement = epStatement;
        }

        /// <summary>
        /// Returns the deployment id
        /// </summary>
        /// <returns>deployment id</returns>
        public string GetDeploymentId()
        {
            return deploymentId;
        }

        /// <summary>
        /// Returns the statement name
        /// </summary>
        /// <returns>statement name</returns>
        public string GetStatementName()
        {
            return statementName;
        }

        /// <summary>
        /// Returns the statement, can safely be cast to EPStatement
        /// </summary>
        /// <returns>statement</returns>
        public object GetEpStatement()
        {
            return epStatement;
        }
    }
} // end of namespace