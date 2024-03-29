///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextAgentInstanceInfo
    {
        public ContextAgentInstanceInfo(
            MappedEventBean contextProperties,
            AgentInstanceFilterProxy filterProxy)
        {
            ContextProperties = contextProperties;
            FilterProxy = filterProxy;
        }

        public MappedEventBean ContextProperties { get; }

        public AgentInstanceFilterProxy FilterProxy { get; }
    }
} // end of namespace