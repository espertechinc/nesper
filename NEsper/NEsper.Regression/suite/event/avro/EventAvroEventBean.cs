///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroEventBean : RegressionExecution
    {
        public static readonly RecordSchema INNER_SCHEMA = SchemaBuilder.Record(
            "InnerSchema", Field("mymap", Map(StringType())));
        public static readonly RecordSchema RECORD_SCHEMA = SchemaBuilder.Record(
            "OuterSchema", Field("i", INNER_SCHEMA));

        public void Run(RegressionEnvironment env)
        {
            RunAssertionDynamicProp(env);
            RunAssertionNestedMap(env);
        }

        private void RunAssertionNestedMap(RegressionEnvironment env)
        {
            env.CompileDeploy("@name('s0') select i.mymap('x') as c0 from MyNestedMap");
            env.AddListener("s0");

            var inner = new GenericRecord(INNER_SCHEMA);
            inner.Put("mymap", Collections.SingletonMap("x", "y"));
            var record = new GenericRecord(RECORD_SCHEMA);
            record.Put("i", inner);
            env.SendEventAvro(record, "MyNestedMap");
            Assert.AreEqual("y", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

            env.UndeployAll();
        }

        private void RunAssertionDynamicProp(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType("create avro schema MyEvent()", path);

            env.CompileDeploy("@name('s0') select * from MyEvent", path).AddListener("s0");

            var schema = ((AvroEventType) env.Statement("s0").EventType).SchemaAvro;
            env.SendEventAvro(new GenericRecord(schema.AsRecordSchema()), "MyEvent");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.AreEqual(null, @event.Get("a?.b"));

            var innerSchema = SchemaBuilder.Record(
                "InnerSchema", Field("b", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            var inner = new GenericRecord(innerSchema);
            inner.Put("b", "X");
            var recordSchema = SchemaBuilder.Record(
                "RecordSchema", Field("a", innerSchema));
            var record = new GenericRecord(recordSchema);
            record.Put("a", inner);
            env.SendEventAvro(record, "MyEvent");
            @event = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("X", @event.Get("a?.b"));

            env.UndeployAll();
        }
    }
} // end of namespace