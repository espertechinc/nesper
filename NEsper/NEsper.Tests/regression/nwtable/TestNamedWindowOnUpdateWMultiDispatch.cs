///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
	public class TestNamedWindowOnUpdateWMultiDispatch 
	{
        [Test]
	    public void TestMultipleDataWindowIntersectOnUpdate()
        {
	        RunAssertion(true, null, null);
	        RunAssertion(false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN);
	        RunAssertion(false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND);
	        RunAssertion(false, false, null);
	    }

	    private void RunAssertion(bool useDefault, bool? preserve, ConfigurationEngineDefaults.ThreadingConfig.Locking? locking)
        {
	        var config = SupportConfigFactory.GetConfiguration();
	        if (!useDefault) {
	            config.EngineDefaults.ThreadingConfig.IsNamedWindowConsumerDispatchPreserveOrder = preserve.GetValueOrDefault();
	            config.EngineDefaults.ThreadingConfig.NamedWindowConsumerDispatchLocking = locking.GetValueOrDefault();
	        }

	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();

	        var listener = new SupportUpdateListener();
	        var fields = "company,value,total".Split(',');

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);}

	        // ESPER-568
	        epService.EPAdministrator.CreateEPL("create schema S2 ( company string, value double, total double)");
		    var stmtWin = epService.EPAdministrator.CreateEPL("create window S2Win.win:time(25 hour).std:firstunique(company) as S2");
	        epService.EPAdministrator.CreateEPL("insert into S2Win select * from S2.std:firstunique(company)");
	        epService.EPAdministrator.CreateEPL("on S2 as a update S2Win as b set total = b.value + a.value");
	        var stmt = epService.EPAdministrator.CreateEPL("select count(*) as cnt from S2Win");
	        stmt.AddListener(listener);

	        CreateSendEvent(epService, "S2", "AComp", 3.0, 0.0);
	        Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
	        EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{ new object[] {"AComp", 3.0, 0.0}});

	        CreateSendEvent(epService, "S2", "AComp", 6.0, 0.0);
	        Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
	        EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{ new object[] {"AComp", 3.0, 9.0}});

	        CreateSendEvent(epService, "S2", "AComp", 5.0, 0.0);
	        Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
	        EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{ new object[] {"AComp", 3.0, 8.0}});

	        CreateSendEvent(epService, "S2", "BComp", 4.0, 0.0);
	        // this example does not have @priority thereby it is undefined whether there are two counts delivered or one
	        if (listener.LastNewData.Length == 2) {
	            Assert.AreEqual(1L, listener.LastNewData[0].Get("cnt"));
	            Assert.AreEqual(2L, listener.LastNewData[1].Get("cnt"));
	        }
	        else {
	            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
	        }
	        EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{ new object[] {"AComp", 3.0, 7.0},  new object[] {"BComp", 4.0, 0.0}});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

	    private void CreateSendEvent(EPServiceProvider engine, string typeName, string company, double value, double total)
        {
	        var map = new LinkedHashMap<string, object>();
	        map.Put("company", company);
	        map.Put("value", value);
	        map.Put("total", total);
	        if (EventRepresentationEnumExtensions.GetEngineDefault(engine).IsObjectArrayEvent()) {
	            engine.EPRuntime.SendEvent(map.Values.ToArray(), typeName);
	        }
	        else {
	            engine.EPRuntime.SendEvent(map, typeName);
	        }
	    }
	}
} // end of namespace
