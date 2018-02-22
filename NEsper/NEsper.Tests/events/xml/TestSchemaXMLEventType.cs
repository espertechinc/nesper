///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.xml
{
    [TestFixture]
    public class TestSchemaXMLEventType
    {
        private EventBean _eventSchemaOne;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var schemaUrl = _container.ResourceManager().ResolveResourceURL("regression/simpleSchema.xsd");
            var configNoNS = new ConfigurationEventTypeXMLDOM();
            configNoNS.IsXPathPropertyExpr = true;
            configNoNS.SchemaResource = schemaUrl.ToString();
            configNoNS.RootElementName = "simpleEvent";
            configNoNS.AddXPathProperty("customProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            configNoNS.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            var model = XSDSchemaMapper.LoadAndMap(
                schemaUrl.ToString(), null, SupportEngineImportServiceFactory.Make(_container),
                _container.ResourceManager());
            var eventTypeNoNS = new SchemaXMLEventType(
                null, 1, configNoNS, model, _container.Resolve<EventAdapterService>(), _container.LockManager());

            var noNSDoc = new XmlDocument();
            using (var stream = _container.ResourceManager().GetResourceAsStream("regression/simpleWithSchema.xml"))
            {
                noNSDoc.Load(stream);
            }

            _eventSchemaOne = new XMLEventBean(noNSDoc.DocumentElement, eventTypeNoNS);
        }
    
        [Test]
        public void TestSimpleProperties()
        {
            Assert.AreEqual("SAMPLE_V6", _eventSchemaOne.Get("prop4"));
            Assert.IsFalse(_eventSchemaOne.EventType.IsProperty("window(*)"));
        }
    
        [Test]
        public void TestNestedProperties()
        {
            Assert.AreEqual(true, _eventSchemaOne.Get("nested1.prop2"));
            Assert.AreEqual(typeof(bool), _eventSchemaOne.Get("nested1.prop2").GetType());
        }
    
        [Test]
        public void TestMappedProperties()
        {
            Assert.AreEqual("SAMPLE_V8", _eventSchemaOne.Get("nested3.nested4('a').prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", _eventSchemaOne.Get("nested3.nested4('c').prop5[1]"));
        }
    
        [Test]
        public void TestIndexedProperties()
        {
            Assert.AreEqual(5, _eventSchemaOne.Get("nested1.nested2.prop3[2]"));
            Assert.AreEqual(typeof(int?), _eventSchemaOne.EventType.GetPropertyType("nested1.nested2.prop3[2]"));
        }
    
        [Test]
        public void TestCustomProperty()
        {
            Assert.AreEqual(typeof(double?), _eventSchemaOne.EventType.GetPropertyType("customProp"));
            Assert.AreEqual(3.0d, _eventSchemaOne.Get("customProp"));
        }
    
        [Test]
        public void TestAttrProperty()
        {
            Assert.AreEqual(true, _eventSchemaOne.Get("prop4.attr2"));
            Assert.AreEqual(typeof(bool?), _eventSchemaOne.EventType.GetPropertyType("prop4.attr2"));
            Assert.AreEqual("c", _eventSchemaOne.Get("nested3.nested4[2].id"));
            Assert.AreEqual(typeof(string), _eventSchemaOne.EventType.GetPropertyType("nested3.nested4[1].id"));
        }
    
        [Test]
        public void TestInvalidCollectionAccess() 
        {
            try
            {
                const string prop = "nested3.nested4.id";
                _eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch
            {
                //Expected
            }
            
            try
            {
                const string prop = "nested3.nested4.nested5";
                _eventSchemaOne.EventType.GetGetter(prop);
                Assert.Fail("Invalid collection access: " + prop + " accepted");
            }
            catch
            {
                //Expected
            }
        }
    }
}
