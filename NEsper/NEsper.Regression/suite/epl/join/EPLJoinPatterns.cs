///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinPatterns
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinPatternFilterJoin());
            execs.Add(new EPLJoin2PatternJoinSelect());
            execs.Add(new EPLJoin2PatternJoinWildcard());
            return execs;
        }

        private static SupportBean_S0 SendEventS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            var theEvent = new SupportBean_S0(id, p00);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static SupportBean_S1 SendEventS1(
            RegressionEnvironment env,
            int id,
            string p10)
        {
            var theEvent = new SupportBean_S1(id, p10);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static SupportBean_S2 SendEventS2(
            RegressionEnvironment env,
            int id,
            string p20)
        {
            var theEvent = new SupportBean_S2(id, p20);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static SupportBean_S3 SendEventS3(
            RegressionEnvironment env,
            int id,
            string p30)
        {
            var theEvent = new SupportBean_S3(id, p30);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void AssertEventData(
            EventBean theEvent,
            int s0es0Id,
            int s0es1Id,
            int s1es2Id,
            int s1es3Id,
            string p00,
            string p10,
            string p20,
            string p30)
        {
            Assert.AreEqual(s0es0Id, theEvent.Get("s0es0Id"));
            Assert.AreEqual(s0es1Id, theEvent.Get("s0es1Id"));
            Assert.AreEqual(s1es2Id, theEvent.Get("s1es2Id"));
            Assert.AreEqual(s1es3Id, theEvent.Get("s1es3Id"));
            Assert.AreEqual(p00, theEvent.Get("es0P00"));
            Assert.AreEqual(p10, theEvent.Get("es1P10"));
            Assert.AreEqual(p20, theEvent.Get("es2P20"));
            Assert.AreEqual(p30, theEvent.Get("es3P30"));
        }

        private static void AssertEventData(
            EventBean theEvent,
            int? es0aId,
            string es0ap00,
            int? es0bId,
            string es0bp00,
            int s1Id,
            string s1p10)
        {
            Assert.AreEqual(es0aId, theEvent.Get("es0aId"));
            Assert.AreEqual(es0ap00, theEvent.Get("es0aP00"));
            Assert.AreEqual(es0bId, theEvent.Get("es0bId"));
            Assert.AreEqual(es0bp00, theEvent.Get("es0bP00"));
            Assert.AreEqual(s1Id, theEvent.Get("s1Id"));
            Assert.AreEqual(s1p10, theEvent.Get("s1P10"));
        }

        internal class EPLJoinPatternFilterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream es0a.Id as es0aId, " +
                               "es0a.P00 as es0aP00, " +
                               "es0b.Id as es0bId, " +
                               "es0b.P00 as es0bP00, " +
                               "S1.Id as s1Id, " +
                               "S1.P10 as s1P10 " +
                               " from " +
                               " pattern [every (es0a=SupportBean_S0(P00='a') " +
                               "or es0b=SupportBean_S0(P00='b'))]#length(5) as S0," +
                               "SupportBean_S1#length(5) as S1" +
                               " where (es0a.Id = S1.Id) or (es0b.Id = S1.Id)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS1(env, 1, "s1A");
                SendEventS0(env, 2, "a");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 1, "b");
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, null, null, 1, "b", 1, "s1A");

                SendEventS1(env, 2, "s2A");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");

                SendEventS1(env, 20, "s20A");
                SendEventS1(env, 30, "s30A");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 20, "a");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, 20, "a", null, null, 20, "s20A");

                SendEventS0(env, 20, "b");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, null, null, 20, "b", 20, "s20A");

                SendEventS0(env, 30, "c"); // filtered out
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 40, "a"); // not matching Id in s1
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 50, "b"); // pushing an event s0(2, "a") out the window
                theEvent = env.Listener("s0").AssertOneGetOldAndReset();
                AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");

                // stop statement
                var listener = env.Listener("s0");
                env.UndeployAll();

                SendEventS1(env, 60, "s20");
                SendEventS0(env, 70, "a");
                SendEventS0(env, 71, "b");
                Assert.IsFalse(listener.GetAndClearIsInvoked());

                // start statement
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS1(env, 70, "s1-70");
                SendEventS0(env, 60, "a");
                SendEventS1(env, 20, "s1");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 70, "b");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, null, null, 70, "b", 70, "s1-70");

                env.UndeployAll();
            }
        }

        internal class EPLJoin2PatternJoinSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream S0.es0.Id as s0es0Id," +
                               "S0.es1.Id as s0es1Id, " +
                               "S1.es2.Id as s1es2Id, " +
                               "S1.es3.Id as s1es3Id, " +
                               "es0.P00 as es0P00, " +
                               "es1.P10 as es1P10, " +
                               "es2.P20 as es2P20, " +
                               "es3.P30 as es3P30" +
                               " from " +
                               " pattern [every (es0=SupportBean_S0" +
                               " and es1=SupportBean_S1" +
                               ")]#length(3) as S0," +
                               " pattern [every (es2=SupportBean_S2" +
                               " and es3=SupportBean_S3)]#length(3) as S1" +
                               " where S0.es0.Id = S1.es2.Id";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS3(env, 2, "d");
                SendEventS0(env, 3, "a");
                SendEventS2(env, 3, "c");
                SendEventS1(env, 1, "b");
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");

                SendEventS0(env, 11, "a1");
                SendEventS2(env, 13, "c1");
                SendEventS1(env, 12, "b1");
                SendEventS3(env, 15, "d1");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS3(env, 25, "d2");
                SendEventS0(env, 21, "a2");
                SendEventS2(env, 21, "c2");
                SendEventS1(env, 26, "b2");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, 21, 26, 21, 25, "a2", "b2", "c2", "d2");

                SendEventS0(env, 31, "a3");
                SendEventS1(env, 32, "b3");
                theEvent = env.Listener("s0").AssertOneGetOldAndReset(); // event moving out of window
                AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");
                SendEventS2(env, 33, "c3");
                SendEventS3(env, 35, "d3");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEventS0(env, 41, "a4");
                SendEventS2(env, 43, "c4");
                SendEventS1(env, 42, "b4");
                SendEventS3(env, 45, "d4");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // stop statement
                var listener = env.Listener("s0");
                env.UndeployAll();

                SendEventS3(env, 52, "d5");
                SendEventS0(env, 53, "a5");
                SendEventS2(env, 53, "c5");
                SendEventS1(env, 51, "b5");
                Assert.IsFalse(listener.IsInvoked);

                // start statement
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS3(env, 55, "d6");
                SendEventS0(env, 51, "a6");
                SendEventS2(env, 51, "c6");
                SendEventS1(env, 56, "b6");
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertEventData(theEvent, 51, 56, 51, 55, "a6", "b6", "c6", "d6");

                env.UndeployAll();
            }
        }

        internal class EPLJoin2PatternJoinWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * " +
                               " from " +
                               " pattern [every (es0=SupportBean_S0" +
                               " and es1=SupportBean_S1)]#length(5) as S0," +
                               " pattern [every (es2=SupportBean_S2" +
                               " and es3=SupportBean_S3)]#length(5) as S1" +
                               " where S0.es0.Id = S1.es2.Id";
                env.CompileDeploy(stmtText).AddListener("s0");

                var s0 = SendEventS0(env, 100, "");
                var s1 = SendEventS1(env, 1, "");
                var s2 = SendEventS2(env, 100, "");
                var s3 = SendEventS3(env, 2, "");

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();

                var result = theEvent.Get("S0")
                    .UnwrapStringDictionary()
                    .TransformLeft<string, object, EventBean>();

                Assert.AreSame(s0, result.Get("es0").Underlying);
                Assert.AreSame(s1, result.Get("es1").Underlying);

                result = theEvent.Get("S1")
                    .UnwrapStringDictionary()
                    .TransformLeft<string, object, EventBean>();

                Assert.AreSame(s2, result.Get("es2").Underlying);
                Assert.AreSame(s3, result.Get("es3").Underlying);

                env.UndeployAll();
            }
        }
    }
} // end of namespace