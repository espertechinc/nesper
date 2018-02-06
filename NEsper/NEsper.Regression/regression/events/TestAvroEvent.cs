///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.events.avro;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.IO;

using System.Text;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestAvroEvent
    {
	    private EPServiceProvider _epService;
	    private readonly SupportUpdateListener _listener = new SupportUpdateListener();

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener.Reset();
	    }

        [Test]
	    public void TestSampleConfigDocOutputSchema() {

	        // schema from statement
	        string epl = EventRepresentationChoice.AVRO.GetAnnotationText() + "select 1 as carId, 'abc' as carType from " + Name.Of<object>();
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        Schema schema = (Schema) ((AvroSchemaEventType) stmt.EventType).Schema;
            String schemaAsString = SchemaToJsonEncoder.Encode(schema).ToString(Formatting.None);
	        Assert.AreEqual("{\"type\":\"record\",\"name\":\"anonymous_1_result_\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}", schemaAsString);
	        stmt.Dispose();

	        // schema to-string Avro
	        Schema schemaTwo = SchemaBuilder.Record(
                "MyAvroEvent",
                TypeBuilder.RequiredInt("carId"),
                TypeBuilder.Field((string) "carType", TypeBuilder.TypeWithProperty("string", AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)));
            String schemaTwoAsString = SchemaToJsonEncoder.Encode(schemaTwo).ToString(Formatting.None);
            Assert.AreEqual("{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}", schemaTwoAsString);

	        // Define CarLocUpdateEvent event type (example for runtime-configuration interface)
	        RecordSchema schemaThree = SchemaBuilder.Record(
                "CarLocUpdateEvent",
                TypeBuilder.Field((string) "carId", TypeBuilder.TypeWithProperty("string", AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)),
                TypeBuilder.RequiredInt("direction"));
	        ConfigurationEventTypeAvro avroEvent = new ConfigurationEventTypeAvro(schemaThree);
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("CarLocUpdateEvent", avroEvent);

	        stmt = _epService.EPAdministrator.CreateEPL("select count(*) from CarLocUpdateEvent(direction = 1)#time(1 min)");
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.AddListener(listener);
	        GenericRecord @event = new GenericRecord(schemaThree);
	        @event.Put("carId", "A123456");
	        @event.Put("direction", 1);
	        _epService.EPRuntime.SendEventAvro(@event, "CarLocUpdateEvent");
	        Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("count(*)"));
	    }

        [Test]
	    public void TestJsonWithSchema()
        {
	        string schemaText =
	            "{\"namespace\": \"example.avro\",\n" +
	            " \"type\": \"record\",\n" +
	            " \"name\": \"User\",\n" +
	            " \"fields\": [\n" +
	            "     {\"name\": \"name\",  \"type\": {\n" +
	            "                              \"type\": \"string\",\n" +
                "                              \"avro.string\": \"string\"\n" +
	            "                            }},\n" +
	            "     {\"name\": \"favorite_number\",  \"type\": \"int\"},\n" +
	            "     {\"name\": \"favorite_color\",  \"type\": {\n" +
	            "                              \"type\": \"string\",\n" +
                "                              \"avro.string\": \"string\"\n" +
	            "                            }}\n" +
	            " ]\n" +
	            "}";

            Schema schema = Schema.Parse(schemaText);
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("User", new ConfigurationEventTypeAvro(schema));

	        string fields = "name,favorite_number,favorite_color";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("select " + fields + " from User");
	        stmt.AddListener(_listener);

	        string eventOneJson = "{\"name\": \"Jane\", \"favorite_number\": 256, \"favorite_color\": \"red\"}";
	        GenericRecord record = Parse(schema, eventOneJson);
	        _epService.EPRuntime.SendEventAvro(record, "User");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.SplitCsv(), new object[] {"Jane", 256, "red"});

	        string eventTwoJson = "{\"name\": \"Hans\", \"favorite_number\": -1, \"favorite_color\": \"green\"}";
	        record = Parse(schema, eventTwoJson);
	        _epService.EPRuntime.SendEventAvro(record, "User");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.SplitCsv(), new object[] {"Hans", -1, "green"});
	    }

	    private static GenericRecord Parse(Schema schema, string json)
        {
            var jsonEntity = (JToken) JsonConvert.DeserializeObject(json);
            var avroEntity = JsonDecoder.DecodeAny(schema, jsonEntity);
            if (avroEntity is GenericRecord)
            {
                return (GenericRecord) avroEntity;
            }

            throw new ArgumentException("schema was not a record");
	    }
	}
} // end of namespace
