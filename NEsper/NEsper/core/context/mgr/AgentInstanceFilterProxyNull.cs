///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class AgentInstanceFilterProxyNull : AgentInstanceFilterProxy
    {
        public static AgentInstanceFilterProxyNull AGENT_INSTANCE_FILTER_PROXY_NULL = new AgentInstanceFilterProxyNull();

        public FilterValueSetParam[][] GetAddendumFilters(FilterSpecCompiled filterSpec)
        {
            return null;
        }
    }
}
