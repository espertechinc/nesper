///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.common;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorHistorical : ViewableActivator
    {
        private HistoricalEventViewableFactory factory;

        public HistoricalEventViewableFactory Factory {
            set => factory = value;
        }

        public EventType EventType => factory.EventType;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            var viewable = factory.Activate(agentInstanceContext);
            return new ViewableActivationResult(viewable, viewable, null, false, false, null, null, null);
        }
    }
} // end of namespace