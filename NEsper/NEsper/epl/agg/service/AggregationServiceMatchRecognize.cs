///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>Aggregation result future for use with match recognize. </summary>
    public interface AggregationServiceMatchRecognize : AggregationResultFuture
    {
        /// <summary>Enter a single event row consisting of one or more events per stream (each stream representing a variable). </summary>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="streamId">variable number that is the base</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        void ApplyEnter(EventBean[] eventsPerStream, int streamId, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>Clear current aggregation state. </summary>
        void ClearResults();
    }
}
