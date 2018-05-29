///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dispatch;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.timer;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement implementation for EPL statements.
    /// </summary>
    public class EPStatementImpl : EPStatementSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatementListenerSet _statementListenerSet;

        private UpdateDispatchViewBase _dispatchChildView;
        private StatementLifecycleSvc _statementLifecycleSvc;

        private Viewable _parentView;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expressionNoAnnotations">expression text witout annotations</param>
        /// <param name="isPattern">is true to indicate this is a pure pattern expression</param>
        /// <param name="dispatchService">for dispatching events to listeners to the statement</param>
        /// <param name="statementLifecycleSvc">handles lifecycle transitions for the statement</param>
        /// <param name="timeLastStateChange">the timestamp the statement was created and started</param>
        /// <param name="isBlockingDispatch">is true if the dispatch to listeners should block to preserve event generation order</param>
        /// <param name="isSpinBlockingDispatch">true to use spin locks blocking to deliver results, as locks are usually uncontended</param>
        /// <param name="msecBlockingTimeout">is the max number of milliseconds of block time</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="statementMetadata">statement metadata</param>
        /// <param name="userObject">the application define user object associated to each statement, if supplied</param>
        /// <param name="statementContext">the statement service context</param>
        /// <param name="isFailed">indicator to start in failed state</param>
        /// <param name="nameProvided">true to indicate a statement name has been provided and is not a system-generated name</param>
        public EPStatementImpl(
            String expressionNoAnnotations,
            bool isPattern,
            DispatchService dispatchService,
            StatementLifecycleSvc statementLifecycleSvc,
            long timeLastStateChange,
            bool isBlockingDispatch,
            bool isSpinBlockingDispatch,
            long msecBlockingTimeout,
            TimeSourceService timeSourceService,
            StatementMetadata statementMetadata,
            Object userObject,
            StatementContext statementContext,
            bool isFailed,
            bool nameProvided)
        {
            _statementLifecycleSvc = statementLifecycleSvc;
            _statementListenerSet = new EPStatementListenerSet();

            IsPattern = isPattern;
            ExpressionNoAnnotations = expressionNoAnnotations;
            StatementContext = statementContext;
            IsNameProvided = nameProvided;

            if (isBlockingDispatch)
            {
                if (isSpinBlockingDispatch)
                {
                    _dispatchChildView = new UpdateDispatchViewBlockingSpin(
                        statementContext.StatementResultService,
                        dispatchService, msecBlockingTimeout,
                        timeSourceService,
                        statementContext.ThreadLocalManager);
                }
                else
                {
                    _dispatchChildView = new UpdateDispatchViewBlockingWait(
                        statementContext.StatementResultService,
                        dispatchService, msecBlockingTimeout,
                        statementContext.ThreadLocalManager);
                }
            }
            else
            {
                _dispatchChildView = new UpdateDispatchViewNonBlocking(
                    statementContext.StatementResultService,
                    dispatchService,
                    statementContext.ThreadLocalManager);
            }

            State = !isFailed ? EPStatementState.STOPPED : EPStatementState.FAILED;
            TimeLastStateChange = timeLastStateChange;
            StatementMetadata = statementMetadata;
            UserObject = userObject;
            statementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
        }

        public void SetListeners(EPStatementListenerSet listenerSet, bool isRecovery)
        {
            _statementListenerSet.Copy(listenerSet);
            StatementContext.StatementResultService.SetUpdateListeners(listenerSet, isRecovery);
        }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <listenerSet>The service provider.</listenerSet>
        public EPServiceProvider ServiceProvider { get; private set; }

        #region EPStatementSPI Members

        /// <summary>
        /// Occurs whenever new events are available or old events are removed.
        /// </summary>
        public event UpdateEventHandler Events
        {
            add
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Event handler cannot be null");
                }

                if (IsDisposed)
                {
                    throw new IllegalStateException("Statement is in disposed state");
                }

                _statementListenerSet.Events.Add(value);
                StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
                if (_statementLifecycleSvc != null)
                {
                    _statementLifecycleSvc.DispatchStatementLifecycleEvent(
                        new StatementLifecycleEvent(this,
                                                    StatementLifecycleEvent.LifecycleEventType.LISTENER_ADD,
                                                    value));
                }
            }

            remove
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Event handler cannot be null");
                }

                _statementListenerSet.Events.Remove(value);
                StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
                if (_statementLifecycleSvc != null)
                {
                    _statementLifecycleSvc.DispatchStatementLifecycleEvent(
                        new StatementLifecycleEvent(this,
                                                    StatementLifecycleEvent.LifecycleEventType.LISTENER_REMOVE,
                                                    value));
                }
            }
        }

        public bool IsDisposed
        {
            get { return State == EPStatementState.DESTROYED; }
        }

        public ICollection<Attribute> Annotations
        {
            get { return StatementContext.Annotations; }
        }

        /// <summary>
        /// Returns the statement id.
        /// </summary>
        /// <listenerSet>statement id</listenerSet>
        public int StatementId
        {
            get { return StatementContext.StatementId; }
        }

        /// <summary>
        /// Start the statement.
        /// </summary>
        public void Start()
        {
            if (_statementLifecycleSvc == null)
            {
                throw new IllegalStateException("Cannot start statement, statement is in destroyed state");
            }
            _statementLifecycleSvc.Start(StatementContext.StatementId);
        }

        /// <summary>
        /// Stop the statement.
        /// </summary>
        public void Stop()
        {
            if (_statementLifecycleSvc == null)
            {
                throw new IllegalStateException("Cannot stop statement, statement is in destroyed state");
            }
            _statementLifecycleSvc.Stop(StatementContext.StatementId);

            // On stop, we give the dispatch view a chance to dispatch readonly results, if any
            StatementContext.StatementResultService.DispatchOnStop();

            _dispatchChildView.Clear();
        }

        /// <summary>
        /// Gets the statement's current state
        /// </summary>
        /// <listenerSet></listenerSet>
        public EPStatementState State { get; private set; }

        /// <summary>
        /// Set statement state.
        /// </summary>
        /// <param name="currentState">new current state</param>
        /// <param name="timeLastStateChange">the timestamp the statement changed state</param>
        public void SetCurrentState(EPStatementState currentState,
                                    long timeLastStateChange)
        {
            State = currentState;
            TimeLastStateChange = timeLastStateChange;
        }

        /// <summary>
        /// Sets the parent view.
        /// </summary>
        /// <listenerSet>is the statement viewable</listenerSet>
        public Viewable ParentView
        {
            get { return _parentView; }
            set
            {
                if (value == null)
                {
                    if (_parentView != null)
                    {
                        _parentView.RemoveView(_dispatchChildView);
                    }
                    _parentView = null;
                }
                else
                {
                    _parentView = value;
                    _parentView.AddView(_dispatchChildView);
                    EventType = _parentView.EventType;
                }
            }
        }

        /// <summary>
        /// Returns the underlying expression text or XML.
        /// </summary>
        /// <listenerSet></listenerSet>
        /// <returns> expression text</returns>
        public string Text
        {
            get { return StatementContext.Expression; }
        }

        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <listenerSet></listenerSet>
        /// <returns> statement name</returns>
        public string Name
        {
            get { return StatementContext.StatementName; }
        }

        public IEnumerator<EventBean> GetEnumerator(ContextPartitionSelector selector)
        {
            if (StatementContext.ContextDescriptor == null)
            {
                throw GetUnsupportedNonContextEnumerator();
            }
            if (selector == null)
            {
                throw new ArgumentException("No selector provided");
            }

            // Return null if not started
            StatementContext.VariableService.SetLocalVersion();
            if (_parentView == null)
            {
                return null;
            }
            return StatementContext.ContextDescriptor.GetEnumerator(StatementContext.StatementId, selector);
        }

        public IEnumerator<EventBean> GetSafeEnumerator(ContextPartitionSelector selector)
        {
            if (StatementContext.ContextDescriptor == null)
            {
                throw GetUnsupportedNonContextEnumerator();
            }
            if (selector == null)
            {
                throw new ArgumentException("No selector provided");
            }

            // Return null if not started
            if (_parentView == null)
            {
                return null;
            }

            StatementContext.VariableService.SetLocalVersion();
            return StatementContext.ContextDescriptor.GetSafeEnumerator(
                StatementContext.StatementId, selector);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<EventBean> GetEnumerator()
        {
            // Return null if not started
            StatementContext.VariableService.SetLocalVersion();
            if (_parentView == null)
            {
                return EnumerationHelper.Empty<EventBean>();
            }

            IEnumerator<EventBean> theEnumerator;

            if (StatementContext.ContextDescriptor != null)
            {
                theEnumerator = StatementContext.ContextDescriptor.GetEnumerator(StatementContext.StatementId);
            }
            else
            {
                theEnumerator = _parentView.GetEnumerator();
            }

            if (StatementContext.EpStatementHandle.HasTableAccess)
            {
                return GetUnsafeEnumeratorWTableImpl(
                    theEnumerator, StatementContext.TableExprEvaluatorContext);
            }

            return theEnumerator;
        }

        private IEnumerator<EventBean> GetUnsafeEnumeratorWTableImpl(
            IEnumerator<EventBean> enumerator,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            try
            {
                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current;
                    tableExprEvaluatorContext.ReleaseAcquiredLocks();
                    yield return value;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Returns a concurrency-safe iterator that iterates over events representing statement results (pull API)
        /// in the face of concurrent event processing by further threads.
        /// <para />
        /// In comparison to the regular iterator, the safe iterator guarantees correct results even
        /// as events are being processed by other threads. The cost is that the iterator holds
        /// one or more locks that must be released. Any locks are acquired at the time this method
        /// is called.
        /// <para/>
        /// This method is a blocking method. It may block until statement processing locks are released
        /// such that the safe iterator can acquire any required locks.
        /// <para/>
        /// An application MUST explicitly close the safe iterator instance using the close method, to release locks held by the
        /// iterator. The call to the close method should be done in a finally block to make sure
        /// the iterator gets closed.
        /// <para/>
        /// Multiple safe iterators may be not be used at the same time by different application threads.
        /// A single application thread may hold and use multiple safe iterators however this is discouraged.
        /// </summary>
        /// <returns>safe iterator;</returns>
        public IEnumerator<EventBean> GetSafeEnumerator()
        {
            // Return null if not started
            if (_parentView == null)
            {
                return null;
            }

            if (StatementContext.ContextDescriptor != null)
            {
                StatementContext.VariableService.SetLocalVersion();
                return StatementContext.ContextDescriptor.GetSafeEnumerator(StatementContext.StatementId);
            }

            // Set variable version and acquire the lock first
            var instanceLockHandler = StatementContext.DefaultAgentInstanceLock.AcquireReadLock();

            try
            {
                StatementContext.VariableService.SetLocalVersion();
                // Provide iterator - that iterator MUST be closed else the lock is not released
                if (StatementContext.EpStatementHandle.HasTableAccess)
                {
                    return GetSafeEnumeratorWTableImpl(
                        instanceLockHandler,
                        _parentView.GetEnumerator(),
                        StatementContext.TableExprEvaluatorContext);
                }

                return GetSafeEnumerator(instanceLockHandler, _parentView.GetEnumerator());
            }
            catch
            {
                // Lock disposal only occurs if the lock has been acquired and the
                // subsequent methods fail to return a true enumerator.  If the
                // enumerator is successfully created, this disposable artifact
                // remains intact.

                instanceLockHandler.Dispose();
                throw;
            }
        }

        private IEnumerator<EventBean> GetSafeEnumeratorWTableImpl(
            IDisposable instanceLockHandler,
            IEnumerator<EventBean> enumerator,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            try
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            finally
            {
                instanceLockHandler.Dispose();
                tableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }

        private IEnumerator<EventBean> GetSafeEnumerator(
            IDisposable instanceLockHandler,
            IEnumerator<EventBean> enumerator)
        {
            try
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            finally
            {
                instanceLockHandler.Dispose();
            }
        }


        public EventType EventType { get; private set; }

        /// <summary>Returns the set of listeners to the statement. </summary>
        /// <listenerSet>statement listeners</listenerSet>
        public void SetListenerSet(EPStatementListenerSet value, bool isRecovery)
        {
            _statementListenerSet.Copy(value);
            StatementContext.StatementResultService.SetUpdateListeners(value, isRecovery);
        }

        /// <summary>Returns the set of listeners to the statement. </summary>
        /// <listenerSet>statement listeners</listenerSet>
        public EPStatementListenerSet GetListenerSet()
        {
            return _statementListenerSet;
        }

        public long TimeLastStateChange { get; private set; }

        public bool IsStarted
        {
            get { return State == EPStatementState.STARTED; }
        }

        public bool IsStopped
        {
            get { return State == EPStatementState.STOPPED; }
        }

        public void SetSubscriber(object subscriber, string methodName)
        {
            _statementListenerSet.Subscriber = new EPSubscriber(subscriber, methodName);
            StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
        }

        public object Subscriber
        {
            get { return _statementListenerSet.Subscriber; }
            set
            {
                if (value is EPSubscriber)
                {
                    _statementListenerSet.Subscriber = (EPSubscriber)value;
                }
                else
                {
                    _statementListenerSet.Subscriber = new EPSubscriber(value);
                }

                StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
            }
        }

        public bool IsPattern { get; private set; }

        public StatementMetadata StatementMetadata { get; private set; }

        public object UserObject { get; private set; }

        public StatementContext StatementContext { get; internal set; }

        public string ExpressionNoAnnotations { get; private set; }

        public string ServiceIsolated { get; set; }

        public bool IsNameProvided { get; private set; }

        public UpdateDispatchViewBase DispatchChildView
        {
            get { return _dispatchChildView; }
        }

        public void Dispose()
        {
            if (State == EPStatementState.DESTROYED)
            {
                throw new IllegalStateException("Statement already destroyed");
            }
            _statementLifecycleSvc.Dispose(StatementContext.StatementId);
            _parentView = null;
            EventType = null;
            _dispatchChildView = null;
            _statementLifecycleSvc = null;
        }

        /// <summary>Remove all event handlers from a statement.</summary>
        public void RemoveAllEventHandlers()
        {
            _statementListenerSet.RemoveAllEventHandlers();
            StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
            if (_statementLifecycleSvc != null)
            {
                _statementLifecycleSvc.DispatchStatementLifecycleEvent(
                    new StatementLifecycleEvent(this, StatementLifecycleEvent.LifecycleEventType.LISTENER_REMOVE_ALL));
            }
        }

        /// <summary>
        /// Add an event handler to the current statement and replays current statement 
        /// results to the handler.
        /// <para/>
        /// The handler receives current statement results as the first call to the Update
        /// method of the event handler, passing in the newEvents parameter the current statement
        /// results as an array of zero or more events. Subsequent calls to the Update
        /// method of the event handler are statement results.
        /// <para/>
        /// Current statement results are the events returned by the GetEnumerator or
        /// GetSafeEnumerator methods.
        /// <para/>
        /// Delivery of current statement results in the first call is performed by the
        /// same thread invoking this method, while subsequent calls to the event handler may
        /// deliver statement results by the same or other threads.
        /// <para/>
        /// Note: this is a blocking call, delivery is atomic: Events occurring during
        /// iteration and delivery to the event handler are guaranteed to be delivered in a separate
        /// call and not lost. The event handler implementation should minimize long-running or
        /// blocking operations.
        /// <para/>
        /// Delivery is only atomic relative to the current statement. If the same event handler
        /// instance is registered with other statements it may receive other statement
        /// result s simultaneously.
        /// <para/>
        /// If a statement is not started an therefore does not have current results, the
        /// event handler receives a single invocation with a null listenerSet in newEvents.
        /// </summary>
        /// <param name="eventHandler">eventHandler that will receive events</param>
        public void AddEventHandlerWithReplay(UpdateEventHandler eventHandler)
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException("eventHandler", "Null listener reference supplied");
            }

            if (IsDisposed)
            {
                throw new IllegalStateException("Statement is in destroyed state");
            }

            using (StatementContext.DefaultAgentInstanceLock.AcquireReadLock())
            {
                _statementListenerSet.Events.Add(eventHandler);
                StatementContext.StatementResultService.SetUpdateListeners(_statementListenerSet, false);
                if (_statementLifecycleSvc != null)
                {
                    _statementLifecycleSvc.DispatchStatementLifecycleEvent(
                        new StatementLifecycleEvent(this,
                                                    StatementLifecycleEvent.LifecycleEventType.LISTENER_ADD,
                                                    eventHandler));
                }

                IEnumerator<EventBean> enumerator = GetEnumerator();
                var events = new List<EventBean>();
                while (enumerator.MoveNext())
                {
                    events.Add(enumerator.Current);
                }

                try
                {
                    if (events.IsEmpty())
                    {
                        eventHandler.Invoke(
                            this,
                            new UpdateEventArgs(
                                ServiceProvider,
                                this,
                                null,
                                null));
                    }
                    else
                    {
                        eventHandler.Invoke(
                            this,
                            new UpdateEventArgs(
                                ServiceProvider,
                                this,
                                events.ToArray(),
                                null));
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(
                        "Unexpected exception invoking eventHandler for replay on event handler '{0}' : {1} : {2}",
                        eventHandler.GetType().Name,
                        exception.GetType().Name,
                        exception.Message);
                }
                finally
                {
                    if (StatementContext.EpStatementHandle.HasTableAccess)
                    {
                        StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Clears the event handlers.  Should be used with caution since this clears
        /// anyone who has registered a handler.
        /// </summary>
        public void ClearEventHandlers()
        {
            _statementListenerSet.Events.Clear();
            _statementLifecycleSvc.DispatchStatementLifecycleEvent(
                new StatementLifecycleEvent(this, StatementLifecycleEvent.LifecycleEventType.LISTENER_REMOVE_ALL));
        }

        private static UnsupportedOperationException GetUnsupportedNonContextEnumerator()
        {
            return new UnsupportedOperationException("Enumerator with context selector is only supported for statements under context");
        }
    }
}