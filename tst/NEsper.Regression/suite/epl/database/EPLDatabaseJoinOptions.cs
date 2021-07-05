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

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinOptions
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNoMetaLexAnalysis(execs);
            WithNoMetaLexAnalysisGroup(execs);
            WithPlaceholderWhere(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPlaceholderWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabasePlaceholderWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithNoMetaLexAnalysisGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseNoMetaLexAnalysisGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithNoMetaLexAnalysis(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseNoMetaLexAnalysis());
            return execs;
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string sql)
        {
            var stmtText = "@Name('s0') select mydouble from " +
                           " sql:MyDBPlain ['" +
                           sql +
                           "'] as S0," +
                           "SupportBean#length(100) as S1";
            env.CompileDeploy(stmtText).AddListener("s0");

            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mydouble"));

            SendSupportBeanEvent(env, 10);
            Assert.AreEqual(1.2, env.Listener("s0").AssertOneGetNewAndReset().Get("mydouble"));

            SendSupportBeanEvent(env, 80);
            Assert.AreEqual(8.2, env.Listener("s0").AssertOneGetNewAndReset().Get("mydouble"));

            env.UndeployAll();
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        internal class EPLDatabaseNoMetaLexAnalysis : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql = "select mydouble from mytesttable where ${IntPrimitive} = myint";
                RunAssertion(env, sql);
            }
        }

        internal class EPLDatabaseNoMetaLexAnalysisGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql =
                    "select mydouble, sum(myint) from mytesttable where ${IntPrimitive} = myint group by mydouble";
                RunAssertion(env, sql);
            }
        }

        internal class EPLDatabasePlaceholderWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql = "select mydouble from mytesttable ${$ESPER-SAMPLE-WHERE} where ${IntPrimitive} = myint";
                RunAssertion(env, sql);
            }
        }
    }
} // end of namespace