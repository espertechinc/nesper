///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeIterator
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypePatternNoWindow());
            execs.Add(new ResultSetQueryTypePatternWithWindow());
            execs.Add(new ResultSetQueryTypeOrderByWildcard());
            execs.Add(new ResultSetQueryTypeOrderByProps());
            execs.Add(new ResultSetQueryTypeFilter());
            execs.Add(new ResultSetQueryTypeRowPerGroupOrdered());
            execs.Add(new ResultSetQueryTypeRowPerGroup());
            execs.Add(new ResultSetQueryTypeRowPerGroupHaving());
            execs.Add(new ResultSetQueryTypeRowPerGroupComplex());
            execs.Add(new ResultSetQueryTypeAggregateGroupedOrdered());
            execs.Add(new ResultSetQueryTypeAggregateGrouped());
            execs.Add(new ResultSetQueryTypeAggregateGroupedHaving());
            execs.Add(new ResultSetQueryTypeRowPerEvent());
            execs.Add(new ResultSetQueryTypeRowPerEventOrdered());
            execs.Add(new ResultSetQueryTypeRowPerEventHaving());
            execs.Add(new ResultSetQueryTypeRowForAll());
            execs.Add(new ResultSetQueryTypeRowForAllHaving());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean(symbol, price, volume, null));
        }

        private static SupportMarketDataBean SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean("SYM", 0, volume, null));
        }

        internal class ResultSetQueryTypePatternNoWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-115
                var epl = "@Name('s0') @IterableUnbound select * from pattern " +
                          "[every ( addressInfo = SupportBean(TheString='address') " +
                          "-> txnWD = SupportBean(TheString='txn') ) ] " +
                          "where addressInfo.IntBoxed = txnWD.IntBoxed";
                env.CompileDeploy(epl).AddListener("s0");

                var myEventBean1 = new SupportBean();
                myEventBean1.TheString = "address";
                myEventBean1.IntBoxed = 9001;
                env.SendEventBean(myEventBean1);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                var myEventBean2 = new SupportBean();
                myEventBean2.TheString = "txn";
                myEventBean2.IntBoxed = 9001;
                env.SendEventBean(myEventBean2);
                Assert.IsTrue(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                var theEvent = env.Statement("s0").First();
                Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
                Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypePatternWithWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern " +
                          "[every ( addressInfo = SupportBean(TheString='address') " +
                          "-> txnWD = SupportBean(TheString='txn') ) ]#lastevent " +
                          "where addressInfo.IntBoxed = txnWD.IntBoxed";
                env.CompileDeploy(epl).AddListener("s0");

                var myEventBean1 = new SupportBean();
                myEventBean1.TheString = "address";
                myEventBean1.IntBoxed = 9001;
                env.SendEventBean(myEventBean1);

                var myEventBean2 = new SupportBean();
                myEventBean2.TheString = "txn";
                myEventBean2.IntBoxed = 9001;
                env.SendEventBean(myEventBean2);

                env.Milestone(0);

                var theEvent = env.Statement("s0").First();
                Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
                Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOrderByWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportMarketDataBean#length(5) order by Symbol, Volume";
                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                object eventOne = SendEvent(env, "SYM", 1);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(new[] {eventOne}, env.Statement("s0").GetEnumerator());

                object eventTwo = SendEvent(env, "OCC", 2);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {eventTwo, eventOne},
                    env.Statement("s0").GetEnumerator());

                object eventThree = SendEvent(env, "TOC", 3);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {eventTwo, eventOne, eventThree},
                    env.Statement("s0").GetEnumerator());

                object eventFour = SendEvent(env, "SYM", 0);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {eventTwo, eventFour, eventOne, eventThree},
                    env.Statement("s0").GetEnumerator());

                object eventFive = SendEvent(env, "SYM", 10);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {eventTwo, eventFour, eventOne, eventFive, eventThree},
                    env.Statement("s0").GetEnumerator());

                object eventSix = SendEvent(env, "SYM", 4);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new[] {eventTwo, eventFour, eventSix, eventFive, eventThree},
                    env.Statement("s0").GetEnumerator());

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOrderByProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "Volume"};
                var stmtText =
                    "@Name('s0') select Symbol, Volume from SupportMarketDataBean#length(3) order by Symbol, Volume";
                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 1L}});

                SendEvent(env, "OCC", 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 2L}, new object[] {"SYM", 1L}});

                SendEvent(env, "SYM", 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 2L}, new object[] {"SYM", 0L}, new object[] {"SYM", 1L}});

                SendEvent(env, "OCC", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 2L}, new object[] {"OCC", 3L}, new object[] {"SYM", 0L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "vol"};
                var stmtText = "@Name('s0') select Symbol, Volume * 10 as vol from SupportMarketDataBean#length(5)" +
                               " where Volume < 0";
                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), fields, null);

                SendEvent(env, "SYM", -1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -10L}});

                SendEvent(env, "SYM", -6);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -10L}, new object[] {"SYM", -60L}});

                env.Milestone(0);

                SendEvent(env, "SYM", 1);
                SendEvent(env, "SYM", 16);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -10L}, new object[] {"SYM", -60L}});

                SendEvent(env, "SYM", -9);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -10L}, new object[] {"SYM", -60L}, new object[] {"SYM", -90L}});

                env.Milestone(1);

                SendEvent(env, "SYM", 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -60L}, new object[] {"SYM", -90L}});

                SendEvent(env, "SYM", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -90L}});

                env.Milestone(2);

                SendEvent(env, "SYM", 4);
                SendEvent(env, "SYM", 5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -90L}});

                env.Milestone(3);

                SendEvent(env, "SYM", 6);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol " +
                               "order by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                SendEvent(env, "SYM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 100L}});

                env.Milestone(1);

                SendEvent(env, "OCC", 5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 5L}, new object[] {"SYM", 100L}});

                SendEvent(env, "SYM", 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 5L}, new object[] {"SYM", 110L}});

                SendEvent(env, "OCC", 6);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"OCC", 11L}, new object[] {"SYM", 110L}});

                env.Milestone(2);

                SendEvent(env, "ATB", 8);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"ATB", 8L}, new object[] {"OCC", 11L}, new object[] {"SYM", 110L}});

                env.Milestone(3);

                SendEvent(env, "ATB", 7);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"ATB", 15L}, new object[] {"OCC", 11L}, new object[] {"SYM", 10L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 100L}});

                SendEvent(env, "SYM", 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 110L}});

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 110L}, new object[] {"TAC", 1L}});

                SendEvent(env, "SYM", 11);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 121L}, new object[] {"TAC", 1L}});

                env.Milestone(1);

                SendEvent(env, "TAC", 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 121L}, new object[] {"TAC", 3L}});

                SendEvent(env, "OCC", 55);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 21L}, new object[] {"TAC", 3L}, new object[] {"OCC", 55L}});

                env.Milestone(2);

                SendEvent(env, "OCC", 4);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"TAC", 3L}, new object[] {"SYM", 11L}, new object[] {"OCC", 59L}});

                SendEvent(env, "OCC", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 11L}, new object[] {"TAC", 2L}, new object[] {"OCC", 62L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol having sum(Volume) > 10";

                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 100L}});

                SendEvent(env, "SYM", 5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 105L}});

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 105L}});

                SendEvent(env, "SYM", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 108L}});

                env.Milestone(1);

                SendEvent(env, "TAC", 12);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 108L}, new object[] {"TAC", 13L}});

                SendEvent(env, "OCC", 55);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"TAC", 13L}, new object[] {"OCC", 55L}});

                SendEvent(env, "OCC", 4);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"TAC", 13L}, new object[] {"OCC", 59L}});

                env.Milestone(2);

                SendEvent(env, "OCC", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"TAC", 12L}, new object[] {"OCC", 62L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupComplex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "msg"};
                var stmtText = "@Name('s0') insert into Cutoff " +
                               "select Symbol, (String.valueOf(count(*)) || 'x1000.0') as msg " +
                               "from SupportMarketDataBean#groupwin(Symbol)#length(1) " +
                               "where Price - Volume >= 1000.0 group by Symbol having count(*) = 1";
                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("SYM", -1, -1L, null));
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                env.SendEventBean(new SupportMarketDataBean("SYM", 100000d, 0L, null));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", "1x1000.0"}});

                env.SendEventBean(new SupportMarketDataBean("SYM", 1d, 1L, null));
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "Price", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, Price, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol " +
                               "order by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", -1, 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}});

                env.Milestone(0);

                SendEvent(env, "TAC", -2, 12);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 12L}});

                SendEvent(env, "TAC", -3, 13);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}
                    });

                env.Milestone(1);

                SendEvent(env, "SYM", -4, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 101L}, new object[] {"SYM", -4d, 101L},
                        new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}
                    });

                env.Milestone(2);

                SendEvent(env, "OCC", -5, 99);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"OCC", -5d, 99L}, new object[] {"SYM", -1d, 101L},
                        new object[] {"SYM", -4d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}
                    });

                SendEvent(env, "TAC", -6, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"OCC", -5d, 99L}, new object[] {"SYM", -4d, 1L}, new object[] {"TAC", -2d, 27L},
                        new object[] {"TAC", -3d, 27L}, new object[] {"TAC", -6d, 27L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "Price", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, Price, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol";

                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", -1, 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}});

                SendEvent(env, "TAC", -2, 12);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 12L}});

                env.Milestone(0);

                SendEvent(env, "TAC", -3, 13);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}
                    });

                env.Milestone(1);

                SendEvent(env, "SYM", -4, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L},
                        new object[] {"SYM", -4d, 101L}
                    });

                SendEvent(env, "OCC", -5, 99);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L},
                        new object[] {"SYM", -4d, 101L}, new object[] {"OCC", -5d, 99L}
                    });

                SendEvent(env, "TAC", -6, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"TAC", -2d, 27L}, new object[] {"TAC", -3d, 27L}, new object[] {"SYM", -4d, 1L},
                        new object[] {"OCC", -5d, 99L}, new object[] {"TAC", -6d, 27L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "Price", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, Price, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(5) " +
                               "group by Symbol having sum(Volume) > 20";

                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", -1, 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}});

                SendEvent(env, "TAC", -2, 12);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", -1d, 100L}});

                env.Milestone(0);

                SendEvent(env, "TAC", -3, 13);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}
                    });

                SendEvent(env, "SYM", -4, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L},
                        new object[] {"SYM", -4d, 101L}
                    });

                env.Milestone(1);

                SendEvent(env, "OCC", -5, 99);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L},
                        new object[] {"SYM", -4d, 101L}, new object[] {"OCC", -5d, 99L}
                    });

                env.Milestone(2);

                SendEvent(env, "TAC", -6, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"TAC", -2d, 27L}, new object[] {"TAC", -3d, 27L}, new object[] {"OCC", -5d, 99L},
                        new object[] {"TAC", -6d, 27L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) ";

                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 100L}});

                SendEvent(env, "TAC", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});

                env.Milestone(0);

                SendEvent(env, "MOV", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 104L}, new object[] {"TAC", 104L}, new object[] {"MOV", 104L}});

                SendEvent(env, "SYM", 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"TAC", 14L}, new object[] {"MOV", 14L}, new object[] {"SYM", 14L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEventOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select irstream Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) " +
                               " order by Symbol asc";
                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 100L}});

                SendEvent(env, "TAC", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});

                SendEvent(env, "MOV", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"MOV", 104L}, new object[] {"SYM", 104L}, new object[] {"TAC", 104L}});

                env.Milestone(0);

                SendEvent(env, "SYM", 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"MOV", 14L}, new object[] {"SYM", 14L}, new object[] {"TAC", 14L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEventHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumVol"};
                var stmtText = "@Name('s0') select Symbol, sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) having sum(Volume) > 100";

                env.CompileDeploy(stmtText).AddListener("s0");

                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, "SYM", 100);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});

                env.Milestone(1);

                SendEvent(env, "MOV", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"SYM", 104L}, new object[] {"TAC", 104L}, new object[] {"MOV", 104L}});

                SendEvent(env, "SYM", 10);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowForAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"sumVol"};
                var stmtText = "@Name('s0') select sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) ";

                env.CompileDeploy(stmtText).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null}});

                env.Milestone(0);

                SendEvent(env, 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {100L}});

                env.Milestone(1);

                SendEvent(env, 50);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {150L}});

                SendEvent(env, 25);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {175L}});

                env.Milestone(2);

                SendEvent(env, 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {85L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowForAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"sumVol"};
                var stmtText = "@Name('s0') select sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) having sum(Volume) > 100";

                env.CompileDeploy(stmtText).AddListener("s0");
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                SendEvent(env, 100);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                SendEvent(env, 50);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {150L}});

                SendEvent(env, 25);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {175L}});

                env.Milestone(1);

                SendEvent(env, 10);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }
    }
} // end of namespace