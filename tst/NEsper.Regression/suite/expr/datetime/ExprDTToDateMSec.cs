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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTToDateMSec
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithToDateTimeExChain(execs);
            With(DTToDateTimeExMSecValue)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithDTToDateTimeExMSecValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTToDateTimeExMSecValue());
            return execs;
        }

        public static IList<RegressionExecution> WithToDateTimeExChain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprToDateTimeExChain());
            return execs;
        }

        public class ExprToDateTimeExChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                env.CompileDeploy(
                    "@name('s0') select current_timestamp.toDateTimeEx().AddDays(1) as c from SupportBean");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.That(theEvent, Is.Not.Null);
                Assert.That(theEvent.Get("c"), Is.InstanceOf<DateTimeEx>());

                var expDateTime = DateTimeEx.UtcInstance(0).AddDays(1);
                var theDateTime = (DateTimeEx)theEvent.Get("c");
                Assert.That(theDateTime, Is.EqualTo(expDateTime));

                env.UndeployAll();
            }
        }

        public class ExprDTToDateTimeExMSecValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("select current_timestamp.toDateTimeEx().AddDays(1) from SupportBean");

                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                string[] fields = {
                    "val1a", "val1b", "val1c", "val1d", "val1e",
                    "val2a", "val2b", "val2c", "val2d", "val2e",
                    "val3a", "val3b", "val3c", "val3d", "val3e",
                    "val4a", "val4b", "val4c", "val4d", "val4e",
                };

                var eplFragment =
                    "@name('s0') select " +
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
                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new[] {
                        typeof(DateTime),
                        typeof(DateTime),
                        typeof(DateTime),
                        typeof(DateTime),
                        typeof(DateTime),

                        typeof(DateTimeOffset),
                        typeof(DateTimeOffset),
                        typeof(DateTimeOffset),
                        typeof(DateTimeOffset),
                        typeof(DateTimeOffset),

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

                env.AssertPropsNew("s0", fields, expected);

                env.SendEventBean(SupportDateTime.Make(null));
                env.AssertPropsNew(
                    "s0",
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
    }
} // end of namespace