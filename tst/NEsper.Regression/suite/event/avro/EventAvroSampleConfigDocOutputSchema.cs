///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroSampleConfigDocOutputSchema : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // schema from statement
            var epl = "@name('s0') " +
                      EventRepresentationChoice.AVRO.GetAnnotationText() +
                      "select 1 as CarId, 'abc' as carType from SupportBean";
            env.CompileDeploy(epl);
            env.AssertStatement(
                "s0",
                statement => {
                    var schema = (Schema)((AvroSchemaEventType)statement.EventType).Schema;
                    var schemaJson = schema.ToJsonObject();
                    Assert.AreEqual(
                        "{\"type\":\"record\",\"name\":\"stmt0_out0\",\"fields\":[{\"name\":\"CarId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}",
                        schemaJson.ToString(Newtonsoft.Json.Formatting.None));
                });

            env.UndeployAll();

            // schema to-string Avro
            var schemaTwo = SchemaBuilder.Record(
                "MyAvroEvent",
                RequiredInt("CarId"),
                Field(
                    "carType",
                    StringType(
                        Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            Assert.AreEqual(
                "{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"CarId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}",
                schemaTwo.ToJsonObject().ToString(Newtonsoft.Json.Formatting.None));
            env.UndeployAll();

            env.CompileDeploy("@name('s0') select count(*) from CarLocUpdateEvent(Direction = 1)#time(1 min)")
                .AddListener("s0");

            var schemaCarLocUpd = env.RuntimeAvroSchemaPreconfigured("CarLocUpdateEvent")
                .AsRecordSchema();
            var @event = new GenericRecord(schemaCarLocUpd);
            @event.Put("CarId", "A123456");
            @event.Put("Direction", 1);
            env.SendEventAvro(@event, "CarLocUpdateEvent");
            env.AssertEqualsNew("s0", "count(*)", 1L);

            env.UndeployAll();
        }
    }
} // end of namespace