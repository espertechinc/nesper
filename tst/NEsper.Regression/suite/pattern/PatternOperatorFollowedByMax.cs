///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.compat;
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
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithMultiple(execs);
            WithMixed(execs);
            WithSinglePermFalseAndQuit(execs);
            WithSingleMaxSimple(execs);
            WithOperatorFollowedByMaxInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOperatorFollowedByMaxInvalid(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorFollowedByMaxInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleMaxSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternSingleMaxSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithSinglePermFalseAndQuit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternSinglePermFalseAndQuit());
            return execs;
        }

        public static IList<RegressionExecution> WithMixed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternMixed());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternMultiple());
            return execs;
        }

        private class PatternOperatorFollowedByMaxInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, event properties are not allowed within the expression [select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]]");
                env.TryInvalidCompile(
                    "select * from pattern[a=SupportBean_A -[false]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, the expression must return an integer value [select * from pattern[a=SupportBean_A -[false]> SupportBean_B]]");
            }
        }

        private class PatternMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var expression = "@name('s0') select a.Id as a, b.Id as b, c.Id as c from pattern [" +
                                 "every a=SupportBean_A -[2]> b=SupportBean_B -[3]> c=SupportBean_C]";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new string[] { "a", "b", "c" };

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_B("B1"));

                env.SendEventBean(new SupportBean_A("A3"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_A("A4"));
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_B("B2"));
                AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 3);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A1", "B1", "C1" }, new object[] { "A2", "B1", "C1" },
                        new object[] { "A3", "B2", "C1" }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class PatternMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var expression = "@name('s0') select a.Id as a, b.Id as b, c.Id as c from pattern [" +
                                 "every a=SupportBean_A -> b=SupportBean_B -[2]> c=SupportBean_C]";
                env.CompileDeploy(expression).AddListener("s0");

                TryAssertionMixed(env, milestone);

                // test SODA
                env.UndeployAll();

                env.EplToModelCompileDeploy(expression).AddListener("s0");

                TryAssertionMixed(env, milestone);

                env.UndeployAll();
            }

            private void TryAssertionMixed(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var fields = new string[] { "a", "b", "c" };

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_A("A3"));

                SupportConditionHandlerFactory.LastHandler.Contexts.Clear();
                env.SendEventBean(new SupportBean_B("B1"));
                AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 2);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A1", "B1", "C1" }, new object[] { "A2", "B1", "C1" } });
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class PatternSinglePermFalseAndQuit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                env.AdvanceTime(0);
                var context = SupportConditionHandlerFactory.FactoryContexts[0];
                Assert.AreEqual("default", context.RuntimeURI);
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts();

                // not-operator
                var expression =
                    "@name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B and not SupportBean_C)]";
                env.CompileDeploy(expression).AddListener("s0");
                var fields = new string[] { "a", "b" };

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_C("C1"));

                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_A("A4"));
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A3", "B1" }, new object[] { "A4", "B1" } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_A("A5"));
                env.SendEventBean(new SupportBean_A("A6"));
                env.SendEventBean(new SupportBean_A("A7"));
                AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 2);
                env.UndeployAll();

                // guard
                var expressionTwo =
                    "@name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B where timer:within(1))]";
                env.CompileDeploy(expressionTwo).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.AdvanceTime(2000); // expires sub-expressions
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_A("A4"));
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A3", "B1" }, new object[] { "A4", "B1" } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_A("A5"));
                env.SendEventBean(new SupportBean_A("A6"));
                env.SendEventBean(new SupportBean_A("A7"));
                AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 2);

                env.UndeployAll();

                // every-operator
                var expressionThree =
                    "@name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> (every b=SupportBean_B(id=a.Id) and not SupportBean_C(Id=a.Id))]";
                env.CompileDeploy(expressionThree).AddListener("s0");

                env.SendEventBean(new SupportBean_A("1"));
                env.SendEventBean(new SupportBean_A("2"));

                env.SendEventBean(new SupportBean_B("1"));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "1", "1" } });

                env.SendEventBean(new SupportBean_B("2"));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "2", "2" } });

                env.SendEventBean(new SupportBean_C("1"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_A("3"));
                env.SendEventBean(new SupportBean_B("3"));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "3", "3" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class PatternSingleMaxSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var context = SupportConditionHandlerFactory.FactoryContexts[0];
                Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);

                var expression =
                    "@name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[2]> b=SupportBean_B]";
                env.CompileDeploy(expression).AddListener("s0");
                RunAssertionSingleMaxSimple(env);
                env.UndeployAll();

                // test SODA
                env.EplToModelCompileDeploy(expression).AddListener("s0");
                RunAssertionSingleMaxSimple(env);
                env.UndeployAll();

                // test variable
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int myvar=3", path);
                expression =
                    "@name('s0') select a.Id as a, b.Id as b from pattern [every a=SupportBean_A -[myvar-1]> b=SupportBean_B]";
                env.CompileDeploy(expression, path).AddListener("s0");
                RunAssertionSingleMaxSimple(env);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private static void RunAssertionSingleMaxSimple(RegressionEnvironment env)
        {
            var fields = new string[] { "a", "b" };

            env.SendEventBean(new SupportBean_A("A1"));
            env.SendEventBean(new SupportBean_A("A2"));

            SupportConditionHandlerFactory.LastHandler.Contexts.Clear();
            env.SendEventBean(new SupportBean_A("A3"));
            AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 2);

            env.SendEventBean(new SupportBean_B("B1"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "A1", "B1" }, new object[] { "A2", "B1" } });

            env.SendEventBean(new SupportBean_A("A4"));
            env.SendEventBean(new SupportBean_B("B2"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "A4", "B2" } });
            Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            for (var i = 5; i < 9; i++) {
                env.SendEventBean(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContext(env, SupportConditionHandlerFactory.LastHandler.Contexts, 2);
                }
            }

            env.SendEventBean(new SupportBean_B("B3"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "A5", "B3" }, new object[] { "A6", "B3" } });

            env.SendEventBean(new SupportBean_B("B4"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_A("A20"));
            env.SendEventBean(new SupportBean_A("A21"));
            env.SendEventBean(new SupportBean_B("B5"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "A20", "B5" }, new object[] { "A21", "B5" } });
            Assert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());
        }

        private static void AssertContext(
            RegressionEnvironment env,
            IList<ConditionHandlerContext> contexts,
            int max)
        {
            env.AssertThat(
                () => {
                    Assert.AreEqual(1, contexts.Count);
                    var context = contexts[0];
                    Assert.AreEqual("default", context.RuntimeURI);
                    Assert.AreEqual(env.Statement("s0").DeploymentId, context.DeploymentId);
                    Assert.AreEqual("s0", context.StatementName);
                    var condition = (ConditionPatternSubexpressionMax)context.EngineCondition;
                    Assert.AreEqual(max, condition.Max);
                    contexts.Clear();
                });
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(PatternOperatorFollowedByMax));
    }
} // end of namespace