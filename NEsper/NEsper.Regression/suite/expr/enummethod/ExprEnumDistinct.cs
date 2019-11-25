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
    public class ExprEnumDistinct
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumDistinctEvents());
            execs.Add(new ExprEnumDistinctScalar());
            return execs;
        }

        internal class ExprEnumDistinctEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0" };
                var eplFragment = "@Name('s0') select " +
                                  "Contained.distinctOf(x -> P00) as val0 " +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<EventBean>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E3,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, null);
                }

                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, "");
                }

                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumDistinctScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0", "val1" };
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.distinctOf() as val0, " +
                                  "Strvals.distinctOf(v -> extractNum(v)) as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E2,E2"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E2", "E1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E2", "E1");
                env.Listener("s0").Reset();

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(env, fields);
                env.UndeployAll();
            }
        }
    }
} // end of namespace