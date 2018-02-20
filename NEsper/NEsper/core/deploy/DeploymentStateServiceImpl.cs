///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.deploy;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.util;

namespace com.espertech.esper.core.deploy
{
    /// <summary>Implementation for storing deployment state. </summary>
    public class DeploymentStateServiceImpl : DeploymentStateService
    {
        private readonly IDictionary<String, DeploymentInformation> _deployments;
        private readonly ILockable _lock; 
    
        public DeploymentStateServiceImpl(ILockManager lockManager)
        {
            _lock = lockManager.CreateLock(GetType());
            _deployments = new ConcurrentDictionary<String, DeploymentInformation>();
        }

        public string NextDeploymentId
        {
            get { return UuidGenerator.Generate(); }
        }

        public DeploymentInformation[] AllDeployments
        {
            get
            {
                using (_lock.Acquire())
                {
                    return _deployments.Values.ToArrayOrNull();
                }
            }
        }

        public void AddUpdateDeployment(DeploymentInformation descriptor)
        {
            using(_lock.Acquire())
            {
                _deployments.Put(descriptor.DeploymentId, descriptor);
            }
        }
    
        public void Remove(String deploymentId)
        {
            using(_lock.Acquire())
            {
                _deployments.Remove(deploymentId);
            }
        }

        public string[] Deployments
        {
            get
            {
                using (_lock.Acquire())
                {
                    ICollection<String> keys = _deployments.Keys;
                    return keys.ToArray();
                }
            }
        }

        public DeploymentInformation GetDeployment(String deploymentId)
        {
            using(_lock.Acquire())
            {
                if (deploymentId == null)
                {
                    return null;
                }
                return _deployments.Get(deploymentId);
            }
        }
    
        public void Dispose()
        {
            using(_lock.Acquire())
            {
                _deployments.Clear();
            }
        }
    }
}
