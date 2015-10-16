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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.plugin;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestPlugInEventRepresentation
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener[] _listeners;

        [SetUp]
        public void SetUp()
        {
            _listeners = new SupportUpdateListener[5];
            for (int i = 0; i < _listeners.Length; i++)
            {
                _listeners[i] = new SupportUpdateListener();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _listeners = null;
        }

        /// <summary>
        /// Use case 1: static event type resolution, no event object reflection (static event type assignment)
        /// Use case 2: static event type resolution, dynamic event object reflection and event type assignment
        ///   a) Register all representations with Uri via configuration
        ///   b) Register event type name and specify the list of Uri to use for resolving:
        ///     // at engine initialization time it obtain instances of an EventType for each name
        ///   c) Create statement using the registered event type name
        ///   d) Get EventSender to send in that specific type of event
        /// </summary>
        [Test]
        public void TestPreConfigStaticTypeResolution()
        {
            Configuration configuration = GetConfiguration();
            configuration.AddPlugInEventType("TestTypeOne", new Uri[]{new Uri("type://properties/test1/testtype")}, "t1");
            configuration.AddPlugInEventType("TestTypeTwo", new Uri[]{new Uri("type://properties/test2")}, "t2");
            configuration.AddPlugInEventType("TestTypeThree", new Uri[]{new Uri("type://properties/test3")}, "t3");
            configuration.AddPlugInEventType("TestTypeFour", new Uri[]{new Uri("type://properties/test2/x"), new Uri("type://properties/test3")}, "t4");
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            RunAssertionCaseStatic(_epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestRuntimeConfigStaticTypeResolution()
        {
            Configuration configuration = GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            ConfigurationOperations runtimeConfig = _epService.EPAdministrator.Configuration;
            runtimeConfig.AddPlugInEventType("TestTypeOne", new Uri[]{new Uri("type://properties/test1/testtype")}, "t1");
            runtimeConfig.AddPlugInEventType("TestTypeTwo", new Uri[]{new Uri("type://properties/test2")}, "t2");
            runtimeConfig.AddPlugInEventType("TestTypeThree", new Uri[]{new Uri("type://properties/test3")}, "t3");
            runtimeConfig.AddPlugInEventType("TestTypeFour", new Uri[]{new Uri("type://properties/test2/x"), new Uri("type://properties/test3")}, "t4");
    
            RunAssertionCaseStatic(_epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        /// <summary>
        /// Use case 3: dynamic event type resolution
        ///    a) Register all representations with Uri via configuration
        ///    b) Via configuration, set a list of URIs to use for resolving new event type names
        ///    c) Compile statement with an event type name that is not defined yet, each of the representations are asked to accept, in Uri hierarchy order
        ///      admin.CreateEPL("select a, b, c from MyEventType");
        ///      engine asks each event representation to create an EventType, takes the first valid one
        ///    d) Get EventSender to send in that specific type of event, or a Uri-list dynamic reflection sender
        /// </summary>
        [Test]
        public void TestRuntimeConfigDynamicTypeResolution()
        {
            Configuration configuration = GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            Uri[] uriList = new Uri[] { new Uri("type://properties/test2/myresolver") };
            _epService.EPAdministrator.Configuration.PlugInEventTypeResolutionURIs = uriList;
    
            RunAssertionCaseDynamic(_epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestStaticConfigDynamicTypeResolution()
        {
            Uri[] uriList = new Uri[] { new Uri("type://properties/test2/myresolver") };
            Configuration configuration = GetConfiguration();
            configuration.PlugInEventTypeResolutionURIs = uriList;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            RunAssertionCaseDynamic(_epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestInvalid()
        {
            Configuration configuration = GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            try {
                _epService.EPRuntime.GetEventSender(new Uri[0]);
                Assert.Fail();
            } catch (EventTypeException ex) {
                Assert.AreEqual("Event sender for resolution URIs '[]' did not return at least one event representation's event factory", ex.Message);
            }


            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestContextContents()
        {
            Configuration configuration = GetConfiguration();
            configuration.AddPlugInEventRepresentation(new Uri("type://test/support"), typeof(SupportEventRepresentation).FullName, "abc");
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            PlugInEventRepresentationContext initContext = SupportEventRepresentation.InitContext;
            Assert.AreEqual(new Uri("type://test/support"), initContext.EventRepresentationRootURI);
            Assert.AreEqual("abc", initContext.RepresentationInitializer);
            Assert.NotNull(initContext.EventAdapterService);
    
            ConfigurationOperations runtimeConfig = _epService.EPAdministrator.Configuration;
            runtimeConfig.AddPlugInEventType("TestTypeOne", new Uri[] { new Uri("type://test/support?a=b&c=d") }, "t1");
    
            PlugInEventTypeHandlerContext context = SupportEventRepresentation.AcceptTypeContext;
            Assert.AreEqual(new Uri("type://test/support?a=b&c=d"), context.EventTypeResolutionURI);
            Assert.AreEqual("t1", context.TypeInitializer);
            Assert.AreEqual("TestTypeOne", context.EventTypeName);
    
            context = SupportEventRepresentation.EventTypeContext;
            Assert.AreEqual(new Uri("type://test/support?a=b&c=d"), context.EventTypeResolutionURI);
            Assert.AreEqual("t1", context.TypeInitializer);
            Assert.AreEqual("TestTypeOne", context.EventTypeName);

            _epService.EPRuntime.GetEventSender(new Uri[] { new Uri("type://test/support?a=b") });
            PlugInEventBeanReflectorContext contextBean = SupportEventRepresentation.EventBeanContext;
            Assert.AreEqual("type://test/support?a=b", contextBean.ResolutionURI.ToString());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunAssertionCaseDynamic(EPServiceProvider epService)
        {
            // type resolved for each by the first event representation picking both up, i.e. the one with "r2" since that is the most specific Uri
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeOne");
            stmt.Events += _listeners[0].Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeTwo");
            stmt.Events += _listeners[1].Update;
    
            // static senders
            EventSender sender = epService.EPRuntime.GetEventSender("TestTypeOne");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "A" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[0].AssertOneGetNewAndReset(), new Object[]{"A"});
            Assert.IsFalse(_listeners[0].IsInvoked);
    
            sender = epService.EPRuntime.GetEventSender("TestTypeTwo");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "B" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[1].AssertOneGetNewAndReset(), new Object[]{"B"});
        }
    
        private Configuration GetConfiguration()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddPlugInEventRepresentation(new Uri("type://properties"), typeof(MyPlugInEventRepresentation).FullName, "r3");
            configuration.AddPlugInEventRepresentation(new Uri("type://properties/test1"), typeof(MyPlugInEventRepresentation).FullName, "r1");
            configuration.AddPlugInEventRepresentation(new Uri("type://properties/test2"), typeof(MyPlugInEventRepresentation).FullName, "r2");
            return configuration;
        }
    
        private void RunAssertionCaseStatic(EPServiceProvider epService)
        {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeOne");
            stmt.Events += _listeners[0].Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeTwo");
            stmt.Events += _listeners[1].Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeThree");
            stmt.Events += _listeners[2].Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeFour");
            stmt.Events += _listeners[3].Update;
    
            // static senders
            EventSender sender = epService.EPRuntime.GetEventSender("TestTypeOne");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r1", "A" }, new String[] { "t1", "B" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[0].AssertOneGetNewAndReset(), new Object[]{"A", "B"});
            Assert.IsFalse(_listeners[3].IsInvoked || _listeners[1].IsInvoked || _listeners[2].IsInvoked);
    
            sender = epService.EPRuntime.GetEventSender("TestTypeTwo");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "C" }, new String[] { "t2", "D" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[1].AssertOneGetNewAndReset(), new Object[]{"C", "D"});
            Assert.IsFalse(_listeners[3].IsInvoked || _listeners[0].IsInvoked || _listeners[2].IsInvoked);
    
            sender = epService.EPRuntime.GetEventSender("TestTypeThree");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r3", "E" }, new String[] { "t3", "F" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[2].AssertOneGetNewAndReset(), new Object[]{"E", "F"});
            Assert.IsFalse(_listeners[3].IsInvoked || _listeners[1].IsInvoked || _listeners[0].IsInvoked);
    
            sender = epService.EPRuntime.GetEventSender("TestTypeFour");
            sender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "G" }, new String[] { "t4", "H" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[3].AssertOneGetNewAndReset(), new Object[]{"G", "H"});
            Assert.IsFalse(_listeners[0].IsInvoked || _listeners[1].IsInvoked || _listeners[2].IsInvoked);
    
            // dynamic sender - decides on event type thus a particular Update listener should see the event
            Uri[] uriList = new Uri[] { new Uri("type://properties/test1"), new Uri("type://properties/test2") };
            EventSender dynamicSender = epService.EPRuntime.GetEventSender(uriList);
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r3", "I" }, new String[] { "t3", "J" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[2].AssertOneGetNewAndReset(), new Object[]{"I", "J"});
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r1", "K" }, new String[] { "t1", "L" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[0].AssertOneGetNewAndReset(), new Object[]{"K", "L"});
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "M" }, new String[] { "t2", "Count" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[1].AssertOneGetNewAndReset(), new Object[]{"M", "Count"});
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "O" }, new String[] { "t4", "P" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[3].AssertOneGetNewAndReset(), new Object[]{"O", "P"});
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "O" }, new String[] { "t3", "P" } }));
            AssertNoneReceived();

            uriList = new Uri[] { new Uri("type://properties/test2") };
            dynamicSender = epService.EPRuntime.GetEventSender(uriList);
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r1", "I" }, new String[] { "t1", "J" } }));
            AssertNoneReceived();
            dynamicSender.SendEvent(MakeProperties(new String[][] { new String[] { "r2", "Q" }, new String[] { "t2", "R" } }));
            EPAssertionUtil.AssertAllPropsSortedByName(_listeners[1].AssertOneGetNewAndReset(), new Object[]{"Q", "R"});
        }
    
        private void AssertNoneReceived()
        {
            for (int i = 0; i < _listeners.Length; i++)
            {
                Assert.IsFalse(_listeners[i].IsInvoked);
            }
        }
    
        private static Properties MakeProperties(String[][] values) 
        {
            var theEvent = new Properties();
            for (int i = 0; i < values.Length; i++)
            {
                theEvent.Put(values[i][0], values[i][1]);
            }
            return theEvent;
        }
    }
}
