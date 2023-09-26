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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

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
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventObservationDOMCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
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
            var stmtExampleOneText = "@name('s0') select ID, Observation.Command, Observation.ID,\n" +
                                     "Observation.Tag[0].ID, Observation.Tag[1].ID\n" +
                                     "from " +
                                     eventTypeName;
            env.CompileDeploy(stmtExampleOneText, path).AddListener("s0");

            env.CompileDeploy(
                "@name('e2_0') @public insert into ObservationStream\n" +
                "select ID, Observation from " +
                eventTypeName,
                path);
            env.CompileDeploy(
                "@name('e2_1') select Observation.Command, Observation.Tag[0].ID from ObservationStream",
                path);

            env.CompileDeploy(
                "@name('e3_0') @public insert into TagListStream\n" +
                "select ID as sensorId, Observation.* from " +
                eventTypeName,
                path);
            env.CompileDeploy("@name('e3_1') select sensorId, Command, Tag[0].ID from TagListStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            env.SendEventXMLDOM(doc, eventTypeName);

            env.AssertIterator("s0", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));
            env.AssertIterator("e2_0", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));
            env.AssertIterator("e2_1", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));
            env.AssertIterator("e3_0", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));

            // e3_1 will fail because esper does not create an intermediary simple type for 'Tag' - the consequence
            // of that is that it creates a property with the name Tag[0].ID.  When consistency attempts to look
            // for Tag[0].ID[0] it fails because it cannot find a simple property matching that name, and it then
            // attempts to break it into a nested property.  As a nested property it *should* find 'Tag' but because
            // we create no intermediate structure, it fails.

            env.AssertIterator("e3_1", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));

            env.AssertIterator(
                "e2_0",
                iterator => EPAssertionUtil.AssertProps(
                    iterator.Advance(),
                    "Observation.Command,Observation.Tag[0].ID".SplitCsv(),
                    new object[] { "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400" }));
            env.AssertIterator(
                "e3_0",
                iterator => EPAssertionUtil.AssertProps(
                    iterator.Advance(),
                    "sensorId,Command,Tag[0].ID".SplitCsv(),
                    new object[] { "urn:epc:1:4.16.36", "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400" }));

            env.TryInvalidCompile(
                path,
                "select Observation.Tag.ID from " + eventTypeName,
                "Failed to validate select-clause expression 'Observation.Tag.ID': Failed to resolve property 'Observation.Tag.ID' to a stream or nested property in a stream");

            env.UndeployAll();
        }
    }
} // end of namespace