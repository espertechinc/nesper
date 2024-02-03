///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesDestroy
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithManageDependency(execs);
            WithDestroyReCreateChangeType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDestroyReCreateChangeType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableDestroyReCreateChangeType());
            return execs;
        }

        public static IList<RegressionExecution> WithManageDependency(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableManageDependency());
            return execs;
        }

        private static void AssertNotFound(
            RegressionEnvironment env,
            string deploymentId,
            string var)
        {
            try {
                env.Runtime.VariableService.GetVariableValue(deploymentId, var);
                Assert.Fail();
            }
            catch (VariableNotFoundException) {
                // expected
            }
        }

        private static void AssertCannotUndeploy(
            RegressionEnvironment env,
            string statementNames)
        {
            var names = statementNames.SplitCsv();
            foreach (var name in names) {
                try {
                    env.Deployment.Undeploy(env.DeploymentId(name));
                    Assert.Fail();
                }
                catch (EPUndeployPreconditionException) {
                    // expected
                }
                catch (EPUndeployException ex) {
                    throw new EPException(ex);
                }
            }
        }

        internal class EPLVariableDestroyReCreateChangeType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('ABC') create variable long varDRR = 2";
                env.CompileDeploy(text);

                ClassicAssert.AreEqual(2L, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("ABC"), "varDRR"));

                var deploymentIdABC = env.DeploymentId("ABC");
                env.UndeployModuleContaining("ABC");

                AssertNotFound(env, deploymentIdABC, "varDRR");

                text = "@name('CDE') create variable string varDRR = 'a'";
                env.CompileDeploy(text);

                ClassicAssert.AreEqual("a", env.Runtime.VariableService.GetVariableValue(env.DeploymentId("CDE"), "varDRR"));

                var deploymentIdCDE = env.DeploymentId("CDE");
                env.UndeployModuleContaining("CDE");
                AssertNotFound(env, deploymentIdCDE, "varDRR");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class EPLVariableManageDependency : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // single variable
                env.CompileDeploy("@name('S0') @public create variable boolean var2vmd = true", path);
                env.CompileDeploy("@name('S1') select * from SupportBean(var2vmd)", path);
                ClassicAssert.AreEqual(true, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("S0"), "var2vmd"));

                try {
                    env.Deployment.Undeploy(env.DeploymentId("S0"));
                    Assert.Fail();
                }
                catch (EPUndeployException) {
                    // expected
                }

                env.UndeployModuleContaining("S1");
                ClassicAssert.AreEqual(true, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("S0"), "var2vmd"));

                var deploymentIdS0 = env.DeploymentId("S0");
                env.UndeployModuleContaining("S0");
                AssertNotFound(env, deploymentIdS0, "var2vmd");

                // multiple variable
                path.Clear();
                env.CompileDeploy("@name('T0') @public create variable boolean v1 = true", path);
                env.CompileDeploy("@name('T1') @public create variable long v2 = 1", path);
                env.CompileDeploy("@name('T2') @public create variable string v3 = 'a'", path);
                env.CompileDeploy("@name('TX') select * from SupportBean(v1, v2=1, v3='a')", path);
                env.CompileDeploy("@name('TY') select * from SupportBean(v2=2)", path);
                env.CompileDeploy("@name('TZ') select * from SupportBean(v3='A', v1)", path);

                AssertCannotUndeploy(env, "T0,T1,T2");
                env.UndeployModuleContaining("TX");
                AssertCannotUndeploy(env, "T0,T1,T2");

                env.UndeployModuleContaining("TY");
                env.UndeployModuleContaining("T1");
                AssertCannotUndeploy(env, "T0,T2");

                env.UndeployModuleContaining("TZ");
                env.UndeployModuleContaining("T0");
                env.UndeployModuleContaining("T2");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }
    }
} // end of namespace