///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLStaticFunctions : RegressionExecution
    {
        private static readonly string STREAM_MDB_LEN5 = " from " + typeof(SupportMarketDataBean).FullName + "#length(5) ";
    
        public override void Configure(Configuration configuration) {
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddImport(typeof(LevelZero));
            configuration.AddImport(typeof(SupportChainTop));
            configuration.AddImport(typeof(NullPrimitive));
    
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType(typeof(SupportChainTop));
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
    
            configuration.AddPlugInSingleRowFunction(
                "sleepme", typeof(SupportStaticMethodLib), "Sleep", ValueCacheEnum.ENABLED);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            RunAssertionNullPrimitive(epService);
            RunAssertionChainedInstance(epService);
            RunAssertionChainedStatic(epService);
            RunAssertionEscape(epService);
            RunAssertionReturnsMapIndexProperty(epService);
            RunAssertionPattern(epService);
            RunAssertionRuntimeException(epService);
            RunAssertionArrayParameter(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionNoParameters(epService);
                RunAssertionPerfConstantParameters(epService);
                RunAssertionPerfConstantParametersNested(epService);
            }
            RunAssertionSingleParameterOM(epService);
            RunAssertionSingleParameterCompile(epService);
            RunAssertionSingleParameter(epService);
            RunAssertionTwoParameters(epService);
            RunAssertionUserDefined(epService);
            RunAssertionComplexParameters(epService);
            RunAssertionMultipleMethodInvocations(epService);
            RunAssertionOtherClauses(epService);
            RunAssertionNestedFunction(epService);
            RunAssertionPassthru(epService);
        }
    
        private void RunAssertionNullPrimitive(EPServiceProvider epService) {
            // test passing null
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select NullPrimitive.GetValue(IntBoxed) from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionChainedInstance(EPServiceProvider epService) {
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select " +
                "LevelZero.GetLevelOne().GetLevelTwoValue() as val0 " +
                "from SupportBean").Events += listener.Update;
    
            LevelOne.Field = "v1";
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{"v1"});
    
            LevelOne.Field = "v2";
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{"v2"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionChainedStatic(EPServiceProvider epService) {
    
            var subexp = "SupportChainTop.Make().GetChildOne(\"abc\",1).GetChildTwo(\"def\").GetText()";
            var statementText = "select " + subexp + " from SupportBean";
            var stmtOne = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            var rows = new object[][]{
                new object[] {subexp, typeof(string)}
            };
            for (var i = 0; i < rows.Length; i++) {
                var prop = stmtOne.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), new string[]{subexp},
                    new object[]{SupportChainTop.Make().GetChildOne("abc", 1).GetChildTwo("def").GetText()});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionEscape(EPServiceProvider epService) {
            var statementText = "select SupportStaticMethodLib.`Join`(abcstream) as value from SupportBean abcstream";
            var stmtOne = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 99));
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), "value".Split(','), new object[]{"E1 99"});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionReturnsMapIndexProperty(EPServiceProvider epService) {
            var statementText = "insert into ABCStream select SupportStaticMethodLib.MyMapFunc() as mymap, SupportStaticMethodLib.MyArrayFunc() as myindex from SupportBean";
            var stmtOne = epService.EPAdministrator.CreateEPL(statementText);
    
            statementText = "select mymap('A') as v0, myindex[1] as v1 from ABCStream";
            var stmtTwo = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), "v0,v1".Split(','), new object[]{"A1", 200});
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionPattern(EPServiceProvider epService) {
            var className = typeof(SupportStaticMethodLib).FullName;
            var statementText = "select * from pattern [myevent =" + typeof(SupportBean).FullName + "(" +
                    className + ".DelimitPipe(TheString) = '|a|')]";
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("b", 0));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("a", 0));
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
            statementText = "select * from pattern [myevent =" + typeof(SupportBean).FullName + "(" +
                    className + ".DelimitPipe(null) = '|<null>|')]";
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", 0));
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionRuntimeException(EPServiceProvider epService) {
            var className = typeof(SupportStaticMethodLib).FullName;
            var statementText = "select price, " + className + ".ThrowException() as value " + STREAM_MDB_LEN5;
            var statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 10d, 4L);
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("value"));
            statement.Dispose();
        }
    
        private void RunAssertionArrayParameter(EPServiceProvider epService) {
            var text = "select " +
                    "SupportStaticMethodLib.ArraySumIntBoxed({1,2,null,3,4}) as v1, " +
                    "SupportStaticMethodLib.ArraySumDouble({1,2,3,4.0}) as v2, " +
                    "SupportStaticMethodLib.ArraySumString({'1','2','3','4'}) as v3, " +
                    "SupportStaticMethodLib.ArraySumObject({'1',2,3.0,'4.0'}) as v4 " +
                    " from " + typeof(SupportBean).FullName;
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL(text);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "v1,v2,v3,v4".Split(','), new object[]{10, 10d, 10d, 10d});
    
            stmt.Dispose();
        }
    
        private void RunAssertionNoParameters(EPServiceProvider epService) {
            var startTime = PerformanceObserver.MilliTime;
            var listener = new SupportUpdateListener();
    
            var statementText = "select com.espertech.esper.compat.PerformanceObserver.GetTimeMillis() " + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            var result = (long) CreateStatementAndGet(epService, listener, statementText, "com.espertech.esper.compat.PerformanceObserver.GetTimeMillis()");
            var finishTime = PerformanceObserver.MilliTime;
            Assert.IsTrue(startTime <= result);
            Assert.IsTrue(result <= finishTime);
            stmt.Dispose();

#if false
            statementText = "select Java.lang.ClassLoader.SystemClassLoader " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Object expected = ClassLoader.SystemClassLoader;
            var resultTwo = AssertStatementAndGetProperty(epService, listener, true, "java.lang.ClassLoader.SystemClassLoader");
            if (resultTwo == null) {
                Assert.Fail();
            }
            Assert.AreEqual(expected, resultTwo[0]);
            stmt.Dispose();
#endif
    
            statementText = "select UnknownClass.InvalidMethod() " + STREAM_MDB_LEN5;
            try {
                stmt = epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            } catch (EPStatementException) {
                // Expected
            }
        }
    
        private void RunAssertionSingleParameterOM(EPServiceProvider epService)
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.StaticMethod(Name.Clean<BitWriter>(), "Write", 7), "value");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(5)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            var statementText = "select " + Name.Clean<BitWriter>() + ".Write(7) as value" + STREAM_MDB_LEN5;
    
            Assert.AreEqual(statementText.Trim(), model.ToEPL());
            var statement = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 10d, 4L);
            Assert.AreEqual(BitWriter.Write(7), listener.AssertOneGetNewAndReset().Get("value"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSingleParameterCompile(EPServiceProvider epService) {
            var statementText = "select " + Name.Clean<BitWriter>() + ".Write(7) as value" + STREAM_MDB_LEN5;
            var model = epService.EPAdministrator.CompileEPL(statementText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual(statementText.Trim(), model.ToEPL());
            var statement = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 10d, 4L);
            Assert.AreEqual(BitWriter.Write(7), listener.AssertOneGetNewAndReset().Get("value"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSingleParameter(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            var statementText = "select " + Name.Clean<BitWriter>() + ".Write(7) " + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            var result = AssertStatementAndGetProperty(epService, listener, true, Name.Clean<BitWriter>() + ".Write(7)");
            Assert.AreEqual(BitWriter.Write(7), result[0]);
            stmt.Dispose();
    
            statementText = "select Convert.ToInt32(\"6\") " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToInt32(\"6\")");
            Assert.AreEqual(6, result[0]);
            stmt.Dispose();

            statementText = "select Convert.ToString(\'a\') " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToString(\"a\")");
            Assert.AreEqual("a", result[0]);
            stmt.Dispose();
        }
    
        private void RunAssertionTwoParameters(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var statementText = "select Math.Max(2,3) " + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Assert.AreEqual(3, AssertStatementAndGetProperty(epService, listener, true, "Math.Max(2,3)")[0]);
            stmt.Dispose();
    
            statementText = "select System.Math.Max(2,3d) " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Assert.AreEqual(3d, AssertStatementAndGetProperty(epService, listener, true, "System.Math.Max(2,3.0d)")[0]);
            stmt.Dispose();
    
            statementText = "select Convert.ToInt64(\"123\")" + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Object expected = long.Parse("123");
            Assert.AreEqual(expected, AssertStatementAndGetProperty(epService, listener, true, "Convert.ToInt64(\"123\")")[0]);
            stmt.Dispose();
        }
    
        private void RunAssertionUserDefined(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var className = typeof(SupportStaticMethodLib).FullName;
            var statementText = "select " + className + ".StaticMethod(2)" + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Assert.AreEqual(2, AssertStatementAndGetProperty(epService, listener, true, className + ".StaticMethod(2)")[0]);
            stmt.Dispose();
    
            // try context passed
            SupportStaticMethodLib.GetMethodInvocationContexts().Clear();
            statementText = "@Name('S0') select " + className + ".StaticMethodWithContext(2)" + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            Assert.AreEqual(2, AssertStatementAndGetProperty(epService, listener, true, className + ".StaticMethodWithContext(2)")[0]);
            EPLMethodInvocationContext first = SupportStaticMethodLib.GetMethodInvocationContexts()[0];
            Assert.AreEqual("S0", first.StatementName);
            Assert.AreEqual(epService.URI, first.EngineURI);
            Assert.AreEqual(-1, first.ContextPartitionId);
            Assert.AreEqual("StaticMethodWithContext", first.FunctionName);
            stmt.Dispose();
        }
    
        private void RunAssertionComplexParameters(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            var statementText = "select Convert.ToString(price) " + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            var result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToString(price)");
            Assert.AreEqual(Convert.ToString(10d), result[0]);
            stmt.Dispose();
    
            statementText = "select Convert.ToString(2 + 3*5) " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToString(2+3*5)");
            Assert.AreEqual(Convert.ToString(17), result[0]);
            stmt.Dispose();
    
            statementText = "select Convert.ToString(price*volume +volume) " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToString(price*volume+volume)");
            Assert.AreEqual(Convert.ToString(44d), result[0]);
            stmt.Dispose();
    
            statementText = "select Convert.ToString(Math.Pow(price,Convert.ToInt32(\"2\"))) " + STREAM_MDB_LEN5;
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            result = AssertStatementAndGetProperty(epService, listener, true, "Convert.ToString(Math.Pow(price,Convert.ToInt32(\"2\")))");
            Assert.AreEqual(Convert.ToString(100d), result[0]);
            stmt.Dispose();
        }
    
        private void RunAssertionMultipleMethodInvocations(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var statementText = "select Math.Max(2d,price), Math.Max(volume,4d) " + STREAM_MDB_LEN5;
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            var props = AssertStatementAndGetProperty(epService, listener, true, "Math.Max(2.0d,price)", "Math.Max(volume,4.0d)");
            Assert.AreEqual(10d, props[0]);
            Assert.AreEqual(4d, props[1]);
            stmt.Dispose();
        }
    
        private void RunAssertionOtherClauses(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            // where
            var statementText = "select *" + STREAM_MDB_LEN5 + "where Math.Pow(price, .5) > 2";
            var statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;
            Assert.AreEqual("IBM", AssertStatementAndGetProperty(epService, listener, true, "symbol")[0]);
            SendEvent(epService, "CAT", 4d, 100);
            Assert.IsNull(GetProperty(listener, "symbol"));
            statement.Dispose();
    
            // group-by
            statementText = "select symbol, sum(price)" + STREAM_MDB_LEN5 + "group by Convert.ToString(symbol)";
            statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;
            Assert.AreEqual(10d, AssertStatementAndGetProperty(epService, listener, true, "sum(price)")[0]);
            SendEvent(epService, "IBM", 4d, 100);
            Assert.AreEqual(14d, GetProperty(listener, "sum(price)"));
            statement.Dispose();
    
            // having
            statementText = "select symbol, sum(price)" + STREAM_MDB_LEN5 + "having Math.Pow(sum(price), .5) > 3";
            statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;
            Assert.AreEqual(10d, AssertStatementAndGetProperty(epService, listener, true, "sum(price)")[0]);
            SendEvent(epService, "IBM", 100d, 100);
            Assert.AreEqual(110d, GetProperty(listener, "sum(price)"));
            statement.Dispose();
    
            // order-by
            statementText = "select symbol, price" + STREAM_MDB_LEN5 + "output every 3 events order by Math.Pow(price, 2)";
            statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;
            AssertStatementAndGetProperty(epService, listener, false, "symbol");
            SendEvent(epService, "CAT", 10d, 0L);
            SendEvent(epService, "MAT", 3d, 0L);
    
            var newEvents = listener.GetAndResetLastNewData();
            Assert.IsTrue(newEvents.Length == 3);
            Assert.AreEqual("MAT", newEvents[0].Get("symbol"));
            Assert.AreEqual("IBM", newEvents[1].Get("symbol"));
            Assert.AreEqual("CAT", newEvents[2].Get("symbol"));
            statement.Dispose();
        }
    
        private void RunAssertionNestedFunction(EPServiceProvider epService) {
            var text = "select " +
                    "SupportStaticMethodLib.AppendPipe(SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))'),temp.geom) as val" +
                    " from Temperature as temp";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            Assert.AreEqual("|POLYGON ((100.0 100, \", 100 100, 400 400))||a", listener.AssertOneGetNewAndReset().Get("val"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPassthru(EPServiceProvider epService) {
            var text = "select " +
                    "SupportStaticMethodLib.Passthru(id) as val" +
                    " from " + typeof(SupportBean_S0).FullName;
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("val"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPerfConstantParameters(EPServiceProvider epService) {
            var text = "select " +
                    "SupportStaticMethodLib.Sleep(100) as val" +
                    " from Temperature as temp";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
    
            Assert.IsTrue(delta < 2000, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerfConstantParametersNested(EPServiceProvider epService) {
            var text = "select " +
                    "SupportStaticMethodLib.Sleep(SupportStaticMethodLib.Passthru(100)) as val" +
                    " from Temperature as temp";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 500; i++) {
                epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
    
            stmt.Dispose();
        }
    
        private Object CreateStatementAndGet(EPServiceProvider epService, SupportUpdateListener listener, string statementText, string propertyName) {
            var statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10d, 4L, ""));
            return GetProperty(listener, propertyName);
        }
    
        private Object GetProperty(SupportUpdateListener listener, string propertyName) {
            var newData = listener.GetAndResetLastNewData();
            if (newData == null || newData.Length == 0) {
                return null;
            } else {
                return newData[0].Get(propertyName);
            }
        }
    
        private object[] AssertStatementAndGetProperty(EPServiceProvider epService, SupportUpdateListener listener, bool expectResult, params string[] propertyNames) {
            if (propertyNames == null) {
                Assert.Fail("no prop names");
            }
            SendEvent(epService, "IBM", 10d, 4L);
    
            if (expectResult) {
                var properties = new List<object>();
                var theEvent = listener.GetAndResetLastNewData()[0];
                foreach (var propertyName in propertyNames) {
                    properties.Add(theEvent.Get(propertyName));
                }
                return properties.ToArray();
            }
            return null;
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, ""));
        }
    
        public class LevelZero
        {
            public static LevelOne GetLevelOne()
            {
                return new LevelOne();
            }

        }
    
        public class LevelOne {
            public static string Field { get; set; }
            public static void SetField(string field) {
                LevelOne.Field = field;
            }
    
            public string GetLevelTwoValue() {
                return Field;
            }
        }
    
        public class NullPrimitive {
    
            public static int? GetValue(int input) {
                return input + 10;
            }
        }
    }
} // end of namespace
