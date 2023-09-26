///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperFAFQuery
    {
        public static string CompileQuery(
            FAFQueryMethodForge query,
            string classPostfix,
            ModuleCompileTimeServices compileTimeServices,
            out ICompileArtifact artifact)
        {
            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields),
                classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                compileTimeServices.Namespace,
                statementFieldsClassName,
                compileTimeServices.IsInstrumented,
                TODO);

            var queryMethodProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(FAFQueryMethodProvider),
                classPostfix);
            var forgeablesQueryMethod = query.MakeForgeables(queryMethodProviderClassName, classPostfix, namespaceScope);

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>(forgeablesQueryMethod);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope, 0));

            // forge with statement-fields last
            var classes = new List<CodegenClass>(forgeables.Count);
            foreach (var forgeable in forgeables) {
                var clazz = forgeable.Forge(true, true);
                classes.Add(clazz);
            }

            // assign the assembly (required for completeness)
            artifact = null;

            // compile with statement-field first
            classes = classes
                .OrderBy(c => c.ClassType.GetSortCode())
                .ToList();

            var container = compileTimeServices.Container;
            var repository = container.ArtifactRepositoryManager().DefaultRepository;
            var compiler = container
                .RoslynCompiler()
                .WithMetaDataReferences(repository.AllMetadataReferences)
                .WithMetaDataReferences(container.MetadataReferenceProvider()?.Invoke())
                .WithDebugOptimization(compileTimeServices.Configuration.Compiler.IsDebugOptimization)
                .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
                .WithCodegenClasses(classes);

            artifact = repository.Register(compiler.Compile());

            return queryMethodProviderClassName;
        }
    }
} // end of namespace