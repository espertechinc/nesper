///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerGroup
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithRowPerGroupSimple(execs);
            WithRowPerGroupSumOneView(execs);
            WithRowPerGroupSumJoin(execs);
            WithCriteriaByDotMethod(execs);
            WithNamedWindowDelete(execs);
            WithUnboundStreamUnlimitedKey(execs);
            WithAggregateGroupedProps(execs);
            WithAggregateGroupedPropsPerGroup(execs);
            WithAggregationOverGroupedProps(execs);
            WithUniqueInBatch(execs);
            WithSelectAvgExprGroupBy(execs);
            WithUnboundStreamIterate(execs);
            WithReclaimSideBySide(execs);
            WithRowPerGrpMultikeyWArray(execs);
            WithRowPerGrpMultikeyWReclaim(execs);
            WithRowPerGrpNullGroupKey(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGrpNullGroupKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGrpNullGroupKey());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGrpMultikeyWReclaim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGrpMultikeyWReclaim());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGrpMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGrpMultikeyWArray(false, true));
            execs.Add(new ResultSetQueryTypeRowPerGrpMultikeyWArray(false, false));
            execs.Add(new ResultSetQueryTypeRowPerGrpMultikeyWArray(true, false));
            return execs;
        }

        public static IList<RegressionExecution> WithReclaimSideBySide(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeReclaimSideBySide());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundStreamIterate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundStreamIterate());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectAvgExprGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSelectAvgExprGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithUniqueInBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUniqueInBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationOverGroupedProps(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregationOverGroupedProps());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateGroupedPropsPerGroup(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregateGroupedPropsPerGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateGroupedProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregateGroupedProps());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundStreamUnlimitedKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundStreamUnlimitedKey());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeNamedWindowDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithCriteriaByDotMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeCriteriaByDotMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupSumJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupSumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupSumOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupSumOneView());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupSimple());
            return execs;
        }

        public class ResultSetQueryTypeRowPerGrpNullGroupKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema MyEventWNullType(Id string, value int, groupkey null);\n" +
                    "@name('s0') select sum(value) as thesum from MyEventWNullType group by groupkey;\n" +
                    "@name('s1') select sum(value) as thesum from MyEventWNullType group by groupkey output snapshot every 5 events;\n" +
                    "@name('s2') select sum(value) as thesum from MyEventWNullType group by Id, groupkey;\n" +
                    "@name('s3') select sum(value) as thesum from MyEventWNullType group by Id, groupkey output snapshot every 5 events;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s2");

                SendEventAssert(env, "G1", 10, 10);

                env.Milestone(0);

                SendEventAssert(env, "G1", 11, 21);

                env.UndeployAll();
            }

            private void SendEventAssert(
                RegressionEnvironment env,
                string id,
                int value,
                int expected)
            {
                env.SendEventMap(CollectionUtil.BuildMap("Id", id, "value", value), "MyEventWNullType");
                env.AssertEqualsNew("s0", "thesum", expected);
                env.AssertEqualsNew("s2", "thesum", expected);
            }
        }

        public class ResultSetQueryTypeRowPerGrpMultikeyWReclaim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Hint('reclaim_group_aged=10,reclaim_group_freq=1') @name('s0') select TheString, IntPrimitive, sum(LongPrimitive) as thesum from SupportBean group by TheString, IntPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventSBAssert(env, "A", 0, 100, 100);
                SendEventSBAssert(env, "A", 0, 101, 201);

                env.Milestone(0);
                env.AdvanceTime(11000);

                SendEventSBAssert(env, "A", 0, 104, 104);

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowPerGrpMultikeyWArray : RegressionExecution
        {
            private readonly bool join;
            private readonly bool unbound;

            public ResultSetQueryTypeRowPerGrpMultikeyWArray(
                bool join,
                bool unbound)
            {
                this.join = join;
                this.unbound = unbound;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = join
                    ? "@name('s0') select sum(Value) as thesum from SupportEventWithIntArray#keepall, SupportBean#keepall group by Array"
                    : (unbound
                        ? "@name('s0') select sum(Value) as thesum from SupportEventWithIntArray group by Array"
                        : "@name('s0') select sum(Value) as thesum from SupportEventWithIntArray#keepall group by Array"
                    );

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean());

                SendAssertIntArray(env, "E1", new int[] { 1, 2 }, 5, 5);

                env.Milestone(0);

                SendAssertIntArray(env, "E2", new int[] { 1, 2 }, 10, 15);
                SendAssertIntArray(env, "E3", new int[] { 1 }, 11, 11);
                SendAssertIntArray(env, "E4", new int[] { 1, 3 }, 12, 12);

                env.Milestone(1);

                SendAssertIntArray(env, "E5", new int[] { 1 }, 13, 24);
                SendAssertIntArray(env, "E6", new int[] { 1, 3 }, 15, 27);
                SendAssertIntArray(env, "E7", new int[] { 1, 2 }, 16, 31);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       ", unbound=" +
                       unbound +
                       '}';
            }
        }

        public class ResultSetQueryTypeRowPerGroupSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();

                var epl = "@name('s0') select TheString as c0, sum(IntPrimitive) as c1," +
                          "min(IntPrimitive) as c2, max(IntPrimitive) as c3 from SupportBean group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventSB(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, 10, 10 });

                env.Milestone(1);

                SendEventSB(env, "E2", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 100, 100, 100 });

                env.Milestone(2);

                SendEventSB(env, "E1", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 21, 10, 11 });

                env.Milestone(3);

                SendEventSB(env, "E1", 9);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 30, 9, 11 });

                env.Milestone(4);

                SendEventSB(env, "E2", 99);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 199, 99, 100 });

                env.Milestone(5);

                SendEventSB(env, "E2", 97);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 296, 97, 100 });

                env.Milestone(6);

                SendEventSB(env, "E3", 1000);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 1000, 1000, 1000 });

                env.Milestone(7);

                SendEventSB(env, "E2", 96);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 392, 96, 100 });

                env.Milestone(8);

                env.Milestone(9);

                SendEventSB(env, "E2", 101);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 493, 96, 101 });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeSelectAvgExprGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select istream avg(Price) as aprice, Symbol from SupportMarketDataBean" +
                               "#length(2) group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = "aprice,Symbol".SplitCsv();

                SendEvent(env, "A", 1);
                env.AssertPropsNew("s0", fields, new object[] { 1.0, "A" });

                SendEvent(env, "B", 3);
                env.AssertPropsNew("s0", fields, new object[] { 3.0, "B" });

                env.Milestone(0);

                SendEvent(env, "B", 5);
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "A" }, new object[] { 4.0, "B" } });

                env.Milestone(1);

                SendEvent(env, "A", 10);
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 10.0, "A" }, new object[] { 5.0, "B" } });

                SendEvent(env, "A", 20);
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 15.0, "A" }, new object[] { null, "B" } });

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeReclaimSideBySide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@name('S0') @Hint('disable_reclaim_group') select sum(IntPrimitive) as val from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplOne).AddListener("S0");
                var eplTwo =
                    "@name('S1') @Hint('disable_reclaim_group') select window(IntPrimitive) as val from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplTwo).AddListener("S1");
                var eplThree =
                    "@name('S2') @Hint('disable_reclaim_group') select sum(IntPrimitive) as val1, window(IntPrimitive) as val2 from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplThree).AddListener("S2");
                var eplFour =
                    "@name('S3') @Hint('reclaim_group_aged=10,reclaim_group_freq=5') select sum(IntPrimitive) as val1, window(IntPrimitive) as val2 from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplFour).AddListener("S3");

                var fieldsOne = "val".SplitCsv();
                var fieldsTwo = "val".SplitCsv();
                var fieldsThree = "val1,val2".SplitCsv();
                var fieldsFour = "val1,val2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("S0", fieldsOne, new object[] { 1 });
                env.AssertPropsNew("S1", fieldsTwo, new object[] { new int?[] { 1 } });
                env.AssertPropsNew("S2", fieldsThree, new object[] { 1, new int?[] { 1 } });
                env.AssertPropsNew("S3", fieldsFour, new object[] { 1, new int?[] { 1 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertPropsNew("S0", fieldsOne, new object[] { 3 });
                env.AssertPropsNew("S1", fieldsTwo, new object[] { new int?[] { 1, 2 } });
                env.AssertPropsNew("S2", fieldsThree, new object[] { 3, new int?[] { 1, 2 } });
                env.AssertPropsNew("S3", fieldsFour, new object[] { 3, new int?[] { 1, 2 } });

                env.SendEventBean(new SupportBean("E2", 4));
                env.AssertPropsNew("S0", fieldsOne, new object[] { 4 });
                env.AssertPropsNew("S1", fieldsTwo, new object[] { new int?[] { 4 } });
                env.AssertPropsNew("S2", fieldsThree, new object[] { 4, new int?[] { 4 } });
                env.AssertPropsNew("S3", fieldsFour, new object[] { 4, new int?[] { 4 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 5));
                env.AssertPropsNew("S0", fieldsOne, new object[] { 9 });
                env.AssertPropsNew("S1", fieldsTwo, new object[] { new int?[] { 4, 5 } });
                env.AssertPropsNew("S2", fieldsThree, new object[] { 9, new int?[] { 4, 5 } });
                env.AssertPropsNew("S3", fieldsFour, new object[] { 9, new int?[] { 4, 5 } });

                env.SendEventBean(new SupportBean("E1", 6));
                env.AssertPropsNew("S0", fieldsOne, new object[] { 9 });
                env.AssertPropsNew("S1", fieldsTwo, new object[] { new int?[] { 1, 2, 6 } });
                env.AssertPropsNew("S2", fieldsThree, new object[] { 9, new int?[] { 1, 2, 6 } });
                env.AssertPropsNew("S3", fieldsFour, new object[] { 9, new int?[] { 1, 2, 6 } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeCriteriaByDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select sb.TheString as c0, sum(IntPrimitive) as c1 " +
                          "from SupportBean#length_batch(2) as sb group by sb.TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 20));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "E1", 30 });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeUnboundStreamIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var milestone = new AtomicLong();

                // with output snapshot
                var epl =
                    "@name('s0') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 10 } });
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E1", 11));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E0", 30));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 }, new object[] { "E0", 30 } });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();

                // with order-by
                epl =
                    "@name('s0') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E1", 11));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E0", 30));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E0", 30 }, new object[] { "E1", 21 }, new object[] { "E2", 20 } });
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E3", 40));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E0", 30 }, new object[] { "E1", 21 }, new object[] { "E2", 20 },
                        new object[] { "E3", 40 }
                    });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();

                // test un-grouped case
                epl =
                    "@name('s0') select null as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { null, 10 } });
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { null, 30 } });
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 11));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { null, 41 } });
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { null, 41 } });

                env.UndeployAll();

                // test reclaim
                env.AdvanceTime(1000);
                epl =
                    "@name('s0') @Hint('reclaim_group_aged=1,reclaim_group_freq=1') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.MilestoneInc(milestone);

                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E0", 11));

                env.MilestoneInc(milestone);

                env.AdvanceTime(1800);
                env.SendEventBean(new SupportBean("E2", 12));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E0", 11 }, new object[] { "E2", 12 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E0", 11 }, new object[] { "E2", 12 } });

                env.MilestoneInc(milestone);

                env.AdvanceTime(2200);
                env.SendEventBean(new SupportBean("E2", 13));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E0", 11 }, new object[] { "E2", 25 } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                var epl = "@public create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A a delete from MyWindow w where w.TheString = a.Id;\n" +
                          "on SupportBean_B delete from MyWindow;\n";
                env.CompileDeploy(epl, path);

                epl =
                    "@Hint('DISABLE_RECLAIM_GROUP') @name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl, path).AddListener("s0");
                var fields = "TheString,mysum".SplitCsv();

                TryAssertionNamedWindowDelete(env, fields, milestone);

                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean_B("delete"));

                epl =
                    "@name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl, path).AddListener("s0");

                TryAssertionNamedWindowDelete(env, fields, milestone);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeUnboundStreamUnlimitedKey : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                // ESPER-396 Unbound stream and aggregating/grouping by unlimited key (i.e. timestamp) configurable state drop
                SendTimer(env, 0);

                // After the oldest group is 60 second old, reclaim group older then  30 seconds
                var epl =
                    "@name('s0') @Hint('reclaim_group_aged=30,reclaim_group_freq=5') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 1000; i++) {
                    SendTimer(env, 1000 + i * 1000); // reduce factor if sending more events
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);

                    //if (i % 100000 == 0)
                    //{
                    //    Console.WriteLine("Sending event number " + i);
                    //}
                }

                env.ListenerReset("s0");

                for (var i = 0; i < 964; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    env.AssertEqualsNew("s0", "count(*)", 1L);
                }

                for (var i = 965; i < 1000; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    env.AssertEqualsNew("s0", "count(*)", 2L);
                }

                env.UndeployAll();

                // no frequency provided
                epl =
                    "@name('s0') @Hint('reclaim_group_aged=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('define-age') @public create variable int myAge = 10;\n" +
                    "@name('define-freq') @public create variable int myFreq = 10;\n",
                    path);

                epl =
                    "@name('s0') @Hint('reclaim_group_aged=myAge,reclaim_group_freq=myFreq') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl, path).AddListener("s0");

                for (var i = 0; i < 1000; i++) {
                    SendTimer(env, 2000000 + 1000 + i * 1000); // reduce factor if sending more events
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);

                    if (i == 500) {
                        env.RuntimeSetVariable("define-age", "myAge", 60);
                        env.RuntimeSetVariable("define-age", "myFreq", 90);
                    }

                    if (i % 100000 == 0) {
                        // Comment-in when needed: Console.WriteLine("Sending event number " + i);
                    }
                }

                env.ListenerReset("s0");

                for (var i = 0; i < 900; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    env.AssertEqualsNew("s0", "count(*)", 1L);
                }

                for (var i = 900; i < 1000; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    env.AssertEqualsNew("s0", "count(*)", 2L);
                }

                env.UndeployAll();

                // invalid tests
                env.TryInvalidCompile(
                    path,
                    "@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                env.TryInvalidCompile(
                    path,
                    "@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                env.TryInvalidCompile(
                    path,
                    "@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Variable type of variable 'MyVar' is not numeric [@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                env.TryInvalidCompile(
                    path,
                    "@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Hint parameter value '-30' is an invalid value, expecting a double-typed seconds value or variable name [@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            }
        }

        private class ResultSetQueryTypeAggregateGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = "mycount".SplitCsv();
                var epl = "@name('s0') select irstream count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Price";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, SYMBOL_DELL, 10);
                env.AssertPropsIRPair("s0", fields, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 1L } });

                env.Milestone(1);

                SendEvent(env, SYMBOL_DELL, 11);
                env.AssertPropsIRPair("s0", fields, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1L }, new object[] { 1L } });

                SendEvent(env, SYMBOL_IBM, 10);
                env.AssertPropsIRPair("s0", fields, new object[] { 2L }, new object[] { 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 2L }, new object[] { 1L } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeAggregateGroupedPropsPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = "mycount".SplitCsv();
                var epl = "@name('s0') select irstream count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Symbol, Price";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 10);
                env.AssertPropsIRPair("s0", fields, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 1L } });

                SendEvent(env, SYMBOL_DELL, 11);
                env.AssertPropsIRPair("s0", fields, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1L }, new object[] { 1L } });

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 10);
                env.AssertPropsIRPair("s0", fields, new object[] { 2L }, new object[] { 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 2L }, new object[] { 1L } });

                SendEvent(env, SYMBOL_IBM, 10);
                env.AssertPropsIRPair("s0", fields, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 2L }, new object[] { 1L }, new object[] { 1L } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeAggregationOverGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = "Symbol,Price,mycount".SplitCsv();
                var epl = "@name('s0') select irstream Symbol,Price,count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Symbol, Price order by Symbol asc";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 10);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "DELL", 10.0, 1L },
                    new object[] { "DELL", 10.0, 0L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "DELL", 10.0, 1L } });

                SendEvent(env, SYMBOL_DELL, 11);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "DELL", 11.0, 1L },
                    new object[] { "DELL", 11.0, 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "DELL", 10.0, 1L }, new object[] { "DELL", 11.0, 1L } });

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 10);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "DELL", 10.0, 2L },
                    new object[] { "DELL", 10.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L } });

                SendEvent(env, SYMBOL_IBM, 5);
                env.AssertPropsIRPair("s0", fields, new object[] { "IBM", 5.0, 1L }, new object[] { "IBM", 5.0, 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L },
                        new object[] { "IBM", 5.0, 1L }
                    });

                SendEvent(env, SYMBOL_IBM, 5);
                env.AssertPropsIRPair("s0", fields, new object[] { "IBM", 5.0, 2L }, new object[] { "IBM", 5.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L },
                        new object[] { "IBM", 5.0, 2L }
                    });

                env.Milestone(1);

                SendEvent(env, SYMBOL_IBM, 5);
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "IBM", 5.0, 3L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "IBM", 5.0, 2L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "DELL", 10.0, 1L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "DELL", 10.0, 2L });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "DELL", 11.0, 1L }, new object[] { "DELL", 10.0, 1L },
                        new object[] { "IBM", 5.0, 3L }
                    });

                SendEvent(env, SYMBOL_IBM, 5);
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "IBM", 5.0, 4L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "IBM", 5.0, 3L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "DELL", 11.0, 0L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "DELL", 11.0, 1L });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "DELL", 10.0, 1L }, new object[] { "IBM", 5.0, 4L } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeUniqueInBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var epl = "insert into MyStream select Symbol, Price from SupportMarketDataBean#time_batch(1 sec);\n" +
                          "@name('s0') select Symbol " +
                          "from MyStream#time_batch(1 sec)#unique(Symbol) " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "IBM", 100);
                SendEvent(env, "IBM", 101);

                env.Milestone(1);

                SendEvent(env, "IBM", 102);
                SendTimer(env, 1000);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var received = listener.DataListsFlattened;
                        ClassicAssert.AreEqual("IBM", received.First[0].Get("Symbol"));
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionNamedWindowDelete(
            RegressionEnvironment env,
            string[] fields,
            AtomicLong milestone)
        {
            env.SendEventBean(new SupportBean("A", 100));
            env.AssertPropsNew("s0", fields, new object[] { "A", 100 });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B", 20));
            env.AssertPropsNew("s0", fields, new object[] { "B", 20 });

            env.SendEventBean(new SupportBean("A", 101));
            env.AssertPropsNew("s0", fields, new object[] { "A", 201 });

            env.SendEventBean(new SupportBean("B", 21));
            env.AssertPropsNew("s0", fields, new object[] { "B", 41 });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] { new object[] { "A", 201 }, new object[] { "B", 41 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_A("A"));
            env.AssertPropsNew("s0", fields, new object[] { "A", null });
            env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "B", 41 } });

            env.SendEventBean(new SupportBean("A", 102));
            env.AssertPropsNew("s0", fields, new object[] { "A", 102 });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] { new object[] { "A", 102 }, new object[] { "B", 41 } });

            env.SendEventBean(new SupportBean_A("B"));
            env.AssertPropsNew("s0", fields, new object[] { "B", null });
            env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "A", 102 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B", 22));
            env.AssertPropsNew("s0", fields, new object[] { "B", 22 });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] { new object[] { "A", 102 }, new object[] { "B", 22 } });
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            double? oldSum,
            double? oldAvg,
            double? newSum,
            double? newAvg)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    ClassicAssert.AreEqual(1, oldData.Length);
                    ClassicAssert.AreEqual(1, newData.Length);

                    ClassicAssert.AreEqual(symbol, oldData[0].Get("Symbol"));
                    ClassicAssert.AreEqual(oldSum, oldData[0].Get("mySum"));
                    ClassicAssert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

                    ClassicAssert.AreEqual(symbol, newData[0].Get("Symbol"));
                    ClassicAssert.AreEqual(newSum, newData[0].Get("mySum"));
                    ClassicAssert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

                    listener.Reset();
                });
        }

        private class ResultSetQueryTypeRowPerGroupSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "       and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowPerGroupSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private static void TryAssertionSum(RegressionEnvironment env)
        {
            var fields = new string[] { "Symbol", "mySum", "myAvg" };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    ClassicAssert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                    ClassicAssert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("myAvg"));
                });

            SendEvent(env, SYMBOL_DELL, 10);
            AssertEvents(
                env,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "DELL", 10d, 10d } });

            env.Milestone(1);

            SendEvent(env, SYMBOL_DELL, 20);
            AssertEvents(
                env,
                SYMBOL_DELL,
                10d,
                10d,
                30d,
                15d);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "DELL", 30d, 15d } });

            env.Milestone(2);

            SendEvent(env, SYMBOL_DELL, 100);
            AssertEvents(
                env,
                SYMBOL_DELL,
                30d,
                15d,
                130d,
                130d / 3d);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "DELL", 130d, 130d / 3d } });

            env.Milestone(3);

            SendEvent(env, SYMBOL_DELL, 50);
            AssertEvents(
                env,
                SYMBOL_DELL,
                130d,
                130 / 3d,
                170d,
                170 / 3d); // 20 + 100 + 50
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "DELL", 170d, 170d / 3d } });

            env.Milestone(4);

            SendEvent(env, SYMBOL_DELL, 5);
            AssertEvents(
                env,
                SYMBOL_DELL,
                170d,
                170 / 3d,
                155d,
                155 / 3d); // 100 + 50 + 5
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "DELL", 155d, 155d / 3d } });

            env.Milestone(5);

            SendEvent(env, "AAA", 1000);
            AssertEvents(
                env,
                SYMBOL_DELL,
                155d,
                155d / 3,
                55d,
                55d / 2); // 50 + 5
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "DELL", 55d, 55d / 2d } });

            env.Milestone(6);

            SendEvent(env, SYMBOL_IBM, 70);
            AssertEvents(
                env,
                SYMBOL_DELL,
                55d,
                55 / 2d,
                5,
                5,
                SYMBOL_IBM,
                null,
                null,
                70,
                70); // Dell:5
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "DELL", 5d, 5d }, new object[] { "IBM", 70d, 70d } });

            env.Milestone(7);

            SendEvent(env, "AAA", 2000);
            AssertEvents(
                env,
                SYMBOL_DELL,
                5d,
                5d,
                null,
                null);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "IBM", 70d, 70d } });

            env.Milestone(8);

            SendEvent(env, "AAA", 3000);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "AAA", 4000);
            AssertEvents(
                env,
                SYMBOL_IBM,
                70d,
                70d,
                null,
                null);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOne,
            double? oldSumOne,
            double? oldAvgOne,
            double newSumOne,
            double newAvgOne,
            string symbolTwo,
            double? oldSumTwo,
            double? oldAvgTwo,
            double newSumTwo,
            double newAvgTwo)
        {
            env.AssertListener(
                "s0",
                listener => {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetDataListsFlattened(),
                        "mySum,myAvg".SplitCsv(),
                        new object[][] { new object[] { newSumOne, newAvgOne }, new object[] { newSumTwo, newAvgTwo } },
                        new object[][]
                            { new object[] { oldSumOne, oldAvgOne }, new object[] { oldSumTwo, oldAvgTwo } });
                });
        }

        private static void SendEventSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendEventSBAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int longPrimitive,
            long expected)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
            env.AssertEqualsNew("s0", "thesum", expected);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendAssertIntArray(
            RegressionEnvironment env,
            string id,
            int[] array,
            int value,
            int expected)
        {
            var fields = "thesum".SplitCsv();
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            env.AssertPropsNew("s0", fields, new object[] { expected });
        }
    }
} // end of namespace