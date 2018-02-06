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
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Event type for variant event streams.
    /// <para/>
    /// Caches properties after having resolved a property via a resolution strategy.
    /// </summary>
    public class VariantEventType : EventTypeSPI
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventTypeMetadata _metadata;
        private readonly EventType[] _variants;
        private readonly VariantPropResolutionStrategy _propertyResStrategy;
        private readonly IDictionary<String, VariantPropertyDesc> _propertyDesc;
        private readonly String[] _propertyNames;
        private readonly EventPropertyDescriptor[] _propertyDescriptors;
        private readonly IDictionary<String, EventPropertyDescriptor> _propertyDescriptorMap;
        private readonly int _eventTypeId;
        private readonly ConfigurationVariantStream _config;
        private IDictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="variantSpec">the variant specification</param>
        /// <param name="propertyResStrategy">stragegy for resolving properties</param>
        /// <param name="config">The config.</param>
        public VariantEventType(
            EventAdapterService eventAdapterService,
            EventTypeMetadata metadata, 
            int eventTypeId, 
            VariantSpec variantSpec,
            VariantPropResolutionStrategy propertyResStrategy, 
            ConfigurationVariantStream config)
        {
            _eventAdapterService = eventAdapterService;
            _metadata = metadata;
            _eventTypeId = eventTypeId;
            _variants = variantSpec.EventTypes;
            _propertyResStrategy = propertyResStrategy;
            _config = config;
            
            _propertyDesc = new Dictionary<String, VariantPropertyDesc>();
    
            foreach (EventType type in _variants)
            {
                IList<string> properties = type.PropertyNames;
                properties = PropertyUtility.CopyAndSort(properties);
                foreach (String property in properties)
                {
                    if (!_propertyDesc.ContainsKey(property))
                    {
                        FindProperty(property);
                    }
                }
            }
    
            ICollection<String> propertyNameKeySet = _propertyDesc.Keys;
            _propertyNames = propertyNameKeySet.ToArray();
    
            // for each of the properties in each type, attempt to load the property to build a property list
            _propertyDescriptors = new EventPropertyDescriptor[_propertyDesc.Count];
            _propertyDescriptorMap = new Dictionary<String, EventPropertyDescriptor>();
            int count = 0;
            foreach (var desc in _propertyDesc)
            {
                var type = desc.Value.PropertyType;
                var indexType = type.GetIndexType();
                var isIndexed = indexType != null;
                var descriptor = new EventPropertyDescriptor(desc.Key, type, indexType, false, false, isIndexed, false, desc.Value.PropertyType.IsFragmentableType());
                _propertyDescriptors[count++] = descriptor;
                _propertyDescriptorMap.Put(desc.Key, descriptor);
            }
        }

        public string StartTimestampPropertyName
        {
            get { return null; }
        }

        public string EndTimestampPropertyName
        {
            get { return null; }
        }

        public Type GetPropertyType(String property)
        {
            VariantPropertyDesc entry = _propertyDesc.Get(property);
            if (entry != null)
            {
                return entry.PropertyType;
            }
            entry = FindProperty(property);
            if (entry != null)
            {
                return entry.PropertyType;
            }
            return null;
        }

        public Type UnderlyingType
        {
            get { return typeof (Object); }
        }

        public string Name
        {
            get { return _metadata.PublicName; }
        }

        public int EventTypeId
        {
            get { return _eventTypeId; }
        }

        public ConfigurationVariantStream Config
        {
            get { return _config; }
        }

        public EventPropertyGetterSPI GetGetterSPI(String property)
        {
            VariantPropertyDesc entry = _propertyDesc.Get(property);
            if (entry != null)
            {
                return entry.Getter;
            }
            entry = FindProperty(property);
            if (entry != null)
            {
                return entry.Getter;
            }
            return null;
        }

        public EventPropertyGetter GetGetter(String propertyName)
        {
            if (!_eventAdapterService.EngineImportService.IsCodegenEventPropertyGetters)
            {
                return GetGetterSPI(propertyName);
            }
            if (_propertyGetterCodegeneratedCache == null)
            {
                _propertyGetterCodegeneratedCache = new Dictionary<string, EventPropertyGetter>();
            }

            var getter = _propertyGetterCodegeneratedCache.Get(propertyName);
            if (getter != null)
            {
                return getter;
            }

            var getterSPI = GetGetterSPI(propertyName);
            if (getterSPI == null)
            {
                return null;
            }

            var getterCode = _eventAdapterService.EngineImportService.CodegenGetter(getterSPI, propertyName);
            _propertyGetterCodegeneratedCache[propertyName] = getterCode;
            return getterCode;
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public bool IsProperty(String property)
        {
            VariantPropertyDesc entry = _propertyDesc.Get(property);
            if (entry != null)
            {
                return entry.IsProperty;
            }
            entry = FindProperty(property);
            if (entry != null)
            {
                return entry.IsProperty;
            }
            return false;
        }

        public EventType[] SuperTypes
        {
            get { return null; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return null; }
        }

        private VariantPropertyDesc FindProperty(String propertyName)
        {
            var desc = _propertyResStrategy.ResolveProperty(propertyName, _variants);
            if (desc != null)
            {
                _propertyDesc.Put(propertyName, desc);
            }

            return desc;
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }    
    
        public FragmentEventType GetFragmentType(String property)
        {
            return null;
        }
    
        public EventPropertyWriter GetWriter(String propertyName)
        {
            return null;
        }
    
        public EventPropertyDescriptor GetWritableProperty(String propertyName)
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
    
        public EventBeanWriter GetWriter(String[] properties)
        {
            return null;
        }

        public EventBeanReader Reader
        {
            get { return null; }
        }

        public EventPropertyGetterMapped GetGetterMapped(String mappedProperty) 
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
}
