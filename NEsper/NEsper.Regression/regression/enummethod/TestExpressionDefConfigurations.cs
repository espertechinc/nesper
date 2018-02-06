///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
	public class TestExpressionDefConfigurations
    {
        [Test]
	    public void TestExpressionCacheSize()
        {
	        RunAssertionExpressionCacheSize(null, 4);
	        RunAssertionExpressionCacheSize(0, 4);
	        RunAssertionExpressionCacheSize(1, 4);
	        RunAssertionExpressionCacheSize(2, 2);
	    }

	    private void RunAssertionExpressionCacheSize(int? configuredCacheSize, int expectedInvocationCount)
        {
	        // get config
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
	        config.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));

	        // set cache size
	        if (configuredCacheSize != null) {
	            config.EngineDefaults.Execution.DeclaredExprValueCacheSize = configuredCacheSize.Value;
	        }

	        // allocate
	        EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("alwaysTrue", typeof(SupportStaticMethodLib).FullName, "AlwaysTrue");

	        // set up
	        EPStatement stmt = epService.EPAdministrator.CreateEPL(
	                "expression myExpr {v => alwaysTrue(null) } select myExpr(st0) as c0, myExpr(st1) as c1, myExpr(st0) as c2, myExpr(st1) as c3 from SupportBean_ST0#lastevent as st0, SupportBean_ST1#lastevent as st1");
	        stmt.AddListener(new SupportUpdateListener());

	        // send event and assert
	        SupportStaticMethodLib.Invocations.Clear();
	        epService.EPRuntime.SendEvent(new SupportBean_ST0("a", 0));
	        epService.EPRuntime.SendEvent(new SupportBean_ST1("a", 0));
	        Assert.AreEqual(expectedInvocationCount, SupportStaticMethodLib.Invocations.Count);
	    }
	}
} // end of namespace
