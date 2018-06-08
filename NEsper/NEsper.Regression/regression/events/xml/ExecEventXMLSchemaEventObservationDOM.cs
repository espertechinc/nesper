///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventObservationDOM : RegressionExecution {
        internal static readonly string CLASSLOADER_SCHEMA_URI = "regression/sensorSchema.xsd";
        internal static readonly string OBSERVATION_XML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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
    
        public override void Configure(Configuration configuration) {
            var typecfg = new ConfigurationEventTypeXMLDOM();
            typecfg.RootElementName = "Sensor";
            string schemaUri = SupportContainer.Instance
                .ResourceManager()
                .ResolveResourceURL(CLASSLOADER_SCHEMA_URI)
                .ToString();
            typecfg.SchemaResource = schemaUri;
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
            configuration.AddEventType("SensorEvent", typecfg);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtExampleOneText = "select ID, Observation.Command, Observation.ID,\n" +
                    "Observation.Tag[0].ID, Observation.Tag[1].ID\n" +
                    "from SensorEvent";
            EPStatement stmtExampleOne = epService.EPAdministrator.CreateEPL(stmtExampleOneText);
    
            EPStatement stmtExampleTwo_0 = epService.EPAdministrator.CreateEPL("insert into ObservationStream\n" +
                    "select ID, Observation from SensorEvent");
            EPStatement stmtExampleTwo_1 = epService.EPAdministrator.CreateEPL("select Observation.Command, Observation.Tag[0].ID from ObservationStream");
    
            EPStatement stmtExampleThree_0 = epService.EPAdministrator.CreateEPL("insert into TagListStream\n" +
                    "select ID as sensorId, Observation.* from SensorEvent");
            EPStatement stmtExampleThree_1 = epService.EPAdministrator.CreateEPL("select sensorId, Command, Tag[0].ID from TagListStream");
    
            XmlDocument doc = SupportXML.GetDocument(OBSERVATION_XML);
            EventSender sender = epService.EPRuntime.GetEventSender("SensorEvent");
            sender.SendEvent(doc);
    
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleOne.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_0.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_1.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleThree_0.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleThree_1.First());
    
            EPAssertionUtil.AssertProps(stmtExampleTwo_1.First(), "Observation.Command,Observation.Tag[0].ID".Split(','), new object[]{"READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
            EPAssertionUtil.AssertProps(stmtExampleThree_1.First(), "sensorId,Command,Tag[0].ID".Split(','), new object[]{"urn:epc:1:4.16.36", "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
    
            try {
                epService.EPAdministrator.CreateEPL("select Observation.Tag.ID from SensorEvent");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'Observation.Tag.ID': Failed to resolve property 'Observation.Tag.ID' to a stream or nested property in a stream [select Observation.Tag.ID from SensorEvent]", ex.Message);
            }
        }
    
    }
} // end of namespace
