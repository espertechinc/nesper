///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    public class VariableCompileTimeResolverImpl : VariableCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ICollection<string> moduleUses;
        private readonly VariableRepositoryPreconfigured publicVariables;
        private readonly VariableCompileTimeRegistry compileTimeRegistry;
        private readonly PathRegistry<string, VariableMetaData> pathVariables;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public VariableCompileTimeResolverImpl(
            string moduleName,
            ICollection<string> moduleUses,
            VariableRepositoryPreconfigured publicVariables,
            VariableCompileTimeRegistry compileTimeRegistry,
            PathRegistry<string, VariableMetaData> pathVariables,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.publicVariables = publicVariables;
            this.compileTimeRegistry = compileTimeRegistry;
            this.pathVariables = pathVariables;
            this.moduleDependencies = moduleDependencies;
        }

        public VariableMetaData Resolve(string variableName)
        {
            VariableMetaData local = compileTimeRegistry.GetVariable(variableName);
            VariableMetaData path = ResolvePath(variableName);
            VariableMetaData preconfigured = ResolvePreconfigured(variableName);

            return CompileTimeResolverUtil.ValidateAmbiguous(
                local,
                path,
                preconfigured,
                PathRegistryObjectType.VARIABLE,
                variableName);
        }

        private VariableMetaData ResolvePreconfigured(string variableName)
        {
            VariableMetaData metadata = publicVariables.GetMetadata(variableName);
            if (metadata == null) {
                return null;
            }

            moduleDependencies.AddPublicVariable(variableName);
            return metadata;
        }

        private VariableMetaData ResolvePath(string variableName)
        {
            try {
                Pair<VariableMetaData, string> pair = pathVariables.GetAnyModuleExpectSingle(variableName, moduleUses);
                if (pair == null) {
                    return null;
                }

                if (!NameAccessModifierExtensions.Visible(
                    pair.First.VariableVisibility,
                    pair.First.VariableModuleName,
                    moduleName)) {
                    return null;
                }

                moduleDependencies.AddPathVariable(variableName, pair.Second);
                return pair.First;
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.VARIABLE, variableName, e);
            }
        }
    }
} // end of namespace