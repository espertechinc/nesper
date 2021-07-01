///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        public class NamedWindowConsumerPreload : StatementAgentInstancePreload
        {
            private readonly ViewableActivatorNamedWindow _nwActivator;
            private readonly NamedWindowConsumerView _consumer;
            private readonly AgentInstanceContext _agentInstanceContext;
            private readonly JoinPreloadMethod _joinPreloadMethod;

            public NamedWindowConsumerPreload(
                ViewableActivatorNamedWindow nwActivator,
                NamedWindowConsumerView consumer,
                AgentInstanceContext agentInstanceContext,
                JoinPreloadMethod joinPreloadMethod)
            {
                this._nwActivator = nwActivator;
                this._consumer = consumer;
                this._agentInstanceContext = agentInstanceContext;
                this._joinPreloadMethod = joinPreloadMethod;
            }

            public void ExecutePreload()
            {
                if (_nwActivator.NamedWindowContextName != null &&
                    !_nwActivator.NamedWindowContextName.Equals(_agentInstanceContext.StatementContext.ContextName)) {
                    return;
                }

                ICollection<EventBean> snapshot = _consumer.ConsumerCallback.Snapshot(_nwActivator.FilterQueryGraph, _agentInstanceContext.Annotations);

                EventBean[] events;
                if (_consumer.Filter == null) {
                    events = CollectionUtil.ToArrayEvents(snapshot);
                }
                else {
                    IList<EventBean> eventsInWindow = new List<EventBean>(snapshot.Count);
                    ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(snapshot.GetEnumerator(), _consumer.Filter, _agentInstanceContext, eventsInWindow);
                    events = eventsInWindow.ToArray();
                }

                if (events.Length == 0) {
                    return;
                }

                _consumer.Update(events, null);

                if (_joinPreloadMethod != null &&
                    !_joinPreloadMethod.IsPreloading) {
                    _agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable?.Execute();
                }
            }
        }
    }
}