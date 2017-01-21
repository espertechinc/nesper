///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.subquery
{
    public class SubselectAggregatorViewFilteredUngrouped : SubselectAggregatorViewBase
    {
        private readonly ExprNode _filterExprNode;

        public SubselectAggregatorViewFilteredUngrouped(
            AggregationService aggregationService,
            ExprEvaluator optionalFilterExpr,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprEvaluator[] groupKeys,
            ExprNode filterExprNode)
            : base(aggregationService, optionalFilterExpr, exprEvaluatorContext, groupKeys)
        {
            _filterExprNode = filterExprNode;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QSubselectAggregation(_filterExprNode);
            }
            if (newData != null)
            {
                foreach (EventBean theEvent in newData)
                {
                    EventsPerStream[0] = theEvent;
                    bool isPass = Filter(true);
                    if (isPass)
                    {
                        AggregationService.ApplyEnter(EventsPerStream, null, ExprEvaluatorContext);
                    }
                }
            }

            if (oldData != null)
            {
                foreach (EventBean theEvent in oldData)
                {
                    EventsPerStream[0] = theEvent;
                    bool isPass = Filter(false);
                    if (isPass)
                    {
                        AggregationService.ApplyLeave(EventsPerStream, null, ExprEvaluatorContext);
                    }
                }
            }
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().ASubselectAggregation();
            }
        }
    }
}