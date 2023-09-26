///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public interface StatementAgentInstanceFactory
    {
        EventType StatementEventType { get; }

        AIRegistryRequirements RegistryRequirements { get; }

        StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient);

        StatementContext StatementCreate { set; }

        void StatementDestroy(StatementContext statementContext);

        void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }
        
        // default void StatementDestroyPreconditions(StatementContext statementContext) { }

        IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId);
    }
} // end of namespace