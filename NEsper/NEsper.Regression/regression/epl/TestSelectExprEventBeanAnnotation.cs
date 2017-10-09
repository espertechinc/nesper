///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

    public class TestSelectExprEventBeanAnnotation
    {
        private EPServiceProvider _epService;
        
        [SetUp]
        public void SetUp() {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);
            }
        }
    
        protected void TearDown() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            }
        }
    
        [Test]
        public void TestEventBeanAnnotation() {
            EnumHelper.ForEach<EventRepresentationChoice>(RunAssertionEventBeanAnnotation);
        }
    
        private void RunAssertionEventBeanAnnotation(EventRepresentationChoice rep) {
            _epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema MyEvent(col1 string)");
            var listenerInsert = new SupportUpdateListener();
            var eplInsert = "insert into DStream select " +
                               "last(*) @eventbean as c0, " +
                               "window(*) @eventbean as c1, " +
                               "prevwindow(s0) @eventbean as c2 " +
                               "from MyEvent#length(2) as s0";
            var stmtInsert = _epService.EPAdministrator.CreateEPL(eplInsert);
            stmtInsert.AddListener(listenerInsert);
    
            foreach (var prop in "c0,c1,c2".Split(',')) {
                AssertFragment(prop, stmtInsert.EventType, "MyEvent", prop.Equals("c1") || prop.Equals("c2"));
            }
    
            // test consuming statement
            var fields = "f0,f1,f2,f3,f4,f5".Split(',');
            var listenerProps = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select " +
                    "c0 as f0, " +
                    "c0.col1 as f1, " +
                    "c1 as f2, " +
                    "c1.lastOf().col1 as f3, " +
                    "c1 as f4, " +
                    "c1.lastOf().col1 as f5 " +
                    "from DStream").AddListener(listenerProps);
    
            var eventOne = SendEvent(rep, "E1");
            Assert.IsTrue(((Map)listenerInsert.AssertOneGetNewAndReset().Underlying).Get("c0") is EventBean);
            EPAssertionUtil.AssertProps(listenerProps.AssertOneGetNewAndReset(), fields, new Object[] {eventOne, "E1", new Object[] {eventOne}, "E1", new Object[] {eventOne}, "E1"});
    
            var eventTwo = SendEvent(rep, "E2");
            EPAssertionUtil.AssertProps(listenerProps.AssertOneGetNewAndReset(), fields, new Object[] {eventTwo, "E2", new Object[]{eventOne, eventTwo}, "E2", new Object[]{eventOne, eventTwo}, "E2"});
    
            // test SODA
            SupportModelHelper.CompileCreate(_epService, eplInsert);
    
            // test invalid
            try {
                _epService.EPAdministrator.CreateEPL("select last(*) @xxx from MyEvent");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Failed to recognize select-expression annotation 'xxx', expected 'eventbean' in text 'last(*) @xxx' [select last(*) @xxx from MyEvent]", ex.Message);
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("DStream", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", false);
        }
    
        [Test]
        public void TestSubquery() {
            // test non-named-window
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(col1 string, col2 string)");
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            var eplInsert = "insert into DStream select " +
                               "(select * from MyEvent#keepall) @eventbean as c0 " +
                               "from SupportBean";
            var stmtInsert = _epService.EPAdministrator.CreateEPL(eplInsert);
    
            foreach (var prop in "c0".Split(',')) {
                AssertFragment(prop, stmtInsert.EventType, "MyEvent", true);
            }
    
            // test consuming statement
            var fields = "f0,f1".Split(',');
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select " +
                    "c0 as f0, " +
                    "c0.lastOf().col1 as f1 " +
                    "from DStream").AddListener(listener);
    
            var eventOne = new Object[] {"E1", null};
            _epService.EPRuntime.SendEvent(eventOne, "MyEvent");
            _epService.EPRuntime.SendEvent(new SupportBean());
            var @out = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@out, fields, new Object[] {new Object[] {eventOne}, "E1"});
    
            var eventTwo = new Object[] {"E2", null};
            _epService.EPRuntime.SendEvent(eventTwo, "MyEvent");
            _epService.EPRuntime.SendEvent(new SupportBean());
            @out = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@out, fields, new Object[] {new Object[]{eventOne, eventTwo}, "E2"});
        }
    
        private void AssertFragment(string prop, EventType eventType, string fragmentTypeName, bool indexed) {
            var desc = eventType.GetPropertyDescriptor(prop);
            Assert.AreEqual(true, desc.IsFragment);
            var fragment = eventType.GetFragmentType(prop);
            Assert.AreEqual(fragmentTypeName, fragment.FragmentType.Name);
            Assert.AreEqual(false, fragment.IsNative);
            Assert.AreEqual(indexed, fragment.IsIndexed);
        }
    
        private Object SendEvent(EventRepresentationChoice rep, string value) {
            Object eventOne;
            if (rep.IsMapEvent()) {
                var @event = Collections.SingletonDataMap("col1", value);
                _epService.EPRuntime.SendEvent(@event, "MyEvent");
                eventOne = @event;
            } else if (rep.IsObjectArrayEvent()) {
                var @event = new Object[] {value};
                _epService.EPRuntime.SendEvent(@event, "MyEvent");
                eventOne = @event;
    
            } else if (rep.IsAvroEvent()) {
                var schema = SupportAvroUtil.GetAvroSchema(_epService, "MyEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("col1", value);
                _epService.EPRuntime.SendEventAvro(@event, "MyEvent");
                eventOne = @event;
            } else {
                throw new IllegalStateException();
            }
            return eventOne;
        }
    }
} // end of namespace
