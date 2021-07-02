///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary> Utility class for querying schema information.</summary>
    /// <author>
    ///     pablo
    /// </author>
    public class SchemaUtil
    {
        private static readonly IDictionary<string, Type> TypeMap;

        private static readonly XmlSchemaSimpleType SchemaTypeString =
            XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);

        private static readonly XmlSchemaSimpleType SchemaTypeBoolean =
            XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);

        private static readonly XmlSchemaSimpleType SchemaTypeInteger =
            XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);

        private static readonly XmlSchemaSimpleType SchemaTypeDecimal =
            XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);

        private static readonly XmlSchemaSimpleType SchemaTypeId = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Id);

        private static readonly XmlSchemaSimpleType SchemaTypeToken =
            XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Token);

        static SchemaUtil()
        {
            TypeMap = new Dictionary<string, Type>();
            TypeMap["nonPositiveInteger"] = typeof(int?);
            TypeMap["nonNegativeInteger"] = typeof(int?);
            TypeMap["negativeInteger"] = typeof(int?);
            TypeMap["positiveInteger"] = typeof(int?);
            TypeMap["long"] = typeof(long?);
            TypeMap["unsignedLong"] = typeof(ulong?);
            TypeMap["int"] = typeof(int?);
            TypeMap["unsignedInt"] = typeof(uint?);
            TypeMap["decimal"] = typeof(double?);
            TypeMap["integer"] = typeof(int?);
            TypeMap["float"] = typeof(float?);
            TypeMap["double"] = typeof(double?);
            TypeMap["string"] = typeof(string);
            TypeMap["short"] = typeof(short?);
            TypeMap["unsignedShort"] = typeof(ushort?);
            TypeMap["byte"] = typeof(byte?);
            TypeMap["unsignedByte"] = typeof(byte?);
            TypeMap["bool"] = typeof(bool?);
            TypeMap["boolean"] = typeof(bool?);
            TypeMap["dateTime"] = typeof(string);
            TypeMap["date"] = typeof(string);
            TypeMap["time"] = typeof(string);
        }

        public static XPathResultType ToXPathResultType(XmlSchemaSimpleType simpleType)
        {
            if (Equals(simpleType, SchemaTypeString)) {
                return XPathResultType.String;
            }

            if (Equals(simpleType, SchemaTypeBoolean)) {
                return XPathResultType.Boolean;
            }

            if (Equals(simpleType, SchemaTypeInteger) ||
                Equals(simpleType, SchemaTypeDecimal)) {
                return XPathResultType.Number;
            }

            if (Equals(simpleType, SchemaTypeId)) {
                return XPathResultType.String;
            }

            if (Equals(simpleType, SchemaTypeToken)) {
                return XPathResultType.String;
            }

            return XPathResultType.Any;
        }

        /// <summary>
        ///     Returns the Type-type of the schema item.
        /// </summary>
        /// <param name="item">to to determine type for</param>
        /// <returns>
        ///     type
        /// </returns>
        public static Type ToReturnType(SchemaItem item)
        {
            if (item is SchemaItemAttribute att) {
                return ToReturnType(att.SimpleType, att.TypeName);
            }

            if (item is SchemaElementSimple simple) {
                var returnType = ToReturnType(simple.SimpleType, simple.TypeName);
                if (simple.IsArray) {
                    returnType = returnType.MakeArrayType();
                }

                return returnType;
            }

            if (item is SchemaElementComplex complex) {
                if (complex.OptionalSimpleType != null) {
                    return ToReturnType(
                        ToXPathResultType(complex.OptionalSimpleType),
                        complex.OptionalSimpleTypeName.Name);
                }

                if (complex.IsArray) {
                    return typeof(XmlNodeList);
                }

                return typeof(XmlNode);
            }

            throw new PropertyAccessException("Invalid schema return type:" + item);
        }

        public static Type ToReturnType(XmlQualifiedName qname)
        {
            if (qname.Namespace == XMLConstants.W3C_XML_SCHEMA_NS_URI) {
                switch (qname.Name) {
                    case "ID":
                    case "string":
                        return typeof(string);

                    case "bool":
                        return typeof(bool?);

                    case "nonPositiveInteger":
                        return typeof(int?);

                    case "nonNegativeInteger":
                        return typeof(int?);

                    case "negativeInteger":
                        return typeof(int?);

                    case "positiveInteger":
                        return typeof(int?);

                    case "unsignedLong":
                        return typeof(ulong?);

                    case "unsignedInt":
                        return typeof(uint?);

                    case "int":
                    case "integer":
                        return typeof(int?);

                    case "short":
                        return typeof(short?);

                    case "unsignedShort":
                        return typeof(ushort?);

                    case "byte":
                        return typeof(byte?);

                    case "unsignedByte":
                        return typeof(byte?);

                    case "long":
                        return typeof(long?);

                    case "decimal":
                        return typeof(double?);

                    case "double":
                        return typeof(double?);

                    case "float":
                        return typeof(float?);

                    case "anyURI":
                        return typeof(XmlNode);

                    case "anySimpleType":
                        return typeof(object);
                }
            }

            return null;
        }

        public static Type ToReturnType(
            XmlSchemaSimpleType simpleType,
            string typeName)
        {
            if (typeName != null) {
                var type = TypeMap.Get(typeName);
                if (type != null) {
                    return type;
                }
            }

            var qualified = simpleType.QualifiedName;
            var returnType = ToReturnType(qualified);
            if (returnType != null) {
                return returnType;
            }

            var asRestrictedType = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            if (asRestrictedType != null) {
                if (asRestrictedType.BaseTypeName != null) {
                    var baseReturnType = ToReturnType(qualified = asRestrictedType.BaseTypeName);
                    if (baseReturnType != null) {
                        return baseReturnType;
                    }
                }
            }

            throw new EPException("Unable to convert qualified type '" + qualified + "' to return result");
        }

        /// <summary>
        ///     Returns the native type based on XPathConstants qname and an optional cast-to
        ///     type, if provided.
        /// </summary>
        /// <param name="resultType">qname</param>
        /// <param name="optionalCastToTypeName">Name of the optional cast to type.</param>
        /// <returns>return type</returns>
        public static Type ToReturnType(
            XPathResultType resultType,
            string optionalCastToTypeName)
        {
            Type optionalCastToType = null;
            if (optionalCastToTypeName != null) {
                optionalCastToType = TypeHelper.GetPrimitiveTypeForName(optionalCastToTypeName);
                if (optionalCastToType == null) {
                    optionalCastToType = TypeHelper.ResolveType(optionalCastToTypeName, false);
                }
            }

            return ToReturnType(resultType, optionalCastToType);
        }

        /// <summary>
        ///     Returns the native type based on XPathConstants qname and an optional cast-to
        ///     type, if provided.
        /// </summary>
        /// <param name="resultType">qname</param>
        /// <param name="optionalCastToType">null or cast-to type</param>
        /// <returns>
        ///     return type
        /// </returns>
        public static Type ToReturnType(
            XPathResultType resultType,
            Type optionalCastToType)
        {
            if (optionalCastToType != null) {
                return optionalCastToType;
            }

            if (resultType == XPathResultType.NodeSet) {
                return typeof(XmlNodeList);
            }

            if (resultType == XPathResultType.Any) {
                return typeof(XmlNode);
            }

            if (resultType == XPathResultType.Boolean) {
                return typeof(bool?);
            }

            if (resultType == XPathResultType.Number) {
                return typeof(double?);
            }

            if (resultType == XPathResultType.String) {
                return typeof(string);
            }

            return typeof(string);
        }

        public static XPathResultType SimpleTypeToResultType(XmlSchemaSimpleType definition)
        {
            var qname = definition.QualifiedName;
            if (qname.Namespace == XMLConstants.W3C_XML_SCHEMA_NS_URI) {
                switch (qname.Name) {
                    case "ID":
                    case "string":
                        return XPathResultType.String;

                    case "bool":
                    case "boolean":
                        return XPathResultType.Boolean;

                    case "nonPositiveInteger":
                    case "nonNegativeInteger":
                    case "negativeInteger":
                    case "positiveInteger":
                    case "unsignedLong":
                    case "unsignedInt":
                    case "integer":
                    case "short":
                    case "unsignedShort":
                    case "byte":
                    case "unsignedByte":
                    case "int":
                    case "long":
                    case "decimal":
                    case "double":
                    case "float":
                        return XPathResultType.Number;

                    case "anyURI":
                        return XPathResultType.Any;
                }
            }

            throw new EPException("Unable to convert qualified type '" + qname + "' to path result");
        }

        /// <summary>
        ///     Returns the XPathConstants type for a given Xerces type definition.
        /// </summary>
        /// <param name="definition">the schema element definition.</param>
        /// <returns>XPathConstants type</returns>
        public static XmlQualifiedName SimpleTypeToQName(XmlSchemaSimpleType definition)
        {
            return definition.QualifiedName;
        }

        public static SchemaElementComplex FindRootElement(
            SchemaModel schema,
            string @namespace,
            string elementName)
        {
            if (!string.IsNullOrEmpty(@namespace)) {
                foreach (var complexElement in schema.Components) {
                    if (complexElement.Namespace.Equals(@namespace) && complexElement.Name.Equals(elementName)) {
                        return complexElement;
                    }
                }
            }
            else {
                foreach (var complexElement in schema.Components) {
                    if (complexElement.Name.Equals(elementName)) {
                        return complexElement;
                    }
                }
            }

            if (elementName.StartsWith("//")) {
                elementName = elementName.Substring(2);
                foreach (var complexElement in schema.Components) {
                    var match = RecursiveDeepMatch(complexElement, @namespace, elementName);
                    if (match != null) {
                        return match;
                    }
                }
            }

            var text = "Could not find root element declaration in schema for element name '" + elementName + '\'';
            if (@namespace != null) {
                text = text + " in namespace '" + @namespace + '\'';
            }

            throw new EPException(text);
        }

        private static SchemaElementComplex RecursiveDeepMatch(
            SchemaElementComplex parent,
            string @namespace,
            string elementName)
        {
            if (!string.IsNullOrEmpty(@namespace)) {
                foreach (var complexElement in parent.ComplexElements) {
                    if (complexElement.Namespace.Equals(@namespace) && complexElement.Name.Equals(elementName)) {
                        return complexElement;
                    }
                }
            }
            else {
                foreach (var complexElement in parent.ComplexElements) {
                    if (complexElement.Name.Equals(elementName)) {
                        return complexElement;
                    }
                }
            }

            foreach (var complexElement in parent.ComplexElements) {
                var found = RecursiveDeepMatch(complexElement, @namespace, elementName);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }


        /// <summary>
        ///     Finds an apropiate definition for the given property, starting at the * given
        ///     definition. First look if the property es an attribute. If not, look at simple and
        ///     then child element definitions.
        /// </summary>
        /// <param name="def">the definition to start looking</param>
        /// <param name="property">the property to look for</param>
        /// <returns>
        ///     schema element or null if not found
        /// </returns>
        public static SchemaItem FindPropertyMapping(
            SchemaElementComplex def,
            string property)
        {
            foreach (var attribute in def.Attributes) {
                if (attribute.Name == property) {
                    return attribute;
                }
            }

            foreach (var simple in def.SimpleElements) {
                if (simple.Name == property) {
                    return simple;
                }
            }

            foreach (var complex in def.ComplexElements) {
                if (complex.Name == property) {
                    return complex;
                }
            }

            //property not found in schema
            return null;
        }

        /// <summary>
        ///     Serialize the given node.
        /// </summary>
        /// <param name="doc">node to serialize</param>
        /// <returns>
        ///     serialized node string
        /// </returns>
        public static string Serialize(XmlNode doc)
        {
            return doc.OuterXml;
        }
    }
}