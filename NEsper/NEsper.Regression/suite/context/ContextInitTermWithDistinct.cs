///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextInitTermWithDistinct
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextInitTermWithDistinctInvalid());
            execs.Add(new ContextInitTermWithDistinctNullSingleKey());
            execs.Add(new ContextInitTermWithDistinctNullKeyMultiKey());
            execs.Add(new ContextInitTermWithDistinctOverlappingSingleKey());
            execs.Add(new ContextInitTermWithDistinctOverlappingMultiKey());
            execs.Add(new ContextInitTermWithDistinctNullSingleKey());
            execs.Add(new ContextInitTermWithDistinctNullKeyMultiKey());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            env.SendEventBean(@event);
        }

        private static void SendSBEvent(
            RegressionEnvironment env,
            string @string,
            int? intBoxed,
            int intPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        internal class ContextInitTermWithDistinctInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // require stream name assignment using 'as'
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds",
                    "Distinct-expressions require that a stream name is assigned to the stream using 'as' [create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds]");

                // require stream
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds",
                    "Distinct-expressions require a stream as the initiated-by condition [create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds]");

                // invalid distinct-clause expression
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds",
                    "Invalid context distinct-clause expression 'subselect_0': Aggregation, sub-select, previous or prior functions are not supported in this context [create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds]");

                // empty list of expressions
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context MyContext initiated by distinct() SupportBean terminated after 15 seconds",
                    "Distinct-expressions have not been provided [create context MyContext initiated by distinct() SupportBean terminated after 15 seconds]");

                // non-overlapping context not allowed with distinct
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context MyContext start distinct(TheString) SupportBean end after 15 seconds",
                    "Incorrect syntax near 'distinct' (a reserved keyword) at line 1 column 31 [create context MyContext start distinct(TheString) SupportBean end after 15 seconds]");
            }
        }

        internal class ContextInitTermWithDistinctOverlappingSingleKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyContext " +
                    "  initiated by distinct(S0.TheString) SupportBean(IntPrimitive = 0) S0" +
                    "  terminated by SupportBean(TheString = S0.TheString and IntPrimitive = 1)",
                    path);

                var fields = new [] { "TheString","LongPrimitive","cnt" };
                env.CompileDeploy(
                    "@Name('s0') context MyContext " +
                    "select TheString, LongPrimitive, count(*) as cnt from SupportBean(TheString = context.S0.TheString)",
                    path);
                env.AddListener("s0");

                SendEvent(env, "A", -1, 10);

                env.Milestone(0);

                SendEvent(env, "A", 1, 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendEvent(env, "A", 0, 12); // allocate context
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 12L, 1L});

                SendEvent(env, "A", 0, 13); // counts towards the existing context, not having a new one
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 13L, 2L});

                env.Milestone(2);

                SendEvent(env, "A", -1, 14);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 14L, 3L});

                SendEvent(env, "A", 1, 15); // context termination
                SendEvent(env, "A", -1, 16);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendEvent(env, "A", 0, 17); // allocate context
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 17L, 1L});

                env.Milestone(4);

                SendEvent(env, "A", -1, 18);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 18L, 2L});

                env.Milestone(5);

                SendEvent(env, "B", 0, 19); // allocate context
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 19L, 1L});

                env.Milestone(6);

                SendEvent(env, "B", -1, 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 20L, 2L});

                SendEvent(env, "A", 1, 21); // context termination

                env.Milestone(7);

                SendEvent(env, "B", 1, 22); // context termination
                SendEvent(env, "A", -1, 23);

                env.Milestone(8);

                SendEvent(env, "B", -1, 24);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "A", 0, 25); // allocate context
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 25L, 1L});

                env.Milestone(9);

                SendEvent(env, "B", 0, 26); // allocate context
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 26L, 1L});

                env.UndeployAll();
            }
        }

        internal class ContextInitTermWithDistinctOverlappingMultiKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create context MyContext as " +
                          "initiated by distinct(TheString, IntPrimitive) SupportBean as sb " +
                          "terminated SupportBean_S1"; // any S1 ends the contexts
                env.EplToModelCompileDeploy(epl, path);

                var fields = new [] { "id","P00","P01","cnt" };
                env.CompileDeploy(
                    "@Name('s0') context MyContext " +
                    "select Id, P00, P01, count(*) as cnt " +
                    "from SupportBean_S0(Id = context.sb.IntPrimitive and P00 = context.sb.TheString)",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "A"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(1, "A", "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A", "E1", 1L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "A", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A", "E2", 2L});

                env.SendEventBean(new SupportBean_S1(-1)); // terminate all
                env.SendEventBean(new SupportBean_S0(1, "A", "E3"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("B", 2));
                env.SendEventBean(new SupportBean("B", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "A", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A", "E4", 1L});

                env.SendEventBean(new SupportBean_S0(2, "B", "E5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "B", "E5", 1L});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(1, "B", "E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "B", "E6", 1L});

                env.SendEventBean(new SupportBean_S0(2, "B", "E7"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "B", "E7", 2L});

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S1(-1)); // terminate all

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S0(2, "B", "E8"));
                env.SendEventBean(new SupportBean("B", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(2, "B", "E9"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "B", "E9", 1L});

                env.Milestone(7);

                env.SendEventBean(new SupportBean_S0(2, "B", "E10"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "B", "E10", 2L});

                env.UndeployAll();
            }
        }

        internal class ContextInitTermWithDistinctNullSingleKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyContext initiated by distinct(TheString) SupportBean as sb terminated after 24 hours",
                    path);
                env.CompileDeploy("@Name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean(null, 10));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean(null, 20));
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A", 30));
                Assert.AreEqual(2, env.Listener("s0").GetAndResetLastNewData().Length);

                env.UndeployAll();
            }
        }

        internal class ContextInitTermWithDistinctNullKeyMultiKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyContext initiated by distinct(TheString, IntBoxed, IntPrimitive) SupportBean as sb terminated after 100 hours",
                    path);
                env.CompileDeploy("@Name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                SendSBEvent(env, "A", null, 1);
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                SendSBEvent(env, "A", null, 1);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.Milestone(0);

                SendSBEvent(env, "A", 10, 1);
                Assert.AreEqual(2, env.Listener("s0").GetAndResetLastNewData().Length);

                env.UndeployAll();
            }
        }
    }
} // end of namespace