///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewFirstUnique
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
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

                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select irstream theString as c0, intPrimitive as c1 from SupportBean#firstunique(theString)";
                if (optionalAnnotations != null) {
                    epl = optionalAnnotations + epl;
                }

                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                SendSupportBean(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E1", 2);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E2", 21);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);
                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E2", 22);
                SendSupportBean(env, "E1", 3);
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, "E3", 30);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 30 });

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
                    "@name('s0') select irstream symbol, price from SupportMarketDataBean#firstunique(symbol) order by symbol";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 100));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "S1" }, new object[] { "price", 100.0 } },
                    null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 5));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "S2" }, new object[] { "price", 5.0 } },
                    null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", 101));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 102));
                env.AssertListenerNotInvoked("s0");

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "price" },
                    new object[][] { new object[] { 100.0 }, new object[] { 5.0 } });

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S3", 6));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "S3" }, new object[] { "price", 6.0 } },
                    null);

                env.UndeployAll();
            }
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
    }
} // end of namespace