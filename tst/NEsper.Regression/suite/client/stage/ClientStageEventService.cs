///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.stage;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public class ClientStageEventService
    {
        public const string XML_TYPENAME = "ClientStageEventServiceXML";
        public const string MAP_TYPENAME = "ClientStageEventServiceMap";
        public const string OA_TYPENAME = "ClientStageEventServiceOA";
        public const string AVRO_TYPENAME = "ClientStageEventServiceAvro";
        public const string JSON_TYPENAME = "ClientStageEventServiceJson";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withd(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageEventServiceEventSend());
            return execs;
        }

        private class ClientStageEventServiceEventSend : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var stage = env.StageService.GetStage("ST");
                var path = new RegressionPath();

                // Bean
                RunAssertion(
                    env,
                    path,
                    stage,
                    "SupportBean",
                    new SupportBean(),
                    svc => svc.SendEventBean(new SupportBean(), "SupportBean"));

                // Map
                RunAssertion(
                    env,
                    path,
                    stage,
                    MAP_TYPENAME,
                    new Dictionary<string, object>(),
                    svc => svc.SendEventMap(new Dictionary<string, object>(), MAP_TYPENAME));

                // Object-Array
                RunAssertion(
                    env,
                    path,
                    stage,
                    OA_TYPENAME,
                    Array.Empty<object>(),
                    svc => svc.SendEventObjectArray(Array.Empty<object>(), OA_TYPENAME));

                // XML
                var node = SupportXML.GetDocument("<myevent/>").DocumentElement;
                RunAssertion(
                    env,
                    path,
                    stage,
                    XML_TYPENAME,
                    node,
                    svc => svc.SendEventXMLDOM(node, XML_TYPENAME));

                // Avro
                var schema = AvroSchemaUtil.ResolveAvroSchema(
                    env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
                var record = new GenericRecord(schema.AsRecordSchema());
                RunAssertion(
                    env,
                    path,
                    stage,
                    AVRO_TYPENAME,
                    record,
                    svc => svc.SendEventAvro(record, AVRO_TYPENAME));

                // Json
                RunAssertion(
                    env,
                    path,
                    stage,
                    JSON_TYPENAME,
                    "{}",
                    svc => svc.SendEventJson("{}", JSON_TYPENAME));

                env.UndeployAll();
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            EPStage stage,
            string typename,
            object underlying,
            Consumer<EPEventService> sender)
        {
            var epl = "@public @buseventtype create schema TriggerEvent();\n" +
                      "@public @buseventtype @name('schema') create json schema " +
                      JSON_TYPENAME +
                      "();\n" +
                      "@name('trigger') select * from TriggerEvent;\n" +
                      "@name('s0') select * from " +
                      typename +
                      ";\n";
            env.CompileDeploy(epl, path).AddListener("s0");
            var deploymentId = env.DeploymentId("s0");
            StageIt(env, "ST", deploymentId);

            // test normal send
            sender.Invoke(stage.EventService);
            AssertUnderlying(env, typename, underlying);

            // test EventSender#send
            var eventSender = stage.EventService.GetEventSender(typename);
            eventSender.SendEvent(underlying);
            AssertUnderlying(env, typename, underlying);

            // test EventSender#route
            GetStatement("trigger", "ST", env.Runtime).Events += (
                _,
                args) => {
                eventSender.RouteEvent(underlying);
            };
            stage.EventService.SendEventMap(new Dictionary<string, object>(), "TriggerEvent");
            AssertUnderlying(env, typename, underlying);

            UnstageIt(env, "ST", deploymentId);

            path.Clear();
            env.UndeployModuleContaining("s0");
        }

        private static void AssertUnderlying(
            RegressionEnvironment env,
            string typename,
            object underlying)
        {
            ClassicAssert.IsNotNull(env.ListenerStage("ST", "s0").AssertOneGetNewAndReset().Underlying);
        }
    }
} // end of namespace