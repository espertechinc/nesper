///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
	public class EPLOtherStaticFunctions {
	    private const string STREAM_MDB_LEN5 = " from SupportMarketDataBean#length(5) ";

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLOtherNullPrimitive());
	        execs.Add(new EPLOtherChainedInstance());
	        execs.Add(new EPLOtherChainedStatic());
	        execs.Add(new EPLOtherEscape());
	        execs.Add(new EPLOtherReturnsMapIndexProperty());
	        execs.Add(new EPLOtherPattern());
	        execs.Add(new EPLOtherRuntimeException());
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

	    internal class EPLOtherStaticFuncEnumConstant : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl =
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

	    internal class EPLOtherStaticFuncWCurrentTimeStamp : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(1000);
	            var epl = "@name('s0') select java.time.format.DateTimeFormatter.ISO_INSTANT.format(java.time.Instant.ofEpochMilli(current_timestamp())) as c0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean());
	            env.AssertEqualsNew("s0", "c0", "1970-01-01T00:00:01Z");

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherPrimitiveConversion : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy("@name('s0') select " +
	                "PrimitiveConversionLib.passIntAsObject(intPrimitive) as c0," +
	                "PrimitiveConversionLib.passIntAsNumber(intPrimitive) as c1," +
	                "PrimitiveConversionLib.passIntAsComparable(intPrimitive) as c2," +
	                "PrimitiveConversionLib.passIntAsSerializable(intPrimitive) as c3" +
	                " from SupportBean").AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertPropsNew("s0", "c0,c1,c2,c3".SplitCsv(), new object[]{10, 10, 10, 10});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherNullPrimitive : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // test passing null
	            env.CompileDeploy("@name('s0') select NullPrimitive.GetValue(intBoxed) from SupportBean").AddListener("s0");

	            env.SendEventBean(new SupportBean());

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherChainedInstance : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy("@name('s0') select " +
	                "LevelZero.GetLevelOne().GetLevelTwoValue() as val0 " +
	                "from SupportBean").AddListener("s0");

	            LevelOne.SetField("v1");
	            env.SendEventBean(new SupportBean());
	            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[]{"v1"});

	            LevelOne.SetField("v2");
	            env.SendEventBean(new SupportBean());
	            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[]{"v2"});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherChainedStatic : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var subexp = "SupportChainTop.Make().GetChildOne(\"abc\",1).GetChildTwo(\"def\").GetText()";
	            var statementText = $"@name('s0') select {subexp} from SupportBean";
	            env.CompileDeploy(statementText).AddListener("s0");

	            var rows = new object[][]{
	                new object[] {subexp, typeof(string)}
	            };
	            env.AssertStatement("s0", statement => {
	                var prop = statement.EventType.PropertyDescriptors;
	                for (var i = 0; i < rows.Length; i++) {
	                    Assert.AreEqual(rows[i][0], prop[i].PropertyName);
	                    Assert.AreEqual(rows[i][1], prop[i].PropertyType);
	                }
	            });

	            env.SendEventBean(new SupportBean());
	            env.AssertPropsNew("s0", new string[]{subexp},
	                new object[]{SupportChainTop.Make().GetChildOne("abc", 1).GetChildTwo("def").Text});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherEscape : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var statementText = "@name('s0') select SupportStaticMethodLib.`Join`(abcstream) as value from SupportBean abcstream";
	            env.CompileDeploy(statementText).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 99));

	            env.AssertPropsNew("s0", "value".SplitCsv(), new object[]{"E1 99"});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherReturnsMapIndexProperty : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var statementText = "@public insert into ABCStream select SupportStaticMethodLib.MyMapFunc() as mymap, SupportStaticMethodLib.MyArrayFunc() as myindex from SupportBean";
	            env.CompileDeploy(statementText, path);

	            statementText = "@name('s0') select mymap('A') as v0, myindex[1] as v1 from ABCStream";
	            env.CompileDeploy(statementText, path).AddListener("s0");

	            env.SendEventBean(new SupportBean());

	            env.AssertPropsNew("s0", "v0,v1".SplitCsv(), new object[]{"A1", 200});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherPattern : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var className = typeof(SupportStaticMethodLib).FullName;
	            var statementText =
		            $"@name('s0') select * from pattern [Myevent=SupportBean({className}.DelimitPipe(TheString) = '|a|')]";
	            env.CompileDeploy(statementText).AddListener("s0");

	            env.SendEventBean(new SupportBean("b", 0));
	            env.AssertListenerNotInvoked("s0");
	            env.SendEventBean(new SupportBean("a", 0));
	            env.AssertListenerInvoked("s0");

	            env.UndeployAll();
	            statementText =
		            $"@name('s0') select * from pattern [Myevent=SupportBean({className}.DelimitPipe(null) = '|<null>|')]";
	            env.CompileDeploy(statementText).AddListener("s0");

	            env.SendEventBean(new SupportBean("a", 0));
	            env.AssertListenerInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherRuntimeException : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var className = typeof(SupportStaticMethodLib).FullName;
	            var statementText =
		            $"@name('s0') select price, {className}.ThrowException() as value {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "value", null);

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherArrayParameter : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select " +
	                       "SupportStaticMethodLib.ArraySumIntBoxed({1,2,null,3,4}) as v1, " +
	                       "SupportStaticMethodLib.ArraySumDouble({1,2,3,4.0}) as v2, " +
	                       "SupportStaticMethodLib.ArraySumString({'1','2','3','4'}) as v3, " +
	                       "SupportStaticMethodLib.ArraySumObject({'1',2,3.0,'4.0'}) as v4 " +
	                       " from SupportBean";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", "v1,v2,v3,v4".SplitCsv(), new object[]{10, 10d, 10d, 10d});

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherNoParameters : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
		        var dateTimeHelper = typeof(DateTimeHelper).FullName;

	            long? startTime = DateTimeHelper.CurrentTimeMillis;
	            var statementText = $"@name('s0') select {dateTimeHelper}.GetCurrentTimeMillis() {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            env.SendEventBean(new SupportMarketDataBean("IBM", 10d, 4L, ""));
	            env.AssertEventNew("s0", @event => {
	                var result = @event.Get($"{dateTimeHelper}.GetCurrentTimeMillis()").AsInt64();
	                long? finishTime = DateTimeHelper.CurrentTimeMillis;
	                Assert.IsTrue(startTime <= result);
	                Assert.IsTrue(result <= finishTime);
	            });

	            env.UndeployAll();

	            statementText = $"@name('s0') select java.lang.ClassLoader.getSystemClassLoader() {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            //object expected = ClassLoader.SystemClassLoader;
	            //SendEvent(env, "IBM", 10d, 4L);
	            //env.AssertEqualsNew("s0", "java.lang.ClassLoader.getSystemClassLoader()", expected);

	            env.UndeployAll();

	            env.TryInvalidCompile(
		            $"select UnknownClass.invalidMethod() {STREAM_MDB_LEN5}",
	                "Failed to validate select-clause expression 'UnknownClass.invalidMethod()': Failed to resolve 'UnknownClass.invalidMethod' to a property, single-row function, aggregation function, script, stream or class name ");
	        }
	    }

	    internal class EPLOtherSingleParameterOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create()
                    .Add(Expressions.StaticMethod<BitWriter>("Write", 7), "value");
	            model.FromClause = FromClause.Create(FilterStream.Create("SupportMarketDataBean").AddView("length", Expressions.Constant(5)));
	            model = env.CopyMayFail(model);
	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

	            var bitWriter = typeof(BitWriter).CleanName();
	            var statementText = $"@name('s0') select {bitWriter}.Write(7) as value{STREAM_MDB_LEN5}";
	            
	            Assert.AreEqual(statementText.Trim(), model.ToEPL());
	            env.CompileDeploy(model).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "value", BitWriter.Write(7));

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherSingleParameterCompile : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
		        var bitWriter = typeof(BitWriter).CleanName();
                var statementText = $"@name('s0') select {bitWriter}.Write(7) as value{STREAM_MDB_LEN5}";
	            env.EplToModelCompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "value", BitWriter.Write(7));

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherSingleParameter : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
		        var bitWriter = typeof(BitWriter).CleanName();
                var statementText = $"@name('s0') select {bitWriter}.Write(7) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", $"{bitWriter}.Write(7)", BitWriter.Write(7));
	            env.UndeployAll();

                statementText = $"@name('s0') select Convert.ToInt32(\"6\") {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToInt32(\"6\")", Convert.ToInt32("6"));

	            env.UndeployAll();

	            statementText = $"@name('s0') select System.Convert.ToString('a') {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "System.Convert.ToString(\"a\")", Convert.ToString('a'));

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherTwoParameters : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

                var statementText = $"@name('s0') select Math.Max(2,3) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Math.Max(2,3)", 3);

	            env.UndeployAll();

	            statementText = $"@name('s0') select System.Math.Math(2,3d) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "System.Math.Math(2,3.0)", 3d);

	            env.UndeployAll();

	            statementText = $"@name('s0') select Convert.ToInt64(\"123\"){STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToInt64(\"123\")", long.Parse("123"));
	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherUserDefined : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var className = typeof(SupportStaticMethodLib).FullName;
	            var statementText = $"@name('s0') select {className}.staticMethod(2){STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", $"{className}.StaticMethod(2)", 2);

	            env.UndeployAll();

	            // try context passed
	            SupportStaticMethodLib.MethodInvocationContexts.Clear();
	            statementText = $"@name('s0') select {className}.StaticMethodWithContext(2){STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", $"{className}.StaticMethodWithContext(2)", 2);
	            env.AssertThat(() => {
	                var first = SupportStaticMethodLib.MethodInvocationContexts[0];
	                Assert.AreEqual("s0", first.StatementName);
	                Assert.AreEqual(env.RuntimeURI, first.RuntimeURI);
	                Assert.AreEqual(-1, first.ContextPartitionId);
	                Assert.AreEqual("StaticMethodWithContext", first.FunctionName);
	            });
	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherComplexParameters : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var statementText = $"@name('s0') select Convert.ToString(Price) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToString(Price)", Convert.ToString(10d));
	            env.UndeployAll();

	            statementText = $"@name('s0') select Convert.ToString(2 + 3*5) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToString(2+3*5)", Convert.ToString(2 + 3 * 5));
	            env.UndeployAll();

	            statementText = $"@name('s0') select Convert.ToString(Price*volume +volume) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToString(Price*volume+volume)", Convert.ToString(44d));
	            env.UndeployAll();

	            statementText =
		            $"@name('s0') select Convert.ToString(Math.Pow(Price,Convert.ToInt32(\"2\"))) {STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "Convert.ToString(Math.Pow(Price,Convert.ToInt32(\"2\")))", Convert.ToString(100d));

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherMultipleMethodInvocations : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var statementText = $"@name('s0') select Math.Max(2d,Price), Math.Max(volume,4d){STREAM_MDB_LEN5}";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertPropsNew("s0", new string[]{"Math.Max(2.0d,Price)", "Math.Max(volume,4.0)"}, new object[]{10d, 4d});
	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherOtherClauses : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // where
	            var statementText = $"@name('s0') select *{STREAM_MDB_LEN5}where Math.Pow(price, .5) > 2";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "symbol", "IBM");

	            SendEvent(env, "CAT", 4d, 100);
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();

	            // group-by
	            statementText =
		            $"@name('s0') select symbol, sum(price){STREAM_MDB_LEN5}group by String.valueOf(symbol)";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "sum(price)", 10d);

	            SendEvent(env, "IBM", 4d, 100);
	            env.AssertEqualsNew("s0", "sum(price)", 14d);
	            env.UndeployAll();

	            // having
	            statementText =
		            $"@name('s0') select symbol, sum(price){STREAM_MDB_LEN5}having Math.Pow(sum(price), .5) > 3";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            env.AssertEqualsNew("s0", "sum(price)", 10d);

	            SendEvent(env, "IBM", 100d, 100);
	            env.AssertEqualsNew("s0", "sum(price)", 110d);

	            env.UndeployAll();

	            // order-by
	            statementText =
		            $"@name('s0') select symbol, price{STREAM_MDB_LEN5}output every 3 events order by Math.Pow(price, 2)";
	            env.CompileDeploy(statementText).AddListener("s0");

	            SendEvent(env, "IBM", 10d, 4L);
	            SendEvent(env, "CAT", 10d, 0L);
	            SendEvent(env, "MAT", 3d, 0L);

	            env.AssertListener("s0", listener => {
	                var newEvents = listener.GetAndResetLastNewData();
	                Assert.IsTrue(newEvents.Length == 3);
	                Assert.AreEqual("MAT", newEvents[0].Get("symbol"));
	                Assert.AreEqual("IBM", newEvents[1].Get("symbol"));
	                Assert.AreEqual("CAT", newEvents[2].Get("symbol"));
	            });
	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherNestedFunction : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select " +
	                       "SupportStaticMethodLib.AppendPipe(SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))'),temp.Geom) as val" +
	                       " from SupportTemperatureBean as temp";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(new SupportTemperatureBean("a"));
	            env.AssertEqualsNew("s0", "val", "|POLYGON ((100.0 100, \", 100 100, 400 400))||a");

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherPassthru : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select " +
	                       "SupportStaticMethodLib.Passthru(Id) as val from SupportBean_S0";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertEqualsNew("s0", "val", 1L);

	            env.SendEventBean(new SupportBean_S0(2));
	            env.AssertEqualsNew("s0", "val", 2L);

	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherPerfConstantParameters : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "select " +
	                       "SupportStaticMethodLib.sleep(100) as val" +
	                       " from SupportTemperatureBean as temp";
	            env.CompileDeploy(text);

                var startTime = PerformanceObserver.MilliTime;
	            for (var i = 0; i < 1000; i++) {
	                env.SendEventBean(new SupportTemperatureBean("a"));
	            }

                var endTime = PerformanceObserver.MilliTime;
	            var delta = endTime - startTime;

	            Assert.IsTrue(delta < 2000, "Failed perf test, delta=" + delta);
	            env.UndeployAll();
	        }
	    }

	    internal class EPLOtherPerfConstantParametersNested : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "select " +
	                       "SupportStaticMethodLib.sleep(SupportStaticMethodLib.passthru(100)) as val" +
	                       " from SupportTemperatureBean as temp";
	            env.CompileDeploy(text);

                var startTime = PerformanceObserver.MilliTime;
	            for (var i = 0; i < 500; i++) {
	                env.SendEventBean(new SupportTemperatureBean("a"));
	            }

                var endTime = PerformanceObserver.MilliTime;
	            var delta = endTime - startTime;

	            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);

	            env.UndeployAll();
	        }
	    }

	    private static void SendEvent(RegressionEnvironment env, string symbol, double price, long volume) {
	        env.SendEventBean(new SupportMarketDataBean(symbol, price, volume, ""));
	    }

	    public class LevelZero {
	        public static LevelOne GetLevelOne() {
	            return new LevelOne();
	        }
	    }

	    public class LevelOne {
	        private static string field;

	        public static void SetField(string field) {
	            LevelOne.field = field;
	        }

	        public static string Field {
		        get => field;
		        set => field = value;
	        }

	        public string GetLevelTwoValue() {
	            return field;
	        }
	        
	        public string LevelTwoValue => field;
	    }

	    public class NullPrimitive {

	        public static int? GetValue(int input) {
	            return input + 10;
	        }
	    }

	    public class PrimitiveConversionLib {
	        public static int PassIntAsObject(object o)
	        {
		        return o.AsInt32();
	        }

	        public static int PassIntAsNumber(object n) {
	            return n.AsInt32();
	        }

	        public static int PassIntAsComparable(IComparable c) {
	            return c.AsInt32();
	        }

	        public static int PassIntAsSerializable(object s) {
	            return s.AsInt32();
	        }
	    }
	}
} // end of namespace
