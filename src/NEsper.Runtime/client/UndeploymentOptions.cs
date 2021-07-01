///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.runtime.client.util;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Option holder for use with <seealso cref="EPDeploymentService.Undeploy(string)" />
    /// </summary>
    [Serializable]
    public class UndeploymentOptions
    {
        public UndeploymentOptions()
        {
            UndeploymentLockStrategy = LockStrategyDefault.INSTANCE;
        }

        /// <summary>
        /// Return the undeployment lock strategy, the default is <seealso cref="LockStrategyDefault" />
        /// </summary>
        /// <value>lock strategy</value>
        public LockStrategy UndeploymentLockStrategy { get; set; }
    }
} // end of namespace