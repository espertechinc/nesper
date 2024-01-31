///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client.stage;
using com.espertech.esper.runtime.client.util;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.kernel.service;

using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperDependencies;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class StagePreconditionHelper
	{
		public static void ValidateDependencyPreconditions(
			ISet<string> deploymentSet,
			EPServicesPath paths,
			DeploymentLifecycleService deploymentLifecycleService)
		{
			foreach (var deploymentId in deploymentSet) {
				var consumed = GetDependenciesConsumed(deploymentId, paths, deploymentLifecycleService);
				if (consumed == null) {
					throw new EPStageException("Deployment '" + deploymentId + "' was not found");
				}

				foreach (var item in consumed.Dependencies) {
					if (!deploymentSet.Contains(item.DeploymentId)) {
						var message = "Failed to stage deployment '" +
						              deploymentId +
						              "': Deployment consumes " +
						              item.ObjectType.GetPrettyName() +
						              " '" +
						              item.ObjectName +
						              "'" +
						              " from deployment '" +
						              item.DeploymentId +
						              "' and must therefore also be staged";
						throw new EPStagePreconditionException(message);
					}
				}

				var provided = GetDependenciesProvided(deploymentId, paths, deploymentLifecycleService);
				foreach (var item in provided.Dependencies) {
					foreach (var other in item.DeploymentIds) {
						if (!deploymentSet.Contains(other)) {
							var message = "Failed to stage deployment '" +
							              deploymentId +
							              "': Deployment provides " +
							              item.ObjectType.GetPrettyName() +
							              " '" +
							              item.ObjectName +
							              "'" +
							              " to deployment '" +
							              other +
							              "' and must therefore also be staged";
							throw new EPStagePreconditionException(message);
						}
					}
				}
			}
		}
	}
} // end of namespace
