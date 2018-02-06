///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestClientMicrosecondResolution  {

        [Test]
	    public void TestClientMicrosecond() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Threading.IsInternalTimerEnabled = true;
	        config.EngineDefaults.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;

	        try {
	            EPServiceProviderManager.GetDefaultProvider(config).Initialize();
	            Assert.Fail();
	        } catch (ConfigurationException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
	        }

	        EPServiceProvider epService = SupportEngineFactory.SetupEngineDefault(TimeUnit.MICROSECONDS, 0);
	        try {
	            epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_INTERNAL));
	            Assert.Fail();
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
	        }
	    }
	}
} // end of namespace
