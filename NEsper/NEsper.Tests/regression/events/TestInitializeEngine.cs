///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestInitializeEngine 
    {
        [Test]
        public void TestInitialize()
        {
            Configuration config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            String eplOne = "insert into A(a) select 1 from " + typeof(SupportBean).FullName + ".win:length(100)";
            String eplTwo = "insert into A(a, b) select 1,2 from " + typeof(SupportBean).FullName + ".win:length(100)";
    
            // Asserting that the engine allows to use the new event stream A with more properties then the old A
            epService.EPAdministrator.CreateEPL(eplOne);
            epService.Initialize();
            epService.EPAdministrator.CreateEPL(eplTwo);
        }
    }
}
