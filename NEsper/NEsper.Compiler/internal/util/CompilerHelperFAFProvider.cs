///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
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

        public static EPCompiled Compile(
            Compilable compilable,
            ModuleCompileTimeServices services,
            CompilerArguments args)
        {
            var statementCompileTimeServices = new StatementCompileTimeServices(0, services);
            var raw = CompilerHelperSingleEPL.ParseWalk(compilable, statementCompileTimeServices);

            var statementType = StatementTypeUtil.GetStatementType(raw);
            if (statementType != StatementType.SELECT) { // the fire-and-forget spec is null for "select" and populated for I/U/D
                throw new StatementSpecCompileException(
                    "Provided EPL expression is a continuous query expression (not an on-demand query)", compilable.ToEPL());
            }

            var annotations = AnnotationUtil.CompileAnnotations(raw.Annotations, services.ImportServiceCompileTime, compilable);

            // walk subselects, alias expressions, declared expressions, dot-expressions
            var visitor = StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(raw);

            // compile context descriptor
            ContextCompileTimeDescriptor contextDescriptor = null;
            var optionalContextName = raw.OptionalContextName;
            if (optionalContextName != null) {
                var detail = services.ContextCompileTimeResolver.GetContextInfo(optionalContextName);
                if (detail == null) {
                    throw new StatementSpecCompileException("Context by name '" + optionalContextName + "' could not be found", compilable.ToEPL());
                }

                contextDescriptor = new ContextCompileTimeDescriptor(
                    optionalContextName, detail.ContextModuleName, detail.ContextVisibility, new ContextPropertyRegistry(detail),
                    detail.ValidationInfos);
            }

            var statementNameFromAnnotation = GetNameFromAnnotation(annotations);
            var statementName = statementNameFromAnnotation == null ? "q0" : statementNameFromAnnotation.Trim();
            var statementRawInfo = new StatementRawInfo(0, statementName, annotations, statementType.Value, contextDescriptor, null, compilable, null);
            var specCompiled = StatementRawCompiler.Compile(
                raw, compilable, false, true, annotations, visitor.Subselects,
                new List<ExprTableAccessNode>(raw.TableExpressions), statementRawInfo,
                statementCompileTimeServices);
            var fafSpec = specCompiled.Raw.FireAndForgetSpec;

            var packageName = "generated";
            IDictionary<string, byte[]> moduleBytes = new Dictionary<string, byte[]>();
            EPCompiledManifest manifest;
            var classPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(statementName);

            FAFQueryMethodForge query;
            if (specCompiled.Raw.InsertIntoDesc != null) {
                query = new FAFQueryMethodIUDInsertIntoForge(specCompiled, compilable, statementRawInfo, statementCompileTimeServices);
            }
            else if (fafSpec == null) { // null indicates a select-statement, same as continuous query
                var desc = new FAFQueryMethodSelectDesc(specCompiled, compilable, statementRawInfo, statementCompileTimeServices);
                var classNameResultSetProcessor =
                    CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(ResultSetProcessorFactoryProvider), classPostfix);
                query = new FAFQueryMethodSelectForge(desc, classNameResultSetProcessor, statementRawInfo);
            }
            else if (fafSpec is FireAndForgetSpecDelete) {
                query = new FAFQueryMethodIUDDeleteForge(specCompiled, compilable, statementRawInfo, statementCompileTimeServices);
            }
            else if (fafSpec is FireAndForgetSpecUpdate) {
                query = new FAFQueryMethodIUDUpdateForge(specCompiled, compilable, statementRawInfo, statementCompileTimeServices);
            }
            else {
                throw new IllegalStateException("Unrecognized FAF code " + fafSpec);
            }

            // verify substitution parameters
            VerifySubstitutionParams(raw.SubstitutionParameters);

            try {
                manifest = CompileToBytes(query, classPostfix, packageName, moduleBytes, args.Options, services);
            }
            catch (EPCompileException ex) {
                throw;
            }
            catch (Exception ex) {
                throw new EPCompileException("Unexpected exception compiling module: " + ex.Message, ex,
                    new EmptyList<EPCompileExceptionItem>());
            }

            return new EPCompiled(moduleBytes, manifest);
        }

        private static EPCompiledManifest CompileToBytes(
            FAFQueryMethodForge query,
            string classPostfix,
            string packageName,
            IDictionary<string, byte[]> moduleBytes,
            CompilerOptions compilerOptions,
            ModuleCompileTimeServices compileTimeServices)
        {
            string queryMethodProviderClassName;
            try {
                queryMethodProviderClassName = CompilerHelperFAFQuery.CompileQuery(
                    query, classPostfix, packageName, moduleBytes, compileTimeServices);
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
            var fafProviderClassName = MakeFAFProvider(queryMethodProviderClassName, classPostfix, moduleBytes, packageName, compileTimeServices);

            // create manifest
            return new EPCompiledManifest(COMPILER_VERSION, null, fafProviderClassName);
        }

        private static string MakeFAFProvider(
            string queryMethodProviderClassName,
            string classPostfix,
            IDictionary<string, byte[]> moduleBytes,
            string @namespace,
            ModuleCompileTimeServices compileTimeServices)
        {
            var packageScope = new CodegenNamespaceScope(@namespace, null, compileTimeServices.IsInstrumented());
            var fafProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(FAFProvider), classPostfix);
            var classScope = new CodegenClassScope(true, packageScope, fafProviderClassName);
            var methods = new CodegenClassMethods();

            // initialize-event-types
            var initializeEventTypesMethod = MakeInitEventTypes(classScope, compileTimeServices);

            // initialize-query
            var initializeQueryMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope).AddParam(
                    typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
            initializeQueryMethod.Block.AssignRef(
                MEMBERNAME_QUERY_METHOD_PROVIDER, NewInstance(queryMethodProviderClassName, EPStatementInitServicesConstants.REF));

            // get-execute
            var getQueryMethodProviderMethod = CodegenMethod.MakeParentNode(
                typeof(FAFQueryMethodProvider), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getQueryMethodProviderMethod.Block.MethodReturn(Ref(MEMBERNAME_QUERY_METHOD_PROVIDER));

            // build stack
            CodegenStackGenerator.RecursiveBuildStack(initializeEventTypesMethod, "InitializeEventTypes", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeQueryMethod, "InitializeQuery", methods);
            CodegenStackGenerator.RecursiveBuildStack(getQueryMethodProviderMethod, "GetQueryMethodProvider", methods);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            var typedParam = new CodegenTypedParam(typeof(FAFQueryMethodProvider), MEMBERNAME_QUERY_METHOD_PROVIDER);
            typedParam.IsFinal = false;
            members.Add(typedParam);

            var clazz = new CodegenClass(
                typeof(FAFProvider), @namespace, fafProviderClassName, classScope, members,
                null, methods, new EmptyList<CodegenInnerClass>());
            RoslynCompiler.Compile(clazz, moduleBytes, compileTimeServices.Configuration.Compiler.Logging.IsEnableCode);

            return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(@namespace, typeof(FAFProvider), classPostfix);
        }
    }
} // end of namespace