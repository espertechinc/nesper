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

        private readonly AgentInstanceContext agentInstanceContext;
        private readonly DataflowDesc dataflowDesc;

        private readonly IDictionary<int, Pair<object, bool>> operators;
        private readonly IList<GraphSourceRunnable> sourceRunnables;
        private readonly OperatorStatisticsProvider statistics;
        private IList<CountDownLatch> joinedThreadLatches;
        private Thread runCurrentThread;

        private EPDataFlowState state;
        private IList<Thread> threads;

        public EPDataFlowInstanceImpl(
            object dataFlowInstanceUserObject, string dataFlowInstanceId, OperatorStatisticsProvider statistics,
            IDictionary<int, object> operators, IList<GraphSourceRunnable> sourceRunnables, DataflowDesc dataflowDesc,
            AgentInstanceContext agentInstanceContext, EPDataFlowInstanceStatistics statisticsProvider,
            IDictionary<string, object> parametersURIs)
        {
            UserObject = dataFlowInstanceUserObject;
            InstanceId = dataFlowInstanceId;
            this.statistics = statistics;
            this.dataflowDesc = dataflowDesc;
            this.agentInstanceContext = agentInstanceContext;
            this.sourceRunnables = sourceRunnables;
            Statistics = statisticsProvider;
            Parameters = parametersURIs;

            State = EPDataFlowState.INSTANTIATED;

            this.operators = new OrderedDictionary<int, Pair<object, bool>>();
            foreach (var entry in operators) {
                this.operators.Put(entry.Key, new Pair<object, bool>(entry.Value, false));
            }
        }

        public string DataFlowName => dataflowDesc.DataflowName;

        public EPDataFlowState State {
            get => state;
            set {
                agentInstanceContext.AuditProvider.DataflowTransition(
                    dataflowDesc.DataflowName, InstanceId, state, value, agentInstanceContext);
                state = value;
            }
        }

        public void Run()
        {
            lock (this) {
                CheckExecCompleteState();
                CheckExecCancelledState();
                CheckExecRunningState();
                var dataFlowName = dataflowDesc.DataflowName;

                if (sourceRunnables.Count != 1) {
                    throw new UnsupportedOperationException(
                        "The data flow '" + dataFlowName +
                        "' has zero or multiple sources and requires the use of the start method instead");
                }

                CallOperatorOpen();

                GraphSourceRunnable sourceRunnable = sourceRunnables.Get(0);
                State = EPDataFlowState.RUNNING;
                runCurrentThread = Thread.CurrentThread();
                try {
                    sourceRunnable.RunSync();
                }
                catch (ThreadInterruptedException ex) {
                    CallOperatorClose();
                    State = EPDataFlowState.CANCELLED;
                    throw new EPDataFlowCancellationException(
                        "Data flow '" + dataFlowName + "' execution was cancelled", dataFlowName);
                }
                catch (Throwable t) {
                    CallOperatorClose();
                    State = EPDataFlowState.COMPLETE;
                    throw new EPDataFlowExecutionException(
                        "Exception encountered running data flow '" + dataFlowName + "': " + t.Message, t,
                        dataFlowName);
                }

                CallOperatorClose();
                if (state != EPDataFlowState.CANCELLED) {
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

            var countdown = new AtomicLong(sourceRunnables.Count);
            threads = new List<Thread>();
            for (var i = 0; i < sourceRunnables.Count; i++) {
                var runnable = sourceRunnables[i];
                var threadName = "esper." + dataflowDesc.DataflowName + "-" + i;
                var thread = new Thread(runnable, threadName);
                thread.IsBackground = true;

                runnable.AddCompletionListener(
                    () => {
                        var remaining = countdown.DecrementAndGet();
                        if (remaining == 0) {
                            Completed();
                        }
                    });

                threads.Add(thread);
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
                foreach (var operatorStatePair in operators.Values) {
                    if (operatorStatePair.First is EPDataFlowEmitterOperator) {
                        var emitterOp = (EPDataFlowEmitterOperator) operatorStatePair.First;
                        emitters.Put(emitterOp.Name, emitterOp);
                    }
                }

                return new EPDataFlowInstanceCaptive(emitters, sourceRunnables);
            }
        }

        public void Join()
        {
            var dataFlowName = dataflowDesc.DataflowName;
            if (state == EPDataFlowState.INSTANTIATED) {
                throw new IllegalStateException(
                    "Data flow '" + dataFlowName +
                    "' instance has not been executed, please use join after start or run");
            }

            if (state == EPDataFlowState.CANCELLED) {
                throw new IllegalStateException(
                    "Data flow '" + dataFlowName + "' instance has been cancelled and cannot be joined");
            }

            // latch used for non-blocking start
            if (threads != null) {
                foreach (var thread in threads) {
                    thread.Join();
                }
            }
            else {
                var latch = new CountDownLatch(1);
                lock (this) {
                    if (joinedThreadLatches == null) {
                        joinedThreadLatches = new List<CountDownLatch>();
                    }

                    joinedThreadLatches.Add(latch);
                }

                if (state != EPDataFlowState.COMPLETE) {
                    latch.Await();
                }
            }
        }

        public void Cancel()
        {
            if (state == EPDataFlowState.COMPLETE || state == EPDataFlowState.CANCELLED) {
                return;
            }

            if (state == EPDataFlowState.INSTANTIATED) {
                State = EPDataFlowState.CANCELLED;
                sourceRunnables.Clear();
                CallOperatorClose();
                return;
            }

            // handle async start
            if (threads != null) {
                foreach (var runnable in sourceRunnables) {
                    runnable.Shutdown();
                }

                foreach (var thread in threads) {
                    if (thread.IsAlive && !thread.IsInterrupted) {
                        thread.Interrupt();
                    }
                }
            }
            else {
                // handle run
                if (runCurrentThread != null) {
                    runCurrentThread.Interrupt();
                }

                runCurrentThread = null;
            }

            CallOperatorClose();

            State = EPDataFlowState.CANCELLED;
            sourceRunnables.Clear();
        }

        public EPDataFlowInstanceStatistics Statistics { get; }

        public object UserObject { get; }

        public string InstanceId { get; }

        public IDictionary<string, object> Parameters { get; }

        public void Completed()
        {
            lock (this) {
                if (state != EPDataFlowState.CANCELLED) {
                    State = EPDataFlowState.COMPLETE;
                }

                CallOperatorClose();

                if (joinedThreadLatches != null) {
                    foreach (var joinedThread in joinedThreadLatches) {
                        joinedThread.CountDown();
                    }
                }
            }
        }

        private void CallOperatorOpen()
        {
            foreach (int opNum in dataflowDesc.OperatorBuildOrder) {
                var operatorStatePair = operators.Get(opNum);
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
                            operatorStatePair.First.GetType().GetSimpleName() + ": " + ex.Message, ex,
                            dataflowDesc.DataflowName);
                    }
                }
            }
        }

        private void CallOperatorClose()
        {
            lock (this) {
                foreach (int opNum in dataflowDesc.OperatorBuildOrder) {
                    var operatorStatePair = operators.Get(opNum);
                    if (operatorStatePair.First is DataFlowOperatorLifecycle && !operatorStatePair.Second) {
                        try {
                            var lf = (DataFlowOperatorLifecycle) operatorStatePair.First;
                            lf.Close(new DataFlowOpCloseContext(opNum));
                        }
                        catch (EPException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(
                                "Exception encountered closing data flow '" + dataflowDesc.DataflowName + "': " +
                                ex.Message, ex);
                        }

                        operatorStatePair.Second = true;
                    }
                }
            }
        }

        private void CheckExecCompleteState()
        {
            if (state == EPDataFlowState.COMPLETE) {
                throw new IllegalStateException(
                    "Data flow '" + dataflowDesc.DataflowName +
                    "' instance has already completed, please use instantiate to run the data flow again");
            }
        }

        private void CheckExecRunningState()
        {
            if (state == EPDataFlowState.RUNNING) {
                throw new IllegalStateException(
                    "Data flow '" + dataflowDesc.DataflowName + "' instance is already running");
            }
        }

        private void CheckExecCancelledState()
        {
            if (state == EPDataFlowState.CANCELLED) {
                throw new IllegalStateException(
                    "Data flow '" + dataflowDesc.DataflowName +
                    "' instance has been cancelled and cannot be run or started");
            }
        }
    }
} // end of namespace