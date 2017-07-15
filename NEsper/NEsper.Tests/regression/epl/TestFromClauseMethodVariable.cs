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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestFromClauseMethodVariable 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddMethodRef(typeof(MyStaticService), new ConfigurationMethodRef());
	        config.AddImport(typeof(MyStaticService));

	        config.EngineDefaults.Logging.IsEnableQueryPlan = true;
	        config.AddEventType<SupportBean>();
	        config.AddEventType(typeof(SupportBean_S0));
	        config.AddEventType(typeof(SupportBean_S1));
	        config.AddEventType(typeof(SupportBean_S2));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestVariables()
        {
	        _epService.EPAdministrator.Configuration.AddVariable("MyConstantServiceVariable", typeof(MyConstantServiceVariable), new MyConstantServiceVariable());
	        RunAssertionConstantVariable();

	        _epService.EPAdministrator.Configuration.AddVariable("MyNonConstantServiceVariable", typeof(MyNonConstantServiceVariable), new MyNonConstantServiceVariable("postfix"));
	        RunAssertionNonConstantVariable(false);
	        RunAssertionNonConstantVariable(true);

	        RunAssertionContextVariable();

            RunAssertionVariableMapAndOA();

	        // invalid footprint
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from method:MyConstantServiceVariable.FetchABean() as h0",
	            "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find enumeration method, date-time method or instance method named 'FetchABean' in class 'com.espertech.esper.regression.epl.TestFromClauseMethodVariable+MyConstantServiceVariable' taking no parameters (nearest match found was 'FetchABean' taking type(s) 'System.Int32') [");
            
            // null variable value and metadata is instance method
            _epService.EPAdministrator.Configuration.AddVariable("MyNullMap", typeof(MyMethodHandlerMap), null);
            SupportMessageAssertUtil.TryInvalid(_epService, "select field1, field2 from method:MyNullMap.GetMapData()",
                    "Error starting statement: Failed to access variable method invocation metadata: The variable value is null and the metadata method is an instance method");

            // variable with context and metadata is instance method
            _epService.EPAdministrator.CreateEPL("create context BetweenStartAndEnd start SupportBean end SupportBean");
            _epService.EPAdministrator.CreateEPL("context BetweenStartAndEnd create variable " + typeof(MyMethodHandlerMap).MaskTypeName() + " themap");
            SupportMessageAssertUtil.TryInvalid(_epService, "context BetweenStartAndEnd select field1, field2 from method:themap.GetMapData()",
                    "Error starting statement: Failed to access variable method invocation metadata: The metadata method is an instance method however the variable is contextual, please declare the metadata method as static or remove the context declaration for the variable");
        }

        private void RunAssertionVariableMapAndOA()
        {
            _epService.EPAdministrator.Configuration.AddVariable<MyMethodHandlerMap>("MyMethodHandlerMap", new MyMethodHandlerMap("a", "b"));
            _epService.EPAdministrator.Configuration.AddVariable<MyMethodHandlerOA>("MyMethodHandlerOA", new MyMethodHandlerOA("a", "b"));

            foreach (var epl in new String[] {
                    "select field1, field2 from method:MyMethodHandlerMap.GetMapData()",
                    "select field1, field2 from method:MyMethodHandlerOA.GetOAData()"
            }) {
                var stmt = _epService.EPAdministrator.CreateEPL(epl);
                EPAssertionUtil.AssertProps(stmt.First(), "field1,field2".SplitCsv(), new Object[] {"a", "b"});
            }
	    }

	    private void RunAssertionContextVariable()
        {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(MyNonConstantServiceVariableFactory));
	        _epService.EPAdministrator.Configuration.AddImport(typeof(MyNonConstantServiceVariable));

	        _epService.EPAdministrator.CreateEPL("create context MyContext " +
	                "initiated by SupportBean_S0 as c_s0 " +
	                "terminated by SupportBean_S1(id=c_s0.id)");
	        _epService.EPAdministrator.CreateEPL("context MyContext " +
	                "create variable MyNonConstantServiceVariable var = MyNonConstantServiceVariableFactory.Make()");
	        _epService.EPAdministrator.CreateEPL("context MyContext " +
	                "select id as c0 from SupportBean(intPrimitive=context.c_s0.id) as sb, " +
	                "method:var.FetchABean(intPrimitive) as h0").AddListener(_listener);
	        _epService.EPAdministrator.CreateEPL("context MyContext on SupportBean_S2(id = context.c_s0.id) set var.Postfix=p20");

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));

	        SendEventAssert("E1", 1, "_1_context_postfix");
	        SendEventAssert("E2", 2, "_2_context_postfix");

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(1, "a"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "b"));

	        SendEventAssert("E1", 1, "_1_a");
	        SendEventAssert("E2", 2, "_2_b");

	        // invalid context
            SupportMessageAssertUtil.TryInvalid(_epService, "select * from method:var.FetchABean(intPrimitive) as h0",
	                "Error starting statement: Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");
	        _epService.EPAdministrator.CreateEPL("create context ABC start @now end after 1 minute");
            SupportMessageAssertUtil.TryInvalid(_epService, "context ABC select * from method:var.FetchABean(intPrimitive) as h0",
	                "Error starting statement: Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");
	    }

	    private void RunAssertionConstantVariable()
	    {
	        var epl = "select id as c0 from SupportBean as sb, " +
	                   "method:MyConstantServiceVariable.FetchABean(intPrimitive) as h0";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        SendEventAssert("E1", 10, "_10_");
	        SendEventAssert("E2", 20, "_20_");

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionNonConstantVariable(bool soda)
	    {
	        var modifyEPL = "on SupportBean_S0 set MyNonConstantServiceVariable.Postfix=p00";
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, modifyEPL);

	        var epl = "select id as c0 from SupportBean as sb, " +
                    "method:MyNonConstantServiceVariable.FetchABean(IntPrimitive) as h0";
	        var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        SendEventAssert("E1", 10, "_10_postfix");

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "newpostfix"));
	        SendEventAssert("E1", 20, "_20_newpostfix");

	        // return to original value
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "postfix"));
	        SendEventAssert("E1", 30, "_30_postfix");

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void SendEventAssert(string theString, int intPrimitive, string expected)
        {
	        var fields = "c0".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{expected});
	    }

	    private class MyConstantServiceVariable {
	        public SupportBean_A FetchABean(int intPrimitive) {
	            return new SupportBean_A("_" + intPrimitive + "_");
	        }
	    }

	    [Serializable]
        public class MyNonConstantServiceVariable
        {
	        private string _postfix;

	        public MyNonConstantServiceVariable(string postfix)
            {
	            _postfix = postfix;
	        }

	        public string Postfix
	        {
	            get { return _postfix; }
	            set { _postfix = value; }
	        }

	        public SupportBean_A FetchABean(int intPrimitive)
            {
	            return new SupportBean_A("_" + intPrimitive + "_" + _postfix);
	        }
	    }

	    public class MyStaticService
        {
	        public static SupportBean_A FetchABean(int intPrimitive) {
	            return new SupportBean_A("_" + intPrimitive + "_");
	        }
	    }

	    public class MyNonConstantServiceVariableFactory
        {
	        public static MyNonConstantServiceVariable Make()
            {
	            return new MyNonConstantServiceVariable("context_postfix");
	        }
	    }

        public class MyMethodHandlerMap
        {
            private readonly string _field1;
            private readonly string _field2;

            public MyMethodHandlerMap(string field1, string field2)
            {
                _field1 = field1;
                _field2 = field2;
            }

            public IDictionary<string, object> GetMapDataMetadata()
            {
                var fields = new Dictionary<string, object>();
                fields.Put("field1", typeof (string));
                fields.Put("field2", typeof (string));
                return fields;
            }

            public IDictionary<string, object>[] GetMapData()
            {
                var maps = new IDictionary<string, object>[1];
                var row = new Dictionary<string, object>();
                maps[0] = row;
                row.Put("field1", _field1);
                row.Put("field2", _field2);
                return maps;
            }
        }

        public class MyMethodHandlerOA
        {
            private readonly string _field1;
            private readonly string _field2;

            public MyMethodHandlerOA(string field1, string field2)
            {
                _field1 = field1;
                _field2 = field2;
            }

            public static IDictionary<string, object> GetOADataMetadata()
            {
                var fields = new LinkedHashMap<String, Object>();
                fields.Put("field1", typeof (string));
                fields.Put("field2", typeof (string));
                return fields;
            }

            public object[][] GetOAData()
            {
                return new object[][] { new object[] {_field1, _field2}};
            }
        }
	}
} // end of namespace
