///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Context object passed to <seealso cref="StatementNameResolver"/> or 
    /// <seealso cref="StatementUserObjectResolver" /> to help in determining the 
    /// right statement name or user object for a statement deployed via the 
    /// deployment admin API.
    /// </summary>
    public class StatementDeploymentContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="epl">EPL expression</param>
        /// <param name="module">encapsulating module</param>
        /// <param name="moduleItem">item in module</param>
        /// <param name="deploymentId">deployment id</param>
        public StatementDeploymentContext(String epl, Module module, ModuleItem moduleItem, String deploymentId)
        {
            Epl = epl;
            Module = module;
            ModuleItem = moduleItem;
            DeploymentId = deploymentId;
        }

        /// <summary>Returns the EPL expression. </summary>
        /// <value>EPL</value>
        public string Epl { get; private set; }

        /// <summary>Returns the module. </summary>
        /// <value>module</value>
        public Module Module { get; private set; }

        /// <summary>Returns the deployment id. </summary>
        /// <value>deployment id</value>
        public string DeploymentId { get; private set; }

        /// <summary>Returns the module item. </summary>
        /// <value>module item</value>
        public ModuleItem ModuleItem { get; private set; }
    }
}