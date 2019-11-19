///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraEventSender : RegressionExecution
    {
        public const string XML_TYPENAME = "EventInfraEventSenderXML";
        public const string MAP_TYPENAME = "EventInfraEventSenderMap";
        public const string OA_TYPENAME = "EventInfraEventSenderOA";
        public const string AVRO_TYPENAME = "EventInfraEventSenderAvro";

        public void Run(RegressionEnvironment env)
        {
            // Bean
            RunAssertionSuccess(env, "SupportBean", new SupportBean());
            RunAssertionInvalid(
                env,
                "SupportBean",
                new SupportBean_G("G1"),
                "Event object of type " +
                typeof(SupportBean_G).FullName +
                " does not equal, extend or implement the type " +
                typeof(SupportBean).FullName +
                " of event type 'SupportBean'");
            RunAssertionSuccess(env, "SupportMarkerInterface", new SupportMarkerImplA("Q2"), new SupportBean_G("Q3"));

            // Map
            RunAssertionSuccess(env, MAP_TYPENAME, new Dictionary<string, object>());
            RunAssertionInvalid(
                env,
                MAP_TYPENAME,
                new SupportBean(),
                "Unexpected event object of type " +
                typeof(SupportBean).CleanName() +
                ", expected " +
                typeof(IDictionary<string, object>).CleanName());

            // Object-Array
            RunAssertionSuccess(env, OA_TYPENAME);
            RunAssertionInvalid(
                env,
                OA_TYPENAME,
                new SupportBean(),
                "Unexpected event object of type " + typeof(SupportBean).CleanName() + ", expected Object[]");

            // XML
            RunAssertionSuccess(env, XML_TYPENAME, SupportXML.GetDocument("<Myevent/>").DocumentElement);
            RunAssertionInvalid(
                env,
                XML_TYPENAME,
                new SupportBean(),
                "Unexpected event object type '" +
                typeof(SupportBean).CleanName() +
                "' encountered, please supply a XmlDocument or XmlElement node");
            RunAssertionInvalid(
                env,
                XML_TYPENAME,
                SupportXML.GetDocument("<xxxx/>"),
                "Unexpected root element name 'xxxx' encountered, expected a root element name of 'Myevent'");

            // Avro
            var schema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            RunAssertionSuccess(env, AVRO_TYPENAME, new GenericRecord(schema));
            RunAssertionInvalid(
                env,
                AVRO_TYPENAME,
                new SupportBean(),
                "Unexpected event object type '" +
                typeof(SupportBean).CleanName() +
                "' encountered, please supply a GenericRecord");

            // No such type
            try {
                env.EventService.GetEventSender("ABC");
                Assert.Fail();
            }
            catch (EventTypeException ex) {
                Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
            }

            // Internal implicit wrapper type
            env.CompileDeploy("insert into ABC select *, TheString as value from SupportBean");
            try {
                env.EventService.GetEventSender("ABC");
                Assert.Fail("Event type named 'ABC' could not be found");
            }
            catch (EventTypeException ex) {
                Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
            }

            env.UndeployAll();
        }

        private void RunAssertionSuccess(
            RegressionEnvironment env,
            string typename,
            params object[] correctUnderlyings)
        {
            var stmtText = "@Name('s0') select * from " + typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var sender = env.EventService.GetEventSender(typename);
            foreach (var underlying in correctUnderlyings) {
                sender.SendEvent(underlying);
                Assert.AreSame(underlying, env.Listener("s0").AssertOneGetNewAndReset().Underlying);
            }

            env.UndeployAll();
        }

        private void RunAssertionInvalid(
            RegressionEnvironment env,
            string typename,
            object incorrectUnderlying,
            string message)
        {
            var sender = env.EventService.GetEventSender(typename);

            try {
                sender.SendEvent(incorrectUnderlying);
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }
    }
} // end of namespace