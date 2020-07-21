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

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogClausePresence : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionMeasurePresence(env, 0, "B.size()", 1);
            RunAssertionMeasurePresence(env, 0, "100+B.size()", 101);
            RunAssertionMeasurePresence(env, 1000000, "B.anyOf(v->TheString='E2')", true);

            RunAssertionDefineNotPresent(env, true);
            RunAssertionDefineNotPresent(env, false);
        }

        private void RunAssertionDefineNotPresent(
            RegressionEnvironment env,
            bool soda)
        {
            var epl = "@name('s0') select * from SupportBean " +
                      "match_recognize (" +
                      " measures A as a, B as b" +
                      " pattern (A B)" +
                      ")";
            env.CompileDeploy(soda, epl).AddListener("s0");

            var fields = new [] { "a","b" };
            var beans = new SupportBean[4];
            for (var i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }

            env.SendEventBean(beans[0]);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(beans[1]);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {beans[0], beans[1]});

            env.SendEventBean(beans[2]);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(beans[3]);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {beans[2], beans[3]});

            env.UndeployAll();
        }

        private void RunAssertionMeasurePresence(
            RegressionEnvironment env,
            long baseTime,
            string select,
            object value)
        {
            env.AdvanceTime(baseTime);
            var epl = "@name('s0') select * from SupportBean  " +
                      "match_recognize (" +
                      "    measures A as a, A.TheString as Id, " +
                      select +
                      " as val " +
                      "    pattern (A B*) " +
                      "    interval 1 minute " +
                      "    define " +
                      "        A as (A.IntPrimitive=1)," +
                      "        B as (B.IntPrimitive=2))";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 2));

            env.AdvanceTimeSpan(baseTime + 60 * 1000 * 2);
            Assert.AreEqual(value, env.Listener("s0").NewDataListFlattened[0].Get("val"));

            env.UndeployAll();
        }
    }
} // end of namespace