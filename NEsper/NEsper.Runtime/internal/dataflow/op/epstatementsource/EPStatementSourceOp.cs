///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class EPStatementSourceOp : DataFlowSourceOperator, DataFlowOperatorLifecycle, DeploymentStateListener
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatementSourceFactory factory;
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly string statementDeploymentId;
        private readonly string statementName;
        private readonly EPDataFlowEPStatementFilter statementFilter;
        private readonly EPDataFlowIRStreamCollector collector;
        private IDictionary<EPStatement, UpdateListener> listeners = new Dictionary<EPStatement, UpdateListener>();

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        private LinkedBlockingQueue<object> emittables = new LinkedBlockingQueue<object>();

        public EPStatementSourceOp(EPStatementSourceFactory factory, AgentInstanceContext agentInstanceContext, string statementDeploymentId, string statementName, EPDataFlowEPStatementFilter statementFilter, EPDataFlowIRStreamCollector collector)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
            this.statementDeploymentId = statementDeploymentId;
            this.statementName = statementName;
            this.statementFilter = statementFilter;
            this.collector = collector;
        }

        public void Next()
        {
            var next = emittables.Pop();
            if (next is EPDataFlowSignal)
            {
                var signal = (EPDataFlowSignal) next;
                graphContext.SubmitSignal(signal);
            }
            else if (next is PortAndMessagePair)
            {
                var pair = (PortAndMessagePair) next;
                graphContext.SubmitPort(pair.Port, pair.Message);
            }
            else
            {
                graphContext.Submit(next);
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            lock (this) {
                // start observing statement management
                EPRuntimeSPI spi = (EPRuntimeSPI) agentInstanceContext.Runtime;
                spi.DeploymentService.AddDeploymentStateListener(this);

                if (statementDeploymentId != null && statementName != null) {
                    EPStatement stmt = spi.DeploymentService.GetStatement(statementDeploymentId, statementName);
                    if (stmt != null) {
                        AddStatement(stmt);
                    }
                }
                else {
                    string[] deployments = spi.DeploymentService.Deployments;
                    foreach (string deploymentId in deployments) {
                        EPDeployment info = spi.DeploymentService.GetDeployment(deploymentId);
                        if (info == null) {
                            continue;
                        }

                        foreach (EPStatement stmt in info.Statements) {
                            if (statementFilter.Pass(ToContext(stmt))) {
                                AddStatement(stmt);
                            }
                        }
                    }
                }
            }
        }

        public void OnDeployment(DeploymentStateEventDeployed @event)
        {
            foreach (EPStatement stmt in @event.Statements)
            {
                if (statementFilter == null)
                {
                    if (stmt.DeploymentId.Equals(statementDeploymentId) && stmt.Name.Equals(statementName))
                    {
                        AddStatement(stmt);
                    }
                }
                else
                {
                    if (statementFilter.Pass(ToContext(stmt)))
                    {
                        AddStatement(stmt);
                    }
                }
            }
        }

        public void OnUndeployment(DeploymentStateEventUndeployed @event)
        {
            foreach (EPStatement stmt in @event.Statements)
            {
                if (listeners.TryRemove(stmt, out var listener))
                {
                    stmt.RemoveListener(listener);
                }
            }
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            foreach (KeyValuePair<EPStatement, UpdateListener> entry in listeners)
            {
                try
                {
                    entry.Key.RemoveListener(entry.Value);
                }
                catch (Exception ex)
                {
                    log.Debug("Exception encountered removing listener: " + ex.Message, ex);
                    // possible
                }
            }
            listeners.Clear();
        }

        private void AddStatement(EPStatement stmt)
        {
            // statement may be added already
            if (listeners.ContainsKey(stmt))
            {
                return;
            }

            // attach listener
            UpdateListener listener;
            if (collector == null)
            {
                listener = new EmitterUpdateListener(emittables, factory.IsSubmitEventBean);
            }
            else
            {
                LocalEmitter emitterForCollector = new LocalEmitter(emittables);
                listener = new EmitterCollectorUpdateListener(collector, emitterForCollector, factory.IsSubmitEventBean);
            }
            stmt.AddListener(listener);

            // save listener instance
            listeners.Put(stmt, listener);
        }

        private EPDataFlowEPStatementFilterContext ToContext(EPStatement stmt)
        {
            return new EPDataFlowEPStatementFilterContext(stmt.DeploymentId, stmt.Name, stmt);
        }
    }
} // end of namespace