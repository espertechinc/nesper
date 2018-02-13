///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;
    using Collection = ICollection<object>;

    public class ExecClientSingleRowFunctionPlugIn : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddPlugInSingleRowFunction(
                "singlerow", typeof(MySingleRowFunctionTwo).FullName, "testSingleRow");
            configuration.AddPlugInSingleRowFunction("power3", typeof(MySingleRowFunction).FullName, "computePower3");
            configuration.AddPlugInSingleRowFunction("chainTop", typeof(MySingleRowFunction).FullName, "getChainTop");
            configuration.AddPlugInSingleRowFunction("surroundx", typeof(MySingleRowFunction).FullName, "surroundx");
            configuration.AddPlugInSingleRowFunction(
                "throwExceptionLogMe", typeof(MySingleRowFunction).FullName, "throwexception",
                ValueCacheEnum.DISABLED,
                FilterOptimizableEnum.ENABLED, false);
            configuration.AddPlugInSingleRowFunction(
                "throwExceptionRethrow", typeof(MySingleRowFunction).FullName, "throwexception",
                ValueCacheEnum.DISABLED,
                FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction(
                "power3Rethrow", typeof(MySingleRowFunction).FullName, "computePower3",
                ValueCacheEnum.DISABLED,
                FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction(
                "power3Context", typeof(MySingleRowFunction).FullName, "computePower3WithContext",
                ValueCacheEnum.DISABLED,
                FilterOptimizableEnum.ENABLED, true);
            configuration.AddPlugInSingleRowFunction(
                "isNullValue", typeof(MySingleRowFunction).FullName, "isNullValue");
            configuration.AddPlugInSingleRowFunction(
                "getValueAsString", typeof(MySingleRowFunction).FullName, "getValueAsString");
            configuration.AddPlugInSingleRowFunction(
                "eventsCheckStrings", typeof(MySingleRowFunction).FullName, "eventsCheckStrings");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyInt", typeof(MySingleRowFunction).FullName, "varargsOnlyInt");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyString", typeof(MySingleRowFunction).FullName, "varargsOnlyString");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyObject", typeof(MySingleRowFunction).FullName, "varargsOnlyObject");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyNumber", typeof(MySingleRowFunction).FullName, "varargsOnlyNumber");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyISupportBaseAB", typeof(MySingleRowFunction).FullName, "varargsOnlyISupportBaseAB");
            configuration.AddPlugInSingleRowFunction(
                "varargsW1Param", typeof(MySingleRowFunction).FullName, "varargsW1Param");
            configuration.AddPlugInSingleRowFunction(
                "varargsW2Param", typeof(MySingleRowFunction).FullName, "varargsW2Param");
            configuration.AddPlugInSingleRowFunction(
                "varargsOnlyWCtx", typeof(MySingleRowFunction).FullName, "varargsOnlyWCtx");
            configuration.AddPlugInSingleRowFunction(
                "varargsW1ParamWCtx", typeof(MySingleRowFunction).FullName, "varargsW1ParamWCtx");
            configuration.AddPlugInSingleRowFunction(
                "varargsW2ParamWCtx", typeof(MySingleRowFunction).FullName, "varargsW2ParamWCtx");
            configuration.AddPlugInSingleRowFunction(
                "varargsObjectsWCtx", typeof(MySingleRowFunction).FullName, "varargsObjectsWCtx");
            configuration.AddPlugInSingleRowFunction(
                "varargsW1ParamObjectsWCtx", typeof(MySingleRowFunction).FullName, "varargsW1ParamObjectsWCtx");
            configuration.AddEventType<SupportBean>();
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionReturnTypeIsEvents(epService);
            RunAssertionVarargs(epService);
            RunAssertionEventBeanFootprint(epService);
            RunAssertionPropertyOrSingleRowMethod(epService);
            RunAssertionChainMethod(epService);
            RunAssertionSingleMethod(epService);
            RunAssertionFailedValidation(epService);
            RunAssertionInvalidConfigure(epService);
        }

        private void RunAssertionReturnTypeIsEvents(EPServiceProvider epService)
        {
            TryAssertionReturnTypeIsEvents(epService, "myItemProducerEventBeanArray");
            TryAssertionReturnTypeIsEvents(epService, "myItemProducerEventBeanCollection");
            TryAssertionReturnTypeIsEventsInvalid(epService);
        }

        private void TryAssertionReturnTypeIsEvents(EPServiceProvider epService, string methodName)
        {
            var entry = new ConfigurationPlugInSingleRowFunction();
            entry.Name = methodName;
            entry.FunctionClassName = GetType().FullName;
            entry.FunctionMethodName = methodName;
            entry.EventTypeName = "MyItem";
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);

            epService.EPAdministrator.CreateEPL("create schema MyItem(id string)");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " + methodName + "(theString).Where(v => v.id in ('id1', 'id3')) as c0 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("id0,id1,id2,id3,id4", 0));
            var coll = (ICollection<Map>) listener.AssertOneGetNewAndReset().Get("c0");
            EPAssertionUtil.AssertPropsPerRow(
                coll.ToArray(), "id".Split(','), new[] {new object[] {"id1"}, new object[] {"id3"}});

            stmt.Dispose();
        }

        private void TryAssertionReturnTypeIsEventsInvalid(EPServiceProvider epService)
        {
            var entry = new ConfigurationPlugInSingleRowFunction();
            entry.FunctionClassName = GetType().FullName;
            entry.FunctionMethodName = "myItemProducerEventBeanArray";

            // test invalid: no event type name
            entry.Name = "myItemProducerInvalidNoType";
            entry.EventTypeName = null;
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);
            epService.EPAdministrator.CreateEPL("select MyItemProducerInvalidNoType(theString) as c0 from SupportBean");
            SupportMessageAssertUtil.TryInvalid(
                epService,
                "select MyItemProducerInvalidNoType(theString).Where(v => v.id='id1') as c0 from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'MyItemProducerInvalidNoType(theStri...(68 chars)': Method 'myItemProducerEventBeanArray' returns EventBean-array but does not provide the event type name [");

            // test invalid: event type name invalid
            entry.Name = "myItemProducerInvalidWrongType";
            entry.EventTypeName = "dummy";
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);
            SupportMessageAssertUtil.TryInvalid(
                epService,
                "select MyItemProducerInvalidWrongType(theString).Where(v => v.id='id1') as c0 from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'MyItemProducerInvalidWrongType(theS...(74 chars)': Method 'myItemProducerEventBeanArray' returns event type 'dummy' and the event type cannot be found [select MyItemProducerInvalidWrongType(theString).Where(v => v.id='id1') as c0 from SupportBean]");

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionVarargs(EPServiceProvider epService)
        {
            RunVarargAssertion(
                epService,
                MakePair("VarargsOnlyInt(1, 2, 3, 4)", "1,2,3,4"),
                MakePair("VarargsOnlyInt(1, 2, 3)", "1,2,3"),
                MakePair("VarargsOnlyInt(1, 2)", "1,2"),
                MakePair("VarargsOnlyInt(1)", "1"),
                MakePair("VarargsOnlyInt()", ""));

            RunVarargAssertion(
                epService, MakePair("VarargsW1Param('abc', 1.0, 2.0)", "abc,1.0,2.0"),
                MakePair("VarargsW1Param('abc', 1, 2)", "abc,1.0,2.0"),
                MakePair("VarargsW1Param('abc', 1)", "abc,1.0"),
                MakePair("VarargsW1Param('abc')", "abc"));

            RunVarargAssertion(
                epService, MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                MakePair("VarargsW2Param(1, 2.0, 3L)", "1,2.0,3"),
                MakePair("VarargsW2Param(1, 2.0)", "1,2.0"),
                MakePair("VarargsW2Param(1, 2.0, 3, 4L)", "1,2.0,3,4"),
                MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                MakePair("VarargsW2Param(1, 2.0, 3, 4)", "1,2.0,3,4"),
                MakePair("VarargsW2Param(1, 2.0, 3L, 4)", "1,2.0,3,4"));

            RunVarargAssertion(
                epService, MakePair("VarargsOnlyWCtx(1, 2, 3)", "CTX+1,2,3"),
                MakePair("VarargsOnlyWCtx(1, 2)", "CTX+1,2"),
                MakePair("VarargsOnlyWCtx(1)", "CTX+1"),
                MakePair("VarargsOnlyWCtx()", "CTX+"));

            RunVarargAssertion(
                epService, MakePair("VarargsW1ParamWCtx('a', 1, 2, 3)", "CTX+a,1,2,3"),
                MakePair("VarargsW1ParamWCtx('a', 1, 2)", "CTX+a,1,2"),
                MakePair("VarargsW1ParamWCtx('a', 1)", "CTX+a,1"),
                MakePair("VarargsW1ParamWCtx('a')", "CTX+a,"));

            RunVarargAssertion(
                epService, MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2, 3)", "CTX+a,b,1,2,3"),
                MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2)", "CTX+a,b,1,2"),
                MakePair("VarargsW2ParamWCtx('a', 'b', 1)", "CTX+a,b,1"),
                MakePair("VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"),
                MakePair(typeof(MySingleRowFunction).FullName + ".VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"));

            RunVarargAssertion(
                epService, MakePair("VarargsOnlyObject('a', 1, new BigInteger('2'))", "a,1,2"));

            RunVarargAssertion(
                epService, MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger('4'))", "1.0,2,3,4"));

            RunVarargAssertion(
                epService, MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger('4'))", "1.0,2,3,4"));

            RunVarargAssertion(
                epService,
                MakePair(
                    "VarargsOnlyISupportBaseAB(new " + typeof(ISupportBImpl).Name + "('a', 'b'))",
                    "ISupportBImpl{valueB='a', valueBaseAB='b'}"));

            // tests for array-passthru
            RunVarargAssertion(
                epService, MakePair("VarargsOnlyString({'a'})", "a"),
                MakePair("VarargsOnlyString({'a', 'b'})", "a,b"),
                MakePair("VarargsOnlyObject({'a', 'b'})", "a,b"),
                MakePair("VarargsOnlyObject({})", ""),
                MakePair("VarargsObjectsWCtx({1, 'a'})", "CTX+1,a"),
                MakePair("VarargsW1ParamObjectsWCtx(1, {'a', 1})", "CTX+,1,a,1")
            );

            // try Arrays.asList
            TryAssertionArraysAsList(epService);
        }

        private void RunAssertionEventBeanFootprint(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport(this.GetType());

            // test select-clause
            var fields = new[] {"c0", "c1"};
            var text = "select IsNullValue(*, 'theString') as c0," +
                       "ExecClientSingleRowFunctionPlugIn.LocalIsNullValue(*, 'theString') as c1 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false});

            epService.EPRuntime.SendEvent(new SupportBean(null, 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, true});
            stmt.Dispose();

            // test pattern
            var textPattern =
                "select * from pattern [a=SupportBean -> b=SupportBean(theString=GetValueAsString(a, 'theString'))]";
            var stmtPattern = epService.EPAdministrator.CreateEPL(textPattern);
            stmtPattern.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "a.intPrimitive,b.intPrimitive".Split(','), new object[] {1, 2});
            stmtPattern.Dispose();

            // test filter
            var textFilter = "select * from SupportBean('E1'=GetValueAsString(*, 'theString'))";
            var stmtFilter = epService.EPAdministrator.CreateEPL(textFilter);
            stmtFilter.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, listener.GetAndResetLastNewData().Length);
            stmtFilter.Dispose();

            // test "first"
            var textAccessAgg =
                "select * from SupportBean#keepall having 'E2' = GetValueAsString(last(*), 'theString')";
            var stmtAccessAgg = epService.EPAdministrator.CreateEPL(textAccessAgg);
            stmtAccessAgg.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, listener.GetAndResetLastNewData().Length);
            stmtAccessAgg.Dispose();

            // test "window"
            var textWindowAgg =
                "select * from SupportBean#keepall having EventsCheckStrings(window(*), 'theString', 'E1')";
            var stmtWindowAgg = epService.EPAdministrator.CreateEPL(textWindowAgg);
            stmtWindowAgg.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.AreEqual(1, listener.GetAndResetLastNewData().Length);
            stmtWindowAgg.Dispose();
        }

        private void RunAssertionPropertyOrSingleRowMethod(EPServiceProvider epService)
        {
            var text = "select Surroundx('test') as val from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            var fields = new[] {"val"};
            epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"XtestX"});
        }

        private void RunAssertionChainMethod(EPServiceProvider epService)
        {
            var text = "select ChainTop().ChainValue(12,intPrimitive) as val from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            TryAssertionChainMethod(epService, listener);

            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, stmt.Text);
            stmt.AddListener(listener);

            TryAssertionChainMethod(epService, listener);
        }

        private void RunAssertionSingleMethod(EPServiceProvider epService)
        {
            var text = "select Power3(intPrimitive) as val from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            TryAssertionSingleMethod(epService, listener);

            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, stmt.Text);
            stmt.AddListener(listener);

            TryAssertionSingleMethod(epService, listener);

            stmt.Dispose();
            text = "select Power3(2) as val from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(text);
            stmt.AddListener(listener);

            TryAssertionSingleMethod(epService, listener);
            stmt.Dispose();

            // test passing a context as well
            text = "@Name('A') select Power3Context(intPrimitive) as val from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(text, (object) "my_user_object");
            stmt.AddListener(listener);

            MySingleRowFunction.MethodInvokeContexts.Clear();
            TryAssertionSingleMethod(epService, listener);
            var context = MySingleRowFunction.MethodInvokeContexts[0];
            Assert.AreEqual("A", context.StatementName);
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(-1, context.ContextPartitionId);
            Assert.AreEqual("power3Context", context.FunctionName);
            Assert.AreEqual("my_user_object", context.StatementUserObject);

            stmt.Dispose();

            // test exception behavior
            // logged-only
            epService.EPAdministrator.CreateEPL("select ThrowExceptionLogMe() from SupportBean").AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPAdministrator.DestroyAllStatements();

            // rethrow
            epService.EPAdministrator.CreateEPL("@Name('S0') select ThrowExceptionRethrow() from SupportBean")
                .AddListener(listener);
            try
            {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "java.lang.RuntimeException: Unexpected exception in statement 'S0': Invocation exception when invoking method 'throwexception' of class 'com.espertech.esper.supportregression.client.MySingleRowFunction' passing parameters [] for statement 'S0': RuntimeException : This is a 'throwexception' generated exception",
                    ex.Message);
                epService.EPAdministrator.DestroyAllStatements();
            }

            // NPE when boxed is null
            epService.EPAdministrator.CreateEPL("@Name('S1') select Power3Rethrow(intBoxed) from SupportBean")
                .AddListener(listener);
            try
            {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "java.lang.RuntimeException: Unexpected exception in statement 'S1': NullPointerException invoking method 'computePower3' of class 'com.espertech.esper.supportregression.client.MySingleRowFunction' in parameter 0 passing parameters [null] for statement 'S1': The method expects a primitive int value but received a null value",
                    ex.Message);
            }
        }

        private void TryAssertionChainMethod(EPServiceProvider epService, SupportUpdateListener listener)
        {
            var fields = new[] {"val"};
            epService.EPRuntime.SendEvent(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {36});

            listener.Reset();
        }

        private void TryAssertionSingleMethod(EPServiceProvider epService, SupportUpdateListener listener)
        {
            var fields = new[] {"val"};
            epService.EPRuntime.SendEvent(new SupportBean("a", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {8});

            listener.Reset();
        }

        private void RunAssertionFailedValidation(EPServiceProvider epService)
        {
            try
            {
                var text = "select Singlerow('a', 'b') from " + typeof(SupportBean).FullName;
                epService.EPAdministrator.CreateEPL(text);
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex,
                    "Error starting statement: Failed to validate select-clause expression 'Singlerow(\"a\",\"b\")': Could not find static method named 'testSingleRow' in class 'com.espertech.esper.supportregression.client.MySingleRowFunctionTwo' with matching parameter number and expected parameter Type(s) 'string, string' (nearest match found was 'testSingleRow' taking Type(s) 'string, int')");
            }
        }

        private void RunAssertionInvalidConfigure(EPServiceProvider epService)
        {
            TryInvalidConfigure(epService, "a b", "MyClass", "some");
            TryInvalidConfigure(epService, "abc", "My Type", "other s");

            // configured twice
            try
            {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "concatstring", typeof(MySingleRowFunction).FullName, "xyz");
                epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(
                    "concatstring", typeof(MyConcatAggregationFunctionFactory).Name);
                Assert.Fail();
            }
            catch (ConfigurationException)
            {
                // expected
            }

            // configured twice
            try
            {
                epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(
                    "teststring", typeof(MyConcatAggregationFunctionFactory).Name);
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "teststring", typeof(MySingleRowFunction).FullName, "xyz");
                Assert.Fail();
            }
            catch (ConfigurationException)
            {
                // expected
            }
        }

        private void TryInvalidConfigure(
            EPServiceProvider epService, string funcName, string className, string methodName)
        {
            try
            {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(funcName, className, methodName);
                Assert.Fail();
            }
            catch (ConfigurationException)
            {
                // expected
            }
        }

        public static bool LocalIsNullValue(EventBean @event, string propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        private void RunVarargAssertion(EPServiceProvider epService, params UniformPair<string>[] pairs)
        {
            var buf = new StringWriter();
            buf.Write("@Name('test') select ");
            var count = 0;
            foreach (var pair in pairs)
            {
                buf.Write(pair.First);
                buf.Write(" as c");
                buf.Write(Convert.ToString(count));
                count++;
                buf.Write(",");
            }

            buf.Write("intPrimitive from SupportBean");

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(buf.ToString()).AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean());
            var @out = listener.AssertOneGetNewAndReset();

            count = 0;
            foreach (var pair in pairs)
            {
                Assert.AreEqual(pair.Second, @out.Get("c" + count), "failed for '" + pair.First + "'");
                count++;
            }

            epService.EPAdministrator.GetStatement("test").Dispose();
        }

        private UniformPair<string> MakePair(string expression, string expected)
        {
            return new UniformPair<string>(expression, expected);
        }

        private void TryAssertionArraysAsList(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "java.util.Collections.List('a') as c0, " +
                "java.util.Collections.List({'a'}) as c1, " +
                "java.util.Collections.List('a', 'b') as c2, " +
                "java.util.Collections.List({'a', 'b'}) as c3 " +
                "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean());
            var @event = listener.AssertOneGetNewAndReset();
            AreEqualColl(@event, "c0", "a");
            AreEqualColl(@event, "c1", "a");
            AreEqualColl(@event, "c2", "a", "b");
            AreEqualColl(@event, "c3", "a", "b");

            stmt.Dispose();
        }

        private void AreEqualColl(EventBean @event, string property, params string[] values)
        {
            var data = (Collection) @event.Get(property);
            EPAssertionUtil.AssertEqualsExactOrder(values, data.ToArray());
        }

        public static EventBean[] MyItemProducerEventBeanArray(string @string, EPLMethodInvocationContext context)
        {
            var split = @string.Split(',');
            var events = new EventBean[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                events[i] = context.EventBeanService.AdapterForMap(
                    Collections.SingletonDataMap("id", split[i]), "MyItem");
            }

            return events;
        }

        public static ICollection<EventBean> MyItemProducerEventBeanCollection(
            string @string, EPLMethodInvocationContext context)
        {
            return Collections.List(MyItemProducerEventBeanArray(@string, context));
        }
    }
} // end of namespace