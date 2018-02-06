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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Event type of revision events.</summary>
    public class RevisionEventType : EventTypeSPI
    {
        private readonly EventTypeMetadata _metadata;
        private readonly int _eventTypeId;
        private readonly string[] _propertyNames;
        private readonly EventPropertyDescriptor[] _propertyDescriptors;
        private readonly IDictionary<string, EventPropertyDescriptor> _propertyDescriptorMap;
        private readonly IDictionary<string, RevisionPropertyTypeDesc> _propertyDesc;
        private readonly EventAdapterService _eventAdapterService;
        private IDictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyDesc">describes each properties type</param>
        /// <param name="eventAdapterService">for nested property handling</param>
        /// <param name="metadata">- event type metadata</param>
        /// <param name="eventTypeId">type id</param>
        public RevisionEventType(
            EventTypeMetadata metadata,
            int eventTypeId,
            IDictionary<string, RevisionPropertyTypeDesc> propertyDesc,
            EventAdapterService eventAdapterService)
        {
            _metadata = metadata;
            _eventTypeId = eventTypeId;
            _propertyDesc = propertyDesc;
            _propertyNames = propertyDesc.Keys.ToArray();
            _eventAdapterService = eventAdapterService;

            _propertyDescriptors = new EventPropertyDescriptor[propertyDesc.Count];
            _propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            var count = 0;
            foreach (var desc in propertyDesc)
            {
                var type = (Type) desc.Value.PropertyType;
                var descriptor = new EventPropertyDescriptor(
                    desc.Key, type, null, false, false, false, false, type.IsFragmentableType());
                _propertyDescriptors[count] = descriptor;
                _propertyDescriptorMap.Put(desc.Key, descriptor);
                count++;
            }
        }

        public int EventTypeId => _eventTypeId;

        public string StartTimestampPropertyName => null;

        public string EndTimestampPropertyName => null;

        public EventPropertyGetterSPI GetGetterSPI(string propertyName)
        {
            var desc = _propertyDesc.Get(propertyName);
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
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (prop is SimpleProperty)
                {
                    // there is no such property since it wasn't found earlier
                    return null;
                }
                string atomic = null;
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
                var nestedClass = (Type) desc.PropertyType;
                var complexProperty = (BeanEventType) _eventAdapterService.AddBeanType(nestedClass.GetDefaultTypeName(), nestedClass, false, false, false);
                return prop.GetGetter(complexProperty, _eventAdapterService);
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is a Java object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);

            desc = _propertyDesc.Get(propertyMap);
            if (desc == null)
            {
                return null; // prefix not a known property
            }

            // only nested classes supported for revision event types since deep property information not currently exposed by EventType
            if (desc.PropertyType is Type)
            {
                // ask the nested class to resolve the property
                var simpleClass = (Type) desc.PropertyType;
                var nestedEventType = (EventTypeSPI) _eventAdapterService.AddBeanType(
                    simpleClass.Name, simpleClass, false, false, false);
                var nestedGetter = nestedEventType.GetGetterSPI(propertyNested);
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

        public virtual EventPropertyGetter GetGetter(string propertyName)
        {
            if (!_eventAdapterService.EngineImportService.IsCodegenEventPropertyGetters)
            {
                return GetGetterSPI(propertyName);
            }
            if (_propertyGetterCodegeneratedCache == null)
            {
                _propertyGetterCodegeneratedCache = new Dictionary<string, EventPropertyGetter>(); 
            }

            EventPropertyGetter getter = _propertyGetterCodegeneratedCache.Get(propertyName);
            if (getter != null)
            {
                return getter;
            }

            EventPropertyGetterSPI getterSPI = GetGetterSPI(propertyName);
            if (getterSPI == null)
            {
                return null;
            }

            EventPropertyGetter getterCode = _eventAdapterService.EngineImportService.CodegenGetter(getterSPI, propertyName);
            _propertyGetterCodegeneratedCache.Put(propertyName, getterCode);
            return getterCode;
        }

        public string Name => _metadata.PublicName;

        public Type GetPropertyType(string propertyName)
        {
            var desc = _propertyDesc.Get(propertyName);
            if (desc != null)
            {
                if (desc.PropertyType is Type)
                {
                    return (Type) desc.PropertyType;
                }
                return null;
            }

            // dynamic property names note allowed
            if (propertyName.IndexOf('?') != -1)
            {
                return null;
            }

            // see if this is a nested property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                return null;
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is a Java object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);

            desc = _propertyDesc.Get(propertyMap);
            if (desc == null)
            {
                return null; // prefix not a known property
            }
            else if (desc.PropertyType is Type)
            {
                var simpleClass = (Type) desc.PropertyType;
                var nestedEventType = _eventAdapterService.AddBeanType(simpleClass.GetDefaultTypeName(), simpleClass, false, false, false);
                return nestedEventType.GetPropertyType(propertyNested);
            }
            else
            {
                return null;
            }
        }

        public Type UnderlyingType => typeof (RevisionEventType);

        public string[] PropertyNames => _propertyNames;

        public bool IsProperty(string property)
        {
            return GetPropertyType(property) != null;
        }

        public EventType[] SuperTypes => null;

        public EventType[] DeepSuperTypes => null;

        public EventTypeMetadata Metadata => _metadata;

        public IList<EventPropertyDescriptor> PropertyDescriptors => _propertyDescriptors;

        public FragmentEventType GetFragmentType(string property)
        {
            return null;
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }

        public EventPropertyWriter GetWriter(string propertyName)
        {
            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties => new EventPropertyDescriptor[0];

        public EventBeanCopyMethod GetCopyMethod(string[] properties)
        {
            return null;
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            return null;
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            return null;
        }

        public EventBeanReader Reader => null;

        public EventPropertyGetterMapped GetGetterMapped(string mappedProperty)
        {
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
} // end of namespace
