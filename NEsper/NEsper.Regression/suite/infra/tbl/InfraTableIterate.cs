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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableIterate : RegressionExecution
    {
        private const string METHOD_NAME = "method:SupportStaticMethodLib.FetchTwoRows3Cols()";

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create table MyTable(pKey0 string primary key, pkey1 int primary key, c0 long)", path);
            env.CompileDeploy(
                "insert into MyTable select TheString as pKey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean",
                path);

            SendSupportBean(env, "E1", 10, 100);
            SendSupportBean(env, "E2", 20, 200);

            RunAssertion(env, path, true);
            RunAssertion(env, path, false);

            env.UndeployAll();
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            RunUnaggregatedUngroupedSelectStar(env, path, useTable);
            RunFullyAggregatedAndUngrouped(env, path, useTable);
            RunAggregatedAndUngrouped(env, path, useTable);
            RunFullyAggregatedAndGrouped(env, path, useTable);
            RunAggregatedAndGrouped(env, path, useTable);
            RunAggregatedAndGroupedRollup(env, path, useTable);
        }

        private static void RunUnaggregatedUngroupedSelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select * from " + (useTable ? "MyTable" : METHOD_NAME);
            env.CompileDeploy(epl, path);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                new [] { "pKey0","pkey1","c0" },
                new[] {new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 200L}});
            env.UndeployModuleContaining("s0");
        }

        private static void RunFullyAggregatedAndUngrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                var @event = env.GetEnumerator("s0").Advance();
                Assert.AreEqual(2L, @event.Get("thecnt"));
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndUngrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pKey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new [] { "pKey0","thecnt" },
                    new[] {new object[] {"E1", 2L}, new object[] {"E2", 2L}});
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunFullyAggregatedAndGrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pKey0, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by pKey0";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new [] { "pKey0","thecnt" },
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 1L}});
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndGrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pKey0, pkey1, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by pKey0";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new [] { "pKey0","pkey1","thecnt" },
                    new[] {new object[] {"E1", 10, 1L}, new object[] {"E2", 20, 1L}});
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndGroupedRollup(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pKey0, pkey1, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by rollup (pKey0, pkey1)";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new [] { "pKey0","pkey1","thecnt" },
                    new[] {
                        new object[] {"E1", 10, 1L},
                        new object[] {"E2", 20, 1L},
                        new object[] {"E1", null, 1L},
                        new object[] {"E2", null, 1L},
                        new object[] {null, null, 2L}
                    });
            }

            env.UndeployAll();
        }

        private SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
            return bean;
        }
    }
} // end of namespace