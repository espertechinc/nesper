///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorOutputConditionType;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgableRSPFactoryProvider : StmtClassForgable
    {
        public const string CLASSNAME_RESULTSETPROCESSORFACTORY = "RSPFactory";
        public const string CLASSNAME_RESULTSETPROCESSOR = "RSP";
        public const string MEMBERNAME_RESULTSETPROCESSORFACTORY = "rspFactory";
        public const string MEMBERNAME_AGGREGATIONSVCFACTORY = "aggFactory";
        public const string MEMBERNAME_ORDERBYFACTORY = "orderByFactory";
        public const string MEMBERNAME_RESULTEVENTTYPE = "resultEventType";
        public const string MEMBERNAME_STATEMENT_FIELDS = "statementFields";

        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly ResultSetProcessorDesc _spec;
        private readonly StatementRawInfo _statementRawInfo;

        public StmtClassForgableRSPFactoryProvider(
            string className,
            ResultSetProcessorDesc spec,
            CodegenNamespaceScope namespaceScope,
            StatementRawInfo statementRawInfo)
        {
            ClassName = className;
            _spec = spec;
            _namespaceScope = namespaceScope;
            _statementRawInfo = statementRawInfo;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            Supplier<string> debugInformationProvider = () => {
                var writer = new StringWriter();
                _statementRawInfo.AppendCodeDebugInfo(writer);
                writer.Write(" result-set-processor ");
                writer.Write(_spec.ResultSetProcessorFactoryForge.GetType().FullName);
                return writer.ToString();
            };

            try {
                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                // build ctor
                IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
                ctorParms.Add(
                    new CodegenTypedParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref,
                        false));
                ctorParms.Add(
                    new CodegenTypedParam(
                        _namespaceScope.FieldsClassName,
                        null,
                        MEMBERNAME_STATEMENT_FIELDS,
                        true,
                        false));

                var providerCtor = new CodegenCtor(
                    typeof(StmtClassForgableRSPFactoryProvider),
                    ClassName,
                    includeDebugSymbols,
                    ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, ClassName);
                var providerExplicitMembers = new List<CodegenTypedParam>(2);

                // add event type
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(EventType), MEMBERNAME_RESULTEVENTTYPE));

                providerCtor.Block.AssignRef(
                    MEMBERNAME_RESULTEVENTTYPE,
                    EventTypeUtility.ResolveTypeCodegen(_spec.ResultEventType, EPStatementInitServicesConstants.REF));

                MakeResultSetProcessorFactory(
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    ClassName);

                MakeResultSetProcessor(
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    ClassName,
                    _spec);

                OrderByProcessorCompiler.MakeOrderByProcessors(
                    _spec.OrderByProcessorFactoryForge,
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    ClassName,
                    MEMBERNAME_ORDERBYFACTORY);

                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(AggregationServiceFactory), MEMBERNAME_AGGREGATIONSVCFACTORY));
                var aggregationClassNames = new AggregationClassNames();
                var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                    _spec.IsJoin,
                    _spec.AggregationServiceForgeDesc.AggregationServiceFactoryForge,
                    providerCtor,
                    classScope,
                    ClassName,
                    aggregationClassNames);
                providerCtor.Block.AssignRef(
                    MEMBERNAME_AGGREGATIONSVCFACTORY,
                    LocalMethod(aggResult.InitMethod, EPStatementInitServicesConstants.REF));
                innerClasses.AddAll(aggResult.InnerClasses);

                MakeSelectExprProcessors(
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    ClassName,
                    _spec.IsRollup,
                    _spec.SelectExprProcessorForges);

                // make provider methods
                var propResultSetProcessorFactoryMethod = CodegenProperty.MakePropertyNode(
                    typeof(ResultSetProcessorFactory),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propResultSetProcessorFactoryMethod
                    .GetterBlock
                    .BlockReturn(Ref(MEMBERNAME_RESULTSETPROCESSORFACTORY));

                var propAggregationServiceFactoryMethod = CodegenProperty.MakePropertyNode(
                    typeof(AggregationServiceFactory),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propAggregationServiceFactoryMethod
                    .GetterBlock
                    .BlockReturn(Ref(MEMBERNAME_AGGREGATIONSVCFACTORY));

                var propOrderByProcessorFactoryMethod = CodegenProperty.MakePropertyNode(
                    typeof(OrderByProcessorFactory),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propOrderByProcessorFactoryMethod
                    .GetterBlock
                    .BlockReturn(Ref(MEMBERNAME_ORDERBYFACTORY));

                var propResultSetProcessorTypeMethod = CodegenProperty.MakePropertyNode(
                    typeof(ResultSetProcessorType),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propResultSetProcessorTypeMethod
                    .GetterBlock
                    .BlockReturn(EnumValue(typeof(ResultSetProcessorType), _spec.ResultSetProcessorType.GetName()));

                var propResultEventTypeMethod = CodegenProperty.MakePropertyNode(
                    typeof(EventType),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propResultEventTypeMethod
                    .GetterBlock
                    .BlockReturn(Ref(MEMBERNAME_RESULTEVENTTYPE));

                var properties = new CodegenClassProperties();

                var methods = new CodegenClassMethods();
                CodegenStackGenerator.RecursiveBuildStack(providerCtor, "ctor", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propResultSetProcessorFactoryMethod,
                    "ResultSetProcessorFactory",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propAggregationServiceFactoryMethod,
                    "AggregationServiceFactory",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propOrderByProcessorFactoryMethod,
                    "OrderByProcessorFactory",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propResultSetProcessorTypeMethod,
                    "ResultSetProcessorType",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propResultEventTypeMethod, 
                    "ResultEventType",
                    methods,
                    properties);

                // render and compile
                return new CodegenClass(
                    typeof(ResultSetProcessorFactoryProvider),
                    _namespaceScope.Namespace,
                    ClassName,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (Exception t) {
                throw new EPException(
                    "Fatal exception during code-generation for " +
                    debugInformationProvider.Invoke() +
                    " : " +
                    t.Message,
                    t);
            }
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.RSPPROVIDER;

        private static void MakeResultSetProcessorFactory(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> providerExplicitMembers,
            CodegenCtor providerCtor,
            string providerClassName)
        {
            var instantiateMethod = CodegenMethod.MakeMethod(
                    typeof(ResultSetProcessor),
                    typeof(StmtClassForgableRSPFactoryProvider),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(OrderByProcessor), NAME_ORDERBYPROCESSOR)
                .AddParam(typeof(AggregationService), NAME_AGGREGATIONSVC)
                .AddParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            instantiateMethod.Block.MethodReturn(
                NewInstance(
                    CLASSNAME_RESULTSETPROCESSOR,
                    Ref("o"),
                    REF_ORDERBYPROCESSOR,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT));

            var properties = new CodegenClassProperties();
            var methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(instantiateMethod, "Instantiate", methods, properties);

            var ctorParams = Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(
                typeof(StmtClassForgableRSPFactoryProvider), 
                classScope,
                ctorParams);

            var innerClass = new CodegenInnerClass(
                CLASSNAME_RESULTSETPROCESSORFACTORY,
                typeof(ResultSetProcessorFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods,
                properties);
            innerClasses.Add(innerClass);

            providerExplicitMembers.Add(new CodegenTypedParam(typeof(ResultSetProcessorFactory), "rspFactory"));
            providerCtor.Block.AssignRef(
                MEMBERNAME_RESULTSETPROCESSORFACTORY,
                NewInstance(CLASSNAME_RESULTSETPROCESSORFACTORY, Ref("this")));
        }

        private static void MakeResultSetProcessor(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> factoryExplicitMembers,
            CodegenCtor factoryCtor,
            string classNameParent,
            ResultSetProcessorDesc spec)
        {
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(classNameParent, "o"));
            ctorParams.Add(new CodegenTypedParam(typeof(OrderByProcessor), "orderByProcessor"));
            ctorParams.Add(new CodegenTypedParam(typeof(AggregationService), "aggregationService"));
            ctorParams.Add(new CodegenTypedParam(typeof(AgentInstanceContext), "agentInstanceContext"));

            // make ctor code
            var serviceCtor = new CodegenCtor(
                typeof(StmtClassForgableRSPFactoryProvider), 
                classScope,
                ctorParams);

            // Get-Result-Type Method
            var forge = spec.ResultSetProcessorFactoryForge;
            var resultEventTypeGetter = CodegenProperty.MakePropertyNode(
                typeof(EventType),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            resultEventTypeGetter.GetterBlock.BlockReturn(Ref("o." + MEMBERNAME_RESULTEVENTTYPE));

            // Instance members and methods
            var instance = new CodegenInstanceAux(serviceCtor);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            instance.Members.Add(
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassName,
                    null,
                    "statementFields",
                    false,
                    false));

            serviceCtor.Block.AssignRef(
                Ref("this.statementFields"),
                Ref("o.statementFields"));

            // --------------------------------------------------------------------------------

            forge.InstanceCodegen(instance, classScope, factoryCtor, factoryExplicitMembers);

            // Process-View-Result Method
            var processViewResultMethod = CodegenMethod.MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(EventBean[]), NAME_NEWDATA)
                .AddParam(typeof(EventBean[]), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin) {
                GenerateInstrumentedProcessView(forge, classScope, processViewResultMethod, instance);
            }
            else {
                processViewResultMethod.Block.MethodThrowUnsupported();
            }

            // Process-Join-Result Method
            var processJoinResultMethod = CodegenMethod
                .MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_NEWDATA)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin) {
                processJoinResultMethod.Block.MethodThrowUnsupported();
            }
            else {
                GenerateInstrumentedProcessJoin(forge, classScope, processJoinResultMethod, instance);
            }

            // Clear-Method
            var clearMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            forge.ClearMethodCodegen(classScope, clearMethod);

            // Get-Iterator-View
            var getEnumeratorMethodView = CodegenMethod
                .MakeMethod(
                    typeof(IEnumerator<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(Viewable), NAME_VIEWABLE);
            if (!spec.IsJoin) {
                forge.GetIteratorViewCodegen(classScope, getEnumeratorMethodView, instance);
            }
            else {
                getEnumeratorMethodView.Block.MethodThrowUnsupported();
            }

            // Get-Iterator-Join
            var getEnumeratorMethodJoin = CodegenMethod
                .MakeMethod(
                    typeof(IEnumerator<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_JOINSET);
            if (!spec.IsJoin) {
                getEnumeratorMethodJoin.Block.MethodThrowUnsupported();
            }
            else {
                forge.GetIteratorJoinCodegen(classScope, getEnumeratorMethodJoin, instance);
            }

            // Process-output-rate-buffered-view
            var processOutputLimitedViewMethod = CodegenMethod
                .MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(IList<UniformPair<EventBean[]>>), NAME_VIEWEVENTSLIST)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin && spec.HasOutputLimit && !spec.HasOutputLimitSnapshot) {
                forge.ProcessOutputLimitedViewCodegen(classScope, processOutputLimitedViewMethod, instance);
            }
            else {
                processOutputLimitedViewMethod.Block.MethodThrowUnsupported();
            }

            // Process-output-rate-buffered-join
            var processOutputLimitedJoinMethod = CodegenMethod
                .MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(IList<UniformPair<ISet<MultiKey<EventBean>>>>), NAME_JOINEVENTSSET)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin || !spec.HasOutputLimit || spec.HasOutputLimitSnapshot) {
                processOutputLimitedJoinMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessOutputLimitedJoinCodegen(classScope, processOutputLimitedJoinMethod, instance);
            }

            // Set-Agent-Instance is supported for fire-and-forget queries only
            var setAgentInstanceContextMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AgentInstanceContext), "value");
            setAgentInstanceContextMethod.Block.AssignRef(NAME_AGENTINSTANCECONTEXT, Ref("value"));

            // Apply-view
            var applyViewResultMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_NEWDATA)
                .AddParam(typeof(EventBean[]), NAME_OLDDATA);
            if (!spec.IsJoin && spec.HasOutputLimit && spec.HasOutputLimitSnapshot) {
                forge.ApplyViewResultCodegen(classScope, applyViewResultMethod, instance);
            }
            else {
                applyViewResultMethod.Block.MethodThrowUnsupported();
            }

            // Apply-join
            var applyJoinResultMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_NEWDATA)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_OLDDATA);
            if (!spec.IsJoin || !spec.HasOutputLimit || !spec.HasOutputLimitSnapshot) {
                applyJoinResultMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ApplyJoinResultCodegen(classScope, applyJoinResultMethod, instance);
            }

            // Process-output-unbuffered-view
            var processOutputLimitedLastAllNonBufferedViewMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_NEWDATA)
                .AddParam(typeof(EventBean[]), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin && spec.HasOutputLimit && spec.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                forge.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
                    classScope,
                    processOutputLimitedLastAllNonBufferedViewMethod,
                    instance);
            }
            else {
                processOutputLimitedLastAllNonBufferedViewMethod.Block.MethodThrowUnsupported();
            }

            // Process-output-unbuffered-join
            var processOutputLimitedLastAllNonBufferedJoinMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_NEWDATA)
                .AddParam(typeof(ISet<MultiKey<EventBean>>), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin || !spec.HasOutputLimit || spec.OutputConditionType != POLICY_LASTALL_UNORDERED) {
                processOutputLimitedLastAllNonBufferedJoinMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                    classScope,
                    processOutputLimitedLastAllNonBufferedJoinMethod,
                    instance);
            }

            // Continue-output-unbuffered-view
            var continueOutputLimitedLastAllNonBufferedViewMethod = CodegenMethod
                .MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin && spec.HasOutputLimit && spec.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                forge.ContinueOutputLimitedLastAllNonBufferedViewCodegen(
                    classScope,
                    continueOutputLimitedLastAllNonBufferedViewMethod,
                    instance);
            }
            else {
                continueOutputLimitedLastAllNonBufferedViewMethod.Block.MethodThrowUnsupported();
            }

            // Continue-output-unbuffered-join
            var continueOutputLimitedLastAllNonBufferedJoinMethod = CodegenMethod
                .MakeMethod(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            if (!spec.IsJoin || !spec.HasOutputLimit || spec.OutputConditionType != POLICY_LASTALL_UNORDERED) {
                continueOutputLimitedLastAllNonBufferedJoinMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
                    classScope,
                    continueOutputLimitedLastAllNonBufferedJoinMethod,
                    instance);
            }

            // Accept-Helper-Visitor
            var acceptHelperVisitorMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ResultSetProcessorOutputHelperVisitor), NAME_RESULTSETVISITOR);
            forge.AcceptHelperVisitorCodegen(classScope, acceptHelperVisitorMethod, instance);

            // Stop-Method (generates last as other methods may allocate members)
            var stopMethod = CodegenMethod.MakeMethod(
                typeof(void),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.StopMethodCodegen(classScope, stopMethod, instance);

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(
                resultEventTypeGetter,
                "ResultEventType",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processViewResultMethod,
                "ProcessViewResult",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processJoinResultMethod,
                "ProcessJoinResult",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getEnumeratorMethodView,
                "GetEnumerator",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getEnumeratorMethodJoin,
                "GetEnumerator",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                clearMethod,
                "Clear",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                stopMethod,
                "Stop",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processOutputLimitedJoinMethod,
                "ProcessOutputLimitedJoin",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processOutputLimitedViewMethod,
                "ProcessOutputLimitedView",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                setAgentInstanceContextMethod,
                "SetAgentInstanceContext",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                applyViewResultMethod,
                "ApplyViewResult",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                applyJoinResultMethod,
                "ApplyJoinResult",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processOutputLimitedLastAllNonBufferedViewMethod,
                "ProcessOutputLimitedLastAllNonBufferedView",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processOutputLimitedLastAllNonBufferedJoinMethod,
                "ProcessOutputLimitedLastAllNonBufferedJoin",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                continueOutputLimitedLastAllNonBufferedViewMethod,
                "ContinueOutputLimitedLastAllNonBufferedView",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                continueOutputLimitedLastAllNonBufferedJoinMethod,
                "ContinueOutputLimitedLastAllNonBufferedJoin",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                acceptHelperVisitorMethod,
                "AcceptHelperVisitor",
                innerMethods,
                innerProperties);

            foreach (var propertyEntry in instance.Properties.Properties)
            {
                CodegenStackGenerator.RecursiveBuildStack(
                    propertyEntry.Value,
                    propertyEntry.Key,
                    innerMethods,
                    innerProperties);
            }

            foreach (var methodEntry in instance.Methods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(
                    methodEntry.Value,
                    methodEntry.Key,
                    innerMethods,
                    innerProperties);
            }

            var innerClass = new CodegenInnerClass(
                CLASSNAME_RESULTSETPROCESSOR,
                forge.InterfaceClass,
                serviceCtor,
                instance.Members,
                innerMethods,
                innerProperties);
            innerClasses.Add(innerClass);
        }

        private static void GenerateInstrumentedProcessJoin(
            ResultSetProcessorFactoryForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!classScope.IsInstrumented) {
                forge.ProcessJoinResultCodegen(classScope, method, instance);
                return;
            }

            var instrumented = method.MakeChild(typeof(UniformPair<EventBean[]>), forge.GetType(), classScope)
                .AddParam(typeof(ISet<object>), NAME_NEWDATA)
                .AddParam(typeof(ISet<object>), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            forge.ProcessJoinResultCodegen(classScope, instrumented, instance);

            method.Block
                .Apply(InstrumentationCode.Instblock(classScope, "q" + forge.InstrumentedQName))
                .DeclareVar<UniformPair<EventBean[]>>(
                    "pair",
                    LocalMethod(instrumented, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE))
                .Apply(InstrumentationCode.Instblock(classScope, "a" + forge.InstrumentedQName, Ref("pair")))
                .MethodReturn(Ref("pair"));
        }

        private static void GenerateInstrumentedProcessView(
            ResultSetProcessorFactoryForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!classScope.IsInstrumented) {
                forge.ProcessViewResultCodegen(classScope, method, instance);
                return;
            }

            var instrumented = method.MakeChild(typeof(UniformPair<EventBean[]>), forge.GetType(), classScope)
                .AddParam(typeof(EventBean[]), NAME_NEWDATA)
                .AddParam(typeof(EventBean[]), NAME_OLDDATA)
                .AddParam(typeof(bool), NAME_ISSYNTHESIZE);
            forge.ProcessViewResultCodegen(classScope, instrumented, instance);

            method.Block
                .Apply(InstrumentationCode.Instblock(classScope, "q" + forge.InstrumentedQName))
                .DeclareVar<UniformPair<EventBean[]>>(
                    "pair",
                    LocalMethod(instrumented, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE))
                .Apply(InstrumentationCode.Instblock(classScope, "a" + forge.InstrumentedQName, Ref("pair")))
                .MethodReturn(Ref("pair"));
        }

        private static void MakeSelectExprProcessors(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> explicitMembers,
            CodegenCtor outerClassCtor,
            string classNameParent,
            bool rollup,
            SelectExprProcessorForge[] forges)
        {
            // handle single-select
            if (!rollup) {
                var name = "SelectExprProcessorImpl";
                explicitMembers.Add(new CodegenTypedParam(typeof(SelectExprProcessor), "selectExprProcessor"));
                outerClassCtor.Block.AssignRef(
                    "selectExprProcessor",
                    NewInstance(name, Ref("this"), EPStatementInitServicesConstants.REF));
                var innerClass = MakeSelectExprProcessor(name, classNameParent, classScope, forges[0]);
                innerClasses.Add(innerClass);
                return;
            }

            // handle multi-select
            for (var i = 0; i < forges.Length; i++) {
                var name = "SelectExprProcessorImpl" + i;
                var forge = forges[i];
                var innerClass = MakeSelectExprProcessor(name, classNameParent, classScope, forge);
                innerClasses.Add(innerClass);
            }

            explicitMembers.Add(new CodegenTypedParam(typeof(SelectExprProcessor[]), "selectExprProcessorArray"));
            outerClassCtor.Block.AssignRef(
                "selectExprProcessorArray",
                NewArrayByLength(typeof(SelectExprProcessor), Constant(forges.Length)));
            for (var i = 0; i < forges.Length; i++) {
                outerClassCtor.Block.AssignArrayElement(
                    "selectExprProcessorArray",
                    Constant(i),
                    NewInstance("SelectExprProcessorImpl" + i, Ref("this"), EPStatementInitServicesConstants.REF));
            }
        }

        private static CodegenInnerClass MakeSelectExprProcessor(
            string className,
            string classNameParent,
            CodegenClassScope classScope,
            SelectExprProcessorForge forge)
        {
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var selectEnv = new SelectExprProcessorCodegenSymbol();
            CodegenSymbolProvider symbolProvider = new ProxyCodegenSymbolProvider {
                ProcProvide = symbols => {
                    exprSymbol.Provide(symbols);
                    selectEnv.Provide(symbols);
                }
            };

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>(2);
            members.Add(new CodegenTypedParam(classScope.NamespaceScope.FieldsClassName, NAME_STATEMENT_FIELDS));
            members.Add(new CodegenTypedParam(typeof(EventBeanTypedEventFactory), "factory"));

            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>(2);
            ctorParams.Add(new CodegenTypedParam(classNameParent, "o"));
            ctorParams.Add(
                new CodegenTypedParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref,
                    false));

            var ctor = new CodegenCtor(typeof(StmtClassForgableRSPFactoryProvider), classScope, ctorParams);
            ctor.Block.AssignRef(
                NAME_STATEMENT_FIELDS,
                ExprDotName(Ref("o"), NAME_STATEMENT_FIELDS));
            ctor.Block.AssignRef(
                "factory",
                ExprDotName(EPStatementInitServicesConstants.REF, "EventBeanTypedEventFactory"));

            var processMethod = CodegenMethod.MakeMethod(
                    typeof(EventBean),
                    typeof(StmtClassForgableRSPFactoryProvider),
                    symbolProvider,
                    classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(typeof(bool), SelectExprProcessorCodegenSymbol.NAME_ISSYNTHESIZE)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

            processMethod.Block.Apply(
                InstrumentationCode.Instblock(
                    classScope,
                    "qSelectClause",
                    REF_EPS,
                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    REF_EXPREVALCONTEXT));
            var performMethod = forge.ProcessCodegen(
                Ref("o." + MEMBERNAME_RESULTEVENTTYPE),
                Ref("factory"),
                processMethod,
                selectEnv,
                exprSymbol,
                classScope);
            exprSymbol.DerivedSymbolsCodegen(processMethod, processMethod.Block, classScope);
            processMethod.Block
                .DeclareVar<EventBean>("@out", LocalMethod(performMethod))
                .Apply(
                    InstrumentationCode.Instblock(
                        classScope,
                        "aSelectClause",
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        Ref("@out"),
                        ConstantNull()))
                .MethodReturn(Ref("@out"));

            var allProperties = new CodegenClassProperties();
            var allMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(
                processMethod, "Process", allMethods, allProperties);

            return new CodegenInnerClass(
                className,
                typeof(SelectExprProcessor),
                ctor,
                members,
                allMethods,
                allProperties);
        }
    }
} // end of namespace