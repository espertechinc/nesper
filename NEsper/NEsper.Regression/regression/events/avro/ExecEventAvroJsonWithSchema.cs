///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NEsper.Avro.IO;

namespace com.espertech.esper.regression.events.avro
{
    public class ExecEventAvroJsonWithSchema : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            var schemaText =
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
            var schema = Schema.Parse(schemaText);
            epService.EPAdministrator.Configuration.AddEventTypeAvro("User", new ConfigurationEventTypeAvro(schema));
    
            var fields = "name,favorite_number,favorite_color";
            var stmt = epService.EPAdministrator.CreateEPL("select " + fields + " from User");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var eventOneJson = "{\"name\": \"Jane\", \"favorite_number\": 256, \"favorite_color\": \"red\"}";
            var record = Parse(schema, eventOneJson);
            epService.EPRuntime.SendEventAvro(record, "User");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields.Split(','), new object[]{"Jane", 256, "red"});
    
            var eventTwoJson = "{\"name\": \"Hans\", \"favorite_number\": -1, \"favorite_color\": \"green\"}";
            record = Parse(schema, eventTwoJson);
            epService.EPRuntime.SendEventAvro(record, "User");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields.Split(','), new object[]{"Hans", -1, "green"});
        }
    
        private static GenericRecord Parse(Schema schema, string json) {
            var jsonEntity = (JToken) JsonConvert.DeserializeObject(json);
            var avroEntity = JsonDecoder.DecodeAny(schema, jsonEntity);
            if (avroEntity is GenericRecord)
            {
                return (GenericRecord)avroEntity;
            }

            throw new ArgumentException("schema was not a record");
        }
    }
} // end of namespace
