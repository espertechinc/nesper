///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Event type of revision events.
    /// </summary>
    public class RevisionEventType : EventTypeSPI
    {
        private readonly EventTypeMetadata _metadata;
        private readonly int _eventTypeId;
        private readonly String[] _propertyNames;
        private readonly EventPropertyDescriptor[] _propertyDescriptors;
        private readonly IDictionary<String, EventPropertyDescriptor> _propertyDescriptorMap;
        private readonly IDictionary<String, RevisionPropertyTypeDesc> _propertyDesc;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="propertyDesc">describes each properties type</param>
        /// <param name="eventAdapterService">for nested property handling</param>
        public RevisionEventType(EventTypeMetadata metadata, int eventTypeId, IDictionary<String, RevisionPropertyTypeDesc> propertyDesc, EventAdapterService eventAdapterService)
        {
            _metadata = metadata;
            _eventTypeId = eventTypeId;
            _propertyDesc = propertyDesc;
            _propertyNames = propertyDesc.Keys.ToArray();
            _eventAdapterService = eventAdapterService;
    
            _propertyDescriptors = new EventPropertyDescriptor[propertyDesc.Count];
            _propertyDescriptorMap = new Dictionary<String, EventPropertyDescriptor>();
            int count = 0;
            foreach (KeyValuePair<String, RevisionPropertyTypeDesc> desc in propertyDesc)
            {
                var type = (Type) desc.Value.PropertyType;
                var indexType = type.GetIndexType();
                var isIndexed = indexType != null;
                var descriptor = new EventPropertyDescriptor(desc.Key, type, indexType, false, false, isIndexed, false, type.IsFragmentableType());
                _propertyDescriptors[count] = descriptor;
                _propertyDescriptorMap.Put(desc.Key, descriptor);
                count++;
            }
        }

        public int EventTypeId
        {
            get { return _eventTypeId; }
        }

        public string StartTimestampPropertyName
        {
            get { return null; }
        }

        public string EndTimestampPropertyName
        {
            get { return null; }
        }

        public EventPropertyGetter GetGetter(String propertyName)
        {
            RevisionPropertyTypeDesc desc = _propertyDesc.Get(propertyName);
            if (desc != null)
            {
                return desc.RevisionGetter;
            }
    
            // dynamic property names note allowed
            if (propertyName.IndexOf('?') != -1)
            {
                return null;
            }
    
            // see if this is a nested property
            int index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                Property prop = PropertyParser.ParseAndWalk(propertyName);
                if (prop is SimpleProperty)
                {
                    // there is no such property since it wasn't found earlier
                    return null;
                }
                String atomic = null;
                if (prop is IndexedProperty)
                {
                    var indexedprop = (IndexedProperty) prop;
                    atomic = indexedprop.PropertyNameAtomic;
                }
                if (prop is MappedProperty)
                {
                    var indexedprop = (MappedProperty) prop;
                    atomic = indexedprop.PropertyNameAtomic;
                }
                desc = _propertyDesc.Get(atomic);
                if (desc == null)
                {
                    return null;
                }
                if (!(desc.PropertyType is Type))
                {
                    return null;
                }
                var nestedClass = (Type)desc.PropertyType;
                var complexProperty = (BeanEventType) _eventAdapterService.AddBeanType(nestedClass.Name, nestedClass, false, false, false);
                return prop.GetGetter(complexProperty, _eventAdapterService);
            }
    
            // Map event types allow 2 types of properties inside:
            //   - a property that is a Java object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
    
            // Take apart the nested property into a map key and a nested value class property name
            String propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            String propertyNested = propertyName.Substring(index + 1);
    
            desc = _propertyDesc.Get(propertyMap);
            if (desc == null)
            {
                return null;  // prefix not a known property
            }
    
            // only nested classes supported for revision event types since deep property information not currently exposed by EventType
            if (desc.PropertyType is Type)
            {
                // ask the nested class to resolve the property
                Type simpleClass = (Type)desc.PropertyType;
                EventType nestedEventType = _eventAdapterService.AddBeanType(simpleClass.FullName, simpleClass, false, false, false);
                EventPropertyGetter nestedGetter = nestedEventType.GetGetter(propertyNested);
                if (nestedGetter == null)
                {
                    return null;
                }
    
                // construct getter for nested property
                return new RevisionNestedPropertyGetter(desc.RevisionGetter, nestedGetter, _eventAdapterService);
            }
            else
            {
                return null;
            }
        }

        public string Name
        {
            get { return _metadata.PublicName; }
        }

        public Type GetPropertyType(String propertyName)
        {
            RevisionPropertyTypeDesc desc = _propertyDesc.Get(propertyName);
            if (desc != null)
            {
                if (desc.PropertyType is Type)
                {
                    return (Type)desc.PropertyType;
                }
                return null;
            }
    
            // dynamic property names note allowed
            if (propertyName.IndexOf('?') != -1)
            {
                return null;
            }
    
            // see if this is a nested property
            int index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                return null;
            }
    
            // Map event types allow 2 types of properties inside:
            //   - a property that is a Java object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
    
            // Take apart the nested property into a map key and a nested value class property name
            String propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            String propertyNested = propertyName.Substring(index + 1);
    
            desc = _propertyDesc.Get(propertyMap);
            if (desc == null)
            {
                return null;  // prefix not a known property
            }

            else if (desc.PropertyType is Type)
            {
                Type simpleClass = (Type)desc.PropertyType;
                EventType nestedEventType = _eventAdapterService.AddBeanType(simpleClass.FullName, simpleClass, false, false, false);
                return nestedEventType.GetPropertyType(propertyNested);
            }
            else
            {
                return null;
            }
        }

        public Type UnderlyingType
        {
            get { return typeof (RevisionEventType); }
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public bool IsProperty(String property)
        {
            return GetPropertyType(property) != null;
        }

        public EventType[] SuperTypes
        {
            get { return null; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return null; }
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public FragmentEventType GetFragmentType(String property)
        {
            return null;
        }
    
        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }
    
        public EventPropertyWriter GetWriter(String propertyName)
        {
            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties
        {
            get { return new EventPropertyDescriptor[0]; }
        }

        public EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            return null;
        }
    
        public EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            return null;
        }
    
        public EventBeanWriter GetWriter(String[] properties)
        {
            return null;
        }

        public EventBeanReader GetReader()
        {
            return null;
        }

        public EventPropertyGetterMapped GetGetterMapped(String mappedProperty) {
            return null;
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedProperty)
        {
            return null;
        }

        public bool EqualsCompareType(EventType eventType)
        {
            return this == eventType;
        }
    }
}
