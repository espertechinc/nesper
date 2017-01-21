///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewStartStop 
    {
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
        public void TestSameWindowReuse()
        {
            String viewExpr = "select * from " + typeof(SupportBean).FullName + ".win:length(3)";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmtOne.Events += _testListener.Update;
    
            // send a couple of events
            SendEvent(1);
            SendEvent(2);
            SendEvent(3);
            SendEvent(4);
    
            // create same statement again
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmtTwo.Events += testListenerTwo.Update;
    
            // Send event, no old data should be received
            SendEvent(5);
            Assert.IsNull(testListenerTwo.LastOldData);
        }
    
        [Test]
        public void TestStartStop()
        {
            String viewExpr = "select count(*) as size from " + typeof(SupportBean).FullName;
            EPStatement sizeView = _epService.EPAdministrator.CreateEPL(viewExpr);
    
            // View created is automatically started
            Assert.AreEqual(0l, sizeView.FirstOrDefault().Get("size"));
            sizeView.Stop();

            // Send an event, view stopped
            SendEvent();
            var sizeViewEnum = sizeView.GetEnumerator();
            Assert.IsNotNull(sizeViewEnum);
            var sizeViewArray = sizeViewEnum.EnumeratorToArray();
            Assert.That(sizeViewArray.Length, Is.EqualTo(0));
    
            // Start view
            sizeView.Start();
            Assert.AreEqual(0l, sizeView.FirstOrDefault().Get("size"));
    
            // Send event
            SendEvent();
            Assert.AreEqual(1l, sizeView.FirstOrDefault().Get("size"));
    
            // Stop view
            sizeView.Stop();
            Assert.That(sizeView.GetEnumerator(), Is.InstanceOf<NullEnumerator<EventBean>>());
    
            // Start again, iterator is zero
            sizeView.Start();
            Assert.AreEqual(0l, sizeView.FirstOrDefault().Get("size"));
        }
    
        [Test]
        public void TestAddRemoveListener()
        {
            String viewExpr = "select count(*) as size from " + typeof(SupportBean).FullName;
            EPStatement sizeView = _epService.EPAdministrator.CreateEPL(viewExpr);
    
            // View is started when created
    
            // Add listener send event
            sizeView.Events += _testListener.Update;
            Assert.IsNull(_testListener.LastNewData);
            Assert.AreEqual(0l, sizeView.FirstOrDefault().Get("size"));
            SendEvent();
            Assert.AreEqual(1l, _testListener.GetAndResetLastNewData()[0].Get("size"));
            Assert.AreEqual(1l, sizeView.FirstOrDefault().Get("size"));
    
            // Stop view, send event, view
            sizeView.Stop();
            SendEvent();
            Assert.That(sizeView.GetEnumerator(), Is.InstanceOf<NullEnumerator<EventBean>>());
            Assert.IsNull(_testListener.LastNewData);
    
            // Start again
            sizeView.Events -= _testListener.Update;
            sizeView.Events += _testListener.Update;
            sizeView.Start();
    
            SendEvent();
            Assert.AreEqual(1l, _testListener.GetAndResetLastNewData()[0].Get("size"));
            Assert.AreEqual(1l, sizeView.FirstOrDefault().Get("size"));
    
            // Stop again, leave listeners
            sizeView.Stop();
            sizeView.Start();
            SendEvent();
            Assert.AreEqual(1l, _testListener.GetAndResetLastNewData()[0].Get("size"));
    
            // Remove listener, send event
            sizeView.Events -= _testListener.Update;
            SendEvent();
            Assert.IsNull(_testListener.LastNewData);
    
            // Add listener back, send event
            sizeView.Events += _testListener.Update;
            SendEvent();
            Assert.AreEqual(3l, _testListener.GetAndResetLastNewData()[0].Get("size"));
        }
    
        private void SendEvent()
        {
            SendEvent(-1);
        }
    
        private void SendEvent(int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
