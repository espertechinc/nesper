///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Avro;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static NEsper.Avro.Core.AvroConstant;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.avro
{
    public class ExecAvroEventBean : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionDynamicProp(epService);
            RunNestedMap(epService);
        }
    
        private void RunNestedMap(EPServiceProvider epService)
        {
            var innerSchema = SchemaBuilder.Record(
                "InnerSchema", TypeBuilder.Field("mymap", TypeBuilder.Map(TypeBuilder.StringType())));
            var recordSchema = SchemaBuilder.Record(
                "OuterSchema", TypeBuilder.Field("i", innerSchema));
            var avro = new ConfigurationEventTypeAvro(recordSchema);
            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyNestedMap", avro);
    
            var stmt = epService.EPAdministrator.CreateEPL("select i.mymap('x') as c0 from MyNestedMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var inner = new GenericRecord(innerSchema);
            inner.Put("mymap", Collections.SingletonMap("x", "y"));
            var record = new GenericRecord(recordSchema);
            record.Put("i", inner);
            epService.EPRuntime.SendEventAvro(record, "MyNestedMap");
            Assert.AreEqual("y", listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        private void RunAssertionDynamicProp(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create avro schema MyEvent()");
            var stmt = epService.EPAdministrator.CreateEPL("select * from MyEvent");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var schema = ((AvroEventType) stmt.EventType).SchemaAvro;
            epService.EPRuntime.SendEventAvro(new GenericRecord(schema), "MyEvent");
            var @event = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual(null, @event.Get("a?.b"));
    
            var innerSchema = SchemaBuilder.Record("InnerSchema",
                    TypeBuilder.Field("b", TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            var inner = new GenericRecord(innerSchema);
            inner.Put("b", "X");
            var recordSchema = SchemaBuilder.Record("RecordSchema",
                TypeBuilder.Field("a", innerSchema));
            var record = new GenericRecord(recordSchema);
            record.Put("a", inner);
            epService.EPRuntime.SendEventAvro(record, "MyEvent");
            @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("X", @event.Get("a?.b"));
        }
    }
} // end of namespace
