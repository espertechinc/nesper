///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedByMax
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new PatternMultiple());
            execs.Add(new PatternMixed());
            execs.Add(new PatternSinglePermFalseAndQuit());
            execs.Add(new PatternSingleMaxSimple());
            execs.Add(new PatternOperatorFollowedByMaxInvalid());
            return execs;
        }

        private static void RunAssertionSingleMaxSimple(
            RegressionEnvironment env,
            SupportConditionHandlerFactory.SupportConditionHandler handler)
        {
            string[] fields = {"a", "b"};

            env.SendEventBean(new SupportBean_A("A1"));
            env.SendEventBean(new SupportBean_A("A2"));

            handler.Contexts.Clear();
            env.SendEventBean(new SupportBean_A("A3"));
            AssertContext(env, handler.Contexts, 2);

            env.SendEventBean(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A1", "B1"}, new object[] {"A2", "B1"}});

            env.SendEventBean(new SupportBean_A("A4"));
            env.SendEventBean(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A4", "B2"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());

            for (var i = 5; i < 9; i++) {
                env.SendEventBean(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContext(env, handler.Contexts, 2);
                }
            }

            env.SendEventBean(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A5", "B3"}, new object[] {"A6", "B3"}});

            env.SendEventBean(new SupportBean_B("B4"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_A("A20"));
            env.SendEventBean(new SupportBean_A("A21"));
            env.SendEventBean(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A20", "B5"}, new object[] {"A21", "B5"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }

        private static void AssertContext(
            RegressionEnvironment env,
            IList<ConditionHandlerContext> contexts,
            int max)
        {
            Assert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            Assert.AreEqual("default", context.RuntimeURI);
            Assert.AreEqual(env.Statement("s0").DeploymentId, context.DeploymentId);
            Assert.AreEqual("s0", context.StatementName);
            var condition = (ConditionPatternSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            contexts.Clear();
        }

        internal class PatternOperatorFollowedByMaxInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]",
                    "InvalId maximum expression in followed-by, event properties are not allowed within the expression [select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern[a=SupportBean_A -[false]> SupportBean_B]",
                    "InvalId maximum expression in followed-by, the expression must return an integer value [select * from pattern[a=SupportBean_A -[false]> SupportBean_B]]");
            }
        }

        internal class PatternMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var handler = SupportConditionHandlerFactory.LastHandler;

                var expression = "@Name('s0') select a.Id as a, b.Id as b, c.Id as c from pattern [" +
                                 "every a=SupportBean_A -[2]> b=SupportBean_B -[3]> c=SupportBean_C]";
                env.CompileDeploy(expression).AddListener("s0");

                string[] fields = {"a", "b", "c"};

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_A("A4"));
                Assert.IsTrue(handler.Contexts.IsEmpty());

                env.SendEventBean(new SupportBean_B("B2"));
                AssertContext(env, handler.Contexts, 3);

                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsTrue(handler.Contexts.IsEmpty());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"A1", "B1", "C1"}, new object[] {"A2", "B1", "C1"},
                        new object[] {"A3", "B2", "C1"}
                    });

                env.UndeployAll();
            }
        }

        internal class PatternMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var handler = SupportConditionHandlerFactory.LastHandler;

                var expression = "@Name('s0') select a.Id as a, b.Id as b, c.Id as c from pattern [" +
                                 "every a=SupportBean_A -> b=SupportBean_B -[2]> c=SupportBean_C]";
                env.CompileDeploy(expression).AddListener("s0");

                TryAssertionMixed(env, handler);

                // test SODA
                env.UndeployAll();

                env.EplToModelCompileDeploy(expression).AddListener("s0");

                TryAssertionMixed(env, handler);

                env.UndeployAll();
            }

            private void TryAssertionMixed(
                RegressionEnvironment env,
                SupportConditionHandlerFactory.SupportConditionHandler handler)
            {
                string[] fields = {"a", "b", "c"};

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_A("A3"));

                handler.Contexts.Clear();
                env.SendEventBean(new SupportBean_B("B1"));
                AssertContext(env, handler.Contexts, 2);

                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsTrue(handler.Contexts.IsEmpty());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A1", "B1", "C1"}, new object[] {"A2", "B1", "C1"}});
            }
        }

        internal class PatternSinglePermFalseAndQuit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var context = SupportConditionHandlerFactory.FactoryContexts[0];
                Assert.AreEqual("default", context.RuntimeURI);
                var handler = SupportConditionHandlerFactory.LastHandler;
                handler.GetAndResetContexts();

                // not-operator
                var expression =
                    "@Name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B and not SupportBean_C)]";
                env.CompileDeploy(expression).AddListener("s0");
                string[] fields = {"a", "b"};

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_C("C1"));

                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_A("A4"));
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsTrue(handler.Contexts.IsEmpty());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A3", "B1"}, new object[] {"A4", "B1"}});

                env.SendEventBean(new SupportBean_A("A5"));
                env.SendEventBean(new SupportBean_A("A6"));
                env.SendEventBean(new SupportBean_A("A7"));
                AssertContext(env, handler.Contexts, 2);
                env.UndeployAll();

                // guard
                var expressionTwo =
                    "@Name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B where timer:within(1))]";
                env.CompileDeploy(expressionTwo).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.AdvanceTime(2000); // expires sub-expressions
                Assert.IsTrue(handler.Contexts.IsEmpty());

                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_A("A4"));
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsTrue(handler.Contexts.IsEmpty());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A3", "B1"}, new object[] {"A4", "B1"}});

                env.SendEventBean(new SupportBean_A("A5"));
                env.SendEventBean(new SupportBean_A("A6"));
                env.SendEventBean(new SupportBean_A("A7"));
                AssertContext(env, handler.Contexts, 2);

                env.UndeployAll();

                // every-operator
                var expressionThree =
                    "@Name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (every b=SupportBean_B(Id=a.Id) and not SupportBean_C(Id=a.Id))]";
                env.CompileDeploy(expressionThree).AddListener("s0");

                env.SendEventBean(new SupportBean_A("1"));
                env.SendEventBean(new SupportBean_A("2"));

                env.SendEventBean(new SupportBean_B("1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"1", "1"}});

                env.SendEventBean(new SupportBean_B("2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"2", "2"}});

                env.SendEventBean(new SupportBean_C("1"));

                env.SendEventBean(new SupportBean_A("3"));
                env.SendEventBean(new SupportBean_B("3"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"3", "3"}});

                env.UndeployAll();
            }
        }

        internal class PatternSingleMaxSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var context = SupportConditionHandlerFactory.FactoryContexts[0];
                Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);
                var handler = SupportConditionHandlerFactory.LastHandler;

                var expression =
                    "@Name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> b=SupportBean_B]";
                env.CompileDeploy(expression).AddListener("s0");
                RunAssertionSingleMaxSimple(env, handler);
                env.UndeployAll();

                // test SODA
                env.EplToModelCompileDeploy(expression).AddListener("s0");
                RunAssertionSingleMaxSimple(env, handler);
                env.UndeployAll();

                // test variable
                var path = new RegressionPath();
                env.CompileDeploy("create variable int myvar=3", path);
                expression =
                    "@Name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[myvar-1]> b=SupportBean_B]";
                env.CompileDeploy(expression, path).AddListener("s0");
                RunAssertionSingleMaxSimple(env, handler);

                env.UndeployAll();
            }
        }
    }
} // end of namespace