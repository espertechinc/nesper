///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public interface DeploymentLifecycleService
    {
        string[] DeploymentIds { get; }

        IDictionary<string, DeploymentInternal> DeploymentMap { get; }

        CopyOnWriteList<DeploymentStateListener> Listeners { get; }

        void AddDeployment(
            string deploymentId,
            DeploymentInternal deployment);

        DeploymentInternal RemoveDeployment(string deploymentId);

        DeploymentInternal GetDeploymentByCRC(long deploymentId);

        DeploymentInternal GetDeploymentById(string deploymentId);

        EPStatement GetStatementByName(
            string deploymentId,
            string statementName);
    }
} // end of namespace