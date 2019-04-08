///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	/// <summary>
	/// Starts and provides the stop method for EPL statements.
	/// </summary>
	public class FAFQueryMethodSelectDesc {
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
	    private bool hasTableAccess;
	    private readonly bool isDistinct;
	    private IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges;

	    public FAFQueryMethodSelectDesc(StatementSpecCompiled statementSpec,
	                                    Compilable compilable,
	                                    StatementRawInfo statementRawInfo,
	                                    StatementCompileTimeServices services)
	            {
	        this.annotations = statementSpec.Annotations;
	        this.contextName = statementSpec.Raw.OptionalContextName;

	        bool queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
	        if (queryPlanLogging) {
	            QUERY_PLAN_LOG.Info("Query plans for Fire-and-forget query '" + compilable.ToEPL() + "'");
	        }

	        this.hasTableAccess = statementSpec.TableAccessNodes != null && statementSpec.TableAccessNodes.Count > 0;
	        foreach (StreamSpecCompiled streamSpec in statementSpec.StreamSpecs) {
	            hasTableAccess |= streamSpec is TableQueryStreamSpec;
	        }
	        this.isDistinct = statementSpec.SelectClauseCompiled.IsDistinct;

	        FAFQueryMethodHelper.ValidateFAFQuery(statementSpec);

	        int numStreams = statementSpec.StreamSpecs.Length;
	        EventType[] typesPerStream = new EventType[numStreams];
	        string[] namesPerStream = new string[numStreams];
	        processors = new FireAndForgetProcessorForge[numStreams];
	        consumerFilters = new ExprNode[numStreams];

	        // check context partition use
	        if (statementSpec.Raw.OptionalContextName != null) {
	            if (numStreams > 1) {
	                throw new ExprValidationException("Joins in runtime queries for context partitions are not supported");
	            }
	        }

	        // resolve types and processors
	        for (int i = 0; i < numStreams; i++) {
	            StreamSpecCompiled streamSpec = statementSpec.StreamSpecs[i];
	            processors[i] = FireAndForgetProcessorForgeFactory.ValidateResolveProcessor(streamSpec);
	            if (numStreams > 1 && processors[i].ContextName != null) {
	                throw new ExprValidationException("Joins against named windows that are under context are not supported");
	            }

	            string streamName = processors[i].NamedWindowOrTableName;
	            if (streamSpec.OptionalStreamName != null) {
	                streamName = streamSpec.OptionalStreamName;
	            }
	            namesPerStream[i] = streamName;
	            typesPerStream[i] = processors[i].EventTypeRSPInputEvents;

	            IList<ExprNode> consumerFilterExprs;
	            if (streamSpec is NamedWindowConsumerStreamSpec) {
	                NamedWindowConsumerStreamSpec namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
	                consumerFilterExprs = namedSpec.FilterExpressions;
	            } else {
	                TableQueryStreamSpec tableSpec = (TableQueryStreamSpec) streamSpec;
	                consumerFilterExprs = tableSpec.FilterExpressions;
	            }
	            consumerFilters[i] = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(consumerFilterExprs);
	        }

	        // compile filter to optimize access to named window
	        bool optionalStreamsIfAny = OuterJoinAnalyzer.OptionalStreamsIfAny(statementSpec.Raw.OuterJoinDescList);
	        StreamTypeServiceImpl types = new StreamTypeServiceImpl(typesPerStream, namesPerStream, new bool[numStreams], false, optionalStreamsIfAny);
	        ExcludePlanHint excludePlanHint = ExcludePlanHint.GetHint(types.StreamNames, statementRawInfo, services);
	        queryGraph = new QueryGraphForge(numStreams, excludePlanHint, false);
	        if (statementSpec.Raw.WhereClause != null) {
	            for (int i = 0; i < numStreams; i++) {
	                try {
	                    ExprValidationContext validationContext = new ExprValidationContextBuilder(types, statementRawInfo, services)
	                            .WithAllowBindingConsumption(true).WithIsFilterExpression(true).Build();
	                    ExprNode validated = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.FILTER, statementSpec.Raw.WhereClause, validationContext);
	                    FilterExprAnalyzer.Analyze(validated, queryGraph, false);
	                } catch (Exception ex) {
	                    Log.Warn("Unexpected exception analyzing filter paths: " + ex.Message, ex);
	                }
	            }
	        }

	        // obtain result set processor
	        bool[] isIStreamOnly = new bool[namesPerStream.Length];
	        CompatExtensions.Fill(isIStreamOnly, true);
	        StreamTypeService typeService = new StreamTypeServiceImpl(typesPerStream, namesPerStream, isIStreamOnly, true, optionalStreamsIfAny);
	        whereClause = EPStatementStartMethodHelperValidate.ValidateNodes(statementSpec.Raw, typeService, null, statementRawInfo, services);

	        ResultSetSpec resultSetSpec = new ResultSetSpec(statementSpec);
	        resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(resultSetSpec,
	                typeService, null, new bool[0], true, null,
	                true, false, statementRawInfo, services);

	        // plan table access
	        tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(statementSpec.Raw.TableExpressions);

	        // plan joins or simple queries
	        if (numStreams > 1) {
	            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult = new StreamJoinAnalysisResultCompileTime(numStreams);
	            CompatExtensions.Fill(streamJoinAnalysisResult.NamedWindowsPerStream, null);
	            for (int i = 0; i < numStreams; i++) {
	                string[][] uniqueIndexes = processors[i].UniqueIndexes;
	                streamJoinAnalysisResult.UniqueKeys[i] = uniqueIndexes;
	            }

	            bool hasAggregations = resultSetProcessor.ResultSetProcessorType.IsAggregated;
	            joins = JoinSetComposerPrototypeForgeFactory.MakeComposerPrototype(statementSpec, streamJoinAnalysisResult,
	                    types, new HistoricalViewableDesc(numStreams), true, hasAggregations, statementRawInfo, services);
	        } else {
	            joins = null;
	        }
	    }

	    public JoinSetComposerPrototypeForge Joins
	    {
	        get => joins;
	    }

	    public FireAndForgetProcessorForge[] Processors
	    {
	        get => processors;
	    }

	    public ResultSetProcessorDesc ResultSetProcessor
	    {
	        get => resultSetProcessor;
	    }

	    public QueryGraphForge QueryGraph
	    {
	        get => queryGraph;
	    }

	    public bool IsTableAccess
	    {
	        get => hasTableAccess;
	    }

	    public ExprNode WhereClause
	    {
	        get => whereClause;
	    }

	    public ExprNode[] ConsumerFilters
	    {
	        get => consumerFilters;
	    }

	    public Attribute[] Annotations
	    {
	        get => annotations;
	    }

	    public string ContextName
	    {
	        get => contextName;
	    }

	    public IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> GetTableAccessForges() {
	        return tableAccessForges;
	    }

	    public bool IsDistinct
	    {
	        get => isDistinct;
	    }
	}
} // end of namespace