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
    public class ExprDTSet
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTSetInput());
            executions.Add(new ExprDTSetFields());
            return executions;
        }

        internal class ExprDTSetInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {
                    "val0",
                    "val1",
                    "val2",
                    "val3",
                    "val4"
                };
                var eplFragment = "@Name('s0') select " +
                                  "DtoDate.set('month', 0) as val0," +
                                  "LongDate.set('month', 0) as val1," +
                                  "DtxDate.set('month', 0) as val2" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(DateTimeOffset?),
                        typeof(long?),
                        typeof(DateTimeEx)
                    });

                var startTime = "2002-05-30T09:00:00.000";
                var expectedTime = "2002-01-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "util", "long", "dtx"));

                env.UndeployAll();
            }
        }

        internal class ExprDTSetFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3","val4","val5","val6","val7" };
                var eplFragment = "@Name('s0') select " +
                                  "DtoDate.set('msec', 1) as val0," +
                                  "DtoDate.set('sec', 2) as val1," +
                                  "DtoDate.set('minutes', 3) as val2," +
                                  "DtoDate.set('hour', 13) as val3," +
                                  "DtoDate.set('day', 5) as val4," +
                                  "DtoDate.set('month', 6) as val5," +
                                  "DtoDate.set('year', 7) as val6," +
                                  "DtoDate.set('week', 8) as val7" +
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
                    "2002-05-30T09:00:00.001",
                    "2002-05-30T09:00:02.000",
                    "2002-05-30T09:03:00.000",
                    "2002-05-30T13:00:00.000",
                    "2002-05-05T09:00:00.000",
                    "2002-07-30T09:00:00.000",
                    "0007-05-30T09:00:00.000",
                    "2002-02-21T09:00:00.000"
                };
                var startTime = "2002-05-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "util"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace