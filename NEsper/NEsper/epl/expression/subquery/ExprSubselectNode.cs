///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.subquery
{
	/// <summary>
	/// Represents a subselect in an expression tree.
	/// </summary>
	[Serializable]
    public abstract class ExprSubselectNode
        : ExprNodeBase
        , ExprEvaluator
        , ExprEvaluatorEnumeration
        , ExprEvaluatorTypableReturn
	{
	    public static readonly ExprSubselectNode[] EMPTY_SUBSELECT_ARRAY = new ExprSubselectNode[0];

	    /// <summary>
	    /// The validated select clause.
	    /// </summary>
	    private ExprNode[] _selectClause;
        [NonSerialized]
        internal ExprEvaluator[] SelectClauseEvaluator;

        private string[] _selectAsNames;

	    /// <summary>
	    /// The validate filter expression.
	    /// </summary>
        [NonSerialized]
        private ExprEvaluator _filterExpr;

	    /// <summary>
	    /// The event type generated for wildcard selects.
	    /// </summary>
        [NonSerialized]
        private EventType _rawEventType;

        internal string StatementName;

	    [NonSerialized] private StreamTypeService _filterSubqueryStreamTypes;
	    private readonly StatementSpecRaw _statementSpecRaw;
	    [NonSerialized] private StatementSpecCompiled _statementSpecCompiled;
	    [NonSerialized] private ExprSubselectStrategy _strategy;
	    [NonSerialized] private SubqueryAggregationType _subselectAggregationType;
        private int _subselectNumber;
	    private bool _filterStreamSubselect;
        [NonSerialized]
        private AggregationService _subselectAggregationService;

	    /// <summary>
	    /// Evaluate the lookup expression returning an evaluation result object.
	    /// </summary>
	    /// <param name="eventsPerStream">is the events for each stream in a join</param>
	    /// <param name="isNewData">is true for new data, or false for old data</param>
	    /// <param name="matchingEvents">is filtered results from the table of stored lookup events</param>
	    /// <param name="exprEvaluatorContext">context for expression evalauation</param>
	    /// <returns>evaluation result</returns>
	    public abstract object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);

	    public abstract bool IsAllowMultiColumnSelect { get; }

	    public abstract void ValidateSubquery(ExprValidationContext validationContext) ;

	    public abstract LinkedHashMap<string, object> TypableGetRowProperties { get; }
	    public abstract object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);

	    public abstract Type ReturnType { get; }
	    public abstract Type ComponentTypeCollection { get; }
	    public abstract EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId);
	    public abstract EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId);

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
	    protected ExprSubselectNode(StatementSpecRaw statementSpec)
	    {
	        _statementSpecRaw = statementSpec;
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return this; }
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
        {
	        StatementName = validationContext.StatementName;
	        ValidateSubquery(validationContext);
	        return null;
	    }

	    /// <summary>
	    /// Supplies a compiled statement spec.
	    /// </summary>
	    /// <param name="statementSpecCompiled">compiled validated filters</param>
        public virtual void SetStatementSpecCompiled(StatementSpecCompiled statementSpecCompiled, int subselectNumber)
	    {
	        _statementSpecCompiled = statementSpecCompiled;
	        _subselectNumber = subselectNumber;
	    }

	    /// <summary>
	    /// Returns the compiled statement spec.
	    /// </summary>
	    /// <value>compiled statement</value>
        public virtual StatementSpecCompiled StatementSpecCompiled
	    {
	        get { return _statementSpecCompiled; }
	    }

        public virtual object Evaluate(EvaluateParams evaluateParams)
	    {
	        return Evaluate(
	            evaluateParams.EventsPerStream,
	            evaluateParams.IsNewData,
	            evaluateParams.ExprEvaluatorContext);
	    }

        public virtual object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QExprSubselect(this);
	            ICollection<EventBean> matchingEventsX = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
	            object result = Evaluate(eventsPerStream, isNewData, matchingEventsX, exprEvaluatorContext);
	            InstrumentationHelper.Get().AExprSubselect(result);
	            return result;
	        }
	        ICollection<EventBean> matchingEvents = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
	        return Evaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext);
	    }

        public virtual ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        ICollection<EventBean> matchingEvents = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
	        return EvaluateGetCollEvents(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext);
	    }

        public virtual ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        ICollection<EventBean> matchingEvents = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
	        return EvaluateGetCollScalar(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext);
	    }

	    public virtual EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        ICollection<EventBean> matchingEvents = EvaluateMatching(eventsPerStream, context);
	        return EvaluateGetEventBean(eventsPerStream, isNewData, matchingEvents, context);
	    }

        private ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
	        return _strategy.EvaluateMatching(eventsPerStream, exprEvaluatorContext);
	    }

        public virtual IDictionary<string, object> RowProperties
	    {
	        get { return TypableGetRowProperties; }
	    }

        public virtual bool? IsMultirow
	    {
	        get { return true; } // subselect can always return multiple rows
	    }

        public virtual object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        ICollection<EventBean> matching = _strategy.EvaluateMatching(eventsPerStream, context);
	        return EvaluateTypableSingle(eventsPerStream, isNewData, matching, context);
	    }

        public virtual object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        ICollection<EventBean> matching = _strategy.EvaluateMatching(eventsPerStream, context);
	        return EvaluateTypableMulti(eventsPerStream, isNewData, matching, context);
	    }

	    /// <summary>
	    /// Returns the uncompiled statement spec.
	    /// </summary>
	    /// <value>statement spec uncompiled</value>
        public StatementSpecRaw StatementSpecRaw
	    {
	        get { return _statementSpecRaw; }
	    }

	    /// <summary>
	    /// Supplies the name of the select expression as-tag
	    /// </summary>
	    /// <value>is the as-name(s)</value>
	    public string[] SelectAsNames
	    {
	        set { _selectAsNames = value; }
	        get { return _selectAsNames; }
	    }

	    /// <summary>
	    /// Sets the validated filter expression, or null if there is none.
	    /// </summary>
	    /// <value>is the filter</value>
	    public ExprEvaluator FilterExpr
	    {
	        set { _filterExpr = value; }
	        get { return _filterExpr; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        if ((_selectAsNames != null) && (_selectAsNames[0] != null))
	        {
	            writer.Write(_selectAsNames[0]);
	            return;
	        }
	        writer.Write("subselect_");
	        writer.Write(_subselectNumber);
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.UNARY; }
	    }

	    public override bool EqualsNode(ExprNode node)
	    {
	        return false;   // 2 subselects are never equivalent
	    }

	    /// <summary>
	    /// Sets the strategy for boiling down the table of lookup events into a subset against which to run the filter.
	    /// </summary>
	    /// <value>is the looking strategy (full table scan or indexed)</value>
	    public ExprSubselectStrategy Strategy
	    {
	        set { _strategy = value; }
	    }

	    /// <summary>
	    /// Sets the event type generated for wildcard selects.
	    /// </summary>
	    /// <value>is the wildcard type (parent view)</value>
	    public EventType RawEventType
	    {
	        set { _rawEventType = value; }
	        get { return _rawEventType; }
	    }

	    /// <summary>
	    /// Returns the select clause or null if none.
	    /// </summary>
	    /// <value>clause</value>
	    public ExprNode[] SelectClause
	    {
	        get { return _selectClause; }
	        set
	        {
	            _selectClause = value;
	            SelectClauseEvaluator = ExprNodeUtility.GetEvaluators(value);
	        }
	    }

	    /// <summary>
	    /// Return stream types.
	    /// </summary>
	    /// <value>types</value>
	    public StreamTypeService FilterSubqueryStreamTypes
	    {
	        get { return _filterSubqueryStreamTypes; }
	        set { _filterSubqueryStreamTypes = value; }
	    }

	    public SubqueryAggregationType SubselectAggregationType
	    {
	        get { return _subselectAggregationType; }
	        set { _subselectAggregationType = value; }
	    }

	    public int SubselectNumber
	    {
	        get { return _subselectNumber; }
	    }

	    public bool IsFilterStreamSubselect
	    {
	        set { _filterStreamSubselect = value; }
	        get { return _filterStreamSubselect; }
	    }

	    public static ExprSubselectNode[] ToArray(IList<ExprSubselectNode> subselectNodes)
        {
	        if (subselectNodes.IsEmpty()) {
	            return EMPTY_SUBSELECT_ARRAY;
	        }
	        return subselectNodes.ToArray();
	    }

	    public void SetSubselectAggregationService(AggregationService subselectAggregationService)
        {
	        _subselectAggregationService = subselectAggregationService;
	    }

	    public AggregationService SubselectAggregationService
	    {
	        get { return _subselectAggregationService; }
            internal set { _subselectAggregationService = value; }
	    }

	    public enum SubqueryAggregationType
        {
	        NONE,
	        FULLY_AGGREGATED,
	        AGGREGATED
	    }
	}
} // end of namespace
