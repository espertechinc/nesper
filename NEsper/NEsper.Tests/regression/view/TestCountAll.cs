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
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestCountAll
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private EPStatement _selectTestView;
    
        [SetUp]
        public void SetUp() {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestSize() {
            String statementText = "select irstream size from " + typeof(SupportMarketDataBean).FullName + ".std:size()";
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
    
            SendEvent("DELL", 1L);
            AssertSize(1, 0);
    
            SendEvent("DELL", 1L);
            AssertSize(2, 1);
    
            _selectTestView.Dispose();
            statementText = "select size, symbol, feed from " + typeof(SupportMarketDataBean).FullName + ".std:size(symbol, feed)";
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
            String[] fields = "size,symbol,feed".Split(',');
    
            SendEvent("DELL", 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1L, "DELL", "f1"});
    
            SendEvent("DELL", 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{2L, "DELL", "f1"});
        }
    
        [Test]
        public void TestCountPlusStar() {
            // Test for ESPER-118
            String statementText = "select *, count(*) as cnt from " + typeof(SupportMarketDataBean).FullName;
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
    
            SendEvent("S0", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(1L, _listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S0", _listener.LastNewData[0].Get("Symbol"));
    
            SendEvent("S1", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(2L, _listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S1", _listener.LastNewData[0].Get("Symbol"));
    
            SendEvent("S2", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(3L, _listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S2", _listener.LastNewData[0].Get("Symbol"));
        }
    
        [Test]
        public void TestCount() {
            String statementText = "select count(*) as cnt from " + typeof(SupportMarketDataBean).FullName + ".win:time(1)";
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
    
            SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(1L, _listener.LastNewData[0].Get("cnt"));
    
            SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(2L, _listener.LastNewData[0].Get("cnt"));
    
            SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(3L, _listener.LastNewData[0].Get("cnt"));

            // test invalid distinct
           SupportMessageAssertUtil.TryInvalid(_epService, "select count(distinct *) from " + typeof(SupportMarketDataBean).FullName,
                "Error starting statement: Failed to validate select-clause expression 'count(distinct *)': Invalid use of the 'distinct' keyword with count and wildcard");
        }
    
        [Test]
        public void TestCountHaving() {
            String theEvent = typeof(SupportBean).FullName;
            String statementText = "select irstream sum(IntPrimitive) as mysum from " + theEvent + " having sum(IntPrimitive) = 2";
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
    
            SendEvent();
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            SendEvent();
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("mysum"));
            SendEvent();
            Assert.AreEqual(2, _listener.AssertOneGetOldAndReset().Get("mysum"));
        }
    
        [Test]
        public void TestSumHaving() {
            String theEvent = typeof(SupportBean).FullName;
            String statementText = "select irstream count(*) as mysum from " + theEvent + " having count(*) = 2";
            _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
            _selectTestView.Events += _listener.Update;
    
            SendEvent();
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            SendEvent();
            Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("mysum"));
            SendEvent();
            Assert.AreEqual(2L, _listener.AssertOneGetOldAndReset().Get("mysum"));
        }
    
        private void SendEvent(String symbol, long? volume) {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent() {
            SupportBean bean = new SupportBean("", 1);
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void AssertSize(long newSize, long oldSize) {
            EPAssertionUtil.AssertPropsPerRow(_listener.AssertInvokedAndReset(), "size", new Object[]{newSize}, new Object[]{oldSize});
        }
    }
}
