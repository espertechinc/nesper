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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestRepeatRouteEvent 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        /// <summary>
        /// Test route of an event within a listener. The listener when it receives an event will generate a single 
        /// new event that it routes back into the runtime, up to X number of times.
        /// </summary>
        [Test]
        public void TestRouteSingle()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // excluded

            var viewExpr = "every tag=" + typeof(SupportBean).FullName;
            var patternStmt = _epService.EPAdministrator.CreatePattern(viewExpr);

            var listener = new SingleRouteUpdateListener { RouteEvent = RouteEvent };
            patternStmt.Events += listener.Update;
    
            // Send first event that triggers the loop
            SendEvent(0);
    
            // Should have fired X times
            Assert.AreEqual(1000, listener.Count);
    
            // test route map and XML doc - smoke test
            patternStmt.Events += (sender, e) =>
            {
                var theEvent = GetXMLEvent("<root><value>5</value></root>");
                e.ServiceProvider.EPRuntime.Route(theEvent);
                e.ServiceProvider.EPRuntime.Route(new Dictionary<string, object>(), "MyMap");
            };
        }
    
        /// <summary>Test route of multiple events within a listener. The Listener when it receives an event will generate multiple new events that it routes back into the runtime, up to X number of times. </summary>
        [Test]
        public void TestRouteCascade()
        {
            String viewExpr = "every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(viewExpr);

            CascadeRouteUpdateListener listener = new CascadeRouteUpdateListener { RouteEvent = RouteEvent };
            patternStmt.Events += listener.Update;
    
            // Send first event that triggers the loop
            SendEvent(2);       // the 2 translates to number of new events routed
    
            // Should have fired X times
            Assert.AreEqual(9, listener.CountReceived);
            Assert.AreEqual(8, listener.CountRouted);
    
            //  Num    Received         Routes      Num
            //  2             1           2         3
            //  3             2           6         4
            //  4             6             -
        }
    
        [Test]
        public void TestRouteTimer()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // excluded

            String viewExpr = "every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(viewExpr);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            // define time-based pattern and listener
            viewExpr = "timer:at(*,*,*,*,*,*)";
            EPStatement atPatternStmt = _epService.EPAdministrator.CreatePattern(viewExpr);
            SingleRouteUpdateListener timeListener = new SingleRouteUpdateListener { RouteEvent = RouteEvent };
            atPatternStmt.Events += timeListener.Update;
    
            // register regular listener
            SingleRouteUpdateListener eventListener = new SingleRouteUpdateListener { RouteEvent = RouteEvent };
            patternStmt.Events += eventListener.Update;
    
            Assert.AreEqual(0, timeListener.Count);
            Assert.AreEqual(0, eventListener.Count);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
    
            Assert.AreEqual(1, timeListener.Count);
            Assert.AreEqual(1000, eventListener.Count);
        }
    
        private SupportBean SendEvent(int intValue)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean RouteEvent(int intValue)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            _epService.EPRuntime.Route(theEvent);
            return theEvent;
        }

        private XmlNode GetXMLEvent(String xml)
        {
            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
            return simpleDoc;
        }

        class SingleRouteUpdateListener
        {
            public Func<int, SupportBean> RouteEvent { get; set; }

            public int Count { get; private set; }

            public SingleRouteUpdateListener()
            {
                Count = 0;
            }

            public void Update(object sender, UpdateEventArgs updateEventArgs)
            {
                Count++;
                if (Count < 1000)
                {
                    RouteEvent(0);
                }
            }
        }
    
        class CascadeRouteUpdateListener
        {
            public Func<int, SupportBean> RouteEvent { get; set; }

            public int CountReceived { get; private set; }

            public int CountRouted { get; private set; }

            public CascadeRouteUpdateListener()
            {
                CountReceived = 0;
                CountRouted = 0;
            }

            public void Update(object sender, UpdateEventArgs updateEventArgs)
            {
                CountReceived++;
                SupportBean theEvent = (SupportBean)(updateEventArgs.NewEvents[0].Get("tag"));
                int numNewEvents = theEvent.IntPrimitive;
    
                for (int i = 0; i < numNewEvents; i++)
                {
                    if (numNewEvents < 4)
                    {
                        RouteEvent(numNewEvents + 1);
                        CountRouted++;
                    }
                }
            }
        };
    }
}
