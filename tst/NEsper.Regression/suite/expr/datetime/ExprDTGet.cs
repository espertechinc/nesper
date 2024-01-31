///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

// INTEGERBOXED

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTGet
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFields(execs);
            WithInput(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInput(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTGetInput());
            return execs;
        }

        public static IList<RegressionExecution> WithFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTGetFields());
            return execs;
        }

        internal class ExprDTGetInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val0", "val1", "val2", "val3" };
                var epl = "@name('s0') select " +
                          "DateTimeEx.get('month') as val0," +
                          "DateTimeOffset.get('month') as val1," +
                          "DateTime.get('month') as val2," +
                          "LongDate.get('month') as val3" +
                          " from SupportDateTime";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new Type[] {
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?)
                    });

                var startTime = "2002-05-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));
                env.AssertPropsNew("s0", fields, new object[] { 5, 5, 5, 5 });

                env.UndeployAll();

                // try event as input
                epl = "@name('s0') select abc.Get('month') as val0 from SupportTimeStartEndA as abc";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.SendEventBean(SupportTimeStartEndA.Make("A0", startTime, 0));
                env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { 5 });

                env.UndeployAll();

                // test "get" method on object is preferred
                epl = "@name('s0') select e.Get() as c0, e.Get('abc') as c1 from SupportEventWithJustGet as e";
                env.CompileDeployAddListenerMile(epl, "s0", 1);
                env.SendEventBean(new SupportEventWithJustGet());
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { 1, 2 });

                env.UndeployAll();
            }
        }

        internal class ExprDTGetFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val0", "val1", "val2", "val3", "val4", "val5", "val6", "val7" };
                var eplFragment = "@name('s0') select " +
                                  "DateTimeOffset.get('msec') as val0," +
                                  "DateTimeOffset.get('sec') as val1," +
                                  "DateTimeOffset.get('minutes') as val2," +
                                  "DateTimeOffset.get('hour') as val3," +
                                  "DateTimeOffset.get('day') as val4," +
                                  "DateTimeOffset.get('month') as val5," +
                                  "DateTimeOffset.get('year') as val6," +
                                  "DateTimeOffset.get('week') as val7" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new Type[] {
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?),
                        typeof(int?)
                    });

                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                env.AssertPropsNew("s0", fields, new object[] { 3, 2, 1, 9, 30, 5, 2002, 22 });

                env.UndeployAll();
            }
        }
    }
} // end of namespace