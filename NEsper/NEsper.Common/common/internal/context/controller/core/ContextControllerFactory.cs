///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public interface ContextControllerFactory
    {
        ContextControllerFactoryEnv FactoryEnv { get; set; }

        ContextController Create(ContextManagerRealization contextManagerRealization);

        FilterValueSetParam[][] PopulateFilterAddendum(
            FilterSpecActivatable filterSpec,
            bool forStatement,
            int nestingLevel,
            object partitionKey,
            ContextControllerStatementDesc optionalStatementDesc,
            AgentInstanceContext agentInstanceContextStatement);

        void PopulateContextProperties(
            IDictionary<string, object> props,
            object allPartitionKey);

        StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(AIRegistryRequirements registryRequirements);

        ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey);
    }

    public static class ContextControllerFactoryExtensions
    {
        public static ContextControllerFactory WithFactoryEnv(
            this ContextControllerFactory factory,
            ContextControllerFactoryEnv contextControllerFactoryEnv)
        {
            factory.FactoryEnv = contextControllerFactoryEnv;
            return factory;
        }
    }
} // end of namespace