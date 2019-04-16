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
    public class ZeroDepthStreamNoIterateWAudit : ZeroDepthStreamNoIterate
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly string _filterSpecText;
        private readonly int _streamNumber;
        private readonly bool _subselect;
        private readonly int _subselectNumber;

        public ZeroDepthStreamNoIterateWAudit(
            EventType eventType,
            AgentInstanceContext agentInstanceContext,
            FilterSpecActivatable filterSpec,
            int streamNumber,
            bool subselect,
            int subselectNumber)
            : base(eventType)
        {
            this._agentInstanceContext = agentInstanceContext;
            _filterSpecText = filterSpec.GetFilterText();
            this._streamNumber = streamNumber;
            this._subselect = subselect;
            this._subselectNumber = subselectNumber;
        }

        public override void Insert(EventBean theEvent)
        {
            _agentInstanceContext.AuditProvider.Stream(theEvent, _agentInstanceContext, _filterSpecText);
            _agentInstanceContext.InstrumentationProvider.QFilterActivationStream(
                theEvent.EventType.Name, _streamNumber, _agentInstanceContext, _subselect, _subselectNumber);
            Insert(theEvent);
            _agentInstanceContext.InstrumentationProvider.AFilterActivationStream(
                _agentInstanceContext, _subselect, _subselectNumber);
        }

        public override void Insert(EventBean[] events)
        {
            _agentInstanceContext.AuditProvider.Stream(events, null, _agentInstanceContext, _filterSpecText);
            base.Insert(events);
        }
    }
} // end of namespace