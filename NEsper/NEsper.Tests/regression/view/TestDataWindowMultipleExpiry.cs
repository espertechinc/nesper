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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestDataWindowMultipleExpiry 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
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
        public void TestTimeViewUnique()
        {
            // Testing the two forms of the case expression
            // Furthermore the test checks the different when clauses and actions related.
            String caseExpr = "select Volume " +
                    "from " +  typeof(SupportMarketDataBean).FullName + ".std:unique(symbol).win:time(10)";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(caseExpr);
            stmt.Events += _listener.Update;
            SendMarketDataEvent("DELL", 1, 50);
            SendMarketDataEvent("DELL", 2, 50);
            Object[] values = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.AreEqual(1, values.Length);
        }
    
        private void SendMarketDataEvent(String symbol, long volume, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
