///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.query;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperModuleProvider;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperStatementProvider;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperValidator;
using static com.espertech.esper.compiler.@internal.util.Version;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperFAFProvider
    {
        private const string MEMBERNAME_QUERY_METHOD_PROVIDER = "provider";

        private static int defaultStatementNameIndex = 0;

        public static string GetDefaultStatementName()
        {
            return "q" + Interlocked.Increment(ref defaultStatementNameIndex);
        }

        public static EPCompiled Compile(
            Compilable compilable,
            ModuleCompileTimeServices services,
            CompilerArguments args)
        {
            var compileTimeServices = new StatementCompileTimeServices(0, services);
            var walkResult = CompilerHelperSingleEPL.ParseCompileInlinedClassesWalk(compilable, compileTimeServices);
            var raw = walkResult.StatementSpecRaw;

            var statementType = StatementTypeUtil.GetStatementType(raw);
            if (statementType != StatementType.SELECT) {
                // the fire-and-forget spec is null for "select" and populated for I/U/D
                throw new StatementSpecCompileException(
                    "Provided EPL expression is a continuous query expression (not an on-demand query)",
                    compilable.ToEPL());
            }

            var annotations = AnnotationUtil.CompileAnnotations(
                raw.Annotations,
                services.ImportServiceCompileTime,
                compilable);

            // walk subselects, alias expressions, declared expressions, dot-expressions
            var visitor = StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(raw);

            // compile context descriptor
            ContextCompileTimeDescriptor contextDescriptor = null;
            var optionalContextName = raw.OptionalContextName;
            if (optionalContextName != null) {
                var detail = services.ContextCompileTimeResolver.GetContextInfo(optionalContextName);
                if (detail == null) {
                    throw new StatementSpecCompileException(
                        "Context by name '" + optionalContextName + "' could not be found",
                        compilable.ToEPL());
                }

                contextDescriptor = new ContextCompileTimeDescriptor(
                    optionalContextName,
                    detail.ContextModuleName,
                    detail.ContextVisibility,
                    new ContextPropertyRegistry(detail),
                    detail.ValidationInfos);
            }

            var statementNameFromAnnotation = GetNameFromAnnotation(annotations);
            var statementName = statementNameFromAnnotation == null ? GetDefaultStatementName() : statementNameFromAnnotation.Trim();
            var statementRawInfo = new StatementRawInfo(
                0,
                statementName,
                annotations,
                statementType.Value,
                contextDescriptor,
                null,
                compilable,
                null);
            StatementSpecCompiledDesc compiledDesc = StatementRawCompiler.Compile(
                raw,
                compilable,
                false,
                true,
                annotations,
                visitor.Subselects,
                new List<ExprTableAccessNode>(raw.TableExpressions),
                statementRawInfo,
                compileTimeServices);
            StatementSpecCompiled specCompiled = compiledDesc.Compiled;

            var fafSpec = specCompiled.Raw.FireAndForgetSpec;

            //var @namespace = "generated";
            var classPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(statementName);

            EPCompiledManifest manifest;
            ICompileArtifact artifact;

            FAFQueryMethodForge query;
            if (specCompiled.Raw.InsertIntoDesc != null) {
                query = new FAFQueryMethodIUDInsertIntoForge(
                    specCompiled,
                    compilable,
                    statementRawInfo,
                    compileTimeServices);
            }
            else if (fafSpec == null) { // null indicates a select-statement, same as continuous query
                var desc = new FAFQueryMethodSelectDesc(
                    specCompiled,
                    compilable,
                    statementRawInfo,
                    compileTimeServices);
                var classNameResultSetProcessor =
                    CodeGenerationIDGenerator.GenerateClassNameSimple(
                        typeof(ResultSetProcessorFactoryProvider),
                        classPostfix);
                query = new FAFQueryMethodSelectForge(desc, classNameResultSetProcessor, statementRawInfo);
            }
            else if (fafSpec is FireAndForgetSpecDelete) {
                query = new FAFQueryMethodIUDDeleteForge(
                    specCompiled,
                    compilable,
                    statementRawInfo,
                    compileTimeServices);
            }
            else if (fafSpec is FireAndForgetSpecUpdate) {
                query = new FAFQueryMethodIUDUpdateForge(
                    specCompiled,
                    compilable,
                    statementRawInfo,
                    compileTimeServices);
            }
            else {
                throw new IllegalStateException("Unrecognized FAF code " + fafSpec);
            }

            // verify substitution parameters
            VerifySubstitutionParams(raw.SubstitutionParameters);

            var repository = compileTimeServices
                .Container
                .ArtifactRepositoryManager()
                .DefaultRepository;

            try {
                manifest = CompileToAssembly(query, classPostfix, args.Options, services, repository, out artifact);
            }
            catch (EPCompileException) {
                throw;
            }
            catch (Exception ex) {
                throw new EPCompileException(
                    "Unexpected exception compiling module: " + ex.Message,
                    ex,
                    new EmptyList<EPCompileExceptionItem>());
            }

            return new EPCompiled(repository, new [] { artifact }, manifest);
        }

        private static EPCompiledManifest CompileToAssembly(
            FAFQueryMethodForge query,
            string classPostfix,
            CompilerOptions compilerOptions,
            ModuleCompileTimeServices compileTimeServices,
            IArtifactRepository artifactRepository,
            out ICompileArtifact artifact)
        {
            string queryMethodProviderClassName;
            try {
                queryMethodProviderClassName = CompilerHelperFAFQuery.CompileQuery(query, classPostfix,compileTimeServices, out artifact);
            }
            catch (StatementSpecCompileException ex) {
                EPCompileExceptionItem first;
                if (ex is StatementSpecCompileSyntaxException) {
                    first = new EPCompileExceptionSyntaxItem(ex.Message, ex.Expression, -1);
                }
                else {
                    first = new EPCompileExceptionItem(ex.Message, ex.Expression, -1);
                }

                var items = Collections.SingletonList(first);
                throw new EPCompileException(ex.Message + " [" + ex.Expression + "]", ex, items);
            }

            // compile query provider
            var fafProviderClassName = MakeFAFProvider(
                queryMethodProviderClassName,
                classPostfix,
                compileTimeServices,
                artifactRepository,
                out artifact);

            // create manifest
            return new EPCompiledManifest(COMPILER_VERSION, null, fafProviderClassName, false);
        }

        private static string MakeFAFProvider(
            string queryMethodProviderClassName,
            string classPostfix,
            ModuleCompileTimeServices compileTimeServices,
            IArtifactRepository artifactRepository,
            out ICompileArtifact artifact)
        {
            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                compileTimeServices.Namespace, statementFieldsClassName, compileTimeServices.IsInstrumented());
            var fafProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(FAFProvider), classPostfix);
            var classScope = new CodegenClassScope(true, namespaceScope, fafProviderClassName);
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            var ctor = new CodegenCtor(
                typeof(CompilerHelperFAFProvider),
                classScope,
                new List<CodegenTypedParam>());

            ctor.Block.AssignRef(Ref("statementFields"), NewInstanceInner(statementFieldsClassName));

            // initialize-event-types
            var initializeEventTypesMethod = MakeInitEventTypes(classScope, compileTimeServices);

            // initialize-query
            var initializeQueryMethod = CodegenMethod
                .MakeMethod(
                    typeof(void),
                    typeof(EPCompilerImpl),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref);
            initializeQueryMethod.Block.AssignMember(
                MEMBERNAME_QUERY_METHOD_PROVIDER,
                NewInstanceInner(queryMethodProviderClassName, EPStatementInitServicesConstants.REF, Ref("statementFields")));

            // get-execute
            var queryMethodProviderProperty = CodegenProperty.MakePropertyNode(
                typeof(FAFQueryMethodProvider),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            queryMethodProviderProperty.GetterBlock.BlockReturn(Ref(MEMBERNAME_QUERY_METHOD_PROVIDER));

            // provide module dependencies
            var moduleDependenciesProperty = CodegenProperty.MakePropertyNode(
                typeof(ModuleDependenciesRuntime),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            moduleDependenciesProperty.GetterBlock
                .BlockReturn(compileTimeServices.ModuleDependencies.Make(
                    initializeQueryMethod, classScope));

            // build stack
            CodegenStackGenerator.RecursiveBuildStack(moduleDependenciesProperty, "ModuleDependencies", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(initializeEventTypesMethod, "InitializeEventTypes", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(initializeQueryMethod, "InitializeQuery", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(queryMethodProviderProperty, "QueryMethodProvider", methods, properties);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(statementFieldsClassName, null, "statementFields"));
            var typedParam = new CodegenTypedParam(typeof(FAFQueryMethodProvider), MEMBERNAME_QUERY_METHOD_PROVIDER);
            typedParam.IsReadonly = false;
            members.Add(typedParam);

            var clazz = new CodegenClass(
                CodegenClassType.FAFPROVIDER,
                typeof(FAFProvider),
                fafProviderClassName,
                classScope,
                members,
                ctor,
                methods,
                properties,
                new EmptyList<CodegenInnerClass>());

            var container = compileTimeServices.Container;
            var compiler = container
                .RoslynCompiler()
                .WithMetaDataReferences(artifactRepository.AllMetadataReferences)
                .WithMetaDataReferences(container.MetadataReferenceProvider()?.Invoke())
                .WithDebugOptimization(compileTimeServices.Configuration.Compiler.IsDebugOptimization)
                .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
                .WithCodegenClasses(new List<CodegenClass>() { clazz });

            artifact = artifactRepository.Register(compiler.Compile());

            return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
                compileTimeServices.Namespace,
                typeof(FAFProvider),
                classPostfix);
        }
    }
} // end of namespace