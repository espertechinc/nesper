///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseNoJoinIteratePerf : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create variable boolean queryvar_bool", path);
            env.CompileDeploy("create variable int lower", path);
            env.CompileDeploy("create variable int upper", path);
            env.CompileDeploy(
                "on SupportBean set queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed",
                path);

            var stmtText =
                "@Name('s0') select * from sql:MyDBWithLRU100000 ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
            string[] fields = {"mybigint", "mybool"};
            env.CompileDeploy(stmtText, path);
            SendSupportBeanEvent(env, true, 20, 60);

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < 10000; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {4L, true}});
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Assert.That(delta, Is.LessThan(1000), "delta=" + delta);

            env.UndeployAll();
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool boolPrimitive,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }
    }
} // end of namespace