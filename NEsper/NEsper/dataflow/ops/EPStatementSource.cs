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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class EPStatementSource
        : DataFlowSourceOperator
        , DataFlowOpLifecycle
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable 649
        [DataFlowOpParameterAttribute]
        private String _statementName;
        [DataFlowOpParameterAttribute]
        private EPDataFlowEPStatementFilter _statementFilter;
        [DataFlowOpParameterAttribute]
        private EPDataFlowIRStreamCollector _collector;

        [DataFlowContextAttribute]
        private EPDataFlowEmitter _graphContext;
#pragma warning restore 649

        private StatementLifecycleSvc _statementLifecycleSvc;
        private readonly IDictionary<EPStatement, UpdateEventHandler> _listeners =
            new Dictionary<EPStatement, UpdateEventHandler>();
        private readonly IBlockingQueue<Object> _emittables = new LinkedBlockingQueue<Object>();
        private bool _submitEventBean;

        private readonly ILockable _iLock =
            LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IThreadLocal<EPDataFlowIRStreamCollectorContext> _collectorDataTL =
            ThreadLocalManager.Create<EPDataFlowIRStreamCollectorContext>(() => null);

        private readonly EventHandler<StatementLifecycleEvent> _lifeCycleEventHandler;

        public EPStatementSource()
        {
            _lifeCycleEventHandler = Observe;
        }

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            if (context.OutputPorts.Count != 1)
            {
                throw new ArgumentException("EPStatementSource operator requires one output stream but produces " + context.OutputPorts.Count + " streams");
            }

            if (_statementName == null && _statementFilter == null)
            {
                throw new EPException("Failed to find required 'StatementName' or 'statementFilter' parameter");
            }
            if (_statementName != null && _statementFilter != null)
            {
                throw new EPException("Both 'StatementName' or 'statementFilter' parameters were provided, only either one is expected");
            }

            DataFlowOpOutputPort portZero = context.OutputPorts[0];
            if (portZero != null && portZero.OptionalDeclaredType != null && portZero.OptionalDeclaredType.IsWildcard)
            {
                _submitEventBean = true;
            }

            _statementLifecycleSvc = context.ServicesContext.StatementLifecycleSvc;
            return null;
        }

        public void Next()
        {
            var next = _emittables.Pop();
            if (next is EPDataFlowSignal)
            {
                var signal = (EPDataFlowSignal)next;
                _graphContext.SubmitSignal(signal);
            }
            else if (next is PortAndMessagePair)
            {
                var pair = (PortAndMessagePair)next;
                _graphContext.SubmitPort(pair.Port, pair.Message);
            }
            else
            {
                _graphContext.Submit(next);
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            using (_iLock.Acquire())
            {
                // start observing statement management
                _statementLifecycleSvc.LifecycleEvent += _lifeCycleEventHandler;

                if (_statementName != null)
                {
                    EPStatement stmt = _statementLifecycleSvc.GetStatementByName(_statementName);
                    if (stmt != null)
                    {
                        AddStatement(stmt);
                    }
                }
                else
                {
                    String[] statements = _statementLifecycleSvc.StatementNames;
                    foreach (String name in statements)
                    {
                        EPStatement stmt = _statementLifecycleSvc.GetStatementByName(name);
                        if (_statementFilter.Pass(stmt))
                        {
                            AddStatement(stmt);
                        }
                    }
                }
            }
        }

        public void Observe(Object sender, StatementLifecycleEvent theEvent)
        {
            using (_iLock.Acquire())
            {
                EPStatement stmt = theEvent.Statement;
                if (theEvent.EventType == StatementLifecycleEvent.LifecycleEventType.STATECHANGE)
                {
                    if (theEvent.Statement.IsStopped || theEvent.Statement.IsDisposed)
                    {
                        var listener = _listeners.Delete(stmt);
                        if (listener != null)
                        {
                            stmt.Events -= listener;
                        }
                    }
                    if (theEvent.Statement.IsStarted)
                    {
                        if (_statementFilter == null)
                        {
                            if (theEvent.Statement.Name.Equals(_statementName))
                            {
                                AddStatement(stmt);
                            }
                        }
                        else
                        {
                            if (_statementFilter.Pass(stmt))
                            {
                                AddStatement(stmt);
                            }
                        }
                    }
                }
            }
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            foreach (KeyValuePair<EPStatement, UpdateEventHandler> entry in _listeners)
            {
                try
                {
                    entry.Key.Events -= entry.Value;
                }
                catch (Exception ex)
                {
                    Log.Debug("Exception encountered removing listener: " + ex.Message, ex);
                    // possible
                }
            }
            _listeners.Clear();
        }

        private void AddStatement(EPStatement stmt)
        {
            // statement may be added already
            if (_listeners.ContainsKey(stmt))
            {
                return;
            }

            // attach listener
            UpdateEventHandler updateEventHandler;
            if (_collector == null)
            {
                updateEventHandler = new EmitterUpdateListener(
                    _emittables, _submitEventBean).Update;
            }
            else
            {
                var emitterForCollector = new LocalEmitter(_emittables);
                updateEventHandler = new EmitterCollectorUpdateListener(
                    _collector, emitterForCollector, _collectorDataTL, _submitEventBean).Update;
            }
            stmt.Events += updateEventHandler;

            // save listener instance
            _listeners.Put(stmt, updateEventHandler);
        }

        public class EmitterUpdateListener : StatementAwareUpdateListener
        {
            private readonly IBlockingQueue<Object> _queue;
            private readonly bool _submitEventBean;

            public EmitterUpdateListener(IBlockingQueue<Object> queue, bool submitEventBean)
            {
                _queue = queue;
                _submitEventBean = submitEventBean;
            }

            public void Update(Object sender, UpdateEventArgs e)
            {
                Update(
                    e.NewEvents,
                    e.OldEvents,
                    e.Statement,
                    e.ServiceProvider);
            }

            public void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider epServiceProvider)
            {
                if (newEvents != null)
                {
                    foreach (EventBean newEvent in newEvents)
                    {
                        if (_submitEventBean)
                        {
                            _queue.Push(newEvent);
                        }
                        else
                        {
                            Object underlying = newEvent.Underlying;
                            _queue.Push(underlying);
                        }
                    }
                }
            }
        }

        public class EmitterCollectorUpdateListener : StatementAwareUpdateListener
        {
            private readonly EPDataFlowIRStreamCollector _collector;
            private readonly LocalEmitter _emitterForCollector;
            private readonly IThreadLocal<EPDataFlowIRStreamCollectorContext> _collectorDataTL;
            private readonly bool _submitEventBean;

            public EmitterCollectorUpdateListener(EPDataFlowIRStreamCollector collector, LocalEmitter emitterForCollector, IThreadLocal<EPDataFlowIRStreamCollectorContext> collectorDataTL, bool submitEventBean)
            {
                _collector = collector;
                _emitterForCollector = emitterForCollector;
                _collectorDataTL = collectorDataTL;
                _submitEventBean = submitEventBean;
            }

            public void Update(Object sender, UpdateEventArgs e)
            {
                Update(
                    e.NewEvents,
                    e.OldEvents,
                    e.Statement,
                    e.ServiceProvider);
            }

            public void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider epServiceProvider)
            {

                EPDataFlowIRStreamCollectorContext holder = _collectorDataTL.GetOrCreate();
                if (holder == null)
                {
                    holder = new EPDataFlowIRStreamCollectorContext(_emitterForCollector, _submitEventBean, newEvents, oldEvents, statement, epServiceProvider);
                    _collectorDataTL.Value = holder;
                }
                else
                {
                    holder.ServiceProvider = epServiceProvider;
                    holder.Statement = statement;
                    holder.OldEvents = oldEvents;
                    holder.NewEvents = newEvents;
                }

                _collector.Collect(holder);
            }
        }

        public class LocalEmitter : EPDataFlowEmitter
        {
            private readonly IBlockingQueue<Object> _queue;

            public LocalEmitter(IBlockingQueue<Object> queue)
            {
                _queue = queue;
            }

            public void Submit(Object @object)
            {
                _queue.Push(@object);
            }

            public void SubmitSignal(EPDataFlowSignal signal)
            {
                _queue.Push(signal);
            }

            public void SubmitPort(int portNumber, Object @object)
            {
                _queue.Push(@object);
            }
        }

        public class PortAndMessagePair
        {
            public PortAndMessagePair(int port, Object message)
            {
                Port = port;
                Message = message;
            }

            public int Port { get; private set; }

            public object Message { get; private set; }
        }
    }
}
