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
using com.espertech.esper.support.util;

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
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddMethodRef(typeof(MyStaticService), new ConfigurationMethodRef());
	        config.AddImport(typeof(MyStaticService));

	        config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
	        config.AddEventType(typeof(SupportBean));
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

	        // invalid footprint
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from method:MyConstantServiceVariable.FetchABean() as h0",
	                "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find enumeration method, date-time method or instance method named 'FetchABean' in class 'com.espertech.esper.regression.epl.TestFromClauseMethodVariable+MyConstantServiceVariable' taking no parameters (nearest match found was 'FetchABean' taking type(s) 'System.Int32') [");
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
	        string epl = "select id as c0 from SupportBean as sb, " +
	                   "method:MyConstantServiceVariable.FetchABean(intPrimitive) as h0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        SendEventAssert("E1", 10, "_10_");
	        SendEventAssert("E2", 20, "_20_");

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionNonConstantVariable(bool soda)
	    {
	        string modifyEPL = "on SupportBean_S0 set MyNonConstantServiceVariable.Postfix=p00";
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, modifyEPL);

	        string epl = "select id as c0 from SupportBean as sb, " +
                    "method:MyNonConstantServiceVariable.FetchABean(IntPrimitive) as h0";
	        EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
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
	        string[] fields = "c0".Split(',');
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
	}
} // end of namespace
