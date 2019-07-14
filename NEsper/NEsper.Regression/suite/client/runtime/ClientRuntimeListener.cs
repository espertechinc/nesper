///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeListener
    {
        public static readonly string BEAN_TYPENAME = typeof(RoutedBeanEvent).Name;
        public static readonly string MAP_TYPENAME = typeof(ClientRuntimeListener).FullName + "_MAP";
        public static readonly string OA_TYPENAME = typeof(ClientRuntimeListener).FullName + "_OA";
        public static readonly string XML_TYPENAME = typeof(ClientRuntimeListener).FullName + "_XML";
        public static readonly string AVRO_TYPENAME = typeof(ClientRuntimeListener).FullName + "_AVRO";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientRuntimeListenerRoute());
            return execs;
        }

        internal class ClientRuntimeListenerRoute : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('bean') select * from " +
                    BEAN_TYPENAME +
                    ";\n" +
                    "@Name('map') select * from " +
                    MAP_TYPENAME +
                    ";\n" +
                    "@Name('oa') select * from " +
                    OA_TYPENAME +
                    ";\n" +
                    "@Name('xml') select * from " +
                    XML_TYPENAME +
                    ";\n" +
                    "@Name('avro') select * from " +
                    AVRO_TYPENAME +
                    ";\n" +
                    "@Name('trigger') select * from SupportBean;";
                env.CompileDeploy(epl)
                    .AddListener("map")
                    .AddListener("oa")
                    .AddListener("xml")
                    .AddListener("avro")
                    .AddListener("bean");

                env.Statement("trigger").Events += (
                    sender,
                    updateEventArgs) => {
                    var newEvents = updateEventArgs.NewEvents;
                    var processEvent = updateEventArgs.Runtime.EventService;
                    var ident = (string) newEvents[0].Get("TheString");

                    processEvent.RouteEventBean(new RoutedBeanEvent(ident), BEAN_TYPENAME);
                    processEvent.RouteEventMap(Collections.SingletonDataMap("ident", ident), MAP_TYPENAME);
                    processEvent.RouteEventObjectArray(new object[] {ident}, OA_TYPENAME);

                    var xml = "<myevent ident=\"XXXXXX\"></myevent>\n".Replace("XXXXXX", ident);
                    processEvent.RouteEventXMLDOM(SupportXML.GetDocument(xml).DocumentElement, XML_TYPENAME);

                    var avroSchema = AvroSchemaUtil
                        .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
                    var datum = new GenericRecord(avroSchema.AsRecordSchema());
                    datum.Put("ident", ident);
                    processEvent.RouteEventAvro(datum, AVRO_TYPENAME);
                };

                env.SendEventBean(new SupportBean("xy", -1));
                foreach (var name in "map,bean,oa,xml,avro".SplitCsv()) {
                    var listener = env.Listener(name);
                    Assert.IsTrue(listener.IsInvoked, "failed for " + name);
                    Assert.AreEqual("xy", env.Listener(name).AssertOneGetNewAndReset().Get("ident"));
                }

                env.UndeployAll();
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