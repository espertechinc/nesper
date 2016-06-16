///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
	public class TestSelectExprEventBeanAnnotation
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestEventAggregationAndPrevWindow()
	    {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(col1 string)");
	        var eplInsert = "insert into DStream select " +
	                "last(*) @eventbean as c0, " +
	                "window(*) @eventbean as c1, " +
	                "prevwindow(s0) @eventbean as c2 " +
	                "from MyEvent.win:length(2) as s0";
	        var stmtInsert = _epService.EPAdministrator.CreateEPL(eplInsert);

	        foreach (var prop in "c0,c1,c2".Split(',')){
	            AssertFragment(prop, stmtInsert.EventType, "MyEvent", prop.Equals("c1") || prop.Equals("c2"));
	        }

	        // test consuming statement
	        var fields = "f0,f1,f2,f3,f4,f5".Split(',');
	        _epService.EPAdministrator.CreateEPL("select " +
	                "c0 as f0, " +
	                "c0.col1 as f1, " +
	                "c1 as f2, " +
	                "c1.lastOf().col1 as f3, " +
	                "c1 as f4, " +
	                "c1.lastOf().col1 as f5 " +
	                "from DStream").AddListener(_listener);

	        var eventOne = new object[] {"E1"};
	        _epService.EPRuntime.SendEvent(eventOne, "MyEvent");
	        var @out = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(@out, fields, new object[] {eventOne, "E1", new object[] {eventOne}, "E1", new object[] {eventOne}, "E1"});

	        var eventTwo = new object[] {"E2"};
	        _epService.EPRuntime.SendEvent(eventTwo, "MyEvent");
	        @out = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(@out, fields, new object[]{eventTwo, "E2", new object[]{eventOne, eventTwo}, "E2", new object[]{eventOne, eventTwo}, "E2"});

	        // test SODA
	        SupportModelHelper.CompileCreate(_epService, eplInsert);

	        // test invalid
	        try {
	            _epService.EPAdministrator.CreateEPL("select last(*) @xxx from MyEvent");
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual("Failed to recognize select-expression annotation 'xxx', expected 'eventbean' in text 'last(*) @xxx' [select last(*) @xxx from MyEvent]", ex.Message);
	        }
	    }

        [Test]
	    public void TestSubquery()
	    {
	        // test non-named-window
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(col1 string, col2 string)");
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        var eplInsert = "insert into DStream select " +
	                "(select * from MyEvent.win:keepall()) @eventbean as c0 " +
	                "from SupportBean";
	        var stmtInsert = _epService.EPAdministrator.CreateEPL(eplInsert);

	        foreach (var prop in "c0".Split(',')){
	            AssertFragment(prop, stmtInsert.EventType, "MyEvent", true);
	        }

	        // test consuming statement
	        var fields = "f0,f1".Split(',');
	        _epService.EPAdministrator.CreateEPL("select " +
	                "c0 as f0, " +
	                "c0.lastOf().col1 as f1 " +
	                "from DStream").AddListener(_listener);

	        var eventOne = new object[] {"E1", null};
	        _epService.EPRuntime.SendEvent(eventOne, "MyEvent");
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        var @out = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(@out, fields, new object[] { new object[] {eventOne}, "E1"});

	        var eventTwo = new object[] {"E2", null};
	        _epService.EPRuntime.SendEvent(eventTwo, "MyEvent");
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        @out = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(@out, fields, new object[]{new object[]{eventOne, eventTwo}, "E2"});
	    }

	    private void AssertFragment(string prop, EventType eventType, string fragmentTypeName, bool indexed)
        {
	        var desc = eventType.GetPropertyDescriptor(prop);
	        Assert.AreEqual(true, desc.IsFragment);
	        var fragment = eventType.GetFragmentType(prop);
	        Assert.AreEqual(fragmentTypeName, fragment.FragmentType.Name);
	        Assert.AreEqual(false, fragment.IsNative);
	        Assert.AreEqual(indexed, fragment.IsIndexed);
	    }
	}
} // end of namespace
