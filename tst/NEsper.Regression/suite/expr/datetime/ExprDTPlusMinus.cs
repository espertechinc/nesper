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
using com.espertech.esper.common.@internal.support;
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
                    "val1a", "val1b", "val1c", "val1d", "val1e",
                    "val2a", "val2b", "val2c", "val2d", "val2e",
                };
                var epl = "@Name('s0') select " +
                          
                          "current_timestamp.plus(varmsec) as val1a," +
                          "DateTimeEx.plus(varmsec) as val1b," +
                          "DateTimeOffset.plus(varmsec) as val1c," +
                          "DateTime.plus(varmsec) as val1d," +
                          "LongDate.plus(varmsec) as val1e," +

                          "current_timestamp.minus(varmsec) as val2a," +
                          "DateTimeEx.minus(varmsec) as val2b," +
                          "DateTimeOffset.minus(varmsec) as val2c," +
                          "DateTime.minus(varmsec) as val2d," +
                          "LongDate.minus(varmsec) as val2e" +

                          " from SupportDateTime";

                env.CompileDeploy(epl, path).AddListener("s0");
                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?),
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?)
                    });

                env.SendEventBean(SupportDateTime.Make(null));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null,
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null,
                    });

                var expectedPlus = SupportDateTime.GetArrayCoerced(startTime, "long", "dtx", "dto", "date", "long");
                var expectedMinus = SupportDateTime.GetArrayCoerced(startTime, "long", "dtx", "dto", "date", "long");
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
                    "dtx", 
                    "dto",
                    "date",
                    "long");
                expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T08:59:59.000",
                    "long",
                    "dtx",
                    "dto",
                    "date",
                    "long");

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
                    "dtx",
                    "dto",
                    "date",
                    "long");
                expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-06-1T09:00:00.000",
                    "long",
                    "dtx",
                    "dto",
                    "date",
                    "long");

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

                var fields = new[] {
                    "val1a",
                    "val1b",
                    "val1c",
                    "val1d",
                    "val1e",

                    "val2a",
                    "val2b",
                    "val2c",
                    "val2d",
                    "val2e"
                };
                var eplFragment = "@Name('s0') select " +
                                  
                                  "current_timestamp.plus(1 hour 10 sec 20 msec) as val1a," +
                                  "DateTimeEx.plus(1 hour 10 sec 20 msec) as val1b," +
                                  "DateTimeOffset.plus(1 hour 10 sec 20 msec) as val1c," +
                                  "DateTime.plus(1 hour 10 sec 20 msec) as val1d," +
                                  "LongDate.plus(1 hour 10 sec 20 msec) as val1e," +

                                  "current_timestamp.minus(1 hour 10 sec 20 msec) as val2a," +
                                  "DateTimeEx.minus(1 hour 10 sec 20 msec) as val2b," +
                                  "DateTimeOffset.minus(1 hour 10 sec 20 msec) as val2c," +
                                  "DateTime.minus(1 hour 10 sec 20 msec) as val2d," +
                                  "LongDate.minus(1 hour 10 sec 20 msec) as val2e" +

                                  " from SupportDateTime";

                env.CompileDeploy(eplFragment).AddListener("s0");
                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?),
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?)
                    });

                env.SendEventBean(SupportDateTime.Make(startTime));
                var expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T10:00:10.020",
                    "long",
                    "dtx",
                    "dto",
                    "date",
                    "long");
                var expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T07:59:49.980",
                    "long",
                    "dtx",
                    "dto",
                    "date",
                    "long");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.SendEventBean(SupportDateTime.Make(null));
                expectedPlus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T10:00:10.020",
                    "long",
                    "null",
                    "null",
                    "null",
                    "null");
                expectedMinus = SupportDateTime.GetArrayCoerced(
                    "2002-05-30T07:59:49.980",
                    "long",
                    "null",
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