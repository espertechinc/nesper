///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using static com.espertech.esper.regression.events.xml.ExecEventXMLSchemaEventObservationDOM;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventObservationXPath : RegressionExecution
    {
        private string _schemaUri;
    
        public override void Configure(Configuration configuration)
        {
            _schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
    
            var sensorcfg = new ConfigurationEventTypeXMLDOM();
            sensorcfg.RootElementName = "Sensor";
            sensorcfg.AddXPathProperty("countTags", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number);
            sensorcfg.AddXPathProperty("countTagsInt", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number, "int");
            sensorcfg.AddNamespacePrefix("ss", "SensorSchema");
            sensorcfg.AddXPathProperty("idarray", "//ss:Tag/ss:ID", XPathResultType.NodeSet, "string[]");
            sensorcfg.AddXPathPropertyFragment("tagArray", "//ss:Tag", XPathResultType.NodeSet, "TagEvent");
            sensorcfg.AddXPathPropertyFragment("tagOne", "//ss:Tag[position() = 1]", XPathResultType.Any, "TagEvent");
            sensorcfg.SchemaResource = _schemaUri;
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
            configuration.AddEventType("SensorEvent", sensorcfg);
        }
    
        public override void Run(EPServiceProvider epService) {
            var tagcfg = new ConfigurationEventTypeXMLDOM();
            tagcfg.RootElementName = "//Tag";
            tagcfg.SchemaResource = _schemaUri;
            epService.EPAdministrator.Configuration.AddEventType("TagEvent", tagcfg);
    
            EPStatement stmtExampleOne = epService.EPAdministrator.CreateEPL("select countTags, countTagsInt, idarray, tagArray, tagOne from SensorEvent");
            EPStatement stmtExampleTwo_0 = epService.EPAdministrator.CreateEPL("insert into TagOneStream select tagOne.* from SensorEvent");
            EPStatement stmtExampleTwo_1 = epService.EPAdministrator.CreateEPL("select ID from TagOneStream");
            EPStatement stmtExampleTwo_2 = epService.EPAdministrator.CreateEPL("insert into TagArrayStream select tagArray as mytags from SensorEvent");
            EPStatement stmtExampleTwo_3 = epService.EPAdministrator.CreateEPL("select mytags[1].ID from TagArrayStream");
    
            XmlDocument doc = SupportXML.GetDocument(OBSERVATION_XML);
            epService.EPRuntime.SendEvent(doc);
    
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleOne.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_0.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_1.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_2.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtExampleTwo_3.First());
    
            Object resultArray = stmtExampleOne.First().Get("idarray");
            EPAssertionUtil.AssertEqualsExactOrder((object[]) resultArray, new string[]{"urn:epc:1:2.24.400", "urn:epc:1:2.24.401"});
            EPAssertionUtil.AssertProps(stmtExampleOne.First(), "countTags,countTagsInt".Split(','), new object[]{2d, 2});
            Assert.AreEqual("urn:epc:1:2.24.400", stmtExampleTwo_1.First().Get("ID"));
            Assert.AreEqual("urn:epc:1:2.24.401", stmtExampleTwo_3.First().Get("mytags[1].ID"));
        }
    }
} // end of namespace
