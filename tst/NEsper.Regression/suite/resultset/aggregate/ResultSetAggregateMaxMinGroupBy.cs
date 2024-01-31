///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateMaxMinGroupBy
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMax(execs);
            WithMaxOM(execs);
            WithMaxViewCompile(execs);
            WithMaxJoin(execs);
            WithNoGroupHaving(execs);
            WithNoGroupSelectHaving(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNoGroupSelectHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinNoGroupSelectHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithNoGroupHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinNoGroupHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithMaxJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMaxJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithMaxViewCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMaxViewCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithMaxOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMaxOM());
            return execs;
        }

        public static IList<RegressionExecution> WithMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMax());
            return execs;
        }

        private class ResultSetAggregateMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select irstream Symbol, " +
                          "min(all Volume) as minVol," +
                          "max(all Volume) as maxVol," +
                          "min(distinct Volume) as minDistVol," +
                          "max(distinct Volume) as maxDistVol" +
                          " from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionMinMax(env, milestone);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinMaxOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("Symbol")
                    .Add(Expressions.Min("Volume"), "minVol")
                    .Add(Expressions.Max("Volume"), "maxVol")
                    .Add(Expressions.MinDistinct("Volume"), "minDistVol")
                    .Add(Expressions.MaxDistinct("Volume"), "maxDistVol");

                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(3)));
                model.WhereClause = Expressions.Or()
                    .Add(Expressions.Eq("Symbol", "DELL"))
                    .Add(Expressions.Eq("Symbol", "IBM"))
                    .Add(Expressions.Eq("Symbol", "GE"));
                model.GroupByClause = GroupByClause.Create("Symbol");
                model = env.CopyMayFail(model);

                var epl = "select irstream Symbol, " +
                          "min(Volume) as minVol, " +
                          "max(Volume) as maxVol, " +
                          "min(distinct Volume) as minDistVol, " +
                          "max(distinct Volume) as maxDistVol " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                ClassicAssert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionMinMax(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinMaxViewCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol, " +
                          "min(Volume) as minVol, " +
                          "max(Volume) as maxVol, " +
                          "min(distinct Volume) as minDistVol, " +
                          "max(distinct Volume) as maxDistVol " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionMinMax(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinMaxJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select irstream Symbol, " +
                          "min(Volume) as minVol," +
                          "max(Volume) as maxVol," +
                          "min(distinct Volume) as minDistVol," +
                          "max(distinct Volume) as maxDistVol" +
                          " from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionMinMax(env, milestone);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinNoGroupHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol from SupportMarketDataBean#time(5 sec) " +
                               "having Volume > min(Volume) * 1.3";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "DELL", 100L);
                SendEvent(env, "DELL", 105L);
                SendEvent(env, "DELL", 100L);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendEvent(env, "DELL", 131L);
                env.AssertEqualsNew("s0", "Symbol", "DELL");

                SendEvent(env, "DELL", 132L);
                env.AssertEqualsNew("s0", "Symbol", "DELL");

                env.Milestone(2);

                SendEvent(env, "DELL", 129L);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinNoGroupSelectHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,mymin".SplitCsv();
                var stmtText = "@name('s0') select Symbol, min(Volume) as mymin from SupportMarketDataBean#length(5) " +
                               "having Volume > min(Volume) * 1.3";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "DELL", 100L);
                SendEvent(env, "DELL", 105L);
                SendEvent(env, "DELL", 100L);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "DELL", 131L);
                env.AssertPropsNew("s0", fields, new object[] { "DELL", 100L });

                env.Milestone(1);

                SendEvent(env, "DELL", 132L);
                env.AssertPropsNew("s0", fields, new object[] { "DELL", 100L });

                SendEvent(env, "DELL", 129L);
                SendEvent(env, "DELL", 125L);
                SendEvent(env, "DELL", 125L);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "DELL", 170L);
                env.AssertPropsNew("s0", fields, new object[] { "DELL", 125L });

                env.UndeployAll();
            }
        }

        private static void TryAssertionMinMax(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("minVol"));
                    ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("maxVol"));
                    ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("minDistVol"));
                    ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("maxDistVol"));
                });

            SendEvent(env, SYMBOL_DELL, 50L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                null,
                null,
                null,
                null,
                SYMBOL_DELL,
                50L,
                50L,
                50L,
                50L
            );

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_DELL, 30L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                50L,
                50L,
                50L,
                50L,
                SYMBOL_DELL,
                30L,
                50L,
                30L,
                50L
            );

            SendEvent(env, SYMBOL_DELL, 30L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                30L,
                50L,
                30L,
                50L,
                SYMBOL_DELL,
                30L,
                50L,
                30L,
                50L
            );

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_DELL, 90L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                30L,
                50L,
                30L,
                50L,
                SYMBOL_DELL,
                30L,
                90L,
                30L,
                90L
            );

            SendEvent(env, SYMBOL_DELL, 100L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                30L,
                90L,
                30L,
                90L,
                SYMBOL_DELL,
                30L,
                100L,
                30L,
                100L
            );

            SendEvent(env, SYMBOL_IBM, 20L);
            SendEvent(env, SYMBOL_IBM, 5L);
            SendEvent(env, SYMBOL_IBM, 15L);
            SendEvent(env, SYMBOL_IBM, 18L);
            AssertEvents(
                env,
                SYMBOL_IBM,
                5L,
                20L,
                5L,
                20L,
                SYMBOL_IBM,
                5L,
                18L,
                5L,
                18L
            );

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_IBM, null);
            AssertEvents(
                env,
                SYMBOL_IBM,
                5L,
                18L,
                5L,
                18L,
                SYMBOL_IBM,
                15L,
                18L,
                15L,
                18L
            );

            SendEvent(env, SYMBOL_IBM, null);
            AssertEvents(
                env,
                SYMBOL_IBM,
                15L,
                18L,
                15L,
                18L,
                SYMBOL_IBM,
                18L,
                18L,
                18L,
                18L
            );

            SendEvent(env, SYMBOL_IBM, null);
            AssertEvents(
                env,
                SYMBOL_IBM,
                18L,
                18L,
                18L,
                18L,
                SYMBOL_IBM,
                null,
                null,
                null,
                null
            );
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOld,
            long? minVolOld,
            long? maxVolOld,
            long? minDistVolOld,
            long? maxDistVolOld,
            string symbolNew,
            long? minVolNew,
            long? maxVolNew,
            long? minDistVolNew,
            long? maxDistVolNew)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    ClassicAssert.AreEqual(1, oldData.Length);
                    ClassicAssert.AreEqual(1, newData.Length);

                    ClassicAssert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
                    ClassicAssert.AreEqual(minVolOld, oldData[0].Get("minVol"));
                    ClassicAssert.AreEqual(maxVolOld, oldData[0].Get("maxVol"));
                    ClassicAssert.AreEqual(minDistVolOld, oldData[0].Get("minDistVol"));
                    ClassicAssert.AreEqual(maxDistVolOld, oldData[0].Get("maxDistVol"));

                    ClassicAssert.AreEqual(symbolNew, newData[0].Get("Symbol"));
                    ClassicAssert.AreEqual(minVolNew, newData[0].Get("minVol"));
                    ClassicAssert.AreEqual(maxVolNew, newData[0].Get("maxVol"));
                    ClassicAssert.AreEqual(minDistVolNew, newData[0].Get("minDistVol"));
                    ClassicAssert.AreEqual(maxDistVolNew, newData[0].Get("maxDistVol"));

                    listener.Reset();
                });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long? volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(bean);
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultSetAggregateMaxMinGroupBy));
    }
} // end of namespace