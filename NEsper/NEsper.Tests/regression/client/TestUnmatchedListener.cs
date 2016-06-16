///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestUnmatchedListener
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

        [Test]
        public void TestUnmatchedSendEvent()
        {
            MyUnmatchedListener listener = new MyUnmatchedListener();
            _epService.EPRuntime.UnmatchedEvent += listener.Update;

            // no statement, should be unmatched
            SupportBean theEvent = SendEvent();
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();

            // no unmatched listener
            _epService.EPRuntime.RemoveAllUnmatchedEventHandlers();
            SendEvent();
            Assert.AreEqual(0, listener.Received.Count);

            // create statement and re-register unmatched listener
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName);
            _epService.EPRuntime.UnmatchedEvent += listener.Update;
            SendEvent();
            Assert.AreEqual(0, listener.Received.Count);

            // stop statement
            stmt.Stop();
            theEvent = SendEvent();
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();

            // start statement
            stmt.Start();
            SendEvent();
            Assert.AreEqual(0, listener.Received.Count);

            // destroy statement
            stmt.Dispose();
            theEvent = SendEvent();
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();
        }

        [Test]
        public void TestUnmatchedCreateStatement()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            UnmatchListenerCreateStmt listener = new UnmatchListenerCreateStmt(_epService);
            _epService.EPRuntime.UnmatchedEvent += listener.Update;

            // no statement, should be unmatched
            SendEvent("E1");
            Assert.AreEqual(1, listener.Received.Count);
            listener.Reset();

            SendEvent("E1");
            Assert.AreEqual(0, listener.Received.Count);
        }

        [Test]
        public void TestUnmatchedInsertInto()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            MyUnmatchedListener listener = new MyUnmatchedListener();
            _epService.EPRuntime.UnmatchedEvent += listener.Update;

            // create insert into
            EPStatement insertInto = _epService.EPAdministrator.CreateEPL("insert into MyEvent select TheString from SupportBean");

            // no statement, should be unmatched
            SendEvent("E1");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreEqual("E1", listener.Received[0].Get("TheString"));
            listener.Reset();

            // stop insert into, now SupportBean itself is unmatched
            insertInto.Stop();
            SupportBean theEvent = SendEvent("E2");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();

            // start insert-into
            SendEvent("E3");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreEqual("E3", listener.Received[0].Get("TheString"));
            listener.Reset();
        }

        private SupportBean SendEvent()
        {
            var bean = new SupportBean();
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private SupportBean SendEvent(String stringValue)
        {
            var bean = new SupportBean { TheString = stringValue };
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        public class MyUnmatchedListener
        {
            public MyUnmatchedListener()
            {
                Received = new List<EventBean>();
            }

            public void Update(Object sender, UnmatchedEventArgs unmatchedEventArgs)
            {
                Received.Add(unmatchedEventArgs.Event);
            }

            public List<EventBean> Received { get; private set; }

            public void Reset()
            {
                Received.Clear();
            }
        }

        public class UnmatchListenerCreateStmt
        {
            private readonly EPServiceProvider _engine;

            public UnmatchListenerCreateStmt(EPServiceProvider engine)
            {
                _engine = engine;
                Received = new List<EventBean>();
            }

            public void Update(Object sender, UnmatchedEventArgs unmatchedEventArgs)
            {
                Received.Add(unmatchedEventArgs.Event);
                _engine.EPAdministrator.CreateEPL("select * from SupportBean");
            }

            public List<EventBean> Received { get; private set; }

            public void Reset()
            {
                Received.Clear();
            }
        }
    }
}
