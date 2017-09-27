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

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Represents a subselect in an expression tree.</summary>
    [Serializable]
    public class ExprSubselectRowNode : ExprSubselectNode
    {
        public static readonly SubselectEvalStrategyRow UNFILTERED_SELECTED =
            new SubselectEvalStrategyRowUnfilteredSelected();

        public static readonly SubselectEvalStrategyRow FILTERED_UNSELECTED =
            new SubselectEvalStrategyRowFilteredUnselected();

        public static readonly SubselectEvalStrategyRow FILTERED_SELECTED =
            new SubselectEvalStrategyRowFilteredSelected();

        public static readonly SubselectEvalStrategyRow HAVING_SELECTED = new SubselectEvalStrategyRowHavingSelected();

        public static readonly SubselectEvalStrategyRow UNFILTERED_SELECTED_GROUPED =
            new SubselectEvalStrategyRowUnfilteredSelectedGroupedNoHaving();

        [NonSerialized] private SubselectMultirowType _subselectMultirowType;
        [NonSerialized] private SubselectEvalStrategyRow _evalStrategy;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
        public ExprSubselectRowNode(StatementSpecRaw statementSpec)
            : base(statementSpec)
        {
        }

        public SubselectEvalStrategyRow EvalStrategy
        {
            get { return _evalStrategy; }
        }

        internal SubselectMultirowType SubselectMultirowType
        {
            get { return _subselectMultirowType; }
        }

        public override Type ReturnType
        {
            get
            {
                if (base.SelectClause == null)
                {
                    // wildcards allowed
                    return RawEventType.UnderlyingType;
                }
                if (base.SelectClause.Length == 1)
                {
                    return base.SelectClause[0].ExprEvaluator.ReturnType.GetBoxedType();
                }
                return null;
            }
        }

        public override void ValidateSubquery(ExprValidationContext validationContext)
        {
            // Strategy for subselect depends on presence of filter + presence of select clause expressions
            // the filter expression is handled elsewhere if there is any aggregation
            if (FilterExpr == null)
            {
                if (SelectClause == null)
                {
                    var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(RawEventType);
                    if (tableMetadata != null)
                    {
                        _evalStrategy = new SubselectEvalStrategyRowUnfilteredUnselectedTable(tableMetadata);
                    }
                    else
                    {
                        _evalStrategy = SubselectEvalStrategyRowUnfilteredUnselected.INSTANCE;
                    }
                }
                else
                {
                    if (StatementSpecCompiled.GroupByExpressions != null &&
                        StatementSpecCompiled.GroupByExpressions.GroupByNodes.Length > 0)
                    {
                        if (HavingExpr != null)
                        {
                            _evalStrategy = new SubselectEvalStrategyRowUnfilteredSelectedGroupedWHaving(HavingExpr);
                        }
                        else
                        {
                            _evalStrategy = UNFILTERED_SELECTED_GROUPED;
                        }
                    }
                    else
                    {
                        if (HavingExpr != null)
                        {
                            _evalStrategy = HAVING_SELECTED;
                        }
                        else
                        {
                            _evalStrategy = UNFILTERED_SELECTED;
                        }
                    }
                }
            }
            else
            {
                if (SelectClause == null)
                {
                    var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(RawEventType);
                    if (tableMetadata != null)
                    {
                        _evalStrategy = new SubselectEvalStrategyRowFilteredUnselectedTable(tableMetadata);
                    }
                    else
                    {
                        _evalStrategy = FILTERED_UNSELECTED;
                    }
                }
                else
                {
                    _evalStrategy = FILTERED_SELECTED;
                }
            }
        }

        public override Object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return null;
            }
            return _evalStrategy.Evaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
        }

        public override ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context)
        {
            if (matchingEvents == null)
            {
                return null;
            }
            if (matchingEvents.Count == 0)
            {
                return Collections.GetEmptyList<EventBean>();
            }
            return _evalStrategy.EvaluateGetCollEvents(eventsPerStream, isNewData, matchingEvents, context, this);
        }

        public override ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context)
        {
            if (matchingEvents == null)
            {
                return null;
            }
            if (matchingEvents.Count == 0)
            {
                return Collections.GetEmptyList<object>();
            }
            return _evalStrategy.EvaluateGetCollScalar(eventsPerStream, isNewData, matchingEvents, context, this);
        }

        public override EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return null;
            }
            return _evalStrategy.EvaluateGetEventBean(
                eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
        }

        public override Object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {

            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return null;
            }
            return _evalStrategy.TypableEvaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
        }

        public override Object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (matchingEvents == null)
            {
                return null;
            }
            if (matchingEvents.Count == 0)
            {
                return new Object[0][];
            }
            return _evalStrategy.TypableEvaluateMultirow(
                eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext, this);
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

        public override EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            if (SelectClause == null)
            {
                return null;
            }
            if (this.SubselectAggregationType != SubqueryAggregationType.FULLY_AGGREGATED_NOPROPS)
            {
                return null;
            }
            return GetAssignAnonymousType(eventAdapterService, statementId);
        }

        public override EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            var selectClause = SelectClause;
            if (selectClause == null)
            {
                // wildcards allowed
                return RawEventType;
            }

            // special case: selecting a single property that is itself an event
            if (selectClause.Length == 1 && selectClause[0] is ExprIdentNode)
            {
                var identNode = (ExprIdentNode) selectClause[0];
                var fragment = RawEventType.GetFragmentType(identNode.ResolvedPropertyName);
                if (fragment != null && !fragment.IsIndexed)
                {
                    return fragment.FragmentType;
                }
            }

            // select of a single value otherwise results in a collection of scalar values
            if (selectClause.Length == 1)
            {
                return null;
            }

            // fully-aggregated always returns zero or one row
            if (this.SubselectAggregationType == SubqueryAggregationType.FULLY_AGGREGATED_NOPROPS)
            {
                return null;
            }

            return GetAssignAnonymousType(eventAdapterService, statementId);
        }

        private EventType GetAssignAnonymousType(EventAdapterService eventAdapterService, int statementId)
        {
            IDictionary<string, Object> rowType = RowType;
            var resultEventType =
                eventAdapterService.CreateAnonymousMapType(
                    statementId + "_subquery_" + this.SubselectNumber, rowType, true);
            _subselectMultirowType = new SubselectMultirowType(resultEventType, eventAdapterService);
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
                var type = new LinkedHashMap<string, Object>();

                var selectAsNames = SelectAsNames;
                var selectClause = SelectClause;
                for (var i = 0; i < selectClause.Length; i++)
                {
                    string assignedName = selectAsNames[i];
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

        public string GetMultirowMessage()
        {
            return "Subselect of statement '" + StatementName + "' returned more then one row in subselect " +
                   SubselectNumber + " '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(this) +
                   "', returning null result";
        }

        internal IDictionary<string, Object> EvaluateRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var selectAsNames = SelectAsNames;
            var selectClauseEvaluator = SelectClauseEvaluator;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, context);
            var map = new Dictionary<string, Object>();
            for (var i = 0; i < selectClauseEvaluator.Length; i++)
            {
                var resultEntry = selectClauseEvaluator[i].Evaluate(evaluateParams);
                map.Put(selectAsNames[i], resultEntry);
            }
            return map;
        }
    }

    internal class SubselectMultirowType
    {
        internal SubselectMultirowType(EventType eventType, EventAdapterService eventAdapterService)
        {
            EventType = eventType;
            EventAdapterService = eventAdapterService;
        }

        public EventType EventType { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }
    }
} // end of namespace
