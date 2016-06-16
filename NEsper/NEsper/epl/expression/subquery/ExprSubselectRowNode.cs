///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
	/// <summary>
	/// Represents a subselect in an expression tree.
	/// </summary>
	[Serializable]
    public class ExprSubselectRowNode : ExprSubselectNode
	{
	    public static readonly ExprSubselectRowEvalStrategy UNFILTERED_UNSELECTED = new ExprSubselectRowEvalStrategyUnfilteredUnselected();
	    public static readonly ExprSubselectRowEvalStrategy UNFILTERED_SELECTED = new ExprSubselectRowEvalStrategyUnfilteredSelected();
	    public static readonly ExprSubselectRowEvalStrategy FILTERED_UNSELECTED = new ExprSubselectRowEvalStrategyFilteredUnselected();
	    public static readonly ExprSubselectRowEvalStrategy FILTERED_SELECTED = new ExprSubselectRowEvalStrategyFilteredSelected();
	    public static readonly ExprSubselectRowEvalStrategy UNFILTERED_SELECTED_GROUPED = new ExprSubselectRowEvalStrategyUnfilteredSelectedGroupedAgg();

	    [NonSerialized] internal SubselectMultirowType subselectMultirowType;
	    [NonSerialized] private ExprSubselectRowEvalStrategy _evalStrategy;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
	    public ExprSubselectRowNode(StatementSpecRaw statementSpec)
            : base(statementSpec)
        {
	    }

	    public override Type ReturnType
	    {
	        get
	        {
	            var selectClause = SelectClause;
	            if (selectClause == null)
	            {
	                // wildcards allowed
	                return RawEventType.UnderlyingType;
	            }
	            if (selectClause.Length == 1)
	            {
	                return selectClause[0].ExprEvaluator.ReturnType.GetBoxedType();
	            }
	            return null;
	        }
	    }

	    public override void ValidateSubquery(ExprValidationContext validationContext)
	    {
	        // Strategy for subselect depends on presence of filter + presence of select clause expressions
	        if (FilterExpr == null) {
	            if (SelectClause == null) {
	                TableMetadata tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(RawEventType);
	                if (tableMetadata != null) {
	                    _evalStrategy = new ExprSubselectRowEvalStrategyUnfilteredUnselectedTable(tableMetadata);
	                }
	                else {
	                    _evalStrategy = UNFILTERED_UNSELECTED;
	                }
	            }
	            else {
	                if (StatementSpecCompiled.GroupByExpressions != null && StatementSpecCompiled.GroupByExpressions.GroupByNodes.Length > 0) {
	                    _evalStrategy = UNFILTERED_SELECTED_GROUPED;
	                }
	                else {
	                    _evalStrategy = UNFILTERED_SELECTED;
	                }
	            }
	        }
	        else { // the filter expression is handled elsewhere if there is any aggregation
	            if (SelectClause == null) {
	                TableMetadata tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(RawEventType);
	                if (tableMetadata != null) {
	                    _evalStrategy = new ExprSubselectRowEvalStrategyFilteredUnselectedTable(tableMetadata);
	                }
	                else {
	                    _evalStrategy = FILTERED_UNSELECTED;
	                }
	            }
	            else {
	                _evalStrategy = FILTERED_SELECTED;
	            }
	        }
	    }

	    public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (matchingEvents == null || matchingEvents.Count == 0) {
	            return null;
	        }
	        return _evalStrategy.Evaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
	    }

	    public override ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context) {
	        if (matchingEvents == null) {
	            return null;
	        }
	        if (matchingEvents.Count == 0) {
	            return Collections.GetEmptyList<EventBean>();
	        }
	        return _evalStrategy.EvaluateGetCollEvents(eventsPerStream, isNewData, matchingEvents, context, this);
	    }

	    public override ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context) {
	        if (matchingEvents == null) {
	            return null;
	        }
	        if (matchingEvents.Count == 0) {
	            return Collections.GetEmptyList<object>();
	        }
	        return _evalStrategy.EvaluateGetCollScalar(eventsPerStream, isNewData, matchingEvents, context, this);
	    }

	    public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
	        if (matchingEvents == null || matchingEvents.Count == 0) {
	            return null;
	        }
	        return _evalStrategy.EvaluateGetEventBean(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
	    }

	    public override object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {

	        if (matchingEvents == null || matchingEvents.Count == 0) {
	            return null;
	        }
	        return _evalStrategy.TypableEvaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
	    }

	    public override object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
	        if (matchingEvents == null) {
	            return null;
	        }
	        if (matchingEvents.Count == 0) {
	            return new object[0][];
	        }
	        return _evalStrategy.TypableEvaluateMultirow(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
	    }

	    public override LinkedHashMap<string, object> TypableGetRowProperties
	    {
	        get
	        {
	            if ((SelectClause == null) || (SelectClause.Length < 2))
	            {
	                return null;
	            }
	            return RowType;
	        }
	    }

	    public override EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId) {
	        if (SelectClause == null) {
	            return null;
	        }
	        if (SubselectAggregationType != SubqueryAggregationType.FULLY_AGGREGATED) {
	            return null;
	        }
	        return GetAssignAnonymousType(eventAdapterService, statementId);
	    }

	    public override EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
	    {
	        var selectClause = SelectClause;
	        var rawEventType = RawEventType;

	        if (selectClause == null) {   // wildcards allowed
	            return rawEventType;
	        }

	        // special case: selecting a single property that is itself an event
	        if (selectClause.Length == 1 && selectClause[0] is ExprIdentNode) {
	            ExprIdentNode identNode = (ExprIdentNode) selectClause[0];
	            FragmentEventType fragment = rawEventType.GetFragmentType(identNode.ResolvedPropertyName);
	            if (fragment != null && !fragment.IsIndexed) {
	                return fragment.FragmentType;
	            }
	        }

	        // select of a single value otherwise results in a collection of scalar values
	        if (selectClause.Length == 1) {
	            return null;
	        }

	        // fully-aggregated always returns zero or one row
	        if (SubselectAggregationType == SubqueryAggregationType.FULLY_AGGREGATED) {
	            return null;
	        }

	        return GetAssignAnonymousType(eventAdapterService, statementId);
	    }

	    private EventType GetAssignAnonymousType(EventAdapterService eventAdapterService, int statementId)
        {
	        IDictionary<string, object> rowType = RowType;
	        EventType resultEventType = eventAdapterService.CreateAnonymousMapType(statementId + "_subquery_" + SubselectNumber, rowType, true);
	        subselectMultirowType = new SubselectMultirowType(resultEventType, eventAdapterService);
	        return resultEventType;
	    }

	    public override Type ComponentTypeCollection
	    {
	        get
	        {
	            if (SelectClause == null)
	            {
	                // wildcards allowed
	                return null;
	            }
	            if (SelectClauseEvaluator.Length > 1)
	            {
	                return null;
	            }
	            return SelectClauseEvaluator[0].ReturnType;
	        }
	    }

	    public override bool IsAllowMultiColumnSelect
	    {
	        get { return true; }
	    }

	    private LinkedHashMap<string, object> RowType
	    {
	        get
	        {
	            var uniqueNames = new HashSet<string>();
	            var type = new LinkedHashMap<string, object>();

                var selectClause = SelectClause;
                for (int i = 0; i < selectClause.Length; i++)
	            {
	                string assignedName = SelectAsNames[i];
	                if (assignedName == null)
	                {
	                    assignedName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(selectClause[i]);
	                }
	                if (uniqueNames.Add(assignedName))
	                {
	                    type.Put(assignedName, selectClause[i].ExprEvaluator.ReturnType);
	                }
	                else
	                {
	                    throw new ExprValidationException(
	                        "Column " + i + " in subquery does not have a unique column name assigned");
	                }
	            }
	            return type;
	        }
	    }

	    public string MultirowMessage
	    {
	        get
	        {
	            return "Subselect of statement '" + StatementName + "' returned more then one row in subselect " +
	                   SubselectNumber + " '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(this) +
	                   "', returning null result";
	        }
	    }

	    internal IDictionary<string, object> EvaluateRow(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        var map = new Dictionary<string, object>();
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, context);
            for (int i = 0; i < SelectClauseEvaluator.Length; i++)
            {
	            var resultEntry = SelectClauseEvaluator[i].Evaluate(evaluateParams);
	            map.Put(SelectAsNames[i], resultEntry);
	        }
	        return map;
	    }

	    public class SubselectMultirowType
        {
	        public SubselectMultirowType(EventType eventType, EventAdapterService eventAdapterService)
            {
	            EventType = eventType;
	            EventAdapterService = eventAdapterService;
	        }

	        public EventType EventType { get; private set; }

	        public EventAdapterService EventAdapterService { get; private set; }
        }
	}
} // end of namespace
