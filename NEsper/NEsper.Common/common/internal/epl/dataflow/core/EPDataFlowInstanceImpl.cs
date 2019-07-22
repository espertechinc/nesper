///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.common.@internal.epl.dataflow.runnables;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
    public class EPDataFlowInstanceImpl : EPDataFlowInstance
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly DataflowDesc _dataflowDesc;

        private readonly IDictionary<int, Pair<object, bool>> _operators;
        private readonly IList<GraphSourceRunnable> _sourceRunnables;
        private readonly OperatorStatisticsProvider _statistics;
        private IList<CountDownLatch> _joinedThreadLatches;
        private Thread _runCurrentThread;

        private EPDataFlowState _state;
        private IList<Thread> _threads;

        public EPDataFlowInstanceImpl(
            object dataFlowInstanceUserObject,
            string dataFlowInstanceId,
            OperatorStatisticsProvider statistics,
            IDictionary<int, object> operators,
            IList<GraphSourceRunnable> sourceRunnables,
            DataflowDesc dataflowDesc,
            AgentInstanceContext agentInstanceContext,
            EPDataFlowInstanceStatistics statisticsProvider,
            IDictionary<string, object> parametersURIs)
        {
            UserObject = dataFlowInstanceUserObject;
            InstanceId = dataFlowInstanceId;
            _statistics = statistics;
            _dataflowDesc = dataflowDesc;
            _agentInstanceContext = agentInstanceContext;
            _sourceRunnables = sourceRunnables;
            Statistics = statisticsProvider;
            Parameters = parametersURIs;

            State = EPDataFlowState.INSTANTIATED;

            _operators = new OrderedDictionary<int, Pair<object, bool>>();
            foreach (var entry in operators) {
                _operators.Put(entry.Key, new Pair<object, bool>(entry.Value, false));
            }
        }

        public string DataFlowName => _dataflowDesc.DataflowName;

        public EPDataFlowState State {
            get => _state;
            set {
                _agentInstanceContext.AuditProvider.DataflowTransition(
                    _dataflowDesc.DataflowName,
                    InstanceId,
                    _state,
                    value,
                    _agentInstanceContext);
                _state = value;
            }
        }

        public void Run()
        {
            lock (this) {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();
                var dataFlowName = _dataflowDesc.DataflowName;

                if (_sourceRunnables.Count != 1) {
                    throw new UnsupportedOperationException(
                        "The data flow '" +
                        dataFlowName +
                        "' has zero or multiple sources and requires the use of the start method instead");
                }

                CallOperatorOpen();

                var sourceRunnable = _sourceRunnables[0];
                State = EPDataFlowState.RUNNING;
                _runCurrentThread = Thread.CurrentThread;
                try {
                    sourceRunnable.RunSync();
                }
                catch (ThreadInterruptedException) {
                    CallOperatorClose();
                    State = EPDataFlowState.CANCELLED;
                    throw new EPDataFlowCancellationException(
                        "Data flow '" + dataFlowName + "' execution was cancelled",
                        dataFlowName);
                }
                catch (Exception t) {
                    CallOperatorClose();
                    State = EPDataFlowState.COMPLETE;
                    throw new EPDataFlowExecutionException(
                        "Exception encountered running data flow '" + dataFlowName + "': " + t.Message,
                        t,
                        dataFlowName);
                }

                CallOperatorClose();
                if (_state != EPDataFlowState.CANCELLED) {
                    State = EPDataFlowState.COMPLETE;
                }
            }
        }

        public void Start()
        {
            CheckExecCompleteState();
            CheckExecCancelledState();
            CheckExecRunningState();

            CallOperatorOpen();

            var countdown = new AtomicLong(_sourceRunnables.Count);
            _threads = new List<Thread>();
            for (var i = 0; i < _sourceRunnables.Count; i++) {
                var runnable = _sourceRunnables[i];
                var threadName = "esper." + _dataflowDesc.DataflowName + "-" + i;
                var thread = new Thread(runnable.Run) {
                    Name = threadName,
                    IsBackground = true
                };

                runnable.AddCompletionListener(
                    () => {
                        var remaining = countdown.DecrementAndGet();
                        if (remaining == 0) {
                            Completed();
                        }
                    });

                _threads.Add(thread);
                thread.Start();
            }

            State = EPDataFlowState.RUNNING;
        }

        public EPDataFlowInstanceCaptive StartCaptive()
        {
            lock (this) {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();
                State = EPDataFlowState.RUNNING;

                CallOperatorOpen();

                IDictionary<string, EPDataFlowEmitterOperator> emitters =
                    new Dictionary<string, EPDataFlowEmitterOperator>();
                foreach (var operatorStatePair in _operators.Values) {
                    if (operatorStatePair.First is EPDataFlowEmitterOperator) {
                        var emitterOp = (EPDataFlowEmitterOperator) operatorStatePair.First;
                        emitters.Put(emitterOp.Name, emitterOp);
                    }
                }

                return new EPDataFlowInstanceCaptive(emitters, _sourceRunnables);
            }
        }

        public void Join()
        {
            var dataFlowName = _dataflowDesc.DataflowName;
            if (_state == EPDataFlowState.INSTANTIATED) {
                throw new IllegalStateException(
                    "Data flow '" +
                    dataFlowName +
                    "' instance has not been executed, please use join after start or run");
            }

            if (_state == EPDataFlowState.CANCELLED) {
                throw new IllegalStateException(
                    "Data flow '" + dataFlowName + "' instance has been cancelled and cannot be joined");
            }

            // latch used for non-blocking start
            if (_threads != null) {
                foreach (var thread in _threads) {
                    thread.Join();
                }
            }
            else {
                var latch = new CountDownLatch(1);
                lock (this) {
                    if (_joinedThreadLatches == null) {
                        _joinedThreadLatches = new List<CountDownLatch>();
                    }

                    _joinedThreadLatches.Add(latch);
                }

                if (_state != EPDataFlowState.COMPLETE) {
                    latch.Await();
                }
            }
        }

        public void Cancel()
        {
            if (_state == EPDataFlowState.COMPLETE || _state == EPDataFlowState.CANCELLED) {
                return;
            }

            if (_state == EPDataFlowState.INSTANTIATED) {
                State = EPDataFlowState.CANCELLED;
                _sourceRunnables.Clear();
                CallOperatorClose();
                return;
            }

            // handle async start
            if (_threads != null) {
                foreach (var runnable in _sourceRunnables) {
                    runnable.Shutdown();
                }

                foreach (var thread in _threads) {
                    if (thread.IsAlive) {
                        thread.Interrupt();
                    }
                }
            }
            else {
                // handle run
                if (_runCurrentThread != null) {
                    _runCurrentThread.Interrupt();
                }

                _runCurrentThread = null;
            }

            CallOperatorClose();

            State = EPDataFlowState.CANCELLED;
            _sourceRunnables.Clear();
        }

        public EPDataFlowInstanceStatistics Statistics { get; }

        public object UserObject { get; }

        public string InstanceId { get; }

        public IDictionary<string, object> Parameters { get; }

        public void Completed()
        {
            lock (this) {
                if (_state != EPDataFlowState.CANCELLED) {
                    State = EPDataFlowState.COMPLETE;
                }

                CallOperatorClose();

                if (_joinedThreadLatches != null) {
                    foreach (var joinedThread in _joinedThreadLatches) {
                        joinedThread.CountDown();
                    }
                }
            }
        }

        private void CallOperatorOpen()
        {
            foreach (var opNum in _dataflowDesc.OperatorBuildOrder) {
                var operatorStatePair = _operators.Get(opNum);
                if (operatorStatePair.First is DataFlowOperatorLifecycle) {
                    try {
                        var lf = (DataFlowOperatorLifecycle) operatorStatePair.First;
                        lf.Open(new DataFlowOpOpenContext(opNum));
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        throw new EPDataFlowExecutionException(
                            "Exception encountered opening data flow 'FlowOne' in operator " +
                            operatorStatePair.First.GetType().GetSimpleName() +
                            ": " +
                            ex.Message,
                            ex,
                            _dataflowDesc.DataflowName);
                    }
                }
            }
        }

        private void CallOperatorClose()
        {
            lock (this) {
                foreach (var opNum in _dataflowDesc.OperatorBuildOrder) {
                    var operatorStatePair = _operators.Get(opNum);
                    if (operatorStatePair.First is DataFlowOperatorLifecycle && !operatorStatePair.Second) {
                        try {
                            var lf = (DataFlowOperatorLifecycle) operatorStatePair.First;
                            lf.Close(new DataFlowOpCloseContext(opNum));
                        }
                        catch (EPException) {
                            throw;
                        }
                        catch (Exception ex) {
                            Log.Error(
                                "Exception encountered closing data flow '" +
                                _dataflowDesc.DataflowName +
                                "': " +
                                ex.Message,
                                ex);
                        }

                        operatorStatePair.Second = true;
                    }
                }
            }
        }

        private void CheckExecCompleteState()
        {
            if (_state == EPDataFlowState.COMPLETE) {
                throw new IllegalStateException(
                    "Data flow '" +
                    _dataflowDesc.DataflowName +
                    "' instance has already completed, please use instantiate to run the data flow again");
            }
        }

        private void CheckExecRunningState()
        {
            if (_state == EPDataFlowState.RUNNING) {
                throw new IllegalStateException(
                    "Data flow '" + _dataflowDesc.DataflowName + "' instance is already running");
            }
        }

        private void CheckExecCancelledState()
        {
            if (_state == EPDataFlowState.CANCELLED) {
                throw new IllegalStateException(
                    "Data flow '" +
                    _dataflowDesc.DataflowName +
                    "' instance has been cancelled and cannot be run or started");
            }
        }
    }
} // end of namespace