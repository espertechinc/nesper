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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfWhereNoWindow 
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MD", typeof(SupportMarketDataIDBean));
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        // Compares the performance of
        //     select * from MD(symbol = 'xyz')
        //  against
        //     select * from MD where symbol = 'xyz'
        [Test]
        public void TestPerfNoDelivery()
        {
            for (int i = 0; i < 1000; i++)
            {
                String text = "select * from MD where Symbol = '" + Convert.ToString(i) + "'";
                epService.EPAdministrator.CreateEPL(text);
            }
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++)
            {
                SupportMarketDataIDBean bean = new SupportMarketDataIDBean("NOMATCH", "", 1);
                epService.EPRuntime.SendEvent(bean);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 500,"Delta=" + delta);
        }
    }
}
