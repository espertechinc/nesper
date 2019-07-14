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
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.compile
{
    public class ContextCompileTimeResolverImpl : ContextCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ICollection<string> moduleUses;
        private readonly ContextCompileTimeRegistry locals;
        private readonly PathRegistry<string, ContextMetaData> path;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public ContextCompileTimeResolverImpl(
            string moduleName,
            ICollection<string> moduleUses,
            ContextCompileTimeRegistry locals,
            PathRegistry<string, ContextMetaData> path,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.locals = locals;
            this.path = path;
            this.moduleDependencies = moduleDependencies;
        }

        public ContextMetaData GetContextInfo(string contextName)
        {
            // try self-originated protected types first
            ContextMetaData localContext = locals.Contexts.Get(contextName);
            if (localContext != null) {
                return localContext;
            }

            try {
                Pair<ContextMetaData, string> pair = path.GetAnyModuleExpectSingle(contextName, moduleUses);
                if (pair != null) {
                    if (!NameAccessModifierExtensions.Visible(pair.First.ContextVisibility, pair.First.ContextModuleName, moduleName)) {
                        return null;
                    }

                    moduleDependencies.AddPathContext(contextName, pair.Second);
                    return pair.First;
                }
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.CONTEXT, contextName, e);
            }

            return null;
        }
    }
} // end of namespace