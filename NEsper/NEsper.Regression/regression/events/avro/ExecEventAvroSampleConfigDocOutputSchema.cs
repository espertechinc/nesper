///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.events.avro;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using Newtonsoft.Json;
using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.IO;
using NUnit.Framework;

namespace com.espertech.esper.regression.events.avro
{
    public class ExecEventAvroSampleConfigDocOutputSchema : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            // schema from statement
            var epl = EventRepresentationChoice.AVRO.GetAnnotationText() + "select 1 as carId, 'abc' as carType from System.Object";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var schema = (Schema) ((AvroSchemaEventType) stmt.EventType).Schema;
            var schemaAsString = SchemaToJsonEncoder.Encode(schema).ToString(Formatting.None);
            Assert.AreEqual("{\"type\":\"record\",\"name\":\"anonymous_1_result_\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}", schemaAsString);
            stmt.Dispose();
    
            // schema to-string Avro
            var schemaTwo = SchemaBuilder.Record("MyAvroEvent",
                    TypeBuilder.RequiredInt("carId"),
                    TypeBuilder.Field("carType",
                        TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))));
            var schemaTwoAsString = SchemaToJsonEncoder.Encode(schemaTwo).ToString(Formatting.None);

            Assert.AreEqual("{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}", schemaTwoAsString);
    
            // Define CarLocUpdateEvent event type (example for runtime-configuration interface)
            var schemaThree = SchemaBuilder.Record("CarLocUpdateEvent",
                    TypeBuilder.Field("carId", 
                        TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))),
                    TypeBuilder.RequiredInt("direction"));
            var avroEvent = new ConfigurationEventTypeAvro(schemaThree);
            epService.EPAdministrator.Configuration.AddEventTypeAvro("CarLocUpdateEvent", avroEvent);
    
            stmt = epService.EPAdministrator.CreateEPL("select count(*) from CarLocUpdateEvent(direction = 1)#time(1 min)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var @event = new GenericRecord(schemaThree);
            @event.Put("carId", "A123456");
            @event.Put("direction", 1);
            epService.EPRuntime.SendEventAvro(@event, "CarLocUpdateEvent");
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("count(*)"));
        }
    }
} // end of namespace
