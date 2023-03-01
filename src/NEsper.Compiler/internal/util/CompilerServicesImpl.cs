///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using XLR8.CGLib;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerServicesImpl : CompilerServices
    {
        public StatementSpecRaw ParseWalk(
            string epl,
            StatementSpecMapEnv mapEnv)
        {
            return CompilerHelperSingleEPL.ParseWalk(epl, mapEnv);
        }

        public string LexSampleSQL(string querySQL)
        {
            return SQLLexer.LexSampleSQL(querySQL);
        }

        public ExprNode CompileExpression(
            string expression,
            StatementCompileTimeServices services)
        {
            var toCompile = "select * from System.Object#time(" + expression + ")";

            StatementSpecRaw raw;
            try {
                raw = services.CompilerServices.ParseWalk(toCompile, services.StatementSpecMapEnv);
            }
            catch (StatementSpecCompileException e) {
                throw new ExprValidationException(
                    "Failed to compile expression '" + expression + "': " + e.Expression,
                    e);
            }

            return raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
        }

        public Type CompileStandInClass(
            CodegenClassType classType,
            string classNameSimple,
            ModuleCompileTimeServices services)
        {
            var namespaceScope = new CodegenNamespaceScope(services.Namespace, null, false);
            var classScope = new CodegenClassScope(true, namespaceScope, null);
            var clazz = new CodegenClass(
                classType,
                null,
                classNameSimple,
                classScope,
                EmptyList<CodegenTypedParam>.Instance,
                null,
                new CodegenClassMethods(),
                new CodegenClassProperties(),
                EmptyList<CodegenInnerClass>.Instance);
            
            // This is a bit hacky... basically, Esper has to generate a "Type" that can be returned and
            // included as the "Underlying" type for the JsonEventType.  This method is called during the
            // portion of the sequence where we are attempting to build the forgeables, so the real type
            // doesnt exist yet.  Esper builds the stand-in but expects that the real type will be used
            // at runtime.  In Java, type erasure allows this to happen because there is no real type in
            // backing arrays and collections.  In .NET we need the types to match.
            //
            // We are creating a "capsule" class which will act as a placeholder.  When we detect that
            // the type is a capsule type in the JsonEventType, we will attempt to "resolve" and replace
            // it.
            
            var classNameFull = namespaceScope.Namespace + '.' + classNameSimple;
            var capsuleClass = CapsuleEmitter.CreateCapsule(classNameFull);

            return capsuleClass.TargetType;
        }

        public Artifact Compile(CompileRequest request)
        {
            var configuration = request.ModuleCompileTimeServices.Configuration;
            var container = request.ModuleCompileTimeServices.Container;
            var repository = container.ArtifactRepositoryManager().DefaultRepository;
            var compiler = container
                .RoslynCompiler()
                .WithMetaDataReferences(repository.AllMetadataReferences)
                .WithCodeLogging(configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(configuration.Compiler.Logging.AuditDirectory)
                .WithSources(
                    request.Classes
                        .Select(_ => new RoslynCompiler.SourceBasic(_.ClassName, _.Code))
                        .ToList<RoslynCompiler.Source>());

            return repository.Register(compiler.Compile());
        }
    }
} // end of namespace