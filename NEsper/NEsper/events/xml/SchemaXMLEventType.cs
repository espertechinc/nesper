///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events.property;


namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// EventType for xml events that have a Schema. Mapped and Indexed properties are
    /// supported. All property types resolved via the declared xsd types. Can access
    /// attributes. Validates the property string at construction time.
    /// </summary>
    /// <author>pablo </author>
    public class SchemaXMLEventType : BaseXMLEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _isPropertyExpressionXPath;
        private readonly IDictionary<String, EventPropertyGetterSPI> _propertyGetterCache;
        private readonly String _rootElementNamespace;

        private readonly SchemaModel _schemaModel;
        private readonly SchemaElementComplex _schemaModelRoot;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypeMetadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="config">configuration for type</param>
        /// <param name="schemaModel">the schema representation</param>
        /// <param name="eventAdapterService">type lookup and registration</param>
        /// <param name="lockManager"></param>
        public SchemaXMLEventType(
            EventTypeMetadata eventTypeMetadata,
            int eventTypeId,
            ConfigurationEventTypeXMLDOM config,
            SchemaModel schemaModel,
            EventAdapterService eventAdapterService,
            ILockManager lockManager)
            : base(eventTypeMetadata, eventTypeId, config, eventAdapterService, lockManager)
        {
            _propertyGetterCache = new Dictionary<String, EventPropertyGetterSPI>();
            _schemaModel = schemaModel;
            _rootElementNamespace = config.RootElementNamespace;
            _schemaModelRoot = SchemaUtil.FindRootElement(schemaModel, _rootElementNamespace, RootElementName);
            _isPropertyExpressionXPath = config.IsXPathPropertyExpr;

            // Set of namespace context for XPath expressions
            var ctx = new XPathNamespaceContext();
            if (config.DefaultNamespace != null)
            {
                ctx.SetDefaultNamespace(config.DefaultNamespace);
            }

            foreach (var entry in config.NamespacePrefixes)
            {
                ctx.AddNamespace(entry.Key, entry.Value);
            }

            NamespaceContext = ctx;

            // add properties for the root element
            var additionalSchemaProps = new List<ExplicitPropertyDescriptor>();

            // Add a property for each complex child element
            foreach (SchemaElementComplex complex in _schemaModelRoot.ComplexElements)
            {
                var propertyName = complex.Name;
                var returnType = typeof(XmlNode);
                Type propertyComponentType = null;

                if (complex.OptionalSimpleType != null)
                {
                    returnType = SchemaUtil.ToReturnType(complex);
                }
                if (complex.IsArray)
                {
                    // We use XmlNode[] for arrays and NodeList for XPath-Expressions returning Nodeset
                    returnType = typeof(XmlNode[]);
                    propertyComponentType = typeof (XmlNode);
                }

                bool isFragment = false;
                if (ConfigurationEventTypeXMLDOM.IsAutoFragment && (!ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr))
                {
                    isFragment = CanFragment(complex);
                }

                var indexType = returnType.GetIndexType();
                var isIndexed = indexType != null;
                var getter = DoResolvePropertyGetter(propertyName, true);
                var desc = new EventPropertyDescriptor(propertyName, returnType, indexType, false, false, isIndexed, false, isFragment);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Add a property for each simple child element
            foreach (SchemaElementSimple simple in _schemaModelRoot.SimpleElements)
            {
                var propertyName = simple.Name;
                var returnType = SchemaUtil.ToReturnType(simple);
                var getter = DoResolvePropertyGetter(propertyName, true);
                var indexType = returnType.GetIndexType();
                var isIndexed = indexType != null;
                var desc = new EventPropertyDescriptor(propertyName, returnType, indexType, false, false, isIndexed, false, false);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Add a property for each attribute
            foreach (SchemaItemAttribute attribute in _schemaModelRoot.Attributes)
            {
                var propertyName = attribute.Name;
                var returnType = SchemaUtil.ToReturnType(attribute);
                var getter = DoResolvePropertyGetter(propertyName, true);
                var indexType = returnType.GetIndexType();
                var isIndexed = indexType != null;
                var desc = new EventPropertyDescriptor(propertyName, returnType, indexType, false, false, isIndexed, false, false);
                var @explicit = new ExplicitPropertyDescriptor(desc, getter, false, null);
                additionalSchemaProps.Add(@explicit);
            }

            // Finally add XPath properties as that may depend on the rootElementNamespace
            Initialize(config.XPathProperties.Values, additionalSchemaProps);
        }

        public SchemaModel SchemaModel
        {
            get { return _schemaModel; }
        }

        public SchemaElementComplex SchemaModelRoot
        {
            get { return _schemaModelRoot; }
        }

        public string RootElementNamespace
        {
            get { return _rootElementNamespace; }
        }

        protected override FragmentEventType DoResolveFragmentType(String property)
        {
            if ((!ConfigurationEventTypeXMLDOM.IsAutoFragment) || (ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr))
            {
                return null;
            }

            Property prop = PropertyParser.ParseAndWalkLaxToSimple(property);

            SchemaItem item = prop.GetPropertyTypeSchema(_schemaModelRoot, EventAdapterService);
            if ((item == null) || (!CanFragment(item)))
            {
                return null;
            }
            var complex = (SchemaElementComplex)item;

            // build name of event type
            String[] atomicProps = prop.ToPropertyArray();
            String delimiterDot = ".";
            var eventTypeNameBuilder = new StringBuilder(Name);
            foreach (String atomic in atomicProps)
            {
                eventTypeNameBuilder.Append(delimiterDot);
                eventTypeNameBuilder.Append(atomic);
            }
            String eventTypeName = eventTypeNameBuilder.ToString();

            // check if the type exists, use the existing type if found
            EventType existingType = EventAdapterService.GetEventTypeByName(eventTypeName);
            if (existingType != null)
            {
                return new FragmentEventType(existingType, complex.IsArray, false);
            }

            // add a new type
            var xmlDom = new ConfigurationEventTypeXMLDOM();
            xmlDom.RootElementName = "//" + complex.Name; // such the reload of the type can resolve it
            xmlDom.RootElementNamespace = complex.Namespace;
            xmlDom.IsAutoFragment = ConfigurationEventTypeXMLDOM.IsAutoFragment;
            xmlDom.IsEventSenderValidatesRoot = ConfigurationEventTypeXMLDOM.IsEventSenderValidatesRoot;
            xmlDom.IsXPathPropertyExpr = ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr;
            xmlDom.IsXPathResolvePropertiesAbsolute = ConfigurationEventTypeXMLDOM.IsXPathResolvePropertiesAbsolute;
            xmlDom.SchemaResource = ConfigurationEventTypeXMLDOM.SchemaResource;
            xmlDom.SchemaText = ConfigurationEventTypeXMLDOM.SchemaText;
            xmlDom.XPathFunctionResolver = ConfigurationEventTypeXMLDOM.XPathFunctionResolver;
            xmlDom.XPathVariableResolver = ConfigurationEventTypeXMLDOM.XPathVariableResolver;
            xmlDom.DefaultNamespace = ConfigurationEventTypeXMLDOM.DefaultNamespace;
            xmlDom.AddNamespacePrefixes(ConfigurationEventTypeXMLDOM.NamespacePrefixes);

            EventType newType;
            try
            {
                newType = EventAdapterService.AddXMLDOMType(eventTypeName, xmlDom, _schemaModel, true);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Failed to add dynamic event type for fragment of XML schema for property '" + property + "' :" +
                    ex.Message, ex);
                return null;
            }
            return new FragmentEventType(newType, complex.IsArray, false);
        }

        protected override Type DoResolvePropertyType(String propertyExpression)
        {
            return DoResolvePropertyType(propertyExpression, false);
        }

        private Type DoResolvePropertyType(String propertyExpression, bool allowSimpleProperties)
        {
            // see if this is an indexed property
            int index = ASTUtil.UnescapedIndexOfDot(propertyExpression);
            if ((!allowSimpleProperties) && (index == -1))
            {
                // parse, can be an indexed property
                Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                if (!property.IsDynamic)
                {
                    if (!(property is IndexedProperty))
                    {
                        return null;
                    }
                    var indexedProp = (IndexedProperty)property;
                    EventPropertyDescriptor descriptor = PropertyDescriptorMap.Get(indexedProp.PropertyNameAtomic);
                    if (descriptor == null)
                    {
                        return null;
                    }
                    return descriptor.PropertyType;
                }
            }

            Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
            if (prop.IsDynamic)
            {
                return typeof(XmlNode);
            }

            SchemaItem item = prop.GetPropertyTypeSchema(_schemaModelRoot, EventAdapterService);
            if (item == null)
            {
                return null;
            }

            return SchemaUtil.ToReturnType(item);
        }

        protected override EventPropertyGetterSPI DoResolvePropertyGetter(String property)
        {
            return DoResolvePropertyGetter(property, false);
        }

        private EventPropertyGetterSPI DoResolvePropertyGetter(String propertyExpression, bool allowSimpleProperties)
        {
            EventPropertyGetterSPI getter = _propertyGetterCache.Get(propertyExpression);
            if (getter != null)
            {
                return getter;
            }

            if (!allowSimpleProperties)
            {
                // see if this is an indexed property
                int index = ASTUtil.UnescapedIndexOfDot(propertyExpression);
                if (index == -1)
                {
                    // parse, can be an indexed property
                    Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                    if (!property.IsDynamic)
                    {
                        if (!(property is IndexedProperty))
                        {
                            return null;
                        }
                        var indexedProp = (IndexedProperty)property;
                        getter = PropertyGetters.Get(indexedProp.PropertyNameAtomic);
                        if (getter == null)
                        {
                            return null;
                        }
                        EventPropertyDescriptor descriptor = PropertyDescriptorMap.Get(indexedProp.PropertyNameAtomic);
                        if (descriptor == null)
                        {
                            return null;
                        }
                        if (!descriptor.IsIndexed)
                        {
                            return null;
                        }
                        if (descriptor.PropertyType == typeof(XmlNodeList))
                        {
                            FragmentFactory fragmentFactory = new FragmentFactoryDOMGetter(
                                EventAdapterService, this, indexedProp.PropertyNameAtomic);
                            return new XPathPropertyArrayItemGetter(getter, indexedProp.Index, fragmentFactory);
                        }
                        if (descriptor.PropertyType == typeof(string))
                        {
                            FragmentFactory fragmentFactory = new FragmentFactoryDOMGetter(
                                EventAdapterService, this, indexedProp.PropertyNameAtomic);
                            return new XPathPropertyArrayItemGetter(getter, indexedProp.Index, fragmentFactory);
                        }
                    }
                }
            }

            if (!_isPropertyExpressionXPath)
            {
                Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
                bool isDynamic = prop.IsDynamic;

                if (!isDynamic)
                {
                    SchemaItem item = prop.GetPropertyTypeSchema(_schemaModelRoot, EventAdapterService);
                    if (item == null)
                    {
                        return null;
                    }

                    getter = prop.GetGetterDOM(_schemaModelRoot, EventAdapterService, this, propertyExpression);
                    if (getter == null)
                    {
                        return null;
                    }

                    Type returnType = SchemaUtil.ToReturnType(item);
                    if ((returnType != typeof(XmlNode)) && (returnType != typeof(XmlNodeList)))
                    {
                        if (!returnType.IsArray)
                        {
                            getter = new DOMConvertingGetter((DOMPropertyGetter)getter, returnType);
                        }
                        else
                        {
                            getter = new DOMConvertingArrayGetter((DOMPropertyGetter)getter, returnType.GetElementType());
                        }
                    }
                }
                else
                {
                    return prop.GetGetterDOM();
                }
            }
            else
            {
                bool allowFragments = !ConfigurationEventTypeXMLDOM.IsXPathPropertyExpr;
                getter = SchemaXMLPropertyParser.GetXPathResolution(
                    propertyExpression,
                    NamespaceContext,
                    RootElementName,
                    _rootElementNamespace,
                    _schemaModel,
                    EventAdapterService,
                    this,
                    allowFragments,
                    ConfigurationEventTypeXMLDOM.DefaultNamespace);
            }

            _propertyGetterCache.Put(propertyExpression, getter);
            return getter;
        }

        private static bool CanFragment(SchemaItem item)
        {
            if (!(item is SchemaElementComplex))
            {
                return false;
            }

            var complex = (SchemaElementComplex)item;
            if (complex.OptionalSimpleType != null)
            {
                return false; // no transposing if the complex type also has a simple value else that is hidden
            }

            return true;
        }
    }
}
