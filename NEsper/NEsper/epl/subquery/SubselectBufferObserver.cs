///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.epl.subquery
{
    /// <summary>
    /// Observer to a buffer that is filled by a subselect view when it posts events,
    /// to be added and removed from indexes.
    /// </summary>
	public class SubselectBufferObserver : BufferObserver
	{
	    private readonly EventTable[] _eventIndex;
        private readonly AgentInstanceContext _agentInstanceContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventIndex">index to Update</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        public SubselectBufferObserver(EventTable[] eventIndex, AgentInstanceContext agentInstanceContext)
        {
            _eventIndex = eventIndex;
            _agentInstanceContext = agentInstanceContext;
        }

        public void NewData(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer)
	    {
            EventBean[] newData = newEventBuffer.GetAndFlush();
            EventBean[] oldData = oldEventBuffer.GetAndFlush();
            foreach (EventTable table in _eventIndex) {
                table.AddRemove(newData, oldData, _agentInstanceContext);
            }
	    }
	}
} // End of namespace
