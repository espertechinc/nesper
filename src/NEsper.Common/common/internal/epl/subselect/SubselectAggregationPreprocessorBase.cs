///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public abstract class SubselectAggregationPreprocessorBase
    {
        internal readonly AggregationService aggregationService;
        internal readonly ExprEvaluator filterEval;
        internal readonly ExprEvaluator groupKeys;

        public SubselectAggregationPreprocessorBase(
            AggregationService aggregationService,
            ExprEvaluator filterEval,
            ExprEvaluator groupKeys)
        {
            this.aggregationService = aggregationService;
            this.filterEval = filterEval;
            this.groupKeys = groupKeys;
        }

        public abstract void Evaluate(
            EventBean[] eventsPerStream,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext);

        protected object GenerateGroupKey(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return groupKeys.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace