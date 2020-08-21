///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitChangeSetOpt : RegressionExecution
    {
        private readonly bool enableOutputLimitOpt;

        public ResultSetOutputLimitChangeSetOpt(bool enableOutputLimitOpt)
        {
            this.enableOutputLimitOpt = enableOutputLimitOpt;
        }

        public void Run(RegressionEnvironment env)
        {
            var currentTime = new AtomicLong(0);
            SendTime(env, currentTime.Get());

            // unaggregated and ungrouped
            //
            TryAssertion(env, currentTime, SupportOutputLimitOpt.DEFAULT, 0, "IntPrimitive", null, null, "last", null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "IntPrimitive",
                null,
                null,
                "last",
                "order by IntPrimitive");

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "IntPrimitive",
                null,
                null,
                "all",
                null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.ENABLED, 0, "IntPrimitive", null, null, "all", null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.DISABLED, 5, "IntPrimitive", null, null, "all", null);

            TryAssertion(env, currentTime, SupportOutputLimitOpt.DEFAULT, 0, "IntPrimitive", null, null, "first", null);

            // fully-aggregated and ungrouped
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "count(*)",
                null,
                null,
                "last",
                null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.ENABLED, 0, "count(*)", null, null, "last", null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.DISABLED, 5, "count(*)", null, null, "last", null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "count(*)",
                null,
                null,
                "all",
                null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.ENABLED, 0, "count(*)", null, null, "all", null);
            TryAssertion(env, currentTime, SupportOutputLimitOpt.DISABLED, 5, "count(*)", null, null, "all", null);

            TryAssertion(env, currentTime, SupportOutputLimitOpt.DEFAULT, 0, "count(*)", null, null, "first", null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "count(*)",
                null,
                "having count(*) > 0",
                "first",
                null);

            // aggregated and ungrouped
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, count(*)",
                null,
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, count(*)",
                null,
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, count(*)",
                null,
                null,
                "last",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, count(*)",
                null,
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, count(*)",
                null,
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, count(*)",
                null,
                null,
                "all",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "TheString, count(*)",
                null,
                null,
                "first",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "TheString, count(*)",
                null,
                "having count(*) > 0",
                "first",
                null);

            // fully-aggregated and grouped
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, count(*)",
                "group by TheString",
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, count(*)",
                "group by TheString",
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, count(*)",
                "group by TheString",
                null,
                "last",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, count(*)",
                "group by TheString",
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, count(*)",
                "group by TheString",
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, count(*)",
                "group by TheString",
                null,
                "all",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "TheString, count(*)",
                "group by TheString",
                null,
                "first",
                null);

            // aggregated and grouped
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "last",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "last",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                enableOutputLimitOpt ? 0 : 5,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.ENABLED,
                0,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "all",
                null);
            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DISABLED,
                5,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "all",
                null);

            TryAssertion(
                env,
                currentTime,
                SupportOutputLimitOpt.DEFAULT,
                0,
                "TheString, IntPrimitive, count(*)",
                "group by TheString",
                null,
                "first",
                null);

            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                SupportOutputLimitOpt.ENABLED.GetHint() +
                " select sum(IntPrimitive) " +
                "from SupportBean output last every 4 events order by TheString",
                "The ENABLE_OUTPUTLIMIT_OPT hint is not supported with order-by");
        }

        private void TryAssertion(
            RegressionEnvironment env,
            AtomicLong currentTime,
            SupportOutputLimitOpt hint,
            int expected,
            string selectClause,
            string groupBy,
            string having,
            string outputKeyword,
            string orderBy)
        {
            var epl = hint.GetHint() +
                      "@Name('s0') select irstream " +
                      selectClause +
                      " " +
                      "from SupportBean#length(2) " +
                      (groupBy == null ? "" : groupBy + " ") +
                      (having == null ? "" : having + " ") +
                      "output " +
                      outputKeyword +
                      " every 1 seconds " +
                      (orderBy == null ? "" : orderBy);
            env.CompileDeploy(epl).AddListener("s0");

            for (var i = 0; i < 5; i++) {
                env.SendEventBean(new SupportBean("E" + i, i));
            }

            AssertResourcesOutputRate(env, expected);

            SendTime(env, currentTime.IncrementAndGet(1000));

            AssertResourcesOutputRate(env, 0);
            env.UndeployAll();
        }

        private void AssertResourcesOutputRate(
            RegressionEnvironment env,
            int numExpectedChangeset)
        {
            var spi = (EPStatementSPI) env.Statement("s0");
            var resources = spi.StatementContext.StatementCPCacheService.StatementResourceService
                .ResourcesUnpartitioned;
            var outputProcessView = (OutputProcessView) resources.FinalView;
            try {
                Assert.AreEqual(
                    numExpectedChangeset,
                    outputProcessView.NumChangesetRows,
                    "enableOutputLimitOpt=" + enableOutputLimitOpt);
            }
            catch (UnsupportedOperationException) {
                // allowed
            }
        }

        private static void SendTime(
            RegressionEnvironment env,
            long currentTime)
        {
            env.AdvanceTime(currentTime);
        }
    }
} // end of namespace