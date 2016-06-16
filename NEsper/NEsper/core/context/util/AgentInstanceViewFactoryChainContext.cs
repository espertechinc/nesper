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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.util
{
    public class AgentInstanceViewFactoryChainContext : ExprEvaluatorContext
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private bool _isRemoveStream;
        private readonly Object _previousNodeGetter;
        private readonly ViewUpdatedCollection _priorViewUpdatedCollection;
    
        public AgentInstanceViewFactoryChainContext(AgentInstanceContext agentInstanceContext, bool isRemoveStream, Object previousNodeGetter, ViewUpdatedCollection priorViewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceContext;
            _isRemoveStream = isRemoveStream;
            _previousNodeGetter = previousNodeGetter;
            _priorViewUpdatedCollection = priorViewUpdatedCollection;
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return _agentInstanceContext.AgentInstanceLock; }
        }

        public AgentInstanceContext AgentInstanceContext
        {
            get { return _agentInstanceContext; }
        }

        public AgentInstanceScriptContext AgentInstanceScriptContext
        {
            get { return _agentInstanceContext.AgentInstanceScriptContext; }
        }

        public bool IsRemoveStream
        {
            get { return _isRemoveStream; }
            set { _isRemoveStream = value; }
        }

        public object PreviousNodeGetter
        {
            get { return _previousNodeGetter; }
        }

        public ViewUpdatedCollection PriorViewUpdatedCollection
        {
            get { return _priorViewUpdatedCollection; }
        }

        public StatementContext StatementContext
        {
            get { return _agentInstanceContext.StatementContext; }
        }

        public TimeProvider TimeProvider
        {
            get { return _agentInstanceContext.TimeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return _agentInstanceContext.ExpressionResultCacheService; }
        }

        public int AgentInstanceId
        {
            get { return _agentInstanceContext.AgentInstanceId; }
        }

        public EventBean ContextProperties
        {
            get { return _agentInstanceContext.ContextProperties; }
        }

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle
        {
            get { return _agentInstanceContext.EpStatementAgentInstanceHandle; }
        }

        public ICollection<StopCallback> TerminationCallbacksRO
        {
            get { return _agentInstanceContext.TerminationCallbackRO; }
        }

        public void AddTerminationCallback(Action action) {
            AddTerminationCallback(new ProxyStopCallback(action));
        }

        public void AddTerminationCallback(StopCallback callback) {
            _agentInstanceContext.AddTerminationCallback(callback);
        }

        public void RemoveTerminationCallback(Action action) {
            RemoveTerminationCallback(new ProxyStopCallback(action));
        }

        public void RemoveTerminationCallback(StopCallback callback) {
            _agentInstanceContext.RemoveTerminationCallback(callback);
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext {
            get { return _agentInstanceContext.TableExprEvaluatorContext; }
        }

        public static AgentInstanceViewFactoryChainContext Create(IList<ViewFactory> viewFactoryChain, AgentInstanceContext agentInstanceContext, ViewResourceDelegateVerifiedStream viewResourceDelegate) {
    
            Object previousNodeGetter = null;
            if (viewResourceDelegate.PreviousRequests != null && !viewResourceDelegate.PreviousRequests.IsEmpty()) {
                DataWindowViewWithPrevious factoryFound = EPStatementStartMethodHelperPrevious.FindPreviousViewFactory(viewFactoryChain);
                previousNodeGetter = factoryFound.MakePreviousGetter();
            }
    
            ViewUpdatedCollection priorViewUpdatedCollection = null;
            if (viewResourceDelegate.PriorRequests != null && !viewResourceDelegate.PriorRequests.IsEmpty())
            {
                var priorEventViewFactory = EPStatementStartMethodHelperPrior.FindPriorViewFactory(viewFactoryChain);
                var callbacksPerIndex = viewResourceDelegate.PriorRequests;
                priorViewUpdatedCollection = priorEventViewFactory.MakeViewUpdatedCollection(callbacksPerIndex, agentInstanceContext.AgentInstanceId);
            }
    
            bool removedStream = false;
            if (viewFactoryChain.Count > 1) {
                int countDataWindow = 0;
                foreach (ViewFactory viewFactory in viewFactoryChain) {
                    if (viewFactory is DataWindowViewFactory) {
                        countDataWindow++;
                    }
                }
                removedStream = countDataWindow > 1;
            }
    
            return new AgentInstanceViewFactoryChainContext(agentInstanceContext, removedStream, previousNodeGetter, priorViewUpdatedCollection);
        }

        public string StatementName
        {
            get { return _agentInstanceContext.StatementName; }
        }

        public string EngineURI
        {
            get { return _agentInstanceContext.EngineURI; }
        }

        public int StatementId
        {
            get { return _agentInstanceContext.StatementId; }
        }

        public StatementType? StatementType
        {
            get { return _agentInstanceContext.StatementType; }
        }

        public Object StatementUserObject
        {
            get { return _agentInstanceContext.StatementUserObject; }
        }
    }
}
