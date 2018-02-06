///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfTimeWindowMinFilter 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MD", typeof(SupportMarketDataIDBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _epService.Initialize();
        }
    
        [Test]
        public void TestPerf()
        {
            var statements = new EPStatement[100];
            var listeners = new SupportUpdateListener[statements.Length];
            for (int i = 0; i < statements.Length; i++)
            {
                int secondsWindowSpan = i % 30 + 1;
                double percent = 0.25 + i;
                int id = i % 5;
    
                String text = "select symbol, min(Price) " +
                        "from MD(id='${id}')#time(${secondsWindowSpan})\n" +
                        "having Price >= min(Price) * ${percent}";
    
                text = text.Replace("${id}", Convert.ToString(id));
                text = text.Replace("${secondsWindowSpan}", Convert.ToString(secondsWindowSpan));
                text = text.Replace("${percent}", percent.ToString());
    
                statements[i] = _epService.EPAdministrator.CreateEPL(text);
                listeners[i] = new SupportUpdateListener();
                statements[i].Events += (listeners[i]).Update;
            }
    
            var start = PerformanceObserver.MilliTime;
            var count = 0;
            for (int i = 0; i < 10000; i++)
            {
                count++;
                if (i % 10000 == 0)
                {
                    long now = PerformanceObserver.MilliTime;
                    double deltaSec = (now - start) / 1000.0;
                    double throughput = 10000.0 / deltaSec;
                    for (int j = 0; j < listeners.Length; j++)
                    {
                        listeners[j].Reset();
                    }
                    start = now;
                }
    
                SupportMarketDataIDBean bean = new SupportMarketDataIDBean("IBM", Convert.ToString(i % 5), 1);
                _epService.EPRuntime.SendEvent(bean);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 2000, "Delta=" + delta);
            //Console.Out.WriteLine("total=" + count + " delta=" + delta + " per sec:" + 10000.0 / (delta / 1000.0));
        }
    }
}
