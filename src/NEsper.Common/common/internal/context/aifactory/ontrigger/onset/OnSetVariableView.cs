///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset
{
    /// <summary>
    ///     A view that handles the setting of variables upon receipt of a triggering event.
    ///     <para />
    ///     Variables are updated atomically and thus a separate commit actually updates the
    ///     new variable values, or a rollback if an exception occured during validation.
    /// </summary>
    public class OnSetVariableView : ViewSupport
    {
        private readonly AgentInstanceContext agentInstanceContext;

        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly StatementAgentInstanceFactoryOnTriggerSet factory;

        public OnSetVariableView(
            StatementAgentInstanceFactoryOnTriggerSet factory,
            AgentInstanceContext agentInstanceContext)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
        }

        public override EventType EventType => factory.StatementEventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData == null || newData.Length == 0) {
                return;
            }

            IDictionary<string, object> values = null;
            var statementResultService = agentInstanceContext.StatementResultService;
            var produceOutputEvents = statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic;

            if (produceOutputEvents) {
                values = new Dictionary<string, object>();
            }

            eventsPerStream[0] = newData[newData.Length - 1];
            factory.VariableReadWrite.WriteVariables(eventsPerStream, values, agentInstanceContext);

            if (values != null) {
                var newDataOut = new EventBean[1];
                newDataOut[0] =
                    agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        values,
                        factory.StatementEventType);
                Child.Update(newDataOut, null);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var values = factory.VariableReadWrite.Iterate(
                agentInstanceContext.VariableManagementService,
                agentInstanceContext.AgentInstanceId);
            EventBean theEvent =
                agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(values, factory.StatementEventType);
            yield return theEvent;
        }
    }
} // end of namespace