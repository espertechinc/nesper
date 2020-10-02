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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewExternallyTimedWin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithWindowSceneOne(execs);
            WithBatchSceneTwo(execs);
            WithWinSceneShort(execs);
            WithTimedMonthScoped(execs);
            WithWindowPrev(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWindowPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedWindowPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithTimedMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedTimedMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithWinSceneShort(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedWinSceneShort());
            return execs;
        }

        public static IList<RegressionExecution> WithBatchSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedWindowSceneOne());
            return execs;
        }

        private static void SendExtTimeEvent(
            RegressionEnvironment env,
            long longPrimitive)
        {
            var theEvent = new SupportBean(null, 0);
            theEvent.LongPrimitive = longPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void SendExtTimeEvent(
            RegressionEnvironment env,
            long longPrimitive,
            string name)
        {
            var theEvent = new SupportBean(name, 0);
            theEvent.LongPrimitive = longPrimitive;
            env.SendEventBean(theEvent);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            long volume)
        {
            return new SupportMarketDataBean(symbol, 0, volume, null);
        }

        private static void SendSupportBeanWLong(
            RegressionEnvironment env,
            string @string,
            long longPrimitive)
        {
            var sb = new SupportBean(@string, 0);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
        }

        public class ViewExternallyTimedWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"Symbol"};
                var text = "@Name('s0') select irstream * from  SupportMarketDataBean#ext_timed(Volume, 1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1", 500));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E1"}}, null);
                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2", 600));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E2"}}, null);
                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3", 1500));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Symbol", "E3"}},
                        new[] {new object[] {"Symbol", "E1"}});

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4", 1600));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Symbol", "E4"}},
                        new[] {new object[] {"Symbol", "E2"}});

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("E5", 1700));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E5"}}, null);

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("E6", 1800));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E6"}}, null);

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("E7", 1900));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E7"}}, null);

                env.Milestone(7);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    fields,
                    new[] {
                        new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"},
                        new object[] {"E7"}
                    });

                env.SendEventBean(MakeMarketDataEvent("E8", 2700));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E8"}});
                env.Listener("s0").Reset();

                env.Milestone(8);

                env.SendEventBean(MakeMarketDataEvent("E9", 3700));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    fields,
                    new[] {new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E9"}});
                env.Listener("s0").Reset();

                env.Milestone(9);

                env.UndeployAll();
            }
        }

        public class ViewExternallyTimedBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0"};
                var epl =
                    "@Name('s0') select irstream TheString as c0 from SupportBean#ext_timed(LongPrimitive, 10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBeanWLong(env, "E1", 1000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                SendSupportBeanWLong(env, "E2", 5000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendSupportBeanWLong(env, "E3", 11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E3"},
                    new object[] {"E1"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                SendSupportBeanWLong(env, "E4", 14000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                env.Milestone(4);
                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
                SendSupportBeanWLong(env, "E5", 21000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                SendSupportBeanWLong(env, "E6", 24000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E4"}});

                env.UndeployAll();
            }
        }

        internal class ViewExternallyTimedWinSceneShort : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#ext_timed(LongPrimitive, 10 minutes)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendExtTimeEvent(env, 0);

                SendExtTimeEvent(env, 10 * 60 * 1000 - 1);
                Assert.IsNull(env.Listener("s0").OldDataList[0]);
                env.Listener("s0").Reset();

                SendExtTimeEvent(env, 10 * 60 * 1000 + 1);
                Assert.AreEqual(1, env.Listener("s0").OldDataList[0].Length);

                env.UndeployAll();
            }
        }

        internal class ViewExternallyTimedTimedMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select rstream * from SupportBean#ext_timed(LongPrimitive, 1 month)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-02-01T09:00:00.000"), "E1");
                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-03-01T09:00:00.000") - 1, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-03-01T09:00:00.000"), "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString"},
                    new object[] {"E1"});

                env.UndeployAll();
            }
        }

        public class ViewExternallyTimedWindowPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream Symbol," +
                           "prev(1, Symbol) as prev1, " +
                           "prevtail(0, Symbol) as prevTail0, " +
                           "prevtail(1, Symbol) as prevTail1, " +
                           "prevcount(Symbol) as prevCountSym, " +
                           "prevwindow(Symbol) as prevWindowSym " +
                           "from SupportMarketDataBean#ext_timed(Volume, 1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                string[] fields = {"Symbol", "prev1", "prevTail0", "prevTail1", "prevCountSym", "prevWindowSym"};

                env.SendEventBean(MakeMarketDataEvent("E1", 500));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E1", null, "E1", null, 1L,
                            new object[] {"E1"}
                        }
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2", 600));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E2", "E1", "E1", "E2", 2L,
                            new object[] {"E2", "E1"}
                        }
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3", 1500));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E3", "E2", "E2", "E3", 2L,
                            new object[] {"E3", "E2"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4", 1600));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "E4", "E3", "E3", "E4", 2L,
                            new object[] {"E4", "E3"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(4);

                env.UndeployAll();
            }
        }
    }
} // end of namespace