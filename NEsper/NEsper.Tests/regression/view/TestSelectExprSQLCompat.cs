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
    public class TestSelectExprSQLCompat 
    {
        private SupportUpdateListener _testListener;
        private Configuration _config;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _config = SupportConfigFactory.GetConfiguration();
            _config.AddEventType("SupportBean", typeof(SupportBean));
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
            _config = null;
        }
    
        [Test]
        public void TestQualifiedPropertyNamed()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetProvider("default", _config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            RunAssertionProperty(epService);
            RunAssertionPrefixStream(epService);
        }
    
        [Test]
        public void TestQualifiedPropertyUnnamed()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(_config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            RunAssertionProperty(epService);
            RunAssertionPrefixStream(epService);

            // allow no as-keyword
            epService.EPAdministrator.CreateEPL("select IntPrimitive abc from SupportBean");
        }
    
        private void RunAssertionProperty(EPServiceProvider engine)
        {
            String epl = "select default.SupportBean.TheString as val1, SupportBean.IntPrimitive as val2 from SupportBean";
            EPStatement selectTestView = engine.EPAdministrator.CreateEPL(epl);
            selectTestView.Events += _testListener.Update;
    
            SendEvent(engine, "E1", 10);
            EventBean received = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("E1", received.Get("val1"));
            Assert.AreEqual(10, received.Get("val2"));
        }
    
        // Test stream name prefixed by engine URI
        private void RunAssertionPrefixStream(EPServiceProvider engine)
        {
            String epl = "select TheString from default.SupportBean";
            EPStatement selectTestView = engine.EPAdministrator.CreateEPL(epl);
            selectTestView.Events += _testListener.Update;
    
            SendEvent(engine, "E1", 10);
            EventBean received = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("E1", received.Get("TheString"));
        }
    
        private void SendEvent(EPServiceProvider engine, String s, int intPrimitive)
        {
            SupportBean bean = new SupportBean(s, intPrimitive);
            engine.EPRuntime.SendEvent(bean);
        }
    }
}
