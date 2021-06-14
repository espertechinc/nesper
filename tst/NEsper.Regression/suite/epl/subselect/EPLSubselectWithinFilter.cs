///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectWithinFilter
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithExistsWhereAndUDF(execs);
            WithRowWhereAndUDF(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRowWhereAndUDF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWithinFilterRowWhereAndUDF());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsWhereAndUDF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWithinFilterExistsWhereAndUDF());
            return execs;
        }

        private class EPLSubselectWithinFilterExistsWhereAndUDF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') " +
                             "inlined_class \"\"\"\n" +
                             "  public class MyUtil { public static bool CompareIt(string one, string two) { return one.Equals(two); } }\n" +
                             "\"\"\" \n" +
                             "select * from SupportBean_S0(exists (select * from SupportBean_S1#keepall where MyUtil.CompareIt(s0.P00,P10))) as s0;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendS0Assert(env, 1, "a", false);
                env.SendEventBean(new SupportBean_S1(10, "x"));
                SendS0Assert(env, 2, "a", false);
                env.SendEventBean(new SupportBean_S1(11, "a"));
                SendS0Assert(env, 3, "a", true);
                SendS0Assert(env, 4, "x", true);

                env.UndeployAll();
            }
        }

        private class EPLSubselectWithinFilterRowWhereAndUDF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') " +
                             "inlined_class \"\"\"\n" +
                             "  public class MyUtil { public static bool CompareIt(string one, string two) { return one.Equals(two); } }\n" +
                             "\"\"\" \n" +
                             "select * from SupportBean_S0('abc' = (select P11 from SupportBean_S1#keepall where MyUtil.CompareIt(s0.P00,P10))) as s0;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendS0Assert(env, 1, "a", false);
                env.SendEventBean(new SupportBean_S1(10, "x", "abc"));
                SendS0Assert(env, 1, "a", false);
                env.SendEventBean(new SupportBean_S1(11, "a", "abc"));
                SendS0Assert(env, 3, "a", true);
                SendS0Assert(env, 4, "x", true);
                SendS0Assert(env, 5, "y", false);

                env.UndeployAll();
            }
        }

        private static void SendS0Assert(
            RegressionEnvironment env,
            int id,
            string p00,
            bool expected)
        {
            env.SendEventBean(new SupportBean_S0(id, p00));
            Assert.AreEqual(expected, env.Listener("s0").IsInvokedAndReset());
        }
    }
} // end of namespace