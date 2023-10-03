///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.createschema
{
    public class StatementAgentInstanceFactoryCreateSchema : StatementAgentInstanceFactory
    {
        private EventType eventType;
        private Viewable viewable;

        public EventType EventType {
            set {
                eventType = value;
                viewable = new ViewableDefaultImpl(value);
            }
        }

        public EventType StatementEventType => viewable.EventType;

        public void StatementCreate(StatementContext value)
        {
            if (eventType.Metadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
                throw new EPException("Unexpected visibility of value " + NameAccessModifier.PRECONFIGURED);
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            return new StatementAgentInstanceFactoryCreateSchemaResult(
                viewable,
                AgentInstanceMgmtCallbackNoAction.INSTANCE,
                agentInstanceContext);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext, agentInstanceId);
        }
    }
} // end of namespace