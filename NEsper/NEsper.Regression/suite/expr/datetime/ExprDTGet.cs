///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTGet
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTGetFields());
            executions.Add(new ExprDTGetInput());
            return executions;
        }

        internal class ExprDTGetInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0", "val1", "val2", "val3", "val4" };
                var epl = "@Name('s0') select " +
                          "DtoDate.Get('month') as val0," +
                          "LongDate.Get('month') as val1," +
                          "DtxDate.Get('month') as val2 " +
                          " from SupportDateTime";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?), typeof(int?)});

                var startTime = "2002-05-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, 4, 4});

                env.UndeployAll();

                // try event as input
                epl = "@Name('s0') select abc.Get('month') as val0 from SupportTimeStartEndA as abc";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.SendEventBean(SupportTimeStartEndA.Make("A0", startTime, 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "val0" },
                    new object[] {4});

                env.UndeployAll();

                // test "Get" method on object is preferred
                epl = "@Name('s0') select e.Get() as c0, e.Get('abc') as c1 from SupportEventWithJustGet as e";
                env.CompileDeployAddListenerMile(epl, "s0", 1);
                env.SendEventBean(new SupportEventWithJustGet());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1" },
                    new object[] {1, 2});

                env.UndeployAll();
            }
        }

        internal class ExprDTGetFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "val0","val1","val2","val3","val4","val5","val6","val7" };
                var eplFragment = "@Name('s0') select " +
                                  "DtoDate.Get('msec') as val0," +
                                  "DtoDate.Get('sec') as val1," +
                                  "DtoDate.Get('minutes') as val2," +
                                  "DtoDate.Get('hour') as val3," +
                                  "DtoDate.Get('day') as val4," +
                                  "DtoDate.Get('month') as val5," +
                                  "DtoDate.Get('year') as val6," +
                                  "DtoDate.Get('week') as val7" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(int?), typeof(int?), typeof(int?), typeof(int?),
                        typeof(int?), typeof(int?), typeof(int?), typeof(int?)
                    });

                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 2, 1, 9, 30, 4, 2002, 22});

                env.UndeployAll();
            }
        }
    }
} // end of namespace