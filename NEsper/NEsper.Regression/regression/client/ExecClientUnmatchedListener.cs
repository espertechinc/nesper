///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientUnmatchedListener : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionUnmatchedSendEvent(epService);
            RunAssertionUnmatchedCreateStatement(epService);
            RunAssertionUnmatchedInsertInto(epService);
        }
    
        private void RunAssertionUnmatchedSendEvent(EPServiceProvider epService) {
            var listener = new MyUnmatchedListener();
            epService.EPRuntime.UnmatchedEvent += listener.Update;
    
            // no statement, should be unmatched
            SupportBean theEvent = SendEvent(epService, "E1");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();
    
            // no unmatched listener
            epService.EPRuntime.RemoveAllUnmatchedEventHandlers();
            SendEvent(epService, "E1");
            Assert.AreEqual(0, listener.Received.Count);
    
            // create statement and re-register unmatched listener
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName);
            epService.EPRuntime.UnmatchedEvent += listener.Update;
            SendEvent(epService, "E1");
            Assert.AreEqual(0, listener.Received.Count);
    
            // stop statement
            stmt.Stop();
            theEvent = SendEvent(epService, "E1");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();
    
            // start statement
            stmt.Start();
            SendEvent(epService, "E1");
            Assert.AreEqual(0, listener.Received.Count);
    
            // destroy statement
            stmt.Dispose();
            theEvent = SendEvent(epService, "E1");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);

            epService.EPRuntime.RemoveAllUnmatchedEventHandlers();
        }

        private void RunAssertionUnmatchedCreateStatement(EPServiceProvider epService) {
            var listener = new UnmatchListenerCreateStmt(epService);
            epService.EPRuntime.UnmatchedEvent += listener.Update;
    
            // no statement, should be unmatched
            SendEvent(epService, "E1");
            Assert.AreEqual(1, listener.Received.Count);
            listener.Reset();
    
            SendEvent(epService, "E1");
            Assert.AreEqual(0, listener.Received.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPRuntime.RemoveAllUnmatchedEventHandlers();
        }
    
        private void RunAssertionUnmatchedInsertInto(EPServiceProvider epService) {
            var listener = new MyUnmatchedListener();
            epService.EPRuntime.UnmatchedEvent += listener.Update;
    
            // create insert into
            EPStatement insertInto = epService.EPAdministrator.CreateEPL("insert into MyEvent select TheString from SupportBean");
    
            // no statement, should be unmatched
            SendEvent(epService, "E1");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreEqual("E1", listener.Received[0].Get("TheString"));
            listener.Reset();
    
            // stop insert into, now SupportBean itself is unmatched
            insertInto.Stop();
            SupportBean theEvent = SendEvent(epService, "E2");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreSame(theEvent, listener.Received[0].Underlying);
            listener.Reset();
    
            // start insert-into
            SendEvent(epService, "E3");
            Assert.AreEqual(1, listener.Received.Count);
            Assert.AreEqual("E3", listener.Received[0].Get("TheString"));
            listener.Reset();

            epService.EPRuntime.RemoveAllUnmatchedEventHandlers();
        }

        private SupportBean SendEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        public class MyUnmatchedListener
        {
            private List<EventBean> received;

            public List<EventBean> Received => received;

            public MyUnmatchedListener() {
                this.received = new List<EventBean>();
            }
    
            public void Update(object sender, UnmatchedEventArgs args) {
                received.Add(args.Event);
            }
    
            public void Reset() {
                received.Clear();
            }
        }
    
        public class UnmatchListenerCreateStmt
        {
            private List<EventBean> received;
            private readonly EPServiceProvider engine;

            public List<EventBean> Received => received;

            public EPServiceProvider Engine => engine;

            public UnmatchListenerCreateStmt(EPServiceProvider engine) {
                this.engine = engine;
                this.received = new List<EventBean>();
            }

            public void Update(object sender, UnmatchedEventArgs args) {
                received.Add(args.Event);
                engine.EPAdministrator.CreateEPL("select * from SupportBean");
            }
    
            public void Reset() {
                received.Clear();
            }
        }
    }
} // end of namespace
