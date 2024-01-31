///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryImpl : EventTypeRepository
    {
        private readonly bool compileTime;
        private readonly IDictionary<long, EventType> idToTypeMap = new Dictionary<long, EventType>();

        public EventTypeRepositoryImpl(bool compileTime)
        {
            this.compileTime = compileTime;
        }

        public ICollection<EventType> AllTypes => NameToTypeMap.Values;

        public IDictionary<string, EventType> NameToTypeMap { get; } = new Dictionary<string, EventType>();

        public EventType GetTypeByName(string typeName)
        {
            return NameToTypeMap.Get(typeName);
        }

        public EventType GetTypeById(long eventTypeIdPublic)
        {
            return idToTypeMap.Get(eventTypeIdPublic);
        }

        public void AddType(EventType eventType)
        {
            var name = eventType.Metadata.Name;
            if (NameToTypeMap.ContainsKey(name)) {
                throw new ArgumentException("Event type by name '" + name + "' already registered");
            }

            NameToTypeMap.Put(name, eventType);

            var publicId = eventType.Metadata.EventTypeIdPair.PublicId;
            if (compileTime) {
                publicId = CRC32Util.ComputeCRC32(name);
            }
            else {
                if (publicId == -1) {
                    throw new ArgumentException("Event type by name '" + name + "' has a public id of -1 at runtime");
                }
            }

            var sameIdType = idToTypeMap.Get(publicId);
            if (sameIdType != null) {
                throw new ArgumentException(
                    "Event type by name '" +
                    name +
                    "' has a public crc32 id overlap with event type by name '" +
                    sameIdType.Name +
                    "', please consider renaming either of these types");
            }

            idToTypeMap.Put(publicId, eventType);
        }

        public void RemoveType(EventType eventType)
        {
            NameToTypeMap.Remove(eventType.Name);
            idToTypeMap.Remove(CRC32Util.ComputeCRC32(eventType.Name));
        }

        public void MergeFrom(EventTypeRepositoryImpl other)
        {
            foreach (var entry in other.NameToTypeMap) {
                if (NameToTypeMap.ContainsKey(entry.Key)) {
                    continue;
                }

                AddType(entry.Value);
            }
        }
    }
} // end of namespace