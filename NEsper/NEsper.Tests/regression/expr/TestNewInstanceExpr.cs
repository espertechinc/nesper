///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestNewInstanceExpr 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNewInstance() {
	        RunAssertionNewInstance(false);
	        RunAssertionNewInstance(true);

	        RunAssertionStreamAlias();

	        // try variable
            _epService.EPAdministrator.CreateEPL("create constant variable com.espertech.esper.compat.AtomicLong cnt = new com.espertech.esper.compat.AtomicLong(1)");

	        // try shallow invalid cases
	        SupportMessageAssertUtil.TryInvalid(_epService, "select new Dummy() from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'new Dummy()': Failed to resolve new-operator class name 'Dummy'");

	        _epService.EPAdministrator.Configuration.AddImport(typeof(MyClassNoCtor));
	        SupportMessageAssertUtil.TryInvalid(_epService, "select new MyClassNoCtor() from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'new MyClassNoCtor()': Failed to find a suitable constructor for type ");
	    }

	    private void RunAssertionStreamAlias() {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(MyClassObjectCtor));
	        _epService.EPAdministrator.CreateEPL("select " +
	                "new MyClassObjectCtor(sb) as c0 " +
	                "from SupportBean as sb").AddListener(_listener);

	        var sb = new SupportBean();
	        _epService.EPRuntime.SendEvent(sb);
	        var @event = _listener.AssertOneGetNewAndReset();
	        Assert.AreSame(sb, ((MyClassObjectCtor) @event.Get("c0")).Object);
	    }

	    private void RunAssertionNewInstance(bool soda) {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean));

	        var epl = "select " +
	                "new SupportBean(\"A\",IntPrimitive) as c0, " +
                    "new SupportBean(\"B\",IntPrimitive+10), " +
	                "new SupportBean() as c2, " +
	                "new SupportBean(\"ABC\",0).get_TheString() as c3 " +
	                "from SupportBean";
	        var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
	        stmt.AddListener(_listener);
            var expectedAggType = new object[][] { new object[] { "c0", typeof(SupportBean) }, new object[] { "new SupportBean(\"B\",IntPrimitive+10)", typeof(SupportBean) } };
	        EventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmt.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);

	        var fields = "TheString,IntPrimitive".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        var @event = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertPropsPONO(@event.Get("c0"), fields, new object[] {"A", 10});
            EPAssertionUtil.AssertPropsPONO(((IDictionary<string, object>)@event.Underlying).Get("new SupportBean(\"B\",IntPrimitive+10)"), fields, new object[] { "B", 20 });
	        EPAssertionUtil.AssertPropsPONO(@event.Get("c2"), fields, new object[] {null, 0});
	        Assert.AreEqual("ABC", @event.Get("c3"));

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    public class MyClassNoCtor
        {
	        private MyClassNoCtor()
            {
	        }
	    }

	    public class MyClassObjectCtor
        {
            public object Object { get; private set; }
            public MyClassObjectCtor(object @object)
            {
	            Object = @object;
	        }
        }
	}
} // end of namespace
