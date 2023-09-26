///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.compiler;
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
using com.espertech.esper.common.@internal.epl.expression.visitor;
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
using static com.espertech.esper.compiler.@internal.util.CompilerVersion;

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
			var compileTimeServices = new StatementCompileTimeServices(0, services);
			var walkResult = CompilerHelperSingleEPL.ParseCompileInlinedClassesWalk(
				compilable,
				args.Options?.InlinedClassInspection,
				compileTimeServices);
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
			var visitor =
				StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(raw);

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
			var statementName = statementNameFromAnnotation?.Trim() ?? "q0";
			var statementRawInfo = new StatementRawInfo(
				0,
				statementName,
				annotations,
				statementType.Value,
				contextDescriptor,
				null,
				compilable,
				null);
			var compiledDesc = StatementRawCompiler.Compile(
				raw,
				compilable,
				false,
				true,
				annotations,
				visitor.Subselects,
				new List<ExprTableAccessNode>(raw.TableExpressions),
				statementRawInfo,
				compileTimeServices);
			var specCompiled = compiledDesc.Compiled;
			var fafSpec = specCompiled.Raw.FireAndForgetSpec;

			var compilerState = services.CompilerAbstraction.NewArtifactCollection();
			compilerState.Add(new [] { walkResult.ClassesInlined.Artifact });

			EPCompiledManifest manifest;
			var classPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(statementName);

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
				var classNameResultSetProcessor = CodeGenerationIDGenerator.GenerateClassNameSimple(
					typeof(ResultSetProcessorFactoryProvider),
					classPostfix);
				query = new FAFQueryMethodSelectForge(desc, classNameResultSetProcessor, statementRawInfo, services);
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
				manifest = CompileToBytes(query, classPostfix, compilerState, services, args.Path);
			}
			catch (EPCompileException ex) {
				throw;
			}
			catch (Exception e) {
				throw new EPCompileException(
					"Unexpected exception compiling module: " + e.Message,
					e,
					EmptyList<EPCompileExceptionItem>.Instance);
			}

			return new EPCompiled(repository, compilerState.Artifacts.OfType<ICompileArtifact>().ToList(), manifest);
		}

		private static EPCompiledManifest CompileToBytes(
			FAFQueryMethodForge query,
			string classPostfix,
			CompilerAbstractionArtifactCollection compilerState,
			ModuleCompileTimeServices compileTimeServices,
			CompilerPath path)
		{

			string queryMethodProviderClassName;
			try {
				queryMethodProviderClassName = CompilerHelperFAFQuery.CompileQuery(
					query,
					classPostfix,
					compilerState,
					compileTimeServices,
					path);
			}
			catch (StatementSpecCompileException ex) {
				EPCompileExceptionItem first;
				if (ex is StatementSpecCompileSyntaxException) {
					first = new EPCompileExceptionSyntaxItem(ex.Message, ex, ex.Expression, -1);
				}
				else {
					first = new EPCompileExceptionItem(ex.Message, ex, ex.Expression, -1);
				}

				var items = Collections.SingletonList(first);
				throw new EPCompileException(ex.Message + " [" + ex.Expression + "]", ex, items);
			}

			// compile query provider
			var fafProviderClassName = MakeFAFProvider(
				queryMethodProviderClassName,
				classPostfix,
				compilerState,
				compileTimeServices,
				path);

			// create manifest
			return new EPCompiledManifest(COMPILER_VERSION, null, fafProviderClassName, false);
		}

		private static string MakeFAFProvider(
			string queryMethodProviderClassName,
			string classPostfix,
			CompilerAbstractionArtifactCollection compilerState,
			ModuleCompileTimeServices compileTimeServices,
			CompilerPath path)
		{
			var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
				typeof(StatementFields), classPostfix);
			var namespaceScope = new CodegenNamespaceScope(
				compileTimeServices.Namespace,
				statementFieldsClassName,
				compileTimeServices.IsInstrumented,
				compileTimeServices.Configuration.Compiler.ByteCode);
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
			var initializeEventTypesMethod = MakeInitEventTypesOptional(classScope, compileTimeServices);

			// initialize-query
			var initializeQueryMethod = CodegenMethod
				.MakeParentNode(
					typeof(void),
					typeof(EPCompilerImpl),
					CodegenSymbolProviderEmpty.INSTANCE,
					classScope)
				.AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
			initializeQueryMethod.Block.AssignMember(
				MEMBERNAME_QUERY_METHOD_PROVIDER,
				NewInstanceInner(queryMethodProviderClassName, EPStatementInitServicesConstants.REF));

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
			
			compileTimeServices.ModuleDependencies.Make(moduleDependenciesProperty, classScope);

			// build stack
			CodegenStackGenerator.RecursiveBuildStack(
				moduleDependenciesProperty,
				"ModuleDependencies",
				methods,
				properties);
			if (initializeEventTypesMethod != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeEventTypesMethod,
					"InitializeEventTypes",
					methods,
					properties);
			}

			CodegenStackGenerator.RecursiveBuildStack(
				initializeQueryMethod,
				"InitializeQuery",
				methods,
				properties);
			CodegenStackGenerator.RecursiveBuildStack(
				queryMethodProviderProperty,
				"QueryMethodProvider",
				methods,
				properties);

			IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
			members.Add(
				new CodegenTypedParam(typeof(FAFQueryMethodProvider), MEMBERNAME_QUERY_METHOD_PROVIDER).WithFinal(false));

			var clazz = new CodegenClass(
				CodegenClassType.FAFPROVIDER,
				typeof(FAFProvider),
				fafProviderClassName,
				classScope,
				members,
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
				
			var context = new CompilerAbstractionCompilationContext(compileTimeServices, path.Compileds);
			
			compileTimeServices.CompilerAbstraction.CompileClasses(
				Collections.SingletonList(clazz),
				context,
				compilerState);

			return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
				compileTimeServices.Namespace,
				typeof(FAFProvider),
				classPostfix);
		}
	}
} // end of namespace
