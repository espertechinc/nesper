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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewGroupWithinGroup
    {
        private const String SYMBOL_MSFT = "MSFT";
        private const String SYMBOL_GE = "GE";

        private const String FEED_INFO = "INFO";
        private const String FEED_REU = "REU";

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener = new SupportUpdateListener();

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestPullDateAndPushData()
        {
            // Listen to all ticks
            EPStatement viewGrouped = _epService.EPAdministrator.CreateEPL(
                    "select irstream datapoints as size, Symbol, feed, Volume from " + typeof(SupportMarketDataBean).FullName +
                            "#groupwin(Symbol)#groupwin(feed)#groupwin(Volume)#uni(Price) order by Symbol, feed, Volume");

            // Counts per Symbol, feed and volume the events
            viewGrouped.Events += _listener.Update;

            List<IDictionary<String, Object>> mapList = new List<IDictionary<String, Object>>();

            // Set up a map of expected values

            IDictionary<String, Object>[] expectedValues = new IDictionary<string, object>[10];
            for (int i = 0; i < expectedValues.Length; i++)
            {
                expectedValues[i] = new Dictionary<String, Object>();
            }

            // Send one event, check results
            SendEvent(SYMBOL_GE, FEED_INFO, 1);

            PopulateMap(expectedValues[0], SYMBOL_GE, FEED_INFO, 1L, 0);
            mapList.Add(expectedValues[0]);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, mapList);
            PopulateMap(expectedValues[0], SYMBOL_GE, FEED_INFO, 1L, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, mapList);
            EPAssertionUtil.AssertPropsPerRow(viewGrouped.GetEnumerator(), mapList);

            // Send a couple of events
            SendEvent(SYMBOL_GE, FEED_INFO, 1);
            SendEvent(SYMBOL_GE, FEED_INFO, 2);
            SendEvent(SYMBOL_GE, FEED_INFO, 1);
            SendEvent(SYMBOL_GE, FEED_REU, 99);
            SendEvent(SYMBOL_MSFT, FEED_INFO, 100);

            PopulateMap(expectedValues[1], SYMBOL_MSFT, FEED_INFO, 100, 0);
            mapList.Clear();
            mapList.Add(expectedValues[1]);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, mapList);
            PopulateMap(expectedValues[1], SYMBOL_MSFT, FEED_INFO, 100, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, mapList);

            PopulateMap(expectedValues[0], SYMBOL_GE, FEED_INFO, 1, 3);
            PopulateMap(expectedValues[2], SYMBOL_GE, FEED_INFO, 2, 1);
            PopulateMap(expectedValues[3], SYMBOL_GE, FEED_REU, 99, 1);
            mapList.Clear();
            mapList.Add(expectedValues[0]);
            mapList.Add(expectedValues[2]);
            mapList.Add(expectedValues[3]);
            mapList.Add(expectedValues[1]);
            EPAssertionUtil.AssertPropsPerRow(viewGrouped.GetEnumerator(), mapList);
        }

        private void PopulateMap(IDictionary<String, Object> map, String symbol, String feed, long volume, long size)
        {
            map["Symbol"] = symbol;
            map["feed"] = feed;
            map["Volume"] = volume;
            map["size"] = size;
        }

        private void SendEvent(String symbol, String feed, long volume)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, 0, volume, feed);
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
