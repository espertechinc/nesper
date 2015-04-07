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
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;
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
        [DataFlowOpParameter] private ExprNode filter;
        [DataFlowOpParameter] private EPDataFlowEventBeanCollector collector;

        [DataFlowContext]
        private EPDataFlowEmitter _graphContext;
#pragma warning restore 649

        private EventType _eventType;
        private AgentInstanceContext _agentInstanceContext;
        private EPStatementHandleCallback _callbackHandle;
        private readonly IBlockingQueue<Object> _emittables = new LinkedBlockingQueue<Object>();
        private bool _submitEventBean;

        private readonly ILockable _iLock =
            LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IThreadLocal<EPDataFlowEventBeanCollectorContext> _collectorDataTL =
            ThreadLocalManager.Create<EPDataFlowEventBeanCollectorContext>(() => null);

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
                _submitEventBean = true;
            }
            _eventType = portZero.OptionalDeclaredType.EventType;
            _agentInstanceContext = context.AgentInstanceContext;

            return new DataFlowOpInitializeResult();
        }

        public void Next()
        {
            Object next = _emittables.Pop();
            _graphContext.Submit(next);
        }

        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            if (collector != null)
            {
                var holder = _collectorDataTL.GetOrCreate();
                if (holder == null)
                {
                    holder = new EPDataFlowEventBeanCollectorContext(_graphContext, _submitEventBean, theEvent);
                    _collectorDataTL.Value = holder;
                }
                else
                {
                    holder.Event = theEvent;
                }
                collector.Collect(holder);
            }
            else if (_submitEventBean)
            {
                _emittables.Push(theEvent);
            }
            else
            {
                _emittables.Push(theEvent.Underlying);
            }
        }

        public bool IsSubSelect
        {
            get { return false; }
        }

        public string StatementId
        {
            get { return _agentInstanceContext.StatementId; }
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
                    _eventType, _eventType.Name, filters, null,
                    null, null, 
                    new StreamTypeServiceImpl(
                        _eventType, 
                        _eventType.Name, true, 
                        _agentInstanceContext.EngineURI), 
                    null, 
                    _agentInstanceContext.StatementContext, 
                    new List<int>());
                valueSet = spec.GetValueSet(null, _agentInstanceContext, null);
            }
            catch (ExprValidationException ex)
            {
                throw new EPException("Failed to open filter: " + ex.Message, ex);
            }

            var handle = new EPStatementAgentInstanceHandle(_agentInstanceContext.StatementContext.EpStatementHandle, _agentInstanceContext.AgentInstanceLock, 0, new StatementAgentInstanceFilterVersion());
            _callbackHandle = new EPStatementHandleCallback(handle, this);
            _agentInstanceContext.StatementContext.FilterService.Add(valueSet, _callbackHandle);
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            using (_iLock.Acquire())
            {
                if (_callbackHandle != null)
                {
                    _agentInstanceContext.StatementContext.FilterService.Remove(_callbackHandle);
                    _callbackHandle = null;
                }
            }
        }
    }
}
