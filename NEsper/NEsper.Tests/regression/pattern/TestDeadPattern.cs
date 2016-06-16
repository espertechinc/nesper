///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestDeadPattern 
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("A", typeof(SupportBean_A).FullName);
            config.AddEventType("B", typeof(SupportBean_B).FullName);
            config.AddEventType("C", typeof(SupportBean_C).FullName);
    
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            epService.Dispose();
        }
    
        [Test]
        public void TestWithDeadPattern()
        {
            String pattern = "(A() -> B()) and not C()";
            // Adjust to 20000 to better test the limit
            for (int i = 0; i < 1000; i++)
            {
                epService.EPAdministrator.CreatePattern(pattern);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
    
            long startTime = PerformanceObserver.MilliTime;
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            long delta =  PerformanceObserver.MilliTime - startTime;
    
            log.Info(".testDeadPattern delta=" + delta);
            Assert.IsTrue(delta < 20, "performance: delta=" + delta);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
