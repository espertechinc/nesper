///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerRolloutDeploymentResult
    {
        public DeployerRolloutDeploymentResult(
            int numStatements,
            DeploymentInternal[] deployments)
        {
            NumStatements = numStatements;
            Deployments = deployments;
        }

        public int NumStatements { get; }

        public DeploymentInternal[] Deployments { get; }
    }
} // end of namespace