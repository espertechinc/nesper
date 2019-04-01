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

namespace com.espertech.esper.common.@internal.epl.dataflow.runnables
{
    public class GraphSourceRunnable : BaseRunnable,
        DataFlowSignalListener
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext agentInstanceContext;
        private readonly bool audit;
        private readonly string dataFlowName;
        private readonly DataFlowSourceOperator graphSource;
        private readonly string instanceId;
        private readonly string operatorName;
        private readonly int operatorNumber;
        private readonly string operatorPrettyPrint;
        private readonly EPDataFlowExceptionHandler optionalExceptionHandler;
        private IList<CompletionListener> completionListeners;

        public GraphSourceRunnable(
            AgentInstanceContext agentInstanceContext, DataFlowSourceOperator graphSource, string dataFlowName,
            string instanceId, string operatorName, int operatorNumber, string operatorPrettyPrint,
            EPDataFlowExceptionHandler optionalExceptionHandler, bool audit)
        {
            this.agentInstanceContext = agentInstanceContext;
            this.graphSource = graphSource;
            this.dataFlowName = dataFlowName;
            this.instanceId = instanceId;
            this.operatorName = operatorName;
            this.operatorNumber = operatorNumber;
            this.operatorPrettyPrint = operatorPrettyPrint;
            this.optionalExceptionHandler = optionalExceptionHandler;
            this.audit = audit;
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
            catch (EPException) {
                throw;
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
            if (optionalExceptionHandler == null) {
                return;
            }

            optionalExceptionHandler.Handle(
                new EPDataFlowExceptionContext(dataFlowName, operatorName, operatorNumber, operatorPrettyPrint, ex));
        }

        private void RunLoop()
        {
            while (true) {
                agentInstanceContext.AuditProvider.DataflowSource(
                    dataFlowName, instanceId, operatorName, operatorNumber, agentInstanceContext);
                graphSource.Next();

                if (IsShutdown) {
                    break;
                }
            }
        }

        private void InvokeCompletionListeners()
        {
            lock (this) {
                if (completionListeners != null) {
                    foreach (var listener in completionListeners) {
                        listener.Completed();
                    }
                }
            }
        }

        public void AddCompletionListener(CompletionListener completionListener)
        {
            lock (this) {
                if (completionListeners == null) {
                    completionListeners = new List<CompletionListener>();
                }

                completionListeners.Add(completionListener);
            }
        }

        public void Next()
        {
            agentInstanceContext.AuditProvider.DataflowSource(
                dataFlowName, instanceId, operatorName, operatorNumber, agentInstanceContext);
            graphSource.Next();
        }
    }
} // end of namespace