///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public class ClientStageDeploy
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withd(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageDeployInvalidUndeployWhileStaged());
            return execs;
        }

        private class ClientStageDeployInvalidUndeployWhileStaged : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from SupportBean");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentId);

                try {
                    env.Deployment.Undeploy(deploymentId);
                    Assert.Fail();
                }
                catch (EPUndeployPreconditionException ex) {
                    Assert.AreEqual(
                        ex.Message,
                        "A precondition is not satisfied: Deployment id '" +
                        deploymentId +
                        "' is staged and cannot be undeployed");
                }

                UnstageIt(env, "ST", deploymentId);
                env.Undeploy(deploymentId);

                env.UndeployAll();
            }
        }
    }
} // end of namespace