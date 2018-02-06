///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Parses event property names and transforms to XPath expressions using the schema
    /// information supplied. Supports the nested, indexed and mapped event properties.
    /// </summary>
    public class SchemaXMLPropertyParser
    {
        /// <summary>
        /// Return the xPath corresponding to the given property. The PropertyName String
        /// may be simple, nested, indexed or mapped.
        /// </summary>
        /// <param name="propertyName">is the event property name</param>
        /// <param name="namespace">is the default namespace</param>
        /// <param name="schemaModel">is the schema model</param>
        /// <param name="xPathContext">is the xpath factory instance to use</param>
        /// <param name="rootElementName">is the name of the root element</param>
        /// <param name="eventAdapterService">for type lookup and creation</param>
        /// <param name="xmlEventType">the resolving type</param>
        /// <param name="isAllowFragment">whether fragmenting is allowed</param>
        /// <param name="defaultNamespace">default namespace</param>
        /// <returns>
        /// xpath expression
        /// </returns>
        /// <throws>EPException is there are XPath errors</throws>
        public static EventPropertyGetterSPI GetXPathResolution(
            String propertyName,
            XPathNamespaceContext xPathContext,
            String rootElementName,
            String @namespace,
            SchemaModel schemaModel,
            EventAdapterService eventAdapterService,
            BaseXMLEventType xmlEventType,
            bool isAllowFragment,
            String defaultNamespace)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Determining XPath expression for property '" + propertyName + "'");
            }

            var ctx = new XPathNamespaceContext();
            var namespaces = schemaModel.Namespaces;

            string defaultNamespacePrefix = null;
            for (int i = 0; i < namespaces.Count; i++)
            {
                var namespacePrefix = "n" + i;
                ctx.AddNamespace(namespacePrefix, namespaces[i]);
                if ((defaultNamespace != null) && (defaultNamespace == namespaces[i]))
                {
                    defaultNamespacePrefix = namespacePrefix;
                }
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            var isDynamic = property.IsDynamic;

            var rootComplexElement = SchemaUtil.FindRootElement(schemaModel, @namespace, rootElementName);
            string prefix = ctx.LookupPrefix(rootComplexElement.Namespace);
            if (prefix == null)
            {
                prefix = "";
            }
            else
            {
                prefix += ':';
            }

            var xPathBuf = new StringBuilder();
            xPathBuf.Append('/');
            xPathBuf.Append(prefix);
            if (rootElementName.StartsWith("//"))
            {
                xPathBuf.Append(rootElementName.Substring(2));
            }
            else
            {
                xPathBuf.Append(rootElementName);
            }

            var parentComplexElement = rootComplexElement;
            Pair<String, XPathResultType> pair = null;

            if (!(property is NestedProperty))
            {
                pair = MakeProperty(rootComplexElement, property, ctx, true, isDynamic, defaultNamespacePrefix);
                if (pair == null)
                {
                    throw new PropertyAccessException("Failed to locate property '" + propertyName + "' in schema");
                }
                xPathBuf.Append(pair.First);
            }
            else
            {
                NestedProperty nestedProperty = (NestedProperty)property;
                int indexLast = nestedProperty.Properties.Count - 1;

                for (int i = 0; i < indexLast + 1; i++)
                {
                    var isLast = i == indexLast;
                    var propertyNested = nestedProperty.Properties[i];
                    pair = MakeProperty(parentComplexElement, propertyNested, ctx, isLast, isDynamic, defaultNamespacePrefix);
                    if (pair == null)
                    {
                        throw new PropertyAccessException("Failed to locate property '" + propertyName + "' nested property part '" + property.PropertyNameAtomic + "' in schema");
                    }

                    var text = propertyNested.PropertyNameAtomic;
                    var obj = SchemaUtil.FindPropertyMapping(parentComplexElement, text);
                    if (obj is SchemaElementComplex)
                    {
                        parentComplexElement = (SchemaElementComplex)obj;
                    }
                    xPathBuf.Append(pair.First);
                }
            }

            var xPath = xPathBuf.ToString();
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".parse XPath for property '" + propertyName + "' is expression=" + xPath);
            }

            // Compile assembled XPath expression
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Compiling XPath expression '" + xPath + "' for property '" + propertyName + "' using namespace context :" + ctx);
            }

            XPathExpression expr;
            try
            {
                expr = XPathExpression.Compile(xPath, ctx);
            }
            catch (XPathException e)
            {
                String detail = "Error constructing XPath expression from property expression '" + propertyName + "' expression '" + xPath + "'";
                if (e.Message != null)
                {
                    throw new EPException(detail + " :" + e.Message, e);
                }
                throw new EPException(detail, e);
            }

            // get type
            var item = property.GetPropertyTypeSchema(rootComplexElement, eventAdapterService);
            if ((item == null) && (!isDynamic))
            {
                return null;
            }

            var resultType = isDynamic ? typeof(XmlNode) : SchemaUtil.ToReturnType(item);

            FragmentFactory fragmentFactory = null;
            if (isAllowFragment)
            {
                fragmentFactory = new FragmentFactoryDOMGetter(eventAdapterService, xmlEventType, propertyName);
            }

            return new XPathPropertyGetter(propertyName, xPath, expr, pair.Second, resultType, fragmentFactory);
        }

        private static Pair<String, XPathResultType> MakeProperty(SchemaElementComplex parent, Property property, XPathNamespaceContext ctx, bool isLast, bool isDynamic, String defaultNamespacePrefix)
        {
            var text = property.PropertyNameAtomic;
            var obj = SchemaUtil.FindPropertyMapping(parent, text);
            if ((obj is SchemaElementSimple) || (obj is SchemaElementComplex))
            {
                return MakeElementProperty((SchemaElement)obj, property, ctx, isLast, isDynamic, defaultNamespacePrefix);
            }
            if (obj != null)
            {
                return MakeAttributeProperty((SchemaItemAttribute)obj, property, ctx);
            }
            if (isDynamic)
            {
                return MakeElementProperty(null, property, ctx, isLast, isDynamic, defaultNamespacePrefix);
            }

            return null;
        }

        private static Pair<String, XPathResultType> MakeAttributeProperty(SchemaItemAttribute attribute, Property property, XPathNamespaceContext ctx)
        {
            var prefix = ctx.LookupPrefix(attribute.Namespace);
            if (String.IsNullOrEmpty(prefix))
            {
                prefix = "";
            }
            else
            {
                prefix += ':';
            }

            if (IsAnySimpleProperty(property))
            {
                XPathResultType type = SchemaUtil.SimpleTypeToResultType(attribute.SimpleType);
                String path = "/@" + prefix + property.PropertyNameAtomic;
                return new Pair<String, XPathResultType>(path, type);
            }

            if (IsAnyMappedProperty(property))
            {
                throw new Exception("Mapped properties not applicable to attributes");
            }

            throw new Exception("Indexed properties not applicable to attributes");
        }

        private static Pair<String, XPathResultType> MakeElementProperty(SchemaElement schemaElement, Property property, XPathNamespaceContext ctx, bool isAlone, bool isDynamic, String defaultNamespacePrefix)
        {
            XPathResultType type;
            if (isDynamic)
            {
                type = XPathResultType.Any;
            }
            else if (schemaElement is SchemaElementSimple)
            {
                var element = (SchemaElementSimple)schemaElement;
                type = SchemaUtil.SimpleTypeToResultType(element.SimpleType);
            }
            else
            {
                var complex = (SchemaElementComplex)schemaElement;
                type = XPathResultType.Any;
                //if (complex.OptionalSimpleType != null)
                //{
                //    type = SchemaUtil.SimpleTypeToQName(complex.OptionalSimpleType);
                //}
                //else
                //{
                //    // The result is a node
                //    type = XPathResultType.Any;
                //}
            }

            var prefix = isDynamic ? defaultNamespacePrefix : ctx.LookupPrefix(schemaElement.Namespace);
            if (String.IsNullOrEmpty(prefix))
            {
                prefix = String.Empty;
            }
            else
            {
                prefix += ':';
            }

            if (IsAnySimpleProperty(property))
            {
                if (!isDynamic && schemaElement.IsArray && !isAlone)
                {
                    throw new PropertyAccessException("Simple property not allowed in repeating elements at '" + schemaElement.Name + "'");
                }
                return new Pair<String, XPathResultType>('/' + prefix + property.PropertyNameAtomic, type);
            }

            if (IsAnyMappedProperty(property))
            {
                if (!isDynamic && !schemaElement.IsArray)
                {
                    throw new PropertyAccessException("Element " + property.PropertyNameAtomic + " is not a collection, cannot be used as mapped property");
                }
                String key = GetMappedPropertyKey(property);
                return new Pair<String, XPathResultType>('/' + prefix + property.PropertyNameAtomic + "[@id='" + key + "']", type);
            }

            if (!isDynamic && !schemaElement.IsArray)
            {
                throw new PropertyAccessException("Element " + property.PropertyNameAtomic + " is not a collection, cannot be used as mapped property");
            }
            int index = GetIndexedPropertyIndex(property);
            int xPathPosition = index + 1;
            return new Pair<String, XPathResultType>('/' + prefix + property.PropertyNameAtomic + "[position() = " + xPathPosition + ']', type);
        }

        private static bool IsAnySimpleProperty(Property property)
        {
            return property is SimpleProperty || property is DynamicSimpleProperty;
        }

        private static bool IsAnyMappedProperty(Property property)
        {
            return property is MappedProperty || property is DynamicMappedProperty;
        }

        private static int GetIndexedPropertyIndex(Property property)
        {
            return property is IndexedProperty ? ((IndexedProperty)property).Index : ((DynamicIndexedProperty)property).Index;
        }

        private static String GetMappedPropertyKey(Property property)
        {
            return property is MappedProperty ? ((MappedProperty)property).Key : ((DynamicMappedProperty)property).Key;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
