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
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableOnUpdate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = new [] { "keyOne","keyTwo","p0" };
            var path = new RegressionPath();

            env.CompileDeploy(
                "create table varagg as (" +
                "keyOne string primary key, keyTwo int primary key, p0 long)",
                path);
            env.CompileDeploy(
                "on SupportBean merge varagg where TheString = keyOne and " +
                "IntPrimitive = keyTwo when not matched then insert select TheString as keyOne, IntPrimitive as keyTwo, 1 as p0",
                path);
            env.CompileDeploy("@Name('s0') select varagg[P00, Id].p0 as value from SupportBean_S0", path)
                .AddListener("s0");
            env.CompileDeploy(
                    "@Name('Update') on SupportTwoKeyEvent update varagg set p0 = newValue " +
                    "where k1 = keyOne and k2 = keyTwo",
                    path)
                .AddListener("update");

            object[][] expectedType = {
                new object[] {"keyOne", typeof(string)},
                new object[] {"keyTwo", typeof(int?)},
                new object[] {"P0", typeof(long?)}
            };
            var updateStmtEventType = env.Statement("update").EventType;
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                updateStmtEventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            env.SendEventBean(new SupportBean("G1", 10));
            AssertValues(
                env,
                new[] {
                    new object[] {"G1", 10}
                },
                new long?[] {1L});

            env.Milestone(0);

            env.SendEventBean(new SupportTwoKeyEvent("G1", 10, 2));
            AssertValues(
                env,
                new[] {
                    new object[] {"G1", 10}
                },
                new long?[] {2L});
            EPAssertionUtil.AssertProps(
                env.Listener("update").LastNewData[0],
                fields,
                new object[] {"G1", 10, 2L});
            EPAssertionUtil.AssertProps(
                env.Listener("update").GetAndResetLastOldData()[0],
                fields,
                new object[] {"G1", 10, 1L});

            // try property method invocation
            env.CompileDeploy("create table MyTableSuppBean as (sb SupportBean)", path);
            env.CompileDeploy("on SupportBean_S0 update MyTableSuppBean sb set sb.setLongPrimitive(10)", path);
            env.UndeployAll();
        }

        private static void AssertValues(
            RegressionEnvironment env,
            object[][] keys,
            long?[] values)
        {
            Assert.AreEqual(keys.Length, values.Length);
            for (var i = 0; i < keys.Length; i++) {
                env.SendEventBean(new SupportBean_S0(keys[i][1].AsInt(), (string) keys[i][0]));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"));
            }
        }
    }
} // end of namespace