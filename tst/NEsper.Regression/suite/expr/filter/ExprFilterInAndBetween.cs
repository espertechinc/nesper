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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionlib.support.multistmtassert;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterInAndBetween
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInDynamic(execs);
            WithSimpleIntAndEnumWrite(execs);
            WithInExpr(execs);
            WithNotIn(execs);
            WithInInvalid(execs);
            WithReuse(execs);
            WithReuseNot(execs);
            WithInMultipleNonMatchingFirst(execs);
            WithInMultipleWithBool(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInMultipleWithBool(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInMultipleWithBool());
            return execs;
        }

        public static IList<RegressionExecution> WithInMultipleNonMatchingFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInMultipleNonMatchingFirst());
            return execs;
        }

        public static IList<RegressionExecution> WithReuseNot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterReuseNot());
            return execs;
        }

        public static IList<RegressionExecution> WithReuse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterReuse());
            return execs;
        }

        public static IList<RegressionExecution> WithInInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNotIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterNotIn());
            return execs;
        }

        public static IList<RegressionExecution> WithInExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleIntAndEnumWrite(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterSimpleIntAndEnumWrite());
            return execs;
        }

        public static IList<RegressionExecution> WithInDynamic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInDynamic());
            return execs;
        }

        public class ExprFilterInMultipleWithBool : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@name('s1') select * from SupportBean(IntPrimitive in (0) and TheString like 'X%')";
                env.CompileDeploy(eplOne).AddListener("s1");

                env.Milestone(0);

                var eplTwo = "@name('s2') select * from SupportBean(IntPrimitive in (0,1) and TheString like 'A%')";
                env.CompileDeploy(eplTwo).AddListener("s2");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A", 1));
                env.AssertEventNew("s2", @event => { });
                env.AssertListenerNotInvoked("s1");

                env.UndeployAll();
            }
        }

        private class ExprFilterInMultipleNonMatchingFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNotMatching =
                    "@name('A') select * from SupportBean(IntPrimitive in (0,0,1) and TheString like 'X%')";
                env.CompileDeploy(eplNotMatching).AddListener("A");

                var eplMatching = "@name('B') select * from SupportBean(IntPrimitive in (0,1) and TheString like 'A%')";
                env.CompileDeploy(eplMatching).AddListener("B");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 0));
                env.AssertEventNew("B", @event => { });
                env.AssertListenerNotInvoked("A");

                env.UndeployAll();
            }
        }

        private class ExprFilterInDynamic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern [a=SupportBeanNumeric -> every b=SupportBean(IntPrimitive in (a.IntOne, a.IntTwo))]";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendBeanNumeric(env, 10, 20);
                SendBeanInt(env, 10);
                env.AssertListenerInvoked("s0");
                SendBeanInt(env, 11);
                env.AssertListenerNotInvoked("s0");
                SendBeanInt(env, 20);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();

                epl =
                    "@name('s0') select * from pattern [a=SupportBean_S0 -> every b=SupportBean(TheString in (a.P00, a.P01, a.P02))]";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.SendEventBean(new SupportBean_S0(1, "a", "b", "c", "d"));
                SendBeanString(env, "a");
                env.AssertListenerInvoked("s0");
                SendBeanString(env, "x");
                env.AssertListenerNotInvoked("s0");
                SendBeanString(env, "b");
                env.AssertListenerInvoked("s0");
                SendBeanString(env, "c");
                env.AssertListenerInvoked("s0");
                SendBeanString(env, "d");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterSimpleIntAndEnumWrite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean(IntPrimitive in (1, 10))";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendBeanInt(env, 10);
                env.AssertListenerInvoked("s0");
                SendBeanInt(env, 11);
                env.AssertListenerNotInvoked("s0");
                SendBeanInt(env, 1);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();

                // try enum collection with substitution param
                ISet<SupportEnum> types = new HashSet<SupportEnum>();
                types.Add(SupportEnum.ENUM_VALUE_2);
                var collectionType = typeof(ICollection<SupportEnum>).CleanName();
                var compiled = env.Compile(
                    "@name('s0') select * from SupportBean ev " + "where ev.EnumValue in (?::`" + collectionType + "`)");
                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(
                        new SupportPortableDeploySubstitutionParams(1, types).SetStatementParameters));
                env.AddListener("s0");

                var theEvent = new SupportBean();
                theEvent.EnumValue = SupportEnum.ENUM_VALUE_2;
                env.SendEventBean(theEvent);

                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterInExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString > 'b')",
                        new bool[] { false, false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString < 'b')",
                        new bool[] { true, false, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString >= 'b')",
                        new bool[] { false, true, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString <= 'b')",
                        new bool[] { true, true, false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    4,
                    num => SendBean(env, "TheString", new string[] { "a", "b", "c", "d" }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ['b':'d'])",
                        new bool[] { false, true, true, true, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ('b':'d'])",
                        new bool[] { false, false, true, true, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ['b':'d'))",
                        new bool[] { false, true, true, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString in ('b':'d'))",
                        new bool[] { false, false, true, false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    5,
                    num => SendBean(env, "TheString", new string[] { "a", "b", "c", "d", "e" }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false))",
                        new bool[] { false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false, false, false))",
                        new bool[] { false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive in (false, true, false))",
                        new bool[] { true, true }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    2,
                    num => SendBean(env, "BoolPrimitive", new object[] { true, false }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (4, 6, 1))",
                        new bool[] { false, true, false, false, true, false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (3))",
                        new bool[] { false, false, false, true, false, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 4 and 6)",
                        new bool[] { false, false, false, false, true, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 2 and 1)",
                        new bool[] { false, true, true, false, false, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed between 4 and -1)",
                        new bool[] { true, true, true, true, true, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in [2:4])",
                        new bool[] { false, false, true, true, true, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (2:4])",
                        new bool[] { false, false, false, true, true, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in [2:4))",
                        new bool[] { false, false, true, true, false, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed in (2:4))",
                        new bool[] { false, false, false, true, false, false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(env, "IntBoxed", new object[] { 0, 1, 2, 3, 4, 5, 6 }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(LongBoxed in (3))",
                        new bool[] { false, false, false, true, false, false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(env, "LongBoxed", new object[] { 0L, 1L, 2L, 3L, 4L, 5L, 6L }[num]),
                    milestone);
            }
        }

        private class ExprFilterNotIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 4 and 6)",
                        new bool[] { true, true, true, true, false, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 2 and 1)",
                        new bool[] { true, false, false, true, true, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not between 4 and -1)",
                        new bool[] { false, false, false, false, false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in [2:4])",
                        new bool[] { true, true, false, false, false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (2:4])",
                        new bool[] { true, true, true, false, false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in [2:4))",
                        new bool[] { true, true, false, false, true, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (2:4))",
                        new bool[] { true, true, true, false, true, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (4, 6, 1))",
                        new bool[] { true, false, true, true, false, true, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(IntBoxed not in (3))",
                        new bool[] { true, true, true, false, true, true, true }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(env, "IntBoxed", new object[] { 0, 1, 2, 3, 4, 5, 6 }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ['b':'d'])",
                        new bool[] { true, false, false, false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('b':'d'])",
                        new bool[] { true, true, false, false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ['b':'d'))",
                        new bool[] { true, false, false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('b':'d'))",
                        new bool[] { true, true, false, true, true }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    5,
                    num => SendBean(env, "TheString", new string[] { "a", "b", "c", "d", "e" }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(TheString not in ('a', 'b'))",
                        new bool[] { false, true, false, true }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    4,
                    num => SendBean(env, "TheString", new string[] { "a", "x", "b", "y" }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false))",
                        new bool[] { true, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false, false, false))",
                        new bool[] { true, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(BoolPrimitive not in (false, true, false))",
                        new bool[] { false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    2,
                    num => SendBean(env, "BoolPrimitive", new object[] { true, false }[num]),
                    milestone);

                assertions.Clear();
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportBean(LongBoxed not in (3))",
                        new bool[] { true, true, true, false, true, true, true }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    7,
                    num => SendBean(env, "LongBoxed", new object[] { 0L, 1L, 2L, 3L, 4L, 5L, 6L }[num]),
                    milestone);
            }
        }

        private class ExprFilterInInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                    // we do not coerce
                    TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1L, 10L))");
                    TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1, 10L))");
                    TryInvalidFilter(env, "select * from SupportBean(IntPrimitive in (1, 'x'))");

                    var expr =
                        "select * from pattern [a=SupportBean -> b=SupportBean(IntPrimitive in (a.LongPrimitive, a.LongBoxed))]";
                    TryInvalidFilter(env, expr);
                }
            }
        }

        private class ExprFilterReuse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expr = "select * from SupportBean(IntBoxed in [2:4])";
                TryReuse(env, new string[] { expr, expr }, milestone);

                expr = "select * from SupportBean(IntBoxed in (1, 2, 3))";
                TryReuse(env, new string[] { expr, expr }, milestone);

                var exprOne = "select * from SupportBean(IntBoxed in (2:3])";
                var exprTwo = "select * from SupportBean(IntBoxed in (1:3])";
                TryReuse(env, new string[] { exprOne, exprTwo }, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (2, 3, 4))";
                exprTwo = "select * from SupportBean(IntBoxed in (1, 3))";
                TryReuse(env, new string[] { exprOne, exprTwo }, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (2, 3, 4))";
                exprTwo = "select * from SupportBean(IntBoxed in (1, 3))";
                var exprThree = "select * from SupportBean(IntBoxed in (8, 3))";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (3, 1, 3))";
                exprTwo = "select * from SupportBean(IntBoxed in (3, 3))";
                exprThree = "select * from SupportBean(IntBoxed in (1, 3))";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);

                exprOne = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (1, 2, 3))";
                exprTwo = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (3, 4))";
                exprThree = "select * from SupportBean(BoolPrimitive=false, IntBoxed in (3))";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);

                exprOne = "select * from SupportBean(IntBoxed in (1, 2, 3), LongPrimitive >= 0)";
                exprTwo = "select * from SupportBean(IntBoxed in (3, 4), IntPrimitive >= 0)";
                exprThree = "select * from SupportBean(IntBoxed in (3), BytePrimitive < 1)";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ExprFilterReuseNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expr = "select * from SupportBean(IntBoxed not in [1:2])";
                TryReuse(env, new string[] { expr, expr }, milestone);

                var exprOne = "select * from SupportBean(IntBoxed in (3, 1, 3))";
                var exprTwo = "select * from SupportBean(IntBoxed not in (2, 1))";
                var exprThree = "select * from SupportBean(IntBoxed not between 0 and -3)";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);

                exprOne = "select * from SupportBean(IntBoxed not in (1, 4, 5))";
                exprTwo = "select * from SupportBean(IntBoxed not in (1, 4, 5))";
                exprThree = "select * from SupportBean(IntBoxed not in (4, 5, 1))";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);

                exprOne = "select * from SupportBean(IntBoxed not in (3:4))";
                exprTwo = "select * from SupportBean(IntBoxed not in [1:3))";
                exprThree = "select * from SupportBean(IntBoxed not in (1,1,1,33))";
                TryReuse(env, new string[] { exprOne, exprTwo, exprThree }, milestone);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void TryReuse(
            RegressionEnvironment env,
            string[] statements,
            AtomicLong milestone)
        {
            // create all statements
            for (var i = 0; i < statements.Length; i++) {
                env.CompileDeploy("@name('s" + i + "')" + statements[i]).AddListener("s" + i);
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
                theEvent.TheString = (string)value;
            }

            if (fieldName.Equals("BoolPrimitive")) {
                theEvent.BoolPrimitive = (bool)value;
            }

            if (fieldName.Equals("IntBoxed")) {
                theEvent.IntBoxed = (int?)value;
            }

            if (fieldName.Equals("LongBoxed")) {
                theEvent.LongBoxed = (long?)value;
            }

            env.SendEventBean(theEvent);
        }

        private static void TryInvalidFilter(
            RegressionEnvironment env,
            string epl)
        {
            env.TryInvalidCompile(epl, "skip");
        }
    }
} // end of namespace