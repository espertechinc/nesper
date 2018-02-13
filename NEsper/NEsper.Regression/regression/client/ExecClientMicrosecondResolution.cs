///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.fail;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientMicrosecondResolution : RegressionExecution {
    
        public override void Run(EPServiceProvider defaultEPService) {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Threading.InternalTimerEnabled = true;
            config.EngineDefaults.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
    
            try {
                EPServiceProviderManager.GetProvider(this.GetType().Name, config).Initialize();
                Assert.Fail();
            } catch (ConfigurationException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
            }
    
            config.EngineDefaults.Threading.InternalTimerEnabled = false;
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(this.GetType().Name, config);
    
            try {
                epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockType.CLOCK_INTERNAL));
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
            }
    
            epService.Dispose();
        }
    }
} // end of namespace
