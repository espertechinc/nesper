///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern {
    public class ExecPatternRepeatRouteEvent : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionRouteSingle(epService);
                RunAssertionRouteCascade(epService);
                RunAssertionRouteTimer(epService);
            }
        }

        /// <summary>
        /// Test route of an event within a listener.
        /// The Listener when it receives an event will generate a single new event
        /// that it routes back into the runtime, up to X number of times.
        /// </summary>
        private void RunAssertionRouteSingle(EPServiceProvider epService) {
            var epl = "every tag=" + typeof(SupportBean).FullName;
            var patternStmt = epService.EPAdministrator.CreatePattern(epl);

            var listener = new SingleRouteUpdateListener(epService, RouteEvent);
            patternStmt.Events += listener.Update;

            // Send first event that triggers the loop
            sendEvent(epService, 0);

            // Should have fired X times
            Assert.AreEqual(1000, listener.Count);

            // test route map and XML doc - smoke test
            patternStmt.Events += (sender, args) => {
                var theEvent = GetXMLEvent("<root><value>5</value></root>");
                epService.EPRuntime.Route(theEvent);
                epService.EPRuntime.Route(new Dictionary<string, object>(), "MyMap");
            };
        }

        /// <summary>
        /// Test route of multiple events within a listener.
        /// The Listener when it receives an event will generate multiple new events
        /// that it routes back into the runtime, up to X number of times.
        /// </summary>
        /// <param name="epService">The ep service.</param>
        private void RunAssertionRouteCascade(EPServiceProvider epService) {
            var epl = "every tag=" + typeof(SupportBean).FullName;
            var patternStmt = epService.EPAdministrator.CreatePattern(epl);

            var listener = new CascadeRouteUpdateListener(epService, RouteEvent);
            patternStmt.Events += listener.Update;

            // Send first event that triggers the loop
            sendEvent(epService, 2); // the 2 translates to number of new events routed

            // Should have fired X times
            Assert.AreEqual(9, listener.CountReceived);
            Assert.AreEqual(8, listener.CountRouted);

            //  Num    Received         Routes      Num
            //  2             1           2         3
            //  3             2           6         4
            //  4             6             -
        }

        private void RunAssertionRouteTimer(EPServiceProvider epService) {
            var epl = "every tag=" + typeof(SupportBean).FullName;
            var patternStmt = epService.EPAdministrator.CreatePattern(epl);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            // define time-based pattern and listener
            epl = "timer:at(*,*,*,*,*,*)";
            var atPatternStmt = epService.EPAdministrator.CreatePattern(epl);
            var timeListener = new SingleRouteUpdateListener(epService, RouteEvent);
            atPatternStmt.Events += timeListener.Update;

            // register regular listener
            var eventListener = new SingleRouteUpdateListener(epService, RouteEvent);
            patternStmt.Events += eventListener.Update;

            Assert.AreEqual(0, timeListener.Count);
            Assert.AreEqual(0, eventListener.Count);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));

            Assert.AreEqual(1, timeListener.Count);
            Assert.AreEqual(1000, eventListener.Count);
        }

        private void sendEvent(EPServiceProvider epService, int intValue) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            epService.EPRuntime.SendEvent(theEvent);
        }

        private void RouteEvent(EPServiceProvider epService, int intValue) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            epService.EPRuntime.Route(theEvent);
        }

        private XmlNode GetXMLEvent(String xml) {
            try {
                return SupportXML.GetDocument(xml);
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex);
            }
        }

        public class SingleRouteUpdateListener {
            private readonly EPServiceProvider _epService;
            private readonly Action<EPServiceProvider, int> _routeEvent;
            private int _count = 0;

            public SingleRouteUpdateListener(EPServiceProvider epService, Action<EPServiceProvider, int> routeEvent) {
                _epService = epService;
                _routeEvent = routeEvent;
            }

            public void Update(object sender, UpdateEventArgs e) {
                _count++;
                if (_count < 1000) {
                    _routeEvent(_epService, 0);
                }
            }

            public int Count => _count;
        }

        public class CascadeRouteUpdateListener {
            private readonly EPServiceProvider _epService;
            private readonly Action<EPServiceProvider, int> _routeEvent;
            private int _countReceived = 0;
            private int _countRouted = 0;

            public CascadeRouteUpdateListener(EPServiceProvider epService, Action<EPServiceProvider, int> routeEvent) {
                _epService = epService;
                _routeEvent = routeEvent;
            }

            public void Update(object sender, UpdateEventArgs e)
            {
                _countReceived++;
                var theEvent = (SupportBean) e.NewEvents[0].Get("tag");
                var numNewEvents = theEvent.IntPrimitive;

                for (var i = 0; i < numNewEvents; i++) {
                    if (numNewEvents < 4) {
                        _routeEvent(_epService, numNewEvents + 1);
                        _countRouted++;
                    }
                }
            }

            public int CountReceived => _countReceived;

            public int CountRouted => _countRouted;
        }
    }
}
