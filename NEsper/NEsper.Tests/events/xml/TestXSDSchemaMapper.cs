///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.xml
{
    [TestFixture]
    public class TestXSDSchemaMapper
    {
        private XmlSchemaType _schemaTypeId;
        private XmlSchemaType _schemaTypeString;
        private XmlSchemaType _schemaTypeBoolean;
        private XmlSchemaType _schemaTypeDecimal;
        private XmlSchemaType _schemaTypeInt;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _schemaTypeId = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Id);
            _schemaTypeString = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
            _schemaTypeBoolean = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);
            _schemaTypeDecimal = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);
            _schemaTypeInt = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);
        }

        [Test]
        public void TestMap()
        {
            Uri uri = _container.ResourceManager().ResolveResourceURL("regression/simpleSchema.xsd");
            String schemaUri = uri.ToString();

            SchemaModel model = XSDSchemaMapper.LoadAndMap(schemaUri, null, null, _container.ResourceManager());
            Assert.That(model.Components.Count, Is.EqualTo(1));

            SchemaElementComplex simpleEvent = model.Components[0];
            VerifyComplexElement(simpleEvent, "simpleEvent", false);
            VerifySizes(simpleEvent, 0, 0, 3);

            SchemaElementComplex nested1 = simpleEvent.ComplexElements[0];
            VerifyComplexElement(nested1, "nested1", false);
            VerifySizes(nested1, 1, 2, 1);
            Assert.AreEqual("attr1", nested1.Attributes[0].Name);
            Assert.AreEqual(string.Empty, nested1.Attributes[0].Namespace);
            Assert.AreEqual(_schemaTypeString, nested1.Attributes[0].SimpleType);
            Assert.AreEqual("prop1", nested1.SimpleElements[0].Name);
            Assert.AreEqual(_schemaTypeString, nested1.SimpleElements[0].SimpleType);
            Assert.AreEqual("prop2", nested1.SimpleElements[1].Name);
            Assert.AreEqual(_schemaTypeBoolean, nested1.SimpleElements[1].SimpleType);

            SchemaElementComplex nested2 = nested1.ComplexElements[0];
            VerifyComplexElement(nested2, "nested2", false);
            VerifySizes(nested2, 0, 1, 0);
            VerifySimpleElement(nested2.SimpleElements[0], "prop3", _schemaTypeInt);

            SchemaElementComplex prop4 = simpleEvent.ComplexElements[1];
            VerifyElement(prop4, "prop4");
            VerifySizes(prop4, 1, 0, 0);
            Assert.AreEqual("attr2", prop4.Attributes[0].Name);
            Assert.AreEqual(_schemaTypeBoolean, prop4.Attributes[0].SimpleType);
            Assert.AreEqual(_schemaTypeString, prop4.OptionalSimpleType);

            SchemaElementComplex nested3 = simpleEvent.ComplexElements[2];
            VerifyComplexElement(nested3, "nested3", false);
            VerifySizes(nested3, 0, 0, 1);

            SchemaElementComplex nested4 = nested3.ComplexElements[0];
            VerifyComplexElement(nested4, "nested4", true);
            VerifySizes(nested4, 1, 4, 0);
            Assert.AreEqual("id", nested4.Attributes[0].Name);
            Assert.AreEqual(_schemaTypeId, nested4.Attributes[0].SimpleType);
            VerifySimpleElement(nested4.SimpleElements[0], "prop5", _schemaTypeString);
            VerifySimpleElement(nested4.SimpleElements[1], "prop6", _schemaTypeString);
            VerifySimpleElement(nested4.SimpleElements[2], "prop7", _schemaTypeString);
            VerifySimpleElement(nested4.SimpleElements[3], "prop8", _schemaTypeString);
        }

#if DEPRECATED
        [Test]
        public void TestMap()
        {
            Uri uri = _container.ResourceManager().ResolveResourceURL("regression/simpleSchema.xsd");
            String schemaUri = uri.ToString();
    
            SchemaModel model = XSDSchemaMapper.LoadAndMap(schemaUri, null);
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
#endif
    
        [Test]
        public void TestEvent()
        {
            //URL url = ResourceLoader.ResolveClassPathOrURLResource("schema", "regression/typeTestSchema.xsd");
            var stream = _container.ResourceManager().GetResourceAsStream("regression/simpleSchema.xsd");
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

        [Test]
        public void TestExtendedElements()
        {
            Uri uri = _container.ResourceManager().ResolveResourceURL("regression/schemaWithExtensions.xsd");
            String schemaUri = uri.ToString();

            SchemaModel model = XSDSchemaMapper.LoadAndMap(schemaUri, null, null, _container.ResourceManager());

            SchemaElementComplex complexEvent = model.Components[0];
            VerifyComplexElement(complexEvent, "complexEvent", false);
            VerifySizes(complexEvent, 0, 0, 1);
        
            SchemaElementComplex mainElement = complexEvent.ComplexElements[0];
            VerifyComplexElement(mainElement, "mainElement", false);
            VerifySizes(mainElement, 0, 0, 4);
        
            SchemaElementComplex baseType4 = mainElement.ComplexElements[0];
            VerifyComplexElement(baseType4, "baseType4", false);
            VerifySizes(baseType4, 0, 0, 0);
        
            SchemaElementComplex aType2 = mainElement.ComplexElements[1];
            VerifyComplexElement(aType2, "aType2", false);
            VerifySizes(aType2, 0, 2, 1);
        
            SchemaElementComplex aType3 = mainElement.ComplexElements[2];
            VerifyComplexElement(aType3, "aType3", false);
            VerifySizes(aType3, 0, 1, 2);
        
            SchemaElementComplex aType3baseType4 = aType3.ComplexElements[0];
            VerifyComplexElement(aType3baseType4, "baseType4", false);
            VerifySizes(aType3baseType4, 0, 0, 0);
        
            SchemaElementComplex aType3type2 = aType3.ComplexElements[1];
            VerifyComplexElement(aType3type2, "aType2", false);
            VerifySizes(aType3type2, 0, 2, 1);

            SchemaElementComplex aType4 = mainElement.ComplexElements[3];
            VerifyComplexElement(aType4, "aType4", false);
            VerifySizes(aType4, 0, 0, 1);
         }

        private static void VerifySimpleElement(SchemaElementSimple element, String name, XmlSchemaType type)
        {
            Assert.That(type, Is.EqualTo(element.SimpleType));
            VerifyElement(element, name);
        }

        private static void VerifyComplexElement(SchemaElementComplex element, string name, bool isArray)
        {
            Assert.That(element.OptionalSimpleType, Is.Null);
            Assert.That(element.IsArray, Is.EqualTo(isArray));
            VerifyElement(element, name);
        }

        private static void VerifyElement(SchemaElement element, String name)
        {
            Assert.That(element.Name, Is.EqualTo(name));
            Assert.That(element.Namespace, Is.EqualTo("samples:schemas:simpleSchema"));
        }

        private static void VerifySizes(SchemaElementComplex element, int expectedNumberOfAttributes, int expectedNumberOfSimpleElements, int expectedNumberOfChildren)
        {
            Assert.That(element.Attributes.Count, Is.EqualTo(expectedNumberOfAttributes));
            Assert.That(element.SimpleElements.Count, Is.EqualTo(expectedNumberOfSimpleElements));
            Assert.That(element.ComplexElements.Count, Is.EqualTo(expectedNumberOfChildren));
        }
    }
}
