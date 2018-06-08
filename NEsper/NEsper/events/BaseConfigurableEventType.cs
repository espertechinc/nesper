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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.events
{
    /// <summary>
    /// EventType than can be supplied with a preconfigured list of properties getters
    /// (aka. explicit properties).
    /// </summary>
    public abstract class BaseConfigurableEventType : EventTypeSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventAdapterService _eventAdapterService;
        private readonly int _eventTypeId;
        private readonly EventTypeMetadata _metadata;
        private readonly Type _underlyngType;
        private EventPropertyDescriptor[] _propertyDescriptors;
        private String[] _propertyNames;
        private IDictionary<String, Pair<ExplicitPropertyDescriptor, FragmentEventType>> _propertyFragmentTypes;
        private readonly ILockable _iLock;
        private IDictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        /// <summary>
        /// Getters for each known property.
        /// </summary>
        protected IDictionary<String, EventPropertyGetterSPI> PropertyGetters;

        /// <summary>
        /// Descriptors for each known property.
        /// </summary>
        protected IDictionary<String, EventPropertyDescriptor> PropertyDescriptorMap;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="eventAdapterService">for dynamic event type creation</param>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="underlyngType">is the underlying type returned by the event type</param>
        protected BaseConfigurableEventType(
            ILockManager lockManager,
            EventAdapterService eventAdapterService,
            EventTypeMetadata metadata, 
            int eventTypeId,
            Type underlyngType)
        {
            _iLock = lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _eventTypeId = eventTypeId;
            _eventAdapterService = eventAdapterService;
            _metadata = metadata;
            _underlyngType = underlyngType;
        }

        /// <summary>
        /// Subclasses must implement this and supply a getter to a given property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>
        /// getter for property
        /// </returns>
        protected abstract EventPropertyGetterSPI DoResolvePropertyGetter(String property);

        /// <summary>
        /// Subclasses must implement this and return a type for a property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>
        /// property type
        /// </returns>
        protected abstract Type DoResolvePropertyType(String property);

        /// <summary>
        /// Subclasses must implement this and return a fragment type for a property.
        /// </summary>
        /// <param name="property">is the property expression</param>
        /// <returns>
        /// fragment property type
        /// </returns>
        protected abstract FragmentEventType DoResolveFragmentType(String property);

        public int EventTypeId => _eventTypeId;

        public string Name => _metadata.PrimaryName;

        /// <summary>
        /// Returns the event adapter service.
        /// </summary>
        /// <returns>
        /// event adapter service
        /// </returns>
        public EventAdapterService EventAdapterService => _eventAdapterService;

        /// <summary>
        /// Sets explicit properties using a map of event property name and getter instance
        /// for each property.
        /// </summary>
        /// <param name="explicitProperties">property descriptors for explicit properties</param>
        protected void Initialize(ICollection<ExplicitPropertyDescriptor> explicitProperties)
        {
            PropertyGetters = new Dictionary<String, EventPropertyGetterSPI>();
            _propertyDescriptors = new EventPropertyDescriptor[explicitProperties.Count];
            _propertyNames = new String[explicitProperties.Count];
            PropertyDescriptorMap = new Dictionary<String, EventPropertyDescriptor>();
            _propertyFragmentTypes = new Dictionary<String, Pair<ExplicitPropertyDescriptor, FragmentEventType>>();

            int count = 0;
            foreach (ExplicitPropertyDescriptor propertyDescriptor in explicitProperties)
            {
                _propertyNames[count] = propertyDescriptor.Descriptor.PropertyName;
                PropertyGetters.Put(propertyDescriptor.Descriptor.PropertyName, propertyDescriptor.Getter);
                EventPropertyDescriptor desc = propertyDescriptor.Descriptor;
                _propertyDescriptors[count] = desc;
                PropertyDescriptorMap.Put(desc.PropertyName, desc);

                if (propertyDescriptor.OptionalFragmentTypeName != null)
                {
                    _propertyFragmentTypes.Put(propertyDescriptor.Descriptor.PropertyName,
                                              new Pair<ExplicitPropertyDescriptor, FragmentEventType>(
                                                  propertyDescriptor, null));
                }

                if (!desc.IsFragment)
                {
                    _propertyFragmentTypes.Put(propertyDescriptor.Descriptor.PropertyName, null);
                }

                count++;
            }
        }

        public Type GetPropertyType(String propertyExpression)
        {
            EventPropertyDescriptor desc = PropertyDescriptorMap.Get(propertyExpression);
            if (desc != null)
            {
                return desc.PropertyType;
            }

            return DoResolvePropertyType(propertyExpression);
        }

        public Type UnderlyingType => _underlyngType;

        public EventPropertyGetterSPI GetGetterSPI(String propertyExpression)
        {
            var getter = PropertyGetters.Get(propertyExpression);
            if (getter != null)
            {
                return getter;
            }

            return DoResolvePropertyGetter(propertyExpression);
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
            _propertyGetterCodegeneratedCache.Put(propertyName, getterCode);
            return getterCode;
        }

        public EventPropertyGetterMapped GetGetterMapped(String mappedProperty)
        {
            EventPropertyGetter getter = GetGetter(mappedProperty);
            if (getter is EventPropertyGetterMapped)
            {
                return (EventPropertyGetterMapped) getter;
            }

            return null;
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedProperty)
        {
            return null;
        }

        public FragmentEventType GetFragmentType(String property)
        {
            using (_iLock.Acquire())
            {
                Pair<ExplicitPropertyDescriptor, FragmentEventType> pair = _propertyFragmentTypes.Get(property);
                if (pair == null)
                {
                    return _propertyFragmentTypes.ContainsKey(property) ? null : DoResolveFragmentType(property);
                }

                // if a type is assigned, use that
                if (pair.Second != null)
                {
                    return pair.Second;
                }

                // resolve event type
                EventType existingType = _eventAdapterService.GetEventTypeByName(pair.First.OptionalFragmentTypeName);
                if (!(existingType is BaseConfigurableEventType))
                {
                    Log.Warn("Type configured for fragment event property '" + property + "' by name '" + pair.First +
                             "' could not be found");
                    return null;
                }

                var fragmentType = new FragmentEventType(existingType, pair.First.IsFragmentArray, false);
                pair.Second = fragmentType;
                return fragmentType;
            }
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public bool IsProperty(String property)
        {
            return (GetGetter(property) != null);
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            return PropertyDescriptorMap.Get(propertyName);
        }

        /// <summary>
        /// Returns an enumeration of event types that are super to this event type, from which this event type inherited event properties.
        /// <para/>
        /// For PONO instances underlying the event this method returns the event types for all superclasses extended by
        /// the PONO and all interfaces implemented by the PONO.
        /// </summary>
        /// <value>The super types.</value>
        /// <returns>an array of event types</returns>
        public abstract EventType[] SuperTypes { get; }

        /// <summary>
        /// Returns enumerator over all super types to event type, going up the hierarchy and including all interfaces (and their
        /// extended interfaces) and superclasses as EventType instances.
        /// </summary>
        /// <value>The deep super types.</value>
        public abstract EventType[] DeepSuperTypes { get; }

        public abstract EventPropertyWriter GetWriter(string propertyName);
        public abstract EventPropertyDescriptor[] WriteableProperties { get; }
        public abstract EventPropertyDescriptor GetWritableProperty(string propertyName);
        public abstract EventBeanCopyMethod GetCopyMethod(string[] properties);
        public abstract EventBeanWriter GetWriter(string[] properties);
        public abstract EventBeanReader Reader { get; }
        public abstract string StartTimestampPropertyName { get; }
        public abstract string EndTimestampPropertyName { get; }
        public abstract bool EqualsCompareType(EventType eventType);
    }
}
