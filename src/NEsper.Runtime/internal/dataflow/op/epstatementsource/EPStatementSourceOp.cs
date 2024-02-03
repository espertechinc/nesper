///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatementSourceFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly string _statementDeploymentId;
        private readonly string _statementName;
        private readonly EPDataFlowEPStatementFilter _statementFilter;
        private readonly EPDataFlowIRStreamCollector _collector;
        private readonly IDictionary<EPStatement, UpdateListener> _listeners = new Dictionary<EPStatement, UpdateListener>();

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        private LinkedBlockingQueue<object> emittables = new LinkedBlockingQueue<object>();

        public EPStatementSourceOp(EPStatementSourceFactory factory, AgentInstanceContext agentInstanceContext, string statementDeploymentId, string statementName, EPDataFlowEPStatementFilter statementFilter, EPDataFlowIRStreamCollector collector)
        {
            this._factory = factory;
            this._agentInstanceContext = agentInstanceContext;
            this._statementDeploymentId = statementDeploymentId;
            this._statementName = statementName;
            this._statementFilter = statementFilter;
            this._collector = collector;
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
            lock (this)
            {
                // start observing statement management
                var spi = (EPRuntimeSPI) _agentInstanceContext.Runtime;
                spi.DeploymentService.AddDeploymentStateListener(this);

                if (_statementDeploymentId != null && _statementName != null)
                {
                    var stmt = spi.DeploymentService.GetStatement(_statementDeploymentId, _statementName);
                    if (stmt != null)
                    {
                        AddStatement(stmt);
                    }
                }
                else
                {
                    var deployments = spi.DeploymentService.Deployments;
                    foreach (var deploymentId in deployments)
                    {
                        var info = spi.DeploymentService.GetDeployment(deploymentId);
                        if (info == null)
                        {
                            continue;
                        }

                        foreach (var stmt in info.Statements)
                        {
                            if (_statementFilter.Pass(ToContext(stmt)))
                            {
                                AddStatement(stmt);
                            }
                        }
                    }
                }
            }
        }

        public void OnDeployment(DeploymentStateEventDeployed @event)
        {
            foreach (var stmt in @event.Statements)
            {
                if (_statementFilter == null)
                {
                    if (stmt.DeploymentId.Equals(_statementDeploymentId) && stmt.Name.Equals(_statementName))
                    {
                        AddStatement(stmt);
                    }
                }
                else
                {
                    if (_statementFilter.Pass(ToContext(stmt)))
                    {
                        AddStatement(stmt);
                    }
                }
            }
        }

        public void OnUndeployment(DeploymentStateEventUndeployed @event)
        {
            foreach (var stmt in @event.Statements)
            {
                if (_listeners.TryRemove(stmt, out var listener))
                {
                    stmt.RemoveListener(listener);
                }
            }
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            foreach (var entry in _listeners)
            {
                try
                {
                    entry.Key.RemoveListener(entry.Value);
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
            UpdateListener listener;
            if (_collector == null)
            {
                listener = new EmitterUpdateListener(emittables, _factory.IsSubmitEventBean);
            }
            else
            {
                var emitterForCollector = new LocalEmitter(emittables);
                listener = new EmitterCollectorUpdateListener(_collector, emitterForCollector, _factory.IsSubmitEventBean);
            }
            stmt.AddListener(listener);

            // save listener instance
            _listeners.Put(stmt, listener);
        }

        private EPDataFlowEPStatementFilterContext ToContext(EPStatement stmt)
        {
            return new EPDataFlowEPStatementFilterContext(stmt.DeploymentId, stmt.Name, stmt);
        }
    }
} // end of namespace