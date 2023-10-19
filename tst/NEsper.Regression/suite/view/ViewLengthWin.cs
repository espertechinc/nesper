///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps; // assertEquals

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewLengthWin
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithdowSceneOne(execs);
            WithdowWPrevPrior(execs);
            WithWPropertyDetail(execs);
            WithdowIterator(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithdowIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthWindowIterator());
            return execs;
        }

        public static IList<RegressionExecution> WithWPropertyDetail(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthWinWPropertyDetail());
            return execs;
        }

        public static IList<RegressionExecution> WithdowWPrevPrior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthWindowWPrevPrior());
            return execs;
        }

        public static IList<RegressionExecution> WithdowSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLengthWindowSceneOne());
            return execs;
        }

        public class ViewLengthWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString".SplitCsv();

                env.Milestone(0);

                var epl = "@name('s0') select irstream * from SupportBean#length(2)";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(2);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                SendSupportBean(env, "E3");
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E1" });

                env.Milestone(4);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(5);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                SendSupportBean(env, "E4");
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E2" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        public class ViewLengthWindowWPrevPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream symbol, " +
                           "prev(1, symbol) as prev1, " +
                           "prior(1, symbol) as prio1, " +
                           "prevtail(symbol) as prevtail0, " +
                           "prevcount(symbol) as prevCountSym, " +
                           "prevwindow(symbol) as prevWindowSym " +
                           "from SupportMarketDataBean.win:length(2)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                var fields = new string[] { "symbol", "prev1", "prio1", "prevtail0", "prevCountSym", "prevWindowSym" };

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", null, null, "E1", 1L, new object[] { "E1" } });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "E2", "E1", "E1", "E1", 2L, new object[] { "E2", "E1" } });

                env.Milestone(2);

                for (var i = 3; i < 10; i++) {
                    env.SendEventBean(MakeMarketDataEvent("E" + i));

                    env.AssertPropsNV(
                        "s0",
                        new object[][] {
                            new object[] { "symbol", "E" + i }, new object[] { "prev1", "E" + (i - 1) },
                            new object[] { "prio1", "E" + (i - 1) }, new object[] { "prevtail0", "E" + (i - 1) }
                        }, // new data
                        new object[][] {
                            new object[] { "symbol", "E" + (i - 2) }, new object[] { "prev1", null },
                            new object[] { "prevtail0", null }
                        } //  old data
                    );

                    env.Milestone(i);
                }

                // Lets try the iterator
                env.AssertIterator(
                    "s0",
                    events => {
                        for (var i = 8; i < 10; i++) {
                            var @event = events.Advance();
                            Assert.AreEqual("E" + i, @event.Get("symbol"));
                        }
                    });
                env.UndeployAll();
            }
        }

        private class ViewLengthWinWPropertyDetail : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select mapped('keyOne') as a," +
                          "indexed[1] as b, nested.nestedNested.nestedNestedValue as c, mapProperty, " +
                          "arrayProperty[0] " +
                          "  from SupportBeanComplexProps#length(3) " +
                          " where mapped('keyOne') = 'valueOne' and " +
                          " indexed[1] = 2 and " +
                          " nested.nestedNested.nestedNestedValue = 'nestedNestedValue'";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var eventObject = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(eventObject);
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(eventObject.GetMapped("keyOne"), theEvent.Get("a"));
                        Assert.AreEqual(eventObject.GetIndexed(1), theEvent.Get("b"));
                        Assert.AreEqual(eventObject.Nested.NestedNested.NestedNestedValue, theEvent.Get("c"));
                        Assert.AreEqual(eventObject.MapProperty, theEvent.Get("mapProperty"));
                        Assert.AreEqual(eventObject.ArrayProperty[0], theEvent.Get("arrayProperty[0]"));
                    });

                env.Milestone(1);

                eventObject.SetIndexed(1, int.MinValue);
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(eventObject);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                eventObject.SetIndexed(1, 2);
                env.SendEventBean(eventObject);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ViewLengthWindowIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "symbol,price".SplitCsv();
                var epl = "@name('s0') select symbol, price from SupportMarketDataBean#length(2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "ABC", 20);
                SendEvent(env, "DEF", 100);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "ABC", 20d }, new object[] { "DEF", 100d } });

                SendEvent(env, "EFG", 50);

                env.Milestone(1);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "DEF", 100d }, new object[] { "EFG", 50d }, });

                env.UndeployAll();
            }
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, "feed1");
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace