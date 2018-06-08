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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.util
{
    public class AgentInstanceContext : ExprEvaluatorContext
    {
        private readonly MappedEventBean _agentInstanceProperties;
        private StatementContextCPPair _statementContextCPPair;
        private Object _terminationCallbacks;
        private AgentInstanceScriptContext _agentInstanceScriptContext;

        public AgentInstanceContext(
            StatementContext statementContext,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            int agentInstanceId,
            AgentInstanceFilterProxy agentInstanceFilterProxy,
            MappedEventBean agentInstanceProperties,
            AgentInstanceScriptContext agentInstanceScriptContext)
        {
            StatementContext = statementContext;
            EpStatementAgentInstanceHandle = epStatementAgentInstanceHandle;
            AgentInstanceId = agentInstanceId;
            AgentInstanceFilterProxy = agentInstanceFilterProxy;
            _agentInstanceProperties = agentInstanceProperties;
            AllocateAgentInstanceScriptContext = agentInstanceScriptContext;
            _terminationCallbacks = null;
        }

        public IContainer Container {
            get => StatementContext?.Container;
        }

        public AgentInstanceFilterProxy AgentInstanceFilterProxy { get; private set; }
        public StatementContext StatementContext { get; private set; }

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle { get; private set; }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext
        {
            get
            {
                if (_agentInstanceScriptContext == null)
                {
                    _agentInstanceScriptContext = AgentInstanceScriptContext.From(StatementContext.EventAdapterService);
                }

                return _agentInstanceScriptContext;
            }
            private set { _agentInstanceScriptContext = value; }
        }

        public TimeProvider TimeProvider
        {
            get { return StatementContext.TimeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return StatementContext.ExpressionResultCacheServiceSharable; }
        }

        public int AgentInstanceId { get; private set; }

        public EventBean ContextProperties
        {
            get { return _agentInstanceProperties; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return StatementContext.TableExprEvaluatorContext; }
        }

        public string StatementName
        {
            get { return StatementContext.StatementName; }
        }

        public string EngineURI
        {
            get { return StatementContext.EngineURI; }
        }

        public int StatementId
        {
            get { return StatementContext.StatementId; }
        }

        public StatementType? StatementType
        {
            get { return StatementContext.StatementType; }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return EpStatementAgentInstanceHandle.StatementAgentInstanceLock; }
        }

        public Object StatementUserObject
        {
            get { return StatementContext.StatementUserObject; }
        }

        public ICollection<StopCallback> TerminationCallbackRO
        {
            get
            {
                if (_terminationCallbacks == null)
                {
                    return Collections.GetEmptyList<StopCallback>();
                }
                else if (_terminationCallbacks is ICollection<StopCallback>)
                {
                    return (ICollection<StopCallback>)_terminationCallbacks;
                }
                return Collections.SingletonList((StopCallback)_terminationCallbacks);
            }
        }

        public void AddTerminationCallback(Action action)
        {
            AddTerminationCallback(new ProxyStopCallback(action));
        }

        public void AddTerminationCallback(StopCallback callback)
        {
            if (_terminationCallbacks == null)
            {
                _terminationCallbacks = callback;
            }
            else if (_terminationCallbacks is ICollection<StopCallback>)
            {
                ((ICollection<StopCallback>)_terminationCallbacks).Add(callback);
            }
            else
            {
                var cb = (StopCallback)_terminationCallbacks;
                var q = new HashSet<StopCallback>();
                q.Add(cb);
                q.Add(callback);
                _terminationCallbacks = q;
            }
        }

        public void RemoveTerminationCallback(Action action)
        {
            RemoveTerminationCallback(new ProxyStopCallback(action));
        }

        public void RemoveTerminationCallback(StopCallback callback)
        {
            if (_terminationCallbacks == null)
            {
            }
            else if (_terminationCallbacks is ICollection<StopCallback>)
            {
                ((ICollection<StopCallback>)_terminationCallbacks).Remove(callback);
            }
            else if (ReferenceEquals(_terminationCallbacks, callback))
            {
                _terminationCallbacks = null;
            }
        }

        public StatementContextCPPair StatementContextCPPair
        {
            get
            {
                if (_statementContextCPPair == null)
                {
                    _statementContextCPPair = new StatementContextCPPair(
                        StatementContext.StatementId, AgentInstanceId, StatementContext);
                }
                return _statementContextCPPair;
            }
        }
    }
}