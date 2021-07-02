///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.xml
{
    public class XMLFragmentEventTypeFactory
    {
        private readonly BeanEventTypeFactory _eventTypeFactory;
        private readonly EventTypeNameResolver _eventTypeNameResolver;
        private readonly EventTypeCompileTimeRegistry _optionalCompileTimeRegistry;
        private IDictionary<string, SchemaXMLEventType> _derivedTypes;

        private IDictionary<string, SchemaXMLEventType> _rootTypes;

        public XMLFragmentEventTypeFactory(
            BeanEventTypeFactory eventTypeFactory,
            EventTypeCompileTimeRegistry optionalCompileTimeRegistry,
            EventTypeNameResolver eventTypeNameResolver)
        {
            this._eventTypeFactory = eventTypeFactory;
            this._optionalCompileTimeRegistry = optionalCompileTimeRegistry;
            this._eventTypeNameResolver = eventTypeNameResolver;
        }

        public void AddRootType(SchemaXMLEventType type)
        {
            if (_rootTypes == null) {
                _rootTypes = new Dictionary<string, SchemaXMLEventType>();
            }

            if (_rootTypes.ContainsKey(type.Name)) {
                throw new IllegalStateException("Type '" + type.Name + "' already exists");
            }

            _rootTypes.Put(type.Name, type);
        }

        public EventType GetTypeByName(string derivedEventTypeName)
        {
            if (_derivedTypes == null) {
                _derivedTypes = new Dictionary<string, SchemaXMLEventType>();
            }

            return _derivedTypes.Get(derivedEventTypeName);
        }

        public EventType GetCreateXMLDOMType(
            string rootTypeName,
            string derivedEventTypeName,
            string moduleName,
            SchemaElementComplex complex,
            string representsFragmentOfProperty)
        {
            if (_rootTypes == null) {
                _rootTypes = new Dictionary<string, SchemaXMLEventType>();
            }

            if (_derivedTypes == null) {
                _derivedTypes = new Dictionary<string, SchemaXMLEventType>();
            }

            var type = _rootTypes.Get(rootTypeName);
            if (type == null) {
                throw new IllegalStateException("Failed to find XML root event type '" + rootTypeName + "'");
            }

            var config = type.ConfigurationEventTypeXMLDOM;

            // add a new type
            var xmlDom = new ConfigurationCommonEventTypeXMLDOM();
            xmlDom.RootElementName = "//" + complex.Name; // such the reload of the type can resolve it
            xmlDom.RootElementNamespace = complex.Namespace;
            xmlDom.IsAutoFragment = config.IsAutoFragment;
            xmlDom.IsEventSenderValidatesRoot = config.IsEventSenderValidatesRoot;
            xmlDom.IsXPathPropertyExpr = config.IsXPathPropertyExpr;
            xmlDom.IsXPathResolvePropertiesAbsolute = config.IsXPathResolvePropertiesAbsolute;
            xmlDom.SchemaResource = config.SchemaResource;
            xmlDom.SchemaText = config.SchemaText;
            xmlDom.XPathFunctionResolver = config.XPathFunctionResolver;
            xmlDom.XPathVariableResolver = config.XPathVariableResolver;
            xmlDom.DefaultNamespace = config.DefaultNamespace;
            xmlDom.AddNamespacePrefixes(config.NamespacePrefixes);

            var metadata = new EventTypeMetadata(
                derivedEventTypeName,
                moduleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.XML,
                NameAccessModifier.PRECONFIGURED,
                EventTypeBusModifier.BUS,
                false,
                new EventTypeIdPair(CRC32Util.ComputeCRC32(derivedEventTypeName), -1));
            var eventType = (SchemaXMLEventType) _eventTypeFactory.EventTypeFactory.CreateXMLType(
                metadata,
                xmlDom,
                type.SchemaModel,
                representsFragmentOfProperty,
                rootTypeName,
                _eventTypeFactory,
                this,
                _eventTypeNameResolver);
            _derivedTypes.Put(derivedEventTypeName, eventType);

            _optionalCompileTimeRegistry?.NewType(eventType);

            return eventType;
        }

        public SchemaXMLEventType GetRootTypeByName(string representsOriginalTypeName)
        {
            return _rootTypes == null ? null : _rootTypes.Get(representsOriginalTypeName);
        }
    }
} // end of namespace