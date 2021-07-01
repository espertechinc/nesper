///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public class NamedWindowManagementServiceImpl : NamedWindowManagementService
    {
        private readonly IDictionary<string, NamedWindowDeployment> deployments =
            new Dictionary<string, NamedWindowDeployment>();

        public void AddNamedWindow(
            string windowName,
            NamedWindowMetaData metadata,
            EPStatementInitServices services)
        {
            NamedWindowDeployment deployment = deployments.Get(services.DeploymentId);
            if (deployment == null) {
                deployment = new NamedWindowDeployment();
                deployments.Put(services.DeploymentId, deployment);
            }

            deployment.Add(windowName, metadata, services);
        }

        public NamedWindow GetNamedWindow(
            string deploymentId,
            string namedWindowName)
        {
            NamedWindowDeployment deployment = deployments.Get(deploymentId);
            return deployment == null ? null : deployment.GetProcessor(namedWindowName);
        }

        public int DeploymentCount {
            get => deployments.Count;
        }

        public void DestroyNamedWindow(
            string deploymentId,
            string namedWindowName)
        {
            NamedWindowDeployment deployment = deployments.Get(deploymentId);
            if (deployment == null) {
                return;
            }

            deployment.Remove(namedWindowName);
            if (deployment.IsEmpty()) {
                deployments.Remove(deploymentId);
            }
        }
    }
} // end of namespace