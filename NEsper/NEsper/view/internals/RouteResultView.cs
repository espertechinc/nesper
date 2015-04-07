///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
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
        private readonly RouteResultViewHandler _handler;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isFirst">true for the first-where clause, false for all where-clauses</param>
        /// <param name="eventType">output type</param>
        /// <param name="epStatementHandle">handle</param>
        /// <param name="internalEventRouter">routining output events</param>
        /// <param name="tableStateInstance">The table state instance.</param>
        /// <param name="isNamedWindowInsert">The is named window insert.</param>
        /// <param name="processors">processors for select clauses</param>
        /// <param name="whereClauses">where expressions</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        /// <exception cref="System.ArgumentException">Number of where-clauses and processors does not match</exception>
        public RouteResultView(
            bool isFirst,
            EventType eventType,
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableStateInstance[] tableStateInstance,
            bool[] isNamedWindowInsert,
            ResultSetProcessor[] processors,
            ExprNode[] whereClauses,
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
                _handler = new RouteResultViewHandlerFirst(epStatementHandle, internalEventRouter, tableStateInstance, isNamedWindowInsert, processors, ExprNodeUtility.GetEvaluators(whereClauses), agentInstanceContext);
            }
            else
            {
                _handler = new RouteResultViewHandlerAll(epStatementHandle, internalEventRouter, tableStateInstance, isNamedWindowInsert, processors, ExprNodeUtility.GetEvaluators(whereClauses), agentInstanceContext);
            }
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
                    UpdateChildren(new EventBean[] {bean}, null);
                }
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
}
