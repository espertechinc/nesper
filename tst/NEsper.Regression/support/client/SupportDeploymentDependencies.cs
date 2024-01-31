///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.client
{
	public class SupportDeploymentDependencies
	{
		public static void AssertEmpty(
			RegressionEnvironment env,
			string deployedStatementName)
		{
			env.AssertThat(() => {
				var deploymentId = env.DeploymentId(deployedStatementName);
				var consumed = env.Runtime.DeploymentService.GetDeploymentDependenciesConsumed(deploymentId);
				ClassicAssert.IsTrue(consumed.Dependencies.IsEmpty());
				var provided = env.Runtime.DeploymentService.GetDeploymentDependenciesProvided(deploymentId);
				ClassicAssert.IsTrue(provided.Dependencies.IsEmpty());
			});
		}

		public static void AssertSingle(
			RegressionEnvironment env,
			string deployedStmtNameConsume,
			string deployedStmtNameProvide,
			EPObjectType objectType,
			string objectName)
		{
			env.AssertThat(
				() => {
					var deploymentIdConsume = env.DeploymentId(deployedStmtNameConsume);
					var deploymentIdProvide = env.DeploymentId(deployedStmtNameProvide);
					var consumed = env.Runtime.DeploymentService.GetDeploymentDependenciesConsumed(deploymentIdConsume);
					CollectionAssert.AreEquivalent(
						new[] {
							new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, objectType, objectName)
						},
						consumed.Dependencies.ToArray());
					var provided = env.Runtime.DeploymentService.GetDeploymentDependenciesProvided(deploymentIdProvide);
					CollectionAssert.AreEquivalent(
						new[] {
							new EPDeploymentDependencyProvided.Item(
								objectType,
								objectName,
								Collections.SingletonSet(deploymentIdConsume))
						},
						provided.Dependencies.ToArray());
				});
		}
	}
} // end of namespace
