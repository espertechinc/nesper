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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinNoTableName 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        private readonly Object[] _setOne = new Object[5];
        private readonly Object[] _setTwo = new Object[5];
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
    
            String joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(3)," +
                    typeof(SupportBean).FullName + "#length(3)" +
                " where Symbol=TheString and Volume=LongBoxed";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            for (int i = 0; i < _setOne.Length; i++)
            {
                _setOne[i] = new SupportMarketDataBean("IBM", 0, (long) i, "");
    
                SupportBean theEvent = new SupportBean();
                theEvent.TheString = "IBM";
                theEvent.LongBoxed = (long)i;
                _setTwo[i] = theEvent;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestJoinUniquePerId()
        {
            SendEvent(_setOne[0]);
            SendEvent(_setTwo[0]);
            Assert.NotNull(_updateListener.LastNewData);
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
