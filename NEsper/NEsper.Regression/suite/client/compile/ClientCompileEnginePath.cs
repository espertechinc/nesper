///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileEnginePath
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientCompileEnginePathObjectTypes());
            return execs;
        }

        private static RegressionEnvironment CompileDeployWEnginePath(
            RegressionEnvironment env,
            string epl)
        {
            EPCompiled compiled;
            try {
                compiled = CompileWEnginePathAndEmptyConfig(env, epl);
            }
            catch (EPCompileException ex) {
                throw new EPException(ex);
            }

            env.Deploy(compiled);
            return env;
        }

        private static EPCompiled CompileWEnginePathAndEmptyConfig(
            RegressionEnvironment env,
            string epl)
        {
            var args = new CompilerArguments(new Configuration());
            args.Path.Add(env.Runtime.RuntimePath);
            return EPCompilerProvider.Compiler.Compile(epl, args);
        }

        public class ClientCompileEnginePathObjectTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var deployed = "create variable int myvariable = 10;\n" +
                               "create schema MySchema();\n" +
                               "create expression myExpr { 'abc' };\n" +
                               "create window MyWindow#keepall as SupportBean_S0;\n" +
                               "create table MyTable(y string);\n" +
                               "create context MyContext start SupportBean_S0 end SupportBean_S1;\n" +
                               "create expression myScript() [ 2 ]";
                env.CompileDeploy(deployed, new RegressionPath());

                var epl =
                    "@Name('s0') select myvariable as c0, myExpr() as c1, myScript() as c2, preconfigured_variable as c3 from SupportBean;\n" +
                    "select * from MySchema;" +
                    "on SupportBean_S1 delete from MyWindow;\n" +
                    "on SupportBean_S1 delete from MyTable;\n" +
                    "context MyContext select * from SupportBean;\n";
                CompileDeployWEnginePath(env, epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1", "c2", "c3" },
                    new object[] {10, "abc", 2, 5});

                env.UndeployAll();
            }
        }
    }
} // end of namespace