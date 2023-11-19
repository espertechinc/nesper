///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowForAll
    {
        private const string JOIN_KEY = "KEY";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAllSimple(execs);
            WithAllSumMinMax(execs);
            WithAllWWindowAgg(execs);
            WithAllMinMaxWindowed(execs);
            WithSumOneView(execs);
            WithSumJoin(execs);
            WithAvgPerSym(execs);
            WithSelectStarStdGroupBy(execs);
            WithSelectExprGroupWin(execs);
            WithSelectAvgExprStdGroupBy(execs);
            WithSelectAvgStdGroupByUni(execs);
            WithNamedWindowWindow(execs);
            WithStaticMethodDoubleNested(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithStaticMethodDoubleNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllStaticMethodDoubleNested());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllNamedWindowWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectAvgStdGroupByUni(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSelectAvgStdGroupByUni());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectAvgExprStdGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSelectAvgExprStdGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectExprGroupWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSelectExprGroupWin());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectStarStdGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSelectStarStdGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithAvgPerSym(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllAvgPerSym());
            return execs;
        }

        public static IList<RegressionExecution> WithSumJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSumOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSumOneView());
            return execs;
        }

        public static IList<RegressionExecution> WithAllMinMaxWindowed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllMinMaxWindowed());
            return execs;
        }

        public static IList<RegressionExecution> WithAllWWindowAgg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllWWindowAgg());
            return execs;
        }

        public static IList<RegressionExecution> WithAllSumMinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSumMinMax());
            return execs;
        }

        public static IList<RegressionExecution> WithAllSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllSimple());
            return execs;
        }

        private class ResultSetQueryTypeRowForAllStaticMethodDoubleNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "import " +
                          typeof(MyHelper).MaskTypeName() +
                          ";\n" +
                          "@name('s0') select MyHelper.DoOuter(MyHelper.DoInner(last(TheString))) as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "c0", "oiE1io");

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowForAllSumMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();

                env.Milestone(0);
                env.AdvanceTime(0);
                var epl = "@name('s0') select TheString as c0, sum(IntPrimitive) as c1," +
                          "min(IntPrimitive) as c2, max(IntPrimitive) as c3 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                SendEventSB(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, 10, 10 });

                env.Milestone(2);

                SendEventSB(env, "E2", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 10 + 100, 10, 100 });

                env.Milestone(3);

                SendEventSB(env, "E3", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 10 + 100 + 11, 10, 100 });

                env.Milestone(4);

                env.Milestone(5);

                SendEventSB(env, "E4", 9);
                env.AssertPropsNew("s0", fields, new object[] { "E4", 10 + 100 + 11 + 9, 9, 100 });

                SendEventSB(env, "E5", 120);
                env.AssertPropsNew("s0", fields, new object[] { "E5", 10 + 100 + 11 + 9 + 120, 9, 120 });

                SendEventSB(env, "E6", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E6", 10 + 100 + 11 + 9 + 120 + 100, 9, 120 });

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowForAllWWindowAgg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();

                var epl = "@name('s0') select irstream TheString as c0, sum(IntPrimitive) as c1," +
                          "window(*) as c2 from SupportBean.win:length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                object e1 = SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, new object[] { e1 } });

                env.Milestone(1);

                object e2 = SendSupportBean(env, "E2", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 10 + 100, new object[] { e1, e2 } });

                env.Milestone(2);

                object e3 = SendSupportBean(env, "E3", 11);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "E3", 100 + 11, new object[] { e2, e3 } },
                    new object[] { "E1", 100 + 11, new object[] { e2, e3 } });

                env.Milestone(3);

                env.Milestone(4);

                object e4 = SendSupportBean(env, "E4", 9);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "E4", 11 + 9, new object[] { e3, e4 } },
                    new object[] { "E2", 11 + 9, new object[] { e3, e4 } });

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowForAllSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream " +
                          "avg(Price) as avgPrice," +
                          "sum(Price) as sumPrice," +
                          "min(Price) as minPrice," +
                          "max(Price) as maxPrice," +
                          "median(Price) as medianPrice," +
                          "stddev(Price) as stddevPrice," +
                          "avedev(Price) as avedevPrice," +
                          "count(*) as datacount, " +
                          "count(distinct Price) as countDistinctPrice " +
                          "from SupportMarketDataBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent(100));

                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "avgPrice", 100d },
                        new object[] { "sumPrice", 100d },
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 100d },
                        new object[] { "medianPrice", 100d },
                        new object[] { "stddevPrice", null },
                        new object[] { "avedevPrice", 0.0 },
                        new object[] { "datacount", 1L },
                        new object[] { "countDistinctPrice", 1L },
                    }, // new data
                    new object[][] {
                        new object[] { "avgPrice", null },
                        new object[] { "sumPrice", null },
                        new object[] { "minPrice", null },
                        new object[] { "maxPrice", null },
                        new object[] { "medianPrice", null },
                        new object[] { "stddevPrice", null },
                        new object[] { "avedevPrice", null },
                        new object[] { "datacount", 0L },
                        new object[] { "countDistinctPrice", 0L },
                    } // old data
                );

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent(200));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "avgPrice", (100 + 200) / 2.0 },
                        new object[] { "sumPrice", 100 + 200d },
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 200d },
                        new object[] { "medianPrice", 150d },
                        new object[] { "stddevPrice", 70.71067811865476 },
                        new object[] { "avedevPrice", 50d },
                        new object[] { "datacount", 2L },
                        new object[] { "countDistinctPrice", 2L },
                    }, // new data
                    new object[][] {
                        new object[] { "avgPrice", 100d },
                        new object[] { "sumPrice", 100d },
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 100d },
                        new object[] { "medianPrice", 100d },
                        new object[] { "stddevPrice", null },
                        new object[] { "avedevPrice", 0.0 },
                        new object[] { "datacount", 1L },
                        new object[] { "countDistinctPrice", 1L },
                    } // old data
                );

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent(150));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "avgPrice", (150 + 100 + 200) / 3.0 },
                        new object[] { "sumPrice", 150 + 100 + 200d },
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 200d },
                        new object[] { "medianPrice", 150d },
                        new object[] { "stddevPrice", 50d },
                        new object[] { "avedevPrice", 33 + 1 / 3d },
                        new object[] { "datacount", 3L },
                        new object[] { "countDistinctPrice", 3L },
                    }, // new data
                    new object[][] {
                        new object[] { "avgPrice", (100 + 200) / 2.0 },
                        new object[] { "sumPrice", 100 + 200d },
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 200d },
                        new object[] { "medianPrice", 150d },
                        new object[] { "stddevPrice", 70.71067811865476 },
                        new object[] { "avedevPrice", 50d },
                        new object[] { "datacount", 2L },
                        new object[] { "countDistinctPrice", 2L },
                    } // old data
                );

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowForAllMinMaxWindowed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream " +
                          "min(Price) as minPrice," +
                          "max(Price) as maxPrice " +
                          "from  SupportMarketDataBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent(100));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 100d },
                    }, // new data
                    new object[][] {
                        new object[] { "minPrice", null },
                        new object[] { "maxPrice", null },
                    } // old data
                );

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent(200));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 200d },
                    }, // new data
                    new object[][] {
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 100d },
                    } // old data
                );

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent(150));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "minPrice", 150d },
                        new object[] { "maxPrice", 200d },
                    }, // new data
                    new object[][] {
                        new object[] { "minPrice", 100d },
                        new object[] { "maxPrice", 200d },
                    } // old data
                );

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream sum(LongBoxed) as mySum " +
                          "from SupportBean#time(10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                SendTimerEvent(env, 0);

                TryAssert(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream sum(LongBoxed) as mySum " +
                          "from SupportBeanString#keepall as one, " +
                          "SupportBean#time(10 sec) as two " +
                          "where one.TheString = two.TheString";
                env.CompileDeploy(epl).AddListener("s0");

                SendTimerEvent(env, 0);

                env.SendEventBean(new SupportBeanString(JOIN_KEY));

                TryAssert(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllAvgPerSym : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Sym,avgp".SplitCsv();
                var epl = "@name('s0') select irstream avg(Price) as avgp, Sym " +
                          "from SupportPriceEvent#groupwin(Sym)#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportPriceEvent(1, "A"));
                env.AssertPropsNew("s0", fields, new object[] { "A", 1.0 });

                env.SendEventBean(new SupportPriceEvent(2, "B"));
                env.AssertPropsNew("s0", fields, new object[] { "B", 1.5 });

                env.Milestone(0);

                env.SendEventBean(new SupportPriceEvent(9, "A"));
                env.AssertPropsNew("s0", fields, new object[] { "A", (1 + 2 + 9) / 3.0 });

                env.SendEventBean(new SupportPriceEvent(18, "B"));
                env.AssertPropsNew("s0", fields, new object[] { "B", (1 + 2 + 9 + 18) / 4.0 });

                env.SendEventBean(new SupportPriceEvent(5, "A"));
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "A", (2 + 9 + 18 + 5) / 4.0 },
                    new object[] { "A", (5 + 2 + 9 + 18) / 4.0 });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSelectStarStdGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select istream * from SupportMarketDataBean#groupwin(Symbol)#length(2)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", 1);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1.0, listener.LastNewData[0].Get("Price"));
                        Assert.IsTrue(listener.LastNewData[0].Underlying is SupportMarketDataBean);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSelectExprGroupWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select istream Price from SupportMarketDataBean#groupwin(Symbol)#length(2)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", 1);
                env.AssertListener(
                    "s0",
                    listener => Assert.AreEqual(1.0, env.Listener("s0").LastNewData[0].Get("Price")));

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSelectAvgExprStdGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select istream avg(Price) as aprice from SupportMarketDataBean" +
                               "#groupwin(Symbol)#length(2)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", 1);
                AssertAPrice(env, 1.0);

                env.Milestone(0);

                SendEvent(env, "B", 3);
                AssertAPrice(env, 2.0);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllSelectAvgStdGroupByUni : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select istream average as aprice from SupportMarketDataBean" +
                    "#groupwin(Symbol)#length(2)#uni(Price)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", 1);
                env.AssertEqualsNew("s0", "aprice", 1.0);

                env.Milestone(0);

                SendEvent(env, "B", 3);
                env.AssertEqualsNew("s0", "aprice", 3.0);

                SendEvent(env, "A", 3);
                env.AssertEqualsNew("s0", "aprice", 2.0);

                env.Milestone(1);

                SendEvent(env, "A", 10);
                env.ListenerReset("s0");

                SendEvent(env, "A", 20);
                env.AssertEqualsNew("s0", "aprice", 15.0);

                env.UndeployAll();
            }
        }

        private static void TryAssert(RegressionEnvironment env)
        {
            // assert select result type
            var fields = "mySum".SplitCsv();
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("mySum")));
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { null } });

            SendTimerEvent(env, 0);
            SendEvent(env, 10);
            AssertMySum(env, 10L);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { 10L } });

            SendTimerEvent(env, 5000);
            SendEvent(env, 15);
            AssertMySum(env, 25L);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { 25L } });

            SendTimerEvent(env, 8000);
            SendEvent(env, -5);
            AssertMySum(env, 20L);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { 20L } });

            SendTimerEvent(env, 10000);
            env.AssertPropsIRPair("s0", fields, new object[] { 10L }, new object[] { 20L });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { 10L } });

            SendTimerEvent(env, 15000);
            env.AssertPropsIRPair("s0", fields, new object[] { -5L }, new object[] { 10L });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { -5L } });

            SendTimerEvent(env, 18000);
            env.AssertPropsIRPair("s0", fields, new object[] { null }, new object[] { -5L });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                new string[] { "mySum" },
                new object[][] { new object[] { null } });
        }

        public class ResultSetQueryTypeRowForAllNamedWindowWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "create window ABCWin.win:keepall() as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "on SupportBean_A delete from ABCWin where TheString = Id;\n" +
                          "@name('s0') select irstream TheString as c0, window(IntPrimitive) as c1 from ABCWin;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", new int?[] { 10 } });

                env.Milestone(1);

                SendSupportBean(env, "E2", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E2", new int?[] { 10, 100 } });

                env.Milestone(2);

                SendSupportBean_A(env, "E2"); // delete E2
                env.AssertPropsOld("s0", fields, new object[] { "E2", new int?[] { 10 } });

                env.Milestone(3);

                SendSupportBean(env, "E3", 50);
                env.AssertPropsNew("s0", fields, new object[] { "E3", new int?[] { 10, 50 } });

                env.Milestone(4);

                env.Milestone(5); // no change

                SendSupportBean_A(env, "E1"); // delete E1
                env.AssertPropsOld("s0", fields, new object[] { "E1", new int?[] { 50 } });

                env.Milestone(6);

                SendSupportBean(env, "E4", -1);
                env.AssertPropsNew("s0", fields, new object[] { "E4", new int?[] { 50, -1 } });

                env.UndeployAll();
            }
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            env.SendEventBean(new SupportBean_A(id));
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            env.SendEventBean(sb);
            return sb;
        }

        private static object SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            object theEvent = new SupportMarketDataBean(symbol, price, null, null);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(double price)
        {
            return new SupportMarketDataBean("DELL", price, 0L, null);
        }

        private static void SendEventSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendTimerEvent(
            RegressionEnvironment env,
            long msec)
        {
            env.AdvanceTime(msec);
        }

        private static void AssertMySum(
            RegressionEnvironment env,
            long expected)
        {
            env.AssertListener(
                "s0",
                listener => Assert.AreEqual(expected, listener.GetAndResetLastNewData()[0].Get("mySum")));
        }

        private static void AssertAPrice(
            RegressionEnvironment env,
            double expected)
        {
            env.AssertListener(
                "s0",
                listener => Assert.AreEqual(expected, listener.GetAndResetLastNewData()[0].Get("aprice")));
        }

        public class MyHelper
        {
            public static string DoOuter(string value)
            {
                return "o" + value + "o";
            }

            public static string DoInner(string value)
            {
                return "i" + value + "i";
            }
        }
    }
} // end of namespace