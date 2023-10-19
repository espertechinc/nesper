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

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumNested
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEquivalentToMinByUncorrelated(execs);
            WithMinByWhere(execs);
            WithCorrelated(execs);
            WithAnyOf(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAnyOf(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAnyOf());
            return execs;
        }

        public static IList<RegressionExecution> WithCorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithMinByWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinByWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithEquivalentToMinByUncorrelated(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumEquivalentToMinByUncorrelated());
            return execs;
        }

        private class ExprEnumEquivalentToMinByUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select contained.where(x => (x.p00 = contained.min(y => y.p00))) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,2");
                env.SendEventBean(bean);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var result = (ICollection<SupportBean_ST0>)@event.Get("val");
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { bean.Contained[1] }, result.ToArray());
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumMinByWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select sales.where(x => x.buyer = persons.minBy(y => age)) as val from PersonSales";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = PersonSales.Make();
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var sales = (ICollection<Sale>)@event.Get("val");
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { bean.Sales[0] }, sales.ToArray());
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select contained.where(x => x = (contained.firstOf(y => y.p00 = x.p00 ))) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,3");
                env.SendEventBean(bean);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var result = (ICollection<SupportBean_ST0>)@event.Get("val");
                        Assert.AreEqual(3, result.Count); // this would be 1 if the cache is invalid
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumAnyOf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try "in" with "Set<String> multivalues"
                env.CompileDeploy(
                        "@name('s0') select * from SupportContainerLevelEvent(level1s.anyOf(x=>x.level2s.anyOf(y => 'A' in (y.multivalues))))")
                    .AddListener("s0");
                TryAssertionAnyOf(env);
                env.UndeployAll();

                // try "in" with "String singlevalue"
                env.CompileDeploy(
                        "@name('s0') select * from SupportContainerLevelEvent(level1s.anyOf(x=>x.level2s.anyOf(y => y.singlevalue = 'A')))")
                    .AddListener("s0");
                TryAssertionAnyOf(env);
                env.UndeployAll();
            }
        }

        private static void TryAssertionAnyOf(RegressionEnvironment env)
        {
            env.SendEventBean(MakeContainerEvent("A"));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(MakeContainerEvent("B"));
            env.AssertListenerNotInvoked("s0");
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
    }
} // end of namespace