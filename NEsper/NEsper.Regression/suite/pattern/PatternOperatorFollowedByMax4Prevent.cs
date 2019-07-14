///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedByMax4Prevent : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            var handler = SupportConditionHandlerFactory.LastHandler;

            RunAssertionFollowedWithMax(env, handler);
            RunAssertionTwoStatementsAndStopDestroy(env, handler);
        }

        private static void RunAssertionFollowedWithMax(
            RegressionEnvironment env,
            SupportConditionHandlerFactory.SupportConditionHandler handler)
        {
            var expressionOne =
                "@Name('S1') select * from pattern [every a=SupportBean(theString like 'A%') -[2]> b=SupportBean_A(id=a.TheString)]";
            env.CompileDeploy(expressionOne).AddListener("S1");

            var expressionTwo =
                "@Name('S2') select * from pattern [every a=SupportBean(theString like 'B%') => b=SupportBean_B(id=a.TheString)]";
            env.CompileDeploy(expressionTwo).AddListener("S2");

            env.SendEventBean(new SupportBean("A1", 0));
            env.SendEventBean(new SupportBean("A2", 0));
            env.SendEventBean(new SupportBean("B1", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("A3", 0));
            AssertContextStatement(env, env.Statement("S1"), handler.GetAndResetContexts(), 2);

            env.SendEventBean(new SupportBean("B2", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("B3", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 2, "S2", 2));

            env.SendEventBean(new SupportBean_A("A2"));
            env.SendEventBean(new SupportBean("B4", 0)); // now A1, B1, B2, B4
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("A3", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 1, "S2", 3));

            env.UndeployModuleContaining("S1");

            env.SendEventBean(new SupportBean("B4", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("B5", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S2", 4));

            env.UndeployAll();
        }

        private static void RunAssertionTwoStatementsAndStopDestroy(
            RegressionEnvironment env,
            SupportConditionHandlerFactory.SupportConditionHandler handler)
        {
            var expressionOne =
                "@Name('S1') select * from pattern [every a=SupportBean(theString like 'A%') => b=SupportBean_A(id=a.TheString)]";
            env.CompileDeploy(expressionOne).AddListener("S1");

            var expressionTwo =
                "@Name('S2') select * from pattern [every a=SupportBean(theString like 'B%') => b=SupportBean_B(id=a.TheString)]";
            env.CompileDeploy(expressionTwo).AddListener("S2");

            env.SendEventBean(new SupportBean("A1", 0));
            env.SendEventBean(new SupportBean("A2", 0));
            env.SendEventBean(new SupportBean("A3", 0));
            env.SendEventBean(new SupportBean("B1", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("B2", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 3, "S2", 1));

            handler = SupportConditionHandlerFactory.LastHandler;

            env.SendEventBean(new SupportBean("A4", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 3, "S2", 1));

            env.UndeployModuleContaining("S1");

            env.SendEventBean(new SupportBean("B3", 0));
            env.SendEventBean(new SupportBean("B4", 0));
            env.SendEventBean(new SupportBean("B5", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            handler = SupportConditionHandlerFactory.LastHandler;

            env.SendEventBean(new SupportBean("B6", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S2", 4));

            env.SendEventBean(new SupportBean("B7", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S2", 4));

            env.UndeployAll();
        }

        internal static IDictionary<string, long> GetExpectedCountMap(
            string statementName,
            long count)
        {
            IDictionary<string, long> result = new Dictionary<string, long>();
            result.Put(statementName, count);
            return result;
        }

        internal static IDictionary<string, long> GetExpectedCountMap(
            string stmtOne,
            long countOne,
            string stmtTwo,
            long countTwo)
        {
            IDictionary<string, long> result = new Dictionary<string, long>();
            result.Put(stmtOne, countOne);
            result.Put(stmtTwo, countTwo);
            return result;
        }

        internal static void AssertContextEnginePool(
            RegressionEnvironment env,
            EPStatement stmt,
            IList<ConditionHandlerContext> contexts,
            int max,
            IDictionary<string, long> counts)
        {
            Assert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            Assert.AreEqual(stmt.DeploymentId, context.DeploymentId);
            Assert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionPatternRuntimeSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            Assert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
            }

            contexts.Clear();
        }

        internal static void AssertContextStatement(
            RegressionEnvironment env,
            EPStatement stmt,
            IList<ConditionHandlerContext> contexts,
            int max)
        {
            Assert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            Assert.AreEqual(stmt.DeploymentId, context.DeploymentId);
            Assert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionPatternSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            contexts.Clear();
        }
    }
} // end of namespace