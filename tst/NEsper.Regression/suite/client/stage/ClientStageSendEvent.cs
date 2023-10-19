///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil; // stageIt, unstageIt
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public class ClientStageSendEvent
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFilter(execs);
            WithNamedWindow(execs);
            WithPatternWFollowedBy(execs);
            WithSubquery(execs);
            WithContextCategory(execs);
            WithContextKeyed(execs);
            WithContextKeyedWithInitiated(execs);
            WithContextKeyedWithTerminated(execs);
            WithContextHash(execs);
            WithContextStartNoEnd(execs);
            WithContextStartWithEndFilter(execs);
            WithContextStartWithEndPattern(execs);
            WithContextInitiatedNoTerminated(execs);
            WithContextNestedCategoryOverKeyed(execs);
            WithContextNestedPartitionedOverStart(execs);
            WithContextNestedInitiatedOverKeyed(execs);
            WithContextNestedHashOverHash(execs);
            WithPatternWAnd(execs);
            WithPatternWEvery(execs);
            WithPatternWEveryDistinct(execs);
            WithPatternWOr(execs);
            WithPatternWGuard(execs);
            WithPatternWNot(execs);
            WithPatternWUntil(execs);
            WithUpdateIStream(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateIStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventUpdateIStream());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWUntil(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWUntil());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWNot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWNot());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWGuard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWGuard());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWOr());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWEveryDistinct(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWEveryDistinct());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWEvery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWEvery());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWAnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWAnd());
            return execs;
        }

        public static IList<RegressionExecution> WithContextNestedHashOverHash(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextNestedHashOverHash());
            return execs;
        }

        public static IList<RegressionExecution> WithContextNestedInitiatedOverKeyed(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextNestedInitiatedOverKeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithContextNestedPartitionedOverStart(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextNestedPartitionedOverStart());
            return execs;
        }

        public static IList<RegressionExecution> WithContextNestedCategoryOverKeyed(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextNestedCategoryOverKeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithContextInitiatedNoTerminated(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextInitiatedNoTerminated());
            return execs;
        }

        public static IList<RegressionExecution> WithContextStartWithEndPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextStartWithEndPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithContextStartWithEndFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextStartWithEndFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithContextStartNoEnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextStartNoEnd());
            return execs;
        }

        public static IList<RegressionExecution> WithContextHash(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextHash());
            return execs;
        }

        public static IList<RegressionExecution> WithContextKeyedWithTerminated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextKeyedWithTerminated());
            return execs;
        }

        public static IList<RegressionExecution> WithContextKeyedWithInitiated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextKeyedWithInitiated());
            return execs;
        }

        public static IList<RegressionExecution> WithContextKeyed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextKeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithContextCategory(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventContextCategory());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWFollowedBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventPatternWFollowedBy());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageSendEventFilter());
            return execs;
        }

        private class ClientStageSendEventUpdateIStream : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "update istream SupportBean set intPrimitive = -1;\n" +
                          "@name('s0') select * from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEvent(env, null, "E1", 10);
                env.AssertEqualsNew("s0", "intPrimitive", -1);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEvent(env, "ST", "E2", 20);
                Assert.AreEqual(-1, env.ListenerStage("ST", "s0").AssertOneGetNewAndReset().Get("intPrimitive"));

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(1);

                SendEvent(env, null, "E3", 30);
                env.AssertEqualsNew("s0", "intPrimitive", -1);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventSubquery : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select (select sum(id) from SupportBean_S0) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventS0(env, null, 10);
                SendEventAssertSum(env, null, "E1", 0, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "E2", 0, 10);
                SendEventS0(env, "ST", 20);
                SendEventS0(env, null, 21);
                SendEventAssertSum(env, "ST", "E3", 0, 10 + 20);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(1);

                SendEventAssertSum(env, null, "E4", 0, 10 + 20);
                SendEventS0(env, "ST", 30);
                SendEventS0(env, null, 31);
                SendEventAssertSum(env, null, "E5", 0, 10 + 20 + 31);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextStartWithEndPattern : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext start SupportBean_S0 end pattern [SupportBean_S1(id=100) -> SupportBean_S1(id=200)];\n" +
                    "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventS0(env, null, 10);
                SendEventS1(env, null, 100);
                SendEventAssertSum(env, null, "E1", 10, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventS1(env, null, 200);
                SendEventAssertSum(env, "ST", "E2", 20, 10 + 20);
                SendEventS1(env, "ST", 200);
                SendEventAssertNoOutput(env, "ST", "E3", -1);

                env.Milestone(1);

                SendEventAssertNoOutput(env, "ST", "E4", -1);
                SendEventS0(env, "ST", 20);
                SendEventAssertSum(env, "ST", "E4", 30, 30);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(2);

                SendEventAssertSum(env, null, "E4", 40, 30 + 40);
                SendEventS1(env, null, 100);
                SendEventS1(env, "ST", 200);
                SendEventAssertSum(env, null, "E5", 41, 30 + 40 + 41);
                SendEventS1(env, null, 200);
                SendEventAssertNoOutput(env, null, "E6", -1);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextKeyedWithTerminated : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext partition by theString from SupportBean, p00 from SupportBean_S0 " +
                    "terminated by SupportBean_S0;\n" +
                    "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventAssertSum(env, null, "A", 10, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "A", 20, 10 + 20);
                env.SendEventBeanStage("ST", new SupportBean_S0(100, "A"));
                SendEventAssertSum(env, "ST", "A", 21, 21);

                env.Milestone(1);

                SendEventAssertSum(env, "ST", "B", 30, 30);
                env.SendEventBeanStage("ST", new SupportBean_S0(101, "B"));
                SendEventAssertSum(env, "ST", "A", 31, 21 + 31);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(2);

                SendEventAssertSum(env, null, "A", 40, 21 + 31 + 40);
                SendEventAssertSum(env, null, "B", 41, 41);
                env.SendEventBeanStage(null, new SupportBean_S0(102, "A"));
                SendEventAssertSum(env, null, "A", 42, 42);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextKeyedWithInitiated : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext partition by theString from SupportBean initiated by SupportBean(intPrimitive=1);\n" +
                    "@name('s0') context MyContext select sum(longPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventAssertSum(env, null, "A", 1, 10, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "A", 0, 20, 10 + 20);
                SendEventAssertNoOutput(env, "ST", "B", 0);

                env.Milestone(1);

                SendEventAssertSum(env, "ST", "A", 0, 30, 10 + 20 + 30);
                SendEventAssertSum(env, "ST", "B", 1, 31, 31);
                SendEventAssertNoOutput(env, "ST", "C", 0);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(2);

                SendEventAssertSum(env, null, "B", 0, 40, 31 + 40);
                SendEventAssertSum(env, null, "A", 0, 41, 10 + 20 + 30 + 41);
                SendEventAssertNoOutput(env, null, "C", 0);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextStartWithEndFilter : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext start SupportBean_S0 end SupportBean_S1;\n" +
                    "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventS0(env, null, 100);
                SendEventAssertSum(env, null, "E1", 10, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "E2", 20, 10 + 20);
                SendEventS1(env, null, 101);
                SendEventAssertSum(env, "ST", "E3", 21, 10 + 20 + 21);
                SendEventS1(env, "ST", 102);
                SendEventAssertNoOutput(env, "ST", "E4", 22);

                env.Milestone(1);

                SendEventAssertNoOutput(env, "ST", "E5", 30);
                SendEventS0(env, null, 103);
                SendEventAssertNoOutput(env, "ST", "E6", 31);
                SendEventS0(env, "ST", 104);
                SendEventAssertSum(env, "ST", "E7", 32, 32);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(2);

                SendEventAssertSum(env, null, "E8", 40, 32 + 40);
                SendEventS1(env, null, 105);
                SendEventAssertNoOutput(env, "ST", null, "E9", 41, 0);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextNestedHashOverHash : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext \n" +
                    "  context MyContextA coalesce by consistent_hash_crc32(theString) from SupportBean granularity 16,\n" +
                    "  context MyContextB coalesce by consistent_hash_crc32(intPrimitive) from SupportBean granularity 16;\n" +
                    "@name('s0') context MyContext select sum(longPrimitive) as thesum from SupportBean group by theString, intPrimitive;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                SendEventAssertSum(env, null, "A", 1, 10, 10);

                StageIt(env, "ST", deploymentId);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "A", 2, 11, 11);
                SendEventAssertSum(env, "ST", "B", 1, 12, 12);
                SendEventAssertSum(env, "ST", "A", 1, 13, 10 + 13);

                env.Milestone(2);

                SendEventAssertSum(env, "ST", "A", 2, 20, 11 + 20);
                SendEventAssertSum(env, "ST", "B", 1, 21, 12 + 21);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEventAssertSum(env, null, "A", 1, 30, 10 + 13 + 30);
                SendEventAssertSum(env, null, "A", 2, 31, 11 + 20 + 31);
                SendEventAssertSum(env, null, "B", 1, 32, 12 + 21 + 32);
                SendEventAssertSum(env, null, "C", 3, 33, 33);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextNestedInitiatedOverKeyed : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext \n" +
                    "  context MyContextA initiated by SupportBean_S0 as e1,\n" +
                    "  context MyContextB partition by theString from SupportBean;\n" +
                    "@name('s0') context MyContext select context.MyContextA.e1.id as c0, context.MyContextB.key1 as c1," +
                    "  sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");
                var fields = "c0,c1,thesum".SplitCsv();

                env.Milestone(0);

                SendEventS0(env, null, 1000);
                SendEvent(env, null, "A", 10);
                AssertProps(
                    env.ListenerStage(null, "s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { 1000, "A", 10 });

                StageIt(env, "ST", deploymentId);

                env.Milestone(1);

                SendEvent(env, "ST", "A", 20);
                AssertPropsPerRow(
                    env.ListenerStage("ST", "s0").GetAndResetLastNewData(),
                    fields,
                    new object[][] { new object[] { 1000, "A", 10 + 20 } });
                SendEvent(env, "ST", "B", 21);
                AssertPropsPerRow(
                    env.ListenerStage("ST", "s0").GetAndResetLastNewData(),
                    fields,
                    new object[][] { new object[] { 1000, "B", 21 } });

                env.Milestone(2);

                SendEventS0(env, "ST", 2000);
                SendEvent(env, "ST", "A", 30);
                AssertPropsPerRow(
                    env.ListenerStage("ST", "s0").GetAndResetLastNewData(),
                    fields,
                    new object[][] { new object[] { 1000, "A", 10 + 20 + 30 }, new object[] { 2000, "A", 30 } });

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEvent(env, null, "A", 40);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { 1000, "A", 10 + 20 + 30 + 40 }, new object[] { 2000, "A", 30 + 40 } });

                SendEventS0(env, null, 3000);

                SendEvent(env, null, "B", 41);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1000, "B", 21 + 41 }, new object[] { 2000, "B", 41 },
                        new object[] { 3000, "B", 41 }
                    });

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextNestedPartitionedOverStart : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext \n" +
                    "  context MyContextCategory partition by theString from SupportBean,\n" +
                    "  context MyContextPartitioned start SupportBean(intPrimitive=1);\n" +
                    "@name('s0') context MyContext select sum(longPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                env.Milestone(0);

                SendEventAssertSum(env, null, "A", 1, 10, 10);
                SendEventAssertNoOutput(env, null, null, "B", 0, 99);

                StageIt(env, "ST", deploymentId);

                SendEventAssertSum(env, "ST", "A", 0, 20, 30);
                SendEventAssertNoOutput(env, "ST", "ST", "C", 0, 99);
                SendEventAssertSum(env, "ST", "B", 1, 21, 21);
                SendEventAssertNoOutput(env, null, "ST", "D", 1, 99);

                env.Milestone(1);

                SendEventAssertSum(env, "ST", "B", 0, 30, 21 + 30);
                SendEventAssertSum(env, "ST", "A", 0, 31, 10 + 20 + 31);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEventAssertNoOutput(env, "ST", null, "C", 1, -1);
                SendEventAssertNoOutput(env, null, null, "D", 0, 99);
                SendEventAssertSum(env, null, "A", 0, 41, 10 + 20 + 31 + 41);
                SendEventAssertSum(env, null, "B", 0, 42, 21 + 30 + 42);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextNestedCategoryOverKeyed : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext \n" +
                    "  context MyContextCategory group by theString = 'A' as grp1, group by theString = 'B' as grp2 from SupportBean,\n" +
                    "  context MyContextPartitioned partition by intPrimitive from SupportBean;\n" +
                    "@name('s0') context MyContext select sum(longPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                env.Milestone(0);

                SendEventAssertSum(env, null, "A", 1000, 10, 10);
                SendEventAssertNoOutput(env, null, "C", 99);

                StageIt(env, "ST", deploymentId);

                SendEventAssertSum(env, "ST", "A", 1000, 11, 21);
                SendEventAssertNoOutput(env, "ST", "C", 1000);
                SendEventAssertSum(env, "ST", "B", 2000, 12, 12);
                SendEventAssertSum(env, "ST", "B", 2001, 13, 13);

                env.Milestone(1);

                SendEventAssertSum(env, "ST", "B", 2000, 14, 26);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEventAssertNoOutput(env, "ST", null, "A", 1000, -1);
                SendEventAssertSum(env, null, "A", 1000, 20, 41);
                SendEventAssertNoOutput(env, null, "C", 1000);
                SendEventAssertSum(env, null, "B", 2000, 21, 12 + 14 + 21);
                SendEventAssertSum(env, null, "B", 2001, 22, 13 + 22);
                SendEventAssertSum(env, null, "A", 1001, 23, 23);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextInitiatedNoTerminated : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext initiated by SupportBean(theString='init') as sb;\n" +
                    "@name('s0') context MyContext select context.sb.intPrimitive as c0, sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                var fields = new string[] { "c0", "thesum" };

                SendEvent(env, null, "init", 100);
                env.AssertPropsNew("s0", fields, new object[] { 100, 100 });

                SendEvent(env, null, "x", 101);
                env.AssertPropsNew("s0", fields, new object[] { 100, 201 });

                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentId);

                SendEvent(env, "ST", "y", 200);
                AssertProps(env.ListenerStage("ST", "s0").AssertOneGetNewAndReset(), fields, new object[] { 100, 401 });

                env.Milestone(0);

                SendEvent(env, "ST", "init", 300);
                AssertPropsPerRowAnyOrder(
                    env.ListenerStage("ST", "s0").GetAndResetLastNewData(),
                    fields,
                    new object[][] { new object[] { 100, 701 }, new object[] { 300, 300 } });

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEvent(env, null, "z", 400);
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 100, 1101 }, new object[] { 300, 700 } });

                SendEvent(env, null, "init", 401);
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 100, 1101 + 401 }, new object[] { 300, 700 + 401 }, new object[] { 401, 401 }
                    });

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextStartNoEnd : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext start SupportBean(theString='start');\n" +
                    "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");

                SendEventAssertNoOutput(env, null, "E1", 1);

                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentId);

                SendEventAssertNoOutput(env, null, "ST", "E2", 1, 0);

                env.Milestone(0);

                SendEventAssertSum(env, "ST", "start", 10, 10);
                SendEventAssertSum(env, "ST", "E3", 11, 21);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEventAssertSum(env, null, "E4", 12, 33);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextHash : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext coalesce by consistent_hash_crc32(theString) from SupportBean granularity 16;\n" +
                    "@name('s0') context MyContext select theString, sum(intPrimitive) as thesum from SupportBean group by theString;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");

                SendEventAssertSum(env, null, "A", 10, 10);

                env.Milestone(0);

                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentId);

                env.Milestone(1);

                SendEventAssertSum(env, "ST", "A", 11, 21);
                SendEventAssertSum(env, "ST", "B", 12, 12);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEventAssertSum(env, null, "A", 13, 34);
                SendEventAssertSum(env, null, "B", 14, 26);
                SendEventAssertSum(env, null, "C", 15, 15);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextCategory : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context MyContext group by theString='A' as grp1, group by theString='B' as grp2 from SupportBean;\n" +
                    "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");

                SendEventAssertSum(env, null, "A", 10, 10);

                env.Milestone(0);

                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentId);

                env.Milestone(1);

                SendEvent(env, null, "A", 11);
                SendEventAssertSum(env, "ST", "A", 12, 22);
                SendEventAssertSum(env, "ST", "B", 13, 13);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(3);

                SendEvent(env, "ST", "A", 14);
                SendEvent(env, null, "C", 15);
                SendEventAssertSum(env, null, "A", 16, 38);
                SendEventAssertSum(env, null, "B", 17, 30);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventContextKeyed : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context MyContext partition by theString from SupportBean",
                    path);
                env.CompileDeploy(
                        "@name('s0') context MyContext select sum(intPrimitive) as thesum from SupportBean",
                        path)
                    .AddListener("s0");
                var deploymentIdContext = env.DeploymentId("context");
                var deploymentIdStmt = env.DeploymentId("s0");

                SendEventAssertSum(env, null, "A", 1, 1);
                SendEventAssertSum(env, null, "A", 2, 3);

                env.Milestone(0);

                env.StageService.GetStage("ST");
                StageIt(env, "ST", deploymentIdContext, deploymentIdStmt);

                env.Milestone(1);

                SendEvent(env, null, "A", 3);
                SendEventAssertSum(env, "ST", "A", 4, 1 + 2 + 4);
                SendEventAssertSum(env, "ST", "B", 10, 10);

                env.Milestone(2);

                UnstageIt(env, "ST", deploymentIdContext, deploymentIdStmt);

                env.Milestone(3);

                SendEvent(env, "ST", "A", 5);
                SendEventAssertSum(env, null, "A", 6, 1 + 2 + 4 + 6);
                SendEventAssertSum(env, null, "C", 20, 20);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWEvery : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[every SupportBean]";
                RunAssertionPatternEvery(env, epl);
            }
        }

        private class ClientStageSendEventPatternWEveryDistinct : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[every-distinct(a.theString) a=SupportBean]";
                RunAssertionPatternEvery(env, epl);
            }
        }

        private class ClientStageSendEventPatternWOr : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[SupportBean(theString='a') or SupportBean(theString='b')]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                StageIt(env, "ST", deploymentId);

                SendEvent(env, "ST", "a");
                env.ListenerStage("ST", "s0").AssertOneGetNewAndReset();

                UnstageIt(env, "ST", deploymentId);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWNot : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[SupportBean(theString='a') and not SupportBean(theString='b')]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                StageIt(env, "ST", deploymentId);

                SendEvent(env, "ST", "b");
                SendEvent(env, "ST", "a");
                Assert.IsFalse(env.ListenerStage("ST", "s0").GetAndClearIsInvoked());

                UnstageIt(env, "ST", deploymentId);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWUntil : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[SupportBean(theString='a') until SupportBean(theString='b')]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                StageIt(env, "ST", deploymentId);

                SendEvent(env, "ST", "b");
                SendEvent(env, "ST", "a");
                Assert.IsTrue(env.ListenerStage("ST", "s0").GetAndClearIsInvoked());

                UnstageIt(env, "ST", deploymentId);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWGuard : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[SupportBean(theString='a') where timer:within(10 sec)]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");
                env.StageService.GetStage("ST");

                StageIt(env, "ST", deploymentId);

                SendEvent(env, "ST", "a");
                env.ListenerStage("ST", "s0").AssertOneGetNewAndReset();

                UnstageIt(env, "ST", deploymentId);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWAnd : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[SupportBean(theString='a') and SupportBean(theString='b')]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");

                SendEvent(env, null, "a");
                env.StageService.GetStage("ST");

                env.Milestone(0);

                StageIt(env, "ST", deploymentId);
                SendEvent(env, "ST", "b");
                env.ListenerStage("ST", "s0").AssertOneGetNewAndReset();

                UnstageIt(env, "ST", deploymentId);

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventPatternWFollowedBy : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[SupportBean(theString='a') -> SupportBean(theString='b') ->" +
                    "SupportBean(theString='c') -> SupportBean(theString='d') -> SupportBean(theString='e')]";
                env.CompileDeploy(epl).AddListener("s0");
                var deploymentId = env.DeploymentId("s0");

                SendEvent(env, null, "a");
                env.StageService.GetStage("ST");

                env.Milestone(0);

                StageIt(env, "ST", deploymentId);
                SendEvent(env, "ST", "b");

                env.Milestone(1);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(2);

                SendEvent(env, null, "c");

                env.Milestone(3);

                StageIt(env, "ST", deploymentId);
                SendEvent(env, "ST", "d");

                env.Milestone(4);

                UnstageIt(env, "ST", deploymentId);

                env.Milestone(5);

                SendEvent(env, null, "e");
                env.AssertEventNew("s0", @event => { });

                env.UndeployAll();
            }
        }

        private class ClientStageSendEventNamedWindow : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "create window MyWindow#keepall as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@name('s0') select sum(intPrimitive) as c0 from MyWindow;\n";
                RunAssertionSimple(env, epl);
            }
        }

        private class ClientStageSendEventFilter : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                RunAssertionSimple(env, "@name('s0') select sum(intPrimitive) as c0 from SupportBean");
            }
        }

        private static void RunAssertionSimple(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            var deploymentId = env.DeploymentId("s0");

            SendEvent(env, null, "E1", 10);
            AssertTotal(env, null, 10);

            env.Milestone(0);

            env.StageService.GetStage("P1");
            StageIt(env, "P1", deploymentId);

            env.Milestone(1);

            Assert.IsNull(env.Deployment.GetDeployment(deploymentId));
            Assert.IsNotNull(env.StageService.GetExistingStage("P1").DeploymentService.GetDeployment(deploymentId));
            AssertEqualsAnyOrder(
                new string[] { deploymentId },
                env.StageService.GetExistingStage("P1").DeploymentService.Deployments);
            SendEvent(env, null, "E3", 21);
            SendEvent(env, "P1", "E4", 22);
            AssertTotal(env, "P1", 10 + 22);

            env.Milestone(2);

            UnstageIt(env, "P1", deploymentId);

            env.Milestone(3);

            Assert.IsNotNull(env.Deployment.GetDeployment(deploymentId));
            Assert.IsNull(env.StageService.GetExistingStage("P1").DeploymentService.GetDeployment(deploymentId));
            SendEvent(env, null, "E5", 31);
            SendEvent(env, "P1", "E6", 32);
            AssertTotal(env, null, 10 + 22 + 31);
            env.AssertThat(
                () => {
                    var listener = env.Listener("s0");

                    env.UndeployAll();

                    SendEvent(env, null, "end", 99);
                    Assert.IsFalse(listener.GetAndClearIsInvoked());
                });
            env.UndeployAll();
        }

        private static void RunAssertionPatternEvery(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            var deploymentId = env.DeploymentId("s0");
            env.StageService.GetStage("ST");

            StageIt(env, "ST", deploymentId);

            SendEvent(env, "ST", "E1");
            env.ListenerStage("ST", "s0").AssertOneGetNewAndReset();

            UnstageIt(env, "ST", deploymentId);

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string stageUri,
            string theString)
        {
            SendEvent(env, stageUri, theString, -1);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string stageUri,
            string theString,
            int intPrimitive)
        {
            SendEvent(env, stageUri, theString, intPrimitive, -1);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string stageUri,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBeanStage(stageUri, sb);
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            string stageUri,
            int id)
        {
            env.SendEventBeanStage(stageUri, new SupportBean_S0(id));
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            string stageUri,
            int id)
        {
            env.SendEventBeanStage(stageUri, new SupportBean_S1(id));
        }

        private static void SendEventAssertNoOutput(
            RegressionEnvironment env,
            string stageUri,
            string theString,
            int intPrimitive)
        {
            SendEvent(env, stageUri, theString, intPrimitive);
            Assert.IsFalse(env.ListenerStage(stageUri, "s0").GetAndClearIsInvoked());
        }

        private static void SendEventAssertNoOutput(
            RegressionEnvironment env,
            string stageSendEvent,
            string stageListener,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            SendEvent(env, stageSendEvent, theString, intPrimitive, longPrimitive);
            Assert.IsFalse(env.ListenerStage(stageListener, "s0").GetAndClearIsInvoked());
        }

        private static void AssertTotal(
            RegressionEnvironment env,
            string stageUri,
            int total)
        {
            AssertProps(
                env.ListenerStage(stageUri, "s0").AssertOneGetNewAndReset(),
                "c0".SplitCsv(),
                new object[] { total });
        }

        private static void SendEventAssertSum(
            RegressionEnvironment env,
            string stageUri,
            string theString,
            int intPrimitive,
            int expected)
        {
            env.SendEventBeanStage(stageUri, new SupportBean(theString, intPrimitive));
            Assert.AreEqual(expected, env.ListenerStage(stageUri, "s0").AssertOneGetNewAndReset().Get("thesum"));
        }

        private static void SendEventAssertSum(
            RegressionEnvironment env,
            string stageUri,
            string theString,
            int intPrimitive,
            long longPrimitive,
            long expected)
        {
            SendEvent(env, stageUri, theString, intPrimitive, longPrimitive);
            Assert.AreEqual(expected, env.ListenerStage(stageUri, "s0").AssertOneGetNewAndReset().Get("thesum"));
        }
    }
} // end of namespace