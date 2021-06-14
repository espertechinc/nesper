///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPatternQueries
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithWhereOM(execs);
            WithWhereCompile(execs);
            WithWhere(execs);
            WithAggregation(execs);
            WithFollowedByAndWindow(execs);
            WithPatternWindow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherPatternWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByAndWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFollowedByAndWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWhereCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWhereOM());
            return execs;
        }

        private static void AssertNewEvent(
            RegressionEnvironment env,
            int idA,
            int idB,
            string p00)
        {
            var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }

        private static void AssertOldEvent(
            RegressionEnvironment env,
            int idA,
            int idB,
            string p00)
        {
            var eventBean = env.Listener("s0").AssertOneGetOldAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }

        private static void CompareEvent(
            EventBean eventBean,
            int idA,
            int idB,
            string p00)
        {
            Assert.AreEqual(idA, eventBean.Get("idA"));
            Assert.AreEqual(idB, eventBean.Get("idB"));
            Assert.AreEqual(p00, eventBean.Get("P00A"));
            Assert.AreEqual(p00, eventBean.Get("P00B"));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            var theEvent = new SupportBean_S0(id, p00);
            env.SendEventBean(theEvent);
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id)
        {
            var theEvent = new SupportBean_S0(id);
            env.SendEventBean(theEvent);
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            int id)
        {
            var theEvent = new SupportBean_S1(id);
            env.SendEventBean(theEvent);
        }

        private static void AssertEventIds(
            RegressionEnvironment env,
            int? idS0,
            int? idS1)
        {
            var eventBean = env.Listener("s0").GetAndResetLastNewData()[0];
            Assert.AreEqual(idS0, eventBean.Get("idS0"));
            Assert.AreEqual(idS1, eventBean.Get("idS1"));
            env.Listener("s0").Reset();
        }

        private static void AssertEventSums(
            RegressionEnvironment env,
            int? sumS0,
            int? sumS1,
            int? sumS0S1)
        {
            var eventBean = env.Listener("s0").GetAndResetLastNewData()[0];
            Assert.AreEqual(sumS0, eventBean.Get("sumS0"));
            Assert.AreEqual(sumS1, eventBean.Get("sumS1"));
            Assert.AreEqual(sumS0S1, eventBean.Get("sumS0S1"));
            env.Listener("s0").Reset();
        }

        internal class EPLOtherWhereOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("S0.Id", "idS0")
                    .AddWithAsProvidedName("S1.Id", "idS1");
                PatternExpr pattern = Patterns.Or()
                    .Add(Patterns.EveryFilter("SupportBean_S0", "S0"))
                    .Add(
                        Patterns.EveryFilter("SupportBean_S1", "S1")
                    );
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model.WhereClause = Expressions.Or()
                    .Add(
                        Expressions.And()
                            .Add(Expressions.IsNotNull("S0.Id"))
                            .Add(Expressions.Lt("S0.Id", 100))
                    )
                    .Add(
                        Expressions.And()
                            .Add(Expressions.IsNotNull("S1.Id"))
                            .Add(Expressions.Ge("S1.Id", 100))
                    );
                model = env.CopyMayFail(model);

                var reverse = model.ToEPL();
                var stmtText = "select S0.Id as idS0, S1.Id as idS1 " +
                               "from pattern [every S0=SupportBean_S0" +
                               " or every S1=SupportBean_S1] " +
                               "where S0.Id is not null and S0.Id<100 or S1.Id is not null and S1.Id>=100";
                Assert.AreEqual(stmtText, reverse);

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        internal class EPLOtherWhereCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id as idS0, S1.Id as idS1 " +
                               "from pattern [every S0=SupportBean_S0" +
                               " or every S1=SupportBean_S1] " +
                               "where S0.Id is not null and S0.Id<100 or S1.Id is not null and S1.Id>=100";
                var model = env.EplToModel(stmtText);
                model = env.CopyMayFail(model);

                var reverse = model.ToEPL();
                Assert.AreEqual(stmtText, reverse);

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        internal class EPLOtherWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id as idS0, S1.Id as idS1 " +
                               "from pattern [every S0=SupportBean_S0" +
                               " or every S1=SupportBean_S1] " +
                               "where (S0.Id is not null and S0.Id < 100) or (S1.Id is not null and S1.Id >= 100)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        internal class EPLOtherAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select sum(S0.Id) as sumS0, sum(S1.Id) as sumS1, sum(S0.Id + S1.Id) as sumS0S1 " +
                    "from pattern [every S0=SupportBean_S0" +
                    " or every S1=SupportBean_S1]";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventSums(env, 1, null, null);

                SendEventS1(env, 2);
                AssertEventSums(env, 1, 2, null);

                SendEventS1(env, 10);
                AssertEventSums(env, 1, 12, null);

                SendEventS0(env, 20);
                AssertEventSums(env, 21, 12, null);

                env.UndeployAll();
            }
        }

        internal class EPLOtherFollowedByAndWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream a.Id as idA, b.Id as idB, " +
                               "a.P00 as P00A, b.P00 as P00B from pattern [every a=SupportBean_S0" +
                               " -> every b=SupportBean_S0(P00=a.P00)]#time(1)";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AdvanceTime(0);

                SendEvent(env, 1, "e1a");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendEvent(env, 2, "e1a");
                AssertNewEvent(env, 1, 2, "e1a");

                env.AdvanceTime(500);
                SendEvent(env, 10, "e2a");
                SendEvent(env, 11, "e2b");
                SendEvent(env, 12, "e2c");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendEvent(env, 13, "e2b");
                AssertNewEvent(env, 11, 13, "e2b");

                env.AdvanceTime(1000);
                AssertOldEvent(env, 1, 2, "e1a");

                env.AdvanceTime(1500);
                AssertOldEvent(env, 11, 13, "e2b");

                env.UndeployAll();
            }
        }

        public class EPLOtherPatternWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select irstream * from pattern [every(S0=SupportMarketDataBean(Symbol='S0') and " +
                    "S1=SupportMarketDataBean(Symbol='S1'))]#length(1)";
                env.CompileDeploy(text).AddListener("s0");

                env.Milestone(0);

                var eventOne = MakeMarketDataEvent("S0");
                env.SendEventBean(eventOne);

                env.Milestone(1);

                var eventTwo = MakeMarketDataEvent("S1");
                env.SendEventBean(eventTwo);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(eventOne.Symbol, @event.Get("S0.Symbol"));
                Assert.AreEqual(eventTwo.Symbol, @event.Get("S1.Symbol"));
                env.Listener("s0").Reset();

                env.Milestone(2);

                var eventThree = MakeMarketDataEvent("S1");
                env.SendEventBean(eventThree);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var eventFour = MakeMarketDataEvent("S0");
                env.SendEventBean(eventFour);

                @event = env.Listener("s0").LastOldData[0];
                Assert.AreEqual(eventOne.Symbol, @event.Get("S0.Symbol"));
                Assert.AreEqual(eventTwo.Symbol, @event.Get("S1.Symbol"));
                @event = env.Listener("s0").LastNewData[0];
                Assert.AreEqual(eventFour.Symbol, @event.Get("S0.Symbol"));
                Assert.AreEqual(eventThree.Symbol, @event.Get("S1.Symbol"));

                env.UndeployAll();
            }

            private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                bean.Id = "1";
                return bean;
            }
        }
    }
} // end of namespace