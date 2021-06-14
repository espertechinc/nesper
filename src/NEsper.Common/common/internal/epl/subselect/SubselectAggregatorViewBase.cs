///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    /// <summary>
    ///     View handling the insert and remove stream generated by a subselect
    ///     for application to aggregation state.
    /// </summary>
    public abstract class SubselectAggregatorViewBase : ViewSupport
    {
        internal readonly AggregationService aggregationService;
        internal readonly EventBean[] eventsPerStream = new EventBean[1];
        internal readonly ExprEvaluatorContext exprEvaluatorContext;
        internal readonly ExprEvaluator groupKeys;
        internal readonly ExprEvaluator optionalFilterExpr;

        public SubselectAggregatorViewBase(
            AggregationService aggregationService,
            ExprEvaluator optionalFilterExpr,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprEvaluator groupKeys)
        {
            this.aggregationService = aggregationService;
            this.optionalFilterExpr = optionalFilterExpr;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.groupKeys = groupKeys;
        }

        public override EventType EventType => Parent.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        internal bool Filter(bool isNewData)
        {
            var result = optionalFilterExpr.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (result == null) {
                return false;
            }

            return true.Equals(result);
        }

        internal object GenerateGroupKey(bool isNewData)
        {
            return groupKeys.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace