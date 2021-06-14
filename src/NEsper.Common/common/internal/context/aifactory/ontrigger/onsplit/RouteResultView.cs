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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    /// <summary>
    ///     View for processing split-stream syntax.
    /// </summary>
    public class RouteResultView : ViewSupport
    {
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly RouteResultViewHandler handler;

        public RouteResultView(
            bool isFirst,
            EventType eventType,
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableInstance[] tableInstances,
            OnSplitItemEval[] items,
            ResultSetProcessor[] processors,
            AgentInstanceContext agentInstanceContext)
        {
            exprEvaluatorContext = agentInstanceContext;
            EventType = eventType;
            if (isFirst) {
                handler = new RouteResultViewHandlerFirst(
                    epStatementHandle,
                    internalEventRouter,
                    tableInstances,
                    items,
                    processors,
                    agentInstanceContext);
            }
            else {
                handler = new RouteResultViewHandlerAll(
                    epStatementHandle,
                    internalEventRouter,
                    tableInstances,
                    items,
                    processors,
                    agentInstanceContext);
            }
        }

        public override EventType EventType { get; }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData == null) {
                return;
            }

            foreach (var bean in newData) {
                bool isHandled = handler.Handle(bean, exprEvaluatorContext);

                if (!isHandled) {
                    Child.Update(new[] {bean}, null);
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace