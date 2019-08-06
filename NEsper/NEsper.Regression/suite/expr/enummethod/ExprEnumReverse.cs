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
    public class ExprEnumReverse
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumReverseEvents());
            execs.Add(new ExprEnumReverseScalar());
            return execs;
        }

        internal class ExprEnumReverseEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Contained.reverse() as val from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "val".SplitCsv(),
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val", "E3,E2,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E2,9", "E1,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val", "E1,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val", "");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumReverseScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.reverse() as val0 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E4", "E5", "E1", "E2");
                env.Listener("s0").Reset();

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(env, fields);

                env.UndeployAll();
            }
        }
    }
} // end of namespace