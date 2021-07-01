///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewFirstUnique
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            Withimple(execs);
            WithceneOne(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstUniqueSceneOne(null));
            return execs;
        }

        public static IList<RegressionExecution> Withimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstUniqueSimple(null));
            return execs;
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, 0L, "");
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        public class ViewFirstUniqueSimple : RegressionExecution
        {
            private readonly string optionalAnnotations;

            public ViewFirstUniqueSimple(string optionalAnnotations)
            {
                this.optionalAnnotations = optionalAnnotations;
            }

            public void Run(RegressionEnvironment env)
            {
                env.Milestone(0);

                var fields = new[] {"c0", "c1"};
                var epl =
                    "@Name('s0') select irstream TheString as c0, IntPrimitive as c1 from SupportBean#firstunique(TheString)";
                if (optionalAnnotations != null) {
                    epl = optionalAnnotations + epl;
                }

                env.CompileDeployAddListenerMile(epl, "s0", 1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                SendSupportBean(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});
                SendSupportBean(env, "E1", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});
                SendSupportBean(env, "E2", 21);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);
                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});
                SendSupportBean(env, "E2", 22);
                SendSupportBean(env, "E1", 3);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "E3", 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30});

                env.UndeployAll();
            }
        }

        public class ViewFirstUniqueSceneOne : RegressionExecution
        {
            private readonly string optionalAnnotation;

            public ViewFirstUniqueSceneOne(string optionalAnnotation)
            {
                this.optionalAnnotation = optionalAnnotation;
            }

            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select irstream Symbol, Price from SupportMarketDataBean#firstunique(Symbol) order by Symbol";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 100));
                env.Listener("s0")
                    .AssertNewOldData(new[] {new object[] {"Symbol", "S1"}, new object[] {"Price", 100.0}}, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 5));
                env.Listener("s0")
                    .AssertNewOldData(new[] {new object[] {"Symbol", "S2"}, new object[] {"Price", 5.0}}, null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", 101));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 102));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Price"},
                    new[] {new object[] {100.0}, new object[] {5.0}});

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S3", 6));
                env.Listener("s0")
                    .AssertNewOldData(new[] {new object[] {"Symbol", "S3"}, new object[] {"Price", 6.0}}, null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace