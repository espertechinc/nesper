///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.container;

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
        }

        public CompilerAbstractionCompilationContext(
            ModuleCompileTimeServices services,
            IList<EPCompiled> path) : this(services, null, path)
        {
        }

        public IContainer Container => Services.Container;

        public TypeResolver ParentTypeResolver => Services.ParentTypeResolver;

        public bool IsLogging => Services.Configuration.Compiler.Logging.IsEnableCode;

        public ModuleCompileTimeServices Services { get; }

        public Consumer<IArtifact> CompileResultConsumer { get; }

        public string GeneratedCodeNamespace => Services.Namespace;

        public IList<EPCompiled> Path { get; }
    }
} // end of namespace