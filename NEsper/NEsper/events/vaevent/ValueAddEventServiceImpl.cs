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
using com.espertech.esper.compat.threading;
using com.espertech.esper.view;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Service for handling revision event types.
    /// <para/>
    /// Each named window instance gets a dedicated revision processor.
    /// </summary>
    public class ValueAddEventServiceImpl : ValueAddEventService
    {
        /// <summary>Map of revision event name and revision compiled specification. </summary>
        protected readonly IDictionary<String, RevisionSpec> SpecificationsByRevisionName;

        /// <summary>Map of named window name and processor. </summary>
        protected readonly IDictionary<String, ValueAddEventProcessor> ProcessorsByNamedWindow;

        /// <summary>Map of revision event stream and variant stream processor. </summary>
        protected readonly IDictionary<String, ValueAddEventProcessor> VariantProcessors;

        private readonly ILockManager _lockManager;

        /// <summary>Ctor. </summary>
        public ValueAddEventServiceImpl(ILockManager lockManager)
        {
            _lockManager = lockManager;
            SpecificationsByRevisionName = new Dictionary<String, RevisionSpec>().WithNullSupport();
            ProcessorsByNamedWindow = new Dictionary<String, ValueAddEventProcessor>().WithNullSupport();
            VariantProcessors = new Dictionary<String, ValueAddEventProcessor>().WithNullSupport();
        }

        public EventType[] ValueAddedTypes
        {
            get
            {
                var types = ProcessorsByNamedWindow.Select(revisionNamedWindow => revisionNamedWindow.Value.ValueAddEventType).ToList();
                types.AddRange(VariantProcessors.Select(variantProcessor => variantProcessor.Value.ValueAddEventType));

                return types.ToArray();
            }
        }

        public void Init(IDictionary<String, ConfigurationRevisionEventType> configRevision, IDictionary<String, ConfigurationVariantStream> configVariant, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
            foreach (KeyValuePair<String, ConfigurationRevisionEventType> entry in configRevision)
            {
                AddRevisionEventType(entry.Key, entry.Value, eventAdapterService);
            }
            foreach (KeyValuePair<String, ConfigurationVariantStream> entry in configVariant)
            {
                AddVariantStream(entry.Key, entry.Value, eventAdapterService, eventTypeIdGenerator);
            }
        }

        public void AddRevisionEventType(String revisioneventTypeName, ConfigurationRevisionEventType config, EventAdapterService eventAdapterService)
        {
            RevisionSpec specification = ValidateRevision(revisioneventTypeName, config, eventAdapterService);
            SpecificationsByRevisionName.Put(revisioneventTypeName, specification);
        }

        public void AddVariantStream(String variantStreamname, ConfigurationVariantStream variantStreamConfig, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
            var variantSpec = ValidateVariantStream(variantStreamname, variantStreamConfig, eventAdapterService);
            var processor = new VAEVariantProcessor(eventAdapterService, variantSpec, eventTypeIdGenerator, variantStreamConfig, _lockManager);
            eventAdapterService.AddTypeByName(variantStreamname, processor.ValueAddEventType);
            VariantProcessors.Put(variantStreamname, processor);
        }

        /// <summary>Validate the variant stream definition. </summary>
        /// <param name="variantStreamname">the stream name</param>
        /// <param name="variantStreamConfig">the configuration information</param>
        /// <param name="eventAdapterService">the event adapters</param>
        /// <returns>specification for variant streams</returns>
        public static VariantSpec ValidateVariantStream(String variantStreamname, ConfigurationVariantStream variantStreamConfig, EventAdapterService eventAdapterService)
        {
            if (variantStreamConfig.TypeVariance == TypeVarianceEnum.PREDEFINED)
            {
                if (variantStreamConfig.VariantTypeNames.IsEmpty())
                {
                    throw new ConfigurationException("Invalid variant stream configuration, no event type name has been added and default type variance requires at least one type, for name '" + variantStreamname + "'");
                }
            }

            ICollection<EventType> types = new LinkedHashSet<EventType>();
            foreach (String typeName in variantStreamConfig.VariantTypeNames)
            {
                EventType type = eventAdapterService.GetEventTypeByName(typeName);
                if (type == null)
                {
                    throw new ConfigurationException("Event type by name '" + typeName + "' could not be found for use in variant stream configuration by name '" + variantStreamname + "'");
                }
                types.Add(type);
            }

            EventType[] eventTypes = types.ToArray();
            return new VariantSpec(variantStreamname, eventTypes, variantStreamConfig.TypeVariance);
        }

        public EventType CreateRevisionType(String namedWindowName, String name, StatementStopService statementStopService, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
            RevisionSpec spec = SpecificationsByRevisionName.Get(name);
            ValueAddEventProcessor processor;
            if (spec.PropertyRevision == PropertyRevisionEnum.OVERLAY_DECLARED)
            {
                processor = new VAERevisionProcessorDeclared(name, spec, statementStopService, eventAdapterService, eventTypeIdGenerator);
            }
            else
            {
                processor = new VAERevisionProcessorMerge(name, spec, statementStopService, eventAdapterService, eventTypeIdGenerator);
            }

            ProcessorsByNamedWindow.Put(namedWindowName, processor);
            return processor.ValueAddEventType;
        }

        public ValueAddEventProcessor GetValueAddProcessor(String name)
        {
            ValueAddEventProcessor proc = ProcessorsByNamedWindow.Get(name);
            if (proc != null)
            {
                return proc;
            }
            return VariantProcessors.Get(name);
        }

        public EventType GetValueAddUnderlyingType(String name)
        {
            RevisionSpec spec = SpecificationsByRevisionName.Get(name);
            if (spec == null)
            {
                return null;
            }
            return spec.BaseEventType;
        }

        public bool IsRevisionTypeName(String revisionTypeName)
        {
            return SpecificationsByRevisionName.ContainsKey(revisionTypeName);
        }

        /// <summary>Valiate the revision configuration. </summary>
        /// <param name="revisioneventTypeName">name of revision types</param>
        /// <param name="config">configures revision type</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <returns>revision specification</returns>
        /// <throws>ConfigurationException if the configs are invalid</throws>
        internal static RevisionSpec ValidateRevision(String revisioneventTypeName, ConfigurationRevisionEventType config, EventAdapterService eventAdapterService)
        {
            if ((config.NameBaseEventTypes == null) || (config.NameBaseEventTypes.Count == 0))
            {
                throw new ConfigurationException("Required base event type name is not set in the configuration for revision event type '" + revisioneventTypeName + "'");
            }

            if (config.NameBaseEventTypes.Count > 1)
            {
                throw new ConfigurationException("Only one base event type name may be added to revision event type '" + revisioneventTypeName + "', multiple base types are not yet supported");
            }

            // get base types
            String baseeventTypeName = config.NameBaseEventTypes.FirstOrDefault();
            EventType baseEventType = eventAdapterService.GetEventTypeByName(baseeventTypeName);
            if (baseEventType == null)
            {
                throw new ConfigurationException("Could not locate event type for name '" + baseeventTypeName + "' in the configuration for revision event type '" + revisioneventTypeName + "'");
            }

            // get name types
            var deltaTypes = new EventType[config.NameDeltaEventTypes.Count];
            var deltaNames = new String[config.NameDeltaEventTypes.Count];
            int count = 0;
            foreach (String deltaName in config.NameDeltaEventTypes)
            {
                EventType deltaEventType = eventAdapterService.GetEventTypeByName(deltaName);
                if (deltaEventType == null)
                {
                    throw new ConfigurationException("Could not locate event type for name '" + deltaName + "' in the configuration for revision event type '" + revisioneventTypeName + "'");
                }
                deltaTypes[count] = deltaEventType;
                deltaNames[count] = deltaName;
                count++;
            }

            // the key properties must be set
            if ((config.KeyPropertyNames == null) || (config.KeyPropertyNames.Length == 0))
            {
                throw new ConfigurationException("Required key properties are not set in the configuration for revision event type '" + revisioneventTypeName + "'");
            }

            // make sure the key properties exist the base type and all delta types
            CheckKeysExist(baseEventType, baseeventTypeName, config.KeyPropertyNames, revisioneventTypeName);
            for (int i = 0; i < deltaTypes.Length; i++)
            {
                CheckKeysExist(deltaTypes[i], deltaNames[i], config.KeyPropertyNames, revisioneventTypeName);
            }

            // key property names shared between base and delta must have the same type
            String[] keyPropertyNames = PropertyUtility.CopyAndSort(config.KeyPropertyNames);
            foreach (String key in keyPropertyNames)
            {
                var typeProperty = baseEventType.GetPropertyType(key);
                foreach (EventType dtype in deltaTypes)
                {
                    var dtypeProperty = dtype.GetPropertyType(key);
                    if ((dtypeProperty != null) && (typeProperty != dtypeProperty))
                    {
                        throw new ConfigurationException("Key property named '" + key + "' does not have the same type for base and delta types of revision event type '" + revisioneventTypeName + "'");
                    }
                }
            }

            // In the "declared" type the change set properties consist of only :
            //   (base event type properties) minus (key properties) minus (properties only on base event type)
            if (config.PropertyRevision == PropertyRevisionEnum.OVERLAY_DECLARED)
            {
                // determine non-key properties: those overridden by any delta, and those simply only present on the base event type
                String[] nonkeyPropertyNames = PropertyUtility.UniqueExclusiveSort(baseEventType.PropertyNames, keyPropertyNames);
                ICollection<String> baseEventOnlyProperties = new HashSet<String>();
                ICollection<String> changesetPropertyNames = new HashSet<String>();
                foreach (String nonKey in nonkeyPropertyNames)
                {
                    var overriddenProperty = false;
                    foreach (EventType type in deltaTypes)
                    {
                        if (type.IsProperty(nonKey))
                        {
                            changesetPropertyNames.Add(nonKey);
                            overriddenProperty = true;
                            break;
                        }
                    }
                    if (!overriddenProperty)
                    {
                        baseEventOnlyProperties.Add(nonKey);
                    }
                }

                String[] changesetProperties = changesetPropertyNames.ToArray();
                String[] baseEventOnlyPropertyNames = baseEventOnlyProperties.ToArray();

                // verify that all changeset properties match event type
                foreach (String changesetProperty in changesetProperties)
                {
                    var typeProperty = baseEventType.GetPropertyType(changesetProperty);
                    foreach (EventType dtype in deltaTypes)
                    {
                        var dtypeProperty = dtype.GetPropertyType(changesetProperty);
                        if ((dtypeProperty != null) && (typeProperty != dtypeProperty))
                        {
                            throw new ConfigurationException("Property named '" + changesetProperty + "' does not have the same type for base and delta types of revision event type '" + revisioneventTypeName + "'");
                        }
                    }
                }

                return new RevisionSpec(config.PropertyRevision, baseEventType, deltaTypes, deltaNames, keyPropertyNames, changesetProperties, baseEventOnlyPropertyNames, false, null);
            }
            else
            {
                // In the "exists" type the change set properties consist of all properties: base event properties plus delta types properties
                ICollection<String> allProperties = new HashSet<String>();
                allProperties.AddAll(baseEventType.PropertyNames);
                foreach (EventType deltaType in deltaTypes)
                {
                    allProperties.AddAll(deltaType.PropertyNames);
                }

                String[] allPropertiesArr = allProperties.ToArray();
                String[] changesetProperties = PropertyUtility.UniqueExclusiveSort(allPropertiesArr, keyPropertyNames);

                // All properties must have the same type, if a property exists for any given type
                bool hasContributedByDelta = false;
                bool[] contributedByDelta = new bool[changesetProperties.Length];
                count = 0;
                foreach (String property in changesetProperties)
                {
                    Type basePropertyType = baseEventType.GetPropertyType(property);
                    Type typeTemp = null;
                    if (basePropertyType != null)
                    {
                        typeTemp = basePropertyType;
                    }
                    else
                    {
                        hasContributedByDelta = true;
                        contributedByDelta[count] = true;
                    }
                    foreach (EventType dtype in deltaTypes)
                    {
                        Type dtypeProperty = dtype.GetPropertyType(property);
                        if (dtypeProperty != null)
                        {
                            if ((typeTemp != null) && (dtypeProperty != typeTemp))
                            {
                                throw new ConfigurationException("Property named '" + property + "' does not have the same type for base and delta types of revision event type '" + revisioneventTypeName + "'");
                            }

                        }
                        typeTemp = dtypeProperty;
                    }
                    count++;
                }

                // Compile changeset
                return new RevisionSpec(config.PropertyRevision, baseEventType, deltaTypes, deltaNames, keyPropertyNames, changesetProperties, new String[0], hasContributedByDelta, contributedByDelta);
            }
        }

        private static void CheckKeysExist(EventType baseEventType, String name, IEnumerable<string> keyProperties, String revisioneventTypeName)
        {
            IList<string> propertyNames = baseEventType.PropertyNames;
            foreach (var keyProperty in keyProperties)
            {
                var property = keyProperty;
                var exists = propertyNames.Any(propertyName => propertyName == property);
                if (!exists)
                {
                    throw new ConfigurationException("Key property '" + keyProperty + "' as defined in the configuration for revision event type '" + revisioneventTypeName + "' does not exists in event type '" + name + "'");
                }
            }
        }
    }
}
