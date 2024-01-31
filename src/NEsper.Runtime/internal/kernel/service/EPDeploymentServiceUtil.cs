///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPDeploymentServiceUtil
    {
        public static EPDeployment ToDeployment(
            DeploymentLifecycleService deploymentLifecycleService,
            string deploymentId)
        {
            var deployed = deploymentLifecycleService.GetDeploymentById(deploymentId);
            if (deployed == null) {
                return null;
            }

            var stmts = deployed.Statements;
            var copy = new EPStatement[stmts.Length];
            Array.Copy(stmts, 0, copy, 0, stmts.Length);
            return new EPDeployment(
                deploymentId,
                deployed.ModuleProvider.ModuleName,
                deployed.ModulePropertiesCached,
                copy,
                CollectionUtil.CopyArray(deployed.DeploymentIdDependencies),
                DateTimeHelper.TimeFromMillis(deployed.LastUpdateDate));
        }
    }
} // end of namespace