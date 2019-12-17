///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorTable : FireAndForgetProcessor
    {
        public override EventType EventTypeResultSetProcessor => Table.MetaData.PublicEventType;

        public override string ContextName => Table.MetaData.OptionalContextName;

        public override string ContextDeploymentId =>
            Table.StatementContextCreateTable.ContextRuntimeDescriptor.ContextDeploymentId;

        public override FireAndForgetInstance ProcessorInstanceNoContext => GetProcessorInstance(null);

        public bool IsVirtualDataWindow => throw new UnsupportedOperationException();

        public override EventType EventTypePublic => Table.MetaData.PublicEventType;

        public Table Table { get; set; }

        public override StatementContext StatementContext => Table.StatementContextCreateTable;

        public FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            TableInstance instance;
            if (agentInstanceContext != null) {
                instance = Table.GetTableInstance(agentInstanceContext.AgentInstanceId);
            }
            else {
                instance = Table.TableInstanceNoContext;
            }

            if (instance != null) {
                return new FireAndForgetInstanceTable(instance);
            }

            return null;
        }

        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            var instance = Table.GetTableInstance(agentInstanceId);
            if (instance != null) {
                return new FireAndForgetInstanceTable(instance);
            }

            return null;
        }
    }
} // end of namespace