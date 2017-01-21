///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml.XPath;

using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration object for enabling the engine to process events represented as XML DOM document nodes.
    /// <para/>
    /// Use this class to configure the engine for processing of XML DOM objects that represent events
    /// and contain all the data for event properties used by statements.
    /// <para/>
    /// Minimally required is the root element name which allows the engine to map the document
    /// to the event type that has been named in an EPL or pattern statement.
    /// <para/>
    /// Event properties that are results of XPath expressions can be made known to the engine via this class.
    /// For XPath expressions that must refer to namespace prefixes those prefixes and their
    /// namespace name must be supplied to the engine. A default namespace can be supplied as well.
    /// <para/>
    /// By supplying a schema resource the engine can interrogate the schema, allowing the engine to
    /// verify event properties and return event properties in the type defined by the schema.
    /// When a schema resource is supplied, the optional root element namespace defines the namespace in case the
    /// root element name occurs in multiple namespaces.
    /// </summary>

    [Serializable]
    public class ConfigurationEventTypeXMLDOM : MetaDefItem
    {
        // Root element namespace.
        // Used to find root element in schema. Useful and required in the case where the root element exists in
        // multiple namespaces.

        // Default name space.
        // For XPath expression evaluation.

        private readonly IDictionary<String, String> _namespacePrefixes;

        /// <summary> Gets or sets the root element name.</summary>
        public string RootElementName { get; set; }

        /// <summary> Gets or sets the root element namespace.</summary>
        public string RootElementNamespace { get; set; }

        /// <summary> Gets or sets the default namespace.</summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        /// Gets or sets  the schema resource.
        /// </summary>
        public string SchemaResource { get; set; }

        /// <summary>
        /// Gets or sets the schema text.  If provided instead of a schema resource, this 
        /// returns the actual text of the schema document.
        /// <para/>
        /// Set a schema text first. This will not resolve the schema resource to a text.
        /// </summary>
        /// <value>The schema text.</value>
        public string SchemaText { get; set; }

        /// <summary>
        /// Indicates whether or not that property expressions are evaluated by the DOM-walker implementation
        /// (the default), or true to  indicate that property expressions are rewritten into XPath expressions.
        /// </summary>
        public bool IsXPathPropertyExpr { get; set; }

        /// <summary>
        /// When set to true (the default), indicates that when properties are compiled to XPath expressions that the
        /// compilation should generate an absolute XPath expression such as "/getQuote/request" for the simple request
        /// property, or "/getQuote/request/symbol" for a "request.symbol" nested property, wherein the root element
        /// node is "getQuote".
        /// <para/>
        /// When set to false, indicates that when properties are compiled to XPath expressions that the compilation
        /// should generate a deep XPath expression such as "//symbol" for the simple symbol property, or "//request/symbol"
        /// for a "request.symbol" nested property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is X path resolve properties absolute; otherwise, <c>false</c>.
        /// </value>
        public bool IsXPathResolvePropertiesAbsolute { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates that an <seealso cref="EventSender"/> returned for this event type
        /// validates the root document element name against the one configured (the default), or false
        /// to not validate the root document element name as configured.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is event sender validates root; otherwise, <c>false</c>.
        /// </value>
        public bool IsEventSenderValidatesRoot { get; set; }

        /// <summary>
        /// Set to true (the default) to look up or create event types representing fragments of an XML
        /// document automatically upon request for fragment event type information; Or false when only
        /// explicit properties may return fragments.
        /// </summary>
        public bool IsAutoFragment { get; set; }

        /// <summary>
        /// Indicator for use with EsperHA, false by default to indicate that stored type
        /// information takes Precedence over configuration type information provided at
        /// engine initialization time. Set to true to indicate that configuration type 
        /// information takes Precedence over stored type information.
        /// <para/>
        /// When setting this flag to true care should be taken about the compatibility 
        /// of the supplied XML type configuration information and the existing EPL 
        /// statements and stored events, if any. For more information please consult 
        /// <see cref="ConfigurationOperations.ReplaceXMLEventType" />.
        /// </summary>
        /// <value>set to false (the default) to indicate that stored type information takes Precedence over configuration type information</value>
        public bool IsUpdateStoredType { get; set; }

        /// <summary>
        /// Gets or sets the property name of the property providing the start timestamp value.
        /// </summary>
        /// <value>The start name of the timestamp property.</value>
        public string StartTimestampPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the property name of the property providing the end timestamp value.
        /// </summary>
        /// <value>The end name of the timestamp property.</value>
        public string EndTimestampPropertyName { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>

        public ConfigurationEventTypeXMLDOM()
        {
            XPathProperties = new LinkedHashMap<String, XPathPropertyDesc>();
            _namespacePrefixes = new Dictionary<String, String>();
            IsXPathResolvePropertiesAbsolute = true;
            IsXPathPropertyExpr = false;
            IsEventSenderValidatesRoot = true;
            IsAutoFragment = true;
        }

        /// <summary> Returns a map of property name and descriptor for XPath-expression properties.</summary>
        /// <returns> XPath property information
        /// </returns>
        public IDictionary<string, XPathPropertyDesc> XPathProperties { get; private set; }

        /// <summary>
        /// Adds an event property for which the engine uses the supplied XPath expression against
        /// a DOM document node to resolve a property value.
        /// </summary>
        /// <param name="name">name of the event property</param>
        /// <param name="xpath">is an arbitrary xpath expression</param>
        /// <param name="type">is the return type of the expression</param>

        public void AddXPathProperty(String name, String xpath, XPathResultType type)
        {
            XPathPropertyDesc desc = new XPathPropertyDesc(name, xpath, type);
            XPathProperties[name] = desc;
        }

        /// <summary>
        /// Adds an event property for which the engine uses the supplied XPath expression againsta DOM document node to resolve a property value.
        /// </summary>
        /// <param name="name">the event property</param>
        /// <param name="xpath">an arbitrary xpath expression</param>
        /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
        /// <param name="castToType">is the type name of the type that the return value of the xpath expression is casted to</param>
        public void AddXPathProperty(String name, String xpath, XPathResultType type, String castToType)
        {
            Type castToTypeClass = null;

            if (castToType != null)
            {
                bool isArray = false;
                if (castToType.Trim().EndsWith("[]"))
                {
                    isArray = true;
                    castToType = castToType.Replace("[]", "");
                }

                castToTypeClass = TypeHelper.GetTypeForSimpleName(castToType);
                if (castToTypeClass == null)
                {
                    throw new ConfigurationException("Invalid cast-to type for xpath expression named '" + name + "', the type is not recognized");
                }

                if (isArray)
                {
                    castToTypeClass = Array.CreateInstance(castToTypeClass, 0).GetType();
                }
            }

            XPathPropertyDesc desc = new XPathPropertyDesc(name, xpath, type, castToTypeClass);
            XPathProperties.Put(name, desc);
        }

        /// <summary>Adds an event property for which the engine uses the supplied XPath expression against a DOM document node to resolve a property value. </summary>
        /// <param name="name">of the event property</param>
        /// <param name="xpath">is an arbitrary xpath expression</param>
        /// <param name="type">is a constant obtained from XPathResultType. Typical values are XPathResultType.NodeSet. </param>
        /// <param name="eventTypeName">is the name of another event type that represents the XPath nodes</param>
        public void AddXPathPropertyFragment(String name, String xpath, XPathResultType type, String eventTypeName)
        {
            if ((type != XPathResultType.Any) && (type != XPathResultType.NodeSet))
            {
                throw new ArgumentException("XPath property for fragments requires an Node or Nodeset return value for property '" + name + "'");
            }

            XPathPropertyDesc desc = new XPathPropertyDesc(name, xpath, type, eventTypeName);
            XPathProperties.Put(name, desc);
        }

        /// <summary> Returns the namespace prefixes in a map of prefix as key and namespace name as value.</summary>
        /// <returns> namespace prefixes
        /// </returns>

        public IDictionary<String, String> NamespacePrefixes
        {
            get { return _namespacePrefixes; }
        }

        /// <summary> Add a prefix and namespace name for use in XPath expressions refering to that prefix.</summary>
        /// <param name="prefix">is the prefix of the namespace
        /// </param>
        /// <param name="namespace">is the namespace name</param>

        public void AddNamespacePrefix(String prefix, String @namespace)
        {
            _namespacePrefixes[prefix] = @namespace;
        }


        /// <summary>Add prefixes and namespace names for use in XPath expressions refering to that prefix. </summary>
        /// <param name="prefixNamespaceMap">map of prefixes and namespaces</param>
        public void AddNamespacePrefixes(IDictionary<String, String> prefixNamespaceMap)
        {
            _namespacePrefixes.PutAll(prefixNamespaceMap);
        }

        /// <summary>Gets or sets the type name of the XPath function resolver to be assigned to the XPath factory instanceupon type initialization.</summary>
        /// <returns>class name of xpath function resolver, or null if none set</returns>
        public string XPathFunctionResolver { get; set; }

        /// <summary>Gets or sets the class name of the XPath variable resolver to be assigned to the XPath factory instanceupon type initialization.</summary>
        /// <returns>class name of xpath function resolver, or null if none set</returns>
        public string XPathVariableResolver { get; set; }

        public override bool Equals(Object otherObj)
        {
            ConfigurationEventTypeXMLDOM other = otherObj as ConfigurationEventTypeXMLDOM;
            if (other == null)
            {
                return false;
            }

            if (other.RootElementName != RootElementName)
            {
                return false;
            }

            if (((other.RootElementNamespace == null) && (RootElementNamespace != null)) ||
                ((other.RootElementNamespace != null) && (RootElementNamespace == null)))
            {
                return false;
            }
            return RootElementNamespace == other.RootElementNamespace;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return RootElementName.GetHashCode();
        }

        /// <summary>
        /// Descriptor class for event properties that are resolved via XPath-expression.
        /// </summary>

        [Serializable]
        public class XPathPropertyDesc
        {
            /// <summary> Returns the event property name.</summary>
            /// <returns> event property name
            /// </returns>
            public string Name { get; private set; }

            /// <summary> Returns the XPath expression.</summary>
            /// <returns> XPath expression
            /// </returns>
            public string XPath { get; private set; }

            /// <summary> Returns the representing the event property type.</summary>
            /// <returns> type infomation
            /// </returns>
            public XPathResultType ResultType { get; private set; }

            /// <summary>
            /// Returns the native data type representing the event property.
            /// </summary>
            public Type ResultDataType { get; private set; }


            /// <summary>
            /// Gets the optional cast to type.
            /// </summary>
            /// <value>The type of the optional cast to.</value>
            public Type OptionalCastToType { get; private set; }

            /// <summary>
            /// Gets the name of the optional event type.
            /// </summary>
            /// <value>The name of the optional event type.</value>
            public string OptionalEventTypeName { get; private set; }

            /// <summary> Ctor.</summary>
            /// <param name="name">the event property name</param>
            /// <param name="xpath">an arbitrary XPath expression</param>
            /// <param name="type">an XPathConstants constant</param>

            public XPathPropertyDesc(String name, String xpath, XPathResultType type)
                : this(name, xpath, type, (Type)null)
            {
            }

            /// <summary>Ctor.</summary>
            /// <param name="name">the event property name</param>
            /// <param name="xpath">an arbitrary XPath expression</param>
            /// <param name="type">a System.Xml.XPath.XPathResultType constant</param>
            /// <param name="optionalCastToType">if non-null then the return value of the xpath expression is cast to this value</param>
            public XPathPropertyDesc(String name, String xpath, XPathResultType type, Type optionalCastToType)
            {
                Name = name;
                XPath = xpath;
                ResultType = type;
                OptionalCastToType = optionalCastToType;
                ResultDataType = typeof(string);

                switch (type)
                {
                    case XPathResultType.Boolean:
                        ResultDataType = typeof(bool);
                        break;
                    case XPathResultType.String:
                        ResultDataType = typeof(string);
                        break;
                    case XPathResultType.Number:
                        ResultDataType = typeof(double);
                        break;
                }
            }

            /// <summary>Ctor. </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="xpath">is an arbitrary XPath expression</param>
            /// <param name="type">is a javax.xml.xpath.XPathConstants constant</param>
            /// <param name="eventTypeName">the name of an event type that represents the fragmented property value</param>
            public XPathPropertyDesc(String name, String xpath, XPathResultType type, String eventTypeName)
            {
                Name = name;
                XPath = xpath;
                ResultType = type;
                OptionalEventTypeName = eventTypeName;
                ResultDataType = typeof(string);

                switch (type)
                {
                    case XPathResultType.Boolean:
                        ResultDataType = typeof(bool);
                        break;
                    case XPathResultType.String:
                        ResultDataType = typeof(string);
                        break;
                    case XPathResultType.Number:
                        ResultDataType = typeof(double);
                        break;
                }
            }

            /// <summary>
            /// Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            public override int GetHashCode()
            {
                return
                    Name.GetHashCode() * 31 +
                    XPath.GetHashCode();
            }
        }
    }
}
