///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    /// <summary>
    ///     Interface for processors of select-clause items, implementors are computing results based on matching events.
    /// </summary>
    public interface SelectExprProcessor
    {
        /// <summary>
        ///     Computes the select-clause results and returns an event of the result event type that contains, in it's
        ///     properties, the selected items.
        /// </summary>
        /// <param name="eventsPerStream">is per stream the event</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <returns>event with properties containing selected items</returns>
        EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxySelectExprProcessor : SelectExprProcessor
    {
        public delegate EventBean ProcessFunc(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext);

        public ProcessFunc ProcProcess { get; set; }

        public ProxySelectExprProcessor()
        {
        }

        public ProxySelectExprProcessor(ProcessFunc procProcess)
        {
            ProcProcess = procProcess;
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ProcProcess(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
        }
    }
} // end of namespace