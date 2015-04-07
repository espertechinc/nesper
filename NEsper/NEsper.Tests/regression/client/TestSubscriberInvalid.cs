///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestSubscriberInvalid 
    {
        private EPServiceProvider _epService;
        private EPAdministrator _epAdmin;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            String pkg = typeof(SupportBean).Namespace;
            config.AddEventTypeAutoName(pkg);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epAdmin = _epService.EPAdministrator;
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epAdmin = null;
        }
    
        [Test]
        public void TestBindWildcardJoin()
        {
            EPStatement stmt = _epAdmin.CreateEPL("select * from SupportBean");
            TryInvalid(this, stmt, "EPSubscriber object does not provide a public method by name 'Update'");
            TryInvalid(new DummySubscriberEmptyUpd(), stmt, "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type com.espertech.esper.support.bean.SupportBean");
            TryInvalid(new DummySubscriberMultipleUpdate(), stmt, "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type com.espertech.esper.support.bean.SupportBean");
            TryInvalid(new DummySubscriberUpdate(), stmt, "EPSubscriber method named 'Update' for parameter number 1 is not assignable, expecting type 'com.espertech.esper.support.bean.SupportBean' but found type 'com.espertech.esper.support.bean.SupportMarketDataBean'");
            TryInvalid(new DummySubscriberPrivateUpd(), stmt, "EPSubscriber object does not provide a public method by name 'Update'");
        }
    
        [Test]
        public void TestInvocationTargetEx()
        {
            // smoke test, need to consider log file; test for ESPER-331 
            EPStatement stmt = _epAdmin.CreateEPL("select * from SupportMarketDataBean");
            stmt.Subscriber = new DummySubscriberException();
            stmt.Events += (sender, args) => { throw new ApplicationException("test exception 1"); };
            stmt.Events += (sender, args) => { throw new ApplicationException("test exception 2"); };
            stmt.AddEventHandlerWithReplay(
                (sender, args) => { throw new ApplicationException("test exception 3"); });
    
            // no exception expected
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 0L, ""));
        }
    
        private void TryInvalid(Object subscriber, EPStatement stmt, String message)
        {
            try
            {
                stmt.Subscriber = subscriber;
                Assert.Fail();
            }
            catch (EPSubscriberException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        public class DummySubscriberException
        {
            public void Update(SupportMarketDataBean bean) {
                throw new ApplicationException("DummySubscriberException-generated");
            }
        }
    
        public class DummySubscriberEmptyUpd
        {
            public void Update() {}
        }
    
        public class DummySubscriberPrivateUpd
        {
            private void Update(SupportBean bean) {}
        }
    
        public class DummySubscriberUpdate
        {
            public void Update(SupportMarketDataBean dummy) {}
        }
    
        public class DummySubscriberMultipleUpdate
        {
            public void Update(long x) {}
            public void Update(int x) {}
        }
    }
}
