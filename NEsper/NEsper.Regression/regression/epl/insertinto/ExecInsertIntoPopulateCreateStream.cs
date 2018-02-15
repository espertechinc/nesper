///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Avro;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoPopulateCreateStream : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionCreateStream(epService, rep);
            }
    
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionCreateStreamTwo(epService, rep);
            }
    
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertPopulateFromNamedWindow(epService, rep);
            }
    
            RunAssertionObjectArrPropertyReorder(epService);
        }
    
        private void RunAssertionObjectArrPropertyReorder(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyInner (p_inner string)");
            epService.EPAdministrator.CreateEPL("create objectarray schema MyOATarget (unfilled string, p0 string, p1 string, i0 MyInner)");
            epService.EPAdministrator.CreateEPL("create objectarray schema MyOASource (p0 string, p1 string, i0 MyInner)");
            epService.EPAdministrator.CreateEPL("insert into MyOATarget select p0, p1, i0, null as unfilled from MyOASource");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyOATarget").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{"p0value", "p1value", new object[]{"i"}}, "MyOASource");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "p0,p1".Split(','), new object[]{"p0value", "p1value"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertPopulateFromNamedWindow(EPServiceProvider epService, EventRepresentationChoice type) {
            epService.EPAdministrator.CreateEPL("create " + type.GetOutputTypeCreateSchemaName() + " schema Node(nid string)");
            epService.EPAdministrator.CreateEPL("create window NodeWindow#unique(nid) as Node");
            epService.EPAdministrator.CreateEPL("insert into NodeWindow select * from Node");
            epService.EPAdministrator.CreateEPL("create " + type.GetOutputTypeCreateSchemaName() + " schema NodePlus(npid string, node Node)");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into NodePlus select 'E1' as npid, n1 as node from NodeWindow n1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            if (type.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"n1"}, "Node");
            } else if (type.IsMapEvent()) {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("nid", "n1"), "Node");
            } else if (type.IsAvroEvent()) {
                var genericRecord = new GenericRecord(SchemaBuilder.Record("name", RequiredString("nid")));
                genericRecord.Put("nid", "n1");
                epService.EPRuntime.SendEventAvro(genericRecord, "Node");
            } else {
                Assert.Fail();
            }
            EventBean @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", @event.Get("npid"));
            Assert.AreEqual("n1", @event.Get("node.nid"));
            EventBean fragment = (EventBean) @event.GetFragment("node");
            Assert.AreEqual("Node", fragment.EventType.Name);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("Node", true);
            epService.EPAdministrator.Configuration.RemoveEventType("NodePlus", true);
            epService.EPAdministrator.Configuration.RemoveEventType("NodeWindow", true);
        }
    
        private void RunAssertionCreateStream(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent(myId int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema CompositeEvent(c1 MyEvent, c2 MyEvent, rule string)");
            epService.EPAdministrator.CreateEPL("insert into MyStream select c, 'additionalValue' as value from MyEvent c");
            epService.EPAdministrator.CreateEPL("insert into CompositeEvent select e1.c as c1, e2.c as c2, '4' as rule " +
                    "from pattern [e1=MyStream -> e2=MyStream]");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " @Name('Target') select * from CompositeEvent");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("Target").Events += listener.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(MakeEvent(10).Values.ToArray(), "MyEvent");
                epService.EPRuntime.SendEvent(MakeEvent(11).Values.ToArray(), "MyEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(MakeEvent(10), "MyEvent");
                epService.EPRuntime.SendEvent(MakeEvent(11), "MyEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                epService.EPRuntime.SendEventAvro(MakeEventAvro(10), "MyEvent");
                epService.EPRuntime.SendEventAvro(MakeEventAvro(11), "MyEvent");
            } else {
                Assert.Fail();
            }
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10, theEvent.Get("c1.myId"));
            Assert.AreEqual(11, theEvent.Get("c2.myId"));
            Assert.AreEqual("4", theEvent.Get("rule"));
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "MyEvent,CompositeEvent,MyStream".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionCreateStreamTwo(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent(myId int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema AllMyEvent as (myEvent MyEvent, class string, reverse bool)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SuspectMyEvent as (myEvent MyEvent, class string)");
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("insert into AllMyEvent " +
                    "select c as myEvent, 'test' as class, false as reverse " +
                    "from MyEvent(myId=1) c");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtOne.EventType.UnderlyingType));
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("insert into SuspectMyEvent " +
                    "select c.myEvent as myEvent, class " +
                    "from AllMyEvent(not reverse) c");
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(MakeEvent(1).Values.ToArray(), "MyEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(MakeEvent(1), "MyEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                epService.EPRuntime.SendEventAvro(MakeEventAvro(1), "MyEvent");
            } else {
                Assert.Fail();
            }
    
            AssertCreateStreamTwo(eventRepresentationEnum, listener.AssertOneGetNewAndReset(), stmtOne);
            AssertCreateStreamTwo(eventRepresentationEnum, listenerTwo.AssertOneGetNewAndReset(), stmtTwo);
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "MyEvent,AllMyEvent,SuspectMyEvent".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void AssertCreateStreamTwo(EventRepresentationChoice eventRepresentationEnum, EventBean eventBean, EPStatement statement) {
            if (eventRepresentationEnum.IsAvroEvent()) {
                Assert.AreEqual(1, eventBean.Get("myEvent.myId"));
            } else {
                Assert.IsTrue(eventBean.Get("myEvent") is EventBean);
                Assert.AreEqual(1, ((EventBean) eventBean.Get("myEvent")).Get("myId"));
            }
            Assert.IsNotNull(statement.EventType.GetFragmentType("myEvent"));
        }
    
        private IDictionary<string, Object> MakeEvent(int myId) {
            return Collections.SingletonDataMap("myId", myId);
        }
    
        private GenericRecord MakeEventAvro(int myId) {
            var schema = SchemaBuilder.Record("schema", RequiredInt("myId"));
            var record = new GenericRecord(schema);
            record.Put("myId", myId);
            return record;
        }
    }
} // end of namespace
