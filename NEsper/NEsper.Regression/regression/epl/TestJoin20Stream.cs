///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestJoin20Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

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
	    public void Test20StreamJoin() {
	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0).FullName);

	        StringWriter buf = new StringWriter();
            buf.Write("select * from ");

	        string delimiter = "";
	        for (int i = 0; i < 20; i++) {
	            buf.Write(delimiter);
                buf.Write("S0(id=" + i + ")#lastevent as s_" + i);
	            delimiter = ", ";
	        }
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(buf.ToString());
	        stmt.AddListener(_listener);

	        for (int i = 0; i < 19; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean_S0(i));
	        }
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(19));
	        Assert.IsTrue(_listener.IsInvoked);
	    }
	}
} // end of namespace
