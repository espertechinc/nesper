///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventObservationDOM
    {
        public const string OBSERVATION_XML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                              "<Sensor xmlns=\"SensorSchema\" >\n" +
                                              "\t<ID>urn:epc:1:4.16.36</ID>\n" +
                                              "\t<Observation Command=\"READ_PALLET_TAGS_ONLY\">\n" +
                                              "\t\t<ID>00000001</ID>\n" +
                                              "\t\t<Tag>\n" +
                                              "\t\t\t<ID>urn:epc:1:2.24.400</ID>\n" +
                                              "\t\t</Tag>\n" +
                                              "\t\t<Tag>\n" +
                                              "\t\t\t<ID>urn:epc:1:2.24.401</ID>\n" +
                                              "\t\t</Tag>\n" +
                                              "\t</Observation>\n" +
                                              "</Sensor>";


        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventObservationDOMCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventObservationDOMPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventObservationDOMPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "SensorEvent", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventObservationDOMCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSensorEvent = resourceManager.ResolveResourceURL("regression/sensorSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='Sensor', SchemaResource='" +
                          schemaUriSensorEvent +
                          "')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }


        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            var stmtExampleOneText =
                "@Name('s0') select ID, Observation.Command, Observation.ID,\n" +
                "Observation.Tag[0].ID, Observation.Tag[1].ID\n" +
                "from " +
                eventTypeName;
            env.CompileDeploy(stmtExampleOneText, path).AddListener("s0");

            env.CompileDeploy(
                "@Name('e2_0') insert into ObservationStream\n" +
                "select ID, Observation from " +
                eventTypeName,
                path);
            env.CompileDeploy("@Name('e2_1') select Observation.Command, Observation.Tag[0].ID from ObservationStream", path);

            env.CompileDeploy(
                "@Name('e3_0') insert into TagListStream\n" +
                "select ID as sensorId, Observation.* from " +
                eventTypeName,
                path);
            env.CompileDeploy("@Name('e3_1') select sensorId, Command, Tag[0].ID from TagListStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            var sender = env.EventService.GetEventSender(eventTypeName);

            sender.SendEvent(doc);

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2_0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2_1").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3_0").Advance());

            // e3_1 will fail because esper does not create an intermediary simple type for 'Tag' - the consequence
            // of that is that it creates a property with the name Tag[0].ID.  When consistency attempts to look
            // for Tag[0].ID[0] it fails because it cannot find a simple property matching that name, and it then
            // attempts to break it into a nested property.  As a nested property it *should* find 'Tag' but because
            // we create no intermediate structure, it fails.

            //SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3_1").Advance());

            EPAssertionUtil.AssertProps(
                env.GetEnumerator("e2_0").Advance(),
                new[] {"Observation.Command", "Observation.Tag[0].ID"},
                new object[] {"READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
            EPAssertionUtil.AssertProps(
                env.GetEnumerator("e3_0").Advance(),
                new[] {"sensorId", "Command", "Tag[0].ID"},
                new object[] {"urn:epc:1:4.16.36", "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});

            TryInvalidCompile(
                env,
                path,
                "select Observation.Tag.ID from " + eventTypeName,
                "Failed to validate select-clause expression 'Observation.Tag.ID': Failed to resolve property 'Observation.Tag.ID' to a stream or nested property in a stream");

            env.UndeployAll();
        }
    }
} // end of namespace