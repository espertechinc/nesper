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
    public class ExprEnumWhere
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumWhereEvents());
            execs.Add(new ExprEnumWhereString());
            return execs;
        }

        internal class ExprEnumWhereEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "Contained.where(x -> P00 = 9) as val0," +
                          "Contained.where((x, i) -> x.P00 = 9 and i >= 1) as val1 from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0", "val1" },
                    new[] {
                        typeof(ICollection<EventBean>), 
                        typeof(ICollection<EventBean>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,9", "E2,1", "E3,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,9"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E3");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", null);
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumWhereString : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0", "val1" };
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.where(x -> x not like '%1%') as val0, " +
                                  "Strvals.where((x, i) -> x not like '%1%' and i > 1) as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E2", "E3");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString("E4,E2,E1"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E4", "E2");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString(""));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1");
                env.Listener("s0").Reset();

                env.UndeployAll();

                // test boolean
                eplFragment = "@Name('s0') select " +
                              "Boolvals.where(x -> x) as val0 " +
                              "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeBoolean("true,true,false"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", true, true);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace