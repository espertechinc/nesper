///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

// using static junit.framework.TestCase.*;
// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertNotNull;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPServiceProvider : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionObtainEngineWideRWLock(epService);
            RunAssertionDefaultEngine(epService);
            RunAssertionListenerStateChange();
            RunAssertionStatementStateChange();
            RunAssertionDestroy();
        }
    
        private void RunAssertionObtainEngineWideRWLock(EPServiceProvider epService) {
            epService.EngineInstanceWideLock.WriteLock().Lock();
            try {
                // some action here
            } finally {
                epService.EngineInstanceWideLock.WriteLock().Unlock();
            }
        }
    
        private void RunAssertionDefaultEngine(EPServiceProvider epService) {
            Assert.AreEqual("default", EPServiceProviderManager.GetDefaultProvider().URI);
            EPServiceProvider engineDefault = EPServiceProviderManager.GetDefaultProvider();
            Assert.IsTrue(engineDefault.EPRuntime.IsExternalClockingEnabled);
    
            EPServiceProvider engine = EPServiceProviderManager.GetProvider("default");
            Assert.AreSame(engineDefault, engine);
            Assert.AreSame(engineDefault, epService);
    
            engine = EPServiceProviderManager.GetProvider(null);
            Assert.AreSame(engineDefault, engine);
    
            engine = EPServiceProviderManager.GetProvider(null, SupportConfigFactory.GetConfiguration());
            Assert.AreSame(engineDefault, engine);
    
            string[] uris = EPServiceProviderManager.ProviderURIs;
            Assert.IsTrue(Collections.List(uris).Contains("default"));
        }
    
        private void RunAssertionDestroy() {
    
            // test destroy
            Configuration config = SupportConfigFactory.GetConfiguration();
            string uriOne = GetType().FullName + "_1";
            EPServiceProvider engineOne = EPServiceProviderManager.GetProvider(uriOne, config);
            string uriTwo = GetType().FullName + "_2";
            EPServiceProvider engineTwo = EPServiceProviderManager.GetProvider(uriTwo, config);
            EPAssertionUtil.AssertContains(EPServiceProviderManager.ProviderURIs, uriOne, uriTwo);
            Assert.IsNotNull(EPServiceProviderManager.GetExistingProvider(uriOne));
            Assert.IsNotNull(EPServiceProviderManager.GetExistingProvider(uriTwo));
    
            engineOne.Destroy();
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, uriOne);
            EPAssertionUtil.AssertContains(EPServiceProviderManager.ProviderURIs, uriTwo);
            Assert.IsNull(EPServiceProviderManager.GetExistingProvider(uriOne));
    
            engineTwo.Destroy();
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, uriOne, uriTwo);
            Assert.IsNull(EPServiceProviderManager.GetExistingProvider(uriTwo));
    
            try {
                engineTwo.EPRuntime;
                Assert.Fail();
            } catch (EPServiceDestroyedException ex) {
                // expected
            }
            try {
                engineTwo.EPAdministrator;
                Assert.Fail();
            } catch (EPServiceDestroyedException ex) {
                // expected
            }
            EPAssertionUtil.AssertNotContains(EPServiceProviderManager.ProviderURIs, uriTwo);
        }
    
        private void RunAssertionListenerStateChange() {
            var listener = new SupportServiceStateListener();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(this.GetType().Name + "__listenerstatechange", configuration);
            epService.AddServiceStateListener(listener);
            epService.Dispose();
            Assert.AreSame(epService, listener.AssertOneGetAndResetDestroyedEvents());
    
            epService.Initialize();
            Assert.AreSame(epService, listener.AssertOneGetAndResetInitializedEvents());
    
            epService.RemoveAllServiceStateListeners();
            epService.Initialize();
            Assert.IsTrue(listener.InitializedEvents.IsEmpty());
    
            epService.AddServiceStateListener(listener);
            var listenerTwo = new SupportServiceStateListener();
            epService.AddServiceStateListener(listenerTwo);
            epService.Initialize();
            Assert.AreSame(epService, listener.AssertOneGetAndResetInitializedEvents());
            Assert.AreSame(epService, listenerTwo.AssertOneGetAndResetInitializedEvents());
    
            epService.RemoveServiceStateListener(listener);
            epService.Initialize();
            Assert.AreSame(epService, listenerTwo.AssertOneGetAndResetInitializedEvents());
            Assert.IsTrue(listener.InitializedEvents.IsEmpty());
    
            epService.Dispose();
        }
    
        private void RunAssertionStatementStateChange() {
            EPServiceProvider stateChangeEngine = EPServiceProviderManager.GetProvider(this.GetType().Name + "_statechange", SupportConfigFactory.GetConfiguration());
            EPServiceProviderSPI spi = (EPServiceProviderSPI) stateChangeEngine;
    
            var observer = new SupportStmtLifecycleObserver();
            spi.StatementLifecycleSvc.AddObserver(observer);
            var listener = new SupportStatementStateListener();
            stateChangeEngine.AddStatementStateListener(listener);
    
            EPStatement stmt = stateChangeEngine.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName);
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
            Assert.IsNotNull(observer.LastContext);
            Assert.IsTrue(observer.LastContext[0] is UpdateListener);
    
            observer.Flush();
            stmt.RemoveAllEventHandlers();
            Assert.AreEqual(StatementLifecycleEvent.LifecycleEventType.LISTENER_REMOVE_ALL.ToString() + ";", observer.EventsAsString);
    
            stmt.Dispose();
            Assert.AreEqual(stmt, listener.AssertOneGetAndResetStateChangeEvents());
    
            stateChangeEngine.Destroy();
        }
    
    }
} // end of namespace
