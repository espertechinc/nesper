///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraEventRenderer : RegressionExecution
    {
        public const string XML_TYPENAME = "EventInfraEventRendererXML";
        public const string MAP_TYPENAME = "EventInfraEventRendererMap";
        public const string OA_TYPENAME = "EventInfraEventRendererOA";
        public const string AVRO_TYPENAME = "EventInfraEventRendererAvro";
        private static readonly Type BEAN_TYPE = typeof(MyEvent);

        public void Run(RegressionEnvironment env)
        {
            // Bean
            RunAssertion(env, BEAN_TYPE.Name, FBEAN, new MyEvent(1, "abc", new MyInsideEvent(10)));

            // Map
            IDictionary<string, object> mapInner = new Dictionary<string, object>();
            mapInner.Put("myInsideInt", 10);
            IDictionary<string, object> topInner = new Dictionary<string, object>();
            topInner.Put("myInt", 1);
            topInner.Put("myString", "abc");
            topInner.Put("nested", mapInner);
            RunAssertion(env, MAP_TYPENAME, FMAP, topInner);

            // Object-array
            object[] oaInner = {10};
            object[] oaTop = {1, "abc", oaInner};
            RunAssertion(env, OA_TYPENAME, FOA, oaTop);

            // XML
            var xml = "<myevent myInt=\"1\" myString=\"abc\"><nested myInsideInt=\"10\"/></myevent>";
            RunAssertion(env, XML_TYPENAME, FXML, xml);

            // Avro
            var schema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var innerSchema = schema.GetField("nested").Schema.AsRecordSchema();
            var avroInner = new GenericRecord(innerSchema);
            avroInner.Put("myInsideInt", 10);
            var avro = new GenericRecord(schema);
            avro.Put("myInt", 1);
            avro.Put("myString", "abc");
            avro.Put("nested", avroInner);
            RunAssertion(env, AVRO_TYPENAME, FAVRO, avro);
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            object @event)
        {
            var epl = "@Name('s0') select * from " + typename;
            env.CompileDeploy(epl).AddListener("s0");
            send.Invoke(env, @event, typename);

            var eventBean = env.Listener("s0").AssertOneGetNewAndReset();

            var jsonEventRenderer = env.Runtime.RenderEventService.GetJSONRenderer(env.Statement("s0").EventType);
            var json = jsonEventRenderer.Render(eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            Assert.AreEqual("{\"myInt\":1,\"myString\":\"abc\",\"nested\":{\"myInsideInt\":10}}", json);

            var xmlEventRenderer = env.Runtime.RenderEventService.GetXMLRenderer(env.Statement("s0").EventType);
            var xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            Assert.AreEqual(
                "<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><myInt>1</myInt><myString>abc</myString><nested><myInsideInt>10</myInsideInt></nested></root>",
                xml);

            env.UndeployAll();
        }

        public class MyInsideEvent
        {
            public MyInsideEvent(int myInsideInt)
            {
                MyInsideInt = myInsideInt;
            }

            public int MyInsideInt { get; }
        }

        public class MyEvent
        {
            public MyEvent(
                int myInt,
                string myString,
                MyInsideEvent nested)
            {
                MyInt = myInt;
                MyString = myString;
                Nested = nested;
            }

            public int MyInt { get; }

            public string MyString { get; }

            public MyInsideEvent Nested { get; }
        }
    }
} // end of namespace