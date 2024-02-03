///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public class PathDeploymentEntry<TE>
    {
        public PathDeploymentEntry(
            string deploymentId,
            TE entity)
        {
            DeploymentId = deploymentId;
            Entity = entity;
        }

        public string DeploymentId { get; }

        public TE Entity { get; }

        public ISet<string> Dependencies { get; private set; }

        public void AddDependency(string deploymentIdDep)
        {
            if (Dependencies == null) {
                Dependencies = new HashSet<string>();
            }

            Dependencies.Add(deploymentIdDep);
        }

        public void RemoveDependency(string deploymentId)
        {
            if (Dependencies == null) {
                return;
            }

            Dependencies.Remove(deploymentId);
            if (Dependencies.IsEmpty()) {
                Dependencies = null;
            }
        }

        public PathDeploymentEntry<TE> Copy()
        {
            TE reference;
            if (Entity is Copyable<TE> copyableEntity) {
                reference = copyableEntity.Copy();
            }
            else {
                reference = Entity;
            }

            return new PathDeploymentEntry<TE>(DeploymentId, reference);
        }
    }
} // end of namespace