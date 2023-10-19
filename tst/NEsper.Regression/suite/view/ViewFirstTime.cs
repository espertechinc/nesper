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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewFirstTime
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withimple(execs);
            WithceneOne(execs);
            WithceneTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstTimeSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstTimeSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> Withimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstTimeSimple());
            return execs;
        }

        public class ViewFirstTimeSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream theString as c0, intPrimitive as c1 from SupportBean#firsttime(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                env.AdvanceTime(2000);
                SendSupportBean(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });

                env.Milestone(3);

                env.AdvanceTime(9999);

                env.Milestone(4);

                env.AdvanceTime(10000);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E3", 30);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                SendSupportBean(env, "E4", 40);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });

                env.UndeployAll();
            }
        }

        public class ViewFirstTimeSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@name('s0') select irstream * from SupportMarketDataBean#firsttime(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "E1" } }, null);
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "symbol" },
                    new object[][] { new object[] { "E1" } });

                env.Milestone(1);

                env.AdvanceTime(600);
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "E2" } }, null);
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "symbol" },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(2);

                env.AdvanceTime(1500);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(1600);
                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.AdvanceTime(2000);
                env.SendEventBean(MakeMarketDataEvent("E4"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                env.UndeployAll();
            }
        }

        private class ViewFirstTimeSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                env.CompileDeployAddListenerMileZero("@name('s0') select * from SupportBean#firsttime(1 month)", "s0");

                SendCurrentTime(env, "2002-02-15T09:00:00.000");
                env.SendEventBean(new SupportBean("E1", 1));

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E2", 2));

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E3", 3));

                env.AssertPropsPerRowIterator(
                    "s0",
                    "theString".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
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