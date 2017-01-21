///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateExtLeaving
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestLeaving()
	    {
	        var epl = "select leaving() as val from SupportBean.win:length(3)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertion();

	        stmt.Dispose();
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, model.ToEPL());

	        RunAssertion();

	        TryInvalid("select leaving(1) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'leaving(1)': The 'leaving' function expects no parameters");
	    }

	    private void RunAssertion() {
	        var fields = "val".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{true});
	    }

	    private void TryInvalid(string epl, string message) {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, ex.Message);
	        }
	    }
	}
} // end of namespace
