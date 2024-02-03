///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class ViewLastEvent
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithMarketData(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMarketData(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLastEventMarketData());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLastEventSceneOne());
            return execs;
        }

        public class ViewLastEventSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();

                var epl =
                    "@name('s0') select irstream TheString as c0, IntPrimitive as c1 from SupportBean#lastevent()";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                SendSupportBean(env, "E2", 2);
                env.AssertPropsIRPair("s0", fields, new object[] { "E2", 2 }, new object[] { "E1", 1 });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E2", 2 } });
                SendSupportBean(env, "E3", 3);
                env.AssertPropsIRPair("s0", fields, new object[] { "E3", 3 }, new object[] { "E2", 2 });

                env.Milestone(4);
                env.Milestone(5);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E3", 3 } });
                SendSupportBean(env, "E4", 4);
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 4 }, new object[] { "E3", 3 });

                env.UndeployAll();
            }
        }

        public class ViewLastEventMarketData : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from  SupportMarketDataBean#lastevent()";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "Symbol", "E1" } }, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "E2" } },
                    new object[][] { new object[] { "Symbol", "E1" } });

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Symbol" },
                    new object[][] { new object[] { "E2" } });

                env.Milestone(2);

                for (var i = 3; i < 10; i++) {
                    env.SendEventBean(MakeMarketDataEvent($"E{i}"));
                    env.AssertPropsNV(
                        "s0",
                        new object[][] { new object[] { "Symbol", $"E{i}" } }, // new data
                        new object[][] { new object[] { "Symbol", $"E{(i - 1)}" } } //  old data
                    );

                    env.Milestone(i);
                }

                env.UndeployAll();
            }
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(@string, intPrimitive));
        }
    }
} // end of namespace