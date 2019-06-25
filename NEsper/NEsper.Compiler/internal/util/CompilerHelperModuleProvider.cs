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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
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
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
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
            var packageName = "generated";
            IDictionary<string, byte[]> moduleBytes = new Dictionary<string, byte[]>();
            EPCompiledManifest manifest;
            try
            {
                manifest = CompileToBytes(
                    moduleBytes, compilables, optionalModuleName, moduleProperties, compileTimeServices, compilerOptions, packageName);
            }
            catch (EPCompileException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EPCompileException("Unexpected exception compiling module: " + ex.Message, ex,
                    new EmptyList<EPCompileExceptionItem>());
            }

            return new EPCompiled(moduleBytes, manifest);
        }

        private static EPCompiledManifest CompileToBytes(
            IDictionary<string, byte[]> moduleBytes,
            IList<Compilable> compilables,
            string optionalModuleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            ModuleCompileTimeServices compileTimeServices,
            CompilerOptions compilerOptions,
            string packageName)
        {
            var moduleAssignedName = optionalModuleName == null ? Guid.NewGuid().ToString() : optionalModuleName;
            var moduleIdentPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(moduleAssignedName);

            // compile each statement
            var statementNumber = 0;
            IList<string> statementClassNames = new List<string>();

            ISet<string> statementNames = new HashSet<string>();
            foreach (var compilable in compilables)
            {
                string className;

                try
                {
                    var statementCompileTimeServices =
                        new StatementCompileTimeServices(statementNumber, compileTimeServices);
                    className = CompilerHelperStatementProvider.CompileItem(
                        compilable, optionalModuleName, moduleIdentPostfix, moduleBytes, statementNumber, packageName, statementNames,
                        statementCompileTimeServices, compilerOptions);
                }
                catch (StatementSpecCompileException ex)
                {
                    EPCompileExceptionItem first;
                    if (ex is StatementSpecCompileSyntaxException)
                    {
                        first = new EPCompileExceptionSyntaxItem(ex.Message, ex.Expression, -1);
                    }
                    else
                    {
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
                optionalModuleName, moduleProperties, statementClassNames, moduleIdentPostfix, moduleBytes, packageName, compileTimeServices);

            // create module XML
            return new EPCompiledManifest(COMPILER_VERSION, moduleProviderClassName, null);
        }

        private static string CompileModule(
            string optionalModuleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            IList<string> statementClassNames,
            string moduleIdentPostfix,
            IDictionary<string, byte[]> moduleBytes,
            string packageName,
            ModuleCompileTimeServices compileTimeServices)
        {
            // write code to create an implementation of StatementResource
            var packageScope = new CodegenNamespaceScope(packageName, null, compileTimeServices.IsInstrumented());
            var moduleClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(ModuleProvider), moduleIdentPostfix);
            var classScope = new CodegenClassScope(true, packageScope, moduleClassName);
            var methods = new CodegenClassMethods();

            // provide module name
            var getModuleNameMethod = CodegenMethod.MakeParentNode(
                typeof(string), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getModuleNameMethod.Block.MethodReturn(Constant(optionalModuleName));

            // provide module properties
            var getModulePropertiesMethod = CodegenMethod.MakeParentNode(
                typeof(IDictionary<string, string>), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            MakeModuleProperties(moduleProperties, getModulePropertiesMethod);

            // provide module dependencies
            var getModuleDependenciesMethod = CodegenMethod.MakeParentNode(
                typeof(ModuleDependenciesRuntime), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getModuleDependenciesMethod.Block.MethodReturn(compileTimeServices.ModuleDependencies.Make(getModuleDependenciesMethod, classScope));

            // register types
            var initializeEventTypesMethod = MakeInitEventTypes(classScope, compileTimeServices);

            // register named windows
            var symbolsNamedWindowInit = new ModuleNamedWindowInitializeSymbol();
            var initializeNamedWindowsMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsNamedWindowInit, classScope).AddParam(
                    typeof(EPModuleNamedWindowInitServices), ModuleNamedWindowInitializeSymbol.REF_INITSVC.Ref);
            foreach (var namedWindow in compileTimeServices.NamedWindowCompileTimeRegistry.NamedWindows)
            {
                var addNamedWindow = RegisterNamedWindowCodegen(
                    namedWindow, initializeNamedWindowsMethod, classScope, symbolsNamedWindowInit);
                initializeNamedWindowsMethod.Block.Expression(LocalMethod(addNamedWindow));
            }

            // register tables
            var symbolsTableInit = new ModuleTableInitializeSymbol();
            var initializeTablesMethod = CodegenMethod.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsTableInit, classScope)
                .AddParam(typeof(EPModuleTableInitServices), ModuleTableInitializeSymbol.REF_INITSVC.Ref);
            foreach (var table in compileTimeServices.TableCompileTimeRegistry.Tables)
            {
                var addTable = RegisterTableCodegen(table, initializeTablesMethod, classScope, symbolsTableInit);
                initializeTablesMethod.Block.Expression(LocalMethod(addTable));
            }

            // register indexes
            var symbolsIndexInit = new ModuleIndexesInitializeSymbol();
            var initializeIndexesMethod = CodegenMethod.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsIndexInit, classScope)
                .AddParam(typeof(EPModuleIndexInitServices), EPModuleIndexInitServicesConstants.REF.Ref);
            foreach (KeyValuePair<IndexCompileTimeKey, IndexDetailForge> index in compileTimeServices.IndexCompileTimeRegistry.Indexes)
            {
                var addIndex = RegisterIndexCodegen(index, initializeIndexesMethod, classScope, symbolsIndexInit);
                initializeIndexesMethod.Block.Expression(LocalMethod(addIndex));
            }

            // register contexts
            var symbolsContextInit = new ModuleContextInitializeSymbol();
            var initializeContextsMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsContextInit, classScope).AddParam(
                    typeof(EPModuleContextInitServices), ModuleContextInitializeSymbol.REF_INITSVC.Ref);
            foreach (var context in compileTimeServices.ContextCompileTimeRegistry.Contexts)
            {
                var addContext = RegisterContextCodegen(context, initializeContextsMethod, classScope, symbolsContextInit);
                initializeContextsMethod.Block.Expression(LocalMethod(addContext));
            }

            // register variables
            var symbolsVariablesInit = new ModuleVariableInitializeSymbol();
            var initializeVariablesMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsVariablesInit, classScope).AddParam(
                    typeof(EPModuleVariableInitServices), ModuleVariableInitializeSymbol.REF_INITSVC.Ref);
            foreach (var variable in compileTimeServices.VariableCompileTimeRegistry.Variables)
            {
                var addVariable = RegisterVariableCodegen(variable, initializeVariablesMethod, classScope, symbolsVariablesInit);
                initializeVariablesMethod.Block.Expression(LocalMethod(addVariable));
            }

            // register expressions
            var symbolsExprDeclaredInit = new ModuleExpressionDeclaredInitializeSymbol();
            var initializeExprDeclaredMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsExprDeclaredInit, classScope).AddParam(
                    typeof(EPModuleExprDeclaredInitServices), ModuleExpressionDeclaredInitializeSymbol.REF_INITSVC.Ref);
            foreach (var expression in compileTimeServices.ExprDeclaredCompileTimeRegistry.Expressions)
            {
                var addExpression = RegisterExprDeclaredCodegen(
                    expression, initializeExprDeclaredMethod, classScope, symbolsExprDeclaredInit);
                initializeExprDeclaredMethod.Block.Expression(LocalMethod(addExpression));
            }

            // register scripts
            var symbolsScriptInit = new ModuleScriptInitializeSymbol();
            var initializeScriptsMethod = CodegenMethod.MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsScriptInit, classScope)
                .AddParam(typeof(EPModuleScriptInitServices), ModuleScriptInitializeSymbol.REF_INITSVC.Ref);
            foreach (var expression in compileTimeServices.ScriptCompileTimeRegistry.Scripts)
            {
                var addScript = RegisterScriptCodegen(expression, initializeScriptsMethod, classScope, symbolsScriptInit);
                initializeScriptsMethod.Block.Expression(LocalMethod(addScript));
            }

            // instantiate factories for statements
            var statementsMethod = CodegenMethod.MakeParentNode(
                typeof(IList<object>), typeof(EPCompilerImpl), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            statementsMethod.Block.DeclareVar(typeof(IList<object>), "statements", NewInstance(typeof(List<object>), Constant(statementClassNames.Count)));
            foreach (var statementClassName in statementClassNames)
            {
                statementsMethod.Block.ExprDotMethod(@Ref("statements"), "add", CodegenExpressionBuilder.NewInstance(statementClassName));
            }

            statementsMethod.Block.MethodReturn(@Ref("statements"));

            // build stack
            CodegenStackGenerator.RecursiveBuildStack(getModuleNameMethod, "getModuleName", methods);
            CodegenStackGenerator.RecursiveBuildStack(getModulePropertiesMethod, "getModuleProperties", methods);
            CodegenStackGenerator.RecursiveBuildStack(getModuleDependenciesMethod, "getModuleDependencies", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeEventTypesMethod, "initializeEventTypes", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeNamedWindowsMethod, "initializeNamedWindows", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeTablesMethod, "initializeTables", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeIndexesMethod, "initializeIndexes", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeContextsMethod, "initializeContexts", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeVariablesMethod, "initializeVariables", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeExprDeclaredMethod, "initializeExprDeclareds", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeScriptsMethod, "initializeScripts", methods);
            CodegenStackGenerator.RecursiveBuildStack(statementsMethod, "statements", methods);

            var clazz = new CodegenClass(
                typeof(ModuleProvider), packageName, moduleClassName, classScope,
                new EmptyList<CodegenTypedParam>(), null, methods,
                new EmptyList<CodegenInnerClass>());
            RoslynCompiler.Compile(clazz, moduleBytes, compileTimeServices.Configuration.Compiler.Logging.IsEnableCode);

            return CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
                packageName, typeof(ModuleProvider), moduleIdentPostfix);
        }

        private static void MakeModuleProperties(
            IDictionary<ModuleProperty, object> props,
            CodegenMethod method)
        {
            if (props.IsEmpty())
            {
                method.Block.MethodReturn(StaticMethod(typeof(Collections), "emptyMap"));
                return;
            }

            if (props.Count == 1)
            {
                var entry = props.First();
                method.Block.MethodReturn(
                    StaticMethod(typeof(Collections), "singletonMap", MakeModulePropKey(entry.Key), MakeModulePropValue(entry.Value)));
                return;
            }

            method.Block.DeclareVar(
                typeof(IDictionary<string, string>), "props",
                NewInstance(typeof(Dictionary<string, string>), Constant(CollectionUtil.CapacityHashMap(props.Count))));
            foreach (var entry in props)
            {
                method.Block.ExprDotMethod(@Ref("props"), "put", MakeModulePropKey(entry.Key), MakeModulePropValue(entry.Value));
            }

            method.Block.MethodReturn(@Ref("props"));
        }

        private static CodegenExpression MakeModulePropKey(ModuleProperty key)
        {
            return EnumValue(typeof(ModuleProperty), key.GetName());
        }

        private static CodegenExpression MakeModulePropValue(object value)
        {
            return SerializerUtil.ExpressionForUserObject(value);
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
                    ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPModuleScriptInitServicesConstants.GETSCRIPTCOLLECTOR)
                        .Add(
                            "registerScript", Constant(script.Key.Name), Constant(script.Key.ParamNum),
                            script.Value.Make(method, symbols, classScope)));
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
                .DeclareVar(typeof(ExpressionDeclItem), "detail", expression.Value.Make(method, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPModuleExprDeclaredInitServicesConstants.GETEXPRDECLAREDCOLLECTOR)
                        .Add("registerExprDeclared", Constant(expression.Key), @Ref("detail")));
            return method;
        }

        internal static CodegenMethod MakeInitEventTypes(
            CodegenClassScope classScope,
            ModuleCompileTimeServices compileTimeServices)
        {
            var symbolsEventTypeInit = new ModuleEventTypeInitializeSymbol();
            var initializeEventTypesMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(EPCompilerImpl), symbolsEventTypeInit, classScope).AddParam(
                    typeof(EPModuleEventTypeInitServices), ModuleEventTypeInitializeSymbol.REF_INITSVC.Ref);
            foreach (var eventType in compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded)
            {
                var addType = RegisterEventTypeCodegen(eventType, initializeEventTypesMethod, classScope, symbolsEventTypeInit);
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
                .DeclareVar(typeof(NamedWindowMetaData), "detail", namedWindow.Value.Make(symbols.GetAddInitSvc(method)))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleNamedWindowInitServicesConstants.GETNAMEDWINDOWCOLLECTOR)
                        .Add(
                        "registerNamedWindow",
                        Constant(namedWindow.Key), @Ref("detail")));
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
                .DeclareVar(typeof(TableMetaData), "detail", table.Value.Make(parent, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleTableInitServicesConstants.GETTABLECOLLECTOR)
                        .Add(
                        "registerTable",
                        Constant(table.Key), @Ref("detail")));
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
                .DeclareVar(typeof(IndexCompileTimeKey), "key", index.Key.Make(symbols.GetAddInitSvc(method)))
                .DeclareVar(typeof(IndexDetail), "detail", index.Value.Make(method, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleIndexInitServicesConstants.GETINDEXCOLLECTOR)
                        .Add("registerIndex", @Ref("key"), @Ref("detail")));
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
                .DeclareVar(typeof(ContextMetaData), "detail", context.Value.Make(symbols.GetAddInitSvc(method)))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleContextInitServicesConstants.GETCONTEXTCOLLECTOR)
                        .Add("registerContext", Constant(context.Key), @Ref("detail")));
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
                .DeclareVar(typeof(VariableMetaData), "detail", variable.Value.Make(symbols.GetAddInitSvc(method)))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleVariableInitServicesConstants.GETVARIABLECOLLECTOR)
                        .Add("registerVariable", Constant(variable.Key), @Ref("detail")));
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
            method.Block.DeclareVar(typeof(EventTypeMetadata), "metadata", eventType.Metadata.ToExpression());

            if (eventType is BaseNestableEventType baseNestable)
            {
                method.Block.DeclareVar<LinkedHashMap<string, object>>(
                    "props", LocalMethod(MakePropsCodegen(
                        baseNestable.Types, method, symbols, classScope,
                        () => baseNestable.DeepSuperTypes.GetEnumerator())));
                var registerMethodName = baseNestable is MapEventType ? "registerMap" : "registerObjectArray";
                string[] superTypeNames = null;
                if (baseNestable.SuperTypes != null && baseNestable.SuperTypes.Length > 0)
                {
                    superTypeNames = new string[baseNestable.SuperTypes.Length];
                    for (var i = 0; i < baseNestable.SuperTypes.Length; i++)
                    {
                        superTypeNames[i] = baseNestable.SuperTypes[i].Name;
                    }
                }

                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add(registerMethodName, @Ref("metadata"), @Ref("props"),
                        Constant(superTypeNames), Constant(baseNestable.StartTimestampPropertyName),
                        Constant(baseNestable.EndTimestampPropertyName)));
            }
            else if (eventType is WrapperEventType wrapper)
            {
                method.Block.DeclareVar(
                    typeof(EventType), "inner",
                    EventTypeUtility.ResolveTypeCodegen(wrapper.UnderlyingEventType, symbols.GetAddInitSvc(method)));
                method.Block.DeclareVar(
                    typeof(LinkedHashMap<string, object>), "props",
                    LocalMethod(
                        MakePropsCodegen(
                            wrapper.UnderlyingMapType.Types, method, symbols, classScope,
                            () => wrapper.UnderlyingMapType.DeepSuperTypes.GetEnumerator())));
                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add("registerWrapper", @Ref("metadata"), @Ref("inner"), @Ref("props")));
            }
            else if (eventType is BeanEventType beanType)
            {
                var superTypes = MakeSupertypes(beanType.SuperTypes, symbols.GetAddInitSvc(method));
                var deepSuperTypes = MakeDeepSupertypes(beanType.DeepSuperTypesCollection, method, symbols, classScope);
                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add("registerBean",
                            @Ref("metadata"),
                            Constant(beanType.UnderlyingType),
                            Constant(beanType.StartTimestampPropertyName),
                            Constant(beanType.EndTimestampPropertyName),
                            superTypes, deepSuperTypes));
            }
            else if (eventType is SchemaXMLEventType xmlType)
            {
                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add("registerXML",
                            @Ref("metadata"),
                            Constant(xmlType.RepresentsFragmentOfProperty),
                            Constant(xmlType.RepresentsOriginalTypeName)));
            }
            else if (eventType is AvroSchemaEventType avroType)
            {
                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add("registerAvro",
                            @Ref("metadata"),
                            Constant(avroType.Schema.ToString())));
            }
            else if (eventType is VariantEventType variantEventType)
            {
                method.Block
                    .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPModuleEventTypeInitServicesConstants.GETEVENTTYPECOLLECTOR)
                        .Add("registerVariant",
                            @Ref("metadata"),
                            EventTypeUtility.ResolveTypeArrayCodegen(variantEventType.Variants, symbols.GetAddInitSvc(method)),
                            Constant(variantEventType.IsVariantAny)));
            }
            else
            {
                throw new IllegalStateException("Event type '" + eventType + "' cannot be registered");
            }

            return method;
        }

        private static CodegenExpression MakeDeepSupertypes(
            ICollection<EventType> deepSuperTypes,
            CodegenMethodScope parent,
            ModuleEventTypeInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (deepSuperTypes == null || deepSuperTypes.IsEmpty())
            {
                return StaticMethod(typeof(Collections), "emptySet");
            }

            if (deepSuperTypes.Count == 1)
            {
                return StaticMethod(
                    typeof(Collections), "Singleton",
                    EventTypeUtility.ResolveTypeCodegen(deepSuperTypes.First(), symbols.GetAddInitSvc(parent)));
            }

            var method = parent.MakeChild(typeof(ISet<object>), typeof(CompilerHelperModuleProvider), classScope);
            method.Block.DeclareVar(
                typeof(ISet<object>), "dst",
                NewInstance(typeof(LinkedHashSet<object>), Constant(CollectionUtil.CapacityHashMap(deepSuperTypes.Count))));
            foreach (var eventType in deepSuperTypes)
            {
                method.Block.ExprDotMethod(
                    @Ref("dst"), "Add",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)));
            }

            method.Block.MethodReturn(@Ref("dst"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeSupertypes(
            EventType[] superTypes,
            CodegenExpressionRef initSvcRef)
        {
            if (superTypes == null || superTypes.Length == 0)
            {
                return ConstantNull();
            }

            var expressions = new CodegenExpression[superTypes.Length];
            for (var i = 0; i < superTypes.Length; i++)
            {
                expressions[i] = EventTypeUtility.ResolveTypeCodegen(superTypes[i], initSvcRef);
            }

            return NewArrayWithInit(typeof(EventType), expressions);
        }

        private static CodegenMethod MakePropsCodegen(
            IDictionary<string, object> types,
            CodegenMethodScope parent,
            ModuleEventTypeInitializeSymbol symbols,
            CodegenClassScope classScope,
            Supplier<IEnumerator<EventType>> deepSuperTypes)
        {
            var method = parent.MakeChild(typeof(LinkedHashMap<string, object>), typeof(CompilerHelperModuleProvider), classScope);
            symbols.GetAddInitSvc(method);

            method.Block.DeclareVar(typeof(LinkedHashMap<string, object>), "props", NewInstance(typeof(LinkedHashMap<string, object>)));
            foreach (var entry in types)
            {
                var propertyOfSupertype = IsPropertyOfSupertype(deepSuperTypes, entry.Key);
                if (propertyOfSupertype)
                {
                    continue;
                }

                var type = entry.Value;
                CodegenExpression typeResolver;
                if (type is Type)
                {
                    typeResolver = EnumValue((Type) entry.Value, "class");
                }
                else if (type is EventType)
                {
                    var innerType = (EventType) type;
                    typeResolver = EventTypeUtility.ResolveTypeCodegen(innerType, ModuleEventTypeInitializeSymbol.REF_INITSVC);
                }
                else if (type is EventType[])
                {
                    var innerType = (EventType[]) type;
                    var typeExpr = EventTypeUtility.ResolveTypeCodegen(innerType[0], ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewArrayWithInit(typeof(EventType), typeExpr);
                }
                else if (type == null)
                {
                    typeResolver = ConstantNull();
                }
                else if (type is TypeBeanOrUnderlying)
                {
                    var innerType = ((TypeBeanOrUnderlying) type).EventType;
                    var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(innerType, ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr);
                }
                else if (type is TypeBeanOrUnderlying[])
                {
                    var innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
                    var innerTypeExpr = EventTypeUtility.ResolveTypeCodegen(innerType, ModuleEventTypeInitializeSymbol.REF_INITSVC);
                    typeResolver = NewArrayWithInit(typeof(TypeBeanOrUnderlying), NewInstance(typeof(TypeBeanOrUnderlying), innerTypeExpr));
                }
                else if (type is IDictionary<string, object>)
                {
                    typeResolver = LocalMethod(MakePropsCodegen((IDictionary<string, object>) type, parent, symbols, classScope, null));
                }
                else
                {
                    throw new IllegalStateException("Unrecognized type '" + type + "'");
                }

                method.Block.ExprDotMethod(@Ref("props"), "put", Constant(entry.Key), typeResolver);
            }

            method.Block.MethodReturn(@Ref("props"));
            return method;
        }

        private static bool IsPropertyOfSupertype(
            Supplier<IEnumerator<EventType>> deepSuperTypes,
            string key)
        {
            if (deepSuperTypes == null)
            {
                return false;
            }

            var deepSuperTypesIterator = deepSuperTypes.Invoke();
            while (deepSuperTypesIterator.MoveNext())
            {
                var type = deepSuperTypesIterator.Current;
                if (type.IsProperty(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace