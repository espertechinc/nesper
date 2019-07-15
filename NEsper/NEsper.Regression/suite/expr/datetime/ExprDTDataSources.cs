///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTDataSources
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTDataSourcesStartEndTS());
            executions.Add(new ExprDTDataSourcesFieldWValue());
            executions.Add(new ExprDTDataSourcesAllCombinations());
            executions.Add(new ExprDTDataSourcesMinMax());
            return executions;
        }

        private static void RunAssertionAllCombinations(
            RegressionEnvironment env,
            string field,
            AtomicLong milestone)
        {
            var methods = Collections.List(
                "getMinuteOfHour",
                "getMonthOfYear",
                "getDayOfMonth",
                "getDayOfWeek",
                "getDayOfYear",
                "getEra",
                "gethourOfDay",
                "getmillisOfSecond",
                "getsecondOfMinute",
                "getweekyear",
                "getyear");
            var epl = new StringBuilder();
            epl.Append("@Name('s0') select ");
            var count = 0;
            var delimiter = "";
            foreach (var method in methods) {
                epl.Append(delimiter)
                    .Append(field)
                    .Append(".")
                    .Append(method)
                    .Append("() ")
                    .Append("c")
                    .Append(Convert.ToString(count++));
                delimiter = ",";
            }

            epl.Append(" from SupportDateTime");

            env.CompileDeployAddListenerMile(epl.ToString(), "s0", milestone.GetAndIncrement());

            var sdt = SupportDateTime.Make("2002-05-30T09:01:02.003");
            sdt.ExDate.SetMillis(3);
            env.SendEventBean(sdt);

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10".SplitCsv(),
                new object[] {
                    1, 4, 30, 5, 150, 1, 9, 3, 2, 22, 2002
                });

            env.UndeployAll();
        }

        private static void SendBean(
            RegressionEnvironment env,
            long longPrimitive,
            long longBoxed)
        {
            var bean = new SupportBean();
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        internal class ExprDTDataSourcesMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "min(LongPrimitive).before(max(LongBoxed), 1 second) as c0," +
                          "min(LongPrimitive, LongBoxed).before(20000L, 1 second) as c1" +
                          " from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "c0,c1".SplitCsv();

                SendBean(env, 20000, 20000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                SendBean(env, 19000, 20000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprDTDataSourcesAllCombinations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "utildate,longdate,caldate,zoneddate,localdate".SplitCsv();
                var milestone = new AtomicLong();

                foreach (var field in fields) {
                    RunAssertionAllCombinations(env, field, milestone);
                }
            }
        }

        internal class ExprDTDataSourcesFieldWValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime =
                    "2002-05-30T09:01:02.003"; // use 2-digit hour, see https://bugs.openjdk.java.net/browse/JDK-8066806
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields =
                    "valmoh,valmoy,valdom,valdow,valdoy,valera,valhod,valmos,valsom,valwye,valyea,val1,val2,val3"
                        .SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "current_timestamp.getMinuteOfHour() as valmoh," +
                                  "current_timestamp.getMonthOfYear() as valmoy," +
                                  "current_timestamp.getDayOfMonth() as valdom," +
                                  "current_timestamp.getDayOfWeek() as valdow," +
                                  "current_timestamp.getDayOfYear() as valdoy," +
                                  "current_timestamp.getEra() as valera," +
                                  "current_timestamp.gethourOfDay() as valhod," +
                                  "current_timestamp.getmillisOfSecond()  as valmos," +
                                  "current_timestamp.getsecondOfMinute() as valsom," +
                                  "current_timestamp.getweekyear() as valwye," +
                                  "current_timestamp.getyear() as valyea," +
                                  "utildate.gethourOfDay() as val1," +
                                  "longdate.gethourOfDay() as val2," +
                                  "exdate.gethourOfDay() as val3" +
                                  " from SupportDateTime";
                env.CompileDeploy(eplFragment).AddListener("s0");
                foreach (var field in fields) {
                    Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(field));
                }

                env.SendEventBean(SupportDateTime.Make(startTime));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        1, 4, 30, 5, 150, 1, 9, 3, 2, 22, 2002, 9, 9, 9, 9, 9
                    });

                env.UndeployAll();
            }
        }

        internal class ExprDTDataSourcesStartEndTS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // test Map inheritance via create-schema
                var eplMap =
                    "create schema ParentType as (startTS long, endTS long) starttimestamp startTS endtimestamp endTS;\n" +
                    "create schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeployWBusPublicType(eplMap, path);

                env.CompileDeploy("@Name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                Assert.AreEqual("startTS", env.Statement("s0").EventType.StartTimestampPropertyName);
                Assert.AreEqual("endTS", env.Statement("s0").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test Object-array inheritance via create-schema
                path.Clear();
                var eplObjectArray =
                    "create objectarray schema ParentType as (startTS long, endTS long) starttimestamp startTS endtimestamp endTS;\n" +
                    "create objectarray schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeployWBusPublicType(eplObjectArray, path);

                env.CompileDeploy("@Name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                Assert.AreEqual("startTS", env.Statement("s0").EventType.StartTimestampPropertyName);
                Assert.AreEqual("endTS", env.Statement("s0").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test POJO inheritance via create-schema
                path.Clear();
                var eplPOJO = "create schema InterfaceType as " +
                              typeof(SupportStartTSEndTSInterface).Name +
                              " starttimestamp startTS endtimestamp endTS;\n" +
                              "create schema DerivedType as " +
                              typeof(SupportStartTSEndTSImpl).Name +
                              " inherits InterfaceType";
                env.CompileDeployWBusPublicType(eplPOJO, path);

                var compiled = env.Compile(
                    "@Name('s2') select * from DerivedType dt where dt.before(current_timestamp())",
                    path);
                env.Deploy(compiled);
                Assert.AreEqual("startTS", env.Statement("s2").EventType.StartTimestampPropertyName);
                Assert.AreEqual("endTS", env.Statement("s2").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test incompatible
                path.Clear();
                var eplT1T2 =
                    "create schema T1 as (startTS long, endTS long) starttimestamp startTS endtimestamp endTS;\n" +
                    "create schema T2 as (startTSOne long, endTSOne long) starttimestamp startTSOne endtimestamp endTSOne;\n";
                env.CompileDeployWBusPublicType(eplT1T2, path);

                TryInvalidCompile(
                    env,
                    path,
                    "create schema T12 as () inherits T1,T2",
                    "Event type declares start timestamp as property 'startTS' however inherited event type 'T2' declares start timestamp as property 'startTSOne'");
                TryInvalidCompile(
                    env,
                    path,
                    "create schema T12 as (startTSOne long, endTSXXX long) inherits T2 starttimestamp startTSOne endtimestamp endTSXXX",
                    "Event type declares end timestamp as property 'endTSXXX' however inherited event type 'T2' declares end timestamp as property 'endTSOne'");

                env.UndeployAll();
            }
        }
    }
} // end of namespace