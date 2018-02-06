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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableIdentNodeSubpropAccessor 
        : ExprNodeBase 
        , ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        private readonly int _streamNum;
        private readonly string _optionalStreamName;
        private readonly TableMetadataColumnAggregation _tableAccessColumn;
        private readonly ExprNode _aggregateAccessMultiValueNode;
    
        [NonSerialized]
        private AggregationMethodFactory _accessorFactory;
        [NonSerialized]
        private AggregationAccessor _accessor;
    
        public ExprTableIdentNodeSubpropAccessor(int streamNum, string optionalStreamName, TableMetadataColumnAggregation tableAccessColumn, ExprNode aggregateAccessMultiValueNode)
        {
            _streamNum = streamNum;
            _optionalStreamName = optionalStreamName;
            _tableAccessColumn = tableAccessColumn;
            _aggregateAccessMultiValueNode = aggregateAccessMultiValueNode;
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (_tableAccessColumn.AccessAccessorSlotPair == null) {
                throw new ExprValidationException("Invalid combination of aggregation state and aggregation accessor");
            }

            var mfNode = (ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode;
            mfNode.ValidatePositionals();
            _accessorFactory = mfNode.ValidateAggregationParamsWBinding(validationContext, _tableAccessColumn);
            _accessor = _accessorFactory.Accessor;

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public virtual Type ReturnType
        {
            get { return _accessorFactory.ResultType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationState state = GetState(eventsPerStream);
            if (state == null) {
                return null;
            }
            return _accessor.GetValue(state, new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
        }
    
        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).GetEventTypeCollection(eventAdapterService, statementId);
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            AggregationState state = GetState(evaluateParams.EventsPerStream);
            if (state == null) {
                return null;
            }
            return _accessor.GetEnumerableEvents(state, evaluateParams);
        }

        public Type ComponentTypeCollection
        {
            get { return ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).ComponentTypeCollection; }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            AggregationState state = GetState(evaluateParams.EventsPerStream);
            if (state == null) {
                return null;
            }
            return _accessor.GetEnumerableScalar(state, evaluateParams);
        }
    
        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return ((ExprAggregateAccessMultiValueNode) _aggregateAccessMultiValueNode).GetEventTypeSingle(eventAdapterService, statementId);
        }
    
        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            AggregationState state = GetState(evaluateParams.EventsPerStream);
            if (state == null) {
                return null;
            }
            return _accessor.GetEnumerableEvent(state, evaluateParams);
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (_optionalStreamName != null) {
                writer.Write(_optionalStreamName);
                writer.Write(".");
            }
            writer.Write(_tableAccessColumn.ColumnName);
            writer.Write(".");
            _aggregateAccessMultiValueNode.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) 
        {
            return false;
        }
    
        private AggregationState GetState(EventBean[] eventsPerStream)
        {
            EventBean @event = eventsPerStream[_streamNum];
            if (@event == null)
            {
                return null;
            }
            AggregationRowPair row = ExprTableEvalStrategyUtil.GetRow((ObjectArrayBackedEventBean) @event);
            return row.States[_tableAccessColumn.AccessAccessorSlotPair.Slot];
        }
    }
}
