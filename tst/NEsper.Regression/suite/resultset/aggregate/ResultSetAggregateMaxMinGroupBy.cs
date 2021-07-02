///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateMaxMinGroupBy
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMax());
            execs.Add(new ResultSetAggregateMinMaxOM());
            execs.Add(new ResultSetAggregateMinMaxViewCompile());
            execs.Add(new ResultSetAggregateMinMaxJoin());
            execs.Add(new ResultSetAggregateMinNoGroupHaving());
            execs.Add(new ResultSetAggregateMinNoGroupSelectHaving());
            return execs;
        }

        private static void TryAssertionMinMax(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("minVol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("maxVol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("minDistVol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("maxDistVol"));

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

            env.Milestone(0);

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

            env.Milestone(1);

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

            env.Milestone(2);

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
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
            Assert.AreEqual(minVolOld, oldData[0].Get("minVol"));
            Assert.AreEqual(maxVolOld, oldData[0].Get("maxVol"));
            Assert.AreEqual(minDistVolOld, oldData[0].Get("minDistVol"));
            Assert.AreEqual(maxDistVolOld, oldData[0].Get("maxDistVol"));

            Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
            Assert.AreEqual(minVolNew, newData[0].Get("minVol"));
            Assert.AreEqual(maxVolNew, newData[0].Get("maxVol"));
            Assert.AreEqual(minDistVolNew, newData[0].Get("minDistVol"));
            Assert.AreEqual(maxDistVolNew, newData[0].Get("maxDistVol"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long? volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetAggregateMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, " +
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

        internal class ResultSetAggregateMinMaxOM : RegressionExecution
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
                          "from " + nameof(SupportMarketDataBean) + "#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionMinMax(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMinMaxViewCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, " +
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

        internal class ResultSetAggregateMinMaxJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, " +
                          "min(Volume) as minVol," +
                          "max(Volume) as maxVol," +
                          "min(distinct Volume) as minDistVol," +
                          "max(distinct Volume) as maxDistVol" +
                          " from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionMinMax(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMinNoGroupHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol from SupportMarketDataBean#time(5 sec) " +
                               "having Volume > min(Volume) * 1.3";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "DELL", 100L);
                SendEvent(env, "DELL", 105L);
                SendEvent(env, "DELL", 100L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendEvent(env, "DELL", 131L);
                Assert.AreEqual("DELL", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                SendEvent(env, "DELL", 132L);
                Assert.AreEqual("DELL", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                SendEvent(env, "DELL", 129L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMinNoGroupSelectHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, min(Volume) as mymin from SupportMarketDataBean#length(5) " +
                               "having Volume > min(Volume) * 1.3";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "DELL", 100L);
                SendEvent(env, "DELL", 105L);
                SendEvent(env, "DELL", 100L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "DELL", 131L);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("DELL", theEvent.Get("Symbol"));
                Assert.AreEqual(100L, theEvent.Get("mymin"));

                env.Milestone(1);

                SendEvent(env, "DELL", 132L);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("DELL", theEvent.Get("Symbol"));
                Assert.AreEqual(100L, theEvent.Get("mymin"));

                SendEvent(env, "DELL", 129L);
                SendEvent(env, "DELL", 125L);
                SendEvent(env, "DELL", 125L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "DELL", 170L);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("DELL", theEvent.Get("Symbol"));
                Assert.AreEqual(125L, theEvent.Get("mymin"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace