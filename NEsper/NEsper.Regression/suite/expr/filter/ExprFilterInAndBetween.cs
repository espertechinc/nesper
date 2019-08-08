///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.multistmtassert;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterInAndBetween
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprFilterInDynamic());
            executions.Add(new ExprFilterSimpleIntAndEnumWrite());
            executions.Add(new ExprFilterInExpr());
            executions.Add(new ExprFilterNotIn());
            executions.Add(new ExprFilterInInvalid());
            executions.Add(new ExprFilterReuse());
            executions.Add(new ExprFilterReuseNot());
            executions.Add(new ExprFilterInMultipleNonMatchingFirst());
            executions.Add(new ExprFilterInMultipleWithBool());
            return executions;
        }

        private static void TryReuse(
            RegressionEnvironment env,
            string[] statements,
            AtomicLong milestone)
        {
            // create all statements
            for (var i = 0; i < statements.Length; i++) {
                env.CompileDeploy("@Name('s" + i + "')" + statements[i]).AddListener("s" + i);
            }

            env.Milestone(milestone.GetAndIncrement());

            var listeners = new SupportListener[statements.Length];
            for (var i = 0; i < statements.Length; i++) {
                listeners[i] = env.Listener("s" + i);
            }

            // send event, all should receive the event
            SendBean(env, "IntBoxed", 3);
            for (var i = 0; i < statements.Length; i++) {
                Assert.IsTrue(listeners[i].IsInvokedAndReset());
            }

            // stop first, then second, then third etc statement
            for (var toStop = 0; toStop < statements.Length; toStop++) {
                env.UndeployModuleContaining("s" + toStop);

                // send event, all remaining statement received it
                SendBean(env, "IntBoxed", 3);
                for (var i = 0; i <= toStop; i++) {
                    Assert.IsFalse(listeners[i].IsInvoked);
                }

                for (var i = toStop + 1; i < statements.Length; i++) {
                    Assert.IsTrue(listeners[i].IsInvokedAndReset());
                }
            }

            // now all statements are stopped, send event and verify no listener received
            SendBean(env, "IntBoxed", 3);
            for (var i = 0; i < statements.Length; i++) {
                Assert.IsFalse(listeners[i].IsInvoked);
            }
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void SendBeanString(
            RegressionEnvironment env,
            string value)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = value;
            env.SendEventBean(theEvent);
        }

        private static void SendBeanNumeric(
            RegressionEnvironment env,
            int intOne,
            int intTwo)
        {
            var num = new SupportBeanNumeric(intOne, intTwo);
            env.SendEventBean(num);
        }

        private static void SendBean(
            RegressionEnvironment env,
            string fieldName,
            object value)
        {
            var theEvent = new SupportBean();
            if (fieldName.Equals("TheString")) {
                theEvent.TheString = (string) value;
            }

            if (fieldName.Equals("BoolPrimitive")) {
                theEvent.BoolPrimitive = (bool) value;
            }

            if (fieldName.Equals("IntBoxed")) {
                theEvent.IntBoxed = (int?) value;
            }

            if (fieldName.Equals("LongBoxed")) {
                theEvent.LongBoxed = value.AsBoxedLong();
            }

            env.SendEventBean(theEvent);
        }

        private static void TryInvalidFilter(
            RegressionEnvironment env,
            string epl)
        {
            TryInvalidCompile(env, epl, "skip");
        }

        public class ExprFilterInMultipleWithBool : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@Name('s1') select * from SupportBean(IntPrimitive in (0) and TheString like 'X%')";
                env.CompileDeploy(eplOne).AddListener("s1");

                env.Milestone(0);

                var eplTwo = "@Name('s2') select * from SupportBean(IntPrimitive in (0,1) and TheString like 'A%')";
                env.CompileDeploy(eplTwo).AddListener("s2");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A", 1));
                env.Listener("s2").AssertOneGetNewAndReset();
                Assert.IsFalse(env.Listener("s1").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterInMultipleNonMatchingFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNotMatching =
                    "@Name('A') select * from SupportBean(IntPrimitive in (0,0,1) and TheString like 'X%')";
                env.CompileDeploy(eplNotMatching).AddListener("A");

                var eplMatching = "@Name('B') select * from SupportBean(IntPrimitive in (0,1) and TheString like 'A%')";
                env.CompileDeploy(eplMatching).AddListener("B");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 0));
                env.Listener("B").AssertOneGetNewAndReset();
                Assert.IsFalse(env.Listener("A").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ExprFilterInDynamic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from pattern [a=SupportBeanNumeric -> every b=SupportBean(IntPrimitive in (a.intOne, a.intTwo))]";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendBeanNumeric(env, 10, 20);
                SendBeanInt(env, 10);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanInt(env, 11);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanInt(env, 20);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();

                epl =
                    "@Name('s0') select * from pattern [a=SupportBean_S0 -> every b=SupportBean(TheString in (a.P00, a.P01, a.P02))]";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.SendEventBean(new SupportBean_S0(1, "a", "b", "c", "d"));
                SendBeanString(env, "a");
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanString(env, "x");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanString(env, "b");
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanString(env, "c");
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanString(env, "d");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ExprFilterSimpleIntAndEnumWrite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean(IntPrimitive in (1, 10))";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendBeanInt(env, 10);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanInt(env, 11);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanInt(env, 1);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();

                // try enum collection with substitution param
                ISet<SupportEnum> types = new HashSet<SupportEnum>();
                types.Add(SupportEnum.ENUM_VALUE_2);
                var compiled = env.Compile(
                    "@Name('s0') select * from SupportBean ev " + "where ev.enumValue in (?::java.util.Collection)");
                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(
                        prepared => prepared.SetObject(1, types)));
                env.AddListener("s0");

                var theEvent = new SupportBean();
                theEvent.EnumValue = SupportEnum.ENUM_VALUE_2;
                env.SendEventBean(theEvent);

                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterInExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString > 'b')",
                        new[] {false, false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString < 'b')",
                        new[] {true, false, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString >= 'b')",
                        new[] {false, true, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString <= 'b')",
                        new[] {true, true, false, false}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    4,
                    num => SendBean(
                        env,
                        "TheString",
                        new[] {"a", "b", "c", "d"}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ['b':'d'])",
                        new[] {false, true, true, true, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ('b':'d'])",
                        new[] {false, false, true, true, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ['b':'d'))",
                        new[] {false, true, true, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ('b':'d'))",
                        new[] {false, false, true, false, false}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    5,
                    num => SendBean(env, "TheString", new[] {"a", "b", "c", "d", "e"}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false))",
                        new[] {false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false, false, false))",
                        new[] {false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false, true, false))",
                        new[] {true, true}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    2,
                    num => SendBean(
                        env,
                        "BoolPrimitive",
                        new object[] {true, false}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (4, 6, 1))",
                        new[] {false, true, false, false, true, false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (3))",
                        new[] {false, false, false, true, false, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 4 and 6)",
                        new[] {false, false, false, false, true, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 2 and 1)",
                        new[] {false, true, true, false, false, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 4 and -1)",
                        new[] {true, true, true, true, true, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in [2:4])",
                        new[] {false, false, true, true, true, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (2:4])",
                        new[] {false, false, false, true, true, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in [2:4))",
                        new[] {false, false, true, true, false, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (2:4))",
                        new[] {false, false, false, true, false, false, false}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(
                        env,
                        "IntBoxed",
                        new object[] {0, 1, 2, 3, 4, 5, 6}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(LongBoxed in (3))",
                        new[] {false, false, false, true, false, false, false}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(
                        env,
                        "LongBoxed",
                        new object[] {0L, 1L, 2L, 3L, 4L, 5L, 6L}[num]),
                    milestone);
            }
        }

        internal class ExprFilterNotIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 4 and 6)",
                        new[] {true, true, true, true, false, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 2 and 1)",
                        new[] {true, false, false, true, true, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 4 and -1)",
                        new[] {false, false, false, false, false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in [2:4])",
                        new[] {true, true, false, false, false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (2:4])",
                        new[] {true, true, true, false, false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in [2:4))",
                        new[] {true, true, false, false, true, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (2:4))",
                        new[] {true, true, true, false, true, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (4, 6, 1))",
                        new[] {true, false, true, true, false, true, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (3))",
                        new[] {true, true, true, false, true, true, true}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(
                        env,
                        "IntBoxed",
                        new object[] {0, 1, 2, 3, 4, 5, 6}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ['b':'d'])",
                        new[] {true, false, false, false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('b':'d'])",
                        new[] {true, true, false, false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ['b':'d'))",
                        new[] {true, false, false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('b':'d'))",
                        new[] {true, true, false, true, true}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    5,
                    num => SendBean(env, "TheString", new[] {"a", "b", "c", "d", "e"}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('a', 'b'))",
                        new[] {false, true, false, true}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    4,
                    num => SendBean(env, "TheString", new[] {"a", "x", "b", "y"}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false))",
                        new[] {true, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false, false, false))",
                        new[] {true, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false, true, false))",
                        new[] {false, false}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    2,
                    num => SendBean(
                        env,
                        "BoolPrimitive",
                        new object[] {true, false}[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(LongBoxed not in (3))",
                        new[] {true, true, true, false, true, true, true}));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(
                        env,
                        "LongBoxed",
                        new object[] {0L, 1L, 2L, 3L, 4L, 5L, 6L}[num]),
                    milestone);
            }
        }

        internal class ExprFilterInInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // we do not coerce
                TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1L, 10L))");
                TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1, 10L))");
                TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1, 'x'))");

                var expr =
                    "select * from pattern [a=SupportBean -> b=SupportBean(IntPrimitive in (a.LongPrimitive, a.LongBoxed))]";
                TryInvalidFilter(env, expr);
            }
        }

        internal class ExprFilterReuse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expr = "select * from SupportBean(IntBoxed in [2:4])";
                TryReuse(env, new[] {expr, expr}, milestone);

                expr = "select * from SupportBean(IntBoxed in (1, 2, 3))";
                TryReuse(env, new[] {expr, expr}, milestone);

                var exprOne = "select * from SupportBean(IntBoxed in (2:3])";
                var exprTwo = "select * from SupportBean(IntBoxed in (1:3])";
                TryReuse(env, new[] {exprOne, exprTwo}, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (2, 3, 4))";
                exprTwo = "select * from SupportBean(IntBoxed in (1, 3))";
                TryReuse(env, new[] {exprOne, exprTwo}, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (2, 3, 4))";
                exprTwo = "select * from SupportBean(IntBoxed in (1, 3))";
                var exprThree = "select * from SupportBean(IntBoxed in (8, 3))";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (3, 1, 3))";
                exprTwo = "select * from SupportBean(IntBoxed in (3, 3))";
                exprThree = "select * from SupportBean(IntBoxed in (1, 3))";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);

                exprOne = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (1, 2, 3))";
                exprTwo = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (3, 4))";
                exprThree = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (3))";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (1, 2, 3), LongPrimitive >= 0)";
                exprTwo = "select * from SupportBean(IntBoxed in (3, 4), IntPrimitive >= 0)";
                exprThree = "select * from SupportBean(IntBoxed in (3), BytePrimitive < 1)";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);
            }
        }

        internal class ExprFilterReuseNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expr = "select * from SupportBean(IntBoxed not in [1:2])";
                TryReuse(env, new[] {expr, expr}, milestone);

                var exprOne = "select * from SupportBean(IntBoxed in (3, 1, 3))";
                var exprTwo = "select * from SupportBean(IntBoxed not in (2, 1))";
                var exprThree = "select * from SupportBean(IntBoxed not between 0 and -3)";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);

                exprOne = "select * from SupportBean(IntBoxed not in (1, 4, 5))";
                exprTwo = "select * from SupportBean(IntBoxed not in (1, 4, 5))";
                exprThree = "select * from SupportBean(IntBoxed not in (4, 5, 1))";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);

                exprOne = "select * from SupportBean(IntBoxed not in (3:4))";
                exprTwo = "select * from SupportBean(IntBoxed not in [1:3))";
                exprThree = "select * from SupportBean(IntBoxed not in (1,1,1,33))";
                TryReuse(env, new[] {exprOne, exprTwo, exprThree}, milestone);
            }
        }
    }
} // end of namespace