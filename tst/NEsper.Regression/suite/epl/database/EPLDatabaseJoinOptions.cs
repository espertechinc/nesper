///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinOptions
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithNoMetaLexAnalysis(execs);
            WithNoMetaLexAnalysisGroup(execs);
            With(PlaceholderWhere)(execs);
#endif
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
            var stmtText = "@name('s0') select mydouble from " +
                           " sql:MyDBPlain ['" +
                           sql +
                           "'] as S0," +
                           "SupportBean#length(100) as S1";
            env.CompileDeploy(stmtText).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => ClassicAssert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mydouble")));

            SendSupportBeanEvent(env, 10);
            env.AssertEqualsNew("s0", "mydouble", 1.2);

            SendSupportBeanEvent(env, 80);
            env.AssertEqualsNew("s0", "mydouble", 8.2);

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