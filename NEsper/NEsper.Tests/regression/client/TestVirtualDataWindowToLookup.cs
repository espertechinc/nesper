///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.virtualdw;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestVirtualDataWindowToLookup
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();

	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName);
            configuration.AddEventType<SupportBean>();
	        configuration.AddEventType<SupportBean_S0>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestLateConsumerNoIterate() {

	        // client-side
	        _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
	        var window = (SupportVirtualDW) GetFromContext("/virtualdw/MyVDW");
	        var supportBean = new SupportBean("E1", 100);
	        window.Data = Collections.SingletonSet<object>(supportBean);

	        var stmt = _epService.EPAdministrator.CreateEPL("select (select sum(IntPrimitive) from MyVDW vdw where vdw.TheString = s0.p00) from SupportBean_S0 s0");
	        stmt.AddListener(_listener);
	        var spiContext = (VirtualDataWindowLookupContextSPI) window.LastRequestedIndex;

	        // CM side
	        _epService.EPAdministrator.CreateEPL("create window MyWin.std:unique(TheString) as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWin select * from SupportBean");
	    }

	    private VirtualDataWindow GetFromContext(string name)
        {
            return (VirtualDataWindow) _epService.Directory.Lookup(name);
	    }
	}
} // end of namespace
