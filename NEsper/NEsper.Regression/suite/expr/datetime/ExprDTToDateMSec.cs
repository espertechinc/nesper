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
    public class ExprDTToDateMSec : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            string[] fields = {
                "val1a", "val1b", "val1c", "val1d", "val1e",
                "val2a", "val2b", "val2c", "val2d", "val2e",
                "val3a", "val3b", "val3c", "val3d", "val3e",
                "val4a", "val4b", "val4c", "val4d", "val4e",
            };

            var eplFragment =
                "@Name('s0') select " +

                "current_timestamp.toDateTime() as val1a," +
                "LongDate.toDateTime() as val1b," +
                "DateTimeEx.toDateTime() as val1c," +
                "DateTimeOffset.toDateTime() as val1d," +
                "DateTimeEx.toDateTime() as val1e," +

                "current_timestamp.toDateTimeOffset() as val2a," +
                "LongDate.toDateTimeOffset() as val2b," +
                "DateTime.toDateTimeOffset() as val2c," +
                "DateTimeOffset.toDateTimeOffset() as val2d," +
                "DateTimeEx.toDateTimeOffset() as val2e," +

                "current_timestamp.toDateTimeEx() as val3a," +
                "LongDate.toDateTimeEx() as val3b," +
                "DateTime.toDateTimeEx() as val3c," +
                "DateTimeOffset.toDateTimeEx() as val3d," +
                "DateTimeEx.toDateTimeEx() as val3e," +

                "current_timestamp.toMillisec() as val4a," +
                "LongDate.toMillisec() as val4b," +
                "DateTime.toMillisec() as val4c," +
                "DateTimeOffset.toMillisec() as val4d," +
                "DateTimeEx.toMillisec() as val4e" +

                " from SupportDateTime";

            env.CompileDeploy(eplFragment).AddListener("s0");
            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                fields,
                new[] {
                    typeof(DateTime?),
                    typeof(DateTime?),
                    typeof(DateTime?),
                    typeof(DateTime?),
                    typeof(DateTime?),

                    typeof(DateTimeOffset?),
                    typeof(DateTimeOffset?),
                    typeof(DateTimeOffset?),
                    typeof(DateTimeOffset?),
                    typeof(DateTimeOffset?),

                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),
                    typeof(DateTimeEx),

                    typeof(long?),
                    typeof(long?),
                    typeof(long?),
                    typeof(long?),
                    typeof(long?)
                });

            env.SendEventBean(SupportDateTime.Make(startTime));

            var expected = EPAssertionUtil.ConcatenateArray(
                SupportDateTime.GetArrayCoerced(startTime, "date".Repeat(5)),
                SupportDateTime.GetArrayCoerced(startTime, "dto".Repeat(5)),
                SupportDateTime.GetArrayCoerced(startTime, "dtx".Repeat(5)),
                SupportDateTime.GetArrayCoerced(startTime, "long".Repeat(5)));

            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.SendEventBean(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {
                    SupportDateTime.GetValueCoerced(startTime, "date"), null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "dto"), null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "dtx"), null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null
                });

            env.UndeployAll();
        }
    }
} // end of namespace