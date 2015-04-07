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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestGroupByEventPerRowHaving
    {
        private const String SYMBOL_DELL = "DELL";
        private const String SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        [Test]
        public void TestSumOneView()
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            String viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
                    "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                    "group by Symbol " +
                    "having sum(Price) >= 50";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;

            RunAssertion(selectTestView);
        }

        [Test]
        public void TestSumJoin()
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            String viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
                    "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                    "  and one.TheString = two.Symbol " +
                    "group by Symbol " +
                    "having sum(Price) >= 50";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

            RunAssertion(selectTestView);
        }

        private void RunAssertion(EPStatement selectTestView)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));

            String[] fields = "Symbol,Volume,mySum".Split(',');
            SendEvent(SYMBOL_DELL, 10000, 49);
            Assert.IsFalse(_testListener.IsInvoked);

            SendEvent(SYMBOL_DELL, 20000, 54);
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(), fields, new Object[] { SYMBOL_DELL, 20000L, 103d });

            SendEvent(SYMBOL_IBM, 1000, 10);
            Assert.IsFalse(_testListener.IsInvoked);

            SendEvent(SYMBOL_IBM, 5000, 20);
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetOldAndReset(), fields, new Object[] { SYMBOL_DELL, 10000L, 54d });

            SendEvent(SYMBOL_IBM, 6000, 5);
            Assert.IsFalse(_testListener.IsInvoked);
        }

        private void SendEvent(String symbol, long volume, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
