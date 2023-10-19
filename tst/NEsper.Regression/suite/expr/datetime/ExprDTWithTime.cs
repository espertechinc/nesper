///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithTime : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl =
                "@name('variables') @public create variable int varhour;\n" +
                "@public create variable int varmin;\n" +
                "@public create variable int varsec;\n" +
                "@public create variable int varmsec;\n";
            env.CompileDeploy(epl, path);

            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            var fields = new[] { "val0", "val1", "val2", "val3", "val4" };
            epl = "@name('s0') select " +
                  "current_timestamp.withTime(varhour, varmin, varsec, varmsec) as val0," +
                  "LongDate.withTime(varhour, varmin, varsec, varmsec) as val1," +
                  "DateTime.withTime(varhour, varmin, varsec, varmsec) as val2," +
                  "DateTimeOffset.withTime(varhour, varmin, varsec, varmsec) as val3," +
                  "DateTimeEx.withTime(varhour, varmin, varsec, varmsec) as val4" +
                  " from SupportDateTime";
            env.CompileDeploy(epl, path).AddListener("s0");
            env.AssertStmtTypes(
                "s0",
                fields,
                new Type[] {
                    typeof(long?),
                    typeof(long?),
                    typeof(DateTime?),
                    typeof(DateTimeOffset?),
                    typeof(DateTimeEx)
                });

            env.SendEventBean(SupportDateTime.Make(null));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    SupportDateTime.GetValueCoerced(startTime, "long"),
                    null,
                    null,
                    null,
                    null
                });

            var expectedTime = "2002-05-30T09:00:00.000";
            env.RuntimeSetVariable("variables", "varhour", null); // variable is null
            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "date", "dto", "dtx"));

            expectedTime = "2002-05-30T01:02:03.004";
            env.RuntimeSetVariable("variables", "varhour", 1);
            env.RuntimeSetVariable("variables", "varmin", 2);
            env.RuntimeSetVariable("variables", "varsec", 3);
            env.RuntimeSetVariable("variables", "varmsec", 4);
            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "date", "dto", "dtx"));

            expectedTime = "2002-05-30T00:00:00.006";
            env.RuntimeSetVariable("variables", "varhour", 0);
            env.RuntimeSetVariable("variables", "varmin", null);
            env.RuntimeSetVariable("variables", "varsec", null);
            env.RuntimeSetVariable("variables", "varmsec", 6);
            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "date", "dto", "dtx"));

            env.UndeployAll();
        }
    }
} // end of namespace