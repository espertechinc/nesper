///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.pattern.PatternOperatorFollowedByMax4Prevent;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedByMax2Prevent : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var handler = SupportConditionHandlerFactory.LastHandler;

            var expression =
                "@Name('A') select a.id as a, b.id as b from pattern [every a=SupportBean_A => b=SupportBean_B]";
            env.CompileDeploy(expression).AddListener("A");

            env.SendEventBean(new SupportBean_A("A1"));

            handler = SupportConditionHandlerFactory.LastHandler;

            env.SendEventBean(new SupportBean_A("A2"));

            handler.Contexts.Clear();
            env.SendEventBean(new SupportBean_A("A3"));
            AssertContextEnginePool(env, env.Statement("A"), handler.Contexts, 2, GetExpectedCountMap("A", 2));

            string[] fields = {"a", "b"};
            env.SendEventBean(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("A").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"A1", "B1"}, new object[] {"A2", "B1"}
                });

            env.SendEventBean(new SupportBean_A("A4"));
            env.SendEventBean(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("A").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A4", "B2"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());

            for (var i = 5; i < 9; i++) {
                env.SendEventBean(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContextEnginePool(env, env.Statement("A"), handler.Contexts, 2, GetExpectedCountMap("A", 2));
                }
            }

            env.SendEventBean(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("A").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A5", "B3"}, new object[] {"A6", "B3"}});

            env.SendEventBean(new SupportBean_B("B4"));
            Assert.IsFalse(env.Listener("A").IsInvoked);

            env.SendEventBean(new SupportBean_A("A20"));
            env.SendEventBean(new SupportBean_A("A21"));
            env.SendEventBean(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("A").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A20", "B5"}, new object[] {"A21", "B5"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.UndeployAll();
        }
    }
} // end of namespace