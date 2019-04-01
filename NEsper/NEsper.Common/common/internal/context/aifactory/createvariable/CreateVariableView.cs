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
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class CreateVariableView : ViewSupport,
        VariableChangeCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly StatementAgentInstanceFactoryCreateVariable parent;
        private readonly VariableReader reader;

        private CreateVariableView(
            StatementAgentInstanceFactoryCreateVariable parent,
            AgentInstanceContext agentInstanceContext,
            VariableReader reader)
        {
            this.parent = parent;
            this.agentInstanceContext = agentInstanceContext;
            this.reader = reader;
        }

        public override EventType EventType => parent.ResultSetProcessorFactoryProvider.ResultEventType;

        public void Update(
            object newValue,
            object oldValue)
        {
            var statementResultService = agentInstanceContext.StatementResultService;
            if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                var variableName = reader.MetaData.VariableName;

                IDictionary<string, object> valuesOld = new Dictionary<string, object>();
                valuesOld.Put(variableName, oldValue);
                EventBean eventOld =
                    agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        valuesOld, parent.StatementEventType);

                IDictionary<string, object> valuesNew = new Dictionary<string, object>();
                valuesNew.Put(variableName, newValue);
                EventBean eventNew =
                    agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        valuesNew, parent.StatementEventType);

                EventBean[] newDataToPost = {eventNew};
                EventBean[] oldDataToPost = {eventOld};
                child.Update(newDataToPost, oldDataToPost);
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // nothing to do
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var value = reader.Value;
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put(reader.MetaData.VariableName, value);
            EventBean theEvent = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(values, EventType);
            return EnumerationHelper.SingletonNullable(theEvent);
        }
    }
} // end of namespace