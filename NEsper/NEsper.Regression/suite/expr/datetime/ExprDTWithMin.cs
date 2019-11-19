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
    public class ExprDTWithMin
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTWithMinInput());
            executions.Add(new ExprDTWithMinFields());
            return executions;
        }

        internal class ExprDTWithMinInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2", "val3" };
                var eplFragment = "@Name('s0') select " +
                                  "LongDate.withMin('month') as val0," +
                                  "DateTime.withMin('month') as val1," + 
                                  "DateTimeOffset.withMin('month') as val2," +
                                  "DateTimeEx.withMin('month') as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(long?),
                        typeof(DateTime?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeEx),
                    });

                var startTime = "2002-05-30T09:00:00.000";
                var expectedTime = "2002-01-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "long", "date", "dto", "dtx"));

                env.UndeployAll();
            }
        }

        internal class ExprDTWithMinFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3","val4","val5","val6","val7" };
                var eplFragment = "@Name('s0') select " +
                                  "DateTimeOffset.withMin('msec') as val0," +
                                  "DateTimeOffset.withMin('sec') as val1," +
                                  "DateTimeOffset.withMin('minutes') as val2," +
                                  "DateTimeOffset.withMin('hour') as val3," +
                                  "DateTimeOffset.withMin('day') as val4," +
                                  "DateTimeOffset.withMin('month') as val5," +
                                  "DateTimeOffset.withMin('year') as val6," +
                                  "DateTimeOffset.withMin('week') as val7" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeOffset?)
                    });

                string[] expected = {
                    "2002-05-30T09:01:02.000",
                    "2002-05-30T09:01:00.003",
                    "2002-05-30T09:00:02.003",
                    "2002-05-30T00:01:02.003",
                    "2002-05-01T09:01:02.003",
                    "2002-01-30T09:01:02.003",
                    "0001-05-30T09:01:02.003",
                    "2002-01-03T09:01:02.003"
                };
                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                //System.out.println("==-> " + SupportDateTime.print(env.Listener("s0").assertOneGetNew().Get("val7")));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "util"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace