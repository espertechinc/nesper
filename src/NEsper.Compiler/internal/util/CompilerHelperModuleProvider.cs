///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperStatementProvider;
using static com.espertech.esper.compiler.@internal.util.CompilerVersion;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerHelperModuleProvider
	{
		private const int NUM_STATEMENT_NAMES_PER_BATCH = 1000;

		internal static EPCompiled Compile(
			IList<Compilable> compilables,
			string optionalModuleName,
			IDictionary<ModuleProperty, object> moduleProperties,
			ModuleCompileTimeServices compileTimeServices,
			CompilerOptions compilerOptions,
			CompilerPath path)
		{
			var compilerAbstraction = compileTimeServices.CompilerAbstraction;
			var compilationState = compilerAbstraction.NewArtifactCollection();
			var container = compileTimeServices.Container;
			var artifactRepository = container.ArtifactRepositoryManager().DefaultRepository;
			
			EPCompiledManifest manifest;
			try {
				manifest = CompileToArtifact(
					compilerAbstraction,
					compilationState,
					compilables,
					optionalModuleName,
					moduleProperties,
					compileTimeServices,
					compilerOptions,
					path,
					artifactRepository);
			}
			catch (EPCompileException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPCompileException(
					"Unexpected exception compiling module: " + ex.Message,
					ex,
					EmptyList<EPCompileExceptionItem>.Instance);
			}

			var artifacts = compilationState.Artifacts.OfType<ICompileArtifact>().ToList();
			var compiled = new EPCompiled(artifactRepository, artifacts, manifest);
			if (compilerOptions.PathCache != null) {
				try {
					((CompilerPathCacheImpl)compilerOptions.PathCache).Put(
						compiled,
						ToPathable(optionalModuleName, compileTimeServices, "cached-entry"));
				}
				catch (PathException ex) {
					throw new EPCompileException("Failed to add compiled to path cache: " + ex.Message, ex);
				}
			}

			return compiled;
		}

		private static EPCompiledManifest CompileToArtifact(
			CompilerAbstraction compilerAbstraction,
			CompilerAbstractionArtifactCollection compilationState,
			IList<Compilable> compilables,
			string optionalModuleName,
			IDictionary<ModuleProperty, object> moduleProperties,
			ModuleCompileTimeServices compileTimeServices,
			CompilerOptions compilerOptions,
			CompilerPath path,
			IArtifactRepository artifactRepository)
		{
			var moduleAssignedName = optionalModuleName ?? Guid.NewGuid().ToString();
			var moduleIdentPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(moduleAssignedName);

			// compile each statement
			IList<string> statementClassNames = new List<string>();
			ISet<string> statementNames = new HashSet<string>();
			IList<EPCompileExceptionItem> exceptions = new List<EPCompileExceptionItem>();
			IList<EPCompileExceptionItem> postLatchThrowables = new List<EPCompileExceptionItem>();
			var compilerPool = new CompilerPool(
				compilables.Count,
				compileTimeServices,
				path.Compileds,
				compilerAbstraction,
				compilationState);
			var targetHA = compileTimeServices.SerdeEventTypeRegistry.IsTargetHA;
			var fabricStatements = targetHA
				? (IList<FabricStatement>)new List<FabricStatement>()
				: EmptyList<FabricStatement>.Instance;

			try {
				var statementNumber = 0;
				foreach (var compilable in compilables) {
					string className = null;
					EPCompileExceptionItem exception = null;

					try {
						var compilableItem = CompileItem(
							compilable,
							optionalModuleName,
							moduleIdentPostfix,
							statementNumber,
							statementNames,
							compileTimeServices,
							compilerOptions,
							artifactRepository);
						
						className = compilableItem.ProviderClassName;

						if (targetHA) {
							var fabricStatement = compileTimeServices.StateMgmtSettingsProvider.Statement(
								statementNumber,
								compilableItem.ContextDescriptor,
								compilableItem.FabricCharge);
							fabricStatements.Add(fabricStatement);
						}

						compilerPool.Submit(statementNumber, compilableItem);

						// there can be a post-compile step, which may block submitting further compilables
						try {
							compilableItem.PostCompileLatch.AwaitAndRun();
						}
						catch (Exception ex) {
							postLatchThrowables.Add(
								new EPCompileExceptionItem(
									ex.Message,
									ex,
									compilable.ToEPL(),
									compilable.LineNumber));
						}
					}
					catch (StatementSpecCompileException ex) {
						if (ex is StatementSpecCompileSyntaxException) {
							exception = new EPCompileExceptionSyntaxItem(
								ex.Message,
								ex,
								ex.Expression,
								compilable.LineNumber);
						}
						else {
							exception = new EPCompileExceptionItem(
								ex.Message,
								ex,
								ex.Expression,
								compilable.LineNumber);
						}

						exceptions.Add(exception);
					}
					catch (Exception ex) {
						exception = new EPCompileExceptionItem(
							ex.Message,
							ex,
							compilable.ToEPL(),
							compilable.LineNumber);
						exceptions.Add(exception);
					}

					if (exception == null) {
						statementClassNames.Add(className);
					}

					statementNumber++;
				}
			}
			catch (EPException) {
				compilerPool.Shutdown();
				throw;
			}
			catch (ThreadInterruptedException ex) {
				compilerPool.Shutdown();
				throw new EPCompileException(ex.Message, ex);
			}
			catch (Exception ex) {
				compilerPool.Shutdown();
				throw new EPCompileException(ex.Message, ex);
			}

			// await async compilation
			compilerPool.ShutdownCollectResults();

			exceptions.AddAll(postLatchThrowables);
			if (!exceptions.IsEmpty()) {
				compilerPool.Shutdown();
				var ex = exceptions[0];
				throw new EPCompileException(ex.Message + " [" + ex.Expression + "]", ex, exceptions);
			}

			// compile module resource
			var moduleProviderClassName = CompileModule(
				optionalModuleName,
				moduleProperties,
				statementClassNames,
				moduleIdentPostfix,
				compilerAbstraction,
				compilationState,
				compileTimeServices,
				path.Compileds,
				artifactRepository,
				out var moduleArtifact);

			compilationState.Artifacts.Add(moduleArtifact);

			// remove path create-class class-provided artifacts
			compileTimeServices.ClassProvidedCompileTimeResolver.RemoveFrom(compilationState.Remove);
			
			// add class-provided create-class classes to artifacts
			compileTimeServices.ClassProvidedCompileTimeRegistry.AddTo(compilationState.Add);

			// add HA-fabric to module bytes
			if (compileTimeServices.SerdeEventTypeRegistry.IsTargetHA) {
				compileTimeServices.StateMgmtSettingsProvider.Spec(
					fabricStatements,
					compileTimeServices,
					new IArtifact[] { moduleArtifact });
			}

			// create manifest
			return new EPCompiledManifest(
				COMPILER_VERSION,
				moduleProviderClassName,
				null,
				compileTimeServices.SerdeResolver.IsTargetHA);
		}

		private static string CompileModule(
			string optionalModuleName,
			IDictionary<ModuleProperty, object> moduleProperties,
			IList<string> statementClassNames,
			string moduleIdentPostfix,
			CompilerAbstraction compilerAbstraction,
			CompilerAbstractionArtifactCollection compilationState,
			ModuleCompileTimeServices compileTimeServices,
			IList<EPCompiled> path,
			IArtifactRepository artifactRepository,
			out ICompileArtifact artifact)
		{
			var serializerFactory = compileTimeServices.Container.SerializerFactory();
			
			// write code to create an implementation of StatementResource
			var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
				typeof(StatementFields),
				moduleIdentPostfix);
			var namespaceScope = new CodegenNamespaceScope(
				compileTimeServices.Namespace,
				statementFieldsClassName,
				compileTimeServices.IsInstrumented,
				compileTimeServices.Configuration.Compiler.ByteCode);
			var moduleClassName =
				CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(ModuleProvider), moduleIdentPostfix);
			var classScope = new CodegenClassScope(true, namespaceScope, moduleClassName);
			var methods = new CodegenClassMethods();
			var properties = new CodegenClassProperties();

			// provide module name
			CodegenProperty moduleNameProp = null;
			if (optionalModuleName != null) {
				moduleNameProp = CodegenProperty.MakePropertyNode(
					typeof(string),
					typeof(EPCompilerImpl),
					CodegenSymbolProviderEmpty.INSTANCE,
					classScope);
				moduleNameProp.GetterBlock.BlockReturn(Constant(optionalModuleName));
			}

			// provide module properties
			CodegenProperty modulePropertiesProp = null;
			if (!moduleProperties.IsEmpty()) {
				modulePropertiesProp = CodegenProperty.MakePropertyNode(
					typeof(IDictionary<ModuleProperty, object>),
					typeof(EPCompilerImpl),
					CodegenSymbolProviderEmpty.INSTANCE,
					classScope);
				MakeModuleProperties(serializerFactory, moduleProperties, modulePropertiesProp);
			}

			// provide module dependencies
			var moduleDependenciesProp = CodegenProperty.MakePropertyNode(
				typeof(ModuleDependenciesRuntime),
				typeof(EPCompilerImpl),
				CodegenSymbolProviderEmpty.INSTANCE,
				classScope);
			compileTimeServices.ModuleDependencies.Make(moduleDependenciesProp, classScope);

			// register types
			var initializeEventTypesMethodOpt = MakeInitEventTypesOptional(classScope, compileTimeServices);

			// register named windows
			var initializeNamedWindowsMethodOpt =
				MakeInitNamedWindowsOptional(classScope, compileTimeServices);

			// register tables
			var initializeTablesMethodOpt = MakeInitTablesOptional(classScope, compileTimeServices);

			// register indexes
			var initializeIndexesMethodOpt = MakeInitIndexesOptional(classScope, compileTimeServices);

			// register contexts
			var initializeContextsMethodOpt = MakeInitContextsOptional(classScope, compileTimeServices);

			// register variables
			var initializeVariablesMethodOpt = MakeInitVariablesOptional(classScope, compileTimeServices);

			// register expressions
			var initializeExprDeclaredMethodOpt = MakeInitDeclExprOptional(classScope, compileTimeServices);

			// register scripts
			var initializeScriptsMethodOpt = MakeInitScriptsOptional(classScope, compileTimeServices);

			// register provided classes
			var initializeClassProvidedMethodOpt =
				MakeInitClassProvidedOptional(classScope, compileTimeServices);

			// instantiate factories for statements
			var statementsProp = CodegenProperty.MakePropertyNode(
				typeof(IList<StatementProvider>),
				typeof(EPCompilerImpl),
				CodegenSymbolProviderEmpty.INSTANCE,
				classScope);
			MakeStatementsMethod(
				statementsProp,
				statementClassNames,
				classScope);

			// build stack
			if (moduleNameProp != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					moduleNameProp,
					"ModuleName",
					methods,
					properties);
			}

			if (modulePropertiesProp != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					modulePropertiesProp,
					"ModuleProperties",
					methods,
					properties);
			}

			CodegenStackGenerator.RecursiveBuildStack(
				moduleDependenciesProp,
				"ModuleDependencies",
				methods,
				properties);
			
			if (initializeEventTypesMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeEventTypesMethodOpt,
					"InitializeEventTypes",
					methods,
					properties);
			}

			if (initializeNamedWindowsMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeNamedWindowsMethodOpt,
					"InitializeNamedWindows",
					methods,
					properties);
			}

			if (initializeTablesMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeTablesMethodOpt,
					"InitializeTables",
					methods,
					properties);
			}

			if (initializeIndexesMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeIndexesMethodOpt,
					"InitializeIndexes",
					methods,
					properties);
			}

			if (initializeContextsMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeContextsMethodOpt,
					"InitializeContexts",
					methods,
					properties);
			}

			if (initializeVariablesMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeVariablesMethodOpt,
					"InitializeVariables",
					methods,
					properties);
			}

			if (initializeExprDeclaredMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeExprDeclaredMethodOpt,
					"InitializeExprDeclareds",
					methods,
					properties);
			}

			if (initializeScriptsMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeScriptsMethodOpt,
					"InitializeScripts",
					methods,
					properties);
			}

			if (initializeClassProvidedMethodOpt != null) {
				CodegenStackGenerator.RecursiveBuildStack(
					initializeClassProvidedMethodOpt,
					"InitializeClassProvided",
					methods,
					properties);
			}

			CodegenStackGenerator.RecursiveBuildStack(statementsProp, "Statements", methods, properties);

			var clazz = new CodegenClass(
				CodegenClassType.MODULEPROVIDER,
				typeof(ModuleProvider),
				moduleClassName,
				classScope,
				EmptyList<CodegenTypedParam>.Instance,
				null,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
			
			var context = new CompilerAbstractionCompilationContext(compileTimeServices, path);
			
			artifact = compilerAbstraction.CompileClasses(
				Collections.SingletonList(clazz),
				context,
				compilationState);

			return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
				compileTimeServices.Namespace,
				typeof(ModuleProvider),
				moduleIdentPostfix);
		}

		private static CodegenMethod MakeInitClassProvidedOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.ClassProvidedCompileTimeRegistry.Classes.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleClassProvidedInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(
					typeof(EPModuleClassProvidedInitServices),
					ModuleClassProvidedInitializeSymbol.REF_INITSVC.Ref);
			foreach (var clazz in compileTimeServices.ClassProvidedCompileTimeRegistry
				         .Classes) {
				var addClassProvided = RegisterClassProvidedCodegen(clazz, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addClassProvided));
			}

			return method;
		}

		private static CodegenMethod MakeInitScriptsOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.ScriptCompileTimeRegistry.Scripts.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleScriptInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleScriptInitServices), ModuleScriptInitializeSymbol.REF_INITSVC.Ref);
			foreach (var expression in compileTimeServices
				         .ScriptCompileTimeRegistry.Scripts) {
				var addScript = RegisterScriptCodegen(expression, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addScript));
			}

			return method;
		}

		private static CodegenMethod MakeInitDeclExprOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.ExprDeclaredCompileTimeRegistry.Expressions.IsEmpty()) {
				return null;
			}

			var serializerFactory = compileTimeServices.Container.SerializerFactory();
			var symbols = new ModuleExpressionDeclaredInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(
					typeof(EPModuleExprDeclaredInitServices),
					ModuleExpressionDeclaredInitializeSymbol.REF_INITSVC.Ref);
			foreach (var expression in compileTimeServices.ExprDeclaredCompileTimeRegistry.Expressions) {
				var addExpression = RegisterExprDeclaredCodegen(serializerFactory, expression, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addExpression));
			}

			return method;
		}

		private static CodegenMethod MakeInitVariablesOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.VariableCompileTimeRegistry.Variables.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleVariableInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleVariableInitServices), ModuleVariableInitializeSymbol.REF_INITSVC.Ref);
			foreach (var variable in compileTimeServices.VariableCompileTimeRegistry
				         .Variables) {
				var addVariable = RegisterVariableCodegen(variable, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addVariable));
			}

			return method;
		}

		private static CodegenMethod MakeInitContextsOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.ContextCompileTimeRegistry.Contexts.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleContextInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleContextInitServices), ModuleContextInitializeSymbol.REF_INITSVC.Ref);
			foreach (var context in compileTimeServices.ContextCompileTimeRegistry
				         .Contexts) {
				var addContext = RegisterContextCodegen(context, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addContext));
			}

			return method;
		}

		private static CodegenMethod MakeInitIndexesOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.IndexCompileTimeRegistry.Indexes.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleIndexesInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleIndexInitServices), EPModuleIndexInitServicesConstants.REF.Ref);
			foreach (var index in compileTimeServices
				         .IndexCompileTimeRegistry.Indexes) {
				var addIndex = RegisterIndexCodegen(index, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addIndex));
			}

			return method;
		}

		private static CodegenMethod MakeInitTablesOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.TableCompileTimeRegistry.Tables.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleTableInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleTableInitServices), ModuleTableInitializeSymbol.REF_INITSVC.Ref);
			foreach (var table in compileTimeServices.TableCompileTimeRegistry.Tables
				         ) {
				var addTable = RegisterTableCodegen(table, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addTable));
			}

			return method;
		}

		private static CodegenMethod MakeInitNamedWindowsOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (compileTimeServices.NamedWindowCompileTimeRegistry.NamedWindows.IsEmpty()) {
				return null;
			}

			var symbols = new ModuleNamedWindowInitializeSymbol();
			var method = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbols, classScope)
				.AddParam(typeof(EPModuleNamedWindowInitServices), ModuleNamedWindowInitializeSymbol.REF_INITSVC.Ref);
			foreach (var namedWindow in compileTimeServices
				         .NamedWindowCompileTimeRegistry.NamedWindows) {
				var addNamedWindow = RegisterNamedWindowCodegen(namedWindow, method, classScope, symbols);
				method.Block.Expression(LocalMethod(addNamedWindow));
			}

			return method;
		}

		private static void MakeStatementsMethod(
			CodegenProperty statementsProperty,
			IList<string> statementClassNames,
			CodegenClassScope classScope)
		{
			CodegenExpression returnValue;
			if (statementClassNames.IsEmpty()) {
				returnValue = StaticMethod(
					typeof(Collections),
					"GetEmptyList",
					new Type[] {
						typeof(StatementProvider)
					});
			}
			else if (statementClassNames.Count == 1) {
				returnValue = StaticMethod(
					typeof(Collections),
					"SingletonList",
					new Type[] {
						typeof(StatementProvider)
					},
					NewInstanceInner(statementClassNames[0]));
			}
			else {
				statementsProperty.GetterBlock.DeclareVar<IList<StatementProvider>>(
					"statements",
					NewInstance(typeof(List<StatementProvider>), Constant(statementClassNames.Count)));
				if (statementClassNames.Count <= NUM_STATEMENT_NAMES_PER_BATCH) {
					MakeStatementsAdd(statementsProperty.GetterBlock, statementClassNames);
				}
				else {
					// subdivide to N each
					var lists = CollectionUtil.Subdivide(
						statementClassNames,
						NUM_STATEMENT_NAMES_PER_BATCH);
					foreach (var names in lists) {
						// var sub = statementsProperty
						// 	.MakeChild(typeof(void), typeof(CompilerHelperModuleProvider), classScope)
						// 	.AddParam(typeof(IList<StatementProvider>), "statements");
						// MakeStatementsAdd(sub.Block, names);
						// statementsProperty.Block.LocalMethod(sub, Ref("statements"));

						MakeStatementsAdd(statementsProperty.Block, names);
					}
				}

				returnValue = Ref("statements");
			}

			statementsProperty.GetterBlock.BlockReturn(returnValue);
		}

		private static void MakeStatementsAdd(
			CodegenBlock statementBlock,
			ICollection<string> statementClassNames)
		{
			foreach (var statementClassName in statementClassNames) {
				statementBlock.ExprDotMethod(
					Ref("statements"),
					"Add",
					NewInstanceInner(statementClassName));
			}
		}

		private static void MakeModuleProperties(
			SerializerFactory serializerFactory,
			IDictionary<ModuleProperty, object> props,
			CodegenProperty property)
		{
			if (props.IsEmpty()) {
                property.GetterBlock.BlockReturn(
                    StaticMethod(
                        typeof(Collections),
                        "GetEmptyMap",
                        new Type[] {
                            typeof(ModuleProperty),
                            typeof(object)
                        }));

				return;
			}

			if (props.Count == 1) {
				var entry = props.First();
				property.GetterBlock.BlockReturn(
					StaticMethod(
						typeof(Collections),
						"SingletonMap",
						new Type[] {
							typeof(ModuleProperty),
							typeof(object)
						},
						MakeModulePropKey(entry.Key),
						MakeModulePropValue(serializerFactory, entry.Value)));
				return;
			}

			property.GetterBlock
				.DeclareVar(
					typeof(IDictionary<ModuleProperty, object>),
					"props",
					NewInstance(typeof(Dictionary<ModuleProperty, object>)));
			foreach (var entry in props) {
				property.GetterBlock.ExprDotMethod(
					Ref("props"),
					"Put",
					MakeModulePropKey(entry.Key),
					MakeModulePropValue(serializerFactory, entry.Value));
			}

			property.GetterBlock.BlockReturn(Ref("props"));
		}

		private static CodegenExpression MakeModulePropKey(ModuleProperty key)
		{
			return EnumValue(typeof(ModuleProperty), key.GetName());
		}

		private static CodegenExpression MakeModulePropValue(
			SerializerFactory serializerFactory,
			object value)
		{
			return SerializerUtil.ExpressionForUserObject(serializerFactory, value);
		}

		private static CodegenMethod RegisterClassProvidedCodegen(
			KeyValuePair<string, ClassProvided> classProvided,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleClassProvidedInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleClassProvidedInitServicesConstants.GETCLASSPROVIDEDCOLLECTOR)
						.Add("RegisterClass", Constant(classProvided.Key), classProvided.Value.Make(method, classScope, symbols)));
			return method;
		}

		private static CodegenMethod RegisterScriptCodegen(
			KeyValuePair<NameAndParamNum, ExpressionScriptProvided> script,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleScriptInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleScriptInitServicesConstants.GETSCRIPTCOLLECTOR)
						.Add(
							"RegisterScript",
							Constant(script.Key.Name),
							Constant(script.Key.ParamNum),
							script.Value.Make(method, classScope)));
			return method;
		}

		private static CodegenMethod RegisterExprDeclaredCodegen(
			SerializerFactory serializerFactory,
			KeyValuePair<string, ExpressionDeclItem> expression,
			CodegenMethod parent,
			CodegenClassScope classScope,
			ModuleExpressionDeclaredInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);

			var item = expression.Value;
			var bytes = SerializerUtil.ObjectToByteArr(serializerFactory, item.OptionalSoda);
			item.OptionalSodaBytes = () => bytes;

			method.Block
				.DeclareVar<ExpressionDeclItem>("detail", expression.Value.Make(method, symbols, classScope))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleExprDeclaredInitServicesConstants.GETEXPRDECLAREDCOLLECTOR)
						.Add("RegisterExprDeclared", Constant(expression.Key), Ref("detail")));
			return method;
		}

		public static CodegenMethod MakeInitEventTypesOptional(
			CodegenClassScope classScope,
			ModuleCompileTimeServices compileTimeServices)
		{
			if (!HasEventTypes(compileTimeServices)) {
				return null;
			}

			var symbolsEventTypeInit = new ModuleEventTypeInitializeSymbol();
			var initializeEventTypesMethod = CodegenMethod
				.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsEventTypeInit, classScope)
				.AddParam(typeof(EPModuleEventTypeInitServices), ModuleEventTypeInitializeSymbol.REF_INITSVC.Ref);
			foreach (var eventType in compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded) {
				var addType = RegisterEventTypeCodegen(
					eventType,
					initializeEventTypesMethod,
					classScope,
					symbolsEventTypeInit);
				initializeEventTypesMethod.Block.Expression(LocalMethod(addType));
			}

			if (compileTimeServices.SerdeEventTypeRegistry.IsTargetHA) {
				foreach (var pair in compileTimeServices
					         .SerdeEventTypeRegistry.EventTypes) {
					var addSerde = RegisterEventTypeSerdeCodegen(
						pair.Key,
						pair.Value,
						initializeEventTypesMethod,
						classScope,
						symbolsEventTypeInit);
					initializeEventTypesMethod.Block.Expression(LocalMethod(addSerde));
				}
			}

			return initializeEventTypesMethod;
		}

		private static bool HasEventTypes(ModuleCompileTimeServices compileTimeServices)
		{
			var has = !compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded.IsEmpty();
			if (!has) {
				has = !compileTimeServices.SerdeEventTypeRegistry.EventTypes.IsEmpty();
			}

			return has;
		}

		private static CodegenMethod RegisterNamedWindowCodegen(
			KeyValuePair<string, NamedWindowMetaData> namedWindow,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleNamedWindowInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<NamedWindowMetaData>("detail", namedWindow.Value.Make(symbols.GetAddInitSvc(method)))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleNamedWindowInitServicesConstants.GETNAMEDWINDOWCOLLECTOR)
						.Add(
							"RegisterNamedWindow",
							Constant(namedWindow.Key),
							Ref("detail")));
			return method;
		}

		private static CodegenMethod RegisterTableCodegen(
			KeyValuePair<string, TableMetaData> table,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleTableInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<TableMetaData>("detail", table.Value.Make(parent, symbols, classScope))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleTableInitServicesConstants.GETTABLECOLLECTOR)
						.Add(
							"RegisterTable",
							Constant(table.Key),
							Ref("detail")));
			return method;
		}

		private static CodegenMethod RegisterIndexCodegen(
			KeyValuePair<IndexCompileTimeKey, IndexDetailForge> index,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleIndexesInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<IndexCompileTimeKey>("key", index.Key.Make(symbols.GetAddInitSvc(method)))
				.DeclareVar<IndexDetail>("detail", index.Value.Make(method, symbols, classScope))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleIndexInitServicesConstants.INDEXCOLLECTOR)
						.Add("RegisterIndex", Ref("key"), Ref("detail")));
			return method;
		}

		private static CodegenMethod RegisterContextCodegen(
			KeyValuePair<string, ContextMetaData> context,
			CodegenMethod parent,
			CodegenClassScope classScope,
			ModuleContextInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<ContextMetaData>("detail", context.Value.Make(symbols.GetAddInitSvc(method)))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleContextInitServicesConstants.GETCONTEXTCOLLECTOR)
						.Add("RegisterContext", Constant(context.Key), Ref("detail")));
			return method;
		}

		private static CodegenMethod RegisterVariableCodegen(
			KeyValuePair<string, VariableMetaData> variable,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleVariableInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<VariableMetaData>("detail", variable.Value.Make(symbols.GetAddInitSvc(method)))
				.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleVariableInitServicesConstants.GETVARIABLECOLLECTOR)
						.Add("RegisterVariable", Constant(variable.Key), Ref("detail")));
			return method;
		}

		private static CodegenMethod RegisterEventTypeCodegen(
			EventType eventType,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleEventTypeInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);

			// metadata
			method.Block.DeclareVar<EventTypeMetadata>("metadata", eventType.Metadata.ToExpression());

			if (eventType is JsonEventType jsonEventType) {
				method.Block
					.DeclareVar(
						typeof(LinkedHashMap<string, object>),
						"props",
						LocalMethod(
							MakePropsCodegen(
								jsonEventType.Types,
								method,
								symbols,
								classScope,
								() => jsonEventType.DeepSuperTypes)));
				var superTypeNames = GetSupertypeNames(jsonEventType);
				var detailExpr = jsonEventType.Detail.ToExpression(method, classScope);
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterJson",
							Ref("metadata"),
							Ref("props"),
							Constant(superTypeNames),
							Constant(jsonEventType.StartTimestampPropertyName),
							Constant(jsonEventType.EndTimestampPropertyName),
							detailExpr));
			}
			else if (eventType is BaseNestableEventType baseNestable) {
				method.Block.DeclareVar(
					typeof(LinkedHashMap<string, object>),
					"props",
					LocalMethod(
						MakePropsCodegen(
							baseNestable.Types,
							method,
							symbols,
							classScope,
							() => baseNestable.DeepSuperTypes)));
				var registerMethodName = baseNestable is MapEventType ? "RegisterMap" : "RegisterObjectArray";
				var superTypeNames = GetSupertypeNames(baseNestable);
				
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							registerMethodName,
							Ref("metadata"),
							Ref("props"),
							Constant(superTypeNames),
							Constant(baseNestable.StartTimestampPropertyName),
							Constant(baseNestable.EndTimestampPropertyName)));
			}
			else if (eventType is WrapperEventType wrapper) {
				method.Block.DeclareVar<EventType>(
					"inner",
					EventTypeUtility.ResolveTypeCodegen(
						wrapper.UnderlyingEventType,
						symbols.GetAddInitSvc(method)));
				method.Block.DeclareVar(
					typeof(LinkedHashMap<string, object>),
					"props",
					LocalMethod(
						MakePropsCodegen(
							wrapper.UnderlyingMapType.Types,
							method,
							symbols,
							classScope,
							() => wrapper.UnderlyingMapType.DeepSuperTypes)));
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add("RegisterWrapper", Ref("metadata"), Ref("inner"), Ref("props")));
			}
			else if (eventType is BeanEventType beanType) {
				var superTypes = MakeSupertypes(beanType.SuperTypes, symbols.GetAddInitSvc(method));
				var deepSuperTypes = MakeDeepSupertypes(
					beanType.DeepSuperTypesCollection,
					method,
					symbols,
					classScope);
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterBean",
							Ref("metadata"),
							Typeof(beanType.UnderlyingType),
							Constant(beanType.StartTimestampPropertyName),
							Constant(beanType.EndTimestampPropertyName),
							superTypes,
							deepSuperTypes));
			}
			else if (eventType is SchemaXMLEventType schemaXmlEventType &&
			         schemaXmlEventType.RepresentsFragmentOfProperty != null) {
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterXML",
							Ref("metadata"),
							Constant(schemaXmlEventType.RepresentsFragmentOfProperty),
							Constant(schemaXmlEventType.RepresentsOriginalTypeName)));
			}
			else if (eventType is BaseXMLEventType baseXmlEventType) {
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterXMLNewType",
							Ref("metadata"),
							baseXmlEventType.ConfigurationEventTypeXMLDOM.ToExpression(method, classScope)));
			}
			else if (eventType is AvroSchemaEventType avroSchemaEventType) {
				var avroTypeSchema = avroSchemaEventType.SchemaAsJson;
				var superTypeNames = GetSupertypeNames(avroSchemaEventType);
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterAvro",
							Ref("metadata"),
							Constant(avroTypeSchema),
							Constant(superTypeNames)));
			}
			else if (eventType is VariantEventType variantEventType) {
				method.Block.Expression(
					ExprDotMethodChain(symbols.GetAddInitSvc(method))
						.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
						.Add(
							"RegisterVariant",
							Ref("metadata"),
							EventTypeUtility.ResolveTypeArrayCodegen(
								variantEventType.Variants,
								symbols.GetAddInitSvc(method)),
							Constant(variantEventType.IsVariantAny)));
			}
			else {
				throw new IllegalStateException("Event type '" + eventType + "' cannot be registered");
			}

			return method;
		}

		private static string[] GetSupertypeNames(EventType eventType)
		{
			var superTypes = eventType.SuperTypes;
			var superTypesCount = superTypes?.Count ?? 0;
			
			if (superTypes != null && superTypesCount > 0) {
				var superTypeNames = new string[superTypesCount];
				for (var i = 0; i < superTypesCount; i++) {
					superTypeNames[i] = superTypes[i].Name;
				}

				return superTypeNames;
			}

			return Array.Empty<string>();
		}

		private static CodegenMethod RegisterEventTypeSerdeCodegen(
			EventType eventType,
			DataInputOutputSerdeForge serdeForge,
			CodegenMethodScope parent,
			CodegenClassScope classScope,
			ModuleEventTypeInitializeSymbol symbols)
		{
			var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
			method.Block
				.DeclareVar<EventTypeMetadata>("metadata", eventType.Metadata.ToExpression())
				.DeclareVar<EventTypeResolver>(
					"resolver",
					ExprDotMethod(symbols.GetAddInitSvc(method), EPModuleEventTypeInitServicesConstants.GETEVENTTYPERESOLVER))
				.DeclareVar<DataInputOutputSerde>(
					"serde",
					serdeForge.Codegen(method, classScope, Ref("resolver")));
			method.Block.Expression(
				ExprDotMethodChain(symbols.GetAddInitSvc(method))
					.Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
					.Add("RegisterSerde", Ref("metadata"), Ref("serde"), Constant(eventType.UnderlyingType)));
			return method;
		}

		private static CodegenExpression MakeDeepSupertypes(
			ICollection<EventType> deepSuperTypes,
			CodegenMethodScope parent,
			ModuleEventTypeInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			if (deepSuperTypes == null || deepSuperTypes.IsEmpty()) {
                return StaticMethod(typeof(Collections), "GetEmptySet", new[] {typeof(EventType)});
			}

			if (deepSuperTypes.Count == 1) {
				return StaticMethod(
					typeof(Collections),
                    "SingletonSet",
                    new Type[] {typeof(EventType)},
					EventTypeUtility.ResolveTypeCodegen(
						deepSuperTypes.First(),
						symbols.GetAddInitSvc(parent)));
			}

			var method = parent.MakeChild(
                typeof(ISet<EventType>),
				typeof(CompilerHelperModuleProvider),
				classScope);
            method.Block.DeclareVar<ISet<EventType>>(
				"dst",
                NewInstance(typeof(LinkedHashSet<EventType>)));
			foreach (var eventType in deepSuperTypes) {
				var initServicesRef = symbols.GetAddInitSvc(method);
				var valueToAdd = EventTypeUtility.ResolveTypeCodegen(eventType, initServicesRef);
				method.Block.ExprDotMethod(Ref("dst"), "Add", valueToAdd);
			}

			method.Block.MethodReturn(Ref("dst"));
			return LocalMethod(method);
		}

		private static CodegenExpression MakeSupertypes(
			IList<EventType> superTypes,
			CodegenExpressionRef initSvcRef)
		{
			if (superTypes == null || superTypes.Count == 0) {
				return ConstantNull();
			}

            var expressions = superTypes
                .Select(e => EventTypeUtility.ResolveTypeCodegen(e, initSvcRef))
                .ToArray();

			return NewArrayWithInit(typeof(EventType), expressions);
		}

		private static CodegenMethod MakePropsCodegen(
			IDictionary<string, object> types,
			CodegenMethodScope parent,
			ModuleEventTypeInitializeSymbol symbols,
			CodegenClassScope classScope,
			Supplier<IEnumerable<EventType>> deepSuperTypes)
		{
			var method = parent.MakeChild(
                typeof(LinkedHashMap<string, object>),
				typeof(CompilerHelperModuleProvider),
				classScope);
			symbols.GetAddInitSvc(method);

            method.Block.DeclareVar<LinkedHashMap<string, object>>(
				"props",
				NewInstance(typeof(LinkedHashMap<string, object>)));


			ICollection<KeyValuePair<string, object>> entries;

			var deepTypes = deepSuperTypes?.Invoke() ?? EmptyList<EventType>.Instance;
			if (deepTypes.HasFirst()) {
				entries = new List<KeyValuePair<string, object>>();
				foreach (var entry in types) {
					var propertyOfSupertype = IsPropertyOfSupertype(deepSuperTypes, entry.Key);
					if (!propertyOfSupertype) {
						entries.Add(entry);
					}
				}
			}
			else {
				entries = types;
			}

			CodegenRepetitiveValueBuilder<KeyValuePair<string, object>>.ConsumerByValue consumer = (
				entry,
				index,
				leafMethod) => {
				var type = entry.Value;
				var typeResolver = MakeTypeResolver(type, leafMethod, symbols, classScope);
				leafMethod.Block.ExprDotMethod(Ref("props"), "Put", Constant(entry.Key), typeResolver);
			};

			new CodegenRepetitiveValueBuilder<KeyValuePair<string, object>>(
					entries,
					method,
					classScope,
					typeof(CompilerHelperModuleProvider))
				.AddParam(typeof(LinkedHashMap<string, object>), "props")
				.SetConsumer(consumer)
				.Build();

			method.Block.MethodReturn(Ref("props"));
			return method;
		}

		private static CodegenExpression MakeTypeResolver(
			object type,
			CodegenMethodScope parent,
			ModuleEventTypeInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			if (type is Type asType) {
				if (asType == typeof(FlexCollection)) {
					asType = typeof(ICollection<object>);
				}

				//return CodegenExpressionType.ToExpression(asType);
				return Typeof(asType);
			}
			else if (type is EventType eventType) {
				return EventTypeUtility.ResolveTypeCodegen(eventType, ModuleEventTypeInitializeSymbol.REF_INITSVC);
			}
			else if (type is EventType[] types) {
				var typeExpr = EventTypeUtility.ResolveTypeCodegen(
					types[0],
					ModuleEventTypeInitializeSymbol.REF_INITSVC);
				return NewArrayWithInit(typeof(EventType), typeExpr);
			}
			else if (type == null) {
				return ConstantNull();
			}
			else if (type is TypeBeanOrUnderlying underlying) {
				var innerType = underlying.EventType;
				var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(
					innerType,
					ModuleEventTypeInitializeSymbol.REF_INITSVC);
				return NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr);
			}
			else if (type is TypeBeanOrUnderlying[] underlyings) {
				var innerType = underlyings[0].EventType;
				var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(
					innerType,
					ModuleEventTypeInitializeSymbol.REF_INITSVC);
				return NewArrayWithInit(
					typeof(TypeBeanOrUnderlying),
					NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr));
			}
			else if (type is IDictionary<string, object> objects) {
				return LocalMethod(
					MakePropsCodegen(objects, parent, symbols, classScope, null));
			}
			else {
				throw new IllegalStateException("Unrecognized type '" + type + "'");
			}
		}

		private static bool IsPropertyOfSupertype(
			Supplier<IEnumerable<EventType>> deepSuperTypes,
			string key)
		{
			if (deepSuperTypes == null) {
				return false;
			}

			var deepSuperTypesEnumerator = deepSuperTypes.Invoke()?.GetEnumerator();
			if (deepSuperTypesEnumerator != null) {
				while (deepSuperTypesEnumerator.MoveNext()) {
					var type = deepSuperTypesEnumerator.Current;
					if (type.IsProperty(key)) {
						return true;
					}
				}
			}

			return false;
		}

		private static EPCompilerPathableImpl ToPathable(
			string moduleName,
			ModuleCompileTimeServices svc,
			string pathDeployId)
		{
			var pathable = new EPCompilerPathableImpl(moduleName);
			foreach (var type in svc.EventTypeCompileTimeRegistry.NewTypesAdded) {
				if (type.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
					pathable.EventTypePathRegistry.Add(type.Name, moduleName, type, pathDeployId);
				}
			}

			foreach (var entry in svc.VariableCompileTimeRegistry.Variables) {
				if (entry.Value.VariableVisibility.IsNonPrivateNonTransient()) {
					pathable.VariablePathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.ExprDeclaredCompileTimeRegistry.Expressions) {
				if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
					pathable.ExprDeclaredPathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.ScriptCompileTimeRegistry.Scripts) {
				if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
					pathable.ScriptPathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.ClassProvidedCompileTimeRegistry.Classes) {
				if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
					pathable.ClassProvidedPathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.NamedWindowCompileTimeRegistry.NamedWindows) {
				if (entry.Value.EventType.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
					pathable.NamedWindowPathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.TableCompileTimeRegistry.Tables) {
				if (entry.Value.TableVisibility.IsNonPrivateNonTransient()) {
					pathable.TablePathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			foreach (var entry in svc.ContextCompileTimeRegistry.Contexts) {
				if (entry.Value.ContextVisibility.IsNonPrivateNonTransient()) {
					pathable.ContextPathRegistry.Add(entry.Key, moduleName, entry.Value, pathDeployId);
				}
			}

			return pathable;
		}
	}
} // end of namespace
