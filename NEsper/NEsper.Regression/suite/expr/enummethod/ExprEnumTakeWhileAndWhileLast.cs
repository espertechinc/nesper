///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumTakeWhileAndWhileLast
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumTakeWhileEvents());
            execs.Add(new ExprEnumTakeWhileScalar());
            return execs;
        }

        internal class ExprEnumTakeWhileEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3" };
                var epl = "@Name('s0') select " +
                          "Contained.takeWhile(x -> x.P00 > 0) as val0," +
                          "Contained.takeWhile( (x, i) -> x.P00 > 0 and i<2) as val1," +
                          "Contained.takeWhileLast(x -> x.P00 > 0) as val2," +
                          "Contained.takeWhileLast( (x, i) -> x.P00 > 0 and i<2) as val3" +
                          " from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<EventBean>), 
                        typeof(ICollection<EventBean>),
                        typeof(ICollection<EventBean>),
                        typeof(ICollection<EventBean>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2,E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E2,E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E2,E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,0", "E2,2", "E3,3"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E2,E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E2,E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,0", "E3,3"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,0"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E1");
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

        internal class ExprEnumTakeWhileScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3" };
                var epl = "@Name('s0') select " +
                          "Strvals.takeWhile(x -> x != 'E1') as val0," +
                          "Strvals.takeWhile( (x, i) -> x != 'E1' and i<2) as val1," +
                          "Strvals.takeWhileLast(x -> x != 'E1') as val2," +
                          "Strvals.takeWhileLast( (x, i) -> x != 'E1' and i<2) as val3" +
                          " from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E2", "E3", "E4");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val3", "E3", "E4");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace