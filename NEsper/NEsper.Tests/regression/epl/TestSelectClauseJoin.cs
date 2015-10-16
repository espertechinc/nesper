///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;



namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSelectClauseJoin 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestJoinSelect()
        {
            String eventA = typeof(SupportBean).FullName;
            String eventB = typeof(SupportBean).FullName;
    
            String joinStatement = "select s0.DoubleBoxed, s1.IntPrimitive*s1.IntBoxed/2.0 as div from " +
                eventA + "(TheString='s0').win:length(3) as s0," +
                eventB + "(TheString='s1').win:length(3) as s1" +
                " where s0.DoubleBoxed = s1.DoubleBoxed";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            EventType result = joinView.EventType;
            Assert.AreEqual(typeof(double?), result.GetPropertyType("s0.DoubleBoxed"));
            Assert.AreEqual(typeof(double?), result.GetPropertyType("div"));
            Assert.AreEqual(2, joinView.EventType.PropertyNames.Length);
    
            Assert.IsNull(_updateListener.LastNewData);
    
            SendEvent("s0", 1, 4, 5);
            SendEvent("s1", 1, 3, 2);
    
            EventBean[] newEvents = _updateListener.LastNewData;
            Assert.AreEqual(1d, newEvents[0].Get("s0.DoubleBoxed"));
            Assert.AreEqual(3d, newEvents[0].Get("div"));
    
            IEnumerator<EventBean> iterator = joinView.GetEnumerator();
            EventBean theEvent = iterator.Advance();
            Assert.AreEqual(1d, theEvent.Get("s0.DoubleBoxed"));
            Assert.AreEqual(3d, theEvent.Get("div"));
        }
    
        private void SendEvent(String s, double doubleBoxed, int intPrimitive, int intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = doubleBoxed;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
