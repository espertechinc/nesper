///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Interface for processors of select-clause items, implementors are computing results based on matching events.
    /// </summary>
    public interface SelectExprProcessor
    {
        /// <summary>
        /// Returns the event type that represents the select-clause items.
        /// </summary>
        /// <value>
        /// 	event type representing select-clause items
        /// </value>
        EventType ResultEventType { get; }

        /// <summary>
        /// Computes the select-clause results and returns an event of the result event type that contains, in it's properties, the selected items.
        /// </summary>
        /// <param name="eventsPerStream">is per stream the event</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns>
        /// event with properties containing selected items
        /// </returns>
        EventBean Process(EventBean[] eventsPerStream,
                          bool isNewData,
                          bool isSynthesize,
                          ExprEvaluatorContext exprEvaluatorContext);
    }
}
