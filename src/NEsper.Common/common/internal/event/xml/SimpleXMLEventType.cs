///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.propertyparser;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    /// Optimistic try to resolve the property string into an appropiate xPath,
    /// and use it as getter.
    /// Mapped and Indexed properties supported.
    /// Because no type information is given, all property are resolved to String.
    /// No namespace support.
    /// Cannot access to xml attributes, only elements content.
    /// <para />If an xsd is present, then use {@link com.espertech.esper.common.@internal.@event.xml.SchemaXMLEventType SchemaXMLEventType }
    /// </summary>
    /// <author>pablo</author>
    public class SimpleXMLEventType : BaseXMLEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool isResolvePropertiesAbsolute;
        private readonly IDictionary<string, EventPropertyGetterSPI> propertyGetterCache;
        private readonly string defaultNamespacePrefix;

        public SimpleXMLEventType(
            EventTypeMetadata eventTypeMetadata,
            ConfigurationCommonEventTypeXMLDOM configurationEventTypeXMLDOM,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNameResolver eventTypeResolver,
            XMLFragmentEventTypeFactory xmlEventTypeFactory) : base(
            eventTypeMetadata,
            configurationEventTypeXMLDOM,
            eventBeanTypedEventFactory,
            eventTypeResolver,
            xmlEventTypeFactory)
        {
            isResolvePropertiesAbsolute = configurationEventTypeXMLDOM.IsXPathResolvePropertiesAbsolute;
            propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();

            // Set of namespace context for XPath expressions
            var xPathNamespaceContext = new XPathNamespaceContext();
            foreach (var entry in configurationEventTypeXMLDOM.NamespacePrefixes) {
                xPathNamespaceContext.AddNamespace(entry.Key, entry.Value);
            }

            if (configurationEventTypeXMLDOM.DefaultNamespace != null) {
                var defaultNamespace = configurationEventTypeXMLDOM.DefaultNamespace;
                xPathNamespaceContext.SetDefaultNamespace(defaultNamespace);

                // determine a default namespace prefix to use to construct XPath expressions from pure property names
                defaultNamespacePrefix = null;
                foreach (var entry in configurationEventTypeXMLDOM.NamespacePrefixes) {
                    if (entry.Value.Equals(defaultNamespace)) {
                        defaultNamespacePrefix = entry.Key;
                        break;
                    }
                }
            }

            NamespaceContext = xPathNamespaceContext;
            Initialize(
                configurationEventTypeXMLDOM.XPathProperties.Values,
                EmptyList<ExplicitPropertyDescriptor>.Instance);
        }

        protected override Type DoResolvePropertyType(string propertyExpression)
        {
            return ResolveSimpleXMLPropertyType(propertyExpression);
        }

        protected override EventPropertyGetterSPI DoResolvePropertyGetter(string propertyExpression)
        {
            var getter = propertyGetterCache.Get(propertyExpression);
            if (getter != null) {
                return getter;
            }

            getter = ResolveSimpleXMLPropertyGetter(
                propertyExpression,
                this,
                defaultNamespacePrefix,
                isResolvePropertiesAbsolute);

            // no fragment factory, fragments not allowed
            propertyGetterCache.Put(propertyExpression, getter);
            return getter;
        }

        protected override FragmentEventType DoResolveFragmentType(string property)
        {
            return
                null; // Since we have no type information, the fragments are not allowed unless explicitly configured via XPath getter
        }

        public static Type ResolveSimpleXMLPropertyType(string propertyExpression)
        {
            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
            if (PropertyParser.IsPropertyDynamic(prop)) {
                return typeof(XmlNode);
            }
            else {
                return typeof(string);
            }
        }

        public static EventPropertyGetterSPI ResolveSimpleXMLPropertyGetter(
            string propertyExpression,
            BaseXMLEventType baseXMLEventType,
            string defaultNamespacePrefix,
            bool isResolvePropertiesAbsolute)
        {
            if (!baseXMLEventType.ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                var getter = prop.GetterDOM;
                if (!prop.IsDynamic) {
                    getter = new DOMConvertingGetter((DOMPropertyGetter)getter, typeof(string));
                }

                return getter;
            }

            XPathExpression xPathExpression;
            string xPathExpr;
            bool isDynamic;
            try {
                var property = PropertyParserNoDep.ParseAndWalkLaxToSimple(propertyExpression, false);
                isDynamic = PropertyParser.IsPropertyDynamic(property);

                xPathExpr = SimpleXMLPropertyParser.Walk(
                    property,
                    baseXMLEventType.RootElementName,
                    defaultNamespacePrefix,
                    isResolvePropertiesAbsolute);

                if (Log.IsInfoEnabled) {
                    Log.Info(
                        "Compiling XPath expression for property '" + propertyExpression + "' as '" + xPathExpr + "'");
                }

                xPathExpression = baseXMLEventType.CreateXPath(xPathExpr);
            }
            catch (XPathException e) {
                throw new EPException(
                    "Error constructing XPath expression from property name '" + propertyExpression + '\'',
                    e);
            }

            var xPathReturnType = isDynamic ? XPathResultType.Any : XPathResultType.String;

            return new XPathPropertyGetter(
                baseXMLEventType,
                propertyExpression,
                xPathExpr,
                xPathExpression,
                xPathReturnType,
                null,
                null);
        }
    }
} // end of namespace