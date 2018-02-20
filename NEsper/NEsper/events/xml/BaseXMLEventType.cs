///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Base class for XML event types.
    /// </summary>
    public abstract class BaseXMLEventType : BaseConfigurableEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IXPathFunctionResolver _functionResolver;
        private readonly IXPathVariableResolver _variableResolver;

        private string _startTimestampPropertyName;
        private string _endTimestampPropertyName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="configurationEventTypeXMLDOM">is the XML DOM configuration such as root element and schema names</param>
        /// <param name="eventAdapterService">for registration and lookup of types</param>
        /// <param name="lockManager"></param>
        protected BaseXMLEventType(
            EventTypeMetadata metadata,
            int eventTypeId,
            ConfigurationEventTypeXMLDOM configurationEventTypeXMLDOM,
            EventAdapterService eventAdapterService,
            ILockManager lockManager)
            : base(lockManager, eventAdapterService, metadata, eventTypeId, typeof(XmlNode))
        {
            RootElementName = configurationEventTypeXMLDOM.RootElementName;
            ConfigurationEventTypeXMLDOM = configurationEventTypeXMLDOM;

            if (configurationEventTypeXMLDOM.XPathFunctionResolver != null)
            {
                try
                {
                    var fresolver = TypeHelper.Instantiate<IXPathFunctionResolver>(
                        configurationEventTypeXMLDOM.XPathFunctionResolver);
                    _functionResolver = fresolver;
                }
                catch (TypeInstantiationException ex)
                {
                    throw new ConfigurationException("Error configuring XPath function resolver for XML type '" + configurationEventTypeXMLDOM.RootElementName + "' : " + ex.Message, ex);
                }
            }

            if (configurationEventTypeXMLDOM.XPathVariableResolver != null)
            {
                try
                {
                    var vresolver = TypeHelper.Instantiate<IXPathVariableResolver>(
                        configurationEventTypeXMLDOM.XPathVariableResolver);
                    _variableResolver = vresolver;
                }
                catch (TypeInstantiationException ex)
                {
                    throw new ConfigurationException("Error configuring XPath variable resolver for XML type '" + configurationEventTypeXMLDOM.RootElementName + "' : " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Returns the name of the root element.
        /// </summary>
        /// <returns>
        /// root element name
        /// </returns>
        public string RootElementName { get; private set; }

        /// <summary>
        /// Sets the namespace context for use in XPath expression resolution.
        /// </summary>
        protected XPathNamespaceContext NamespaceContext { get; set; }

        /// <summary>
        /// Gets the extended context.
        /// </summary>
        /// <value>The extended context.</value>
        protected XsltContext GetExtendedContext()
        {
            if (NamespaceContext == null)
                return null;
            if ((_functionResolver == null) && (_variableResolver == null))
                return NamespaceContext;
            return new ExtendedContext(NamespaceContext, _functionResolver, _variableResolver);
        }

        /// <summary>
        /// Set the preconfigured event properties resolved by XPath expression.
        /// </summary>
        /// <param name="explicitXPathProperties">are preconfigured event properties</param>
        /// <param name="additionalSchemaProperties">the explicit properties</param>
        protected void Initialize(ICollection<ConfigurationEventTypeXMLDOM.XPathPropertyDesc> explicitXPathProperties,
                                  IList<ExplicitPropertyDescriptor> additionalSchemaProperties)
        {
            // make sure we override those explicitly provided with those derived from a metadataz
            var namedProperties = new LinkedHashMap<String, ExplicitPropertyDescriptor>();
            foreach (ExplicitPropertyDescriptor desc in additionalSchemaProperties)
            {
                namedProperties[desc.Descriptor.PropertyName] = desc;
            }

            String xPathExpression = null;
            try
            {
                foreach (ConfigurationEventTypeXMLDOM.XPathPropertyDesc property in explicitXPathProperties)
                {
                    xPathExpression = property.XPath;
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info("Compiling XPath expression for property '" + property.Name + "' as '" +
                                 xPathExpression + "'");
                    }

                    var expressionContext = NamespaceContext ?? GetExtendedContext();
                    var expression = XPathExpression.Compile(xPathExpression, expressionContext);

                    FragmentFactoryXPathPredefinedGetter fragmentFactory = null;

                    var isFragment = false;
                    if (property.OptionalEventTypeName != null)
                    {
                        fragmentFactory = new FragmentFactoryXPathPredefinedGetter(
                            EventAdapterService,
                            property.OptionalEventTypeName,
                            property.Name);
                        isFragment = true;
                    }

                    var getter = new XPathPropertyGetter(
                        property.Name,
                        xPathExpression,
                        expression,
                        property.ResultType,
                        property.OptionalCastToType,
                        fragmentFactory);
                    var returnType = SchemaUtil.ToReturnType(
                        property.ResultType,
                        property.OptionalCastToType.GetBoxedType());
                    var indexType = returnType.GetIndexType();
                    var isIndexed = indexType != null;

                    if (property.ResultType == XPathResultType.NodeSet)
                    {
                        isIndexed = true;
                    }

                    var desc = new EventPropertyDescriptor(
                        property.Name, returnType, indexType, false, false,
                        isIndexed, false, isFragment);
                    var @explicit = new ExplicitPropertyDescriptor(
                        desc, getter, isIndexed,
                        property.OptionalEventTypeName);

                    namedProperties[desc.PropertyName] = @explicit;
                }
            }
            catch (XPathException ex)
            {
                throw new EPException(
                    "XPath expression could not be compiled for expression '" + xPathExpression + '\'', ex);
            }

            Initialize(namedProperties.Values);

            // evaluate start and end timestamp properties if any
            _startTimestampPropertyName = ConfigurationEventTypeXMLDOM.StartTimestampPropertyName;
            _endTimestampPropertyName = ConfigurationEventTypeXMLDOM.EndTimestampPropertyName;
            EventTypeUtility.ValidateTimestampProperties(this, _startTimestampPropertyName, _endTimestampPropertyName);
        }

        public override EventType[] SuperTypes
        {
            get { return null; }
        }

        public override EventType[] DeepSuperTypes
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the configuration XML for the XML type.
        /// </summary>
        /// <returns>
        /// config XML
        /// </returns>
        public ConfigurationEventTypeXMLDOM ConfigurationEventTypeXMLDOM { get; private set; }

        public override bool EqualsCompareType(EventType eventType)
        {
            var other = eventType as BaseXMLEventType;
            return other != null && Equals(ConfigurationEventTypeXMLDOM, other.ConfigurationEventTypeXMLDOM);
        }

        public override bool Equals(Object otherObj)
        {
            var other = otherObj as BaseXMLEventType;
            if (other == null)
            {
                return false;
            }

            return Equals(ConfigurationEventTypeXMLDOM, other.ConfigurationEventTypeXMLDOM);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ConfigurationEventTypeXMLDOM.GetHashCode();
        }

        public override EventPropertyWriter GetWriter(String propertyName)
        {
            return null;
        }

        public override EventPropertyDescriptor[] WriteableProperties
        {
            get { return new EventPropertyDescriptor[0]; }
        }

        public override EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            return null;
        }

        public override EventBeanWriter GetWriter(String[] properties)
        {
            return null;
        }

        public override EventBeanReader Reader
        {
            get { return null; }
        }

        public override string StartTimestampPropertyName
        {
            get { return _startTimestampPropertyName; }
        }

        public override string EndTimestampPropertyName
        {
            get { return _endTimestampPropertyName; }
        }
    }
}
