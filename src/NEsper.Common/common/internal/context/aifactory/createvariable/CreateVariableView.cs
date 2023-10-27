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
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly StatementAgentInstanceFactoryCreateVariable _parent;
        private readonly VariableReader _reader;

        internal CreateVariableView(
            StatementAgentInstanceFactoryCreateVariable parent,
            AgentInstanceContext agentInstanceContext,
            VariableReader reader)
        {
            _parent = parent;
            _agentInstanceContext = agentInstanceContext;
            _reader = reader;
        }

        public override EventType EventType => _parent.ResultSetProcessorFactoryProvider.ResultEventType;

        public void Update(
            object newValue,
            object oldValue)
        {
            var statementResultService = _agentInstanceContext.StatementResultService;
            if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                var variableName = _reader.MetaData.VariableName;

                IDictionary<string, object> valuesOld = new Dictionary<string, object>();
                valuesOld.Put(variableName, oldValue);
                EventBean eventOld =
                    _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        valuesOld,
                        _parent.StatementEventType);

                IDictionary<string, object> valuesNew = new Dictionary<string, object>();
                valuesNew.Put(variableName, newValue);
                EventBean eventNew =
                    _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        valuesNew,
                        _parent.StatementEventType);

                EventBean[] newDataToPost = { eventNew };
                EventBean[] oldDataToPost = { eventOld };
                Child.Update(newDataToPost, oldDataToPost);
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
            var value = _reader.Value;
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put(_reader.MetaData.VariableName, value);
            EventBean theEvent = _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(values, EventType);
            return EnumerationHelper.SingletonNullable(theEvent);
        }
    }
} // end of namespace