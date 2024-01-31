///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubselectAggregatorViewFilteredUngrouped : SubselectAggregatorViewBase
    {
        public SubselectAggregatorViewFilteredUngrouped(
            AggregationService aggregationService,
            ExprEvaluator optionalFilterExpr,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprEvaluator groupKeys)
            : base(
                aggregationService,
                optionalFilterExpr,
                exprEvaluatorContext,
                groupKeys)
        {
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            exprEvaluatorContext.InstrumentationProvider.QSubselectAggregation();

            if (newData != null) {
                foreach (var theEvent in newData) {
                    eventsPerStream[0] = theEvent;
                    var isPass = Filter(true);
                    if (isPass) {
                        aggregationService.ApplyEnter(eventsPerStream, null, exprEvaluatorContext);
                    }
                }
            }

            if (oldData != null) {
                foreach (var theEvent in oldData) {
                    eventsPerStream[0] = theEvent;
                    var isPass = Filter(false);
                    if (isPass) {
                        aggregationService.ApplyLeave(eventsPerStream, null, exprEvaluatorContext);
                    }
                }
            }

            exprEvaluatorContext.InstrumentationProvider.ASubselectAggregation();
        }
    }
} // end of namespace