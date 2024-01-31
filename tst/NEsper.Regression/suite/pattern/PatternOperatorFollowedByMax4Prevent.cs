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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedByMax4Prevent : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.STATICHOOK);
        }

        public void Run(RegressionEnvironment env)
        {
            var context = SupportConditionHandlerFactory.FactoryContexts[0];
            ClassicAssert.AreEqual(env.RuntimeURI, context.RuntimeURI);

            var milestone = new AtomicLong();
            RunAssertionFollowedWithMax(env, milestone);
            RunAssertionTwoStatementsAndStopDestroy(env, milestone);
        }

        private static void RunAssertionFollowedWithMax(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var expressionOne =
                "@name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -[2]> b=SupportBean_A(Id=a.TheString)]";
            env.CompileDeploy(expressionOne).AddListener("S1");

            var expressionTwo =
                "@name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(Id=a.TheString)]";
            env.CompileDeploy(expressionTwo).AddListener("S2");

            env.SendEventBean(new SupportBean("A1", 0));
            env.SendEventBean(new SupportBean("A2", 0));
            env.SendEventBean(new SupportBean("B1", 0));
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("A3", 0));
            AssertContextStatement(
                env,
                env.Statement("S1"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                2);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B2", 0));
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B3", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 2, "S2", 2));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_A("A2"));
            env.SendEventBean(new SupportBean("B4", 0)); // now A1, B1, B2, B4
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("A3", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 1, "S2", 3));

            env.UndeployModuleContaining("S1");

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B4", 0));
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("B5", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S2", 4));

            env.UndeployAll();
        }

        private static void RunAssertionTwoStatementsAndStopDestroy(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var expressionOne =
                "@name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean_A(Id=a.TheString)]";
            env.CompileDeploy(expressionOne).AddListener("S1");

            var expressionTwo =
                "@name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(Id=a.TheString)]";
            env.CompileDeploy(expressionTwo).AddListener("S2");

            env.SendEventBean(new SupportBean("A1", 0));
            env.SendEventBean(new SupportBean("A2", 0));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("A3", 0));
            env.SendEventBean(new SupportBean("B1", 0));
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B2", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 3, "S2", 1));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("A4", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S1", 3, "S2", 1));

            env.MilestoneInc(milestone);

            env.UndeployModuleContaining("S1");

            env.SendEventBean(new SupportBean("B3", 0));
            env.SendEventBean(new SupportBean("B4", 0));
            env.SendEventBean(new SupportBean("B5", 0));
            ClassicAssert.IsTrue(SupportConditionHandlerFactory.LastHandler.Contexts.IsEmpty());

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B6", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
                4,
                GetExpectedCountMap("S2", 4));

            env.SendEventBean(new SupportBean("B7", 0));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                SupportConditionHandlerFactory.LastHandler.GetAndResetContexts(),
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
            ClassicAssert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            ClassicAssert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            ClassicAssert.AreEqual(stmt.DeploymentId, context.DeploymentId);
            ClassicAssert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionPatternRuntimeSubexpressionMax)context.EngineCondition;
            ClassicAssert.AreEqual(max, condition.Max);
            ClassicAssert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                ClassicAssert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
            }

            contexts.Clear();
        }

        internal static void AssertContextStatement(
            RegressionEnvironment env,
            EPStatement stmt,
            IList<ConditionHandlerContext> contexts,
            int max)
        {
            ClassicAssert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            ClassicAssert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            ClassicAssert.AreEqual(stmt.DeploymentId, context.DeploymentId);
            ClassicAssert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionPatternSubexpressionMax)context.EngineCondition;
            ClassicAssert.AreEqual(max, condition.Max);
            contexts.Clear();
        }
    }
} // end of namespace