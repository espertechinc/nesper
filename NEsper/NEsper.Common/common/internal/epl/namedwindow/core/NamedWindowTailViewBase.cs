///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     This view is hooked into a named window's view chain as the last view and handles dispatching of named window
    ///     insert and remove stream results via <seealso cref="NamedWindowManagementService" /> to consuming statements.
    /// </summary>
    public abstract class NamedWindowTailViewBase : NamedWindowTailView
    {
        internal readonly EventType eventType;
        internal readonly bool isParentBatchWindow;
        internal readonly bool isPrioritized;
        internal readonly NamedWindowDispatchService namedWindowDispatchService;
        internal readonly NamedWindowManagementService namedWindowManagementService;
        internal readonly StatementResultService statementResultService;
        internal readonly ConfigurationRuntimeThreading threadingConfig;
        internal readonly TimeSourceService timeSourceService;

        public NamedWindowTailViewBase(
            EventType eventType,
            bool isParentBatchWindow,
            EPStatementInitServices services)
        {
            this.eventType = eventType;
            namedWindowManagementService = services.NamedWindowManagementService;
            namedWindowDispatchService = services.NamedWindowDispatchService;
            statementResultService = services.StatementResultService;
            isPrioritized = services.RuntimeSettingsService.ConfigurationRuntime.Execution.IsPrioritized;
            this.isParentBatchWindow = isParentBatchWindow;
            threadingConfig = services.RuntimeSettingsService.ConfigurationRuntime.Threading;
            timeSourceService = services.TimeSourceService;
        }

        public NamedWindowManagementService NamedWindowManagementService => namedWindowManagementService;

        public bool IsParentBatchWindow => isParentBatchWindow;

        public EventType EventType => eventType;

        public StatementResultService StatementResultService => statementResultService;

        public bool IsPrioritized => isPrioritized;

        public abstract NamedWindowConsumerLatchFactory MakeLatchFactory();

        public abstract void AddDispatches(
            NamedWindowConsumerLatchFactory latchFactory,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumersInContext,
            NamedWindowDeltaData delta,
            AgentInstanceContext agentInstanceContext);

        public abstract NamedWindowConsumerView AddConsumerNoContext(NamedWindowConsumerDesc consumerDesc);
        public abstract void RemoveConsumerNoContext(NamedWindowConsumerView namedWindowConsumerView);
    }
} // end of namespace