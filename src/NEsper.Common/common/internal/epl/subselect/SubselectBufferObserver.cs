///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
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
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly EventTable[] _eventIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventIndex">index to update</param>
        /// <param name="exprEvaluatorContext">expression evaluator context</param>
        public SubselectBufferObserver(
            EventTable[] eventIndex,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventIndex = eventIndex;
            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public void NewData(
            int streamId,
            FlushedEventBuffer newEventBuffer,
            FlushedEventBuffer oldEventBuffer)
        {
            var newData = newEventBuffer.GetAndFlush();
            var oldData = oldEventBuffer.GetAndFlush();
            foreach (var table in _eventIndex) {
                table.AddRemove(newData, oldData, _exprEvaluatorContext);
            }
        }
    }
} // end of namespace