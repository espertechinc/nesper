///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.start
{
    public abstract class FireAndForgetProcessor
    {
        public abstract EventType EventTypeResultSetProcessor { get; }
        public abstract string ContextName { get; }
        public abstract FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext);
        public abstract FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId);
        public abstract FireAndForgetInstance GetProcessorInstanceNoContext();
        public abstract ICollection<int> GetProcessorInstancesAll();
        public abstract string NamedWindowOrTableName { get; }
        public abstract bool IsVirtualDataWindow { get; }
        public abstract string[][] GetUniqueIndexes(FireAndForgetInstance processorInstance);
        public abstract EventType EventTypePublic { get; }
    }
}
