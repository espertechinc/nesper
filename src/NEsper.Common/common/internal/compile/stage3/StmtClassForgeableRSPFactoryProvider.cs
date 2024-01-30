///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.handthru;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.resultset.select.eval;
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
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCompiler;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgeableRSPFactoryProvider : StmtClassForgeable
    {
        public const string CLASSNAME_RESULTSETPROCESSORFACTORY = "RSPFactory";
        public const string CLASSNAME_RESULTSETPROCESSOR = "RSP";
        public const string MEMBERNAME_RESULTSETPROCESSORFACTORY = "rspFactory";
        public const string MEMBERNAME_AGGREGATIONSVCFACTORY = "aggFactory";
        public const string MEMBERNAME_ORDERBYFACTORY = "orderByFactory";
        public const string MEMBERNAME_RESULTEVENTTYPE = "resultEventType";

        public static readonly CodegenExpressionMember MEMBER_EVENTBEANFACTORY = Member("ebfactory");
        public static readonly CodegenExpressionMember MEMBER_STATEMENT_FIELDS = Member("statementFields");

        private readonly string _className;
        private readonly ResultSetProcessorDesc _spec;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly StatementRawInfo _statementRawInfo;
        private readonly bool _isTargetHA;

        public StmtClassForgeableRSPFactoryProvider(
            string className,
            ResultSetProcessorDesc spec,
            CodegenNamespaceScope namespaceScope,
            StatementRawInfo statementRawInfo,
            bool isTargetHA)
        {
            _className = className;
            _spec = spec;
            _namespaceScope = namespaceScope;
            _statementRawInfo = statementRawInfo;
            _isTargetHA = isTargetHA;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            Supplier<string> debugInformationProvider = () => {
                var writer = new StringWriter();
                _statementRawInfo.AppendCodeDebugInfo(writer);
                writer.Write(" result-set-processor ");
                writer.Write(_spec.ResultSetProcessorFactoryForge.GetType().Name);
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
                        _namespaceScope.FieldsClassNameOptional,
                        null,
                        StmtClassForgeableStmtProvider.MEMBERNAME_STATEMENT_FIELDS,
                        true,
                        false));
                var providerCtor = new CodegenCtor(
                    typeof(StmtClassForgeableRSPFactoryProvider),
                    includeDebugSymbols,
                    ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
                var providerExplicitMembers = new List<CodegenTypedParam>(2);

                // add event type
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(EventType), MEMBERNAME_RESULTEVENTTYPE));
                providerCtor.Block.AssignMember(
                    MEMBERNAME_RESULTEVENTTYPE,
                    EventTypeUtility.ResolveTypeCodegen(_spec.ResultEventType, EPStatementInitServicesConstants.REF));

                providerExplicitMembers.Add(new CodegenTypedParam(typeof(ResultSetProcessorFactory), "rspFactory"));

                if (_spec.ResultSetProcessorType != ResultSetProcessorType.HANDTHROUGH) {
                    MakeResultSetProcessorFactory(classScope, innerClasses, providerCtor, _className);

                    MakeResultSetProcessor(
                        classScope,
                        innerClasses,
                        providerExplicitMembers,
                        providerCtor,
                        _className,
                        _spec);
                }

                MakeOrderByProcessors(
                    _spec.OrderByProcessorFactoryForge,
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    _className,
                    MEMBERNAME_ORDERBYFACTORY);

                var aggregationForge =
                    _spec.AggregationServiceForgeDesc.AggregationServiceFactoryForge;
                var aggregationNull = aggregationForge == AggregationServiceNullFactory.INSTANCE;
                if (!aggregationNull) {
                    providerExplicitMembers.Add(
                        new CodegenTypedParam(typeof(AggregationServiceFactory), MEMBERNAME_AGGREGATIONSVCFACTORY));
                    var aggregationClassNames = new AggregationClassNames();
                    var aggResult =
                        AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                            aggregationForge,
                            providerCtor,
                            classScope,
                            _className,
                            aggregationClassNames,
                            _isTargetHA);
                    providerCtor.Block.AssignMember(
                        MEMBERNAME_AGGREGATIONSVCFACTORY,
                        LocalMethod(aggResult.InitMethod, EPStatementInitServicesConstants.REF));
                    innerClasses.AddAll(aggResult.InnerClasses);
                }

                MakeSelectExprProcessors(
                    classScope,
                    innerClasses,
                    providerExplicitMembers,
                    providerCtor,
                    _className,
                    _spec.IsRollup,
                    _spec.SelectExprProcessorForges);

                if (_spec.ResultSetProcessorType == ResultSetProcessorType.HANDTHROUGH) {
                    var handThrough = (ResultSetProcessorHandThroughFactoryForge)_spec.ResultSetProcessorFactoryForge;
                    providerCtor.Block.AssignMember(
                        MEMBERNAME_RESULTSETPROCESSORFACTORY,
                        NewInstance(
                            typeof(ResultSetProcessorHandThroughFactory),
                            Ref("selectExprProcessor"),
                            Ref("resultEventType"),
                            Constant(handThrough.IsSelectRStream)));
                }

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
                    .BlockReturn(
                        aggregationNull
                            ? PublicConstValue(typeof(AggregationServiceNullFactory), "INSTANCE")
                            : Ref(MEMBERNAME_AGGREGATIONSVCFACTORY));

                var propOrderByProcessorFactoryMethod = CodegenProperty.MakePropertyNode(
                    typeof(OrderByProcessorFactory),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propOrderByProcessorFactoryMethod
                    .GetterBlock
                    .BlockReturn(
                        _spec.OrderByProcessorFactoryForge == null ? ConstantNull() : Ref(MEMBERNAME_ORDERBYFACTORY));

                var propResultSetProcessorTypeMethod = CodegenProperty.MakePropertyNode(
                    typeof(ResultSetProcessorType),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propResultSetProcessorTypeMethod
                    .GetterBlock
                    .BlockReturn(EnumValue(typeof(ResultSetProcessorType), EnumHelper.GetName(_spec.ResultSetProcessorType)));

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
                    CodegenClassType.RESULTSETPROCESSORFACTORYPROVIDER,
                    typeof(ResultSetProcessorFactoryProvider),
                    _className,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (Exception t) {
                throw new EPException(
                    "Fatal exception during code-generation for " + debugInformationProvider() + " : " + t.Message,
                    t);
            }
        }

        private static void MakeResultSetProcessorFactory(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            CodegenCtor providerCtor,
            string providerClassName)
        {
            var instantiateMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ResultSetProcessor),
                    typeof(StmtClassForgeableRSPFactoryProvider),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<OrderByProcessor>(NAME_ORDERBYPROCESSOR)
                .AddParam<AggregationService>(NAME_AGGREGATIONSVC)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            instantiateMethod.Block.MethodReturn(
                NewInstanceInner(
                    CLASSNAME_RESULTSETPROCESSOR,
                    Ref("o"),
                    MEMBER_ORDERBYPROCESSOR,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT));

            var properties = new CodegenClassProperties();
            var methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(instantiateMethod, "Instantiate", methods, properties);

            var ctorParams =
                Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);

            var innerClass = new CodegenInnerClass(
                CLASSNAME_RESULTSETPROCESSORFACTORY,
                typeof(ResultSetProcessorFactory),
                ctor,
                EmptyList<CodegenTypedParam>.Instance,
                methods,
                properties);
            innerClasses.Add(innerClass);

            providerCtor.Block.AssignMember(
                MEMBERNAME_RESULTSETPROCESSORFACTORY,
                NewInstanceInner(CLASSNAME_RESULTSETPROCESSORFACTORY, Ref("this")));
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
            ctorParams.Add(
                new CodegenTypedParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT)
                    .WithFinal(false));

            // make ctor code
            var serviceCtor = new CodegenCtor(
                typeof(StmtClassForgeableRSPFactoryProvider),
                classScope,
                ctorParams);

            // Get-Result-Type Method
            var forge = spec.ResultSetProcessorFactoryForge;
            var resultEventTypeGetter = CodegenProperty.MakePropertyNode(
                typeof(EventType),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            resultEventTypeGetter.GetterBlock.BlockReturn(Member("o." + MEMBERNAME_RESULTEVENTTYPE));

            // Instance members and methods
            var instance = new CodegenInstanceAux(serviceCtor);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            instance.Members.Add(
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassNameOptional,
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
            var processViewResultMethod = CodegenMethod
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<EventBean[]>(NAME_NEWDATA)
                .AddParam<EventBean[]>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
            if (!spec.IsJoin) {
                GenerateInstrumentedProcessView(forge, classScope, processViewResultMethod, instance);
            }
            else {
                processViewResultMethod.Block.MethodThrowUnsupported();
            }

            // Process-Join-Result Method
            var processJoinResultMethod = CodegenMethod
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<ISet<MultiKeyArrayOfKeys<EventBean>>>(NAME_NEWDATA)
                .AddParam<ISet<MultiKeyArrayOfKeys<EventBean>>>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
            if (!spec.IsJoin) {
                processJoinResultMethod.Block.MethodThrowUnsupported();
            }
            else {
                GenerateInstrumentedProcessJoin(forge, classScope, processJoinResultMethod, instance);
            }

            // Clear-Method
            var clearMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            forge.ClearMethodCodegen(classScope, clearMethod);

            // Get-Enumerator-View
            var getEnumeratorMethodView = CodegenMethod
                .MakeParentNode(
                    typeof(IEnumerator<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<Viewable>(NAME_VIEWABLE);
            if (!spec.IsJoin) {
                forge.GetEnumeratorViewCodegen(classScope, getEnumeratorMethodView, instance);
            }
            else {
                getEnumeratorMethodView.Block.MethodThrowUnsupported();
            }

            // Get-Enumerator-Join
            var getEnumeratorMethodJoin = CodegenMethod
                .MakeParentNode(
                    typeof(IEnumerator<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<ISet<MultiKeyArrayOfKeys<EventBean>>>(NAME_JOINSET);
            if (!spec.IsJoin) {
                getEnumeratorMethodJoin.Block.MethodThrowUnsupported();
            }
            else {
                forge.GetEnumeratorJoinCodegen(classScope, getEnumeratorMethodJoin, instance);
            }

            // Process-output-rate-buffered-view
            var processOutputLimitedViewMethod = CodegenMethod
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(EPTYPE_LIST_UNIFORMPAIR_EVENTBEANARRAY, NAME_VIEWEVENTSLIST)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
            if (!spec.IsJoin && spec.HasOutputLimit && !spec.HasOutputLimitSnapshot) {
                forge.ProcessOutputLimitedViewCodegen(classScope, processOutputLimitedViewMethod, instance);
            }
            else {
                processOutputLimitedViewMethod.Block.MethodThrowUnsupported();
            }

            // Process-output-rate-buffered-join
            var processOutputLimitedJoinMethod = CodegenMethod
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(EPTYPE_LIST_UNIFORMPAIR_SET_MKARRAY_EVENTBEAN, NAME_JOINEVENTSSET)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
            if (!spec.IsJoin || !spec.HasOutputLimit || spec.HasOutputLimitSnapshot) {
                processOutputLimitedJoinMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessOutputLimitedJoinCodegen(classScope, processOutputLimitedJoinMethod, instance);
            }

            // Set-Agent-Instance is supported for fire-and-forget queries only
            var exprEvalContextProperty = CodegenProperty
                .MakePropertyNode<ExprEvaluatorContext>(forge.GetType(), classScope)
                .WithGetter(block => block.BlockReturn(Ref(NAME_EXPREVALCONTEXT)))
                .WithSetter(block => block.AssignRef(NAME_EXPREVALCONTEXT, Ref("value")));

            // Apply-view
            var applyViewResultMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<EventBean[]>(NAME_NEWDATA)
                .AddParam<EventBean[]>(NAME_OLDDATA);
            if (!spec.IsJoin && spec.HasOutputLimit && spec.HasOutputLimitSnapshot) {
                forge.ApplyViewResultCodegen(classScope, applyViewResultMethod, instance);
            }
            else {
                applyViewResultMethod.Block.MethodThrowUnsupported();
            }

            // Apply-join
            var applyJoinResultMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN, NAME_NEWDATA)
                .AddParam(EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN, NAME_OLDDATA);
            if (!spec.IsJoin || !spec.HasOutputLimit || !spec.HasOutputLimitSnapshot) {
                applyJoinResultMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ApplyJoinResultCodegen(classScope, applyJoinResultMethod, instance);
            }

            // Process-output-unbuffered-view
            var processOutputLimitedLastAllNonBufferedViewMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<EventBean[]>(NAME_NEWDATA)
                .AddParam<EventBean[]>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<ISet<MultiKeyArrayOfKeys<EventBean>>>(NAME_NEWDATA)
                .AddParam<ISet<MultiKeyArrayOfKeys<EventBean>>>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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
                .MakeParentNode(
                    typeof(UniformPair<EventBean[]>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<ResultSetProcessorOutputHelperVisitor>(NAME_RESULTSETVISITOR);
            forge.AcceptHelperVisitorCodegen(classScope, acceptHelperVisitorMethod, instance);

            // Stop-Method (generates last as other methods may allocate members)
            var stopMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
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
            CodegenStackGenerator.RecursiveBuildStack(clearMethod, "Clear", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(stopMethod, "Stop", innerMethods, innerProperties);
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
                exprEvalContextProperty,
                "ExprEvaluatorContext",
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

            foreach (var propertyEntry in instance.Properties.Properties) {
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

            var instrumented = method
                .MakeChild(typeof(UniformPair<EventBean[]>), forge.GetType(), classScope)
                .AddParam<ISet<object>>(NAME_NEWDATA)
                .AddParam<ISet<object>>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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

            var instrumented = method
                .MakeChild(typeof(UniformPair<EventBean[]>), forge.GetType(), classScope)
                .AddParam<EventBean[]>(NAME_NEWDATA)
                .AddParam<EventBean[]>(NAME_OLDDATA)
                .AddParam<bool>(NAME_ISSYNTHESIZE);
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
                var shortcut = false;

                if (forges[0] is ListenerOnlySelectExprProcessorForge forge) {
                    if (forge.SyntheticProcessorForge is SelectEvalWildcardNonJoin) {
                        shortcut = true;
                    }
                }

                if (shortcut) {
                    outerClassCtor.Block.AssignRef(
                        "selectExprProcessor",
                        NewInstance(
                            typeof(SelectEvalWildcardNonJoinImpl),
                            ExprDotName(EPStatementInitServicesConstants.REF, EPStatementInitServicesConstants.STATEMENTRESULTSERVICE)));
                }
                else {
                    outerClassCtor.Block.AssignRef(
                        "selectExprProcessor",
                        NewInstanceInner(name, Ref("this"), EPStatementInitServicesConstants.REF));
                    var innerClass = MakeSelectExprProcessor(
                        name,
                        classNameParent,
                        classScope,
                        forges[0]);
                    innerClasses.Add(innerClass);
                }

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
                    NewInstanceInner(
                        "SelectExprProcessorImpl" + i,
                        Ref("this"),
                        EPStatementInitServicesConstants.REF));
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
            CodegenSymbolProvider symbolProvider = new ProxyCodegenSymbolProvider(
                symbols => {
                    exprSymbol.Provide(symbols);
                    selectEnv.Provide(symbols);
                });

            var members = new List<CodegenTypedParam>(2);
            members.Add(new CodegenTypedParam(classScope.NamespaceScope.FieldsClassNameOptional, NAME_STATEMENT_FIELDS));
            members.Add(new CodegenTypedParam(typeof(EventBeanTypedEventFactory), MEMBER_EVENTBEANFACTORY.Ref));

            var ctorParams = new List<CodegenTypedParam>(2);
            ctorParams.Add(new CodegenTypedParam(classNameParent, "o"));
            ctorParams.Add(
                new CodegenTypedParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref,
                    false));

            var ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);
            ctor.Block.AssignRef(MEMBER_STATEMENT_FIELDS, ExprDotName(Ref("o"), NAME_STATEMENT_FIELDS));
            ctor.Block.AssignRef(MEMBER_EVENTBEANFACTORY, ExprDotName(EPStatementInitServicesConstants.REF, "EventBeanTypedEventFactory"));

            var processMethod = CodegenMethod
                .MakeParentNode(
                    typeof(EventBean),
                    typeof(StmtClassForgeableRSPFactoryProvider),
                    symbolProvider,
                    classScope)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<bool>(SelectExprProcessorCodegenSymbol.NAME_ISSYNTHESIZE)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            processMethod.Block.Apply(
                InstrumentationCode.Instblock(
                    classScope,
                    "qSelectClause",
                    REF_EPS,
                    REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    REF_EXPREVALCONTEXT));
            var performMethod = forge.ProcessCodegen(
                Member("o." + MEMBERNAME_RESULTEVENTTYPE),
                MEMBER_EVENTBEANFACTORY,
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
                        REF_ISNEWDATA,
                        Ref("@out"),
                        ConstantNull()))
                .MethodReturn(Ref("@out"));

            var allProperties = new CodegenClassProperties();
            var allMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(processMethod, "Process", allMethods, allProperties);

            return new CodegenInnerClass(
                className,
                typeof(SelectExprProcessor),
                ctor,
                members,
                allMethods,
                allProperties);
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.RSPPROVIDER;
    }
} // end of namespace