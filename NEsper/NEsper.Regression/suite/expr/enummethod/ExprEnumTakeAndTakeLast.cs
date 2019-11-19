///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumTakeAndTakeLast
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumTakeEvents());
            execs.Add(new ExprEnumTakeScalar());
            return execs;
        }

        internal class ExprEnumTakeEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3","val4","val5" };
                var epl = "@Name('s0') select " +
                          "Contained.take(2) as val0," +
                          "Contained.take(1) as val1," +
                          "Contained.take(0) as val2," +
                          "Contained.take(-1) as val3," +
                          "Contained.takeLast(2) as val4," +
                          "Contained.takeLast(1) as val5" +
                          " from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>),
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val4", "E2,E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val5", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val4", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val5", "E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val4", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val5", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, "");
                }

                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, null);
                }

                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumTakeScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3" };
                var epl = "@Name('s0') select " +
                          "Strvals.take(2) as val0," +
                          "Strvals.take(1) as val1," +
                          "Strvals.takeLast(2) as val2," +
                          "Strvals.takeLast(1) as val3" +
                          " from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E2", "E3");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val3", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString("E1,E2"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E1", "E2");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val3", "E2");
                env.Listener("s0").Reset();

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(env, fields);

                env.UndeployAll();
            }
        }
    }
} // end of namespace