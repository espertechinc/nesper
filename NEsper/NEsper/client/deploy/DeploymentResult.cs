///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Result of a deployment operation carries a deployment id for use in undeploy and 
    /// statement-level information.
    /// </summary>
    public class DeploymentResult
    {
        /// <summary>Ctor. </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statements">statements deployed and started</param>
        /// <param name="imports">the imports that are part of the deployment</param>
        public DeploymentResult(String deploymentId, IList<EPStatement> statements, IList<String> imports)
        {
            DeploymentId = deploymentId;
            Statements = statements;
            Imports = imports;
        }

        /// <summary>Returns the deployment id. </summary>
        /// <value>id</value>
        public string DeploymentId { get; private set; }

        /// <summary>Returns the statements. </summary>
        /// <value>statements</value>
        public IList<EPStatement> Statements { get; private set; }

        /// <summary>Returns a list of imports that were declared in the deployment. </summary>
        /// <value>imports</value>
        public IList<string> Imports { get; private set; }
    }
}
