///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.schedule;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreCast
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreCastDates());
            executions.Add(new ExprCoreCastSimple());
            executions.Add(new ExprCoreCastSimpleMoreTypes());
            executions.Add(new ExprCoreCastAsParse());
            executions.Add(new ExprCoreCastDates());
            executions.Add(new ExprCoreDoubleAndNullOM());
            executions.Add(new ExprCoreCastInterface());
            executions.Add(new ExprCastStringAndNullCompile());
            executions.Add(new ExprCoreCastBoolean());
            executions.Add(new ExprCastWStaticType());
            executions.Add(new ExprCastWArray(false));
            executions.Add(new ExprCastWArray(true));
            return executions;
        }

        private static void RunAssertionDatetimeBaseTypes(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select " +
                      "cast(yyyymmdd,System.DateTimeOffset,dateformat:\"yyyyMMdd\") as c0, " +
                      "cast(yyyymmdd,System.DateTime,dateformat:\"yyyyMMdd\") as c1, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\") as c2, " +
                      "cast(yyyymmdd,System.Int64,dateformat:\"yyyyMMdd\") as c3, " +
                      "cast(yyyymmdd,dateTimeEx,dateformat:\"yyyyMMdd\") as c4, " +
                      "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\") as c5, " +
                      "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\").Get(\"month\") as c6, " +
                      "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\").Get(\"month\") as c7, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\").Get(\"month\") as c8 " +
                      "from MyDateType";
            env.CompileDeploy(soda, epl).AddListener("s0").MilestoneInc(milestone);

            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("yyyymmdd", "20100510");
            env.SendEventMap(values, "MyDateType");

            var formatYYYYMMdd = new SimpleDateFormat("yyyyMMdd");
            var dateYYMMddDate = formatYYYYMMdd.Parse("20100510");
            var dtxYYMMddDate = DateTimeEx.GetInstance(TimeZoneInfo.Local, dateYYMMddDate);

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv(),
                new object[] {
                    dateYYMMddDate.DateTime, // c0
                    dateYYMMddDate.DateTime.DateTime, // c1
                    dateYYMMddDate.TimeInMillis, // c2
                    dateYYMMddDate.TimeInMillis, // c3
                    dtxYYMMddDate, // c4
                    dtxYYMMddDate, // c5
                    4, // c6
                    4, // c7
                    4 // c8
                });

            env.UndeployAll();
        }

        private static void RunAssertionDatetimeVariance(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select " +
                      "cast(yyyymmdd,datetimeoffset,dateformat:\"yyyyMMdd\") as c0, " +
                      "cast(yyyymmdd,System.DateTimeOffset,dateformat:\"yyyyMMdd\") as c1, " +
                      "cast(yyyymmddhhmmss,datetimeoffset,dateformat:\"yyyyMMddHHmmss\") as c2, " +
                      "cast(yyyymmddhhmmss,System.DateTimeOffset,dateformat:\"yyyyMMddHHmmss\") as c3, " +
                      "cast(hhmmss,datetimeoffset,dateformat:\"HHmmss\") as c4, " +
                      "cast(hhmmss,System.DateTimeOffset,dateformat:\"HHmmss\") as c5, " +
                      "cast(yyyymmddhhmmssvv,datetimeoffset,dateformat:\"yyyyMMddHHmmssVV\") as c6, " +
                      "cast(yyyymmddhhmmssvv,DateTimeOffset,dateformat:\"yyyyMMddHHmmssVV\") as c7 " +
                      "from MyDateType";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            var yyyymmdd = "20100510";
            var yyyymmddhhmmss = "20100510141516";
            var yyyymmddhhmmssvv = "20100510141516America/Los_Angeles";
            var hhmmss = "141516";

            // Send an event with all of the "values" - keep in mind, these are just strings so
            // the intent is to have the cast properly parse the values using the given format
            // and then return the correct data type as specified.

            var values = new Dictionary<string, object>();
            values.Put("yyyymmdd", yyyymmdd);
            values.Put("yyyymmddhhmmss", yyyymmddhhmmss);
            values.Put("yyyymmddhhmmssvv", yyyymmddhhmmssvv);
            values.Put("hhmmss", hhmmss);
            env.SendEventMap(values, "MyDateType");

            // Get the most recent value
            var result = env.Listener("S0").AssertOneGetNewAndReset();

            // Checking: yyyymmdd
            var yyyymmddDtx = new SimpleDateFormat("yyyyMMdd").Parse(yyyymmdd);
            Assert.That(result.Get("c0"), Is.EqualTo(yyyymmddDtx.DateTime));
            Assert.That(result.Get("c1"), Is.EqualTo(yyyymmddDtx.DateTime));

            // Checking: yyyymmddhhmmss
            var yyyymmddhhmmssDtx = new SimpleDateFormat("yyyyMMddHHmmss").Parse(yyyymmddhhmmss);
            Assert.That(result.Get("c2"), Is.EqualTo(yyyymmddDtx.DateTime));
            Assert.That(result.Get("c3"), Is.EqualTo(yyyymmddDtx.DateTime));

            // Checking: hhmmss
            var mmhhssDtx = new SimpleDateFormat("HHmmss").Parse(hhmmss);
            Assert.That(result.Get("c4"), Is.EqualTo(mmhhssDtx.DateTime));
            Assert.That(result.Get("c5"), Is.EqualTo(mmhhssDtx.DateTime));

            // Checking: yyyymmddhhmmssvv
            var yyyymmddhhmmssvvDtx = new SimpleDateFormat("yyyyMMddHHmmssVV").Parse(yyyymmddhhmmssvv);
            Assert.That(result.Get("c6"), Is.EqualTo(mmhhssDtx.DateTime));
            Assert.That(result.Get("c7"), Is.EqualTo(mmhhssDtx.DateTime));

            env.UndeployAll();
        }

        private static void RunAssertionDynamicDateFormat(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // try legacy date types
            var epl = "@Name('s0') select " +
                      "cast(a,long,dateformat:b) as c0," +
                      "cast(a,datetime,dateformat:b) as c1," +
                      "cast(a,datetimeoffset,dateformat:b) as c2," +
                      "cast(a,dtx,dateformat:b) as c3" +
                      " from SupportBean_StringAlphabetic";

            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());

            AssertDynamicDateFormat(env, "20100502", "yyyyMMdd");
            AssertDynamicDateFormat(env, "20100502101112", "yyyyMMddhhmmss");
            AssertDynamicDateFormat(env, null, "yyyyMMdd");

            // invalid date
            try {
                env.SendEventBean(new SupportBean_StringAlphabetic("x", "yyyyMMddhhmmss"));
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(
                    ex,
                    "Exception parsing date 'x' format 'yyyyMMddhhmmss': Unparseable date: \"x\"");
            }

            // invalid format
            try {
                env.SendEventBean(new SupportBean_StringAlphabetic("20100502", "UUHHYY"));
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(ex, "Illegal pattern character 'U'");
            }

            env.UndeployAll();
        }

        private static void RunAssertionDatetimeRenderOutCol(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") from MyDateType";
            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());
            Assert.AreEqual(
                "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\")",
                env.Statement("s0").EventType.PropertyNames[0]);
            env.UndeployAll();
        }

        private static void AssertDynamicDateFormat(
            RegressionEnvironment env,
            string date,
            string format)
        {
            env.SendEventBean(new SupportBean_StringAlphabetic(date, format));

            var dateFormat = new SimpleDateFormat(format);
            var expectedDateTimeEx = date == null ? null : dateFormat.Parse(date);
            var expectedDateTimeOffset = expectedDateTimeEx?.DateTime;
            var expectedDateTime = expectedDateTimeOffset?.DateTime;
            var expectedLong = expectedDateTimeEx?.TimeInMillis;

            var result = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.That(result.Get("c0"), Is.EqualTo(expectedDateTimeEx));
            Assert.That(result.Get("c1"), Is.EqualTo(expectedDateTimeOffset));
            Assert.That(result.Get("c2"), Is.EqualTo(expectedDateTime));
            Assert.That(result.Get("c3"), Is.EqualTo(expectedLong));
        }

        private static void RunAssertionConstantDate(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select cast('20030201',date,dateformat:\"yyyyMMdd\") as c0 from SupportBean";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            var dateFormat = new SimpleDateFormat("yyyyMMdd");
            var expectedDate = dateFormat.Parse("20030201");

            env.SendEventBean(new SupportBean("E1", 1));

            var result = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.That(result.Get("c0"), Is.EqualTo(expectedDate));

            env.UndeployAll();
        }

        private static void RunAssertionISO8601Date(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select " +
                      "cast('1997-07-16T19:20:30Z',dtx,dateformat:'iso') as c0," +
                      "cast('1997-07-16T19:20:30+01:00',dtx,dateformat:'iso') as c1," +
                      "cast('1997-07-16T19:20:30',dtx,dateformat:'iso') as c2," +
                      "cast('1997-07-16T19:20:30.45Z',dtx,dateformat:'iso') as c3," +
                      "cast('1997-07-16T19:20:30.45+01:00',dtx,dateformat:'iso') as c4," +
                      "cast('1997-07-16T19:20:30.45',dtx,dateformat:'iso') as c5," +
                      "cast('1997-07-16T19:20:30.45',long,dateformat:'iso') as c6," +
                      "cast('1997-07-16T19:20:30.45',date,dateformat:'iso') as c7," +
                      "cast(TheString,dtx,dateformat:'iso') as c8," +
                      "cast(TheString,long,dateformat:'iso') as c9," +
                      "cast(TheString,date,dateformat:'iso') as c10," +
                      "cast('1997-07-16T19:20:30.45',datetimeoffset,dateformat:'iso') as c11," +
                      "cast('1997-07-16T19:20:30+01:00',datetime,dateformat:'iso') as c12," +
                      "cast('1997-07-16',datetimeoffset,dateformat:'iso') as c13," +
                      "cast('19:20:30',datetimeoffset,dateformat:'iso') as c14" +
                      " from SupportBean";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c0"), 1997, 6, 16, 19, 20, 30, 0, "GMT+00:00");
            SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c1"), 1997, 6, 16, 19, 20, 30, 0, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(
                (DateTimeEx) @event.Get("c2"),
                1997,
                6,
                16,
                19,
                20,
                30,
                0,
                TimeZoneInfo.Local.Id);
            SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c3"), 1997, 6, 16, 19, 20, 30, 450, "GMT+00:00");
            SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c4"), 1997, 6, 16, 19, 20, 30, 450, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(
                (DateTimeEx) @event.Get("c5"),
                1997,
                6,
                16,
                19,
                20,
                30,
                450,
                TimeZoneInfo.Local.Id);

            Assert.That(@event.Get("c6"), Is.InstanceOf<long>());
            Assert.That(@event.Get("c7"), Is.InstanceOf<DateTime>());

            foreach (var prop in new[] {"c8", "c9", "c10"}) {
                Assert.IsNull(@event.Get(prop));
            }

            var isoDateTimeFormat = DateTimeFormat.ISO_DATE_TIME;

            var expectedC11 = DateTimeParsingFunctions.ParseIso8601Ex("1997-07-16T19:20:30.45");
            var expectedC12 = DateTimeParsingFunctions.ParseIso8601Ex("1997-07-16T19:20:30+01:00");
            var expectedC13 = DateTimeParsingFunctions.ParseDefault("1997-07-16");
            var expectedC14 = DateTimeParsingFunctions.ParseDefault("19:20:30");

            Assert.That(@event.Get("c11"), Is.EqualTo(expectedC11));
            Assert.That(@event.Get("c12"), Is.EqualTo(expectedC12));
            Assert.That(@event.Get("c13"), Is.EqualTo(expectedC13));
            Assert.That(@event.Get("c14"), Is.EqualTo(expectedC14));

            env.UndeployAll();
        }

        private static void RunAssertionDateformatNonString(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var sdt = SupportDateTime.Make("2002-05-30T09:00:00.000");
            var sdfDate = sdt.ExDate.DateTime.ToString("s");

            var epl = "@Name('s0') select " +
                      "cast('" +
                      sdfDate +
                      "',dtx,dateformat:SimpleDateFormat.getInstance()) as c0," +
                      "cast('" +
                      sdfDate +
                      "',datetimeoffset,dateformat:SimpleDateFormat.getInstance()) as c1," +
                      "cast('" +
                      sdfDate +
                      "',datetime,dateformat:SimpleDateFormat.getInstance()) as c2," +
                      "cast('" +
                      sdfDate +
                      "',long,dateformat:SimpleDateFormat.getInstance()) as c3" +
                      " from SupportBean";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.That(@event.Get("c0"), Is.EqualTo(sdt.ExDate));
            Assert.That(@event.Get("c1"), Is.EqualTo(sdt.ExDate.DateTime));
            Assert.That(@event.Get("c2"), Is.EqualTo(sdt.ExDate.DateTime.DateTime));
            Assert.That(@event.Get("c3"), Is.EqualTo(sdt.ExDate.TimeInMillis));

            env.UndeployAll();
        }

        private static void RunAssertionDatetimeInvalid(RegressionEnvironment env)
        {
            // not a valid named parameter
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast(TheString, date, x:1) from SupportBean",
                "Failed to validate select-clause expression 'cast(TheString,date,x:1)': Unexpected named parameter 'x', expecting any of the following: [dateformat]");

            // invalid date format
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast(TheString, date, dateformat:'BBBBMMDD') from SupportBean",
                "Failed to validate select-clause expression 'cast(TheString,date,dateformat:\"BBB...(42 chars)': Invalid date format 'BBBBMMDD' (as obtained from new SimpleDateFormat): Illegal pattern character 'B'");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast(TheString, date, dateformat:1) from SupportBean",
                "Failed to validate select-clause expression 'cast(TheString,date,dateformat:1)': Failed to validate named parameter 'dateformat', expected a single expression returning any of the following types: string,DateFormat,DateTimeFormatter");

            // invalid input
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast(IntPrimitive, date, dateformat:'yyyyMMdd') from SupportBean",
                "Failed to validate select-clause expression 'cast(IntPrimitive,date,dateformat:\"...(45 chars)': Use of the 'dateformat' named parameter requires a string-type input");

            // invalid target
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast(TheString, int, dateformat:'yyyyMMdd') from SupportBean",
                "Failed to validate select-clause expression 'cast(TheString,int,dateformat:\"yyyy...(41 chars)': Use of the 'dateformat' named parameter requires a target type of calendar, date, long, localdatetime, localdate, localtime or zoneddatetime");

            // invalid parser
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast('xx', date, dateformat:java.time.format.DateTimeFormatter.ofPattern(\"yyyyMMddHHmmssVV\")) from SupportBean",
                "Failed to validate select-clause expression 'cast(\"xx\",date,dateformat:java.time...(91 chars)': Invalid format, expected string-format or DateFormat but received java.time.format.DateTimeFormatter");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select cast('xx', localdatetime, dateformat:SimpleDateFormat.getInstance()) from SupportBean",
                "Failed to validate select-clause expression 'cast(\"xx\",localdatetime,dateformat:...(66 chars)': Invalid format, expected string-format or DateTimeFormatter but received java.text.DateFormat");
        }

        private static void AssertResults(
            EventBean theEvent,
            object[] result)
        {
            for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }

        private static void AssertTypes(
            EPStatement stmt,
            string[] fields,
            params Type[] types)
        {
            for (var i = 0; i < fields.Length; i++) {
                Assert.AreEqual(types[i], stmt.EventType.GetPropertyType(fields[i]), "failed for " + i);
            }
        }

        internal class ExprCastWArray : RegressionExecution
        {
            private readonly bool soda;

            public ExprCastWArray(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create schema MyEvent(arr_string System.Object, arr_primitive System.Object, " +
                    "arr_boxed_one System.Object, arr_boxed_two System.Object, arr_object System.Object," +
                    "arr_2dim_primitive System.Object, arr_2dim_object System.Object," +
                    "arr_3dim_primitive System.Object, arr_3dim_object System.Object" +
                    ");\n" +
                    "create schema MyArrayEvent as " +
                    typeof(MyArrayEvent).Name +
                    ";\n";
                env.CompileDeployWBusPublicType(epl, path);

                var insert = "@Name('s0') insert into MyArrayEvent select " +
                             "cast(arr_string, string[]) as c0, " +
                             "cast(arr_primitive, int[primitive]) as c1, " +
                             "cast(arr_boxed_one, int[]) as c2, " +
                             "cast(arr_boxed_two, System.int[]) as c3, " +
                             "cast(arr_object, System.Object[]) as c4," +
                             "cast(arr_2dim_primitive, int[primitive][]) as c5," +
                             "cast(arr_2dim_object, System.Object[][]) as c6," +
                             "cast(arr_3dim_primitive, int[primitive][][]) as c7," +
                             "cast(arr_3dim_object, System.Object[][][]) as c8 " +
                             "from MyEvent";
                env.CompileDeploy(soda, insert, path);

                var stmt = env.AddListener("s0").Statement("s0");
                var eventType = stmt.EventType;
                Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("c0"));
                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("c1"));
                Assert.AreEqual(typeof(int?[]), eventType.GetPropertyType("c2"));
                Assert.AreEqual(typeof(int?[]), eventType.GetPropertyType("c3"));
                Assert.AreEqual(typeof(object[]), eventType.GetPropertyType("c4"));
                Assert.AreEqual(typeof(int[][]), eventType.GetPropertyType("c5"));
                Assert.AreEqual(typeof(object[][]), eventType.GetPropertyType("c6"));
                Assert.AreEqual(typeof(int[][][]), eventType.GetPropertyType("c7"));
                Assert.AreEqual(typeof(object[][][]), eventType.GetPropertyType("c8"));

                IDictionary<string, object> map = new Dictionary<string, object>();
                map.Put("arr_string", new[] {"a"});
                map.Put("arr_primitive", new[] {1});
                map.Put("arr_boxed_one", new int?[] {2});
                map.Put("arr_boxed_two", new int?[] {3});
                map.Put("arr_object", new[] {new SupportBean("E1", 0)});
                map.Put("arr_2dim_primitive", new[] {new[] {10}});
                map.Put("arr_2dim_object", new[] {new int?[] {11}});
                map.Put("arr_3dim_primitive", new[] {new[] {new[] {12}}});
                map.Put("arr_3dim_object", new[] {new[] {new int?[] {13}}});

                env.SendEventMap(map, "MyEvent");

                var mae = (MyArrayEvent) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("a", mae.C0[0]);
                Assert.AreEqual(1, mae.C1[0]);
                Assert.AreEqual(2, mae.C2[0].AsInt());
                Assert.AreEqual(3, mae.C3[0].AsInt());
                Assert.AreEqual(new SupportBean("E1", 0), mae.C4[0]);
                Assert.AreEqual(10, mae.C5[0][0]);
                Assert.AreEqual(11, mae.C6[0][0]);
                Assert.AreEqual(12, mae.C7[0][0][0]);
                Assert.AreEqual(13, mae.C8[0][0][0]);

                env.SendEventMap(Collections.EmptyDataMap, "MyEvent");

                env.UndeployAll();
            }
        }

        internal class ExprCastWStaticType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "@Name('s0') select " +
                           "cast(anInt, int) as intVal, " +
                           "cast(anDouble, double) as doubleVal, " +
                           "cast(anLong, long) as longVal, " +
                           "cast(anFloat, float) as floatVal, " +
                           "cast(anByte, byte) as byteVal, " +
                           "cast(anShort, short) as shortVal, " +
                           "cast(IntPrimitive, int) as intOne, " +
                           "cast(IntBoxed, int) as intTwo, " +
                           "cast(IntPrimitive, System.Long) as longOne, " +
                           "cast(IntBoxed, long) as longTwo " +
                           "from StaticTypeMapEvent";

                env.CompileDeploy(stmt).AddListener("s0");

                IDictionary<string, object> map = new Dictionary<string, object>();
                map.Put("anInt", "100");
                map.Put("anDouble", "1.4E-1");
                map.Put("anLong", "-10");
                map.Put("anFloat", "1.001");
                map.Put("anByte", "0x0A");
                map.Put("anShort", "223");
                map.Put("IntPrimitive", 10);
                map.Put("IntBoxed", 11);

                env.SendEventMap(map, "StaticTypeMapEvent");
                var row = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(100, row.Get("intVal"));
                Assert.AreEqual(0.14d, row.Get("doubleVal"));
                Assert.AreEqual(-10L, row.Get("longVal"));
                Assert.AreEqual(1.001f, row.Get("floatVal"));
                Assert.AreEqual((byte) 10, row.Get("byteVal"));
                Assert.AreEqual((short) 223, row.Get("shortVal"));
                Assert.AreEqual(10, row.Get("intOne"));
                Assert.AreEqual(11, row.Get("intTwo"));
                Assert.AreEqual(10L, row.Get("longOne"));
                Assert.AreEqual(11L, row.Get("longTwo"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCastSimpleMoreTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv();
                var epl = "@Name('s0') select " +
                          "cast(IntPrimitive, float) as c0," +
                          "cast(IntPrimitive, short) as c1," +
                          "cast(IntPrimitive, byte) as c2," +
                          "cast(TheString, char) as c3," +
                          "cast(TheString, boolean) as c4," +
                          "cast(IntPrimitive, BigInteger) as c5," +
                          "cast(IntPrimitive, BigDecimal) as c6," +
                          "cast(DoublePrimitive, BigDecimal) as c7," +
                          "cast(TheString, char) as c8" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(float?),
                    typeof(short?),
                    typeof(byte?),
                    typeof(char?),
                    typeof(bool?),
                    typeof(BigInteger),
                    typeof(decimal),
                    typeof(decimal),
                    typeof(char?));

                var bean = new SupportBean("true", 1);
                bean.DoublePrimitive = 1;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        1.0f,
                        (short) 1,
                        (byte) 1,
                        't',
                        true,
                        BigInteger.One,
                        1.0m,
                        1.0m, 't'
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCoreCastSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select cast(TheString as string) as t0, " +
                          " cast(IntBoxed, int) as t1, " +
                          " cast(FloatBoxed, System.Single) as t2, " +
                          " cast(TheString, System.String) as t3, " +
                          " cast(IntPrimitive, System.Integer) as t4, " +
                          " cast(IntPrimitive, long) as t5, " +
                          " cast(FloatBoxed, long) as t7 " +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(string), type.GetPropertyType("t0"));
                Assert.AreEqual(typeof(int?), type.GetPropertyType("t1"));
                Assert.AreEqual(typeof(float?), type.GetPropertyType("t2"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("t3"));
                Assert.AreEqual(typeof(int?), type.GetPropertyType("t4"));
                Assert.AreEqual(typeof(long?), type.GetPropertyType("t5"));
                Assert.AreEqual(typeof(long?), type.GetPropertyType("t7"));

                var bean = new SupportBean("abc", 100);
                bean.FloatBoxed = 9.5f;
                bean.IntBoxed = 3;
                env.SendEventBean(bean);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {"abc", 3, 9.5f, "abc", 100, 100L, 100, 9L});

                bean = new SupportBean(null, 100);
                bean.FloatBoxed = null;
                bean.IntBoxed = null;
                env.SendEventBean(bean);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {null, null, null, null, 100, 100L, 100, null});
                bean = new SupportBean(null, 100);
                bean.FloatBoxed = null;
                bean.IntBoxed = null;
                env.SendEventBean(bean);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {null, null, null, null, 100, 100L, 100, null});

                env.UndeployAll();

                // test cast with chained and null
                epl = "@Name('s0') select cast(one as " +
                      typeof(SupportBean).Name +
                      ").getTheString() as t0," +
                      "cast(null, " +
                      typeof(SupportBean).Name +
                      ") as t1" +
                      " from SupportBeanObject";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanObject(new SupportBean("E1", 1)));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "t0,t1".SplitCsv(),
                    new object[] {"E1", null});
                Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("t1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreDoubleAndNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select cast(item?,double) as t0 from SupportBeanDynRoot";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.Cast("item?", "double"), "t0");
                model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBeanDynRoot).Name));
                model = env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("t0"));

                env.SendEventBean(new SupportBeanDynRoot(100));
                Assert.AreEqual(100d, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot((byte) 2));
                Assert.AreEqual(2d, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(77.7777));
                Assert.AreEqual(77.7777d, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(6L));
                Assert.AreEqual(6d, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(null));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot("abc"));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCastDates : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                RunAssertionDatetimeBaseTypes(env, true, milestone);

                RunAssertionDatetimeVariance(env, milestone);

                RunAssertionDatetimeRenderOutCol(env, milestone);

                RunAssertionDynamicDateFormat(env, milestone);

                RunAssertionConstantDate(env, milestone);

                RunAssertionISO8601Date(env, milestone);

                RunAssertionDateformatNonString(env, milestone);

                RunAssertionDatetimeInvalid(env);
            }
        }

        internal class ExprCoreCastAsParse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select cast(TheString, int) as t0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("t0"));

                env.SendEventBean(new SupportBean("12", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "t0".SplitCsv(),
                    new object[] {12});

                env.UndeployAll();
            }
        }

        internal class ExprCoreCastInterface : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select cast(item?, " +
                          typeof(SupportMarkerInterface).FullName +
                          ") as t0, " +
                          " cast(item?, " +
                          typeof(ISupportA).FullName +
                          ") as t1, " +
                          " cast(item?, " +
                          typeof(ISupportBaseAB).FullName +
                          ") as t2, " +
                          " cast(item?, " +
                          typeof(ISupportBaseABImpl).FullName +
                          ") as t3, " +
                          " cast(item?, " +
                          typeof(ISupportC).FullName +
                          ") as t4, " +
                          " cast(item?, " +
                          typeof(ISupportD).FullName +
                          ") as t5, " +
                          " cast(item?, " +
                          typeof(ISupportAImplSuperG).FullName +
                          ") as t6, " +
                          " cast(item?, " +
                          typeof(ISupportAImplSuperGImplPlus).FullName +
                          ") as t7 " +
                          " from SupportBeanDynRoot";

                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportMarkerInterface), type.GetPropertyType("t0"));
                Assert.AreEqual(typeof(ISupportA), type.GetPropertyType("t1"));
                Assert.AreEqual(typeof(ISupportBaseAB), type.GetPropertyType("t2"));
                Assert.AreEqual(typeof(ISupportBaseABImpl), type.GetPropertyType("t3"));
                Assert.AreEqual(typeof(ISupportC), type.GetPropertyType("t4"));
                Assert.AreEqual(typeof(ISupportD), type.GetPropertyType("t5"));
                Assert.AreEqual(typeof(ISupportAImplSuperG), type.GetPropertyType("t6"));
                Assert.AreEqual(typeof(ISupportAImplSuperGImplPlus), type.GetPropertyType("t7"));

                object bean = new SupportBeanDynRoot("abc");
                env.SendEventBean(new SupportBeanDynRoot(bean));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new[] {bean, null, null, null, null, null, null, null});

                bean = new ISupportDImpl("", "", "");
                env.SendEventBean(new SupportBeanDynRoot(bean));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new[] {null, null, null, null, null, bean, null, null});

                bean = new ISupportBCImpl("", "", "");
                env.SendEventBean(new SupportBeanDynRoot(bean));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new[] {null, null, bean, null, bean, null, null, null});

                bean = new ISupportAImplSuperGImplPlus();
                env.SendEventBean(new SupportBeanDynRoot(bean));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new[] {null, bean, bean, null, bean, null, bean, bean});

                bean = new ISupportBaseABImpl("");
                env.SendEventBean(new SupportBeanDynRoot(bean));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new[] {null, null, bean, bean, null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ExprCastStringAndNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select cast(item?,System.String) as t0 from SupportBeanDynRoot";

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("t0"));

                env.SendEventBean(new SupportBeanDynRoot(100));
                Assert.AreEqual("100", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot((byte) 2));
                Assert.AreEqual("2", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(77.7777));
                Assert.AreEqual("77.7777", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(6L));
                Assert.AreEqual("6", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot(null));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new SupportBeanDynRoot("abc"));
                Assert.AreEqual("abc", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCastBoolean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select cast(BoolPrimitive as System.Boolean) as t0, " +
                          " cast(BoolBoxed | BoolPrimitive, boolean) as t1, " +
                          " cast(BoolBoxed, string) as t2 " +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(bool?), type.GetPropertyType("t0"));
                Assert.AreEqual(typeof(bool?), type.GetPropertyType("t1"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("t2"));

                var bean = new SupportBean("abc", 100);
                bean.BoolPrimitive = true;
                bean.BoolBoxed = true;
                env.SendEventBean(bean);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {true, true, "true"});

                bean = new SupportBean(null, 100);
                bean.BoolPrimitive = false;
                bean.BoolBoxed = false;
                env.SendEventBean(bean);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {false, false, "false"});

                bean = new SupportBean(null, 100);
                bean.BoolPrimitive = true;
                bean.BoolBoxed = null;
                env.SendEventBean(bean);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                AssertResults(
                    theEvent,
                    new object[] {true, null, null});

                env.UndeployAll();
            }
        }

        public class MyArrayEvent
        {
            public MyArrayEvent(
                string[] c0,
                int[] c1,
                int?[] c2,
                int?[] c3,
                object[] c4,
                int[][] c5,
                object[][] c6,
                int[][][] c7,
                object[][][] c8)
            {
                C0 = c0;
                C1 = c1;
                C2 = c2;
                C3 = c3;
                C4 = c4;
                C5 = c5;
                C6 = c6;
                C7 = c7;
                C8 = c8;
            }

            public string[] C0 { get; }

            public int[] C1 { get; }

            public int?[] C2 { get; }

            public int?[] C3 { get; }

            public object[] C4 { get; }

            public int[][] C5 { get; }

            public object[][] C6 { get; }

            public int[][][] C7 { get; }

            public object[][][] C8 { get; }
        }
    }
} // end of namespace