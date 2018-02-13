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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

using static com.espertech.esper.regression.events.plugin.ExecEventPlugInConfigRuntimeTypeResolution;

namespace com.espertech.esper.regression.events.plugin {
    public class ExecEventPlugInConfigStaticTypeResolution : RegressionExecution {
        /*
         * Use case 1: static event type resolution, no event object reflection (static event type assignment)
         * Use case 2: static event type resolution, dynamic event object reflection and event type assignment
         *   a) Register all representations with URI via configuration
         *   b) Register event type name and specify the list of URI to use for resolving:
         *     // at engine initialization time it obtain instances of an EventType for each name
         *   c) Create statement using the registered event type name
         *   d) Get EventSender to send in that specific type of event
         */

        public override void Configure(Configuration configuration) {
            ConfigureURIs(configuration);

            configuration.AddPlugInEventType("TestTypeOne", new[] {new Uri("type://properties/test1/testtype")}, "t1");
            configuration.AddPlugInEventType("TestTypeTwo", new[] {new Uri("type://properties/test2")}, "t2");
            configuration.AddPlugInEventType("TestTypeThree", new[] {new Uri("type://properties/test3")}, "t3");
            configuration.AddPlugInEventType(
                "TestTypeFour", new[] {new Uri("type://properties/test2/x"), new Uri("type://properties/test3")}, "t4");
        }

        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventPlugInConfigStaticTypeResolution))) {
                return;
            }

            RunAssertionCaseStatic(epService);
        }

        public static void RunAssertionCaseStatic(EPServiceProvider epService) {
            var listeners = SupportUpdateListener.MakeListeners(5);
            var stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeOne");
            stmt.AddListener(listeners[0]);
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeTwo");
            stmt.AddListener(listeners[1]);
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeThree");
            stmt.AddListener(listeners[2]);
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeFour");
            stmt.AddListener(listeners[3]);

            // static senders
            var sender = epService.EPRuntime.GetEventSender("TestTypeOne");
            sender.SendEvent(MakeProperties(new string[][] {{"r1", "A"}, {"t1", "B"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[0].AssertOneGetNewAndReset(), new object[] {"A", "B"});
            Assert.IsFalse(listeners[3].IsInvoked || listeners[1].IsInvoked || listeners[2].IsInvoked);

            sender = epService.EPRuntime.GetEventSender("TestTypeTwo");
            sender.SendEvent(MakeProperties(new string[][] {{"r2", "C"}, {"t2", "D"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[1].AssertOneGetNewAndReset(), new object[] {"C", "D"});
            Assert.IsFalse(listeners[3].IsInvoked || listeners[0].IsInvoked || listeners[2].IsInvoked);

            sender = epService.EPRuntime.GetEventSender("TestTypeThree");
            sender.SendEvent(MakeProperties(new string[][] {{"r3", "E"}, {"t3", "F"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[2].AssertOneGetNewAndReset(), new object[] {"E", "F"});
            Assert.IsFalse(listeners[3].IsInvoked || listeners[1].IsInvoked || listeners[0].IsInvoked);

            sender = epService.EPRuntime.GetEventSender("TestTypeFour");
            sender.SendEvent(MakeProperties(new string[][] {{"r2", "G"}, {"t4", "H"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[3].AssertOneGetNewAndReset(), new object[] {"G", "H"});
            Assert.IsFalse(listeners[0].IsInvoked || listeners[1].IsInvoked || listeners[2].IsInvoked);

            // dynamic sender - decides on event type thus a particular update listener should see the event
            var uriList = new[] {new Uri("type://properties/test1"), new Uri("type://properties/test2")};
            var dynamicSender = epService.EPRuntime.GetEventSender(uriList);
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r3", "I"}, {"t3", "J"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[2].AssertOneGetNewAndReset(), new object[] {"I", "J"});
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r1", "K"}, {"t1", "L"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[0].AssertOneGetNewAndReset(), new object[] {"K", "L"});
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r2", "M"}, {"t2", "N"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[1].AssertOneGetNewAndReset(), new object[] {"M", "N"});
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r2", "O"}, {"t4", "P"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[3].AssertOneGetNewAndReset(), new object[] {"O", "P"});
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r2", "O"}, {"t3", "P"}}));
            AssertNoneReceived(listeners);

            uriList = new[] {new Uri("type://properties/test2")};
            dynamicSender = epService.EPRuntime.GetEventSender(uriList);
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r1", "I"}, {"t1", "J"}}));
            AssertNoneReceived(listeners);
            dynamicSender.SendEvent(MakeProperties(new string[][] {{"r2", "Q"}, {"t2", "R"}}));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[1].AssertOneGetNewAndReset(), new object[] {"Q", "R"});
        }

        private static void AssertNoneReceived(SupportUpdateListener[] listeners) {
            for (var i = 0; i < listeners.Length; i++) {
                Assert.IsFalse(listeners[i].IsInvoked);
            }
        }

        private static Properties MakeProperties(string[][] values) {
            var theEvent = new Properties();
            for (var i = 0; i < values.Length; i++) {
                theEvent.Put(values[i][0], values[i][1]);
            }

            return theEvent;
        }
    }
} // end of namespace