///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     Event type for variant event streams.
    ///     <para />
    ///     Caches properties after having resolved a property via a resolution strategy.
    /// </summary>
    public class VariantEventType : EventTypeSPI
    {
        private readonly IDictionary<string, VariantPropertyDesc> propertyDesc;
        private readonly IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap;
        private readonly VariantPropResolutionStrategy propertyResStrategy;
        private IDictionary<string, EventPropertyGetter> propertyGetterCodegeneratedCache;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="variantSpec">the variant specification</param>
        /// <param name="metadata">event type metadata</param>
        public VariantEventType(
            EventTypeMetadata metadata,
            VariantSpec variantSpec)
        {
            Metadata = metadata;
            Variants = variantSpec.EventTypes;
            IsVariantAny = variantSpec.TypeVariance == TypeVariance.ANY;
            VariantPropertyGetterCache = new VariantPropertyGetterCache(variantSpec.EventTypes);
            if (IsVariantAny) {
                propertyResStrategy = new VariantPropResolutionStrategyAny(this);
            }
            else {
                propertyResStrategy = new VariantPropResolutionStrategyDefault(this);
            }

            propertyDesc = new Dictionary<string, VariantPropertyDesc>();

            foreach (var type in Variants) {
                var properties = type.PropertyNames;
                properties = CollectionUtil.CopyAndSort(properties);
                foreach (var property in properties) {
                    if (!propertyDesc.ContainsKey(property)) {
                        FindProperty(property);
                    }
                }
            }

            var propertyNameKeySet = propertyDesc.Keys;
            PropertyNames = propertyNameKeySet.ToArray();

            // for each of the properties in each type, attempt to load the property to build a property list
            PropertyDescriptors = new EventPropertyDescriptor[propertyDesc.Count];
            propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            var count = 0;
            foreach (var desc in propertyDesc) {
                var type = desc.Value.PropertyType;
                var descriptor = new EventPropertyDescriptor(
                    desc.Key,
                    type,
                    null,
                    false,
                    false,
                    false,
                    false,
                    desc.Value.PropertyType.IsFragmentableType());
                PropertyDescriptors[count++] = descriptor;
                propertyDescriptorMap.Put(desc.Key, descriptor);
            }
        }

        public EventType[] Variants { get; }

        public VariantPropertyGetterCache VariantPropertyGetterCache { get; }

        public bool IsVariantAny { get; }

        public string StartTimestampPropertyName => null;

        public string EndTimestampPropertyName => null;

        public Type GetPropertyType(string property)
        {
            var entry = propertyDesc.Get(property);
            if (entry != null) {
                return entry.PropertyType;
            }

            entry = FindProperty(property);
            return entry?.PropertyType;
        }

        public Type UnderlyingType => typeof(object);

        public string Name => Metadata.Name;

        public EventPropertyGetterSPI GetGetterSPI(string property)
        {
            var entry = propertyDesc.Get(property);
            if (entry != null) {
                return entry.Getter;
            }

            entry = FindProperty(property);
            return entry?.Getter;
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            if (propertyGetterCodegeneratedCache == null) {
                propertyGetterCodegeneratedCache = new Dictionary<string, EventPropertyGetter>();
            }

            var getter = propertyGetterCodegeneratedCache.Get(propertyName);
            if (getter != null) {
                return getter;
            }

            var getterSPI = GetGetterSPI(propertyName);
            if (getterSPI == null) {
                return null;
            }

            propertyGetterCodegeneratedCache.Put(propertyName, getterSPI);
            return getterSPI;
        }

        public string[] PropertyNames { get; }

        public bool IsProperty(string property)
        {
            var entry = propertyDesc.Get(property);
            if (entry != null) {
                return entry.IsProperty;
            }

            entry = FindProperty(property);
            if (entry != null) {
                return entry.IsProperty;
            }

            return false;
        }

        public EventType[] SuperTypes => null;

        public IEnumerable<EventType> DeepSuperTypes => EmptySet<EventType>.Instance;

        public EventTypeMetadata Metadata { get; private set; }

        public EventPropertyDescriptor[] PropertyDescriptors { get; }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return propertyDescriptorMap.Get(propertyName);
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

        public EventPropertyDescriptor[] WriteableProperties => new EventPropertyDescriptor[0];

        public EventBeanWriter GetWriter(string[] properties)
        {
            return null;
        }

        public EventBeanReader Reader => null;

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
            Metadata = Metadata.WithIds(publicId, protectedId);
        }

        public ICollection<EventType> DeepSuperTypesCollection => Collections.GetEmptySet<EventType>();

        private VariantPropertyDesc FindProperty(string propertyName)
        {
            var desc = propertyResStrategy.ResolveProperty(propertyName, Variants);
            if (desc != null) {
                propertyDesc.Put(propertyName, desc);
            }

            return desc;
        }

        public EventBeanCopyMethod GetCopyMethod(string[] properties)
        {
            return null;
        }

        public void ValidateInsertedIntoEventType(EventType eventType)
        {
            VariantEventTypeUtil.ValidateInsertedIntoEventType(eventType, this);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="theEvent">event</param>
        /// <returns>event</returns>
        public EventBean GetValueAddEventBean(EventBean theEvent)
        {
            return new VariantEventBean(this, theEvent);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="event">event</param>
        /// <returns>event type</returns>
        public EventType EventTypeForNativeObject(object @event)
        {
            if (@event == null) {
                throw new EPException("Null event object returned");
            }

            foreach (var variant in Variants) {
                if (variant is BeanEventType) {
                    var beanEventType = (BeanEventType) variant;
                    if (TypeHelper.IsSubclassOrImplementsInterface(@event.GetType(), beanEventType.UnderlyingType)) {
                        return variant;
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
    }
} // end of namespace