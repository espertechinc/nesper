///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorNamedWindow : FireAndForgetProcessor
    {
        public NamedWindow NamedWindow { get; set; }

        public override EventType EventTypeResultSetProcessor => NamedWindow.RootView.EventType;

        public override string ContextName => NamedWindow.RootView.ContextName;

        public override string ContextDeploymentId =>
            NamedWindow.StatementContext.ContextRuntimeDescriptor.ContextDeploymentId;

        public override FireAndForgetInstance ProcessorInstanceNoContext => GetProcessorInstance(null);

        public string NamedWindowOrTableName => NamedWindow.Name;

        public override EventType EventTypePublic => NamedWindow.RootView.EventType;

        public override StatementContext StatementContext => NamedWindow.StatementContext;

        public FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            NamedWindowInstance instance;
            if (agentInstanceContext != null) {
                instance = NamedWindow.GetNamedWindowInstance(agentInstanceContext);
            }
            else {
                instance = NamedWindow.NamedWindowInstanceNoContext;
            }

            if (instance != null) {
                return new FireAndForgetInstanceNamedWindow(instance);
            }

            return null;
        }

        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            var instance = NamedWindow.GetNamedWindowInstance(agentInstanceId);
            if (instance != null) {
                return new FireAndForgetInstanceNamedWindow(instance);
            }

            return null;
        }

        public string[][] GetUniqueIndexes(FireAndForgetInstance processorInstance)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace