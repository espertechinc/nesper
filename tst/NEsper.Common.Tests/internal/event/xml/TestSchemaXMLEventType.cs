///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.xml
{
    [TestFixture]
    public class TestSchemaXmlEventType : AbstractCommonTest
    {
        private EventBean eventSchemaOne;

        [SetUp]
        public void SetUp()
        {
            var schemaUrl = container.ResourceManager().ResolveResourceURL("regression/simpleSchema.xsd");
            var configNoNS = new ConfigurationCommonEventTypeXMLDOM();
            configNoNS.IsXPathPropertyExpr = true;
            configNoNS.SchemaResource = schemaUrl.ToString();
            configNoNS.RootElementName = "simpleEvent";
            configNoNS.AddXPathProperty("customProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            configNoNS.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");

            var model = XSDSchemaMapper.LoadAndMap(
                schemaUrl.ToString(),
                null,
                container.ResourceManager());
            var eventTypeNoNS = new SchemaXMLEventType(
                null, configNoNS, model, null, null, null, null, null, new EventTypeXMLXSDHandlerImpl());

            using (var stream = container.ResourceManager().GetResourceAsStream("regression/simpleWithSchema.xml")) {
                var noNSDoc = new XmlDocument();
                noNSDoc.Load(stream);
                eventSchemaOne = new XMLEventBean(noNSDoc.DocumentElement, eventTypeNoNS);
            }
        }

        [Test]
        public void TestSimpleProperties()
        {
            Assert.AreEqual("SAMPLE_V6", eventSchemaOne.Get("prop4"));
        }

        [Test]
        public void TestNestedProperties()
        {
            Assert.AreEqual(true, eventSchemaOne.Get("nested1.prop2"));
            Assert.AreEqual(typeof(bool), eventSchemaOne.Get("nested1.prop2").GetType());
        }

        [Test]
        public void TestMappedProperties()
        {
            Assert.AreEqual("SAMPLE_V8", eventSchemaOne.Get("nested3.nested4('a').prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", eventSchemaOne.Get("nested3.nested4('c').prop5[1]"));
        }

        [Test]
        public void TestIndexedProperties()
        {
            Assert.AreEqual(5, eventSchemaOne.Get("nested1.nested2.prop3[2]"));
            Assert.AreEqual(typeof(int?), eventSchemaOne.EventType.GetPropertyType("nested1.nested2.prop3[2]"));
        }

        [Test]
        public void TestCustomProperty()
        {
            Assert.AreEqual(typeof(double?), eventSchemaOne.EventType.GetPropertyType("customProp"));
            Assert.AreEqual(3.0d, eventSchemaOne.Get("customProp"));
        }

        [Test]
        public void TestAttrProperty()
        {
            Assert.AreEqual(true, eventSchemaOne.Get("prop4.attr2"));
            Assert.AreEqual(typeof(bool?), eventSchemaOne.EventType.GetPropertyType("prop4.attr2"));

            Assert.AreEqual("c", eventSchemaOne.Get("nested3.nested4[2].id"));
            Assert.AreEqual(typeof(string), eventSchemaOne.EventType.GetPropertyType("nested3.nested4[1].id"));
        }

        [Test]
        public void TestInvalidCollectionAccess()
        {
            try
            {
                var prop = "nested3.nested4.id";
                eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch (Exception)
            {
                //Expected
            }
            try
            {
                var prop = "nested3.nested4.nested5";
                eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch (Exception)
            {
                //Expected
            }
        }
    }

    public class EventTypeXMLXSDHandlerImpl : EventTypeXMLXSDHandler
    {
        public XPathResultType SimpleTypeToResultType(XmlSchemaSimpleType type)
        {
            throw new NotImplementedException();
        }

        public Type ToReturnType(
            XmlSchemaSimpleType xsType,
            string typeName,
            int? optionalFractionDigits)
        {
            throw new NotImplementedException();
        }

        public SchemaModel LoadAndMap(
            string schemaResource,
            string schemaText,
            ImportService importService)
        {
            throw new NotImplementedException();
        }
    }
} // end of namespace
