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
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithTime : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl = "@Name('variables') create variable int varhour;\n" +
                      "create variable int varmin;\n" +
                      "create variable int varsec;\n" +
                      "create variable int varmsec;\n";
            env.CompileDeploy(epl, path);
            var variablesDepId = env.DeploymentId("variables");

            var startTime = "2002-05-30T09:00:00.000";
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

            var fields = "val0,val1,val2,val3".SplitCsv();
            epl = "@Name('s0') select " +
                  "current_timestamp.withTime(varhour, varmin, varsec, varmsec) as val0," +
                  "DtoDate.withTime(varhour, varmin, varsec, varmsec) as val1," +
                  "LongDate.withTime(varhour, varmin, varsec, varmsec) as val2," +
                  "DtxDate.withTime(varhour, varmin, varsec, varmsec) as val3" +
                  " from SupportDateTime";
            env.CompileDeploy(epl, path).AddListener("s0");
            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                fields,
                new[] {
                    typeof(long?),
                    typeof(DateTimeOffset),
                    typeof(long?),
                    typeof(DateTimeEx)
                });

            env.SendEventBean(SupportDateTime.Make(null));

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null});

            var expectedTime = "2002-05-30T09:00:00.000";
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varhour", null); // variable is null
            env.SendEventBean(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "util"));

            expectedTime = "2002-05-30T01:02:03.004";
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varhour", 1);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varmin", 2);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varsec", 3);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varmsec", 4);
            env.SendEventBean(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "util", "long", "dtx"));

            expectedTime = "2002-05-30T00:00:00.006";
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varhour", 0);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varmin", null);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varsec", null);
            env.Runtime.VariableService.SetVariableValue(variablesDepId, "varmsec", 6);
            env.SendEventBean(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "util", "long", "dtx"));

            env.UndeployAll();
        }

        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            return executions;
        }
    }
} // end of namespace