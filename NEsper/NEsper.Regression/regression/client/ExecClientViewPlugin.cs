///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientViewPlugin : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("A", typeof(SupportMarketDataBean));
            configuration.AddPlugInView("mynamespace", "flushedsimple", typeof(MyFlushedSimpleViewFactory).FullName);
            configuration.AddPlugInView("mynamespace", "invalid", typeof(object).FullName);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPlugInViewFlushed(epService);
            RunAssertionPlugInViewTrend(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionPlugInViewFlushed(EPServiceProvider epService) {
            string text = "select * from A.mynamespace:flushedsimple(price)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            SendEvent(epService, 1);
            SendEvent(epService, 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            stmt.Stop();
            Assert.AreEqual(2, testListener.LastNewData.Length);
        }
    
        private void RunAssertionPlugInViewTrend(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInView("mynamespace", "trendspotter", typeof(MyTrendSpotterViewFactory));
            string text = "select irstream * from A.mynamespace:trendspotter(price)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            SendEvent(epService, 10);
            AssertReceived(testListener, 1L, null);
    
            SendEvent(epService, 11);
            AssertReceived(testListener, 2L, 1L);
    
            SendEvent(epService, 12);
            AssertReceived(testListener, 3L, 2L);
    
            SendEvent(epService, 11);
            AssertReceived(testListener, 0L, 3L);
    
            SendEvent(epService, 12);
            AssertReceived(testListener, 1L, 0L);
    
            SendEvent(epService, 0);
            AssertReceived(testListener, 0L, 1L);
    
            SendEvent(epService, 0);
            AssertReceived(testListener, 0L, 0L);
    
            SendEvent(epService, 1);
            AssertReceived(testListener, 1L, 0L);
    
            SendEvent(epService, 1);
            AssertReceived(testListener, 1L, 1L);
    
            SendEvent(epService, 2);
            AssertReceived(testListener, 2L, 1L);
    
            SendEvent(epService, 2);
            AssertReceived(testListener, 2L, 2L);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            TryInvalid(epService, "select * from A.mynamespace:xxx()",
                "Error starting statement: View name 'mynamespace:xxx' is not a known view name [select * from A.mynamespace:xxx()]");
            TryInvalid(epService, "select * from A.mynamespace:invalid()",
                "Error starting statement: Error casting view factory instance to com.espertech.esper.view.ViewFactory interface for view 'invalid' [select * from A.mynamespace:invalid()]");
        }
    
        private void SendEvent(EPServiceProvider epService, double price) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("", price, null, null));
        }
    
        private void AssertReceived(SupportUpdateListener testListener, long newTrendCount, long? oldTrendCount) {
            EPAssertionUtil.AssertPropsPerRow(testListener.AssertInvokedAndReset(), "trendcount", new object[]{newTrendCount}, new object[]{oldTrendCount});
        }
    }
} // end of namespace
