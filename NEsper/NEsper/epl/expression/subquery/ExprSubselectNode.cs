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
    /// <summary>Represents a subselect in an expression tree.</summary>
    [Serializable]
    public abstract class ExprSubselectNode
        : ExprNodeBase
        , ExprEvaluator
        , ExprEvaluatorEnumeration
        , ExprEvaluatorTypableReturn
    {
        public static readonly ExprSubselectNode[] EMPTY_SUBSELECT_ARRAY = new ExprSubselectNode[0];
    
        /// <summary>The validated select clause.</summary>
        private ExprNode[] _selectClause;
        [NonSerialized] private ExprEvaluator[] _selectClauseEvaluator;
    
        private string[] _selectAsNames;
    
        /// <summary>The validate filter expression.</summary>
        [NonSerialized] private ExprEvaluator _filterExpr;
    
        /// <summary>The validated having expression.</summary>
        [NonSerialized] private ExprEvaluator _havingExpr;
    
        /// <summary>The event type generated for wildcard selects.</summary>
        [NonSerialized] private EventType _rawEventType;
    
        private string _statementName;
        private int _subselectNumber;
        [NonSerialized] private AggregationService _subselectAggregationService;
        [NonSerialized] private StreamTypeService _filterSubqueryStreamTypes;
        private readonly StatementSpecRaw _statementSpecRaw;
        [NonSerialized] private StatementSpecCompiled _statementSpecCompiled;
        [NonSerialized] private ExprSubselectStrategy _strategy;
        [NonSerialized] private SubqueryAggregationType _subselectAggregationType;
        private bool _filterStreamSubselect;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
        protected ExprSubselectNode(StatementSpecRaw statementSpec)
        {
            _statementSpecRaw = statementSpec;
        }

        public static ExprSubselectNode[] ToArray(IList<ExprSubselectNode> subselectNodes)
        {
            if (subselectNodes.IsEmpty())
            {
                return EMPTY_SUBSELECT_ARRAY;
            }
            return subselectNodes.ToArray();
        }

        public virtual object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        /// <summary>
        /// Evaluate the lookup expression returning an evaluation result object.
        /// </summary>
        /// <param name="eventsPerStream">is the events for each stream in a join</param>
        /// <param name="isNewData">is true for new data, or false for old data</param>
        /// <param name="matchingEvents">is filtered results from the table of stored lookup events</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>evaluation result</returns>
        public abstract Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);

        public abstract ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
    
        public abstract ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
    
        public abstract EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);

        public abstract bool IsAllowMultiColumnSelect { get; }

        public abstract void ValidateSubquery(ExprValidationContext validationContext) ;

        public abstract LinkedHashMap<string, object> TypableGetRowProperties { get; }

        public abstract Object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract Object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext);

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public ExprEvaluator[] SelectClauseEvaluator
        {
            get { return _selectClauseEvaluator; }
        }

        public string StatementName
        {
            get { return _statementName; }
        }

        /// <summary>
        /// Sets the strategy for boiling down the table of lookup events into a subset against which to run the filter.
        /// </summary>
        /// <value>is the looking strategy (full table scan or indexed)</value>
        public ExprSubselectStrategy Strategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }

        public bool FilterStreamSubselect
        {
            get { return _filterStreamSubselect; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext) {
            _statementName = validationContext.StatementName;
            ValidateSubquery(validationContext);
            return null;
        }
    
        /// <summary>
        /// Supplies a compiled statement spec.
        /// </summary>
        /// <param name="statementSpecCompiled">compiled validated filters</param>
        /// <param name="subselectNumber">subselect assigned number</param>
        public void SetStatementSpecCompiled(StatementSpecCompiled statementSpecCompiled, int subselectNumber) {
            _statementSpecCompiled = statementSpecCompiled;
            _subselectNumber = subselectNumber;
        }

        /// <summary>
        /// Returns the compiled statement spec.
        /// </summary>
        /// <value>compiled statement</value>
        public StatementSpecCompiled StatementSpecCompiled
        {
            get { return _statementSpecCompiled; }
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QExprSubselect(this);
                ICollection<EventBean> matchingEventsX = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
                Object result = Evaluate(eventsPerStream, isNewData, matchingEventsX, exprEvaluatorContext);
                InstrumentationHelper.Get().AExprSubselect(result);
                return result;
            }
            ICollection<EventBean> matchingEvents = EvaluateMatching(eventsPerStream, exprEvaluatorContext);
            return Evaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext);
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams) {
            ICollection<EventBean> matchingEvents = EvaluateMatching(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
            return EvaluateGetCollEvents(evaluateParams.EventsPerStream, evaluateParams.IsNewData, matchingEvents, evaluateParams.ExprEvaluatorContext);
        }
    
        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams) {
            ICollection<EventBean> matchingEvents = EvaluateMatching(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
            return EvaluateGetCollScalar(evaluateParams.EventsPerStream, evaluateParams.IsNewData, matchingEvents, evaluateParams.ExprEvaluatorContext);
        }
    
        public virtual EventBean EvaluateGetEventBean(EvaluateParams evaluateParams) {
            ICollection<EventBean> matchingEvents = EvaluateMatching(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
            return EvaluateGetEventBean(evaluateParams.EventsPerStream, evaluateParams.IsNewData, matchingEvents, evaluateParams.ExprEvaluatorContext);
        }
    
        private ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            return _strategy.EvaluateMatching(eventsPerStream, exprEvaluatorContext);
        }

        public IDictionary<string, object> RowProperties
        {
            get { return TypableGetRowProperties; }
        }

        public bool? IsMultirow
        {
            get { return true; }  // subselect can always return multiple rows
        }

        public Object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ICollection<EventBean> matching = _strategy.EvaluateMatching(eventsPerStream, context);
            return EvaluateTypableSingle(eventsPerStream, isNewData, matching, context);
        }
    
        public Object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
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
        /// <value>is the as-Name(s)</value>
        public string[] SelectAsNames
        {
            get { return _selectAsNames; }
            set { _selectAsNames = value; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if ((_selectAsNames != null) && (_selectAsNames[0] != null)) {
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

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
            return false;   // 2 subselects are never equivalent
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
                _selectClauseEvaluator = ExprNodeUtility.GetEvaluators(value);
            }
        }

        /// <summary>
        /// Returns filter expr or null if none.
        /// </summary>
        /// <value>filter</value>
        public ExprEvaluator FilterExpr
        {
            get { return _filterExpr; }
            set { _filterExpr = value; }
        }

        public ExprEvaluator HavingExpr
        {
            get { return _havingExpr; }
            set { _havingExpr = value; }
        }

        /// <summary>
        /// Returns the event type.
        /// </summary>
        /// <value>type</value>
        public EventType RawEventType
        {
            get { return _rawEventType; }
            set { _rawEventType = value; }
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
            get { return _filterStreamSubselect; }
            set { _filterStreamSubselect = value; }
        }

        public AggregationService SubselectAggregationService
        {
            get { return _subselectAggregationService; }
            set { _subselectAggregationService = value; }
        }

        public abstract Type ReturnType { get; }
        public abstract EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId);
        public abstract Type ComponentTypeCollection { get; }
        public abstract EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId);

        public enum SubqueryAggregationType
        {
            NONE,
            FULLY_AGGREGATED_NOPROPS,
            FULLY_AGGREGATED_WPROPS
        }
    }
} // end of namespace
