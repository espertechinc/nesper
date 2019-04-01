///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

namespace com.espertech.esper.regression.expr.expr
{
    using Map = IDictionary<string, object>;

    public class ExecExprTypeOf : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionFragment(epService);
            RunAssertionNamedUnnamedPono(epService);
            RunAssertionVariantStream(epService);
            RunAssertionInvalid(epService);
            RunAssertionDynamicProps(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select typeof(xx) from System.Object",
                    "Error starting statement: Failed to validate select-clause expression 'typeof(xx)': Property named 'xx' is not valid in any stream [select typeof(xx) from System.Object]");
        }
    
        private void RunAssertionDynamicProps(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create schema MyDynoPropSchema as (key string)");
    
            var stmtText = "select typeof(prop?), typeof(key) from MyDynoPropSchema as s0";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionDynamicProps(epService, listener);
    
            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            TryAssertionDynamicProps(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssertionDynamicProps(EPServiceProvider epService, SupportUpdateListener listener) {
    
            var fields = new[]{"typeof(prop?)", "typeof(key)"};
    
            SendSchemaEvent(epService, 1, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ "Int32", "String" });
    
            SendSchemaEvent(epService, "test", "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ "String", "String" });
    
            SendSchemaEvent(epService, null, "E3");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ null, "String" });
        }
    
        private void SendSchemaEvent(EPServiceProvider epService, object prop, string key) {
            var theEvent = new Dictionary<string, object>();
            theEvent.Put("prop", prop);
            theEvent.Put("key", key);
    
            if (EventRepresentationChoiceExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(theEvent, "MyDynoPropSchema");
            } else {
                epService.EPRuntime.SendEvent(theEvent, "MyDynoPropSchema");
            }
        }
    
        private void RunAssertionVariantStream(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionVariantStream(epService, rep);
            }
        }
    
        private void TryAssertionVariantStream(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventOne as (key string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTwo as (key string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema S0 as " + typeof(SupportBean_S0).FullName);
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create variant schema VarSchema as *");
    
            epService.EPAdministrator.CreateEPL("insert into VarSchema select * from EventOne");
            epService.EPAdministrator.CreateEPL("insert into VarSchema select * from EventTwo");
            epService.EPAdministrator.CreateEPL("insert into VarSchema select * from S0");
            epService.EPAdministrator.CreateEPL("insert into VarSchema select * from SupportBean");
    
            var stmtText = "select typeof(A) as t0 from VarSchema as A";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"value"}, "EventOne");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("key", "value"), "EventOne");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(SchemaBuilder.Record("EventOne",
                    TypeBuilder.RequiredString("key")));
                record.Put("key", "value");
                epService.EPRuntime.SendEventAvro(record, "EventOne");
            } else {
                Assert.Fail();
            }
            Assert.AreEqual("EventOne", listener.AssertOneGetNewAndReset().Get("t0"));
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"value"}, "EventTwo");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("key", "value"), "EventTwo");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(SchemaBuilder.Record("EventTwo",
                    TypeBuilder.RequiredString("key")));
                record.Put("key", "value");
                epService.EPRuntime.SendEventAvro(record, "EventTwo");
            } else {
                Assert.Fail();
            }
            Assert.AreEqual("EventTwo", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual("S0", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual("SupportBean", listener.AssertOneGetNewAndReset().Get("t0"));
    
            stmt.Dispose();
            listener.Reset();
            stmt = epService.EPAdministrator.CreateEPL("select * from VarSchema Match_recognize(\n" +
                    "  measures A as a, B as b\n" +
                    "  pattern (A B)\n" +
                    "  define A as typeof(A) = \"EventOne\",\n" +
                    "         B as typeof(B) = \"EventTwo\"\n" +
                    "  )");
            stmt.Events += listener.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"value"}, "EventOne");
                epService.EPRuntime.SendEvent(new object[]{"value"}, "EventTwo");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("key", "value"), "EventOne");
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("key", "value"), "EventTwo");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("EventTwo",
                    TypeBuilder.RequiredString("key"));
                var eventOne = new GenericRecord(schema);
                eventOne.Put("key", "value");
                var eventTwo = new GenericRecord(schema);
                eventTwo.Put("key", "value");
                epService.EPRuntime.SendEventAvro(eventOne, "EventOne");
                epService.EPRuntime.SendEventAvro(eventTwo, "EventTwo");
            } else {
                Assert.Fail();
            }
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "EventOne,EventTwo,S0,VarSchema".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionNamedUnnamedPono(EPServiceProvider epService) {
            // test name-provided or no-name-provided
            epService.EPAdministrator.Configuration.AddEventType("ISupportA", typeof(ISupportA));
            epService.EPAdministrator.Configuration.AddEventType("ISupportABCImpl", typeof(ISupportABCImpl));
    
            var stmtText = "select typeof(A) as t0 from ISupportA as A";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new ISupportAImpl(null, null));
            Assert.AreEqual(typeof(ISupportAImpl).FullName, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new ISupportABCImpl(null, null, null, null));
            Assert.AreEqual("ISupportABCImpl", listener.AssertOneGetNewAndReset().Get("t0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionFragment(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionFragment(epService, rep);
            }
        }
    
        private void TryAssertionFragment(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema InnerSchema as (key string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MySchema as (inside InnerSchema, insidearr InnerSchema[])");
    
            var fields = new[]{"t0", "t1"};
            var stmtText = eventRepresentationEnum.GetAnnotationText() + " select typeof(s0.inside) as t0, typeof(s0.insidearr) as t1 from MySchema as s0";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[2], "MySchema");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MySchema");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                epService.EPRuntime.SendEventAvro(new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(epService, "MySchema").AsRecordSchema()), "MySchema");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{new object[2], null}, "MySchema");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("inside", new Dictionary<string, object>());
                epService.EPRuntime.SendEvent(theEvent, "MySchema");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var mySchema = SupportAvroUtil.GetAvroSchema(epService, "MySchema").AsRecordSchema();
                var innerSchema = SupportAvroUtil.GetAvroSchema(epService, "InnerSchema").AsRecordSchema();
                var @event = new GenericRecord(mySchema);
                @event.Put("inside", new GenericRecord(innerSchema));
                epService.EPRuntime.SendEventAvro(@event, "MySchema");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"InnerSchema", null});
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{null, new object[2][]}, "MySchema");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("insidearr", new Map[0]);
                epService.EPRuntime.SendEvent(theEvent, "MySchema");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(
                    epService, "MySchema").AsRecordSchema());
                @event.Put("insidearr", Collections.GetEmptyList<object>());
                epService.EPRuntime.SendEventAvro(@event, "MySchema");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, "InnerSchema[]"});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("InnerSchema", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
        }
    }
} // end of namespace
