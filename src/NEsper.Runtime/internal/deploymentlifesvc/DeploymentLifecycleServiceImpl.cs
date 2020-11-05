///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public class DeploymentLifecycleServiceImpl : DeploymentLifecycleService
    {
        private readonly IDictionary<long, DeploymentInternal> deploymentsByCRC = new Dictionary<long, DeploymentInternal>();
        
        private readonly int _stageId;

        public DeploymentLifecycleServiceImpl(int stageId)
        {
            this._stageId = stageId;
        }

        public void AddDeployment(
            string deploymentId,
            DeploymentInternal deployment)
        {
            var existing = DeploymentMap.Get(deploymentId);
            if (existing != null)
            {
                throw new IllegalStateException("Deployment already exists by name '" + deploymentId + "'");
            }

            var crc = CRC32Util.ComputeCRC32(deploymentId);
            existing = deploymentsByCRC.Get(crc);
            if (existing != null)
            {
                throw new IllegalStateException("Deployment already exists by same-value crc");
            }

            DeploymentMap.Put(deploymentId, deployment);
            deploymentsByCRC.Put(crc, deployment);
        }

        public string[] DeploymentIds => DeploymentMap.Keys.ToArray();

        public DeploymentInternal RemoveDeployment(string deploymentId)
        {
            var deployment = DeploymentMap.Delete(deploymentId);
            if (deployment != null)
            {
                var crc = CRC32Util.ComputeCRC32(deploymentId);
                deploymentsByCRC.Remove(crc);
            }

            return deployment;
        }

        public DeploymentInternal GetDeploymentByCRC(long deploymentId)
        {
            return deploymentsByCRC.Get(deploymentId);
        }

        public DeploymentInternal GetDeploymentById(string deploymentId)
        {
            return DeploymentMap.Get(deploymentId);
        }

        public EPStatement GetStatementByName(
            string deploymentId,
            string statementName)
        {
            var deployment = DeploymentMap.Get(deploymentId);
            if (deployment == null)
            {
                return null;
            }

            foreach (var stmt in deployment.Statements)
            {
                if (stmt.Name.Equals(statementName))
                {
                    return stmt;
                }
            }

            return null;
        }

        public IDictionary<string, DeploymentInternal> DeploymentMap { get; } = new Dictionary<string, DeploymentInternal>();

        public CopyOnWriteList<DeploymentStateListener> Listeners { get; } = new CopyOnWriteList<DeploymentStateListener>();
    }
} // end of namespace