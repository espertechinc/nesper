///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public abstract class FireAndForgetProcessor
    {
        public abstract EventType EventTypeResultSetProcessor { get; }

        public abstract string ContextName { get; }

        public abstract string ContextDeploymentId { get; }

        public abstract FireAndForgetInstance ProcessorInstanceNoContext { get; }

        public abstract EventType EventTypePublic { get; }

        public abstract StatementContext StatementContext { get; }

        public abstract FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId);
    }
} // end of namespace