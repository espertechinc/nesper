///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Event stream implementation that does not keep any window by itself of the events coming into the stream,
    ///     however is itself iterable and keeps the last event.
    /// </summary>
    public class ZeroDepthStreamIterableWAudit : ZeroDepthStreamIterable
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly string _filterSpecText;
        private readonly int _streamNumber;
        private readonly bool _subselect;
        private readonly int _subselectNumber;

        public ZeroDepthStreamIterableWAudit(
            EventType eventType,
            AgentInstanceContext agentInstanceContext,
            FilterSpecActivatable filterSpec,
            int streamNumber,
            bool subselect,
            int subselectNumber)
            : base(eventType)
        {
            _agentInstanceContext = agentInstanceContext;
            _filterSpecText = filterSpec.GetFilterText();
            _streamNumber = streamNumber;
            _subselect = subselect;
            _subselectNumber = subselectNumber;
        }

        public override void Insert(EventBean theEvent)
        {
            _agentInstanceContext.AuditProvider.StreamSingle(theEvent, _agentInstanceContext, _filterSpecText);
            _agentInstanceContext.InstrumentationProvider.QFilterActivationStream(
                theEvent.EventType.Name,
                _streamNumber,
                _agentInstanceContext,
                _subselect,
                _subselectNumber);
            base.Insert(theEvent);
            _agentInstanceContext.InstrumentationProvider.AFilterActivationStream(
                _agentInstanceContext,
                _subselect,
                _subselectNumber);
        }

        public override void Insert(EventBean[] events)
        {
            _agentInstanceContext.AuditProvider.StreamMulti(events, null, _agentInstanceContext, _filterSpecText);
            base.Insert(events);
        }
    }
} // end of namespace