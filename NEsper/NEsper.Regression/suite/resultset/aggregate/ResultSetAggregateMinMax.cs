///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateMinMax
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinMaxNoDataWindowSubquery());
            execs.Add(new ResultSetAggregateMemoryMinHaving());
            execs.Add(new ResultSetAggregateMinMaxNamedWindowWEver(false));
            execs.Add(new ResultSetAggregateMinMaxNamedWindowWEver(true));
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            double price)
        {
            var bean = new SupportMarketDataBean("DELL", price, -1L, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetAggregateMinMaxNamedWindowWEver : RegressionExecution
        {
            private readonly bool soda;

            public ResultSetAggregateMinMaxNamedWindowWEver(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "lower","upper","lowerever","upperever" };
                var path = new RegressionPath();

                var epl = "create window NamedWindow5m#length(2) as select * from SupportBean;\n" +
                          "insert into NamedWindow5m select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                epl = "@name('s0') select " +
                      "min(IntPrimitive) as lower, " +
                      "max(IntPrimitive) as upper, " +
                      "minever(IntPrimitive) as lowerever, " +
                      "maxever(IntPrimitive) as upperever from NamedWindow5m";
                env.CompileDeploy(soda, epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean(null, 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 1, 1, 1});

                env.Milestone(0);

                env.SendEventBean(new SupportBean(null, 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 5, 1, 5});

                env.Milestone(1);

                env.SendEventBean(new SupportBean(null, 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 5, 1, 5});

                env.SendEventBean(new SupportBean(null, 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 6, 1, 6});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMinMaxNoDataWindowSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "maxi","mini","max0","min0" };
                var epl = "@name('s0') select max(IntPrimitive) as maxi, min(IntPrimitive) as mini," +
                          "(select max(Id) from SupportBean_S0#lastevent) as max0, (select min(Id) from SupportBean_S0#lastevent) as min0" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 3, null, null});

                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, 3, null, null});

                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean("E3", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, 3, 2, 2});

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean("E4", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, 3, 1, 1});

                env.UndeployAll();
                /// <summary>
                /// Comment out here for sending many more events.
                /// env.SendEventBean(new SupportBean(null, i));
                /// if (i % 10000 == 0) {
                /// System.out.println("Sent " + i + " events");
                /// }
                /// }
                /// </summary>
            }
        }

        internal class ResultSetAggregateMemoryMinHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select Price, min(Price) as minPrice " +
                                    "from SupportMarketDataBean#time(30)" +
                                    "having Price >= min(Price) * (1.02)";

                env.CompileDeploy(statementText).AddListener("s0");

                var random = new Random();
                // Change to perform a long-running tests, each loop is 1 second
                var loopcount = 2;
                var loopCount = 0;

                while (true) {
                    log.Info("Sending batch " + loopCount);

                    // send events
                    var startTime = PerformanceObserver.MilliTime;
                    for (var i = 0; i < 5000; i++) {
                        var price = 50 + 49 * random.Next(100) / 100.0;
                        SendEvent(env, price);
                    }

                    var endTime = PerformanceObserver.MilliTime;

                    // sleep remainder of 1 second
                    var delta = startTime - endTime;
                    if (delta < 950) {
                        try {
                            Thread.Sleep((int) (950 - delta));
                        }
                        catch (ThreadInterruptedException) {
                            break;
                        }
                    }

                    env.Listener("s0").Reset();
                    loopCount++;
                    if (loopCount > loopcount) {
                        break;
                    }
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace