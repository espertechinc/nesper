///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

// record
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterSimpleNoFragment : RegressionExecution
    {
        public const string XMLTYPENAME = nameof(EventInfraGetterSimpleNoFragment) + "XML";

        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string> bean = property => { env.SendEventBean(new LocalEvent(property)); };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, "LocalEvent", beanepl, bean);

            // Map
            Consumer<string> map = property => {
                env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalEvent(Property string);\n";
            RunAssertion(env, "LocalEvent", mapepl, map);

            // Object-array
            Consumer<string> oa = property => { env.SendEventObjectArray(new object[] { property }, "LocalEvent"); };
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent(Property string);\n";
            RunAssertion(env, "LocalEvent", oaepl, oa);

            // Json
            Consumer<string> json = property => {
                env.SendEventJson(new JObject(new JProperty("Property", property)).ToString(), "LocalEvent");
            };
            RunAssertion(
                env,
                "LocalEvent",
                "@public @buseventtype create json schema LocalEvent(Property string);\n",
                json);

            // Json-Class-Provided
            RunAssertion(
                env,
                "LocalEvent",
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<string> avro = property => {
                Schema schema;
                if (property == null) {
                    schema = SchemaBuilder.Record("name", TypeBuilder.OptionalString("Property"));
                }
                else {
                    schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                }

                var theEvent = new GenericRecord(schema.AsRecordSchema());
                theEvent.Put("Property", property);
                env.SendEventAvro(theEvent, "LocalEvent");
            };
            RunAssertion(
                env,
                "LocalEvent",
                "@name('schema') @public @buseventtype create avro schema LocalEvent(Property string);\n",
                avro);

            // XML
            Consumer<string> xml = property => {
                var doc = "<" + XMLTYPENAME + (property != null ? " Property=\"" + property + "\"" : "") + "/>";
                SupportXML.SendXMLEvent(env, doc, XMLTYPENAME);
            };
            RunAssertion(env, XMLTYPENAME, "", xml);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string typeName,
            string createSchemaEPL,
            Consumer<string> sender)
        {
            var epl = createSchemaEPL +
                      "@name('s0') select * from " +
                      typeName +
                      ";\n" +
                      "@name('s1') select Property as c0, exists(Property) as c1, typeof(Property) as c2 from " +
                      typeName +
                      ";\n";
            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

            sender.Invoke("a");
            env.AssertEventNew("s0", @event => AssertGetter(@event, "a"));
            AssertProps(env, "a");

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetter(@event, null));
            AssertProps(env, null);

            env.UndeployAll();
        }

        private void AssertProps(
            RegressionEnvironment env,
            string expected)
        {
            env.AssertPropsNew(
                "s1",
                "c0,c1,c2".SplitCsv(),
                new object[] { expected, true, expected == null ? null : nameof(String) });
        }

        private void AssertGetter(
            EventBean @event,
            string value)
        {
            var getter = @event.EventType.GetGetter("Property");
            ClassicAssert.IsTrue(getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        public class LocalEvent
        {
            public LocalEvent(string property)
            {
                this.Property = property;
            }

            public string Property { get; }
        }

        public class MyLocalJsonProvided
        {
            public string Property;
        }
    }
} // end of namespace