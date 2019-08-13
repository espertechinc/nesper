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

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterExpressions
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> executions = new List<RegressionExecution>();
            executions.Add(new ExprFilterConstant());
            executions.Add(new ExprFilterRelationalOpRange());
            executions.Add(new ExprFilterMathExpression());
            executions.Add(new ExprFilterBooleanExpr());
            executions.Add(new ExprFilterIn3ValuesAndNull());
            executions.Add(new ExprFilterNotEqualsNull());
            executions.Add(new ExprFilterInSet());
            executions.Add(new ExprFilterOverInClause());
            executions.Add(new ExprFilterNotEqualsConsolidate());
            executions.Add(new ExprFilterPromoteIndexToSetNotIn());
            executions.Add(new ExprFilterShortCircuitEvalAndOverspecified());
            executions.Add(new ExprFilterRelationalOpConstantFirst());
            executions.Add(new ExprFilterNullBooleanExpr());
            executions.Add(new ExprFilterEnumSyntaxOne());
            executions.Add(new ExprFilterEnumSyntaxTwo());
            executions.Add(new ExprFilterPatternFunc3Stream());
            executions.Add(new ExprFilterPatternFunc());
            executions.Add(new ExprFilterStaticFunc());
            executions.Add(new ExprFilterWithEqualsSameCompare());
            executions.Add(new ExprFilterEqualsSemanticFilter());
            executions.Add(new ExprFilterPatternWithExpr());
            executions.Add(new ExprFilterExprReversed());
            executions.Add(new ExprFilterRewriteWhere());
            executions.Add(new ExprFilterNotEqualsOp());
            executions.Add(new ExprFilterCombinationEqualsOp());
            executions.Add(new ExprFilterEqualsSemanticExpr());
            executions.Add(new ExprFilterInvalid());
            executions.Add(new ExprFilterInstanceMethodWWildcard());
            return executions;
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
            theEvent.TheString = stringValue;
            theEvent.IntPrimitive = intPrimitive;
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void TryRewriteWhereNamedWindow(RegressionEnvironment env)
        {
            var epl = "create window NamedWindowA#length(1) as SupportBean;\n" +
                      "select * from NamedWindowA mywindow WHERE (mywindow.TheString.trim() is 'abc');\n";
            env.CompileDeploy(epl).UndeployAll();
        }

        private static void SendBean(
            RegressionEnvironment env,
            string fieldName,
            object value)
        {
            var theEvent = new SupportBean();
            switch (fieldName) {
                case "TheString":
                    theEvent.TheString = (string) value;
                    break;

                case "BoolPrimitive":
                    theEvent.BoolPrimitive = value.AsBoolean();
                    break;

                case "IntBoxed":
                    theEvent.IntBoxed = value.AsBoxedInt();
                    break;

                case "LongBoxed":
                    theEvent.LongBoxed = value.AsBoxedLong();
                    break;

                default:
                    throw new ArgumentException("field name not known");
            }

            env.SendEventBean(theEvent);
        }

        private static void SendBeanLong(
            RegressionEnvironment env,
            long? longBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.LongBoxed = longBoxed;
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntDoubleString(
            RegressionEnvironment env,
            int? intBoxed,
            double? doubleBoxed,
            string theString)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            theEvent.TheString = theString;
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntDouble(
            RegressionEnvironment env,
            int? intBoxed,
            double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            env.SendEventBean(theEvent);
        }

        private static void SendBeanIntIntDouble(
            RegressionEnvironment env,
            int intPrimitive,
            int? intBoxed,
            double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
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
                Assert.AreEqual(
                    invoked[i],
                    env.Listener(statementNames[i]).GetAndClearIsInvoked(),
                    "Failed for statement " + i + " name " + statementNames[i]);
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
                env.CompileDeployAddListenerMile("@Name('s0')" + text, "s0", milestone.GetAndIncrement());

                SendBeanIntDouble(env, intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(env, intBoxedB[i], doubleBoxedB[i]);
                SendBeanIntDouble(env, intBoxedC[i], doubleBoxedC[i]);
                Assert.AreEqual(expected[i], env.Listener("s0").GetAndClearIsInvoked(), "failed at index " + i);

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
            env.CompileDeployAddListenerMile("@Name('s0')" + text, "s0", milestone.IncrementAndGet());

            Assert.AreEqual(intPrimitive.Length, doubleBoxed.Length);
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (var i = 0; i < intBoxed.Length; i++) {
                SendBeanIntIntDouble(env, intPrimitive[i], intBoxed[i], doubleBoxed[i]);
                Assert.AreEqual(expected[i], env.Listener("s0").GetAndClearIsInvoked(), "failed at index " + i);
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
                env.CompileDeploy("@Name('s0') " + text).AddListener("s0");

                SendBeanIntDouble(env, intBoxedA[i], doubleBoxedA[i]);

                env.MilestoneInc(milestone);

                SendBeanIntDouble(env, intBoxedB[i], doubleBoxedB[i]);
                Assert.AreEqual(expected[i], env.Listener("s0").GetAndClearIsInvoked(), "failed at index " + i);
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
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 5L, ""));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanLong(env, 0L);
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 0L, ""));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 1L, ""));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanLong(env, 20L);
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 10L, ""));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        private static void TryRewriteWhere(
            RegressionEnvironment env,
            string prefix,
            AtomicLong milestone)
        {
            var epl = prefix + " @Name('s0') select * from SupportBean as A0 where A0.IntPrimitive = 3";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            SendSupportBean(env, new SupportBean("E1", 3));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            SendSupportBean(env, new SupportBean("E2", 4));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        internal class ExprFilterRelationalOpRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(IntBoxed in [2:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, true, false}));

                text = "select * from SupportBean(IntBoxed in [2:3] and IntBoxed in [2:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, true, false}));

                text = "select * from SupportBean(IntBoxed in [2:3] and IntBoxed in [2:2])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false, false}));

                text = "select * from SupportBean(IntBoxed in [1:10] and IntBoxed in [3:2])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, true, false}));

                text = "select * from SupportBean(IntBoxed in [3:3] and IntBoxed in [1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, true, false}));

                text = "select * from SupportBean(IntBoxed in [3:3] and IntBoxed in [1:3] and IntBoxed in [4:5])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, false, false}));

                text = "select * from SupportBean(IntBoxed not in [3:3] and IntBoxed not in [1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, false, true}));

                text = "select * from SupportBean(IntBoxed not in (2:4) and IntBoxed not in (1:3))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false, false, true}));

                text = "select * from SupportBean(IntBoxed not in [2:4) and IntBoxed not in [1:3))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, false, true}));

                text = "select * from SupportBean(IntBoxed not in (2:4] and IntBoxed not in (1:3])";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false, false, false}));

                text = "select * from SupportBean where IntBoxed not in (2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, true, false, true}));

                text = "select * from SupportBean where IntBoxed not in [2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false, false, false}));

                text = "select * from SupportBean where IntBoxed not in [2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false, false, true}));

                text = "select * from SupportBean where IntBoxed not in (2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, true, false, false}));

                text = "select * from SupportBean where IntBoxed in (2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, true, false}));

                text = "select * from SupportBean where IntBoxed in [2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, true, true}));

                text = "select * from SupportBean where IntBoxed in [2:4)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, true, false}));

                text = "select * from SupportBean where IntBoxed in (2:4]";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, true, true}));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] {1, 2, 3, 4},
                    data => SendBeanIntDouble(env, data.AsInt(), 0D),
                    milestone);
            }
        }

        internal class ExprFilterMathExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<string> epl = new List<string>();
                var milestone = new AtomicLong();

                epl.Add("select * from SupportBean(IntBoxed*DoubleBoxed > 20)");
                epl.Add("select * from SupportBean(20 < IntBoxed*DoubleBoxed)");
                epl.Add("select * from SupportBean(20/IntBoxed < DoubleBoxed)");
                epl.Add("select * from SupportBean(20/IntBoxed/DoubleBoxed < 1)");

                MultiStmtAssertUtil.RunSendAssertPairs(
                    env,
                    epl,
                    new[] {
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 5d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => Assert.IsTrue(env.Listener(statementName).GetAndClearIsInvoked())),
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 4d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => Assert.IsFalse(env.Listener(statementName).GetAndClearIsInvoked())),
                        new SendAssertPair(
                            () => SendBeanIntDouble(env, 5, 4.001d),
                            (
                                eventIndex,
                                statementName,
                                failMessage) => Assert.IsTrue(env.Listener(statementName).GetAndClearIsInvoked()))
                    },
                    milestone);
            }
        }

        internal class ExprFilterBooleanExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select * from SupportBean(2*IntBoxed=DoubleBoxed)";
                env.CompileDeployAddListenerMile(text, "s0", 0);

                SendBeanIntDouble(env, 20, 50d);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendBeanIntDouble(env, 25, 50d);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                text = "@Name('s1') select * from SupportBean(2*IntBoxed=DoubleBoxed, TheString='s')";
                env.CompileDeployAddListenerMile(text, "s1", 1);

                SendBeanIntDoubleString(env, 25, 50d, "s");
                Assert.IsTrue(env.Listener("s1").GetAndClearIsInvoked());
                SendBeanIntDoubleString(env, 25, 50d, "x");
                Assert.IsFalse(env.Listener("s1").GetAndClearIsInvoked());

                env.UndeployAll();

                // test priority of equals and boolean
                env.CompileDeploy("@Name('s0') select * from SupportBean(IntPrimitive = 1 or IntPrimitive = 2)")
                    .AddListener("s0");
                env.CompileDeploy(
                        "@Name('s1') select * from SupportBean(IntPrimitive = 3, SupportStaticMethodLib.alwaysTrue())")
                    .AddListener("s1");

                SupportStaticMethodLib.Invocations.Clear();
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(SupportStaticMethodLib.Invocations.IsEmpty());

                env.UndeployAll();
            }
        }

        internal class ExprFilterIn3ValuesAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from SupportBean(IntPrimitive in (IntBoxed, DoubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {0, 1, 0},
                    new double?[] {2d, 2d, 1d},
                    new[] {false, true, true});

                text = "select * from SupportBean(IntPrimitive in (IntBoxed, " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(DoubleBoxed)))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {0, 1, 0},
                    new double?[] {2d, 2d, 1d},
                    new[] {true, true, false});

                text = "select * from SupportBean(IntPrimitive not in (IntBoxed, DoubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {0, 1, 0},
                    new double?[] {2d, 2d, 1d},
                    new[] {true, false, false});

                text = "select * from SupportBean(IntBoxed = DoubleBoxed)";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {null, 1, null},
                    new double?[] {null, null, 1d},
                    new[] {false, false, false});

                text = "select * from SupportBean(IntBoxed in (DoubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {null, 1, null},
                    new double?[] {null, null, 1d},
                    new[] {false, false, false});

                text = "select * from SupportBean(IntBoxed not in (DoubleBoxed))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {null, 1, null},
                    new double?[] {null, null, 1d},
                    new[] {false, false, false});

                text = "select * from SupportBean(IntBoxed in [DoubleBoxed:10))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {null, 1, 2},
                    new double?[] {null, null, 1d},
                    new[] {false, false, true});

                text = "select * from SupportBean(IntBoxed not in [DoubleBoxed:10))";
                Try3Fields(
                    env,
                    milestone,
                    text,
                    new[] {1, 1, 1},
                    new int?[] {null, 1, 2},
                    new double?[] {null, null, 1d},
                    new[] {false, true, false});
            }
        }

        internal class ExprFilterNotEqualsNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string[] stmts;
                string epl;

                // test equals&where-clause (can be optimized into filter)
                env.CompileDeploy("@Name('s0') select * from SupportBean where TheString != 'A'").AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from SupportBean where TheString != 'A' or IntPrimitive != 0")
                    .AddListener("s1");
                env.CompileDeploy("@Name('s2') select * from SupportBean where TheString = 'A'").AddListener("s2");
                env.CompileDeploy("@Name('s3') select * from SupportBean where TheString = 'A' or IntPrimitive != 0")
                    .AddListener("s3");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new[] {false, false, false, false});

                SendSupportBean(env, new SupportBean(null, 1));
                AssertListeners(env, stmts, new[] {false, true, false, true});

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new[] {false, false, true, true});

                SendSupportBean(env, new SupportBean("A", 1));
                AssertListeners(env, stmts, new[] {false, true, true, true});

                SendSupportBean(env, new SupportBean("B", 0));
                AssertListeners(env, stmts, new[] {true, true, false, false});

                SendSupportBean(env, new SupportBean("B", 1));
                AssertListeners(env, stmts, new[] {true, true, false, true});

                env.UndeployAll();

                // test equals&selection
                var fields = "val0,val1,val2,val3,val4,val5".SplitCsv();
                epl = "@Name('s0') select " +
                      "TheString != 'A' as val0, " +
                      "TheString != 'A' or IntPrimitive != 0 as val1, " +
                      "TheString != 'A' and IntPrimitive != 0 as val2, " +
                      "TheString = 'A' as val3," +
                      "TheString = 'A' or IntPrimitive != 0 as val4, " +
                      "TheString = 'A' and IntPrimitive != 0 as val5 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0").MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, false, null, null, false});

                env.MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        null, true, null, null, true, null
                    });

                SendSupportBean(env, new SupportBean("A", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, true, true, false});

                SendSupportBean(env, new SupportBean("A", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, true, true, true});

                SendSupportBean(env, new SupportBean("B", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false, false, false});

                SendSupportBean(env, new SupportBean("B", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, false, true, false});

                env.UndeployAll().MilestoneInc(milestone);

                // test is-and-isnot&where-clause
                env.CompileDeploy("@Name('s0') select * from SupportBean where TheString is null").AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from SupportBean where TheString is null or IntPrimitive != 0")
                    .AddListener("s1");
                env.CompileDeploy("@Name('s2') select * from SupportBean where TheString is not null")
                    .AddListener("s2");
                env.CompileDeploy(
                        "@Name('s3') select * from SupportBean where TheString is not null or IntPrimitive != 0")
                    .AddListener("s3");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new[] {true, true, false, false});

                SendSupportBean(env, new SupportBean(null, 1));
                AssertListeners(env, stmts, new[] {true, true, false, true});

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new[] {false, false, true, true});

                SendSupportBean(env, new SupportBean("A", 1));
                AssertListeners(env, stmts, new[] {false, true, true, true});

                env.UndeployAll();

                // test is-and-isnot&selection
                epl = "@Name('s0') select " +
                      "TheString is null as val0, " +
                      "TheString is null or IntPrimitive != 0 as val1, " +
                      "TheString is null and IntPrimitive != 0 as val2, " +
                      "TheString is not null as val3," +
                      "TheString is not null or IntPrimitive != 0 as val4, " +
                      "TheString is not null and IntPrimitive != 0 as val5 " +
                      "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0").MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean(null, 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false, false, false});

                SendSupportBean(env, new SupportBean(null, 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, false, true, false});

                SendSupportBean(env, new SupportBean("A", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, true, true, false});

                SendSupportBean(env, new SupportBean("A", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, true, true, true});

                env.UndeployAll();

                // filter expression
                env.CompileDeploy("@Name('s0') select * from SupportBean(TheString is null)").AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from SupportBean where TheString = null").AddListener("s1");
                env.CompileDeploy("@Name('s2') select * from SupportBean(TheString = null)").AddListener("s2");
                env.CompileDeploy("@Name('s3') select * from SupportBean(TheString is not null)").AddListener("s3");
                env.CompileDeploy("@Name('s4') select * from SupportBean where TheString != null").AddListener("s4");
                env.CompileDeploy("@Name('s5') select * from SupportBean(TheString != null)").AddListener("s5");
                env.MilestoneInc(milestone);
                stmts = "s0,s1,s2,s3,s4,s5".SplitCsv();

                SendSupportBean(env, new SupportBean(null, 0));
                AssertListeners(env, stmts, new[] {true, false, false, false, false, false});

                SendSupportBean(env, new SupportBean("A", 0));
                AssertListeners(env, stmts, new[] {false, false, false, true, false, false});

                env.UndeployAll();

                // select constants
                fields = "val0,val1,val2,val3".SplitCsv();
                env.CompileDeploy(
                        "@Name('s0') select " +
                        "2 != null as val0," +
                        "null = null as val1," +
                        "2 != null or 1 = 2 as val2," +
                        "2 != null and 2 = 2 as val3 " +
                        "from SupportBean")
                    .AddListener("s0");
                env.MilestoneInc(milestone);

                SendSupportBean(env, new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.UndeployAll();

                // test SODA
                epl =
                    "@Name('s0') select IntBoxed is null, IntBoxed is not null, IntBoxed=1, IntBoxed!=1 from SupportBean";
                env.EplToModelCompileDeploy(epl);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new[] {
                        "IntBoxed is null", "IntBoxed is not null",
                        "IntBoxed=1", "IntBoxed!=1"
                    },
                    env.Statement("s0").EventType.PropertyNames);
                env.UndeployAll();
            }
        }

        internal class ExprFilterInSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from \n" +
                          "pattern [ \n" +
                          " every start_load=SupportBeanArrayCollMap \n" +
                          " -> \n" +
                          " single_load=SupportBean(TheString in (start_load.SetOfString)) \n" +
                          "]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var setOfString = new HashSet<string>();
                setOfString.Add("Version1");
                setOfString.Add("Version2");
                env.SendEventBean(new SupportBeanArrayCollMap(setOfString));

                env.SendEventBean(new SupportBean("Version1", 0));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterOverInClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from pattern[every event1=SupportTradeEvent(userId in ('100','101'),amount>=1000)]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportTradeEvent(1, "100", 1001));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("event1.Id"));

                var eplTwo =
                    "@Name('s1') select * from pattern [every event1=SupportTradeEvent(userId in ('100','101'))]";
                env.CompileDeployAddListenerMileZero(eplTwo, "s1");

                env.SendEventBean(new SupportTradeEvent(2, "100", 1001));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("event1.Id"));
                Assert.AreEqual(2, env.Listener("s1").AssertOneGetNewAndReset().Get("event1.Id"));

                env.UndeployAll();
            }
        }

        internal class ExprFilterNotEqualsConsolidate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                string[] epl = {
                    "select * from SupportBean(IntPrimitive not in (1, 2))",
                    "select * from SupportBean(IntPrimitive != 1, IntPrimitive != 2)",
                    "select * from SupportBean(IntPrimitive != 1 and IntPrimitive != 2)"
                };
                MultiStmtAssertUtil.RunEPL(
                    env,
                    Arrays.AsList(epl),
                    new object[] {0, 1, 2, 3, 4},
                    data => SendSupportBean(env, new SupportBean("", data.AsInt())),
                    (
                        eventIndex,
                        eventData,
                        assertionDesc,
                        statementName,
                        failMessage) => {
                        if (1.Equals(eventData) || 2.Equals(eventData)) {
                            Assert.IsFalse(env.Listener(statementName).IsInvoked, failMessage);
                        }
                        else {
                            Assert.IsTrue(env.Listener(statementName).IsInvoked, failMessage);
                        }

                        env.Listener(statementName).Reset();
                    },
                    milestone);
            }
        }

        internal class ExprFilterPromoteIndexToSetNotIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@Name('s0') select * from SupportBean(TheString != 'x' and TheString != 'y' and DoubleBoxed is not null)";
                var eplTwo =
                    "@Name('s1') select * from SupportBean(TheString != 'x' and TheString != 'y' and LongBoxed is not null)";

                env.CompileDeploy(eplOne).AddListener("s0");
                env.CompileDeploy(eplTwo).AddListener("s1");
                env.Milestone(0);

                var bean = new SupportBean("E1", 0);
                bean.DoubleBoxed = 1d;
                bean.LongBoxed = 1L;
                env.SendEventBean(bean);

                env.Listener("s0").AssertOneGetNewAndReset();
                env.Listener("s1").AssertOneGetNewAndReset();

                env.UndeployAll();
            }
        }

        internal class ExprFilterShortCircuitEvalAndOverspecified : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from SupportRuntimeExBean(SupportRuntimeExBean.property2 = '4' and SupportRuntimeExBean.property1 = '1')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportRuntimeExBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked, "Subscriber should not have received result(s)");

                env.UndeployAll();

                epl = "@Name('s0') select * from SupportBean(TheString='A' and TheString='B')";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendSupportBean(env, new SupportBean("A", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterRelationalOpConstantFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 < x",
                        new[] {false, false, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 <= x",
                        new[] {false, true, true}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 > x",
                        new[] {true, false, false}));
                assertions.Add(
                    new EPLWithInvokedFlags(
                        "select * from SupportInstanceMethodBean where 4 >= x",
                        new[] {true, true, false}));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] {3, 4, 5},
                    data => env.SendEventBean(new SupportInstanceMethodBean(data.AsInt())),
                    milestone);
            }
        }

        internal class ExprFilterNullBooleanExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [every event1=SupportTradeEvent(userId like '123%')]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportTradeEvent(1, null, 1001));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportTradeEvent(2, "1234", 1001));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("event1.Id"));

                env.UndeployAll();
            }
        }

        internal class ExprFilterConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [SupportBean(IntPrimitive=" +
                          typeof(ISupportA).FullName +
                          ".VALUE_1)]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var theEvent = new SupportBean("e1", 2);
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                theEvent = new SupportBean("e1", 1);
                env.SendEventBean(theEvent);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterEnumSyntaxOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [SupportBeanWithEnum(SupportEnum=" +
                          typeof(SupportEnum).FullName +
                          ".ValueOf('ENUM_VALUE_1'))]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterEnumSyntaxTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [SupportBeanWithEnum(SupportEnum=" +
                          typeof(SupportEnum).FullName +
                          ".ENUM_VALUE_2)]";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                // test where clause
                epl = "@Name('s0') select * from SupportBeanWithEnum where SupportEnum=" +
                      typeof(SupportEnum).FullName +
                      ".ENUM_VALUE_2";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
                env.SendEventBean(theEvent);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterPatternFunc3Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed and IntBoxed != null)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 4, -2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 5, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, false, false, false, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed is a.IntBoxed, IntBoxed is b.IntBoxed and IntBoxed is not null)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 4, -2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 5, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, true, false, false, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed=a.IntBoxed or IntBoxed=b.IntBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 4, -2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 5, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, true, true, true, false, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 4, -2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 5, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, true, false, false, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed!=a.IntBoxed, IntBoxed!=b.IntBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 4, -2},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, 3, 1, 8, null, 5, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, false, false, false, true, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed!=a.IntBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 8, null, 2, 1, null, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {-2, null, null, 3, 1, 8, 4},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, null, null, 3, 1, 8, 5},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, false, true, false, false, true});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed is not a.IntBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 8, null, 2, 1, null, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {-2, null, null, 3, 1, 8, 4},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {null, null, null, 3, 1, 8, 5},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {true, true, false, true, false, true, true});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed=a.IntBoxed, DoubleBoxed=b.DoubleBoxed)]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 2, 1, 2, 1, 7, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {0, 0, 0, 0, 0, 0, 0},
                    new double?[] {1d, 2d, 0d, 2d, 0d, 1d, 0d},
                    new int?[] {2, 2, 3, 2, 1, 7, 5},
                    new double?[] {1d, 1d, 1d, 2d, 1d, 1d, 1d},
                    new[] {true, false, false, true, false, true, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed in (a.IntBoxed, b.IntBoxed))]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 1, 1, null, 1, null, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {1, 2, 1, null, null, 2, 0},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {2, 2, 3, null, 1, null, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {true, true, false, false, true, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed in [a.IntBoxed:b.IntBoxed])]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 1, 1, null, 1, null, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {1, 2, 1, null, null, 2, 0},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {2, 1, 3, null, 1, null, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {true, true, false, false, false, false, false});

                text = "select * from pattern [" +
                       "a=SupportBean -> " +
                       "b=SupportBean -> " +
                       "c=SupportBean(IntBoxed not in [a.IntBoxed:b.IntBoxed])]";
                TryPattern3Stream(
                    env,
                    text,
                    milestone,
                    new int?[] {2, 1, 1, null, 1, null, 1},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {1, 2, 1, null, null, 2, 0},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[] {2, 1, 3, null, 1, null, null},
                    new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[] {false, false, true, false, false, false, false});
            }
        }

        internal class ExprFilterPatternFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                var milestone = new AtomicLong();

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(IntBoxed = a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {2d, 2d, 2d, 1d, 5d, 6d, 7d},
                    new int?[] {null, 3, 1, 8, null, 1, 2},
                    new double?[] {2d, 3d, 2d, 1d, 5d, 6d, 8d},
                    new[] {false, false, true, false, false, true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(IntBoxed is a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {null, 2, 1, null, 8, 1, 2},
                    new double?[] {2d, 2d, 2d, 1d, 5d, 6d, 7d},
                    new int?[] {null, 3, 1, 8, null, 1, 2},
                    new double?[] {2d, 3d, 2d, 1d, 5d, 6d, 8d},
                    new[] {true, false, true, false, false, true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.DoubleBoxed = DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0},
                    new double?[] {2d, 2d},
                    new int?[] {0, 0},
                    new double?[] {2d, 3d},
                    new[] {true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.DoubleBoxed = b.DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0},
                    new double?[] {2d, 2d},
                    new int?[] {0, 0},
                    new double?[] {2d, 3d},
                    new[] {true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.DoubleBoxed != DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0},
                    new double?[] {2d, 2d},
                    new int?[] {0, 0},
                    new double?[] {2d, 3d},
                    new[] {false, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(a.DoubleBoxed != b.DoubleBoxed)]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0},
                    new double?[] {2d, 2d},
                    new int?[] {0, 0},
                    new double?[] {2d, 3d},
                    new[] {false, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, true, true, true, true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed in (a.DoubleBoxed:a.IntBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, false, true, true, true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.DoubleBoxed in (a.DoubleBoxed:a.IntBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, false, true, true, false, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, true, true, true, false, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, false, false, false, false, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed])]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, true, false, false, false, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, true, false, false, true, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, false, false, false, true, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed not in (a.DoubleBoxed, a.IntBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, false, true, false, false, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed in (a.DoubleBoxed, a.IntBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, true, false, true, true, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(b.DoubleBoxed in (DoubleBoxed, a.IntBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {true, true, true, true, true, true});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed not in (DoubleBoxed, a.IntBoxed, 9))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {1, 1, 1, 1, 1, 1},
                    new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[] {0, 0, 0, 0, 0, 0},
                    new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                    new[] {false, false, false, false, false, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.DoubleBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0, 0},
                    new double?[] {10d, 10d, 10d},
                    new int?[] {0, 0, 0},
                    new double?[] {9d, 10d, 11d},
                    new[] {true, false, false});

                text = "select * from pattern [a=SupportBean -> b=" +
                       typeof(SupportBean).Name +
                       "(DoubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.DoubleBoxed) or " +
                       "DoubleBoxed = " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".minusOne(a.IntBoxed))]";
                TryPattern(
                    env,
                    text,
                    milestone,
                    new int?[] {0, 0, 12},
                    new double?[] {10d, 10d, 10d},
                    new int?[] {0, 0, 0},
                    new double?[] {9d, 10d, 11d},
                    new[] {true, false, true});
            }
        }

        internal class ExprFilterStaticFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(" +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('b', TheString))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean(" +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', TheString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean('b'=TheString," +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', TheString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean('b'=TheString, TheString='b', TheString != 'a')";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean(TheString != 'a', TheString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean(TheString = 'b', TheString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean(TheString != 'a' and TheString != 'c')";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                text = "select * from SupportBean(TheString = 'a' and TheString = 'c' and " +
                       typeof(SupportStaticMethodLib).FullName +
                       ".isStringEquals('bx', TheString || 'x'))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, false, false}));

                MultiStmtAssertUtil.RunIsInvokedWTestdata(
                    env,
                    assertions,
                    new object[] {"a", "b", "c"},
                    data => SendBeanString(env, (string) data),
                    milestone);
            }
        }

        internal class ExprFilterWithEqualsSameCompare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text;
                IList<EPLWithInvokedFlags> assertions = new List<EPLWithInvokedFlags>();
                var milestone = new AtomicLong();

                text = "select * from SupportBean(IntBoxed=DoubleBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false}));

                text = "select * from SupportBean(IntBoxed=IntBoxed and DoubleBoxed=DoubleBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, true}));

                text = "select * from SupportBean(DoubleBoxed=IntBoxed)";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false}));

                text = "select * from SupportBean(DoubleBoxed in (IntBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false}));

                text = "select * from SupportBean(IntBoxed in (DoubleBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false}));

                {
                    var intArray = new[] {1, 1};
                    var doubleArray = new double[] {1, 10};

                    MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                        env,
                        assertions,
                        2,
                        num => SendBeanIntDouble(env, intArray[num], doubleArray[num]),
                        milestone);
                }

                assertions.Clear();
                text = "select * from SupportBean(DoubleBoxed not in (10, IntBoxed))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {false, true, false}));

                {
                    var intArray = new[] {1, 1, 1};
                    var doubleArray = new double[] {1, 5, 10};

                    MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                        env,
                        assertions,
                        3,
                        num => SendBeanIntDouble(env, intArray[num], doubleArray[num]),
                        milestone);
                }

                assertions.Clear();
                text = "select * from SupportBean(DoubleBoxed in (IntBoxed:20))";
                assertions.Add(new EPLWithInvokedFlags(text, new[] {true, false, false}));

                {
                    var intArray = new[] {0, 1, 2};
                    var doubleArray = new double[] {1, 1, 1};

                    MultiStmtAssertUtil.RunIsInvokedWithEventSender(
                        env,
                        assertions,
                        3,
                        num => SendBeanIntDouble(
                            env,
                            intArray[num],
                            doubleArray[num]),
                        milestone);
                }
            }
        }

        internal class ExprFilterEqualsSemanticFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBeanComplexProps(nested=nested)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var eventOne = SupportBeanComplexProps.MakeDefaultBean();
                eventOne.SimpleProperty = "1";

                env.SendEventBean(eventOne);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterPatternWithExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var text = "@Name('s0') select * from pattern [every a=SupportBean -> " +
                           "b=SupportMarketDataBean(a.LongBoxed=Volume*2)]";
                TryPatternWithExpr(env, text, milestone);

                text = "@Name('s0') select * from pattern [every a=SupportBean -> " +
                       "b=SupportMarketDataBean(Volume*2=a.LongBoxed)]";
                TryPatternWithExpr(env, text, milestone);
            }
        }

        internal class ExprFilterExprReversed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expr = "@Name('s0') select * from SupportBean(5 = IntBoxed)";
                env.CompileDeployAddListenerMileZero(expr, "s0");

                SendBean(env, "IntBoxed", 5);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ExprFilterRewriteWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryRewriteWhere(env, "", milestone);
                TryRewriteWhere(env, "@Hint('DISABLE_WHEREEXPR_MOVETO_FILTER')", milestone);
                TryRewriteWhereNamedWindow(env);
            }
        }

        internal class ExprFilterNotEqualsOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean(TheString != 'a')";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "a");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var theEvent = SendEvent(env, "b");
                Assert.AreSame(theEvent, env.Listener("s0").GetAndResetLastNewData()[0].Underlying);

                SendEvent(env, "a");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendEvent(env, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterCombinationEqualsOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean(TheString != 'a', IntPrimitive=0)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "b", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendEvent(env, "a", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var theEvent = SendEvent(env, "x", 0);
                Assert.AreSame(theEvent, env.Listener("s0").GetAndResetLastNewData()[0].Underlying);

                SendEvent(env, null, 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprFilterInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern [every a=SupportBean -> " +
                    "b=SupportMarketDataBean(sum(a.LongBoxed) = 2)]",
                    "Aggregation functions not allowed within filters [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern [every a=SupportBean(prior(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prior(1,a.LongBoxed)': Prior function cannot be used in this context [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern [every a=SupportBean(prev(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prev(1,a.LongBoxed)': Previous function cannot be used in this context [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean(5 - 10)",
                    "Filter expression not returning a boolean value: '5-10' [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBeanWithEnum(TheString=" +
                    typeof(SupportEnum).FullName +
                    ".ENUM_VALUE_1)",
                    "Failed to validate filter expression 'TheString=ENUM_VALUE_1': Implicit conversion from datatype 'SupportEnum' to 'String' is not allowed [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBeanWithEnum(SupportEnum=A.b)",
                    "Failed to validate filter expression 'SupportEnum=A.b': Failed to resolve property 'A.b' to a stream or nested property in a stream [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern [a=SupportBean -> b=" +
                    typeof(SupportBean).Name +
                    "(DoubleBoxed not in (DoubleBoxed, x.IntBoxed, 9))]",
                    "Failed to validate filter expression 'DoubleBoxed not in (DoubleBoxed,x.i...(45 chars)': Failed to find a stream named 'x' (did you mean 'b'?) [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern [a=SupportBean" +
                    " -> b=SupportBean(cluedo.IntPrimitive=a.IntPrimitive)" +
                    " -> c=SupportBean" +
                    "]",
                    "Failed to validate filter expression 'cluedo.IntPrimitive=a.IntPrimitive': Failed to resolve property 'cluedo.IntPrimitive' to a stream or nested property in a stream [");
            }
        }

        internal class ExprFilterInstanceMethodWWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodAlwaysTrue()) as s0",
                    new[] {true, true, true});
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodEventBean(s0, 'x', 1)) as s0",
                    new[] {false, true, false});
                TryFilterInstanceMethod(
                    env,
                    "select * from SupportInstanceMethodBean(s0.myInstanceMethodEventBean(*, 'x', 1)) as s0",
                    new[] {false, true, false});
            }

            private void TryFilterInstanceMethod(
                RegressionEnvironment env,
                string epl,
                bool[] expected)
            {
                env.CompileDeploy("@Name('s0') " + epl).AddListener("s0");
                for (var i = 0; i < 3; i++) {
                    env.SendEventBean(new SupportInstanceMethodBean(i));
                    Assert.AreEqual(expected[i], env.Listener("s0").GetAndClearIsInvoked());
                }

                env.UndeployAll();
            }
        }

        internal class ExprFilterEqualsSemanticExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select * from SupportBeanComplexProps(simpleProperty='1')#keepall as s0" +
                           ", SupportBeanComplexProps(simpleProperty='2')#keepall as s1" +
                           " where s0.Nested = s1.Nested";
                env.CompileDeploy(text).AddListener("s0");

                var eventOne = SupportBeanComplexProps.MakeDefaultBean();
                eventOne.SimpleProperty = "1";

                var eventTwo = SupportBeanComplexProps.MakeDefaultBean();
                eventTwo.SimpleProperty = "2";

                Assert.AreEqual(eventOne.Nested, eventTwo.Nested);

                env.SendEventBean(eventOne);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(eventTwo);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
}