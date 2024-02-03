///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public abstract class ContextControllerFactoryBase : ContextControllerFactory
    {
        public ContextControllerFactoryEnv FactoryEnv { get; set; }

        public abstract ContextController Create(ContextManagerRealization contextManagerRealization);

        public abstract FilterValueSetParam[][] PopulateFilterAddendum(
            FilterSpecActivatable filterSpec,
            bool forStatement,
            int nestingLevel,
            object partitionKey,
            ContextControllerStatementDesc optionalStatementDesc,
            IDictionary<int, ContextControllerStatementDesc> statements,
            AgentInstanceContext agentInstanceContextStatement);

        public abstract void PopulateContextProperties(
            IDictionary<string, object> props,
            object allPartitionKey);

        public abstract StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements);

        public abstract ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey);
    }
} // end of namespace