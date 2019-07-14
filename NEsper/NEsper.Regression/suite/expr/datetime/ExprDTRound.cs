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
    public class ExprDTRound
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTRoundInput());
            executions.Add(new ExprDTRoundCeil());
            executions.Add(new ExprDTRoundFloor());
            executions.Add(new ExprDTRoundHalf());
            return executions;
        }

        internal class ExprDTRoundInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.roundCeiling('hour') as val0," +
                                  "longdate.roundCeiling('hour') as val1," +
                                  "exdate.roundCeiling('hour') as val2" +
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

                var startTime = "2002-05-30T09:01:02.003";
                var expectedTime = "2002-05-30T10:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "util", "long", "dtx"));

                env.UndeployAll();
            }
        }

        internal class ExprDTRoundCeil : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5,val6".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.roundCeiling('msec') as val0," +
                                  "utildate.roundCeiling('sec') as val1," +
                                  "utildate.roundCeiling('minutes') as val2," +
                                  "utildate.roundCeiling('hour') as val3," +
                                  "utildate.roundCeiling('day') as val4," +
                                  "utildate.roundCeiling('month') as val5," +
                                  "utildate.roundCeiling('year') as val6" +
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
                        typeof(DateTimeOffset?)
                    });

                string[] expected = {
                    "2002-05-30T09:01:02.003",
                    "2002-05-30T09:01:03.000",
                    "2002-05-30T09:02:00.000",
                    "2002-05-30T10:00:00.000",
                    "2002-05-31T00:00:00.000",
                    "2002-06-01T00:00:00.000",
                    "2003-01-01T00:00:00.000"
                };
                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "util"));

                env.UndeployAll();
            }
        }

        internal class ExprDTRoundFloor : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5,val6".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.roundFloor('msec') as val0," +
                                  "utildate.roundFloor('sec') as val1," +
                                  "utildate.roundFloor('minutes') as val2," +
                                  "utildate.roundFloor('hour') as val3," +
                                  "utildate.roundFloor('day') as val4," +
                                  "utildate.roundFloor('month') as val5," +
                                  "utildate.roundFloor('year') as val6" +
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
                        typeof(DateTimeOffset?)
                    });

                string[] expected = {
                    "2002-05-30T09:01:02.003",
                    "2002-05-30T09:01:02.000",
                    "2002-05-30T09:01:00.000",
                    "2002-05-30T09:00:00.000",
                    "2002-05-30T00:00:00.000",
                    "2002-05-1T00:00:00.000",
                    "2002-01-1T00:00:00.000"
                };
                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "util"));

                env.UndeployAll();
            }
        }

        internal class ExprDTRoundHalf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5,val6".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "utildate.roundHalf('msec') as val0," +
                                  "utildate.roundHalf('sec') as val1," +
                                  "utildate.roundHalf('minutes') as val2," +
                                  "utildate.roundHalf('hour') as val3," +
                                  "utildate.roundHalf('day') as val4," +
                                  "utildate.roundHalf('month') as val5," +
                                  "utildate.roundHalf('year') as val6" +
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
                        typeof(DateTimeOffset?)
                    });

                string[] expected = {
                    "2002-05-30T15:30:02.550",
                    "2002-05-30T15:30:03.000",
                    "2002-05-30T15:30:00.000",
                    "2002-05-30T16:00:00.00",
                    "2002-05-31T00:00:00.000",
                    "2002-06-01T00:00:00.000",
                    "2002-01-01T00:00:00.000"
                };
                var startTime = "2002-05-30T15:30:02.550";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "util"));

                // test rounding up/down
                env.UndeployAll();
                fields = "val0".SplitCsv();
                eplFragment = "@Name('s0') select utildate.roundHalf('min') as val0 from SupportDateTime";
                env.CompileDeployAddListenerMile(eplFragment, "s0", 1);

                env.SendEventBean(SupportDateTime.Make("2002-05-30T15:30:29.999"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {SupportDateTime.GetValueCoerced("2002-05-30T15:30:00.000", "util")});

                env.SendEventBean(SupportDateTime.Make("2002-05-30T15:30:30.000"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {SupportDateTime.GetValueCoerced("2002-05-30T15:31:00.000", "util")});

                env.SendEventBean(SupportDateTime.Make("2002-05-30T15:30:30.001"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {SupportDateTime.GetValueCoerced("2002-05-30T15:31:00.000", "util")});

                env.UndeployAll();
            }
        }
    }
} // end of namespace