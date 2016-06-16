///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestAggregationFunctionPlugIn 
	{
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunctionFactory).FullName);
	        configuration.AddPlugInAggregationFunctionFactory("concatstringTwo", typeof(MyConcatTwoAggFunctionFactory).FullName);
	        configuration.EngineDefaults.ThreadingConfig.IsEngineFairlock = true;
	        _epService = EPServiceProviderManager.GetProvider("TestAggregationFunctionPlugIn", configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _epService.Initialize();
	    }

        [Test]
	    public void TestGrouped()
	    {
	        var textOne = "select irstream CONCATSTRING(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(10) group by IntPrimitive";
	        TryGrouped(textOne, null);

	        var textTwo = "select irstream concatstring(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(10) group by IntPrimitive";
	        TryGrouped(textTwo, null);

	        var textThree = "select irstream concatstring(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(10) group by IntPrimitive";
	        var model = _epService.EPAdministrator.CompileEPL(textThree);
	        SerializableObjectCopier.Copy(model);
	        Assert.AreEqual(textThree, model.ToEPL());
	        TryGrouped(null, model);

	        var textFour = "select irstream concatstring(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(10) group by IntPrimitive";
	        var modelTwo = new EPStatementObjectModel();
	        modelTwo.SelectClause = SelectClause.Create().SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
	                .Add(Expressions.PlugInAggregation("concatstring", Expressions.Property("TheString")), "val");
	        modelTwo.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView("win", "length", Expressions.Constant(10)));
	        modelTwo.GroupByClause = GroupByClause.Create("IntPrimitive");
	        Assert.AreEqual(textFour, modelTwo.ToEPL());
	        SerializableObjectCopier.Copy(modelTwo);
	        TryGrouped(null, modelTwo);

	        var textFive = "select irstream concatstringTwo(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(10) group by IntPrimitive";
	        TryGrouped(textFive, null);
	    }

	    private void TryGrouped(string text, EPStatementObjectModel model)
	    {
	        EPStatement statement;
	        if (model != null)
	        {
	            statement = _epService.EPAdministrator.Create(model);
	        }
	        else
	        {
	            statement = _epService.EPAdministrator.CreateEPL(text);
	        }
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("a", 1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

	        _epService.EPRuntime.SendEvent(new SupportBean("b", 2));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"b"}, new object[] {""});

	        _epService.EPRuntime.SendEvent(new SupportBean("c", 1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a c"}, new object[] {"a"});

	        _epService.EPRuntime.SendEvent(new SupportBean("d", 2));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"b d"}, new object[] {"b"});

	        _epService.EPRuntime.SendEvent(new SupportBean("e", 1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a c e"}, new object[] {"a c"});

	        _epService.EPRuntime.SendEvent(new SupportBean("f", 2));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"b d f"}, new object[] {"b d"});

	        listener.Reset();
	    }

        [Test]
	    public void TestWindow()
	    {
	        var text = "select irstream concatstring(TheString) as val from " + typeof(SupportBean).FullName + ".win:length(2)";
	        var statement = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("a", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

	        _epService.EPRuntime.SendEvent(new SupportBean("b", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a"});

	        _epService.EPRuntime.SendEvent(new SupportBean("c", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"b c"}, new object[] {"a b"});

	        _epService.EPRuntime.SendEvent(new SupportBean("d", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"c d"}, new object[] {"b c"});
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestDistinctAndStarParam()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        // test *-parameter
	        var textTwo = "select concatstring(*) as val from SupportBean";
	        var statementTwo = _epService.EPAdministrator.CreateEPL(textTwo);
	        var listenerTwo = new SupportUpdateListener();
	        statementTwo.AddListener(listenerTwo);

	        _epService.EPRuntime.SendEvent(new SupportBean("d", -1));
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "val".Split(','), new object[] {"SupportBean(d, -1)"});

	        _epService.EPRuntime.SendEvent(new SupportBean("e", 2));
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "val".Split(','), new object[] {"SupportBean(d, -1) SupportBean(e, 2)"});

	        try {
	            _epService.EPAdministrator.CreateEPL("select concatstring(*) as val from SupportBean.std:lastevent(), SupportBean unidirectional");
	        }
	        catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'concatstring(*)': The 'concatstring' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead");
	        }

	        // test distinct
	        var text = "select irstream concatstring(distinct TheString) as val from SupportBean";
	        var statement = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("a", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

	        _epService.EPRuntime.SendEvent(new SupportBean("b", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a"});

	        _epService.EPRuntime.SendEvent(new SupportBean("b", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a b"});

	        _epService.EPRuntime.SendEvent(new SupportBean("c", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a b c"}, new object[] {"a b"});

	        _epService.EPRuntime.SendEvent(new SupportBean("a", -1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a b c"}, new object[] {"a b c"});
	    }

        [Test]
	    public void TestArrayParamsAndDotMethod()
	    {
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("countback", typeof(SupportPluginAggregationMethodOneFactory).FullName);

	        var text = "select irstream countback({1,2,IntPrimitive}) as val from " + typeof(SupportBean).FullName;
	        var statement = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {-1}, new object[] {0});

	        // test dot-method
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("myagg", typeof(MyAggFuncFactory).FullName);
	        var fields = "val0,val1".Split(',');
	        _epService.EPAdministrator.CreateEPL("select (myagg(id)).get_TheString() as val0, (myagg(id)).get_IntPrimitive() as val1 from SupportBean_A").AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"XX", 1});
	        Assert.AreEqual(1, MyAggFuncFactory.InstanceCount);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"XX", 2});
	    }

        [Test]
	    public void TestMultipleParams()
	    {
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("countboundary", typeof(SupportPluginAggregationMethodThreeFactory).FullName);

	        RunAssertionMultipleParams(false);
	        RunAssertionMultipleParams(true);
	    }

	    private void RunAssertionMultipleParams(bool soda) {

	        var text = "select irstream countboundary(1,10,IntPrimitive,*) as val from " + typeof(SupportBean).FullName;
	        var statement = SupportModelHelper.CreateByCompileOrParse(_epService, soda, text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        var validContext = SupportPluginAggregationMethodThreeFactory.Contexts[0];
	        EPAssertionUtil.AssertEqualsExactOrder(new Type[]{typeof(int?), typeof(int?), typeof(int?), typeof(SupportBean)}, validContext.ParameterTypes);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{1, 10, null, null}, validContext.ConstantValues);
	        EPAssertionUtil.AssertEqualsExactOrder(new bool[]{true, true, false, false}, validContext.IsConstantValue);

	        var e1 = new SupportBean("E1", 5);
	        _epService.EPRuntime.SendEvent(e1);
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {1}, new object[] {0});
	        EPAssertionUtil.AssertEqualsExactOrder(new object[] {1, 10, 5, e1}, SupportPluginAggregationMethodThree.LastEnterParameters);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {1}, new object[] {1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {1}, new object[] {1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {2}, new object[] {1});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestNoSubnodesRuntimeAdd()
	    {
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("countback", typeof(SupportPluginAggregationMethodOneFactory).FullName);

	        var text = "select irstream countback() as val from " + typeof(SupportBean).FullName;
	        var statement = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{-1}, new object[]{0});

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {-2}, new object[] {-1});
	    }

        [Test]
	    public void TestMappedPropertyLookAlike()
	    {
	        var text = "select irstream concatstring('a') as val from " + typeof(SupportBean).FullName;
	        var statement = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);
	        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("val"));

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a"}, new object[]{""});

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a a"}, new object[] {"a"});

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[] {"a a a"}, new object[] {"a a"});
	    }

        [Test]
	    public void TestFailedValidation()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddPlugInAggregationFunctionFactory("concat", typeof(SupportPluginAggregationMethodTwoFactory).FullName);
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        try
	        {
                var text = "select concat(1) from " + typeof(SupportBean).FullName;
	            _epService.EPAdministrator.CreateEPL(text);
	        }
	        catch (EPStatementException ex)
	        {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'concat(1)': Plug-in aggregation function 'concat' failed validation: Invalid parameter type '" + Name.Of<int>() + "', expecting string [");
            }
	    }

        [Test]
	    public void TestInvalidUse()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddPlugInAggregationFunctionFactory("xxx", typeof(object).FullName);
	        configuration.AddPlugInAggregationFunctionFactory("yyy", "com.NoSuchClass");
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        try
	        {
	            var text = "select * from " + typeof(SupportBean).FullName + " group by xxx(1)";
	            _epService.EPAdministrator.CreateEPL(text);
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual("Error in expression: Error resolving aggregation: Aggregation class by name 'System.Object' does not implement AggregationFunctionFactory [select * from com.espertech.esper.support.bean.SupportBean group by xxx(1)]", ex.Message);
	        }

	        try
	        {
	            var text = "select * from " + typeof(SupportBean).FullName + " group by yyy(1)";
	            _epService.EPAdministrator.CreateEPL(text);
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual("Error in expression: Error resolving aggregation: Could not load aggregation factory class by name 'com.NoSuchClass' [select * from com.espertech.esper.support.bean.SupportBean group by yyy(1)]", ex.Message);
	        }
	    }

        [Test]
	    public void TestInvalidConfigure()
	    {
	        TryInvalidConfigure("a b", "MyClass");
	        TryInvalidConfigure("abc", "My Class");

	        // configure twice
	        try
	        {
	            _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunction).FullName);
                Assert.Fail();
	        }
	        catch (ConfigurationException)
	        {
	            // expected
	        }
	    }

	    private void TryInvalidConfigure(string funcName, string className)
	    {
	        try
	        {
	            _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(funcName, className);
                Assert.Fail();
	        }
	        catch (ConfigurationException)
	        {
	            // expected
	        }
	    }

        [Test]
	    public void TestInvalid()
	    {
	        TryInvalid("select xxx(TheString) from " + typeof(SupportBean).FullName,
	                "Error starting statement: Failed to validate select-clause expression 'xxx(TheString)': Unknown single-row function, aggregation function or mapped or indexed property named 'xxx' could not be resolved [select xxx(TheString) from com.espertech.esper.support.bean.SupportBean]");
	    }

	    private void TryInvalid(string stmtText, string expectedMsg)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(stmtText);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(expectedMsg, ex.Message);
	        }
	    }

        public class MyAggFuncFactory : AggregationFunctionFactory
        {
            private static int _instanceCount;

            public static int InstanceCount
            {
                get { return _instanceCount; }
            }

            public string FunctionName
            {
                set { }
            }

            public void Validate(AggregationValidationContext validationContext)
            {
            }

            public AggregationMethod NewAggregator()
            {
                _instanceCount++;
                return new MyAggFuncMethod();
            }

            public Type ValueType
            {
                get { return typeof (SupportBean); }
            }
        }

        public class MyAggFuncMethod : AggregationMethod
        {
            private int _count;

            public void Enter(object value)
            {
                _count++;
            }

            public void Leave(object value)
            {
                _count--;
            }

            public object Value
            {
                get { return new SupportBean("XX", _count); }
            }

            public Type ValueType
            {
                get { return typeof (SupportBean); }
            }

            public void Clear()
            {
                _count = 0;
            }
        }
	}
} // end of namespace
