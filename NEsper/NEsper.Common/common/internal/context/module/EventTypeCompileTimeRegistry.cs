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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.module
{
    public class EventTypeCompileTimeRegistry
    {
        private readonly IDictionary<string, EventType> _moduleTypesAdded = new LinkedHashMap<string, EventType>();
        private readonly IDictionary<string, EventType> _newTypesAdded = new LinkedHashMap<string, EventType>();
        private readonly EventTypeRepository _eventTypeRepositoryPreconfigured;

        public EventTypeCompileTimeRegistry(EventTypeRepository eventTypeRepositoryPreconfigured)
        {
            this._eventTypeRepositoryPreconfigured = eventTypeRepositoryPreconfigured;
        }

        public void NewType(EventType type)
        {
            if (type.Metadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
                if (type.Metadata.ApplicationType != EventTypeApplicationType.XML) {
                    throw new ArgumentException("Preconfigured-visibility is not allowed here");
                }

                _eventTypeRepositoryPreconfigured.AddType(type);
            }

            if (_moduleTypesAdded.ContainsKey(type.Name)) {
                throw new ArgumentException("Event type '" + type.Name + "' has already been added by the module");
            }

            if (type.Metadata.AccessModifier == NameAccessModifier.PRIVATE ||
                type.Metadata.AccessModifier == NameAccessModifier.PUBLIC) {
                _moduleTypesAdded.Put(type.Name, type);
            }

            // We allow private types to register multiple times, the first one counts (i.e. rollup with multiple select-clauses active)
            if (!_newTypesAdded.ContainsKey(type.Name)) {
                _newTypesAdded.Put(type.Name, type);
            }
            else {
                throw new ArgumentException("Event type '" + type.Name + "' has already been added by the module");
            }
        }

        public EventType GetModuleTypes(string typeName)
        {
            return _moduleTypesAdded.Get(typeName);
        }

        public ICollection<EventType> NewTypesAdded => _newTypesAdded.Values;
    }
} // end of namespace