///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;

    public class ExecEPLSelectExprEventBeanAnnotation : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionEventBeanAnnotation(epService, rep);
            }
            RunAssertionSubquery(epService);
        }
    
        private void RunAssertionEventBeanAnnotation(EPServiceProvider epService, EventRepresentationChoice rep) {
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema MyEvent(col1 string)");
            var listenerInsert = new SupportUpdateListener();
            string eplInsert = "insert into DStream select " +
                    "last(*) @eventbean as c0, " +
                    "window(*) @eventbean as c1, " +
                    "prevwindow(s0) @eventbean as c2 " +
                    "from MyEvent#length(2) as s0";
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(eplInsert);
            stmtInsert.Events += listenerInsert.Update;
    
            foreach (string prop in "c0,c1,c2".Split(',')) {
                AssertFragment(prop, stmtInsert.EventType, "MyEvent", prop.Equals("c1") || prop.Equals("c2"));
            }
    
            // test consuming statement
            string[] fields = "f0,f1,f2,f3,f4,f5".Split(',');
            var listenerProps = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select " +
                    "c0 as f0, " +
                    "c0.col1 as f1, " +
                    "c1 as f2, " +
                    "c1.lastOf().col1 as f3, " +
                    "c1 as f4, " +
                    "c1.lastOf().col1 as f5 " +
                    "from DStream").Events += listenerProps.Update;
    
            object eventOne = SendEvent(epService, rep, "E1");
            Assert.IsTrue(((Map) listenerInsert.AssertOneGetNewAndReset().Underlying).Get("c0") is EventBean);
            EPAssertionUtil.AssertProps(listenerProps.AssertOneGetNewAndReset(), fields, new[]{eventOne, "E1", new[]{eventOne}, "E1", new[]{eventOne}, "E1"});
    
            object eventTwo = SendEvent(epService, rep, "E2");
            EPAssertionUtil.AssertProps(listenerProps.AssertOneGetNewAndReset(), fields, new[]{eventTwo, "E2", new[]{eventOne, eventTwo}, "E2", new[]{eventOne, eventTwo}, "E2"});
    
            // test SODA
            SupportModelHelper.CompileCreate(epService, eplInsert);
    
            // test invalid
            try {
                epService.EPAdministrator.CreateEPL("select last(*) @xxx from MyEvent");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Failed to recognize select-expression annotation 'xxx', expected 'eventbean' in text 'last(*) @xxx' [select last(*) @xxx from MyEvent]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("DStream", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", false);
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
            // test non-named-window
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(col1 string, col2 string)");
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string eplInsert = "insert into DStream select " +
                    "(select * from MyEvent#keepall) @eventbean as c0 " +
                    "from SupportBean";
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(eplInsert);
    
            foreach (string prop in "c0".Split(',')) {
                AssertFragment(prop, stmtInsert.EventType, "MyEvent", true);
            }
    
            // test consuming statement
            string[] fields = "f0,f1".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select " +
                    "c0 as f0, " +
                    "c0.lastOf().col1 as f1 " +
                    "from DStream").Events += listener.Update;
    
            var eventOne = new object[]{"E1", null};
            epService.EPRuntime.SendEvent(eventOne, "MyEvent");
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @out = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@out, fields, new object[]{new object[]{eventOne}, "E1"});
    
            var eventTwo = new object[]{"E2", null};
            epService.EPRuntime.SendEvent(eventTwo, "MyEvent");
            epService.EPRuntime.SendEvent(new SupportBean());
            @out = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@out, fields, new object[]{new object[]{eventOne, eventTwo}, "E2"});
        }
    
        private void AssertFragment(string prop, EventType eventType, string fragmentTypeName, bool indexed) {
            EventPropertyDescriptor desc = eventType.GetPropertyDescriptor(prop);
            Assert.AreEqual(true, desc.IsFragment);
            FragmentEventType fragment = eventType.GetFragmentType(prop);
            Assert.AreEqual(fragmentTypeName, fragment.FragmentType.Name);
            Assert.AreEqual(false, fragment.IsNative);
            Assert.AreEqual(indexed, fragment.IsIndexed);
        }
    
        private object SendEvent(EPServiceProvider epService, EventRepresentationChoice rep, string value) {
            object eventOne;
            if (rep.IsMapEvent()) {
                var @event = Collections.SingletonDataMap("col1", value);
                epService.EPRuntime.SendEvent(@event, "MyEvent");
                eventOne = @event;
            } else if (rep.IsObjectArrayEvent()) {
                object[] @event = new object[]{value};
                epService.EPRuntime.SendEvent(@event, "MyEvent");
                eventOne = @event;
    
            } else if (rep.IsAvroEvent()) {
                var schema = SupportAvroUtil.GetAvroSchema(epService, "MyEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("col1", value);
                epService.EPRuntime.SendEventAvro(@event, "MyEvent");
                eventOne = @event;
            } else {
                throw new IllegalStateException();
            }
            return eventOne;
        }
    }
} // end of namespace
