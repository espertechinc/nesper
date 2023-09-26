///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

//using static com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     EventType than can be supplied with a preconfigured list of properties getters (aka. @explicit properties).
    /// </summary>
    /// <author>pablo</author>
    public abstract class BaseConfigurableEventType : EventTypeSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventTypeNameResolver eventTypeResolver;

        /// <summary>
        ///     Descriptors for each known property.
        /// </summary>
        protected IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap;

        private EventPropertyDescriptor[] propertyDescriptors;
        private IDictionary<string, Pair<ExplicitPropertyDescriptor, FragmentEventType>> propertyFragmentTypes;
        private IDictionary<string, EventPropertyGetter> propertyGetterCodegeneratedCache;

        /// <summary>
        ///     Getters for each known property.
        /// </summary>
        protected IDictionary<string, EventPropertyGetterSPI> propertyGetters;

        private readonly XMLFragmentEventTypeFactory xmlEventTypeFactory;

        protected BaseConfigurableEventType(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeMetadata metadata,
            Type underlyingType,
            EventTypeNameResolver eventTypeResolver,
            XMLFragmentEventTypeFactory xmlEventTypeFactory)
        {
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            Metadata = metadata;
            UnderlyingType = underlyingType;
            this.eventTypeResolver = eventTypeResolver;
            this.xmlEventTypeFactory = xmlEventTypeFactory;
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            Metadata = Metadata.WithIds(publicId, protectedId);
        }

        public Type GetPropertyType(string propertyName)
        {
            var desc = propertyDescriptorMap.Get(propertyName);
            if (desc != null) {
                return desc.PropertyType;
            }

            return DoResolvePropertyType(propertyName);
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
                var existingType = eventTypeResolver.GetTypeByName(pair.First.OptionalFragmentTypeName);
                if (!(existingType is BaseConfigurableEventType)) {
                    Log.Warn(
                        "Type configured for fragment event property '" +
                        property +
                        "' by name '" +
                        pair.First +
                        "' could not be found");
                    return null;
                }

                var fragmentType = new FragmentEventType(existingType, pair.First.IsFragmentArray, false, false);
                pair.Second = fragmentType;
                return fragmentType;
            }
        }

        public string[] PropertyNames { get; private set; }

        public bool IsProperty(string property)
        {
            return GetGetter(property) != null;
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors => propertyDescriptors;
        public EventTypeMetadata Metadata { get; private set; }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return propertyDescriptorMap.Get(propertyName);
        }

        /// <summary>
        ///     Subclasses must implement this and supply a getter to a given property.
        /// </summary>
        /// <param name = "property">is the property expression</param>
        /// <returns>getter for property</returns>
        protected abstract EventPropertyGetterSPI DoResolvePropertyGetter(string property);

        /// <summary>
        ///     Subclasses must implement this and return a type for a property.
        /// </summary>
        /// <param name = "property">is the property expression</param>
        /// <returns>property type</returns>
        protected abstract Type DoResolvePropertyType(string property);

        /// <summary>
        ///     Subclasses must implement this and return a fragment type for a property.
        /// </summary>
        /// <param name = "property">is the property expression</param>
        /// <returns>fragment property type</returns>
        protected abstract FragmentEventType DoResolveFragmentType(string property);

        /// <summary>
        ///     Sets @explicit properties using a map of event property name and getter instance for each property.
        /// </summary>
        /// <param name = "explicitProperties">property descriptors for @explicit properties</param>
        protected void Initialize(IList<ExplicitPropertyDescriptor> explicitProperties)
        {
            propertyGetters = new Dictionary<string, EventPropertyGetterSPI>();
            propertyDescriptors = new EventPropertyDescriptor[explicitProperties.Count];
            PropertyNames = new string[explicitProperties.Count];
            propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            propertyFragmentTypes = new Dictionary<string, Pair<ExplicitPropertyDescriptor, FragmentEventType>>();
            var count = 0;
            foreach (var @explicit in explicitProperties) {
                PropertyNames[count] = @explicit.Descriptor.PropertyName;
                propertyGetters.Put(@explicit.Descriptor.PropertyName, @explicit.Getter);
                var desc = @explicit.Descriptor;
                propertyDescriptors[count] = desc;
                propertyDescriptorMap.Put(desc.PropertyName, desc);
                if (@explicit.OptionalFragmentTypeName != null) {
                    propertyFragmentTypes.Put(
                        @explicit.Descriptor.PropertyName,
                        new Pair<ExplicitPropertyDescriptor, FragmentEventType>(@explicit, null));
                }

                if (!desc.IsFragment) {
                    propertyFragmentTypes.Put(@explicit.Descriptor.PropertyName);
                }

                count++;
            }
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedProperty)
        {
            return null;
        }

        public string Name => Metadata.Name;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => eventBeanTypedEventFactory;

        public EventTypeNameResolver EventTypeResolver => eventTypeResolver;

        public XMLFragmentEventTypeFactory XmlEventTypeFactory => xmlEventTypeFactory;

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
    }
} // end of namespace