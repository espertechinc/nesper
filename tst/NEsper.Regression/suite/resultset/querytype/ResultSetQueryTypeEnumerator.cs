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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeIterator
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPatternNoWindow(execs);
            WithPatternWithWindow(execs);
            WithOrderByWildcard(execs);
            WithOrderByProps(execs);
            WithFilter(execs);
            WithRowPerGroupOrdered(execs);
            WithRowPerGroup(execs);
            WithRowPerGroupHaving(execs);
            WithRowPerGroupComplex(execs);
            WithAggregateGroupedOrdered(execs);
            WithAggregateGrouped(execs);
            WithAggregateGroupedHaving(execs);
            WithRowPerEvent(execs);
            WithRowPerEventOrdered(execs);
            WithRowPerEventHaving(execs);
            WithRowForAll(execs);
            WithRowForAllHaving(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAllHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAll());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerEventHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventOrdered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerEventOrdered());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateGroupedHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregateGroupedHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregateGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateGroupedOrdered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregateGroupedOrdered());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupComplex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupComplex());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupOrdered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupOrdered());
            return execs;
        }

        public static IList<RegressionExecution> WithFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithOrderByProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeOrderByProps());
            return execs;
        }

        public static IList<RegressionExecution> WithOrderByWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeOrderByWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWithWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypePatternWithWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternNoWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypePatternNoWindow());
            return execs;
        }

        internal class ResultSetQueryTypePatternNoWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-115
                var epl = "@name('s0') @IterableUnbound select * from pattern " +
                          "[every ( addressInfo = SupportBean(TheString='address') " +
                          "-> txnWD = SupportBean(TheString='txn') ) ] " +
                          "where addressInfo.IntBoxed = txnWD.IntBoxed";
                env.CompileDeploy(epl).AddListener("s0");

                var myEventBean1 = new SupportBean();
                myEventBean1.TheString = "address";
                myEventBean1.IntBoxed = 9001;
                env.SendEventBean(myEventBean1);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.Milestone(0);

                var myEventBean2 = new SupportBean();
                myEventBean2.TheString = "txn";
                myEventBean2.IntBoxed = 9001;
                env.SendEventBean(myEventBean2);
                env.AssertIterator("s0", enumerator => Assert.IsTrue(enumerator.MoveNext()));

                env.Milestone(1);

                env.AssertIterator(
                    "s0",
                    enumerator => {
                        Assert.IsTrue(enumerator.MoveNext());
                        var theEvent = enumerator.Current;
                        Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
                        Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypePatternWithWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern " +
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

                env.AssertIterator(
                    "s0",
                    enumerator => {
                        Assert.IsTrue(enumerator.MoveNext());
                        var theEvent = enumerator.Current;
                        Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
                        Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOrderByWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportMarketDataBean#length(5) Order by Symbol, Volume";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                object eventOne = SendEvent(env, "SYM", 1);
                AssertIterator(env, new object[] { eventOne });

                object eventTwo = SendEvent(env, "OCC", 2);
                AssertIterator(env, new object[] { eventTwo, eventOne });

                object eventThree = SendEvent(env, "TOC", 3);
                AssertIterator(env, new object[] { eventTwo, eventOne, eventThree });

                object eventFour = SendEvent(env, "SYM", 0);
                AssertIterator(env, new object[] { eventTwo, eventFour, eventOne, eventThree });

                object eventFive = SendEvent(env, "SYM", 10);
                AssertIterator(env, new object[] { eventTwo, eventFour, eventOne, eventFive, eventThree });

                object eventSix = SendEvent(env, "SYM", 4);
                AssertIterator(env, new object[] { eventTwo, eventFour, eventSix, eventFive, eventThree });

                env.UndeployAll();
            }

            private void AssertIterator(
                RegressionEnvironment env,
                object[] expecteds)
            {
                env.AssertIterator(
                    "s0",
                    iterator => EPAssertionUtil.AssertEqualsExactOrderUnderlying(expecteds, iterator));
            }
        }

        internal class ResultSetQueryTypeOrderByProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "Volume" };
                var stmtText =
"@name('s0') select Symbol, Volume from SupportMarketDataBean#length(3) Order by Symbol, Volume";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 1L } });

                SendEvent(env, "OCC", 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "OCC", 2L }, new object[] { "SYM", 1L } });

                SendEvent(env, "SYM", 0);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "OCC", 2L }, new object[] { "SYM", 0L }, new object[] { "SYM", 1L } });

                SendEvent(env, "OCC", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "OCC", 2L }, new object[] { "OCC", 3L }, new object[] { "SYM", 0L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "vol" };
                var stmtText = "@name('s0') select Symbol, Volume * 10 as vol from SupportMarketDataBean#length(5)"+
                               " where Volume < 0";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "SYM", 100);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));
                env.AssertPropsPerRowIterator("s0", fields, null);

                SendEvent(env, "SYM", -1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -10L } });

                SendEvent(env, "SYM", -6);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -10L }, new object[] { "SYM", -60L } });

                env.Milestone(0);

                SendEvent(env, "SYM", 1);
                SendEvent(env, "SYM", 16);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -10L }, new object[] { "SYM", -60L } });

                SendEvent(env, "SYM", -9);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "SYM", -10L }, new object[] { "SYM", -60L }, new object[] { "SYM", -90L } });

                env.Milestone(1);

                SendEvent(env, "SYM", 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -60L }, new object[] { "SYM", -90L } });

                SendEvent(env, "SYM", 3);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -90L } });

                env.Milestone(2);

                SendEvent(env, "SYM", 4);
                SendEvent(env, "SYM", 5);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -90L } });

                env.Milestone(3);

                SendEvent(env, "SYM", 6);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol "+
"Order by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.Milestone(0);

                SendEvent(env, "SYM", 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 100L } });

                env.Milestone(1);

                SendEvent(env, "OCC", 5);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "OCC", 5L }, new object[] { "SYM", 100L } });

                SendEvent(env, "SYM", 10);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "OCC", 5L }, new object[] { "SYM", 110L } });

                SendEvent(env, "OCC", 6);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "OCC", 11L }, new object[] { "SYM", 110L } });

                env.Milestone(2);

                SendEvent(env, "ATB", 8);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ATB", 8L }, new object[] { "OCC", 11L }, new object[] { "SYM", 110L } });

                env.Milestone(3);

                SendEvent(env, "ATB", 7);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ATB", 15L }, new object[] { "OCC", 11L }, new object[] { "SYM", 10L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 100L } });

                SendEvent(env, "SYM", 10);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 110L } });

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 110L }, new object[] { "TAC", 1L } });

                SendEvent(env, "SYM", 11);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 121L }, new object[] { "TAC", 1L } });

                env.Milestone(1);

                SendEvent(env, "TAC", 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 121L }, new object[] { "TAC", 3L } });

                SendEvent(env, "OCC", 55);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "SYM", 21L }, new object[] { "TAC", 3L }, new object[] { "OCC", 55L } });

                env.Milestone(2);

                SendEvent(env, "OCC", 4);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "TAC", 3L }, new object[] { "SYM", 11L }, new object[] { "OCC", 59L } });

                SendEvent(env, "OCC", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "SYM", 11L }, new object[] { "TAC", 2L }, new object[] { "OCC", 62L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol having sum(Volume) > 10";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 100L } });

                SendEvent(env, "SYM", 5);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 105L } });

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 105L } });

                SendEvent(env, "SYM", 3);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 108L } });

                env.Milestone(1);

                SendEvent(env, "TAC", 12);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 108L }, new object[] { "TAC", 13L } });

                SendEvent(env, "OCC", 55);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "TAC", 13L }, new object[] { "OCC", 55L } });

                SendEvent(env, "OCC", 4);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "TAC", 13L }, new object[] { "OCC", 59L } });

                env.Milestone(2);

                SendEvent(env, "OCC", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "TAC", 12L }, new object[] { "OCC", 62L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupComplex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "msg" };
                var stmtText = "@name('s0') insert into Cutoff " +
"select Symbol, (String.valueOf(count(*)) || 'x1000.0') as msg "+
"from SupportMarketDataBean#groupwin(Symbol)#length(1) "+
"where Price - Volume >= 1000.0 group by Symbol having count(*) = 1";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("SYM", -1, -1L, null));
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.Milestone(1);

                env.SendEventBean(new SupportMarketDataBean("SYM", 100000d, 0L, null));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", "1x1000.0" } });

                env.SendEventBean(new SupportMarketDataBean("SYM", 1d, 1L, null));
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "Price", "sumVol" };
                var stmtText = "@name('s0') select Symbol, Price, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol "+
"Order by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", -1, 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -1d, 100L } });

                env.Milestone(0);

                SendEvent(env, "TAC", -2, 12);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -1d, 100L }, new object[] { "TAC", -2d, 12L } });

                SendEvent(env, "TAC", -3, 13);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 100L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }
                    });

                env.Milestone(1);

                SendEvent(env, "SYM", -4, 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 101L }, new object[] { "SYM", -4d, 101L },
                        new object[] { "TAC", -2d, 25L }, new object[] { "TAC", -3d, 25L }
                    });

                env.Milestone(2);

                SendEvent(env, "OCC", -5, 99);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "OCC", -5d, 99L }, new object[] { "SYM", -1d, 101L },
                        new object[] { "SYM", -4d, 101L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }
                    });

                SendEvent(env, "TAC", -6, 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "OCC", -5d, 99L }, new object[] { "SYM", -4d, 1L },
                        new object[] { "TAC", -2d, 27L }, new object[] { "TAC", -3d, 27L },
                        new object[] { "TAC", -6d, 27L }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "Price", "sumVol" };
                var stmtText = "@name('s0') select Symbol, Price, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol";

                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", -1, 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -1d, 100L } });

                SendEvent(env, "TAC", -2, 12);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -1d, 100L }, new object[] { "TAC", -2d, 12L } });

                env.Milestone(0);

                SendEvent(env, "TAC", -3, 13);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 100L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }
                    });

                env.Milestone(1);

                SendEvent(env, "SYM", -4, 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 101L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }, new object[] { "SYM", -4d, 101L }
                    });

                SendEvent(env, "OCC", -5, 99);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 101L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }, new object[] { "SYM", -4d, 101L },
                        new object[] { "OCC", -5d, 99L }
                    });

                SendEvent(env, "TAC", -6, 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "TAC", -2d, 27L }, new object[] { "TAC", -3d, 27L },
                        new object[] { "SYM", -4d, 1L }, new object[] { "OCC", -5d, 99L },
                        new object[] { "TAC", -6d, 27L }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "Price", "sumVol" };
                var stmtText = "@name('s0') select Symbol, Price, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(5) " +
"group by Symbol having sum(Volume) > 20";

                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", -1, 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", -1d, 100L } });

                SendEvent(env, "TAC", -2, 12);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", -1d, 100L } });

                env.Milestone(0);

                SendEvent(env, "TAC", -3, 13);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 100L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }
                    });

                SendEvent(env, "SYM", -4, 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 101L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }, new object[] { "SYM", -4d, 101L }
                    });

                env.Milestone(1);

                SendEvent(env, "OCC", -5, 99);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "SYM", -1d, 101L }, new object[] { "TAC", -2d, 25L },
                        new object[] { "TAC", -3d, 25L }, new object[] { "SYM", -4d, 101L },
                        new object[] { "OCC", -5d, 99L }
                    });

                env.Milestone(2);

                SendEvent(env, "TAC", -6, 2);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "TAC", -2d, 27L }, new object[] { "TAC", -3d, 27L },
                        new object[] { "OCC", -5d, 99L }, new object[] { "TAC", -6d, 27L }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(3) ";

                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 100L } });

                SendEvent(env, "TAC", 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 101L }, new object[] { "TAC", 101L } });

                env.Milestone(0);

                SendEvent(env, "MOV", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "SYM", 104L }, new object[] { "TAC", 104L }, new object[] { "MOV", 104L } });

                SendEvent(env, "SYM", 10);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "TAC", 14L }, new object[] { "MOV", 14L }, new object[] { "SYM", 14L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEventOrdered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select irstream Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(3) " +
" Order by Symbol asc";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "SYM", 100L } });

                SendEvent(env, "TAC", 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 101L }, new object[] { "TAC", 101L } });

                SendEvent(env, "MOV", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "MOV", 104L }, new object[] { "SYM", 104L }, new object[] { "TAC", 104L } });

                env.Milestone(0);

                SendEvent(env, "SYM", 10);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "MOV", 14L }, new object[] { "SYM", 14L }, new object[] { "TAC", 14L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEventHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumVol" };
                var stmtText = "@name('s0') select Symbol, sum(Volume) as sumVol "+
                               "from SupportMarketDataBean#length(3) having sum(Volume) > 100";

                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, "SYM", 100);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.Milestone(0);

                SendEvent(env, "TAC", 1);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "SYM", 101L }, new object[] { "TAC", 101L } });

                env.Milestone(1);

                SendEvent(env, "MOV", 3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "SYM", 104L }, new object[] { "TAC", 104L }, new object[] { "MOV", 104L } });

                SendEvent(env, "SYM", 10);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowForAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "sumVol" };
                var stmtText = "@name('s0') select sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) ";

                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { null } });

                env.Milestone(0);

                SendEvent(env, 100);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 100L } });

                env.Milestone(1);

                SendEvent(env, 50);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 150L } });

                SendEvent(env, 25);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 175L } });

                env.Milestone(2);

                SendEvent(env, 10);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 85L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowForAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "sumVol" };
                var stmtText = "@name('s0') select sum(Volume) as sumVol " +
                               "from SupportMarketDataBean#length(3) having sum(Volume) > 100";

                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                SendEvent(env, 100);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.Milestone(0);

                SendEvent(env, 50);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 150L } });

                SendEvent(env, 25);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 175L } });

                env.Milestone(1);

                SendEvent(env, 10);
                env.AssertIterator("s0", enumerator => Assert.IsFalse(enumerator.MoveNext()));

                env.UndeployAll();
            }
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
    }
} // end of namespace