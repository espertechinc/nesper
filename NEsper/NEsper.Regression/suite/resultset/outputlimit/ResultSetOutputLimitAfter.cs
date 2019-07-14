///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitAfter
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAfterWithOutputLast());
            execs.Add(new ResultSetEveryPolicy());
            execs.Add(new ResultSetMonthScoped());
            execs.Add(new ResultSetDirectNumberOfEvents());
            execs.Add(new ResultSetDirectTimePeriod());
            execs.Add(new ResultSetSnapshotVariable());
            execs.Add(new ResultSetOutputWhenThen());
            return execs;
        }

        private static void TryAssertionSnapshotVar(RegressionEnvironment env)
        {
            SendTimer(env, 6000);
            SendEvent(env, "E1");
            SendEvent(env, "E2");

            env.Milestone(0);

            SendTimer(env, 19999);
            SendEvent(env, "E3");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimer(env, 20000);
            SendEvent(env, "E4");
            var fields = "TheString".SplitCsv();
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
            env.Listener("s0").Reset();

            env.Milestone(1);

            SendTimer(env, 21000);
            SendEvent(env, "E5");
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"},
                    new object[] {"E5"}
                });
            env.Listener("s0").Reset();
        }

        private static void TryAssertionEveryPolicy(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "TheString".SplitCsv();
            SendTimer(env, 1);
            SendEvent(env, "E1");

            env.MilestoneInc(milestone);

            SendTimer(env, 6000);
            SendEvent(env, "E2");

            env.MilestoneInc(milestone);

            SendTimer(env, 16000);
            SendEvent(env, "E3");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            SendTimer(env, 20000);
            SendEvent(env, "E4");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimer(env, 24999);
            SendEvent(env, "E5");

            env.MilestoneInc(milestone);

            SendTimer(env, 25000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E4"}, new object[] {"E5"}});
            env.Listener("s0").Reset();

            env.MilestoneInc(milestone);

            SendTimer(env, 27000);
            SendEvent(env, "E6");

            SendTimer(env, 29999);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            SendTimer(env, 30000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E6"});
        }

        private static void RunAssertionAfterWithOutputLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "@Name('s0') select sum(IntPrimitive) as thesum " +
                      "from SupportBean#keepall " +
                      "output after 4 events last every 2 events";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));

            env.Milestone(0);

            env.SendEventBean(new SupportBean("E3", 30));
            env.SendEventBean(new SupportBean("E4", 40));

            env.Milestone(1);

            env.SendEventBean(new SupportBean("E5", 50));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean("E6", 60));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "thesum".SplitCsv(),
                new object[] {210});

            env.UndeployAll();
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        internal class ResultSetAfterWithOutputLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertionAfterWithOutputLast(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetEveryPolicy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                SendTimer(env, 0);
                var stmtText =
                    "select TheString from SupportBean#keepall output after 0 days 0 hours 0 minutes 20 seconds 0 milliseconds every 0 days 0 hours 0 minutes 5 seconds 0 milliseconds";
                env.CompileDeploy("@Name('s0') " + stmtText).AddListener("s0");

                TryAssertionEveryPolicy(env, milestone);

                env.UndeployAll();

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("TheString");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
                model.OutputLimitClause = OutputLimitClause.Create(Expressions.TimePeriod(0, 0, 0, 5, 0))
                    .WithAfterTimePeriodExpression(Expressions.TimePeriod(0, 0, 0, 20, 0));
                Assert.AreEqual(stmtText, model.ToEPL());
            }
        }

        internal class ResultSetMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@Name('s0') select * from SupportBean output after 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "TheString".SplitCsv(),
                    new object[] {"E3"});

                env.UndeployAll();
            }
        }

        internal class ResultSetDirectNumberOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var stmtText = "@Name('s0') select TheString from SupportBean#keepall output after 3 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "E1");

                env.Milestone(0);

                SendEvent(env, "E2");
                SendEvent(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendEvent(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                SendEvent(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.UndeployAll();

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("TheString");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
                model.OutputLimitClause = OutputLimitClause.CreateAfter(3);
                Assert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                env.CompileDeploy(model).AddListener("s0");

                SendEvent(env, "E1");
                SendEvent(env, "E2");
                SendEvent(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                SendEvent(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                model = env.EplToModel("select TheString from SupportBean#keepall output after 3 events");
                Assert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class ResultSetDirectTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var fields = "TheString".SplitCsv();
                var stmtText = "@Name('s0') select TheString from SupportBean#keepall output after 20 seconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.Milestone(0);

                SendTimer(env, 1);
                SendEvent(env, "E1");

                env.Milestone(1);

                SendTimer(env, 6000);
                SendEvent(env, "E2");

                env.Milestone(2);

                SendTimer(env, 19999);
                SendEvent(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendTimer(env, 20000);
                SendEvent(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                SendTimer(env, 21000);
                SendEvent(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.UndeployAll();
            }
        }

        internal class ResultSetSnapshotVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int myvar_local = 1", path);

                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select TheString from SupportBean#keepall output after 20 seconds snapshot when myvar_local=1";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                TryAssertionSnapshotVar(env);

                env.UndeployModuleContaining("s0");

                env.EplToModelCompileDeploy(stmtText, path).UndeployAll();
            }
        }

        internal class ResultSetOutputWhenThen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable boolean myvar0 = false;\n" +
                          "create variable boolean myvar1 = false;\n" +
                          "create variable boolean myvar2 = false;\n" +
                          "@Name('s0')\n" +
                          "select a.* from SupportBean#time(10) a output after 3 events when myvar0=true then set myvar1=true, myvar2=true";
                env.CompileDeploy(epl).AddListener("s0");
                var depId = env.DeploymentId("s0");

                SendEvent(env, "E1");
                SendEvent(env, "E2");
                SendEvent(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(depId, "myvar0", true);
                SendEvent(env, "E4");
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                Assert.AreEqual(true, env.Runtime.VariableService.GetVariableValue(depId, "myvar1"));
                Assert.AreEqual(true, env.Runtime.VariableService.GetVariableValue(depId, "myvar2"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace