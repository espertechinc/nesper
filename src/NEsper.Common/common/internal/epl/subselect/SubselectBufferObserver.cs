///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    /// <summary>
    ///     Observer to a buffer that is filled by a subselect view when it posts events,
    ///     to be added and removed from indexes.
    /// </summary>
    public class SubselectBufferObserver : BufferObserver
    {
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly EventTable[] eventIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventIndex">index to update</param>
        /// <param name="exprEvaluatorContext">agent instance context</param>
        public SubselectBufferObserver(
            EventTable[] eventIndex,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            this.eventIndex = eventIndex;
            this.exprEvaluatorContext = exprEvaluatorContext;
        }

        public void NewData(
            int streamId,
            FlushedEventBuffer newEventBuffer,
            FlushedEventBuffer oldEventBuffer)
        {
            var newData = newEventBuffer.GetAndFlush();
            var oldData = oldEventBuffer.GetAndFlush();
            foreach (var table in eventIndex) {
                table.AddRemove(newData, oldData, exprEvaluatorContext);
            }
        }
    }
} // end of namespace