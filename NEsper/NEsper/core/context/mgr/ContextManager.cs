///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.core.context.util;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextManager : FilterFaultHandler
    {
        ContextDescriptor ContextDescriptor { get; }
        int NumNestingLevels { get; }
        ContextStateCache ContextStateCache { get; }

        void AddStatement(ContextControllerStatementBase statement, bool isRecoveringResilient);
        void StopStatement(String statementName, int statementId);
        void DestroyStatement(String statementName, int statementId);
    
        void SafeDestroy();
    
        FilterSpecLookupable GetFilterLookupable(EventType eventType);
    
        ContextStatePathDescriptor ExtractPaths(ContextPartitionSelector contextPartitionSelector);
        ContextStatePathDescriptor ExtractStopPaths(ContextPartitionSelector contextPartitionSelector);
        ContextStatePathDescriptor ExtractDestroyPaths(ContextPartitionSelector contextPartitionSelector);
        void ImportStartPaths(ContextControllerState state, AgentInstanceSelector agentInstanceSelector);
        IDictionary<int, ContextPartitionDescriptor> StartPaths(ContextPartitionSelector contextPartitionSelector);
    
        ICollection<int> GetAgentInstanceIds(ContextPartitionSelector contextPartitionSelector);
        IDictionary<int, ContextControllerStatementDesc> Statements { get; }
    }
}
