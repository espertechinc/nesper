///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventObservationDOM : RegressionExecution
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

        public void Run(RegressionEnvironment env)
        {
            var stmtExampleOneText = "@Name('s0') select ID, Observation.Command, Observation.ID,\n" +
                                     "Observation.Tag[0].ID, Observation.Tag[1].ID\n" +
                                     "from SensorEvent";
            env.CompileDeploy(stmtExampleOneText).AddListener("s0");

            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('e2_0') insert into ObservationStream\n" +
                "select ID, Observation from SensorEvent",
                path);
            env.CompileDeploy(
                "@Name('e2_1') select Observation.Command, Observation.Tag[0].ID from ObservationStream",
                path);

            env.CompileDeploy(
                "@Name('e3_0') insert into TagListStream\n" +
                "select ID as sensorId, Observation.* from SensorEvent",
                path);
            env.CompileDeploy("@Name('e3_1') select sensorId, Command, Tag[0].ID from TagListStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            var sender = env.EventService.GetEventSender("SensorEvent");
            sender.SendEvent(doc);

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2_0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2_1").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3_0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3_1").Advance());

            EPAssertionUtil.AssertProps(
                env.GetEnumerator("e2_0").Advance(),
                new [] { "Observation.Command","Observation.Tag[0].ID" },
                new object[] {"READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
            EPAssertionUtil.AssertProps(
                env.GetEnumerator("e3_0").Advance(),
                new [] { "sensorId","Command","Tag[0].ID" },
                new object[] {"urn:epc:1:4.16.36", "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});

            TryInvalidCompile(
                env,
                "select Observation.Tag.ID from SensorEvent",
                "Failed to validate select-clause expression 'Observation.Tag.ID': Failed to resolve property 'Observation.Tag.ID' to a stream or nested property in a stream [select Observation.Tag.ID from SensorEvent]");

            env.UndeployAll();
        }
    }
} // end of namespace