///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.@event.avro;
using com.espertech.esper.regressionrun.runner;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventAvro
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {typeof(SupportBean)}) {
                configuration.Common.AddEventType(clazz);
            }

            var schemaUser =
                "{\"namespace\": \"example.avro\",\n" +
                " \"type\": \"record\",\n" +
                " \"name\": \"User\",\n" +
                " \"fields\": [\n" +
                "     {\"name\": \"name\",  \"type\": {\n" +
                "                              \"type\": \"string\",\n" +
                "                              \"avro.string\": \"String\"\n" +
                "                            }},\n" +
                "     {\"name\": \"favorite_number\",  \"type\": \"int\"},\n" +
                "     {\"name\": \"favorite_color\",  \"type\": {\n" +
                "                              \"type\": \"string\",\n" +
                "                              \"avro.string\": \"String\"\n" +
                "                            }}\n" +
                " ]\n" +
                "}";
            var schema = Schema.Parse(schemaUser);
            configuration.Common.AddEventTypeAvro("User", new ConfigurationCommonEventTypeAvro(schema));

            var schemaCarLocUpdateEvent = SchemaBuilder.Record(
                "CarLocUpdateEvent",
                TypeBuilder.Field(
                    "carId",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                TypeBuilder.RequiredInt("direction"));
            var avroEvent = new ConfigurationCommonEventTypeAvro(schemaCarLocUpdateEvent);
            configuration.Common.AddEventTypeAvro("CarLocUpdateEvent", avroEvent);

            var avro = new ConfigurationCommonEventTypeAvro(EventAvroEventBean.RECORD_SCHEMA);
            configuration.Common.AddEventTypeAvro("MyNestedMap", avro);
        }

        [Test, RunInApplicationDomain]
        public void TestEventAvroEventBean()
        {
            RegressionRunner.Run(session, new EventAvroEventBean());
        }

        [Test, RunInApplicationDomain]
        public void TestEventAvroJsonWithSchema()
        {
            RegressionRunner.Run(session, new EventAvroJsonWithSchema());
        }

        [Test, RunInApplicationDomain]
        public void TestEventAvroSampleConfigDocOutputSchema()
        {
            RegressionRunner.Run(session, new EventAvroSampleConfigDocOutputSchema());
        }
        
        [Test, RunInApplicationDomain]
        public void TestEventAvroSupertypeInsertInto()
        {
            RegressionRunner.Run(session, new EventAvroSupertypeInsertInto());
        }
    }
} // end of namespace