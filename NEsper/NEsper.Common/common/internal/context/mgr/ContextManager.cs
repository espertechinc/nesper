///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public interface ContextManager : FilterFaultHandler
    {
        void SetStatementContext(StatementContext value);

        void AddStatement(
            ContextControllerStatementDesc statement,
            bool recovery);

        void StopStatement(ContextControllerStatementDesc statement);

        int CountStatements(Func<StatementContext, Boolean> filter);

        IDictionary<int, ContextControllerStatementDesc> Statements { get; }

        int NumNestingLevels { get; }

        ContextAgentInstanceInfo GetContextAgentInstanceInfo(
            StatementContext statementContextOfStatement,
            int agentInstanceId);

        ContextManagerRealization Realization { get; }

        ContextRuntimeDescriptor ContextRuntimeDescriptor { get; }

        StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(AIRegistryRequirements registryRequirements);

        DataInputOutputSerdeWCollation<object>[] ContextPartitionKeySerdes { get; }

        ContextManagerRealization AllocateNewRealization(AgentInstanceContext agentInstanceContext);

        IDictionary<string, object> GetContextPartitions(int contextPartitionId);

        MappedEventBean GetContextPropertiesEvent(int contextPartitionId);

        ContextPartitionIdentifier GetContextIdentifier(int agentInstanceId);

        ContextPartitionCollection GetContextPartitions(ContextPartitionSelector selector);

        ISet<int> GetContextPartitionIds(ContextPartitionSelector selector);

        void AddListener(ContextPartitionStateListener listener);

        void RemoveListener(ContextPartitionStateListener listener);

        IEnumerator<ContextPartitionStateListener> Listeners { get; }

        void RemoveListeners();

        void DestroyContext();
    }
} // end of namespace