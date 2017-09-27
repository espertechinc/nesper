///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// View for processing split-stream syntax.
    /// </summary>
    public class RouteResultView : ViewSupport
    {
        private readonly EventType _eventType;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly RouteResultViewHandler _handler;

        public RouteResultView(
            bool isFirst,
            EventType eventType,
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableStateInstance[] tableStateInstances,
            EPStatementStartMethodOnTriggerItem[] items,
            ResultSetProcessor[] processors,
            ExprEvaluator[] whereClauses,
            AgentInstanceContext agentInstanceContext)
        {
            if (whereClauses.Length != processors.Length)
            {
                throw new ArgumentException("Number of where-clauses and processors does not match");
            }

            _exprEvaluatorContext = agentInstanceContext;
            _eventType = eventType;
            if (isFirst)
            {
                _handler = new RouteResultViewHandlerFirst(
                    epStatementHandle, internalEventRouter, tableStateInstances, items, processors, whereClauses,
                    agentInstanceContext);
            }
            else
            {
                _handler = new RouteResultViewHandlerAll(
                    epStatementHandle, internalEventRouter, tableStateInstances, items, processors, whereClauses,
                    agentInstanceContext);
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (newData == null)
            {
                return;
            }

            foreach (EventBean bean in newData)
            {
                bool isHandled = _handler.Handle(bean, _exprEvaluatorContext);

                if (!isHandled)
                {
                    UpdateChildren(
                        new EventBean[]
                        {
                            bean
                        }, null);
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace