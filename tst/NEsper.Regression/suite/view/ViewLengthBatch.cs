///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewLengthBatch
    {
        public enum ViewLengthBatchNormalRunType
        {
            VIEW,
            GROUPWIN,
            NAMEDWINDOW
        }

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSize2(execs);
            WithSize1(execs);
            WithSize3(execs);
            WithInvalid(execs);
            WithNormal(execs);
            WithPrev(execs);
            WithDelete(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithNormal(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchNormal(ViewLengthBatchNormalRunType.VIEW, null));
            execs.Add(new ViewLengthBatchNormal(ViewLengthBatchNormalRunType.NAMEDWINDOW, null));
            execs.Add(new ViewLengthBatchNormal(ViewLengthBatchNormalRunType.GROUPWIN, null));
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSize3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchSize3());
            return execs;
        }

        public static IList<RegressionExecution> WithSize1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchSize1());
            return execs;
        }

        public static IList<RegressionExecution> WithSize2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchSize2());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthBatchSceneOne());
            return execs;
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string e3)
        {
            env.SendEventBean(new SupportBean_A(e3));
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string e1)
        {
            env.SendEventBean(new SupportBean(e1, 0));
        }

        private static void SendEvent(
            SupportBean theEvent,
            RegressionEnvironment env)
        {
            env.SendEventBean(theEvent);
        }

        private static SupportBean[] Get10Events()
        {
            var events = new SupportBean[10];
            for (var i = 0; i < events.Length; i++) {
                events[i] = new SupportBean();
            }

            return events;
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        internal class ViewLengthBatchSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from SupportMarketDataBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                var newEvents = env.Listener("s0").NewDataListFlattened;
                EPAssertionUtil.AssertPropsPerRow(
                    newEvents,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"},
                        new object[] {"E3"}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4"));

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("E5"));

                env.Milestone(5);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.SendEventBean(MakeMarketDataEvent("E6"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"},
                        new object[] {"E6"}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"},
                        new object[] {"E3"}
                    });
                env.Listener("s0").Reset();

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("E7"));

                env.Milestone(7);

                env.SendEventBean(MakeMarketDataEvent("E8"));

                env.Milestone(8);

                env.SendEventBean(MakeMarketDataEvent("E9"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E7"},
                        new object[] {"E8"},
                        new object[] {"E9"}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new[] {"Symbol"},
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"},
                        new object[] {"E6"}
                    });
                env.Listener("s0").Reset();

                env.Milestone(9);

                env.SendEventBean(MakeMarketDataEvent("E10"));

                env.Milestone(10);

                env.UndeployAll();
            }
        }

        internal class ViewLengthBatchSize2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#length_batch(2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[0]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[1], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[0], events[1]},
                    null);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                SendEvent(events[2], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[2]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[3], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[2], events[3]},
                    new[] {events[0], events[1]});
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                SendEvent(events[4], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[4]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[5], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[4], events[5]},
                    new[] {events[2], events[3]});
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                env.UndeployAll();
            }
        }

        internal class ViewLengthBatchSize1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#length_batch(1)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[0]},
                    null);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                SendEvent(events[1], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[1]},
                    new[] {events[0]});
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                SendEvent(events[2], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[2]},
                    new[] {events[1]});
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                env.UndeployAll();
            }
        }

        internal class ViewLengthBatchSize3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[0]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[1], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[0], events[1]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[2], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[0], events[1], events[2]},
                    null);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                SendEvent(events[3], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[3]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[4], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {events[3], events[4]},
                    env.Statement("s0").GetEnumerator());

                SendEvent(events[5], env);
                EPAssertionUtil.AssertUnderlyingPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    new[] {events[3], events[4], events[5]},
                    new[] {events[0], events[1], events[2]});
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, env.Statement("s0").GetEnumerator());

                env.UndeployAll();
            }
        }

        internal class ViewLengthBatchInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean#length_batch(0)",
                    "Failed to validate data window declaration: Error in view 'length_batch', Length-Batch view requires a positive integer for size but received 0");
            }
        }

        public class ViewLengthBatchPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream *, " +
                           "prev(1, Symbol) as prev1, " +
                           "prevtail(0, Symbol) as prevTail0, " +
                           "prevtail(1, Symbol) as prevTail1, " +
                           "prevcount(Symbol) as prevCountSym, " +
                           "prevwindow(Symbol) as prevWindowSym " +
                           "from SupportMarketDataBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                string[] fields = {"Symbol", "prev1", "prevTail0", "prevTail1", "prevCountSym", "prevWindowSym"};
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                var newEvents = env.Listener("s0").NewDataListFlattened;
                object[] win = {"E3", "E2", "E1"};
                EPAssertionUtil.AssertPropsPerRow(
                    newEvents,
                    fields,
                    new[] {
                        new object[] {"E1", null, "E1", "E2", 3L, win},
                        new object[] {"E2", "E1", "E1", "E2", 3L, win},
                        new object[] {"E3", "E2", "E1", "E2", 3L, win}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewLengthBatchDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                var epl = "create window ABCWin#length_batch(3) as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "on SupportBean_A delete from ABCWin where TheString = Id;\n" +
                          "@Name('s0') select irstream * from ABCWin;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1"}
                    });

                SendSupportBean_A(env, "E1"); // delete
                Assert.IsFalse(env.Listener("s0").IsInvoked); // batch is quiet-delete

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                SendSupportBean(env, "E2");
                SendSupportBean(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E2"},
                        new object[] {"E3"}
                    });
                SendSupportBean_A(env, "E3"); // delete
                Assert.IsFalse(env.Listener("s0").IsInvoked); // batch is quiet-delete

                env.Milestone(4);

                SendSupportBean(env, "E4");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E5");
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2"},
                        new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.Milestone(5);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                SendSupportBean(env, "E6");
                SendSupportBean(env, "E7");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E6"},
                        new object[] {"E7"}
                    });

                SendSupportBean(env, "E8");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetIRPair(),
                    fields,
                    new[] {
                        new object[] {"E6"},
                        new object[] {"E7"},
                        new object[] {"E8"}
                    },
                    new[] {
                        new object[] {"E2"},
                        new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.UndeployAll();
            }
        }

        public class ViewLengthBatchNormal : RegressionExecution
        {
            private readonly string optionalDatawindow;
            private readonly ViewLengthBatchNormalRunType runType;

            public ViewLengthBatchNormal(
                ViewLengthBatchNormalRunType runType,
                string optionalDatawindow)
            {
                this.runType = runType;
                this.optionalDatawindow = optionalDatawindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                string epl;
                if (runType == ViewLengthBatchNormalRunType.VIEW) {
                    epl = "@Name('s0') select irstream TheString, prev(1, TheString) as prevString " +
                          "from SupportBean" +
                          (optionalDatawindow == null ? "#length_batch(3)" : optionalDatawindow);
                }
                else if (runType == ViewLengthBatchNormalRunType.GROUPWIN) {
                    epl = "@Name('s0') select irstream * from SupportBean#groupwin(DoubleBoxed)#length_batch(3)";
                }
                else if (runType == ViewLengthBatchNormalRunType.NAMEDWINDOW) {
                    epl = "create window ABCWin#length_batch(3) as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "@Name('s0') select irstream * from ABCWin;\n";
                }
                else {
                    throw new EPException("Unrecognized variant " + runType);
                }

                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1"}
                    });

                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"}
                    });

                SendSupportBean(env, "E3");
                Assert.IsNull(env.Listener("s0").LastOldData);
                if (runType == ViewLengthBatchNormalRunType.VIEW) {
                    EPAssertionUtil.AssertPropsPerRow(
                        env.Listener("s0").LastNewData,
                        new[] {"prevString"},
                        new[] {
                            new object[] {null},
                            new object[] {"E1"},
                            new object[] {"E2"}
                        });
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"},
                        new object[] {"E3"}
                    });

                env.Milestone(4);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                SendSupportBean(env, "E4");
                SendSupportBean(env, "E5");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"}
                    });

                SendSupportBean(env, "E6");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetIRPair(),
                    fields,
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"},
                        new object[] {"E6"}
                    },
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"},
                        new object[] {"E3"}
                    });

                env.Milestone(6);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                env.Milestone(7);

                SendSupportBean(env, "E7");
                SendSupportBean(env, "E8");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "E9");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetIRPair(),
                    fields,
                    new[] {
                        new object[] {"E7"},
                        new object[] {"E8"},
                        new object[] {"E9"}
                    },
                    new[] {
                        new object[] {"E4"},
                        new object[] {"E5"},
                        new object[] {"E6"}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace