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
    public class ExprEnumAllOfAnyOf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumAllOfAnyOfEvents());
            execs.Add(new ExprEnumAllOfAnyOfScalar());
            return execs;
        }

        internal class ExprEnumAllOfAnyOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Contained.allof(x => P00 = 12) as val0," +
                                  "Contained.anyof(x => P00 = 12) as val1 " +
                                  "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(bool?), typeof(bool?)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,12", "E2,2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12", "E2,12", "E2,12"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,0", "E2,0", "E2,0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                env.UndeployAll();
            }
        }

        internal class ExprEnumAllOfAnyOfScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.allof(x => x='E2') as val0," +
                                  "Strvals.anyof(x => x='E2') as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(bool?), typeof(bool?)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeString("E2,E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                env.UndeployAll();
            }
        }
    }
} // end of namespace