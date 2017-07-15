///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Options for use in undeployment of a module to control the behavior of the undeploy operation.
    /// </summary>
    [Serializable]
    public class UndeploymentOptions  {
    
        private bool destroyStatements = true;
        private DeploymentLockStrategy deploymentLockStrategy = DeploymentLockStrategyDefault.INSTANCE;
    
        /// <summary>
        /// Returns indicator whether undeploy will destroy any associated statements (true by default).
        /// </summary>
        /// <returns>
        /// flag indicating whether undeploy also destroys associated statements
        /// </returns>
        public bool IsDestroyStatements() {
            return destroyStatements;
        }
    
        /// <summary>
        /// Sets indicator whether undeploy will destroy any associated statements.
        /// </summary>
        /// <param name="destroyStatements">flag indicating whether undeploy also destroys associated statements</param>
        public void SetDestroyStatements(bool destroyStatements) {
            this.destroyStatements = destroyStatements;
        }
    
        /// <summary>
        /// Return the deployment lock strategy, the default is <seealso cref="DeploymentLockStrategyDefault" />
        /// </summary>
        /// <returns>lock strategy</returns>
        public DeploymentLockStrategy GetDeploymentLockStrategy() {
            return deploymentLockStrategy;
        }
    
        /// <summary>
        /// Sets the deployment lock strategy, the default is <seealso cref="DeploymentLockStrategyDefault" />
        /// </summary>
        /// <param name="deploymentLockStrategy">lock strategy</param>
        public void SetDeploymentLockStrategy(DeploymentLockStrategy deploymentLockStrategy) {
            this.deploymentLockStrategy = deploymentLockStrategy;
        }
    }
} // end of namespace
