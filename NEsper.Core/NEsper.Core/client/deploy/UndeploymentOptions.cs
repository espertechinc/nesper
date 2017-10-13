///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Options for use in undeployment of a module to control the behavior of the undeploy operation.
    /// </summary>
    [Serializable]
    public class UndeploymentOptions  {
        public UndeploymentOptions()
        {
            IsDestroyStatements = true;
            DeploymentLockStrategy = DeploymentLockStrategyDefault.INSTANCE;
        }

        /// <summary>
        /// Returns indicator whether undeploy will destroy any associated statements (true by default).
        /// </summary>
        /// <value>
        ///   flag indicating whether undeploy also destroys associated statements
        /// </value>
        public bool IsDestroyStatements { get; set; }

        /// <summary>
        /// Return the deployment lock strategy, the default is <seealso cref="DeploymentLockStrategyDefault" />
        /// </summary>
        /// <value>lock strategy</value>
        public DeploymentLockStrategy DeploymentLockStrategy { get; set; }
    }
} // end of namespace
