///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithUniquePerId(execs);
            WithNonUniquePerId(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNonUniquePerId(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinNonUniquePerId());
            return execs;
        }

        public static IList<RegressionExecution> WithUniquePerId(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinUniquePerId());
            return execs;
        }

        private static SelectJoinHolder SetupStmt(RegressionEnvironment env)
        {
            var holder = new SelectJoinHolder();

            var epl =
                "@Name('s0') select irstream * from SupportBean_A#length(3) as streamA, SupportBean_B#length(3) as streamB where streamA.Id = streamB.Id";
            holder.stmt = env.CompileDeploy(epl).Statement("s0");
            holder.listener = env.ListenerNew();
            holder.stmt.AddListener(holder.listener);

            Assert.AreEqual(typeof(SupportBean_A), holder.stmt.EventType.GetPropertyType("streamA"));
            Assert.AreEqual(typeof(SupportBean_B), holder.stmt.EventType.GetPropertyType("streamB"));
            Assert.AreEqual(2, holder.stmt.EventType.PropertyNames.Length);

            holder.eventsA = new SupportBean_A[10];
            holder.eventsASetTwo = new SupportBean_A[10];
            holder.eventsB = new SupportBean_B[10];
            holder.eventsBSetTwo = new SupportBean_B[10];
            for (var i = 0; i < holder.eventsA.Length; i++) {
                holder.eventsA[i] = new SupportBean_A(Convert.ToString(i));
                holder.eventsASetTwo[i] = new SupportBean_A(Convert.ToString(i));
                holder.eventsB[i] = new SupportBean_B(Convert.ToString(i));
                holder.eventsBSetTwo[i] = new SupportBean_B(Convert.ToString(i));
            }

            return holder;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        internal class EPLOtherJoinUniquePerId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var holder = SetupStmt(env);

                SendEvent(env, holder.eventsA[0]);
                SendEvent(env, holder.eventsB[1]);
                Assert.IsNull(holder.listener.LastNewData);

                // Test join new B with id 0
                SendEvent(env, holder.eventsB[0]);
                Assert.AreSame(holder.eventsA[0], holder.listener.LastNewData[0].Get("streamA"));
                Assert.AreSame(holder.eventsB[0], holder.listener.LastNewData[0].Get("streamB"));
                Assert.IsNull(holder.listener.LastOldData);
                holder.listener.Reset();

                // Test join new A with id 1
                SendEvent(env, holder.eventsA[1]);
                Assert.AreSame(holder.eventsA[1], holder.listener.LastNewData[0].Get("streamA"));
                Assert.AreSame(holder.eventsB[1], holder.listener.LastNewData[0].Get("streamB"));
                Assert.IsNull(holder.listener.LastOldData);
                holder.listener.Reset();

                SendEvent(env, holder.eventsA[2]);
                Assert.IsNull(holder.listener.LastOldData);

                // Test join old A id 0 leaves length window of 3 events
                SendEvent(env, holder.eventsA[3]);
                Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[0].Get("streamA"));
                Assert.AreSame(holder.eventsB[0], holder.listener.LastOldData[0].Get("streamB"));
                Assert.IsNull(holder.listener.LastNewData);
                holder.listener.Reset();

                // Test join old B id 1 leaves window
                SendEvent(env, holder.eventsB[4]);
                Assert.IsNull(holder.listener.LastOldData);
                SendEvent(env, holder.eventsB[5]);
                Assert.AreSame(holder.eventsA[1], holder.listener.LastOldData[0].Get("streamA"));
                Assert.AreSame(holder.eventsB[1], holder.listener.LastOldData[0].Get("streamB"));
                Assert.IsNull(holder.listener.LastNewData);

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinNonUniquePerId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var holder = SetupStmt(env);

                SendEvent(env, holder.eventsA[0]);
                SendEvent(env, holder.eventsA[1]);
                SendEvent(env, holder.eventsASetTwo[0]);
                Assert.IsTrue(holder.listener.LastOldData == null && holder.listener.LastNewData == null);

                SendEvent(env, holder.eventsB[0]); // Event B id 0 joins to A id 0 twice
                var data = holder.listener.LastNewData;
                Assert.IsTrue(
                    holder.eventsASetTwo[0] == data[0].Get("streamA") ||
                    holder.eventsASetTwo[0] == data[1].Get("streamA")); // Order arbitrary
                Assert.AreSame(holder.eventsB[0], data[0].Get("streamB"));
                Assert.IsTrue(
                    holder.eventsA[0] == data[0].Get("streamA") || holder.eventsA[0] == data[1].Get("streamA"));
                Assert.AreSame(holder.eventsB[0], data[1].Get("streamB"));
                Assert.IsNull(holder.listener.LastOldData);
                holder.listener.Reset();

                SendEvent(env, holder.eventsB[2]);
                SendEvent(env, holder.eventsBSetTwo[0]); // Ignore events generated
                holder.listener.Reset();

                SendEvent(env, holder.eventsA[3]); // Pushes A id 0 out of window, which joins to B id 0 twice
                data = holder.listener.LastOldData;
                Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[0].Get("streamA"));
                Assert.IsTrue(
                    holder.eventsB[0] == data[0].Get("streamB") ||
                    holder.eventsB[0] == data[1].Get("streamB")); // B order arbitrary
                Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[1].Get("streamA"));
                Assert.IsTrue(
                    holder.eventsBSetTwo[0] == data[0].Get("streamB") ||
                    holder.eventsBSetTwo[0] == data[1].Get("streamB"));
                Assert.IsNull(holder.listener.LastNewData);
                holder.listener.Reset();

                SendEvent(env, holder.eventsBSetTwo[2]); // Pushes B id 0 out of window, which joins to A set two id 0
                Assert.AreSame(holder.eventsASetTwo[0], holder.listener.LastOldData[0].Get("streamA"));
                Assert.AreSame(holder.eventsB[0], holder.listener.LastOldData[0].Get("streamB"));
                Assert.AreEqual(1, holder.listener.LastOldData.Length);

                env.UndeployAll();
            }
        }

        internal class SelectJoinHolder
        {
            internal SupportBean_A[] eventsA;
            internal SupportBean_A[] eventsASetTwo;
            internal SupportBean_B[] eventsB;
            internal SupportBean_B[] eventsBSetTwo;
            internal SupportListener listener;
            internal EPStatement stmt;
        }
    }
} // end of namespace