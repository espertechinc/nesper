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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestViewPlugin
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("A", typeof(SupportMarketDataBean));
            configuration.AddPlugInView("mynamespace", "flushedsimple", typeof(MyFlushedSimpleViewFactory).FullName);
            configuration.AddPlugInView("mynamespace", "invalid", typeof(string).FullName);
            _epService = EPServiceProviderManager.GetProvider("TestViewPlugin", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
            _epService.Dispose();
        }

        [Test]
        public void TestPlugInViewFlushed()
        {
            String text = "select * from A.mynamespace:flushedsimple(Price)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _testListener.Update;

            SendEvent(1);
            SendEvent(2);
            Assert.IsFalse(_testListener.IsInvoked);

            stmt.Stop();
            Assert.AreEqual(2, _testListener.LastNewData.Length);
        }

        [Test]
        public void TestPlugInViewTrend()
        {
            _epService.EPAdministrator.Configuration.AddPlugInView("mynamespace", "trendspotter", typeof(MyTrendSpotterViewFactory).FullName);
            String text = "select irstream * from A.mynamespace:trendspotter(Price)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _testListener.Update;

            SendEvent(10);
            AssertReceived(1L, null);

            SendEvent(11);
            AssertReceived(2L, 1L);

            SendEvent(12);
            AssertReceived(3L, 2L);

            SendEvent(11);
            AssertReceived(0L, 3L);

            SendEvent(12);
            AssertReceived(1L, 0L);

            SendEvent(0);
            AssertReceived(0L, 1L);

            SendEvent(0);
            AssertReceived(0L, 0L);

            SendEvent(1);
            AssertReceived(1L, 0L);

            SendEvent(1);
            AssertReceived(1L, 1L);

            SendEvent(2);
            AssertReceived(2L, 1L);

            SendEvent(2);
            AssertReceived(2L, 2L);
        }

        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from A.mynamespace:xxx()",
                "Error starting statement: View name 'mynamespace:xxx' is not a known view name [select * from A.mynamespace:xxx()]");
            TryInvalid("select * from A.mynamespace:invalid()",
                       "Error starting statement: Error invoking view factory constructor for view 'invalid', no invocation access for Activator.CreateInstance [select * from A.mynamespace:invalid()]");
        }

        private void SendEvent(double price)
        {
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("", price, null, null));
        }

        private void AssertReceived(long? newTrendCount, long? oldTrendCount)
        {
            EPAssertionUtil.AssertPropsPerRow(_testListener.AssertInvokedAndReset(), "trendcount", new Object[] { newTrendCount }, new Object[] { oldTrendCount });
        }

        private void TryInvalid(String stmtText, String expectedMsg)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(expectedMsg, ex.Message);
            }
        }
    }
}
