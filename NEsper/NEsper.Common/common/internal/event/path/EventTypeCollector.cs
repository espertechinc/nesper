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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.path
{
    public interface EventTypeCollector
    {
        void RegisterMap(
            EventTypeMetadata metadata,
            LinkedHashMap<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName);

        void RegisterObjectArray(
            EventTypeMetadata metadata,
            LinkedHashMap<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName);

        void RegisterWrapper(
            EventTypeMetadata metadata,
            EventType underlying,
            LinkedHashMap<string, object> properties);

        void RegisterBean(
            EventTypeMetadata metadata,
            Type clazz,
            string startTimestampName,
            string endTimestampName,
            EventType[] superTypes,
            ISet<EventType> deepSupertypes);

        void RegisterXML(
            EventTypeMetadata metadata,
            string representsFragmentOfProperty,
            string representsOriginalTypeName);

        void RegisterAvro(
            EventTypeMetadata metadata,
            string schemaJson);

        void RegisterVariant(
            EventTypeMetadata metadata,
            EventType[] variants,
            bool any);
    }
} // end of namespace