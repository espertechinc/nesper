///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherStaticFunctions
    {
        private const string STREAM_MDB_LEN5 = " from SupportMarketDataBean#length(5) ";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherNullPrimitive());
            execs.Add(new EPLOtherChainedInstance());
            execs.Add(new EPLOtherChainedStatic());
            execs.Add(new EPLOtherEscape());
            execs.Add(new EPLOtherReturnsMapIndexProperty());
            execs.Add(new EPLOtherPattern());
            execs.Add(new EPLOtherException());
            execs.Add(new EPLOtherArrayParameter());
            execs.Add(new EPLOtherNoParameters());
            execs.Add(new EPLOtherPerfConstantParameters());
            execs.Add(new EPLOtherPerfConstantParametersNested());
            execs.Add(new EPLOtherSingleParameterOM());
            execs.Add(new EPLOtherSingleParameterCompile());
            execs.Add(new EPLOtherSingleParameter());
            execs.Add(new EPLOtherTwoParameters());
            execs.Add(new EPLOtherUserDefined());
            execs.Add(new EPLOtherComplexParameters());
            execs.Add(new EPLOtherMultipleMethodInvocations());
            execs.Add(new EPLOtherOtherClauses());
            execs.Add(new EPLOtherNestedFunction());
            execs.Add(new EPLOtherPassthru());
            execs.Add(new EPLOtherPrimitiveConversion());
            execs.Add(new EPLOtherStaticFuncWCurrentTimeStamp());
            execs.Add(new EPLOtherStaticFuncEnumConstant());
            return execs;
        }

        private static object GetProperty(
            RegressionEnvironment env,
            string propertyName)
        {
            var newData = env.Listener("s0").GetAndResetLastNewData();
            if (newData == null || newData.Length == 0) {
                return null;
            }

            return newData[0].Get(propertyName);
        }

        private static object[] AssertStatementAndGetProperty(
            RegressionEnvironment env,
            bool expectResult,
            params string[] propertyNames)
        {
            if (propertyNames == null) {
                Assert.Fail("no prop names");
            }

            SendEvent(env, "IBM", 10d, 4L);

            if (expectResult) {
                IList<object> properties = new List<object>();
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                foreach (var propertyName in propertyNames) {
                    properties.Add(theEvent.Get(propertyName));
                }

                return properties.ToArray();
            }

            return null;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean(symbol, price, volume, ""));
        }

        internal class EPLOtherPrimitiveConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var primitiveConversionLib = typeof(PrimitiveConversionLib).Name;
                env.CompileDeploy(
                        "@name('s0') select " +
                        $"{primitiveConversionLib}.PassIntAsObject(IntPrimitive) as c0," +
                        $"{primitiveConversionLib}.PassIntAsNumber(IntPrimitive) as c1," +
                        $"{primitiveConversionLib}.PassIntAsComparable(IntPrimitive) as c2" +
                        " from SupportBean")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1", "c2" },
                    new object[] {10, 10, 10});

                env.UndeployAll();
            }
        }

        // TBD: New Java Tests
        internal class EPLOtherStaticFuncEnumConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "create map schema MyEvent(someDate Date, dateFrom string, dateTo string, minutesOfDayFrom int, minutesOfDayTo int, daysOfWeek string);\n" +
                    "select " +
                    "java.time.ZonedDateTime.ofInstant(someDate.toInstant(),java.time.ZoneId.of('CET')).isAfter(cast(dateFrom||'T00:00:00Z', zoneddatetime, dateformat:java.time.format.DateTimeFormatter.ISO_OFFSET_DATE_TIME.withZone(java.time.ZoneId.of('CET')))) as c0,\n" +
                    "java.time.ZonedDateTime.ofInstant(someDate.toInstant(),java.time.ZoneId.of('CET')).isBefore(cast(dateTo||'T00:00:00Z', zoneddatetime, dateformat:java.time.format.DateTimeFormatter.ISO_OFFSET_DATE_TIME.withZone(java.time.ZoneId.of('CET')))) as c1,\n" +
                    "java.time.ZonedDateTime.ofInstant(someDate.toInstant(),java.time.ZoneId.of('CET')).get(java.time.temporal.ChronoField.MINUTE_OF_DAY)>= minutesOfDayFrom as c2,\n" +
                    "java.time.ZonedDateTime.ofInstant(someDate.toInstant(),java.time.ZoneId.of('CET')).get(java.time.temporal.ChronoField.MINUTE_OF_DAY)<= minutesOfDayTo as c3,\n" +
                    "daysOfWeek.contains(System.String.valueOf(java.time.ZonedDateTime.ofInstant(someDate.toInstant(),java.time.ZoneId.of('CET')).getDayOfWeek().getValue())) as c4\n" +
                    "from MyEvent";
                env.Compile(epl);
            }
        }

        internal class EPLOtherStaticFuncWCurrentTimeStamp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);
                string epl =
                    "@name('s0') select java.time.format.DateTimeFormatter.ISO_INSTANT.format(java.time.Instant.ofEpochMilli(current_timestamp())) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.AreEqual("1970-01-01T00:00:01Z", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherNullPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test passing null
                env.CompileDeploy("@name('s0') select NullPrimitive.GetValue(IntBoxed) from SupportBean")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean());

                env.UndeployAll();
            }
        }

        internal class EPLOtherChainedInstance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@name('s0') select " +
                        "LevelZero.GetLevelOne().GetLevelTwoValue() as val0 " +
                        "from SupportBean")
                    .AddListener("s0");

                LevelOne.Field = "v1";
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "val0" },
                    new object[] {"v1"});

                LevelOne.Field = "v2";
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "val0" },
                    new object[] {"v2"});

                env.UndeployAll();
            }
        }

        internal class EPLOtherChainedStatic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subexp = "SupportChainTop.Make().GetChildOne(\"abc\",1).GetChildTwo(\"def\").GetText()";
                var statementText = "@name('s0') select " + subexp + " from SupportBean";
                env.CompileDeploy(statementText).AddListener("s0");

                object[][] rows = {
                    new object[] {subexp, typeof(string)}
                };
                var prop = env.Statement("s0").EventType.PropertyDescriptors;
                for (var i = 0; i < rows.Length; i++) {
                    Assert.AreEqual(rows[i][0], prop[i].PropertyName);
                    Assert.AreEqual(rows[i][1], prop[i].PropertyType);
                }

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    new[] {subexp},
                    new object[] {SupportChainTop.Make().GetChildOne("abc", 1).GetChildTwo("def").Text});

                env.UndeployAll();
            }
        }

        internal class EPLOtherEscape : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select SupportStaticMethodLib.`Join`(abcstream) as value from SupportBean abcstream";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 99));

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    new [] { "value" },
                    new object[] {"E1 99"});

                env.UndeployAll();
            }
        }

        internal class EPLOtherReturnsMapIndexProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var statementText =
                    "insert into ABCStream select SupportStaticMethodLib.MyMapFunc() as mymap, SupportStaticMethodLib.MyArrayFunc() as myindex from SupportBean";
                env.CompileDeploy(statementText, path);

                statementText = "@name('s0') select mymap('A') as v0, myindex[1] as v1 from ABCStream";
                env.CompileDeploy(statementText, path).AddListener("s0");

                env.SendEventBean(new SupportBean());

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    new [] { "v0","v1" },
                    new object[] {"A1", 200});

                env.UndeployAll();
            }
        }

        internal class EPLOtherPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).Name;
                var statementText = "@name('s0') select * from pattern [Myevent=SupportBean(" +
                                    className +
                                    ".DelimitPipe(TheString) = '|a|')]";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean("b", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("a", 0));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
                statementText = "@name('s0') select * from pattern [Myevent=SupportBean(" +
                                className +
                                ".DelimitPipe(null) = '|<null>|')]";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean("a", 0));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLOtherException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).Name;
                var statementText = "@name('s0') select Price, " +
                                    className +
                                    ".ThrowException() as value " +
                                    STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "IBM", 10d, 4L);
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherArrayParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select " +
                           "SupportStaticMethodLib.ArraySumIntBoxed({1,2,null,3,4}) as v1, " +
                           "SupportStaticMethodLib.ArraySumDouble({1,2,3,4.0}) as v2, " +
                           "SupportStaticMethodLib.ArraySumString({'1','2','3','4'}) as v3, " +
                           "SupportStaticMethodLib.ArraySumObject({'1',2,3.0,'4.0'}) as v4 " +
                           " from SupportBean";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "v1","v2","v3","v4" },
                    new object[] {10, 10d, 10d, 10d});

                env.UndeployAll();
            }
        }

        internal class EPLOtherNoParameters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var dateTimeHelper = typeof(DateTimeHelper).FullName;
                
                long? startTime = DateTimeHelper.GetCurrentTimeMillis();
                var statementText = $"@name('s0') select {dateTimeHelper}.GetCurrentTimeMillis() " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("IBM", 10d, 4L, ""));
                var result = GetProperty(env, $"{dateTimeHelper}.GetCurrentTimeMillis()").AsInt64();
                long? finishTime = DateTimeHelper.CurrentTimeMillis;
                Assert.IsTrue(startTime <= result);
                Assert.IsTrue(result <= finishTime);
                env.UndeployAll();

                //statementText = "@name('s0') select System.ClassLoader.getSystemClassLoader() " + STREAM_MDB_LEN5;
                //env.CompileDeploy(statementText).AddListener("s0");

                env.UndeployAll();

                TryInvalidCompile(
                    env,
                    "select UnknownClass.InvalidMethod() " + STREAM_MDB_LEN5,
                    "Failed to validate select-clause expression 'UnknownClass.InvalidMethod()': Failed to resolve 'UnknownClass.InvalidMethod' to a property, single-row function, aggregation function, script, stream or class name ");
            }
        }

        internal class EPLOtherSingleParameterOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.StaticMethod<BitWriter>("Write", 7), "value");
                model.FromClause = FromClause.Create(
                    FilterStream.Create("SupportMarketDataBean").AddView("length", Expressions.Constant(5)));
                model = env.CopyMayFail(model);
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                var bitWriter = typeof(BitWriter).CleanName();
                var statementText = $"@name('s0') select {bitWriter}.Write(7) as value" + STREAM_MDB_LEN5;

                Assert.AreEqual(statementText.Trim(), model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                SendEvent(env, "IBM", 10d, 4L);
                Assert.AreEqual(BitWriter.Write(7), env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherSingleParameterCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bitWriter = typeof(BitWriter).CleanName();
                var statementText = $"@name('s0') select {bitWriter}.Write(7) as value" + STREAM_MDB_LEN5;
                env.EplToModelCompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "IBM", 10d, 4L);
                Assert.AreEqual(BitWriter.Write(7), env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherSingleParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bitWriter = typeof(BitWriter).CleanName();
                var statementText = $"@name('s0') select {bitWriter}.Write(7) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                var result = AssertStatementAndGetProperty(env, true, $"{bitWriter}.Write(7)");
                Assert.AreEqual(BitWriter.Write(7), result[0]);
                env.UndeployAll();

                statementText = "@name('s0') select Convert.ToInt32(\"6\") " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                result = AssertStatementAndGetProperty(env, true, "Convert.ToInt32(\"6\")");
                Assert.AreEqual(Convert.ToInt32("6"), result[0]);
                env.UndeployAll();

                statementText = "@name('s0') select System.Convert.ToString(\'a\') " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                result = AssertStatementAndGetProperty(env, true, "System.Convert.ToString(\"a\")");
                Assert.AreEqual(Convert.ToString('a'), result[0]);
                env.UndeployAll();
            }
        }

        internal class EPLOtherTwoParameters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select Math.Max(2,3) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                Assert.AreEqual(3, AssertStatementAndGetProperty(env, true, "Math.Max(2,3)")[0]);
                env.UndeployAll();

                statementText = "@name('s0') select System.Math.Max(2,3d) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                Assert.AreEqual(3d, AssertStatementAndGetProperty(env, true, "System.Math.Max(2,3.0d)")[0]);
                env.UndeployAll();

                statementText = "@name('s0') select Convert.ToInt64(\"123\")" + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                object expected = Convert.ToInt64("123");
                Assert.AreEqual(expected, AssertStatementAndGetProperty(env, true, "Convert.ToInt64(\"123\")")[0]);
                env.UndeployAll();
            }
        }

        internal class EPLOtherUserDefined : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).FullName;
                var statementText = $"@name('s0') select {className}.StaticMethod(2){STREAM_MDB_LEN5}";
                env.CompileDeploy(statementText).AddListener("s0");

                Assert.AreEqual(2, AssertStatementAndGetProperty(env, true, $"{className}.StaticMethod(2)")[0]);
                env.UndeployAll();

                // try context passed
                SupportStaticMethodLib.MethodInvocationContexts.Clear();
                statementText = $"@name('s0') select {className}.StaticMethodWithContext(2){STREAM_MDB_LEN5}";
                env.CompileDeploy(statementText).AddListener("s0");

                Assert.AreEqual(
                    2,
                    AssertStatementAndGetProperty(env, true, $"{className}.StaticMethodWithContext(2)")[0]);
                var first = SupportStaticMethodLib.MethodInvocationContexts[0];
                Assert.AreEqual("s0", first.StatementName);
                Assert.AreEqual(env.RuntimeURI, first.RuntimeURI);
                Assert.AreEqual(-1, first.ContextPartitionId);
                Assert.AreEqual("StaticMethodWithContext", first.FunctionName);
                env.UndeployAll();
            }
        }

        internal class EPLOtherComplexParameters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select Convert.ToString(Price) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                var result = AssertStatementAndGetProperty(env, true, "Convert.ToString(Price)");
                Assert.AreEqual(Convert.ToString(10d), result[0]);
                env.UndeployAll();

                statementText = "@name('s0') select Convert.ToString(2 + 3*5) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                result = AssertStatementAndGetProperty(env, true, "Convert.ToString(2+3*5)");
                Assert.AreEqual(Convert.ToString(17), result[0]);
                env.UndeployAll();

                statementText = "@name('s0') select Convert.ToString(Price*Volume +Volume) " + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                result = AssertStatementAndGetProperty(env, true, "Convert.ToString(Price*Volume+Volume)");
                Assert.AreEqual(Convert.ToString(44d), result[0]);
                env.UndeployAll();

                statementText = "@name('s0') select Convert.ToString(Math.Pow(Price,Convert.ToInt32(\"2\"))) " +
                                STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                result = AssertStatementAndGetProperty(
                    env,
                    true,
                    "Convert.ToString(Math.Pow(Price,Convert.ToInt32(\"2\")))");
                Assert.AreEqual(Convert.ToString(100d), result[0]);

                env.UndeployAll();
            }
        }

        internal class EPLOtherMultipleMethodInvocations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select Math.Max(2d,Price), Math.Max(Volume,4d)" + STREAM_MDB_LEN5;
                env.CompileDeploy(statementText).AddListener("s0");

                var props = AssertStatementAndGetProperty(env, true, "Math.Max(2.0d,Price)", "Math.Max(Volume,4.0d)");
                Assert.AreEqual(10d, props[0]);
                Assert.AreEqual(4d, props[1]);
                env.UndeployAll();
            }
        }

        internal class EPLOtherOtherClauses : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // where
                var statementText = "@name('s0') select *" + STREAM_MDB_LEN5 + "where Math.Pow(Price, .5) > 2";
                env.CompileDeploy(statementText).AddListener("s0");
                Assert.AreEqual("IBM", AssertStatementAndGetProperty(env, true, "Symbol")[0]);
                SendEvent(env, "CAT", 4d, 100);
                Assert.IsNull(GetProperty(env, "Symbol"));
                env.UndeployAll();

                // group-by
                statementText = "@name('s0') select Symbol, sum(Price)" +
                                STREAM_MDB_LEN5 +
                                "group by Convert.ToString(Symbol)";
                env.CompileDeploy(statementText).AddListener("s0");
                Assert.AreEqual(10d, AssertStatementAndGetProperty(env, true, "sum(Price)")[0]);
                SendEvent(env, "IBM", 4d, 100);
                Assert.AreEqual(14d, GetProperty(env, "sum(Price)"));
                env.UndeployAll();

                // having
                statementText = "@name('s0') select Symbol, sum(Price)" +
                                STREAM_MDB_LEN5 +
                                "having Math.Pow(sum(Price), .5) > 3";
                env.CompileDeploy(statementText).AddListener("s0");
                Assert.AreEqual(10d, AssertStatementAndGetProperty(env, true, "sum(Price)")[0]);
                SendEvent(env, "IBM", 100d, 100);
                Assert.AreEqual(110d, GetProperty(env, "sum(Price)"));
                env.UndeployAll();

                // order-by
                statementText = "@name('s0') select Symbol, Price" +
                                STREAM_MDB_LEN5 +
                                "output every 3 events order by Math.Pow(Price, 2)";
                env.CompileDeploy(statementText).AddListener("s0");
                AssertStatementAndGetProperty(env, false, "Symbol");
                SendEvent(env, "CAT", 10d, 0L);
                SendEvent(env, "MAT", 3d, 0L);

                var newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.IsTrue(newEvents.Length == 3);
                Assert.AreEqual("MAT", newEvents[0].Get("Symbol"));
                Assert.AreEqual("IBM", newEvents[1].Get("Symbol"));
                Assert.AreEqual("CAT", newEvents[2].Get("Symbol"));
                env.UndeployAll();
            }
        }

        internal class EPLOtherNestedFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select " +
                           "SupportStaticMethodLib.AppendPipe(SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))'),temp.Geom) as val" +
                           " from SupportTemperatureBean as temp";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportTemperatureBean("a"));
                Assert.AreEqual(
                    "|POLYGON ((100.0 100, \", 100 100, 400 400))||a",
                    env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherPassthru : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select " +
                           "SupportStaticMethodLib.Passthru(Id) as val from SupportBean_S0";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherPerfConstantParameters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "select " +
                           "SupportStaticMethodLib.Sleep(100) as val" +
                           " from SupportTemperatureBean as temp";
                env.CompileDeploy(text);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportTemperatureBean("a"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(2000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLOtherPerfConstantParametersNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "select " +
                           "SupportStaticMethodLib.Sleep(SupportStaticMethodLib.Passthru(100)) as val" +
                           " from SupportTemperatureBean as temp";
                env.CompileDeploy(text);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 500; i++) {
                    env.SendEventBean(new SupportTemperatureBean("a"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);

                env.UndeployAll();
            }
        }

        public class LevelZero
        {
            public static LevelOne GetLevelOne()
            {
                return new LevelOne();
            }
        }

        public class LevelOne
        {
            private static string field;

            public string LevelTwoValue => field;

            public string GetLevelTwoValue()
            {
                return field;
            }

            public static string Field {
                set => field = value;
            }

            public static void SetField(string field)
            {
                LevelOne.field = field;
            }
        }

        public class NullPrimitive
        {
            public static int? GetValue(int input)
            {
                return input + 10;
            }
        }

        public class PrimitiveConversionLib
        {
            public static int PassIntAsObject(object o)
            {
                return o.AsInt32();
            }

            public static int PassIntAsNumber(object n)
            {
                return n.AsInt32();
            }

            public static int PassIntAsComparable(IComparable c)
            {
                return c.AsInt32();
            }
        }
    }
} // end of namespace