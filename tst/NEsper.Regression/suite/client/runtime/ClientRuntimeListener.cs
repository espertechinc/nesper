///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeListener
    {
        public static readonly string BEAN_TYPENAME = nameof(RoutedBeanEvent);
        public static readonly string MAP_TYPENAME = $"{nameof(ClientRuntimeListener)}_MAP";
        public static readonly string OA_TYPENAME = $"{nameof(ClientRuntimeListener)}_OA";
        public static readonly string XML_TYPENAME = $"{nameof(ClientRuntimeListener)}_XML";
        public static readonly string AVRO_TYPENAME = $"{nameof(ClientRuntimeListener)}_AVRO";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(e)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> Withe(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeListenerRoute());
            return execs;
        }

        internal class ClientRuntimeListenerRoute : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    $"@name('bean') select * from {BEAN_TYPENAME};\n" +
                    $"@name('map') select * from {MAP_TYPENAME};\n" +
                    $"@name('oa') select * from {OA_TYPENAME};\n" +
                    $"@name('xml') select * from {XML_TYPENAME};\n" + 
                    $"@name('avro') select * from {AVRO_TYPENAME};\n" + 
                    "@public @buseventtype create json schema JsonEvent(Ident string);\n" +
                    "@name('json') select * from JsonEvent;\n" + 
                    "@name('trigger') select * from SupportBean;";
                env.CompileDeploy(epl)
                    .AddListener("map")
                    .AddListener("oa")
                    .AddListener("xml")
                    .AddListener("avro")
                    .AddListener("bean")
                    .AddListener("json");

                env.Statement("trigger").Events += (
                    sender,
                    updateEventArgs) => {
                    var newEvents = updateEventArgs.NewEvents;
                    var processEvent = updateEventArgs.Runtime.EventService;
                    var ident = (string)newEvents[0].Get("TheString");

                    processEvent.RouteEventBean(new RoutedBeanEvent(ident), BEAN_TYPENAME);
                    processEvent.RouteEventMap(Collections.SingletonDataMap("Ident", ident), MAP_TYPENAME);
                    processEvent.RouteEventObjectArray(new object[] { ident }, OA_TYPENAME);

                    var xml = "<Myevent Ident=\"XXXXXX\"></Myevent>\n".Replace("XXXXXX", ident);
                    processEvent.RouteEventXMLDOM(SupportXML.GetDocument(xml).DocumentElement, XML_TYPENAME);

                    var avroSchema = AvroSchemaUtil.ResolveAvroSchema(
                        env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
                    var datum = new GenericRecord(avroSchema.AsRecordSchema());
                    datum.Put("Ident", ident);
                    processEvent.RouteEventAvro(datum, AVRO_TYPENAME);

                    var jsonObject = new JObject(new JProperty("Ident", ident));
                    processEvent.RouteEventJson(jsonObject.ToString(), "JsonEvent");
                };

                env.SendEventBean(new SupportBean("xy", -1));

                foreach (var name in new[] { "map", "bean", "oa", "xml", "avro", "json" }) {
                    var listener = env.Listener(name);
                    ClassicAssert.IsTrue(listener.IsInvoked, $"failed for {name}");
                    ClassicAssert.AreEqual("xy", env.Listener(name).AssertOneGetNewAndReset().Get("Ident"));
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        public class RoutedBeanEvent
        {
            public RoutedBeanEvent(string ident)
            {
                Ident = ident;
            }

            public string Ident { get; }
        }
    }
} // end of namespace