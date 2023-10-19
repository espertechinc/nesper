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

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTPlusMinus
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithTimePeriod(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimePeriod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTPlusMinusTimePeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTPlusMinusSimple());
            return execs;
        }

        private class ExprDTPlusMinusSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('var') @public create variable long varmsec", path);
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = new[] {
                    "val1a", "val1b", "val1c", "val1d", "val1e",
                    "val2a", "val2b", "val2c", "val2d", "val2e",
                };
                var epl = "@name('s0') select " +
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
                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new[] {
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?),
                        typeof(long?), typeof(DateTimeEx), typeof(DateTimeOffset?), typeof(DateTime?), typeof(long?)
                    });

                env.SendEventBean(SupportDateTime.Make(null));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null,
                        SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null
                    });

                var expectedPlus = SupportDateTime.GetArrayCoerced(startTime, "long", "dtx", "dto", "date", "long");
                var expectedMinus = SupportDateTime.GetArrayCoerced(startTime, "long", "dtx", "dto", "date", "long");
                env.SendEventBean(SupportDateTime.Make(startTime));
                env.AssertPropsNew("s0", fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.RuntimeSetVariable("var", "varmsec", 1000);
                env.SendEventBean(SupportDateTime.Make(startTime));
                //Console.WriteLine("===> " + SupportDateTime.print(listener.assertOneGetNew().get("val4")));
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
                env.AssertPropsNew("s0", fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.RuntimeSetVariable("var", "varmsec", 2 * 24 * 60 * 60 * 1000);
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

                env.AssertPropsNew("s0", fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.UndeployAll();
            }
        }

        private class ExprDTPlusMinusTimePeriod : RegressionExecution
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
                var eplFragment = "@name('s0') select " +
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
                env.AssertStmtTypes(
                    "s0",
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

                env.AssertPropsNew("s0", fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

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
                env.AssertPropsNew("s0", fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));

                env.UndeployAll();
            }
        }
    }
} // end of namespace