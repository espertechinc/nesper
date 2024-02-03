///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
        /// <param name = "deploymentId">deployment id</param>
        /// <param name = "statementName">statement name</param>
        /// <param name = "epStatement">statement</param>
        public EPDataFlowEPStatementFilterContext(
            string deploymentId,
            string statementName,
            object epStatement)
        {
            this.deploymentId = deploymentId;
            this.statementName = statementName;
            this.epStatement = epStatement;
        }

        public string DeploymentId => deploymentId;

        public string StatementName => statementName;

        public object EpStatement => epStatement;
    }
} // end of namespace