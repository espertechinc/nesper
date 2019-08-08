///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMostLeastFrequent
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumMostLeastEvents());
            execs.Add(new ExprEnumScalar());
            return execs;
        }

        internal class ExprEnumMostLeastEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Contained.mostFrequent(x -> P00) as val0," +
                                  "Contained.leastFrequent(x -> P00) as val1 " +
                                  "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                var bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12");
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12, 11});

                bean = SupportBean_ST0_Container.Make2Value("E1,12");
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12, 12});

                bean = SupportBean_ST0_Container.Make2Value(
                    "E1,12",
                    "E2,11",
                    "E2,2",
                    "E3,12",
                    "E1,12",
                    "E2,11",
                    "E3,11");
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12, 2});

                bean = SupportBean_ST0_Container.Make2Value(
                    "E2,11",
                    "E1,12",
                    "E2,15",
                    "E3,12",
                    "E1,12",
                    "E2,11",
                    "E3,11");
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 15});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.mostFrequent() as val0, " +
                                  "Strvals.leastFrequent() as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(string), typeof(string)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", "E4"});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1"});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();

                var eplLambda = "@Name('s0') select " +
                                "Strvals.mostFrequent(v -> extractNum(v)) as val0, " +
                                "Strvals.leastFrequent(v -> extractNum(v)) as val1 " +
                                "from SupportCollection";
                env.CompileDeploy(eplLambda).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 4});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 1});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }
    }
} // end of namespace