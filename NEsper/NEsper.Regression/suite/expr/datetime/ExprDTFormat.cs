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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTFormat
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTFormatSimple());
            executions.Add(new ExprDTFormatWString());
            return executions;
        }

        internal class ExprDTFormatSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = "val0,val1,val2,val3".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "current_timestamp.format() as val0," +
                                  "utildate.format() as val1," +
                                  "longdate.format() as val2," +
                                  "exdate.format() as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)
                    });

                env.SendEventBean(SupportDateTime.Make(startTime));
                var expected = SupportDateTime.GetArrayCoerced(startTime, "sdf", "sdf", "sdf", "sdf");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

                env.SendEventBean(SupportDateTime.Make(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {SupportDateTime.GetValueCoerced(startTime, "sdf"), null, null, null, null, null});

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

                var fields = "val0,val1,val2,val3,val4,val5,val6".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "longdate.format(\"" +
                                  sdfPattern +
                                  "\") as val0," +
                                  "utildate.format(\"" +
                                  sdfPattern +
                                  "\") as val1," +
                                  "exdate.format(\"" +
                                  sdfPattern +
                                  "\") as val2" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                LambdaAssertionUtil.AssertTypesAllSame(env.Statement("s0").EventType, fields, typeof(string));

                var sdt = SupportDateTime.Make(startTime);
                env.SendEventBean(SupportDateTime.Make(startTime));

                var received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.That(received.Get("val0"), Is.EqualTo(sdf.Format(sdt.LongDate)));
                Assert.That(received.Get("val1"), Is.EqualTo(sdf.Format(sdt.UtilDate)));
                Assert.That(received.Get("val2"), Is.EqualTo(sdf.Format(sdt.ExDate)));

                env.SendEventBean(SupportDateTime.Make(null));
                received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.That(received.Get("val0"), Is.Null);
                Assert.That(received.Get("val1"), Is.Null);
                Assert.That(received.Get("val2"), Is.Null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace