///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;

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
            configuration.AddImport(typeof(BigInteger));

            configuration.AddPlugInSingleRowFunction("power3", typeof(MySingleRowFunction).FullName, "ComputePower3");
            configuration.AddPlugInSingleRowFunction("chainTop", typeof(MySingleRowFunction).FullName, "GetChainTop");
            configuration.AddPlugInSingleRowFunction("surroundx", typeof(MySingleRowFunction).FullName, "Surroundx");
            configuration.AddPlugInSingleRowFunction("throwExceptionLogMe", typeof(MySingleRowFunction).FullName, "ThrowException", ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, false);
            configuration.AddPlugInSingleRowFunction("throwExceptionRethrow", typeof(MySingleRowFunction).FullName, "ThrowException", ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("power3Rethrow", typeof(MySingleRowFunction).FullName, "ComputePower3", ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("power3Context", typeof(MySingleRowFunction).FullName, "ComputePower3WithContext", ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction("isNullValue", typeof(MySingleRowFunction).FullName, "IsNullValue");
            configuration.AddPlugInSingleRowFunction("getValueAsString", typeof(MySingleRowFunction).FullName, "GetValueAsString");
            configuration.AddPlugInSingleRowFunction("eventsCheckStrings", typeof(MySingleRowFunction).FullName, "EventsCheckStrings");

            configuration.AddPlugInSingleRowFunction("VarargsOnlyInt", typeof(MySingleRowFunction).FullName, "VarargsOnlyInt");
            configuration.AddPlugInSingleRowFunction("VarargsOnlyString", typeof(MySingleRowFunction).FullName, "VarargsOnlyString");
            configuration.AddPlugInSingleRowFunction("VarargsOnlyObject", typeof(MySingleRowFunction).FullName, "VarargsOnlyObject");
            configuration.AddPlugInSingleRowFunction("VarargsOnlyNumber", typeof(MySingleRowFunction).FullName, "VarargsOnlyNumber");
            configuration.AddPlugInSingleRowFunction("VarargsOnlyISupportBaseAB", typeof(MySingleRowFunction).FullName, "VarargsOnlyISupportBaseAB");
            configuration.AddPlugInSingleRowFunction("VarargsW1Param", typeof(MySingleRowFunction).FullName, "VarargsW1Param");
            configuration.AddPlugInSingleRowFunction("VarargsW2Param", typeof(MySingleRowFunction).FullName, "VarargsW2Param");
            configuration.AddPlugInSingleRowFunction("VarargsOnlyWCtx", typeof(MySingleRowFunction).FullName, "VarargsOnlyWCtx");
            configuration.AddPlugInSingleRowFunction("VarargsW1ParamWCtx", typeof(MySingleRowFunction).FullName, "VarargsW1ParamWCtx");
            configuration.AddPlugInSingleRowFunction("VarargsW2ParamWCtx", typeof(MySingleRowFunction).FullName, "VarargsW2ParamWCtx");
            configuration.AddPlugInSingleRowFunction("VarargsObjectsWCtx", typeof(MySingleRowFunction).FullName, "VarargsObjectsWCtx");
            configuration.AddPlugInSingleRowFunction("VarargsW1ParamObjectsWCtx", typeof(MySingleRowFunction).FullName, "VarargsW1ParamObjectsWCtx");

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
        
        [Test]
        public void TestReturnTypeIsEvents()
        {
            RunAssertionReturnTypeIsEvents("MyItemProducerEventBeanArray");
            RunAssertionReturnTypeIsEvents("MyItemProducerEventBeanCollection");
            RunAssertionReturnTypeIsEventsInvalid();
        }

        private void RunAssertionReturnTypeIsEvents(String methodName)
        {
            ConfigurationPlugInSingleRowFunction entry = new ConfigurationPlugInSingleRowFunction();
            entry.Name = methodName;
            entry.FunctionClassName = this.GetType().FullName;
            entry.FunctionMethodName = methodName;
            entry.EventTypeName = "MyItem";
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);

            _epService.EPAdministrator.CreateEPL("create schema MyItem(id string)");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select " + methodName + "(theString).where(v => v.id in ('id1', 'id3')) as c0 from SupportBean");
            stmt.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("id0,id1,id2,id3,id4", 0));
            var coll = _listener.AssertOneGetNewAndReset().Get("c0").UnwrapIntoArray<IDictionary<string, object>>();
            EPAssertionUtil.AssertPropsPerRow(coll, "id".SplitCsv(), new Object[][] { new Object[] { "id1" }, new Object[] { "id3" } });

            stmt.Dispose();
        }

        private void RunAssertionReturnTypeIsEventsInvalid()
        {
            ConfigurationPlugInSingleRowFunction entry = new ConfigurationPlugInSingleRowFunction();
            entry.FunctionClassName = this.GetType().FullName;
            entry.FunctionMethodName = "MyItemProducerEventBeanArray";

            // test invalid: no event type name
            entry.Name = "myItemProducerInvalidNoType";
            entry.EventTypeName = null;
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);
            _epService.EPAdministrator.CreateEPL("select myItemProducerInvalidNoType(theString) as c0 from SupportBean");
            SupportMessageAssertUtil.TryInvalid(
                _epService,
                "select myItemProducerInvalidNoType(theString).where(v => v.id='id1') as c0 from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'myItemProducerInvalidNoType(theStri...(68 chars)': Method 'MyItemProducerEventBeanArray' returns EventBean-array but does not provide the event type name [");

            // test invalid: event type name invalid
            entry.Name = "myItemProducerInvalidWrongType";
            entry.EventTypeName = "dummy";
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);
            SupportMessageAssertUtil.TryInvalid(
                _epService,
                "select myItemProducerInvalidWrongType(theString).where(v => v.id='id1') as c0 from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'myItemProducerInvalidWrongType(theS...(74 chars)': Method 'MyItemProducerEventBeanArray' returns event type 'dummy' and the event type cannot be found [select myItemProducerInvalidWrongType(theString).where(v => v.id='id1') as c0 from SupportBean]");
        }

        [Test]
        public void TestVarargs()
        {
            RunVarargAssertion(
                    MakePair("VarargsOnlyInt(1, 2, 3, 4)", "1,2,3,4"),
                    MakePair("VarargsOnlyInt(1, 2, 3)", "1,2,3"),
                    MakePair("VarargsOnlyInt(1, 2)", "1,2"),
                    MakePair("VarargsOnlyInt(1)", "1"),
                    MakePair("VarargsOnlyInt()", ""));

            RunVarargAssertion(
                    MakePair("VarargsW1Param('abc', 1.0, 2.0)", "abc,1.0,2.0"),
                    MakePair("VarargsW1Param('abc', 1, 2)", "abc,1.0,2.0"),
                    MakePair("VarargsW1Param('abc', 1)", "abc,1.0"),
                    MakePair("VarargsW1Param('abc')", "abc"));

            RunVarargAssertion(
                    MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                    MakePair("VarargsW2Param(1, 2.0, 3L)", "1,2.0,3"),
                    MakePair("VarargsW2Param(1, 2.0)", "1,2.0"),
                    MakePair("VarargsW2Param(1, 2.0, 3, 4L)", "1,2.0,3,4"),
                    MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                    MakePair("VarargsW2Param(1, 2.0, 3, 4)", "1,2.0,3,4"),
                    MakePair("VarargsW2Param(1, 2.0, 3L, 4)", "1,2.0,3,4"));

            RunVarargAssertion(
                    MakePair("VarargsOnlyWCtx(1, 2, 3)", "CTX+1,2,3"),
                    MakePair("VarargsOnlyWCtx(1, 2)", "CTX+1,2"),
                    MakePair("VarargsOnlyWCtx(1)", "CTX+1"),
                    MakePair("VarargsOnlyWCtx()", "CTX+"));

            RunVarargAssertion(
                    MakePair("VarargsW1ParamWCtx('a', 1, 2, 3)", "CTX+a,1,2,3"),
                    MakePair("VarargsW1ParamWCtx('a', 1, 2)", "CTX+a,1,2"),
                    MakePair("VarargsW1ParamWCtx('a', 1)", "CTX+a,1"),
                    MakePair("VarargsW1ParamWCtx('a')", "CTX+a,"));

            RunVarargAssertion(
                    MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2, 3)", "CTX+a,b,1,2,3"),
                    MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2)", "CTX+a,b,1,2"),
                    MakePair("VarargsW2ParamWCtx('a', 'b', 1)", "CTX+a,b,1"),
                    MakePair("VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"),
                    MakePair(typeof(MySingleRowFunction).FullName + ".VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"));

            RunVarargAssertion(
                    MakePair("VarargsOnlyObject('a', 1, new BigInteger(2))", "a,1,2"));

            RunVarargAssertion(
                    MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger(4))", "1.0,2,3,4"));

            RunVarargAssertion(
                    MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger(4))", "1.0,2,3,4"));

            RunVarargAssertion(
                    MakePair("VarargsOnlyISupportBaseAB(new " + typeof(ISupportBImpl).FullName + "('a', 'b'))", "ISupportBImpl{valueB='a', valueBaseAB='b'}"));

            // tests for array-passthru
            RunVarargAssertion(
                    MakePair("VarargsOnlyString({'a'})", "a"),
                    MakePair("VarargsOnlyString({'a', 'b'})", "a,b"),
                    MakePair("VarargsOnlyObject({'a', 'b'})", "a,b"),
                    MakePair("VarargsOnlyObject({})", ""),
                    MakePair("VarargsObjectsWCtx({1, 'a'})", "CTX+1,a"),
                    MakePair("VarargsW1ParamObjectsWCtx(1, {'a', 1})", "CTX+,1,a,1")
                    );

            // try Arrays.asList
            RunAssertionArraysAsList();
        }

        [Test]
        public void TestEventBeanFootprint()
        {
            _epService.EPAdministrator.Configuration.AddImport(GetType());

            // test select-clause
            var fields = new String[] { "c0", "c1" };
            var text = "select isNullValue(*, 'TheString') as c0," +
                    "TestSingleRowFunctionPlugIn.LocalIsNullValue(*, 'TheString') as c1 from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, false });

            _epService.EPRuntime.SendEvent(new SupportBean(null, 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, true });
            stmt.Dispose();

            // test pattern
            var textPattern = "select * from pattern [a=SupportBean -> b=SupportBean(TheString=getValueAsString(a, 'TheString'))]";
            var stmtPattern = _epService.EPAdministrator.CreateEPL(textPattern);
            stmtPattern.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.IntPrimitive,b.IntPrimitive".Split(','), new Object[] { 1, 2 });
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
            var textAccessAgg = "select * from SupportBean#keepall having 'E2' = getValueAsString(last(*), 'TheString')";
            var stmtAccessAgg = _epService.EPAdministrator.CreateEPL(textAccessAgg);
            stmtAccessAgg.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
            stmtAccessAgg.Dispose();

            // test "window"
            var textWindowAgg = "select * from SupportBean#keepall having eventsCheckStrings(window(*), 'TheString', 'E1')";
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

            var fields = new String[] { "val" };
            _epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "XtestX" });
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
            stmt = _epService.EPAdministrator.CreateEPL(text, (Object)"my_user_object");
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
            try
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("com.espertech.esper.client.EPException: Unexpected exception in statement 'S0': Invocation exception when invoking method 'ThrowException' of class 'com.espertech.esper.regression.client.MySingleRowFunction' passing parameters [] for statement 'S0': System.Exception : This is a 'throwexception' generated exception", ex.Message);
                _epService.EPAdministrator.DestroyAllStatements();
            }

            // NPE when boxed is null
            _epService.EPAdministrator.CreateEPL("@Name('S1') select power3Rethrow(IntBoxed) from SupportBean").Events += _listener.Update;
            try
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("com.espertech.esper.client.EPException: Unexpected exception in statement 'S1': NullPointerException invoking method 'ComputePower3' of class 'com.espertech.esper.regression.client.MySingleRowFunction' in parameter 0 passing parameters [null] for statement 'S1': The method expects a primitive Int32 value but received a null value", ex.Message);
            }
        }

        private void RunAssertionChainMethod()
        {
            var fields = new String[] { "val" };
            _epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 36 });

            _listener.Reset();
        }

        private void RunAssertionSingleMethod()
        {
            var fields = new String[] { "val" };
            _epService.EPRuntime.SendEvent(new SupportBean("a", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 8 });

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
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'Singlerow(\"a\",\"b\")': Could not find static method named 'TestSingleRow' in class 'com.espertech.esper.regression.client.MySingleRowFunctionTwo' with matching parameter number and expected parameter type(s) 'System.String, System.String' (nearest match found was 'TestSingleRow' taking type(s) 'System.String, System.Int32') [select Singlerow('a', 'b') from " + Name.Of<SupportBean>() + "]");
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
            catch (ConfigurationException)
            {
                // expected
            }
        }

        public static bool LocalIsNullValue(EventBean @event, String propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        private void RunVarargAssertion(params UniformPair<String>[] pairs)
        {
            var buf = new StringBuilder();
            buf.Append("@Name('test') select ");
            int count = 0;
            foreach (UniformPair<String> pair in pairs)
            {
                buf.Append(pair.First);
                buf.Append(" as c");
                buf.Append(count);
                count++;
                buf.Append(",");
            }
            buf.Append("intPrimitive from SupportBean");

            _epService.EPAdministrator.CreateEPL(buf.ToString()).AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @out = _listener.AssertOneGetNewAndReset();

            count = 0;
            foreach (UniformPair<String> pair in pairs)
            {
                Assert.That(pair.Second, Is.EqualTo(@out.Get("c" + count)), "failed for '" + pair.First + "'");
                count++;
            }
            _epService.EPAdministrator.GetStatement("test").Dispose();
        }

        private UniformPair<String> MakePair(String expression, String expected)
        {
            return new UniformPair<String>(expression, expected);
        }

        private void RunAssertionArraysAsList()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select " +
                "com.espertech.esper.compat.collections.CompatExtensions.AsList('a') as c0, " +
                "com.espertech.esper.compat.collections.CompatExtensions.AsList({'a'}) as c1, " +
                "com.espertech.esper.compat.collections.CompatExtensions.AsList('a', 'b') as c2, " +
                "com.espertech.esper.compat.collections.CompatExtensions.AsList({'a', 'b'}) as c3 " +
                "from SupportBean");

            stmt.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEqualsColl(@event, "c0", "a");
            AssertEqualsColl(@event, "c1", "a");
            AssertEqualsColl(@event, "c2", "a", "b");
            AssertEqualsColl(@event, "c3", "a", "b");

            stmt.Dispose();
        }

        private void AssertEqualsColl(EventBean @event, String property, params String[] values)
        {
            var data = @event.Get(property).Unwrap<string>();
            EPAssertionUtil.AssertEqualsExactOrder(values, data);
        }

        public static EventBean[] MyItemProducerEventBeanArray(String @string, EPLMethodInvocationContext context)
        {
            String[] split = @string.SplitCsv();
            EventBean[] events = new EventBean[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                events[i] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("id", split[i]), "MyItem");
            }
            return events;
        }

        public static ICollection<EventBean> MyItemProducerEventBeanCollection(String @string, EPLMethodInvocationContext context)
        {
            return Collections.List(MyItemProducerEventBeanArray(@string, context));
        }
    }
}