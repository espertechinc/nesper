///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.runnables;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowInstanceImpl : EPDataFlowInstance
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _engineUri;
        private readonly String _statementName;
        private readonly bool _audit;
        private readonly String _dataFlowName;
        private readonly Object _userObject;
        private readonly String _instanceId;
        private volatile EPDataFlowState _state;
        private readonly IList<GraphSourceRunnable> _sourceRunnables;
        private readonly IDictionary<int, Pair<Object, Boolean>> _operators;
        private readonly ICollection<int> _operatorBuildOrder;
        private readonly EPDataFlowInstanceStatistics _statisticsProvider;
        private readonly IDictionary<String, Object> _parameters;

        private IList<CountDownLatch> _joinedThreadLatches;
        private IList<Thread> _threads;
        private Thread _runCurrentThread;

        public EPDataFlowInstanceImpl(
            String engineURI,
            String statementName,
            bool audit,
            String dataFlowName,
            Object userObject,
            String instanceId,
            EPDataFlowState state,
            IList<GraphSourceRunnable> sourceRunnables,
            IDictionary<int, Object> operators,
            ICollection<int> operatorBuildOrder,
            EPDataFlowInstanceStatistics statisticsProvider,
            IDictionary<String, Object> parameters)
        {
            _engineUri = engineURI;
            _statementName = statementName;
            _audit = audit;
            _dataFlowName = dataFlowName;
            _userObject = userObject;
            _instanceId = instanceId;
            _sourceRunnables = sourceRunnables;
            _operators = new OrderedDictionary<int, Pair<object, bool>>();
            foreach (var entry in operators)
            {
                _operators.Put(entry.Key, new Pair<Object, Boolean>(entry.Value, false));
            }
            _operatorBuildOrder = operatorBuildOrder;
            _statisticsProvider = statisticsProvider;
            SetState(state);
            _parameters = parameters;
        }

        public string DataFlowName
        {
            get { return _dataFlowName; }
        }

        public EPDataFlowState State
        {
            get { return _state; }
        }

        public object UserObject
        {
            get { return _userObject; }
        }

        public string InstanceId
        {
            get { return _instanceId; }
        }

        public IDictionary<string, object> Parameters
        {
            get { return _parameters; }
        }

        public EPDataFlowInstanceCaptive StartCaptive()
        {
            lock (this)
            {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();
                SetState(EPDataFlowState.RUNNING);

                CallOperatorOpen();

                var emitters = new Dictionary<String, Emitter>();
                foreach (var operatorStatePair in _operators.Values)
                {
                    if (operatorStatePair.First is Emitter)
                    {
                        Emitter emitter = (Emitter) operatorStatePair.First;
                        emitters.Put(emitter.Name, emitter);
                    }
                }

                return new EPDataFlowInstanceCaptive(emitters, _sourceRunnables);
            }
        }

        public void Run()
        {
            lock (this)
            {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();

                if (_sourceRunnables.Count != 1)
                {
                    throw new UnsupportedOperationException(
                        "The data flow '" + _dataFlowName +
                        "' has zero or multiple sources and requires the use of the start method instead");
                }

                CallOperatorOpen();

                GraphSourceRunnable sourceRunnable = _sourceRunnables[0];
                SetState(EPDataFlowState.RUNNING);
                _runCurrentThread = Thread.CurrentThread;
                try
                {
                    sourceRunnable.RunSync();
                }
                catch (ThreadInterruptedException)
                {
                    CallOperatorClose();
                    SetState(EPDataFlowState.CANCELLED);
                    throw new EPDataFlowCancellationException(
                        "Data flow '" + _dataFlowName + "' execution was cancelled", _dataFlowName);
                }
                catch (Exception t)
                {
                    CallOperatorClose();
                    SetState(EPDataFlowState.COMPLETE);
                    throw new EPDataFlowExecutionException(
                        "Exception encountered running data flow '" + _dataFlowName + "': " + t.Message, t, _dataFlowName);
                }
                CallOperatorClose();
                if (_state != EPDataFlowState.CANCELLED)
                {
                    SetState(EPDataFlowState.COMPLETE);
                }
            }
        }

        public void Start()
        {
            lock (this)
            {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();

                CallOperatorOpen();

                var countdown = new int[]
                {
                    _sourceRunnables.Count
                };

                _threads = new List<Thread>();
                for (int i = 0; i < _sourceRunnables.Count; i++)
                {
                    var runnable = _sourceRunnables[i];
                    var threadName = "esper." + _dataFlowName + "-" + i;
                    var thread = new Thread(runnable.Run);
                    thread.Name = threadName;
                    thread.IsBackground = true;

                    runnable.AddCompletionListener(
                        () =>
                        {
                            int remaining = Interlocked.Decrement(ref countdown[0]);
                            if (remaining == 0)
                            {
                                Completed();
                            }
                        });
                    _threads.Add(thread);
                }

                SetState(EPDataFlowState.RUNNING);

                _threads.ForEach(t => t.Start());
            }
        }

        public void Join()
        {
            if (_state == EPDataFlowState.INSTANTIATED)
            {
                throw new IllegalStateException(
                    "Data flow '" + _dataFlowName +
                    "' instance has not been executed, please use join after start or run");
            }
            if (_state == EPDataFlowState.CANCELLED)
            {
                throw new IllegalStateException(
                    "Data flow '" + _dataFlowName + "' instance has been cancelled and cannot be joined");
            }

            // latch used for non-blocking start
            if (_threads != null)
            {
                _threads.ForEach(t => t.Join());
            }
            else
            {
                var latch = new CountDownLatch(1);
                lock (this)
                {
                    if (_joinedThreadLatches == null)
                    {
                        _joinedThreadLatches = new List<CountDownLatch>();
                    }
                    _joinedThreadLatches.Add(latch);
                }
                if (_state != EPDataFlowState.COMPLETE)
                {
                    latch.Await();
                }
            }
        }

        public void Cancel()
        {
            if (_state == EPDataFlowState.COMPLETE || _state == EPDataFlowState.CANCELLED)
            {
                return;
            }
            if (_state == EPDataFlowState.INSTANTIATED)
            {
                SetState(EPDataFlowState.CANCELLED);
                _sourceRunnables.Clear();
                CallOperatorClose();
                return;
            }

            // handle async start
            if (_threads != null)
            {
                foreach (GraphSourceRunnable runnable in _sourceRunnables)
                {
                    runnable.Shutdown();
                }
                foreach (Thread thread in _threads)
                {
                    if (thread.IsAlive)
                    {
                        thread.Interrupt();
                    }
                }
            }
                // handle run
            else
            {
                if (_runCurrentThread != null)
                {
                    _runCurrentThread.Interrupt();
                }
                _runCurrentThread = null;
            }

            CallOperatorClose();

            SetState(EPDataFlowState.CANCELLED);
            _sourceRunnables.Clear();
        }

        public void Completed()
        {
            lock (this)
            {
                if (_state != EPDataFlowState.CANCELLED)
                {
                    SetState(EPDataFlowState.COMPLETE);
                }

                CallOperatorClose();

                if (_joinedThreadLatches != null)
                {
                    foreach (CountDownLatch joinedThread in _joinedThreadLatches)
                    {
                        joinedThread.CountDown();
                    }
                }
            }
        }

        public EPDataFlowInstanceStatistics Statistics
        {
            get { return _statisticsProvider; }
        }

        private void CheckExecCompleteState()
        {
            if (_state == EPDataFlowState.COMPLETE)
            {
                throw new IllegalStateException(
                    "Data flow '" + _dataFlowName +
                    "' instance has already completed, please use instantiate to run the data flow again");
            }
        }

        private void CheckExecRunningState()
        {
            if (_state == EPDataFlowState.RUNNING)
            {
                throw new IllegalStateException("Data flow '" + _dataFlowName + "' instance is already running");
            }
        }

        private void CheckExecCancelledState()
        {
            if (_state == EPDataFlowState.CANCELLED)
            {
                throw new IllegalStateException(
                    "Data flow '" + _dataFlowName + "' instance has been cancelled and cannot be run or started");
            }
        }

        private void CallOperatorClose()
        {
            lock (this)
            {
                foreach (int? opNum in _operatorBuildOrder)
                {
                    var operatorStatePair = _operators.Get(opNum.Value);
                    if (operatorStatePair.First is DataFlowOpLifecycle && !operatorStatePair.Second)
                    {
                        try
                        {
                            var lf = (DataFlowOpLifecycle) operatorStatePair.First;
                            lf.Close(new DataFlowOpCloseContext());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(
                                "Exception encountered closing data flow '" + _dataFlowName + "': " + ex.Message, ex);
                        }
                        operatorStatePair.Second = true;
                    }
                }
            }
        }

        private void CallOperatorOpen()
        {
            foreach (int? opNum in _operatorBuildOrder)
            {
                var operatorStatePair = _operators.Get(opNum.Value);
                if (operatorStatePair.First is DataFlowOpLifecycle)
                {
                    try
                    {
                        var lf = (DataFlowOpLifecycle) operatorStatePair.First;
                        lf.Open(new DataFlowOpOpenContext());
                    }
                    catch (Exception ex)
                    {
                        throw new EPDataFlowExecutionException(
                            "Exception encountered opening data flow 'FlowOne' in operator " +
                            operatorStatePair.First.GetType().Name + ": " + ex.Message, ex, _dataFlowName);
                    }
                }
            }
        }

        private void SetState(EPDataFlowState newState)
        {
            if (_audit)
            {
                AuditPath.AuditLog(
                    _engineUri, _statementName, AuditEnum.DATAFLOW_TRANSITION,
                    "dataflow " + _dataFlowName + " instance " + _instanceId + " from state " + _state + " to state " +
                    newState);
            }
            _state = newState;
        }
    }
}
