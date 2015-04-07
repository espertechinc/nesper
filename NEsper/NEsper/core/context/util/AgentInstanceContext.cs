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
using com.espertech.esper.compat.collections;
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
        private Object _terminationCallbacks;

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
            AgentInstanceScriptContext = agentInstanceScriptContext;

            if (statementContext.IsStatelessSelect)
            {
                ExpressionResultCacheService = statementContext.ExpressionResultCacheServiceSharable;
            }
            else
            {
                ExpressionResultCacheService = new ExpressionResultCacheServiceAgentInstance();
            }

            _terminationCallbacks = null;
        }

        public AgentInstanceFilterProxy AgentInstanceFilterProxy { get; private set; }
        public StatementContext StatementContext { get; private set; }

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle { get; private set; }

        public AgentInstanceScriptContext AgentInstanceScriptContext { get; private set; }

        public TimeProvider TimeProvider
        {
            get { return StatementContext.TimeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService { get; private set; }

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

        public string StatementId
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
                    return (ICollection<StopCallback>) _terminationCallbacks;
                }
                return Collections.SingletonList((StopCallback) _terminationCallbacks);
            }
        }

        public void AddTerminationCallback(StopCallback callback)
        {
            if (_terminationCallbacks == null)
            {
                _terminationCallbacks = callback;
            }
            else if (_terminationCallbacks is ICollection<StopCallback>)
            {
                ((ICollection<StopCallback>) _terminationCallbacks).Add(callback);
            }
            else
            {
                var cb = (StopCallback) _terminationCallbacks;
                var q = new HashSet<StopCallback>();
                q.Add(cb);
                q.Add(callback);
                _terminationCallbacks = q;
            }
        }

        public void RemoveTerminationCallback(StopCallback callback)
        {
            if (_terminationCallbacks == null)
            {
            }
            else if (_terminationCallbacks is ICollection<StopCallback>)
            {
                ((ICollection<StopCallback>) _terminationCallbacks).Remove(callback);
            }
            else if (ReferenceEquals(_terminationCallbacks, callback))
            {
                _terminationCallbacks = null;
            }
        }
    }
}