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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.compile;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.path
{
    public class NamedWindowCompileTimeResolverImpl : NamedWindowCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ISet<string> moduleUses;
        private readonly NamedWindowCompileTimeRegistry locals;
        private readonly PathRegistry<string, NamedWindowMetaData> path;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public NamedWindowCompileTimeResolverImpl(
            string moduleName,
            ISet<string> moduleUses,
            NamedWindowCompileTimeRegistry locals,
            PathRegistry<string, NamedWindowMetaData> path,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.locals = locals;
            this.path = path;
            this.moduleDependencies = moduleDependencies;
        }

        public NamedWindowMetaData Resolve(string namedWindowName)
        {
            // try self-originated protected types first
            var localNamedWindow = locals.NamedWindows.Get(namedWindowName);
            if (localNamedWindow != null) {
                return localNamedWindow;
            }

            try {
                var pair = path.GetAnyModuleExpectSingle(namedWindowName, moduleUses);
                if (pair != null) {
                    if (!NameAccessModifier.Visible(pair.First.EventType.Metadata.AccessModifier, pair.First.NamedWindowModuleName, moduleName)) {
                        return null;
                    }

                    moduleDependencies.AddPathNamedWindow(namedWindowName, pair.Second);
                    return pair.First;
                }
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.NAMEDWINDOW, namedWindowName, e);
            }

            return null;
        }
    }
} // end of namespace