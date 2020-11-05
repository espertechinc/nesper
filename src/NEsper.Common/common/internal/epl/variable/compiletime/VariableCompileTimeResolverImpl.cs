///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    public class VariableCompileTimeResolverImpl : VariableCompileTimeResolver
    {
        private readonly string _moduleName;
        private readonly ICollection<string> _moduleUses;
        private readonly VariableRepositoryPreconfigured _publicVariables;
        private readonly VariableCompileTimeRegistry _compileTimeRegistry;
        private readonly PathRegistry<string, VariableMetaData> _pathVariables;
        private readonly ModuleDependenciesCompileTime _moduleDependencies;
        private readonly bool _isFireAndForget;

        public VariableCompileTimeResolverImpl(
            string moduleName,
            ICollection<string> moduleUses,
            VariableRepositoryPreconfigured publicVariables,
            VariableCompileTimeRegistry compileTimeRegistry,
            PathRegistry<string, VariableMetaData> pathVariables,
            ModuleDependenciesCompileTime moduleDependencies,
            bool isFireAndForget)
        {
            _moduleName = moduleName;
            _moduleUses = moduleUses;
            _publicVariables = publicVariables;
            _compileTimeRegistry = compileTimeRegistry;
            _pathVariables = pathVariables;
            _moduleDependencies = moduleDependencies;
            _isFireAndForget = isFireAndForget;
        }

        public VariableMetaData Resolve(string variableName)
        {
            var local = _compileTimeRegistry.GetVariable(variableName);
            var path = ResolvePath(variableName);
            var preconfigured = ResolvePreconfigured(variableName);

            return CompileTimeResolverUtil.ValidateAmbiguous(
                local,
                path,
                preconfigured,
                PathRegistryObjectType.VARIABLE,
                variableName);
        }

        private VariableMetaData ResolvePreconfigured(string variableName)
        {
            var metadata = _publicVariables.GetMetadata(variableName);
            if (metadata == null) {
                return null;
            }

            _moduleDependencies.AddPublicVariable(variableName);
            return metadata;
        }

        private VariableMetaData ResolvePath(string variableName)
        {
            try {
                var pair = _pathVariables.GetAnyModuleExpectSingle(variableName, _moduleUses);
                if (pair == null) {
                    return null;
                }

                if (!_isFireAndForget &&
                    !NameAccessModifierExtensions.Visible(
                        pair.First.VariableVisibility,
                        pair.First.VariableModuleName,
                        _moduleName)) {
                    return null;
                }

                _moduleDependencies.AddPathVariable(variableName, pair.Second);
                return pair.First;
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.VARIABLE, variableName, e);
            }
        }
    }
} // end of namespace