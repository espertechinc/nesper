///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTToDateCalMSec : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            string[] fields = {
                "val0",
                "val1",
                "val2",
                "val3",
                "val6",
                "val7",
                "val8",
                "val9",
                "val12",
                "val13",
                "val14",
                "val15"
            };

            var eplFragment = "@Name('s0') select " +
                              "current_timestamp.toDate() as val0," +
                              "DtoDate.toDate() as val1," +
                              "LongDate.toDate() as val2," +
                              "DtxDate.toDate() as val3," +
                              "current_timestamp.toCalendar() as val6," +
                              "DtoDate.toCalendar() as val7," +
                              "LongDate.toCalendar() as val8," +
                              "DtxDate.toCalendar() as val9," +
                              "current_timestamp.toMillisec() as val12," +
                              "DtoDate.toMillisec() as val13," +
                              "LongDate.toMillisec() as val14," +
                              "DtxDate.toMillisec() as val15" +
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
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(long?),
                    typeof(long?),
                    typeof(long?),
                    typeof(long?)
                });

            env.SendEventBean(SupportDateTime.Make(startTime));
            var expectedUtil = SupportDateTime.GetArrayCoerced(startTime, "util", "util", "util", "util");
            var expectedCal = SupportDateTime.GetArrayCoerced(startTime, "dtx", "dtx", "dtx", "dtx");
            var expectedMsec = SupportDateTime.GetArrayCoerced(startTime, "long", "long", "long", "long");
            var expected = EPAssertionUtil.ConcatenateArray(expectedUtil, expectedCal, expectedMsec);

            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.SendEventBean(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {
                    SupportDateTime.GetValueCoerced(startTime, "util"), null, null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "dtx"), null, null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null, null
                });

            env.UndeployAll();
        }
    }
} // end of namespace