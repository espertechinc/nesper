///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.xml
{
    [TestFixture]
    public class TestXsdSchemaMapper : AbstractCommonTest
    {
        private XmlSchemaType _schemaTypeId;
        private XmlSchemaType _schemaTypeString;
        private XmlSchemaType _schemaTypeBoolean;
        private XmlSchemaType _schemaTypeDecimal;
        private XmlSchemaType _schemaTypeInt;

        [SetUp]
        public void SetUp()
        {
            _schemaTypeId = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Id);
            _schemaTypeString = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
            _schemaTypeBoolean = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);
            _schemaTypeDecimal = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);
            _schemaTypeInt = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);
        }

        [Test]
        public void TestMap()
        {
            var uri = container.ResourceManager().ResolveResourceURL("regression/simpleSchema.xsd");
            var schemaUri = uri.ToString();

            var model = XSDSchemaMapper.LoadAndMap(schemaUri, null, container.ResourceManager());
            Assert.That(model.Components.Count, Is.EqualTo(1));

            var simpleEvent = model.Components[0];
            VerifyComplexElement(simpleEvent, "simpleEvent", false);
            VerifySizes(simpleEvent, 0, 0, 3);

            var nested1 = simpleEvent.ComplexElements[0];
            VerifyComplexElement(nested1, "nested1", false);
            VerifySizes(nested1, 1, 2, 1);
            ClassicAssert.AreEqual("attr1", nested1.Attributes[0].Name);
            ClassicAssert.AreEqual(string.Empty, nested1.Attributes[0].Namespace);
            ClassicAssert.AreEqual(_schemaTypeString, nested1.Attributes[0].SimpleType);
            ClassicAssert.AreEqual("prop1", nested1.SimpleElements[0].Name);
            ClassicAssert.AreEqual(_schemaTypeString, nested1.SimpleElements[0].SimpleType);
            ClassicAssert.AreEqual("prop2", nested1.SimpleElements[1].Name);
            ClassicAssert.AreEqual(_schemaTypeBoolean, nested1.SimpleElements[1].SimpleType);

            var nested2 = nested1.ComplexElements[0];
            VerifyComplexElement(nested2, "nested2", false);
            VerifySizes(nested2, 0, 1, 0);
            VerifySimpleElement(nested2.SimpleElements[0], "prop3", _schemaTypeInt);

            var prop4 = simpleEvent.ComplexElements[1];
            VerifyElement(prop4, "prop4");
            VerifySizes(prop4, 1, 0, 0);
            ClassicAssert.AreEqual("attr2", prop4.Attributes[0].Name);
            ClassicAssert.AreEqual(_schemaTypeBoolean, prop4.Attributes[0].SimpleType);
            ClassicAssert.AreEqual(_schemaTypeString, prop4.OptionalSimpleType);

            var nested3 = simpleEvent.ComplexElements[2];
            VerifyComplexElement(nested3, "nested3", false);
            VerifySizes(nested3, 0, 0, 1);

            var nested4 = nested3.ComplexElements[0];
            VerifyComplexElement(nested4, "nested4", true);
            VerifySizes(nested4, 1, 4, 0);
            ClassicAssert.AreEqual("id", nested4.Attributes[0].Name);
            ClassicAssert.AreEqual(_schemaTypeId, nested4.Attributes[0].SimpleType);
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
            ClassicAssert.AreEqual(1, model.Components.Count);

            SchemaElementComplex component = model.Components[0];
            ClassicAssert.AreEqual("simpleEvent", component.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", component.Namespace);
            ClassicAssert.AreEqual(0, component.Attributes.Count);
            ClassicAssert.AreEqual(0, component.SimpleElements.Count);
            ClassicAssert.AreEqual(3, component.ComplexElements.Count);
            ClassicAssert.IsFalse(component.IsArray);
            ClassicAssert.IsNull(component.OptionalSimpleType);

            SchemaElementComplex nested1Element = component.ComplexElements[0];
            ClassicAssert.AreEqual("Nested1", nested1Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", nested1Element.Namespace);
            ClassicAssert.AreEqual(1, nested1Element.Attributes.Count);
            ClassicAssert.AreEqual(2, nested1Element.SimpleElements.Count);
            ClassicAssert.AreEqual(1, nested1Element.ComplexElements.Count);
            ClassicAssert.IsFalse(nested1Element.IsArray);
            ClassicAssert.IsNull(nested1Element.OptionalSimpleType);

            var schemaTypeString = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
            var schemaTypeBoolean = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);
            var schemaTypeInteger = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);
            var schemaTypeId = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Id);

            ClassicAssert.AreEqual("attr1", nested1Element.Attributes[0].Name);
            ClassicAssert.AreEqual(String.Empty, nested1Element.Attributes[0].Namespace);
            ClassicAssert.AreEqual(schemaTypeString, nested1Element.Attributes[0].SimpleType);
            ClassicAssert.AreEqual("prop1", nested1Element.SimpleElements[0].Name);
            ClassicAssert.AreEqual(schemaTypeString, nested1Element.SimpleElements[0].SimpleType);
            ClassicAssert.AreEqual("prop2", nested1Element.SimpleElements[1].Name);
            ClassicAssert.AreEqual(schemaTypeBoolean, nested1Element.SimpleElements[1].SimpleType);

            SchemaElementComplex nested2Element = nested1Element.ComplexElements[0];
            ClassicAssert.AreEqual("Nested2", nested2Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", nested2Element.Namespace);
            ClassicAssert.AreEqual(0, nested2Element.Attributes.Count);
            ClassicAssert.AreEqual(1, nested2Element.SimpleElements.Count);
            ClassicAssert.AreEqual(0, nested2Element.ComplexElements.Count);
            ClassicAssert.IsFalse(nested2Element.IsArray);
            ClassicAssert.IsNull(nested2Element.OptionalSimpleType);

            SchemaElementSimple simpleProp3 = nested2Element.SimpleElements[0];
            ClassicAssert.AreEqual("prop3", simpleProp3.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", simpleProp3.Namespace);
            ClassicAssert.AreEqual(schemaTypeInteger, simpleProp3.SimpleType);
            ClassicAssert.IsTrue(simpleProp3.IsArray);

            SchemaElementComplex prop4Element = component.ComplexElements[1];
            ClassicAssert.AreEqual("prop4", prop4Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", prop4Element.Namespace);
            ClassicAssert.AreEqual(1, prop4Element.Attributes.Count);
            ClassicAssert.AreEqual(0, prop4Element.SimpleElements.Count);
            ClassicAssert.AreEqual(0, prop4Element.ComplexElements.Count);
            ClassicAssert.AreEqual("attr2", prop4Element.Attributes[0].Name);
            ClassicAssert.AreEqual(schemaTypeBoolean, prop4Element.Attributes[0].SimpleType);
            ClassicAssert.IsFalse(prop4Element.IsArray);
            ClassicAssert.AreEqual(schemaTypeString, prop4Element.OptionalSimpleType);

            SchemaElementComplex nested3Element = component.ComplexElements[2];
            ClassicAssert.AreEqual("Nested3", nested3Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", nested3Element.Namespace);
            ClassicAssert.AreEqual(0, nested3Element.Attributes.Count);
            ClassicAssert.AreEqual(0, nested3Element.SimpleElements.Count);
            ClassicAssert.AreEqual(1, nested3Element.ComplexElements.Count);
            ClassicAssert.IsFalse(nested3Element.IsArray);
            ClassicAssert.IsNull(nested3Element.OptionalSimpleType);

            SchemaElementComplex nested4Element = nested3Element.ComplexElements[0];
            ClassicAssert.AreEqual("Nested4", nested4Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", nested4Element.Namespace);
            ClassicAssert.AreEqual(1, nested4Element.Attributes.Count);
            ClassicAssert.AreEqual(1, nested4Element.SimpleElements.Count);
            ClassicAssert.AreEqual(0, nested4Element.ComplexElements.Count);
            ClassicAssert.AreEqual("id", nested4Element.Attributes[0].Name);
            ClassicAssert.AreEqual(schemaTypeId, nested4Element.Attributes[0].SimpleType);
            ClassicAssert.IsTrue(nested4Element.IsArray);
            ClassicAssert.IsNull(nested4Element.OptionalSimpleType);

            SchemaElementSimple prop5Element = nested4Element.SimpleElements[0];
            ClassicAssert.AreEqual("prop5", prop5Element.Name);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", prop5Element.Namespace);
            ClassicAssert.AreEqual(schemaTypeString, prop5Element.SimpleType);
            ClassicAssert.IsTrue(prop5Element.IsArray);
        }
#endif

        [Test]
        public void TestEvent()
        {
            //URL url = ResourceLoader.ResolveClassPathOrURLResource("schema", "regression/typeTestSchema.xsd");
            var stream = container.ResourceManager().GetResourceAsStream("regression/simpleSchema.xsd");
            var xsModel = XmlSchema.Read(stream, delegate { });

            var elements = new List<XmlSchemaElement>();
            foreach (var item in xsModel.Items)
            {
                if (item is XmlSchemaElement)
                {
                    elements.Add((XmlSchemaElement) item);
                }
            }

            foreach (var element in elements) {
                var namespaces = element.Namespaces;
                var namespacesAsStr = namespaces.RenderAny();

                Console.WriteLine(
                    "name '{0}' namespace '{1}'",
                    element.Name,
                    namespacesAsStr);
            }

            var firstElement = elements.First();
            var firstElementType = firstElement.SchemaType;
            if (!(firstElementType is XmlSchemaComplexType))
            {
                throw new PropertyAccessException("Invalid schema - the root element must have at least either attribute declarations or childs elements");
            }

            Console.WriteLine(firstElementType.RenderAny());
        }

        [Test]
        public void TestExtendedElements()
        {
            var uri = container.ResourceManager().ResolveResourceURL("regression/schemaWithExtensions.xsd");
            var schemaUri = uri.ToString();

            var model = XSDSchemaMapper.LoadAndMap(schemaUri, null, container.ResourceManager());

            var complexEvent = model.Components[0];
            VerifyComplexElement(complexEvent, "complexEvent", false);
            VerifySizes(complexEvent, 0, 0, 1);

            var mainElement = complexEvent.ComplexElements[0];
            VerifyComplexElement(mainElement, "mainElement", false);
            VerifySizes(mainElement, 0, 0, 4);

            var baseType4 = mainElement.ComplexElements[0];
            VerifyComplexElement(baseType4, "baseType4", false);
            VerifySizes(baseType4, 0, 0, 0);

            var aType2 = mainElement.ComplexElements[1];
            VerifyComplexElement(aType2, "aType2", false);
            VerifySizes(aType2, 0, 2, 1);

            var aType3 = mainElement.ComplexElements[2];
            VerifyComplexElement(aType3, "aType3", false);
            VerifySizes(aType3, 0, 1, 2);

            var aType3baseType4 = aType3.ComplexElements[0];
            VerifyComplexElement(aType3baseType4, "baseType4", false);
            VerifySizes(aType3baseType4, 0, 0, 0);

            var aType3type2 = aType3.ComplexElements[1];
            VerifyComplexElement(aType3type2, "aType2", false);
            VerifySizes(aType3type2, 0, 2, 1);

            var aType4 = mainElement.ComplexElements[3];
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
} // end of namespace
