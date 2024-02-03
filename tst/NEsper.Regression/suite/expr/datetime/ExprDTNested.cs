///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTNested : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = new[] { "val0", "val1", "val2", "val3" };
            var eplFragment = "@name('s0') select " +
                              "LongDate.set('hour', 1).set('minute', 2).set('second', 3) as val0," +
                              "DateTimeEx.set('hour', 1).set('minute', 2).set('second', 3) as val1," +
                              "DateTimeOffset.set('hour', 1).set('minute', 2).set('second', 3) as val2," +
                              "DateTime.set('hour', 1).set('minute', 2).set('second', 3) as val3" +
                              " from SupportDateTime";
            env.CompileDeploy(eplFragment).AddListener("s0");
            env.AssertStmtTypes(
                "s0",
                fields,
                new[] {
                    typeof(long?),
                    typeof(DateTimeEx),
                    typeof(DateTimeOffset?),
                    typeof(DateTime?)
                });

            var startTime = "2002-05-30T09:00:00.000";
            var expectedTime = "2002-05-30T01:02:03.000";
            env.SendEventBean(SupportDateTime.Make(startTime));

            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "dtx", "dto", "date"));

            env.UndeployAll();

            eplFragment = "@name('s0') select " +
                          "LongDate.set('hour', 1).set('minute', 2).set('second', 3).toDateTimeEx() as val0," +
                          "DateTimeEx.set('hour', 1).set('minute', 2).set('second', 3).toDateTimeEx() as val1," +
                          "DateTimeOffset.set('hour', 1).set('minute', 2).set('second', 3).toDateTimeEx() as val2," +
                          "DateTime.set('hour', 1).set('minute', 2).set('second', 3).toDateTimeEx() as val3" +
                          " from SupportDateTime";
            env.CompileDeployAddListenerMile(eplFragment, "s0", 1);
            env.AssertStmtTypesAllSame(
                "s0",
                fields,
                typeof(DateTimeEx)
            );

            env.SendEventBean(SupportDateTime.Make(startTime));
            env.AssertPropsNew(
                "s0",
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "dtx", "dtx", "dtx", "dtx"));

            env.UndeployAll();
        }
    }
} // end of namespace