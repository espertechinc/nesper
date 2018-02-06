///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

namespace com.espertech.esper.supportregression.util
{
	public class SupportEngineFactory
    {
	    public static IDictionary<TimeUnit, EPServiceProvider> SetupEnginesByTimeUnit() {
            IDictionary<TimeUnit, EPServiceProvider> engines = new Dictionary<TimeUnit, EPServiceProvider>();
	        engines.Put(TimeUnit.MILLISECONDS, SetupEngine("default_millis", TimeUnit.MILLISECONDS));
	        engines.Put(TimeUnit.MICROSECONDS, SetupEngine("default_micros", TimeUnit.MICROSECONDS));
	        return engines;
	    }

	    public static EPServiceProvider SetupEngineDefault(TimeUnit timeUnit, long startTime) {
	        EPServiceProvider epService = SetupEngine(EPServiceProviderConstants.DEFAULT_ENGINE_URI, timeUnit);
	        epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));
	        return epService;
	    }

	    private static EPServiceProvider SetupEngine(string engineURI, TimeUnit timeUnit) {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.TimeSource.TimeUnit = timeUnit;
	        configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
	        configuration.EngineDefaults.ViewResources.IsShareViews = false;
	        EPServiceProvider epService = EPServiceProviderManager.GetProvider(
	            SupportContainer.Instance, engineURI, configuration);
	        epService.Initialize();
	        return epService;
	    }
	}
} // end of namespace
