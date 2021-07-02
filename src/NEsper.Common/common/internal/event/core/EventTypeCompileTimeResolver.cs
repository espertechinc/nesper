///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.eventtyperepo;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeCompileTimeResolver : EventTypeNameResolver
    {
        private readonly EventTypeCompileTimeRegistry _locals;
        private readonly ModuleDependenciesCompileTime _moduleDependencies;
        private readonly string _moduleName;
        private readonly ICollection<string> _moduleUses;
        private readonly EventTypeRepositoryImpl _publics;
        private readonly bool _isFireAndForget;

        public EventTypeCompileTimeResolver(
            string moduleName,
            ICollection<string> moduleUses,
            EventTypeCompileTimeRegistry locals,
            EventTypeRepositoryImpl publics,
            PathRegistry<string, EventType> path,
            ModuleDependenciesCompileTime moduleDependencies,
            bool isFireAndForget)
        {
            this._moduleName = moduleName;
            this._moduleUses = moduleUses;
            this._locals = locals;
            this._publics = publics;
            Path = path;
            this._moduleDependencies = moduleDependencies;
            this._isFireAndForget = isFireAndForget;
        }

        public PathRegistry<string, EventType> Path { get; }

        public EventType GetTypeByName(string typeName)
        {
            var local = _locals.GetModuleTypes(typeName);
            var path = ResolvePath(typeName);
            var preconfigured = ResolvePreconfigured(typeName);
            return CompileTimeResolverUtil.ValidateAmbiguous(
                local,
                path,
                preconfigured,
                PathRegistryObjectType.EVENTTYPE,
                typeName);
        }

        private EventType ResolvePreconfigured(string typeName)
        {
            var eventType = _publics.GetTypeByName(typeName);
            if (eventType == null) {
                return null;
            }

            _moduleDependencies.AddPublicEventType(typeName);
            return eventType;
        }

        private EventType ResolvePath(string typeName)
        {
            try {
                var typeAndModule = Path.GetAnyModuleExpectSingle(typeName, _moduleUses);
                if (typeAndModule == null) {
                    return null;
                }

                if (!_isFireAndForget &&
                    !NameAccessModifierExtensions.Visible(
                        typeAndModule.First.Metadata.AccessModifier,
                        typeAndModule.Second,
                        _moduleName)) {
                    return null;
                }

                _moduleDependencies.AddPathEventType(typeName, typeAndModule.Second);
                return typeAndModule.First;
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.EVENTTYPE, typeName, e);
            }
        }
    }
} // end of namespace