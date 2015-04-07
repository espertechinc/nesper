///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Options class passed to <seealso cref="EPDeploymentAdmin.GetDeploymentOrder" />
    /// for controlling the behavior of ordering and dependency checking logic.
    /// </summary>
    public class DeploymentOrderOptions
    {
        public DeploymentOrderOptions()
        {
            IsCheckUses = true;
            IsCheckCircularDependency = true;
        }

        /// <summary>Returns true (the default) to indicate that the algorithm checks for circular dependencies among the uses-dependency graph, or false to not perform this check. </summary>
        /// <value>indicator.</value>
        public bool IsCheckCircularDependency { get; set; }

        /// <summary>Returns true (the default) to cause the algorithm to check uses-dependencies ensuring all dependencies are satisfied i.e. all dependent modules are either deployed or are part of the modules passed in, or false to not perform the checking. </summary>
        /// <value>indicator</value>
        public bool IsCheckUses { get; set; }
    }
}
