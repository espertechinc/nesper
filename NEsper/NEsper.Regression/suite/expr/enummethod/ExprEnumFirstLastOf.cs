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
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumFirstLastOf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumFirstLastScalar());
            execs.Add(new ExprEnumFirstLastProperty());
            execs.Add(new ExprEnumFirstLastNoPred());
            execs.Add(new ExprEnumFirstLastPredicate());
            return execs;
        }

        private static void AssertId(
            SupportListener listener,
            string property,
            string id)
        {
            var result = (SupportBean_ST0) listener.AssertOneGetNew().Get(property);
            Assert.AreEqual(id, result.Id);
        }

        internal class ExprEnumFirstLastScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3" };
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.firstOf() as val0, " +
                                  "Strvals.lastOf() as val1, " +
                                  "Strvals.firstOf(x -> x like '%1%') as val2, " +
                                  "Strvals.lastOf(x -> x like '%1%') as val3 " +
                                  " from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(string), typeof(string), typeof(string), typeof(string)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", "E1", "E1"});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", "E1", "E1"});

                env.SendEventBean(SupportCollection.MakeString("E2,E3,E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E4", null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumFirstLastProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0", "val1" };
                var eplFragment = "@Name('s0') select " +
                                  "Contained.firstOf().P00 as val0, " +
                                  "Contained.lastOf().P00 as val1 " +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 3});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 1});

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

        internal class ExprEnumFirstLastNoPred : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@Name('s0') select " +
                                  "Contained.firstOf() as val0, " +
                                  "Contained.lastOf() as val1 " +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0", "val1" },
                    new[] {typeof(SupportBean_ST0), typeof(SupportBean_ST0)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E3,9", "E2,9"));
                AssertId(env.Listener("s0"), "val0", "E1");
                AssertId(env.Listener("s0"), "val1", "E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E2,2"));
                AssertId(env.Listener("s0"), "val0", "E2");
                AssertId(env.Listener("s0"), "val1", "E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNew().Get("val0"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val1"));

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                Assert.IsNull(env.Listener("s0").AssertOneGetNew().Get("val0"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val1"));

                env.UndeployAll();
            }
        }

        internal class ExprEnumFirstLastPredicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Contained.firstOf(x -> P00 = 9) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val" },
                    new[] {typeof(SupportBean_ST0)});

                var bean = SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9");
                env.SendEventBean(bean);
                var result = (SupportBean_ST0) env.Listener("s0").AssertOneGetNewAndReset().Get("val");
                Assert.AreSame(result, bean.Contained[1]);

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E2,1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace