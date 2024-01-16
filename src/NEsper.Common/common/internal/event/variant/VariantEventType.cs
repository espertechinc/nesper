///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.core.EventTypeUtility;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    /// Event type for variant event streams.
    /// <para>
    /// Caches properties after having resolved a property via a resolution strategy.
    /// </para>
    /// </summary>
    public class VariantEventType : EventTypeSPI
    {
        private EventTypeMetadata _metadata;
        private readonly EventType[] _variants;
        private readonly bool _variantAny;
        private readonly VariantPropResolutionStrategy _propertyResStrategy;
        private readonly IDictionary<string, VariantPropertyDesc> _propertyDesc;
        private readonly string[] _propertyNames;
        private readonly EventPropertyDescriptor[] _propertyDescriptors;
        private readonly IDictionary<string, EventPropertyDescriptor> _propertyDescriptorMap;
        private readonly VariantPropertyGetterCache _variantPropertyGetterCache;
        private IDictionary<string, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "variantSpec">the variant specification</param>
        /// <param name = "metadata">event type metadata</param>
        public VariantEventType(
            EventTypeMetadata metadata,
            VariantSpec variantSpec)
        {
            _metadata = metadata;
            _variants = variantSpec.EventTypes;
            _variantAny = variantSpec.TypeVariance == TypeVariance.ANY;
            _variantPropertyGetterCache = new VariantPropertyGetterCache(variantSpec.EventTypes);
            if (_variantAny) {
                _propertyResStrategy = new VariantPropResolutionStrategyAny(this);
            }
            else {
                _propertyResStrategy = new VariantPropResolutionStrategyDefault(this);
            }

            _propertyDesc = new Dictionary<string, VariantPropertyDesc>();
            foreach (var type in _variants) {
                var properties = type.PropertyNames;
                properties = CollectionUtil.CopyAndSort(properties);
                foreach (var property in properties) {
                    if (!_propertyDesc.ContainsKey(property)) {
                        FindProperty(property);
                    }
                }
            }

            var propertyNameKeySet = _propertyDesc.Keys;
            _propertyNames = propertyNameKeySet.ToArray();
            
            // for each of the properties in each type, attempt to load the property to build a property list
            _propertyDescriptors = new EventPropertyDescriptor[_propertyDesc.Count];
            _propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            var count = 0;
            foreach (var desc in _propertyDesc) {
                var type = desc.Value.PropertyType;
                var descriptor = new EventPropertyDescriptor(
                    desc.Key,
                    type,
                    false,
                    false,
                    false,
                    false,
                    type.IsFragmentableType());
                _propertyDescriptors[count++] = descriptor;
                _propertyDescriptorMap.Put(desc.Key, descriptor);
            }
        }

        public Type GetPropertyType(string propertyName)
        {
            var entry = _propertyDesc.Get(propertyName);
            if (entry != null) {
                return entry.PropertyType;
            }

            entry = FindProperty(propertyName);
            return entry?.PropertyType;
        }

        public EventPropertyGetterSPI GetGetterSPI(string property)
        {
            var entry = _propertyDesc.Get(property);
            if (entry != null) {
                return entry.Getter;
            }

            entry = FindProperty(property);
            return entry?.Getter;
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            if (_propertyGetterCodegeneratedCache == null) {
                _propertyGetterCodegeneratedCache = new Dictionary<string, EventPropertyGetter>();
            }

            var getter = _propertyGetterCodegeneratedCache.Get(propertyName);
            if (getter != null) {
                return getter;
            }

            var getterSPI = GetGetterSPI(propertyName);
            if (getterSPI == null) {
                return null;
            }

            _propertyGetterCodegeneratedCache.Put(propertyName, getterSPI);
            return getterSPI;
        }

        public bool IsProperty(string property)
        {
            var entry = _propertyDesc.Get(property);
            if (entry != null) {
                return entry.IsProperty;
            }

            entry = FindProperty(property);
            if (entry != null) {
                return entry.IsProperty;
            }

            return false;
        }

        private VariantPropertyDesc FindProperty(string propertyName)
        {
            var desc = _propertyResStrategy.ResolveProperty(propertyName, _variants);
            if (desc != null) {
                _propertyDesc.Put(propertyName, desc);
            }

            return desc;
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }

        public FragmentEventType GetFragmentType(string property)
        {
            return null;
        }

        public EventPropertyWriterSPI GetWriter(string propertyName)
        {
            return null;
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            return null;
        }

        public EventBeanCopyMethod GetCopyMethod(string[] properties)
        {
            return null;
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            return null;
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedProperty)
        {
            return null;
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedProperty)
        {
            return null;
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedProperty)
        {
            return null;
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string propertyName)
        {
            return null;
        }

        public ExprValidationException EqualsCompareType(EventType eventType)
        {
            return Equals(this, eventType) ? null : new ExprValidationException("Variant types mismatch");
        }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            return null;
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            _metadata = _metadata.WithIds(publicId, protectedId);
        }

        public bool IsVariantAny => _variantAny;

        public void ValidateInsertedIntoEventType(EventType eventType)
        {
            VariantEventTypeUtil.ValidateInsertedIntoEventType(eventType, this);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name = "theEvent">event</param>
        /// <returns>event</returns>
        public EventBean GetValueAddEventBean(EventBean theEvent)
        {
            return new VariantEventBean(this, theEvent);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name = "event">event</param>
        /// <returns>event type</returns>
        public EventType EventTypeForNativeObject(object @event)
        {
            if (@event == null) {
                throw new EPException("Null event object returned");
            }

            foreach (var variant in _variants) {
                if (variant is BeanEventType beanEventType) {
                    if (TypeHelper.IsSubclassOrImplementsInterface(@event.GetType(), beanEventType.UnderlyingType)) {
                        return beanEventType;
                    }
                }
            }

            throw new EPException(
                "Failed to determine event type for event object of type '" +
                @event.GetType() +
                "' for use with variant stream '" +
                Name +
                "'");
        }

        public string StartTimestampPropertyName => null;

        public string EndTimestampPropertyName => null;

        public EventType[] Variants => _variants;

        public Type UnderlyingType => typeof(object);

        public string Name => _metadata.Name;

        public VariantPropertyGetterCache VariantPropertyGetterCache => _variantPropertyGetterCache;

        public string[] PropertyNames => _propertyNames;

        public IList<EventType> SuperTypes => null;

        public IEnumerable<EventType> DeepSuperTypes => null;

        public EventTypeMetadata Metadata => _metadata;

        public IList<EventPropertyDescriptor> PropertyDescriptors => _propertyDescriptors;

        public EventPropertyDescriptor[] WriteableProperties => Array.Empty<EventPropertyDescriptor>();

        public ICollection<EventType> DeepSuperTypesCollection => EmptySet<EventType>.Instance;
    }
} // end of namespace