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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewLengthWin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
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

        public class ViewLengthWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                env.Milestone(0);

                var epl = "@Name('s0') select irstream * from SupportBean#length(2)";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});

                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E3"},
                    new object[] {"E1"});

                env.Milestone(4);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E4"},
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});

                env.UndeployAll();
            }
        }

        public class ViewLengthWindowWPrevPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream Symbol, " +
                           "prev(1, Symbol) as prev1, " +
                           "prior(1, Symbol) as prio1, " +
                           "prevtail(Symbol) as prevtail0, " +
                           "prevcount(Symbol) as prevCountSym, " +
                           "prevwindow(Symbol) as prevWindowSym " +
                           "from SupportMarketDataBean.win:length(2)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                string[] fields = {"Symbol", "prev1", "prio1", "prevtail0", "prevCountSym", "prevWindowSym"};

                env.SendEventBean(MakeMarketDataEvent("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "E1", null, null, "E1", 1L,
                        new object[] {"E1"}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "E2", "E1", "E1", "E1", 2L,
                        new object[] {"E2", "E1"}
                    });

                env.Milestone(2);

                for (var i = 3; i < 10; i++) {
                    env.SendEventBean(MakeMarketDataEvent("E" + i));

                    env.Listener("s0")
                        .AssertNewOldData(
                            new[] {
                                new object[] {"Symbol", "E" + i}, new object[] {"prev1", "E" + (i - 1)},
                                new object[] {"prio1", "E" + (i - 1)}, new object[] {"prevtail0", "E" + (i - 1)}
                            }, // new data
                            new[] {
                                new object[] {"Symbol", "E" + (i - 2)}, new object[] {"prev1", null},
                                new object[] {"prevtail0", null}
                            } //  old data
                        );

                    env.Milestone(i);
                }

                // Lets try the iterator
                var events = env.GetEnumerator("s0");
                for (var i = 8; i < 10; i++) {
                    var @event = events.Advance();
                    Assert.AreEqual("E" + i, @event.Get("Symbol"));
                }

                env.UndeployAll();
            }
        }

        internal class ViewLengthWinWPropertyDetail : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "Mapped('keyOne') as a," +
                          "Indexed[1] as b, " +
                          "Nested.NestedNested.NestedNestedValue as c, " +
                          "MapProperty, " +
                          "ArrayProperty[0] " +
                          "  from SupportBeanComplexProps#length(3) " +
                          " where Mapped('keyOne') = 'valueOne' and " +
                          " Indexed[1] = 2 and " +
                          " Nested.NestedNested.NestedNestedValue = 'NestedNestedValue'";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var eventObject = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(eventObject);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(eventObject.GetMapped("keyOne"), theEvent.Get("a"));
                Assert.AreEqual(eventObject.GetIndexed(1), theEvent.Get("b"));
                Assert.AreEqual(eventObject.Nested.NestedNested.NestedNestedValue, theEvent.Get("c"));
                Assert.AreEqual(eventObject.MapProperty, theEvent.Get("MapProperty"));
                Assert.AreEqual(eventObject.ArrayProperty[0], theEvent.Get("ArrayProperty[0]"));

                eventObject.SetIndexed(1, int.MinValue);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(eventObject);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                eventObject.SetIndexed(1, 2);
                env.SendEventBean(eventObject);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewLengthWindowIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Price from SupportMarketDataBean#length(2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "ABC", 20);
                SendEvent(env, "DEF", 100);

                // check iterator results
                var events = env.Statement("s0").GetEnumerator();
                var theEvent = events.Advance();
                Assert.AreEqual("ABC", theEvent.Get("Symbol"));
                Assert.AreEqual(20d, theEvent.Get("Price"));

                theEvent = events.Advance();
                Assert.AreEqual("DEF", theEvent.Get("Symbol"));
                Assert.AreEqual(100d, theEvent.Get("Price"));
                Assert.IsFalse(events.MoveNext());

                SendEvent(env, "EFG", 50);

                // check iterator results
                events = env.Statement("s0").GetEnumerator();
                theEvent = events.Advance();
                Assert.AreEqual("DEF", theEvent.Get("Symbol"));
                Assert.AreEqual(100d, theEvent.Get("Price"));

                theEvent = events.Advance();
                Assert.AreEqual("EFG", theEvent.Get("Symbol"));
                Assert.AreEqual(50d, theEvent.Get("Price"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace