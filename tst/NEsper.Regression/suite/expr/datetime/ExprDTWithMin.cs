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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTWithMin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithInput(execs);
            With(Fields)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTWithMinFields());
            return execs;
        }

        public static IList<RegressionExecution> WithInput(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTWithMinInput());
            return execs;
        }

        internal class ExprDTWithMinInput : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val0", "val1", "val2", "val3" };
                var eplFragment = "@name('s0') select " +
                                  "LongDate.withMin('month') as val0," +
                                  "DateTime.withMin('month') as val1," +
                                  "DateTimeOffset.withMin('month') as val2," +
                                  "DateTimeEx.withMin('month') as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new[] {
                        typeof(long?),
                        typeof(DateTime?),
                        typeof(DateTimeOffset?),
                        typeof(DateTimeEx),
                    });

                var startTime = "2002-05-30T09:00:00.000";
                var expectedTime = "2002-01-30T09:00:00.000";
                env.SendEventBean(SupportDateTime.Make(startTime));

                env.AssertPropsNew(
                    "s0",
                    fields,
                    SupportDateTime.GetArrayCoerced(expectedTime, "long", "date", "dto", "dtx"));

                env.UndeployAll();
            }
        }

        internal class ExprDTWithMinFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val0", "val1", "val2", "val3", "val4", "val5", "val6" };
                var eplFragment = "@name('s0') select " +
                                  "DateTimeOffset.withMin('msec') as val0," +
                                  "DateTimeOffset.withMin('sec') as val1," +
                                  "DateTimeOffset.withMin('minutes') as val2," +
                                  "DateTimeOffset.withMin('hour') as val3," +
                                  "DateTimeOffset.withMin('day') as val4," +
                                  "DateTimeOffset.withMin('month') as val5," +
                                  "DateTimeOffset.withMin('year') as val6" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                env.AssertStmtTypesAllSame("s0", fields, typeof(DateTimeOffset?));

                string[] expected = {
                    "2002-05-30T09:01:02.000",
                    "2002-05-30T09:01:00.003",
                    "2002-05-30T09:00:02.003",
                    "2002-05-30T00:01:02.003",
                    "2002-05-01T09:01:02.003",
                    "2002-01-30T09:01:02.003",
                    "0001-05-30T09:01:02.003"
                };
                var startTime = "2002-05-30T09:01:02.003";
                env.SendEventBean(SupportDateTime.Make(startTime));
                //Console.WriteLine("==-> " + SupportDateTime.print(env.Listener("s0").AssertOneGetNew().Get("val7")));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    SupportDateTime.GetArrayCoerced(expected, "dto"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace