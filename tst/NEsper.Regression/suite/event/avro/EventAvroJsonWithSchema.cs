///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroJsonWithSchema : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = "name,favorite_number,favorite_color";
            env.CompileDeploy("@name('s0') select " + fields + " from User").AddListener("s0");

            var schema = env.RuntimeAvroSchemaPreconfigured("User");
            var eventOneJson = "{\"name\": \"Jane\", \"favorite_number\": 256, \"favorite_color\": \"red\"}";
            var record = Parse(schema, eventOneJson);
            env.SendEventAvro(record, "User");
            env.AssertPropsNew(
                "s0",
                fields.SplitCsv(),
                new object[] { "Jane", 256, "red" });

            var eventTwoJson = "{\"name\": \"Hans\", \"favorite_number\": -1, \"favorite_color\": \"green\"}";
            record = Parse(schema, eventTwoJson);
            env.SendEventAvro(record, "User");
            env.AssertPropsNew(
                "s0",
                fields.SplitCsv(),
                new object[] { "Hans", -1, "green" });

            env.UndeployAll();
        }

        private static GenericRecord Parse(
            Schema schema,
            string json)
        {
            var jsonEntity = (JToken)JsonConvert.DeserializeObject(json);
            var avroEntity = schema.DecodeAny(jsonEntity);
            if (avroEntity is GenericRecord) {
                return (GenericRecord)avroEntity;
            }

            throw new ArgumentException("schema was not a record");
        }
    }
} // end of namespace