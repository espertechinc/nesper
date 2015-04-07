///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestSingleRowFunctionPlugIn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
    
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddPlugInSingleRowFunction("power3", typeof(MySingleRowFunction).FullName, "ComputePower3");
            configuration.AddPlugInSingleRowFunction("chainTop", typeof(MySingleRowFunction).FullName, "GetChainTop");
            configuration.AddPlugInSingleRowFunction("surroundx", typeof(MySingleRowFunction).FullName, "Surroundx");
            configuration.AddPlugInSingleRowFunction("throwExceptionLogMe", typeof(MySingleRowFunction).FullName, "ThrowException", ValueCache.DISABLED, FilterOptimizable.ENABLED, false);
            configuration.AddPlugInSingleRowFunction("throwExceptionRethrow", typeof(MySingleRowFunction).FullName, "ThrowException", ValueCache.DISABLED, FilterOptimizable.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("power3Rethrow", typeof(MySingleRowFunction).FullName, "ComputePower3", ValueCache.DISABLED, FilterOptimizable.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("power3Context", typeof(MySingleRowFunction).FullName, "ComputePower3WithContext", ValueCache.DISABLED, FilterOptimizable.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("isNullValue", typeof(MySingleRowFunction).FullName, "IsNullValue");
            configuration.AddPlugInSingleRowFunction("getValueAsString", typeof(MySingleRowFunction).FullName, "GetValueAsString");
            configuration.AddPlugInSingleRowFunction("eventsCheckStrings", typeof(MySingleRowFunction).FullName, "EventsCheckStrings");
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestEventBeanFootprint()
        {
            _epService.EPAdministrator.Configuration.AddImport(GetType());
    
            // test select-clause
            var fields = new String[] {"c0", "c1"};
            var text = "select isNullValue(*, 'TheString') as c0," +
                    "TestSingleRowFunctionPlugIn.LocalIsNullValue(*, 'TheString') as c1 from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true});
            stmt.Dispose();
    
            // test pattern
            var textPattern = "select * from pattern [a=SupportBean -> b=SupportBean(TheString=getValueAsString(a, 'TheString'))]";
            var stmtPattern = _epService.EPAdministrator.CreateEPL(textPattern);
            stmtPattern.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.IntPrimitive,b.IntPrimitive".Split(','), new Object[] {1, 2});
            stmtPattern.Dispose();
    
            // test filter
            var textFilter = "select * from SupportBean('E1'=getValueAsString(*, 'TheString'))";
            var stmtFilter = _epService.EPAdministrator.CreateEPL(textFilter);
            stmtFilter.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
            stmtFilter.Dispose();
    
            // test "first"
            var textAccessAgg = "select * from SupportBean.win:keepall() having 'E2' = GetValueAsString(last(*), 'TheString')";
            var stmtAccessAgg = _epService.EPAdministrator.CreateEPL(textAccessAgg);
            stmtAccessAgg.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
            stmtAccessAgg.Dispose();
    
            // test "window"
            var textWindowAgg = "select * from SupportBean.win:keepall() having EventsCheckStrings(window(*), 'TheString', 'E1')";
            var stmtWindowAgg = _epService.EPAdministrator.CreateEPL(textWindowAgg);
            stmtWindowAgg.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
            stmtWindowAgg.Dispose();
        }
    
        [Test]
        public void TestPropertyOrSingleRowMethod()
        {
            var text = "select surroundx('test') as val from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"val"};
            _epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"XtestX"});
        }
    
        [Test]
        public void TestChainMethod()
        {
            var text = "select chainTop().ChainValue(12,IntPrimitive) as val from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            RunAssertionChainMethod();
    
            stmt.Dispose();
            var model = _epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, stmt.Text);
            stmt.Events += _listener.Update;
    
            RunAssertionChainMethod();
        }
    
        [Test]
        public void TestSingleMethod()
        {
            var text = "select power3(IntPrimitive) as val from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            RunAssertionSingleMethod();
    
            stmt.Dispose();
            var model = _epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, stmt.Text);
            stmt.Events += _listener.Update;
    
            RunAssertionSingleMethod();
    
            stmt.Dispose();
            text = "select power3(2) as val from SupportBean";
            stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            RunAssertionSingleMethod();
            stmt.Dispose();
    
            // test passing a context as well
            text = "@Name('A') select power3Context(IntPrimitive) as val from SupportBean";
            stmt = _epService.EPAdministrator.CreateEPL(text, (Object) "my_user_object");
            stmt.Events += _listener.Update;
    
            MySingleRowFunction.MethodInvokeContexts.Clear();
            RunAssertionSingleMethod();
            var context = MySingleRowFunction.MethodInvokeContexts[0];
            Assert.AreEqual("A", context.StatementName);
            Assert.AreEqual(_epService.URI, context.EngineURI);
            Assert.AreEqual(-1, context.ContextPartitionId);
            Assert.AreEqual("power3Context", context.FunctionName);
            Assert.AreEqual("my_user_object", context.StatementUserObject);
    
            stmt.Dispose();
    
            // test exception behavior
            // logged-only
            _epService.EPAdministrator.CreateEPL("select throwExceptionLogMe() from SupportBean").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPAdministrator.DestroyAllStatements();
    
            // rethrow
            _epService.EPAdministrator.CreateEPL("@Name('S0') select throwExceptionRethrow() from SupportBean").Events += _listener.Update;
            try {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex) {
                Assert.AreEqual("com.espertech.esper.client.EPException: Unexpected exception in statement 'S0': Invocation exception when invoking method 'ThrowException' of class 'com.espertech.esper.regression.client.MySingleRowFunction' passing parameters [] for statement 'S0': System.Exception : This is a 'throwexception' generated exception", ex.Message);
                _epService.EPAdministrator.DestroyAllStatements();
            }
    
            // NPE when boxed is null
            _epService.EPAdministrator.CreateEPL("@Name('S1') select power3Rethrow(IntBoxed) from SupportBean").Events += _listener.Update;
            try {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex) {
                Assert.AreEqual("com.espertech.esper.client.EPException: Unexpected exception in statement 'S1': NullPointerException invoking method 'ComputePower3' of class 'com.espertech.esper.regression.client.MySingleRowFunction' in parameter 0 passing parameters [null] for statement 'S1': The method expects a primitive Int32 value but received a null value", ex.Message);
            }
        }
    
        private void RunAssertionChainMethod()
        {
            var fields = new String[] {"val"};
            _epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{36});
    
            _listener.Reset();
        }
    
        private void RunAssertionSingleMethod()
        {
            var fields = new String[] {"val"};
            _epService.EPRuntime.SendEvent(new SupportBean("a", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{8});
    
            _listener.Reset();
        }
    
        [Test]
        public void TestFailedValidation()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddPlugInSingleRowFunction("singlerow", typeof(MySingleRowFunctionTwo).FullName, "TestSingleRow");
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
    
            try
            {
                var text = "select Singlerow('a', 'b') from " + typeof(SupportBean).FullName;
                _epService.EPAdministrator.CreateEPL(text);
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'Singlerow(\"a\",\"b\")': Could not find static method named 'TestSingleRow' in class 'com.espertech.esper.regression.client.MySingleRowFunctionTwo' with matching parameter number and expected parameter type(s) 'System.String, System.String' (nearest match found was 'TestSingleRow' taking type(s) 'System.String, System.Int32') [select Singlerow('a', 'b') from com.espertech.esper.support.bean.SupportBean]", ex.Message);
            }
        }
    
        [Test]
        public void TestInvalidConfigure()
        {
            TryInvalidConfigure("a b", "MyClass", "some");
            TryInvalidConfigure("abc", "My Class", "other s");
    
            // configured twice
            try
            {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("concatstring", typeof(MySingleRowFunction).FullName, "xyz");
                _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunctionFactory).FullName);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                // expected
            }
    
            // configured twice
            try
            {
                _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("teststring", typeof(MyConcatAggregationFunctionFactory).FullName);
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("teststring", typeof(MySingleRowFunction).FullName, "xyz");
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                // expected
            }
        }
    
        private void TryInvalidConfigure(String funcName, String className, String methodName)
        {
            try
            {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(funcName, className, methodName);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                // expected
            }
        }
    
        public static bool LocalIsNullValue(EventBean @event, String propertyName) {
            return @event.Get(propertyName) == null;
        }
    }
}
