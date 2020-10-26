///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinUnidirectional
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With2Stream(execs);
            With3StreamAllUnidirectional(execs);
            With3StreamMixed(execs);
            With4StreamWhereClause(execs);
            WithOuterInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOuterInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterInvalid());
            return execs;
        }

        public static IList<RegressionExecution> With4StreamWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin4StreamWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> With3StreamMixed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3StreamMixed());
            return execs;
        }

        public static IList<RegressionExecution> With3StreamAllUnidirectional(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3StreamAllUnidirectional(false));
            execs.Add(new EPLJoin3StreamAllUnidirectional(true));
            return execs;
        }

        public static IList<RegressionExecution> With2Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2Stream());
            return execs;
        }

        private static void SendAssert(
            RegressionEnvironment env,
            SupportBeanAtoFBase @event,
            bool b)
        {
            env.SendEventBean(@event);
            Assert.AreEqual(b, env.Listener("s0").GetAndClearIsInvoked());
        }

        private static void AssertReceived2Stream(
            RegressionEnvironment env,
            string a,
            string b)
        {
            var fields = new[] {"aId", "bId"};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {a, b});
        }

        private static void AssertReceived3Stream(
            RegressionEnvironment env,
            string a,
            string b,
            string c)
        {
            var fields = new[] {"a.Id", "b.Id", "c.Id"};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {a, b, c});
        }

        private static void AssertReceived3StreamMixed(
            RegressionEnvironment env,
            string a,
            string b,
            string c,
            string d)
        {
            var fields = new[] {"aId", "bId", "cId", "dId"};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {a, b, c, d});
        }

        public class EPLJoinOuterInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // all: unidirectional and full-outer-join

                // no-view-declared
                TryInvalidCompile(
                    env,
                    "select * from SupportBean_A unidirectional full outer join SupportBean_B#keepall unidirectional",
                    "The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");

                // not-all-unidirectional
                TryInvalidCompile(
                    env,
                    "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional full outer join SupportBean_C#keepall",
                    "The unidirectional keyword must either apply to a single stream or all streams in a full outer join");

                // no iterate
                TryInvalidIterate(
                    env,
                    "@Name('s0') select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional",
                    "Iteration over a unidirectional join is not supported");
            }
        }

        internal class EPLJoin2Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select a.Id as aId, b.Id as bId from SupportBean_A as a unidirectional " +
                          "full outer join SupportBean_B as b unidirectional";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_A("A1"));
                AssertReceived2Stream(env, "A1", null);

                env.SendEventBean(new SupportBean_B("B1"));
                AssertReceived2Stream(env, null, "B1");

                env.SendEventBean(new SupportBean_B("B2"));
                AssertReceived2Stream(env, null, "B2");

                env.SendEventBean(new SupportBean_A("A2"));
                AssertReceived2Stream(env, "A2", null);

                env.UndeployAll();
            }
        }

        internal class EPLJoin3StreamAllUnidirectional : RegressionExecution
        {
            private readonly bool soda;

            public EPLJoin3StreamAllUnidirectional(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean_A as a unidirectional " +
                          "full outer join SupportBean_B as b unidirectional " +
                          "full outer join SupportBean_C as c unidirectional";

                env.CompileDeploy(soda, epl).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean_A("A1"));
                AssertReceived3Stream(env, "A1", null, null);

                env.SendEventBean(new SupportBean_C("C1"));
                AssertReceived3Stream(env, null, null, "C1");

                env.SendEventBean(new SupportBean_C("C2"));
                AssertReceived3Stream(env, null, null, "C2");

                env.SendEventBean(new SupportBean_A("A2"));
                AssertReceived3Stream(env, "A2", null, null);

                env.SendEventBean(new SupportBean_B("B1"));
                AssertReceived3Stream(env, null, "B1", null);

                env.SendEventBean(new SupportBean_B("B2"));
                AssertReceived3Stream(env, null, "B2", null);

                env.UndeployAll();
            }
        }

        internal class EPLJoin3StreamMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyCWindow#keepall as SupportBean_C;\n" +
                          "insert into MyCWindow select * from SupportBean_C;\n" +
                          "@Name('s0') select a.Id as aId, b.Id as bId, MyCWindow.Id as cId, SupportBean_D.Id as dId " +
                          "from pattern[every a=SupportBean_A -> b=SupportBean_B] t1 unidirectional " +
                          "full outer join " +
                          "MyCWindow unidirectional " +
                          "full outer join " +
                          "SupportBean_D unidirectional;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_C("c1"));
                AssertReceived3StreamMixed(env, null, null, "c1", null);

                env.SendEventBean(new SupportBean_A("a1"));
                env.SendEventBean(new SupportBean_B("b1"));
                AssertReceived3StreamMixed(env, "a1", "b1", null, null);

                env.SendEventBean(new SupportBean_A("a2"));
                env.SendEventBean(new SupportBean_B("b2"));
                AssertReceived3StreamMixed(env, "a2", "b2", null, null);

                env.SendEventBean(new SupportBean_D("d1"));
                AssertReceived3StreamMixed(env, null, null, null, "d1");

                env.UndeployAll();
            }
        }

        internal class EPLJoin4StreamWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean_A as a unidirectional " +
                          "full outer join SupportBean_B as b unidirectional " +
                          "full outer join SupportBean_C as c unidirectional " +
                          "full outer join SupportBean_D as d unidirectional " +
                          "where coalesce(a.Id,b.Id,c.Id,d.Id) in ('YES')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendAssert(env, new SupportBean_A("A1"), false);
                SendAssert(env, new SupportBean_A("YES"), true);
                SendAssert(env, new SupportBean_C("YES"), true);
                SendAssert(env, new SupportBean_C("C1"), false);
                SendAssert(env, new SupportBean_D("YES"), true);
                SendAssert(env, new SupportBean_B("YES"), true);
                SendAssert(env, new SupportBean_B("B1"), false);

                env.UndeployAll();
            }
        }
    }
} // end of namespace