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
    public class ViewFirstLength
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
            execs.Add(new ViewFirstLengthMarketData());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewFirstLengthSceneOne());
            return execs;
        }

        public class ViewFirstLengthSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Milestone(0);

                var fields = "c0".SplitCsv();
                var epl = "@name('s0') select irstream theString as c0 from SupportBean#firstlength(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                SendSupportBean(env, "E3");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                SendSupportBean(env, "E4");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        public class ViewFirstLengthMarketData : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from  SupportMarketDataBean#firstlength(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "E1" } }, null);
                env.AssertPropsPerRowIterator("s0", "symbol".SplitCsv(), new object[][] { new object[] { "E1" } });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "E2" } }, null);
                env.AssertPropsPerRowIterator(
                    "s0",
                    "symbol".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "E3" } }, null);
                env.AssertPropsPerRowIterator(
                    "s0",
                    "symbol".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4"));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    "symbol".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.UndeployAll();
            }
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string)
        {
            env.SendEventBean(new SupportBean(@string, 0));
        }
    }
} // end of namespace