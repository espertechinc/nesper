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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableExprEvaluatorAccess
        : ExprTableExprEvaluatorBase
            ,
            ExprEvaluator
            ,
            ExprEvaluatorEnumeration
    {
        private readonly AggregationAccessorSlotPair _accessAccessorSlotPair;
        private readonly EventType _eventTypeColl;

        public ExprTableExprEvaluatorAccess(
            ExprNode exprNode,
            string tableName,
            string subpropName,
            int streamNum,
            Type returnType,
            AggregationAccessorSlotPair accessAccessorSlotPair,
            EventType eventTypeColl)
            : base(exprNode, tableName, subpropName, streamNum, returnType)
        {
            _accessAccessorSlotPair = accessAccessorSlotPair;
            _eventTypeColl = eventTypeColl;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTableSubproperty(exprNode, tableName, subpropName); }

            var oa = (ObjectArrayBackedEventBean) eventsPerStream[streamNum];
            var row = ExprTableEvalStrategyUtil.GetRow(oa);
            var result = _accessAccessorSlotPair.Accessor.GetValue(
                row.States[_accessAccessorSlotPair.Slot], eventsPerStream, isNewData, context);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTableSubproperty(result); }

            return result;
        }

        public Type ReturnType
        {
            get { return returnType; }
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return _eventTypeColl;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var oa = (ObjectArrayBackedEventBean) eventsPerStream[streamNum];
            var row = ExprTableEvalStrategyUtil.GetRow(oa);
            return _accessAccessorSlotPair.Accessor.GetEnumerableEvents(
                row.States[_accessAccessorSlotPair.Slot], eventsPerStream, isNewData, context);
        }

        public Type ComponentTypeCollection
        {
            get { return null; }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
