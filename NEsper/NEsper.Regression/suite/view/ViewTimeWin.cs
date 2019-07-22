///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.schedule.SupportDateTimeUtil;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeWin
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewTimeWindowSceneOne());
            execs.Add(new ViewTimeWindowSceneTwo());
            execs.Add(new ViewTimeJustSelectStar());
            execs.Add(new ViewTimeSum());
            execs.Add(new ViewTimeSumGroupBy());
            execs.Add(new ViewTimeSumWFilter());
            execs.Add(new ViewTimeWindowMonthScoped());
            execs.Add(new ViewTimeWindowWPrev());
            execs.Add(new ViewTimeWindowPreparedStmt());
            execs.Add(new ViewTimeWindowVariableStmt());
            execs.Add(new ViewTimeWindowTimePeriod());
            execs.Add(new ViewTimeWindowVariableTimePeriodStmt());
            execs.Add(new ViewTimeWindowTimePeriodParams());
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

        private static void TryGroupByAssertions(RegressionEnvironment env)
        {
            AssertSelectResultType(env.Statement("s0"));

            env.AdvanceTime(0);

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            AssertEvents(env, SYMBOL_DELL, 10000, 51, false);

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            AssertEvents(env, SYMBOL_IBM, 30000, 70, false);

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            AssertEvents(env, SYMBOL_DELL, 20000, 103, false);

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            AssertEvents(env, SYMBOL_IBM, 30000, 140, false);

            env.AdvanceTime(35000);

            //These events are out of the window and new sums are generated
            SendEvent(env, SYMBOL_DELL, 10000, 90);
            AssertEvents(env, SYMBOL_DELL, 10000, 90, false);

            SendEvent(env, SYMBOL_IBM, 30000, 120);
            AssertEvents(env, SYMBOL_IBM, 30000, 120, false);

            SendEvent(env, SYMBOL_DELL, 20000, 90);
            AssertEvents(env, SYMBOL_DELL, 20000, 180, false);

            SendEvent(env, SYMBOL_IBM, 30000, 120);
            AssertEvents(env, SYMBOL_IBM, 30000, 240, false);
        }

        private static void TrySingleAssertion(RegressionEnvironment env)
        {
            AssertSelectResultType(env.Statement("s0"));

            env.AdvanceTime(0);

            SendEvent(env, SYMBOL_IBM, 20000, 52);
            AssertEvents(env, SYMBOL_IBM, 20000, 52, false);

            SendEvent(env, SYMBOL_IBM, 20000, 100);
            AssertEvents(env, SYMBOL_IBM, 20000, 152, false);

            env.AdvanceTime(35000);

            //These events are out of the window and new sums are generated
            SendEvent(env, SYMBOL_IBM, 20000, 252);
            AssertEvents(env, SYMBOL_IBM, 20000, 252, false);

            SendEvent(env, SYMBOL_IBM, 20000, 100);
            AssertEvents(env, SYMBOL_IBM, 20000, 352, false);
        }

        private static void TryTimeWindow(
            RegressionEnvironment env,
            string intervalSpec)
        {
            env.AdvanceTime(0);
            var epl = "@Name('s0') select irstream * from SupportBean#time(" + intervalSpec + ")";
            env.CompileDeploy(epl).AddListener("s0");

            SendEvent(env, "E1");
            env.Listener("s0").Reset();

            SendTimerAssertNotInvoked(env, 29999 * 1000);
            SendTimerAssertInvoked(env, 30000 * 1000);

            env.UndeployAll();
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double sum,
            bool unique)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            if (!unique) {
                Assert.IsNull(oldData);
            }

            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(volume, newData[0].Get("Volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));

            env.Listener("s0").Reset();
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

            Assert.IsFalse(env.Listener("s0").IsInvoked);
            Assert.IsFalse(env.Listener("s1").IsInvoked);

            env.AdvanceTime(4000);
            Assert.AreEqual("E1", env.Listener("s1").AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.AdvanceTime(5000);
            Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));
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
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendTimerAssertInvoked(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            env.Listener("s0").Reset();
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

        public class ViewTimeWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();

                env.AdvanceTime(0);
                var epl = "@Name('s0') select irstream * from SupportBean#time(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);
                env.AdvanceTime(1000);
                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                env.AdvanceTime(2000);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.AdvanceTime(3000);
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.AdvanceTime(11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                env.AdvanceTime(12000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}});
                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});
                SendSupportBean(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                env.AdvanceTime(13000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(8);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});
                env.AdvanceTime(22000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                env.Milestone(9);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new object[][] { });

                env.Milestone(10);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new object[][] { });

                env.UndeployAll();
            }
        }

        public class ViewTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                env.AdvanceTime(1000);
                var epl = "@Name('s0') select irstream * from SupportBean#time(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendSupportBean(env, "E3");
                SendSupportBean(env, "E4");

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                env.AdvanceTime(2000);
                SendSupportBean(env, "E5");

                env.Milestone(3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.AdvanceTime(10999);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.Milestone(4);

                env.AdvanceTime(11000);

                env.Milestone(5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E5"}});

                env.Milestone(6);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@Name('s0') select rstream * from SupportBean#time(1 month)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                SendCurrentTime(env, "2002-02-15T09:00:00.000");
                env.SendEventBean(new SupportBean("E2", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "TheString".SplitCsv(),
                    new object[] {"E1"});

                SendCurrentTimeWithMinus(env, "2002-03-15T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-15T09:00:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "TheString".SplitCsv(),
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }

        internal class ViewTimeSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum from SupportMarketDataBean#time(30)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                AssertSelectResultType(env.Statement("s0"));

                SendEvent(env, SYMBOL_DELL, 10000, 51);
                AssertEvents(env, SYMBOL_DELL, 10000, 51, false);

                SendEvent(env, SYMBOL_IBM, 20000, 52);
                AssertEvents(env, SYMBOL_IBM, 20000, 103, false);

                SendEvent(env, SYMBOL_DELL, 40000, 45);
                AssertEvents(env, SYMBOL_DELL, 40000, 148, false);

                env.AdvanceTime(35000);

                //These events are out of the window and new sums are generated

                SendEvent(env, SYMBOL_IBM, 30000, 70);
                AssertEvents(env, SYMBOL_IBM, 30000, 70, false);

                SendEvent(env, SYMBOL_DELL, 10000, 20);
                AssertEvents(env, SYMBOL_DELL, 10000, 90, false);

                env.UndeployAll();
            }
        }

        internal class ViewTimeSumGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#time(30) group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryGroupByAssertions(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeSumWFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl =
                    "@Name('s0') select Symbol, Volume, sum(Price) as mySum from SupportMarketDataBean(Symbol = 'IBM')#time(30)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TrySingleAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeJustSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@Name('s0') select irstream * from SupportMarketDataBean#time(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                string[] fields = {"Symbol"};

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E1"}}, null);

                env.Milestone(1);

                env.AdvanceTime(600);
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E2"}}, null);

                env.Milestone(2);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.Statement("s0").GetEnumerator());
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.AdvanceTime(1500);
                env.Listener("s0")
                    .AssertNewOldData(
                        null,
                        new[] {new object[] {"Symbol", "E1"}}); // olddata

                env.Milestone(3);

                env.AdvanceTime(1600);
                env.Listener("s0")
                    .AssertNewOldData(
                        null,
                        new[] {new object[] {"Symbol", "E2"}}); // olddata

                env.Milestone(4);

                env.AdvanceTime(2000);

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E3"}}, null);

                env.Milestone(6);

                env.UndeployAll();
            }
        }

        public class ViewTimeWindowWPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@Name('s0') select irstream Symbol, " +
                           "prev(1, Symbol) as prev1, " +
                           "prevtail(Symbol) as prevtail, " +
                           "prevcount(Symbol) as prevCountSym, " +
                           "prevwindow(Symbol) as prevWindowSym " +
                           "from  SupportMarketDataBean#time(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                string[] fields = {"Symbol", "prev1", "prevtail", "prevCountSym", "prevWindowSym"};

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.SendEventBean(MakeMarketDataEvent("E3"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E1", null, "E1", 1L,
                            new object[] {"E1"}
                        },
                        new object[] {
                            "E2", "E1", "E1", 2L,
                            new object[] {"E2", "E1"}
                        },
                        new object[] {
                            "E3", "E2", "E1", 3L,
                            new object[] {"E3", "E2", "E1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(1);

                env.AdvanceTime(1200);
                env.SendEventBean(MakeMarketDataEvent("E4"));
                env.SendEventBean(MakeMarketDataEvent("E5"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E4", "E3", "E1", 4L,
                            new object[] {"E4", "E3", "E2", "E1"}
                        },
                        new object[] {
                            "E5", "E4", "E1", 5L,
                            new object[] {"E5", "E4", "E3", "E2", "E1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(2);

                env.AdvanceTime(1600);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    "Symbol".SplitCsv(),
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                env.Listener("s0").Reset();

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E6"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E6"}}, null);

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowPreparedStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var text = "select rstream TheString from SupportBean#time(?::int)";
                var compiled = env.Compile(text);

                env.Deploy(
                    compiled,
                    new DeploymentOptions()
                        .WithStatementSubstitutionParameter(prepared => prepared.SetObject(1, 4))
                        .WithStatementNameRuntime(ctx => "s0"));
                env.Deploy(
                    compiled,
                    new DeploymentOptions()
                        .WithStatementSubstitutionParameter(prepared => prepared.SetObject(1, 3))
                        .WithStatementNameRuntime(ctx => "s1"));
                env.AddListener("s0").AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowVariableStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var text = "select rstream TheString from SupportBean#time(TIME_WIN_ONE)";
                env.CompileDeploy("@Name('s0') " + text).AddListener("s0");

                env.Runtime.VariableService.SetVariableValue(null, "TIME_WIN_ONE", 3);

                env.CompileDeploy("@Name('s1') " + text).AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@Name('s0') select rstream TheString from SupportBean#time(4 sec)";
                env.CompileDeploy(text).AddListener("s0");

                text = "@Name('s1') select rstream TheString from SupportBean#time(3000 milliseconds)";
                env.CompileDeploy(text).AddListener("s1").Milestone(0);

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowVariableTimePeriodStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO milliseconds)";
                env.CompileDeploy("@Name('s0')" + text).AddListener("s0");

                text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO minutes)";
                env.Runtime.VariableService.SetVariableValue(null, "TIME_WIN_TWO", 0.05);
                env.CompileDeploy("@Name('s1')" + text).AddListener("s1");

                RunAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ViewTimeWindowTimePeriodParams : RegressionExecution
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
            private readonly long flipTime;
            private readonly string size;

            private readonly long startTime;

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
                var epl = "@Name('s0') select * from SupportBean#time(" + size + ")";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.AdvanceTime(flipTime - 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.AdvanceTime(flipTime);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace