///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;
using NUnit.Framework.Legacy;


namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitAfter
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if TEMPORARY
            WithAfterWithOutputLast(execs);
            WithEveryPolicy(execs);
            WithMonthScoped(execs);
            WithDirectNumberOfEvents(execs);
            WithDirectTimePeriod(execs);
            WithSnapshotVariable(execs);
            WithOutputWhenThen(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputWhenThen(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputWhenThen());
            return execs;
        }

        public static IList<RegressionExecution> WithSnapshotVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSnapshotVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithDirectTimePeriod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetDirectTimePeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithDirectNumberOfEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetDirectNumberOfEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryPolicy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetEveryPolicy());
            return execs;
        }

        public static IList<RegressionExecution> WithAfterWithOutputLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAfterWithOutputLast());
            return execs;
        }

        private class ResultSetAfterWithOutputLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertionAfterWithOutputLast(env, outputLimitOpt, milestone);
                }
            }
        }

        private class ResultSetEveryPolicy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                SendTimer(env, 0);
                var stmtText =
                    "select TheString from SupportBean#keepall output after 0 days 0 hours 0 minutes 20 seconds 0 milliseconds every 0 days 0 hours 0 minutes 5 seconds 0 milliseconds";
                env.CompileDeploy("@name('s0') " + stmtText).AddListener("s0");

                TryAssertionEveryPolicy(env, milestone);

                env.UndeployAll();

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("TheString");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
                model.OutputLimitClause = OutputLimitClause.Create(Expressions.TimePeriod(0, 0, 0, 5, 0))
                    .WithAfterTimePeriodExpression(Expressions.TimePeriod(0, 0, 0, 20, 0));
                ClassicAssert.AreEqual(stmtText, model.ToEPL());
            }
        }

        private class ResultSetMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@name('s0') select * from SupportBean output after 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", "TheString".SplitCsv(), new object[] { "E3" });

                env.UndeployAll();
            }
        }

        private class ResultSetDirectNumberOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var stmtText = "@name('s0') select TheString from SupportBean#keepall output after 3 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "E1");

                env.Milestone(0);

                SendEvent(env, "E2");
                SendEvent(env, "E3");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendEvent(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                SendEvent(env, "E5");
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.UndeployAll();

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("TheString");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
                model.OutputLimitClause = OutputLimitClause.CreateAfter(3);
                ClassicAssert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                env.CompileDeploy(model).AddListener("s0");

                SendEvent(env, "E1");
                SendEvent(env, "E2");
                SendEvent(env, "E3");
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                SendEvent(env, "E5");
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                model = env.EplToModel("select TheString from SupportBean#keepall output after 3 events");
                ClassicAssert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());

                env.UndeployAll();
            }
        }

        private class ResultSetDirectTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var fields = "TheString".SplitCsv();
                var stmtText = "@name('s0') select TheString from SupportBean#keepall output after 20 seconds";
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
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimer(env, 20000);
                SendEvent(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                SendTimer(env, 21000);
                SendEvent(env, "E5");
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.UndeployAll();
            }
        }

        private class ResultSetSnapshotVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int myvar_local = 1", path);

                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select TheString from SupportBean#keepall output after 20 seconds snapshot when myvar_local=1";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                TryAssertionSnapshotVar(env);

                env.UndeployModuleContaining("s0");

                env.EplToModelCompileDeploy(stmtText, path).UndeployAll();
            }
        }

        private class ResultSetOutputWhenThen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable boolean myvar0 = false;\n" +
                          "create variable boolean myvar1 = false;\n" +
                          "create variable boolean myvar2 = false;\n" +
                          "@name('s0')\n" +
                          "select a.* from SupportBean#time(10) a output after 3 events when myvar0=true then set myvar1=true, myvar2=true";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1");
                SendEvent(env, "E2");
                SendEvent(env, "E3");
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable("s0", "myvar0", true);
                SendEvent(env, "E4");
                env.AssertListenerInvoked("s0");
                env.AssertRuntime(
                    runtime => {
                        var depId = env.DeploymentId("s0");
                        ClassicAssert.AreEqual(true, runtime.VariableService.GetVariableValue(depId, "myvar1"));
                        ClassicAssert.AreEqual(true, runtime.VariableService.GetVariableValue(depId, "myvar2"));
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionSnapshotVar(RegressionEnvironment env)
        {
            SendTimer(env, 6000);
            SendEvent(env, "E1");
            SendEvent(env, "E2");

            env.Milestone(0);

            SendTimer(env, 19999);
            SendEvent(env, "E3");
            env.AssertListenerNotInvoked("s0");

            SendTimer(env, 20000);
            SendEvent(env, "E4");
            var fields = "TheString".SplitCsv();
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][]
                    { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

            env.Milestone(1);

            SendTimer(env, 21000);
            SendEvent(env, "E5");
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" },
                    new object[] { "E5" }
                });
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
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendTimer(env, 20000);
            SendEvent(env, "E4");
            env.AssertListenerNotInvoked("s0");

            SendTimer(env, 24999);
            SendEvent(env, "E5");

            env.MilestoneInc(milestone);

            SendTimer(env, 25000);
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E4" }, new object[] { "E5" } });

            env.MilestoneInc(milestone);

            SendTimer(env, 27000);
            SendEvent(env, "E6");

            SendTimer(env, 29999);
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendTimer(env, 30000);
            env.AssertPropsNew("s0", fields, new object[] { "E6" });
        }

        private static void RunAssertionAfterWithOutputLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var epl = opt.GetHint() +
                      "@name('s0') select sum(IntPrimitive) as thesum " +
                      "from SupportBean#keepall " +
                      "output after 4 events last every 2 events";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E3", 30));
            env.SendEventBean(new SupportBean("E4", 40));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E5", 50));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("E6", 60));
            env.AssertPropsNew("s0", "thesum".SplitCsv(), new object[] { 210 });

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
    }
} // end of namespace