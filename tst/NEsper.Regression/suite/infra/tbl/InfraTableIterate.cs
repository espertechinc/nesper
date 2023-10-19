///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableIterate : RegressionExecution
    {
        private const string METHOD_NAME = "method:SupportStaticMethodLib.fetchTwoRows3Cols()";

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public create table MyTable(pkey0 string primary key, pkey1 int primary key, c0 long)",
                path);
            env.CompileDeploy(
                "insert into MyTable select theString as pkey0, intPrimitive as pkey1, longPrimitive as c0 from SupportBean",
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
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                "pkey0,pkey1,c0".SplitCsv(),
                new object[][] { new object[] { "E1", 10, 100L }, new object[] { "E2", 20, 200L } });
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
                env.AssertIterator(
                    "s0",
                    iterator => {
                        var @event = iterator.Advance();
                        Assert.AreEqual(2L, @event.Get("thecnt"));
                    });
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndUngrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pkey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    "pkey0,thecnt".SplitCsv(),
                    new object[][] { new object[] { "E1", 2L }, new object[] { "E2", 2L } });
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunFullyAggregatedAndGrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pkey0, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by pkey0";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    "pkey0,thecnt".SplitCsv(),
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 1L } });
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndGrouped(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pkey0, pkey1, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by pkey0";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    "pkey0,pkey1,thecnt".SplitCsv(),
                    new object[][] { new object[] { "E1", 10, 1L }, new object[] { "E2", 20, 1L } });
            }

            env.UndeployModuleContaining("s0");
        }

        private static void RunAggregatedAndGroupedRollup(
            RegressionEnvironment env,
            RegressionPath path,
            bool useTable)
        {
            var epl = "@name('s0') select pkey0, pkey1, count(*) as thecnt from " +
                      (useTable ? "MyTable" : METHOD_NAME) +
                      " group by rollup (pkey0, pkey1)";
            env.CompileDeploy(epl, path);
            for (var i = 0; i < 2; i++) {
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    "pkey0,pkey1,thecnt".SplitCsv(),
                    new object[][] {
                        new object[] { "E1", 10, 1L },
                        new object[] { "E2", 20, 1L },
                        new object[] { "E1", null, 1L },
                        new object[] { "E2", null, 1L },
                        new object[] { null, null, 2L },
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