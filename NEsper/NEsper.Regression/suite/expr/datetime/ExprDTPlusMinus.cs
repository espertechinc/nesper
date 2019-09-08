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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTPlusMinus
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTPlusMinusSimple());
            executions.Add(new ExprDTPlusMinusTimePeriod());
            return executions;
        }

        internal class ExprDTPlusMinusSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('var') create variable long varmsec", path);
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = new[] {
                    "val0", "val1", "val2", "val3",
                    "val6", "val7", "val8", "val9"
                };
                var epl = "@Name('s0') select " +
                          "current_timestamp.plus(varmsec) as val0," +
                          "DtoDate.plus(varmsec) as val1," +
                          "LongDate.plus(varmsec) as val2," +
                          "DtxDate.plus(varmsec) as val3," +
                          "current_timestamp.minus(varmsec) as val6," +
                          "DtoDate.minus(varmsec) as val7," +
                          "LongDate.minus(varmsec) as val8," +
                          "DtxDate.minus(varmsec) as val9" +
                          " from SupportDateTime";

                env.CompileDeploy(epl, path).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(long?), typeof(DateTimeOffset), typeof(long?), typeof(DateTimeEx),
                        typeof(long?), typeof(DateTimeOffset), typeof(long?), typeof(DateTimeEx)
                    });

                env.SendEventBean(SupportDateTime.Make(null));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null, null,
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null, null
                    });

                var expectedPlus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long", "dtx");
                var expectedMinus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long", "dtx");
                env.SendEventBean(SupportDateTime.Make(startTime));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(
                        expectedPlus,
                        expectedMinus));

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "varmsec", 1000);
                env.SendEventBean(SupportDateTime.Make(startTime));

                //System.out.println("==-> " + SupportDateTime.print(env.Listener("s0").assertOneGetNew().Get("val4")));
                expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T09:00:01.000",
                    "long",
                    "util",
                    "long",
                    "dtx");
                expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T08:59:59.000",
                    "long",
                    "util",
                    "long",
                    "dtx");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(
                        expectedPlus,
                        expectedMinus));

                env.Runtime.VariableService.SetVariableValue(
                    env.DeploymentId("var"),
                    "varmsec",
                    2 * 24 * 60 * 60 * 1000);
                env.SendEventBean(SupportDateTime.Make(startTime));
                expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-28T09:00:00.000",
                    "long",
                    "util",
                    "long",
                    "dtx");
                expectedPlus = SupportDateTime.GetArrayCoerced("2002-06-1T09:00:00.000", "long", "util", "long", "dtx");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(
                        expectedPlus,
                        expectedMinus));

                env.UndeployAll();
            }
        }

        internal class ExprDTPlusMinusTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = new [] { "val0","val1","val2","val3","val6","val7","val8","val9" };
                var eplFragment = "@Name('s0') select " +
                                  "current_timestamp.plus(1 hour 10 sec 20 msec) as val0," +
                                  "DtoDate.plus(1 hour 10 sec 20 msec) as val1," +
                                  "LongDate.plus(1 hour 10 sec 20 msec) as val2," +
                                  "DtxDate.plus(1 hour 10 sec 20 msec) as val3," +
                                  "current_timestamp.minus(1 hour 10 sec 20 msec) as val6," +
                                  "DtoDate.minus(1 hour 10 sec 20 msec) as val7," +
                                  "LongDate.minus(1 hour 10 sec 20 msec) as val8," +
                                  "DtxDate.minus(1 hour 10 sec 20 msec) as val9" +
                                  " from SupportDateTime";

                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(long?), typeof(DateTimeOffset?), typeof(long?), typeof(DateTimeEx),
                        typeof(long?), typeof(DateTimeOffset?), typeof(long?), typeof(DateTimeEx)
                    });

                env.SendEventBean(SupportDateTime.Make(startTime));
                var expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T010:00:10.020",
                    "long",
                    "util",
                    "long",
                    "dtx");
                var expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T07:59:49.980",
                    "long",
                    "util",
                    "long",
                    "dtx");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.SendEventBean(SupportDateTime.Make(null));
                expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T010:00:10.020",
                    "long",
                    "null",
                    "null",
                    "null");
                expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T07:59:49.980",
                    "long",
                    "null",
                    "null",
                    "null");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.UndeployAll();
            }
        }
    }
} // end of namespace