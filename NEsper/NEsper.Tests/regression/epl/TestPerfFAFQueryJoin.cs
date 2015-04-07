///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfFAFQueryJoin 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = false;
            config.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            config.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [Test]
        public void TestPerfFAFJoin()
        {
            _epService.EPAdministrator.CreateEPL("create window W1.std:unique(s1) as SSB1");
            _epService.EPAdministrator.CreateEPL("insert into W1 select * from SSB1");
    
            _epService.EPAdministrator.CreateEPL("create window W2.std:unique(s2) as SSB2");
            _epService.EPAdministrator.CreateEPL("insert into W2 select * from SSB2");
    
            for (int i = 0; i < 1000; i++) {
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("A" + i, 0, 0, 0));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("A" + i, 0, 0, 0));
            }
    
            long start = Environment.TickCount;
            for (int i = 0; i < 100; i++)
            {
                EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("select * from W1 as w1, W2 as w2 " +
                        "where w1.s1 = w2.s2");
                Assert.AreEqual(1000, result.Array.Length);
            }
            long end = Environment.TickCount;
            long delta = end - start;
            Log.Debug("Delta = {0}", delta);
            Assert.That(delta, Is.LessThan(1000), "Delta=" + delta);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
