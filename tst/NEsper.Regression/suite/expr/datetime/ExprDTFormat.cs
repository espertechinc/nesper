///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTFormat
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithWString(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWString(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTFormatWString());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTFormatSimple());
            return execs;
        }

        internal class ExprDTFormatSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = new[] { "val0", "val1", "val2", "val3", "val4" };
                var eplFragment = "@name('s0') select " +
                                  "current_timestamp.format() as val0," +
                                  "DateTimeEx.format() as val1," +
                                  "DateTimeOffset.format() as val2," +
                                  "DateTime.format() as val3," +
                                  "LongDate.format() as val4" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                env.AssertStmtTypesAllSame("s0", fields, typeof(string));

                env.SendEventBean(SupportDateTime.Make(startTime));
                var expected = SupportDateTime.GetArrayCoerced(
                    startTime,
                    "iso",
                    "iso",
                    "iso",
                    "iso",
                    "iso");

                env.AssertPropsNew("s0", fields, expected);

                env.SendEventBean(SupportDateTime.Make(null));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        SupportDateTime.GetValueCoerced(startTime, "iso"),
                        null,
                        null,
                        null,
                        null
                    });

                env.UndeployAll();
            }
        }

        internal class ExprDTFormatWString : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));
                var sdfPattern = "yyyy.MM.dd G 'at' HH:mm:ss";
                var sdf = new SimpleDateFormat(sdfPattern);

                var fields = "val0,val1,val2,val3".SplitCsv();
                var eplFragment =
                    "@name('s0') select " +
                    $"DateTimeEx.format(\"{sdfPattern}\") as val0," +
                    $"DateTimeOffset.format(\"{sdfPattern}\") as val1," +
                    $"DateTime.format(\"{sdfPattern}\") as val2," +
                    $"LongDate.format(\"{sdfPattern}\") as val3" +
                    " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                env.AssertStmtTypesAllSame("s0", fields, typeof(string));

                var sdt = SupportDateTime.Make(startTime);
                env.SendEventBean(SupportDateTime.Make(startTime));

                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        sdf.Format(sdt.DateTimeEx),
                        sdf.Format(sdt.DateTimeOffset),
                        sdf.Format(sdt.DateTime),
                        sdf.Format(sdt.LongDate)
                    });

                env.SendEventBean(SupportDateTime.Make(null));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        null,
                        null,
                        null,
                        null
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace