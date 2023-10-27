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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.schedule.SupportDateTimeUtil; // timePlusMonth

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeWin
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithWindowSceneOne(execs);
            WithWindowSceneTwo(execs);
            WithJustSelectStar(execs);
            WithSum(execs);
            WithSumGroupBy(execs);
            WithSumWFilter(execs);
            WithWindowMonthScoped(execs);
            WithWindowWPrev(execs);
            WithWindowPreparedStmt(execs);
            WithWindowVariableStmt(execs);
            WithWindowTimePeriod(execs);
            WithWindowVariableTimePeriodStmt(execs);
            WithWindowTimePeriodParams(execs);
            WithWindowFlipTimer(execs);

            return execs;
        }

        public static IList<RegressionExecution> WithWindowFlipTimer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowFlipTimer(0, "1", 1000));
            execs.Add(new ViewTimeWindowFlipTimer(123456789, "10", 123456789 + 10 * 1000));
            execs.Add(new ViewTimeWindowFlipTimer(0, "1 months 10 milliseconds", TimePlusMonth(0, 1) + 10));

            var currentTime = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:00:01.999");
            execs.Add(
                new ViewTimeWindowFlipTimer(
                    currentTime,
                    "1 months 50 milliseconds",
                    TimePlusMonth(currentTime, 1) + 50));
            return execs;
        }

        public static IList<RegressionExecution> WithWindowTimePeriodParams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowTimePeriodParams());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowVariableTimePeriodStmt(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowVariableTimePeriodStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowTimePeriod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowTimePeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowVariableStmt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowVariableStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowPreparedStmt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowPreparedStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowWPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowWPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithSumWFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeSumWFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithSumGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeSumGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeSum());
            return execs;
        }

        public static IList<RegressionExecution> WithJustSelectStar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeJustSelectStar());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowSceneOne());
            return execs;
        }

        public class ViewTimeWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();

                env.AdvanceTime(0);
                var epl = "@name('s0') select irstream * from SupportBean#time(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, null);
                env.AdvanceTime(1000);
                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });
                env.AdvanceTime(2000);
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                env.AdvanceTime(3000);
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.AdvanceTime(11000);
                env.AssertPropsOld("s0", fields, new object[] { "E1" });

                env.Milestone(4);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                env.AdvanceTime(12000);
                env.AssertPropsOld("s0", fields, new object[] { "E2" });

                env.Milestone(5);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E3" } });
                SendSupportBean(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(6);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });
                SendSupportBean(env, "E5");
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.Milestone(7);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } });
                env.AdvanceTime(13000);
                env.AssertPropsOld("s0", fields, new object[] { "E3" });

                env.Milestone(8);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });
                env.AdvanceTime(22000);
                env.AssertPropsPerRowLastOld(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.Milestone(9);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { });

                env.Milestone(10);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { });

                env.UndeployAll();
            }
        }

        public class ViewTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                env.AdvanceTime(1000);
                var epl = "@name('s0') select irstream * from SupportBean#time(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");

                env.Milestone(1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                SendSupportBean(env, "E3");
                SendSupportBean(env, "E4");

                env.Milestone(2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

                env.AdvanceTime(2000);
                SendSupportBean(env, "E5");

                env.Milestone(3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" },
                        new object[] { "E5" }
                    });

                env.AdvanceTime(10999);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" },
                        new object[] { "E5" }
                    });

                env.Milestone(4);

                env.AdvanceTime(11000);

                env.Milestone(5);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E5" } });

                env.Milestone(6);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@name('s0') select rstream * from SupportBean#time(1 month)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                SendCurrentTime(env, "2002-02-15T09:00:00.000");
                env.SendEventBean(new SupportBean("E2", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.AssertPropsNew("s0", "TheString".SplitCsv(), new object[] { "E1" });

                SendCurrentTimeWithMinus(env, "2002-03-15T09:00:00.000", 1);
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-15T09:00:00.000");
                env.AssertPropsNew("s0", "TheString".SplitCsv(), new object[] { "E2" });

                env.UndeployAll();
            }
        }

        private class ViewTimeSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select Symbol, Volume, sum(Price) as mySum from SupportMarketDataBean#time(30)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertStatement("s0", ViewTimeWin.AssertSelectResultType);

                SendEvent(env, SYMBOL_DELL, 10000, 51);
                env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 10000, 51, false));

                SendEvent(env, SYMBOL_IBM, 20000, 52);
                env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 20000, 103, false));

                SendEvent(env, SYMBOL_DELL, 40000, 45);
                env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 40000, 148, false));

                env.AdvanceTime(35000);

                //These events are out of the window and new sums are generated

                SendEvent(env, SYMBOL_IBM, 30000, 70);
                env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 30000, 70, false));

                SendEvent(env, SYMBOL_DELL, 10000, 20);
                env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 10000, 90, false));

                env.UndeployAll();
            }
        }

        private class ViewTimeSumGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select Symbol, Volume, sum(Price) as mySum "+
"from SupportMarketDataBean#time(30) group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryGroupByAssertions(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeSumWFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl =
"@name('s0') select Symbol, Volume, sum(Price) as mySum from SupportMarketDataBean(Symbol = 'IBM')#time(30)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TrySingleAssertion(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeJustSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@name('s0') select irstream * from SupportMarketDataBean#time(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "Symbol", "E1" } }, null);

                env.Milestone(1);

                env.AdvanceTime(600);
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "Symbol", "E2" } }, null);

                env.Milestone(2);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Symbol"},
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.AdvanceTime(1500);
                env.AssertPropsNV("s0", null, new object[][] { new object[] { "Symbol", "E1" } }); // olddata

                env.Milestone(3);

                env.AdvanceTime(1600);
                env.AssertPropsNV("s0", null, new object[][] { new object[] { "Symbol", "E2" } }); // olddata

                env.Milestone(4);

                env.AdvanceTime(2000);

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "Symbol", "E3" } }, null);

                env.Milestone(6);

                env.UndeployAll();
            }
        }

        private static void TryGroupByAssertions(RegressionEnvironment env)
        {
            env.AssertStatement("s0", ViewTimeWin.AssertSelectResultType);

            env.AdvanceTime(0);

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 10000, 51, false));

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 30000, 70, false));

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 20000, 103, false));

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 30000, 140, false));

            env.AdvanceTime(35000);

            //These events are out of the window and new sums are generated
            SendEvent(env, SYMBOL_DELL, 10000, 90);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 10000, 90, false));

            SendEvent(env, SYMBOL_IBM, 30000, 120);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 30000, 120, false));

            SendEvent(env, SYMBOL_DELL, 20000, 90);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_DELL, 20000, 180, false));

            SendEvent(env, SYMBOL_IBM, 30000, 120);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 30000, 240, false));
        }

        private static void TrySingleAssertion(RegressionEnvironment env)
        {
            env.AssertStatement("s0", ViewTimeWin.AssertSelectResultType);

            env.AdvanceTime(0);

            SendEvent(env, SYMBOL_IBM, 20000, 52);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 20000, 52, false));

            SendEvent(env, SYMBOL_IBM, 20000, 100);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 20000, 152, false));

            env.AdvanceTime(35000);

            //These events are out of the window and new sums are generated
            SendEvent(env, SYMBOL_IBM, 20000, 252);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 20000, 252, false));

            SendEvent(env, SYMBOL_IBM, 20000, 100);
            env.AssertListener("s0", listener => AssertEvents(listener, SYMBOL_IBM, 20000, 352, false));
        }

        public class ViewTimeWindowWPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@name('s0') select irstream Symbol, "+
"prev(1, Symbol) as prev1, "+
"prevtail(Symbol) as prevtail, "+
"prevcount(Symbol) as prevCountSym, "+
"prevwindow(Symbol) as prevWindowSym "+
                           "from SupportMarketDataBean#time(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                var fields = new string[] { "Symbol", "prev1", "prevtail", "prevCountSym", "prevWindowSym" };

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", null, "E1", 1L, new object[] { "E1" } },
                        new object[] { "E2", "E1", "E1", 2L, new object[] { "E2", "E1" } },
                        new object[] { "E3", "E2", "E1", 3L, new object[] { "E3", "E2", "E1" } }
                    });

                env.Milestone(1);

                env.AdvanceTime(1200);
                env.SendEventBean(MakeMarketDataEvent("E4"));
                env.SendEventBean(MakeMarketDataEvent("E5"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E4", "E3", "E1", 4L, new object[] { "E4", "E3", "E2", "E1" } },
                        new object[] { "E5", "E4", "E1", 5L, new object[] { "E5", "E4", "E3", "E2", "E1" } }
                    });

                env.Milestone(2);

                env.AdvanceTime(1600);
                env.AssertPropsPerRowOldFlattened(
                    "s0",
"Symbol".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E6"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "Symbol", "E6" } }, null);

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowPreparedStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var text = "select rstream TheString from SupportBean#time(?::int)";
                var compiled = env.Compile(text);

                env.Deploy(
                    compiled,
                    new DeploymentOptions()
                        .WithStatementSubstitutionParameter(
                            new SupportPortableDeploySubstitutionParams(1, 4).SetStatementParameters)
                        .WithStatementNameRuntime(new SupportPortableDeployStatementName("s0").GetStatementName));
                env.Deploy(
                    compiled,
                    new DeploymentOptions()
                        .WithStatementSubstitutionParameter(
                            new SupportPortableDeploySubstitutionParams(1, 3).SetStatementParameters)
                        .WithStatementNameRuntime(new SupportPortableDeployStatementName("s1").GetStatementName));
                env.AddListener("s0").AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowVariableStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var text = "select rstream TheString from SupportBean#time(TIME_WIN_ONE)";
                env.CompileDeploy($"@name('s0') {text}").AddListener("s0");

                env.RuntimeSetVariable(null, "TIME_WIN_ONE", 3);

                env.CompileDeploy($"@name('s1') {text}").AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@name('s0') select rstream TheString from SupportBean#time(4 sec)";
                env.CompileDeploy(text).AddListener("s0");

                text = "@name('s1') select rstream TheString from SupportBean#time(3000 milliseconds)";
                env.CompileDeploy(text).AddListener("s1").Milestone(0);

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowVariableTimePeriodStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO milliseconds)";
                env.CompileDeploy($"@name('s0'){text}").AddListener("s0");

                text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO minutes)";
                env.RuntimeSetVariable(null, "TIME_WIN_TWO", 0.05);
                env.CompileDeploy($"@name('s1'){text}").AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        private class ViewTimeWindowTimePeriodParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryTimeWindow(env, "30000");
                TryTimeWindow(env, "30E6 milliseconds");
                TryTimeWindow(env, "30000 seconds");
                TryTimeWindow(env, "500 minutes");
                TryTimeWindow(env, "8.33333333333333333333 hours");
                TryTimeWindow(env, "0.34722222222222222222222222222222 days");
                TryTimeWindow(env, "0.1 hour 490 min 240 sec");
            }
        }

        public class ViewTimeWindowFlipTimer : RegressionExecution
        {
            private readonly long startTime;
            private readonly string size;
            private readonly long flipTime;

            public ViewTimeWindowFlipTimer(
                long startTime,
                string size,
                long flipTime)
            {
                this.startTime = startTime;
                this.size = size;
                this.flipTime = flipTime;
            }

            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(startTime);

                var fields = "TheString".SplitCsv();
                var epl = $"@name('s0') select * from SupportBean#time({size})";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.AdvanceTime(flipTime - 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });

                env.AdvanceTime(flipTime);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{startTime={startTime}, size='{size}', flipTime={flipTime}}}";
            }
        }

        private static void TryTimeWindow(
            RegressionEnvironment env,
            string intervalSpec)
        {
            env.AdvanceTime(0);
            var epl = $"@name('s0') select irstream * from SupportBean#time({intervalSpec})";
            env.CompileDeploy(epl).AddListener("s0");

            SendEvent(env, "E1");
            env.AssertListener("s0", _ => _.Reset());

            SendTimerAssertNotInvoked(env, 29999 * 1000);
            SendTimerAssertInvoked(env, 30000 * 1000);

            env.UndeployAll();
        }

        private static void AssertEvents(
            SupportListener listener,
            string symbol,
            long volume,
            double sum,
            bool unique)
        {
            var oldData = listener.LastOldData;
            var newData = listener.LastNewData;

            if (!unique)
                Assert.IsNull(oldData);

            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(volume, newData[0].Get("Volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));

            listener.Reset();
        }

        private static void AssertSelectResultType(EPStatement stmt)
        {
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("mySum"));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void RunAssertion(RegressionEnvironment env)
        {
            env.AdvanceTime(1000);
            SendEvent(env, "E1");

            env.AdvanceTime(2000);
            SendEvent(env, "E2");

            env.AdvanceTime(3000);
            SendEvent(env, "E3");

            env.AssertListenerNotInvoked("s0");
            env.AssertListenerNotInvoked("s1");

            env.AdvanceTime(4000);
            env.AssertEqualsNew("s1", "TheString", "E1");
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(5000);
            env.AssertEqualsNew("s0", "TheString", "E1");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString)
        {
            var theEvent = new SupportBean(theString, 1);
            env.SendEventBean(theEvent);
        }

        private static void SendTimerAssertNotInvoked(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
            env.AssertListenerNotInvoked("s0");
        }

        private static void SendTimerAssertInvoked(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
            env.AssertListenerInvoked("s0");
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