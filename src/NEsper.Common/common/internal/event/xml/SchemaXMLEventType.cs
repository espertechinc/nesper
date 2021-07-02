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
using System.Text;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     EventType for xml events that have a Schema.
    ///     Mapped and Indexed properties are supported.
    ///     All property types resolved via the declared xsd types.
    ///     Can access attributes.
    ///     Validates the property string at construction time.
    /// </summary>
    /// <author>pablo</author>
    public class SchemaXMLEventType : BaseXMLEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly bool _isPropertyExpressionXPath;
        private readonly IDictionary<string, EventPropertyGetterSPI> _propertyGetterCache;
        private readonly string _rootElementNamespace;

        private readonly SchemaElementComplex _schemaModelRoot;

        public SchemaXMLEventType(
            EventTypeMetadata eventTypeMetadata,
            ConfigurationCommonEventTypeXMLDOM config,
            SchemaModel schemaModel,
            string representsFragmentOfProperty,
            string representsOriginalTypeName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNameResolver eventTypeResolver,
            XMLFragmentEventTypeFactory xmlEventTypeFactory)
            : base(
                eventTypeMetadata,
                config,
                eventBeanTypedEventFactory,
                eventTypeResolver,
                xmlEventTypeFactory)
        {
            _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            SchemaModel = schemaModel;
            _rootElementNamespace = config.RootElementNamespace;
            _schemaModelRoot = SchemaUtil.FindRootElement(schemaModel, _rootElementNamespace, RootElementName);
            _isPropertyExpressionXPath = config.IsXPathPropertyExpr;
            RepresentsFragmentOfProperty = representsFragmentOfProperty;
            RepresentsOriginalTypeName = representsOriginalTypeName;

            // Set of namespace context for XPath expressions
            var ctx = new XPathNamespaceContext();
            if (config.DefaultNamespace != null) {
                ctx.SetDefaultNamespace(config.DefaultNamespace);
            }

            foreach (var entry in config.NamespacePrefixes) {
                ctx.AddNamespace(entry.Key, entry.Value);
            }

            NamespaceContext = ctx;

            // add properties for the root element
            IList<ExplicitPropertyDescriptor> additionalSchemaProps = new List<ExplicitPropertyDescriptor>();

            // Add a property for each complex child element
            foreach (SchemaElementComplex complex in _schemaModelRoot.ComplexElements) {
                var propertyName = complex.Name;
                var returnType = typeof(XmlNode);

                if (complex.OptionalSimpleType != null) {
                    returnType = SchemaUtil.ToReturnType(complex);
                }

                if (complex.IsArray) {
                    returnType = typeof(XmlNode[]); // We use Node[] for arrays and NodeList for XPath-Expressions returning Nodeset
                }

                var isFragment = false;
                if (ConfigurationEventTypeXMLDOM.IsAutoFragment && !ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr) {
                    isFragment = CanFragment(complex);
                }

                var getter = DoResolvePropertyGetter(propertyName, true);
                var desc = new EventPropertyDescriptor(
                    propertyName,
                    returnType,
                    false,
                    false,
                    complex.IsArray,
                    false,
                    isFragment);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Add a property for each simple child element
            foreach (var simple in _schemaModelRoot.SimpleElements) {
                var propertyName = simple.Name;
                var returnType = SchemaUtil.ToReturnType(simple);
                var componentType = GenericExtensions.GetComponentType(returnType);
                var isIndexed = simple.IsArray || componentType != null;
                var getter = DoResolvePropertyGetter(propertyName, true);
                var desc = new EventPropertyDescriptor(
                    propertyName,
                    returnType,
                    false,
                    false,
                    isIndexed,
                    false,
                    false);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Add a property for each attribute
            foreach (var attribute in _schemaModelRoot.Attributes) {
                var propertyName = attribute.Name;
                var returnType = SchemaUtil.ToReturnType(attribute);
                var componentType = GenericExtensions.GetComponentType(returnType);
                var isIndexed = componentType != null;
                var getter = DoResolvePropertyGetter(propertyName, true);
                var desc = new EventPropertyDescriptor(
                    propertyName,
                    returnType,
                    false,
                    false,
                    isIndexed,
                    false,
                    false);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Finally add XPath properties as that may depend on the rootElementNamespace
            Initialize(config.XPathProperties.Values, additionalSchemaProps);
        }

        public SchemaModel SchemaModel { get; }

        public string RepresentsFragmentOfProperty { get; }

        public string RepresentsOriginalTypeName { get; }

        protected override FragmentEventType DoResolveFragmentType(string property)
        {
            if (!ConfigurationEventTypeXMLDOM.IsAutoFragment || ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr) {
                return null;
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(property);

            var item = prop.GetPropertyTypeSchema(_schemaModelRoot);
            if (item == null || !CanFragment(item)) {
                return null;
            }

            var complex = (SchemaElementComplex) item;

            // build name of event type
            var atomicProps = prop.ToPropertyArray();
            var delimiterDot = ".";
            var eventTypeNameBuilder = new StringBuilder(Name);
            foreach (var atomic in atomicProps) {
                eventTypeNameBuilder.Append(delimiterDot);
                eventTypeNameBuilder.Append(atomic);
            }

            var derivedEventTypeName = eventTypeNameBuilder.ToString();

            // check if the type exists, use the existing type if found
            var existingType = XmlEventTypeFactory.GetTypeByName(derivedEventTypeName);
            if (existingType != null) {
                return new FragmentEventType(existingType, complex.IsArray, false);
            }

            EventType newType;
            var represents = RepresentsFragmentOfProperty == null
                ? property
                : RepresentsFragmentOfProperty + "." + property;
            try {
                newType = XmlEventTypeFactory.GetCreateXMLDOMType(
                    RepresentsOriginalTypeName,
                    derivedEventTypeName,
                    Metadata.ModuleName,
                    complex,
                    represents);
            }
            catch (Exception ex) {
                Log.Error(
                    "Failed to add dynamic event type for fragment of XML schema for property '" +
                    property +
                    "' :" +
                    ex.Message,
                    ex);
                return null;
            }

            return new FragmentEventType(newType, complex.IsArray, false);
        }

        protected override Type DoResolvePropertyType(string propertyExpression)
        {
            return DoResolvePropertyType(propertyExpression, false);
        }

        private Type DoResolvePropertyType(
            string propertyExpression,
            bool allowSimpleProperties)
        {
            // see if this is an indexed property
            var index = StringValue.UnescapedIndexOfDot(propertyExpression);
            if (!allowSimpleProperties && index == -1) {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                if (!property.IsDynamic) {
                    if (!(property is IndexedProperty)) {
                        return null;
                    }

                    var indexedProp = (IndexedProperty) property;
                    var descriptor = PropertyDescriptorMap.Get(indexedProp.PropertyNameAtomic);

                    return descriptor?.PropertyType;
                }
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
            if (prop.IsDynamic) {
                return typeof(XmlNode);
            }

            var item = prop.GetPropertyTypeSchema(_schemaModelRoot);
            if (item == null) {
                return null;
            }

            return SchemaUtil.ToReturnType(item);
        }

        protected override EventPropertyGetterSPI DoResolvePropertyGetter(string property)
        {
            return DoResolvePropertyGetter(property, false);
        }

        private EventPropertyGetterSPI DoResolvePropertyGetter(
            string propertyExpression,
            bool allowSimpleProperties)
        {
            var getter = _propertyGetterCache.Get(propertyExpression);
            if (getter != null) {
                return getter;
            }

            if (!allowSimpleProperties) {
                // see if this is an indexed property
                var index = StringValue.UnescapedIndexOfDot(propertyExpression);
                if (index == -1) {
                    // parse, can be an indexed property
                    var property = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                    if (!property.IsDynamic) {
                        if (!(property is IndexedProperty)) {
                            return null;
                        }

                        var indexedProp = (IndexedProperty) property;
                        getter = propertyGetters.Get(indexedProp.PropertyNameAtomic);
                        if (null == getter) {
                            return null;
                        }

                        var descriptor = PropertyDescriptorMap.Get(indexedProp.PropertyNameAtomic);
                        if (descriptor == null) {
                            return null;
                        }

                        if (!descriptor.IsIndexed) {
                            return null;
                        }

                        if (descriptor.PropertyType == typeof(XmlNodeList)) {
                            FragmentFactorySPI fragmentFactory = new FragmentFactoryDOMGetter(
                                EventBeanTypedEventFactory,
                                this,
                                indexedProp.PropertyNameAtomic);
                            return new XPathPropertyArrayItemGetter(getter, indexedProp.Index, fragmentFactory);
                        } else if (descriptor.PropertyType == typeof(string)) {
                            FragmentFactorySPI fragmentFactory = new FragmentFactoryDOMGetter(
                                EventBeanTypedEventFactory,
                                this,
                                indexedProp.PropertyNameAtomic);
                            return new XPathPropertyArrayItemGetter(getter, indexedProp.Index, fragmentFactory);
                        }
                    }
                }
            }

            if (!_isPropertyExpressionXPath) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                var isDynamic = prop.IsDynamic;

                if (!isDynamic) {
                    var item = prop.GetPropertyTypeSchema(_schemaModelRoot);
                    if (item == null) {
                        return null;
                    }

                    getter = prop.GetGetterDOM(_schemaModelRoot, EventBeanTypedEventFactory, this, propertyExpression);
                    if (getter == null) {
                        return null;
                    }

                    var returnType = SchemaUtil.ToReturnType(item);
                    if (returnType != typeof(XmlNode) && returnType != typeof(XmlNodeList)) {
                        if (!returnType.IsArray) {
                            getter = new DOMConvertingGetter((DOMPropertyGetter) getter, returnType);
                        }
                        else {
                            getter = new DOMConvertingArrayGetter(
                                (DOMPropertyGetter) getter,
                                returnType.GetElementType());
                        }
                    }
                }
                else {
                    return prop.GetterDOM;
                }
            }
            else {
                var allowFragments = !ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr;
                getter = SchemaXMLPropertyParser.GetXPathResolution(
                    propertyExpression,
                    NamespaceContext,
                    RootElementName,
                    _rootElementNamespace,
                    SchemaModel,
                    EventBeanTypedEventFactory,
                    this,
                    allowFragments,
                    ConfigurationEventTypeXMLDOM.DefaultNamespace);
            }

            _propertyGetterCache.Put(propertyExpression, getter);
            return getter;
        }

        private bool CanFragment(SchemaItem item)
        {
            if (!(item is SchemaElementComplex)) {
                return false;
            }

            var complex = (SchemaElementComplex) item;
            if (complex.OptionalSimpleType != null) {
                return false; // no transposing if the complex type also has a simple value else that is hidden
            }

            return true;
        }
    }
} // end of namespace