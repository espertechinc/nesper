///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithDate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            var fields = new [] { "val0","val1","val2","val3", "val4" };
            var epl = "" +
                      "create variable int varyear;\n" +
                      "create variable int varmonth;\n" +
                      "create variable int varday;\n" +
                      "@Name('s0') select " +
                      "current_timestamp.withDate(varyear, varmonth, varday) as val0," +
                      "LongDate.withDate(varyear, varmonth, varday) as val1," +
                      "DateTimeEx.withDate(varyear, varmonth, varday) as val2," + 
                      "DateTimeOffset.withDate(varyear, varmonth, varday) as val3," +
                      "DateTime.withDate(varyear, varmonth, varday) as val4" +
                      " from SupportDateTime";
            env.CompileDeploy(epl).AddListener("s0");
            var deployId = env.DeploymentId("s0");
            SupportEventPropUtil.AssertTypes(
                env.Statement("s0").EventType,
                fields,
                new[] {
                    typeof(long?),
                    typeof(long?),
                    typeof(DateTimeEx),
                    typeof(DateTimeOffset?),
                    typeof(DateTime?),
                });

            env.SendEventBean(SupportDateTime.Make(null));

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {
                    SupportDateTime.GetValueCoerced(startTime, "long"),
                    null,
                    null,
                    null,
                    null
                });

            var expectedTime = "2004-09-03T09:00:00.000";
            env.Runtime.VariableService.SetVariableValue(deployId, "varyear", 2004);
            env.Runtime.VariableService.SetVariableValue(deployId, "varmonth", 9);
            env.Runtime.VariableService.SetVariableValue(deployId, "varday", 3);
            env.SendEventBean(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "dtx", "dto", "date"));

            expectedTime = "2002-09-30T09:00:00.000";
            env.Runtime.VariableService.SetVariableValue(deployId, "varyear", null);
            env.Runtime.VariableService.SetVariableValue(deployId, "varmonth", 9);
            env.Runtime.VariableService.SetVariableValue(deployId, "varday", null);
            env.SendEventBean(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "long", "dtx", "dto", "date"));

            env.UndeployAll();
        }
    }
} // end of namespace