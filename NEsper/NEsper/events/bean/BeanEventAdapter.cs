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

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// A factory for <seealso cref="BeanEventType"/> instances based on type information and 
    /// using configured settings for
    /// </summary>
    public class BeanEventAdapter : BeanEventTypeFactory
    {
        private readonly IDictionary<Type, BeanEventType> _typesPerObject;
        private readonly ILockable _typesPerObjectLock;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventTypeIdGenerator _eventTypeIdGenerator;

        private readonly IDictionary<String, ConfigurationEventTypeLegacy> _typeToLegacyConfigs;
        private PropertyResolutionStyle _defaultPropertyResolutionStyle;
        private AccessorStyleEnum _defaultAccessorStyle = AccessorStyleEnum.NATIVE;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="typesPerObject">shareable collection that this adapter writes tofor caching bean types per class</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        public BeanEventAdapter(IDictionary<Type, BeanEventType> typesPerObject,
                                EventAdapterService eventAdapterService,
                                EventTypeIdGenerator eventTypeIdGenerator)
        {
            _typesPerObject = typesPerObject;
            _typesPerObjectLock = LockManager.CreateLock(GetType());
            _typeToLegacyConfigs = new Dictionary<String, ConfigurationEventTypeLegacy>();
            _defaultPropertyResolutionStyle = PropertyResolutionStyle.DEFAULT;
            _eventAdapterService = eventAdapterService;
            _eventTypeIdGenerator = eventTypeIdGenerator;
        }

        public BeanEventType[] CachedTypes
        {
            get
            {
                ICollection<BeanEventType> types = _typesPerObject.Values;
                return types.ToArray();
            }
        }

        /// <summary>Gets or sets the default accessor style. </summary>
        /// <value>style to set</value>
        public AccessorStyleEnum DefaultAccessorStyle
        {
            get { return _defaultAccessorStyle; }
            set { _defaultAccessorStyle = value; }
        }

        /// <summary>Set the additional mappings for legacy classes. </summary>
        /// <value>legacy class information</value>
        public IDictionary<string, ConfigurationEventTypeLegacy> TypeToLegacyConfigs
        {
            get { return _typeToLegacyConfigs; }
            set { _typeToLegacyConfigs.PutAll(value); }
        }

        /// <summary>Gets the default property resolution style for class properties. </summary>
        /// <value>resolution style</value>
        public PropertyResolutionStyle DefaultPropertyResolutionStyle
        {
            get { return _defaultPropertyResolutionStyle; }
            set { _defaultPropertyResolutionStyle = value; }
        }

        /// <summary>Creates a new EventType object for a object of the specified class if this is the first time the class has been seen. Else uses a cached EventType instance, i.e. client classes do not need to cache. </summary>
        /// <param name="clazz">is the class of the object.</param>
        /// <returns>EventType implementation for bean class</returns>
        public BeanEventType CreateBeanTypeDefaultName(Type clazz)
        {
            return CreateBeanType(clazz.Name, clazz, false, false, false);
        }

        /// <summary>
        /// Creates a new EventType object for a object of the specified class if this is the first time the class has been seen. Else uses a cached EventType instance, i.e. client classes do not need to cache.
        /// </summary>
        /// <param name="name">is the name</param>
        /// <param name="clazz">is the class of the object.</param>
        /// <param name="isPreconfiguredStatic">if from static engine config</param>
        /// <param name="isPreconfigured">if configured before use</param>
        /// <param name="isConfigured">if the class is a configuration value, false if discovered</param>
        /// <returns>EventType implementation for bean class</returns>
        public BeanEventType CreateBeanType(String name, Type clazz, bool isPreconfiguredStatic, bool isPreconfigured, bool isConfigured)
        {
            if (clazz == null)
            {
                throw new ArgumentException("Null value passed as class");
            }

            BeanEventType eventType;

            // not created yet, thread-safe create
            using (_typesPerObjectLock.Acquire())
            {
                eventType = _typesPerObject.Get(clazz);
                if (eventType != null)
                {
                    _eventTypeIdGenerator.AssignedType(name, eventType);
                    return eventType;
                }

                // Check if we have a legacy type definition for this class
                ConfigurationEventTypeLegacy legacyDef = _typeToLegacyConfigs.Get(clazz.FullName);
                if ((legacyDef == null) && (_defaultAccessorStyle != AccessorStyleEnum.NATIVE))
                {
                    legacyDef = new ConfigurationEventTypeLegacy();
                    legacyDef.AccessorStyle = _defaultAccessorStyle;
                }

                int typeId = _eventTypeIdGenerator.GetTypeId(name);
                EventTypeMetadata metadata = EventTypeMetadata.CreateBeanType(name, clazz, isPreconfiguredStatic, isPreconfigured, isConfigured, TypeClass.APPLICATION);
                eventType = new BeanEventType(metadata, typeId, clazz, _eventAdapterService, legacyDef);
                _typesPerObject.Put(clazz, eventType);
            }

            return eventType;
        }

        public ConfigurationEventTypeLegacy GetClassToLegacyConfigs(String className)
        {
            return _typeToLegacyConfigs.Get(className);
        }
    }
}