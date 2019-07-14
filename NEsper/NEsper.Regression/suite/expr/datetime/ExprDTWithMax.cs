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
                var fields = "val0,val1,val2".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.withMax('month') as val0," +
                                  "longdate.withMax('month') as val1," +
                                  "exdate.withMax('month') as val2" +
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
                var expectedTime = "2002-12-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "util", "long", "dtx"));

                env.UndeployAll();
            }
        }

        internal class ExprDTWithMaxFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5,val6,val7".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.withMax('msec') as val0," +
                                  "utildate.withMax('sec') as val1," +
                                  "utildate.withMax('minutes') as val2," +
                                  "utildate.withMax('hour') as val3," +
                                  "utildate.withMax('day') as val4," +
                                  "utildate.withMax('month') as val5," +
                                  "utildate.withMax('year') as val6," +
                                  "utildate.withMax('week') as val7" +
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
                    "2002-5-30T09:00:00.999",
                    "2002-5-30T09:00:59.000",
                    "2002-5-30T09:59:00.000",
                    "2002-5-30T23:00:00.000",
                    "2002-5-31T09:00:00.000",
                    "2002-12-30T09:00:00.000",
                    "292278994-5-30T09:00:00.000",
                    "2002-12-26T09:00:00.000"
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