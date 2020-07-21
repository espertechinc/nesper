///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.stage;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
	public class ClientStageEPStage
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientStageEPStageDestroy());
			execs.Add(new ClientStageEPStageStageInvalid());
			return execs;
		}

		private class ClientStageEPStageStageInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				EPStage stageA = env.StageService.GetStage("ST");

				TryIllegalArgument(() => stageA.Stage(null));
				TryOp(() => stageA.Stage(EmptyList<string>.Instance));
				TryIllegalArgument(() => stageA.Stage(Arrays.AsList(new string[] {null})));
				TryIllegalArgument(() => stageA.Stage(Arrays.AsList(new string[] {"a", null})));

				TryIllegalArgument(() => stageA.Unstage(null));
				TryOp(() => stageA.Unstage(EmptyList<string>.Instance));
				TryIllegalArgument(() => stageA.Unstage(Arrays.AsList(new string[] {null})));
				TryIllegalArgument(() => stageA.Unstage(Arrays.AsList(new string[] {"a", null})));

				TryDeploymentNotFound(() => stageA.Stage(Arrays.AsList(new string[] {"x"})), "Deployment 'x' was not found");
				TryDeploymentNotFound(() => stageA.Unstage(Arrays.AsList(new string[] {"x"})), "Deployment 'x' was not found");
			}
		}

		private class ClientStageEPStageDestroy : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				EPStage stageA = env.StageService.GetStage("ST");
				env.CompileDeploy("@name('s0') select * from SupportBean");
				string deploymentId = env.DeploymentId("s0");

				StageIt(env, "ST", deploymentId);
				try {
					stageA.Destroy();
					Assert.Fail();
				}
				catch (EPException ex) {
					Assert.AreEqual("Failed to destroy stage 'ST': The stage has existing deployments", ex.Message);
				}

				UnstageIt(env, "ST", deploymentId);

				stageA.Destroy();
				Assert.AreEqual("ST", stageA.URI);

				TryInvalidDestroyed(() => Noop(stageA.EventService));
				TryInvalidDestroyed(() => Noop(stageA.DeploymentService));

				TryInvalidDestroyed(
					() => {
						try {
							stageA.Stage(Collections.SingletonList(deploymentId));
						}
						catch (EPStageException ex) {
							throw new EPRuntimeException(ex);
						}
					});

				TryInvalidDestroyed(
					() => {
						try {
							stageA.Unstage(Collections.SingletonList(deploymentId));
						}
						catch (EPStageException ex) {
							throw new EPRuntimeException(ex);
						}
					});

				env.UndeployAll();
			}
		}

		private static void Noop(object value)
		{
		}

		private static void TryInvalidDestroyed(Runnable r)
		{
			Assert.Throws<EPStageDestroyedException>(r.Invoke);
		}

		private static void TryIllegalArgument(Runnable r)
		{
			Assert.Throws<ArgumentException>(r.Invoke);
		}

		private static void TryDeploymentNotFound(
			Runnable r,
			string expected)
		{
			var ex = Assert.Throws<EPStageException>(r.Invoke);
			AssertMessage(ex.Message, expected);
		}

		private static void TryOp(Runnable r)
		{
			Assert.DoesNotThrow(r.Invoke);
		}
	}
} // end of namespace
