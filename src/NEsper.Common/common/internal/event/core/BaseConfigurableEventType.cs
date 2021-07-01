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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     EventType than can be supplied with a preconfigured list of properties getters (aka. explicit properties).
    /// </summary>
    /// <author>pablo</author>
    public abstract class BaseConfigurableEventType : EventTypeSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BaseConfigurableEventType));

        /// <summary>
        ///     Descriptors for each known property.
        /// </summary>
        private IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap;

        private IDictionary<string, Pair<ExplicitPropertyDescriptor, FragmentEventType>> propertyFragmentTypes;
        private IDictionary<string, EventPropertyGetter> propertyGetterCodegeneratedCache;

        /// <summary>
        ///     Getters for each known property.
        /// </summary>
        internal IDictionary<string, EventPropertyGetterSPI> propertyGetters;

        protected BaseConfigurableEventType(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeMetadata metadata,
            Type underlyingType,
            EventTypeNameResolver eventTypeResolver,
            XMLFragmentEventTypeFactory xmlEventTypeFactory)
        {
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
            Metadata = metadata;
            UnderlyingType = underlyingType;
            EventTypeResolver = eventTypeResolver;
            XmlEventTypeFactory = xmlEventTypeFactory;
        }

        /// <summary>
        ///     Returns the event adapter service.
        /// </summary>
        /// <returns>event adapter service</returns>
        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EventTypeNameResolver EventTypeResolver { get; }

        public XMLFragmentEventTypeFactory XmlEventTypeFactory { get; }

        public IDictionary<string, EventPropertyDescriptor> PropertyDescriptorMap => propertyDescriptorMap;

        public string Name => Metadata.Name;

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            Metadata = Metadata.WithIds(publicId, protectedId);
        }

        public Type GetPropertyType(string propertyExpression)
        {
            var desc = propertyDescriptorMap.Get(propertyExpression);
            if (desc != null) {
                return desc.PropertyType;
            }

            return DoResolvePropertyType(propertyExpression);
        }

        public Type UnderlyingType { get; }

        public EventPropertyGetterSPI GetGetterSPI(string propertyExpression)
        {
            var getter = propertyGetters.Get(propertyExpression);
            if (getter != null) {
                return getter;
            }

            return DoResolvePropertyGetter(propertyExpression);
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            return GetGetterSPI(propertyName);
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedProperty)
        {
            return GetGetterMappedSPI(mappedProperty);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedProperty)
        {
            var getter = GetGetter(mappedProperty);
            return getter as EventPropertyGetterMappedSPI;
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string propertyName)
        {
            return null;
        }

        public FragmentEventType GetFragmentType(string property)
        {
            lock (this) {
                var pair = propertyFragmentTypes.Get(property);
                if (pair == null) {
                    if (propertyFragmentTypes.ContainsKey(property)) {
                        return null;
                    }

                    return DoResolveFragmentType(property);
                }

                // if a type is assigned, use that
                if (pair.Second != null) {
                    return pair.Second;
                }

                // resolve event type
                var existingType = EventTypeResolver.GetTypeByName(pair.First.OptionalFragmentTypeName);
                if (!(existingType is BaseConfigurableEventType)) {
                    Log.Warn(
                        "Type configured for fragment event property '" +
                        property +
                        "' by name '" +
                        pair.First +
                        "' could not be found");
                    return null;
                }

                var fragmentType = new FragmentEventType(existingType, pair.First.IsFragmentArray, false);
                pair.Second = fragmentType;
                return fragmentType;
            }
        }

        public string[] PropertyNames { get; private set; }

        public bool IsProperty(string property)
        {
            return GetGetter(property) != null;
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors { get; private set; }

        public EventTypeMetadata Metadata { get; private set; }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return propertyDescriptorMap.Get(propertyName);
        }

        EventPropertyGetterIndexed EventType.GetGetterIndexed(string indexedPropertyName)
        {
            return GetGetterIndexed(indexedPropertyName);
        }

        public abstract IList<EventType> SuperTypes { get; }
        public abstract IEnumerable<EventType> DeepSuperTypes { get; }
        public abstract ICollection<EventType> DeepSuperTypesCollection { get; }
        public abstract string StartTimestampPropertyName { get; }
        public abstract string EndTimestampPropertyName { get; }
        public abstract EventPropertyDescriptor[] WriteableProperties { get; }
        public abstract EventPropertyWriterSPI GetWriter(string propertyName);
        public abstract EventPropertyDescriptor GetWritableProperty(string propertyName);
        public abstract EventBeanCopyMethodForge GetCopyMethodForge(string[] properties);
        public abstract EventBeanWriter GetWriter(string[] properties);
        public abstract ExprValidationException EqualsCompareType(EventType eventType);

        /// <summary>
        ///     Subclasses must implement this and supply a getter to a given property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>getter for property</returns>
        protected abstract EventPropertyGetterSPI DoResolvePropertyGetter(string property);

        /// <summary>
        ///     Subclasses must implement this and return a type for a property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>property type</returns>
        protected abstract Type DoResolvePropertyType(string property);

        /// <summary>
        ///     Subclasses must implement this and return a fragment type for a property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>fragment property type</returns>
        protected abstract FragmentEventType DoResolveFragmentType(string property);

        /// <summary>
        ///     Sets explicit properties using a map of event property name and getter instance for each property.
        /// </summary>
        /// <param name="explicitProperties">property descriptors for explicit properties</param>
        protected void Initialize(IList<ExplicitPropertyDescriptor> explicitProperties)
        {
            propertyGetters = new Dictionary<string, EventPropertyGetterSPI>();
            PropertyDescriptors = new EventPropertyDescriptor[explicitProperties.Count];
            PropertyNames = new string[explicitProperties.Count];
            propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            propertyFragmentTypes = new Dictionary<string, Pair<ExplicitPropertyDescriptor, FragmentEventType>>();

            var count = 0;
            foreach (var @explicit in explicitProperties) {
                PropertyNames[count] = @explicit.Descriptor.PropertyName;
                propertyGetters.Put(@explicit.Descriptor.PropertyName, @explicit.Getter);
                
                var desc = @explicit.Descriptor;
                PropertyDescriptors[count] = desc;
                propertyDescriptorMap.Put(desc.PropertyName, desc);

                if (@explicit.OptionalFragmentTypeName != null) {
                    propertyFragmentTypes.Put(
                        @explicit.Descriptor.PropertyName,
                        new Pair<ExplicitPropertyDescriptor, FragmentEventType>(@explicit, null));
                }

                if (!desc.IsFragment) {
                    propertyFragmentTypes.Put(@explicit.Descriptor.PropertyName, null);
                }

                count++;
            }
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexed(string indexedProperty)
        {
            return null;
        }
    }
} // end of namespace