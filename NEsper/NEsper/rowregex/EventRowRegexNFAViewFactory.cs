///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.rowregex
{
	/// <summary>
	/// View factory for match-recognize view.
	/// </summary>
	public class EventRowRegexNFAViewFactory : ViewFactorySupport
	{
	    private readonly MatchRecognizeSpec _matchRecognizeSpec;
	    private readonly LinkedHashMap<string, Pair<int, Boolean>> _variableStreams;
	    private readonly IDictionary<int, string> _streamVariables;
	    private readonly ISet<string> _variablesSingle;
	    private readonly ObjectArrayEventType _compositeEventType;
	    private readonly EventType _rowEventType;
	    private readonly AggregationServiceMatchRecognize _aggregationService;
	    private readonly IList<AggregationServiceAggExpressionDesc> _aggregationExpressions;
	    private readonly SortedDictionary<int, IList<ExprPreviousMatchRecognizeNode>> _callbacksPerIndex = new SortedDictionary<int, IList<ExprPreviousMatchRecognizeNode>>();
	    private readonly bool _isUnbound;
	    private readonly bool _isIterateOnly;
	    private readonly bool _isCollectMultimatches;
	    private readonly bool _isDefineAsksMultimatches;
	    private readonly ObjectArrayBackedEventBean _defineMultimatchEventBean;
	    private readonly bool[] _isExprRequiresMultimatchState;
        private readonly RowRegexExprNode _expandedPatternNode;
        private readonly ConfigurationEngineDefaults.MatchRecognizeConfig _matchRecognizeConfig;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="viewChain">views</param>
        /// <param name="matchRecognizeSpec">specification</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="isUnbound">true for unbound stream</param>
        /// <param name="annotations">annotations</param>
        /// <param name="matchRecognizeConfig">The match recognize configuration.</param>
        /// <throws>ExprValidationException if validation fails</throws>
	    public EventRowRegexNFAViewFactory(
            IContainer container,
	        ViewFactoryChain viewChain,
	        MatchRecognizeSpec matchRecognizeSpec,
	        AgentInstanceContext agentInstanceContext,
	        bool isUnbound,
	        Attribute[] annotations,
            ConfigurationEngineDefaults.MatchRecognizeConfig matchRecognizeConfig)
	    {
	        var parentViewType = viewChain.EventType;
	        _matchRecognizeSpec = matchRecognizeSpec;
	        _isUnbound = isUnbound;
	        _isIterateOnly = HintEnum.ITERATE_ONLY.GetHint(annotations) != null;
            _matchRecognizeConfig = matchRecognizeConfig;
 
	        var statementContext = agentInstanceContext.StatementContext;

            // Expand repeats and permutations
            _expandedPatternNode = RegexPatternExpandUtil.Expand(
                container, matchRecognizeSpec.Pattern);

	        // Determine single-row and multiple-row variables
	        _variablesSingle = new LinkedHashSet<string>();
	        ISet<string> variablesMultiple = new LinkedHashSet<string>();
            EventRowRegexHelper.RecursiveInspectVariables(_expandedPatternNode, false, _variablesSingle, variablesMultiple);

	        // each variable gets associated with a stream number (multiple-row variables as well to hold the current event for the expression).
	        var streamNum = 0;
	        _variableStreams = new LinkedHashMap<string, Pair<int, bool>>();
	        foreach (var variableSingle in _variablesSingle)
	        {
                _variableStreams.Put(variableSingle, new Pair<int, bool>(streamNum, false));
	            streamNum++;
	        }
	        foreach (var variableMultiple in variablesMultiple)
	        {
                _variableStreams.Put(variableMultiple, new Pair<int, bool>(streamNum, true));
	            streamNum++;
	        }

	        // mapping of stream to variable
	        _streamVariables = new SortedDictionary<int, string>();
            foreach (var entry in _variableStreams)
	        {
	            _streamVariables.Put(entry.Value.First, entry.Key);
	        }

	        // determine visibility rules
	        var visibility = EventRowRegexHelper.DetermineVisibility(_expandedPatternNode);

	        // assemble all single-row variables for expression validation
	        var allStreamNames = new string[_variableStreams.Count];
	        var allTypes = new EventType[_variableStreams.Count];

	        streamNum = 0;
	        foreach (var variableSingle in _variablesSingle)
	        {
	            allStreamNames[streamNum] = variableSingle;
	            allTypes[streamNum] = parentViewType;
	            streamNum++;
	        }
	        foreach (var variableMultiple in variablesMultiple)
	        {
	            allStreamNames[streamNum] = variableMultiple;
	            allTypes[streamNum] = parentViewType;
	            streamNum++;
	        }

	        // determine type service for use with DEFINE
	        // validate each DEFINE clause expression
	        ISet<string> definedVariables = new HashSet<string>();
	        IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
	        var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
	        _isExprRequiresMultimatchState = new bool[_variableStreams.Count];

	        for (var defineIndex = 0; defineIndex < matchRecognizeSpec.Defines.Count; defineIndex++)
	        {
	            var defineItem = matchRecognizeSpec.Defines[defineIndex];
	            if (definedVariables.Contains(defineItem.Identifier))
	            {
	                throw new ExprValidationException("Variable '" + defineItem.Identifier + "' has already been defined");
	            }
	            definedVariables.Add(defineItem.Identifier);

	            // stream-type visibilities handled here
	            var typeServiceDefines = EventRowRegexNFAViewFactoryHelper.BuildDefineStreamTypeServiceDefine(statementContext, _variableStreams, defineItem, visibility, parentViewType);

	            var exprNodeResult = HandlePreviousFunctions(defineItem.Expression);
	            var validationContext = new ExprValidationContext(
	                statementContext.Container,
	                typeServiceDefines,
                    statementContext.EngineImportService, 
                    statementContext.StatementExtensionServicesContext, null, 
                    statementContext.SchedulingService,
	                statementContext.VariableService, 
                    statementContext.TableService, exprEvaluatorContext,
	                statementContext.EventAdapterService, 
                    statementContext.StatementName, 
                    statementContext.StatementId,
	                statementContext.Annotations, 
                    statementContext.ContextDescriptor, 
                    statementContext.ScriptingService,
                    true, false, true, false, null, false);

	            ExprNode validated;
	            try {
	                // validate
	                validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGDEFINE, exprNodeResult, validationContext);

	                // check aggregates
	                defineItem.Expression = validated;
	                ExprAggregateNodeUtil.GetAggregatesBottomUp(validated, aggregateNodes);
	                if (!aggregateNodes.IsEmpty()) {
	                    throw new ExprValidationException("An aggregate function may not appear in a DEFINE clause");
	                }
	            }
	            catch (ExprValidationException ex) {
	                throw new ExprValidationException("Failed to validate condition expression for variable '" + defineItem.Identifier + "': " + ex.Message, ex);
	            }

	            // determine access to event properties from multi-matches
                var visitor = new ExprNodeStreamRequiredVisitor();
	            validated.Accept(visitor);
	            var streamsRequired = visitor.StreamsRequired;
	            foreach (var streamRequired in streamsRequired) {
	                if (streamRequired >= _variableStreams.Count) {
	                    var streamNumIdent = _variableStreams.Get(defineItem.Identifier).First;
	                    _isExprRequiresMultimatchState[streamNumIdent] = true;
	                    break;
	                }
	            }
	        }
	        _isDefineAsksMultimatches = CollectionUtil.IsAnySet(_isExprRequiresMultimatchState);
	        _defineMultimatchEventBean = _isDefineAsksMultimatches ? EventRowRegexNFAViewFactoryHelper.GetDefineMultimatchBean(statementContext, _variableStreams, parentViewType) : null;

	        // assign "prev" node indexes
	        // Since an expression such as "prior(2, price), prior(8, price)" translates into {2, 8} the relative index is {0, 1}.
	        // Map the expression-supplied index to a relative index
	        var countPrev = 0;
	        foreach (var entry in _callbacksPerIndex) {
	            foreach (var callback in entry.Value) {
	                callback.AssignedIndex = countPrev;
	            }
	            countPrev++;
	        }

	        // determine type service for use with MEASURE
	        IDictionary<string, object> measureTypeDef = new LinkedHashMap<string, object>();
	        foreach (var variableSingle in _variablesSingle)
	        {
	            measureTypeDef.Put(variableSingle, parentViewType);
	        }
	        foreach (var variableMultiple in variablesMultiple)
	        {
	            measureTypeDef.Put(variableMultiple, new EventType[] {parentViewType});
	        }
	        var outputEventTypeName = statementContext.StatementId + "_rowrecog";
	        _compositeEventType = (ObjectArrayEventType) statementContext.EventAdapterService.CreateAnonymousObjectArrayType(outputEventTypeName, measureTypeDef);
	        StreamTypeService typeServiceMeasure = new StreamTypeServiceImpl(_compositeEventType, "MATCH_RECOGNIZE", true, statementContext.EngineURI);

	        // find MEASURE clause aggregations
	        var measureReferencesMultivar = false;
	        IList<ExprAggregateNode> measureAggregateExprNodes = new List<ExprAggregateNode>();
	        foreach (var measureItem in matchRecognizeSpec.Measures)
	        {
	            ExprAggregateNodeUtil.GetAggregatesBottomUp(measureItem.Expr, measureAggregateExprNodes);
	        }
	        if (!measureAggregateExprNodes.IsEmpty())
	        {
	            var isIStreamOnly = new bool[allStreamNames.Length];
	            CompatExtensions.Fill(isIStreamOnly, true);
	            var typeServiceAggregateMeasure = new StreamTypeServiceImpl(allTypes, allStreamNames, isIStreamOnly, statementContext.EngineURI, false);
	            var measureExprAggNodesPerStream = new Dictionary<int, IList<ExprAggregateNode>>();

	            foreach (var aggregateNode in measureAggregateExprNodes)
	            {
                    // validate absence of group-by
                    aggregateNode.ValidatePositionals();
                    if (aggregateNode.OptionalLocalGroupBy != null)
                    {
                        throw new ExprValidationException("Match-recognize does not allow aggregation functions to specify a group-by");
                    }

                    // validate node and params
                    var count = 0;
	                var visitor = new ExprNodeIdentifierVisitor(true);

	                var validationContext = new ExprValidationContext(
	                    statementContext.Container,
	                    typeServiceAggregateMeasure,
                        statementContext.EngineImportService, 
                        statementContext.StatementExtensionServicesContext, null,
	                    statementContext.SchedulingService,
                        statementContext.VariableService,
                        statementContext.TableService,
	                    exprEvaluatorContext,
                        statementContext.EventAdapterService, 
                        statementContext.StatementName,
	                    statementContext.StatementId,
                        statementContext.Annotations, 
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, true, false, null, false);
                    for (int ii = 0 ; ii < aggregateNode.ChildNodes.Count ; ii++)
                    {
                        var child = aggregateNode.ChildNodes[ii];
	                    var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGMEASURE, child, validationContext);
	                    validated.Accept(visitor);
	                    aggregateNode.SetChildNode(count++, new ExprNodeValidated(validated));
	                }
	                validationContext = new ExprValidationContext(
                        statementContext.Container,
	                    typeServiceMeasure,
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext, null,
	                    statementContext.SchedulingService,
                        statementContext.VariableService,
                        statementContext.TableService,
	                    exprEvaluatorContext,
                        statementContext.EventAdapterService,
                        statementContext.StatementName,
	                    statementContext.StatementId, 
                        statementContext.Annotations, 
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, true, false, null, false);
	                aggregateNode.Validate(validationContext);

	                // verify properties used within the aggregation
	                var aggregatedStreams = new HashSet<int>();
	                foreach (var pair in visitor.ExprProperties)
	                {
	                    aggregatedStreams.Add(pair.First);
	                }

	                int? multipleVarStream = null;
	                foreach (int streamNumAggregated in aggregatedStreams)
	                {
	                    var variable = _streamVariables.Get(streamNumAggregated);
	                    if (variablesMultiple.Contains(variable))
	                    {
	                        measureReferencesMultivar = true;
	                        if (multipleVarStream == null)
	                        {
	                            multipleVarStream = streamNumAggregated;
	                            continue;
	                        }
	                        throw new ExprValidationException("Aggregation functions in the measure-clause must only refer to properties of exactly one group variable returning multiple events");
	                    }
	                }

	                if (multipleVarStream == null)
	                {
	                    throw new ExprValidationException("Aggregation functions in the measure-clause must refer to one or more properties of exactly one group variable returning multiple events");
	                }

	                var aggNodesForStream = measureExprAggNodesPerStream.Get(multipleVarStream.Value);
	                if (aggNodesForStream == null)
	                {
	                    aggNodesForStream = new List<ExprAggregateNode>();
	                    measureExprAggNodesPerStream.Put(multipleVarStream.Value, aggNodesForStream);
	                }
	                aggNodesForStream.Add(aggregateNode);
	            }

	            var factoryDesc = AggregationServiceFactoryFactory.GetServiceMatchRecognize(_streamVariables.Count, measureExprAggNodesPerStream, typeServiceAggregateMeasure.EventTypes);
	            _aggregationService = factoryDesc.AggregationServiceFactory.MakeService(agentInstanceContext);
	            _aggregationExpressions = factoryDesc.Expressions;
	        }
	        else
	        {
	            _aggregationService = null;
	            _aggregationExpressions = Collections.GetEmptyList<AggregationServiceAggExpressionDesc>();
	        }

	        // validate each MEASURE clause expression
	        IDictionary<string, object> rowTypeDef = new LinkedHashMap<string, object>();
	        var streamRefVisitor = new ExprNodeStreamUseCollectVisitor();
	        foreach (var measureItem in matchRecognizeSpec.Measures)
	        {
	            if (measureItem.Name == null)
	            {
	                throw new ExprValidationException("The measures clause requires that each expression utilizes the AS keyword to assign a column name");
	            }
	            var validated = ValidateMeasureClause(measureItem.Expr, typeServiceMeasure, variablesMultiple, _variablesSingle, statementContext);
	            measureItem.Expr = validated;
	            rowTypeDef.Put(measureItem.Name, validated.ExprEvaluator.ReturnType);
	            validated.Accept(streamRefVisitor);
	        }

	        // Determine if any of the multi-var streams are referenced in the measures (non-aggregated only)
	        foreach (var @ref in streamRefVisitor.Referenced) {
	            var rootPropName = @ref.RootPropertyNameIfAny;
	            if (rootPropName != null) {
	                if (variablesMultiple.Contains(rootPropName)) {
	                    measureReferencesMultivar = true;
	                    break;
	                }
	            }

	            var streamRequired = @ref.StreamReferencedIfAny;
	            if (streamRequired != null) {
	                var streamVariable = _streamVariables.Get(streamRequired.Value);
	                if (streamVariable != null) {
	                    var def = _variableStreams.Get(streamVariable);
	                    if (def != null && def.Second) {
	                        measureReferencesMultivar = true;
	                        break;
	                    }
	                }
	            }
	        }
	        _isCollectMultimatches = measureReferencesMultivar || _isDefineAsksMultimatches;

	        // create rowevent type
	        var rowEventTypeName = statementContext.StatementId + "_rowrecogrow";
            _rowEventType = statementContext.EventAdapterService.CreateAnonymousMapType(rowEventTypeName, rowTypeDef, true);

	        // validate partition-by expressions, if any
	        if (!matchRecognizeSpec.PartitionByExpressions.IsEmpty())
	        {
	            var typeServicePartition = new StreamTypeServiceImpl(parentViewType, "MATCH_RECOGNIZE_PARTITION", true, statementContext.EngineURI);
	            var validated = new List<ExprNode>();
	            var validationContext = new ExprValidationContext(
	                statementContext.Container,
	                typeServicePartition,
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext, null,
	                statementContext.SchedulingService,
	                statementContext.VariableService,
	                statementContext.TableService,
	                exprEvaluatorContext, 
	                statementContext.EventAdapterService, 
	                statementContext.StatementName,
	                statementContext.StatementId,
	                statementContext.Annotations,
	                statementContext.ContextDescriptor,
                    statementContext.ScriptingService, 
                    false, false, true, false, null, false);
	            foreach (var partitionExpr in matchRecognizeSpec.PartitionByExpressions)
	            {
	                validated.Add(ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGPARTITION, partitionExpr, validationContext));
	            }
	            matchRecognizeSpec.PartitionByExpressions = validated;
	        }

	        // validate interval if present
	        if (matchRecognizeSpec.Interval != null)
	        {
	            var validationContext =
	                new ExprValidationContext(
	                    statementContext.Container,
	                    new StreamTypeServiceImpl(statementContext.EngineURI, false),
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext, null,
                        statementContext.SchedulingService,
	                    statementContext.VariableService, 
	                    statementContext.TableService, exprEvaluatorContext,
	                    statementContext.EventAdapterService, 
	                    statementContext.StatementName,
	                    statementContext.StatementId,
	                    statementContext.Annotations, 
	                    statementContext.ContextDescriptor,
	                    statementContext.ScriptingService,
                        false, false, true, false, null, false);
	            matchRecognizeSpec.Interval.Validate(validationContext);
	        }
	    }

	    private ExprNode ValidateMeasureClause(
	        ExprNode measureNode,
	        StreamTypeService typeServiceMeasure,
	        ISet<string> variablesMultiple,
	        ISet<string> variablesSingle,
	        StatementContext statementContext)
	    {
	        try
	        {
	            var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
	            var validationContext = new ExprValidationContext(
	                statementContext.Container,
	                typeServiceMeasure,
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext, null,
                    statementContext.SchedulingService,
	                statementContext.VariableService, 
	                statementContext.TableService, exprEvaluatorContext,
	                statementContext.EventAdapterService,
	                statementContext.StatementName,
	                statementContext.StatementId,
	                statementContext.Annotations,
	                statementContext.ContextDescriptor,
	                statementContext.ScriptingService,
                    false, false, true, false, null, false);
	            return ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGMEASURE, measureNode, validationContext);
	        }
	        catch (ExprValidationPropertyException e)
	        {
	            var grouped = CollectionUtil.ToString(variablesMultiple);
	            var single = CollectionUtil.ToString(variablesSingle);
	            var message = e.Message;
	            if (!variablesMultiple.IsEmpty())
	            {
	                message += ", ensure that grouped variables (variables " + grouped + ") are accessed via index (i.e. variable[0].property) or appear within an aggregation";
	            }
	            if (!variablesSingle.IsEmpty())
	            {
	                message += ", ensure that singleton variables (variables " + single + ") are not accessed via index";
	            }
	            throw new ExprValidationPropertyException(message, e);
	        }
	    }

	    private ExprNode HandlePreviousFunctions(ExprNode defineItemExpression)
	    {
	        var previousVisitor = new ExprNodePreviousVisitorWParent();
	        defineItemExpression.Accept(previousVisitor);

	        if (previousVisitor.Previous == null)
	        {
	            return defineItemExpression;
	        }

	        foreach (var previousNodePair in previousVisitor.Previous)
	        {
	            var previousNode = previousNodePair.Second;
	            var matchRecogPrevNode = new ExprPreviousMatchRecognizeNode();

	            if (previousNodePair.Second.ChildNodes.Count == 1)
	            {
	                matchRecogPrevNode.AddChildNode(previousNode.ChildNodes[0]);
	                matchRecogPrevNode.AddChildNode(new ExprConstantNodeImpl(1));
	            }
                else if (previousNodePair.Second.ChildNodes.Count == 2)
	            {
	                var first = previousNode.ChildNodes[0];
	                var second = previousNode.ChildNodes[1];
	                if ((first.IsConstantResult) && (!second.IsConstantResult))
	                {
	                    matchRecogPrevNode.AddChildNode(second);
	                    matchRecogPrevNode.AddChildNode(first);
	                }
	                else if ((!first.IsConstantResult) && (second.IsConstantResult))
	                {
	                    matchRecogPrevNode.AddChildNode(first);
	                    matchRecogPrevNode.AddChildNode(second);
	                }
	                else
	                {
	                    throw new ExprValidationException("PREV operator requires a constant index");
	                }
	            }

	            if (previousNodePair.First == null)
	            {
	                defineItemExpression = matchRecogPrevNode;
	            }
	            else
	            {
	                ExprNodeUtility.ReplaceChildNode(previousNodePair.First, previousNodePair.Second, matchRecogPrevNode);
	            }

	            // store in a list per index such that we can consolidate this into a single buffer
	            var index = matchRecogPrevNode.ConstantIndexNumber;
	            var callbackList = _callbacksPerIndex.Get(index);
	            if (callbackList == null)
	            {
	                callbackList = new List<ExprPreviousMatchRecognizeNode>();
	                _callbacksPerIndex.Put(index, callbackList);
	            }
	            callbackList.Add(matchRecogPrevNode);
	        }

	        return defineItemExpression;
	    }

	    public override void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
	    }

        public override void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
	    }

        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            EventRowRegexNFAViewScheduler scheduler = null;
            if (_matchRecognizeSpec.Interval != null)
            {
                scheduler = new EventRowRegexNFAViewSchedulerImpl();
            }

            EventRowRegexNFAView view = new EventRowRegexNFAView(
                this,
                _compositeEventType,
                _rowEventType,
                _matchRecognizeSpec,
                _variableStreams,
                _streamVariables,
                _variablesSingle,
                agentInstanceViewFactoryContext.AgentInstanceContext,
                _callbacksPerIndex,
                _aggregationService,
                _isDefineAsksMultimatches,
                _defineMultimatchEventBean,
                _isExprRequiresMultimatchState,
                _isUnbound,
                _isIterateOnly,
                _isCollectMultimatches,
                _expandedPatternNode,
                _matchRecognizeConfig,
                scheduler
                );

            if (scheduler != null)
            {
                scheduler.SetScheduleCallback(agentInstanceViewFactoryContext.AgentInstanceContext, view);
            }

            return view;
        }

	    public override EventType EventType
	    {
	        get { return _rowEventType; }
	    }

	    public IList<AggregationServiceAggExpressionDesc> AggregationExpressions
	    {
	        get { return _aggregationExpressions; }
	    }

	    public AggregationServiceMatchRecognize AggregationService
	    {
	        get { return _aggregationService; }
	    }

	    public ISet<ExprPreviousMatchRecognizeNode> PreviousExprNodes
	    {
	        get
	        {
	            if (_callbacksPerIndex.IsEmpty())
	            {
                    return Collections.GetEmptySet<ExprPreviousMatchRecognizeNode>();
	            }
	            var nodes = new HashSet<ExprPreviousMatchRecognizeNode>();
	            foreach (IList<ExprPreviousMatchRecognizeNode> list in _callbacksPerIndex.Values)
	            {
	                foreach (var node in list)
	                {
	                    nodes.Add(node);
	                }
	            }
	            return nodes;
	        }
	    }

	    public override string ViewName
	    {
	        get { return "Match-recognize"; }
	    }
	}
} // end of namespace
