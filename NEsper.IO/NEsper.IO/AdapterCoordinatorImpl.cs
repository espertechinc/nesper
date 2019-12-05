// ---------------------------------------------------------------------------------- /
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
// ---------------------------------------------------------------------------------- /

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esperio
{
    /// <summary>
    /// An implementation of AdapterCoordinator.
    /// </summary>
    public class AdapterCoordinatorImpl
        : AbstractCoordinatedAdapter
        , AdapterCoordinator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<SendableEvent, CoordinatedAdapter> _eventsFromAdapters = new Dictionary<SendableEvent, CoordinatedAdapter>();
        private readonly ICollection<CoordinatedAdapter> _emptyAdapters = new HashSet<CoordinatedAdapter>();
        private readonly bool _usingEngineThread;
        private readonly bool _usingExternalTimer;
        private readonly ScheduleBucket _scheduleBucket;
        private readonly EPServiceProvider _epService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epService">the EPServiceProvider for the engine services and runtime</param>
        /// <param name="usingEngineThread">true if the coordinator should set time by the scheduling service in the engine,
        ///                           false if it should set time externally through the calling thread
        /// </param>
        public AdapterCoordinatorImpl(EPServiceProvider epService, bool usingEngineThread)
            : this(epService, usingEngineThread, false, false)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epService">the EPServiceProvider for the engine services and runtime</param>
        /// <param name="usingEngineThread">true if the coordinator should set time by the scheduling service in the engine, false if it should set time externally through the calling thread</param>
        /// <param name="usingExternalTimer">true to use esper's external timer mechanism instead of internal timing</param>
        /// <param name="usingTimeSpanEvents"></param>
        /// <exception cref="System.ArgumentNullException">epService;epService cannot be null</exception>
        /// <exception cref="System.ArgumentException">Illegal type of EPServiceProvider</exception>
	    public AdapterCoordinatorImpl(EPServiceProvider epService, bool usingEngineThread, bool usingExternalTimer, bool usingTimeSpanEvents)
            : base(epService, usingEngineThread, usingExternalTimer, usingTimeSpanEvents)
        {
            if (epService == null)
            {
                throw new ArgumentNullException("epService", "epService cannot be null");
            }

            if (!(epService is EPServiceProviderSPI))
            {
                throw new ArgumentException("Illegal type of EPServiceProvider");
            }
            this._epService = epService;
            this._scheduleBucket = ((EPServiceProviderSPI) epService).SchedulingMgmtService.AllocateBucket();
            this._usingEngineThread = usingEngineThread;
            this._usingExternalTimer = usingExternalTimer;
        }

        /// <summary>
        ///@see com.espertech.esper.adapter.ReadableAdapter#read()
        /// </summary>
        public override SendableEvent Read()
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".read");
            }

            PollEmptyAdapters();

            var isEventsToSendEmpty = EventsToSend.IsEmpty();
            var isEventsFromAdaptersEmpty = _eventsFromAdapters.IsEmpty();
            var isEmptyAdaptersEmpty = _emptyAdapters.IsEmpty();

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".read eventsToSend.isEmpty==" + isEventsToSendEmpty);
                Log.Debug(".read eventsFromAdapters.isEmpty==" + isEventsFromAdaptersEmpty);
                Log.Debug(".read emptyAdapters.isEmpty==" + isEmptyAdaptersEmpty);
            }

            if (isEventsToSendEmpty && isEventsFromAdaptersEmpty && isEmptyAdaptersEmpty)
            {
                Stop();
            }

            if (StateManager.State == AdapterState.DESTROYED || isEventsToSendEmpty)
            {
                return null;
            }

            SendableEvent result = EventsToSend.First();
            ReplaceFirstEventToSend();

            return result;
        }

        /// <summary>
        ///@see com.espertech.esper.adapter.AdapterCoordinator#add(com.espertech.esper.adapter.Adapter)
        /// </summary>
        public virtual void Coordinate(InputAdapter inputAdapter)
        {
            if (inputAdapter == null)
            {
                throw new ArgumentException("AdapterSpec cannot be null");
            }

            if (!(inputAdapter is CoordinatedAdapter))
            {
                throw new ArgumentException("Cannot coordinate a Adapter of type " + inputAdapter.GetType());
            }
            CoordinatedAdapter adapter = (CoordinatedAdapter) inputAdapter;
            if (_eventsFromAdapters.Values.Contains(adapter) || _emptyAdapters.Contains(adapter))
            {
                return;
            }
            adapter.DisallowStateTransitions();
            adapter.EPService = _epService;
            adapter.UsingEngineThread = _usingEngineThread;
            adapter.UsingExternalTimer = _usingExternalTimer;
            adapter.ScheduleSlot = _scheduleBucket.AllocateSlot();
            AddNewEvent(adapter);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        protected override void Close()
        {
            // Do nothing
        }

        /// <summary>
        /// Replace the first member of eventsToSend with the next
        /// event returned by the read() method of the same Adapter that
        /// provided the first event.
        /// </summary>
        protected override void ReplaceFirstEventToSend()
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".ReplaceFirstEventToSend Replacing event");
            }
            SendableEvent _event = EventsToSend.First();
            EventsToSend.Remove(_event);
            AddNewEvent(_eventsFromAdapters.Get(_event));
            _eventsFromAdapters.Remove(_event);
            PollEmptyAdapters();
        }

        /// <summary>
        /// Reset all the changeable state of this ReadableAdapter, as if it were just created.
        /// </summary>
        protected override void Reset()
        {
            _eventsFromAdapters.Clear();
            _emptyAdapters.Clear();
        }

        private void AddNewEvent(CoordinatedAdapter adapter)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".addNewEvent eventsFromAdapters==" + _eventsFromAdapters);
            }
            SendableEvent _event = adapter.Read();
            if (_event != null)
            {
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".addNewEvent event==" + _event);
                }
                EventsToSend.Add(_event);
                _eventsFromAdapters[_event] = adapter;
            }
            else
            {
                if (adapter.State == AdapterState.DESTROYED)
                {
                    LinkedList<SendableEvent> keyList = new LinkedList<SendableEvent>();

                    foreach (KeyValuePair<SendableEvent, CoordinatedAdapter> entry in _eventsFromAdapters)
                    {
                        if (entry.Value == adapter)
                        {
                            keyList.AddFirst(entry.Key);
                        }
                    }

                    foreach (SendableEvent keyEvent in keyList)
                    {
                        _eventsFromAdapters.Remove(keyEvent);
                    }
                }
                else
                {
                    _emptyAdapters.Add(adapter);
                }
            }
        }

        private void PollEmptyAdapters()
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".pollEmptyAdapters emptyAdapters.size==" + _emptyAdapters.Count);
            }

            List<CoordinatedAdapter> tempList = new List<CoordinatedAdapter>();

            foreach (CoordinatedAdapter adapter in _emptyAdapters)
            {
                if (adapter.State == AdapterState.DESTROYED)
                {
                    tempList.Add(adapter);
                    continue;
                }

                SendableEvent _event = adapter.Read();
                if (_event != null)
                {
                    EventsToSend.Add(_event);
                    _eventsFromAdapters[_event] = adapter;
                }
            }

            foreach (CoordinatedAdapter adapter in tempList)
            {
                _emptyAdapters.Remove(adapter);
            }
        }
    }
}