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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.sales;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumNested
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            #if false
            execs.Add(new ExprEnumEquivalentToMinByUncorrelated());
            execs.Add(new ExprEnumMinByWhere());
            execs.Add(new ExprEnumCorrelated());
            #endif
            execs.Add(new ExprEnumAnyOf());
            return execs;
        }

        private static void TryAssertionAnyOf(RegressionEnvironment env)
        {
            env.SendEventBean(MakeContainerEvent("A"));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(MakeContainerEvent("B"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
        }

        private static SupportContainerLevelEvent MakeContainerEvent(string value)
        {
            ISet<SupportContainerLevel1Event> level1s = new LinkedHashSet<SupportContainerLevel1Event>();
            level1s.Add(
                new SupportContainerLevel1Event(
                    Collections.SingletonSet(new SupportContainerLevel2Event(Collections.SingletonSet("X1"), "X1"))));
            level1s.Add(
                new SupportContainerLevel1Event(
                    Collections.SingletonSet(new SupportContainerLevel2Event(Collections.SingletonSet(value), value))));
            level1s.Add(
                new SupportContainerLevel1Event(
                    Collections.SingletonSet(new SupportContainerLevel2Event(Collections.SingletonSet("X2"), "X2"))));
            return new SupportContainerLevelEvent(level1s);
        }

        internal class ExprEnumEquivalentToMinByUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Contained.where(x => (x.P00 = Contained.min(y -> y.P00))) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,2");
                env.SendEventBean(bean);
                var result = env.Listener("s0").AssertOneGetNewAndReset().Get("val").UnwrapIntoArray<SupportBean_ST0>();
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {bean.Contained[1]}, result);

                env.UndeployAll();
            }
        }

        internal class ExprEnumMinByWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Sales.where(x => x.Buyer = Persons.minBy(y -> Age)) as val from PersonSales";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = PersonSales.Make();
                env.SendEventBean(bean);

                var sales = env.Listener("s0")
                    .AssertOneGetNewAndReset()
                    .Get("val")
                    .UnwrapIntoArray<Sale>();
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] { bean.Sales[0] }, sales);

                env.UndeployAll();
            }
        }

        internal class ExprEnumCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Contained.where(x => x = (Contained.firstOf(y -> y.P00 = x.P00 ))) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,3");
                env.SendEventBean(bean);
                var result = env.Listener("s0").AssertOneGetNewAndReset().Get("val").Unwrap<SupportBean_ST0>();
                Assert.AreEqual(3, result.Count); // this would be 1 if the cache is invalid

                env.UndeployAll();
            }
        }

        internal class ExprEnumAnyOf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try "in" with "Set<String> multivalues"
                env.CompileDeploy(
                        "@Name('s0') select * from SupportContainerLevelEvent(Level1s.anyOf(x -> x.Level2s.anyOf(y -> 'A' in (y.Multivalues))))")
                    .AddListener("s0");
                TryAssertionAnyOf(env);
                env.UndeployAll();

                // try "in" with "String singlevalue"
                env.CompileDeploy(
                        "@Name('s0') select * from SupportContainerLevelEvent(Level1s.anyOf(x -> x.Level2s.anyOf(y -> y.Singlevalue = 'A')))")
                    .AddListener("s0");
                TryAssertionAnyOf(env);
                env.UndeployAll();
            }
        }
    }
} // end of namespace