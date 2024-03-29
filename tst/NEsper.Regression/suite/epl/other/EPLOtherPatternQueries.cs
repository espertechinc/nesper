///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPatternQueries
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithWhereOM(execs);
            WithWhereCompile(execs);
            WithWhere(execs);
            WithAggregation(execs);
            WithFollowedByAndWindow(execs);
            With(PatternWindow)(execs);
#endif
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

        private class EPLOtherWhereOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("s0.Id", "idS0")
                    .AddWithAsProvidedName("s1.Id", "idS1");
                PatternExpr pattern = Patterns.Or()
                    .Add(Patterns.EveryFilter("SupportBean_S0", "s0"))
                    .Add(
                        Patterns.EveryFilter("SupportBean_S1", "s1")
                    );
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model.WhereClause = Expressions.Or()
                    .Add(
                        Expressions.And()
                            .Add(Expressions.IsNotNull("s0.Id"))
                            .Add(Expressions.Lt("s0.Id", 100))
                    )
                    .Add(
                        Expressions.And()
                            .Add(Expressions.IsNotNull("s1.Id"))
                            .Add(Expressions.Ge("s1.Id", 100))
                    );
                model = env.CopyMayFail(model);

                var reverse = model.ToEPL();
                var stmtText = "select s0.Id as idS0, s1.Id as idS1 " +
                               "from pattern [every s0=SupportBean_S0" +
                               " or every s1=SupportBean_S1] " +
                               "where s0.Id is not null and s0.Id<100 or s1.Id is not null and s1.Id>=100";
                ClassicAssert.AreEqual(stmtText, reverse);

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        private class EPLOtherWhereCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s0.Id as idS0, s1.Id as idS1 " +
                               "from pattern [every s0=SupportBean_S0" +
                               " or every s1=SupportBean_S1] " +
                               "where s0.Id is not null and s0.Id<100 or s1.Id is not null and s1.Id>=100";
                var model = env.EplToModel(stmtText);
                model = env.CopyMayFail(model);

                var reverse = model.ToEPL();
                ClassicAssert.AreEqual(stmtText, reverse);

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        private class EPLOtherWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s0.Id as idS0, s1.Id as idS1 " +
                               "from pattern [every s0=SupportBean_S0" +
                               " or every s1=SupportBean_S1] " +
                               "where (s0.Id is not null and s0.Id < 100) or (s1.Id is not null and s1.Id >= 100)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertEventIds(env, 1, null);

                SendEventS0(env, 101);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 100);
                AssertEventIds(env, null, 100);

                env.UndeployAll();
            }
        }

        private class EPLOtherAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select sum(s0.Id) as sumS0, sum(s1.Id) as sumS1, sum(s0.Id + s1.Id) as sumS0S1 " +
                    "from pattern [every s0=SupportBean_S0" +
                    " or every s1=SupportBean_S1]";
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

        private class EPLOtherFollowedByAndWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream a.Id as idA, b.Id as idB, " +
                               "a.P00 as p00A, b.P00 as p00B from pattern [every a=SupportBean_S0" +
                               " -> every b=SupportBean_S0(P00=a.P00)]#time(1)";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AdvanceTime(0);

                SendEvent(env, 1, "e1a");
                env.AssertListenerNotInvoked("s0");
                SendEvent(env, 2, "e1a");
                AssertNewEvent(env, 1, 2, "e1a");

                env.AdvanceTime(500);
                SendEvent(env, 10, "e2a");
                SendEvent(env, 11, "e2b");
                SendEvent(env, 12, "e2c");
                env.AssertListenerNotInvoked("s0");
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
                    "@name('s0') select irstream * from pattern [every(s0=SupportMarketDataBean(Symbol='S0') and " +
                    "s1=SupportMarketDataBean(Symbol='S1'))]#length(1)";
                env.CompileDeploy(text).AddListener("s0");

                env.Milestone(0);

                var eventOne = MakeMarketDataEvent("S0");
                env.SendEventBean(eventOne);

                env.Milestone(1);

                var eventTwo = MakeMarketDataEvent("S1");
                env.SendEventBean(eventTwo);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        ClassicAssert.AreEqual(eventOne.Symbol, @event.Get("s0.Symbol"));
                        ClassicAssert.AreEqual(eventTwo.Symbol, @event.Get("s1.Symbol"));
                    });

                env.Milestone(2);

                var eventThree = MakeMarketDataEvent("S1");
                env.SendEventBean(eventThree);
                env.AssertListenerNotInvoked("s0");

                var eventFour = MakeMarketDataEvent("S0");
                env.SendEventBean(eventFour);

                env.AssertListener(
                    "s0",
                    listener => {
                        var @event = listener.LastOldData[0];
                        ClassicAssert.AreEqual(eventOne.Symbol, @event.Get("s0.Symbol"));
                        ClassicAssert.AreEqual(eventTwo.Symbol, @event.Get("s1.Symbol"));
                        @event = listener.LastNewData[0];
                        ClassicAssert.AreEqual(eventFour.Symbol, @event.Get("s0.Symbol"));
                        ClassicAssert.AreEqual(eventThree.Symbol, @event.Get("s1.Symbol"));
                    });

                env.UndeployAll();
            }

            private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                bean.Id = "1";
                return bean;
            }
        }

        private static void AssertNewEvent(
            RegressionEnvironment env,
            int idA,
            int idB,
            string p00)
        {
            env.AssertEventNew("s0", eventBean => CompareEvent(eventBean, idA, idB, p00));
        }

        private static void AssertOldEvent(
            RegressionEnvironment env,
            int idA,
            int idB,
            string p00)
        {
            env.AssertEventOld("s0", eventBean => CompareEvent(eventBean, idA, idB, p00));
        }

        private static void CompareEvent(
            EventBean eventBean,
            int idA,
            int idB,
            string p00)
        {
            ClassicAssert.AreEqual(idA, eventBean.Get("idA"));
            ClassicAssert.AreEqual(idB, eventBean.Get("idB"));
            ClassicAssert.AreEqual(p00, eventBean.Get("p00A"));
            ClassicAssert.AreEqual(p00, eventBean.Get("p00B"));
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
            env.AssertListener(
                "s0",
                listener => {
                    var eventBean = listener.GetAndResetLastNewData()[0];
                    ClassicAssert.AreEqual(idS0, eventBean.Get("idS0"));
                    ClassicAssert.AreEqual(idS1, eventBean.Get("idS1"));
                    listener.Reset();
                });
        }

        private static void AssertEventSums(
            RegressionEnvironment env,
            int? sumS0,
            int? sumS1,
            int? sumS0S1)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var eventBean = listener.GetAndResetLastNewData()[0];
                    ClassicAssert.AreEqual(sumS0, eventBean.Get("sumS0"));
                    ClassicAssert.AreEqual(sumS1, eventBean.Get("sumS1"));
                    ClassicAssert.AreEqual(sumS0S1, eventBean.Get("sumS0S1"));
                    listener.Reset();
                });
        }
    }
} // end of namespace