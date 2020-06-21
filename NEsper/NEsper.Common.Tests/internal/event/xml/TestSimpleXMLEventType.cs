///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.xml
{
    [TestFixture]
    public class TestSimpleXmlEventType : AbstractCommonTest
    {
        private const string xml =
            "<simpleEvent>\n" +
            "\t<nested1 attr1=\"SAMPLE_ATTR1\">\n" +
            "\t\t<prop1>SAMPLE_V1</prop1>\n" +
            "\t\t<prop2>true</prop2>\n" +
            "\t\t<nested2>\n" +
            "\t\t\t<prop3>3</prop3>\n" +
            "\t\t\t<prop3>4</prop3>\n" +
            "\t\t\t<prop3>5</prop3>\n" +
            "\t\t</nested2>\n" +
            "\t</nested1>\n" +
            "\t<prop4 attr2=\"true\">SAMPLE_V6</prop4>\n" +
            "\t<nested3>\n" +
            "\t\t<nested4 id=\"a\">\n" +
            "\t\t\t<prop5>SAMPLE_V7</prop5>\n" +
            "\t\t\t<prop5>SAMPLE_V8</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t\t<nested4 id=\"b\">\n" +
            "\t\t\t<prop5>SAMPLE_V9</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t\t<nested4 id=\"c\">\n" +
            "\t\t\t<prop5>SAMPLE_V10</prop5>\n" +
            "\t\t\t<prop5>SAMPLE_V11</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t</nested3>\n" +
            "</simpleEvent>";

        private EventBean theEvent;

        [SetUp]
        public void SetUp()
        {
            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);

            var config = new ConfigurationCommonEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            config.AddXPathProperty("customProp", "count(/simpleEvent/nested3/nested4)", XPathResultType.Number);

            var eventType = new SimpleXMLEventType(null, config, null, null, null);
            theEvent = new XMLEventBean(simpleDoc.DocumentElement, eventType);
        }

        [Test, RunInApplicationDomain]
        public void TestCustomProperty()
        {
            Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("customProp"));
            Assert.AreEqual(3.0d, theEvent.Get("customProp"));
        }

        [Test, RunInApplicationDomain]
        public void TestIndexedProperties()
        {
            Assert.AreEqual("5", theEvent.Get("nested1.nested2.prop3[2]"));
            Assert.AreEqual(typeof(string), theEvent.EventType.GetPropertyType("nested1.nested2.prop3[2]"));
        }

        [Test, RunInApplicationDomain]
        public void TestMappedProperties()
        {
            Assert.AreEqual("SAMPLE_V8", theEvent.Get("nested3.nested4('a').prop5[1]"));
            Assert.AreEqual("SAMPLE_V10", theEvent.Get("nested3.nested4('c').prop5[0]"));
        }

        [Test, RunInApplicationDomain]
        public void TestNestedProperties()
        {
            Assert.AreEqual("true", theEvent.Get("nested1.prop2"));
        }

        [Test, RunInApplicationDomain]
        public void TestSimpleProperties()
        {
            Assert.AreEqual("SAMPLE_V6", theEvent.Get("prop4"));
        }
    }
} // end of namespace
