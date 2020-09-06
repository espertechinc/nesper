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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.dataflow.Runnables
{
    public class GraphSourceRunnable : BaseRunnable,
        DataFlowSignalListener
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly bool _audit;
        private readonly string _dataFlowName;
        private readonly DataFlowSourceOperator _graphSource;
        private readonly string _instanceId;
        private readonly string _operatorName;
        private readonly int _operatorNumber;
        private readonly string _operatorPrettyPrint;
        private readonly EPDataFlowExceptionHandler _optionalExceptionHandler;
        private IList<CompletionListener> _completionListeners;

        public GraphSourceRunnable(
            AgentInstanceContext agentInstanceContext,
            DataFlowSourceOperator graphSource,
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            string operatorPrettyPrint,
            EPDataFlowExceptionHandler optionalExceptionHandler,
            bool audit)
        {
            _agentInstanceContext = agentInstanceContext;
            _graphSource = graphSource;
            _dataFlowName = dataFlowName;
            _instanceId = instanceId;
            _operatorName = operatorName;
            _operatorNumber = operatorNumber;
            _operatorPrettyPrint = operatorPrettyPrint;
            _optionalExceptionHandler = optionalExceptionHandler;
            _audit = audit;
        }

        public bool IsShutdown { get; private set; }

        public void Run()
        {
            try {
                RunLoop();
            }
            catch (ThreadInterruptedException ex) {
                Log.Debug("Interruped runnable: " + ex.Message, ex);
            }
            catch (Exception ex) {
                Log.Error("Exception encountered: " + ex.Message, ex);
                HandleException(ex);
            }

            InvokeCompletionListeners();
        }

        public void Shutdown()
        {
            IsShutdown = true;
        }

        public void ProcessSignal(EPDataFlowSignal signal)
        {
            if (signal is EPDataFlowSignalFinalMarker) {
                IsShutdown = true;
            }
        }

        public void RunSync()
        {
            try {
                RunLoop();
            }
            catch (ThreadInterruptedException ex) {
                Log.Debug("Interruped runnable: " + ex.Message, ex);
                throw;
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Error("Exception encountered: " + ex.Message, ex);
                HandleException(ex);
                throw;
            }
        }

        private void HandleException(Exception ex)
        {
            _optionalExceptionHandler?.Handle(
                new EPDataFlowExceptionContext(
                    _dataFlowName,
                    _operatorName,
                    _operatorNumber,
                    _operatorPrettyPrint,
                    ex));
        }

        private void RunLoop()
        {
            while (true) {
                _agentInstanceContext.AuditProvider.DataflowSource(
                    _dataFlowName,
                    _instanceId,
                    _operatorName,
                    _operatorNumber,
                    _agentInstanceContext);
                _graphSource.Next();

                if (IsShutdown) {
                    break;
                }
            }
        }

        private void InvokeCompletionListeners()
        {
            lock (this) {
                if (_completionListeners != null) {
                    foreach (var listener in _completionListeners) {
                        listener.Invoke();
                    }
                }
            }
        }

        public void AddCompletionListener(CompletionListener completionListener)
        {
            lock (this) {
                if (_completionListeners == null) {
                    _completionListeners = new List<CompletionListener>();
                }

                _completionListeners.Add(completionListener);
            }
        }

        public void Next()
        {
            _agentInstanceContext.AuditProvider.DataflowSource(
                _dataFlowName,
                _instanceId,
                _operatorName,
                _operatorNumber,
                _agentInstanceContext);
            _graphSource.Next();
        }
    }
} // end of namespace