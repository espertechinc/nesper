///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithDate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            var fields = "val0,val1,val2,val3,val4".SplitCsv();
            var epl = "" +
                      "create variable int varyear;\n" +
                      "create variable int varmonth;\n" +
                      "create variable int varday;\n" +
                      "@name('s0') select " +
                      "current_timestamp.withDate(varyear, varmonth, varday) as val0," +
                      "LongDate.withDate(varyear, varmonth, varday) as val1," +
                      "DateTimeEx.withDate(varyear, varmonth, varday) as val2," +
                      "DateTimeOffset.withDate(varyear, varmonth, varday) as val3," +
                      "DateTime.withDate(varyear, varmonth, varday) as val4" +
                      " from SupportDateTime";
            env.CompileDeploy(epl).AddListener("s0");
            env.AssertStmtTypes(
                "s0",
                fields,
                new Type[] {
                    typeof(long?),
                    typeof(long?),
                    typeof(DateTimeEx),
                    typeof(DateTimeOffset?),
                    typeof(DateTime?),
                });

            env.SendEventBean(SupportDateTime.Make(null));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null });

            var expectedTime = "2004-09-03T09:00:00.000";
            env.RuntimeSetVariable("s0", "varyear", 2004);
            env.RuntimeSetVariable("s0", "varmonth", 9);
            env.RuntimeSetVariable("s0", "varday", 3);
            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "dtx", "dto", "date"));

            expectedTime = "2002-09-30T09:00:00.000";
            env.RuntimeSetVariable("s0", "varyear", null);
            env.RuntimeSetVariable("s0", "varmonth", 9);
            env.RuntimeSetVariable("s0", "varday", null);
            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "dtx", "dto", "date"));

            env.UndeployAll();
        }
    }
} // end of namespace