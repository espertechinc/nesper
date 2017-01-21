///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Returned by the <seealso cref="EPDeploymentAdmin.GetDeploymentOrder" /> 
    /// operation to holds an ordered list of modules considering each module's 
    /// uses-dependencies on other modules.
    /// </summary>
    [Serializable]
    public class DeploymentOrder
    {
        /// <summary>Cotr. </summary>
        /// <param name="ordered">list of modules</param>
        public DeploymentOrder(IList<Module> ordered)
        {
            Ordered = ordered;
        }

        /// <summary>Returns the list of modules. </summary>
        /// <value>modules</value>
        public IList<Module> Ordered { get; private set; }
    }
}
