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
        private NamedWindow namedWindow;

        public NamedWindow NamedWindow {
            get => namedWindow;
            set => namedWindow = value;
        }

        public override EventType EventTypeResultSetProcessor => namedWindow.RootView.EventType;

        public override string ContextName => namedWindow.RootView.ContextName;

        public override string ContextDeploymentId =>
            namedWindow.StatementContext.ContextRuntimeDescriptor.ContextDeploymentId;

        public override FireAndForgetInstance ProcessorInstanceNoContext => GetProcessorInstance(null);

        public string NamedWindowOrTableName => namedWindow.Name;

        public override EventType EventTypePublic => namedWindow.RootView.EventType;

        public override StatementContext StatementContext => namedWindow.StatementContext;

        public FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            NamedWindowInstance instance;
            if (agentInstanceContext != null) {
                instance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
            }
            else {
                instance = namedWindow.NamedWindowInstanceNoContext;
            }

            if (instance != null) {
                return new FireAndForgetInstanceNamedWindow(instance);
            }

            return null;
        }

        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            var instance = namedWindow.GetNamedWindowInstance(agentInstanceId);
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