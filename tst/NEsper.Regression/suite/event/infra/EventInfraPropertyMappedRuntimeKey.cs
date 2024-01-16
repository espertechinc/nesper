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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyMappedRuntimeKey : RegressionExecution
    {
        private readonly EventRepresentationChoice _eventRepresentationChoice;
        
        /// <summary>
        /// Constructor for test
        /// </summary>
        /// <param name="eventRepresentationChoice"></param>
        public EventInfraPropertyMappedRuntimeKey(EventRepresentationChoice eventRepresentationChoice)
        {
            _eventRepresentationChoice = eventRepresentationChoice;
        }
        
        public void Run(RegressionEnvironment env)
        {
            var mapType = typeof(IDictionary<string, object>).CleanName();

            // Local function to send a json object given a set of entries
            void SendEventJson(IDictionary<string, string> entries)
            {
                var mapValues = new JObject();
                foreach (var entry in entries)
                {
                    mapValues.Add(entry.Key, entry.Value);
                }

                var @event = new JObject(new JProperty("Mapped", mapValues));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            }

            switch (_eventRepresentationChoice)
            {
                case EventRepresentationChoice.OBJECTARRAY:
                    // Object-array
                    Consumer<IDictionary<string, string>> oa = entries => {
                        env.SendEventObjectArray(new object[] { entries }, "LocalEvent");
                    };
                    var oaepl = $"@public @buseventtype create objectarray schema LocalEvent(Mapped `{mapType}`);\n";
                    RunAssertion(env, oaepl, oa);
                    break;

                case EventRepresentationChoice.MAP:
                    // Map
                    Consumer<IDictionary<string, string>> map = entries => {
                        env.SendEventMap(Collections.SingletonDataMap("Mapped", entries), "LocalEvent");
                    };
                    var mapepl = $"@public @buseventtype create schema LocalEvent(Mapped `{mapType}`);\n";
                    RunAssertion(env, mapepl, map);
                    break;
                
                case EventRepresentationChoice.AVRO:
                    // Avro
                    Consumer<IDictionary<string, string>> avro = entries => {
                        var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                        var @event = new GenericRecord(schema.AsRecordSchema());
                        @event.Put("Mapped", entries);
                        env.SendEventAvro(@event, "LocalEvent");
                    };
                    var avroepl = $"@name('schema') @public @buseventtype create avro schema LocalEvent(Mapped `{mapType}`);\n";
                    RunAssertion(env, avroepl, avro);
                    break;
                
                case EventRepresentationChoice.JSON:
                    // Json
                    var jsonepl = $"@public @buseventtype create json schema LocalEvent(Mapped `{mapType}`);\n";
                    RunAssertion(env, jsonepl, SendEventJson);

                    break;
                case EventRepresentationChoice.JSONCLASSPROVIDED:
                    // Json-Class-Provided
                    var jsonProvidedType = typeof(MyLocalJsonProvided).MaskTypeName();
                    var jsonProvidedEpl = $"@JsonSchema(ClassName='{jsonProvidedType}') @public @buseventtype create json schema LocalEvent();\n";
                    RunAssertion(env, jsonProvidedEpl, SendEventJson);
                    
                    break;
                case EventRepresentationChoice.DEFAULT:
                    // Bean
                    Consumer<IDictionary<string, string>> bean = entries => { env.SendEventBean(new LocalEvent(entries)); };
                    var beanepl = $"@Public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n";
                    RunAssertion(env, beanepl, bean);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<IDictionary<string, string>> sender)
        {
            env.CompileDeploy(
                    createSchemaEPL +
                    "create constant variable string keyChar = 'a';" +
                    "@name('s0') select Mapped(keyChar||'1') as c0, Mapped(keyChar||'2') as c1 from LocalEvent as e;\n"
                )
                .AddListener("s0");

            IDictionary<string, string> values = new Dictionary<string, string>();
            values.Put("a1", "x");
            values.Put("a2", "y");
            sender.Invoke(values);
            env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "x", "y" });

            env.UndeployAll();
        }

        public class LocalEvent
        {
            public LocalEvent(IDictionary<string, string> mapped)
            {
                this.Mapped = mapped;
            }

            public IDictionary<string, string> Mapped { get; }
        }

        public class MyLocalJsonProvided
        {
            public IDictionary<string, string> Mapped;
        }
    }
} // end of namespace