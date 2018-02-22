///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupWithinGroup : RegressionExecution {
        private const string SYMBOL_MSFT = "MSFT";
        private const string SYMBOL_GE = "GE";
    
        private const string FEED_INFO = "INFO";
        private const string FEED_REU = "REU";
    
        public override void Run(EPServiceProvider epService) {
            // Listen to all ticks
            EPStatement viewGrouped = epService.EPAdministrator.CreateEPL(
                    "select irstream datapoints as size, symbol, feed, volume from " + typeof(SupportMarketDataBean).FullName +
                            "#groupwin(symbol)#groupwin(feed)#groupwin(volume)#uni(price) order by symbol, feed, volume");
            var listener = new SupportUpdateListener();
    
            // Counts per symbol, feed and volume the events
            viewGrouped.Events += listener.Update;
    
            var mapList = new List<IDictionary<string, object>>();
    
            // Set up a map of expected values
    
            var expectedValues = new IDictionary<string, object>[10];
            for (int i = 0; i < expectedValues.Length; i++) {
                expectedValues[i] = new Dictionary<string, object>();
            }
    
            // Send one event, check results
            SendEvent(epService, SYMBOL_GE, FEED_INFO, 1);
    
            PopulateMap(expectedValues[0], SYMBOL_GE, FEED_INFO, 1L, 0);
            mapList.Add(expectedValues[0]);
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, mapList);
            PopulateMap(expectedValues[0], SYMBOL_GE, FEED_INFO, 1L, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, mapList);
            EPAssertionUtil.AssertPropsPerRow(viewGrouped.GetEnumerator(), mapList);
    
            // Send a couple of events
            SendEvent(epService, SYMBOL_GE, FEED_INFO, 1);
            SendEvent(epService, SYMBOL_GE, FEED_INFO, 2);
            SendEvent(epService, SYMBOL_GE, FEED_INFO, 1);
            SendEvent(epService, SYMBOL_GE, FEED_REU, 99);
            SendEvent(epService, SYMBOL_MSFT, FEED_INFO, 100);
    
            PopulateMap(expectedValues[1], SYMBOL_MSFT, FEED_INFO, 100, 0);
            mapList.Clear();
            mapList.Add(expectedValues[1]);
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, mapList);
            PopulateMap(expectedValues[1], SYMBOL_MSFT, FEED_INFO, 100, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, mapList);
    
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
    
        private void PopulateMap(IDictionary<string, Object> map, string symbol, string feed, long volume, long size) {
            map.Put("symbol", symbol);
            map.Put("feed", feed);
            map.Put("volume", volume);
            map.Put("size", size);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, string feed, long volume) {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, feed);
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
