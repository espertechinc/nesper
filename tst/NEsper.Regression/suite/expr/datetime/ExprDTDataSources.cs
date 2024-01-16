///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTDataSources
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStartEndTS(execs);
            WithFieldWValue(execs);
            WithAllCombinations(execs);
            WithMinMax(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTDataSourcesMinMax());
            return execs;
        }

        public static IList<RegressionExecution> WithAllCombinations(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTDataSourcesAllCombinations());
            return execs;
        }

        public static IList<RegressionExecution> WithFieldWValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTDataSourcesFieldWValue());
            return execs;
        }

        public static IList<RegressionExecution> WithStartEndTS(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTDataSourcesStartEndTS());
            return execs;
        }

        private class ExprDTDataSourcesMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "min(LongPrimitive).before(max(LongBoxed), 1 second) as c0," +
                          "min(LongPrimitive, LongBoxed).before(20000L, 1 second) as c1" +
                          " from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "c0,c1".SplitCsv();

                SendBean(env, 20000, 20000);
                env.AssertPropsNew("s0", fields, new object[] { false, false });

                SendBean(env, 19000, 20000);
                env.AssertPropsNew("s0", fields, new object[] { true, true });

                env.UndeployAll();
            }
        }

        internal class ExprDTDataSourcesAllCombinations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "LongDate", "DateTime", "DateTimeOffset", "DateTimeEx" };
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
                var startTime = "2002-05-30T09:01:02.003";
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

                var eplFragment = "@name('s0') select " +
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
                env.AssertStatement(
                    "s0",
                    statement => {
                        foreach (var field in fields) {
                            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType(field));
                        }
                    });

                env.SendEventBean(SupportDateTime.Make(startTime));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        1, // valmoh
                        5, // valmoy
                        30, // valdom
                        4, // valdow
                        150, // valdoy
                        9, // valhod
                        3, // valmos
                        2, // valsom
                        22, // valwye
                        2002, // valyea
                        9, // val0
                        9, // val1
                        9, // val2    
                        9 // val3
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
                    "@buseventtype @public create schema ParentType as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "@buseventtype @public create schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeploy(eplMap, path);

                env.CompileDeploy("@name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual("StartTS", statement.EventType.StartTimestampPropertyName);
                        Assert.AreEqual("EndTS", statement.EventType.EndTimestampPropertyName);
                    });

                env.UndeployAll();

                // test Object-array inheritance via create-schema
                path.Clear();
                var eplObjectArray =
                    "@buseventtype @public create objectarray schema ParentType as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "@buseventtype @public create objectarray schema ChildType as (foo string) inherits ParentType;\n";
                env.CompileDeploy(eplObjectArray, path);

                env.CompileDeploy("@name('s0') select * from ChildType dt where dt.before(current_timestamp())", path);
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual("StartTS", statement.EventType.StartTimestampPropertyName);
                        Assert.AreEqual("EndTS", statement.EventType.EndTimestampPropertyName);
                    });

                env.UndeployAll();

                // test PONO inheritance via create-schema
                path.Clear();
                var eplPONO =
                    $"@public @buseventtype create schema InterfaceType as {typeof(SupportStartTSEndTSInterface).FullName} starttimestamp StartTS endtimestamp EndTS;\n" +
                    $"@public @buseventtype create schema DerivedType as {typeof(SupportStartTSEndTSImpl).FullName} inherits InterfaceType";
                env.CompileDeploy(eplPONO, path);

                var compiled = env.Compile(
                    "@name('s2') select * from DerivedType dt where dt.before(current_timestamp())",
                    path);
                env.Deploy(compiled);
                env.AssertStatement(
                    "s2",
                    statement => {
                        Assert.AreEqual("StartTS", statement.EventType.StartTimestampPropertyName);
                        Assert.AreEqual("EndTS", statement.EventType.EndTimestampPropertyName);
                    });

                env.UndeployAll();

                // test PONO inheritance via create-schema
                path.Clear();
                String eplXML = "@XMLSchema(RootElementName='root', SchemaText='') " +
                                "@XMLSchemaField(Name='StartTS', XPath='/abc', Type='string', CastToType='long')" +
                                "@XMLSchemaField(Name='EndTS', XPath='/def', Type='string', CastToType='long')" +
                                "@public @buseventtype create xml schema MyXMLEvent() starttimestamp StartTS endtimestamp EndTS;\n";
                env.CompileDeploy(eplXML, path);

                compiled = env.Compile(
                    "@name('s2') select * from MyXMLEvent dt where dt.before(current_timestamp())",
                    path);
                env.Deploy(compiled);
                env.AssertStatement(
                    "s2",
                    statement => {
                        Assert.AreEqual("StartTS", statement.EventType.StartTimestampPropertyName);
                        Assert.AreEqual("EndTS", statement.EventType.EndTimestampPropertyName);
                    });

                env.UndeployAll();

                // test incompatible
                path.Clear();
                var eplT1T2 =
                    "@public @buseventtype create schema T1 as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS;\n" +
                    "@public @buseventtype create schema T2 as (StartTSOne long, EndTSOne long) starttimestamp StartTSOne endtimestamp EndTSOne;\n";
                env.CompileDeploy(eplT1T2, path);

                env.TryInvalidCompile(
                    path,
                    "create schema T12 as () inherits T1,T2",
                    "Event type declares start timestamp as property 'StartTS' however inherited event type 'T2' declares start timestamp as property 'StartTSOne'");
                env.TryInvalidCompile(
                    path,
                    "create schema T12 as (StartTSOne long, EndTSXXX long) inherits T2 starttimestamp StartTSOne endtimestamp EndTSXXX",
                    "Event type declares end timestamp as property 'EndTSXXX' however inherited event type 'T2' declares end timestamp as property 'EndTSOne'");
                env.TryInvalidCompile(
                    path,
                    "create schema T12 as (StartTSOne null, EndTSXXX long) starttimestamp StartTSOne endtimestamp EndTSXXX",
                    "Declared start timestamp property 'StartTSOne' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns 'System.Object'");

                env.UndeployAll();
            }
        }

        private static void RunAssertionAllCombinations(
            RegressionEnvironment env,
            string field,
            AtomicLong milestone)
        {
            var methods = new string[] {
                "getMinuteOfHour",
                "getMonthOfYear", 
                "getDayOfMonth",
                "getDayOfWeek", 
                "getDayOfYear",
                "getHourOfDay",
                "getMillisOfSecond",
                "getSecondOfMinute",
                "getWeekYear",
                "getYear"
            };
            var epl = new StringWriter();
            epl.Write("@name('s0') select ");
            var count = 0;
            var delimiter = "";
            foreach (var method in methods) {
                epl.Write(delimiter);
                epl.Write(field);
                epl.Write(".");
                epl.Write(method);
                epl.Write("() ");
                epl.Write("c");
                epl.Write(Convert.ToString(count++));
                delimiter = ",";
            }

            epl.Write(" from SupportDateTime");

            env.CompileDeployAddListenerMile(epl.ToString(), "s0", milestone.GetAndIncrement());

            var sdt = SupportDateTime.Make("2002-05-30T09:01:02.003");
            sdt.DateTimeEx.SetMillis(3);
            env.SendEventBean(sdt);

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
    }
} // end of namespace