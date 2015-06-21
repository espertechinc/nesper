///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPServiceProvider 
    {
        private EPServiceProvider _epService;
        private SupportServiceStateListener _listener;
        private SupportServiceStateListener _listenerTwo;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportServiceStateListener();
            _listenerTwo = new SupportServiceStateListener();
    
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
            _listenerTwo = null;
        }
    
        [Test]
        public void TestUnit()
        {
            using(_epService.EngineInstanceWideLock.AcquireWriteLock())
            {
            }
        }

        [Test]
        public void TestDefaultEngine()
        {
            Assert.AreEqual("default", EPServiceProviderManager.GetDefaultProvider().URI);
            var engineDefault = EPServiceProviderManager.GetDefaultProvider();
            Assert.IsTrue(engineDefault.EPRuntime.IsExternalClockingEnabled);
    
            var engine = EPServiceProviderManager.GetProvider("default");
            Assert.AreSame(engineDefault, engine);
    
            engine = EPServiceProviderManager.GetProvider(null);
            Assert.AreSame(engineDefault, engine);
    
            engine = EPServiceProviderManager.GetProvider(null, SupportConfigFactory.GetConfiguration());
            Assert.AreSame(engineDefault, engine);
    
            var uris = EPServiceProviderManager.ProviderURIs;
            Assert.IsTrue(uris.Contains("default"));
            
            _epService.Dispose();
            try {
                var temp = _epService.EPRuntime;
                Assert.Fail();
            }
            catch (EPServiceDestroyedException) {
                // expected
            }
            try {
                var temp = _epService.EPAdministrator;
                Assert.Fail();
            }
            catch (EPServiceDestroyedException) {
                // expected
            }
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, "default");
    
            // test destroy
            var config = SupportConfigFactory.GetConfiguration();
            var uriOne = GetType().FullName + "_1";
            var engineOne = EPServiceProviderManager.GetProvider(uriOne, config);
            var uriTwo = GetType().FullName + "_2";
            var engineTwo = EPServiceProviderManager.GetProvider(uriTwo, config);
            EPAssertionUtil.AssertContains(EPServiceProviderManager.ProviderURIs, uriOne, uriTwo);
            Assert.IsNotNull(EPServiceProviderManager.GetExistingProvider(uriOne));
            Assert.IsNotNull(EPServiceProviderManager.GetExistingProvider(uriTwo));
    
            engineOne.Dispose();
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, uriOne);
            EPAssertionUtil.AssertContains(EPServiceProviderManager.ProviderURIs, uriTwo);
            Assert.IsNull(EPServiceProviderManager.GetExistingProvider(uriOne));

            engineTwo.Dispose();
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, uriOne, uriTwo);
            Assert.IsNull(EPServiceProviderManager.GetExistingProvider(uriTwo));
        }
    
        [Test]
        public void TestListenerStateChange()
        {
            _epService.ServiceInitialized += _listener.OnEPServiceInitialized;
            _epService.ServiceDestroyRequested += _listener.OnEPServiceDestroyRequested;
            _epService.Dispose();
            Assert.AreSame(_epService, _listener.AssertOneGetAndResetDestroyedEvents());
    
            _epService.Initialize();
            Assert.AreSame(_epService, _listener.AssertOneGetAndResetInitializedEvents());

            _epService.RemoveAllServiceStateEventHandlers();
            _epService.Initialize();
            Assert.IsTrue(_listener.InitializedEvents.IsEmpty());

            _epService.ServiceInitialized += _listener.OnEPServiceInitialized;
            _epService.ServiceDestroyRequested += _listener.OnEPServiceDestroyRequested;
            _epService.ServiceInitialized += _listenerTwo.OnEPServiceInitialized;
            _epService.ServiceDestroyRequested += _listenerTwo.OnEPServiceDestroyRequested;

            _epService.Initialize();
            Assert.AreSame(_epService, _listener.AssertOneGetAndResetInitializedEvents());
            Assert.AreSame(_epService, _listenerTwo.AssertOneGetAndResetInitializedEvents());

            _epService.ServiceInitialized -= _listener.OnEPServiceInitialized;
            _epService.ServiceDestroyRequested -= _listener.OnEPServiceDestroyRequested;
            _epService.Initialize();

            Assert.AreSame(_epService, _listenerTwo.AssertOneGetAndResetInitializedEvents());
            Assert.IsTrue(_listener.InitializedEvents.IsEmpty());
        }
    
        [Test]
        public void TestStatementStateChange()
        {
            var spi = (EPServiceProviderSPI) _epService;
    
            var observer = new SupportStmtLifecycleObserver();
            spi.StatementLifecycleSvc.LifecycleEvent += observer.Observe;
            var listener = new SupportStatementStateListener();
            _epService.StatementCreate += listener.OnStatementCreate;
            _epService.StatementStateChange += listener.OnStatementStateChange;
    
            var stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName);
            Assert.AreEqual("CREATE;STATECHANGE;", observer.EventsAsString);
            Assert.AreEqual(stmt, listener.AssertOneGetAndResetCreatedEvents());
            Assert.AreEqual(stmt, listener.AssertOneGetAndResetStateChangeEvents());
    
            observer.Flush();
            stmt.Stop();
            Assert.AreEqual("STATECHANGE;", observer.EventsAsString);
            Assert.AreEqual(stmt.Name, observer.Events[0].Statement.Name);
            Assert.AreEqual(stmt, listener.AssertOneGetAndResetStateChangeEvents());
    
            observer.Flush();
            stmt.Events += (sender, args) => { };
            Assert.AreEqual("LISTENER_ADD;", observer.EventsAsString);
            Assert.NotNull(observer.LastContext);
            Assert.IsTrue(observer.LastContext[0] is UpdateEventHandler);
    
            observer.Flush();
            stmt.RemoveAllEventHandlers();
            Assert.AreEqual(StatementLifecycleEvent.LifecycleEventType.LISTENER_REMOVE_ALL.ToString()+";", observer.EventsAsString);
    
            stmt.Dispose();
            Assert.AreEqual(stmt, listener.AssertOneGetAndResetStateChangeEvents());
        }
    
    }
}
