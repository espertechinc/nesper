///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;



namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestSumWinTime 
    {
        private static String SYMBOL_DELL = "DELL";
        private static String SYMBOL_IBM = "IBM";
    
        private EPServiceProvider epService;
        private SupportUpdateListener testListener;
    
        [SetUp]
        public void SetUp()
        {
            testListener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            testListener = null;
        }
    
        [Test]
        public void TestWinTimeSum()
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            String sumTimeExpr = "select Symbol, Volume, sum(Price) as mySum " +
                                 "from " + typeof(SupportMarketDataBean).FullName + ".win:time(30)";
    
            EPStatement selectTestView = epService.EPAdministrator.CreateEPL(sumTimeExpr);
            selectTestView.Events += testListener.Update;
    
            RunAssertion(selectTestView);
        }
    
        [Test]
        public void TestWinTimeSumGroupBy()
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            String sumTimeUniExpr = "select Symbol, Volume, sum(Price) as mySum " +
                                 "from " + typeof(SupportMarketDataBean).FullName +
                                 ".win:time(30) group by Symbol";
    
            EPStatement selectTestView = epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
            selectTestView.Events += testListener.Update;
    
            RunGroupByAssertions(selectTestView);
        }
    
        [Test]
        public void TestWinTimeSumSingle()
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            String sumTimeUniExpr = "select Symbol, Volume, sum(Price) as mySum " +
                                 "from " + typeof(SupportMarketDataBean).FullName +
                                 "(Symbol = 'IBM').win:time(30)";
    
            EPStatement selectTestView = epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
            selectTestView.Events += testListener.Update;
    
            RunSingleAssertion(selectTestView);
        }
    
        private void RunAssertion(EPStatement selectTestView)
        {
            AssertSelectResultType(selectTestView);
    
            CurrentTimeEvent currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(SYMBOL_DELL, 10000, 51);
            AssertEvents(SYMBOL_DELL, 10000, 51,false);
    
            SendEvent(SYMBOL_IBM, 20000, 52);
            AssertEvents(SYMBOL_IBM, 20000, 103,false);
    
            SendEvent(SYMBOL_DELL, 40000, 45);
            AssertEvents(SYMBOL_DELL, 40000, 148,false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
    
            SendEvent(SYMBOL_IBM, 30000, 70);
            AssertEvents(SYMBOL_IBM, 30000,70,false);
    
            SendEvent(SYMBOL_DELL, 10000, 20);
            AssertEvents(SYMBOL_DELL, 10000, 90,false);
    
        }
    
        private void RunGroupByAssertions(EPStatement selectTestView)
        {
            AssertSelectResultType(selectTestView);
    
            CurrentTimeEvent currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(SYMBOL_DELL, 10000, 51);
            AssertEvents(SYMBOL_DELL, 10000, 51,false);
    
            SendEvent(SYMBOL_IBM, 30000, 70);
            AssertEvents(SYMBOL_IBM, 30000, 70,false);
    
            SendEvent(SYMBOL_DELL, 20000, 52);
            AssertEvents(SYMBOL_DELL, 20000, 103,false);
    
            SendEvent(SYMBOL_IBM, 30000, 70);
            AssertEvents(SYMBOL_IBM, 30000, 140,false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
            SendEvent(SYMBOL_DELL, 10000, 90);
            AssertEvents(SYMBOL_DELL, 10000, 90,false);
    
            SendEvent(SYMBOL_IBM, 30000, 120);
            AssertEvents(SYMBOL_IBM, 30000, 120,false);
    
            SendEvent(SYMBOL_DELL, 20000, 90);
            AssertEvents(SYMBOL_DELL, 20000, 180,false);
    
            SendEvent(SYMBOL_IBM, 30000, 120);
            AssertEvents(SYMBOL_IBM, 30000, 240,false);
         }
    
        private void RunSingleAssertion(EPStatement selectTestView)
        {
            AssertSelectResultType(selectTestView);
    
            CurrentTimeEvent currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(SYMBOL_IBM, 20000, 52);
            AssertEvents(SYMBOL_IBM, 20000, 52,false);
    
            SendEvent(SYMBOL_IBM, 20000, 100);
            AssertEvents(SYMBOL_IBM, 20000, 152,false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
            SendEvent(SYMBOL_IBM, 20000, 252);
            AssertEvents(SYMBOL_IBM, 20000, 252,false);
    
            SendEvent(SYMBOL_IBM, 20000, 100);
            AssertEvents(SYMBOL_IBM, 20000, 352,false);
        }
    
        private void AssertEvents(String symbol, long volume, double sum, bool unique)
        {
            EventBean[] oldData = testListener.LastOldData;
            EventBean[] newData = testListener.LastNewData;
    
            if( ! unique)
             Assert.IsNull(oldData);
    
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(volume, newData[0].Get("Volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));
    
            testListener.Reset();
            Assert.IsFalse(testListener.IsInvoked);
        }
    
        private void AssertSelectResultType(EPStatement selectTestView)
        {
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
        }
    
        private void SendEvent(String symbol, long volume, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
    
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
