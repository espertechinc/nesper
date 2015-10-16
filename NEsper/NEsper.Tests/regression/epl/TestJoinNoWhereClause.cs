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


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinNoWhereClause 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        private Object[] _setOne;
        private Object[] _setTwo;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder = false;
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
    
            _setOne = new Object[5];
            _setTwo = new Object[5];
            for (int i = 0; i < _setOne.Length; i++)
            {
                _setOne[i] = new SupportMarketDataBean("IBM", 0, (long) i, "");
    
                SupportBean theEvent = new SupportBean();
                theEvent.LongBoxed = (long)i;
                _setTwo[i] = theEvent;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
            _setOne = null;
            _setTwo = null;
        }
    
        [Test]
        public void TestWithJoinNoWhereClause()
        {
            String[] fields = new String[] {"stream_0.Volume", "stream_1.LongBoxed"};
            String joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + ".win:length(3)," +
                    typeof(SupportBean).FullName + "().win:length(3)";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            // Send 2 events, should join on second one
            SendEvent(_setOne[0]);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), fields, null);
    
            SendEvent(_setTwo[0]);
            Assert.AreEqual(1, _updateListener.LastNewData.Length);
            Assert.AreEqual(_setOne[0], _updateListener.LastNewData[0].Get("stream_0"));
            Assert.AreEqual(_setTwo[0], _updateListener.LastNewData[0].Get("stream_1"));
            _updateListener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), fields,
                    new Object[][] { new Object[] {0L, 0L}});
    
            SendEvent(_setOne[1]);
            SendEvent(_setOne[2]);
            SendEvent(_setTwo[1]);
            Assert.AreEqual(3, _updateListener.LastNewData.Length);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), fields,
                    new Object[][] { new Object[] {0L, 0L},
                                    new Object[] {1L, 0L},
                                    new Object[] {2L, 0L},
                                    new Object[] {0L, 1L},
                                    new Object[] {1L, 1L},
                                    new Object[] {2L, 1L}});
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
