///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesInlinedClass
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithLocal(execs);
            WithGlobal(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGlobal(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariablesInlinedClassGlobal());
            return execs;
        }

        public static IList<RegressionExecution> WithLocal(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariablesInlinedClassLocal());
            return execs;
        }

        private class EPLVariablesInlinedClassGlobal : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplClass = "@public @name('clazz') create inlined_class \"\"\"\n" +
                               "[System.Serializable]\n" +
                               "public class MyStatefulValue {\n" +
                               "    private string _value = \"X\";\n" +
                               "    public string Value {\n" +
                               "        get => _value;\n" +
                               "        set => _value = value;\n" +
                               "    }\n" +
                               "}\n" +
                               "\"\"\"\n";
                env.CompileDeploy(eplClass, path);

                var epl = "@public create variable MyStatefulValue msf = new MyStatefulValue();\n" +
                          "@name('s0') select msf.Value as c0 from SupportBean;\n" +
                          "on SupportBean_S0 set msf.Value = P00;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendAssert(env, "X");
                env.SendEventBean(new SupportBean_S0(1, "A"));
                SendAssert(env, "A");

                env.Milestone(0);

                SendAssert(env, "A");
                env.SendEventBean(new SupportBean_S0(2, "B"));
                SendAssert(env, "B");

                SupportDeploymentDependencies.AssertSingle(
                    env,
                    "s0",
                    "clazz",
                    EPObjectType.CLASSPROVIDED,
                    "MyStatefulValue");

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string expected)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { expected });
            }
        }

        private class EPLVariablesInlinedClassLocal : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "inlined_class \"\"\"\n" +
                          "[System.Serializable]\n" +
                          "public class MyStatefulPair {\n" +
                          "    public MyStatefulPair(int a, int b) {\n" +
                          "        this.A = a;\n" +
                          "        this.B = b;\n" +
                          "    }\n" +
                          "    public int A { get; set; }\n" +
                          "    public int B { get; set; }\n" +
                          "}\n" +
                          "\"\"\"\n" +
                          "create variable MyStatefulPair msf = new MyStatefulPair(2, 3);\n" +
                          "@name('s0') select msf.A as c0, msf.B as c1 from SupportBean;\n" +
                          "on SupportBeanNumeric set msf.A = IntOne, msf.B = IntTwo;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, 2, 3);

                env.Milestone(0);

                SendAssert(env, 2, 3);
                env.SendEventBean(new SupportBeanNumeric(10, 20));
                SendAssert(env, 10, 20);

                env.Milestone(1);

                SendAssert(env, 10, 20);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                int expectedA,
                int expectedB)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { expectedA, expectedB });
            }
        }
    }
} // end of namespace