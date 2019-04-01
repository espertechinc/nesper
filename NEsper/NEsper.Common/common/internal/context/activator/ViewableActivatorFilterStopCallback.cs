///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorFilterStopCallback : AgentInstanceStopCallback
    {
        private readonly ILockable _lock = new MonitorLock();
        private FilterHandle filterHandle;
        private readonly FilterSpecActivatable filterSpecActivatable;

        public ViewableActivatorFilterStopCallback(
            FilterHandle filterHandle, FilterSpecActivatable filterSpecActivatable)
        {
            this.filterHandle = filterHandle;
            this.filterSpecActivatable = filterSpecActivatable;
        }

        public void Stop(AgentInstanceStopServices services)
        {
            using (_lock.Acquire())
            {
                if (filterHandle != null)
                {
                    FilterValueSetParam[][] addendum = null;
                    var agentInstanceContext = services.AgentInstanceContext;
                    if (agentInstanceContext.AgentInstanceFilterProxy != null)
                    {
                        addendum = agentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(
                            filterSpecActivatable, agentInstanceContext);
                    }

                    FilterValueSetParam[][] filterValues = filterSpecActivatable.GetValueSet(
                        null, addendum, agentInstanceContext, agentInstanceContext.StatementContextFilterEvalEnv);
                    services.AgentInstanceContext.FilterService.Remove(
                        filterHandle, filterSpecActivatable.FilterForEventType, filterValues);
                }

                filterHandle = null;
            }
        }
    }
} // end of namespace