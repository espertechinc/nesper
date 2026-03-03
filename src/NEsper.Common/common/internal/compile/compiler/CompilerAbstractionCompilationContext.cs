///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Loader;
#endif

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public class CompilerAbstractionCompilationContext
    {
        public CompilerAbstractionCompilationContext(
            ModuleCompileTimeServices services,
            Consumer<IArtifact> compileResultConsumer,
            IList<EPCompiled> path)
        {
            Services = services;
            CompileResultConsumer = compileResultConsumer;
            Path = path;

            ArtifactRepository = services.ArtifactRepository;
            MetadataReferenceResolver = services.MetadataReferenceResolver;
            MetadataReferenceProvider = services.MetadataReferenceProvider;
            CoreAssemblies = GetCoreAssemblies();
        }

        public CompilerAbstractionCompilationContext(
            ModuleCompileTimeServices services,
            IList<EPCompiled> path) : this(services, null, path)
        {
        }

        public TypeResolver ParentTypeResolver => Services.ParentTypeResolver;

        public bool IsLogging => Services.Configuration.Compiler.Logging.IsEnableCode;

        public IArtifactRepository ArtifactRepository { get; }

        public MetadataReferenceResolver MetadataReferenceResolver { get; }

        public MetadataReferenceProvider MetadataReferenceProvider { get; }

        public IEnumerable<Assembly> CoreAssemblies { get; }

        public ModuleCompileTimeServices Services { get; }

        public Consumer<IArtifact> CompileResultConsumer { get; }

        public string GeneratedCodeNamespace => Services.Namespace;

        public IList<EPCompiled> Path { get; }

        private static IEnumerable<Assembly> GetCoreAssemblies()
        {
#if NETCOREAPP3_0_OR_GREATER
            return AssemblyLoadContext.Default.Assemblies;
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }
    }
} // end of namespace