///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinSingleOp3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            Withd(execs);
            WithdOM(execs);
            With(dCompile)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithdCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinSingleOp3StreamUniquePerIdCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithdOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinSingleOp3StreamUniquePerIdOM());
            return execs;
        }

        public static IList<RegressionExecution> Withd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinSingleOp3StreamUniquePerId());
            return execs;
        }

        private static void RunJoinUniquePerId(RegressionEnvironment env)
        {
            var eventsA = new SupportBean_A[10];
            var eventsB = new SupportBean_B[10];
            var eventsC = new SupportBean_C[10];
            for (var i = 0; i < eventsA.Length; i++) {
                eventsA[i] = new SupportBean_A(Convert.ToString(i));
                eventsB[i] = new SupportBean_B(Convert.ToString(i));
                eventsC[i] = new SupportBean_C(Convert.ToString(i));
            }

            // Test sending a C event
            SendEvent(env, eventsA[0]);
            SendEvent(env, eventsB[0]);
            env.AssertListenerNotInvoked("s0");
            SendEvent(env, eventsC[0]);
            AssertEventsReceived(env, eventsA[0], eventsB[0], eventsC[0]);

            // Test sending a B event
            SendEvent(env, new object[] { eventsA[1], eventsB[2], eventsC[3] });
            SendEvent(env, eventsC[1]);
            env.AssertListenerNotInvoked("s0");
            SendEvent(env, eventsB[1]);
            AssertEventsReceived(env, eventsA[1], eventsB[1], eventsC[1]);

            // Test sending a C event
            SendEvent(env, new object[] { eventsA[4], eventsA[5], eventsB[4], eventsB[3] });
            env.AssertListenerNotInvoked("s0");
            SendEvent(env, eventsC[4]);
            AssertEventsReceived(env, eventsA[4], eventsB[4], eventsC[4]);
        }

        private static void AssertEventsReceived(
            RegressionEnvironment env,
            SupportBean_A eventA,
            SupportBean_B eventB,
            SupportBean_C eventC)
        {
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                    ClassicAssert.AreSame(eventA, listener.LastNewData[0].Get("streamA"));
                    ClassicAssert.AreSame(eventB, listener.LastNewData[0].Get("streamB"));
                    ClassicAssert.AreSame(eventC, listener.LastNewData[0].Get("streamC"));
                    listener.Reset();
                });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        internal class EPLJoinSingleOp3StreamUniquePerId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from " +
                          "SupportBean_A#length(3) as streamA," +
                          "SupportBean_B#length(3) as streamB," +
                          "SupportBean_C#length(3) as streamC" +
                          " where (streamA.Id = streamB.Id) " +
                          "   and (streamB.Id = streamC.Id)" +
                          "   and (streamA.Id = streamC.Id)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                RunJoinUniquePerId(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoinSingleOp3StreamUniquePerIdOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                var fromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_A", "streamA")
                        .AddView(View.Create("length", Expressions.Constant(3))),
                    FilterStream.Create("SupportBean_B", "streamB")
                        .AddView(View.Create("length", Expressions.Constant(3))),
                    FilterStream.Create("SupportBean_C", "streamC")
                        .AddView(View.Create("length", Expressions.Constant(3))));
                model.FromClause = fromClause;
                model.WhereClause = Expressions.And(
                    Expressions.EqProperty("streamA.Id", "streamB.Id"),
                    Expressions.EqProperty("streamB.Id", "streamC.Id"),
                    Expressions.EqProperty("streamA.Id", "streamC.Id"));
                model = env.CopyMayFail(model);

                var epl = "select * from " +
                          "SupportBean_A#length(3) as streamA, " +
                          "SupportBean_B#length(3) as streamB, " +
                          "SupportBean_C#length(3) as streamC " +
                          "where streamA.Id=streamB.Id " +
                          "and streamB.Id=streamC.Id " +
                          "and streamA.Id=streamC.Id";
                ClassicAssert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                RunJoinUniquePerId(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoinSingleOp3StreamUniquePerIdCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from " +
                          "SupportBean_A#length(3) as streamA, " +
                          "SupportBean_B#length(3) as streamB, " +
                          "SupportBean_C#length(3) as streamC " +
                          "where streamA.Id=streamB.Id " +
                          "and streamB.Id=streamC.Id " +
                          "and streamA.Id=streamC.Id";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                RunJoinUniquePerId(env);

                env.UndeployAll();
            }
        }
    }
} // end of namespace