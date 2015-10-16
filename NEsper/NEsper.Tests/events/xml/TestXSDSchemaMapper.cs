///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

using com.espertech.esper.client;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.events.xml
{
    [TestFixture]
    public class TestXSDSchemaMapper 
    {
        [Test]
        public void TestMap()
        {
            Uri uri = ResourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
            String schemaUri = uri.ToString();
    
            SchemaModel model = XSDSchemaMapper.LoadAndMap(schemaUri, null, 2);
            Assert.AreEqual(1, model.Components.Count);
    
            SchemaElementComplex component = model.Components[0];
            Assert.AreEqual("simpleEvent", component.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", component.Namespace);
            Assert.AreEqual(0, component.Attributes.Count);
            Assert.AreEqual(0, component.SimpleElements.Count);
            Assert.AreEqual(3, component.ComplexElements.Count);
            Assert.IsFalse(component.IsArray);
            Assert.IsNull(component.OptionalSimpleType);

            SchemaElementComplex nested1Element = component.ComplexElements[0];
            Assert.AreEqual("nested1", nested1Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", nested1Element.Namespace);
            Assert.AreEqual(1, nested1Element.Attributes.Count);
            Assert.AreEqual(2, nested1Element.SimpleElements.Count);
            Assert.AreEqual(1, nested1Element.ComplexElements.Count);
            Assert.IsFalse(nested1Element.IsArray);
            Assert.IsNull(nested1Element.OptionalSimpleType);

            var schemaTypeString = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
            var schemaTypeBoolean = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);
            var schemaTypeInteger = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);
            var schemaTypeId = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Id);

            Assert.AreEqual("attr1", nested1Element.Attributes[0].Name);
            Assert.AreEqual(String.Empty, nested1Element.Attributes[0].Namespace);
            Assert.AreEqual(schemaTypeString, nested1Element.Attributes[0].SimpleType);
            Assert.AreEqual("prop1", nested1Element.SimpleElements[0].Name);
            Assert.AreEqual(schemaTypeString, nested1Element.SimpleElements[0].SimpleType);
            Assert.AreEqual("prop2", nested1Element.SimpleElements[1].Name);
            Assert.AreEqual(schemaTypeBoolean, nested1Element.SimpleElements[1].SimpleType);
    
            SchemaElementComplex nested2Element = nested1Element.ComplexElements[0];
            Assert.AreEqual("nested2", nested2Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", nested2Element.Namespace);
            Assert.AreEqual(0, nested2Element.Attributes.Count);
            Assert.AreEqual(1, nested2Element.SimpleElements.Count);
            Assert.AreEqual(0, nested2Element.ComplexElements.Count);
            Assert.IsFalse(nested2Element.IsArray);
            Assert.IsNull(nested2Element.OptionalSimpleType);
    
            SchemaElementSimple simpleProp3 = nested2Element.SimpleElements[0];
            Assert.AreEqual("prop3", simpleProp3.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", simpleProp3.Namespace);
            Assert.AreEqual(schemaTypeInteger, simpleProp3.SimpleType);
            Assert.IsTrue(simpleProp3.IsArray);
    
            SchemaElementComplex prop4Element = component.ComplexElements[1];
            Assert.AreEqual("prop4", prop4Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", prop4Element.Namespace);
            Assert.AreEqual(1, prop4Element.Attributes.Count);
            Assert.AreEqual(0, prop4Element.SimpleElements.Count);
            Assert.AreEqual(0, prop4Element.ComplexElements.Count);
            Assert.AreEqual("attr2", prop4Element.Attributes[0].Name);
            Assert.AreEqual(schemaTypeBoolean, prop4Element.Attributes[0].SimpleType);
            Assert.IsFalse(prop4Element.IsArray);
            Assert.AreEqual(schemaTypeString, prop4Element.OptionalSimpleType);
    
            SchemaElementComplex nested3Element = component.ComplexElements[2];
            Assert.AreEqual("nested3", nested3Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", nested3Element.Namespace);
            Assert.AreEqual(0, nested3Element.Attributes.Count);
            Assert.AreEqual(0, nested3Element.SimpleElements.Count);
            Assert.AreEqual(1, nested3Element.ComplexElements.Count);
            Assert.IsFalse(nested3Element.IsArray);
            Assert.IsNull(nested3Element.OptionalSimpleType);
    
            SchemaElementComplex nested4Element = nested3Element.ComplexElements[0];
            Assert.AreEqual("nested4", nested4Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", nested4Element.Namespace);
            Assert.AreEqual(1, nested4Element.Attributes.Count);
            Assert.AreEqual(1, nested4Element.SimpleElements.Count);
            Assert.AreEqual(0, nested4Element.ComplexElements.Count);
            Assert.AreEqual("id", nested4Element.Attributes[0].Name);
            Assert.AreEqual(schemaTypeId, nested4Element.Attributes[0].SimpleType);
            Assert.IsTrue(nested4Element.IsArray);
            Assert.IsNull(nested4Element.OptionalSimpleType);
    
            SchemaElementSimple prop5Element = nested4Element.SimpleElements[0];
            Assert.AreEqual("prop5", prop5Element.Name);
            Assert.AreEqual("samples:schemas:simpleSchema", prop5Element.Namespace);
            Assert.AreEqual(schemaTypeString, prop5Element.SimpleType);
            Assert.IsTrue(prop5Element.IsArray);
        }
    
        [Test]
        public void TestEvent()
        {
            //URL url = ResourceLoader.ResolveClassPathOrURLResource("schema", "regression/typeTestSchema.xsd");
            var stream = ResourceManager.GetResourceAsStream("regression/simpleSchema.xsd");
            var xsModel = XmlSchema.Read(stream, delegate { });

            var elements = new List<XmlSchemaElement>();
            foreach( XmlSchemaObject item in xsModel.Items ) {
                if ( item is XmlSchemaElement ) {
                    elements.Add((XmlSchemaElement) item);
                }
            }

            foreach( XmlSchemaElement element in elements ) {
                Console.WriteLine("name '{0}' namespace '{1}'",
                                  element.Name,
                                  element.Namespaces);
            }

            var firstElement = elements.First();
            var firstElementType = firstElement.SchemaType;
            if (!(firstElementType is XmlSchemaComplexType)) {
                throw new PropertyAccessException("Invalid schema - the root element must have at least either attribute declarations or childs elements");
            }

            Console.WriteLine(firstElementType);
        }
    }
}
