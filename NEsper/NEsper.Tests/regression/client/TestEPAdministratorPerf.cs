///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPAdministratorPerf 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableTimerDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [Test]
        public void Test1kValidStmtsPerformance()
        {
            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        String text = "select * from " + typeof (SupportBean).FullName;
                        EPStatement stmt = _epService.EPAdministrator.CreateEPL(text, "s1");
                        Assert.AreEqual("s1", stmt.Name);
                        stmt.Stop();
                        stmt.Start();
                        stmt.Stop();
                        stmt.Dispose();
                    }
                });

            Assert.That(delta, Is.LessThan(5000), ".test10kValid delta=" + delta);
        }
    
        [Test]
        public void Test1kInvalidStmts()
        {
            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        try
                        {
                            String text = "select xxx from " + typeof (SupportBean).FullName;
                            _epService.EPAdministrator.CreateEPL(text, "s1");
                        }
                        catch (Exception ex)
                        {
                            // expected
                        }
                    }
                });

            Assert.That(delta, Is.LessThan(2500), ".test1kInvalid delta=" + delta);
        }
    }
}
