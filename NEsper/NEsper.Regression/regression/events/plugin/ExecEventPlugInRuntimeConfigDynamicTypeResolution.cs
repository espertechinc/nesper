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
    public class ExecEventPlugInRuntimeConfigDynamicTypeResolution : RegressionExecution {
        /*
         * Use case 3: dynamic event type resolution
         *   a) Register all representations with URI via configuration
         *   b) Via configuration, set a list of URIs to use for resolving new event type names
         *   c) Compile statement with an event type name that is not defined yet, each of the representations are asked to accept, in URI hierarchy order
         *     admin.CreateEPL("select a, b, c from MyEventType");
         *    // engine asks each event representation to create an EventType, takes the first valid one
         *   d) Get EventSender to send in that specific type of event, or a URI-list dynamic reflection sender
         */

        public override void Configure(Configuration configuration) {
            ConfigureURIs(configuration);
        }

        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventPlugInRuntimeConfigDynamicTypeResolution))) {
                return;
            }

            var uriList = new[] {new Uri("type://properties/test2/myresolver")};
            epService.EPAdministrator.Configuration.PlugInEventTypeResolutionURIs = uriList;

            RunAssertionCaseDynamic(epService);
        }

        public static void RunAssertionCaseDynamic(EPServiceProvider epService) {
            // type resolved for each by the first event representation picking both up, i.e. the one with "r2" since that is the most specific URI
            var stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeOne");
            var listeners = SupportUpdateListener.MakeListeners(5);
            stmt.Events += listeners[0].Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from TestTypeTwo");
            stmt.Events += listeners[1].Update;

            // static senders
            var sender = epService.EPRuntime.GetEventSender("TestTypeOne");
            sender.SendEvent(
                MakePropertiesFromStrings(
                    new string[][] {
                        new string[] {
                            "r2",
                            "A"
                        }
                    }));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[0].AssertOneGetNewAndReset(), new object[] {"A"});
            Assert.IsFalse(listeners[0].IsInvoked);

            sender = epService.EPRuntime.GetEventSender("TestTypeTwo");
            sender.SendEvent(
                MakePropertiesFromStrings(
                    new string[][] {
                        new string[] {
                            "r2",
                            "B"
                        }
                    }));
            EPAssertionUtil.AssertAllPropsSortedByName(listeners[1].AssertOneGetNewAndReset(), new object[] {"B"});
        }

        private static Properties MakePropertiesFromStrings(string[][] values) {
            var theEvent = new Properties();
            for (var i = 0; i < values.Length; i++) {
                theEvent.Put(values[i][0], values[i][1]);
            }

            return theEvent;
        }
    }
} // end of namespace