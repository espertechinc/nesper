///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using Antlr4.Runtime.Tree.Xpath;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.propertyparser;
using com.espertech.esper.compat;
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
	/// <para />If an xsd is present, then use {@link com.espertech.esper.common.internal.event.xml.SchemaXMLEventType SchemaXMLEventType }
	/// </summary>
	/// <author>pablo</author>
	public class SimpleXMLEventType : BaseXMLEventType {
	    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly IDictionary<string, EventPropertyGetterSPI> propertyGetterCache;
	    private string defaultNamespacePrefix;
	    private readonly bool isResolvePropertiesAbsolute;

	    public SimpleXMLEventType(
	        EventTypeMetadata eventTypeMetadata,
	        ConfigurationCommonEventTypeXMLDOM configurationEventTypeXMLDOM,
	        EventBeanTypedEventFactory eventBeanTypedEventFactory, 
	        EventTypeNameResolver eventTypeResolver, 
	        XMLFragmentEventTypeFactory xmlEventTypeFactory)
	        : base(eventTypeMetadata, configurationEventTypeXMLDOM, eventBeanTypedEventFactory, eventTypeResolver, xmlEventTypeFactory)
	    {
	        isResolvePropertiesAbsolute = configurationEventTypeXMLDOM.IsXPathResolvePropertiesAbsolute;
	        propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();

	        // Set of namespace context for XPath expressions
	        XPathNamespaceContext xPathNamespaceContext = new XPathNamespaceContext();
	        foreach (KeyValuePair<string, string> entry in configurationEventTypeXMLDOM.NamespacePrefixes) {
	            xPathNamespaceContext.AddNamespace(entry.Key, entry.Value);
	        }
	        if (configurationEventTypeXMLDOM.DefaultNamespace != null) {
	            string defaultNamespace = configurationEventTypeXMLDOM.DefaultNamespace;
	            xPathNamespaceContext.SetDefaultNamespace(defaultNamespace);

	            // determine a default namespace prefix to use to construct XPath expressions from pure property names
	            defaultNamespacePrefix = null;
	            foreach (KeyValuePair<string, string> entry in configurationEventTypeXMLDOM.NamespacePrefixes) {
	                if (entry.Value.Equals(defaultNamespace)) {
	                    defaultNamespacePrefix = entry.Key;
	                    break;
	                }
	            }
	        }
	        base.NamespaceContext = xPathNamespaceContext;
	            base.Initialize(configurationEventTypeXMLDOM.XPathProperties.Values, Collections.GetEmptyList<ExplicitPropertyDescriptor>());
	    }

	    protected override Type DoResolvePropertyType(string propertyExpression) {
	        return ResolveSimpleXMLPropertyType(propertyExpression);
	    }

	    protected override EventPropertyGetterSPI DoResolvePropertyGetter(string propertyExpression) {
	        EventPropertyGetterSPI getter = propertyGetterCache.Get(propertyExpression);
	        if (getter != null) {
	            return getter;
	        }

	        getter = ResolveSimpleXMLPropertyGetter(propertyExpression, this, defaultNamespacePrefix, isResolvePropertiesAbsolute);

	        // no fragment factory, fragments not allowed
	        propertyGetterCache.Put(propertyExpression, getter);
	        return getter;
	    }

	    protected override FragmentEventType DoResolveFragmentType(string property) {
	        return null;  // Since we have no type information, the fragments are not allowed unless explicitly configured via XPath getter
	    }

	    public static Type ResolveSimpleXMLPropertyType(string propertyExpression) {
	        Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
	        if (PropertyParser.IsPropertyDynamic(prop)) {
	            return typeof(XmlNode);
	        } else {
	            return typeof(string);
	        }
	    }

	    public static EventPropertyGetterSPI ResolveSimpleXMLPropertyGetter(string propertyExpression, BaseXMLEventType baseXMLEventType, string defaultNamespacePrefix, bool isResolvePropertiesAbsolute) {
	        if (!baseXMLEventType.ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr) {
	            Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
	            EventPropertyGetterSPI getter = prop.GetterDOM;
	            if (!prop.IsDynamic) {
	                getter = new DOMConvertingGetter((DOMPropertyGetter) getter, typeof(string));
	            }
	            return getter;
	        }

	        XPathExpression xPathExpression;
	        string xPathExpr;
	        bool isDynamic;
	        try {
	            Property property = PropertyParserNoDep.ParseAndWalkLaxToSimple(propertyExpression, false);
	            isDynamic = PropertyParser.IsPropertyDynamic(property);

	            xPathExpr = SimpleXMLPropertyParser.Walk(property, baseXMLEventType.RootElementName, defaultNamespacePrefix, isResolvePropertiesAbsolute);
	            var xpath = baseXMLEventType.XPathFactory.NewXPath();
	            xpath.NamespaceContext = baseXMLEventType.namespaceContext;
	            if (log.IsInfoEnabled) {
	                log.Info("Compiling XPath expression for property '" + propertyExpression + "' as '" + xPathExpr + "'");
	            }
	            xPathExpression = xpath.Compile(xPathExpr);
	        } catch (XPathExpressionException e) {
	            throw new EPException("Error constructing XPath expression from property name '" + propertyExpression + '\'', e);
	        }

	        XPathResultType xPathReturnType;
	        if (isDynamic) {
	            xPathReturnType = XPathResultType.Any;
	        } else {
	            xPathReturnType = XPathResultType.String;
	        }
	        return new XPathPropertyGetter(baseXMLEventType, propertyExpression, xPathExpr, xPathExpression, xPathReturnType, null, null);
	    }
	}
} // end of namespace