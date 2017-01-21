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
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestPatternGuardPlugIn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddPlugInPatternGuard("myplugin", "count_to", typeof(MyCountToPatternGuardFactory).FullName);
            configuration.AddEventType<SupportBean>("Bean");
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestGuard()
        {
            const string stmtText = "select * from pattern [(every Bean) where myplugin:count_to(10)]";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;
    
            for (int i = 0; i < 10; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.IsTrue(_listener.IsInvoked);
                _listener.Reset();
            }
            
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestGuardVariable()
        {
            _epService.EPAdministrator.CreateEPL("create variable int COUNT_TO = 3");
            const string stmtText = "select * from pattern [(every Bean) where myplugin:count_to(COUNT_TO)]";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;
    
            for (int i = 0; i < 3; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.IsTrue(_listener.IsInvoked);
                _listener.Reset();
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                Configuration configuration = SupportConfigFactory.GetConfiguration();
                configuration.AddPlugInPatternGuard("namespace", "name", typeof(string).FullName);
                _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
                _epService.Initialize();
                String stmtText = "select * from pattern [every " + typeof(SupportBean).FullName +
                                   " where namespace:name(10)]";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex, "Failed to resolve pattern guard 'com.espertech.esper.support.bean.SupportBean where namespace:name(10)': Error invoking pattern object factory constructor for object 'name', no invocation access for Activator.CreateInstance [select * from pattern [every com.espertech.esper.support.bean.SupportBean where namespace:name(10)]]");
            }
        }
    }
}
