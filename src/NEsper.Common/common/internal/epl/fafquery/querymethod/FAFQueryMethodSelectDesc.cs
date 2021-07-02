///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectDesc
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FAFQueryMethodSelectDesc(
            StatementSpecCompiled statementSpec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            Annotations = statementSpec.Annotations;
            ContextName = statementSpec.Raw.OptionalContextName;

            if (ContextName != null) {
                var contextMetaData = services.ContextCompileTimeResolver.GetContextInfo(ContextName);
                if (contextMetaData == null) {
                    throw new ExprValidationException("Failed to find context '" + ContextName + "'");
                }

                ContextModuleName = contextMetaData.ContextModuleName;
            }
            else {
                ContextModuleName = null;
            }

            var queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
            if (queryPlanLogging) {
                QUERY_PLAN_LOG.Info("Query plans for Fire-and-forget query '" + compilable.ToEPL() + "'");
            }

            HasTableAccess = statementSpec.TableAccessNodes != null && statementSpec.TableAccessNodes.Count > 0;
            foreach (var streamSpec in statementSpec.StreamSpecs) {
                HasTableAccess |= streamSpec is TableQueryStreamSpec;
            }

            HasTableAccess |= StatementLifecycleSvcUtil.IsSubqueryWithTable(
                statementSpec.SubselectNodes,
                services.TableCompileTimeResolver);
            IsDistinct = statementSpec.SelectClauseCompiled.IsDistinct;

            FAFQueryMethodHelper.ValidateFAFQuery(statementSpec);

            var numStreams = statementSpec.StreamSpecs.Length;
            var typesPerStream = new EventType[numStreams];
            var namesPerStream = new string[numStreams];
            var eventTypeNames = new string[numStreams];
            Processors = new FireAndForgetProcessorForge[numStreams];
            ConsumerFilters = new ExprNode[numStreams];

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
                Processors[i] = FireAndForgetProcessorForgeFactory.ValidateResolveProcessor(streamSpec);
                if (numStreams > 1 && Processors[i].ContextName != null) {
                    throw new ExprValidationException(
                        "Joins against named windows that are under context are not supported");
                }

                var streamName = Processors[i].NamedWindowOrTableName;
                if (streamSpec.OptionalStreamName != null) {
                    streamName = streamSpec.OptionalStreamName;
                }

                namesPerStream[i] = streamName;
                typesPerStream[i] = Processors[i].EventTypeRspInputEvents;
                eventTypeNames[i] = typesPerStream[i].Name;

                IList<ExprNode> consumerFilterExprs;
                if (streamSpec is NamedWindowConsumerStreamSpec) {
                    var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
                    consumerFilterExprs = namedSpec.FilterExpressions;
                }
                else {
                    var tableSpec = (TableQueryStreamSpec) streamSpec;
                    consumerFilterExprs = tableSpec.FilterExpressions;
                }

                ConsumerFilters[i] = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(consumerFilterExprs);
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
            QueryGraph = new QueryGraphForge(numStreams, excludePlanHint, false);
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
                        FilterExprAnalyzer.Analyze(validated, QueryGraph, false);
                    }
                    catch (Exception ex) {
                        Log.Warn("Unexpected exception analyzing filter paths: " + ex.Message, ex);
                    }
                }
            }

            // handle subselects
            // first we create streams for subselects, if there are any
            var @base = new StatementBaseInfo(compilable, statementSpec, null, statementRawInfo, null);
            var subqueryNamedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                true,
                EmptyList<FilterSpecCompiled>.Instance,
                subqueryNamedWindowConsumers,
                @base,
                services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            AdditionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);

            var subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                namesPerStream,
                typesPerStream,
                eventTypeNames,
                services);
            SubselectForges = subSelectForgePlan.Subselects;
            AdditionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);

            // obtain result set processor
            var isIStreamOnly = new bool[namesPerStream.Length];
            isIStreamOnly.Fill(true);
            StreamTypeService typeService = new StreamTypeServiceImpl(
                typesPerStream,
                namesPerStream,
                isIStreamOnly,
                true,
                optionalStreamsIfAny);
            WhereClause = EPStatementStartMethodHelperValidate.ValidateNodes(
                statementSpec.Raw,
                typeService,
                null,
                statementRawInfo,
                services);

            var resultSetSpec = new ResultSetSpec(statementSpec);
            ResultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                resultSetSpec,
                typeService,
                null,
                new bool[0],
                true,
                null,
                true,
                false,
                statementRawInfo,
                services);
            AdditionalForgeables.AddAll(ResultSetProcessor.AdditionalForgeables);

            // plan table access
            TableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(statementSpec.Raw.TableExpressions);

            // plan joins or simple queries
            if (numStreams > 1) {
                var streamJoinAnalysisResult = new StreamJoinAnalysisResultCompileTime(numStreams);
                CompatExtensions.Fill(streamJoinAnalysisResult.NamedWindowsPerStream, (NamedWindowMetaData) null);
                for (var i = 0; i < numStreams; i++) {
                    var uniqueIndexes = Processors[i].UniqueIndexes;
                    streamJoinAnalysisResult.UniqueKeys[i] = uniqueIndexes;
                }

                var hasAggregations = ResultSetProcessor.ResultSetProcessorType.IsAggregated();
                var desc = JoinSetComposerPrototypeForgeFactory.MakeComposerPrototype(
                    statementSpec,
                    streamJoinAnalysisResult,
                    types,
                    new HistoricalViewableDesc(numStreams),
                    true,
                    hasAggregations,
                    statementRawInfo,
                    services);
                AdditionalForgeables.AddAll(desc.AdditionalForgeables);
                Joins = desc.Forge;
            }
            else {
                Joins = null;
            }

            // no-from-clause with context does not currently allow order-by
            if (Processors.Length == 0 && ContextName != null && ResultSetProcessor.OrderByProcessorFactoryForge != null) {
                throw new ExprValidationException("Fire-and-forget queries without a from-clause and with context do not allow order-by");
            }

            var multiKeyPlan = MultiKeyPlanner.PlanMultiKeyDistinct(
                IsDistinct,
                ResultSetProcessor.ResultEventType,
                statementRawInfo,
                SerdeCompileTimeResolverNonHA.INSTANCE);
            AdditionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
            DistinctMultiKey = multiKeyPlan.ClassRef;
        }

        public JoinSetComposerPrototypeForge Joins { get; }

        public FireAndForgetProcessorForge[] Processors { get; }

        public ResultSetProcessorDesc ResultSetProcessor { get; }

        public QueryGraphForge QueryGraph { get; }

        public bool HasTableAccess { get; }

        public ExprNode WhereClause { get; }

        public ExprNode[] ConsumerFilters { get; }

        public Attribute[] Annotations { get; }

        public string ContextName { get; }
        
        public string ContextModuleName { get; }

        public IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> TableAccessForges { get; }

        public bool IsDistinct { get; }
        
        public MultiKeyClassRef DistinctMultiKey { get; }
        
        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; } = new List<StmtClassForgeableFactory>();
        
        public IDictionary<ExprSubselectNode, SubSelectFactoryForge> SubselectForges { get; }
    }
} // end of namespace