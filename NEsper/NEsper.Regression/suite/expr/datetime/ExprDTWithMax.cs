///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithMax
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTWithMaxInput());
            executions.Add(new ExprDTWithMaxFields());
            return executions;
        }

        internal class ExprDTWithMaxInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2", "val3" };
                var eplFragment = "@name('s0') select " +
                                  "DateTime.withMax('month') as val0," +
                                  "DateTimeOffset.withMax('month') as val1," +
                                  "DateTimeEx.withMax('month') as val2," +
                                  "LongDate.withMax('month') as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(DateTime?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeEx),
                        typeof(long?)
                    });

                var startTime = "2002-05-30T09:00:00.000";
                var expectedTime = "2002-12-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "datetime", "dto", "dtx", "long"));

                env.UndeployAll();
            }
        }

        internal class ExprDTWithMaxFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3","val4","val5","val6" };
                var eplFragment = "@name('s0') select " +
                                  "DateTimeOffset.withMax('msec') as val0," +
                                  "DateTimeOffset.withMax('sec') as val1," +
                                  "DateTimeOffset.withMax('minutes') as val2," +
                                  "DateTimeOffset.withMax('hour') as val3," +
                                  "DateTimeOffset.withMax('day') as val4," +
                                  "DateTimeOffset.withMax('month') as val5," +
                                  "DateTimeOffset.withMax('year') as val6" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(DateTimeOffset?), // val0
                        typeof(DateTimeOffset?), // val1
                        typeof(DateTimeOffset?), // val2
                        typeof(DateTimeOffset?), // val3
                        typeof(DateTimeOffset?), // val4
                        typeof(DateTimeOffset?), // val5
                        typeof(DateTimeOffset?)  // val6
                    });

                string[] expected = {
                    "2002-05-30T09:00:00.999", // val0
                    "2002-05-30T09:00:59.000", // val1
                    "2002-05-30T09:59:00.000", // val2
                    "2002-05-30T23:00:00.000", // val3
                    "2002-05-31T09:00:00.000", // val4
                    "2002-12-30T09:00:00.000", // val5
                    "9999-05-30T09:00:00.000"  // val6
                };
                var startTime = "2002-05-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "dto"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace