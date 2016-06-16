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

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
	public class TestRowPatternRecognitionEnumMethod
    {
        [Test]
	    public void TestNamedWindowOnDeleteOutOfSeq()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);}

	        var fields = "c0,c1".Split(',');
	        var epl = "select * from SupportBean match_recognize ("
	                + "partition by TheString "
	                + "measures A.TheString as c0, C.IntPrimitive as c1 "
	                + "pattern (A B+ C) "
	                + "define "
	                + "B as B.IntPrimitive > A.IntPrimitive, "
	                + "C as C.DoublePrimitive > B.firstOf().IntPrimitive)";
	                // can also be expressed as: B[0].IntPrimitive
	        var listener = new SupportUpdateListener();
	        epService.EPAdministrator.CreateEPL(epl).AddListener(listener);

	        SendEvent(epService, "E1", 10, 0);
	        SendEvent(epService, "E1", 11, 50);
	        SendEvent(epService, "E1", 12, 11);
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent(epService, "E2", 10, 0);
	        SendEvent(epService, "E2", 11, 50);
	        SendEvent(epService, "E2", 12, 12);
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 12});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

	    private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, double doublePrimitive)
        {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.DoublePrimitive = doublePrimitive;
	        epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
