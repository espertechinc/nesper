///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.xml
{
    [TestFixture]
    public class TestSchemaXmlEventType : AbstractTestBase
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
                null, configNoNS, model, null, null, null, null, null);

            using (var stream = container.ResourceManager().GetResourceAsStream("regression/simpleWithSchema.xml")) {
                var noNSDoc = new XmlDocument();
                noNSDoc.Load(stream);
                eventSchemaOne = new XMLEventBean(noNSDoc.DocumentElement, eventTypeNoNS);
            }
        }

        [Test]
        public void TestSimpleProperies()
        {
            Assert.AreEqual("SAMPLE_V6", eventSchemaOne.Get("prop4"));
        }

        [Test]
        public void TestNestedProperties()
        {
            Assert.AreEqual(true, eventSchemaOne.Get("Nested1.prop2"));
            Assert.AreEqual(typeof(bool?), eventSchemaOne.Get("Nested1.prop2").GetType());
        }

        [Test]
        public void TestMappedProperties()
        {
            Assert.AreEqual("SAMPLE_V8", eventSchemaOne.Get("Nested3.Nested4('a').prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", eventSchemaOne.Get("Nested3.Nested4('c').prop5[1]"));
        }

        [Test]
        public void TestIndexedProperties()
        {
            Assert.AreEqual(5, eventSchemaOne.Get("Nested1.Nested2.prop3[2]"));
            Assert.AreEqual(typeof(int?), eventSchemaOne.EventType.GetPropertyType("Nested1.Nested2.prop3[2]"));
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

            Assert.AreEqual("c", eventSchemaOne.Get("Nested3.Nested4[2].id"));
            Assert.AreEqual(typeof(string), eventSchemaOne.EventType.GetPropertyType("Nested3.Nested4[1].id"));
        }

        [Test]
        public void TestInvalidCollectionAccess()
        {
            try
            {
                var prop = "Nested3.Nested4.id";
                eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch (Exception e)
            {
                //Expected
            }
            try
            {
                var prop = "Nested3.Nested4.Nested5";
                eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch (Exception)
            {
                //Expected
            }
        }
    }
} // end of namespace
