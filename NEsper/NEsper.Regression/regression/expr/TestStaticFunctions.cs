///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestStaticFunctions 
	{
        private EPServiceProvider _epService;
        private String _stream;
        private String _statementText;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _stream = " from " + typeof(SupportMarketDataBean).FullName + "#length(5) ";
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestNullPrimitive()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(TestStaticFunctions.NullPrimitive));

            // test passing null
            _epService.EPAdministrator.CreateEPL("select NullPrimitive.GetValue(IntBoxed) from SupportBean").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
        }

        [Test]
        public void TestChainedInstance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(TestStaticFunctions.LevelZero));

            _epService.EPAdministrator.CreateEPL("select " +
                    "LevelZero.GetLevelOne().GetLevelTwoValue() as val0 " +
                    "from SupportBean").Events += _listener.Update;

            TestStaticFunctions.LevelOne.Field = "v1";
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[] { "v1" });

            TestStaticFunctions.LevelOne.Field = "v2";
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[] { "v2" });
        }

        [Test]
        public void TestChainedStatic()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportChainTop));

            var subexp = "SupportChainTop.Make().GetChildOne(\"abc\",1).GetChildTwo(\"def\").GetText()";
            _statementText = "select " + subexp + " from SupportBean";
            var stmtOne = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            stmtOne.Events += _listener.Update;

            var rows = new Object[][] {
                    new Object[] {subexp, typeof(String)}
                    };
            for (var i = 0; i < rows.Length; i++)
            {
                var prop = stmtOne.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }

            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), new String[] { subexp },
                    new Object[] { SupportChainTop.Make().GetChildOne("abc", 1).GetChildTwo("def").GetText() });
        }

        [Test]
        public void TestEscape()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);

            _statementText = "select SupportStaticMethodLib.`Join`(abcstream) as value from SupportBean abcstream";
            var stmtOne = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 99));

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), "value".Split(','), new Object[] { "E1 99" });
        }

        [Test]
        public void TestReturnsMapIndexProperty()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);

            _statementText = "insert into ABCStream select SupportStaticMethodLib.MyMapFunc() as mymap, SupportStaticMethodLib.MyArrayFunc() as myindex from SupportBean";
            var stmtOne = _epService.EPAdministrator.CreateEPL(_statementText);

            _statementText = "select mymap('A') as v0, myindex[1] as v1 from ABCStream";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            stmtTwo.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), "v0,v1".Split(','), new Object[] { "A1", 200 });
        }

        [Test]
        public void TestPattern()
        {
            var className = typeof(SupportStaticMethodLib).FullName;
            _statementText = "select * from pattern [myevent=" + typeof(SupportBean).FullName + "(" +
                    className + ".DelimitPipe(TheString) = '|a|')]";
            var stmt = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("b", 0));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean("a", 0));
            Assert.IsTrue(_listener.IsInvoked);

            stmt.Dispose();
            _statementText = "select * from pattern [myevent=" + typeof(SupportBean).FullName + "(" +
                    className + ".DelimitPipe(null) = '|<null>|')]";
            stmt = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("a", 0));
            Assert.IsTrue(_listener.IsInvoked);
        }

        [Test]
        public void TestRuntimeException()
        {
            var className = typeof(SupportStaticMethodLib).FullName;
            _statementText = "select price, " + className + ".ThrowException() as value " + _stream;
            var statement = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            SendEvent("IBM", 10d, 4L);
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("value"));
        }

        [Test]
        public void TestArrayParameter()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));

            var text = "select " +
                    "SupportStaticMethodLib.ArraySumIntBoxed({1,2,null,3,4}) as v1, " +
                    "SupportStaticMethodLib.ArraySumDouble({1,2,3,4.0}) as v2, " +
                    "SupportStaticMethodLib.ArraySumString({'1','2','3','4'}) as v3, " +
                    "SupportStaticMethodLib.ArraySumObject({'1',2,3.0,'4.0'}) as v4 " +
                    " from " + typeof(SupportBean).FullName;
            _listener = new SupportUpdateListener();
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "v1,v2,v3,v4".Split(','), new Object[] { 10, 10d, 10d, 10d });
        }

        [Test]
        public void TestNoParameters()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // not instrumented

            var startTime = PerformanceObserver.MilliTime;
            _statementText = "select com.espertech.esper.compat.PerformanceObserver.GetTimeMillis() " + _stream;
            var result = (long)CreateStatementAndGet("com.espertech.esper.compat.PerformanceObserver.GetTimeMillis()");
            var finishTime = PerformanceObserver.MilliTime;
            Assert.IsTrue(startTime <= result);
            Assert.IsTrue(result <= finishTime);

#if false
    		_statementText = "select java.typeof(lang)Loader.SystemClassLoader " + _stream;
    		Object expected = ClassLoader.SystemClassLoader;
    		var resultTwo = CreateStatementAndGetProperty(true, "java.typeof(lang)Loader.SystemClassLoader");
    		Assert.AreEqual(expected, resultTwo[0]);
#endif

            _statementText = "select UnknownClass.InvalidMethod() " + _stream;
            try
            {
                CreateStatementAndGetProperty(true, "invalidMethod()");
                Assert.Fail();
            }
            catch (EPStatementException)
            {
                // Expected
            }
        }

        [Test]
        public void TestSingleParameterOM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.StaticMethod(Name.Of<BitWriter>(), "Write", 7), "value");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(5)));
            model = (EPStatementObjectModel)SerializableObjectCopier.Copy(model);
            _statementText = "select " + Name.Of<BitWriter>() + ".Write(7) as value" + _stream;

            Assert.AreEqual(_statementText.Trim(), model.ToEPL());
            var statement = _epService.EPAdministrator.Create(model);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            SendEvent("IBM", 10d, 4L);
            Assert.AreEqual(BitWriter.Write(7), _listener.AssertOneGetNewAndReset().Get("value"));
        }

        [Test]
        public void TestSingleParameterCompile()
        {
            _statementText = "select " + Name.Of<BitWriter>() + ".Write(7) as value" + _stream;
            var model = _epService.EPAdministrator.CompileEPL(_statementText);
            model = (EPStatementObjectModel)SerializableObjectCopier.Copy(model);

            Assert.AreEqual(_statementText.Trim(), model.ToEPL());
            var statement = _epService.EPAdministrator.Create(model);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            SendEvent("IBM", 10d, 4L);
            Assert.AreEqual(BitWriter.Write(7), _listener.AssertOneGetNewAndReset().Get("value"));
        }

        [Test]
        public void TestSingleParameter()
        {
            _statementText = "select " + Name.Of<BitWriter>() + ".Write(7) " + _stream;
            var result = CreateStatementAndGetProperty(true, Name.Of<BitWriter>() + ".Write(7)");
            Assert.AreEqual(BitWriter.Write(7), result[0]);

            _statementText = "select Convert.ToInt32(\"6\") " + _stream;
            result = CreateStatementAndGetProperty(true, "Convert.ToInt32(\"6\")");
            Assert.AreEqual(6, result[0]);

            _statementText = "select Convert.ToString(\'a\') " + _stream;
            result = CreateStatementAndGetProperty(true, "Convert.ToString(\"a\")");
            Assert.AreEqual("a", result[0]);
        }

        [Test]
        public void TestTwoParameters()
        {
            _statementText = "select Math.Max(2,3) " + _stream;
            Assert.AreEqual(3, CreateStatementAndGetProperty(true, "Math.Max(2,3)")[0]);

            _statementText = "select System.Math.Max(2,3d) " + _stream;
            Assert.AreEqual(3d, CreateStatementAndGetProperty(true, "System.Math.Max(2,3.0d)")[0]);

            _statementText = "select Convert.ToInt64(\"123\")" + _stream;
            Object expected = long.Parse("123");
            Assert.AreEqual(expected, CreateStatementAndGetProperty(true, "Convert.ToInt64(\"123\")")[0]);
        }

        [Test]
        public void TestUserDefined()
        {
            var className = typeof(SupportStaticMethodLib).FullName;
            _statementText = "select " + className + ".StaticMethod(2)" + _stream;
            Assert.AreEqual(2, CreateStatementAndGetProperty(true, className + ".StaticMethod(2)")[0]);

            // try context passed
            SupportStaticMethodLib.GetMethodInvocationContexts().Clear();
            _statementText = "@Name('S0') select " + className + ".StaticMethodWithContext(2)" + _stream;
            Assert.AreEqual(2, CreateStatementAndGetProperty(true, className + ".StaticMethodWithContext(2)")[0]);
            var first = SupportStaticMethodLib.GetMethodInvocationContexts()[0];
            Assert.AreEqual("S0", first.StatementName);
            Assert.AreEqual(_epService.URI, first.EngineURI);
            Assert.AreEqual(-1, first.ContextPartitionId);
            Assert.AreEqual("StaticMethodWithContext", first.FunctionName);
        }

        [Test]
        public void TestComplexParameters()
        {
            _statementText = "select Convert.ToString(price) " + _stream;
            var result = CreateStatementAndGetProperty(true, "Convert.ToString(price)");
            Assert.AreEqual(Convert.ToString(10d), result[0]);

            _statementText = "select Convert.ToString(2 + 3*5) " + _stream;
            result = CreateStatementAndGetProperty(true, "Convert.ToString(2+3*5)");
            Assert.AreEqual(Convert.ToString(17), result[0]);

            _statementText = "select Convert.ToString(price*volume+volume) " + _stream;
            result = CreateStatementAndGetProperty(true, "Convert.ToString(price*volume+volume)");
            Assert.AreEqual(Convert.ToString(44d), result[0]);

            _statementText = "select Convert.ToString(Math.Pow(price,Convert.ToInt32(\"2\"))) " + _stream;
            result = CreateStatementAndGetProperty(true, "Convert.ToString(Math.Pow(price,Convert.ToInt32(\"2\")))");
            Assert.AreEqual(Convert.ToString(100d), result[0]);
        }

        [Test]
        public void TestMultipleMethodInvocations()
        {
            _statementText = "select Math.Max(2d,price),Math.Max(volume,4d)" + _stream;
            var props = CreateStatementAndGetProperty(true, "Math.Max(2.0d,price)", "Math.Max(volume,4.0d)");
            Assert.AreEqual(10d, props[0]);
            Assert.AreEqual(4d, props[1]);
        }

        [Test]
        public void TestOtherClauses()
        {
            // where
            _statementText = "select *" + _stream + "where Math.Pow(price, .5) > 2";
            Assert.AreEqual("IBM", CreateStatementAndGetProperty(true, "symbol")[0]);
            SendEvent("CAT", 4d, 100);
            Assert.IsNull(GetProperty("symbol"));

            // group-by
            _statementText = "select symbol, Sum(price)" + _stream + "group by Convert.ToString(symbol)";
            Assert.AreEqual(10d, CreateStatementAndGetProperty(true, "sum(price)")[0]);
            SendEvent("IBM", 4d, 100);
            Assert.AreEqual(14d, GetProperty("sum(price)"));

            _epService.Initialize();

            // having
            _statementText = "select symbol, Sum(price)" + _stream + "having Math.Pow(Sum(price), .5) > 3";
            Assert.AreEqual(10d, CreateStatementAndGetProperty(true, "sum(price)")[0]);
            SendEvent("IBM", 100d, 100);
            Assert.AreEqual(110d, GetProperty("sum(price)"));

            // order-by
            _statementText = "select symbol, price" + _stream + "output every 3 events order by Math.Pow(price, 2)";
            CreateStatementAndGetProperty(false, "symbol");
            SendEvent("CAT", 10d, 0L);
            SendEvent("MAT", 3d, 0L);

            var newEvents = _listener.GetAndResetLastNewData();
            Assert.IsTrue(newEvents.Length == 3);
            Assert.AreEqual("MAT", newEvents[0].Get("symbol"));
            Assert.AreEqual("IBM", newEvents[1].Get("symbol"));
            Assert.AreEqual("CAT", newEvents[2].Get("symbol"));
        }

        [Test]
        public void TestNestedFunction()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            var text = "select " +
                    "SupportStaticMethodLib.AppendPipe(SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))'),temp.geom) as val" +
                    " from Temperature as temp";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            Assert.AreEqual("|POLYGON ((100.0 100, \", 100 100, 400 400))||a", listener.AssertOneGetNewAndReset().Get("val"));
        }

        [Test]
        public void TestPassthru()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            var text = "select " +
                    "SupportStaticMethodLib.Passthru(id) as val" +
                    " from " + typeof(SupportBean_S0).FullName;
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("val"));

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("val"));
        }

        [Test]
        public void TestPerfConstantParameters()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // not instrumented

            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
            configuration.AddPlugInSingleRowFunction("sleepme", typeof(SupportStaticMethodLib).FullName, "Sleep", ValueCacheEnum.ENABLED);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            var text = "select " +
                    "SupportStaticMethodLib.Sleep(100) as val" +
                    " from Temperature as temp";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            long startTime = Environment.TickCount;
            for (var i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            long endTime = Environment.TickCount;
            var delta = endTime - startTime;

            Assert.IsTrue(delta < 2000, "Failed perf test, delta=" + delta);
            stmt.Dispose();

            // test case with non-cache
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            stmt = _epService.EPAdministrator.CreateEPL(text);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            startTime = Environment.TickCount;
            _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            endTime = Environment.TickCount;
            delta = endTime - startTime;

            Assert.IsTrue(delta > 120, "Failed perf test, delta=" + delta);
            stmt.Dispose();

            // test plug-in single-row function
            var textSingleRow = "select " +
                    "sleepme(100) as val" +
                    " from Temperature as temp";
            var stmtSingleRow = _epService.EPAdministrator.CreateEPL(textSingleRow);
            var listenerSingleRow = new SupportUpdateListener();
            stmtSingleRow.Events += listenerSingleRow.Update;

            startTime = Environment.TickCount;
            for (var i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            delta = Environment.TickCount - startTime;

            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            stmtSingleRow.Dispose();
        }

        [Test]
        public void TestPerfConstantParametersNested()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            var text = "select " +
                    "SupportStaticMethodLib.Sleep(SupportStaticMethodLib.Passthru(100)) as val" +
                    " from Temperature as temp";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            long startTime = Environment.TickCount;
            for (var i = 0; i < 500; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            long endTime = Environment.TickCount;
            var delta = endTime - startTime;

            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
        }

        private Object CreateStatementAndGet(String propertyName)
        {
            var statement = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10d, 4L, ""));
            return GetProperty(propertyName);
        }

        private Object GetProperty(String propertyName)
        {
            var newData = _listener.GetAndResetLastNewData();
            if (newData == null || newData.Length == 0)
            {
                return null;
            }
            else
            {
                return newData[0].Get(propertyName);
            }
        }

        private Object[] CreateStatementAndGetProperty(bool expectResult, params string[] propertyNames)
        {
            var statement = _epService.EPAdministrator.CreateEPL(_statementText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            SendEvent("IBM", 10d, 4L);

            if (expectResult)
            {
                var theEvent = _listener.GetAndResetLastNewData()[0];
                var properties = propertyNames.Select(theEvent.Get).ToList();
                return properties.ToArray();
            }
            return null;
        }

        private void SendEvent(String symbol, double price, long volume)
        {
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, ""));
        }

        public class LevelZero
        {
            public static TestStaticFunctions.LevelOne GetLevelOne()
            {
                return new TestStaticFunctions.LevelOne();
            }
        }

        public class LevelOne
        {
            private static String _field;

            public static string Field
            {
                set { _field = value; }
            }

            public string LevelTwoValue
            {
                get { return _field; }
            }

            public string GetLevelTwoValue()
            {
                return LevelTwoValue;
            }

        }

        public class NullPrimitive
        {
            public static int GetValue(int input)
            {
                return input + 10;
            }
        }
    }
} // end of namespace
