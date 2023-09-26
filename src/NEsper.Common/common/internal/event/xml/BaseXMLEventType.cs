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

using Antlr4.Runtime.Tree.Xpath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.xml;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    /// Base class for XML event types.
    /// </summary>
    public abstract class BaseXMLEventType : BaseConfigurableEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IXPathFunctionResolver _functionResolver;
        private readonly IXPathVariableResolver _variableResolver;
        
        private readonly string _rootElementName;
        private readonly ConfigurationCommonEventTypeXMLDOM _configurationEventTypeXMLDOM;
        private string _startTimestampPropertyName;
        private string _endTimestampPropertyName;

        /// <summary>
        /// XPath namespace context.
        /// </summary>
        private XPathNamespaceContext _namespaceContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "configurationEventTypeXMLDOM">is the XML DOM configuration such as root element and schema names</param>
        /// <param name = "metadata">event type metadata</param>
        /// <param name = "eventBeanTypedEventFactory">for registration and lookup of types</param>
        /// <param name = "xmlEventTypeFactory">xml type factory</param>
        /// <param name = "eventTypeResolver">resolver</param>
        public BaseXMLEventType(
            EventTypeMetadata metadata,
            ConfigurationCommonEventTypeXMLDOM configurationEventTypeXMLDOM,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNameResolver eventTypeResolver,
            XMLFragmentEventTypeFactory xmlEventTypeFactory) : base(
            eventBeanTypedEventFactory,
            metadata,
            typeof(XmlNode),
            eventTypeResolver,
            xmlEventTypeFactory)
        {
            _rootElementName = configurationEventTypeXMLDOM.RootElementName;
            _configurationEventTypeXMLDOM = configurationEventTypeXMLDOM;
            
            if (configurationEventTypeXMLDOM.XPathFunctionResolver != null) {
                try {
                    _functionResolver = TypeHelper.Instantiate<IXPathFunctionResolver>(
                        configurationEventTypeXMLDOM.XPathFunctionResolver,
                        TypeResolverDefault.INSTANCE);
                }
                catch (ClassInstantiationException ex) {
                    throw new ConfigurationException(
                        "Error configuring XPath function resolver for XML type '" +
                        configurationEventTypeXMLDOM.RootElementName +
                        "' : " +
                        ex.Message,
                        ex);
                }
            }

            if (configurationEventTypeXMLDOM.XPathVariableResolver != null) {
                try {
                    _variableResolver = TypeHelper.Instantiate<IXPathVariableResolver>(
                        configurationEventTypeXMLDOM.XPathVariableResolver,
                        TypeResolverDefault.INSTANCE);
                }
                catch (ClassInstantiationException ex) {
                    throw new ConfigurationException(
                        "Error configuring XPath variable resolver for XML type '" +
                        configurationEventTypeXMLDOM.RootElementName +
                        "' : " +
                        ex.Message,
                        ex);
                }
            }
        }

        public DataInputOutputSerde UnderlyingBindingDIO {
            get => null;
            set => throw new UnsupportedOperationException("XML event type does not receive a serde");
        }

        /// <summary>
        /// Returns the name of the root element.
        /// </summary>
        /// <value>root element name</value>
        public string RootElementName => _rootElementName;

        /// <summary>
        /// Sets the namespace context for use in XPath expression resolution.
        /// </summary>
        /// <value>for XPath expressions</value>
        protected XPathNamespaceContext NamespaceContext {
            get => _namespaceContext;
            set => _namespaceContext = value;
        }
        
        /// <summary>
        /// Creates a new XPath expression object from the text representation.
        /// </summary>
        /// <param name="xPathExpression">The xpath expression as text.</param>
        internal XPathExpression CreateXPath(string xPathExpression)
        {
            return XPathExpression.Compile(xPathExpression, NamespaceContext);
        }

        /// <summary>
        /// Set the preconfigured event properties resolved by XPath expression.
        /// </summary>
        /// <param name = "explicitXPathProperties">are preconfigured event properties</param>
        /// <param name = "additionalSchemaProperties">the explicit properties</param>
        protected void Initialize(
            ICollection<ConfigurationCommonEventTypeXMLDOM.XPathPropertyDesc> explicitXPathProperties,
            IList<ExplicitPropertyDescriptor> additionalSchemaProperties)
        {
            // make sure we override those explicitly provided with those derived from a metadataz
            IDictionary<string, ExplicitPropertyDescriptor> namedProperties =
                new LinkedHashMap<string, ExplicitPropertyDescriptor>();
            foreach (var desc in additionalSchemaProperties) {
                namedProperties.Put(desc.Descriptor.PropertyName, desc);
            }

            string xpathExpression = null;
            try {
                foreach (var property in explicitXPathProperties) {
                    xpathExpression = property.XPath;
                    if (Log.IsDebugEnabled) {
                        Log.Debug(
                            "Compiling XPath expression for property '" +
                            property.Name +
                            "' as '" +
                            xpathExpression +
                            "'");
                    }

                    var expression = CreateXPath(xpathExpression);

                    FragmentFactoryXPathPredefinedGetter fragmentFactory = null;
                    var isFragment = false;
                    if (property.OptionalEventTypeName != null) {
                        fragmentFactory = new FragmentFactoryXPathPredefinedGetter(
                            EventBeanTypedEventFactory,
                            EventTypeResolver,
                            property.OptionalEventTypeName,
                            property.Name);
                        isFragment = true;
                    }

                    var isArray = false;
                    if (property.Type == XPathResultType.NodeSet) {
                        isArray = true;
                    }

                    EventPropertyGetterSPI getter = new XPathPropertyGetter(
                        this,
                        property.Name,
                        xpathExpression,
                        expression,
                        property.Type,
                        property.OptionalCastToType,
                        fragmentFactory);
                    var returnType = SchemaUtil.ToReturnType(property.Type, property.OptionalCastToType);
                    var desc = new EventPropertyDescriptor(
                        property.Name,
                        returnType,
                        false,
                        false,
                        isArray,
                        false,
                        isFragment);
                    var @explicit = new ExplicitPropertyDescriptor(
                        desc,
                        getter,
                        isArray,
                        property.OptionalEventTypeName);
                    namedProperties.Put(desc.PropertyName, @explicit);
                }
            }
            catch (XPathException ex) {
                throw new EPException(
                    "XPath expression could not be compiled for expression '" + xpathExpression + '\'',
                    ex);
            }

            base.Initialize(new List<ExplicitPropertyDescriptor>(namedProperties.Values));
            // evaluate start and end timestamp properties if any
            _startTimestampPropertyName = _configurationEventTypeXMLDOM.StartTimestampPropertyName;
            _endTimestampPropertyName = _configurationEventTypeXMLDOM.EndTimestampPropertyName;
            EventTypeUtility.ValidateTimestampProperties(this, _startTimestampPropertyName, _endTimestampPropertyName);
        }

        public override IList<EventType> SuperTypes => null;
        public override IEnumerable<EventType> DeepSuperTypes => null;
        public override ICollection<EventType> DeepSuperTypesCollection => EmptySet<EventType>.Instance;

        /// <summary>
        /// Returns the configuration XML for the XML type.
        /// </summary>
        /// <value>config XML</value>
        public ConfigurationCommonEventTypeXMLDOM ConfigurationEventTypeXMLDOM => _configurationEventTypeXMLDOM;

        public override ExprValidationException EqualsCompareType(EventType eventType)
        {
            if (!(eventType is BaseXMLEventType other)) {
                return new ExprValidationException("Expected a base-xml event type but received " + eventType);
            }

            if (!_configurationEventTypeXMLDOM.Equals(other._configurationEventTypeXMLDOM)) {
                return new ExprValidationException("XML configuration mismatches between types");
            }

            return null;
        }

        /// <summary>
        /// Same-Root XML types are actually equivalent.
        /// </summary>
        /// <param name = "otherObj">to compare to</param>
        /// <returns>indicator</returns>
        public override bool Equals(object otherObj)
        {
            if (!(otherObj is BaseXMLEventType other)) {
                return false;
            }

            return _configurationEventTypeXMLDOM.Equals(other._configurationEventTypeXMLDOM);
        }

        public override int GetHashCode()
        {
            return _configurationEventTypeXMLDOM.GetHashCode();
        }

        public override EventPropertyWriterSPI GetWriter(string propertyName)
        {
            return null;
        }

        public override EventPropertyDescriptor[] WriteableProperties => Array.Empty<EventPropertyDescriptor>();

        public override EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            return null;
        }

        public override EventBeanWriter GetWriter(string[] properties)
        {
            return null;
        }

        public override string StartTimestampPropertyName => _startTimestampPropertyName;
        public override string EndTimestampPropertyName => _endTimestampPropertyName;
    }
} // end of namespace