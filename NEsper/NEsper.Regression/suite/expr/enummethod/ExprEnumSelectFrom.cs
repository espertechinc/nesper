///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumSelectFrom
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumNew());
            execs.Add(new ExprEnumSelect());
            return execs;
        }

        private static IDictionary<string, object>[] ToMapArray(object result)
        {
            if (result == null) {
                return null;
            }

            return ((ICollection<IDictionary<string, object>>) result).ToArray();
        }

        internal class ExprEnumNew : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@Name('s0') select " +
                                  "Contained.selectFrom(x -> new {c0 = Id||'x', c1 = Key0||'y'}) as val0 " +
                                  "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("E1,12,0", "E2,11,0", "E3,2,0"));
                EPAssertionUtil.AssertPropsPerRow(
                    ToMapArray(env.Listener("s0").AssertOneGetNewAndReset().Get("val0")),
                    new [] { "c0", "c1" },
                    new[] {new object[] {"E1x", "12y"}, new object[] {"E2x", "11y"}, new object[] {"E3x", "2y"}});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("E4,0,1"));
                EPAssertionUtil.AssertPropsPerRow(
                    ToMapArray(env.Listener("s0").AssertOneGetNewAndReset().Get("val0")),
                    new [] { "c0", "c1" },
                    new[] {new object[] {"E4x", "0y"}});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value(null));
                EPAssertionUtil.AssertPropsPerRow(
                    ToMapArray(env.Listener("s0").AssertOneGetNewAndReset().Get("val0")),
                    new [] { "c0", "c1" },
                    null);

                env.SendEventBean(SupportBean_ST0_Container.Make3Value());
                EPAssertionUtil.AssertPropsPerRow(
                    ToMapArray(env.Listener("s0").AssertOneGetNewAndReset().Get("val0")),
                    new [] { "c0", "c1" },
                    new object[0][]);

                env.UndeployAll();
            }
        }

        internal class ExprEnumSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@Name('s0') select " +
                                  "Contained.selectFrom(x -> Id) as val0 " +
                                  "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E3,2"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2", "E3");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0");
                env.Listener("s0").Reset();
                env.UndeployAll();

                // test scalar-coll with lambda
                var fields = new [] { "val0" };
                var eplLambda = "@Name('s0') select " +
                                "Strvals.selectFrom(v -> extractNum(v)) as val0 " +
                                "from SupportCollection";
                env.CompileDeploy(eplLambda).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", 2, 1, 5, 4);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString("E1"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", 1);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString(null));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString(""));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0");

                env.UndeployAll();
            }
        }
    }
} // end of namespace