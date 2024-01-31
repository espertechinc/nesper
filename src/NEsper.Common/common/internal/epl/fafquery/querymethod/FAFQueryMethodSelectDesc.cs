///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;


namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectDesc
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly FireAndForgetProcessorForge[] processors;
        private readonly ResultSetProcessorDesc resultSetProcessor;
        private readonly QueryGraphForge queryGraph;
        private readonly ExprNode whereClause;
        private readonly ExprNode[] consumerFilters;
        private readonly JoinSetComposerPrototypeForge joins;
        private readonly Attribute[] annotations;
        private readonly string contextName;
        private readonly string contextModuleName;
        private bool hasTableAccess;
        private readonly bool isDistinct;
        private readonly MultiKeyClassRef distinctMultiKey;
        private IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges;

        public FAFQueryMethodSelectDesc(
            StatementSpecCompiled statementSpec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            annotations = statementSpec.Annotations;
            contextName = statementSpec.Raw.OptionalContextName;
            if (contextName != null) {
                var contextMetaData = services.ContextCompileTimeResolver.GetContextInfo(contextName);
                if (contextMetaData == null) {
                    throw new ExprValidationException("Failed to find context '" + contextName + "'");
                }

                contextModuleName = contextMetaData.ContextModuleName;
            }
            else {
                contextModuleName = null;
            }

            var queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
            if (queryPlanLogging) {
                QUERY_PLAN_LOG.Info("Query plans for Fire-and-forget query '" + compilable.ToEPL() + "'");
            }

            hasTableAccess = statementSpec.TableAccessNodes != null && statementSpec.TableAccessNodes.Count > 0;
            foreach (var streamSpec in statementSpec.StreamSpecs) {
                hasTableAccess |= streamSpec is TableQueryStreamSpec;
            }

            hasTableAccess |= StatementLifecycleSvcUtil.IsSubqueryWithTable(
                statementSpec.SubselectNodes,
                services.TableCompileTimeResolver);
            isDistinct = statementSpec.SelectClauseCompiled.IsDistinct;
            FAFQueryMethodHelper.ValidateFAFQuery(statementSpec);
            var numStreams = statementSpec.StreamSpecs.Length;
            var typesPerStream = new EventType[numStreams];
            var namesPerStream = new string[numStreams];
            var eventTypeNames = new string[numStreams];
            processors = new FireAndForgetProcessorForge[numStreams];
            consumerFilters = new ExprNode[numStreams];
            // check context partition use
            if (statementSpec.Raw.OptionalContextName != null) {
                if (numStreams > 1) {
                    throw new ExprValidationException(
                        "Joins in runtime queries for context partitions are not supported");
                }
            }

            // resolve types and processors
            for (var i = 0; i < numStreams; i++) {
                var streamSpec = statementSpec.StreamSpecs[i];
                processors[i] = FireAndForgetProcessorForgeFactory.ValidateResolveProcessor(
                    streamSpec,
                    statementSpec,
                    statementRawInfo,
                    services);
                if (numStreams > 1 && processors[i].ContextName != null) {
                    throw new ExprValidationException(
                        "Joins against named windows that are under context are not supported");
                }

                var streamName = processors[i].ProcessorName;
                if (streamSpec.OptionalStreamName != null) {
                    streamName = streamSpec.OptionalStreamName;
                }

                namesPerStream[i] = streamName;
                typesPerStream[i] = processors[i].EventTypeRSPInputEvents;
                eventTypeNames[i] = typesPerStream[i].Name;
                IList<ExprNode> consumerFilterExprs;
                if (streamSpec is NamedWindowConsumerStreamSpec namedSpec) {
                    consumerFilterExprs = namedSpec.FilterExpressions;
                }
                else if (streamSpec is TableQueryStreamSpec tableSpec) {
                    consumerFilterExprs = tableSpec.FilterExpressions;
                }
                else {
                    consumerFilterExprs = EmptyList<ExprNode>.Instance;
                    if (i > 0) {
                        throw new ExprValidationException(
                            "Join between SQL query results in fire-and-forget is not supported");
                    }

                    if (contextName != null) {
                        throw new ExprValidationException(
                            "Context specification for SQL queries in fire-and-forget is not supported");
                    }
                }

                consumerFilters[i] = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(consumerFilterExprs);
            }

            // compile filter to optimize access to named window
            var optionalStreamsIfAny = OuterJoinAnalyzer.OptionalStreamsIfAny(statementSpec.Raw.OuterJoinDescList);
            var types = new StreamTypeServiceImpl(
                typesPerStream,
                namesPerStream,
                new bool[numStreams],
                false,
                optionalStreamsIfAny);
            var excludePlanHint = ExcludePlanHint.GetHint(types.StreamNames, statementRawInfo, services);
            queryGraph = new QueryGraphForge(numStreams, excludePlanHint, false);
            if (statementSpec.Raw.WhereClause != null) {
                for (var i = 0; i < numStreams; i++) {
                    try {
                        var validationContext = new ExprValidationContextBuilder(types, statementRawInfo, services)
                            .WithAllowBindingConsumption(true)
                            .WithIsFilterExpression(true)
                            .Build();
                        var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                            ExprNodeOrigin.FILTER,
                            statementSpec.Raw.WhereClause,
                            validationContext);
                        FilterExprAnalyzer.Analyze(validated, queryGraph, false);
                    }
                    catch (Exception ex) {
                        Log.Warn("Unexpected exception analyzing filter paths: " + ex.Message, ex);
                    }
                }
            }

            // handle subselects
            // first we create streams for subselects, if there are any
            var @base = new StatementBaseInfo(compilable, statementSpec, null, statementRawInfo, null);
            IList<NamedWindowConsumerStreamSpec> subqueryNamedWindowConsumers =
                new List<NamedWindowConsumerStreamSpec>();
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                true,
                EmptyList<FilterSpecTracked>.Instance, 
                subqueryNamedWindowConsumers,
                @base,
                services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);
            // validate dependent expressions which may have subselects themselves
            for (var i = 0; i < numStreams; i++) {
                processors[i].ValidateDependentExpr(statementSpec, statementRawInfo, services);
            }

            var subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                namesPerStream,
                typesPerStream,
                eventTypeNames,
                services);
            subselectForges = subSelectForgePlan.Subselects;
            additionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);
            // obtain result set processor
            var isIStreamOnly = new bool[namesPerStream.Length];
            isIStreamOnly.Fill(true);
            StreamTypeService typeService = new StreamTypeServiceImpl(
                typesPerStream,
                namesPerStream,
                isIStreamOnly,
                true,
                optionalStreamsIfAny);
            whereClause = EPStatementStartMethodHelperValidate.ValidateNodes(
                statementSpec.Raw,
                typeService,
                null,
                statementRawInfo,
                services);
            var resultSetSpec = new ResultSetSpec(statementSpec);
            resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                ResultSetProcessorAttributionKeyStatement.INSTANCE,
                resultSetSpec,
                typeService,
                null,
                Array.Empty<bool>(),
                true,
                null,
                true,
                false,
                statementRawInfo,
                services);
            additionalForgeables.AddAll(resultSetProcessor.AdditionalForgeables);
            // plan table access
            tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(statementSpec.Raw.TableExpressions);
            // plan joins or simple queries
            if (numStreams > 1) {
                var streamJoinAnalysisResult = new StreamJoinAnalysisResultCompileTime(numStreams);
                CompatExtensions.Fill(streamJoinAnalysisResult.NamedWindowsPerStream, (NamedWindowMetaData) null);
                for (var i = 0; i < numStreams; i++) {
                    var uniqueIndexes = processors[i].UniqueIndexes;
                    streamJoinAnalysisResult.UniqueKeys[i] = uniqueIndexes;
                }

                bool hasAggregations = resultSetProcessor.ResultSetProcessorType.IsAggregated();
                var desc = JoinSetComposerPrototypeForgeFactory.MakeComposerPrototype(
                    statementSpec,
                    streamJoinAnalysisResult,
                    types,
                    new HistoricalViewableDesc(numStreams),
                    true,
                    hasAggregations,
                    statementRawInfo,
                    services);
                additionalForgeables.AddAll(desc.AdditionalForgeables);
                joins = desc.Forge;
            }
            else {
                joins = null;
            }

            // no-from-clause with context does not currently allow order-by
            if (processors.Length == 0 &&
                contextName != null &&
                resultSetProcessor.OrderByProcessorFactoryForge != null) {
                throw new ExprValidationException(
                    "Fire-and-forget queries without a from-clause and with context do not allow order-by");
            }

            var multiKeyPlan = MultiKeyPlanner.PlanMultiKeyDistinct(
                isDistinct,
                resultSetProcessor.ResultEventType,
                statementRawInfo,
                SerdeCompileTimeResolverNonHA.INSTANCE);
            additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
            distinctMultiKey = multiKeyPlan.ClassRef;
        }

        public bool HasTableAccess => hasTableAccess;

        public bool IsDistinct => isDistinct;

        public JoinSetComposerPrototypeForge Joins => joins;

        public FireAndForgetProcessorForge[] Processors => processors;

        public ResultSetProcessorDesc ResultSetProcessor => resultSetProcessor;

        public QueryGraphForge QueryGraph => queryGraph;

        public ExprNode WhereClause => whereClause;

        public ExprNode[] ConsumerFilters => consumerFilters;

        public Attribute[] Annotations => annotations;

        public string ContextName => contextName;

        public IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> TableAccessForges =>
            tableAccessForges;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public MultiKeyClassRef DistinctMultiKey => distinctMultiKey;

        public IDictionary<ExprSubselectNode, SubSelectFactoryForge> SubselectForges => subselectForges;

        public string ContextModuleName => contextModuleName;
    }
} // end of namespace