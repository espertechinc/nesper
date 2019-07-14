///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumCountOf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumCountOfEvents());
            execs.Add(new ExprEnumCountOfScalar());
            return execs;
        }

        internal class ExprEnumCountOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0", "val1"};
                var eplFragment = "@Name('s0') select " +
                                  "contained.countof(x=> x.p00 = 9) as val0, " +
                                  "contained.countof() as val1 " +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 3});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, 0});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,9"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 1});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, 1});

                env.UndeployAll();
            }
        }

        internal class ExprEnumCountOfScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0", "val1"};
                var eplFragment = "@Name('s0') select " +
                                  "strvals.countof() as val0, " +
                                  "strvals.countof(x => x = 'E1') as val1 " +
                                  " from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 1});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E1,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, 2});

                env.UndeployAll();
            }
        }
    }
} // end of namespace