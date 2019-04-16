///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryVariantStreamUtil
    {
        public static void BuildVariantStreams(
            EventTypeRepositoryImpl repo,
            IDictionary<string, ConfigurationCommonVariantStream> variantStreams,
            EventTypeFactory eventTypeFactory)
        {
            foreach (var entry in variantStreams) {
                AddVariantStream(entry.Key, entry.Value, repo, eventTypeFactory);
            }
        }

        /// <summary>
        ///     Validate the variant stream definition.
        /// </summary>
        /// <param name="variantStreamname">the stream name</param>
        /// <param name="variantStreamConfig">the configuration information</param>
        /// <param name="repo">the event types</param>
        /// <returns>specification for variant streams</returns>
        private static VariantSpec ValidateVariantStream(
            string variantStreamname,
            ConfigurationCommonVariantStream variantStreamConfig,
            EventTypeRepositoryImpl repo)
        {
            if (variantStreamConfig.TypeVariance == TypeVariance.PREDEFINED) {
                if (variantStreamConfig.VariantTypeNames.IsEmpty()) {
                    throw new ConfigurationException(
                        "Invalid variant stream configuration, no event type name has been added and default type variance requires at least one type, for name '" +
                        variantStreamname + "'");
                }
            }

            ISet<EventType> types = new LinkedHashSet<EventType>();
            foreach (var typeName in variantStreamConfig.VariantTypeNames) {
                var type = repo.GetTypeByName(typeName);
                if (type == null) {
                    throw new ConfigurationException(
                        "Event type by name '" + typeName + "' could not be found for use in variant stream configuration by name '" +
                        variantStreamname + "'");
                }

                types.Add(type);
            }

            var eventTypes = types.ToArray();
            return new VariantSpec(eventTypes, variantStreamConfig.TypeVariance);
        }

        private static void AddVariantStream(
            string name,
            ConfigurationCommonVariantStream config,
            EventTypeRepositoryImpl repo,
            EventTypeFactory eventTypeFactory)
        {
            var variantSpec = ValidateVariantStream(name, config, repo);
            var metadata = new EventTypeMetadata(
                name, null, EventTypeTypeClass.VARIANT, EventTypeApplicationType.VARIANT, NameAccessModifier.PRECONFIGURED, EventTypeBusModifier.BUS,
                false, new EventTypeIdPair(CRC32Util.ComputeCRC32(name), -1));
            var variantEventType = eventTypeFactory.CreateVariant(metadata, variantSpec);
            repo.AddType(variantEventType);
        }
    }
} // end of namespace