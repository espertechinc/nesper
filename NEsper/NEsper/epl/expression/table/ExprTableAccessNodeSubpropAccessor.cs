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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableAccessNodeSubpropAccessor
        : ExprTableAccessNode
        , ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        private readonly string _subpropName;
        private readonly ExprNode _aggregateAccessMultiValueNode;
        [NonSerialized] private AggregationMethodFactory _accessorFactory;

        public ExprTableAccessNodeSubpropAccessor(
            string tableName,
            string subpropName,
            ExprNode aggregateAccessMultiValueNode)
            : base(tableName)
        {
            _subpropName = subpropName;
            _aggregateAccessMultiValueNode = aggregateAccessMultiValueNode;
        }

        public ExprAggregateNodeBase AggregateAccessMultiValueNode
        {
            get { return (ExprAggregateNodeBase) _aggregateAccessMultiValueNode; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public Type ReturnType
        {
            get { return _accessorFactory.ResultType; }
        }

        public AggregationAccessor Accessor
        {
            get { return _accessorFactory.Accessor; }
        }

        protected override void ValidateBindingInternal(
            ExprValidationContext validationContext,
            TableMetadata tableMetadata)
        {
            // validate group keys
            ValidateGroupKeys(tableMetadata);
            var column =
                (TableMetadataColumnAggregation) ValidateSubpropertyGetCol(tableMetadata, _subpropName);

            // validate accessor factory i.e. the parameters types and the match to the required state
            if (column.AccessAccessorSlotPair == null)
            {
                throw new ExprValidationException("Invalid combination of aggregation state and aggregation accessor");
            }
            var mfNode =
                (ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode;
            mfNode.ValidatePositionals();
            _accessorFactory = mfNode.ValidateAggregationParamsWBinding(validationContext, column);
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get()
                    .QExprTableSubpropAccessor(this, TableName, _subpropName, _accessorFactory.AggregationExpression);
                var result = Strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                InstrumentationHelper.Get().AExprTableSubpropAccessor(result);
                return result;
            }
            return Strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public string SubpropName
        {
            get { return _subpropName; }
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return
                ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).GetEventTypeCollection(
                    eventAdapterService, statementId);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return Strategy.EvaluateGetROCollectionEvents(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public Type ComponentTypeCollection
        {
            get { return ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).ComponentTypeCollection; }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return Strategy.EvaluateGetROCollectionScalar(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return
                ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).GetEventTypeSingle(
                    eventAdapterService, statementId);
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return Strategy.EvaluateGetEventBean(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer, _subpropName);
            writer.Write(".");
            _aggregateAccessMultiValueNode.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            var that = (ExprTableAccessNodeSubpropAccessor) other;
            if (!_subpropName.Equals(that._subpropName))
            {
                return false;
            }
            return ExprNodeUtility.DeepEquals(_aggregateAccessMultiValueNode, that._aggregateAccessMultiValueNode, false);
        }
    }
} // end of namespace
