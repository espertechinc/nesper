///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.plugineventrep
{
    public class MyPlugInEventRepresentation : PlugInEventRepresentation
    {
        // Properties shared by all event types, for testing
        private ICollection<String> baseProps;
    
        // Since this implementation also tests dynamic event reflection, keep a list of event types
        private List<MyPlugInPropertiesEventType> types;
    
        public void Init(PlugInEventRepresentationContext context)
        {
            // Load a fixed set of properties from a String initializer, in comma-separated list.
            // Each type we generate will have this base set of properties.
            String initialValues = (String) context.RepresentationInitializer;
            String[] propertyList = (initialValues != null) ? initialValues.Split(',') : new String[0];
            baseProps = new HashSet<String>(propertyList);
    
            types = new List<MyPlugInPropertiesEventType>();
        }
    
        public bool AcceptsType(PlugInEventTypeHandlerContext context)
        {
            return true;
        }
    
        public PlugInEventTypeHandler GetTypeHandler(PlugInEventTypeHandlerContext eventTypeContext)
        {
            String typeProperyies = (String) eventTypeContext.TypeInitializer;
            String[] propertyList = (typeProperyies != null) ? typeProperyies.Split(',') : new String[0];
    
            // the set of properties know are the set of this name as well as the set for the base
            ICollection<String> typeProps = new HashSet<String>(propertyList);
            typeProps.AddAll(baseProps);
    
            IDictionary<String, EventPropertyDescriptor> metadata = new LinkedHashMap<String, EventPropertyDescriptor>();
            foreach (String prop in typeProps)
            {
                metadata.Put(prop, new EventPropertyDescriptor(prop, typeof(string), typeof(char), false, false, true, false, false));
            }
    
            // save type for testing dynamic event object reflection
            MyPlugInPropertiesEventType eventType = new MyPlugInPropertiesEventType(null, eventTypeContext.EventTypeId, typeProps, metadata);
    
            types.Add(eventType);
            
            return new MyPlugInPropertiesEventTypeHandler(eventType);
        }
    
        public bool AcceptsEventBeanResolution(PlugInEventBeanReflectorContext eventBeanContext)
        {
            return true;
        }
    
        public PlugInEventBeanFactory GetEventBeanFactory(PlugInEventBeanReflectorContext eventBeanContext)
        {
            return new MyPlugInPropertiesBeanFactory(types).Create;
        }
    }
}
