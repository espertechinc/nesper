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
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployUserObject
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientDeployUserObjectValues());
            execs.Add(new ClientDeployUserObjectResolveContext());
            return execs;
        }

        private static void AssertDeploy(
            RegressionEnvironment env,
            EPCompiled compiled,
            AtomicLong milestone,
            object userObject)
        {
            var options = new DeploymentOptions();
            options.StatementUserObjectRuntime = new ProxyStatementUserObjectRuntimeOption {
                ProcGetUserObject = _ => { return userObject; }
            };

            env.Deployment.Deploy(compiled, options);
            env.UndeployAll();
        }

        internal class ClientDeployUserObjectResolveContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyUserObjectRuntimeOption.Contexts.Clear();
                var epl = "@name('s0') select * from SupportBean";
                var compiled = env.Compile(epl);
                var options = new DeploymentOptions();
                options.StatementUserObjectRuntime = new MyUserObjectRuntimeOption();

                env.Deployment.Deploy(compiled, options);

                var ctx = MyUserObjectRuntimeOption.Contexts[0];
                Assert.AreEqual("s0", ctx.StatementName);
                Assert.AreEqual(env.DeploymentId("s0"), ctx.DeploymentId);
                Assert.AreSame(env.Statement("s0").Annotations, ctx.Annotations);
                Assert.AreEqual(epl, ctx.Epl);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientDeployUserObjectValues : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("@name('s0') select * from SupportBean");

                var milestone = new AtomicLong();
                AssertDeploy(env, compiled, milestone, null);
                AssertDeploy(env, compiled, milestone, "ABC");
                AssertDeploy(env, compiled, milestone, new MyUserObject("hello"));
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        [Serializable]
        internal class MyUserObject
        {
            public MyUserObject(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (o == null || GetType() != o.GetType()) {
                    return false;
                }

                var that = (MyUserObject)o;

                return Id.Equals(that.Id);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        internal class MyUserObjectRuntimeOption : StatementUserObjectRuntimeOption
        {
            public static IList<StatementUserObjectRuntimeContext> Contexts { get; } =
                new List<StatementUserObjectRuntimeContext>();

            public object GetUserObject(StatementUserObjectRuntimeContext env)
            {
                Contexts.Add(env);
                return true;
            }
        }
    }
} // end of namespace