///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewLengthBatch
    {
        public static ICollection<RegressionExecution> Executions()
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

        private class ViewLengthBatchSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "symbol".SplitCsv();
                var text = "@name('s0') select irstream * from SupportMarketDataBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } },
                    null);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4"));

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("E5"));

                env.Milestone(5);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "symbol" },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.SendEventBean(MakeMarketDataEvent("E6"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("E7"));

                env.Milestone(7);

                env.SendEventBean(MakeMarketDataEvent("E8"));

                env.Milestone(8);

                env.SendEventBean(MakeMarketDataEvent("E9"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" }, new object[] { "E8" }, new object[] { "E9" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.Milestone(9);

                env.SendEventBean(MakeMarketDataEvent("E10"));

                env.Milestone(10);

                env.UndeployAll();
            }
        }

        private class ViewLengthBatchSize2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportBean#length_batch(2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[0] });

                SendEvent(events[1], env);
                AssertUnderlyingPerRow(env, new SupportBean[] { events[0], events[1] }, null);
                AssertUnderlyingIterator(env, null);

                SendEvent(events[2], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[2] });

                SendEvent(events[3], env);
                AssertUnderlyingPerRow(
                    env,
                    new SupportBean[] { events[2], events[3] },
                    new SupportBean[] { events[0], events[1] });
                AssertUnderlyingIterator(env, null);

                SendEvent(events[4], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[4] });

                SendEvent(events[5], env);
                AssertUnderlyingPerRow(
                    env,
                    new SupportBean[] { events[4], events[5] },
                    new SupportBean[] { events[2], events[3] });
                AssertUnderlyingIterator(env, null);

                env.UndeployAll();
            }
        }

        private class ViewLengthBatchSize1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportBean#length_batch(1)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                AssertUnderlyingPerRow(env, new SupportBean[] { events[0] }, null);
                AssertUnderlyingIterator(env, null);

                SendEvent(events[1], env);
                AssertUnderlyingPerRow(env, new SupportBean[] { events[1] }, new SupportBean[] { events[0] });
                AssertUnderlyingIterator(env, null);

                SendEvent(events[2], env);
                AssertUnderlyingPerRow(env, new SupportBean[] { events[2] }, new SupportBean[] { events[1] });
                AssertUnderlyingIterator(env, null);

                env.UndeployAll();
            }
        }

        private class ViewLengthBatchSize3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var events = Get10Events();

                SendEvent(events[0], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[0] });

                SendEvent(events[1], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[0], events[1] });

                SendEvent(events[2], env);
                AssertUnderlyingPerRow(env, new SupportBean[] { events[0], events[1], events[2] }, null);
                AssertUnderlyingIterator(env, null);

                SendEvent(events[3], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[3] });

                SendEvent(events[4], env);
                env.AssertListenerNotInvoked("s0");
                AssertUnderlyingIterator(env, new SupportBean[] { events[3], events[4] });

                SendEvent(events[5], env);
                AssertUnderlyingPerRow(
                    env,
                    new SupportBean[] { events[3], events[4], events[5] },
                    new SupportBean[] { events[0], events[1], events[2] });
                AssertUnderlyingIterator(env, null);

                env.UndeployAll();
            }
        }

        private class ViewLengthBatchInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportMarketDataBean#length_batch(0)",
                    "Failed to validate data window declaration: Error in view 'length_batch', Length-Batch view requires a positive integer for size but received 0");
            }
        }

        public class ViewLengthBatchPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream *, " +
                           "prev(1, symbol) as prev1, " +
                           "prevtail(0, symbol) as prevTail0, " +
                           "prevtail(1, symbol) as prevTail1, " +
                           "prevcount(symbol) as prevCountSym, " +
                           "prevwindow(symbol) as prevWindowSym " +
                           "from SupportMarketDataBean#length_batch(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                var fields = new string[]
                    { "symbol", "prev1", "prevTail0", "prevTail1", "prevCountSym", "prevWindowSym" };
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.NewDataListFlattened;
                        var win = new object[] { "E3", "E2", "E1" };
                        EPAssertionUtil.AssertPropsPerRow(
                            newEvents,
                            fields,
                            new object[][] {
                                new object[] { "E1", null, "E1", "E2", 3L, win },
                                new object[] { "E2", "E1", "E1", "E2", 3L, win },
                                new object[] { "E3", "E2", "E1", "E2", 3L, win }
                            });
                        Assert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        public class ViewLengthBatchDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString".SplitCsv();

                var epl = "create window ABCWin#length_batch(3) as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "on SupportBean_A delete from ABCWin where theString = id;\n" +
                          "@name('s0') select irstream * from ABCWin;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                SendSupportBean_A(env, "E1"); // delete
                env.AssertListenerNotInvoked("s0"); // batch is quiet-delete

                env.Milestone(2);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                SendSupportBean(env, "E2");
                SendSupportBean(env, "E3");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                SendSupportBean_A(env, "E3"); // delete
                env.AssertListenerNotInvoked("s0"); // batch is quiet-delete

                env.Milestone(4);

                SendSupportBean(env, "E4");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E5");
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E4" }, new object[] { "E5" } },
                    null);

                env.Milestone(5);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                SendSupportBean(env, "E6");
                SendSupportBean(env, "E7");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" }, new object[] { "E7" } });

                SendSupportBean(env, "E8");
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" } },
                    new object[][] { new object[] { "E2" }, new object[] { "E4" }, new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        public class ViewLengthBatchNormal : RegressionExecution
        {
            private readonly ViewLengthBatchNormalRunType runType;
            private readonly string optionalDatawindow;

            public ViewLengthBatchNormal(
                ViewLengthBatchNormalRunType runType,
                string optionalDatawindow)
            {
                this.runType = runType;
                this.optionalDatawindow = optionalDatawindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "theString".SplitCsv();

                string epl;
                if (runType == ViewLengthBatchNormalRunType.VIEW) {
                    epl = "@name('s0') select irstream theString, prev(1, theString) as prevString " +
                          "from SupportBean" +
                          (optionalDatawindow == null ? "#length_batch(3)" : optionalDatawindow);
                }
                else if (runType == ViewLengthBatchNormalRunType.GROUPWIN) {
                    epl = "@name('s0') select irstream * from SupportBean#groupwin(doubleBoxed)#length_batch(3)";
                }
                else if (runType == ViewLengthBatchNormalRunType.NAMEDWINDOW) {
                    epl = "create window ABCWin#length_batch(3) as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "@name('s0') select irstream * from ABCWin;\n";
                }
                else {
                    throw new EPRuntimeException("Unrecognized variant " + runType);
                }

                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);
                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                SendSupportBean(env, "E2");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                SendSupportBean(env, "E3");
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastOldData);
                        if (runType == ViewLengthBatchNormalRunType.VIEW) {
                            EPAssertionUtil.AssertPropsPerRow(
                                listener.LastNewData,
                                "prevString".SplitCsv(),
                                new object[][] { new object[] { null }, new object[] { "E1" }, new object[] { "E2" } });
                        }

                        EPAssertionUtil.AssertPropsPerRow(
                            listener.GetAndResetLastNewData(),
                            fields,
                            new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
                    });

                env.Milestone(4);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                SendSupportBean(env, "E4");
                SendSupportBean(env, "E5");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                SendSupportBean(env, "E6");

                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(6);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                env.Milestone(7);

                SendSupportBean(env, "E7");
                SendSupportBean(env, "E8");
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, "E9");
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" }, new object[] { "E8" }, new object[] { "E9" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "runType=" +
                       runType +
                       ", optionalDatawindow='" +
                       optionalDatawindow +
                       '\'' +
                       '}';
            }
        }

        public enum ViewLengthBatchNormalRunType
        {
            VIEW,
            GROUPWIN,
            NAMEDWINDOW
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

        private static void AssertUnderlyingIterator(
            RegressionEnvironment env,
            SupportBean[] supportBeans)
        {
            env.AssertIterator(
                "s0",
                iterator => { EPAssertionUtil.AssertEqualsExactOrderUnderlying(supportBeans, iterator); });
        }

        private static void AssertUnderlyingPerRow(
            RegressionEnvironment env,
            SupportBean[] newData,
            SupportBean[] oldData)
        {
            env.AssertListener(
                "s0",
                listener => {
                    EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(), newData, oldData);
                });
        }
    }
} // end of namespace