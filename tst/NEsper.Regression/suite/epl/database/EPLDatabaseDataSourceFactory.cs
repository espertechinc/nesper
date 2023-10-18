///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseDataSourceFactory : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            string[] fields = {"myint"};
            var stmtText = "@name('s0') select istream myint from " +
                           " sql:MyDBWithPooledWithLRU100 ['select myint from mytesttable where ${IntPrimitive} = mytesttable.myBigint'] as S0," +
                           "SupportBean as S1";
            env.CompileDeploy(stmtText).AddListener("s0");

            SendSupportBeanEvent(env, 10);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {100});

            SendSupportBeanEvent(env, 6);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {60});

            var startTime = PerformanceObserver.MilliTime;
            // Send 100 events which all fireStatementStopped a join
            for (var i = 0; i < 100; i++) {
                SendSupportBeanEvent(env, 10);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {100});
            }

            var endTime = PerformanceObserver.MilliTime;
            log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 5000);

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
    }
} // end of namespace