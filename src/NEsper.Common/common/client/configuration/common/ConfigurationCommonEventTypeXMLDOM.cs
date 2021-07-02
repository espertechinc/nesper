///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml.XPath;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Configuration object for enabling the runtime to process events represented as XML DOM document nodes.
    ///     <para>
    ///     Use this class to configure the runtime for processing of XML DOM objects that represent events
    ///     and contain all the data for event properties used by statements.
    ///     </para>
    ///     <para>
    ///     Minimally required is the root element name which allows the runtime to map the document
    ///     to the event type that has been named in an EPL or pattern statement.
    ///     </para>
    ///     <para>
    ///     Event properties that are results of XPath expressions can be made known to the runtime via this class.
    ///     For XPath expressions that must refer to namespace prefixes those prefixes and their
    ///     namespace name must be supplied to the runtime. A default namespace can be supplied as well.
    ///     </para>
    ///     <para>
    ///     By supplying a schema resource the runtime can interrogate the schema, allowing the runtime to
    ///     verify event properties and return event properties in the type defined by the schema.
    ///     When a schema resource is supplied, the optional root element namespace defines the namespace in case the
    ///     root element name occurs in multiple namespaces.
    ///     </para>
    /// </summary>
    [Serializable]
    public partial class ConfigurationCommonEventTypeXMLDOM
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeXMLDOM()
        {
            XPathProperties = new LinkedHashMap<string, XPathPropertyDesc>();
            NamespacePrefixes = new Dictionary<string, string>();
            IsXPathResolvePropertiesAbsolute = true;
            IsXPathPropertyExpr = false;
            IsEventSenderValidatesRoot = true;
            IsAutoFragment = true;
        }

        /// <summary>
        ///     Returns the root element name.
        /// </summary>
        /// <returns>root element name</returns>
        public string RootElementName { get; set; }

        /// <summary>
        ///     Returns the root element namespace.
        /// </summary>
        /// <returns>root element namespace</returns>
        public string RootElementNamespace { get; set; }

        /// <summary>
        ///     Returns the default namespace.
        /// </summary>
        /// <returns>default namespace</returns>
        public string DefaultNamespace { get; set; }

        /// <summary>
        ///     Returns the schema resource.
        /// </summary>
        /// <returns>schema resource</returns>
        public string SchemaResource { get; set; }

        /// <summary>
        ///     Returns the schema text, if provided instead of a schema resource, this call returns the actual text of the schema
        ///     document.
        ///     <para />
        ///     Set a schema text first. This call will not resolve the schema resource to a text.
        /// </summary>
        /// <returns>schema text, if provided, or null value</returns>
        public string SchemaText { get; set; }

        /// <summary>
        ///     Returns a map of property name and descriptor for XPath-expression properties.
        /// </summary>
        /// <value>XPath property information</value>
        public IDictionary<string, XPathPropertyDesc> XPathProperties { get; set;  }

        /// <summary>
        ///     Returns false to indicate that property expressions are evaluated by the DOM-walker
        ///     implementation (the default), or true to  indicate that property expressions are rewritten into XPath expressions.
        /// </summary>
        /// <value>indicator how property expressions are evaluated</value>
        public bool IsXPathPropertyExpr { get; set; }

        /// <summary>
        ///     Returns true to indicate that an <seealso cref="EventSender" /> returned for this event type validates
        ///     the root document element name against the one configured (the default), or false to not validate the root document
        ///     element name as configured.
        /// </summary>
        /// <value>true for validation of root document element name by event sender, false for no validation</value>
        public bool IsEventSenderValidatesRoot { get; set; }

        /// <summary>
        ///     Set to true (the default) to look up or create event types representing fragments of an XML document
        ///     automatically upon request for fragment event type information; Or false when only explicit
        ///     properties may return fragments.
        /// </summary>
        /// <value>indicator whether to allow splitting-up (fragmenting) properties (nodes) in an document</value>
        public bool IsAutoFragment { get; set; }

        /// <summary>
        ///     Returns the namespace prefixes in a map of prefix as key and namespace name as value.
        /// </summary>
        /// <value>namespace prefixes</value>
        public IDictionary<string, string> NamespacePrefixes { get; set; }

        /// <summary>
        ///     When set to true (the default), indicates that when properties are compiled to XPath expressions that the
        ///     compilation should generate an absolute XPath expression such as "/getQuote/request" for the
        ///     simple request property, or "/getQuote/request/symbol" for a "request.symbol" nested property,
        ///     wherein the root element node is "getQuote".
        ///     <para />
        ///     When set to false, indicates that when properties are compiled to XPath expressions that the
        ///     compilation should generate a deep XPath expression such as "//symbol" for the
        ///     simple symbol property, or "//request/symbol" for a "request.symbol" nested property.
        /// </summary>
        /// <value>true for absolute XPath for properties (default), false for deep XPath</value>
        public bool IsXPathResolvePropertiesAbsolute { set; get; }

        /// <summary>
        ///     Returns the class name of the XPath function resolver to be assigned to the XPath factory instance
        ///     upon type initialization.
        /// </summary>
        /// <returns>class name of xpath function resolver, or null if none set</returns>
        public string XPathFunctionResolver { get; set; }

        /// <summary>
        ///     Returns the class name of the XPath variable resolver to be assigned to the XPath factory instance
        ///     upon type initialization.
        /// </summary>
        /// <returns>class name of xpath function resolver, or null if none set</returns>
        public string XPathVariableResolver { get; set; }

        /// <summary>
        ///     Returns the property name of the property providing the start timestamp value.
        /// </summary>
        /// <returns>start timestamp property name</returns>
        public string StartTimestampPropertyName { get; set; }

        /// <summary>
        ///     Returns the property name of the property providing the end timestamp value.
        /// </summary>
        /// <returns>end timestamp property name</returns>
        public string EndTimestampPropertyName { get; set; }

        /// <summary>
        ///     Adds an event property for which the runtime uses the supplied XPath expression against
        ///     a DOM document node to resolve a property value.
        /// </summary>
        /// <param name="name">of the event property</param>
        /// <param name="xpath">is an arbitrary xpath expression</param>
        /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
        public void AddXPathProperty(
            string name,
            string xpath,
            XPathResultType type)
        {
            var desc = new XPathPropertyDesc(name, xpath, type);
            XPathProperties.Put(name, desc);
        }

        /// <summary>
        ///     Adds an event property for which the runtime uses the supplied XPath expression against
        ///     a DOM document node to resolve a property value.
        /// </summary>
        /// <param name="name">of the event property</param>
        /// <param name="xpath">is an arbitrary xpath expression</param>
        /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
        /// <param name="castToType">is the type name of the type that the return value of the xpath expression is casted to</param>
        public void AddXPathProperty(
            string name,
            string xpath,
            XPathResultType type,
            string castToType)
        {
            Type castToTypeClass = null;

            if (castToType != null) {
                var isArray = false;
                if (castToType.Trim().EndsWith("[]")) {
                    isArray = true;
                    castToType = castToType.Replace("[]", "");
                }

                castToTypeClass = TypeHelper.GetTypeForSimpleName(castToType, ClassForNameProviderDefault.INSTANCE);
                if (castToTypeClass == null) {
                    throw new ConfigurationException(
                        "Invalid cast-to type for xpath expression named '" + name + "', the type is not recognized");
                }

                if (isArray) {
                    castToTypeClass = castToTypeClass.MakeArrayType();
                }
            }

            var desc = new XPathPropertyDesc(name, xpath, type, castToTypeClass);
            XPathProperties.Put(name, desc);
        }

        /// <summary>
        ///     Adds an event property for which the runtime uses the supplied XPath expression against
        ///     a DOM document node to resolve a property value.
        /// </summary>
        /// <param name="name">of the event property</param>
        /// <param name="xpath">is an arbitrary xpath expression</param>
        /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
        /// <param name="eventTypeName">is the name of another event type that represents the XPath nodes</param>
        public void AddXPathPropertyFragment(
            string name,
            string xpath,
            XPathResultType type,
            string eventTypeName)
        {
            if ((type != XPathResultType.Any) && (type != XPathResultType.NodeSet)) {
                throw new ArgumentException(
                    "XPath property for fragments requires an XmlNode or XmlNodeset return value for property '" +
                    name +
                    "'");
            }

            var desc = new XPathPropertyDesc(name, xpath, type, eventTypeName);
            XPathProperties.Put(name, desc);
        }

        /// <summary>
        ///     Add a prefix and namespace name for use in XPath expressions refering to that prefix.
        /// </summary>
        /// <param name="prefix">is the prefix of the namespace</param>
        /// <param name="namespace">is the namespace name</param>
        public void AddNamespacePrefix(
            string prefix,
            string @namespace)
        {
            NamespacePrefixes.Put(prefix, @namespace);
        }

        /// <summary>
        ///     Add prefixes and namespace names for use in XPath expressions refering to that prefix.
        /// </summary>
        /// <param name="prefixNamespaceMap">map of prefixes and namespaces</param>
        public void AddNamespacePrefixes(IDictionary<string, string> prefixNamespaceMap)
        {
            NamespacePrefixes.PutAll(prefixNamespaceMap);
        }

        public CodegenExpression ToExpression(
            CodegenMethodScope parent,
            CodegenClassScope scope)
        {
            CodegenSetterBuilderItemConsumer<XPathPropertyDesc> xPathBuild = (o, parentXPath, scopeXPath) => 
                o.ToCodegenExpression(parentXPath, scopeXPath);

            return new CodegenSetterBuilder(typeof(ConfigurationCommonEventTypeXMLDOM), typeof(ConfigurationCommonEventTypeXMLDOM), "xmlconfig", parent, scope)
                .ConstantExplicit("RootElementName", RootElementName)
                .Map("XPathProperties", XPathProperties, xPathBuild)
                .MapOfConstants("NamespacePrefixes", NamespacePrefixes)
                .ConstantExplicit("SchemaResource", SchemaResource)
                .ConstantExplicit("SchemaText", SchemaText)
                .ConstantExplicit("IsEventSenderValidatesRoot", IsEventSenderValidatesRoot)
                .ConstantExplicit("IsAutoFragment", IsAutoFragment)
                .ConstantExplicit("IsXPathPropertyExpr", IsXPathPropertyExpr)
                .ConstantExplicit("XPathFunctionResolver", XPathFunctionResolver)
                .ConstantExplicit("XPathVariableResolver", XPathVariableResolver)
                .ConstantExplicit("IsXPathResolvePropertiesAbsolute", IsXPathResolvePropertiesAbsolute)
                .ConstantExplicit("DefaultNamespace", DefaultNamespace)
                .ConstantExplicit("RootElementNamespace", RootElementNamespace)
                .ConstantExplicit("StartTimestampPropertyName", StartTimestampPropertyName)
                .ConstantExplicit("EndTimestampPropertyName", EndTimestampPropertyName)
                .Build();
        }

        public override bool Equals(object otherObj)
        {
            if (!(otherObj is ConfigurationCommonEventTypeXMLDOM)) {
                return false;
            }

            var other = (ConfigurationCommonEventTypeXMLDOM) otherObj;
            if (!other.RootElementName.Equals(RootElementName)) {
                return false;
            }

            if (other.RootElementNamespace == null && RootElementNamespace != null ||
                other.RootElementNamespace != null && RootElementNamespace == null) {
                return false;
            }

            if (other.RootElementNamespace != null && RootElementNamespace != null) {
                return RootElementNamespace.Equals(other.RootElementNamespace);
            }

            return true;
        }

        public override int GetHashCode()
        {
            return RootElementName.GetHashCode();
        }
    }
} // end of namespace