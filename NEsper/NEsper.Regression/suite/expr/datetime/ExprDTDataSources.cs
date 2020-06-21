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
                "getMinuteOfHour",    // c0
                "getMonthOfYear",     // c1
                "getDayOfMonth",      // c2
                "getDayOfWeek",       // c3
                "getDayOfYear",       // c4
                "getHourOfDay",       // c5
                "getMillisOfSecond",  // c6
                "getSecondOfMinute",  // c7
                "getWeekyear",        // c8
                "getYear"             // c9
                );
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
            sdt.DateTimeEx.SetMillis(3);
            env.SendEventBean(sdt);

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "c0","c1","c2","c3","c4","c5","c6","c7","c8","c9" },
                new object[] {
                    1, 5, 30, 4, 150, 9, 3, 2, 22, 2002
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
                var fields = new [] { "c0", "c1" };

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
                var fields = new [] { "LongDate","DateTime","DateTimeOffset", "DateTimeEx" };
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
                    "2002-05-30T09:01:02.003";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fields = Collections.Array(
                    "valmoh",
                    "valmoy",
                    "valdom",
                    "valdow",
                    "valdoy",
                    "valhod",
                    "valmos",
                    "valsom",
                    "valwye",
                    "valyea",
                    "val0",
                    "val1",
                    "val2",
                    "val3");

                var eplFragment = "@Name('s0') select " +
                                  "current_timestamp.getMinuteOfHour() as valmoh," +
                                  "current_timestamp.getMonthOfYear() as valmoy," +
                                  "current_timestamp.getDayOfMonth() as valdom," +
                                  "current_timestamp.getDayOfWeek() as valdow," +
                                  "current_timestamp.getDayOfYear() as valdoy," +
                                  "current_timestamp.getHourOfDay() as valhod," +
                                  "current_timestamp.getMillisOfSecond()  as valmos," +
                                  "current_timestamp.getSecondOfMinute() as valsom," +
                                  "current_timestamp.getWeekyear() as valwye," +
                                  "current_timestamp.getYear() as valyea," +
                                  "DateTimeEx.getHourOfDay() as val0," +
                                  "DateTimeOffset.getHourOfDay() as val1," +
                                  "DateTime.getHourOfDay() as val2," +
                                  "LongDate.getHourOfDay() as val3" +
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
                        1,    // valmoh
                        5,    // valmoy
                        30,   // valdom
                        4,    // valdow
                        150,  // valdoy
                        9,    // valhod
                        3,    // valmos
                        2,    // valsom
                        22,   // valwye
                        2002, // valyea
                        9,    // val0
                        9,    // val1
                        9,    // val2    
                        9     // val3
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
                    "create schema ParentType as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "create schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeployWBusPublicType(eplMap, path);

                env.CompileDeploy("@Name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                Assert.AreEqual("StartTS", env.Statement("s0").EventType.StartTimestampPropertyName);
                Assert.AreEqual("EndTS", env.Statement("s0").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test Object-array inheritance via create-schema
                path.Clear();
                var eplObjectArray =
                    "create objectarray schema ParentType as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "create objectarray schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeployWBusPublicType(eplObjectArray, path);

                env.CompileDeploy("@Name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                Assert.AreEqual("StartTS", env.Statement("s0").EventType.StartTimestampPropertyName);
                Assert.AreEqual("EndTS", env.Statement("s0").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test PONO inheritance via create-schema
                path.Clear();
                var eplPONO = "create schema InterfaceType as " +
                              typeof(SupportStartTSEndTSInterface).FullName +
                              " starttimestamp StartTS endtimestamp EndTS;\n" +
                              "create schema DerivedType as " +
                              typeof(SupportStartTSEndTSImpl).FullName +
                              " inherits InterfaceType";
                env.CompileDeployWBusPublicType(eplPONO, path);

                var compiled = env.Compile(
                    "@Name('s2') select * from DerivedType dt where dt.before(current_timestamp())",
                    path);
                env.Deploy(compiled);
                Assert.AreEqual("StartTS", env.Statement("s2").EventType.StartTimestampPropertyName);
                Assert.AreEqual("EndTS", env.Statement("s2").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test PONO inheritance via create-schema
                path.Clear();
                String eplXML = "@XMLSchema(RootElementName='root', SchemaText='') " +
                                "@XMLSchemaField(Name='StartTS', XPath='/abc', Type='string', CastToType='long')" +
                                "@XMLSchemaField(Name='EndTS', XPath='/def', Type='string', CastToType='long')" +
                                "create xml schema MyXMLEvent() starttimestamp StartTS endtimestamp EndTS;\n";
                env.CompileDeployWBusPublicType(eplXML, path);

                compiled = env.Compile("@Name('s2') select * from MyXMLEvent dt where dt.before(current_timestamp())", path);

                env.Deploy(compiled);
                Assert.AreEqual("StartTS", env.Statement("s2").EventType.StartTimestampPropertyName);
                Assert.AreEqual("EndTS", env.Statement("s2").EventType.EndTimestampPropertyName);

                env.UndeployAll();

                // test incompatible
                path.Clear();
                var eplT1T2 =
                    "create schema T1 as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "create schema T2 as (StartTSOne long, EndTSOne long) starttimestamp StartTSOne endtimestamp EndTSOne;\n";
                env.CompileDeployWBusPublicType(eplT1T2, path);

                TryInvalidCompile(
                    env,
                    path,
                    "create schema T12 as () inherits T1,T2",
                    "Event type declares start timestamp as property 'StartTS' however inherited event type 'T2' declares start timestamp as property 'StartTSOne'");
                TryInvalidCompile(
                    env,
                    path,
                    "create schema T12 as (StartTSOne long, EndTSXXX long) inherits T2 starttimestamp StartTSOne endtimestamp EndTSXXX",
                    "Event type declares end timestamp as property 'EndTSXXX' however inherited event type 'T2' declares end timestamp as property 'EndTSOne'");

                env.UndeployAll();
            }
        }
    }
} // end of namespace