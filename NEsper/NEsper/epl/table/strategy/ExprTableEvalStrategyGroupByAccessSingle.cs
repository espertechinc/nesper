///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByAccessSingle : ExprTableEvalStrategyGroupByAccessBase
    {
        private readonly ExprEvaluator _groupExpr;

        public ExprTableEvalStrategyGroupByAccessSingle(TableAndLockProviderGrouped provider, AggregationAccessorSlotPair pair, ExprEvaluator groupExpr)
            : base(provider, pair)
        {
            this._groupExpr = groupExpr;
        }
    
        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            object group = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            return EvaluateInternal(group, eventsPerStream, isNewData, exprEvaluatorContext);
        }
    
        public override ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            object group = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            return EvaluateGetROCollectionEventsInternal(group, eventsPerStream, isNewData, context);
        }
    
        public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            object group = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            return EvaluateGetEventBeanInternal(group, eventsPerStream, isNewData, context);
        }
    
        public override ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            object group = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            return EvaluateGetROCollectionScalarInternal(group, eventsPerStream, isNewData, context);
        }
    }
}
