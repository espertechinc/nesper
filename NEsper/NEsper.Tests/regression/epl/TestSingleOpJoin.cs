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
    public class TestSingleOpJoin 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        private readonly SupportBean_A[] _eventsA = new SupportBean_A[10];
        private readonly SupportBean_A[] _eventsASetTwo = new SupportBean_A[10];
        private readonly SupportBean_B[] _eventsB = new SupportBean_B[10];
        private readonly SupportBean_B[] _eventsBSetTwo = new SupportBean_B[10];
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
    
            String eventA = typeof(SupportBean_A).FullName;
            String eventB = typeof(SupportBean_B).FullName;
    
            String joinStatement = "select irstream * from " +
                eventA + "().win:length(3) as streamA," +
                eventB + "().win:length(3) as streamB" +
                " where streamA.id = streamB.id";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            Assert.AreEqual(typeof(SupportBean_A), joinView.EventType.GetPropertyType("streamA"));
            Assert.AreEqual(typeof(SupportBean_B), joinView.EventType.GetPropertyType("streamB"));
            Assert.AreEqual(2, joinView.EventType.PropertyNames.Length);
    
            for (int i = 0; i < _eventsA.Length; i++)
            {
                _eventsA[i] = new SupportBean_A(Convert.ToString(i));
                _eventsASetTwo[i] = new SupportBean_A(Convert.ToString(i));
                _eventsB[i] = new SupportBean_B(Convert.ToString(i));
                _eventsBSetTwo[i] = new SupportBean_B(Convert.ToString(i));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestJoinUniquePerId()
        {
            SendEvent(_eventsA[0]);
            SendEvent(_eventsB[1]);
            Assert.IsNull(_updateListener.LastNewData);
    
            // Test join new B with id 0
            SendEvent(_eventsB[0]);
            Assert.AreSame(_eventsA[0], _updateListener.LastNewData[0].Get("streamA"));
            Assert.AreSame(_eventsB[0], _updateListener.LastNewData[0].Get("streamB"));
            Assert.IsNull(_updateListener.LastOldData);
            _updateListener.Reset();
    
            // Test join new A with id 1
            SendEvent(_eventsA[1]);
            Assert.AreSame(_eventsA[1], _updateListener.LastNewData[0].Get("streamA"));
            Assert.AreSame(_eventsB[1], _updateListener.LastNewData[0].Get("streamB"));
            Assert.IsNull(_updateListener.LastOldData);
            _updateListener.Reset();
    
            SendEvent(_eventsA[2]);
            Assert.IsNull(_updateListener.LastOldData);
    
            // Test join old A id 0 leaves length window of 3 events
            SendEvent(_eventsA[3]);
            Assert.AreSame(_eventsA[0], _updateListener.LastOldData[0].Get("streamA"));
            Assert.AreSame(_eventsB[0], _updateListener.LastOldData[0].Get("streamB"));
            Assert.IsNull(_updateListener.LastNewData);
            _updateListener.Reset();
    
            // Test join old B id 1 leaves window
            SendEvent(_eventsB[4]);
            Assert.IsNull(_updateListener.LastOldData);
            SendEvent(_eventsB[5]);
            Assert.AreSame(_eventsA[1], _updateListener.LastOldData[0].Get("streamA"));
            Assert.AreSame(_eventsB[1], _updateListener.LastOldData[0].Get("streamB"));
            Assert.IsNull(_updateListener.LastNewData);
        }
    
        [Test]
        public void TestJoinNonUniquePerId()
        {
            SendEvent(_eventsA[0]);
            SendEvent(_eventsA[1]);
            SendEvent(_eventsASetTwo[0]);
            Assert.IsTrue(_updateListener.LastOldData == null && _updateListener.LastNewData == null);
    
            SendEvent(_eventsB[0]); // Event B id 0 joins to A id 0 twice
            EventBean[] data = _updateListener.LastNewData;
            Assert.IsTrue(_eventsASetTwo[0] == data[0].Get("streamA") || _eventsASetTwo[0] == data[1].Get("streamA"));    // Order arbitrary
            Assert.AreSame(_eventsB[0], data[0].Get("streamB"));
            Assert.IsTrue(_eventsA[0] == data[0].Get("streamA") || _eventsA[0] == data[1].Get("streamA"));
            Assert.AreSame(_eventsB[0], data[1].Get("streamB"));
            Assert.IsNull(_updateListener.LastOldData);
            _updateListener.Reset();
    
            SendEvent(_eventsB[2]);
            SendEvent(_eventsBSetTwo[0]);  // Ignore events generated
            _updateListener.Reset();
    
            SendEvent(_eventsA[3]);  // Pushes A id 0 out of window, which joins to B id 0 twice
            data = _updateListener.LastOldData;
            Assert.AreSame(_eventsA[0], _updateListener.LastOldData[0].Get("streamA"));
            Assert.IsTrue(_eventsB[0] == data[0].Get("streamB") || _eventsB[0] == data[1].Get("streamB"));    // B order arbitrary
            Assert.AreSame(_eventsA[0], _updateListener.LastOldData[1].Get("streamA"));
            Assert.IsTrue(_eventsBSetTwo[0] == data[0].Get("streamB") || _eventsBSetTwo[0] == data[1].Get("streamB"));
            Assert.IsNull(_updateListener.LastNewData);
            _updateListener.Reset();
    
            SendEvent(_eventsBSetTwo[2]);  // Pushes B id 0 out of window, which joins to A set two id 0
            Assert.AreSame(_eventsASetTwo[0], _updateListener.LastOldData[0].Get("streamA"));
            Assert.AreSame(_eventsB[0], _updateListener.LastOldData[0].Get("streamB"));
            Assert.AreEqual(1, _updateListener.LastOldData.Length);
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
