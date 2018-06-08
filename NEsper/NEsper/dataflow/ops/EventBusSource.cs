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
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.filter;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class EventBusSource
        : DataFlowSourceOperator
        , DataFlowOpLifecycle
        , FilterHandleCallback
    {
#pragma warning disable 649
        [DataFlowOpParameter] protected ExprNode filter;
        [DataFlowOpParameter] protected EPDataFlowEventBeanCollector collector;

        [DataFlowContext] protected EPDataFlowEmitter graphContext;
#pragma warning restore 649

        protected EventType eventType;
        protected AgentInstanceContext agentInstanceContext;
        protected EPStatementHandleCallback callbackHandle;
        protected FilterServiceEntry filterServiceEntry;
        protected readonly IBlockingQueue<Object> emittables = new LinkedBlockingQueue<Object>();
        protected bool submitEventBean;

        private readonly ILockable _iLock;
        private readonly IThreadLocal<EPDataFlowEventBeanCollectorContext> _collectorDataTL;

        public EventBusSource(
            ILockManager lockManager,
            IThreadLocalManager threadLocalManager)
        {
            _iLock = lockManager.CreateLock(GetType());
            _collectorDataTL = threadLocalManager.Create<EPDataFlowEventBeanCollectorContext>(() => null);
        }

    public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            if (context.OutputPorts.Count != 1)
            {
                throw new ArgumentException("EventBusSource operator requires one output stream but produces " + context.OutputPorts.Count + " streams");
            }

            DataFlowOpOutputPort portZero = context.OutputPorts[0];
            if (portZero.OptionalDeclaredType == null || portZero.OptionalDeclaredType.EventType == null)
            {
                throw new ArgumentException("EventBusSource operator requires an event type declated for the output stream");
            }

            if (!portZero.OptionalDeclaredType.IsUnderlying)
            {
                submitEventBean = true;
            }
            eventType = portZero.OptionalDeclaredType.EventType;
            agentInstanceContext = context.AgentInstanceContext;

            return new DataFlowOpInitializeResult();
        }

        public void Next()
        {
            Object next = emittables.Pop();
            graphContext.Submit(next);
        }

        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            if (collector != null)
            {
                var holder = _collectorDataTL.GetOrCreate();
                if (holder == null)
                {
                    holder = new EPDataFlowEventBeanCollectorContext(graphContext, submitEventBean, theEvent);
                    _collectorDataTL.Value = holder;
                }
                else
                {
                    holder.Event = theEvent;
                }
                collector.Collect(holder);
            }
            else if (submitEventBean)
            {
                emittables.Push(theEvent);
            }
            else
            {
                emittables.Push(theEvent.Underlying);
            }
        }

        public bool IsSubSelect
        {
            get { return false; }
        }

        public int StatementId
        {
            get { return agentInstanceContext.StatementId; }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            FilterValueSet valueSet;
            try
            {
                IList<ExprNode> filters = new ExprNode[0];
                if (filter != null)
                {
                    filters = new ExprNode[] { filter };
                }

                var spec = FilterSpecCompiler.MakeFilterSpec(
                    eventType, eventType.Name, filters, null,
                    null, null, 
                    new StreamTypeServiceImpl(
                        eventType, 
                        eventType.Name, true, 
                        agentInstanceContext.EngineURI), 
                    null, 
                    agentInstanceContext.StatementContext, 
                    new List<int>());
                valueSet = spec.GetValueSet(null, agentInstanceContext, null);
            }
            catch (ExprValidationException ex)
            {
                throw new EPException("Failed to open filter: " + ex.Message, ex);
            }

            var handle = new EPStatementAgentInstanceHandle(
                agentInstanceContext.StatementContext.EpStatementHandle, agentInstanceContext.AgentInstanceLock, 0,
                new StatementAgentInstanceFilterVersion(),
                agentInstanceContext.StatementContext.FilterFaultHandlerFactory);
            callbackHandle = new EPStatementHandleCallback(handle, this);
            filterServiceEntry = agentInstanceContext.StatementContext.FilterService.Add(valueSet, callbackHandle);
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            using (_iLock.Acquire())
            {
                if (callbackHandle != null)
                {
                    agentInstanceContext.StatementContext.FilterService.Remove(callbackHandle, filterServiceEntry);
                    callbackHandle = null;
                    filterServiceEntry = null;
                }
            }
        }
    }
}
