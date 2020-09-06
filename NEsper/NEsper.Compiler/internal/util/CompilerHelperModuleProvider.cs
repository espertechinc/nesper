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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
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
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.compiler.@internal.util.Version;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperModuleProvider
    {
        protected internal static EPCompiled Compile(
            IList<Compilable> compilables,
            string optionalModuleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            ModuleCompileTimeServices compileTimeServices,
            CompilerOptions compilerOptions)
        {
            try {
                EPCompiledManifest manifest = CompileToBytes(
                    compilables,
                    optionalModuleName,
                    moduleProperties,
                    compileTimeServices,
                    compilerOptions,
                    out var assembly);

                return new EPCompiled(assembly, manifest);
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
        }

        private static EPCompiledManifest CompileToBytes(
            IList<Compilable> compilables,
            string optionalModuleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            ModuleCompileTimeServices compileTimeServices,
            CompilerOptions compilerOptions,
            out Assembly assembly)
        {
            var moduleAssignedName = optionalModuleName ?? Guid.NewGuid().ToString();
            var moduleIdentPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(moduleAssignedName);

            // compile each statement
            var statementNumber = 0;
            IList<string> statementClassNames = new List<string>();
            ISet<string> statementNames = new HashSet<string>();
            foreach (var compilable in compilables) {
                string className = null;
                EPCompileExceptionItem exception = null;


                try {
                    CompilableItem compilableItem = CompilerHelperStatementProvider.CompileItem(
                        compilable,
                        optionalModuleName,
                        moduleIdentPostfix,
                        statementNumber,
                        statementNames,
                        compileTimeServices,
                        compilerOptions,
                        out assembly);
                    className = compilableItem.ProviderClassName;
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

                statementClassNames.Add(className);
                statementNumber++;
            }

            // compile module resource
            var moduleProviderClassName = CompileModule(
                optionalModuleName,
                moduleProperties,
                statementClassNames,
                moduleIdentPostfix,
                compileTimeServices,
                out assembly);

            // create module XML
            return new EPCompiledManifest(COMPILER_VERSION, moduleProviderClassName, null, compileTimeServices.SerdeResolver.IsTargetHA);
        }

        private static string CompileModule(
            string optionalModuleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            IList<string> statementClassNames,
            string moduleIdentPostfix,
            ModuleCompileTimeServices compileTimeServices,
            out Assembly assembly)
        {
            // write code to create an implementation of StatementResource
            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields),
                moduleIdentPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                compileTimeServices.Namespace,
                statementFieldsClassName,
                compileTimeServices.IsInstrumented());
            var moduleClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ModuleProvider),
                moduleIdentPostfix);
            var classScope = new CodegenClassScope(true, namespaceScope, moduleClassName);
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();

            // provide module name
            var moduleNameProp = CodegenProperty.MakePropertyNode(
                typeof(string),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            moduleNameProp.GetterBlock.BlockReturn(Constant(optionalModuleName));

            // provide module properties
            var modulePropertiesProp = CodegenProperty.MakePropertyNode(
                typeof(IDictionary<ModuleProperty, object>),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            MakeModuleProperties(moduleProperties, modulePropertiesProp);

            // provide module dependencies
            var moduleDependenciesProp = CodegenProperty.MakePropertyNode(
                typeof(ModuleDependenciesRuntime),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            compileTimeServices.ModuleDependencies.Inject(moduleDependenciesProp.GetterBlock);

            // register types
            var initializeEventTypesMethod = MakeInitEventTypes(classScope, compileTimeServices);

            // register named windows
            var symbolsNamedWindowInit = new ModuleNamedWindowInitializeSymbol();
            var initializeNamedWindowsMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsNamedWindowInit, classScope)
                .AddParam(
                    typeof(EPModuleNamedWindowInitServices),
                    ModuleNamedWindowInitializeSymbol.REF_INITSVC.Ref);
            foreach (var namedWindow in compileTimeServices.NamedWindowCompileTimeRegistry.NamedWindows) {
                var addNamedWindow = RegisterNamedWindowCodegen(
                    namedWindow,
                    initializeNamedWindowsMethod,
                    classScope,
                    symbolsNamedWindowInit);
                initializeNamedWindowsMethod.Block.Expression(LocalMethod(addNamedWindow));
            }

            // register tables
            var symbolsTableInit = new ModuleTableInitializeSymbol();
            var initializeTablesMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsTableInit, classScope)
                .AddParam(typeof(EPModuleTableInitServices), ModuleTableInitializeSymbol.REF_INITSVC.Ref);
            foreach (var table in compileTimeServices.TableCompileTimeRegistry.Tables) {
                var addTable = RegisterTableCodegen(table, initializeTablesMethod, classScope, symbolsTableInit);
                initializeTablesMethod.Block.Expression(LocalMethod(addTable));
            }

            // register indexes
            var symbolsIndexInit = new ModuleIndexesInitializeSymbol();
            var initializeIndexesMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsIndexInit, classScope)
                .AddParam(typeof(EPModuleIndexInitServices), EPModuleIndexInitServicesConstants.REF.Ref);
            foreach (KeyValuePair<IndexCompileTimeKey, IndexDetailForge> index in compileTimeServices
                .IndexCompileTimeRegistry.Indexes) {
                var addIndex = RegisterIndexCodegen(index, initializeIndexesMethod, classScope, symbolsIndexInit);
                initializeIndexesMethod.Block.Expression(LocalMethod(addIndex));
            }

            // register contexts
            var symbolsContextInit = new ModuleContextInitializeSymbol();
            var initializeContextsMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsContextInit, classScope)
                .AddParam(
                    typeof(EPModuleContextInitServices),
                    ModuleContextInitializeSymbol.REF_INITSVC.Ref);
            foreach (var context in compileTimeServices.ContextCompileTimeRegistry.Contexts) {
                var addContext = RegisterContextCodegen(
                    context,
                    initializeContextsMethod,
                    classScope,
                    symbolsContextInit);
                initializeContextsMethod.Block.Expression(LocalMethod(addContext));
            }

            // register variables
            var symbolsVariablesInit = new ModuleVariableInitializeSymbol();
            var initializeVariablesMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsVariablesInit, classScope)
                .AddParam(
                    typeof(EPModuleVariableInitServices),
                    ModuleVariableInitializeSymbol.REF_INITSVC.Ref);
            foreach (var variable in compileTimeServices.VariableCompileTimeRegistry.Variables) {
                var addVariable = RegisterVariableCodegen(
                    variable,
                    initializeVariablesMethod,
                    classScope,
                    symbolsVariablesInit);
                initializeVariablesMethod.Block.Expression(LocalMethod(addVariable));
            }

            // register expressions
            var symbolsExprDeclaredInit = new ModuleExpressionDeclaredInitializeSymbol();
            var initializeExprDeclaredMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsExprDeclaredInit, classScope)
                .AddParam(
                    typeof(EPModuleExprDeclaredInitServices),
                    ModuleExpressionDeclaredInitializeSymbol.REF_INITSVC.Ref);
            foreach (var expression in compileTimeServices.ExprDeclaredCompileTimeRegistry.Expressions) {
                var addExpression = RegisterExprDeclaredCodegen(
                    expression,
                    initializeExprDeclaredMethod,
                    classScope,
                    symbolsExprDeclaredInit);
                initializeExprDeclaredMethod.Block.Expression(LocalMethod(addExpression));
            }

            // register scripts
            var symbolsScriptInit = new ModuleScriptInitializeSymbol();
            var initializeScriptsMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsScriptInit, classScope)
                .AddParam(typeof(EPModuleScriptInitServices), ModuleScriptInitializeSymbol.REF_INITSVC.Ref);
            foreach (var expression in compileTimeServices.ScriptCompileTimeRegistry.Scripts) {
                var addScript = RegisterScriptCodegen(
                    expression,
                    initializeScriptsMethod,
                    classScope,
                    symbolsScriptInit);
                initializeScriptsMethod.Block.Expression(LocalMethod(addScript));
            }

            // register provided classes
            var symbolsClassProvidedInit = new ModuleClassProvidedInitializeSymbol();
            var initializeClassProvidedMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsClassProvidedInit, classScope)
                .AddParam(typeof(EPModuleClassProvidedInitServices), ModuleClassProvidedInitializeSymbol.REF_INITSVC.Ref);
            foreach (var currClazz in compileTimeServices.ClassProvidedCompileTimeRegistry.Classes) {
                var addClassProvided = RegisterClassProvidedCodegen(
                    currClazz,
                    initializeClassProvidedMethod,
                    classScope,
                    symbolsClassProvidedInit);
                initializeClassProvidedMethod.Block.Expression(LocalMethod(addClassProvided));
            }
            
            // instantiate factories for statements
            var statementsProp = CodegenProperty.MakePropertyNode(
                typeof(IList<StatementProvider>),
                typeof(EPCompilerImpl),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            statementsProp.GetterBlock.DeclareVar<IList<StatementProvider>>(
                "statements",
                NewInstance(typeof(List<StatementProvider>), Constant(statementClassNames.Count)));
            foreach (var statementClassName in statementClassNames) {
                statementsProp.GetterBlock.ExprDotMethod(
                    Ref("statements"),
                    "Add",
                    NewInstanceInner(statementClassName));
            }

            statementsProp.GetterBlock.BlockReturn(Ref("statements"));

            // build stack
            CodegenStackGenerator.RecursiveBuildStack(
                moduleNameProp,
                "ModuleName",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                modulePropertiesProp,
                "ModuleProperties",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                moduleDependenciesProp,
                "ModuleDependencies",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeEventTypesMethod,
                "InitializeEventTypes",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeNamedWindowsMethod,
                "InitializeNamedWindows",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeTablesMethod,
                "InitializeTables",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeIndexesMethod,
                "InitializeIndexes",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeContextsMethod,
                "InitializeContexts",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeVariablesMethod,
                "InitializeVariables",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeExprDeclaredMethod,
                "InitializeExprDeclareds",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeScriptsMethod,
                "InitializeScripts",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeClassProvidedMethod,
                "InitializeClassProvided",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                statementsProp,
                "Statements",
                methods,
                properties);

            var clazz = new CodegenClass(
                CodegenClassType.MODULEPROVIDER,
                typeof(ModuleProvider),
                moduleClassName,
                classScope,
                new EmptyList<CodegenTypedParam>(),
                null,
                methods,
                properties,
                new EmptyList<CodegenInnerClass>());
            var compiler = new RoslynCompiler()
                .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
                .WithCodegenClasses(new[] {clazz});

            assembly = compiler.Compile();

            return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
                compileTimeServices.Namespace,
                typeof(ModuleProvider),
                moduleIdentPostfix);
        }
        
        private static void MakeModuleProperties(
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
                        MakeModulePropValue(entry.Value)));
                return;
            }

            property.GetterBlock.DeclareVar<IDictionary<ModuleProperty, object>>(
                "props",
                NewInstance(typeof(Dictionary<ModuleProperty, object>)));
            foreach (var entry in props) {
                property.GetterBlock.ExprDotMethod(
                    Ref("props"),
                    "Put",
                    MakeModulePropKey(entry.Key),
                    MakeModulePropValue(entry.Value));
            }

            property.GetterBlock.BlockReturn(Ref("props"));
        }

        private static CodegenExpression MakeModulePropKey(ModuleProperty key)
        {
            return EnumValue(typeof(ModuleProperty), key.GetName());
        }

        private static CodegenExpression MakeModulePropValue(object value)
        {
            return SerializerUtil.ExpressionForUserObject(value);
        }

        private static CodegenMethod RegisterClassProvidedCodegen(
            KeyValuePair<string, ClassProvided> classProvided,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            ModuleClassProvidedInitializeSymbol symbols)
        {
            CodegenMethod method = parent
                .MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
            method.Block.Expression(
                ExprDotMethodChain(symbols.GetAddInitSvc(method))
                    .Get(EPModuleClassProvidedInitServicesConstants.GETCLASSPROVIDEDCOLLECTOR)
                    .Add("RegisterClass", Constant(classProvided.Key), classProvided.Value.Make(method, classScope)));
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
            KeyValuePair<string, ExpressionDeclItem> expression,
            CodegenMethod parent,
            CodegenClassScope classScope,
            ModuleExpressionDeclaredInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);

            var item = expression.Value;
            var bytes = SerializerUtil.ObjectToByteArr(item.OptionalSoda);
            item.OptionalSodaBytes = () => bytes;

            method.Block
                .DeclareVar<ExpressionDeclItem>("detail", expression.Value.Make(method, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPModuleExprDeclaredInitServicesConstants.GETEXPRDECLAREDCOLLECTOR)
                        .Add("RegisterExprDeclared", Constant(expression.Key), Ref("detail")));
            return method;
        }

        internal static CodegenMethod MakeInitEventTypes(
            CodegenClassScope classScope,
            ModuleCompileTimeServices compileTimeServices)
        {
            var symbolsEventTypeInit = new ModuleEventTypeInitializeSymbol();
            var initializeEventTypesMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(EPCompilerImpl), symbolsEventTypeInit, classScope)
                .AddParam(
                    typeof(EPModuleEventTypeInitServices),
                    ModuleEventTypeInitializeSymbol.REF_INITSVC.Ref);
            foreach (var eventType in compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded) {
                var addType = RegisterEventTypeCodegen(
                    eventType,
                    initializeEventTypesMethod,
                    classScope,
                    symbolsEventTypeInit);
                initializeEventTypesMethod.Block.Expression(LocalMethod(addType));
            }

            return initializeEventTypesMethod;
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

            if (eventType is JsonEventType) {
                JsonEventType jsonEventType = (JsonEventType) eventType;
                method.Block.DeclareVar<LinkedHashMap<string, object>>(
                    "props",
                    LocalMethod(
                        MakePropsCodegen(
                            jsonEventType.Types,
                            method,
                            symbols,
                            classScope,
                            () => jsonEventType.DeepSuperTypes.GetEnumerator())));
                string[] superTypeNames = GetSupertypeNames(jsonEventType);
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
                method.Block.DeclareVar<LinkedHashMap<string, object>>(
                    "props",
                    LocalMethod(
                        MakePropsCodegen(
                            baseNestable.Types,
                            method,
                            symbols,
                            classScope,
                            () => baseNestable.DeepSuperTypes.GetEnumerator())));
                var registerMethodName = baseNestable is MapEventType ? "RegisterMap" : "RegisterObjectArray";
                var superTypeNames = GetSupertypeNames(baseNestable);

                method.Block
                    .Expression(
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
                    EventTypeUtility.ResolveTypeCodegen(wrapper.UnderlyingEventType, symbols.GetAddInitSvc(method)));
                method.Block.DeclareVar<LinkedHashMap<string, object>>(
                    "props",
                    LocalMethod(
                        MakePropsCodegen(
                            wrapper.UnderlyingMapType.Types,
                            method,
                            symbols,
                            classScope,
                            () => wrapper.UnderlyingMapType.DeepSuperTypes.GetEnumerator())));
                method.Block
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method))
                            .Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                            .Add("RegisterWrapper", Ref("metadata"), Ref("inner"), Ref("props")));
            }
            else if (eventType is BeanEventType beanType) {
                var superTypes = MakeSupertypes(beanType.SuperTypes, symbols.GetAddInitSvc(method));
                var deepSuperTypes = MakeDeepSupertypes(beanType.DeepSuperTypesCollection, method, symbols, classScope);
                method.Block
                    .Expression(
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
            else if (eventType is SchemaXMLEventType xmlType && (xmlType.RepresentsFragmentOfProperty != null)) {
                method.Block
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method))
                            .Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                            .Add(
                                "RegisterXML",
                                Ref("metadata"),
                                Constant(xmlType.RepresentsFragmentOfProperty),
                                Constant(xmlType.RepresentsOriginalTypeName)));
            }
            else if (eventType is BaseXMLEventType baseXmlType) {
                method.Block
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method))
                            .Get(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                            .Add(
                                "RegisterXMLNewType",
                                Ref("metadata"),
                                baseXmlType.ConfigurationEventTypeXMLDOM.ToExpression(method, classScope)));
            }
            else if (eventType is AvroSchemaEventType avroType) {
                var avroTypeSchema = avroType.SchemaAsJson;
                var superTypeNames = GetSupertypeNames(avroType);
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
                method.Block
                    .Expression(
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

        private static string[] GetSupertypeNames(EventType eventType) {
            var superTypes = eventType.SuperTypes;
            var superTypesLength = superTypes?.Count ?? 0;
            
            if (superTypes != null && superTypesLength > 0) {
                string[] superTypeNames = new string[superTypesLength];
                for (int i = 0; i < superTypesLength; i++) {
                    superTypeNames[i] = superTypes[i].Name;
                }
                return superTypeNames;
            }
            return new string[0];
        }

        private static CodegenMethod RegisterEventTypeSerdeCodegen(
            EventType eventType,
            DataInputOutputSerdeForge serdeForge,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            ModuleEventTypeInitializeSymbol symbols)
        {
            CodegenMethod method = parent.MakeChild(typeof(void), typeof(EPCompilerImpl), classScope);
            method.Block
                .DeclareVar<EventTypeMetadata>("metadata", eventType.Metadata.ToExpression())
                .DeclareVar<EventTypeResolver>(
                    "resolver",
                    ExprDotMethod(symbols.GetAddInitSvc(method), EPModuleEventTypeInitServicesConstants.GETEVENTTYPERESOLVER))
                .DeclareVar<DataInputOutputSerde>("serde", serdeForge.Codegen(method, classScope, Ref("resolver")));
            method.Block
                .Expression(
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
                    EventTypeUtility.ResolveTypeCodegen(deepSuperTypes.First(), symbols.GetAddInitSvc(parent)));
            }

            var method = parent.MakeChild(
                typeof(ISet<EventType>),
                typeof(CompilerHelperModuleProvider),
                classScope);
            method.Block.DeclareVar<ISet<EventType>>(
                "dst",
                NewInstance(typeof(LinkedHashSet<EventType>)));
            foreach (var eventType in deepSuperTypes) {
                var dstRef = Ref("dst");
                var initServicesRef = symbols.GetAddInitSvc(method);
                var valueToAdd = EventTypeUtility.ResolveTypeCodegen(eventType, initServicesRef);
                method.Block.ExprDotMethod(dstRef, "Add", valueToAdd);
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
            Supplier<IEnumerator<EventType>> deepSuperTypes)
        {
            var method = parent.MakeChild(
                typeof(LinkedHashMap<string, object>),
                typeof(CompilerHelperModuleProvider),
                classScope);
            symbols.GetAddInitSvc(method);

            method.Block.DeclareVar<LinkedHashMap<string, object>>(
                "props",
                NewInstance(typeof(LinkedHashMap<string, object>)));
            foreach (var entry in types) {
                var propertyOfSupertype = IsPropertyOfSupertype(deepSuperTypes, entry.Key);
                if (propertyOfSupertype) {
                    continue;
                }

                var type = entry.Value;
                CodegenExpression typeResolver;
                if (type is Type asType) {
                    if (asType == typeof(FlexCollection)) {
                        asType = typeof(ICollection<object>);
                    }
                    
                    typeResolver = Typeof(asType);
                }
                else if (type is EventType asEventType) {
                    typeResolver = EventTypeUtility.ResolveTypeCodegen(
                        asEventType,
                        ModuleEventTypeInitializeSymbol.REF_INITSVC);
                }
                else if (type is EventType[] asEventTypeArray) {
                    var typeExpr = EventTypeUtility.ResolveTypeCodegen(
                        asEventTypeArray[0],
                        ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewArrayWithInit(typeof(EventType), typeExpr);
                }
                else if (type == null) {
                    typeResolver = ConstantNull();
                }
                else if (type is TypeBeanOrUnderlying) {
                    var innerType = ((TypeBeanOrUnderlying) type).EventType;
                    var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(
                        innerType,
                        ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr);
                }
                else if (type is TypeBeanOrUnderlying[]) {
                    var innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
                    var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(
                        innerType,
                        ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewArrayWithInit(
                        typeof(TypeBeanOrUnderlying),
                        NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr));
                }
                else if (type is IDictionary<string, object>) {
                    typeResolver = LocalMethod(
                        MakePropsCodegen((IDictionary<string, object>) type, parent, symbols, classScope, null));
                }
                else {
                    throw new IllegalStateException("Unrecognized type '" + type + "'");
                }

                method.Block.ExprDotMethod(Ref("props"), "Put", Constant(entry.Key), typeResolver);
            }

            method.Block.MethodReturn(Ref("props"));
            return method;
        }

        private static bool IsPropertyOfSupertype(
            Supplier<IEnumerator<EventType>> deepSuperTypes,
            string key)
        {
            if (deepSuperTypes == null) {
                return false;
            }

            var deepSuperTypesIterator = deepSuperTypes.Invoke();
            while (deepSuperTypesIterator.MoveNext()) {
                var type = deepSuperTypesIterator.Current;
                if (type.IsProperty(key)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace