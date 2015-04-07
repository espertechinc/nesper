///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.deploy;

namespace com.espertech.esper.core.deploy
{
    public interface IDeploymentStateService : IDisposable
    {
        string NextDeploymentId();

        string[] Deployments { get; }
        DeploymentInformation[] AllDeployments { get; }
        DeploymentInformation GetDeployment(String deploymentId);

        void AddUpdateDeployment(DeploymentInformation descriptor);
        void Remove(String deploymentId);
    }
}
