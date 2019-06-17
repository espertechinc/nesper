///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    public class EPStatementImpl : EPStatementSPI
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(EPStatementImpl));

        private readonly EPStatementListenerSet statementListenerSet = new EPStatementListenerSet();

        protected readonly StatementContext statementContext;
        protected readonly UpdateDispatchView dispatchChildView;
        protected readonly StatementResultServiceImpl statementResultService;
        protected StatementDestroyCallback stopCallback;
        protected Viewable parentView;
        protected bool destroyed;

        public EPStatementImpl(EPStatementFactoryArgs args)
        {
            this.statementContext = args.StatementContext;
            this.dispatchChildView = args.DispatchChildView;
            this.statementResultService = args.StatementResultService;
            this.statementResultService.SetUpdateListeners(statementListenerSet, false);
        }

        public void AddListener(UpdateListener listener)
        {
            if (listener == null) {
                throw new ArgumentException("Null listener reference supplied");
            }

            CheckDestroyed();
            statementListenerSet.AddListener(listener);
            statementResultService.SetUpdateListeners(statementListenerSet, false);
        }

        public StatementDestroyCallback DestroyCallback {
            get => stopCallback;
            set => stopCallback = value;
        }

        public int StatementId {
            get => statementContext.StatementId;
        }

        public StatementContext StatementContext {
            get => statementContext;
        }

        public string Name {
            get => statementContext.StatementName;
        }

        public UpdateDispatchView DispatchChildView {
            get => dispatchChildView;
        }

        public bool IsDestroyed {
            get => destroyed;
        }

        public void RecoveryUpdateListeners(EPStatementListenerSet listenerSet)
        {
            statementListenerSet.SetListeners(listenerSet);
            statementResultService.SetUpdateListeners(listenerSet, true);
        }

        public EventType EventType {
            get => dispatchChildView.EventType;
        }

        public Attribute[] Annotations {
            get => statementContext.Annotations;
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            CheckDestroyed();
            // Return null if not started
            statementContext.VariableManagementService.SetLocalVersion();
            if (destroyed || parentView == null) {
                return null;
            }

            IEnumerator<EventBean> theIterator;
            if (statementContext.ContextRuntimeDescriptor != null) {
                theIterator = statementContext.ContextRuntimeDescriptor.IteratorHandler.GetEnumerator(statementContext.StatementId);
            }
            else {
                theIterator = parentView.GetEnumerator();
            }

            if (statementContext.EpStatementHandle.HasTableAccess) {
                return new UnsafeEnumeratorWTableImpl<EventBean>(statementContext.TableExprEvaluatorContext, theIterator);
            }

            return theIterator;
        }

        public SafeEnumerator<EventBean> GetSafeEnumerator()
        {
            CheckDestroyed();
            // Return null if not started
            if (parentView == null) {
                return null;
            }

            if (statementContext.ContextRuntimeDescriptor != null) {
                statementContext.VariableManagementService.SetLocalVersion();
                return statementContext.ContextRuntimeDescriptor.IteratorHandler.GetSafeEnumerator(statementContext.StatementId);
            }

            // Set variable version and acquire the lock first
            StatementResourceHolder holder = statementContext.StatementCPCacheService.StatementResourceService.ResourcesUnpartitioned;
            StatementAgentInstanceLock @lock = holder.AgentInstanceContext.AgentInstanceLock;
            @lock.AcquireReadLock();
            try {
                statementContext.VariableManagementService.SetLocalVersion();

                // Provide iterator - that iterator MUST be closed else the lock is not released
                if (statementContext.EpStatementHandle.HasTableAccess) {
                    return new SafeEnumeratorWTableImpl<EventBean>(@lock, parentView.GetEnumerator(), statementContext.TableExprEvaluatorContext);
                }

                return new SafeEnumeratorImpl<EventBean>(@lock, parentView.GetEnumerator());
            }
            catch (Exception) {
                @lock.ReleaseReadLock();
                throw;
            }
        }

        public void RemoveListener(UpdateListener listener)
        {
            if (listener == null) {
                throw new ArgumentException("Null listener reference supplied");
            }

            statementListenerSet.RemoveListener(listener);
            statementResultService.SetUpdateListeners(statementListenerSet, true);
        }

        public void RemoveAllListeners()
        {
            statementListenerSet.RemoveAllListeners();
            statementResultService.SetUpdateListeners(statementListenerSet, true);
        }

        public IEnumerator<UpdateListener> UpdateListeners {
            get => Collections.List(statementListenerSet.Listeners).GetEnumerator();
        }

        public Viewable ParentView {
            get => parentView;
            set {
                if (value == null) {
                    if (parentView != null) {
                        parentView.Child = null;
                    }

                    parentView = null;
                }
                else {
                    parentView = value;
                    parentView.Child = dispatchChildView;
                }
            }
        }

        public void SetDestroyed()
        {
            this.destroyed = true;
        }

        public IEnumerator<EventBean> GetEnumerator(ContextPartitionSelector selector)
        {
            CheckDestroyed();
            if (statementContext.ContextRuntimeDescriptor == null) {
                throw GetUnsupportedNonContextEnumerator();
            }

            if (selector == null) {
                throw new ArgumentException("No selector provided");
            }

            // Return null if not started
            statementContext.VariableManagementService.SetLocalVersion();
            if (parentView == null) {
                return null;
            }

            return statementContext.ContextRuntimeDescriptor.IteratorHandler.GetEnumerator(statementContext.StatementId, selector);
        }

        public SafeEnumerator<EventBean> GetSafeEnumerator(ContextPartitionSelector selector)
        {
            CheckDestroyed();
            if (statementContext.ContextRuntimeDescriptor == null) {
                throw GetUnsupportedNonContextEnumerator();
            }

            if (selector == null) {
                throw new ArgumentException("No selector provided");
            }

            // Return null if not started
            if (parentView == null) {
                return null;
            }

            statementContext.VariableManagementService.SetLocalVersion();
            return statementContext.ContextRuntimeDescriptor.IteratorHandler.GetSafeEnumerator(statementContext.StatementId, selector);
        }

        public string DeploymentId {
            get => statementContext.DeploymentId;
        }

        public void SetSubscriber(object subscriber)
        {
            CheckAllowSubscriber();
            CheckDestroyed();
            SetSubscriber(subscriber, null);
        }

        public void SetSubscriber(
            object subscriber,
            string methodName)
        {
            CheckAllowSubscriber();
            CheckDestroyed();
            statementListenerSet.SetSubscriber(subscriber, methodName);
            statementResultService.SetUpdateListeners(statementListenerSet, false);
        }

        public object Subscriber {
            get => statementListenerSet.Subscriber;
        }

        public object GetProperty(StatementProperty field)
        {
            if (field == StatementProperty.STATEMENTTYPE) {
                return statementContext.StatementType;
            }

            return statementContext.StatementInformationals.Properties.Get(field);
        }

        public void AddListenerWithReplay(UpdateListener listener)
        {
            if (listener == null) {
                throw new ArgumentException("Null listener reference supplied");
            }

            CheckDestroyed();
            if (statementContext.StatementInformationals.OptionalContextName != null) {
                throw new EPException("Operation is not available for use with contexts");
            }

            EPRuntime runtime = (EPRuntime) statementContext.Runtime;
            StatementResourceHolder holder = statementContext.StatementCPCacheService.StatementResourceService.ResourcesUnpartitioned;
            StatementAgentInstanceLock @lock = holder.AgentInstanceContext.AgentInstanceLock;
            @lock.AcquireReadLock();

            try {
                // Add listener - listener not receiving events from this statement, as the statement is locked
                statementListenerSet.AddListener(listener);
                this.statementResultService.SetUpdateListeners(statementListenerSet, false);

                IEnumerator<EventBean> it = GetEnumerator();
                if (it == null) {
                    try {
                        listener.Update(null, null, this, runtime);
                    }
                    catch (Exception ex) {
                        string message = string.Format(
                            "Unexpected exception invoking listener update method for replay on listener class '{0}' : {1} : {2}",
                            listener.GetType().Name, ex.GetType().Name, ex.Message);
                        Log.Error(message, ex);
                    }

                    return;
                }

                List<EventBean> events = new List<EventBean>();
                while (it.MoveNext()) {
                    events.Add(it.Current);
                }

                if (events.IsEmpty()) {
                    try {
                        listener.Update(null, null, this, runtime);
                    }
                    catch (Exception ex) {
                        string message = string.Format(
                            "Unexpected exception invoking listener update method for replay on listener class '{0}' : {1} : {2}",
                            listener.GetType().Name, ex.GetType().Name, ex.Message);
                        Log.Error(message, ex);
                    }
                }
                else {
                    EventBean[] iteratorResult = events.ToArray();
                    try {
                        listener.Update(iteratorResult, null, this, runtime);
                    }
                    catch (Exception ex) {
                        string message = string.Format(
                            "Unexpected exception invoking listener update method for replay on listener class '{0}' : {1} : {2}",
                            listener.GetType().Name, ex.GetType().Name, ex.Message);
                        Log.Error(message, ex);
                    }
                }
            }
            finally {
                if (statementContext.EpStatementHandle.HasTableAccess) {
                    statementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                @lock.ReleaseReadLock();
            }
        }

        public object UserObjectCompileTime {
            get => statementContext.StatementInformationals.UserObjectCompileTime;
        }

        public object UserObjectRuntime {
            get => statementContext.UserObjectRuntime;
        }

        private UnsupportedOperationException GetUnsupportedNonContextEnumerator()
        {
            return new UnsupportedOperationException("Enumerator with context selector is only supported for statements under context");
        }

        protected void CheckDestroyed()
        {
            if (destroyed) {
                throw new IllegalStateException("Statement has already been undeployed");
            }
        }

        private void CheckAllowSubscriber()
        {
            if (!statementContext.StatementInformationals.IsAllowSubscriber) {
                throw new EPSubscriberException(
                    "Setting a subscriber is not allowed for the statement, the statement has been compiled with allowSubscriber=false");
            }
        }
    }
} // end of namespace