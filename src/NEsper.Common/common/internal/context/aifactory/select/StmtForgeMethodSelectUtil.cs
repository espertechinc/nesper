///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.epl.historical.method.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.prior;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.context.aifactory.select.StatementForgeMethodSelectUtil;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StmtForgeMethodSelectUtil
    {
        public static StmtForgeMethodSelectResult Make(
            IContainer container,
            bool dataflowOperator,
            string packageName,
            string classPostfix,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IList<FilterSpecTracked> filterSpecCompileds = new List<FilterSpecTracked>();
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders = new List<ScheduleHandleTracked>();
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            var statementSpec = @base.StatementSpec;
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(1);
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();

            var streamNames = DetermineStreamNames(statementSpec.StreamSpecs);
            var numStreams = streamNames.Length;

            // first we create streams for subselects, if there are any
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                false,
                filterSpecCompileds,
                namedWindowConsumers,
                @base,
                services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);
            fabricCharge.Add(subSelectActivationDesc.FabricCharge);
            scheduleHandleCallbackProviders.AddAll(subSelectActivationDesc.Schedules);

            // verify for joins that required views are present
            var joinAnalysisResult = VerifyJoinViews(statementSpec);

            var streamEventTypes = new EventType[statementSpec.StreamSpecs.Length];
            var eventTypeNames = new string[numStreams];
            var isNamedWindow = new bool[numStreams];
            var viewableActivatorForges = new ViewableActivatorForge[numStreams];
            IList<ViewFactoryForge>[] viewForges = new IList<ViewFactoryForge>[numStreams];
            var historicalEventViewables = new HistoricalEventViewableForge[numStreams];

            for (var stream = 0; stream < numStreams; stream++) {
                var streamSpec = statementSpec.StreamSpecs[stream];
                var isCanIterateUnbound = streamSpec.ViewSpecs.Length == 0 &&
                                          (services.Configuration.Compiler.ViewResources.IsIterableUnbound ||
                                           AnnotationUtil.HasAnnotation(
                                               statementSpec.Annotations,
                                               typeof(IterableUnboundAttribute)));
                var args = new ViewFactoryForgeArgs(
                    stream,
                    null,
                    streamSpec.Options,
                    null,
                    @base.StatementRawInfo,
                    services);

                if (dataflowOperator) {
                    var dfResult = HandleDataflowActivation(args, streamSpec);
                    streamEventTypes[stream] = dfResult.StreamEventType;
                    eventTypeNames[stream] = dfResult.EventTypeName;
                    viewableActivatorForges[stream] = dfResult.ViewableActivatorForge;
                    viewForges[stream] = dfResult.ViewForges;
                    additionalForgeables.AddAll(dfResult.AdditionalForgeables);
                    scheduleHandleCallbackProviders.AddAll(dfResult.Schedules);
                }
                else if (streamSpec is FilterStreamSpecCompiled) {
                    var filterStreamSpec = (FilterStreamSpecCompiled)statementSpec.StreamSpecs[stream];
                    var filterSpecCompiled = filterStreamSpec.FilterSpecCompiled;
                    streamEventTypes[stream] = filterSpecCompiled.ResultEventType;
                    eventTypeNames[stream] = filterStreamSpec.FilterSpecCompiled.FilterForEventTypeName;

                    viewableActivatorForges[stream] = new ViewableActivatorFilterForge(
                        filterSpecCompiled,
                        isCanIterateUnbound,
                        stream,
                        false,
                        -1);
                    services.StateMgmtSettingsProvider.FilterViewable(
                        fabricCharge,
                        stream,
                        isCanIterateUnbound,
                        @base.StatementRawInfo,
                        filterStreamSpec.FilterSpecCompiled.FilterForEventType);
                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(
                        streamSpec.ViewSpecs,
                        args,
                        streamEventTypes[stream]);
                    viewForges[stream] = viewForgeDesc.Forges;
                    fabricCharge.Add(viewForgeDesc.FabricCharge);
                    additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
                    filterSpecCompileds.Add(
                        new FilterSpecTracked(new CallbackAttributionStream(stream), filterSpecCompiled));
                    scheduleHandleCallbackProviders.AddAll(viewForgeDesc.Schedules);
                }
                else if (streamSpec is PatternStreamSpecCompiled patternStreamSpec) {
                    var forges = patternStreamSpec.Root.CollectFactories();
                    foreach (var forge in forges) {
                        var streamNum = stream;
                        forge.CollectSelfFilterAndSchedule(
                            factoryNodeId => new CallbackAttributionStreamPattern(streamNum, factoryNodeId),
                            filterSpecCompileds,
                            scheduleHandleCallbackProviders);
                    }

                    var patternType = ViewableActivatorPatternForge.MakeRegisterPatternType(
                        @base.ModuleName,
                        stream,
                        null,
                        patternStreamSpec,
                        services);
                    var patternContext = new PatternContext(
                        stream,
                        patternStreamSpec.MatchedEventMapMeta,
                        false,
                        -1,
                        false);
                    viewableActivatorForges[stream] = new ViewableActivatorPatternForge(
                        patternType,
                        patternStreamSpec,
                        patternContext,
                        isCanIterateUnbound);
                    services.StateMgmtSettingsProvider.FilterViewable(
                        fabricCharge,
                        stream,
                        isCanIterateUnbound,
                        @base.StatementRawInfo,
                        patternType);
                    streamEventTypes[stream] = patternType;
                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, patternType);
                    fabricCharge.Add(viewForgeDesc.FabricCharge);
                    viewForges[stream] = viewForgeDesc.Forges;
                    scheduleHandleCallbackProviders.AddAll(viewForgeDesc.Schedules);
                    additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
                    services.StateMgmtSettingsProvider.Pattern(
                        fabricCharge,
                        new PatternAttributionKeyStream(stream),
                        patternStreamSpec,
                        @base.StatementRawInfo);
                }
                else if (streamSpec is NamedWindowConsumerStreamSpec namedSpec) {
                    var namedWindow =
                        services.NamedWindowCompileTimeResolver.Resolve(namedSpec.NamedWindow.EventType.Name);
                    var namedWindowType = namedWindow.EventType;
                    if (namedSpec.OptPropertyEvaluator != null) {
                        namedWindowType = namedSpec.OptPropertyEvaluator.FragmentEventType;
                    }

                    var typesFilterValidation = new StreamTypeServiceImpl(
                        namedWindowType,
                        namedSpec.OptionalStreamName,
                        false);
                    var filterSingle =
                        ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(namedSpec.FilterExpressions);
                    var filterQueryGraph = EPLValidationUtil.ValidateFilterGetQueryGraphSafe(
                        filterSingle,
                        typesFilterValidation,
                        @base.StatementRawInfo,
                        services);

                    namedWindowConsumers.Add(namedSpec);
                    viewableActivatorForges[stream] = new ViewableActivatorNamedWindowForge(
                        namedSpec,
                        namedWindow,
                        filterSingle,
                        filterQueryGraph,
                        true,
                        namedSpec.OptPropertyEvaluator);
                    streamEventTypes[stream] = namedWindowType;
                    viewForges[stream] = EmptyList<ViewFactoryForge>.Instance;
                    joinAnalysisResult.SetNamedWindowsPerStream(stream, namedWindow);
                    eventTypeNames[stream] = namedSpec.NamedWindow.EventType.Name;
                    isNamedWindow[stream] = true;

                    // Consumers to named windows cannot declare a data window view onto the named window to avoid duplicate remove streams
                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, namedWindowType);
                    viewForges[stream] = viewForgeDesc.Forges;
                    additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
                    scheduleHandleCallbackProviders.AddAll(viewForgeDesc.Schedules);
                    EPStatementStartMethodHelperValidate.ValidateNoDataWindowOnNamedWindow(viewForges[stream]);
                }
                else if (streamSpec is TableQueryStreamSpec tableStreamSpec) {
                    ValidateNoViews(tableStreamSpec, "Table data");
                    if (numStreams > 1 && tableStreamSpec.FilterExpressions.Count > 0) {
                        throw new ExprValidationException(
                            "Joins with tables do not allow table filter expressions, please add table filters to the where-clause instead");
                    }

                    var table = tableStreamSpec.Table;
                    EPLValidationUtil.ValidateContextName(
                        true,
                        table.TableName,
                        table.OptionalContextName,
                        statementSpec.Raw.OptionalContextName,
                        false);
                    var filter =
                        ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(tableStreamSpec.FilterExpressions);
                    viewableActivatorForges[stream] = new ViewableActivatorTableForge(table, filter);
                    viewForges[stream] = EmptyList<ViewFactoryForge>.Instance;
                    eventTypeNames[stream] = tableStreamSpec.Table.TableName;
                    streamEventTypes[stream] = tableStreamSpec.Table.InternalEventType;
                    joinAnalysisResult.SetTablesForStream(stream, table);

                    if (tableStreamSpec.Options.IsUnidirectional) {
                        throw new ExprValidationException("Tables cannot be marked as unidirectional");
                    }

                    if (tableStreamSpec.Options.IsRetainIntersection || tableStreamSpec.Options.IsRetainUnion) {
                        throw new ExprValidationException("Tables cannot be marked with retain");
                    }
                }
                else if (streamSpec is DBStatementStreamSpec sqlStreamSpec) {
                    ValidateNoViews(sqlStreamSpec, "Historical data");
                    var typeConversionHook = (SQLColumnTypeConversion)ImportUtil.GetAnnotationHook(
                        statementSpec.Annotations,
                        HookType.SQLCOL,
                        typeof(SQLColumnTypeConversion),
                        services.ImportServiceCompileTime);
                    var outputRowConversionHook = (SQLOutputRowConversion)ImportUtil.GetAnnotationHook(
                        statementSpec.Annotations,
                        HookType.SQLROW,
                        typeof(SQLOutputRowConversion),
                        services.ImportServiceCompileTime);
                    var viewable = HistoricalEventViewableDatabaseForgeFactory.CreateDBStatementView(
                        stream,
                        sqlStreamSpec,
                        typeConversionHook,
                        outputRowConversionHook,
                        @base.StatementRawInfo,
                        services,
                        statementSpec.Annotations);
                    streamEventTypes[stream] = viewable.EventType;
                    viewForges[stream] = EmptyList<ViewFactoryForge>.Instance;;
                    viewableActivatorForges[stream] = new ViewableActivatorHistoricalForge(viewable);
                    historicalEventViewables[stream] = viewable;
                }
                else if (streamSpec is MethodStreamSpec methodStreamSpec) {
                    ValidateNoViews(methodStreamSpec, "Method data");
                    var desc = HistoricalEventViewableMethodForgeFactory.CreateMethodStatementView(
                        stream,
                        methodStreamSpec,
                        @base,
                        services);
                    var viewable = desc.Forge;
                    fabricCharge.Add(desc.FabricCharge);
                    historicalEventViewables[stream] = viewable;
                    streamEventTypes[stream] = viewable.EventType;
                    viewForges[stream] = EmptyList<ViewFactoryForge>.Instance;
                    viewableActivatorForges[stream] = new ViewableActivatorHistoricalForge(viewable);
                    historicalEventViewables[stream] = viewable;
                }
                else {
                    throw new IllegalStateException("Unrecognized stream " + streamSpec);
                }

                // plan serde for iterate-unbound
                if (isCanIterateUnbound) {
                    var serdeForgeables = SerdeEventTypeUtility.Plan(
                        streamEventTypes[stream],
                        @base.StatementRawInfo,
                        services.SerdeEventTypeRegistry,
                        services.SerdeResolver,
                        services.StateMgmtSettingsProvider);
                    additionalForgeables.AddAll(serdeForgeables);
                }
            }

            // handle match-recognize pattern
            if (statementSpec.Raw.MatchRecognizeSpec != null) {
                if (numStreams > 1) {
                    throw new ExprValidationException("Joins are not allowed when using match-recognize");
                }

                if (joinAnalysisResult.TablesPerStream[0] != null) {
                    throw new ExprValidationException("Tables cannot be used with match-recognize");
                }

                var isUnbound = viewForges[0].IsEmpty() &&
                                !(statementSpec.StreamSpecs[0] is NamedWindowConsumerStreamSpec);
                var eventType = viewForges[0].IsEmpty()
                    ? streamEventTypes[0]
                    : viewForges[0][viewForges[0].Count - 1].EventType;
                var plan = RowRecogNFAViewPlanUtil.ValidateAndPlan(eventType, isUnbound, @base, services);
                var forgeX = new RowRecogNFAViewFactoryForge(plan.Forge);
                additionalForgeables.AddAll(plan.AdditionalForgeables);
                scheduleHandleCallbackProviders.Add(
                    new ScheduleHandleTracked(CallbackAttributionMatchRecognize.INSTANCE, forgeX));
                viewForges[0].Add(forgeX);
                var serdeForgeables = SerdeEventTypeUtility.Plan(
                    eventType,
                    @base.StatementRawInfo,
                    services.SerdeEventTypeRegistry,
                    services.SerdeResolver,
                    services.StateMgmtSettingsProvider);
                additionalForgeables.AddAll(serdeForgeables);
                fabricCharge.Add(plan.FabricCharge);
            }

            // Obtain event types from view factory chains
            for (var i = 0; i < viewForges.Length; i++) {
                streamEventTypes[i] = viewForges[i].IsEmpty()
                    ? streamEventTypes[i]
                    : viewForges[i][viewForges[i].Count - 1].EventType;
            }

            // add unique-information to join analysis
            joinAnalysisResult.AddUniquenessInfo(viewForges, statementSpec.Annotations);

            // plan sub-selects
            var subselectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                streamNames,
                streamEventTypes,
                eventTypeNames,
                services);
            var subselectForges = subselectForgePlan.Subselects;
            additionalForgeables.AddAll(subselectForgePlan.AdditionalForgeables);
            fabricCharge.Add(subselectForgePlan.FabricCharge);

            // determine view schedules
            var viewResourceDelegateExpr = new ViewResourceDelegateExpr();

            var hasIStreamOnly = GetHasIStreamOnly(isNamedWindow, viewForges);
            var optionalStreamsIfAny = OuterJoinAnalyzer.OptionalStreamsIfAny(statementSpec.Raw.OuterJoinDescList);
            StreamTypeService typeService = new StreamTypeServiceImpl(
                streamEventTypes,
                streamNames,
                hasIStreamOnly,
                false,
                optionalStreamsIfAny);

            // Validate views that require validation, specifically streams that don't have
            // sub-views such as DB SQL joins
            var historicalViewableDesc = new HistoricalViewableDesc(numStreams);
            for (var stream = 0; stream < historicalEventViewables.Length; stream++) {
                var historicalEventViewable = historicalEventViewables[stream];
                if (historicalEventViewable == null) {
                    continue;
                }

                scheduleHandleCallbackProviders.Add(
                    new ScheduleHandleTracked(new CallbackAttributionStream(stream), historicalEventViewable));
                var forgeables = historicalEventViewable.Validate(
                    typeService,
                    @base.StatementSpec.Raw.SqlParameters,
                    @base.StatementRawInfo,
                    services);
                additionalForgeables.AddAll(forgeables);
                historicalViewableDesc.SetHistorical(stream, historicalEventViewable.RequiredStreams);
                if (historicalEventViewable.RequiredStreams.Contains(stream)) {
                    throw new ExprValidationException(
                        "Parameters for historical stream " +
                        stream +
                        " indicate that the stream is subordinate to itself as stream parameters originate in the same stream");
                }
            }

            // Validate where-clause filter tree, outer join clause and output limit expression
            var whereClauseValidated = EPStatementStartMethodHelperValidate.ValidateNodes(
                statementSpec.Raw,
                typeService,
                viewResourceDelegateExpr,
                @base.StatementRawInfo,
                services);
            var whereClauseForge = whereClauseValidated?.Forge;

            // Obtain result set processor
            var resultSetProcessorDesc = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                ResultSetProcessorAttributionKeyStatement.INSTANCE,
                new ResultSetSpec(statementSpec),
                typeService,
                viewResourceDelegateExpr,
                joinAnalysisResult.UnidirectionalInd,
                true,
                @base.ContextPropertyRegistry,
                false,
                false,
                @base.StatementRawInfo,
                services);
            additionalForgeables.AddAll(resultSetProcessorDesc.AdditionalForgeables);
            fabricCharge.Add(resultSetProcessorDesc.FabricCharge);

            // Handle 'prior' function nodes in terms of view requirements
            var viewVerifyResult = ViewResourceVerifyHelper.VerifyPreviousAndPriorRequirements(
                viewForges,
                viewResourceDelegateExpr,
                null,
                @base.StatementRawInfo,
                services);
            var viewResourceDelegateDesc = viewVerifyResult.Descriptors;
            fabricCharge.Add(viewVerifyResult.FabricCharge);
            var hasPrior = ViewResourceDelegateDesc.HasPrior(viewResourceDelegateDesc);
            if (hasPrior) {
                for (var stream = 0; stream < numStreams; stream++) {
                    var priorRequesteds = viewResourceDelegateDesc[stream].PriorRequests;
                    if (!priorRequesteds.IsEmpty()) {
                        var unbound = viewForges[stream].IsEmpty();
                        var eventTypePrior = streamEventTypes[stream];
                        var setting = services.StateMgmtSettingsProvider.Prior(
                            fabricCharge,
                            @base.StatementRawInfo,
                            stream,
                            null,
                            unbound,
                            eventTypePrior,
                            priorRequesteds);
                        viewForges[stream].Add(new PriorEventViewForge(unbound, eventTypePrior, setting));
                        var serdeForgeables = SerdeEventTypeUtility.Plan(
                            eventTypePrior,
                            @base.StatementRawInfo,
                            services.SerdeEventTypeRegistry,
                            services.SerdeResolver,
                            services.StateMgmtSettingsProvider);
                        additionalForgeables.AddAll(serdeForgeables);
                    }
                }
            }

            var outputProcessDesc = OutputProcessViewForgeFactory.Make(
                typeService.EventTypes,
                resultSetProcessorDesc.ResultEventType,
                resultSetProcessorDesc.ResultSetProcessorType,
                statementSpec,
                @base.StatementRawInfo,
                services);
            var outputProcessViewFactoryForge = outputProcessDesc.Forge;
            additionalForgeables.AddAll(outputProcessDesc.AdditionalForgeables);
            fabricCharge.Add(outputProcessDesc.FabricCharge);
            outputProcessViewFactoryForge.CollectSchedules(scheduleHandleCallbackProviders);

            JoinSetComposerPrototypeForge joinForge = null;
            if (numStreams > 1) {
                var hasAggregations = !resultSetProcessorDesc.AggregationServiceForgeDesc.Expressions.IsEmpty();
                var desc = JoinSetComposerPrototypeForgeFactory.MakeComposerPrototype(
                    statementSpec,
                    joinAnalysisResult,
                    typeService,
                    historicalViewableDesc,
                    false,
                    hasAggregations,
                    @base.StatementRawInfo,
                    services);
                joinForge = desc.Forge;
                additionalForgeables.AddAll(desc.AdditionalForgeables);
                fabricCharge.Add(desc.FabricCharge);
                HandleIndexDependencies(joinForge.OptionalQueryPlan, services);
            }

            // plan table access
            var tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(@base.StatementSpec.TableAccessNodes);
            ValidateTableAccessUse(statementSpec.Raw.IntoTableSpec, statementSpec.Raw.TableExpressions);
            if (joinAnalysisResult.IsUnidirectional && statementSpec.Raw.IntoTableSpec != null) {
                throw new ExprValidationException("Into-table does not allow unidirectional joins");
            }

            var orderByWithoutOutputLimit = statementSpec.Raw.OrderByList != null &&
                                            !statementSpec.Raw.OrderByList.IsEmpty() &&
                                            statementSpec.Raw.OutputLimitSpec == null;

            var statementAIFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var resultSetProcessorProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider),
                classPostfix);
            var outputProcessViewProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(OutputProcessViewFactoryProvider),
                classPostfix);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);

            var forgeX2 = new StatementAgentInstanceFactorySelectForge(
                typeService.StreamNames,
                viewableActivatorForges,
                resultSetProcessorProviderClassName,
                viewForges,
                viewResourceDelegateDesc,
                whereClauseForge,
                joinForge,
                outputProcessViewProviderClassName,
                outputProcessViewFactoryForge.IsDirectAndSimple,
                subselectForges,
                tableAccessForges,
                orderByWithoutOutputLimit,
                joinAnalysisResult.IsUnidirectional);

            CodegenNamespaceScope namespaceScope = new CodegenNamespaceScope(
                packageName,
                statementFieldsClassName,
                services.IsInstrumented,
                services.Configuration.Compiler.ByteCode);
            IList<StmtClassForgeable> forgeablesX = new List<StmtClassForgeable>();
            foreach (var additional in additionalForgeables) {
                forgeablesX.Add(additional.Make(namespaceScope, classPostfix));
            }

            forgeablesX.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    resultSetProcessorProviderClassName,
                    resultSetProcessorDesc,
                    namespaceScope,
                    @base.StatementRawInfo,
                    services.SerdeResolver.IsTargetHA));
            forgeablesX.Add(
                new StmtClassForgeableOPVFactoryProvider(
                    outputProcessViewProviderClassName,
                    outputProcessViewFactoryForge,
                    namespaceScope,
                    numStreams,
                    @base.StatementRawInfo));
            forgeablesX.Add(
                new StmtClassForgeableAIFactoryProviderSelect(
                    statementAIFactoryProviderClassName,
                    namespaceScope,
                    forgeX2));
            forgeablesX.Add(
                new StmtClassForgeableStmtFields(namespaceScope.FieldsClassNameOptional, namespaceScope, true));

            if (!dataflowOperator) {
                var informationals = StatementInformationalsUtil.GetInformationals(
                    @base,
                    filterSpecCompileds,
                    scheduleHandleCallbackProviders,
                    namedWindowConsumers,
                    true,
                    resultSetProcessorDesc.SelectSubscriberDescriptor,
                    namespaceScope,
                    services);
                forgeablesX.Add(
                    new StmtClassForgeableStmtProvider(
                        statementAIFactoryProviderClassName,
                        statementProviderClassName,
                        informationals,
                        namespaceScope));
            }

            var forgeableResult = new StmtForgeMethodResult(
                forgeablesX,
                filterSpecCompileds,
                scheduleHandleCallbackProviders,
                namedWindowConsumers,
                FilterSpecCompiled.MakeExprNodeList(filterSpecCompileds, EmptyList<FilterSpecParamExprNodeForge>.Instance),
                namespaceScope,
                fabricCharge);
            return new StmtForgeMethodSelectResult(forgeableResult, resultSetProcessorDesc.ResultEventType, numStreams);
        }

        private static DataFlowActivationResult HandleDataflowActivation(
            ViewFactoryForgeArgs args,
            StreamSpecCompiled streamSpec)
        {
            if (!(streamSpec is FilterStreamSpecCompiled filterStreamSpec)) {
                throw new ExprValidationException(
                    "Dataflow operator only allows filters for event types and does not allow tables, named windows or patterns");
            }

            var filterSpecCompiled = filterStreamSpec.FilterSpecCompiled;
            var eventType = filterSpecCompiled.ResultEventType;
            var typeName = filterStreamSpec.FilterSpecCompiled.FilterForEventTypeName;
            var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, eventType);
            var views = viewForgeDesc.Forges;
            var viewableActivator = new ViewableActivatorDataFlowForge(eventType);
            return new DataFlowActivationResult(
                eventType,
                typeName,
                viewableActivator,
                views,
                viewForgeDesc.MultikeyForges,
                viewForgeDesc.Schedules);
        }

        private static void ValidateNoViews(
            StreamSpecCompiled streamSpec,
            string conceptName)
        {
            if (streamSpec.ViewSpecs.Length > 0) {
                throw new ExprValidationException(
                    conceptName +
                    " joins do not allow views onto the data, view '" +
                    streamSpec.ViewSpecs[0].ObjectName +
                    "' is not valid in this context");
            }
        }

        private static void ValidateTableAccessUse(
            IntoTableSpec intoTableSpec,
            ISet<ExprTableAccessNode> tableNodes)
        {
            if (intoTableSpec != null && tableNodes != null && tableNodes.Count > 0) {
                foreach (var node in tableNodes) {
                    if (node.TableName.Equals(intoTableSpec.Name)) {
                        throw new ExprValidationException(
                            "Invalid use of table '" +
                            intoTableSpec.Name +
                            "', aggregate-into requires write-only, the expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(node) +
                            "' is not allowed");
                    }
                }
            }
        }

        private static void HandleIndexDependencies(
            QueryPlanForge queryPlan,
            StatementCompileTimeServices services)
        {
            if (queryPlan == null) {
                return;
            }

            var indexes = new HashSet<TableLookupIndexReqKey>();
            for (var streamnum = 0; streamnum < queryPlan.ExecNodeSpecs.Length; streamnum++) {
                var node = queryPlan.ExecNodeSpecs[streamnum];
                indexes.Clear();
                node.AddIndexes(indexes);
                foreach (var index in indexes) {
                    if (index.TableName != null) {
                        var tableMeta = services.TableCompileTimeResolver.Resolve(index.TableName);
                        if (tableMeta.TableVisibility == NameAccessModifier.PUBLIC) {
                            services.ModuleDependenciesCompileTime.AddPathIndex(
                                false,
                                index.TableName,
                                tableMeta.TableModuleName,
                                index.IndexName,
                                index.IndexModuleName,
                                services.NamedWindowCompileTimeRegistry,
                                services.TableCompileTimeRegistry);
                        }
                    }
                }
            }
        }
    }
} // end of namespace