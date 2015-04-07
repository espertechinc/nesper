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
    public class TestPatternInsertInto 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
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
            _updateListener = null;
        }
    
        [Test]
        public void TestPropsWildcard()
        {
            String stmtText =
                    "insert into MyThirdStream(es0id, es1id) " +
                    "select es0.id, es1.id " +
                    "from " +
                    "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                                 " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            String stmtTwoText =
                    "select * from MyThirdStream";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            _updateListener = new SupportUpdateListener();
            statement.Events += _updateListener.Update;
    
            SendEventsAndAssert();
        }
    
        [Test]
        public void TestProps()
        {
            String stmtText =
                    "insert into MySecondStream(s0, s1) " +
                    "select es0, es1 " +
                    "from " +
                    "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                                 " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            String stmtTwoText =
                    "select s0.id as es0id, s1.id as es1id from MySecondStream";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            _updateListener = new SupportUpdateListener();
            statement.Events += _updateListener.Update;
    
            SendEventsAndAssert();
        }
    
        [Test]
        public void TestNoProps()
        {
            String stmtText =
                    "insert into MyStream " +
                    "select es0, es1 " +
                    "from " +
                    "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                                 " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            String stmtTwoText =
                    "select es0.id as es0id, es1.id as es1id from MyStream.win:length(10)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            _updateListener = new SupportUpdateListener();
            statement.Events += _updateListener.Update;
    
            SendEventsAndAssert();
        }
    
        private void SendEventsAndAssert()
        {
            SendEventS1(10, "");
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.IsNull(theEvent.Get("es0id"));
            Assert.AreEqual(10, theEvent.Get("es1id"));
    
            SendEventS0(20, "");
            theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(20, theEvent.Get("es0id"));
            Assert.IsNull(theEvent.Get("es1id"));
        }
    
        private SupportBean_S0 SendEventS0(int id, String p00)
        {
            SupportBean_S0 theEvent = new SupportBean_S0(id, p00);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S1 SendEventS1(int id, String p10)
        {
            SupportBean_S1 theEvent = new SupportBean_S1(id, p10);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
}
