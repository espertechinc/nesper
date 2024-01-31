// ---------------------------------------------------------------------------------- /
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
// ---------------------------------------------------------------------------------- /

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esperio
{
	/// <summary>
	/// A skeleton implementation for coordinated adapter reading, for adapters that
	/// can do timestamp-coordinated input.
	/// </summary>
	public abstract class AbstractCoordinatedAdapter : CoordinatedAdapter
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    /// <summary>
	    /// Statement management.
	    /// </summary>
	    protected readonly AdapterStateManager StateManager = new AdapterStateManager();

	    /// <summary>
	    /// Sorted events to be sent.
	    /// </summary>
	    protected readonly ICollection<SendableEvent> EventsToSend = new SortedSet<SendableEvent>(new SendableEventComparator());

	    private readonly EPRuntime _runtime;
	    private EPEventService _processEvent;
	    private SchedulingService _schedulingService;
	    private long _currentTime;
        private long _lastEventTime;
		private long _startTime;
        private AbstractSender _sender;
	    private IContainer _container;

        /// <summary>
        /// Get the state of this Adapter.
        /// </summary>
        /// <value></value>
        public AdapterState State => StateManager.State;

        /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="runtime">the runtime for the engine runtime</param>
	    /// <param name="usingEngineThread">true if the Adapter should set time by the scheduling service in the runtime, false if it should set time externally through the calling thread</param>
	    /// <param name="usingExternalTimer">true to use esper's external timer mechanism instead of internal timing</param>
	    /// <param name="usingTimeSpanEvents"></param>
	    protected AbstractCoordinatedAdapter(EPRuntime runtime, bool usingEngineThread, bool usingExternalTimer, bool usingTimeSpanEvents)
        {
            UsingEngineThread = usingEngineThread;
            UsingExternalTimer = usingExternalTimer;
            UsingTimeSpanEvents = usingTimeSpanEvents;
            Sender = new DirectSender();

			if(runtime == null)
			{
				return;
			}
			if(!(runtime is EPRuntimeSPI runtimeSpi))
			{
				throw new ArgumentException("Invalid epService provided");
			}

            _runtime = runtimeSpi;
            _container = runtimeSpi.Container;
            _processEvent = runtime.EventService;
			_schedulingService = runtimeSpi.ServicesContext.SchedulingService;
		}

	    /// <summary>
        /// Start the sending of events into the runtime egine.
        /// </summary>
        /// <throws>EPException in case of errors processing the events</throws>
        public virtual void Start()
        {
            if ((ExecutionPathDebugLog.IsDebugEnabled) &&
                (Log.IsDebugEnabled)) {
                Log.Debug(".start");
            }
            if (_processEvent == null) {
                throw new EPException("Attempting to start an Adapter that hasn't had the runtime provided");
            }
            _startTime = CurrentTime;
            if ((ExecutionPathDebugLog.IsDebugEnabled) &&
                (Log.IsDebugEnabled)) {
                Log.Debug(".start startTime==" + _startTime);
            }
            StateManager.Start();
            _sender.Runtime = _processEvent;
            ContinueSendingEvents();
        }

	    /// <summary>
        /// Pause the sending of events after a Adapter has been started.
        /// </summary>
        /// <throws>EPException if this Adapter has already been stopped</throws>
		public virtual void Pause()
		{
			StateManager.Pause();
		}

        /// <summary>
        /// Resume sending events after the Adapter has been paused.
        /// </summary>
        /// <throws>EPException in case of errors processing the events</throws>
		public virtual void Resume()
		{
			StateManager.Resume();
			ContinueSendingEvents();
		}

        /// <summary>
        /// Dispose the Adapter, stopping the sending of all events and releasing all
        /// the resources, and disallowing any further state changes on the Adapter.
        /// </summary>
        /// <throws>EPException to indicate errors during destroy</throws>
		public virtual void Destroy()
		{
            var tempSender = Interlocked.Exchange(ref _sender, null);
            if ( tempSender != null )
            {
                tempSender.OnFinish();
            }

            StateManager.Destroy();
			Close();
		}

        /// <summary>
        /// Stop sending events and return the Adapter to the OPENED state, ready to be
        /// started once again.
        /// </summary>
        /// <throws>EPException in case of errors releasing resources</throws>
		public virtual void Stop()
		{
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".Stop");
            }
            
            StateManager.Stop();
			EventsToSend.Clear();
			_currentTime = 0;
			Reset();
		}

        /// <summary>
        /// Disallow subsequent state changes and throw an IllegalStateTransitionException
        /// if they are attempted.
        /// </summary>
		public virtual void DisallowStateTransitions()
		{
			StateManager.DisallowStateTransitions();
		}

	    /// <summary>
	    /// Gets or sets the using engine thread.
	    /// </summary>
	    /// <value>The using engine thread.</value>
	    public bool UsingEngineThread { get; set; }

	    /// <summary>
	    /// Gets or sets a value indicating whether to use esper's external timer mechanism
	    /// instead of internal timing
	    /// </summary>
	    /// <value><c>true</c> if [using external timer]; otherwise, <c>false</c>.</value>
	    public bool UsingExternalTimer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use timespan events.
        /// </summary>
        /// <value>
        /// <c>true</c> if [using time span events]; otherwise, <c>false</c>.
        /// </value>
        public bool UsingTimeSpanEvents { get; set; }

	    /// <summary>
	    /// Gets or sets the schedule slot.
	    /// </summary>
	    /// <value>The schedule slot.</value>
        public long ScheduleSlot { get; set; }

	    /// <summary>
        /// Sets the service.
        /// </summary>

        public virtual EPRuntime Runtime
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "epService cannot be null");
				}

				if (!(value is EPRuntimeSPI spi))
                {
                    throw new ArgumentException("Invalid type of EPServiceProvider");
                }
                
                _processEvent = spi.EventService;
                _schedulingService = spi.ServicesContext.SchedulingService;
                _sender.Runtime = _processEvent;
            }
		}

		/// <summary>
		/// Perform any actions specific to this Adapter that should
		/// be completed before the Adapter is stopped.
		/// </summary>
		protected abstract void Close();

		/// <summary>
		/// Remove the first member of eventsToSend and insert
		/// another event chosen in some fashion specific to this
		/// Adapter.
		/// </summary>
		protected abstract void ReplaceFirstEventToSend();

		/// <summary>
		/// Reset all the changeable state of this Adapter, as if it were just created.
		/// </summary>
		protected abstract void Reset();

		private void ContinueSendingEvents()
		{
            bool keepLooping = true;
            while (StateManager.State == AdapterState.STARTED && keepLooping) {
                _currentTime = CurrentTime;
                if ((ExecutionPathDebugLog.IsDebugEnabled) &&
                    (Log.IsDebugEnabled)) {
                    Log.Debug(".ContinueSendingEvents currentTime==" + _currentTime);
                }
                FillEventsToSend();
                SendSoonestEvents();
                keepLooping = WaitToSendEvents();
            }
		}

        private bool WaitToSendEvents()
        {
            if (UsingExternalTimer)
            {
                return false;
            }
            
            if (UsingEngineThread)
            {
                ScheduleNextCallback();
                return false;
            }

            long sleepTime;
            if (EventsToSend.IsEmpty())
            {
                sleepTime = 100;
            }
            else
            {
                sleepTime = EventsToSend.First().SendTime - (_currentTime - _startTime);
            }

            Thread.Sleep((int) sleepTime);

            return true;
        }

	    private long CurrentTime
		{
			get
			{
			    return UsingEngineThread
                    ? _schedulingService.Time
			        : DateTimeHelper.CurrentTimeMillis;
			}
		}

		private void FillEventsToSend()
		{
			if(EventsToSend.IsEmpty())
			{
				SendableEvent theEvent = Read();
				if(theEvent != null)
				{
					EventsToSend.Add(theEvent);
				}
			}
		}

		private void SendSoonestEvents()
		{
            if (UsingExternalTimer)
            {
                // send all events in order and when time clicks over send time event for previous time
                while (!EventsToSend.IsEmpty())
                {
                    long currentEventTime = EventsToSend.First().SendTime;
                    // check whether time has increased. Cannot go backwards due to checks elsewhere
                    if (currentEventTime > _lastEventTime)
                    {
                        if (UsingTimeSpanEvents) {
	                        _runtime.EventService.AdvanceTimeSpan(currentEventTime);
                        }
                        else {
	                        _runtime.EventService.AdvanceTime(currentEventTime);
                        }

                        _lastEventTime = currentEventTime;
                    }
                    SendFirstEvent();
                }
            }
            else
            {
                // watch time and send events to catch up
                while (EventsToSend.IsNotEmpty() && EventsToSend.First().SendTime <= _currentTime - _startTime)
                {
                    SendFirstEvent();
                }
            }
        }

        private void SendFirstEvent()
        {
            var sendableEvent = EventsToSend.First();

            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendFirstEvent currentTime==" + _currentTime);
                Log.Debug(".sendFirstEvent sending event " + sendableEvent + ", its sendTime==" + sendableEvent.SendTime);
            }
            _sender.Runtime = _processEvent;
            sendableEvent.Send(_sender);
            ReplaceFirstEventToSend();
        }

		private void ScheduleNextCallback()
		{
		    var nextScheduleCallback = new ProxyScheduleHandleCallback(delegate { ContinueSendingEvents(); });
            var spi = (EPRuntimeSPI) _runtime;
            var deploymentId = "CSV-adapter-" + UuidGenerator.Generate();
            var metricsHandle = spi.ServicesContext.MetricReportingService.GetStatementHandle(-1, deploymentId, "AbstractCoordinatedAdapter");
            var lockImpl = _container.RWLockManager().CreateLock("CSV");
            var stmtHandle = new EPStatementHandle(
	            "AbstractCoordinatedAdapter", deploymentId, -1, null, 0, false, false, 
	            spi.ServicesContext.MultiMatchHandlerFactory.Make(false, false), false, false, metricsHandle, null, null);
            var agentInstanceHandle = new EPStatementAgentInstanceHandle(stmtHandle, -1, lockImpl);
            var scheduleCSVHandle = new EPStatementHandleCallbackSchedule(agentInstanceHandle, nextScheduleCallback);

	        long nextScheduleSlot;

			if(EventsToSend.IsEmpty())
			{
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".scheduleNextCallback no events to send, scheduling callback in 100 ms");
                }
			    nextScheduleSlot = 0L;
                _schedulingService.Add(100, scheduleCSVHandle, nextScheduleSlot);
			}
			else
			{
                // Offset is not a function of the currentTime alone.
			    var first = EventsToSend.First();
                long baseMsec = _currentTime - _startTime;
                long afterMsec = first.SendTime - baseMsec;
				nextScheduleSlot = first.ScheduleSlot;
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".scheduleNextCallback schedulingCallback in " + afterMsec + " milliseconds");
                }
                _schedulingService.Add(afterMsec, scheduleCSVHandle, nextScheduleSlot);
            }
		}

		/// <summary>
		/// Gets the process event.
		/// </summary>
		/// <value>The process event.</value>
		public EPEventService ProcessEvent {
			get => _processEvent;
			set => _processEvent = value;
		}

		/// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>The sender.</value>
	    public AbstractSender Sender
	    {
	        get => _sender;
	        set
            {
                _sender = value;
                _sender.Runtime = _processEvent;
            }
	    }

	    public abstract SendableEvent Read();
   }
}
