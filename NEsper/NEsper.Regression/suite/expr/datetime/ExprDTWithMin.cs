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
                var fields = new [] { "val0","val1","val2" };
                var eplFragment = "@Name('s0') select " +
                                  "DtoDate.withMin('month') as val0," +
                                  "LongDate.withMin('month') as val1," +
                                  "DtxDate.withMin('month') as val2" +
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

        internal class ExprDTWithMinFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5,val6,val7".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "DtoDate.withMin('msec') as val0," +
                                  "DtoDate.withMin('sec') as val1," +
                                  "DtoDate.withMin('minutes') as val2," +
                                  "DtoDate.withMin('hour') as val3," +
                                  "DtoDate.withMin('day') as val4," +
                                  "DtoDate.withMin('month') as val5," +
                                  "DtoDate.withMin('year') as val6," +
                                  "DtoDate.withMin('week') as val7" +
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