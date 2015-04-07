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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByPropMulti : ExprTableEvalStrategyGroupByPropBase
    {
        private readonly ExprEvaluator[] _groupExpr;

        public ExprTableEvalStrategyGroupByPropMulti(ILockable @lock, IDictionary<Object, ObjectArrayBackedEventBean> aggregationState, int propertyIndex, ExprEvaluatorEnumerationGivenEvent optionalEnumEval, ExprEvaluator[] groupExpr)
                : base(@lock, aggregationState, propertyIndex, optionalEnumEval)
        {
            this._groupExpr = groupExpr;
        }
    
        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, exprEvaluatorContext);
            return EvaluateInternal(groupKey, exprEvaluatorContext);
        }

        public override ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, context);
            return EvaluateGetROCollectionEventsInternal(groupKey, context);
        }

        public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, context);
            return EvaluateGetEventBeanInternal(groupKey, context);
        }

        public override ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, context);
            return EvaluateGetROCollectionScalarInternal(groupKey, context);
        }
    }
}
