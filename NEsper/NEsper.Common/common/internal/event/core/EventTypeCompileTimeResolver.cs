///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.eventtyperepo;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeCompileTimeResolver : EventTypeNameResolver {
        private readonly string moduleName;
        private readonly ISet<string> moduleUses;
        private readonly EventTypeCompileTimeRegistry locals;
        private readonly EventTypeRepositoryImpl publics;
        private readonly PathRegistry<string, EventType> path;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public EventTypeCompileTimeResolver(
            string moduleName, 
            ISet<string> moduleUses, 
            EventTypeCompileTimeRegistry locals,
            EventTypeRepositoryImpl publics, 
            PathRegistry<string, EventType> path,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.locals = locals;
            this.publics = publics;
            this.path = path;
            this.moduleDependencies = moduleDependencies;
        }

        public PathRegistry<string, EventType> Path {
            get { return path; }
        }

        public EventType GetTypeByName(string typeName)
        {
            EventType local = locals.GetModuleTypes(typeName);
            EventType path = ResolvePath(typeName);
            EventType preconfigured = ResolvePreconfigured(typeName);
            return CompileTimeResolver.ValidateAmbiguous(
                local, path, preconfigured, PathRegistryObjectType.EVENTTYPE, typeName);
        }

        private EventType ResolvePreconfigured(string typeName)
        {
            EventType eventType = publics.GetTypeByName(typeName);
            if (eventType == null) {
                return null;
            }

            moduleDependencies.AddPublicEventType(typeName);
            return eventType;
        }

        private EventType ResolvePath(string typeName)
        {
            try {
                Pair<EventType, string> typeAndModule = path.GetAnyModuleExpectSingle(typeName, moduleUses);
                if (typeAndModule == null) {
                    return null;
                }

                if (!NameAccessModifier.Visible(
                    typeAndModule.First.Metadata.AccessModifier, typeAndModule.Second, moduleName)) {
                    return null;
                }

                moduleDependencies.AddPathEventType(typeName, typeAndModule.Second);
                return typeAndModule.First;
            }
            catch (PathException e) {
                throw CompileTimeResolver.MakePathAmbiguous(PathRegistryObjectType.EVENTTYPE, typeName, e);
            }
        }
    }
} // end of namespace