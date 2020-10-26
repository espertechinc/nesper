///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
	public class ClientStageMgmt
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientStageMgmtInvalidStageDestroyWhileNotEmpty());
			return execs;
		}

		private class ClientStageMgmtInvalidStageDestroyWhileNotEmpty : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@Name('s0') select * from SupportBean");
				string deploymentId = env.DeploymentId("s0");
				env.StageService.GetStage("ST");
				StageIt(env, "ST", deploymentId);

				try {
					env.StageService.GetExistingStage("ST").Destroy();
					Assert.Fail();
				}
				catch (EPException ex) {
					Assert.AreEqual(ex.Message, "Failed to destroy stage 'ST': The stage has existing deployments");
				}

				UnstageIt(env, "ST", deploymentId);

				env.StageService.GetExistingStage("ST").Destroy();
				env.UndeployAll();
			}
		}
	}
} // end of namespace
