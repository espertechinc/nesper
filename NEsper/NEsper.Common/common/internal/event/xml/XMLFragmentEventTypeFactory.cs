///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
	public class XMLFragmentEventTypeFactory {
	    private readonly BeanEventTypeFactory eventTypeFactory;
	    private readonly EventTypeCompileTimeRegistry optionalCompileTimeRegistry;
	    private readonly EventTypeNameResolver eventTypeNameResolver;

	    private IDictionary<string, SchemaXMLEventType> rootTypes;
	    private IDictionary<string, SchemaXMLEventType> derivedTypes;

	    public XMLFragmentEventTypeFactory(BeanEventTypeFactory eventTypeFactory, EventTypeCompileTimeRegistry optionalCompileTimeRegistry, EventTypeNameResolver eventTypeNameResolver) {
	        this.eventTypeFactory = eventTypeFactory;
	        this.optionalCompileTimeRegistry = optionalCompileTimeRegistry;
	        this.eventTypeNameResolver = eventTypeNameResolver;
	    }

	    public void AddRootType(SchemaXMLEventType type) {
	        if (type.Metadata.AccessModifier != NameAccessModifier.PRECONFIGURED) {
	            throw new IllegalStateException("Type '" + type.Name + "' is not public");
	        }
	        if (rootTypes == null) {
	            rootTypes = new Dictionary<string, SchemaXMLEventType>();
	        }
	        if (rootTypes.ContainsKey(type.Name)) {
	            throw new IllegalStateException("Type '" + type.Name + "' already exists");
	        }
	        rootTypes.Put(type.Name, type);
	    }

	    public EventType GetTypeByName(string derivedEventTypeName) {
	        if (derivedTypes == null) {
	            derivedTypes = new Dictionary<string, SchemaXMLEventType>();
	        }
	        return derivedTypes.Get(derivedEventTypeName);
	    }

	    public EventType GetCreateXMLDOMType(string rootTypeName, string derivedEventTypeName, string moduleName, SchemaElementComplex complex, string representsFragmentOfProperty) {
	        if (rootTypes == null) {
	            rootTypes = new Dictionary<string, SchemaXMLEventType>();
	        }
	        if (derivedTypes == null) {
	            derivedTypes = new Dictionary<string, SchemaXMLEventType>();
	        }
	        SchemaXMLEventType type = rootTypes.Get(rootTypeName);
	        if (type == null) {
	            throw new IllegalStateException("Failed to find XML root event type '" + rootTypeName + "'");
	        }
	        ConfigurationCommonEventTypeXMLDOM config = type.ConfigurationEventTypeXMLDOM;

	        // add a new type
	        ConfigurationCommonEventTypeXMLDOM xmlDom = new ConfigurationCommonEventTypeXMLDOM();
	        xmlDom.RootElementName = "//" + complex.Name;    // such the reload of the type can resolve it
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

	        EventTypeMetadata metadata = new EventTypeMetadata(derivedEventTypeName, moduleName, EventTypeTypeClass.STREAM, EventTypeApplicationType.XML, NameAccessModifier.PRECONFIGURED, EventTypeBusModifier.BUS, false, new EventTypeIdPair(CRC32Util.ComputeCRC32(derivedEventTypeName), -1));
	        SchemaXMLEventType eventType = (SchemaXMLEventType) eventTypeFactory.EventTypeFactory.CreateXMLType(metadata, xmlDom, type.SchemaModel, representsFragmentOfProperty, rootTypeName, eventTypeFactory, this, eventTypeNameResolver);
	        derivedTypes.Put(derivedEventTypeName, eventType);

	        if (optionalCompileTimeRegistry != null) {
	            optionalCompileTimeRegistry.NewType(eventType);
	        }
	        return eventType;
	    }

	    public SchemaXMLEventType GetRootTypeByName(string representsOriginalTypeName) {
	        return rootTypes == null ? null : rootTypes.Get(representsOriginalTypeName);
	    }
	}
} // end of namespace