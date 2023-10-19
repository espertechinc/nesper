///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;

using static
    com.espertech.esper.regressionlib.suite.pattern.PatternOperatorFollowedByMax4Prevent; // assertContextEnginePool

// getExpectedCountMap

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedByMax2Noprevent : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var expression =
                "@name('A') select a.id as a, b.id as b from pattern [every a=SupportBean_A -> b=SupportBean_B]";
            env.CompileDeploy(expression).AddListener("A");

            env.SendEventBean(new SupportBean_A("A1"));
            env.SendEventBean(new SupportBean_A("A2"));

            env.Milestone(0);

            SupportConditionHandlerFactory.LastHandler.Contexts.Clear();
            env.SendEventBean(new SupportBean_A("A3"));
            AssertContextEnginePool(
                env,
                env.Statement("A"),
                SupportConditionHandlerFactory.LastHandler.Contexts,
                2,
                GetExpectedCountMap("A", 2));

            env.Milestone(1);

            SupportConditionHandlerFactory.LastHandler.Contexts.Clear();
            env.SendEventBean(new SupportBean_A("A4"));
            AssertContextEnginePool(
                env,
                env.Statement("A"),
                SupportConditionHandlerFactory.LastHandler.Contexts,
                2,
                GetExpectedCountMap("A", 3));

            var fields = new string[] { "a", "b" };
            env.SendEventBean(new SupportBean_B("B1"));
            env.AssertPropsPerRowLastNew(
                "A",
                fields,
                new object[][] {
                    new object[] { "A1", "B1" }, new object[] { "A2", "B1" }, new object[] { "A3", "B1" },
                    new object[] { "A4", "B1" }
                });

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.STATICHOOK);
        }
    }
} // end of namespace