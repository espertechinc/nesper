///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.multistmtassert;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.RegressionFlag;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterExpressions
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithConstant(execs);
            WithRelationalOpRange(execs);
            WithMathExpression(execs);
            WithBooleanExpr(execs);
            WithIn3ValuesAndNull(execs);
            WithNotEqualsNull(execs);
            WithInSet(execs);
            WithOverInClause(execs);
            WithNotEqualsConsolidate(execs);
            WithPromoteIndexToSetNotIn(execs);
            WithShortCircuitEvalAndOverspecified(execs);
            WithRelationalOpConstantFirst(execs);
            WithNullBooleanExpr(execs);
            WithEnumSyntaxOne(execs);
            WithEnumSyntaxTwo(execs);
            WithPatternFunc3Stream(execs);
            WithPatternFunc(execs);
            WithStaticFunc(execs);
            WithWithEqualsSameCompare(execs);
            WithEqualsSemanticFilter(execs);
            WithPatternWithExpr(execs);
            WithExprReversed(execs);
            WithRewriteWhere(execs);
            WithNotEqualsOp(execs);
            WithCombinationEqualsOp(execs);
            WithEqualsSemanticExpr(execs);
            WithInvalid(execs);
            WithInstanceMethodWWildcard(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceMethodWWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInstanceMethodWWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsSemanticExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterEqualsSemanticExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithCombinationEqualsOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterCombinationEqualsOp());
            return execs;
        }

        public static IList<RegressionExecution> WithNotEqualsOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterNotEqualsOp());
            return execs;
        }

        public static IList<RegressionExecution> WithRewriteWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterRewriteWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithExprReversed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterExprReversed());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternWithExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterPatternWithExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsSemanticFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterEqualsSemanticFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithWithEqualsSameCompare(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterWithEqualsSameCompare());
            return execs;
        }

        public static IList<RegressionExecution> WithStaticFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterStaticFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterPatternFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternFunc3Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterPatternFunc3Stream());
            return execs;
        }

        public static IList<RegressionExecution> WithEnumSyntaxTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterEnumSyntaxTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithEnumSyntaxOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterEnumSyntaxOne());
            return execs;
        }

        public static IList<RegressionExecution> WithNullBooleanExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterNullBooleanExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpConstantFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterRelationalOpConstantFirst());
            return execs;
        }

        public static IList<RegressionExecution> WithShortCircuitEvalAndOverspecified(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterShortCircuitEvalAndOverspecified());
            return execs;
        }

        public static IList<RegressionExecution> WithPromoteIndexToSetNotIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterPromoteIndexToSetNotIn());
            return execs;
        }

        public static IList<RegressionExecution> WithNotEqualsConsolidate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterNotEqualsConsolidate());
            return execs;
        }

        public static IList<RegressionExecution> WithOverInClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOverInClause());
            return execs;
        }

        public static IList<RegressionExecution> WithInSet(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInSet());
            return execs;
        }

        public static IList<RegressionExecution> WithNotEqualsNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterNotEqualsNull());
            return execs;
        }

        public static IList<RegressionExecution> WithIn3ValuesAndNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterIn3ValuesAndNull());
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterBooleanExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithMathExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterMathExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterRelationalOpRange());
            return execs;
        }

        public static IList<RegressionExecution> WithConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterConstant());
            return execs;
        }

        private class ExprFilterRelationalOpRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(intBoxed in [2:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, true, false }));

                text = "select * from SupportBean(intBoxed in [2:3] and intBoxed in [2:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, true, false }));

                text = "select * from SupportBean(intBoxed in [2:3] and intBoxed in [2:2])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false, false }));

                text = "select * from SupportBean(intBoxed in [1:10] and intBoxed in [3:2])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, true, false }));

                text = "select * from SupportBean(intBoxed in [3:3] and intBoxed in [1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, true, false }));

                text = "select * from SupportBean(intBoxed in [3:3] and intBoxed in [1:3] and intBoxed in [4:5])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, false, false }));

                text = "select * from SupportBean(intBoxed not in [3:3] and intBoxed not in [1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, false, true }));

                text = "select * from SupportBean(intBoxed not in (2:4) and intBoxed not in (1:3))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false, false, true }));

                text = "select * from SupportBean(intBoxed not in [2:4) and intBoxed not in [1:3))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, false, true }));

                text = "select * from SupportBean(intBoxed not in (2:4] and intBoxed not in (1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false, false, false }));

                text = "select * from SupportBean where intBoxed not in (2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, true, false, true }));

                text = "select * from SupportBean where intBoxed not in [2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false, false, false }));

                text = "select * from SupportBean where intBoxed not in [2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false, false, true }));

                text = "select * from SupportBean where intBoxed not in (2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, true, false, false }));

                text = "select * from SupportBean where intBoxed in (2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, true, false }));

                text = "select * from SupportBean where intBoxed in [2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, true, true }));

                text = "select * from SupportBean where intBoxed in [2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, true, false }));

                text = "select * from SupportBean where intBoxed in (2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, true, true }));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] { 1, 2, 3, 4 },
                    data => SendBeanIntDouble(env, (int?)data, 0D),
                    milestone);
            }
        }

        private class ExprFilterMathExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<string> epl = new List<string>();
                var milestone = new AtomicLong();

                epl.Add("select * from SupportBean(intBoxed*doubleBoxed > 20)");
                epl.Add("select * from SupportBean(20 < intBoxed*doubleBoxed)");
                epl.Add("select * from SupportBean(20/intBoxed < doubleBoxed)");
                epl.Add("select * from SupportBean(20/intBoxed/doubleBoxed < 1)");

                MultiStmtAssertUtil.RunSendAssertPairs(
                    env,
                    epl,
                    new SendAssertPair[] {
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 5d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => env.AssertListenerInvoked(statementName)),
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 4d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => env.AssertListenerNotInvoked(statementName)),
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 4.001d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => env.AssertListenerInvoked(statementName))
                    },
                    milestone);
            }
        }

        private class ExprFilterBooleanExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportBean(2*intBoxed=doubleBoxed)";
                env.CompileDeployAddListenerMile(text, "s0", 0);

                SendBeanIntDouble(env, 20, 50d);
                env.AssertListenerNotInvoked("s0");
                SendBeanIntDouble(env, 25, 50d);
                env.AssertListenerInvoked("s0");

                text = "@name('s1') select * from SupportBean(2*intBoxed=doubleBoxed, theString='s')";
                env.CompileDeployAddListenerMile(text, "s1", 1);

                SendBeanIntDoubleString(env, 25, 50d, "s");
                env.AssertListenerInvoked("s1");
                SendBeanIntDoubleString(env, 25, 50d, "x");
                env.AssertListenerNotInvoked("s1");

                env.UndeployAll();

                // test priority of equals and boolean
                env.CompileDeploy("@name('s0') select * from SupportBean(intPrimitive = 1 or intPrimitive = 2)")
                    .AddListener("s0");
                env.CompileDeploy(
                        "@name('s1') select * from SupportBean(intPrimitive = 3, SupportStaticMethodLib.alwaysTrue())")
                    .AddListener("s1");

                SupportStaticMethodLib.Invocations.Clear();
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(SupportStaticMethodLib.Invocations.IsEmpty());

                env.UndeployAll();
            }
        }

        private class ExprFilterIn3ValuesAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from SupportBean(intPrimitive in (intBoxed, doubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { 0, 1, 0 },
                    new double?[] { 2d, 2d, 1d },
                    new bool[] { false, true, true });

                text = "select * from SupportBean(intPrimitive in (intBoxed, " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(doubleBoxed)))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { 0, 1, 0 },
                    new double?[] { 2d, 2d, 1d },
                    new bool[] { true, true, false });

                text = "select * from SupportBean(intPrimitive not in (intBoxed, doubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { 0, 1, 0 },
                    new double?[] { 2d, 2d, 1d },
                    new bool[] { true, false, false });

                text = "select * from SupportBean(intBoxed = doubleBoxed)";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { null, 1, null },
                    new double?[] { null, null, 1d },
                    new bool[] { false, false, false });

                text = "select * from SupportBean(intBoxed in (doubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { null, 1, null },
                    new double?[] { null, null, 1d },
                    new bool[] { false, false, false });

                text = "select * from SupportBean(intBoxed not in (doubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { null, 1, null },
                    new double?[] { null, null, 1d },
                    new bool[] { false, false, false });

                text = "select * from SupportBean(intBoxed in [doubleBoxed:10))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { null, 1, 2 },
                    new double?[] { null, null, 1d },
                    new bool[] { false, false, true });

                text = "select * from SupportBean(intBoxed not in [doubleBoxed:10))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new int[] { 1, 1, 1 },
                    new int?[] { null, 1, 2 },
                    new double?[] { null, null, 1d },
                    new bool[] { false, true, false });
            }
        }

        private class ExprFilterNotEqualsNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string[] stmts;
                string epl;

                // test equals&where-clause (can be optimized into filter)
                env.CompileDeploy("@name('s0') select * from SupportBean where theString != 'A'").AddListener("s0");
                env.CompileDeploy("@name('s1') select * from SupportBean where theString != 'A' or intPrimitive != 0")
                    .AddListener("s1");
                env.CompileDeploy("@name('s2') select * from SupportBean where theString = 'A'").AddListener("s2");
                env.CompileDeploy("@name('s3') select * from SupportBean where theString = 'A' or intPrimitive != 0")
                    .AddListener("s3");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new bool[] { false, false, false, false });

                SendSupportBean(env, new SupportBean(null, 1));
                AssertListeners(env, stmts, new bool[] { false, true, false, true });

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new bool[] { false, false, true, true });

                SendSupportBean(env, new SupportBean("A", 1));
                AssertListeners(env, stmts, new bool[] { false, true, true, true });

                SendSupportBean(env, new SupportBean("B", 0));
                AssertListeners(env, stmts, new bool[] { true, true, false, false });

                SendSupportBean(env, new SupportBean("B", 1));
                AssertListeners(env, stmts, new bool[] { true, true, false, true });

                env.UndeployAll();

                // test equals&selection
                var fields = "val0,val1,val2,val3,val4,val5".SplitCsv();
                epl = "@name('s0') select " +
                      "theString != 'A' as val0, " +
                      "theString != 'A' or intPrimitive != 0 as val1, " +
                      "theString != 'A' and intPrimitive != 0 as val2, " +
                      "theString = 'A' as val3," +
                      "theString = 'A' or intPrimitive != 0 as val4, " +
                      "theString = 'A' and intPrimitive != 0 as val5 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0").MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 0));
                env.AssertPropsNew("s0", fields, new object[] { null, null, false, null, null, false });

                env.MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 1));
                env.AssertPropsNew("s0", fields, new object[] { null, true, null, null, true, null });

                SendSupportBean(env, new SupportBean("A", 0));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, true, true, false });

                SendSupportBean(env, new SupportBean("A", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, true, true, true });

                SendSupportBean(env, new SupportBean("B", 0));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false, false, false });

                SendSupportBean(env, new SupportBean("B", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, false, true, false });

                env.UndeployAll().MilestoneInc(milestone);

                // test is-and-isnot&where-clause
                env.CompileDeploy("@name('s0') select * from SupportBean where theString is null").AddListener("s0");
                env.CompileDeploy("@name('s1') select * from SupportBean where theString is null or intPrimitive != 0")
                    .AddListener("s1");
                env.CompileDeploy("@name('s2') select * from SupportBean where theString is not null")
                    .AddListener("s2");
                env.CompileDeploy(
                        "@name('s3') select * from SupportBean where theString is not null or intPrimitive != 0")
                    .AddListener("s3");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new bool[] { true, true, false, false });

                SendSupportBean(env, new SupportBean(null, 1));
                AssertListeners(env, stmts, new bool[] { true, true, false, true });

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new bool[] { false, false, true, true });

                SendSupportBean(env, new SupportBean("A", 1));
                AssertListeners(env, stmts, new bool[] { false, true, true, true });

                env.UndeployAll();

                // test is-and-isnot&selection
                epl = "@name('s0') select " +
                      "theString is null as val0, " +
                      "theString is null or intPrimitive != 0 as val1, " +
                      "theString is null and intPrimitive != 0 as val2, " +
                      "theString is not null as val3," +
                      "theString is not null or intPrimitive != 0 as val4, " +
                      "theString is not null and intPrimitive != 0 as val5 " +
                      "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0").MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 0));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false, false, false });

                SendSupportBean(env, new SupportBean(null, 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, false, true, false });

                SendSupportBean(env, new SupportBean("A", 0));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, true, true, false });

                SendSupportBean(env, new SupportBean("A", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, true, true, true });

                env.UndeployAll();

                // filter expression
                env.CompileDeploy("@name('s0') select * from SupportBean(theString is null)").AddListener("s0");
                env.CompileDeploy("@name('s1') select * from SupportBean where theString = null").AddListener("s1");
                env.CompileDeploy("@name('s2') select * from SupportBean(theString = null)").AddListener("s2");
                env.CompileDeploy("@name('s3') select * from SupportBean(theString is not null)").AddListener("s3");
                env.CompileDeploy("@name('s4') select * from SupportBean where theString != null").AddListener("s4");
                env.CompileDeploy("@name('s5') select * from SupportBean(theString != null)").AddListener("s5");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3,s4,s5".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new bool[] { true, false, false, false, false, false });

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new bool[] { false, false, false, true, false, false });

                env.UndeployAll();

                // select constants
                fields = "val0,val1,val2,val3".SplitCsv();
                env.CompileDeploy(
                        "@name('s0') select " +
                        "2 != null as val0," +
                        "null = null as val1," +
                        "2 != null or 1 = 2 as val2," +
                        "2 != null and 2 = 2 as val3 " +
                        "from SupportBean")
                    .AddListener("s0");
                env.MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean("E1", 0));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null });

                env.UndeployAll();

                // test SODA
                epl =
                    "@name('s0') select intBoxed is null, intBoxed is not null, intBoxed=1, intBoxed!=1 from SupportBean";
                env.EplToModelCompileDeploy(epl);
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertEqualsExactOrder(
                        new string[] {
                            "intBoxed is null",
                            "intBoxed is not null",
                            "intBoxed=1",
                            "intBoxed!=1"
                        },
                        statement.EventType.PropertyNames));
                env.UndeployAll();
            }
        }

        private class ExprFilterInSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from \n" +
                          "pattern [ \n" +
                          " every start_load=SupportBeanArrayCollMap \n" +
                          " -> \n" +
                          " single_load=SupportBean(theString in (start_load.setOfString)) \n" +
                          "]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var setOfString = new HashSet<string>();
                setOfString.Add("Version1");
                setOfString.Add("Version2");
                env.SendEventBean(new SupportBeanArrayCollMap(setOfString));

                env.SendEventBean(new SupportBean("Version1", 0));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterOverInClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[every event1=SupportTradeEvent(userId in ('100','101'),amount>=1000)]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportTradeEvent(1, "100", 1001));
                env.AssertEqualsNew("s0", "event1.id", 1);

                var eplTwo =
                    "@name('s1') select * from pattern [every event1=SupportTradeEvent(userId in ('100','101'))]";
                env.CompileDeployAddListenerMile(eplTwo, "s1", 1);

                env.SendEventBean(new SupportTradeEvent(2, "100", 1001));
                env.AssertEqualsNew("s0", "event1.id", 2);
                env.AssertEqualsNew("s1", "event1.id", 2);

                env.UndeployAll();
            }
        }

        private class ExprFilterNotEqualsConsolidate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = new string[] {
                    "select * from SupportBean(intPrimitive not in (1, 2))",
                    "select * from SupportBean(intPrimitive != 1, intPrimitive != 2)",
                    "select * from SupportBean(intPrimitive != 1 and intPrimitive != 2)"
                };
                MultiStmtAssertUtil.RunEPL(
                    env,
                    Arrays.AsList(epl),
                    new object[] { 0, 1, 2, 3, 4 },
                    data => SendSupportBean(env, new SupportBean("", data.AsInt32())),
                    (
                        eventIndex,
                        eventData,
                        assertionDesc,
                        statementName,
                        failMessage) => {
                        if (eventData.Equals(1) || eventData.Equals(2)) {
                            env.AssertListenerNotInvoked(statementName);
                        }
                        else {
                            env.AssertListenerInvoked(statementName);
                        }
                    },
                    milestone);
            }
        }

        private class ExprFilterPromoteIndexToSetNotIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@name('s0') select * from SupportBean(theString != 'x' and theString != 'y' and doubleBoxed is not null)";
                var eplTwo =
                    "@name('s1') select * from SupportBean(theString != 'x' and theString != 'y' and longBoxed is not null)";

                env.CompileDeploy(eplOne).AddListener("s0");
                env.CompileDeploy(eplTwo).AddListener("s1");
                env.Milestone(0);

                var bean = new SupportBean("E1", 0);
                bean.DoubleBoxed = (1d);
                bean.LongBoxed = (1L);
                env.SendEventBean(bean);

                env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());
                env.AssertListener("s1", _ => _.AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        private class ExprFilterShortCircuitEvalAndOverspecified : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportRuntimeExBean(SupportRuntimeExBean.property2 = '4' and SupportRuntimeExBean.property1 = '1')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportRuntimeExBean());
                env.AssertListener(
                    "s0",
                    listener => Assert.IsFalse(
                        listener.IsInvoked,
                        "Subscriber should not have received result(s)"));

                env.UndeployAll();

                epl = "@name('s0') select * from SupportBean(theString='A' and theString='B')";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendSupportBean(env, new SupportBean("A", 0));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterRelationalOpConstantFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 < x",
                        new bool[] { false, false, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 <= x",
                        new bool[] { false, true, true }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 > x",
                        new bool[] { true, false, false }));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 >= x",
                        new bool[] { true, true, false }));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] { 3, 4, 5 },
                    data => env.SendEventBean(new SupportInstanceMethodBean(data.AsInt32())),
                    milestone);
            }
        }

        private class ExprFilterNullBooleanExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern [every event1=SupportTradeEvent(userId like '123%')]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportTradeEvent(1, null, 1001));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportTradeEvent(2, "1234", 1001));
                env.AssertEqualsNew("s0", "event1.id", 2);

                env.UndeployAll();
            }
        }

        private class ExprFilterConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern [SupportBean(intPrimitive=" +
                          typeof(ISupportA).FullName +
                          ".VALUE_1)]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var theEvent = new SupportBean("e1", 2);
                env.SendEventBean(theEvent);
                env.AssertListenerNotInvoked("s0");

                theEvent = new SupportBean("e1", 1);
                env.SendEventBean(theEvent);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterEnumSyntaxOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern [SupportBeanWithEnum(supportEnum=" +
                          typeof(SupportEnum).FullName +
                          ".valueOf('ENUM_VALUE_1'))]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                env.AssertListenerNotInvoked("s0");

                theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(ENUMHASHCODEPROCESSDEPENDENT);
            }
        }

        private class ExprFilterEnumSyntaxTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[SupportBeanWithEnum(supportEnum=" +
                          typeof(SupportEnum).FullName +
                          ".ENUM_VALUE_2)]";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                env.AssertListenerInvoked("s0");

                theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();

                // test where clause
                epl = "@name('s0') select * from SupportBeanWithEnum where supportEnum=" +
                      typeof(SupportEnum).FullName +
                      ".ENUM_VALUE_2";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                env.AssertListenerInvoked("s0");

                theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(ENUMHASHCODEPROCESSDEPENDENT);
            }
        }

        private class ExprFilterPatternFunc3Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed=a.intBoxed, intBoxed=b.intBoxed and intBoxed != null)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 4, -2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 5, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, false, false, false, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed is a.intBoxed, intBoxed is b.intBoxed and intBoxed is not null)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 4, -2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 5, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, true, false, false, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed=a.intBoxed or intBoxed=b.intBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 4, -2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 5, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, true, true, true, false, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed=a.intBoxed, intBoxed=b.intBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 4, -2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 5, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, true, false, false, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed!=a.intBoxed, intBoxed!=b.intBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 4, -2 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, 3, 1, 8, null, 5, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, false, false, false, true, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed!=a.intBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 8, null, 2, 1, null, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { -2, null, null, 3, 1, 8, 4 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, null, null, 3, 1, 8, 5 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, false, true, false, false, true });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed is not a.intBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 8, null, 2, 1, null, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { -2, null, null, 3, 1, 8, 4 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { null, null, null, 3, 1, 8, 5 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { true, true, false, true, false, true, true });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed=a.intBoxed, doubleBoxed=b.doubleBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 2, 1, 2, 1, 7, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 0, 0, 0, 0, 0, 0, 0 },
                    new double?[] { 1d, 2d, 0d, 2d, 0d, 1d, 0d },
                    new int?[] { 2, 2, 3, 2, 1, 7, 5 },
                    new double?[] { 1d, 1d, 1d, 2d, 1d, 1d, 1d },
                    new bool[] { true, false, false, true, false, true, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed in (a.intBoxed, b.intBoxed))]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 1, 1, null, 1, null, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 1, 2, 1, null, null, 2, 0 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 2, 2, 3, null, 1, null, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { true, true, false, false, true, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed in [a.intBoxed:b.intBoxed])]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 1, 1, null, 1, null, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 1, 2, 1, null, null, 2, 0 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 2, 1, 3, null, 1, null, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { true, true, false, false, false, false, false });

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(intBoxed not in [a.intBoxed:b.intBoxed])]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] { 2, 1, 1, null, 1, null, 1 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 1, 2, 1, null, null, 2, 0 },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new int?[] { 2, 1, 3, null, 1, null, null },
                    new double?[] { 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                    new bool[] { false, false, true, false, true, false, false });
            }
        }

        private class ExprFilterPatternFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(intBoxed = a.intBoxed and doubleBoxed = a.doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 2d, 2d, 2d, 1d, 5d, 6d, 7d },
                    new int?[] { null, 3, 1, 8, null, 1, 2 },
                    new double?[] { 2d, 3d, 2d, 1d, 5d, 6d, 8d },
                    new bool[] { false, false, true, false, false, true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(intBoxed is a.intBoxed and doubleBoxed = a.doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { null, 2, 1, null, 8, 1, 2 },
                    new double?[] { 2d, 2d, 2d, 1d, 5d, 6d, 7d },
                    new int?[] { null, 3, 1, 8, null, 1, 2 },
                    new double?[] { 2d, 3d, 2d, 1d, 5d, 6d, 8d },
                    new bool[] { true, false, true, false, false, true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.doubleBoxed = doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 2d },
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 3d },
                    new bool[] { true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.doubleBoxed = b.doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 2d },
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 3d },
                    new bool[] { true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.doubleBoxed != doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 2d },
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 3d },
                    new bool[] { false, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.doubleBoxed != b.doubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 2d },
                    new int?[] { 0, 0 },
                    new double?[] { 2d, 3d },
                    new bool[] { false, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed in [a.doubleBoxed:a.intBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, true, true, true, true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed in (a.doubleBoxed:a.intBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, false, true, true, true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.doubleBoxed in (a.doubleBoxed:a.intBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, false, true, true, false, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed in [a.doubleBoxed:a.intBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, true, true, true, false, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed not in [a.doubleBoxed:a.intBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, false, false, false, false, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed not in (a.doubleBoxed:a.intBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, true, false, false, false, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.doubleBoxed not in (a.doubleBoxed:a.intBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, true, false, false, true, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed not in [a.doubleBoxed:a.intBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, false, false, false, true, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed not in (a.doubleBoxed, a.intBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, false, true, false, false, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed in (a.doubleBoxed, a.intBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, true, false, true, true, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.doubleBoxed in (doubleBoxed, a.intBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { true, true, true, true, true, true });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed not in (doubleBoxed, a.intBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 1, 1, 1, 1, 1, 1 },
                    new double?[] { 10d, 10d, 10d, 10d, 10d, 10d },
                    new int?[] { 0, 0, 0, 0, 0, 0 },
                    new double?[] { 0d, 1d, 2d, 9d, 10d, 11d },
                    new bool[] { false, false, false, false, false, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.doubleBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0, 0 },
                    new double?[] { 10d, 10d, 10d },
                    new int?[] { 0, 0, 0 },
                    new double?[] { 9d, 10d, 11d },
                    new bool[] { true, false, false });

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(doubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.doubleBoxed) or " +
                       "doubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.intBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] { 0, 0, 12 },
                    new double?[] { 10d, 10d, 10d },
                    new int?[] { 0, 0, 0 },
                    new double?[] { 9d, 10d, 11d },
                    new bool[] { true, false, true });
            }
        }

        private class ExprFilterStaticFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(" +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('b', theString))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean(" +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', theString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean('b'=theString," +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', theString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean('b'=theString, theString='b', theString != 'a')";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean(theString != 'a', theString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean(theString = 'b', theString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean(theString != 'a' and theString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));

                text = "select * from SupportBean(theString = 'a' and theString = 'c' and " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', theString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, false, false }));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] { "a", "b", "c" },
                    data => SendBeanString(env, (string)data),
                    milestone);
            }
        }

        private class ExprFilterWithEqualsSameCompare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(intBoxed=doubleBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false }));

                text = "select * from SupportBean(intBoxed=intBoxed and doubleBoxed=doubleBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, true }));

                text = "select * from SupportBean(doubleBoxed=intBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false }));

                text = "select * from SupportBean(doubleBoxed in (intBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false }));

                text = "select * from SupportBean(intBoxed in (doubleBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false }));

                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    2,
                    num => SendBeanIntDouble(env, new int[] { 1, 1 }[num], new double[] { 1, 10 }[num]),
                    milestone);

                assertions.Clear();
                text = "select * from SupportBean(doubleBoxed not in (10, intBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { false, true, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    3,
                    num => SendBeanIntDouble(env, new int[] { 1, 1, 1 }[num], new double[] { 1, 5, 10 }[num]),
                    milestone);

                assertions.Clear();
                text = "select * from SupportBean(doubleBoxed in (intBoxed:20))";
                assertions.Add(new EPLWithInvokedFlags(text, new bool[] { true, false, false }));
                MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                    env,
                    assertions,
                    3,
                    num => SendBeanIntDouble(env, new int[] { 0, 1, 2 }[num], new double[] { 1, 1, 1 }[num]),
                    milestone);
            }
        }

        private class ExprFilterEqualsSemanticFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBeanComplexProps(nested=nested)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var eventOne = SupportBeanComplexProps.MakeDefaultBean();
                eventOne.SimpleProperty = ("1");

                env.SendEventBean(eventOne);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterPatternWithExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var text = "@name('s0') select * from pattern [every a=SupportBean -> " +
                           "b=SupportMarketDataBean(a.longBoxed=volume*2)]";
                TryPatternWithExpr(env, text, milestone);

                text = "@name('s0') select * from pattern [every a=SupportBean -> " +
                       "b=SupportMarketDataBean(volume*2=a.longBoxed)]";
                TryPatternWithExpr(env, text, milestone);
            }
        }

        private class ExprFilterExprReversed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expr = "@name('s0') select * from SupportBean(5 = intBoxed)";
                env.CompileDeployAddListenerMileZero(expr, "s0");

                SendBean(env, "intBoxed", 5);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterRewriteWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryRewriteWhere(env, "", milestone);
                TryRewriteWhere(env, "@Hint('DISABLE_WHEREEXPR_MOVETO_FILTER')", milestone);
                TryRewriteWhereNamedWindow(env);
            }
        }

        private class ExprFilterNotEqualsOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean(theString != 'a')";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "a");
                env.AssertListenerNotInvoked("s0");

                var theEvent = SendEvent(env, "b");
                env.AssertListener(
                    "s0",
                    listener => Assert.AreSame(theEvent, listener.GetAndResetLastNewData()[0].Underlying));

                SendEvent(env, "a");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                SendEvent(env, null);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterCombinationEqualsOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean(theString != 'a', intPrimitive=0)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "b", 1);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                SendEvent(env, "a", 0);
                env.AssertListenerNotInvoked("s0");

                var theEvent = SendEvent(env, "x", 0);
                env.AssertListener(
                    "s0",
                    listener => Assert.AreSame(theEvent, listener.GetAndResetLastNewData()[0].Underlying));

                SendEvent(env, null, 0);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static object SendEvent(
            RegressionEnvironment env,
            string stringValue)
        {
            return SendEvent(env, stringValue, -1);
        }

        private static object SendEvent(
            RegressionEnvironment env,
            string stringValue,
            int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = (stringValue);
            theEvent.IntPrimitive = (intPrimitive);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private class ExprFilterInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from pattern [every a=SupportBean -> " +
                    "b=SupportMarketDataBean(sum(a.longBoxed) = 2)]",
                    "Aggregation functions not allowed within filters [");

                env.TryInvalidCompile(
                    "select * from pattern [every a=SupportBean(prior(1, a.longBoxed))]",
                    "Failed to validate filter expression 'prior(1,a.longBoxed)': Prior function cannot be used in this context [");

                env.TryInvalidCompile(
                    "select * from pattern [every a=SupportBean(prev(1, a.longBoxed))]",
                    "Failed to validate filter expression 'prev(1,a.longBoxed)': Previous function cannot be used in this context [");

                env.TryInvalidCompile(
                    "select * from SupportBean(5 - 10)",
                    "Filter expression not returning a boolean value: '5-10' [");

                env.TryInvalidCompile(
                    "select * from SupportBeanWithEnum(theString=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_1)",
                    "Failed to validate filter expression 'theString=ENUM_VALUE_1': Implicit conversion from datatype '" +
                    typeof(SupportEnum).FullName +
                    "' to 'String' is not allowed [");

                env.TryInvalidCompile(
                    "select * from SupportBeanWithEnum(supportEnum=A.b)",
                    "Failed to validate filter expression 'supportEnum=A.b': Failed to resolve property 'A.b' to a stream or nested property in a stream [");

                env.TryInvalidCompile(
                    "select * from pattern [a=SupportBean -> b=" +
                    typeof(SupportBean).Name +
                    "(doubleBoxed not in (doubleBoxed, x.intBoxed, 9))]",
                    "Failed to validate filter expression 'doubleBoxed not in (doubleBoxed,x.i...(45 chars)': Failed to find a stream named 'x' (did you mean 'b'?) [");

                env.TryInvalidCompile(
                    "select * from pattern [a=SupportBean" +
                    " -> b=SupportBean(cluedo.intPrimitive=a.intPrimitive)" +
                    " -> c=SupportBean" +
                    "]",
                    "Failed to validate filter expression 'cluedo.intPrimitive=a.intPrimitive': Failed to resolve property 'cluedo.intPrimitive' to a stream or nested property in a stream [");
            }
        }

        private static void TryRewriteWhereNamedWindow(RegressionEnvironment env)
        {
            var epl = "create window NamedWindowA#length(1) as SupportBean;\n" +
                      "select * from NamedWindowA mywindow WHERE (mywindow.theString.trim() is 'abc');\n";
            env.CompileDeploy(epl).UndeployAll();
        }

        private class ExprFilterInstanceMethodWWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodAlwaysTrue()) as s0",
                    new bool[] { true, true, true });
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodEventBean(s0, 'x', 1)) as s0",
                    new bool[] { false, true, false });
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodEventBean(*, 'x', 1)) as s0",
                    new bool[] { false, true, false });
            }

            private void TryFilterInstanceMethod(
                RegressionEnvironment env,
                string epl,
                bool[] expected)
            {
                env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
                for (var i = 0; i < 3; i++) {
                    env.SendEventBean(new SupportInstanceMethodBean(i));
                    env.AssertListenerInvokedFlag("s0", expected[i]);
                }

                env.UndeployAll();
            }
        }

        private class ExprFilterEqualsSemanticExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportBeanComplexProps(simpleProperty='1')#keepall as s0" +
                           ", SupportBeanComplexProps(simpleProperty='2')#keepall as s1" +
                           " where s0.nested = s1.nested";
                env.CompileDeploy(text).AddListener("s0");

                var eventOne = SupportBeanComplexProps.MakeDefaultBean();
                eventOne.SimpleProperty = ("1");

                var eventTwo = SupportBeanComplexProps.MakeDefaultBean();
                eventTwo.SimpleProperty = ("2");

                Assert.AreEqual(eventOne.Nested, eventTwo.Nested);

                env.SendEventBean(eventOne);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(eventTwo);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void SendBean(
            RegressionEnvironment env,
            string fieldName,
            object value)
        {
            var theEvent = new SupportBean();
            if (fieldName.Equals("theString")) {
                theEvent.TheString = ((string)value);
            }
            else if (fieldName.Equals("boolPrimitive")) {
                theEvent.BoolPrimitive = ((bool)value);
            }
            else if (fieldName.Equals("intBoxed")) {
                theEvent.IntBoxed = ((int?)value);
            }
            else if (fieldName.Equals("longBoxed")) {
                theEvent.LongBoxed = ((long?)value);
            }
            else {
                throw new ArgumentException("field name not known");
            }

            env.SendEventBean(theEvent);
        }

        private static void SendBeanLong(
            RegressionEnvironment env,
            long? longBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.LongBoxed = (longBoxed);
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntDoubleString(
            RegressionEnvironment env,
            int? intBoxed,
            double? doubleBoxed,
            string theString)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = (intBoxed);
            theEvent.DoubleBoxed = (doubleBoxed);
            theEvent.TheString = (theString);
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntDouble(
            RegressionEnvironment env,
            int? intBoxed,
            double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = (intBoxed);
            theEvent.DoubleBoxed = (doubleBoxed);
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntIntDouble(
            RegressionEnvironment env,
            int intPrimitive,
            int? intBoxed,
            double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = (intPrimitive);
            theEvent.IntBoxed = (intBoxed);
            theEvent.DoubleBoxed = (doubleBoxed);
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            SupportBean sb)
        {
            env.SendEventBean(sb);
        }

        private static void AssertListeners(
            RegressionEnvironment env,
            string[] statementNames,
            bool[] invoked)
        {
            for (var i = 0; i < invoked.Length; i++) {
                var index = i;
                env.AssertListener(
                    statementNames[i],
                    listener => Assert.AreEqual(
                        invoked[index],
                        listener.GetAndClearIsInvoked(),
                        "Failed for statement " + index + " name " + statementNames[index]));
            }
        }

        private static void SendBeanString(
            RegressionEnvironment env,
            string theString)
        {
            var num = new SupportBean(theString, -1);
            env.SendEventBean(num);
        }

        private static void TryPattern3Stream(
            RegressionEnvironment env,
            string text,
            AtomicLong milestone,
            int?[] intBoxedA,
            double?[] doubleBoxedA,
            int?[] intBoxedB,
            double?[] doubleBoxedB,
            int?[] intBoxedC,
            double?[] doubleBoxedC,
            bool[] expected)
        {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);
            Assert.AreEqual(intBoxedC.Length, doubleBoxedC.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedC.Length);

            for (var i = 0; i < intBoxedA.Length; i++) {
                env.CompileDeployAddListenerMile("@name('s0')" + text, "s0", milestone.GetAndIncrement());

                SendBeanIntDouble(env, intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(env, intBoxedB[i], doubleBoxedB[i]);
                SendBeanIntDouble(env, intBoxedC[i], doubleBoxedC[i]);
                var index = i;
                env.AssertListener(
                    "s0",
                    listener => Assert.AreEqual(
                        expected[index],
                        listener.GetAndClearIsInvoked(),
                        "failed at index " + index));

                env.UndeployAll();
            }
        }

        private static void Try3Fields(
            RegressionEnvironment env,
            AtomicLong milestone,
            string text,
            int[] intPrimitive,
            int?[] intBoxed,
            double?[] doubleBoxed,
            bool[] expected)
        {
            env.CompileDeployAddListenerMile("@name('s0')" + text, "s0", milestone.IncrementAndGet());

            Assert.AreEqual(intPrimitive.Length, doubleBoxed.Length);
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (var i = 0; i < intBoxed.Length; i++) {
                SendBeanIntIntDouble(env, intPrimitive[i], intBoxed[i], doubleBoxed[i]);
                var index = i;
                env.AssertListener(
                    "s0",
                    listener => Assert.AreEqual(
                        expected[index],
                        listener.GetAndClearIsInvoked(),
                        "failed at index " + index));
                if (i == 1) {
                    env.Milestone(milestone.IncrementAndGet());
                }
            }

            env.UndeployAll();
        }

        private static void TryPattern(
            RegressionEnvironment env,
            string text,
            AtomicLong milestone,
            int?[] intBoxedA,
            double?[] doubleBoxedA,
            int?[] intBoxedB,
            double?[] doubleBoxedB,
            bool[] expected)
        {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);

            for (var i = 0; i < intBoxedA.Length; i++) {
                env.CompileDeploy("@name('s0') " + text).AddListener("s0");

                SendBeanIntDouble(env, intBoxedA[i], doubleBoxedA[i]);

                env.MilestoneInc(milestone);

                SendBeanIntDouble(env, intBoxedB[i], doubleBoxedB[i]);
                var index = i;
                env.AssertListener(
                    "s0",
                    listener => Assert.AreEqual(
                        expected[index],
                        listener.GetAndClearIsInvoked(),
                        "failed at index " + index));
                env.UndeployAll();
            }
        }

        private static void TryPatternWithExpr(
            RegressionEnvironment env,
            string text,
            AtomicLong milestone)
        {
            env.CompileDeployAddListenerMile(text, "s0", milestone.GetAndIncrement());

            SendBeanLong(env, 10L);
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 0L, ""));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 5L, ""));
            env.AssertListenerInvoked("s0");

            SendBeanLong(env, 0L);
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 0L, ""));
            env.AssertListenerInvoked("s0");
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 1L, ""));
            env.AssertListenerNotInvoked("s0");

            SendBeanLong(env, 20L);
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 10L, ""));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private static void TryRewriteWhere(
            RegressionEnvironment env,
            string prefix,
            AtomicLong milestone)
        {
            var epl = prefix + " @name('s0') select * from SupportBean as A0 where A0.intPrimitive = 3";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            SendSupportBean(env, new SupportBean("E1", 3));
            env.AssertListenerInvoked("s0");

            SendSupportBean(env, new SupportBean("E2", 4));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }
    }
}