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
            var execs = new List<RegressionExecution>();
            WithInput(execs);
            WithFields(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTSetFields());
            return execs;
        }

        public static IList<RegressionExecution> WithInput(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTSetInput());
            return execs;
        }

        internal class ExprDTSetInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {
                    "val0",
                    "val1",
                    "val2",
                    "val3"
                };
                var eplFragment = "@Name('s0') select " +
                                  "DateTimeEx.set('month', 1) as val0," +
                                  "DateTimeOffset.set('month', 1) as val1," +
                                  "DateTime.set('month', 1) as val2," +
                                  "LongDate.set('month', 1) as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(DateTimeEx),
                        typeof(DateTimeOffset?),
                        typeof(DateTime?),
                        typeof(long?)
                    });

                var startTime = "2002-05-30T09:00:00.000";
                var expectedTime = "2002-01-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                var expectedResults = SupportDateTime.GetArrayCoerced(expectedTime, "dtx", "dto", "date", "long");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expectedResults);

                env.UndeployAll();
            }
        }

        internal class ExprDTSetFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val0", "val1", "val2", "val3", "val4", "val5", "val6" };
                var eplFragment = "@Name('s0') select " +
                                  "DateTimeOffset.set('msec', 1) as val0," +
                                  "DateTimeOffset.set('sec', 2) as val1," +
                                  "DateTimeOffset.set('minutes', 3) as val2," +
                                  "DateTimeOffset.set('hour', 13) as val3," +
                                  "DateTimeOffset.set('day', 5) as val4," +
                                  "DateTimeOffset.set('month', 6) as val5," +
                                  "DateTimeOffset.set('year', 7) as val6" +
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
                        typeof(DateTimeOffset?) // val6
                    });

                string[] expected = {
                    "2002-05-30T09:00:00.001", // val0
                    "2002-05-30T09:00:02.000", // val1
                    "2002-05-30T09:03:00.000", // val2
                    "2002-05-30T13:00:00.000", // val3
                    "2002-05-05T09:00:00.000", // val4
                    "2002-06-30T09:00:00.000", // val5
                    "0007-05-30T09:00:00.000" // val6
                };
                var startTime = "2002-05-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));
                var datesCoerced = SupportDateTime.GetArrayCoerced(expected, "dto");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    datesCoerced);

                env.UndeployAll();
            }
        }
    }
} // end of namespace