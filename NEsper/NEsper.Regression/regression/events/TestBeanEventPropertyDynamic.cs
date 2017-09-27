///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestBeanEventPropertyDynamic
    {
	    private SupportUpdateListener _listener;
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestPerformance() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();   // exclude test
	        }
	        string stmtText = "select simpleProperty?, " +
	                          "indexed[1]? as indexed, " +
	                          "mapped('keyOne')? as mapped " +
	                          "from " + typeof(SupportBeanComplexProps).FullName;
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        EventType type = stmt.EventType;
	        Assert.AreEqual(typeof(object), type.GetPropertyType("simpleProperty?"));
	        Assert.AreEqual(typeof(object), type.GetPropertyType("indexed"));
	        Assert.AreEqual(typeof(object), type.GetPropertyType("mapped"));

	        SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
	        _epService.EPRuntime.SendEvent(inner);
	        EventBean theEvent = _listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(inner.SimpleProperty, theEvent.Get("simpleProperty?"));
	        Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("indexed"));
	        Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("mapped"));

            var delta = PerformanceObserver.TimeMillis(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    _epService.EPRuntime.SendEvent(inner);
                    if (i % 1000 == 0)
                    {
                        _listener.Reset();
                    }
                }
            });

            Assert.IsTrue(delta < 1000, "delta=" + delta);
	    }
	}
} // end of namespace
