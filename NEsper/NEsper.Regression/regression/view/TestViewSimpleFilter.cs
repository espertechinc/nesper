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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewSimpleFilter
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp() {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestNotEqualsOp() {
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "(TheString != 'a')");
            statement.Events += _testListener.Update;
    
            SendEvent("a");
            Assert.IsFalse(_testListener.IsInvoked);

            Object theEvent = SendEvent("b");
            Assert.AreSame(theEvent, _testListener.GetAndResetLastNewData()[0].Underlying);
    
            SendEvent("a");
            Assert.IsFalse(_testListener.IsInvoked);
    
            theEvent = SendEvent(null);
            Assert.IsFalse(_testListener.IsInvoked);
        }
    
        [Test]
        public void TestCombinationEqualsOp() {
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "(TheString != 'a', IntPrimitive=0)");
            statement.Events += _testListener.Update;
    
            SendEvent("b", 1);
            Assert.IsFalse(_testListener.IsInvoked);
    
            SendEvent("a", 0);
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object theEvent = SendEvent("x", 0);
            Assert.AreSame(theEvent, _testListener.GetAndResetLastNewData()[0].Underlying);
    
            theEvent = SendEvent(null, 0);
            Assert.IsFalse(_testListener.IsInvoked);
        }
    
        private Object SendEvent(String stringValue) {
            return SendEvent(stringValue, -1);
        }
    
        private Object SendEvent(String stringValue, int intPrimitive) {
            SupportBean theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            theEvent.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
}
