///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestSchemaXMLForObservation
    {
        private const String CLASSLOADER_SCHEMA_URI = "regression/sensorSchema.xsd";

        private EPServiceProvider _epService;
    
        private const String XML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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
    
        [Test]
        public void TestObservationExamplePropertyExpression()
        {
            ConfigurationEventTypeXMLDOM typecfg = new ConfigurationEventTypeXMLDOM();
            typecfg.RootElementName = "Sensor";
            String schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            typecfg.SchemaResource = schemaUri;
    
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.ViewResourcesConfig.IsIterableUnbound = true;
            configuration.AddEventType("SensorEvent", typecfg);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            String stmtExampleOneText = "select ID, Observation.Command, Observation.ID,\n" +
                    "Observation.Tag[0].ID, Observation.Tag[1].ID\n" +
                    "from SensorEvent";
            EPStatement stmtExampleOne = _epService.EPAdministrator.CreateEPL(stmtExampleOneText);
    
            EPStatement stmtExampleTwo_0 = _epService.EPAdministrator.CreateEPL("insert into ObservationStream\n" +
                    "select ID, Observation from SensorEvent");
            EPStatement stmtExampleTwo_1 = _epService.EPAdministrator.CreateEPL("select Observation.Command, Observation.Tag[0].ID from ObservationStream");
    
            EPStatement stmtExampleThree_0 = _epService.EPAdministrator.CreateEPL("insert into TagListStream\n" +
                    "select ID as sensorId, Observation.* from SensorEvent");
            EPStatement stmtExampleThree_1 = _epService.EPAdministrator.CreateEPL("select sensorId, Command, Tag[0].ID from TagListStream");
    
            XmlDocument doc = SupportXML.GetDocument(XML);
            EventSender sender = _epService.EPRuntime.GetEventSender("SensorEvent");
            sender.SendEvent(doc);
    
            EventTypeAssertionUtil.AssertConsistency(stmtExampleOne.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_0.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_1.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleThree_0.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleThree_1.First());
    
            EPAssertionUtil.AssertProps(stmtExampleTwo_1.First(), "Observation.Command,Observation.Tag[0].ID".Split(','), new Object[]{"READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
            EPAssertionUtil.AssertProps(stmtExampleThree_1.First(), "sensorId,Command,Tag[0].ID".Split(','), new Object[]{"urn:epc:1:4.16.36", "READ_PALLET_TAGS_ONLY", "urn:epc:1:2.24.400"});
    
            try {
                _epService.EPAdministrator.CreateEPL("select Observation.Tag.ID from SensorEvent");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'Observation.Tag.ID': Failed to resolve property 'Observation.Tag.ID' to a stream or nested property in a stream [select Observation.Tag.ID from SensorEvent]", ex.Message);
            }
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestObservationExampleXPathExpr()
        {
            String schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
    
            ConfigurationEventTypeXMLDOM sensorcfg = new ConfigurationEventTypeXMLDOM();
            sensorcfg.RootElementName = "Sensor";
            sensorcfg.AddXPathProperty("countTags", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number);
            sensorcfg.AddXPathProperty("countTagsInt", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number, "int");
            sensorcfg.AddNamespacePrefix("ss", "SensorSchema");
            sensorcfg.AddXPathProperty("idarray", "//ss:Tag/ss:ID", XPathResultType.NodeSet, "String[]");
            sensorcfg.AddXPathPropertyFragment("tagArray", "//ss:Tag", XPathResultType.NodeSet, "TagEvent");
            sensorcfg.AddXPathPropertyFragment("tagOne", "//ss:Tag[position() = 1]", XPathResultType.Any, "TagEvent");
            sensorcfg.SchemaResource = schemaUri;
    
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.ViewResourcesConfig.IsIterableUnbound = true;
            configuration.AddEventType("SensorEvent", sensorcfg);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            ConfigurationEventTypeXMLDOM tagcfg = new ConfigurationEventTypeXMLDOM();
            tagcfg.RootElementName = "//Tag";
            tagcfg.SchemaResource = schemaUri;
            _epService.EPAdministrator.Configuration.AddEventType("TagEvent", tagcfg);
    
            EPStatement stmtExampleOne = _epService.EPAdministrator.CreateEPL("select countTags, countTagsInt, idarray, tagArray, tagOne from SensorEvent");
            EPStatement stmtExampleTwo_0 = _epService.EPAdministrator.CreateEPL("insert into TagOneStream select tagOne.* from SensorEvent");
            EPStatement stmtExampleTwo_1 = _epService.EPAdministrator.CreateEPL("select ID from TagOneStream");
            EPStatement stmtExampleTwo_2 = _epService.EPAdministrator.CreateEPL("insert into TagArrayStream select tagArray as mytags from SensorEvent");
            EPStatement stmtExampleTwo_3 = _epService.EPAdministrator.CreateEPL("select mytags[1].ID from TagArrayStream");
    
            XmlDocument doc = SupportXML.GetDocument(XML);
            _epService.EPRuntime.SendEvent(doc);
    
            EventTypeAssertionUtil.AssertConsistency(stmtExampleOne.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_0.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_1.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_2.First());
            EventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_3.First());
    
            Object resultArray = stmtExampleOne.First().Get("idarray");
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) resultArray, new String[]{"urn:epc:1:2.24.400", "urn:epc:1:2.24.401"});
            EPAssertionUtil.AssertProps(stmtExampleOne.First(), "countTags,countTagsInt".Split(','), new Object[]{2d, 2});
            Assert.AreEqual("urn:epc:1:2.24.400", stmtExampleTwo_1.First().Get("ID"));
            Assert.AreEqual("urn:epc:1:2.24.401", stmtExampleTwo_3.First().Get("mytags[1].ID"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
